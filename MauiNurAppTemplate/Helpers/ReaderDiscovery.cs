using NurApiDotNet;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MauiNurAppTemplate.Helpers
{
    public class ReaderDiscovery
    {
        /// <summary>
        /// After discovering readers from various sources (Bluetooth, local network, USB..) result are stored here.
        /// Key = name of device Value=Uri used for connection.
        /// </summary>
        public ConcurrentDictionary<string, Uri> ReadersDiscovered { get; set; } = new ConcurrentDictionary<string, Uri>();

        private readonly NurDeviceDiscoveryCallback _nurDeviceDiscoveryCallback;

        public ReaderDiscovery()
        {
            _nurDeviceDiscoveryCallback = OnDeviceDiscovered;
            IsDiscovering = false;
        }

        public bool IsDiscovering { get; set; }

        public void Start()
        {            
            if (IsDiscovering) { Stop(); } //Stop before starting new one
            IsDiscovering = true;
            NurDeviceDiscovery.Start(_nurDeviceDiscoveryCallback);
        }

        public void Stop()
        {
            try
            {
                NurDeviceDiscovery.Stop(_nurDeviceDiscoveryCallback);
                IsDiscovering = false;
            }
            catch (Exception e)
            {
                Utilities.ShowToast("problems to stop device discovery: " + e.Message);
            }
        }

        private void OnDeviceDiscovered(object sender, NurDeviceDiscoveryEventArgs discoveredDevice)
        {
            string devName = discoveredDevice.Uri?.GetQueryParam("name") ?? "";
            if(string.IsNullOrEmpty(devName)) 
                devName = discoveredDevice.Uri?.GetQueryParam("productName") ?? "";

            if (discoveredDevice.Uri != null)
            {
                if (ReadersDiscovered.TryAdd(devName, discoveredDevice.Uri))
                {
                    //Added
                    Debug.WriteLine("Already in list: " + discoveredDevice.Uri.ToString());
                }
            }
        }
    }
}
