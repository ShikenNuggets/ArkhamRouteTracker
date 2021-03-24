using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkhamDisplay{
	public class SaveParser{
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

		protected string Decompress(){
			string finalResult = "";
			if(!File.Exists(GetFile())){
				return finalResult;
			}

			var buffer = File.ReadAllBytes(GetFile());

			var offsets = ExtractSaveOffsets(buffer);

			var reader = new BinaryReader(new MemoryStream(buffer));

			for(int i = 0; i < offsets.Length; i++){
				reader.BaseStream.Seek(offsets[i], SeekOrigin.Begin);
				//- Header: 0x9e2a83c1 (2653586369)
				var header = Get32(reader);
				var unknown = Get32(reader); // + 4
				var compressedSize = Get32(reader); // + 8
				var decompressedSize = Get32(reader); // + 12

				reader.BaseStream.Position += 8;

				var compressedBuffer = new byte[compressedSize];
				reader.Read(compressedBuffer, 0, (int)compressedSize);
				//var compressedBuffer = reader.ReadBytes((int)compressedSize);

				byte[] decompressed = new byte[decompressedSize];
				MiniLZO.MiniLZO.Decompress(compressedBuffer, decompressed);

				finalResult += Encoding.ASCII.GetString(decompressed);
			}

			return finalResult;
		}

		private static uint Get32(BinaryReader reader){
			var pos = reader.BaseStream.Position;

			var a1 = reader.ReadUInt32();
			/*var a2 = */
			reader.ReadUInt32();
			var a3 = reader.ReadUInt32();
			var a4 = reader.ReadUInt32();

			// get32
			var val = a3 << 16 | a1 | a4 << 24;

			// myswap32
			var ret = ((val & 0xFF00) << 8) | ((val & 0xFF0000) >> 8) | (val >> 24) | (val << 24);

			reader.BaseStream.Position = pos + 4;

			return ret;
		}

		private static int[] ExtractSaveOffsets(byte[] buffer){
			var offsets = new List<int>();

			for(int i = 0; i < buffer.Length - 4; i++){
				if(buffer[i] != 0x9E)
					continue;

				if(buffer[i + 1] != 0x2A)
					continue;

				if(buffer[i + 2] != 0x83)
					continue;

				if(buffer[i + 3] != 0xC1)
					continue;

				offsets.Add(i);
			}

			return offsets.ToArray();
		}

		public virtual void Update(){
			lock(m_rawData) m_rawData = Decompress();
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