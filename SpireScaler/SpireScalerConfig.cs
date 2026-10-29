using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Logging;

namespace SpireScaler;

public enum EnemyKind
{
    Normal,
    Elite,
    Boss
}

/// <summary>
/// Settings shown in BaseLib's mod config screen. BaseLib saves the static properties to
/// user://mod_configs/SpireScaler.cfg, which is the only source of the multipliers.
/// The global HP/attack multipliers apply everywhere unless an Act has "custom" turned on,
/// in which case that Act uses its own normal/elite/boss values instead.
/// </summary>
public sealed class SpireScalerConfig : SimpleModConfig
{
    public const double NormalMin = 1.0;
    public const double NormalMax = 10.0;
    public const double AdvancedMin = 0.0;
    public const double AdvancedMax = 100.0;
    public const double SliderStep = 0.1;
    public const int ActCount = 3;
    private const string SliderFormat = "x{0:0.#}";
    private const string PresetName = @"[\p{L}\p{N}_\- ]*";
    private const string Number = @"[0-9]{0,3}(\.[0-9]?)?";

    // Restore Defaults resets the multipliers only (spec 2.6), not the mode or the per-Act switches.
    [ConfigIgnoreRestoreDefaults] public static bool AdvancedMode { get; set; }

    // Spec 2.7: show "HP×n / Attack×n" during combat.
    [ConfigIgnoreRestoreDefaults] public static bool ShowMultiplierDisplay { get; set; } = true;

    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double HpMultiplier { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double AttackMultiplier { get; set; } = 1.0;

    [ConfigIgnoreRestoreDefaults] public static bool Act1Custom { get; set; }
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1NormalHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1NormalAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1EliteHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1EliteAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1BossHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act1BossAttack { get; set; } = 1.0;

    [ConfigIgnoreRestoreDefaults] public static bool Act2Custom { get; set; }
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2NormalHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2NormalAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2EliteHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2EliteAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2BossHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act2BossAttack { get; set; } = 1.0;

    [ConfigIgnoreRestoreDefaults] public static bool Act3Custom { get; set; }
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3NormalHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3NormalAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3EliteHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3EliteAttack { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3BossHp { get; set; } = 1.0;
    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = SliderFormat)] public static double Act3BossAttack { get; set; } = 1.0;

    // Backing text for the numeric inputs; not saved, always derived from the values above.
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string HpMultiplierInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string AttackMultiplierInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1NormalHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1NormalAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1EliteHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1EliteAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1BossHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act1BossAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2NormalHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2NormalAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2EliteHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2EliteAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2BossHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act2BossAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3NormalHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3NormalAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3EliteHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3EliteAttackInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3BossHpInput { get; set; } = "1";
    [ConfigIgnore, ConfigTextInput(Number, MaxLength = 5)] public static string Act3BossAttackInput { get; set; } = "1";

    // Names typed into the preset slots; the presets themselves live in PresetStore.
    [ConfigIgnore, ConfigTextInput(PresetName, MaxLength = 20)] public static string Preset1Name { get; set; } = "";
    [ConfigIgnore, ConfigTextInput(PresetName, MaxLength = 20)] public static string Preset2Name { get; set; } = "";
    [ConfigIgnore, ConfigTextInput(PresetName, MaxLength = 20)] public static string Preset3Name { get; set; } = "";
    [ConfigIgnore, ConfigTextInput(PresetName, MaxLength = 20)] public static string Preset4Name { get; set; } = "";
    [ConfigIgnore, ConfigTextInput(PresetName, MaxLength = 20)] public static string Preset5Name { get; set; } = "";

    public static double RangeMin => AdvancedMode ? AdvancedMin : NormalMin;
    public static double RangeMax => AdvancedMode ? AdvancedMax : NormalMax;

    private static readonly EnemyKind[] Kinds = { EnemyKind.Normal, EnemyKind.Elite, EnemyKind.Boss };

    private readonly MultiplierField _globalHp = new(nameof(HpMultiplier), act: 0);
    private readonly MultiplierField _globalAttack = new(nameof(AttackMultiplier), act: 0);
    private readonly List<MultiplierField> _fields = new();
    private readonly bool[] _lastCustom = new bool[ActCount + 1];
    private bool _lastAdvanced;
    private bool _syncing;
    private Label? _presetNotice;

