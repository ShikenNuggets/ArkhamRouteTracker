using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
			Style = (Style)FindResource(typeof(Window));

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

			if(string.IsNullOrWhiteSpace(Data.SaveLocations[(int)game])){
				OpenSavePathWindow();
			}
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

		//TODO - This might be better with a predicate
		private List<Entry> GetEntries(List<Entry> entries, bool completed){
			List<Entry> entriesToUse = new List<Entry>();
			foreach(Entry e in entries){
				if(saveParser.HasKey(e, minRequiredMatches) == completed){
					entriesToUse.Add(e);
				}
			}

			return entriesToUse;
		}

		protected virtual void UpdateRouteWindow(){
			displayGrid.Children.Clear();
			displayGrid.RowDefinitions.Clear();
			displayGrid.ColumnDefinitions.Clear();

			displayGrid.ShowGridLines = true;
			displayGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL1_WIDTH) });
			displayGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL2_WIDTH) });

			int lineCount = 1;
			int firstNotDone = -1;
			int numDone = 0;

			int totalEntries = Data.GetRoute(currentRoute).entries.Count;
			int doneEntries = -1; //TODO - two values (numDone and doneEntries) for this is kinda weird

			List<Entry> entriesToUse = Data.GetRoute(currentRoute).entries;
			if(Data.DisplayType == DisplayType.SortDoneToTop){
				entriesToUse = GetEntries(Data.GetRoute(currentRoute).entries, true);
				entriesToUse.AddRange(GetEntries(Data.GetRoute(currentRoute).entries, false));
			}else if(Data.DisplayType == DisplayType.HideDone){
				entriesToUse = GetEntries(Data.GetRoute(currentRoute).entries, false);
				doneEntries = totalEntries - entriesToUse.Count;
			}

			foreach(Entry entry in entriesToUse){
				displayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT) });
				TextBlock txtBlock = new TextBlock();
				txtBlock.Text = entry.name;
				txtBlock.TextWrapping = TextWrapping.Wrap;
				Grid.SetColumn(txtBlock, 0);
				Grid.SetRow(txtBlock, lineCount - 1);
				displayGrid.Children.Add(txtBlock);

				if(saveParser.HasKey(entry, minRequiredMatches)){
					txtBlock = new TextBlock();
					txtBlock.Text = "Done";
					Grid.SetColumn(txtBlock, 1);
					Grid.SetRow(txtBlock, lineCount - 1);
					displayGrid.Children.Add(txtBlock);
					numDone++;
				}else if(firstNotDone == -1){
					firstNotDone = lineCount;
				}
				lineCount++;
			}

			if(firstNotDone > -1){
				int scrollHeight = (firstNotDone - 4) * (ROW_HEIGHT);
				gridScroll.ScrollToVerticalOffset(scrollHeight);
			}

			//lineCount - 2 because the last row is "Done"
			double percentDone = 100.0 * numDone / (lineCount - 2);
			if(doneEntries >= 0){
				percentDone = 100.0 * doneEntries / totalEntries;
			}

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

		private void KillWindow(Window newWindow){
			if(newWindow != null){
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
				KillWindow(new AsylumWindow());
			}else if(sender == cityMenuItem && game != Game.City){
				KillWindow(new CityWindow());
			}else if(sender == originsMenuItem && game != Game.Origins){
				KillWindow(new OriginsWindow());
			}else if(sender == knightMenuItem && game != Game.Knight){
				KillWindow(new KnightWindow());
			}else{
				(sender as MenuItem).IsChecked = true;
			}
		}
	}
}