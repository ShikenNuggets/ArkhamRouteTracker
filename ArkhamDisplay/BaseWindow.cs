using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		private Button stopButton;
		private Button startButton;
		private Grid displayGrid;
		private ScrollViewer gridScroll;
		private TextBlock progressCounter;
		private TextBlock riddleCounter;
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

			base.OnClosed(e);
		}

		protected virtual SaveParser CreateSaveParser(){
			return new SaveParser(Data.SaveLocations[(int)game], Data.SaveIDs[(int)game]);
		}

		protected void Stop_Button_Click(object sender, RoutedEventArgs e){
			saveParser = null;
			stopButton.IsEnabled = false;
			startButton.IsEnabled = true;
			updateWorker.CancelAsync();
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
			saveParser.Update();

			SetCurrentRoute();
			if(!string.IsNullOrWhiteSpace(currentRoute)){
				UpdateRouteWindow();
			}

			if(!string.IsNullOrWhiteSpace(currentSecondaryRoute)){
				UpdateSecondaryRouteWindow();
			}
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

			displayGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL1_WIDTH) });
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

					finalEntries.Add(new FinalEntry(entry.name, true));
				}else{
					if(Data.DisplayType == DisplayType.All || Data.DisplayType == DisplayType.HideDone){
						finalEntries.Add(new FinalEntry(entry.name, false));
					}else{
						bottomEntries.Add(new FinalEntry(entry.name, false));
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

			double percentDone = 100.0 * doneEntries / (totalEntries - 1);
			progressCounter.Text = string.Format("{0:0.0}", percentDone) + "%";
			riddleCounter.Text = UpdateRiddleCount();
		}

		protected virtual void UpdateSecondaryRouteWindow(){}

		protected virtual string UpdateRiddleCount(){ return ""; }

		protected virtual void UpdateUI(){
			if(saveParser != null){
				switch(saveParser.GetID()){
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
				}
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
	}
}