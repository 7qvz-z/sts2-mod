using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace EnemyScaler;

/// <summary>Multipliers in effect for one combat.</summary>
public sealed record MultiplierSettings(decimal HpMultiplier, decimal AttackMultiplier)
{
    public static readonly MultiplierSettings Default = new(1m, 1m);
}

/// <summary>
/// Reads multipliers from user://EnemyScaler/config.cfg (Godot ConfigFile format).
/// Kept out of mods/ because every .json there is parsed as a mod manifest.
/// </summary>
public static class ModConfig
{
    public const string Dir = "user://EnemyScaler";
    public const string Path = Dir + "/config.cfg";
    private const string Section = "multipliers";
    private const decimal Min = 0m;
    private const decimal Max = 100m;

    public static MultiplierSettings Load()
    {
        try
        {
            var cfg = new ConfigFile();
            if (!FileAccess.FileExists(Path))
            {
                WriteDefaults(cfg);
                return MultiplierSettings.Default;
            }

            Error err = cfg.Load(Path);
            if (err != Error.Ok)
            {
                Log.Warn($"[EnemyScaler] Failed to load {Path} ({err}); using 1.0x");
                return MultiplierSettings.Default;
            }

            return new MultiplierSettings(
                ReadMultiplier(cfg, "hp"),
                ReadMultiplier(cfg, "attack"));
        }
        catch (Exception e)
        {
            Log.Error($"[EnemyScaler] Error reading config: {e}");
            return MultiplierSettings.Default;
        }
    }

    private static decimal ReadMultiplier(ConfigFile cfg, string key)
    {
        double value = cfg.GetValue(Section, key, 1.0).AsDouble();
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1m;
        }
        return Math.Clamp((decimal)value, Min, Max);
    }

    private static void WriteDefaults(ConfigFile cfg)
    {
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(Dir));
        cfg.SetValue(Section, "hp", 1.0);
        cfg.SetValue(Section, "attack", 1.0);
        Error err = cfg.Save(Path);
        if (err == Error.Ok)
        {
            Log.Info($"[EnemyScaler] Created default config at {ProjectSettings.GlobalizePath(Path)}");
        }
        else
        {
            Log.Warn($"[EnemyScaler] Could not create {Path} ({err})");
        }
    }
}
