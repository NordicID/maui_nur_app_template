using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiNurAppTemplate.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {

        }

        public void Init()
        {
            //Start looking for devices in case user want to select reader.
            App.DeviceDiscovery.Start();
        }

        public void Release()
        {
            App.DeviceDiscovery?.Stop();
        }
    }
}
