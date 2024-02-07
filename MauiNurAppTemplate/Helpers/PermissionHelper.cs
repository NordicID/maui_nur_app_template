namespace MauiNurAppTemplate
{
    internal class PermissionHelper
    {
        public static async Task<PermissionStatus> CheckBluetoohPermission()
        {            
            PermissionStatus bluetoothStatus = PermissionStatus.Granted;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (DeviceInfo.Version.Major >= 12)
                {
                    bluetoothStatus = await CheckPermissions<Permissions.Bluetooth>();
                }
                else
                {
                    bluetoothStatus = await CheckPermissions<Permissions.LocationWhenInUse>();
                }
            }

            return bluetoothStatus;
            
        }

        public static async Task<PermissionStatus> CheckPermissions<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<TPermission>();
            }

            return status;
        }

        public static bool IsGranted(PermissionStatus status)
        {
            return status == PermissionStatus.Granted || status == PermissionStatus.Limited;
        }
    }
}
