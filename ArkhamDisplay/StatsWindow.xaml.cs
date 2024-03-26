namespace ArkhamDisplay;

public partial class StatsWindow : Window
{
    public bool isClosedByMainWindow;

    public StatsWindow()
    {
        InitializeComponent();

        var rect = Utils.DetermineFinalWindowRectPosition(Data.StatsWindowRect);
        Width = rect.Width;
        Height = rect.Height;
        Left = rect.X;
        Top = rect.Y;
    }

    protected override void OnClosed(EventArgs e)
    {
        Data.StatsWindowRect = new Rect(Left, Top, 750, 400); //Window size is hardcoded for now, TODO - actual scaling logic for this would be nice

        if (!isClosedByMainWindow)
        { //This feels stupid and gross but whatever it works
            Data.StatsWindowOpen = false;
        }

        Data.Save();

        base.OnClosed(e);
    }

    public void SetStats(string percent, string riddles, string extra = "")
    {
        PercentageLabel.Text = percent;
        RiddleLabel.Text = riddles;
        ExtraLabel.Text = extra;
    }
}