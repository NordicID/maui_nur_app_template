using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using NordicID.NurApi.Utils;
using NurApiDotNet.TagCodec;
using System.Text;
using static NurApiDotNet.NurApi;


namespace MauiNurAppTemplate
{
    public static class Utilities
    {       
        
        public static void ShowToast(string message,ToastDuration duration = ToastDuration.Long, double textSize=14)
        {            
            var toast = Toast.Make(message, duration, textSize);

            if (MainThread.IsMainThread)
            {
                toast.Show();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    toast.Show();
                });
            }
        }

        public static void ShowSnackbar(string message, Color backGround, Color textColor, int durationSec = 4)
        {
            SnackbarOptions snackbarOptions = new SnackbarOptions();
            snackbarOptions.CornerRadius = 5;
            snackbarOptions.BackgroundColor = backGround;
            snackbarOptions.TextColor = textColor;
                                               
            var snackbar = Snackbar.Make(message,null,"", TimeSpan.FromSeconds(durationSec), snackbarOptions,null);                       

            if (MainThread.IsMainThread)
            {
                snackbar.Show();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    snackbar.Show();
                });
            }
        }

        public static void ShowErrorSnackbar(string message,bool doErrorBeep=true, int durationSec = 4)
        {
            ShowSnackbar(message, Colors.DarkRed, Colors.White,durationSec);
            if(doErrorBeep)
            {                
                App.ErrorBeep.Play();
            }
        }

        /// <summary>
        /// IAudioPlayer App.TagSeenTick defined beginning at App and when playing it playing in loop.
        /// This is simple method to produce sound when tags added to TagStorage.
        /// Used in InventoryStream event handler
        /// </summary>
        /// <param name="tagsAdded">if count > 0, sound loop activated</param>
        public static void PlayTagsAddedSound(int tagsAdded)
        {            
            if (tagsAdded > 0)
            {
                if(!App.TagSeenTick.IsPlaying)
                    App.TagSeenTick.Play();
            }
            else
                App.TagSeenTick.Stop();                               

        }

        /// <summary>
        /// Convert TxLevel range 1-100% to Nur value which may be different depending device type.
        /// </summary>
        /// <param name="percentValue">1-100</param>
        /// <returns>value for the Nurapi.Txlevel</returns>
        public static int ToNurTxLevel(int percentValue)
        {
            if (App.DeviceCapabilites != null)
            {
                if (percentValue <= 0)
                    return App.DeviceCapabilites.txSteps - 1; //Illegal value. set lowest possible
                if (percentValue > 100)
                    return 0; //Full power

                double val = 100.0 / App.DeviceCapabilites.txSteps;
                return (int)(App.DeviceCapabilites.txSteps - (percentValue / val));
            }

            return 0; //Full power by default.
        }

        /// <summary>
        /// Iterate current TagStorage items and create single csv formatted string
        /// </summary>
        /// <param name="tags">Nur TagStorage</param>
        /// <returns>csv formatted string of items</returns>
        public static string TagStorageToCsv(TagStorage tags)
        {
            int cnt = 0;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LastSeenUtc,EPC,Scheme,Code,Serial,TagModel,Company,Antenna,RSSI,Frequency");
            foreach (var tag in tags)
            {

                sb.Append(tag.LastSeenUtc.ToString());
                sb.Append(',');
                sb.Append(tag.GetEpcString());
                sb.Append(',');
                
                TagCodecUri uri = TagCodecService.Decode(tag.GetEpcString());
                if (uri != null)
                {
                    sb.Append(uri.Scheme);
                    sb.Append(',');
                    sb.Append(uri.Barcode);
                    sb.Append(',');
                    sb.Append(uri.Serial);
                }
                else
                {
                    sb.Append("N/A,N/A,N/A");
                }

                cnt++;               

                sb.Append(',');

                if (tag.irData != null)
                {
                    TagInformation tagInformation = TIDUtils.GetTagInformationFromHeader(tag.irData);

                    if (tagInformation != null)
                    {
                        sb.Append(tagInformation.TagModel);
                        sb.Append(',');
                        sb.Append(tagInformation.Company);
                    }
                    else
                    {
                        sb.Append("N/A,N/A,N/A");
                    }
                }
                else
                {
                    sb.Append("N/A,N/A,N/A");
                }

                sb.Append(',');

                sb.Append(tag.PhysicalAntenna);
                sb.Append(",");
                sb.Append(tag.rssi.ToString());
                sb.Append(',');
                sb.Append(tag.frequency.ToString());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
                        
        }

        public static class ServiceHelper
        {
            public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
            IPlatformApplication.Current.Services;
#elif ANDROID
                    IPlatformApplication.Current.Services;
#elif IOS || MACCATALYST
        IPlatformApplication.Current.Services;
#else
        null;
#endif
            public static TService GetService<TService>()
            {
                return Current.GetService<TService>();
            }
        }

        public static async Task<bool> ShareAsCsv(ContentPage page, string headerText, string items, string filetype = ".csv")
        {
            string fn = headerText + "_" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            fn = fn.Replace(' ', '_');
            string[] arr = { "Save to device", "Email", "Other", "Cancel" };
            string action;
            action = await page.DisplayActionSheet("Share", null, null, arr);
            if (string.IsNullOrEmpty(action))
                return false;
            if (action.Equals("Cancel"))
                return false;
            if (action.Equals("Save to device"))
            {              
                              
                try
                {                                        
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(items ?? ""));
                    
                    var result = await ServiceHelper.GetService<IFileSaver>().SaveAsync(headerText + filetype, ms);
                    result.EnsureSuccess();

                    if (result.IsSuccessful)
                        await page.DisplayAlert("File save success!", result.FilePath, "OK");
                    else
                        await page.DisplayAlert("File save Fails!", result.Exception.Message, "OK");                                                                               
                }
                catch(Exception e)
                {
                    ShowErrorSnackbar(e.Message);
                    return false;
                }       

                return true;
            }
            else if (action.Equals("Email"))
            {
                try
                {
                    EmailMessage message = new EmailMessage();
                    message.Subject = headerText;                    
                    message.Body = items;
                    await Email.ComposeAsync(message);
                }
                catch (Exception ex)
                {
                    ShowErrorSnackbar(ex.Message + "\nTry 'other'");
                    return false;
                }

                return true;
            }
            else if (action.Equals("Other"))
            {
                fn += filetype;
                var file = Path.Combine(FileSystem.CacheDirectory, fn);
                try
                {
                    File.WriteAllText(file, items);                           
                    ShareFileRequest share = new ShareFileRequest();
                    share.Title = "Share" + " " + headerText;
                    share.File = new ShareFile(file);
                    await Share.RequestAsync(share);
                }
                catch (Exception ex1)
                {
                    ShowErrorSnackbar(ex1.Message);
                    return false;
                }
            }

            return true;
        }
    }
}