    public SpireScalerConfig()
    {
        _fields.Add(_globalHp);
        _fields.Add(_globalAttack);
        for (int act = 1; act <= ActCount; act++)
        {
            foreach (EnemyKind kind in Kinds)
            {
                _fields.Add(new MultiplierField(CellName(act, kind, hp: true), act));
                _fields.Add(new MultiplierField(CellName(act, kind, hp: false), act));
            }
            _lastCustom[act] = IsCustom(act);
        }
        _lastAdvanced = AdvancedMode;
        ClampToRange();
        _fields.ForEach(f => f.SyncTextFromValue());
        for (int slot = 1; slot <= PresetStore.SlotCount; slot++)
        {
            Property(PresetNameProperty(slot)).SetValue(null, PresetStore.GetName(slot) ?? "");
        }
        ConfigChanged += (_, _) => OnConfigChanged();
    }

    /// <summary>
    /// The multipliers for a combat in the given Act (0-based run act index) against the given enemy kind.
    /// Acts beyond the third use the third Act's settings.
    /// </summary>
    /// In a multiplayer run the host's values frozen at run start are used instead of the local settings.
    public static MultiplierSettings Snapshot(int actIndex, EnemyKind kind)
    {
        IReadOnlyDictionary<string, string>? values = MultiplayerSync.HostValues;
        int act = Math.Clamp(actIndex + 1, 1, ActCount);
        if (!ReadBool(values, $"Act{act}Custom"))
        {
            return new MultiplierSettings(
                ToMultiplier(ReadDouble(values, nameof(HpMultiplier))),
                ToMultiplier(ReadDouble(values, nameof(AttackMultiplier))));
        }
        return new MultiplierSettings(
            ToMultiplier(ReadDouble(values, CellName(act, kind, hp: true))),
            ToMultiplier(ReadDouble(values, CellName(act, kind, hp: false))));
    }

    /// <summary>The settings that decide the multipliers (not the display toggle), by property name.</summary>
    public static IEnumerable<PropertyInfo> SettingProperties()
    {
        yield return Property(nameof(AdvancedMode));
        yield return Property(nameof(HpMultiplier));
        yield return Property(nameof(AttackMultiplier));
        for (int act = 1; act <= ActCount; act++)
        {
            yield return Property($"Act{act}Custom");
            foreach (EnemyKind kind in Kinds)
            {
                yield return Property(CellName(act, kind, hp: true));
                yield return Property(CellName(act, kind, hp: false));
            }
        }
    }

    /// <summary>Current local settings as invariant strings, for presets and multiplayer sync.</summary>
    public static Dictionary<string, string> CaptureValues()
    {
        var values = new Dictionary<string, string>();
        foreach (PropertyInfo property in SettingProperties())
        {
            values[property.Name] = Convert.ToString(property.GetValue(null), CultureInfo.InvariantCulture) ?? "";
        }
        return values;
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string>? values, string name)
    {
        if (values == null)
        {
            return (bool)Property(name).GetValue(null)!;
        }
        return values.TryGetValue(name, out string? text) && bool.TryParse(text, out bool flag) && flag;
    }

    private static double ReadDouble(IReadOnlyDictionary<string, string>? values, string name)
    {
        if (values == null)
        {
            return (double)Property(name).GetValue(null)!;
        }
        return values.TryGetValue(name, out string? text)
            && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
            ? number
            : 1.0;
    }

    private static string CellName(int act, EnemyKind kind, bool hp) => $"Act{act}{kind}{(hp ? "Hp" : "Attack")}";

    private static bool IsCustom(int act) => (bool)Property($"Act{act}Custom").GetValue(null)!;

