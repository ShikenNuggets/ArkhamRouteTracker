using System.Windows;

namespace ArkhamDisplay
{
    public partial class InitialSetGameWindow : Window
    {
        public InitialSetGameWindow()
        {
            Data.Load();
            (Application.Current as App).ChangeSkin(Data.UseTheme());

            if (OpenWindowForGame(Data.SelectedGame))
            {
                Close();
                return;
            }

            InitializeComponent();
        }

        private bool OpenWindowForGame(Game game, bool skipSaveDataCheck = false)
        {
            if (game == Game.None)
            {
                return false;
            }

            if (!skipSaveDataCheck && string.IsNullOrWhiteSpace(Data.SaveLocations[(int)game]))
            {
                SetSavePathWindow wnd = new SetSavePathWindow(game);
                wnd.Activate();
                if (wnd.ShowDialog() == false)
                {
                    return false; //Save file selection was cancelled, do not continue
                }
            }

            Window window;
            switch (game)
            {
                case Game.Asylum:
                    window = new AsylumWindow();
                    break;
                case Game.City:
                    window = new CityWindow();
                    break;
                case Game.Origins:
                    window = new OriginsWindow();
                    break;
                case Game.Knight:
                    window = new KnightWindow();
                    break;
                default:
                    return false;
            }

            window.Activate();
            window.Show();
            return true;
        }

        private void AsylumButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenWindowForGame(Game.Asylum))
            {
                Close();
            }
        }

        private void CityButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenWindowForGame(Game.City))
            {
                Close();
            }
        }

        private void OriginsButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenWindowForGame(Game.Origins))
            {
                Close();
            }
        }

        private void KnightButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenWindowForGame(Game.Knight))
            {
                Close();
            }
        }
    }
}