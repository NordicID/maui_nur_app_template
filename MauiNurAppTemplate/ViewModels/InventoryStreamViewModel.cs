using CommunityToolkit.Mvvm.ComponentModel;
using MauiNurAppTemplate.Helpers;
using System.Diagnostics;
using static NurApiDotNet.NurApi;

namespace MauiNurAppTemplate
{
    partial class InventoryStreamViewModel : ObservableObject
    {
        [ObservableProperty] string _tagCount;
        [ObservableProperty] bool _activityRunning;
        [ObservableProperty] string _startStopRead;
        [ObservableProperty] string _txLevelText;
        [ObservableProperty] double _sliderValue;
        [ObservableProperty] bool _silenceMode;
        [ObservableProperty] bool _readTagModel;

        private List<double>? txLevels;
        ExpirationWatcher? txLevelSetWatcher;
        private int _newTxLevel;
      
        public InventoryStreamViewModel()
        {                        
            TagCount = "0";
            ActivityRunning = false;
            StartStopRead = "START";
            TxLevelText = "";      
            SilenceMode = false;            
        }

        private void OnNur_DisconnectedEvent(object? sender, NurEventArgs e)
        {
            //It's possible that reader disconnects in middle of reading operation.
            //make sure UI shows "not scanning"
            ActivityRunning = false;
            StartStopRead = "START";
        }

        private void OnTxLevelSetWatcher_Expired(object? sender, EventArgs e)
        {
            Debug.WriteLine("EXPRIRED");

            //Make sure reader is connected before trying to adjust Tx level.
            if (App.Nur.IsConnected())
            {                
                try
                {
                    App.Nur.TxLevel = _newTxLevel;
                    Debug.WriteLine("New Tx Level:" + App.Nur.TxLevel);
                }
                catch (Exception ex)
                {
                    Utilities.ShowSnackbar(ex.Message, Colors.DarkRed, Colors.White, 5);
                }
            }            
        }

        /// <summary>
        /// Start or Stop inventory streaming
        /// </summary>
        public void StartStop()
        {             
            try
            {
                if (App.Nur.IsConnected())
                {
                    if (App.Nur.IsInventoryStreamRunning())
                    {
                        Stop();
                    }
                    else
                    {
                        Start();                                      
                    }
                }
                else
                {                    
                    Utilities.ShowErrorSnackbar("Reader not connected");
                }
                
            }
            catch (Exception ex)
            {
                Utilities.ShowErrorSnackbar(ex.Message);
            }

            Utilities.PlayTagsAddedSound(0); //Stop sound
            
            Debug.WriteLine("Pause");
            
        }
              
        /// <summary>
        /// Stop inventory if not already stopped.
        /// </summary>
        public void Stop()
        {
            if(App.Nur.IsConnected())
            {
                if (App.Nur.IsInventoryStreamRunning())
                {
                    ActivityRunning = false;
                    StartStopRead = "START";
                    App.Nur.StopInventoryStream();
                    Utilities.PlayTagsAddedSound(0);
                }
            }
        }

        /// <summary>
        /// When active, tag model read using these parameters
        /// </summary>
        private void PrepareTagModelReadSettings()
        {
            //Read TID header. Used for reading TagType
            //Reading is little bit slower.
            App.InvReadParams.bank = BANK_TID;
            App.InvReadParams.wAddress = 0;
            App.InvReadParams.wLength = 2;            
        }

        /// <summary>
        /// Start inventory if not already started
        /// </summary>
        public void Start()
        {
            if (App.Nur.IsConnected())
            {
                if (!App.Nur.IsInventoryStreamRunning())
                {
                    PrepareTagModelReadSettings();
                    App.InvReadParams.active = ReadTagModel; //Activate it or not.
                    
                    App.Nur.SetInventoryRead(App.InvReadParams);
                    App.Nur.StartInventoryStream(0, 0, 0); //Use Auto Q and Rounds. Session=0
                    ActivityRunning = true;
                    StartStopRead = "STOP";
                }
            }
            else
            {
                Utilities.ShowErrorSnackbar("Reader not connected");
            }
        }

