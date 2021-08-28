using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ArkhamDisplay{
	class Utils{
		public static string GetSHA1Hash(string filePath){
			if(!File.Exists(filePath)){
				return "";
			}

			return GetSHA1Hash(File.ReadAllBytes(filePath));
		}

		public static string GetSHA1Hash(List<string> data){
			string final = string.Concat(data);
			return GetSHA1Hash(Encoding.UTF8.GetBytes(final));
		}

		public static string GetSHA1Hash(byte[] data){
			using var cryptoProvider = new SHA1CryptoServiceProvider();
			return BitConverter.ToString(cryptoProvider.ComputeHash(data));
		}

		public static string ListToNewlinedString(List<string> list){
			string final = "";

			foreach(string s in list){
				final += "\n" + s;
			}

			return final;
		}
	}
}