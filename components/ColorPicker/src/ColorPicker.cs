// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.Primitives;
using CommunityToolkit.WinUI.Helpers;
using System.Collections.Specialized;
using Windows.UI;
using ColorSpectrum = Microsoft.UI.Xaml.Controls.Primitives.ColorSpectrum;
using ColorPickerSlider = CommunityToolkit.WinUI.Controls.Primitives.ColorPickerSlider;
#if  WINAPPSDK || HAS_UNO && WINUI
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Dispatching;
using Colors = Microsoft.UI.Colors;
#elif WINDOWS_UWP || HAS_UNO && WINUI2
using Windows.System;
using Windows.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Colors = Windows.UI.Colors;
#endif
namespace CommunityToolkit.WinUI.Controls;

/// <summary>
/// Presents a color spectrum, a palette of colors, and color channel sliders for user selection of a color.
/// </summary>
[TemplatePart(Name = nameof(ColorPicker.AlphaChannelNumberBox),       Type = typeof(NumberBox))]
[TemplatePart(Name = nameof(ColorPicker.AlphaChannelSlider),          Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.Channel1NumberBox),           Type = typeof(NumberBox))]
[TemplatePart(Name = nameof(ColorPicker.Channel1Slider),              Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.Channel2NumberBox),           Type = typeof(NumberBox))]
[TemplatePart(Name = nameof(ColorPicker.Channel2Slider),              Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.Channel3NumberBox),           Type = typeof(NumberBox))]
[TemplatePart(Name = nameof(ColorPicker.Channel3Slider),              Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground1Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground2Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground3Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground4Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground5Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground6Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground7Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground8Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground9Border),  Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.CheckeredBackground10Border), Type = typeof(Border))]
[TemplatePart(Name = nameof(ColorPicker.ColorPanelSelector),          Type = typeof(Segmented))]
[TemplatePart(Name = nameof(ColorPicker.ColorSpectrumControl),        Type = typeof(ColorSpectrum))]
[TemplatePart(Name = nameof(ColorPicker.ColorSpectrumAlphaSlider),    Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.ColorSpectrumThirdDimensionSlider), Type = typeof(ColorPickerSlider))]
[TemplatePart(Name = nameof(ColorPicker.HexInputTextBox),             Type = typeof(TextBox))]
[TemplatePart(Name = nameof(ColorPicker.ColorModeComboBox), Type = typeof(ComboBox))]

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:Statement should not be on a single line", Justification = "Inline brackets are used to improve code readability with repeated null checks.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Whitespace is used to align code in columns for readability.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "Only template parts start with a capital letter. This differentiates them from other fields.")]
public partial class ColorPicker : Microsoft.UI.Xaml.Controls.ColorPicker
{
    internal Color CheckerBackgroundColor { get; set; } = Color.FromArgb(0x19, 0x80, 0x80, 0x80); // Overridden later

    /// <summary>
    /// The period that scheduled color updates will be applied.
    /// This is only used when updating colors using the ScheduleColorUpdate() method.
    /// Color changes made directly to the Color property will apply instantly.
    /// </summary>
    private const int ColorUpdateInterval = 30; // Milliseconds

    private long tokenColor;

    private bool callbacksConnected = false;
    private bool eventsConnected    = false;
    private bool isInitialized      = false;

    // Color information for updates
    private HsvColor?            savedHsvColor              = null;
    private Color?               savedHsvColorRgbEquivalent = null;
    private Color?               updatedRgbColor            = null;
    private DispatcherQueueTimer? dispatcherQueueTimer       = null;

    private Segmented           ColorPanelSelector;
    private ColorSpectrum     ColorSpectrumControl;
    private ColorPickerSlider ColorSpectrumAlphaSlider;
    private ColorPickerSlider ColorSpectrumThirdDimensionSlider;
    private TextBox           HexInputTextBox;
    private ComboBox          ColorModeComboBox;

    private NumberBox         Channel1NumberBox;
    private NumberBox         Channel2NumberBox;
    private NumberBox         Channel3NumberBox;
    private NumberBox         AlphaChannelNumberBox;
    private ColorPickerSlider Channel1Slider;
    private ColorPickerSlider Channel2Slider;
    private ColorPickerSlider Channel3Slider;
    private ColorPickerSlider AlphaChannelSlider;

    private ColorPreviewer ColorPreviewer;

    // Up to 10 checkered backgrounds may be used by name anywhere in the template
    private Border CheckeredBackground1Border;
    private Border CheckeredBackground2Border;
    private Border CheckeredBackground3Border;
    private Border CheckeredBackground4Border;
    private Border CheckeredBackground5Border;
    private Border CheckeredBackground6Border;
    private Border CheckeredBackground7Border;
    private Border CheckeredBackground8Border;
    private Border CheckeredBackground9Border;
    private Border CheckeredBackground10Border;

    /***************************************************************************************
     *
     * Constructor/Destructor
     *
     ***************************************************************************************/

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPicker"/> class.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ColorPicker()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.DefaultStyleKey = typeof(ColorPicker);

        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/3502
        this.DefaultStyleResourceUri = new Uri("ms-appx:///CommunityToolkit.WinUI.Controls.ColorPicker/Themes/Generic.xaml");

        // Setup collections
        this.SetValue(CustomPaletteColorsProperty, new ObservableCollection<Color>());
        this.CustomPaletteColors.CollectionChanged += CustomPaletteColors_CollectionChanged;

        this.Loaded += ColorPickerButton_Loaded;

        // Checkered background color is found only one time for performance
        // This may need to change in the future if theme changes should be supported
        this.CheckerBackgroundColor = (Color)Application.Current.Resources["SystemListLowColor"];

        this.ConnectCallbacks(true);
        this.SetDefaultPalette();
        this.StartDispatcherQueueTimer();
        this.RegisterPropertyChangedCallback(IsColorChannelTextInputVisibleProperty, OnPanelVisibilityChanged);
        this.RegisterPropertyChangedCallback(IsColorSpectrumVisibleProperty, OnPanelVisibilityChanged);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ColorPicker"/> class.
    /// </summary>
    ~ColorPicker()
    {
        this.StopDispatcherQueueTimer();
        this.CustomPaletteColors.CollectionChanged -= CustomPaletteColors_CollectionChanged;
    }

