using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ArkhamDisplay{
	public abstract class BaseWindow : Window{
		protected Game game = Game.None;
		protected SaveParser saveParser = null;
		protected int minRequiredMatches = 1;
		private BackgroundWorker updateWorker;
		public const int COL1_WIDTH = 210;
		public const int COL2_WIDTH = 40;
		public const int ROW_HEIGHT = 40;

		protected string currentRoute = "";
		protected string currentSecondaryRoute = "";
		protected TextBlock progressCounter;
		protected TextBlock riddleCounter;

		protected StatsWindow statsWindow = null;

		private Button stopButton;
		private Button startButton;
		private Grid displayGrid;
		private ScrollViewer gridScroll;
		private MenuItem asylumMenuItem;
		private MenuItem cityMenuItem;
		private MenuItem originsMenuItem;
		private MenuItem knightMenuItem;
		private RadioButton saveSelector0;
		private RadioButton saveSelector1;
		private RadioButton saveSelector2;
		private RadioButton saveSelector3;

		public BaseWindow(Game game){
			this.game = game;
			Data.SelectedGame = game;
			Data.Save();
		}

		protected void PostInitialize(){
			stopButton = FindName("StopButton") as Button;
			startButton = FindName("StartButton") as Button;
			displayGrid = FindName("DisplayGrid") as Grid;
			gridScroll = FindName("GridScroll") as ScrollViewer;
			progressCounter = FindName("ProgressCounter") as TextBlock;
			riddleCounter = FindName("RiddleCounter") as TextBlock;
			asylumMenuItem = FindName("AsylumMenuItem") as MenuItem;
			cityMenuItem = FindName("CityMenuItem") as MenuItem;
			originsMenuItem = FindName("OriginsMenuItem") as MenuItem;
			knightMenuItem = FindName("KnightMenuItem") as MenuItem;
			saveSelector0 = FindName("Save0") as RadioButton;
			saveSelector1 = FindName("Save1") as RadioButton;
			saveSelector2 = FindName("Save2") as RadioButton;
			saveSelector3 = FindName("Save3") as RadioButton;

			if(stopButton == null || startButton == null || displayGrid == null || gridScroll == null || progressCounter == null || riddleCounter == null || saveSelector0 == null || saveSelector1 == null || saveSelector2 == null || saveSelector3 == null){
				throw new NullReferenceException();
			}

			UpdateUI();
		}

		protected override void OnInitialized(EventArgs e){
			var screenLeft = SystemParameters.VirtualScreenLeft;
			var screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
			var screenTop = SystemParameters.VirtualScreenTop;
			var screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

			//Load window stuff
			MinWidth = 280;
			MinHeight = 360;

			Rect rect = Data.WindowRect[(int)game];
			if(rect.Width >= MinWidth && rect.Height >= MinHeight){
				Width = rect.Width;
				Height = rect.Height;

				if(rect.X < screenLeft){
					rect.X = screenLeft;
				}else if(rect.X + rect.Width > screenRight){
					rect.X = screenRight - rect.Width;
				}

				if(rect.Y < screenTop){
					rect.Y = screenTop;
				}else if(rect.Y + rect.Height > screenBottom){
					rect.Y = screenBottom - rect.Height;
				}
				 
				Left = rect.X;
				Top = rect.Y;
			}

			var mainGrid = FindName("MainGrid");
			if(mainGrid != null && mainGrid is Grid){
				foreach(RowDefinition row in (mainGrid as Grid).RowDefinitions){
					if(row.Name == "MainRow"){
						row.Height = new GridLength(Data.MainRowHeight, GridUnitType.Star);
					}else if(row.Name == "SecondaryRow"){
						row.Height = new GridLength(Data.SecondaryRowHeight, GridUnitType.Star);
					}
				}
			}

			if(Data.StatsWindowOpen){
				OpenStatsWindow();
			}

			base.OnInitialized(e);
		}

		protected override void OnActivated(EventArgs e){
			if(string.IsNullOrWhiteSpace(Data.SaveLocations[(int)game])){
				OpenSavePathWindow();
			}

			base.OnActivated(e);
		}

		protected override void OnClosed(EventArgs e){
			stopButton.IsEnabled = false;
			startButton.IsEnabled = true;

			if(updateWorker != null && updateWorker.IsBusy){
				updateWorker.CancelAsync();
			}

			//Save window stuff
			Data.WindowRect[(int)game].X = Left;
			Data.WindowRect[(int)game].Y = Top;
			Data.WindowRect[(int)game].Width = Width;
			Data.WindowRect[(int)game].Height = Height;

			var mainGrid = FindName("MainGrid");
			if(mainGrid != null && mainGrid is Grid){
				foreach(RowDefinition row in (mainGrid as Grid).RowDefinitions){
					if(row.Name == "MainRow"){
						Data.MainRowHeight = row.Height.Value;
					}else if(row.Name == "SecondaryRow"){
						Data.SecondaryRowHeight = row.Height.Value;
					}
				}
			}
			Data.Save();

			if(statsWindow != null){
				statsWindow.isClosedByMainWindow = true;
				statsWindow.Close();
			}

			base.OnClosed(e);
		}

		protected virtual SaveParser CreateSaveParser(){
			return new SaveParser(Data.SaveLocations[(int)game], Data.SaveIDs[(int)game]);
		}

		protected void Stop_Button_Click(object sender, RoutedEventArgs e){
			updateWorker.CancelAsync();
			stopButton.IsEnabled = false;
			startButton.IsEnabled = true;
			lock(saveParser){ saveParser = null; }
		}

		protected void Start_Button_Click(object sender, RoutedEventArgs e){
			if(string.IsNullOrWhiteSpace(Data.SaveLocations[(int)game])){
				OpenSavePathWindow();
			}

			if(!System.IO.Directory.Exists(Data.SaveLocations[(int)game])){
				System.Windows.MessageBox.Show("Invalid save file! Please ensure the file exists and that the path has been entered correctly");
				return;
			}

			updateWorker = new BackgroundWorker();
			updateWorker.WorkerSupportsCancellation = true;
			updateWorker.WorkerReportsProgress = true;
			updateWorker.DoWork += BackgroundWorkerOnDoWork;
			updateWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;

			updateWorker.RunWorkerAsync();

			stopButton.IsEnabled = true;
			startButton.IsEnabled = false;

			saveParser = CreateSaveParser();
		}

		private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e){
			BackgroundWorker worker = (BackgroundWorker)sender;
			while(!worker.CancellationPending){
				worker.ReportProgress(0, "Dummy");
				Thread.Sleep(Data.RefreshRate);
			}
		}

		private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e){
			try{
				Update();
			}catch(Exception ex){
				stopButton.IsEnabled = false;
				startButton.IsEnabled = true;
				updateWorker.CancelAsync();
				MessageBox.Show("Error: " + ex.Message);
			}
		}

		protected abstract void SetCurrentRoute();

		private void Update(){
			string saveFile = Data.SaveLocations[(int)game];
			if(string.IsNullOrWhiteSpace(saveFile)){
				throw new Exception("Save file path is not valid");
			}

			if(saveParser != null){
				bool updated = saveParser.Update();
				if(!updated){
					return; //Save file didn't change, no need to do anythign else
				}
			}

			SetCurrentRoute();
			if(!string.IsNullOrWhiteSpace(currentRoute)){
				UpdateRouteWindow();
			}

			if(!string.IsNullOrWhiteSpace(currentSecondaryRoute)){
				UpdateSecondaryRouteWindow();
			}

			SetStatsWindowStats();
		}

		private struct FinalEntry{
			public FinalEntry(string name_, bool done_){
				name = name_;
				done = done_;
			}

			public string name;
			public bool done;
		}

		protected virtual List<Entry> GetEntriesForDisplay(Route route){
			return route.GetEntriesWithoutPlaceholders();
		}

		protected virtual void UpdateRouteWindow(){
			displayGrid.Children.Clear();
			displayGrid.RowDefinitions.Clear();
			displayGrid.ColumnDefinitions.Clear();

			displayGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.8, GridUnitType.Star) });
			displayGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL2_WIDTH) });

			int lineCount = 1;
			int firstNotDone = -1;

			List<Entry> routeEntries = GetEntriesForDisplay(Data.GetRoute(currentRoute));
			int totalEntries = routeEntries.Count;
			int doneEntries = 0;

			List<FinalEntry> finalEntries = new List<FinalEntry>(totalEntries);
			List<FinalEntry> bottomEntries = new List<FinalEntry>(totalEntries);
			foreach(Entry entry in routeEntries){
				if(saveParser.HasKey(entry, minRequiredMatches)){
					doneEntries++;
					if(Data.DisplayType == DisplayType.HideDone){
						continue;
					}

					finalEntries.Add(new FinalEntry(GetEntryName(entry), true));
				}else{
					if(Data.DisplayType == DisplayType.All || Data.DisplayType == DisplayType.HideDone){
						finalEntries.Add(new FinalEntry(GetEntryName(entry), false));
					}else{
						bottomEntries.Add(new FinalEntry(GetEntryName(entry), false));
					}
				}
			}
			finalEntries.AddRange(bottomEntries);

			foreach(FinalEntry entry in finalEntries){
				displayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT) });

				Rectangle rectangle = new Rectangle{
					Style = FindResource("GridRectangle") as Style
				};
				Grid.SetColumn(rectangle, 0);
				Grid.SetRow(rectangle, lineCount - 1);
				displayGrid.Children.Add(rectangle);

				TextBlock txtBlock = new TextBlock{
					Text = entry.name,
					Style = FindResource("EntryText") as Style
				};
				Grid.SetColumn(txtBlock, 0);
				Grid.SetRow(txtBlock, lineCount - 1);
				displayGrid.Children.Add(txtBlock);

				rectangle = new Rectangle{
					Style = FindResource("GridRectangle") as Style
				};
				Grid.SetColumn(rectangle, 1);
				Grid.SetRow(rectangle, lineCount - 1);
				displayGrid.Children.Add(rectangle);

				if(entry.done){
					txtBlock = new TextBlock{
						Style = FindResource("DoneText") as Style
					};
					Grid.SetColumn(txtBlock, 1);
					Grid.SetRow(txtBlock, lineCount - 1);
					displayGrid.Children.Add(txtBlock);
				}else if(firstNotDone == -1){
					firstNotDone = lineCount;
				}
				lineCount++;
			}

			if(firstNotDone > -1){
				int scrollHeight = (firstNotDone - 4) * (ROW_HEIGHT);
				gridScroll.ScrollToVerticalOffset(scrollHeight);
			}
			UpdatePercent(doneEntries, totalEntries);
		}

		protected virtual void UpdatePercent(int doneEntries, int totalEntries)
		{
			double percentDone = 100.0 * doneEntries / totalEntries;
			progressCounter.Text = string.Format("{0:0.0}", percentDone) + "%";
			riddleCounter.Text = GetRiddleCount();

			SetStatsWindowStats();
		}

		protected virtual void UpdateSecondaryRouteWindow(){}

		protected virtual string GetRiddleCount(){ return ""; }

		protected virtual string GetEntryName(Entry entry) { return entry.name; }

		protected virtual void UpdateUI(){
			int value = Data.SaveIDs[(int)game];
			switch(value){
				case 0:
					saveSelector0.IsChecked = true;
					break;
				case 1:
					saveSelector1.IsChecked = true;
					break;
				case 2:
					saveSelector2.IsChecked = true;
					break;
				case 3:
					saveSelector3.IsChecked = true;
					break;
				default:
					saveSelector0.IsChecked = true;
					break;
			}

			if(Data.ShowPercent){
				progressCounter.Visibility = Visibility.Visible;
			}else{
				progressCounter.Visibility = Visibility.Collapsed;
			}

			if(Data.ShowRiddleCount){
				riddleCounter.Visibility = Visibility.Visible;
			}else{
				riddleCounter.Visibility = Visibility.Collapsed;
			}
		}

		protected virtual void UpdatePreferences(object sender = null, RoutedEventArgs e = null){
			if(saveSelector0.IsChecked == true && Data.SaveIDs[(int)game] != 0){
				Data.SaveIDs[(int)game] = 0;
				saveParser = CreateSaveParser();
			}else if(saveSelector1.IsChecked == true && Data.SaveIDs[(int)game] != 1){
				Data.SaveIDs[(int)game] = 1;
				saveParser = CreateSaveParser();
			}else if(saveSelector2.IsChecked == true && Data.SaveIDs[(int)game] != 2){
				Data.SaveIDs[(int)game] = 2;
				saveParser = CreateSaveParser();
			}else if(saveSelector3.IsChecked == true && Data.SaveIDs[(int)game] != 3){
				Data.SaveIDs[(int)game] = 3;
				saveParser = CreateSaveParser();
			}

			Data.Save();
		}

		protected void CheckForUpdatedRoutes(object sender = null, RoutedEventArgs e = null){
			List<string> routesWithUpdates = new List<string>();
			Dictionary<string, string> routeFileData = new Dictionary<string, string>();

			try{
				var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("ArkhamRouteTracker"));
				var routes = client.Repository.Content.GetAllContents("ShikenNuggets", "ArkhamRouteTracker", "ArkhamDisplay/Routes").Result;
				foreach(var r in routes){
					var rawData = client.Repository.Content.GetRawContent("ShikenNuggets", "ArkhamRouteTracker", r.Path).Result;

					string dataString = System.Text.Encoding.UTF8.GetString(rawData);
					dataString = dataString.Replace("\n", "\r\n"); //Replace Unix line endings with Windows line endings
					routeFileData.Add(r.Name, dataString);
					
					var split = Regex.Split(dataString, @"(?<=[\n])").ToList();

					if(!Data.HasRouteFile(r.Name)){
						routesWithUpdates.Add(r.Name);
						continue;
					}

					if(r.Name.Contains("json")){
						if(Utils.GetSHA1Hash("Routes/" + r.Name) != Utils.GetSHA1Hash(split)){
							routesWithUpdates.Add(r.Name);
							continue;
						}
					}else{
						Route r1 = new Route("Routes/" + r.Name);
						Route r2 = new Route(null, split);
						if(r1.IsEqual(r2) == false){
							routesWithUpdates.Add(r.Name);
							continue;
						}
					}

					
				}
			}catch(Exception){
				MessageBox.Show("An error occurred while checking for updates, please try again later.");
				return;
			}

			MessageBoxResult result = MessageBoxResult.No;
			if(routesWithUpdates.Count > 0){
				result = MessageBox.Show("The following routes have updates. Would you like to download? Any custom changes will be lost.\n" + Utils.ListToNewlinedString(routesWithUpdates), "Updates Available", MessageBoxButton.YesNo);
			}else{
				MessageBox.Show("Routes are already up to date.");
			}

			try{
				if(result == MessageBoxResult.Yes){
					foreach(var v in routeFileData){
						if(routesWithUpdates.Contains(v.Key)){
							System.IO.File.WriteAllText("Routes/" + v.Key, v.Value);
						}
					}

					stopButton.IsEnabled = false;
					startButton.IsEnabled = true;

					if(updateWorker != null && updateWorker.IsBusy){
						updateWorker.CancelAsync();
					}

					Data.ReloadRoutes();
					MessageBox.Show("Routes successfully updated.");
				}
			}catch(System.IO.IOException){
				MessageBox.Show("An error occurred while updating the route files!");
			}
		}

		protected void OpenSavePathWindow(object sender = null, RoutedEventArgs e = null){
			SetSavePathWindow wnd = new SetSavePathWindow(game);
			wnd.Activate();
			if(wnd.ShowDialog() == true){
				UpdateUI();
			}
		}

		private void KillWindow(Window newWindow, Game newGame, MenuItem sender){
			if(newWindow != null){
				if(string.IsNullOrWhiteSpace(Data.SaveLocations[(int)newGame])){
					Window wnd = new SetSavePathWindow(newGame);
					wnd.Activate();
					if(wnd.ShowDialog() == false){
						newWindow.Close();
						sender.IsChecked = false;
						return;
					}
				}

				newWindow.Activate();
				newWindow.Show();
			}

			Close();
		}

		protected void PrefMenuItem_Click(object sender, RoutedEventArgs e){
			PreferencesWindow wnd = new PreferencesWindow();
			wnd.Activate();
			if(wnd.ShowDialog() == true){
				UpdateUI();
			}
		}

		protected virtual void SwitchGameWindow(object sender, RoutedEventArgs e){
			if(sender == null || sender is MenuItem == false){
				return;
			}

			if(sender == asylumMenuItem && game != Game.Asylum){
				KillWindow(new AsylumWindow(), Game.Asylum, sender as MenuItem);
			}else if(sender == cityMenuItem && game != Game.City){
				KillWindow(new CityWindow(), Game.City, sender as MenuItem);
			}else if(sender == originsMenuItem && game != Game.Origins){
				KillWindow(new OriginsWindow(), Game.Origins, sender as MenuItem);
			}else if(sender == knightMenuItem && game != Game.Knight){
				KillWindow(new KnightWindow(), Game.Knight, sender as MenuItem);
			}else{
				(sender as MenuItem).IsChecked = true;
			}
		}

		protected virtual void OpenStatsWindow(object sender = null, RoutedEventArgs e = null){
			if(statsWindow != null){
				if(statsWindow.IsVisible){
					statsWindow.Focus();
					return;
				}else{
					statsWindow.Close();
					statsWindow = null;
				}
			}

			statsWindow = new StatsWindow();
			statsWindow.Activate();
			statsWindow.Show();
			SetStatsWindowStats();

			Data.StatsWindowOpen = true;
			Data.Save();
		}

		protected virtual void SetStatsWindowStats(){
			if(statsWindow != null && progressCounter != null && riddleCounter != null){
				statsWindow.SetStats(progressCounter.Text, riddleCounter.Text + " riddles");
			}
		}
	}
}