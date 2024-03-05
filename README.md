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