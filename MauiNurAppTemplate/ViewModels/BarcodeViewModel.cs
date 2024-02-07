using CommunityToolkit.Mvvm.ComponentModel;
using MauiNurAppTemplate.Helpers;
using System.Diagnostics;
using static NurApiDotNet.NurApi;

namespace MauiNurAppTemplate
{
    public partial class BarcodeViewModel : ObservableObject
    {
        [ObservableProperty] string _barcodeText;
        [ObservableProperty] bool _activityRunning;
        [ObservableProperty] bool _enableReadButton;

        private bool _barcodeReadPending;

        public BarcodeViewModel() 
        {
            _barcodeText = string.Empty;
            _activityRunning = false;
            _enableReadButton = true;
            _barcodeReadPending = false;
        }

        /// <summary>
        /// Allocate trigger events for start/stop reading
        /// </summary>
        public void Init()
        {
            App.Nur.IOChangeEvent += OnNur_IOChangeEvent;
        }        

        /// <summary>
        /// Abort activities and release resourcs
        /// </summary>
        public void Release()
        {
            App.Nur.AccBarcodeCancel(); //Cancel barcode reading immediately if any.                  
            App.Nur.IOChangeEvent -= OnNur_IOChangeEvent;
        }
                
        private void OnNur_IOChangeEvent(object? sender, NurApiDotNet.NurApi.IOChangeEventArgs e)
        {
            //Trigger activity
            AccessorySensorSource source = (AccessorySensorSource)e.data.source;
                        
            if (_barcodeReadPending)
                return; //do not mess with ongoing reading..

            Debug.WriteLine(source.ToString() + " Dir=" + e.data.dir.ToString());

            try
            {
                if (source == AccessorySensorSource.ButtonTrigger)
                {
                    if (e.data.dir == 0)
                    {
                        //We are intrested only when trigger released                        
                        Read();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IOChangeEvent exception=" + ex.Message);
            }
        }

        public async void Read()
        {
            _barcodeReadPending = true;

            if (App.Nur.IsConnected())
            {                  
                //Reader is connected and we trust this reader has imager and support barcode scanning..otherwise crash..

                EnableReadButton = false;
                ActivityRunning = true;

                //Start barcode reading. this blocked until barcode read, reading aborted or timeout                
                string result = await Barcode.Read(App.Nur);                
                //If result is not empty, then real barcode has been read successfully

                ActivityRunning = false;
                EnableReadButton = true;
                
                if (!string.IsNullOrEmpty(result))
                {
                    BarcodeText = result;
                    App.BarcodeSuccessBeep.Play();
                }
            }
            else
            {
                Utilities.ShowErrorSnackbar("Reader not connected");
            }

            _barcodeReadPending = false;
        }
    }
}
