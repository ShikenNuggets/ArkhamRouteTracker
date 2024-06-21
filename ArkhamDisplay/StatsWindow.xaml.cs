using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArkhamDisplay{
	public partial class StatsWindow : Window{
		public bool isClosedByMainWindow = false;

		public StatsWindow(){
			InitializeComponent();

            MinWidth = 750;
            MinHeight = 400;

            Rect rect = Utils.DetermineFinalWindowRectPosition(Data.StatsWindowRect, MinWidth, MinHeight);
            Width = rect.Width;
            Height = rect.Height;
            Left = rect.X;
            Top = rect.Y;
        }

		protected override void OnClosed(EventArgs e){
			Data.StatsWindowRect = new Rect(Left, Top, 750, 400); //Window size is hardcoded for now, TODO - actual scaling logic for this would be nice

            if(!isClosedByMainWindow){ //This feels stupid and gross but whatever it works
				Data.StatsWindowOpen = false;
			}

            Data.Save();

            base.OnClosed(e);
		}

		public void SetStats(string percent, string riddles, string extra = ""){
			PercentageLabel.Text = percent;
			RiddleLabel.Text = riddles;
			ExtraLabel.Text = extra;
		}
	}
}