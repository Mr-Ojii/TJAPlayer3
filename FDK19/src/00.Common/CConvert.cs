using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FDK
{
	public static class CConvert
	{
		// プロパティ

		public static readonly string str16進数文字 = "0123456789ABCDEFabcdef";
		public static readonly string str36進数文字 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		

		// メソッド

		public static bool bONorOFF( char c )
		{
			return ( c != '0' );
		}

		public static double DegreeToRadian( double angle )
		{
			return ( ( Math.PI * angle ) / 180.0 );
		}
		public static double RadianToDegree( double angle )
		{
			return ( angle * 180.0 / Math.PI );
		}
		public static float DegreeToRadian( float angle )
		{
			return (float) DegreeToRadian( (double) angle );
		}
		public static float RadianToDegree( float angle )
		{
			return (float) RadianToDegree( (double) angle );
		}

		/// <summary>
		/// 百分率数値を255段階数値に変換するメソッド。透明度用。
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static int nParsentTo255(double num)
		{
			return (int)(255.0 * num);
		}

		/// <summary>
		/// 255段階数値を百分率に変換するメソッド。
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static int n255ToParsent(int num)
		{
			return (int)(100.0 / num);
		}

		//参考:https://gist.github.com/vurdalakov/00d9471356da94454b372843067af24e
		public static Image<Rgba32> ToImageSharpImage(System.Drawing.Bitmap bitmap)
		{
			using (var memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

				memoryStream.Seek(0, SeekOrigin.Begin);

				return SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream);
			}
		}
	} 
}
