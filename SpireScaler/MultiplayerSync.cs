using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace SpireScaler;

/// <summary>Client → host: "send me the run's settings" (on run start, continue and reconnect).</summary>
public sealed class SpireScalerSettingsRequest : INetMessage
{
    public bool ShouldBroadcast => false;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;
    public bool ShouldBuffer => true;

    public void Serialize(PacketWriter writer) => writer.WriteBool(true);

    public void Deserialize(PacketReader reader) => reader.ReadBool();
}

/// <summary>Host → clients: the host's settings frozen for this run, as invariant-culture strings.</summary>
public sealed class SpireScalerSettingsMessage : INetMessage
{
    public long RunStartTime;
    public Dictionary<string, string> Values = new();

    public bool ShouldBroadcast => false;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;
    public bool ShouldBuffer => true;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteLong(RunStartTime);
        writer.WriteInt(Values.Count);
        foreach ((string key, string value) in Values)
        {
            writer.WriteString(key);
            writer.WriteString(value);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        RunStartTime = reader.ReadLong();
        int count = reader.ReadInt();
        Values = new Dictionary<string, string>(count);
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            Values[key] = reader.ReadString();
        }
    }
}

/// <summary>
/// Spec 3: in multiplayer everyone uses the host's settings, frozen when the run starts.
/// The host keeps the frozen values in user://SpireScaler/multiplayer_run.json keyed by the
/// run's start time, so continuing a saved run keeps them. Single-player keeps using the live
/// settings ("from the next combat").
/// </summary>
[HarmonyPatch(typeof(RunManager), "InitializeShared")]
public static class MultiplayerSync
{
    /// <summary>The host's frozen settings for the current multiplayer run, or null outside one.</summary>
    public static IReadOnlyDictionary<string, string>? HostValues { get; private set; }

    private static INetGameService? _netService;
    private static long _runStartTime;

    private static string RunFilePath => Path.Combine(OS.GetUserDataDir(), "SpireScaler", "multiplayer_run.json");

    [HarmonyPostfix]
    public static void AfterInitializeShared(INetGameService netService, long startTime)
    {
        Detach();
        HostValues = null;
        _runStartTime = startTime;

        switch (netService.Type)
        {
            case NetGameType.Host:
                _netService = netService;
                netService.RegisterMessageHandler<SpireScalerSettingsRequest>(OnRequest);
                HostValues = LoadFrozen(startTime) ?? Freeze(startTime);
                Log.Info($"[SpireScaler] Multiplayer host: using frozen settings for run {startTime} ({Describe(HostValues)})");
                netService.SendMessage(BuildMessage());
                break;
            case NetGameType.Client:
                _netService = netService;
                netService.RegisterMessageHandler<SpireScalerSettingsMessage>(OnSettings);
                Log.Info("[SpireScaler] Multiplayer client: requesting the host's settings");
                netService.SendMessage(new SpireScalerSettingsRequest());
                break;
            default:
                Log.Info("[SpireScaler] Single-player run: using local settings");
                break;
        }
    }

    private static void Detach()
    {
        if (_netService == null)
        {
            return;
        }
        _netService.UnregisterMessageHandler<SpireScalerSettingsRequest>(OnRequest);
        _netService.UnregisterMessageHandler<SpireScalerSettingsMessage>(OnSettings);
        _netService = null;
    }

    private static SpireScalerSettingsMessage BuildMessage() => new()
    {
        RunStartTime = _runStartTime,
        Values = new Dictionary<string, string>(HostValues ?? SpireScalerConfig.CaptureValues()),
    };

    private static void OnRequest(SpireScalerSettingsRequest message, ulong senderId)
    {
        Log.Info($"[SpireScaler] Sending host settings to player {senderId}");
        _netService?.SendMessage(BuildMessage(), senderId);
    }

    private static void OnSettings(SpireScalerSettingsMessage message, ulong senderId)
    {
        HostValues = message.Values;
        Log.Info($"[SpireScaler] Multiplayer client: received host settings for run {message.RunStartTime} ({Describe(HostValues)})");
    }

    private static Dictionary<string, string> Freeze(long startTime)
    {
        Dictionary<string, string> values = SpireScalerConfig.CaptureValues();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RunFilePath)!);
            var file = new Dictionary<string, Dictionary<string, string>> { [startTime.ToString()] = values };
            File.WriteAllText(RunFilePath, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e)
        {
            Log.Error($"[SpireScaler] Could not save multiplayer run settings: {e.Message}");
        }
        return values;
    }

    private static Dictionary<string, string>? LoadFrozen(long startTime)
    {
        try
        {
            if (!File.Exists(RunFilePath))
            {
                return null;
            }
            var file = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(RunFilePath));
            return file != null && file.TryGetValue(startTime.ToString(), out var values) ? values : null;
        }
        catch (Exception e)
        {
            Log.Error($"[SpireScaler] Could not read multiplayer run settings: {e.Message}");
            return null;
        }
    }

    private static string Describe(IReadOnlyDictionary<string, string> values) =>
        $"HP x{values.GetValueOrDefault(nameof(SpireScalerConfig.HpMultiplier))}, Attack x{values.GetValueOrDefault(nameof(SpireScalerConfig.AttackMultiplier))}, " +
        $"Act custom {values.GetValueOrDefault("Act1Custom")}/{values.GetValueOrDefault("Act2Custom")}/{values.GetValueOrDefault("Act3Custom")}";
}
