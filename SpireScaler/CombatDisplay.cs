using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SpireScaler;

/// <summary>
/// Spec 2.7: a small "HP×n / Attack×n" label just above each enemy's health bar (summons
/// included), showing the multipliers applied to the current combat. Toggled by ShowMultiplierDisplay.
/// The label is a child of the health bar, so it follows it and goes away with the creature node.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class CombatDisplay
{
    private const string LabelName = "SpireScalerMultiplierLabel";

    [HarmonyPostfix]
    public static void Postfix(NCreature __instance)
    {
        if (!SpireScalerConfig.ShowMultiplierDisplay
            || __instance.Entity is not { IsEnemy: true, IsMonster: true } enemy)
        {
            return;
        }

        // NCreature's "%HealthBar" is the state display; the bar itself is its own "%HealthBar".
        Control? stateDisplay = __instance.GetNodeOrNull<Control>("%HealthBar");
        Control? bar = stateDisplay?.GetNodeOrNull<Control>("%HealthBar") ?? stateDisplay;
        if (bar == null || bar.GetNodeOrNull(LabelName) != null)
        {
            return;
        }

        var label = new Label
        {
            Name = LabelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            GrowHorizontal = Control.GrowDirection.Both,
            AnchorLeft = 0f,
            AnchorRight = 1f,
            AnchorTop = 0f,
            AnchorBottom = 0f,
            OffsetTop = -30f,
            OffsetBottom = -4f,
        };
        SpireScalerConfig.Labels.ApplyFont(label);
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.93f, 0.8f));
        label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.9f));
        label.AddThemeConstantOverride("outline_size", 5);
        bar.AddChild(label);
        SetText(label, enemy);
        if (label.Text == "")
        {
            // The creature may join the combat state after its node is ready; try again next frame.
            Callable.From(() => SetText(label, enemy)).CallDeferred();
        }
    }

    private static void SetText(Label label, Creature creature)
    {
        if (GodotObject.IsInstanceValid(label) && creature.CombatState is { } combatState)
        {
            label.Text = SpireScalerConfig.Labels.Current().Display(ModEntry.SettingsFor(combatState));
        }
    }
}
