using System;

namespace ArkhamDisplay{
	public class KnightSave : SaveParser{
		private const string SAVE_FILE_PREFIX = "BAK1Save";
		private const string SAVE_FILE_SUFFIX = ".sgd";

		public KnightSave(string filePath, int id) : base(filePath, id){}

		public override bool Update(){
			string file = GetFile();

			var lastWriteTime = System.IO.File.GetLastWriteTimeUtc(file);
			if(lastWriteTime == m_LastWriteTime){
				return false; //No need to update, file hasn't been written to since last check
			}

			m_LastWriteTime = lastWriteTime;

			if(!System.IO.File.Exists(file)){
				return false;
			}

			int numRetries = 0;
			while(m_rawData == null){
				try{
					m_rawData = System.IO.File.ReadAllText(file);
					m_rawData = m_rawData.Trim('\0');
				}catch(System.IO.IOException){
					numRetries++;
					if(numRetries >= 3){
						throw;
					}

					System.Threading.Thread.Sleep(100);
				}
			}

			return true;
		}

		public override bool HasKey(Entry entry, int requiredMatches)
		{
			requiredMatches = 1;
			if ("NG+".Equals(entry.metadata))
			{
				requiredMatches = 2;
				//Hush is a weird case, since it appears twice in NG until you finish Hush.
				if ("SS_Hush_WayneTower".Equals(entry.id) && !base.HasKey("SS_Hush_ShowdownStart", 1))
				{
					requiredMatches = 3;
				}
				// This is a special case in NG+ 100% and 240%, because you do NG+ riddler before NG riddler
				if ("Lockup_B1_Riddler_Lockedup".Equals(entry.id))
				{
					requiredMatches = 1;
				}
			}
			// This is a special case in 240%, where you do NG riddler last.
			if ("240".Equals(entry.metadata))
			{
				requiredMatches = 2;
			}
			return base.HasKey(entry, requiredMatches);
		}

		protected override string GetFile(){
			string filename0 = System.IO.Path.Combine(m_filePath, SAVE_FILE_PREFIX + m_id + "x0" + SAVE_FILE_SUFFIX);
			string filename1 = System.IO.Path.Combine(m_filePath, SAVE_FILE_PREFIX + m_id + "x1" + SAVE_FILE_SUFFIX);
			string filename2 = System.IO.Path.Combine(m_filePath, SAVE_FILE_PREFIX + m_id + "x2" + SAVE_FILE_SUFFIX);
			string filename3 = System.IO.Path.Combine(m_filePath, SAVE_FILE_PREFIX + m_id + "x3" + SAVE_FILE_SUFFIX);
			DateTime writetime0 = DateTime.MinValue;
			DateTime writetime1 = DateTime.MinValue;
			DateTime writetime2 = DateTime.MinValue;
			DateTime writetime3 = DateTime.MinValue;
			if(System.IO.File.Exists(filename0)){
				writetime0 = System.IO.File.GetLastWriteTimeUtc(filename0);
			}

			if(System.IO.File.Exists(filename1)){
				writetime1 = System.IO.File.GetLastWriteTimeUtc(filename1);
			}

			if(System.IO.File.Exists(filename2)){
				writetime2 = System.IO.File.GetLastWriteTimeUtc(filename2);
			}

			if(System.IO.File.Exists(filename3)){
				writetime3 = System.IO.File.GetLastWriteTimeUtc(filename3);
			}

			string currentfile = filename0;
			DateTime currentwritetime = writetime0;
			if(currentwritetime < writetime1){
				currentfile = filename1;
			}

			if(currentwritetime < writetime2){
				currentfile = filename2;
			}

			
			if(currentwritetime < writetime3){
				currentfile = filename3;
			}

			return currentfile;
		}
	}
}