using System;
using System.Globalization;
using System.Reflection;
using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using MegaCrit.Sts2.Core.Localization;

namespace SpireScaler;

/// <summary>
/// Settings shown in BaseLib's mod config screen. BaseLib saves the static properties to
/// user://mod_configs/SpireScaler.cfg, which is the only source of the multipliers.
/// </summary>
public sealed class SpireScalerConfig : SimpleModConfig
{
    public const double NormalMin = 1.0;
    public const double NormalMax = 10.0;
    public const double AdvancedMin = 0.0;
    public const double AdvancedMax = 100.0;
    public const double SliderStep = 0.01;

    // Restore Defaults resets the multipliers only (spec 2.6), not the mode.
    [ConfigIgnoreRestoreDefaults]
    public static bool AdvancedMode { get; set; }

    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = "x{0:0.##}")]
    public static double HpMultiplier { get; set; } = 1.0;

    [ConfigSlider(NormalMin, NormalMax, SliderStep, Format = "x{0:0.##}")]
    public static double AttackMultiplier { get; set; } = 1.0;

    // Backing text for the numeric inputs; not saved, always derived from the values above.
    [ConfigIgnore]
    [ConfigTextInput(@"[0-9]{0,3}(\.[0-9]{0,2})?", MaxLength = 6)]
    public static string HpMultiplierInput { get; set; } = "1";

    [ConfigIgnore]
    [ConfigTextInput(@"[0-9]{0,3}(\.[0-9]{0,2})?", MaxLength = 6)]
    public static string AttackMultiplierInput { get; set; } = "1";

    public static double RangeMin => AdvancedMode ? AdvancedMin : NormalMin;
    public static double RangeMax => AdvancedMode ? AdvancedMax : NormalMax;

    private readonly MultiplierField _hp;
    private readonly MultiplierField _attack;
    private bool _lastAdvanced;
    private bool _syncing;

    public SpireScalerConfig()
    {
        _hp = new MultiplierField(nameof(HpMultiplier), nameof(HpMultiplierInput));
        _attack = new MultiplierField(nameof(AttackMultiplier), nameof(AttackMultiplierInput));
        _lastAdvanced = AdvancedMode;
        ClampToRange();
        _hp.SyncTextFromValue();
        _attack.SyncTextFromValue();
        ConfigChanged += (_, _) => OnConfigChanged();
    }

    /// <summary>The multipliers as currently saved, for snapshotting at combat start.</summary>
    public static MultiplierSettings Snapshot()
    {
        return new MultiplierSettings(ToMultiplier(HpMultiplier), ToMultiplier(AttackMultiplier));
    }

    private static decimal ToMultiplier(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1m;
        }
        return Math.Round((decimal)Math.Clamp(value, AdvancedMin, AdvancedMax), 2);
    }

    public override void SetupConfigUI(Control optionContainer)
    {
        _lastAdvanced = AdvancedMode;
        Labels t = Labels.Current();

        MegaCrit.Sts2.addons.mega_text.MegaRichTextLabel note = CreateRawLabelControl(t.Note, 22);
        optionContainer.AddChild(note);

        AddRow(optionContainer, nameof(AdvancedMode), t.Advanced, CreateRawTickboxControl(Property(nameof(AdvancedMode))));
        AddField(optionContainer, _hp, t.Hp, t.HpInput);
        AddField(optionContainer, _attack, t.Attack, t.AttackInput);

        AddRestoreDefaultsButton(optionContainer);
        SetupFocusNeighbors(optionContainer);
    }

    protected override void RestoreDefaultsNoConfirm()
    {
        base.RestoreDefaultsNoConfirm();
        RunSync(() =>
        {
            ClampToRange();
            _hp.SyncAll();
            _attack.SyncAll();
        });
    }

    private void AddField(Control container, MultiplierField field, string sliderLabel, string inputLabel)
    {
        NConfigSlider slider = CreateRawSliderControl(field.ValueProperty);
        slider.SetRange(RangeMin, RangeMax, SliderStep);
        field.Slider = slider;
        AddRow(container, field.ValueProperty.Name, sliderLabel, slider);

        NConfigLineEdit input = CreateRawLineEditControl(field.TextProperty);
        input.FocusExited += () => RunSync(field.SyncTextFromValue);
        field.Input = input;
        AddRow(container, field.TextProperty.Name, inputLabel, input);
    }

    private void AddRow(Control container, string name, string labelText, Control control)
    {
        if (container.GetChildCount() > 1)
        {
            container.AddChild(CreateDividerControl());
        }
        var row = new NConfigOptionRow(ModPrefix, name, CreateRawLabelControl(labelText, 28), control);
        row.UniqueNameInOwner = true;
        container.AddChild(row);
        row.Owner = container;
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
                _hp.ApplyRange();
                _attack.ApplyRange();
                if (!AdvancedMode)
                {
                    // Spec 2.2: turning advanced mode off resets every multiplier to 1.0.
                    HpMultiplier = 1.0;
                    AttackMultiplier = 1.0;
                }
                ClampToRange();
                _hp.SyncAll();
                _attack.SyncAll();
                return;
            }
            _hp.Sync();
            _attack.Sync();
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

    private static void ClampToRange()
    {
        HpMultiplier = Clamp(HpMultiplier);
        AttackMultiplier = Clamp(AttackMultiplier);
    }

    private static double Clamp(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1.0;
        }
        return Math.Round(Math.Clamp(value, RangeMin, RangeMax), 2);
    }

    private static PropertyInfo Property(string name)
    {
        return typeof(SpireScalerConfig).GetProperty(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(nameof(SpireScalerConfig), name);
    }

    private static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Keeps one multiplier's slider and numeric input in agreement.</summary>
    private sealed class MultiplierField
    {
        public readonly PropertyInfo ValueProperty;
        public readonly PropertyInfo TextProperty;
        public NConfigSlider? Slider;
        public NConfigLineEdit? Input;
        private double _lastValue;
        private string _lastText = "";

        public MultiplierField(string valueName, string textName)
        {
            ValueProperty = Property(valueName);
            TextProperty = Property(textName);
        }

        private double Value
        {
            get => (double)ValueProperty.GetValue(null)!;
            set => ValueProperty.SetValue(null, value);
        }

        private string Text
        {
            get => (string?)TextProperty.GetValue(null) ?? "";
            set => TextProperty.SetValue(null, value);
        }

        /// <summary>Called after any config change: whichever control moved updates the other.</summary>
        public void Sync()
        {
            if (Value != _lastValue)
            {
                SyncTextFromValue();
            }
            else if (Text != _lastText)
            {
                _lastText = Text;
                if (double.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double typed))
                {
                    // Out-of-range input moves the slider to the limit; the text is corrected on focus exit.
                    Value = Clamp(typed);
                    _lastValue = Value;
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
            _lastValue = Value;
            Text = Format(Value);
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

    private sealed record Labels(string Note, string Advanced, string Hp, string HpInput, string Attack, string AttackInput)
    {
        private static readonly Labels Japanese = new(
            "倍率の変更は次の戦闘から反映されます。",
            "詳細設定(0〜100倍)",
            "敵のHP倍率",
            "敵のHP倍率(数値入力)",
            "敵の攻撃力倍率",
            "敵の攻撃力倍率(数値入力)");

        private static readonly Labels English = new(
            "Changes apply from the next combat.",
            "Advanced (0-100x range)",
            "Enemy HP multiplier",
            "Enemy HP multiplier (type a value)",
            "Enemy attack multiplier",
            "Enemy attack multiplier (type a value)");

        // TODO(step 6): Simplified Chinese and Korean; move to localization files.
        public static Labels Current() => LocManager.Instance?.Language == "jpn" ? Japanese : English;
    }
}
