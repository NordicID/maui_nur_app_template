using CommunityToolkit.Maui.Alerts;
using MauiNurAppTemplate.Helpers;
using MauiNurAppTemplate.ViewModels;
using NurApiDotNet;
using System.Text;


namespace MauiNurAppTemplate.Views;

public partial class SettingsPage : ContentPage
{
    SettingsViewModel _viewModel;

    public SettingsPage()
	{
        _viewModel = new SettingsViewModel();
        BindingContext = _viewModel;
        InitializeComponent();
	}

    private async void OnReader(object sender, EventArgs e)
    {
        //Go to select discovered reader
        await ReaderConnect.SelectReader(this);
    }

    private async void OnUpdate(object sender, EventArgs e)
    {
        if(!App.Nur.IsConnected())
        {
            Utilities.ShowErrorSnackbar("Reader not connected");
            return;
        }

        //Go to reader update page
        await Navigation.PushAsync(new UpdatePage());
    }

    private async void OnShareLogs(object sender, EventArgs e)
    {
        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < App.LogStock.Count; x++)        
            sb.AppendLine(App.LogStock[x]);
        
        await Utilities.ShareAsCsv(this,"UpdateLogs",sb.ToString());
        App.LogStock.Clear();
    }

    protected override void OnAppearing()
    {
        _viewModel.Init();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (App.Nur.GetLogLevel() == NurApi.LOG_ERROR)
            {
                //No logs
                LogEnableCheck.IsChecked = false;
            }
            else
                LogEnableCheck.IsChecked = true;
        });        

        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        _viewModel.Release();
        base.OnDisappearing();
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if(e.Value == true)
        {
            App.Nur.SetLogLevel(NurApi.LOG_ERROR| NurApi.LOG_VERBOSE|NurApi.LOG_USER);
        }
        else
        {
            App.Nur.SetLogLevel(NurApi.LOG_ERROR);
        }
    }
}