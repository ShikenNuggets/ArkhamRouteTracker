using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ArkhamDisplay
{
    public partial class CityWindow : BaseWindow
    {
        private const int SECONDARY_COL_WIDTH = 250;

        public CityWindow() : base(Game.City)
        {
            InitializeComponent();
            PostInitialize();
        }

        protected override void SetCurrentRoute()
        {
            currentRoute = "CityDefault";
            currentSecondaryRoute = "CityPrisoners";

            if (Data.CityNGPlus && Data.CityCatwoman)
            {
                currentRoute = "CityNG+Catwoman";
            }
            else if (Data.CityCatwoman)
            {
                currentRoute = "CityCatwoman";
            }
            else if (Data.CityNGPlus)
            {
                currentRoute = "CityNG+";
            }
        }

        protected override List<Entry> GetEntriesForDisplay(Route route)
        {
            if (!Data.CityBreakablesAtEnd)
            {
                return base.GetEntriesForDisplay(route);
            }
            else
            {
                return route.GetEntriesWithPlaceholdersMoved();
            }
        }

        protected override void UpdateUI()
        {
            BreakablesAtBottomMenuItem.IsChecked = Data.CityBreakablesAtEnd;
            CatwomanMenuItem.IsChecked = Data.CityCatwoman;
            base.UpdateUI();
        }

        protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null)
        {
            Data.CityBreakablesAtEnd = BreakablesAtBottomMenuItem.IsChecked;
            Data.CityCatwoman = CatwomanMenuItem.IsChecked;
            Data.CityNGPlus = NGPlusMenuItem.IsChecked;

            if (NGPlusMenuItem.IsChecked)
            {
                minRequiredMatches = 2;
            }
            else
            {
                minRequiredMatches = 1;
            }

            base.UpdatePreferences(sender, e);
        }

        protected override void UpdateSecondaryRouteWindow()
        {
            PrisonerGrid.Children.Clear();
            PrisonerGrid.RowDefinitions.Clear();
            PrisonerGrid.ColumnDefinitions.Clear();

            PrisonerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });

            int lineCount = 1;
            int savedCount = 0;
            foreach (var entry in Data.GetRoute(currentSecondaryRoute).entries)
            {
                bool isSaved = saveParser.HasKey(entry.id, minRequiredMatches);

                if (!isSaved)
                {
                    PrisonerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(ROW_HEIGHT / 2) });

                    var rectangle = new Rectangle
                    {
                        Style = FindResource("GridRectangle") as Style
                    };
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetRow(rectangle, lineCount - 1);
                    PrisonerGrid.Children.Add(rectangle);

                    var txt0 = new TextBlock
                    {
                        Text = entry.name,
                        Style = FindResource("EntryText") as Style
                    };
                    Grid.SetColumn(txt0, 0);
                    Grid.SetRow(txt0, lineCount - 1);
                    PrisonerGrid.Children.Add(txt0);
                    lineCount++;
                }
                else
                {
                    savedCount++;
                }
            }
            SavedPrisoners.Text = savedCount + "/15 saved";
        }

        protected override string GetRiddleCount() => saveParser.GetMatch(@"\b\d*\/400|\d*\/440\b");

        protected override void SetStatsWindowStats()
        {
            if (statsWindow != null && progressCounter != null && riddleCounter != null && SavedPrisoners != null)
            {
                statsWindow.SetStats(progressCounter.Text, riddleCounter.Text + " riddles", SavedPrisoners.Text);
            }
        }
    }
}