using System.Windows;

namespace ArkhamDisplay
{
    public partial class AsylumWindow : BaseWindow
    {
        public AsylumWindow() : base(Game.Asylum)
        {
            InitializeComponent();
            PostInitialize();
        }

        protected override void SetCurrentRoute()
        {
            currentRoute = "AsylumDefault";
            if (Data.AsylumGlitchless)
            {
                currentRoute = "AsylumGlitchless";
            }
        }

        protected override string GetRiddleCount() => saveParser.GetMatch(@"\b\d*\/240\b");

        protected override void UpdateUI()
        {
            GlitchlessMenuItem.IsChecked = Data.AsylumGlitchless;
            base.UpdateUI();
        }

        protected override void UpdatePreferences(object sender = null, RoutedEventArgs e = null)
        {
            Data.AsylumGlitchless = GlitchlessMenuItem.IsChecked;
            base.UpdatePreferences(sender, e);
        }
    }
}