using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkhamDisplay{
	public class SaveParser{
		private static readonly string DECOMPRESSED_FILE_PREFIX = "batmancompressor/decompressed.sgd";
		protected string m_filePath;
		protected int m_id;
		protected string m_rawData;

		public SaveParser(string filePath, int id){
			if(string.IsNullOrWhiteSpace(filePath)){
				throw new NullReferenceException("Invalid file name!");
			}

			if(!System.IO.Directory.Exists(filePath)){
				throw new System.IO.FileNotFoundException();
			}

			if(id < 0 || id > 3){
				throw new ArgumentOutOfRangeException();
			}

			m_filePath = filePath;
			m_id = id;
			m_rawData = string.Empty;
		}

		protected void Decompress(){
			var files = System.IO.Directory.GetFiles("batmancompressor");
			foreach(string file in files){
				if(file.Contains("decompressed.sgd")){
					System.IO.File.Delete(file);
				}
			}

			System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
			pProcess.StartInfo.Arguments = "-d \"" + GetFile() + "\" " + DECOMPRESSED_FILE_PREFIX;
			pProcess.StartInfo.UseShellExecute = false;
			pProcess.StartInfo.RedirectStandardOutput = true;
			pProcess.StartInfo.FileName = "batmancompressor/batman.exe";
			pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
			pProcess.Start();
			_ = pProcess.StandardOutput.ReadToEnd(); //The output result
			pProcess.WaitForExit();
		}

		public virtual void Update(){
			Decompress();

			StringBuilder builder = new StringBuilder();

			int i = 1;
			string fileName = DECOMPRESSED_FILE_PREFIX + i.ToString();
			while(System.IO.File.Exists(fileName)){
				builder.Append(System.IO.File.ReadAllText(fileName));
				i++;
				fileName = DECOMPRESSED_FILE_PREFIX + i.ToString();
			}

			lock(m_rawData) m_rawData = builder.ToString();
		}

		public virtual bool HasKey(Entry entry, int requiredMatches){
			return HasKey(entry.id, requiredMatches) || HasKey(entry.alternateID, requiredMatches);
		}

		public virtual bool HasKey(string key, int requiredMatches){
			if(string.IsNullOrWhiteSpace(key)){
				return false;
			}

			return HasKeyCustomRegex(@"\b" + key + @"\b", requiredMatches);
		}

		public virtual bool HasKeyCustomRegex(string regex, int requiredMatches){
			if(string.IsNullOrWhiteSpace(regex)){
				return false;
			}

			Regex rx = new Regex(regex);
			MatchCollection collectibleMatches = rx.Matches(m_rawData);
			return collectibleMatches.Count >= requiredMatches;
		}

		public virtual string GetMatch(string regex){
			Regex rx = new Regex(regex);
			Match match = rx.Match(m_rawData);
			return match.ToString();
		}

		protected virtual string GetFile(){
			return System.IO.Path.Combine(m_filePath, "Save" + m_id.ToString() + ".sgd");
		}

		public int GetID(){
			return m_id;
		}
	}
}