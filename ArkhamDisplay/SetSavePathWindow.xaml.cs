namespace ArkhamDisplay;

public partial class SetSavePathWindow : Window
{
    private readonly Game selectedGame;
    private string savedPath = "";
    private int? savedID;

    public SetSavePathWindow(Game game)
    {
        InitializeComponent();

        selectedGame = game;
        if (!string.IsNullOrWhiteSpace(Data.SaveLocations[(int)game]))
        {
            savedPath = Data.SaveLocations[(int)game];
            SavePathBox.Text = savedPath;
        }

        switch (selectedGame)
        {
            case Game.Asylum:
                Title = "Arkham Asylum - Set Save File Path";
                break;
            case Game.City:
                Title = "Arkham City - Set Save File Path";
                break;
            case Game.Origins:
                Title = "Arkham Origins - Set Save File Path";
                break;
            case Game.Knight:
                Title = "Arkham Knight - Set Save File Path";
                break;
            case Game.None:
                throw new ArgumentException("Selected Game was somehow invalid!");
        }
    }

    private void GetDataFromFile(string fullPath)
    {
        savedPath = fullPath;
        if (Path.HasExtension(fullPath))
        {
            savedPath = Path.GetDirectoryName(fullPath);
            string id = Path.GetFileNameWithoutExtension(fullPath);
            if (id.EndsWith('0'))
            {
                savedID = 0;
            }
            else if (id.EndsWith('1'))
            {
                savedID = 1;
            }
            else if (id.EndsWith('2'))
            {
                savedID = 2;
            }
            else if (id.EndsWith('3'))
            {
                savedID = 3;
            }
        }
    }

    private void FileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Multiselect = false,
            Title = "Select a folder"
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            GetDataFromFile(openFolderDialog.FolderName);
            SavePathBox.Text = savedPath;
        }
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SavePathBox.Text))
        {
            MessageBox.Show("Error - File path cannot be blank");
            return;
        }

        if (!Directory.Exists(SavePathBox.Text) && !File.Exists(SavePathBox.Text))
        {
            MessageBox.Show("Error - File path must be a valid path. Make sure you've typed it correctly, and then try again.");
            return;
        }

        GetDataFromFile(SavePathBox.Text);
        Data.SelectedGame = selectedGame;
        if (!string.IsNullOrWhiteSpace(savedPath))
        {
            Data.SaveLocations[(int)selectedGame] = savedPath;
        }
        else
        {
            Data.SaveLocations[(int)selectedGame] = SavePathBox.Text;
        }

        if (savedID.HasValue)
        {
            Data.SaveIDs[(int)selectedGame] = savedID.Value;
        }

        Data.Save();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}