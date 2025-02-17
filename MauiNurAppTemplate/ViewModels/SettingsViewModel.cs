using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiNurAppTemplate.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] bool _updateIsVisible;

        public SettingsViewModel()
        {
            UpdateIsVisible = false;
        }

        public void Init()
        {
            //Start looking for devices in case user want to select reader.
            App.DeviceDiscovery.Start();            
            Utilities.ShowToast("Device discovery started!");
            UpdateIsVisible = App.Nur.IsConnected();
            App.Nur.ConnectedEvent += Nur_ConnectedEvent;
            App.Nur.DisconnectedEvent += Nur_DisconnectedEvent;
        }

        private void Nur_DisconnectedEvent(object? sender, NurApiDotNet.NurApi.NurEventArgs e)
        {
            UpdateIsVisible = App.Nur.IsConnected();
        }

        private void Nur_ConnectedEvent(object? sender, NurApiDotNet.NurApi.NurEventArgs e)
        {
            UpdateIsVisible = App.Nur.IsConnected();
        }

        public void Release()
        {
            App.Nur.ConnectedEvent -= Nur_ConnectedEvent;
            App.Nur.DisconnectedEvent -= Nur_DisconnectedEvent;
            App.DeviceDiscovery?.Stop();
            Utilities.ShowToast("Device discovery stopped!");
        }
    }
}
