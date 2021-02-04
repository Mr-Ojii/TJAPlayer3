using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Buffers;
using System.IO;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FDK
{
    public class CSaveScreen
    {
		/// <summary>
		/// Capture screen and save as .png file
		/// </summary>
		/// <param name="device">Device</param>
		/// <param name="strFullPath">Filename(FullPath)</param>
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
					Trace.TraceError("An exception has occurred, but processing continues.");
					return false;
				}
			}

			using (IMemoryOwner<Rgb24> pixels = Configuration.Default.MemoryAllocator.Allocate<Rgb24>(GameWindowSize.Width * GameWindowSize.Height))
			{
				GL.ReadPixels(0, 0, GameWindowSize.Width, GameWindowSize.Height, PixelFormat.Rgb, PixelType.UnsignedByte, ref MemoryMarshal.GetReference(pixels.Memory.Span));
				using (Image<Rgb24> image = Image.LoadPixelData<Rgb24>(pixels.Memory.Span, GameWindowSize.Width, GameWindowSize.Height))
				{
					image.Mutate(con => con.Flip(FlipMode.Vertical));
					image.SaveAsPng(strFullPath);
				}
			}

			return true;
		}
    }
}
