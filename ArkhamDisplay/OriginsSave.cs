using System;

namespace ArkhamDisplay{
	public class OriginsSave : SaveParser{
		public OriginsSave(string filePath, int id) : base(filePath, id){}

		public override bool HasKey(Entry entry){
			if(entry.type.Equals("Data Handler")){
				return !HasKey(entry.id) && !HasKey(entry.alternateID);
			}else if (entry.metadata.Equals("DarkKnightFinish")){
				return HasKeyCustomRegex(@"\b" + entry.id + @"............" + Convert.ToChar(Convert.ToByte("0x1", 16)));
			}

			return base.HasKey(entry);
		}
	}
}