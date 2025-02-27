using MauiNurAppTemplate.Helpers;
using NurApiDotNet;
using Plugin.Maui.Audio;
using System.Diagnostics;
using static NurApiDotNet.NurApi;

namespace MauiNurAppTemplate
{
    public partial class App : Application
    {
        /// <summary>
        /// Single NurApi instance for this App.
        /// </summary>
        public static NurApi Nur { get; set; } = new NurApi();

        /// <summary>
        /// After successfull read connection, these are filled from reader
        /// </summary>
        public static ReaderInfo? ReaderInfo { get; set; }       
        public static DeviceCapabilites? DeviceCapabilites { get; set; }        
        public static AccessoryConfig ReaderAccessory {  get; set; }
        public static bool IsAccessories { get; set; } = false; //After connect, this flag goes true if device has any accessories like barcode scanner

        public static bool IsUpdating { get; set; } = false;
        /// <summary>
        /// Use this when need to read other banks than just EPC
        /// </summary>
        public static IrInformation InvReadParams { get; set; } = new IrInformation();

        /// <summary>
        /// Reader discovery utility helps to find correct reader
        /// </summary>
        public static ReaderDiscovery DeviceDiscovery { get; set; } = new ReaderDiscovery();

        /// <summary>
        /// Generating Audio sound when reading tags and barcode.
        /// </summary>
        public static IAudioPlayer? ErrorBeep;
        public static IAudioPlayer? TagSeenTick;
        public static IAudioPlayer? BarcodeSuccessBeep;

        public static List<string> LogStock { get; set; }

        /// <summary>
        /// When true, reader not disconnected when App going to inactive state (OnSleep)        
        /// </summary>
        public static bool KeepNurConnectedWhileInactive { get; set; } = false;

        public App()
        {
            InitializeComponent();
                        
            LogStock = new List<string>();

            //Loading sounds ready to play. Make sure these sound files found from "Resources\Raw"
            ErrorBeep = AudioManager.Current.CreatePlayer(FileSystem.OpenAppPackageFileAsync("System_error.mp3").GetAwaiter().GetResult());
            TagSeenTick = AudioManager.Current.CreatePlayer(FileSystem.OpenAppPackageFileAsync("tick02.wav").GetAwaiter().GetResult());
            BarcodeSuccessBeep = AudioManager.Current.CreatePlayer(FileSystem.OpenAppPackageFileAsync("barcode_sound.wav").GetAwaiter().GetResult());

            //used in inventory stream playing sound in loop..
            TagSeenTick.Loop = true;                  

            ReaderConnect.Init();
            
            Nur.LogEvent += Nur_LogEvent;

            //Activate this if need to get more detailed log from reader. Error logs in to Nur_LogEvent are received in all cases.
            App.Nur.SetLogLevel(NurApi.LOG_ERROR);
            //Nur.SetLogLevel(NurApi.LOG_ERROR|LOG_VERBOSE);

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            var navigationPage = new NavigationPage(new MainPage());
            navigationPage.BarBackground = Color.FromArgb("#324C86");
            navigationPage.BarTextColor = Colors.White;
            
            MainPage = navigationPage;            

        }               

        private void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            Utilities.ShowSnackbar("NETWORK CONNECTIVITY: " + e.NetworkAccess.ToString(), Colors.Gold,Colors.Black);            
        }

        private void Nur_LogEvent(object? sender, LogEventArgs e)
        {
            string message = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + " : " + e.message;
            LogStock.Add(message);
            Console.WriteLine(message);

            //Console.WriteLine("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now + " " + e.message);
            //Debug.WriteLine(e.message);
        }
                
        protected override void OnStart()
        {
            base.OnStart();
            ReaderConnect.AppStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            if (Nur.IsConnected())
            {
                if (KeepNurConnectedWhileInactive)
                {                   
                    Debug.WriteLine("OnSleep() KEEP CONNECTION UP");
                    return; //Wanted to keep connection up
                }                
            }

            //When app goes inactive, reader disconnect.
            ReaderConnect.AppSleep();

        }

        protected override void OnResume()
        {
            base.OnResume();

            //Reconnect reader if it was connected before sleep.
            ReaderConnect.AppResume();
        }                
    }
}
