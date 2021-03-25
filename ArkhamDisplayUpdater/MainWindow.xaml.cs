using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ArkhamDisplayUpdater{
	internal class ReleaseModel{
		[JsonPropertyName("name")]
		public string Name{ get; set; }

		[JsonPropertyName("assets")]
		public Asset[] Assets{ get; set; }

		internal class Asset{
			[JsonPropertyName("browser_download_url")]
			public Uri BrowserDownloadUrl{ get; set; }
		}
	}

	public partial class MainWindow : Window{
		private HttpClient client;
		private Stream _stream;
		private ReleaseModel _latest;

		public MainWindow(){
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e){
			base.OnInitialized(e);
			CheckForUpdate();
		}

		private void CheckForUpdate(){
			UpdateUI("Checking for new version...");

			client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0");

			try{
				var response = client.GetAsync("https://api.github.com/repos/ShikenNuggets/ArkhamRouteTracker/releases").Result;

				var json = response.Content.ReadAsStringAsync().Result;

				var data = JsonSerializer.Deserialize<ReleaseModel[]>(json);

				_latest = data[0];

				response.Dispose();
			}catch{
				//Failed to retrieve data from server
				UpdateUI("Failed to retrieve data from server", false, true);
				return;
			}

			var file = "ArkhamDisplay.exe";
			if(File.Exists(file)){
				var fileVersion = Version.Parse(FileVersionInfo.GetVersionInfo(file).FileVersion);
				var latestVersion = Version.Parse(_latest.Name.Replace("v", ""));

				if(latestVersion <= fileVersion){
					//You are running the latest or newer version
					UpdateUI("You are already running the latest version", false, true);
					return;
				}

				//A newer update has been found
			}

			UpdateUI("A newer version is available. Would you like to download it?", true);
		}

		private void Update(){
			if(client == null){
				CheckForUpdate();
			}

			//Terminate all current Arkham Display processes
			var processes = Process.GetProcessesByName("ArkhamDisplay");
			foreach(var process in processes){
				process.Kill();
			}

			//Add a potential check for the specific file since there can be multiple releases
			var assets = _latest.Assets.First();

			try{
				var response = client.GetAsync(assets.BrowserDownloadUrl).Result;
			
				_stream = response.Content.ReadAsStreamAsync().Result;
				_stream.Seek(0, SeekOrigin.Begin);
			}catch{
				//Failed to retrieve data from server
				UpdateUI("Failed to retrieve data from server", false, true);
				return;
			}

			UpdateUI("Extracting...");

			var currentDir = Directory.GetCurrentDirectory();

			using(var archive = new ZipArchive(_stream)){
				foreach(var entry in archive.Entries){
					try{
						var folder = Path.Combine(currentDir, Path.GetDirectoryName(entry.FullName));
						Directory.CreateDirectory(folder);
						entry.ExtractToFile(Path.Combine(currentDir, entry.FullName), true);
					}catch{ /* Just continue */ }
				}
			}

			UpdateUI("Update complete", false, true);
		}

		private void OnUpdateClicked(object sender, RoutedEventArgs e){
			UpdateUI("Downloading new version...");
			Update();
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e){
			if(Process.GetProcessesByName("ArkhamDisplay").Length == 0){
				Process.Start("ArkhamDisplay.exe"); //Restart tracker if it isn't already running
			}
			
			Close();
		}

		private void UpdateUI(string text, bool showUpdateButton = false, bool showCloseButton = false){
			(FindName("UpdaterText") as TextBlock).Text = text;
			(FindName("UpdateButton") as Button).Visibility = showUpdateButton ? Visibility.Visible : Visibility.Hidden;
			(FindName("CloseButton") as Button).Visibility = showCloseButton ? Visibility.Visible : Visibility.Hidden;
		}
	}
}