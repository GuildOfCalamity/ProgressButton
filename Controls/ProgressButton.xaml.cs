using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ProgressButtonDemo.Controls;

/// <summary>
/// TODO: We could create a version of this by inheriting from <see cref="ButtonBase"/> instead of <see cref="UserControl"/>.
/// Checkout the "\WindowsCommunityToolkit-rel-winui-7.1.2\CommunityToolkit.WinUI.UI.Controls.Media\Eyedropper" control
/// for more information on creating a <see cref="ButtonBase"/> control (it's more involved than this example).
/// </summary>
public sealed partial class ProgressButton : UserControl
{
    Compositor? _compositor;
    float ctrlOffsetX = 0; // Store the grid's initial offset for later animation.

    /// <summary>
    /// This control offers the flexibility for an <see cref="Action"/> to be bound to the control 
    /// or an <see cref="ICommand"/>. The <see cref="ButtonBusy"/> property determines if the
    /// <see cref="ProgressBar"/> is shown while the command or action is executing.
    /// </summary>
    public ProgressButton()
    {
        this.InitializeComponent();
        this.Loaded += ProgressButton_Loaded;
        this.SizeChanged += ProgressButton_SizeChanged;
    }

    #region [Dependency Properties]
    /// <summary>
    /// The button's click event property.
    /// </summary>
    public Action? ButtonEvent
    {
        get => (Action?)GetValue(ButtonEventProperty);
        set => SetValue(ButtonEventProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonEvent
    /// </summary>
    public static readonly DependencyProperty ButtonEventProperty = DependencyProperty.Register(
        nameof(ButtonEvent),
        typeof(Action),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonEventPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="Action"/> object contained within.
    /// </summary>
    static void OnButtonEventPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is Action act)
        {
            Debug.WriteLine($"[OnButtonEventPropertyChanged] => {act}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// The button's <see cref="ICommand"/>.
    /// </summary>
    public ICommand? ButtonCommand
    {
        get => (ICommand?)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonCommand
    /// </summary>
    public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
        nameof(ButtonCommand),
        typeof(ICommand),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonCommandPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="ICommand"/> object contained within.
    /// </summary>
    static void OnButtonCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is ICommand cmd)
        {
            Debug.WriteLine($"[OnButtonCommandPropertyChanged] => {cmd}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// The button's command parameter.
    /// </summary>
    public object? ButtonParameter
    {
        get => (object?)GetValue(ButtonParameterProperty);
        set => SetValue(ButtonParameterProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonCommand
    /// </summary>
    public static readonly DependencyProperty ButtonParameterProperty = DependencyProperty.Register(
        nameof(ButtonParameter),
        typeof(object),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonParameterPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="object"/> object contained within.
    /// </summary>
    static void OnButtonParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is object obj)
        {
            Debug.WriteLine($"[OnButtonParameterPropertyChanged] => {obj}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// The text content to display.
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonText
    /// </summary>
    public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
        nameof(ButtonText),
        typeof(string),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonTextPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="string"/> object contained within.
    /// </summary>
    static void OnButtonTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is string str)
        {
            Debug.WriteLine($"[OnButtonTextPropertyChanged] => {str}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// If true, the progress bar is shown.
    /// </summary>
    public bool ButtonBusy
    {
        get => (bool)GetValue(ButtonBusyProperty);
        set => SetValue(ButtonBusyProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBusy
    /// </summary>
    public static readonly DependencyProperty ButtonBusyProperty = DependencyProperty.Register(
        nameof(ButtonBusy),
        typeof(bool),
        typeof(ProgressButton),
        new PropertyMetadata(false, OnButtonBusyPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="bool"/> object contained within.
    /// </summary>
    static void OnButtonBusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is bool bl)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnBusyChanged((bool)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }
    void OnBusyChanged(bool newValue)
    {
        if (newValue)
        {
            ThisProgress.Visibility = Visibility.Visible;
            if (EnableSpringAnimation) // force a return to original position
                AnimateGridX(ThisGrid, ctrlOffsetX);
        }
        else
        {
            ThisProgress.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// The progress bar button's value.
    /// If the value property is used then the min/max will be defaulted to 0/100 respectively.
    /// </summary>
    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }
    /// <summary>
    /// Backing property for ProgressValue
    /// </summary>
    public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(
        nameof(ProgressValue),
        typeof(double),
        typeof(ProgressButton),
        new PropertyMetadata(0d, OnProgressValuePropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="double"/> object contained within.
    /// </summary>
    static void OnProgressValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is double dbl)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnValueChanged((double)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }
    /// <summary>
    /// Trying to add some automated smarts to the control; if the value property is set 
    /// then the <see cref="ProgressBar"/> will assume it is not in an indeterminate mode.
    /// </summary>
    void OnValueChanged(double newValue)
    {
        if (ThisProgress.IsIndeterminate)
        {
            ThisProgress.IsIndeterminate = false;
            ThisProgress.Minimum = 0d;
            ThisProgress.Maximum = 100d;
        }
        ThisProgress.Value = newValue;
    }

    /// <summary>
    /// The color of the progress bar.
    /// </summary>
    public SolidColorBrush ProgressBrush
    {
        get => (SolidColorBrush)GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ProgressBrush
    /// </summary>
    public static readonly DependencyProperty ProgressBrushProperty = DependencyProperty.Register(
        nameof(ProgressBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.SpringGreen), OnProgressBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnProgressBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnProgressBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnProgressBrushChanged(SolidColorBrush newValue)
    {
        ThisProgress.Foreground = newValue;
    }

    /// <summary>
    /// The color of the button's background.
    /// </summary>
    public SolidColorBrush ButtonBackgroundBrush
    {
        get => (SolidColorBrush)GetValue(ButtonBackgroundBrushProperty);
        set => SetValue(ButtonBackgroundBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBackgroundBrush
    /// </summary>
    public static readonly DependencyProperty ButtonBackgroundBrushProperty = DependencyProperty.Register(
        nameof(ButtonBackgroundBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.Transparent), OnButtonBackgroundBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnButtonBackgroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonBackgroundBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonBackgroundBrushChanged(SolidColorBrush newValue)
    {
        ThisButton.Background = newValue;
    }

    /// <summary>
    /// The color of the button's foreground.
    /// </summary>
    public SolidColorBrush ButtonForegroundBrush
    {
        get => (SolidColorBrush)GetValue(ButtonForegroundBrushProperty);
        set => SetValue(ButtonForegroundBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBackgroundBrush
    /// </summary>
    public static readonly DependencyProperty ButtonForegroundBrushProperty = DependencyProperty.Register(
        nameof(ButtonForegroundBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.White), OnButtonForegroundBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnButtonForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonForegroundBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonForegroundBrushChanged(SolidColorBrush newValue)
    {
        ThisButton.Foreground = newValue;
    }

    /// <summary>
    /// The CornerRadius of the control.
    /// </summary>
    public CornerRadius ButtonCornerRadius
    {
        get => (CornerRadius)GetValue(ButtonCornerRadiusProperty);
        set => SetValue(ButtonCornerRadiusProperty, value);
    }
    /// <summary>
    /// CornerRadius property for ProgressBrush
    /// </summary>
    public static readonly DependencyProperty ButtonCornerRadiusProperty = DependencyProperty.Register(
        nameof(ButtonCornerRadius),
        typeof(CornerRadius),
        typeof(ProgressButton),
        new PropertyMetadata(new CornerRadius(4), OnButtonCornerRadiusPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="CornerRadius"/> object contained within.
    /// </summary>
    static void OnButtonCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is CornerRadius cr)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonCornerRadiusChanged((CornerRadius)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WRONG_TYPE] => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonCornerRadiusChanged(CornerRadius newValue)
    {
        // Make sure they match each other.
        ThisProgress.CornerRadius = newValue;
        ThisButton.CornerRadius = newValue;
    }

    /// <summary>
    /// The button's width value.
    /// </summary>
    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonWidth
    /// </summary>
    public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(
        nameof(ButtonWidth),
        typeof(double),
        typeof(ProgressButton),
        new PropertyMetadata(150d, OnButtonWidthPropertyChanged));

    static void OnButtonWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is double value)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonWidthChanged((double)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonWidthChanged(double newValue)
    {
        ThisButton.Width = newValue;
    }

    /// <summary>
    /// The button's height value.
    /// </summary>
    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonWidth
    /// </summary>
    public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register(
        nameof(ButtonHeight),
        typeof(double),
        typeof(ProgressButton),
        new PropertyMetadata(40d, OnButtonHeightPropertyChanged));

    static void OnButtonHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is double value)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonHeightChanged((double)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonHeightChanged(double newValue)
    {
        ThisButton.Height = newValue;
    }

    /// <summary>
    /// If true, the <see cref="SpringScalarNaturalMotionAnimation"/> is active.
    /// </summary>
    public bool EnableSpringAnimation
    {
        get => (bool)GetValue(EnableSpringAnimationProperty);
        set => SetValue(EnableSpringAnimationProperty, value);
    }
    /// <summary>
    /// Backing property for EnabledSpringAnimation
    /// </summary>
    public static readonly DependencyProperty EnableSpringAnimationProperty = DependencyProperty.Register(
        nameof(EnableSpringAnimation),
        typeof(bool),
        typeof(ProgressButton),
        new PropertyMetadata(true, OnEnableSpringAnimationPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="bool"/> object contained within.
    /// </summary>
    static void OnEnableSpringAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is bool bl)
        {
            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnEnableSpringAnimationChanged((bool)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }
    void OnEnableSpringAnimationChanged(bool newValue)
    {
        Debug.WriteLine($"[INFO] SpringAnimationChanged => {newValue}");
    }
    #endregion

    #region [Control Events]
    void ProgressButton_Loaded(object sender, RoutedEventArgs e)
    {
        if (ThisButton.ActualHeight != double.NaN && ThisButton.ActualHeight > 0)
        {
            ThisProgress.MinHeight = ThisButton.ActualHeight;
            // We're setting this in the DP now.
            //ThisProgress.CornerRadius = ThisButton.CornerRadius;
        }
        else
        {
            ThisProgress.MinHeight = ThisButton.MinHeight = 30;
            ThisProgress.CornerRadius = ThisButton.CornerRadius = new CornerRadius(4);
        }
        // Get rid of the single line that appears when using the non-indeterminate mode.
        ThisProgress.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        // Keep this in the control's loaded event and not the constructor.
        if (EnableSpringAnimation)
        {
            ThisGrid.Loaded += (s, e) =>
            {   // It seems when the grid's offset is modified from Grid/Stack centering,
                // we must force an animation to run to setup the initial starting conditions.
                // If you skip this step then you'll have to mouse-over the grid twice to
                // see the intended animation (for first run only).
                ctrlOffsetX = ThisGrid.ActualOffset.X;
                AnimateGridX(ThisGrid, ctrlOffsetX);
            };
            ThisGrid.PointerEntered += (s, e) =>
            {
                if (!ButtonBusy)
                    AnimateGridX(ThisGrid, ctrlOffsetX + 4f);
            };
            ThisGrid.PointerExited += (s, e) =>
            {
                if (!ButtonBusy)
                    AnimateGridX(ThisGrid, ctrlOffsetX);
            };
        }
    }

    void ProgressButton_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ThisButton.ActualHeight != double.NaN && ThisButton.ActualHeight > 0)
        {
            ThisProgress.MinHeight = ThisButton.ActualHeight;
            // We're setting this in the DP now.
            //ThisProgress.CornerRadius = ThisButton.CornerRadius;
        }
        else
        {
            ThisProgress.MinHeight = ThisButton.MinHeight = 30;
            ThisProgress.CornerRadius = ThisButton.CornerRadius = new CornerRadius(4);
        }
    }

    /// <summary>
    /// Handles the <see cref="ICommand"/> or <see cref="Action"/>
    /// that is bound to the <see cref="ProgressButton"/>.
    /// </summary>
    void ThisButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Has the user bound an Action to the click event?
            if (ButtonEvent != null)
                ButtonEvent.Invoke();

            // Has the user bound an ICommand to the click event?
            if (ButtonParameter != null)
                ButtonCommand?.Execute(ButtonParameter);
            else
                ButtonCommand?.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CLICK_ERROR] => {ex.Message}");
            //Debugger.Break();
        }
    }

    /// <summary>
    /// Ensures the button starts with Offset.Y = 0
    /// </summary>
    public void InitializeButtonOffsetY(Button button)
    {
        if (button == null) { return; }
        Visual buttonVisual = ElementCompositionPreview.GetElementVisual(button);
        buttonVisual.Offset = new System.Numerics.Vector3(buttonVisual.Offset.X, 0, buttonVisual.Offset.Z);
    }
    public void AnimateButtonY(Button button, float offset)
    {
        if (button == null)
            return;

        if (_compositor == null)
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

        // Create a Spring Animation for Y offset
        SpringScalarNaturalMotionAnimation _springAnimation = _compositor.CreateSpringScalarAnimation();
        _springAnimation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        _springAnimation.Target = "Offset.Y";   // Move vertically
        _springAnimation.InitialVelocity = 50f; // Adjust movement speed
        _springAnimation.FinalValue = offset;   // Set the final target Y position
        _springAnimation.DampingRatio = 0.3f;   // Lower values are more "springy"
        _springAnimation.Period = TimeSpan.FromMilliseconds(50);

        // Get the button's visual and apply the animation
        Visual buttonVisual = ElementCompositionPreview.GetElementVisual(button);
        buttonVisual.StartAnimation("Offset.Y", _springAnimation);
    }

    /// <summary>
    /// Ensures the button starts with Offset.X = 0
    /// </summary>
    public void InitializeButtonOffsetX(Button button)
    {
        if (button == null) { return; }
        Visual buttonVisual = ElementCompositionPreview.GetElementVisual(button);
        buttonVisual.Offset = new System.Numerics.Vector3(0, buttonVisual.Offset.Y, buttonVisual.Offset.Z);
    }
    public void AnimateButtonX(Button button, float offset)
    {
        if (button == null)
            return;

        if (_compositor == null)
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

        // Create a Spring Animation for X offset
        SpringScalarNaturalMotionAnimation _springAnimation = _compositor.CreateSpringScalarAnimation();
        _springAnimation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        _springAnimation.Target = "Offset.X";   // Move vertically
        _springAnimation.InitialVelocity = 50f; // Adjust movement speed
        _springAnimation.FinalValue = offset;   // Set the final target X position
        _springAnimation.DampingRatio = 0.3f;   // Lower values are more "springy"
        _springAnimation.Period = TimeSpan.FromMilliseconds(50);

        // Get the button's visual and apply the animation
        Visual buttonVisual = ElementCompositionPreview.GetElementVisual(button);
        buttonVisual.StartAnimation("Offset.X", _springAnimation);
    }

    /// <summary>
    /// Ensures the grid starts with Offset.X = 0
    /// </summary>
    public void InitializeGridOffsetX(Grid grid)
    {
        if (grid == null) { return; }
        Visual gridVisual = ElementCompositionPreview.GetElementVisual(grid);
        gridVisual.Offset = new System.Numerics.Vector3(0, gridVisual.Offset.Y, gridVisual.Offset.Z);
    }
    public void AnimateGridX(Grid grid, float offset)
    {
        if (grid == null)
            return;

        if (_compositor == null)
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

        // Create a Spring Animation for X offset
        SpringScalarNaturalMotionAnimation _springAnimation = _compositor.CreateSpringScalarAnimation();
        _springAnimation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        _springAnimation.Target = "Offset.X";   // Move vertically
        _springAnimation.InitialVelocity = 50f; // Adjust movement speed
        _springAnimation.FinalValue = offset;   // Set the final target X position
        _springAnimation.DampingRatio = 0.3f;   // Lower values are more "springy"
        _springAnimation.Period = TimeSpan.FromMilliseconds(50);

        // Get the button's visual and apply the animation
        Visual gridVisual = ElementCompositionPreview.GetElementVisual(grid);
        gridVisual.StartAnimation("Offset.X", _springAnimation);
    }
    #endregion
}
