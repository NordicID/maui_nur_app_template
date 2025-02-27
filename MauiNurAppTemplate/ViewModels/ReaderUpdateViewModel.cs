using CommunityToolkit.Mvvm.ComponentModel;
using NordicID.UpdateLib;

namespace MauiNurAppTemplate.ViewModels
{
    public partial class ReaderUpdateViewModel : ObservableObject
    {
        [ObservableProperty] bool _isLocalEnabled;
        [ObservableProperty] bool _isUpdateNowEnabled;
        [ObservableProperty] string _textStatus;
        [ObservableProperty] string _textProgress;
        [ObservableProperty] bool _activityIsRunning;

        NurUpdate upd = new NurUpdate();

        /// <summary>
        /// Used to prevent user to leave from page at middle of update..
        /// </summary>
        public bool IsUpdatePending { get; set; }

        public ReaderUpdateViewModel() 
        {
            upd.OnUpdatingEvent += Upd_OnUpdatingEvent;
            upd.SetNurApi(App.Nur);

            IsLocalEnabled = false;
            IsUpdateNowEnabled = false;
            TextStatus = "Checking updates...";
            TextProgress = "";
            IsUpdatePending = false;
            ActivityIsRunning = false;

            //Give time for UI to load because CheckUpdates start right away and blocks everything.
            _ = Task.Delay(200).ContinueWith(async _ =>
            {
                CheckUpdates();
            });
        }

        private void Upd_OnUpdatingEvent(object? sender, NurUpdate.UpdatingEventArgs e)
        {
            if (e.niduEvent == NurUpdate.Event.LOG)
            {
                App.Nur.VLog(e.msg);                
            }
            else if (e.niduEvent == NurUpdate.Event.STATUS)
            {
                App.Nur.VLog("NurUpdate.Event:Status = " + e.msg);
            }
            else if (e.niduEvent == NurUpdate.Event.PROGRESS)
            {
                TextProgress = e.prg.ToString()+"%";
            }
            else if (e.niduEvent == NurUpdate.Event.VALIDATE)
            {
                App.Nur.VLog("NurUpdate.Event:VALIDATE = " + e.msg);
            }
            else if (e.niduEvent == NurUpdate.Event.PRG_ITEM_START)
            {
                App.Nur.VLog("NurUpdate.Event:PRG_ITEM_START = " + e.msg);
            }
            else if(e.niduEvent == NurUpdate.Event.PRG_BEGIN)
            {
                App.Nur.VLog("NurUpdate.Event:PRG_BEGIN = " + e.msg);
            }
            else if (e.niduEvent == NurUpdate.Event.PRG_END)
            {
                App.Nur.VLog("NurUpdate.Event:PRG_END = " + e.msg);
            }
            else if (e.niduEvent == NurUpdate.Event.PRG_ERROR)
            {
                App.Nur.VLog("NurUpdate.Event:PRG_ERROR = " + e.msg);
            }

        }

        /// <summary>
        /// Pick zip file and validate it.
        /// </summary>
        public async void LocalUpdate()
        {
            PickOptions pickOptions = new PickOptions();
            pickOptions.PickerTitle = "Load update zip";
            App.KeepNurConnectedWhileInactive = true;
            var result = await FilePicker.Default.PickAsync(pickOptions);
            App.KeepNurConnectedWhileInactive = false;

            _ = Task.Delay(100).ContinueWith(async _ =>
            {
                if (result != null)
                {
                    if (result.FileName.EndsWith("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        upd.LoadZipFromFile(result.FullPath);
                        Validate();
                    }
                }
            });
                       
        }

        /// <summary>
        /// Must use separate task for this because otherwise everything is stuck
        /// </summary>
        public void StartUpdate()
        {
            if (upd.STATUS == NurUpdate.Status.READY)
            {                
                _ = Task.Factory.StartNew(() =>
                {
                    UpdatingWorker();
                });                                
            }            
        }

        private void UpdatingWorker()
        {
            IsUpdatePending = true;
            IsLocalEnabled = false;
            IsUpdateNowEnabled = false;
            ActivityIsRunning = true;
            
            App.IsUpdating = true; //Global: Prevent to show "Unable To Connect" snack while updating

            TextStatus = "Updating.. please wait!";
            //This may take long time
            NurUpdate.Error err = upd.StartUpdate();
            
            //All updates done now
            if (err == NurUpdate.Error.NONE)
            {
                //Success
                Utilities.ShowSnackbar("UPDATE SUCCESS!", Colors.DarkGreen, Colors.White, 10);
                TextStatus = "SUCCESS!";
                App.BarcodeSuccessBeep.Play();

            }
            else
            {
                //Something went wrong
                TextStatus = "Something went wrong!";
                TextProgress = "ERROR: " + err.ToString();
                App.ErrorBeep.Play();
            }

            App.IsUpdating = false;
            IsUpdatePending = false;
            IsLocalEnabled = true;
            ActivityIsRunning = false;

        }

        private void Validate()
        {
            TextStatus = "Validating..";
            NurUpdate.Error status = upd.Validate();
            IsLocalEnabled = true;
          
            if (upd.STATUS == NurUpdate.Status.READY)
            {
                TextStatus = "Available updates!";
                TextProgress = "";
                for(int i=0;i<upd.GetItemCount();i++)
                {
                    NurUpdate.UpdateItem ui = upd.GetUpdateItem(i);
                    if(ui.status == NurUpdate.Status.READY)                    
                        TextProgress += ui.name+"\n";                    
                }
                                
                IsUpdateNowEnabled = true;
            }
            else
            {
                TextStatus = "Device UP-TO-DATE";
            }

            App.BarcodeSuccessBeep.Play();
        }

        private void CheckUpdates()
        {
            ActivityIsRunning = true;
            upd.LoadZipFromNordicIDServer();
            Validate();
            ActivityIsRunning = false;
        }

        /// <summary>
        /// Init resources if any
        /// </summary>
        public void Init()
        {
            
        }

        /// <summary>
        /// Release resourcs if any
        /// </summary>
        public void Release()
        {
            
        }
    }
}
