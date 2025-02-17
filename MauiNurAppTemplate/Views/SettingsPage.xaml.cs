using MauiNurAppTemplate.Helpers;
using MauiNurAppTemplate.ViewModels;

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

    protected override void OnAppearing()
    {
        _viewModel.Init();
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        _viewModel.Release();
        base.OnDisappearing();
    }
        
}