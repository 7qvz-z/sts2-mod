using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SpireScaler;

/// <summary>
/// Spec 2.7: a small "HP×n / Attack×n" label in the top bar, just right of the floor and
/// ascension icons, showing the multipliers applied to the current combat.
/// Toggled by ShowMultiplierDisplay.
/// </summary>
[HarmonyPatch(typeof(NCombatUi))]
public static class CombatDisplay
{
    private const string LabelName = "SpireScalerMultiplierLabel";
    private const float GapAfterIcons = 28f;
    private const float LabelWidth = 420f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCombatUi.Activate))]
    public static void AfterActivate(CombatState state)
    {
        NTopBar? topBar = NRun.Instance?.GlobalUi?.TopBar;
        if (topBar == null)
        {
            return;
        }
        Label label = GetOrCreateLabel(topBar);
        label.Text = SpireScalerConfig.Labels.Current().Display(ModEntry.SettingsFor(state));
        PlaceAfterIcons(topBar, label);
        label.Visible = SpireScalerConfig.ShowMultiplierDisplay;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCombatUi.Deactivate))]
    public static void AfterDeactivate() => Hide();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCombatUi._ExitTree))]
    public static void AfterExitTree() => Hide();

    private static void Hide()
    {
        if (NRun.Instance?.GlobalUi?.TopBar?.GetNodeOrNull<Label>(LabelName) is { } label)
        {
            label.Visible = false;
        }
    }

    /// <summary>Positions the label from the icons' actual rects, so it follows the HUD layout.</summary>
    private static void PlaceAfterIcons(NTopBar topBar, Label label)
    {
        Rect2? icons = null;
        foreach (Control? icon in new[] { topBar.RoomIcon, topBar.FloorIcon, topBar.BossIcon, topBar.GetNodeOrNull<Control>("%AscensionIcon") })
        {
            if (icon != null && icon.IsVisibleInTree())
            {
                Rect2 rect = icon.GetGlobalRect();
                icons = icons?.Merge(rect) ?? rect;
            }
        }
        Rect2 anchor = icons ?? topBar.Hp.GetGlobalRect();
        Transform2D toLocal = topBar.GetGlobalTransform().AffineInverse();
        Vector2 left = toLocal * new Vector2(anchor.End.X + GapAfterIcons, anchor.Position.Y + anchor.Size.Y / 2f);
        float height = label.GetMinimumSize().Y;
        label.Position = new Vector2(left.X, left.Y - height / 2f);
        label.Size = new Vector2(LabelWidth, height);
    }

    private static Label GetOrCreateLabel(NTopBar topBar)
    {
        if (topBar.GetNodeOrNull<Label>(LabelName) is { } existing)
        {
            return existing;
        }
        var label = new Label
        {
            Name = LabelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        label.AddThemeFontSizeOverride("font_size", 28);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.93f, 0.8f));
        label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.9f));
        label.AddThemeConstantOverride("outline_size", 6);
        topBar.AddChild(label);
        return label;
    }
}
