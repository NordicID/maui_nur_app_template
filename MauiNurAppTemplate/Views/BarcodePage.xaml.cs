namespace MauiNurAppTemplate.Views;

public partial class BarcodePage : ContentPage
{
    BarcodeViewModel _viewModel;   

	public BarcodePage()
	{
		InitializeComponent();
        _viewModel = new BarcodeViewModel();
        BindingContext = _viewModel;
    }

    private void OnReadBarcode(object sender, EventArgs e)
    {
        _viewModel.Read();
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
}