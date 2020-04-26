using System;

namespace ArkhamDisplay{
	public class OriginsSave : SaveParser{
		public OriginsSave(string filePath, int id) : base(filePath, id){}

		public override bool HasKey(Entry entry){
			if("Data Handler".Equals(entry.type)){
				return !HasKey(entry.id) && !HasKey(entry.alternateID);
			}else if ("DarkKnightFinish".Equals(entry.metadata)){
				return HasKeyCustomRegex(@"\b" + entry.id + @"............" + Convert.ToChar(Convert.ToByte("0x1", 16)));
			}

			return base.HasKey(entry);
		}

		protected override string GetFile()
		{
			return System.IO.Path.Combine(m_filePath, "SpSave_v2_" + m_id.ToString() + ".sgd");
		}
	}
}