    /***************************************************************************************
     *
     * Methods
     *
     ***************************************************************************************/

    /// <summary>
    /// Gets whether or not the color is considered empty (all fields zero).
    /// In the future Color.IsEmpty will hopefully be added to UWP.
    /// </summary>
    /// <param name="color">The Windows.UI.Color to calculate with.</param>
    /// <returns>Whether the color is considered empty.</returns>
    private static bool IsColorEmpty(Color color)
    {
        return color.A == 0x00 &&
               color.R == 0x00 &&
               color.G == 0x00 &&
               color.B == 0x00;
    }

    /// <summary>
    /// Overrides when a template is applied in order to get the required controls.
    /// </summary>
    protected override void OnApplyTemplate()
    {
        // We need to disconnect old events first
        this.ConnectEvents(false);

        this.ColorPanelSelector = (Segmented)GetTemplateChild(nameof(ColorPanelSelector));

        this.ColorSpectrumControl              = (ColorSpectrum)GetTemplateChild(nameof(ColorSpectrumControl));
        this.ColorSpectrumAlphaSlider          = (ColorPickerSlider)this.GetTemplateChild(nameof(ColorSpectrumAlphaSlider));
        this.ColorSpectrumThirdDimensionSlider = (ColorPickerSlider)this.GetTemplateChild(nameof(ColorSpectrumThirdDimensionSlider));

        this.HexInputTextBox = (TextBox)this.GetTemplateChild(nameof(HexInputTextBox));
        this.ColorModeComboBox = (ComboBox)this.GetTemplateChild(nameof(ColorModeComboBox));

        this.Channel1NumberBox     = (NumberBox)this.GetTemplateChild(nameof(Channel1NumberBox));
        this.Channel2NumberBox     = (NumberBox)this.GetTemplateChild(nameof(Channel2NumberBox));
        this.Channel3NumberBox     = (NumberBox)this.GetTemplateChild(nameof(Channel3NumberBox));
        this.AlphaChannelNumberBox = (NumberBox)this.GetTemplateChild(nameof(AlphaChannelNumberBox));

        this.Channel1Slider     = (ColorPickerSlider)this.GetTemplateChild(nameof(Channel1Slider));
        this.Channel2Slider     = (ColorPickerSlider)this.GetTemplateChild(nameof(Channel2Slider));
        this.Channel3Slider     = (ColorPickerSlider)this.GetTemplateChild(nameof(Channel3Slider));
        this.AlphaChannelSlider = (ColorPickerSlider)this.GetTemplateChild(nameof(AlphaChannelSlider));

        this.ColorPreviewer = (ColorPreviewer)this.GetTemplateChild(nameof(ColorPreviewer));

        this.CheckeredBackground1Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground1Border));
        this.CheckeredBackground2Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground2Border));
        this.CheckeredBackground3Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground3Border));
        this.CheckeredBackground4Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground4Border));
        this.CheckeredBackground5Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground5Border));
        this.CheckeredBackground6Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground6Border));
        this.CheckeredBackground7Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground7Border));
        this.CheckeredBackground8Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground8Border));
        this.CheckeredBackground9Border  = (Border)this.GetTemplateChild(nameof(CheckeredBackground9Border));
        this.CheckeredBackground10Border = (Border)this.GetTemplateChild(nameof(CheckeredBackground10Border));

        // Must connect after controls are resolved
        this.ConnectEvents(true);

        base.OnApplyTemplate();
        this.UpdateVisualState(false);
        this.ValidateSelectedPanel();
        this.isInitialized = true;

        this.SetActiveColorRepresentation(ColorRepresentation.Rgba);
        this.UpdateColorControlValues(); // TODO: This will also connect events after, can we optimize vs. doing it twice with the ConnectEvents above?
    }

    /// <summary>
    /// Connects or disconnects all dependency property callbacks.
    /// </summary>
    private void ConnectCallbacks(bool connected)
    {
        if (connected == true &&
            this.callbacksConnected == false)
        {
            // Add callbacks for dependency properties
            this.tokenColor = this.RegisterPropertyChangedCallback(ColorProperty, OnColorChanged);

            this.callbacksConnected = true;
        }
        else if (connected == false &&
                 this.callbacksConnected == true)
        {
            // Remove callbacks for dependency properties
            this.UnregisterPropertyChangedCallback(ColorProperty, this.tokenColor);

            this.callbacksConnected = false;
        }

        return;
    }

    /// <summary>
    /// Connects or disconnects all control event handlers.
    /// </summary>
    /// <param name="connected">True to connect event handlers, otherwise false.</param>
    private void ConnectEvents(bool connected)
    {
        if (connected == true &&
            this.eventsConnected == false)
        {
            // Add all events
            if (this.ColorSpectrumControl != null) { this.ColorSpectrumControl.ColorChanged += ColorSpectrum_ColorChanged; }
            if (this.ColorSpectrumControl != null) { this.ColorSpectrumControl.GotFocus     += ColorSpectrum_GotFocus; }
            if (this.HexInputTextBox      != null) { this.HexInputTextBox.KeyDown           += HexInputTextBox_KeyDown; }
            if (this.HexInputTextBox      != null) { this.HexInputTextBox.LostFocus         += HexInputTextBox_LostFocus; }
            if (this.ColorModeComboBox    != null) { this.ColorModeComboBox.SelectionChanged += ColorModeComboBox_SelectionChanged; }

            if (this.Channel1NumberBox     != null) { this.Channel1NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
            if (this.Channel2NumberBox     != null) { this.Channel2NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
            if (this.Channel3NumberBox     != null) { this.Channel3NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
            if (this.AlphaChannelNumberBox != null) { this.AlphaChannelNumberBox.ValueChanged += ChannelNumberBox_ValueChanged; }

            if (this.Channel1Slider                    != null) { this.Channel1Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
            if (this.Channel2Slider                    != null) { this.Channel2Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
            if (this.Channel3Slider                    != null) { this.Channel3Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
            if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.ValueChanged                += ChannelSlider_ValueChanged; }
            if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.ValueChanged          += ChannelSlider_ValueChanged; }
            if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.ValueChanged += ChannelSlider_ValueChanged; }

            if (this.Channel1Slider                    != null) { this.Channel1Slider.Loaded                    += ChannelSlider_Loaded; }
            if (this.Channel2Slider                    != null) { this.Channel2Slider.Loaded                    += ChannelSlider_Loaded; }
            if (this.Channel3Slider                    != null) { this.Channel3Slider.Loaded                    += ChannelSlider_Loaded; }
            if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.Loaded                += ChannelSlider_Loaded; }
            if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.Loaded          += ChannelSlider_Loaded; }
            if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.Loaded += ChannelSlider_Loaded; }

            if (this.ColorPreviewer != null) { this.ColorPreviewer.ColorChangeRequested += ColorPreviewer_ColorChangeRequested; }

            if (this.CheckeredBackground1Border  != null) { this.CheckeredBackground1Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground2Border  != null) { this.CheckeredBackground2Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground3Border  != null) { this.CheckeredBackground3Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground4Border  != null) { this.CheckeredBackground4Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground5Border  != null) { this.CheckeredBackground5Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground6Border  != null) { this.CheckeredBackground6Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground7Border  != null) { this.CheckeredBackground7Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground8Border  != null) { this.CheckeredBackground8Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground9Border  != null) { this.CheckeredBackground9Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground10Border != null) { this.CheckeredBackground10Border.Loaded += CheckeredBackgroundBorder_Loaded; }

            this.eventsConnected = true;
        }
        else if (connected == false &&
                 this.eventsConnected == true)
        {
            // Remove all events
            if (this.ColorSpectrumControl != null) { this.ColorSpectrumControl.ColorChanged -= ColorSpectrum_ColorChanged; }
            if (this.ColorSpectrumControl != null) { this.ColorSpectrumControl.GotFocus     -= ColorSpectrum_GotFocus; }
            if (this.HexInputTextBox      != null) { this.HexInputTextBox.KeyDown           -= HexInputTextBox_KeyDown; }
            if (this.HexInputTextBox      != null) { this.HexInputTextBox.LostFocus         -= HexInputTextBox_LostFocus; }
            if (this.ColorModeComboBox != null) { this.ColorModeComboBox.SelectionChanged -= ColorModeComboBox_SelectionChanged; }

            if (this.Channel1NumberBox     != null) { this.Channel1NumberBox.ValueChanged     -= ChannelNumberBox_ValueChanged; }
            if (this.Channel2NumberBox     != null) { this.Channel2NumberBox.ValueChanged     -= ChannelNumberBox_ValueChanged; }
            if (this.Channel3NumberBox     != null) { this.Channel3NumberBox.ValueChanged     -= ChannelNumberBox_ValueChanged; }
            if (this.AlphaChannelNumberBox != null) { this.AlphaChannelNumberBox.ValueChanged -= ChannelNumberBox_ValueChanged; }

            if (this.Channel1Slider                    != null) { this.Channel1Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
            if (this.Channel2Slider                    != null) { this.Channel2Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
            if (this.Channel3Slider                    != null) { this.Channel3Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
            if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.ValueChanged                -= ChannelSlider_ValueChanged; }
            if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.ValueChanged          -= ChannelSlider_ValueChanged; }
            if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.ValueChanged -= ChannelSlider_ValueChanged; }

            if (this.Channel1Slider                    != null) { this.Channel1Slider.Loaded                    -= ChannelSlider_Loaded; }
            if (this.Channel2Slider                    != null) { this.Channel2Slider.Loaded                    -= ChannelSlider_Loaded; }
            if (this.Channel3Slider                    != null) { this.Channel3Slider.Loaded                    -= ChannelSlider_Loaded; }
            if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.Loaded                -= ChannelSlider_Loaded; }
            if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.Loaded          -= ChannelSlider_Loaded; }
            if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.Loaded -= ChannelSlider_Loaded; }

            if (this.ColorPreviewer != null) { this.ColorPreviewer.ColorChangeRequested -= ColorPreviewer_ColorChangeRequested; }

            if (this.CheckeredBackground1Border  != null) { this.CheckeredBackground1Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground2Border  != null) { this.CheckeredBackground2Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground3Border  != null) { this.CheckeredBackground3Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground4Border  != null) { this.CheckeredBackground4Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground5Border  != null) { this.CheckeredBackground5Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground6Border  != null) { this.CheckeredBackground6Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground7Border  != null) { this.CheckeredBackground7Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground8Border  != null) { this.CheckeredBackground8Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground9Border  != null) { this.CheckeredBackground9Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
            if (this.CheckeredBackground10Border != null) { this.CheckeredBackground10Border.Loaded -= CheckeredBackgroundBorder_Loaded; }

            this.eventsConnected = false;
        }

        return;
    }


    /// <summary>
    /// Updates all visual states based on current control properties.
    /// </summary>
    /// <param name="useTransitions">Whether transitions should occur when changing states.</param>
    private void UpdateVisualState(bool useTransitions = true)
    {
        VisualStateManager.GoToState(this, this.IsEnabled ? "Normal" : "Disabled", useTransitions);
        VisualStateManager.GoToState(this, this.GetActiveColorRepresentation() == ColorRepresentation.Hsva ? "HsvSelected" : "RgbSelected", useTransitions);
        VisualStateManager.GoToState(this, this.IsColorPaletteVisible ? "ColorPaletteVisible" : "ColorPaletteCollapsed", useTransitions);

        // Check if only a single vie is selected and hide the Segmented control
        VisualStateManager.GoToState(this, (Truth(IsColorPaletteVisible, IsColorSpectrumVisible, IsColorChannelTextInputVisible) <= 1) ? "ColorPanelSelectorCollapsed" : "ColorPanelSelectorVisible", useTransitions);
    }

    private static int Truth(params bool[] booleans)
    {
        return booleans.Count(b => b);
    }

    /// <summary>
    /// Gets the active representation of the color: HSV or RGB.
    /// </summary>
    private ColorRepresentation GetActiveColorRepresentation()
    {
        // If the HSV representation control is missing for whatever reason,
        // the default will be RGB

        if (this.ColorModeComboBox != null &&
            this.ColorModeComboBox.SelectedIndex == 1)
        {
            return ColorRepresentation.Hsva;
        }

        return ColorRepresentation.Rgba;
    }

    /// <summary>
    /// Sets the active color representation in the UI controls.
    /// </summary>
    /// <param name="colorRepresentation">The color representation to set.
    /// Setting to null (the default) will attempt to keep the current state.</param>
    private void SetActiveColorRepresentation(ColorRepresentation? colorRepresentation = null)
    {
        bool eventsDisconnectedByMethod = false;

        if (colorRepresentation == null)
        {
            // Use the control's current value
            colorRepresentation = this.GetActiveColorRepresentation();
        }

        // Disable events during the update
        if (this.eventsConnected)
        {
            this.ConnectEvents(false);
            eventsDisconnectedByMethod = true;
        }

        // Sync the UI controls and visual state
        // The default is always RGBA
        if (colorRepresentation == ColorRepresentation.Hsva)
        {
            this.ColorModeComboBox.SelectedIndex = 1;
        }
        else
        {
            this.ColorModeComboBox.SelectedIndex = 0;
        }

        this.UpdateVisualState(false);

        if (eventsDisconnectedByMethod)
        {
            this.ConnectEvents(true);
        }

        return;
    }

    /// <summary>
    /// Gets the active third dimension in the color spectrum: Hue, Saturation or Value.
    /// </summary>
    private ColorChannel GetActiveColorSpectrumThirdDimension()
    {
        switch (this.ColorSpectrumComponents)
        {
            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.SaturationValue:
            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.ValueSaturation:
            {
                // Hue
                return ColorChannel.Channel1;
            }

            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.HueValue:
            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.ValueHue:
            {
                // Saturation
                return ColorChannel.Channel2;
            }

            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.HueSaturation:
            case Microsoft.UI.Xaml.Controls.ColorSpectrumComponents.SaturationHue:
            {
                // Value
                return ColorChannel.Channel3;
            }
        }

        return ColorChannel.Alpha; // Error, should never get here
    }

    /// <summary>
    /// Declares a new color to set to the control.
    /// Application of this color will be scheduled to avoid overly rapid updates.
    /// </summary>
    /// <param name="newColor">The new color to set to the control. </param>
    private void ScheduleColorUpdate(Color newColor)
    {
        // Coerce the value as needed
        if (this.IsAlphaEnabled == false)
        {
            newColor = new Color()
            {
                R = newColor.R,
                G = newColor.G,
                B = newColor.B,
                A = 255
            };
        }

        this.updatedRgbColor = newColor;

        return;
    }

    /// <summary>
    /// Updates the color values in all editing controls to match the current color.
    /// </summary>
    private void UpdateColorControlValues()
    {
        bool eventsDisconnectedByMethod = false;
        Color rgbColor = this.Color;
        HsvColor hsvColor;

        if (this.isInitialized)
        {
            // Disable events during the update
            if (this.eventsConnected)
            {
                this.ConnectEvents(false);
                eventsDisconnectedByMethod = true;
            }

            if (this.HexInputTextBox != null)
            {
                if (this.IsAlphaEnabled)
                {
                    // Remove only the "#" sign
                    this.HexInputTextBox.Text = rgbColor.ToHex().Replace("#", string.Empty);
                }
                else
                {
                    // Remove the "#" sign and alpha hex
                    this.HexInputTextBox.Text = rgbColor.ToHex().Replace("#", string.Empty).Substring(2);
                }
            }

            // Regardless of the active color representation, the spectrum is always HSV
            // Therefore, always calculate HSV color here
            // Warning: Always maintain/use HSV information in the saved HSV color
            // This avoids loss of precision and drift caused by continuously converting to/from RGB
            if (this.savedHsvColor == null)
            {
                hsvColor = rgbColor.ToHsv();

                // Round the channels, be sure rounding matches with the scaling next
                // Rounding of SVA requires at MINIMUM 2 decimal places
                int decimals = 0;
                hsvColor = new HsvColor()
                {
                    H = Math.Round(hsvColor.H, decimals),
                    S = Math.Round(hsvColor.S, 2 + decimals),
                    V = Math.Round(hsvColor.V, 2 + decimals),
                    A = Math.Round(hsvColor.A, 2 + decimals)
                };

                // Must update HSV color
                this.savedHsvColor              = hsvColor;
                this.savedHsvColorRgbEquivalent = rgbColor;
            }
            else
            {
                hsvColor = this.savedHsvColor.Value;
            }

            // Update the color spectrum
            // Remember the spectrum is always HSV and must be updated as such to avoid
            // conversion errors
            if (this.ColorSpectrumControl != null)
            {
                this.ColorSpectrumControl.HsvColor = new System.Numerics.Vector4()
                {
                    X = Convert.ToSingle(hsvColor.H),
                    Y = Convert.ToSingle(hsvColor.S),
                    Z = Convert.ToSingle(hsvColor.V),
                    W = Convert.ToSingle(hsvColor.A)
                };
            }

            // Update the color spectrum third dimension channel
            if (this.ColorSpectrumThirdDimensionSlider != null)
            {
                // Convert the channels into a usable range for the user
                double hue         = hsvColor.H;
                double staturation = hsvColor.S * 100;
                double value       = hsvColor.V * 100;

                switch (this.GetActiveColorSpectrumThirdDimension())
                {
                    case ColorChannel.Channel1:
                    {
                        // Hue
                        this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                        this.ColorSpectrumThirdDimensionSlider.Maximum = 360;
                        this.ColorSpectrumThirdDimensionSlider.Value   = hue;
                        break;
                    }

                    case ColorChannel.Channel2:
                    {
                        // Saturation
                        this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                        this.ColorSpectrumThirdDimensionSlider.Maximum = 100;
                        this.ColorSpectrumThirdDimensionSlider.Value   = staturation;
                        break;
                    }

                    case ColorChannel.Channel3:
                    {
                        // Value
                        this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                        this.ColorSpectrumThirdDimensionSlider.Maximum = 100;
                        this.ColorSpectrumThirdDimensionSlider.Value   = value;
                        break;
                    }
                }
            }

            // Update the preview color
            if (this.ColorPreviewer != null)
            {
                this.ColorPreviewer.HsvColor = hsvColor;
            }

            // Update all other color channels
            if (this.GetActiveColorRepresentation() == ColorRepresentation.Hsva)
            {
                // Convert the channels into a usable range for the user
                double hue         = hsvColor.H;
                double staturation = hsvColor.S * 100;
                double value       = hsvColor.V * 100;
                double alpha       = hsvColor.A * 100;

                // Hue
                if (this.Channel1NumberBox != null)
                {
                    this.Channel1NumberBox.Minimum = 0;
                    this.Channel1NumberBox.Maximum = 360;
                    this.Channel1NumberBox.Value   = hue;
                }

                if (this.Channel1Slider != null)
                {
                    this.Channel1Slider.Minimum = 0;
                    this.Channel1Slider.Maximum = 360;
                    this.Channel1Slider.Value   = hue;
                }

                // Saturation
                if (this.Channel2NumberBox != null)
                {
                    this.Channel2NumberBox.Minimum = 0;
                    this.Channel2NumberBox.Maximum = 100;
                    this.Channel2NumberBox.Value   = staturation;
                }

                if (this.Channel2Slider != null)
                {
                    this.Channel2Slider.Minimum = 0;
                    this.Channel2Slider.Maximum = 100;
                    this.Channel2Slider.Value   = staturation;
                }

                // Value
                if (this.Channel3NumberBox != null)
                {
                    this.Channel3NumberBox.Minimum = 0;
                    this.Channel3NumberBox.Maximum = 100;
                    this.Channel3NumberBox.Value   = value;
                }

                if (this.Channel3Slider != null)
                {
                    this.Channel3Slider.Minimum = 0;
                    this.Channel3Slider.Maximum = 100;
                    this.Channel3Slider.Value   = value;
                }

                // Alpha
                if (this.AlphaChannelNumberBox != null)
                {
                    this.AlphaChannelNumberBox.Minimum = 0;
                    this.AlphaChannelNumberBox.Maximum = 100;
                    this.AlphaChannelNumberBox.Value   = alpha;
                }

                if (this.AlphaChannelSlider != null)
                {
                    this.AlphaChannelSlider.Minimum = 0;
                    this.AlphaChannelSlider.Maximum = 100;
                    this.AlphaChannelSlider.Value   = alpha;
                }

                // Color spectrum alpha
                if (this.ColorSpectrumAlphaSlider != null)
                {
                    this.ColorSpectrumAlphaSlider.Minimum = 0;
                    this.ColorSpectrumAlphaSlider.Maximum = 100;
                    this.ColorSpectrumAlphaSlider.Value   = alpha;
                }
            }
            else
            {
                // Red
                if (this.Channel1NumberBox != null)
                {
                    this.Channel1NumberBox.Minimum = 0;
                    this.Channel1NumberBox.Maximum = 255;
                    this.Channel1NumberBox.Value   = Convert.ToDouble(rgbColor.R);
                }

                if (this.Channel1Slider != null)
                {
                    this.Channel1Slider.Minimum = 0;
                    this.Channel1Slider.Maximum = 255;
                    this.Channel1Slider.Value   = Convert.ToDouble(rgbColor.R);
                }

                // Green
                if (this.Channel2NumberBox != null)
                {
                    this.Channel2NumberBox.Minimum = 0;
                    this.Channel2NumberBox.Maximum = 255;
                    this.Channel2NumberBox.Value   = Convert.ToDouble(rgbColor.G);
                }

                if (this.Channel2Slider != null)
                {
                    this.Channel2Slider.Minimum = 0;
                    this.Channel2Slider.Maximum = 255;
                    this.Channel2Slider.Value   = Convert.ToDouble(rgbColor.G);
                }

                // Blue
                if (this.Channel3NumberBox != null)
                {
                    this.Channel3NumberBox.Minimum = 0;
                    this.Channel3NumberBox.Maximum = 255;
                    this.Channel3NumberBox.Value   = Convert.ToDouble(rgbColor.B);
                }

                if (this.Channel3Slider != null)
                {
                    this.Channel3Slider.Minimum = 0;
                    this.Channel3Slider.Maximum = 255;
                    this.Channel3Slider.Value   = Convert.ToDouble(rgbColor.B);
                }

                // Alpha
                if (this.AlphaChannelNumberBox != null)
                {
                    this.AlphaChannelNumberBox.Minimum = 0;
                    this.AlphaChannelNumberBox.Maximum = 255;
                    this.AlphaChannelNumberBox.Value   = Convert.ToDouble(rgbColor.A);
                }

                if (this.AlphaChannelSlider != null)
                {
                    this.AlphaChannelSlider.Minimum = 0;
                    this.AlphaChannelSlider.Maximum = 255;
                    this.AlphaChannelSlider.Value   = Convert.ToDouble(rgbColor.A);
                }

                // Color spectrum alpha
                if (this.ColorSpectrumAlphaSlider != null)
                {
                    this.ColorSpectrumAlphaSlider.Minimum = 0;
                    this.ColorSpectrumAlphaSlider.Maximum = 255;
                    this.ColorSpectrumAlphaSlider.Value   = Convert.ToDouble(rgbColor.A);
                }
            }

            if (eventsDisconnectedByMethod)
            {
                this.ConnectEvents(true);
            }
        }

        return;
    }

    /// <summary>
    /// Sets a new color channel value to the current color.
    /// Only the specified color channel will be modified.
    /// </summary>
    /// <param name="colorRepresentation">The color representation of the given channel.</param>
    /// <param name="channel">The specified color channel to modify.</param>
    /// <param name="newValue">The new color channel value.</param>
    private void SetColorChannel(
        ColorRepresentation colorRepresentation,
        ColorChannel channel,
        double newValue)
    {
        Color oldRgbColor = this.Color;
        Color newRgbColor;
        HsvColor oldHsvColor;

        if (colorRepresentation == ColorRepresentation.Hsva)
        {
            // Warning: Always maintain/use HSV information in the saved HSV color
            // This avoids loss of precision and drift caused by continuously converting to/from RGB
            if (this.savedHsvColor == null)
            {
                oldHsvColor = oldRgbColor.ToHsv();
            }
            else
            {
                oldHsvColor = this.savedHsvColor.Value;
            }

            double hue        = oldHsvColor.H;
            double saturation = oldHsvColor.S;
            double value      = oldHsvColor.V;
            double alpha      = oldHsvColor.A;

            switch (channel)
            {
                case ColorChannel.Channel1:
                {
                    hue = Math.Clamp(double.IsNaN(newValue) ? 0 : newValue, 0, 360);
                    break;
                }

                case ColorChannel.Channel2:
                {
                    saturation = Math.Clamp((double.IsNaN(newValue) ? 0 : newValue) / 100, 0, 1);
                    break;
                }

                case ColorChannel.Channel3:
                {
                    value = Math.Clamp((double.IsNaN(newValue) ? 0 : newValue) / 100, 0, 1);
                    break;
                }

                case ColorChannel.Alpha:
                {
                    // Unlike color channels, default to no transparency
                    alpha = Math.Clamp((double.IsNaN(newValue) ? 100 : newValue) / 100, 0, 1);
                    break;
                }
            }

            newRgbColor = Helpers.ColorHelper.FromHsv(
                hue,
                saturation,
                value,
                alpha);

            // Must update HSV color
            this.savedHsvColor = new HsvColor()
            {
                H = hue,
                S = saturation,
                V = value,
                A = alpha
            };
            this.savedHsvColorRgbEquivalent = newRgbColor;
        }
        else
        {
            byte red   = oldRgbColor.R;
            byte green = oldRgbColor.G;
            byte blue  = oldRgbColor.B;
            byte alpha = oldRgbColor.A;

            switch (channel)
            {
                case ColorChannel.Channel1:
                {
                    red = Convert.ToByte(Math.Clamp(double.IsNaN(newValue) ? 0 : newValue, 0, 255));
                    break;
                }

                case ColorChannel.Channel2:
                {
                    green = Convert.ToByte(Math.Clamp(double.IsNaN(newValue) ? 0 : newValue, 0, 255));
                    break;
                }

                case ColorChannel.Channel3:
                {
                    blue = Convert.ToByte(Math.Clamp(double.IsNaN(newValue) ? 0 : newValue, 0, 255));
                    break;
                }

                case ColorChannel.Alpha:
                {
                    // Unlike color channels, default to no transparency
                    alpha = Convert.ToByte(Math.Clamp(double.IsNaN(newValue) ? 255 : newValue, 0, 255));
                    break;
                }
            }

            newRgbColor = new Color()
            {
                R = red,
                G = green,
                B = blue,
                A = alpha
            };

            // Must clear saved HSV color
            this.savedHsvColor              = null;
            this.savedHsvColorRgbEquivalent = null;
        }

        this.ScheduleColorUpdate(newRgbColor);
        return;
    }

    /// <summary>
    /// Updates all channel slider control backgrounds.
    /// </summary>
    private void UpdateChannelSliderBackgrounds()
    {
        this.UpdateChannelSliderBackground(this.Channel1Slider);
        this.UpdateChannelSliderBackground(this.Channel2Slider);
        this.UpdateChannelSliderBackground(this.Channel3Slider);
        this.UpdateChannelSliderBackground(this.AlphaChannelSlider);
        this.UpdateChannelSliderBackground(this.ColorSpectrumAlphaSlider);
        this.UpdateChannelSliderBackground(this.ColorSpectrumThirdDimensionSlider);
        return;
    }

    /// <summary>
    /// Updates a specific channel slider control background.
    /// </summary>
    /// <param name="slider">The color channel slider to update the background for.</param>
    private void UpdateChannelSliderBackground(ColorPickerSlider slider)
    {
        if (slider != null)
        {
            // Regardless of the active color representation, the sliders always use HSV
            // Therefore, always calculate HSV color here
            // Warning: Always maintain/use HSV information in the saved HSV color
            // This avoids loss of precision and drift caused by continuously converting to/from RGB
            if (this.savedHsvColor == null)
            {
                var rgbColor = this.Color;

                // Must update HSV color
                this.savedHsvColor = rgbColor.ToHsv();
                this.savedHsvColorRgbEquivalent = rgbColor;
            }

            slider.IsAutoUpdatingEnabled = false;

            if (object.ReferenceEquals(slider, this.Channel1Slider))
            {
                slider.ColorChannel = ColorChannel.Channel1;
                slider.ColorRepresentation = this.GetActiveColorRepresentation();
            }
            else if (object.ReferenceEquals(slider, this.Channel2Slider))
            {
                slider.ColorChannel = ColorChannel.Channel2;
                slider.ColorRepresentation = this.GetActiveColorRepresentation();
            }
            else if (object.ReferenceEquals(slider, this.Channel3Slider))
            {
                slider.ColorChannel = ColorChannel.Channel3;
                slider.ColorRepresentation = this.GetActiveColorRepresentation();
            }
            else if (object.ReferenceEquals(slider, this.AlphaChannelSlider))
            {
                slider.ColorChannel = ColorChannel.Alpha;
                slider.ColorRepresentation = this.GetActiveColorRepresentation();
            }
            else if (object.ReferenceEquals(slider, this.ColorSpectrumAlphaSlider))
            {
                slider.ColorChannel = ColorChannel.Alpha;
                slider.ColorRepresentation = this.GetActiveColorRepresentation();
            }
            else if (object.ReferenceEquals(slider, this.ColorSpectrumThirdDimensionSlider))
            {
                slider.ColorChannel = this.GetActiveColorSpectrumThirdDimension();
                slider.ColorRepresentation = ColorRepresentation.Hsva; // Always HSV
            }

            slider.HsvColor = this.savedHsvColor.Value;
            slider.UpdateColors();
        }

        return;
    }

    /// <summary>
    /// Sets the default color palette to the control.
    /// </summary>
    private void SetDefaultPalette()
    {
        this.CustomPalette = new FluentColorPalette();

        return;
    }

    /// <summary>
    /// Validates and updates the current 'tab' or 'panel' selection.
    /// If the currently selected tab is collapsed, the next visible tab will be selected.
    /// </summary>
    private void ValidateSelectedPanel()
    {
        object? selectedItem = null;

        if (this.ColorPanelSelector != null)
        {
            if (this.ColorPanelSelector.SelectedItem == null &&
                this.ColorPanelSelector.Items.Count > 0)
            {
                // As a failsafe, forcefully select the first item
                selectedItem = this.ColorPanelSelector.Items[0];
            }
            else
            {
                selectedItem = this.ColorPanelSelector.SelectedItem;
            }

            if (selectedItem is UIElement selectedElement &&
                selectedElement.Visibility == Visibility.Collapsed)
            {
                // Select the first visible item instead
                foreach (object item in this.ColorPanelSelector.Items)
                {
                    if (item is UIElement element &&
                        element.Visibility == Visibility.Visible)
                    {
                        selectedItem = item;
                        break;
                    }
                }
            }

            this.ColorPanelSelector.SelectedItem = selectedItem;
        }

        return;
    }
    private void OnPanelVisibilityChanged(DependencyObject sender, DependencyProperty dp)
    {
        this.UpdateVisualState(false);
        this.ValidateSelectedPanel();
    }

    private void OnDependencyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is DependencyObject senderControl)
        {
            /* Note: ColorProperty is defined in the base class and cannot be used here
             * See the OnColorChanged callback below
             */

            if (object.ReferenceEquals(args.Property, CustomPaletteProperty))
            {
                IColorPalette palette = this.CustomPalette;

                if (palette != null)
                {
                    this.CustomPaletteColumnCount = palette.ColorCount;
                    this.CustomPaletteColors.Clear();

                    for (int shadeIndex = 0; shadeIndex < palette.ShadeCount; shadeIndex++)
                    {
                        for (int colorIndex = 0; colorIndex < palette.ColorCount; colorIndex++)
                        {
                            this.CustomPaletteColors.Add(palette.GetColor(colorIndex, shadeIndex));
                        }
                    }
                }
            }
            else if (object.ReferenceEquals(args.Property, IsColorPaletteVisibleProperty))
            {
                this.UpdateVisualState(false);
                this.ValidateSelectedPanel();
            }
        }

        return;
    }

    /***************************************************************************************
     *
     * Color Update Timer
     *
     ***************************************************************************************/

    private void StartDispatcherQueueTimer()
    {
        this.dispatcherQueueTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        this.dispatcherQueueTimer.Interval = new TimeSpan(0, 0, 0, 0, ColorUpdateInterval);
        this.dispatcherQueueTimer.Tick += DispatcherQueueTimer_Tick;
        this.dispatcherQueueTimer.Start();

        return;
    }

    private void StopDispatcherQueueTimer()
    {
        if (this.dispatcherQueueTimer != null)
        {
            this.dispatcherQueueTimer.Stop();
        }

        return;
    }

    private void DispatcherQueueTimer_Tick(object sender, object e)
    {
        if (this.updatedRgbColor != null)
        {
            var newColor = this.updatedRgbColor.Value;

            // Clear first to avoid timing issues if it takes longer than the timer interval to set the new color
            this.updatedRgbColor = null;

            // An equality check here is important
            // Without it, OnColorChanged would continuously be invoked and preserveHsvColor overwritten when not wanted
            if (object.Equals(newColor, this.GetValue(ColorProperty)) == false)
            {
                // Disable events here so the color update isn't repeated as other controls in the UI are updated through binding.
                // For example, the Spectrum should be bound to Color, as soon as Color is changed here the Spectrum is updated.
                // Then however, the ColorSpectrum.ColorChanged event would fire which would schedule a new color update --
                // with the same color. This causes several problems:
                //   1. Layout cycle that may crash the app
                //   2. A performance hit recalculating for no reason
                //   3. preserveHsvColor gets overwritten unexpectedly by the ColorChanged handler
                this.ConnectEvents(false);
                this.SetValue(ColorProperty, newColor);
                this.ConnectEvents(true);
            }
        }

        return;
    }

    /***************************************************************************************
     *
     * Callbacks
     *
     ***************************************************************************************/

    /// <summary>
    /// Callback for when the <see cref="Microsoft.UI.Xaml.Controls.ColorPicker.Color"/> dependency property value changes.
    /// </summary>
    private void OnColorChanged(DependencyObject d, DependencyProperty e)
    {
        // TODO: Coerce the value if Alpha is disabled, is this handled in the base ColorPicker?
        if ((this.savedHsvColor != null) &&
            (object.Equals(d.GetValue(e), this.savedHsvColorRgbEquivalent) == false))
        {
            // The color was updated from an unknown source
            // The RGB and HSV colors are no longer in sync so the HSV color must be cleared
            this.savedHsvColor              = null;
            this.savedHsvColorRgbEquivalent = null;
        }

        this.UpdateColorControlValues();
        this.UpdateChannelSliderBackgrounds();

        return;
    }

    /***************************************************************************************
     *
     * Event Handling
     *
     ***************************************************************************************/

    /// <summary>
    /// Event handler for when the control has finished loaded.
    /// </summary>
    private void ColorPickerButton_Loaded(object sender, RoutedEventArgs e)
    {
        // Available but not currently used
        return;
    }

    /// <summary>
    /// Event handler for when a color channel slider is loaded.
    /// This will draw an initial background.
    /// </summary>
    private void ChannelSlider_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is ColorPickerSlider slider)
        {
            this.UpdateChannelSliderBackground(slider);
        }
        return;
    }

    /// <summary>
    /// Event handler to draw checkered backgrounds on-demand as controls are loaded.
    /// </summary>
    private async void CheckeredBackgroundBorder_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            await ColorPickerRenderingHelpers.UpdateBorderBackgroundWithCheckerAsync(
             border,
             CheckerBackgroundColor);
        }
       
    }

    /// <summary>
    /// Event handler for when the list of custom palette colors is changed.
    /// </summary>
    private void CustomPaletteColors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Available but not currently used
        return;
    }

    /// <summary>
    /// Event handler for when the color spectrum color is changed.
    /// This occurs when the user presses on the spectrum to select a new color.
    /// </summary>
    private void ColorSpectrum_ColorChanged(ColorSpectrum sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
    {
        // It is OK in this case to use the RGB representation
        this.ScheduleColorUpdate(this.ColorSpectrumControl.Color);
        return;
    }

    /// <summary>
    /// Event handler for when the color spectrum is focused.
    /// This is used only to work around some bugs that cause usability problems.
    /// </summary>
    private void ColorSpectrum_GotFocus(object sender, RoutedEventArgs e)
    {
        Color rgbColor = this.ColorSpectrumControl.Color;

        /* If this control has a color that is currently empty (#00000000),
         * selecting a new color directly in the spectrum will fail. This is
         * a bug in the color spectrum. Selecting a new color in the spectrum will
         * keep zero for all channels (including alpha and the third dimension).
         *
         * In practice this means a new color cannot be selected using the spectrum
         * until both the alpha and third dimension slider are raised above zero.
         * This is extremely user unfriendly and must be corrected as best as possible.
         *
         * In order to work around this, detect when the color spectrum has selected
         * a new color and then automatically set the alpha and third dimension
         * channel to maximum. However, the color spectrum has a second bug, the
         * ColorChanged event is never raised if the color is empty. This prevents
         * automatically setting the other channels where it normally should be done
         * (in the ColorChanged event).
         *
         * In order to work around this second bug, the GotFocus event is used
         * to detect when the spectrum is engaged by the user. It's somewhat equivalent
         * to ColorChanged for this purpose. Then when the GotFocus event is fired
         * set the alpha and third channel values to maximum. The problem here is that
         * the GotFocus event does not have access to the new color that was selected
         * in the spectrum. It is not available due to the afore mentioned bug or due to
         * timing. This means the best that can be done is to just set a 'neutral'
         * color such as white.
         *
         * There is still a small usability issue with this as it requires two
         * presses to set a color. That's far better than having to slide up both
         * sliders though.
         *
         *  1. If the color is empty, the first press on the spectrum will set white
         *     and ignore the pressed color on the spectrum
         *  2. The second press on the spectrum will be correctly handled.
         *
         */

        // In the future Color.IsEmpty will hopefully be added to UWP
        if (IsColorEmpty(rgbColor))
        {
            /* The following code may be used in the future if ever the selected color is available

            Color newColor = this.ColorSpectrum.Color;
            HsvColor newHsvColor = newColor.ToHsv();

            switch (this.GetActiveColorSpectrumThirdDimension())
            {
                case ColorChannel.Channel1:
                    {
                        newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                        (
                            360.0,
                            newHsvColor.S,
                            newHsvColor.V,
                            100.0
                        );
                        break;
                    }

                case ColorChannel.Channel2:
                    {
                        newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                        (
                            newHsvColor.H,
                            100.0,
                            newHsvColor.V,
                            100.0
                        );
                        break;
                    }

                case ColorChannel.Channel3:
                    {
                        newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                        (
                            newHsvColor.H,
                            newHsvColor.S,
                            100.0,
                            100.0
                        );
                        break;
                    }
            }
            */

            this.ScheduleColorUpdate(Colors.White);
        }
        else if (rgbColor.A == 0x00)
        {
            // As an additional usability improvement, reset alpha to maximum when the spectrum is used.
            // The color spectrum has no alpha channel and it is much more intuitive to do this for the user
            // especially if the picker was initially set with Colors.Transparent.
            this.ScheduleColorUpdate(Color.FromArgb(0xFF, rgbColor.R, rgbColor.G, rgbColor.B));
        }

        return;
    }

    /// <summary>
    /// Event handler for when the selected color representation changes.
    /// This will convert between RGB and HSV.
    /// </summary>
    private void ColorModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.ColorModeComboBox.SelectedIndex == 1)
        {
            this.SetActiveColorRepresentation(ColorRepresentation.Hsva);
        }
        else
        {
            this.SetActiveColorRepresentation(ColorRepresentation.Rgba);
        }

        this.UpdateColorControlValues();
        this.UpdateChannelSliderBackgrounds();

        return;
    }

    /// <summary>
    /// Event handler for when the color previewer requests a new color.
    /// </summary>
    private void ColorPreviewer_ColorChangeRequested(object? sender, HsvColor hsvColor)
    {
        Color rgbColor = Helpers.ColorHelper.FromHsv(hsvColor.H, hsvColor.S, hsvColor.V, hsvColor.A);

        // Regardless of the active color model, the previewer always uses HSV
        // Therefore, always calculate HSV color here
        // Warning: Always maintain/use HSV information in the saved HSV color
        // This avoids loss of precision and drift caused by continuously converting to/from RGB
        this.savedHsvColor = hsvColor;
        this.savedHsvColorRgbEquivalent = rgbColor;

        this.ScheduleColorUpdate(rgbColor);

        return;
    }

    /// <summary>
    /// Event handler for when a key is pressed within the Hex RGB value TextBox.
    /// This is used to trigger a re-evaluation of the color based on the TextBox value.
    /// </summary>
    private void HexInputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            try
            {
                ColorToHexConverter converter = new ColorToHexConverter();
                this.Color = (Color)converter.ConvertBack(((TextBox)sender).Text, typeof(TextBox), null, null);
            }
            catch
            {
                // Reset hex value
                this.UpdateColorControlValues();
                this.UpdateChannelSliderBackgrounds();
            }
        }

        return;
    }

    /// <summary>
    /// Event handler for when the Hex RGB value TextBox looses focus.
    /// This is used to trigger a re-evaluation of the color based on the TextBox value.
    /// </summary>
    private void HexInputTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
           ColorToHexConverter converter = new ColorToHexConverter();
            this.Color = (Color)converter.ConvertBack(((TextBox)sender).Text, typeof(TextBox), null, null);
        }
        catch
        {
            // Reset hex value
            this.UpdateColorControlValues();
            this.UpdateChannelSliderBackgrounds();
        }

        return;
    }

    /// <summary>
    /// Event handler for when the value within one of the channel NumberBoxes is changed.
    /// </summary>
    private void ChannelNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        double senderValue = sender.Value;

        if (object.ReferenceEquals(sender, this.Channel1NumberBox))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel1, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.Channel2NumberBox))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel2, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.Channel3NumberBox))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel3, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.AlphaChannelNumberBox))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
        }

        return;
    }

    /// <summary>
    /// Event handler for when the value within one of the channel Sliders is changed.
    /// </summary>
    private void ChannelSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        double senderValue = (sender as Slider)?.Value ?? double.NaN;

        if (object.ReferenceEquals(sender, this.Channel1Slider))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel1, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.Channel2Slider))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel2, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.Channel3Slider))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel3, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.AlphaChannelSlider))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.ColorSpectrumAlphaSlider))
        {
            this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
        }
        else if (object.ReferenceEquals(sender, this.ColorSpectrumThirdDimensionSlider))
        {
            // Regardless of the active color representation, the spectrum is always HSV
            this.SetColorChannel(ColorRepresentation.Hsva, this.GetActiveColorSpectrumThirdDimension(), senderValue);
        }

        return;
    }
}
