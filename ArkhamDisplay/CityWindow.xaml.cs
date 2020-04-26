using System.Windows;
using System.Windows.Controls;

namespace ArkhamDisplay{
	public partial class CityWindow : BaseWindow{
		private const int SECONDARY_COL_WIDTH = 250;

		public CityWindow() : base(Game.City){
			InitializeComponent();
			PostInitialize();
		}

		protected override void SetCurrentRoute(){
			if(Data.CityBreakablesAtEnd){
				currentRoute = "CityBreakablesAtBottom";
			}else{
				currentRoute = "CityDefault";
			}

			currentSecondaryRoute = "CityPrisoners";
		}

		protected override void UpdateUI(){
			BreakablesAtBottomMenuItem.IsChecked = Data.CityBreakablesAtEnd;
			base.UpdateUI();
		}

		protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null){
			Data.CityBreakablesAtEnd = BreakablesAtBottomMenuItem.IsChecked;
			base.UpdatePreferences(sender, e);
	}

		protected override void UpdateSecondaryRouteWindow(){
			PrisonerGrid.Children.Clear();
			PrisonerGrid.RowDefinitions.Clear();
			PrisonerGrid.ColumnDefinitions.Clear();

			PrisonerGrid.ShowGridLines = true;
			PrisonerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SECONDARY_COL_WIDTH) });

			int lineCount = 1;
			int savedCount = 0;
			foreach(Entry entry in Data.GetRoute(currentSecondaryRoute).entries){
				bool isSaved = saveParser.HasKey(entry.id);

				if(!isSaved){
					PrisonerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT / 2) });
					TextBlock txt0 = new TextBlock();
					txt0.Text = entry.name;
					txt0.TextWrapping = TextWrapping.Wrap;
					Grid.SetColumn(txt0, 0);
					Grid.SetRow(txt0, lineCount - 1);
					PrisonerGrid.Children.Add(txt0);
					lineCount++;
				}else{
					savedCount++;
				}
			}
			SavedPrisoners.Text = savedCount + " saved";
		}

		protected override string UpdateRiddleCount(){
			return saveParser.GetMatch(@"\b\d*\/400|\d*\/440\b");
		}
	}
}