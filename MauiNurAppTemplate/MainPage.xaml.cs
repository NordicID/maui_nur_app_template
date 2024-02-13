using MauiNurAppTemplate.Helpers;
using MauiNurAppTemplate.Views;

namespace MauiNurAppTemplate
{
    public partial class MainPage : ContentPage
    {        
       
        public MainPage()
        {
            InitializeComponent();                   
        }
                
        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }

        private async void OnSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnInventory(object sender, EventArgs e)
        {            
            await Navigation.PushAsync(new InventoryStreamPage());            
        }

        private async void OnBarcode(object sender, EventArgs e)
        {
            if (App.Nur.IsConnected())
            {
                if (App.ReaderAccessory != null)
                {
                    if (App.ReaderAccessory.hasImagerScanner())
                    {
                        //Imager found
                        await Navigation.PushAsync(new BarcodePage());
                        return;
                    }
                }

                Utilities.ShowErrorSnackbar("Sorry, no imager present!");
            }
            else
                Utilities.ShowErrorSnackbar("Reader not connected!");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AskPermissions();
        }

        private async void AskPermissions()
        {
            PermissionStatus bluetoothStatus = await PermissionHelper.CheckBluetoohPermission();

            if (!PermissionHelper.IsGranted(bluetoothStatus))
            {
                Utilities.ShowToast("Bluetooth/Location permissions not accepted.");
                return;
            }

            PermissionStatus writeStoragePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (writeStoragePermissionStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
        }
    }

}
