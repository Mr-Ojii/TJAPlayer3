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

			using (IMemoryOwner<Rgba32> pixels = Configuration.Default.MemoryAllocator.Allocate<Rgba32>(Game.Instance.ClientSize.Width * Game.Instance.ClientSize.Height)) 
			{
				GL.ReadPixels(0, 0, Game.Instance.ClientSize.Width, Game.Instance.ClientSize.Height, PixelFormat.Rgba, PixelType.UnsignedByte, ref MemoryMarshal.GetReference(pixels.Memory.Span));
				Image<Rgba32> image = Image.LoadPixelData<Rgba32>(pixels.Memory.Span, Game.Instance.ClientSize.Width, Game.Instance.ClientSize.Height);
				Task.Factory.StartNew(() =>
				{
					image.Mutate(con => con.Flip(FlipMode.Vertical));
					image.SaveAsPng(strFullPath);
					image.Dispose();
				}
					);
			}

			return true;
		}
    }
}
