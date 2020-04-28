using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkhamDisplay{
	public class Entry{
		public string name;
		public string type;
		public string id;
		public string alternateID;
		public string metadata;

		public Entry(string name, string type, string id, string alternateID = null, string metadata = null){
			this.name = name;
			this.type = type;
			this.id = id;
			this.alternateID = alternateID;
			this.metadata = metadata;
		}

		public bool IsType(string type_){
			if(string.IsNullOrWhiteSpace(type_)){
				return false;
			}

			return type_.Equals(type);
		}

		public static bool IsPlaceholder(Entry e){
			return e.IsType("[PLACEHOLDER]");
		}
	}

	public class Route{
		public enum RouteType{
			Default,
			CityPrisoners,
			OriginsRoute,
			OriginsCrimes,
			KnightRoute
		}

		private string fileName;
		private RouteType routeType;
		public List<Entry> entries;

		public Route(string file, RouteType type = RouteType.Default){
			fileName = file;
			routeType = type;
			entries = new List<Entry>();

			if(string.IsNullOrWhiteSpace(fileName)){
				throw new NullReferenceException();
			}

			if(!System.IO.File.Exists(fileName)){
				throw new System.IO.FileNotFoundException();
			}

			var allLines = System.IO.File.ReadAllLines(fileName).Skip(1);
			entries.Capacity = allLines.Count();
			foreach(string line in allLines){
				string[] lineComponents = line.Split('\t');

				switch(routeType){
					case RouteType.Default:
						entries.Add(SplitRoute(lineComponents));
						break;
					case RouteType.CityPrisoners:
						entries.Add(SplitCityPrisoners(lineComponents));
						break;
					case RouteType.OriginsRoute:
						entries.Add(SplitOriginsRoute(lineComponents));
						break;
					case RouteType.OriginsCrimes:
						entries.Add(SplitOriginsCrimes(lineComponents));
						break;
					case RouteType.KnightRoute:
						entries.Add(SplitKnightRoute(lineComponents));
						break;
				}
			}
		}

		public List<Entry> GetEntriesWithPlaceholdersMoved(){
			List<Entry> newEntries = new List<Entry>(entries);

			List<Entry> placeHolders = newEntries.FindAll(Entry.IsPlaceholder);
			foreach(Entry p in placeHolders){
				List<Entry> onesToMove = newEntries.FindAll(x => x.IsType(p.name));
				newEntries.RemoveAll(x => x.IsType(p.name));
				newEntries.InsertRange(newEntries.FindIndex(x => x == p), onesToMove);
			}

			newEntries.RemoveAll(Entry.IsPlaceholder);
			return newEntries;
		}

		public List<Entry> GetEntriesWithoutPlaceholders(){
			List<Entry> newEntries = new List<Entry>(entries);
			newEntries.RemoveAll(Entry.IsPlaceholder);
			return newEntries;
		}

		private Entry SplitRoute(string[] lineComponents){
			if(lineComponents.Length < 4){
				throw new ArgumentOutOfRangeException(); //TODO - This sucks
			}

			return new Entry(lineComponents[0].Trim(), lineComponents[1].Trim(), lineComponents[2].Trim(), lineComponents[3].Trim());
		}

		private Entry SplitKnightRoute(string[] lineComponents){
			if(lineComponents.Length < 4){
				throw new ArgumentOutOfRangeException(); //TODO - This sucks
			}

			return new Entry(lineComponents[0].Trim(), lineComponents[1].Trim(), lineComponents[2].Trim());
		}

		private Entry SplitCityPrisoners(string[] lineComponents){
			if(lineComponents.Length < 4){
				throw new ArgumentOutOfRangeException(); //TODO - This sucks
			}

			return new Entry(lineComponents[0].Trim(), null, lineComponents[3].Trim());
		}

		private Entry SplitOriginsRoute(string[] lineComponents){
			if(lineComponents.Length < 5){
				throw new ArgumentOutOfRangeException(); //TODO - This sucks
			}

			return new Entry(lineComponents[0].Trim(), lineComponents[1].Trim(), lineComponents[2].Trim(), lineComponents[3].Trim(), lineComponents[4].Trim());
		}

		private Entry SplitOriginsCrimes(string[] lineComponents){
			if(lineComponents.Length < 3){
				throw new ArgumentOutOfRangeException(); //TODO - This sucks
			}

			return new Entry(lineComponents[0].Trim(), lineComponents[1].Trim(), lineComponents[2].Trim());
		}
	}
}