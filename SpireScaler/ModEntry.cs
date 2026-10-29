using System;
using System.Runtime.CompilerServices;
using BaseLib.Config;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Rooms;

namespace SpireScaler;

/// <summary>Multipliers in effect for one combat.</summary>
public sealed record MultiplierSettings(decimal HpMultiplier, decimal AttackMultiplier);

[ModInitializer(nameof(Init))]
public static class ModEntry
{
    public const string ModId = "SpireScaler";
    public const string HarmonyId = "com.7qvz-z.SpireScaler";

    // One settings snapshot per combat, taken when its first creature is created,
    // so config edits apply from the next combat only.
    private static readonly ConditionalWeakTable<ICombatState, MultiplierSettings> CombatSettings = new();

    public static void Init()
    {
        ModConfigRegistry.Register(ModId, new SpireScalerConfig());
        var harmony = new Harmony(HarmonyId);
        harmony.PatchAll(typeof(ModEntry).Assembly);
        Log.Info($"[SpireScaler] Loaded. Global HP x{SpireScalerConfig.HpMultiplier}, Attack x{SpireScalerConfig.AttackMultiplier}");
    }

    /// <summary>
    /// Snapshot for the whole combat, keyed by its Act and encounter type, so summoned
    /// enemies use the encounter's type.
    /// </summary>
    public static MultiplierSettings SettingsFor(ICombatState combatState)
    {
        return CombatSettings.GetValue(combatState, cs =>
        {
            int actIndex = cs.RunState?.CurrentActIndex ?? 0;
            EnemyKind kind = cs.Encounter?.RoomType switch
            {
                RoomType.Elite => EnemyKind.Elite,
                RoomType.Boss => EnemyKind.Boss,
                _ => EnemyKind.Normal,
            };
            MultiplierSettings s = SpireScalerConfig.Snapshot(actIndex, kind);
            Log.Info($"[SpireScaler] New combat (Act {actIndex + 1}, {kind}): HP x{s.HpMultiplier}, Attack x{s.AttackMultiplier}");
            return s;
        });
    }

    public static MultiplierSettings? SettingsFor(Creature? creature)
    {
        ICombatState? combatState = creature?.CombatState;
        return combatState == null ? null : SettingsFor(combatState);
    }

    /// <summary>Rounds up, as the spec requires (2.3).</summary>
    public static decimal Scale(decimal value, decimal multiplier) => Math.Ceiling(value * multiplier);
}
