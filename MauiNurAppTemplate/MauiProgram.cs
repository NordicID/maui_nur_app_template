using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;

namespace MauiNurAppTemplate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                    
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

                .ConfigureMopups();



#if __ANDROID__
            NurApiDotNet.Android.Support.Init(
            Microsoft.Maui.ApplicationModel.Platform.AppContext);
#elif __IOS__ || __MACCATALYST__
            NurApiDotNet.iOS.Support.Init();
#elif WINDOWS
            NordicID.NurApi.USBTransport.Support.Init();
            NordicID.NurApi.USBTransport.Support.EnableFTIDDeviceDiscovery = true;
#endif


#if DEBUG

            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);           

            return builder.Build();
        }
    }
}
