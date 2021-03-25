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
		public MainWindow(){
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e){
			base.OnInitialized(e);
			Update();
		}

		private void Update(){
			//Terminate all current Arkham Display processes
			var processes = Process.GetProcessesByName("ArkhamDisplay");
			foreach(var process in processes){
				process.Kill();
			}

			UpdateUI("Checking for new version...");

			using(var client = new HttpClient()){
				client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0");

				try{
					var response = client.GetAsync("https://api.github.com/repos/ShikenNuggets/ArkhamRouteTracker/releases").Result;

					var json = response.Content.ReadAsStringAsync().Result;

					var data = JsonSerializer.Deserialize<ReleaseModel[]>(json);

					_latest = data[0];

					response.Dispose();
				}catch{
					//Failed to retrieve data from server
					UpdateUI("Failed to retrieve data from server", true);
					return;
				}

				var file = "ArkhamDisplay.exe";
				if(File.Exists(file)){
					var fileVersion = Version.Parse(FileVersionInfo.GetVersionInfo(file).FileVersion);
					var latestVersion = Version.Parse(_latest.Name.Replace("v", ""));

					if(latestVersion <= fileVersion){
						//You are running the latest or newer version
						UpdateUI("You are already running the latest version", true);
						return;
					}

					//A newer update has been found
				}

				//Add a potential check for the specific file since there can be multiple releases
				var assets = _latest.Assets.First();

				UpdateUI("Downloading new version...");

				try{
					var response = client.GetAsync(assets.BrowserDownloadUrl).Result;
				
					_stream = response.Content.ReadAsStreamAsync().Result;
					_stream.Seek(0, SeekOrigin.Begin);
				}catch{
					//Failed to retrieve data from server
					UpdateUI("Failed to retrieve data from server", true);
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

				UpdateUI("Update complete", true);
			}
		}

		private void OnOKClicked(object sender, RoutedEventArgs e){
			Process.Start("ArkhamDisplay.exe");
			Close();
		}

		private void UpdateUI(string text, bool showButton = false){
			(FindName("UpdaterText") as TextBlock).Text = text;
			(FindName("OKButton") as Button).Visibility = showButton ? Visibility.Visible : Visibility.Hidden;
		}

		private static Stream _stream;

		private static ReleaseModel _latest;
	}
}