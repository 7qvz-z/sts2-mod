using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace SpireScaler;

[ModInitializer(nameof(Init))]
public static class ModEntry
{
    public const string HarmonyId = "com.7qvz-z.SpireScaler";

    // One settings snapshot per combat, taken when its first creature is created,
    // so config edits apply from the next combat only.
    private static readonly ConditionalWeakTable<ICombatState, MultiplierSettings> CombatSettings = new();

    public static void Init()
    {
        var harmony = new Harmony(HarmonyId);
        harmony.PatchAll(typeof(ModEntry).Assembly);
        MultiplierSettings s = ModConfig.Load();
        Log.Info($"[SpireScaler] Loaded. HP x{s.HpMultiplier}, Attack x{s.AttackMultiplier}");
    }

    public static MultiplierSettings SettingsFor(ICombatState combatState)
    {
        return CombatSettings.GetValue(combatState, _ =>
        {
            MultiplierSettings s = ModConfig.Load();
            Log.Info($"[SpireScaler] New combat: HP x{s.HpMultiplier}, Attack x{s.AttackMultiplier}");
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
