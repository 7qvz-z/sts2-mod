using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using MegaCrit.Sts2.Core.Localization;

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
    private const string Number = @"[0-9]{0,3}(\.[0-9]?)?";

    // Restore Defaults resets the multipliers only (spec 2.6), not the mode or the per-Act switches.
    [ConfigIgnoreRestoreDefaults] public static bool AdvancedMode { get; set; }

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

    public static double RangeMin => AdvancedMode ? AdvancedMin : NormalMin;
    public static double RangeMax => AdvancedMode ? AdvancedMax : NormalMax;

    private static readonly EnemyKind[] Kinds = { EnemyKind.Normal, EnemyKind.Elite, EnemyKind.Boss };

    private readonly MultiplierField _globalHp = new(nameof(HpMultiplier), act: 0);
    private readonly MultiplierField _globalAttack = new(nameof(AttackMultiplier), act: 0);
    private readonly List<MultiplierField> _fields = new();
    private readonly bool[] _lastCustom = new bool[ActCount + 1];
    private bool _lastAdvanced;
    private bool _syncing;

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
        ConfigChanged += (_, _) => OnConfigChanged();
    }

    /// <summary>
    /// The multipliers for a combat in the given Act (0-based run act index) against the given enemy kind.
    /// Acts beyond the third use the third Act's settings.
    /// </summary>
    public static MultiplierSettings Snapshot(int actIndex, EnemyKind kind)
    {
        int act = Math.Clamp(actIndex + 1, 1, ActCount);
        if (!IsCustom(act))
        {
            return new MultiplierSettings(ToMultiplier(HpMultiplier), ToMultiplier(AttackMultiplier));
        }
        return new MultiplierSettings(
            ToMultiplier(GetDouble(CellName(act, kind, hp: true))),
            ToMultiplier(GetDouble(CellName(act, kind, hp: false))));
    }

    private static string CellName(int act, EnemyKind kind, bool hp) => $"Act{act}{kind}{(hp ? "Hp" : "Attack")}";

    private static bool IsCustom(int act) => (bool)Property($"Act{act}Custom").GetValue(null)!;

    private static double GetDouble(string name) => (double)Property(name).GetValue(null)!;

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
        AddRow(optionContainer, optionContainer, nameof(AdvancedMode), t.Advanced, CreateRawTickboxControl(Property(nameof(AdvancedMode))));
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

    private sealed record Labels(
        string Note,
        string Advanced,
        string GlobalHp,
        string GlobalAttack,
        string ActFormat,
        string ActCustomFormat,
        string[] KindNames,
        string Hp,
        string Attack)
    {
        private static readonly Labels Japanese = new(
            "倍率の変更は次の戦闘から反映されます。幕ごとの設定をオンにした幕では、全体の倍率の代わりにその幕の値を使います。",
            "詳細設定(0〜100倍)",
            "敵のHP倍率(全体)",
            "敵の攻撃力倍率(全体)",
            "第{0}幕",
            "第{0}幕を個別に設定する",
            new[] { "通常敵", "エリート", "ボス" },
            "HP倍率",
            "攻撃力倍率");

        private static readonly Labels English = new(
            "Changes apply from the next combat. An Act with its own settings turned on uses them instead of the global multipliers.",
            "Advanced (0-100x range)",
            "Enemy HP multiplier (all)",
            "Enemy attack multiplier (all)",
            "Act {0}",
            "Use custom multipliers in Act {0}",
            new[] { "Normal", "Elite", "Boss" },
            "HP",
            "attack");

        public string ActTitle(int act) => string.Format(ActFormat, act);

        public string ActCustom(int act) => string.Format(ActCustomFormat, act);

        public string Cell(EnemyKind kind, bool hp) => $"{KindNames[(int)kind]} {(hp ? Hp : Attack)}";

        // TODO(step 6): Simplified Chinese and Korean; move to localization files.
        public static Labels Current() => LocManager.Instance?.Language == "jpn" ? Japanese : English;
    }
}
