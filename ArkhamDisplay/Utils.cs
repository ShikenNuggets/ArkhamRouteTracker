using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ArkhamDisplay{
	class Utils{
		public static string GetSHA1Hash(string filePath){
			if(!System.IO.File.Exists(filePath)){
				return "";
			}

			return GetSHA1Hash(System.IO.File.ReadAllBytes(filePath));
		}

		public static string GetSHA1Hash(byte[] data){
			using var cryptoProvider = new SHA1CryptoServiceProvider();
			return BitConverter.ToString(cryptoProvider.ComputeHash(data));
		}
	}
}