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
			}else if(Data.Knight240){
				if(saveParser != null && saveParser.HasKey("Knightmare", 1)){
					minRequiredMatches = 2;
					currentRoute = "KnightNG+120";
				}else{
					minRequiredMatches = 1;
					currentRoute = "Knight120";
				}
			}else{
				currentRoute = "KnightDefault";
			}
		}

		protected override void UpdateUI(){
			FirstEndingMenuItem.IsChecked = Data.KnightFirstEnding;
			NGPlusMenuItem.IsChecked = Data.KnightNGPlus;
			OneTwentyPercentMenuItem.IsChecked = Data.Knight120;
			TwoFortyPercentMenuItem.IsChecked = Data.Knight240;
			base.UpdateUI();
		}

		protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null){
			//Some settings are incompatible, so if one switches one make sure the other does too
			if(sender == FirstEndingMenuItem && FirstEndingMenuItem.IsChecked){
				OneTwentyPercentMenuItem.IsChecked = false;
				TwoFortyPercentMenuItem.IsChecked = false;
			}else if(sender == OneTwentyPercentMenuItem && OneTwentyPercentMenuItem.IsChecked){
				FirstEndingMenuItem.IsChecked = false;
				TwoFortyPercentMenuItem.IsChecked = false;
			}else if(sender == TwoFortyPercentMenuItem && TwoFortyPercentMenuItem.IsChecked){
				NGPlusMenuItem.IsChecked = false;
				FirstEndingMenuItem.IsChecked = false;
				OneTwentyPercentMenuItem.IsChecked = false;
			}

			if(NGPlusMenuItem.IsChecked){
				minRequiredMatches = 2;
			}else{
				minRequiredMatches = 1;
			}

			Data.KnightFirstEnding = FirstEndingMenuItem.IsChecked;
			Data.KnightNGPlus = NGPlusMenuItem.IsChecked;
			Data.Knight120 = OneTwentyPercentMenuItem.IsChecked;
			Data.Knight240 = TwoFortyPercentMenuItem.IsChecked;

			base.UpdatePreferences(sender, e);
		}

		protected override void UpdatePercent(int doneEntries, int totalEntries)
		{
			double percentDone = 100.0 * doneEntries / totalEntries;
			if (TwoFortyPercentMenuItem.IsChecked)
			{
				percentDone = 1.2 * percentDone;
				if ("KnightNG+120".Equals(currentRoute))
				{
					percentDone += 120.0;
				}
			}
			progressCounter.Text = string.Format("{0:0.0}", percentDone) + "%";
			riddleCounter.Text = UpdateRiddleCount();
		}
	}
}