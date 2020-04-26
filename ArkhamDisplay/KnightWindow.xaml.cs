using System.Windows;
using System.Windows.Controls;

namespace ArkhamDisplay{
	public partial class KnightWindow : BaseWindow{
		public KnightWindow() : base(Game.Knight){
			InitializeComponent();
			PostInitialize();
		}

		protected override SaveParser CreateSaveParser(){
			return new KnightSave(Data.SaveLocations[(int)game], Data.SaveIDs[(int)game]);
		}

		protected override void SetCurrentRoute(){
			if(Data.KnightFirstEnding){
				currentRoute = "KnightFirstEnding";
			}else if (Data.KnightNGPlus){
				currentRoute = "KnightNG+";
			}else{
				currentRoute = "KnightDefault";
			}
		}

		protected override void UpdateUI(){
			FirstEndingMenuItem.IsChecked = Data.KnightFirstEnding;
			NGPlusMenuItem.IsChecked = Data.KnightNGPlus;
			//OneTwentyPercentMenuItem.IsChecked = Data.knight120;
			base.UpdateUI();
		}

		protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null){
			FirstEndingMenuItem.IsChecked = false;
			NGPlusMenuItem.IsChecked = false;
			//OneTwentyPercentMenuItem.IsChecked = false;

			if(sender is MenuItem){
				(sender as MenuItem).IsChecked = true; //TODO - in hindsight this is weird and will probably cause bugs
			}

			Data.KnightFirstEnding = FirstEndingMenuItem.IsChecked;
			Data.KnightNGPlus = NGPlusMenuItem.IsChecked;
			//Data.knight120 = OneTwentyPercentMenuItem.IsChecked;

			base.UpdatePreferences(sender, e);
		}
	}
}