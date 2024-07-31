# maui_nur_app_template
 Sample .NET MAUI project for use with a Brady RFID reader.
 More info: https://nordicid.github.io/nurapi_docs_website/articles/introduction.html

 #### Basic features
	Permission handling (Helpers/PermissionHelper.cs)
	Platform specific initialization required by NurApi
	Required NurApi Nugets
	Toast/snackbar (from Maui communitytoolkit) for status messages.(Helpers/Utilities.cs)
	
 #### Reader discovery ((Helpers/ReaderDiscovery.cs))
	Searches readers from various sources (Bluetooth, local network, USB..)
	
 #### Reader connection (Helpers/ReaderConnect.cs)
	The user selects the reader from the list of found devices
	Last used connection saved in settings for auto connect when app start.
	Connecting/disconnecting handling when the application is active/inactive

#### Sample View Inventory	
	TxLevel adjust using 'Slider'
	Sound generation while reading
	Reader trigger for start/stop inventory streaming.	
	Share inventory results as *.csv
#### Sample View Barcode
	Read barcode and show it in display.
	Success beep generated.
	Reader trigger for start/stop
	(Helpers/ReadBarcode.cs)

## Installation
### Debug
#### Android physical
1. Install USB Device Connectivity

Tools -> Get Tools and Features -> Individual Components -> USB Device Connectivity

2. Install Android SDK Manager USB driver

Tools -> Android -> Android SDK Manager -> Tools -> Extras -> Google USB Driver

3. Restart VS

Now the run drop-down box should have an Android Local Devices

## Creating Visual Studio project using .NET MAUI RFID App template


1. Copy `.NET MAUI RFID App.zip` from [Releases](https://github.com/NordicID/maui_nur_app_template/releases) in to your Windows PC `Documents/Visual Studio 2022/Templates/ProjectTemplates` folder.
2. Open Visual Studio 2022
3. Create new project
4. Select template ".NET MAUI RFID App" (use search key: "RFID")
5. After project is created, go to `Tools-->NuGet Package Manager-->Package Manage Console`
6. Type the console command `dotnet restore` and hit enter.
7. After packets are restored, close PM console.
8. Modify "MAUI Shared" property from your app properties.
	- Application Title
	- Application ID use format like `com.<mycompany>.<appname>`
	- Application Display Version. Semantic versioning recommended (major.minor.patch)
	- Application version (start from 1)
9. Targeting to Android: Open `AndroidManifest.xml` and edit the Application name, Package name, Version information, etc.
10. Setup app icon, app theme etc..
11. Build and run on a physical device. Ensure the Android device is in developer mode and connect to the PC via USB.
12. Happy coding!
