using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Platform;

namespace FDK
{
    public class CSaveScreen
    {
		/// <summary>
		/// TJAPlayer3.csより
		/// この関数はFDK側にあるべきだと思ったので。
		/// 
		/// デバイス画像のキャプチャと保存。
		/// </summary>
		/// <param name="device">デバイス</param>
		/// <param name="strFullPath">保存するファイル名(フルパス)</param>
		/// <returns></returns>
		public static bool CSaveFromDevice(Device device, string strFullPath)
		{
			string strSavePath = Path.GetDirectoryName(strFullPath);
			if (!Directory.Exists(strSavePath))
			{
				try
				{
					Directory.CreateDirectory(strSavePath);
				}
				catch (Exception e)
				{
					Trace.TraceError(e.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (0bfe6bff-2a56-4df4-9333-2df26d9b765b)");
					return false;
				}
			}

			//https://stackoverflow.com/questions/8606253/saving-a-bitmap-of-opentk-screen-but-the-quickfont-text-doesnt-show-up
			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextException();

			Bitmap bmp = new Bitmap(GameWindowSize.Width, GameWindowSize.Height);
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, GameWindowSize.Width, GameWindowSize.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, GameWindowSize.Width, GameWindowSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
			bmp.Save(strFullPath, ImageFormat.Png);
			bmp.Dispose();
			return true;
		}
    }
}
