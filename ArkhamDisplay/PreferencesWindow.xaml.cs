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
		}

		private void OKButton_Click(object sender, RoutedEventArgs e){
			if(OneSecond.IsChecked == true){
				Data.RefreshRate = 1000;
			}else if (TwoSeconds.IsChecked == true){
				Data.RefreshRate = 2000;
			}else if (TenSeconds.IsChecked == true){
				Data.RefreshRate = 10000;
			}

			Data.ShowPercent = ShowProgressCheckbox.IsChecked ?? false;
			Data.ShowRiddleCount = ShowRiddlesCheckbox.IsChecked ?? false;

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