/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using SharpDX;
using SharpDX.Direct3D9;


namespace FDK
{
	/// <summary>
	/// Manages aspects of the graphics device unique to Direct3D9.
	/// </summary>
	public class Direct3D9Manager
	{
		GraphicsDeviceManager manager;

		/// <summary>
		/// Gets the graphics device.
		/// </summary>
		/// <value>The graphics device.</value>
		public Device Device
		{
			get;
			internal set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Direct3D9Manager"/> class.
		/// </summary>
		/// <param name="manager">The parent manager.</param>
		internal Direct3D9Manager(GraphicsDeviceManager manager)
		{
			this.manager = manager;
		}

		/// <summary>
		/// Creates a render target surface that is compatible with the current device settings.
		/// </summary>
		/// <param name="width">The width of the surface.</param>
		/// <param name="height">The height of the surface.</param>
		/// <returns>The newly created render target surface.</returns>
		public Texture CreateRenderTarget(int width, int height)
		{
			return new Texture(Device, width, height, 1, Usage.RenderTarget, manager.CurrentSettings.BackBufferFormat, Pool.Default);
		}

		/// <summary>
		/// Creates a resolve target for capturing the back buffer.
		/// </summary>
		/// <returns>The newly created resolve target.</returns>
		public Texture CreateResolveTarget()
		{
			return new Texture(Device, manager.ScreenWidth, manager.ScreenHeight, 1, Usage.RenderTarget, manager.CurrentSettings.BackBufferFormat, Pool.Default);
		}

		/// <summary>
		/// Resolves the current back buffer into a texture.
		/// </summary>
		/// <param name="target">The target texture.</param>
		/// <exception cref="InvalidOperationException">Thrown when the resolve process fails.</exception>
		public void ResolveBackBuffer(Texture target)
		{
			ResolveBackBuffer(target, 0);
		}

		/// <summary>
		/// Resolves the current back buffer into a texture.
		/// </summary>
		/// <param name="target">The target texture.</param>
		/// <param name="backBufferIndex">The index of the back buffer.</param>
		/// <exception cref="InvalidOperationException">Thrown when the resolve process fails.</exception>
		public void ResolveBackBuffer(Texture target, int backBufferIndex)
		{
			// disable exceptions for this method
			//bool storedThrow = Configuration.ThrowOnError;
			//Configuration.ThrowOnError = false;
			Surface destination = null;

			try
			{
				// grab the current back buffer
				Surface backBuffer = Device.GetBackBuffer(0, backBufferIndex);
				if (backBuffer == null || Result.GetResultFromWin32Error(Marshal.GetLastWin32Error()).Failure)
					throw new InvalidOperationException("Could not obtain back buffer surface.");

				// grab the destination surface
				destination = target.GetSurfaceLevel(0);
				if (destination == null || Result.GetResultFromWin32Error(Marshal.GetLastWin32Error()).Failure)
					throw new InvalidOperationException("Could not obtain resolve target surface.");

				// first try to copy using linear filtering
				Device.StretchRectangle(backBuffer, destination, TextureFilter.Linear);
				if (Result.GetResultFromWin32Error(Marshal.GetLastWin32Error()).Failure)
				{
					// that failed, so try with no filtering
					Device.StretchRectangle(backBuffer, destination, TextureFilter.None);
					if (Result.GetResultFromWin32Error(Marshal.GetLastWin32Error()).Failure)
					{
						// that failed as well, so the last thing we can try is a load surface call
						Surface.FromSurface(destination, backBuffer, Filter.Default, 0);
						if (Result.GetResultFromWin32Error(Marshal.GetLastWin32Error()).Failure)
							throw new InvalidOperationException("Could not copy surfaces.");
					}
				}
			}
			finally
			{
				if (destination != null)
					destination.Dispose();
				//Configuration.ThrowOnError = storedThrow;
			}
		}

		/// <summary>
		/// Resets the render target.
		/// </summary>
		public void ResetRenderTarget()
		{
			Surface backBuffer = Device.GetBackBuffer(0, 0);

			try
			{
				Device.SetRenderTarget(0, backBuffer);
			}
			finally
			{
				backBuffer.Dispose();
			}
		}
	}
}