    private static decimal ToMultiplier(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1m;
        }
        return Math.Round((decimal)Math.Clamp(value, AdvancedMin, AdvancedMax), 1);
    }

    public override void SetupConfigUI(Control optionContainer)
    {
        _lastAdvanced = AdvancedMode;
        Labels t = Labels.Current();

        optionContainer.AddChild(CreateRawLabelControl(t.Note, 22));
        if (MultiplayerSync.HostValues != null)
        {
            optionContainer.AddChild(CreateRawLabelControl($"[color=#f0c060]{t.MultiplayerNote}[/color]", 22));
        }
        AddRow(optionContainer, optionContainer, nameof(AdvancedMode), t.Advanced, CreateRawTickboxControl(Property(nameof(AdvancedMode))));
        AddRow(optionContainer, optionContainer, nameof(ShowMultiplierDisplay), t.ShowDisplay, CreateRawTickboxControl(Property(nameof(ShowMultiplierDisplay))));
        AddField(optionContainer, optionContainer, _globalHp, t.GlobalHp);
        AddField(optionContainer, optionContainer, _globalAttack, t.GlobalAttack);

        for (int act = 1; act <= ActCount; act++)
        {
            _lastCustom[act] = IsCustom(act);
            NConfigCollapsibleSection section = CreateCollapsibleSection(t.ActTitle(act), alignToTop: false, collapsedByDefault: !IsCustom(act));
            optionContainer.AddChild(section);
            Control content = section.ContentContainer;
            AddRow(optionContainer, content, $"Act{act}Custom", t.ActCustom(act), CreateRawTickboxControl(Property($"Act{act}Custom")));
            foreach (EnemyKind kind in Kinds)
            {
                AddField(optionContainer, content, Field(CellName(act, kind, hp: true)), t.Cell(kind, hp: true));
                AddField(optionContainer, content, Field(CellName(act, kind, hp: false)), t.Cell(kind, hp: false));
            }
        }

        NConfigCollapsibleSection presets = CreateCollapsibleSection(t.Presets, alignToTop: false, collapsedByDefault: true);
        optionContainer.AddChild(presets);
        for (int slot = 1; slot <= PresetStore.SlotCount; slot++)
        {
            AddPresetRow(optionContainer, presets.ContentContainer, slot, t);
        }
        _presetNotice = new Label
        {
            Name = "PresetNotice",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0f, 48f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = Colors.Transparent,
        };
        Labels.ApplyFont(_presetNotice);
        _presetNotice.AddThemeFontSizeOverride("font_size", 26);
        _presetNotice.AddThemeColorOverride("font_color", new Color(0.55f, 0.9f, 0.55f));
        presets.ContentContainer.AddChild(_presetNotice);

        AddRestoreDefaultsButton(optionContainer);
        SetupFocusNeighbors(optionContainer);
        UpdateVisibility();
    }

    protected override void RestoreDefaultsNoConfirm()
    {
        base.RestoreDefaultsNoConfirm();
        RunSync(() =>
        {
            ClampToRange();
            _fields.ForEach(f => f.SyncAll());
        });
    }

    private static string PresetNameProperty(int slot) => $"Preset{slot}Name";

    /// <summary>Everything a preset restores: the mode, the per-Act switches and every multiplier.</summary>
    private static IEnumerable<PropertyInfo> PresetProperties() => SettingProperties();

    private void AddPresetRow(Control owner, Control container, int slot, Labels t)
    {
        NConfigLineEdit name = CreateRawLineEditControl(Property(PresetNameProperty(slot)));
        name.CustomMinimumSize = new Vector2(260f, 64f);
        NConfigButton save = CreateRawButtonControl(t.Save, () => SavePreset(slot));
        NConfigButton load = CreateRawButtonControl(t.Load, () => LoadPreset(slot));
        save.CustomMinimumSize = new Vector2(150f, 64f);
        load.CustomMinimumSize = new Vector2(150f, 64f);

        var controls = new HBoxContainer
        {
            Name = $"Preset{slot}Controls",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        controls.AddThemeConstantOverride("separation", 16);
        controls.AddChild(name);
        controls.AddChild(save);
        controls.AddChild(load);
        AddRow(owner, container, $"Preset{slot}", t.Slot(slot), controls);
    }

    private void SavePreset(int slot)
    {
        var values = new Dictionary<string, string>();
        foreach (PropertyInfo property in PresetProperties())
        {
            values[property.Name] = Convert.ToString(property.GetValue(null), CultureInfo.InvariantCulture) ?? "";
        }
        string name = (string?)Property(PresetNameProperty(slot)).GetValue(null) ?? "";
        PresetStore.Set(slot, name, values);
        ShowPresetNotice(Labels.Current().Saved(slot, name));
        Log.Info($"[SpireScaler] Saved preset {slot} \"{name}\"");
    }

    private void LoadPreset(int slot)
    {
        IReadOnlyDictionary<string, string>? values = PresetStore.Get(slot);
        if (values == null)
        {
            ShowPresetNotice(Labels.Current().Empty(slot));
            return;
        }
        RunSync(() =>
        {
            foreach (PropertyInfo property in PresetProperties())
            {
                if (!values.TryGetValue(property.Name, out string? text))
                {
                    continue;
                }
                if (property.PropertyType == typeof(bool) && bool.TryParse(text, out bool flag))
                {
                    property.SetValue(null, flag);
                }
                else if (property.PropertyType == typeof(double)
                    && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                {
                    property.SetValue(null, number);
                }
            }
            _lastAdvanced = AdvancedMode;
            for (int act = 1; act <= ActCount; act++)
            {
                _lastCustom[act] = IsCustom(act);
            }
            _fields.ForEach(f => f.ApplyRange());
            ClampToRange();
            ConfigReloaded();
            _fields.ForEach(f => f.SyncAll());
            UpdateVisibility();
        });
        Save();
        Log.Info($"[SpireScaler] Loaded preset {slot} \"{PresetStore.GetName(slot)}\"");
        ShowPresetNotice(Labels.Current().Loaded(slot, PresetStore.GetName(slot) ?? ""));
    }

    /// <summary>Shows a short message under the preset slots, then fades it out.</summary>
    private void ShowPresetNotice(string text)
    {
        if (_presetNotice == null || !GodotObject.IsInstanceValid(_presetNotice))
        {
            return;
        }
        _presetNotice.Text = text;
        _presetNotice.Modulate = Colors.White;
        Tween tween = _presetNotice.CreateTween();
        tween.TweenInterval(2.5);
        tween.TweenProperty(_presetNotice, "modulate:a", 0.0, 0.6);
    }

    private MultiplierField Field(string valueName) => _fields.First(f => f.ValueProperty.Name == valueName);

    /// <summary>One row holding a slider and a numeric input for the same multiplier.</summary>
    private void AddField(Control owner, Control container, MultiplierField field, string label)
    {
        NConfigSlider slider = CreateRawSliderControl(field.ValueProperty);
        slider.SetRange(RangeMin, RangeMax, SliderStep);
        field.Slider = slider;

        NConfigLineEdit input = CreateRawLineEditControl(field.TextProperty);
        input.CustomMinimumSize = new Vector2(110f, 64f);
        input.FocusExited += () => RunSync(field.SyncTextFromValue);
        field.Input = input;

        var controls = new HBoxContainer
        {
            Name = field.ValueProperty.Name + "Controls",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        controls.AddThemeConstantOverride("separation", 16);
        controls.AddChild(slider);
        controls.AddChild(input);

        field.Row = AddRow(owner, container, field.ValueProperty.Name, label, controls);
    }

    private NConfigOptionRow AddRow(Control owner, Control container, string name, string labelText, Control control)
    {
        // Dividers only in the top-level list; rows inside Act sections can be hidden.
        if (container == owner && container.GetChildCount() > 1)
        {
            container.AddChild(CreateDividerControl());
        }
        var row = new NConfigOptionRow(ModPrefix, name, CreateRawLabelControl(labelText, 28), control);
        row.UniqueNameInOwner = true;
        container.AddChild(row);
        row.Owner = owner;
        return row;
    }

    private void UpdateVisibility()
    {
        foreach (MultiplierField field in _fields.Where(f => f.Act > 0))
        {
            if (field.Row != null && GodotObject.IsInstanceValid(field.Row))
            {
                field.Row.Visible = IsCustom(field.Act);
            }
        }
    }

    private void OnConfigChanged()
    {
        if (_syncing)
        {
            return;
        }
        RunSync(() =>
        {
            if (AdvancedMode != _lastAdvanced)
            {
                _lastAdvanced = AdvancedMode;
                _fields.ForEach(f => f.ApplyRange());
                if (!AdvancedMode)
                {
                    // Spec 2.2: turning advanced mode off resets every multiplier to 1.0.
                    _fields.ForEach(f => f.SetValue(1.0));
                }
                ClampToRange();
                _fields.ForEach(f => f.SyncAll());
                return;
            }

            for (int act = 1; act <= ActCount; act++)
            {
                if (IsCustom(act) == _lastCustom[act])
                {
                    continue;
                }
                _lastCustom[act] = IsCustom(act);
                if (IsCustom(act))
                {
                    // Start the Act's overrides from the current global values.
                    foreach (MultiplierField field in _fields.Where(f => f.Act == act))
                    {
                        field.SetValue(field.ValueProperty.Name.EndsWith("Hp") ? HpMultiplier : AttackMultiplier);
                        field.SyncAll();
                    }
                }
                UpdateVisibility();
                return;
            }

            _fields.ForEach(f => f.Sync());
        });
    }

    private void RunSync(Action action)
    {
        bool wasSyncing = _syncing;
        _syncing = true;
        try
        {
            action();
        }
        finally
        {
            _syncing = wasSyncing;
        }
    }

    private void ClampToRange()
    {
        _fields.ForEach(f => f.SetValue(Clamp(f.GetValue())));
    }

    private static double Clamp(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1.0;
        }
        return Math.Round(Math.Clamp(value, RangeMin, RangeMax), 1);
    }

    private static PropertyInfo Property(string name)
    {
        return typeof(SpireScalerConfig).GetProperty(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(nameof(SpireScalerConfig), name);
    }

    private static string Format(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary>Keeps one multiplier's slider and numeric input in agreement.</summary>
    private sealed class MultiplierField
    {
        public readonly PropertyInfo ValueProperty;
        public readonly PropertyInfo TextProperty;
        /// <summary>1-based Act this field overrides, or 0 for the global values.</summary>
        public readonly int Act;
        public NConfigSlider? Slider;
        public NConfigLineEdit? Input;
        public NConfigOptionRow? Row;
        private double _lastValue;
        private string _lastText = "";

        public MultiplierField(string valueName, int act)
        {
            ValueProperty = Property(valueName);
            TextProperty = Property(valueName + "Input");
            Act = act;
        }

        public double GetValue() => (double)ValueProperty.GetValue(null)!;

        public void SetValue(double value) => ValueProperty.SetValue(null, value);

        private string Text
        {
            get => (string?)TextProperty.GetValue(null) ?? "";
            set => TextProperty.SetValue(null, value);
        }

        /// <summary>Called after any config change: whichever control moved updates the other.</summary>
        public void Sync()
        {
            if (GetValue() != _lastValue)
            {
                SyncTextFromValue();
            }
            else if (Text != _lastText)
            {
                _lastText = Text;
                if (double.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double typed))
                {
                    // Out-of-range input moves the slider to the limit; the text is corrected on focus exit.
                    SetValue(Clamp(typed));
                    _lastValue = GetValue();
                    RefreshSlider();
                }
            }
        }

        public void SyncAll()
        {
            RefreshSlider();
            SyncTextFromValue();
        }

        public void SyncTextFromValue()
        {
            _lastValue = GetValue();
            Text = Format(_lastValue);
            _lastText = Text;
            if (Input != null && GodotObject.IsInstanceValid(Input))
            {
                Input.Call(NConfigLineEdit.MethodName.SetFromProperty);
            }
        }

        public void ApplyRange()
        {
            if (Slider != null && GodotObject.IsInstanceValid(Slider))
            {
                Slider.SetRange(RangeMin, RangeMax, SliderStep);
            }
        }

        private void RefreshSlider()
        {
            if (Slider != null && GodotObject.IsInstanceValid(Slider))
            {
                Slider.Call(NConfigSlider.MethodName.SetFromProperty);
            }
        }
    }

    internal sealed record Labels(
        string Note,
        string MultiplayerNote,
        string Advanced,
        string ShowDisplay,
        string DisplayFormat,
        string Presets,
        string SlotFormat,
        string Save,
        string Load,
        string SavedFormat,
        string LoadedFormat,
        string EmptyFormat,
        string GlobalHp,
        string GlobalAttack,
        string ActFormat,
        string ActCustomFormat,
        string[] KindNames,
        string Hp,
        string Attack,
        string QuoteFormat)
    {
        private static readonly Labels Japanese = new(
            "倍率の変更は次の戦闘から反映されます。幕ごとの設定をオンにした幕では、全体の倍率の代わりにその幕の値を使います。",
            "マルチプレイ中は、ホストがランを始めたときの設定が全員に使われます。ここでの変更は次のランから反映されます。",
            "詳細設定(0〜100倍)",
            "戦闘中に倍率を表示する",
            "HP×{0} / 攻撃×{1}",
            "プリセット",
            "スロット{0}",
            "保存",
            "読み込み",
            "スロット{0}{1}に保存しました",
            "スロット{0}{1}を読み込みました",
            "スロット{0}は空です",
            "敵のHP倍率(全体)",
            "敵の攻撃力倍率(全体)",
            "第{0}幕",
            "第{0}幕を個別に設定する",
            new[] { "通常敵", "エリート", "ボス" },
            "HP倍率",
            "攻撃力倍率",
            "「{0}」");

        private static readonly Labels English = new(
            "Changes apply from the next combat. An Act with its own settings turned on uses them instead of the global multipliers.",
            "In multiplayer, everyone uses the host's settings from when the run started. Changes here apply from the next run.",
            "Advanced (0-100x range)",
            "Show multipliers during combat",
            "HP×{0} / ATK×{1}",
            "Presets",
            "Slot {0}",
            "Save",
            "Load",
            "Saved to slot {0}{1}",
            "Loaded slot {0}{1}",
            "Slot {0} is empty",
            "Enemy HP multiplier (all)",
            "Enemy attack multiplier (all)",
            "Act {0}",
            "Use custom multipliers in Act {0}",
            new[] { "Normal", "Elite", "Boss" },
            "HP",
            "attack",
            " \"{0}\"");

        private static readonly Labels SimplifiedChinese = new(
            "倍率的更改将从下一场战斗开始生效。开启单独设置的幕会使用该幕的数值，而不是全局倍率。",
            "多人游戏中，所有人都使用房主开始本局时的设置。此处的更改将从下一局开始生效。",
            "高级设置（0-100倍）",
            "战斗中显示倍率",
            "生命×{0} / 攻击×{1}",
            "预设",
            "槽位{0}",
            "保存",
            "读取",
            "已保存到槽位{0}{1}",
            "已读取槽位{0}{1}",
            "槽位{0}为空",
            "敌人生命倍率（全局）",
            "敌人攻击倍率（全局）",
            "第{0}幕",
            "单独设置第{0}幕",
            new[] { "普通敌人", "精英", "首领" },
            "生命倍率",
            "攻击倍率",
            "“{0}”");

        private static readonly Labels Korean = new(
            "배율 변경은 다음 전투부터 적용됩니다. 개별 설정을 켠 막에서는 전체 배율 대신 그 막의 값을 사용합니다.",
            "멀티플레이 중에는 호스트가 런을 시작할 때의 설정이 모두에게 적용됩니다. 여기서 바꾼 내용은 다음 런부터 적용됩니다.",
            "고급 설정 (0~100배)",
            "전투 중 배율 표시",
            "HP×{0} / 공격×{1}",
            "프리셋",
            "슬롯 {0}",
            "저장",
            "불러오기",
            "슬롯 {0}{1}에 저장했습니다",
            "슬롯 {0}{1}을(를) 불러왔습니다",
            "슬롯 {0}은(는) 비어 있습니다",
            "적 HP 배율 (전체)",
            "적 공격력 배율 (전체)",
            "{0}막",
            "{0}막 개별 설정",
            new[] { "일반 적", "엘리트", "보스" },
            "HP 배율",
            "공격력 배율",
            " '{0}'");

        public string Display(MultiplierSettings s) =>
            string.Format(CultureInfo.InvariantCulture, DisplayFormat, s.HpMultiplier.ToString("0.#", CultureInfo.InvariantCulture), s.AttackMultiplier.ToString("0.#", CultureInfo.InvariantCulture));

        public string Slot(int slot) => string.Format(SlotFormat, slot);

        public string Saved(int slot, string name) => string.Format(SavedFormat, slot, Quote(name));

        public string Loaded(int slot, string name) => string.Format(LoadedFormat, slot, Quote(name));

        public string Empty(int slot) => string.Format(EmptyFormat, slot);

        private string Quote(string name) => string.IsNullOrWhiteSpace(name) ? "" : string.Format(QuoteFormat, name);

        public string ActTitle(int act) => string.Format(ActFormat, act);

        public string ActCustom(int act) => string.Format(ActCustomFormat, act);

        public string Cell(EnemyKind kind, bool hp) => $"{KindNames[(int)kind]} {(hp ? Hp : Attack)}";

        /// <summary>Labels for the game's current language; anything other than these four falls back to English.</summary>
        public static Labels Current() => LocManager.Instance?.Language switch
        {
            "jpn" => Japanese,
            "zhs" => SimplifiedChinese,
            "kor" => Korean,
            _ => English,
        };

        /// <summary>Gives a plain Godot label the game's font for CJK languages, as the game's own controls do.</summary>
        public static void ApplyFont(Control control)
        {
            string? language = LocManager.Instance?.Language;
            if (language != null && FontManager.GetSubstituteFont(language, FontType.Regular) is { } font)
            {
                control.AddThemeFontOverride("font", font);
            }
        }
    }
}
