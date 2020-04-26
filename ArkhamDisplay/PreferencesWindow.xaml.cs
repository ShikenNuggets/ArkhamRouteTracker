using System.Windows;

namespace ArkhamDisplay{
	public partial class PreferencesWindow : Window{
		public PreferencesWindow(){
			InitializeComponent();

			switch(Data.RefreshRate){
				case 1000:
					OneSecond.IsChecked = true;
					break;
				case 2000:
					TwoSeconds.IsChecked = true;
					break;
				case 10000:
					TenSeconds.IsChecked = true;
					break;
			}

			ShowProgressCheckbox.IsChecked = Data.ShowPercent;
			ShowRiddlesCheckbox.IsChecked = Data.ShowRiddleCount;

			switch(Data.DisplayType){
				case DisplayType.All:
					ShowAllButton.IsChecked = true;
					break;
				case DisplayType.SortDoneToTop:
					SortDoneButton.IsChecked = true;
					break;
				case DisplayType.HideDone:
					HideDoneButton.IsChecked = true;
					break;
			}
		}

		private void OKButton_Click(object sender, RoutedEventArgs e){
			if(OneSecond.IsChecked == true){
				Data.RefreshRate = 1000;
			}else if(TwoSeconds.IsChecked == true){
				Data.RefreshRate = 2000;
			}else if(TenSeconds.IsChecked == true){
				Data.RefreshRate = 10000;
			}

			Data.ShowPercent = ShowProgressCheckbox.IsChecked ?? false;
			Data.ShowRiddleCount = ShowRiddlesCheckbox.IsChecked ?? false;

			if(ShowAllButton.IsChecked == true){
				Data.DisplayType = DisplayType.All;
			}else if(SortDoneButton.IsChecked == true){
				Data.DisplayType = DisplayType.SortDoneToTop;
			}else if(HideDoneButton.IsChecked == true){
				Data.DisplayType = DisplayType.HideDone;
			}

			Data.Save();
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e){
			DialogResult = false;
			Close();
		}
	}
}