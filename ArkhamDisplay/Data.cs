using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArkhamDisplay{
	public enum Game{
		None = -1,
		Asylum = 0,
		City = 1,
		Origins = 2,
		Knight = 3
	}

	public class DataBlock{
		public volatile Game selectedGame = Game.None;
		public volatile string[] saveLocations = { "", "", "", "" };
		public volatile int[] saveIDs = { 0, 0, 0, 0 };

		public volatile bool showPercent = true;
		public volatile bool showRiddleCount = true;
		public volatile int refreshRateInMS = 1000;

		public volatile bool cityBreakablesAtEnd = false;
		public volatile bool cityCatwoman = false;
		public volatile bool knightFirstEnding = false;
		public volatile bool knightNGPlus = false;
		public volatile bool knight120 = false;
	}

	public class Data{
		private static readonly string saveFileName = "settings.json";
		private static DataBlock data = new DataBlock();
		private static volatile Dictionary<string, Route> routes = new Dictionary<string, Route>();

		public static Game SelectedGame { get { return data.selectedGame; } set { data.selectedGame = value; } }
		public static string[] SaveLocations { get { return data.saveLocations; } }
		public static int[] SaveIDs { get { return data.saveIDs; } }

		public static bool ShowPercent { get { return data.showPercent; } set { data.showPercent = value; } }
		public static bool ShowRiddleCount { get { return data.showRiddleCount; } set { data.showRiddleCount = value; } }
		public static int RefreshRate { get { return data.refreshRateInMS; } set { data.refreshRateInMS = value; } }

		public static bool CityBreakablesAtEnd { get { return data.cityBreakablesAtEnd; } set { data.cityBreakablesAtEnd = value; } }
		public static bool CityCatwoman { get { return data.cityCatwoman; } set { data.cityCatwoman = value; } }
		public static bool KnightFirstEnding { get { return data.knightFirstEnding; } set { data.knightFirstEnding = value; } }
		public static bool KnightNGPlus { get { return data.knightNGPlus; } set { data.knightNGPlus = value; } }
		public static bool Knight120 { get { return data.knight120; } set { data.knight120 = value; } }

		public static void Load(){
			if(!System.IO.File.Exists(saveFileName)){
				Save();
			}

			lock(data) { data = JsonConvert.DeserializeObject<DataBlock>(System.IO.File.ReadAllText(saveFileName)); }

			//Load Routes (TODO - Don't have the routes hardcoded like this)
			//TODO - Lazy initialize routes instead of always having all of them in memory
			routes.Add("AsylumDefault", new Route("Routes/Arkham Asylum 100% Route - Route.tsv"));

			routes.Add("CityDefault", new Route("Routes/Arkham City 100% Route - Route.tsv"));
			routes.Add("CityBreakablesAtBottom", new Route("Routes/Arkham City 100% Route - Route (Cameras and Balloons at Bottom).tsv"));
			routes.Add("CityPrisoners", new Route("Routes/Arkham City 100% Route - Political Prisoners.tsv", Route.RouteType.CityPrisoners));

			routes.Add("OriginsDefault", new Route("Routes/Arkham Origins 100% Route - Route.tsv", Route.RouteType.OriginsRoute));
			routes.Add("OriginsCrime", new Route("Routes/Arkham Origins 100% Route - CrimeInProgress.tsv", Route.RouteType.OriginsCrimes));

			routes.Add("KnightDefault", new Route("Routes/Arkham Knight 100% Route - Route.tsv", Route.RouteType.KnightRoute));
			routes.Add("KnightNG+", new Route("Routes/Arkham Knight 100% Route - NG+ Route.tsv", Route.RouteType.KnightRoute));
			routes.Add("KnightFirstEnding", new Route("Routes/Arkham Knight 100% Route - First Ending.tsv", Route.RouteType.KnightRoute));
		}

		public static void Save(){
			lock(data){ System.IO.File.WriteAllText(saveFileName, JsonConvert.SerializeObject(data, Formatting.Indented)); }
		}

		public static Route GetRoute(string name){
			if(!routes.ContainsKey(name)){
				return null;
			}

			return routes[name];
		}
	}
}