using System.Windows;
using System.Windows.Controls;

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

			CrimeInProgressGrid.ShowGridLines = true;
			CrimeInProgressGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL1_WIDTH) });
			CrimeInProgressGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(COL2_WIDTH) });

			int lineCount = 1;
			foreach(Entry entry in Data.GetRoute(currentSecondaryRoute).entries){
				CrimeInProgressGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT / 2) });
				TextBlock txt0 = new TextBlock();
				txt0.Text = entry.name;
				txt0.TextWrapping = TextWrapping.Wrap;
				Grid.SetColumn(txt0, 0);
				Grid.SetRow(txt0, lineCount - 1);
				CrimeInProgressGrid.Children.Add(txt0);

				if(saveParser.HasKey(entry, minRequiredMatches)){
					TextBlock txt1 = new TextBlock();
					txt1.Text = "Done";
					Grid.SetColumn(txt1, 1);
					Grid.SetRow(txt1, lineCount - 1);
					CrimeInProgressGrid.Children.Add(txt1);
				}

				lineCount++;
			}
		}
	}
}