using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ArkhamDisplay{
	public partial class CityWindow : BaseWindow{
		private const int SECONDARY_COL_WIDTH = 250;

		public CityWindow() : base(Game.City){
			InitializeComponent();
			PostInitialize();
		}

		protected override void SetCurrentRoute(){
			currentRoute = "CityDefault";
			currentSecondaryRoute = "CityPrisoners";
		}

		protected override List<Entry> GetEntriesForDisplay(Route route){
			if(!Data.CityBreakablesAtEnd){
				return base.GetEntriesForDisplay(route);
			}else{
				return route.GetEntriesWithPlaceholdersMoved();
			}
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

			PrisonerGrid.ColumnDefinitions.Add(new ColumnDefinition(){ Width = new GridLength(1.0, GridUnitType.Star) });

			int lineCount = 1;
			int savedCount = 0;
			foreach(Entry entry in Data.GetRoute(currentSecondaryRoute).entries){
				bool isSaved = saveParser.HasKey(entry.id, minRequiredMatches);

				if(!isSaved){
					PrisonerGrid.RowDefinitions.Add(new RowDefinition(){ Height = new GridLength(ROW_HEIGHT / 2) });

					Rectangle rectangle = new Rectangle{
						Style = FindResource("GridRectangle") as Style
					};
					Grid.SetColumn(rectangle, 0);
					Grid.SetRow(rectangle, lineCount - 1);
					PrisonerGrid.Children.Add(rectangle);

					TextBlock txt0 = new TextBlock{
						Text = entry.name,
						Style = FindResource("EntryText") as Style
					};
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