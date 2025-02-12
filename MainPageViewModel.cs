using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ProgressButtonDemo;

public partial class MainPageViewModel : ObservableObject
{
	CancellationTokenSource? _cts;

	[ObservableProperty]
	DelayTime _delay;

    [ObservableProperty]
	bool _isBusy = false;

	[ObservableProperty]
	bool _isAsync = false;

	[ObservableProperty]
	bool _pageLoaded = false;

	[ObservableProperty]
	double _amount = 0;

	[ObservableProperty]
	string _dataText1 = "data1";

	[ObservableProperty]
	string _dataText2 = "data2";

	[ObservableProperty]
	string _status = string.Empty;

	public ICommand TestCommand { get; }
    public ICommand SwitchDelayCommand { get; }
    public IAsyncRelayCommand TestCommandAsync { get; }
	public Action? ButtonClickEvent1 { get; set; }
	public Action? ButtonClickEvent2 { get; set; }

	public MainPageViewModel()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");
		
		_cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		// RelayCommand example for our ProgressButton.
		TestCommand = new RelayCommand<string>(async (param) =>
		{
			IsAsync = false;
			IsBusy = true;
			if (!string.IsNullOrEmpty(param))
				Status = $"Doing something with '{param}'…";
			else
				Status = $"Parameter was empty";
			await Task.Delay(1000 * (int)Delay);
			Status = $"Complete";
			IsBusy = false;
		});

		// AsyncRelayCommand example for our ProgressButton.
		TestCommandAsync = new AsyncRelayCommand<string>((param) =>
		{
			IsAsync = true;
			if (_cts != null && !_cts.Token.IsCancellationRequested)
			{
				return DoSomething(param, _cts.Token);
			}
			else
			{
				Status = $"Token expired, renewing…";
				_cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
				return Task.FromResult("Token Expired");
			}
		});

        // Configure delay time command.
        SwitchDelayCommand = new RelayCommand<string>(async (param) =>
        {
            if (!string.IsNullOrEmpty(param))
            {
                if (Enum.IsDefined(typeof(DelayTime), param))
                {
                    await Task.Delay(10);
                    Delay = (DelayTime)Enum.Parse(typeof(DelayTime), param);
                }
                else
                {
                    Debug.WriteLine($"Parameter is not of type '{nameof(DelayTime)}'.");
                }
            }
            else
            {
                Debug.WriteLine($"Parameter was empty, nothing to do.");
            }
        });

        // Action example for our ProgressButton.
        ButtonClickEvent1 += async () =>
		{
			IsAsync = false;
			IsBusy = true;
			Status = $"Doing something…";
			await Task.Delay(1000 * (int)Delay);
			Status = $"Complete";
			IsBusy = false;
		};

		// Action example for our ProgressButton.
		ButtonClickEvent2 += async () =>
		{
			IsAsync = false;
			IsBusy = true;
			Status = $"Running progress…";
			Amount = 0;
			for (int i = 0; i < 100; i++)
			{
				Amount += 1;
				await Task.Delay(10 * (int)Delay);
			}
			Status = $"Finished";
			IsBusy = false;
		};

	}

	static int entrance = 0;
	/// <summary>
	/// This is for our <see cref="AsyncRelayCommand"/>
	/// </summary>
	/// <param name="data">some text</param>
	/// <param name="token"><see cref="CancellationToken"/></param>
	/// <returns>text</returns>
	async Task<string> DoSomething(string data, CancellationToken token = default)
	{
		Debug.WriteLine($"Entry #{++entrance}");

		IsBusy = true;

		if (!string.IsNullOrEmpty(data))
			Status = $"Doing something with '{data}'…";
		else
			Status = $"Parameter was empty";

		try
		{
			await Task.Delay(1000 * (int)Delay, token);
		}
		catch (TaskCanceledException) 
		{
			Status = "Token was canceled!"; 
		}

		IsBusy = false;

		return Status = $"Complete";
	}

	/// <summary>
	/// {x:Bind} allows us to bind page events directly into our ViewModel.
	/// </summary>
    public void MainPageLoading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
		Delay = DelayTime.Medium;
    }
}
