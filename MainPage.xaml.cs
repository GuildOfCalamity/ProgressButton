using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ProgressButtonDemo;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public static DispatcherQueue? dispatcher { get; private set; }
    public MainPageViewModel? ViewModel { get; }

    public MainPage()
    {
		ViewModel = App.Current.IoCServices.GetService<MainPageViewModel>() ?? new MainPageViewModel();
        dispatcher = DispatcherQueue;
		this.InitializeComponent();
		this.Loaded += MainPage_Loaded;
    }

	void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		if (App.AnimationsEffectsEnabled)
			StoryboardPath.Begin();
	}
}
