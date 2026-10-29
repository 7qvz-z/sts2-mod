using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SpireScaler;

/// <summary>
/// Spec 2.7: a small "HP×n / Attack×n" label near the top of the combat screen showing
/// the multipliers applied to the current combat. Toggled by ShowMultiplierDisplay.
/// </summary>
[HarmonyPatch(typeof(NCombatUi))]
public static class CombatDisplay
{
    private const string LabelName = "SpireScalerMultiplierLabel";

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCombatUi.Activate))]
    public static void AfterActivate(NCombatUi __instance, CombatState state)
    {
        Label label = GetOrCreateLabel(__instance);
        MultiplierSettings s = ModEntry.SettingsFor(state);
        label.Text = SpireScalerConfig.Labels.Current().Display(s);
        label.Visible = SpireScalerConfig.ShowMultiplierDisplay;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    public static void AfterDeactivate(NCombatUi __instance)
    {
        if (__instance.GetNodeOrNull<Label>(LabelName) is { } label)
        {
            label.Visible = false;
        }
    }

    private static Label GetOrCreateLabel(NCombatUi ui)
    {
        if (ui.GetNodeOrNull<Label>(LabelName) is { } existing)
        {
            return existing;
        }
        var label = new Label
        {
            Name = LabelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetLeft = -240f,
            OffsetRight = 240f,
            OffsetTop = 84f,
            OffsetBottom = 120f,
        };
        label.AddThemeFontSizeOverride("font_size", 24);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.93f, 0.8f, 0.85f));
        label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.9f));
        label.AddThemeConstantOverride("outline_size", 6);
        ui.AddChild(label);
        return label;
    }
}
