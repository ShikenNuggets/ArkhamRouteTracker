using System.Windows;

namespace ArkhamDisplay
{
    public partial class KnightWindow : BaseWindow
    {
        public KnightWindow() : base(Game.Knight)
        {
            InitializeComponent();
            PostInitialize();
        }

        protected override SaveParser CreateSaveParser()
        {
            return new KnightSave(Data.SaveLocations[(int)game], Data.SaveIDs[(int)game]);
        }

        protected override string GetEntryName(Entry entry)
        {
            if (TwoFortyPercentMenuItem.IsChecked && "NG+".Equals(entry.metadata))
            {
                return entry.name + " (" + entry.metadata + ")";
            }
            return base.GetEntryName(entry);
        }

        protected override void SetCurrentRoute()
        {
            if (Data.KnightFirstEnding)
            {
                currentRoute = "KnightFirstEnding";
            }
            else if (Data.KnightNGPlus)
            {
                currentRoute = "KnightNG+";
            }
            else if (Data.Knight240)
            {
                currentRoute = "Knight240";
            }
            else if (Data.KnightMoF)
            {
                currentRoute = "KnightMoF";
            }
            else
            {
                currentRoute = "KnightDefault";
            }
        }

        protected override void UpdateUI()
        {
            FirstEndingMenuItem.IsChecked = Data.KnightFirstEnding;
            NGPlusMenuItem.IsChecked = Data.KnightNGPlus;
            TwoFortyPercentMenuItem.IsChecked = Data.Knight240;
            MatterOfFamilyMenuItem.IsChecked = Data.KnightMoF;
            base.UpdateUI();
        }

        protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null)
        {
            //Some settings are incompatible, so if one switches one make sure the other does too
            if (sender == FirstEndingMenuItem && FirstEndingMenuItem.IsChecked)
            {
                TwoFortyPercentMenuItem.IsChecked = false;
                MatterOfFamilyMenuItem.IsChecked = false;
            }
            else if (sender == TwoFortyPercentMenuItem && TwoFortyPercentMenuItem.IsChecked)
            {
                NGPlusMenuItem.IsChecked = false;
                FirstEndingMenuItem.IsChecked = false;
                MatterOfFamilyMenuItem.IsChecked = false;
            }
            else if (sender == MatterOfFamilyMenuItem && MatterOfFamilyMenuItem.IsChecked)
            {
                NGPlusMenuItem.IsChecked = false;
                FirstEndingMenuItem.IsChecked = false;
                TwoFortyPercentMenuItem.IsChecked = false;
            }
            else if (sender == NGPlusMenuItem && NGPlusMenuItem.IsChecked)
            {
                TwoFortyPercentMenuItem.IsChecked = false;
                MatterOfFamilyMenuItem.IsChecked = false;
            }

            // TODO: We can probably get rid of this
            if (NGPlusMenuItem.IsChecked)
            {
                minRequiredMatches = 2;
            }
            else
            {
                minRequiredMatches = 1;
            }

            Data.KnightFirstEnding = FirstEndingMenuItem.IsChecked;
            Data.KnightNGPlus = NGPlusMenuItem.IsChecked;
            Data.Knight240 = TwoFortyPercentMenuItem.IsChecked;
            Data.KnightMoF = MatterOfFamilyMenuItem.IsChecked;

            base.UpdatePreferences(sender, e);
        }

        protected override void UpdatePercent(int doneEntries, int totalEntries)
        {
            double percentDone = 100.0 * doneEntries / totalEntries;
            if (TwoFortyPercentMenuItem.IsChecked)
            {
                // TODO: This is just a hack. Ideally, we'd know the number of rows that are NG+
                // and scale that to be 120%, while the remaining would scale to 120%. However,
                // that's too much work for me to bother.
                int newGameEntries = 532;
                if (doneEntries <= newGameEntries)
                {
                    // The number of newGame entries should be equal to 119%
                    percentDone = 119.0 * doneEntries / newGameEntries;
                }
                else
                {
                    // The remaining entries (totalEntries - newGameEntries) should be equal to 121%
                    percentDone = 119.0 + (doneEntries - newGameEntries) * 121 / (totalEntries - newGameEntries);
                }
            }
            progressCounter.Text = string.Format("{0:0.0}", percentDone) + "%";
            riddleCounter.Text = GetRiddleCount();
        }

        protected override string GetRiddleCount()
        {
            return saveParser.GetLastMatch(@"\b\d*\/243\b");
        }
    }
}