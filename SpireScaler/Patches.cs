using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace SpireScaler;

/// <summary>
/// Enemy HP. CreateCreature is used for initial enemies and for summons/splits (CreatureCmd.Add),
/// and already includes the game's unique-HP roll and multiplayer scaling.
/// </summary>
[HarmonyPatch(typeof(CombatState), nameof(CombatState.CreateCreature))]
public static class CreateCreaturePatch
{
    private static readonly ConditionalWeakTable<Creature, object> Applied = new();

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(CombatState __instance, CombatSide side, Creature __result)
    {
        if (side != CombatSide.Enemy || __result == null || !__result.IsMonster)
        {
            return;
        }
        if (Applied.TryGetValue(__result, out _))
        {
            return;
        }
        Applied.Add(__result, new object());

        MultiplierSettings s = ModEntry.SettingsFor(__instance);
        if (s.HpMultiplier == 1m)
        {
            return;
        }

        int oldMax = __result.MaxHp;
        int oldCurrent = __result.CurrentHp;
        decimal newMax = Math.Max(1m, ModEntry.Scale(oldMax, s.HpMultiplier));
        decimal newCurrent = Math.Clamp(ModEntry.Scale(oldCurrent, s.HpMultiplier), 1m, newMax);
        __result.SetMaxHpInternal(newMax);
        __result.SetCurrentHpInternal(newCurrent);
        Log.Info($"[SpireScaler] {__result.LogName}: HP {oldCurrent}/{oldMax} -> {__result.CurrentHp}/{__result.MaxHp} (x{s.HpMultiplier})");
    }
}

/// <summary>
/// Enemy attack damage. Hook.ModifyDamage serves both real damage (CreatureCmd.Damage) and the
/// attack intent number, so both stay in sync. Only Move damage (attacks) dealt by enemy monsters is scaled.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class ModifyDamagePatch
{
    private static (Creature? dealer, decimal before, decimal after) _lastLogged;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Creature? dealer, ValueProp props, ref decimal __result)
    {
        if (dealer == null || !dealer.IsEnemy || !dealer.IsMonster || !props.HasFlag(ValueProp.Move))
        {
            return;
        }
        MultiplierSettings? s = ModEntry.SettingsFor(dealer);
        if (s == null || s.AttackMultiplier == 1m)
        {
            return;
        }

        decimal before = __result;
        __result = Math.Max(0m, ModEntry.Scale(before, s.AttackMultiplier));

        // Intent previews call this every refresh; only log when the values change.
        (Creature? dealer, decimal before, decimal after) entry = (dealer, before, __result);
        if (!entry.Equals(_lastLogged))
        {
            _lastLogged = entry;
            Log.Info($"[SpireScaler] {dealer.LogName}: attack {before} -> {__result} (x{s.AttackMultiplier})");
        }
    }
}

/// <summary>Spec 2.3: with an HP multiplier of 0, enemies gain no block.</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
public static class ModifyBlockPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Creature target, ref decimal __result)
    {
        if (target == null || !target.IsEnemy || !target.IsMonster)
        {
            return;
        }
        MultiplierSettings? s = ModEntry.SettingsFor(target);
        if (s != null && s.HpMultiplier == 0m && __result != 0m)
        {
            Log.Info($"[SpireScaler] {target.LogName}: block {__result} -> 0 (HP x0)");
            __result = 0m;
        }
    }
}
