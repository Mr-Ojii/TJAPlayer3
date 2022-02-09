using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Buffers;
using System.IO;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SDL2;

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

			unsafe
			{
				SDL.SDL_GetWindowSize(device.window, out int width, out int height);
				SDL.SDL_Surface* sshot = (SDL.SDL_Surface*)SDL.SDL_CreateRGBSurface(0, width, height, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);
				SDL.SDL_Rect rect = new SDL.SDL_Rect()
				{
					x = 0,
					y = 0,
					w = sshot->w,
					h = sshot->h,
				};
				SDL.SDL_RenderReadPixels(device.renderer, ref rect, SDL.SDL_PIXELFORMAT_ARGB8888, sshot->pixels, sshot->pitch);
				SDL.SDL_SaveBMP((IntPtr)sshot, strFullPath);
				SDL.SDL_FreeSurface((IntPtr)sshot);
			}

			return true;
		}
    }
}
