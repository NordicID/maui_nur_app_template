#if __ANDROID__
using NordicID.NurApi.Android.Helpers;
#endif
using NurApiDotNet;
using System.Diagnostics;


namespace MauiNurAppTemplate.Helpers
{
    /// <summary>
    /// Handle reader connection. Disconnects when App Sleep or destroyed. Connects automatically to last connected device when app start.
    /// Connects automatically in to integrated reader if exist.
    /// </summary>
    public static class ReaderConnect
    {
        /// <summary>
        /// Init reader connection related event handlers
        /// </summary>
        public static void Init()
        {
            App.Nur.ConnectedEvent += OnNurApi_ConnectedEvent;
            App.Nur.ConnectionStatusEvent += OnNurApi_ConnectionStatusEvent;
            App.Nur.DisconnectedEvent += OnNurApi_DisconnectedEvent;

#if __ANDROID__
            App.Nur.PermissionRequiredEvent += OnNur_PermissionRequiredEvent; ;
#endif
            
        }
#if __ANDROID__
        private static void OnNur_PermissionRequiredEvent(object? sender, PermissionRequiredEventArgs e)
        {
            Utilities.ShowSnackbar($"Attempting to request permissions for {e.Uri}...", Colors.White, Colors.Black, 10);
            if (e.Uri.Scheme == "usb")
            {
                NurApiUSBDevicePermissionRequest.Request(e.Uri, (isGranted) => OnUserPermissionResponse(e.Uri, isGranted)); // Forwarding callback result 
            }
        }

        // Handling permission result.
        private static void OnUserPermissionResponse(Uri connectionUri, bool isGranted)
        {
            if (!isGranted)
            {
                Utilities.ShowSnackbar($"Permission was not granted for the USB device ´{connectionUri}´",Colors.Orange,Colors.Black);
                return;
            }

            Utilities.ShowSnackbar($"Permission was successfully granted for {connectionUri}", Colors.Green, Colors.Black);
            Uri currentDeviceUri = App.Nur.ConnectedDeviceUri;
            if (currentDeviceUri != null)
                App.Nur.Disconnect();

            App.Nur.Connect(connectionUri);
        }
#endif

        /// <summary>
        /// Provides simple list of discovered readers where user can choose to connect or disconnect existing connection.
        /// </summary>
        /// <param name="page">used for DisplayActionSheet</param>
        /// <returns></returns>
        public static async Task SelectReader(Page page)
        {
            try
            {                
                List<string> deviceNames = new List<string>();

                Uri currentDeviceUri = App.Nur.ConnectedDeviceUri;
                string currentDeviceName = currentDeviceUri?.GetQueryParam("name") ?? "";
                if (string.IsNullOrEmpty(currentDeviceName))
                    currentDeviceName = currentDeviceUri?.GetQueryParam("productName") ?? "";

                if (currentDeviceUri == null)
                    currentDeviceName = "";

                while (true)
                {
                    if (!string.IsNullOrEmpty(currentDeviceName))
                        deviceNames.Add("DISCONNECT: " + currentDeviceName);
                                        
                    foreach (KeyValuePair<string, Uri> kvp in App.DeviceDiscovery.ReadersDiscovered)
                    {
                        if (currentDeviceName.Equals(kvp.Key))
                            continue; //this is currently connected

                        deviceNames.Add(kvp.Key);
                    }

                    if (deviceNames.Count == 0)
                    {
                        deviceNames.Add("No readers found!. Try 'Refresh'");
                    }

                    string action = await page.DisplayActionSheet("Readers", "Cancel", "Refresh", deviceNames.ToArray());

                    if (!string.IsNullOrEmpty(action) && !action.Equals("Cancel"))
                    {
                        if (action.Contains("Refresh") || action.StartsWith("No readers found"))
                        {
                            deviceNames.Clear();
                            App.DeviceDiscovery.Start();
                            continue;
                        }
                        else if (action.Contains("DISCONNECT"))
                        {
                            DisconnectCurrentDevice();
                        }
                        else
                        {

                            ConnectToDevice(action);
                        }
                    }

                    break;
                }

                ClearReadersFoundAndRestartDiscovery();

                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, $"Error reader select: {ex.Message}");
            }
        }

        private static void DisconnectCurrentDevice()
        {
            App.Nur.Disconnect();
            App.Nur.AutoReconnect = false;
            Preferences.Set("last_connected_reader", "");
        }

