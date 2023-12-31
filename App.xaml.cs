﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProgressButtonDemo;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	#region [Properties]
	private Window? _window;
	private static UISettings _UISettings = new UISettings();
	public static bool IsClosing { get; set; } = false;
    public static IntPtr WindowHandle { get; set; }
    public static FrameworkElement? MainRoot { get; set; }
	public static bool AnimationsEffectsEnabled
	{
		get => _UISettings.AnimationsEnabled;
	}

#if IS_UNPACKAGED // We're using a custom PropertyGroup Condition we defined in the csproj to help us with the decision.
	public static bool IsPackaged { get => false; }
#else
    public static bool IsPackaged { get => true; }
#endif
	#endregion

	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        #region [Exception handlers]
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        UnhandledException += ApplicationUnhandledException;
        #endregion

        IoCServices = ConfigureServices();

        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();

        // Save the FrameworkElement for future content dialogs.
        App.MainRoot = _window.Content as FrameworkElement;

        var AppWin = GetAppWindow(_window);
        if (AppWin != null)
        {
            AppWin.Closing += (s, e) =>
            {
                App.IsClosing = true;
            };

            // NOTE: png to ico converter https://www.img2go.com/convert/png-to-ico

            if (App.IsPackaged)
                AppWin.SetIcon(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets/Win10Logo.ico"));
            else
                AppWin.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets/Win10Logo.ico"));
        }

        _window.Activate();

        AppWin?.Resize(new Windows.Graphics.SizeInt32(1200, 800));
        CenterWindow(_window);
    }

    #region [IoC]
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    /// <example>
    /// var logger = App.Current.IoCServices.GetService{ILogger}();
    /// </example>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider IoCServices
    {
        get;
    }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<MainPageViewModel>();
        services.AddSingleton<MainPage>();
        Debug.WriteLine($"Building {services.Count} services.");
        return services.BuildServiceProvider();
    }
    #endregion

    #region [Domain Events]
    void ApplicationUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        Exception? ex = e.Exception;
        Debug.WriteLine($"[UnhandledException]: {e.Message}");
        Debug.WriteLine($"Unhandled exception of type {ex?.GetType()}: {ex}", $"{nameof(App)}");
        e.Handled = true;
    }

    void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        if (!IsClosing)
            IsClosing = true;

        if (sender is null)
            return;

        if (sender is AppDomain ad)
        {
            Debug.WriteLine($"[OnProcessExit]", $"{nameof(App)}");
            Debug.WriteLine($"DomainID: {ad.Id}", $"{nameof(App)}");
            Debug.WriteLine($"FriendlyName: {ad.FriendlyName}", $"{nameof(App)}");
            Debug.WriteLine($"BaseDirectory: {ad.BaseDirectory}", $"{nameof(App)}");
        }
    }

    void CurrentDomainFirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        Debug.WriteLine($"First chance exception: {e.Exception}", $"{nameof(App)}");
    }

    void CurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        Debug.WriteLine($"Thread exception of type {ex?.GetType()}: {ex}", $"{nameof(App)}");
    }

    void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Debug.WriteLine($"Unobserved task exception: {e.Exception}", $"{nameof(App)}");
        e.SetObserved(); // suppress and handle manually
    }
    #endregion

    #region [Window Helpers]
    /// <summary>
    /// This code example demonstrates how to retrieve an AppWindow from a WinUI3 window.
    /// The AppWindow class is available for any top-level HWND in your app.
    /// AppWindow is available only to desktop apps (both packaged and unpackaged), it's not available to UWP apps.
    /// https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/windowing/windowing-overview
    /// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.appwindow.create?view=windows-app-sdk-1.3
    /// </summary>
    public Microsoft.UI.Windowing.AppWindow? GetAppWindow(object window)
    {
        // Retrieve the window handle (HWND) of the current (XAML) WinUI3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // For other classes to use.
        App.WindowHandle = hWnd;

        // Retrieve the WindowId that corresponds to hWnd.
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

        // Lastly, retrieve the AppWindow for the current (XAML) WinUI3 window.
        Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            // You now have an AppWindow object, and you can call its methods to manipulate the window.
            // As an example, let's change the title text of the window: appWindow.Title = "Title text updated via AppWindow!";
            //appWindow.Move(new Windows.Graphics.PointInt32(200, 100));
            appWindow?.MoveAndResize(new Windows.Graphics.RectInt32(250, 100, 1300, 800), Microsoft.UI.Windowing.DisplayArea.Primary);
        }

        return appWindow;
    }

    /// <summary>
    /// Centers a <see cref="Microsoft.UI.Xaml.Window"/> based on the <see cref="Microsoft.UI.Windowing.DisplayArea"/>.
    /// </summary>
    public void CenterWindow(Window window)
    {
        try
        {
            System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            if (Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId) is Microsoft.UI.Windowing.AppWindow appWindow &&
                Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest) is Microsoft.UI.Windowing.DisplayArea displayArea)
            {
                Windows.Graphics.PointInt32 CenteredPosition = appWindow.Position;
                CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                appWindow.Move(CenteredPosition);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// The <see cref="Microsoft.UI.Windowing.DisplayArea"/> exposes properties such as:
    /// OuterBounds     (Rect32)
    /// WorkArea.Width  (int)
    /// WorkArea.Height (int)
    /// IsPrimary       (bool)
    /// DisplayId.Value (ulong)
    /// </summary>
    /// <param name="window"></param>
    /// <returns><see cref="DisplayArea"/></returns>
    public Microsoft.UI.Windowing.DisplayArea? GetDisplayArea(Window window)
    {
        try
        {
            System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var da = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
            return da;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetDisplayArea: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region [Static Helpers]
    /// <summary>
    /// Returns the declaring type's namespace.
    /// </summary>
    public static string? GetCurrentNamespace()
    {
        return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;
    }

    /// <summary>
    /// Returns the declaring type's assembly name.
    /// </summary>
    public static string? GetCurrentAssemblyName()
    {
        //return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName;
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
    }

    /// <summary>
    /// Returns the AssemblyVersion, not the FileVersion.
    /// </summary>
    public static Version GetCurrentAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
    }
    #endregion
}
