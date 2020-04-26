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
			}else if(Data.KnightNGPlus && !Data.Knight120){
				currentRoute = "KnightNG+";
			}else if(Data.KnightNGPlus && Data.Knight120){
				currentRoute = "KnightNG+120";
			}else if(Data.Knight120 && !Data.KnightNGPlus){
				currentRoute = "Knight120";
			}else{
				currentRoute = "KnightDefault";
			}
		}

		protected override void UpdateUI(){
			FirstEndingMenuItem.IsChecked = Data.KnightFirstEnding;
			NGPlusMenuItem.IsChecked = Data.KnightNGPlus;
			OneTwentyPercentMenuItem.IsChecked = Data.Knight120;
			base.UpdateUI();
		}

		protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null){
			//First Ending and 120% are incompatible, so if one switches one make sure the other does too
			if(sender == FirstEndingMenuItem && FirstEndingMenuItem.IsChecked){
				OneTwentyPercentMenuItem.IsChecked = false;
			}else if(sender == OneTwentyPercentMenuItem && OneTwentyPercentMenuItem.IsChecked){
				FirstEndingMenuItem.IsChecked = false;
			}

			if(NGPlusMenuItem.IsChecked){
				minRequiredMatches = 2;
			}else{
				minRequiredMatches = 1;
			}

			Data.KnightFirstEnding = FirstEndingMenuItem.IsChecked;
			Data.KnightNGPlus = NGPlusMenuItem.IsChecked;
			Data.Knight120 = OneTwentyPercentMenuItem.IsChecked;

			base.UpdatePreferences(sender, e);
		}
	}
}