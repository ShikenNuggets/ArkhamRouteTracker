using Microsoft.Win32;
using Newtonsoft.Json;

namespace ArkhamDisplay;

public enum Theme
{
    Default = 0,
    Light = 1,
    Dark = 2
}

public enum Game
{
    None = -1,
    Asylum = 0,
    City = 1,
    Origins = 2,
    Knight = 3
}

public enum DisplayType
{
    All = 0,
    SortDoneToTop = 1,
    HideDone = 2
}

public class DataBlock
{
    public volatile Theme savedTheme = Theme.Default;
    public volatile Game selectedGame = Game.None;
    public volatile string[] saveLocations = ["", "", "", ""];
    public volatile int[] saveIDs = [0, 0, 0, 0];
    public volatile Rect[] windowRects = [new Rect(), new Rect(), new Rect(), new Rect()];
    public Rect statsWindowRect;
    public volatile float mainRowHeight = 7.0f;
    public volatile float secondaryRowHeight = 3.0f;
    public volatile bool statsWindowOpen;

    public volatile bool showPercent = true;
    public volatile bool showRiddleCount = true;
    public volatile int refreshRateInMS = 1000;
    public volatile DisplayType displayType = DisplayType.All;

    public volatile bool asylumGlitchless;
    public volatile bool cityBreakablesAtEnd;
    public volatile bool cityCatwoman;
    public volatile bool cityNGPlus;
    public volatile bool knightFirstEnding;
    public volatile bool knightNGPlus;
    public volatile bool knight120;
    public volatile bool knight240;
    public volatile bool knightMoF;
}

public static class Data
{
    private const string saveFileName = "settings.json";
    private const string routeFileName = "Routes/routes.json";
    private static volatile DataBlock data = new DataBlock();
    private static volatile Dictionary<string, string> routeFiles = [];
    private static readonly Dictionary<string, Route> routes = [];

    public static string RoutePath => "\\Routes\\";
    public static Game SelectedGame { get => data.selectedGame; set => data.selectedGame = value; }
    public static string[] SaveLocations => data.saveLocations;
    public static int[] SaveIDs => data.saveIDs;
    public static Rect[] WindowRect => data.windowRects;
    public static Rect StatsWindowRect { get => data.statsWindowRect; set => data.statsWindowRect = value; }
    public static double MainRowHeight { get => data.mainRowHeight; set => data.mainRowHeight = (float)value; }
    public static double SecondaryRowHeight { get => data.secondaryRowHeight; set => data.secondaryRowHeight = (float)value; }
    public static bool StatsWindowOpen { get => data.statsWindowOpen; set => data.statsWindowOpen = value; }

    public static bool ShowPercent { get => data.showPercent; set => data.showPercent = value; }
    public static bool ShowRiddleCount { get => data.showRiddleCount; set => data.showRiddleCount = value; }
    public static int RefreshRate { get => data.refreshRateInMS; set => data.refreshRateInMS = value; }
    public static DisplayType DisplayType { get => data.displayType; set => data.displayType = value; }

    public static bool AsylumGlitchless { get => data.asylumGlitchless; set => data.asylumGlitchless = value; }
    public static bool CityBreakablesAtEnd { get => data.cityBreakablesAtEnd; set => data.cityBreakablesAtEnd = value; }
    public static bool CityCatwoman { get => data.cityCatwoman; set => data.cityCatwoman = value; }
    public static bool CityNGPlus { get => data.cityNGPlus; set => data.cityNGPlus = value; }
    public static bool KnightFirstEnding { get => data.knightFirstEnding; set => data.knightFirstEnding = value; }
    public static bool KnightNGPlus { get => data.knightNGPlus; set => data.knightNGPlus = value; }

    public static bool Knight240 { get => data.knight240; set => data.knight240 = value; }
    public static bool KnightMoF { get => data.knightMoF; set => data.knightMoF = value; }

    public static void Load()
    {
        if (!File.Exists(saveFileName))
        {
            Save();
        }

        lock (data)
        {
            data = JsonConvert.DeserializeObject<DataBlock>(File.ReadAllText(saveFileName));
        }

        if (!File.Exists(routeFileName))
        {
            MessageBox.Show("Error: " + routeFileName + " not found - please re-download the application");
            throw new FileNotFoundException("routes.json file not found", routeFileName);
        }

        lock (routeFiles)
        {
            routeFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(routeFileName));
        }
    }

    public static void Save()
    {
        lock (data) { File.WriteAllText(saveFileName, JsonConvert.SerializeObject(data, Formatting.Indented)); }
    }

    public static Route GetRoute(string name)
    {
        if (!routes.ContainsKey(name))
        {
            if (!routeFiles.ContainsKey(name))
            {
                return null;
            }

            routes.Add(name, new Route(routeFiles[name]));
        }

        return routes[name];
    }

    public static bool HasRouteFile(string fileName)
    {
        if (fileName == Path.GetFileName(routeFileName))
        {
            return true;
        }

        return routeFiles.ContainsValue(fileName) || routeFiles.ContainsValue("Routes/" + fileName);
    }

    public static void ReloadRoutes()
    {
        lock (routes)
        {
            routes.Clear(); //Routes will be reloaded later when used
        }

        lock (routeFiles)
        {
            routeFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(routeFileName));
        }
    }

    public static void SetTheme(Theme theme)
    {
        if (data.savedTheme == Theme.Default && UseTheme() == theme)
        {
            return;
        }
        else
        {
            data.savedTheme = theme;
        }
    }

    public static Theme UseTheme()
    {
        if (data.savedTheme != Theme.Default)
        {
            return data.savedTheme;
        }

        try
        {
            const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string RegistryValueName = "AppsUseLightTheme";

            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            object registryValueObject = key?.GetValue(RegistryValueName);
            if (registryValueObject != null)
            {
                int registryValue = (int)registryValueObject;
                return registryValue > 0 ? Theme.Light : Theme.Dark;
            }
        }
        catch (Exception) { }

        return Theme.Dark;
    }
}