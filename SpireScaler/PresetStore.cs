using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace SpireScaler;

/// <summary>
/// Spec 2.5: presets in the mod's own file, user://SpireScaler/presets.json
/// (outside mods/, where every .json is read as a manifest). Each slot maps
/// config property names to invariant strings, plus the slot's display name.
/// </summary>
public static class PresetStore
{
    public const int SlotCount = 5;
    private const string NameKey = "__name";

    public static string FilePath => Path.Combine(OS.GetUserDataDir(), "SpireScaler", "presets.json");

    private static Dictionary<string, Dictionary<string, string>>? _slots;

    private static Dictionary<string, Dictionary<string, string>> Slots => _slots ??= Read();

    public static string? GetName(int slot) =>
        Slots.TryGetValue(slot.ToString(), out var values) ? values.GetValueOrDefault(NameKey, "") : null;

    public static IReadOnlyDictionary<string, string>? Get(int slot) =>
        Slots.TryGetValue(slot.ToString(), out var values) ? values : null;

    public static void Set(int slot, string name, Dictionary<string, string> values)
    {
        values[NameKey] = name;
        Slots[slot.ToString()] = values;
        Write();
    }

    private static Dictionary<string, Dictionary<string, string>> Read()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                using FileStream stream = File.OpenRead(FilePath);
                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? new();
            }
        }
        catch (Exception e)
        {
            Log.Error($"[SpireScaler] Could not read presets from {FilePath}: {e.Message}");
        }
        return new();
    }

    private static void Write()
    {
        string temp = FilePath + ".new";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            using (FileStream stream = File.Create(temp))
            {
                JsonSerializer.Serialize(stream, Slots, new JsonSerializerOptions { WriteIndented = true });
            }
            File.Move(temp, FilePath, overwrite: true);
        }
        catch (Exception e)
        {
            Log.Error($"[SpireScaler] Could not save presets to {FilePath}: {e.Message}");
        }
    }
}
