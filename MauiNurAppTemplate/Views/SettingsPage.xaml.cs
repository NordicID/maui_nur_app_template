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