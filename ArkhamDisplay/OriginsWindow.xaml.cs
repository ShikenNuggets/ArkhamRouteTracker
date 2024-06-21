using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ArkhamDisplay{
	public partial class OriginsWindow : BaseWindow{
		public OriginsWindow() : base(Game.Origins){
			InitializeComponent();
			PostInitialize();
		}

		protected override SaveParser CreateSaveParser(){
			return new OriginsSave(Data.SaveLocations[(int)game], Data.SaveIDs[(int)game]);
		}

		protected override void SetCurrentRoute(){
			currentRoute = "OriginsDefault";
			currentSecondaryRoute = "OriginsCrime";
		}

		protected override void UpdateSecondaryRouteWindow(){
			CrimeInProgressGrid.Children.Clear();
			CrimeInProgressGrid.RowDefinitions.Clear();
			CrimeInProgressGrid.ColumnDefinitions.Clear();

			CrimeInProgressGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL1_WIDTH) });
			CrimeInProgressGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL2_WIDTH) });

			int lineCount = 1;
			foreach(Entry entry in Data.GetRoute(currentSecondaryRoute).entries){
				CrimeInProgressGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT / 2) });

				Rectangle rectangle = new Rectangle{
					Style = FindResource("GridRectangle") as Style
				};
				Grid.SetColumn(rectangle, 0);
				Grid.SetRow(rectangle, lineCount - 1);
				CrimeInProgressGrid.Children.Add(rectangle);

				TextBlock txt0 = new TextBlock{
					Text = entry.name,
					Style = FindResource("EntryText") as Style
				};
				Grid.SetColumn(txt0, 0);
				Grid.SetRow(txt0, lineCount - 1);
				CrimeInProgressGrid.Children.Add(txt0);

				rectangle = new Rectangle{
					Style = FindResource("GridRectangle") as Style
				};
				Grid.SetColumn(rectangle, 1);
				Grid.SetRow(rectangle, lineCount - 1);
				CrimeInProgressGrid.Children.Add(rectangle);

				if(saveParser.HasKey(entry, minRequiredMatches)){
					TextBlock txt1 = new TextBlock{
						Style = FindResource("DoneText") as Style,
						FontSize = 15.0
					};
					Grid.SetColumn(txt1, 1);
					Grid.SetRow(txt1, lineCount - 1);
					CrimeInProgressGrid.Children.Add(txt1);
				}

				lineCount++;
			}
		}

		protected override void SetStatsWindowStats(){
			if(statsWindow != null && progressCounter != null){
				statsWindow.SetStats(progressCounter.Text, "", ""); //TODO - If we ever get the actual Crime in Progress counter, display that here
			}
		}
	}
}