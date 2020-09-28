using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
using System.IO;
using System.Diagnostics;

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

			// http://www.gamedev.net/topic/594369-dx9slimdxati-incorrect-saving-surface-to-file/
			using (Surface pSurface = device.GetRenderTarget(0))
			{
				Surface.ToFile(pSurface, strFullPath, ImageFileFormat.Png);
			}
			return true;
		}
    }
}
