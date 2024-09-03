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
            await ShareAsCsv(this, "Inventory_results", TagStorageToCsv(App.Nur.GetTagStorage()));            
        }
        else
        {
            ShowToast("Nothing to share");
        }
        
    }
       
    protected override void OnAppearing()
    {
        //Let's keep reader connection up as long we are in InventoryPage
        App.KeepNurConnectedWhileInactive = true;

        //Let's keep display on as long we are in InventoryPage (prevent to go sleep)
        DeviceDisplay.Current.KeepScreenOn = true;

        _viewModel.Init();      
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        //Leaving from inventory page. Let reader disconnect when app inactive.
        App.KeepNurConnectedWhileInactive = false;

        //Let display shut down after user inactivity
        DeviceDisplay.Current.KeepScreenOn = false;

        _viewModel.Release();        
        base.OnDisappearing();
    }
}