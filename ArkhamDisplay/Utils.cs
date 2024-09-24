﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

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
			return BitConverter.ToString(SHA1.HashData(data));
		}

		public static string ListToNewlinedString(List<string> list){
			string final = "";

			foreach(string s in list){
				final += "\n" + s;
			}

			return final;
		}

		public static Rect DetermineFinalWindowRectPosition(Rect originalRect, double minWidth = 1, double minHeight = 1){
            var screenLeft = SystemParameters.VirtualScreenLeft;
            var screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
            var screenTop = SystemParameters.VirtualScreenTop;
            var screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

            if(originalRect.Width < minWidth || originalRect.Height < minHeight){
                //Input is not valid, return default rect with min sizes
                return new Rect(0.0, 0.0, minWidth, minHeight);
            }

			Rect finalRect;

            finalRect.Width = originalRect.Width;
            finalRect.Height = originalRect.Height;

            if(originalRect.X < screenLeft){
                finalRect.X = screenLeft;
            }else if(originalRect.X + originalRect.Width > screenRight){
                finalRect.X = screenRight - originalRect.Width;
            }else{
				finalRect.X = originalRect.X;
			}

            if(originalRect.Y < screenTop){
                finalRect.Y = screenTop;
            }else if (originalRect.Y + originalRect.Height > screenBottom){
                finalRect.Y = screenBottom - originalRect.Height;
            }else{ 
				finalRect.Y = originalRect.Y;
			}

			return finalRect;
        }

		public static string AppendTimestampToFileName(string fileName){
			if(fileName == null){
				return null;
			}

			string ext = Path.GetExtension(fileName);
			if(ext == null){
				return fileName;
			}

			string result = Path.GetFileNameWithoutExtension(fileName);
			if(result == null){
				return fileName;
			}

			return result + "_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ext;
		}
	}
}