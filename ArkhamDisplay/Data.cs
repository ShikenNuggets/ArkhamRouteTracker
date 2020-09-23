using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ArkhamDisplay{
	public enum Theme{
		Default = 0,
		Light = 1,
		Dark = 2
	}

	public enum Game{
		None = -1,
		Asylum = 0,
		City = 1,
		Origins = 2,
		Knight = 3
	}

	public enum DisplayType{
		All = 0,
		SortDoneToTop = 1,
		HideDone = 2
	}

	public class DataBlock{
		public volatile Theme savedTheme = Theme.Default;
		public volatile Game selectedGame = Game.None;
		public volatile string[] saveLocations = { "", "", "", "" };
		public volatile int[] saveIDs = { 0, 0, 0, 0 };
		public volatile Rect[] windowRects = { new Rect(), new Rect(), new Rect(), new Rect() };
		public volatile float mainRowHeight = 7.0f;
		public volatile float secondaryRowHeight = 3.0f;

		public volatile bool showPercent = true;
		public volatile bool showRiddleCount = true;
		public volatile int refreshRateInMS = 1000;
		public volatile DisplayType displayType = DisplayType.All;

		public volatile bool cityBreakablesAtEnd = false;
		public volatile bool cityCatwoman = false;
		public volatile bool knightFirstEnding = false;
		public volatile bool knightNGPlus = false;
		public volatile bool knight120 = false;
		public volatile bool knight240 = false;
		public volatile bool knightMoF = false;
	}

	public class Data{
		private static readonly string saveFileName = "settings.json";
		private static readonly string routeFileName = "Routes/routes.json";
		private static volatile DataBlock data = new DataBlock();
		private static volatile Dictionary<string, string> routeFiles = new Dictionary<string, string>();
		private static volatile Dictionary<string, Route> routes = new Dictionary<string, Route>();

		public static Game SelectedGame { get { return data.selectedGame; } set { data.selectedGame = value; } }
		public static string[] SaveLocations { get { return data.saveLocations; } }
		public static int[] SaveIDs { get { return data.saveIDs; } }
		public static Rect[] WindowRect { get{ return data.windowRects; } }
		public static double MainRowHeight { get { return (double)data.mainRowHeight; } set { data.mainRowHeight = (float)value; } }
		public static double SecondaryRowHeight { get { return (double)data.secondaryRowHeight; } set { data.secondaryRowHeight = (float)value; } }

		public static bool ShowPercent { get { return data.showPercent; } set { data.showPercent = value; } }
		public static bool ShowRiddleCount { get { return data.showRiddleCount; } set { data.showRiddleCount = value; } }
		public static int RefreshRate { get { return data.refreshRateInMS; } set { data.refreshRateInMS = value; } }
		public static DisplayType DisplayType { get { return data.displayType; } set { data.displayType = value; } }

		public static bool CityBreakablesAtEnd { get { return data.cityBreakablesAtEnd; } set { data.cityBreakablesAtEnd = value; } }
		public static bool CityCatwoman { get { return data.cityCatwoman; } set { data.cityCatwoman = value; } }
		public static bool KnightFirstEnding { get { return data.knightFirstEnding; } set { data.knightFirstEnding = value; } }
		public static bool KnightNGPlus { get { return data.knightNGPlus; } set { data.knightNGPlus = value; } }
		public static bool Knight120 { get { return data.knight120; } set { data.knight120 = value; } }

		public static bool Knight240 { get { return data.knight240; } set { data.knight240 = value; } }
		public static bool KnightMoF {  get { return data.knightMoF;  } set { data.knightMoF = value; } }

		public static void Load(){
			if(!System.IO.File.Exists(saveFileName)){
				Save();
			}

			lock(data){
				data = JsonConvert.DeserializeObject<DataBlock>(System.IO.File.ReadAllText(saveFileName));
			}

			lock(routeFiles){
				routeFiles = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(routeFileName));
			}
		}

		public static void Save(){
			lock(data){ System.IO.File.WriteAllText(saveFileName, JsonConvert.SerializeObject(data, Formatting.Indented)); }
		}

		public static Route GetRoute(string name){
			if(!routes.ContainsKey(name)){
				if(!routeFiles.ContainsKey(name)){
					return null;
				}

				routes.Add(name, new Route(routeFiles[name]));
			}

			return routes[name];
		}

		public static void SetTheme(Theme theme){
			if(data.savedTheme == Theme.Default && UseTheme() == theme){
				return;
			}else{
				data.savedTheme = theme;
			}
		}

		public static Theme UseTheme(){
			if(data.savedTheme != Theme.Default){
				return data.savedTheme;
			}

			try{
				const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
				const string RegistryValueName = "AppsUseLightTheme";

				using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
				object registryValueObject = key?.GetValue(RegistryValueName);
				if(registryValueObject != null){
					int registryValue = (int)registryValueObject;
					return registryValue > 0 ? Theme.Light : Theme.Dark;
				}
			}catch(Exception){}

			return Theme.Dark;
		}
	}
}