        public void SetTxLevelFromSlider(double sliderValue)
        {
            if (App.Nur.IsConnected())
            {
                double val = App.DeviceCapabilites.txSteps * (1 - sliderValue);
                if (val >= App.DeviceCapabilites.txSteps)
                    val = App.DeviceCapabilites.txSteps - 1;

                TxLevelText = txLevels[(int)val].ToString("0.00");
                _newTxLevel = (int)val;

                txLevelSetWatcher.Reset(350); //Do txLevel set action after no slide movement within 350ms
            }            
        }

        /// <summary>
        /// Clear TagStorage content
        /// </summary>
        public void Clear()
        {
            if (App.Nur.IsConnected())
            {
                App.Nur.ClearTags();
                App.Nur.ClearTagsEx();
                TagCount = "0";
            }            
        }               
        
        private void OnNur_IOChangeEvent(object? sender, NurApiDotNet.NurApi.IOChangeEventArgs e)
        {
            AccessorySensorSource source = (AccessorySensorSource)e.data.source;
            try
            {
                if (source == AccessorySensorSource.ButtonTrigger)
                {
                    if (e.data.dir == 0)
                    {
                        //Trigger released.
                        StartStop();
                    }                    
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowErrorSnackbar(ex.Message);
            }
        }

        /// <summary>
        /// Init resources. Trust reader is connected
        /// </summary>
        public void Init()
        {            
            App.Nur.InventoryStreamEvent += OnNur_InventoryStreamEvent;
            App.Nur.IOChangeEvent += OnNur_IOChangeEvent;
            App.Nur.DisconnectedEvent += OnNur_DisconnectedEvent;

            txLevelSetWatcher = new ExpirationWatcher();
            txLevelSetWatcher.Expired += OnTxLevelSetWatcher_Expired;

            if(App.Nur.IsConnected())
            { 
                TagCount = App.Nur.GetTagStorage().Count.ToString();
                txLevels = App.DeviceCapabilites.GetTxLevels();
                TxLevelText = txLevels[App.Nur.TxLevel].ToString("0.00");

                double sl = (double)App.Nur.TxLevel / (double)txLevels.Count;
                SliderValue = 1 - sl;
            }
            else
            {
                Utilities.ShowErrorSnackbar("Reader not connected!");
            }
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Release()
        {           
            try
            {
                //Force stop all Nur continuous functions (like InventoryStreaming)
                App.Nur.StopContinuous();
            }
            catch { }

            App.Nur.InventoryStreamEvent -= OnNur_InventoryStreamEvent;
            App.Nur.IOChangeEvent -= OnNur_IOChangeEvent;
            App.Nur.DisconnectedEvent -= OnNur_DisconnectedEvent;

            txLevelSetWatcher.Expired -= OnTxLevelSetWatcher_Expired;
            txLevelSetWatcher.Dispose();
        }

        private void OnNur_InventoryStreamEvent(object? sender, NurApiDotNet.NurApi.InventoryStreamEventArgs e)
        {
            //Let's show only count at this point.
            TagStorage storage = App.Nur.GetTagStorage();
            TagCount = storage.Count.ToString();

            if(!SilenceMode && App.Nur.IsInventoryStreamRunning())
                Utilities.PlayTagsAddedSound(e.data.tagsAdded);                          

            /* ==================
            //Inventoried tags are now added or updated in to the internal TagStore
            TagStorage storage = App.Nur.GetTagStorage();

            //Get list of tags added.
            Dictionary<byte[], Tag> added = storage.GetAddedTags();
            Dictionary<byte[], Tag> updated = storage.GetUpdatedTags();

            //Handle added and updated.. and then clear

            added.Clear();
            updated.Clear();
            ==================== */

            if (e.data.stopped)
            {
                try
                {
                    //Stopped automatically after 25 sec or so..
                    App.Nur.StartInventoryStream(); //Start again
                    Debug.WriteLine("Restart");
                }
                catch (Exception ex)
                {
                    Utilities.ShowErrorSnackbar(ex.Message);
                }
            }
        }
    }
}
