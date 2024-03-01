using static MauiNurAppTemplate.Utilities;

namespace MauiNurAppTemplate;

public partial class InventoryStreamPage : ContentPage
{
	private readonly InventoryStreamViewModel _viewModel;    

    public InventoryStreamPage()
	{
		InitializeComponent();
        
        _viewModel = new InventoryStreamViewModel();
		BindingContext = _viewModel;
	}

    void OnTxLevelChanged(object sender, ValueChangedEventArgs args)
    {
        //User still moving slider. Show only TxLevel value at this point.
        _viewModel.ShowSliderTxLevel(args.NewValue);
    }

    void OnDragCompleted(object sender, EventArgs args)
    {        
        //User stop moving slider so let's set new txlevel
        _viewModel.SetNewTxLevel();
    }

    private void OnStartStop(object sender, EventArgs e)
    {
        _viewModel.StartStop();
    }

    private void OnClear(object sender, EventArgs e)
    {
        _viewModel.Clear();
    }

    private async void OnShare(object sender, EventArgs e)
    {
        _viewModel.Stop(); //Stop inventory if not already stopped.
        
        if (App.Nur.GetTagStorage().Count > 0)
        {
            App.KeepNurConnectedWhileInactive = true;
            await ShareAsCsv(this, "Inventory_results", TagStorageToCsv(App.Nur.GetTagStorage()));            
        }
        else
        {
            ShowToast("Nothing to share");
        }
        
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