        private static void ConnectToDevice(string selectedDevice)
        {
            try
            {
                if (App.DeviceDiscovery.ReadersDiscovered.ContainsKey(selectedDevice))
                {
#if __ANDROID__
                    try
                    {
                        Uri uri = App.DeviceDiscovery.ReadersDiscovered[selectedDevice];
                        if (uri.Scheme == "usb")
                        {                           
                            NurApiUSBDevicePermissionRequest.Request(uri, (isGranted) => OnUserPermissionResponse(uri, isGranted)); // Forwarding callback result                        
                            return; //Connection for this is in OnUserPermissionResponse handler
                        }
                    }
                    catch(Exception ex) 
                    {
                        Utilities.ShowSnackbar($"Error reconnect {ex.Message}", Colors.Red, Colors.Black);
                    }
#endif

                    Uri currentDeviceUri = App.Nur.ConnectedDeviceUri;
                    if (currentDeviceUri != null)
                        App.Nur.Disconnect();

                    App.Nur.Connect(App.DeviceDiscovery.ReadersDiscovered[selectedDevice]);
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowSnackbar($"Error connecting {App.DeviceDiscovery.ReadersDiscovered[selectedDevice]}!  {ex.Message}", Colors.Red, Colors.Black);
            }
        }

        private static void ClearReadersFoundAndRestartDiscovery()
        {
            App.DeviceDiscovery.ReadersDiscovered.Clear();
            App.DeviceDiscovery.Start();
        }

        /// <summary>
        /// When application start, connecting to last connected device if any. If no last connection, it trying to connect integrated_reader if device is correct.
        /// </summary>
        public static void AppStart()
        {
            //======= CONNECT READER =============
            string lastConnectedReaderUri = Preferences.Get("last_connected_reader", "");
            Uri? rdrUri = null;

            if (string.IsNullOrEmpty(lastConnectedReaderUri))
            {
                if (DeviceInfo.Model.StartsWith("HH"))
                {
                    Debug.WriteLine("HHxx device. Most likely there is integrated reader.");
                    rdrUri = new Uri("int://integrated_reader/?name=" + "Integrated reader");
                }
            }
            else
                rdrUri = new Uri(lastConnectedReaderUri);

            if (rdrUri != null)
            {               
                App.Nur.AutoReconnect = true;
                App.Nur.Connect(rdrUri);
            }            
        }

        /// <summary>
        /// Skip this if you still want reader connection to be active while app inactive. Otherwise reader disconnected.
        /// </summary>
        public static void AppSleep()
        {
            if (App.Nur.IsConnected())
            {
                Preferences.Set("was_connected", true);
                try
                {
                    App.Nur.Disconnect();
                }
                catch (Exception) { }
            }
            else
            {
                Preferences.Set("was_connected", false);
            }
        }

        /// <summary>
        /// Reconnects if there was previously connection before entering to sleep.
        /// </summary>
        public static void AppResume()
        {
            try
            {
                App.Nur.CommTimeoutMilliSec = 3000;

                if (!App.Nur.IsConnected())
                {
                    bool wasConnected = Preferences.Get("was_connected", false);
                    if (wasConnected)
                    {
                        App.Nur.AutoReconnect = true;
                        App.Nur.Connect(); //Connect to existing
                    }
                }
            }
            catch (Exception) { }
        }

        private static void OnNurApi_DisconnectedEvent(object? sender, NurApi.NurEventArgs e)
        {
            Utilities.ShowSnackbar("Reader disconnected!", Colors.Red, Colors.Black);            
            App.Nur.AutoReconnect = true;
        }

        private static void OnNurApi_ConnectionStatusEvent(object? sender, NurTransportStatus e)
        {
            switch (e)
            {
                case NurTransportStatus.Connected:
                    break;
                case NurTransportStatus.Connecting:
                    Utilities.ShowSnackbar("Connecting to:\n" + App.Nur.LastConnectUri, Colors.Yellow, Colors.Black, 100);
                    break;
                case NurTransportStatus.Disconnected:
                    break;
            }
        }

        private static void OnNurApi_ConnectedEvent(object? sender, NurApi.NurEventArgs e)
        {
            //There is now reader connection. Load useful info from reader in to our memory.
            App.DeviceDiscovery.Stop();

            try
            {
                App.ReaderInfo = App.Nur.GetReaderInfo();
                App.DeviceCapabilites = App.Nur.Capabilites;
                try
                {
                    App.ReaderAccessory = App.Nur.AccGetConfig();                                        
                }
                catch(Exception)
                {
                    App.ReaderAccessory=null;
                    //Utilities.ShowToast("No accessories");
                }

                Preferences.Set("last_connected_reader", App.Nur.ConnectedDeviceUri.ToString());                               
                List<double> txLevels = App.DeviceCapabilites.GetTxLevels();
                Utilities.ShowSnackbar("Reader connected! (" + App.ReaderInfo.altSerial + ")", Colors.DarkGreen, Colors.White);                

            }
            catch (Exception ex)
            {
                Utilities.ShowErrorSnackbar("Error: " + ex.Message);
            }
        }
    }
}
