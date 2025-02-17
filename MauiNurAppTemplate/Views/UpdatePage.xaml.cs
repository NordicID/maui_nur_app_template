using MauiNurAppTemplate.ViewModels;

namespace MauiNurAppTemplate.Views;

public partial class UpdatePage : ContentPage
{
   ReaderUpdateViewModel _viewModel;

    public UpdatePage()
	{
		InitializeComponent();
		_viewModel = new ReaderUpdateViewModel();
		BindingContext = _viewModel;
	}

    private async void OnClose(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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

    private void OnLocalUpdate(object sender, EventArgs e)
    {
        _viewModel.LocalUpdate();
    }

    private void OnUpdateNow(object sender, EventArgs e)
    {
        _viewModel.StartUpdate();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.IsUpdatePending)
        {
            Utilities.ShowToast("Update inprogress.. Please wait!");
            return true;
        }

        return base.OnBackButtonPressed();  
    }
}