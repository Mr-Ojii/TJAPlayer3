using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FDK
{
	public class CDecodedFrame : IDisposable
	{
		public unsafe CDecodedFrame(double time, AVFrame* frame, Size texsize)
		{
			this.Time = time;
			this.TexSize = texsize;

			this.TexPointer = Marshal.AllocHGlobal(frame->linesize[0] * frame->height);

			for (int y = 0; y < frame->height; y++)
			{
				Buffer.MemoryCopy(frame->data[0] + (frame->linesize[0] * frame->height - (frame->linesize[0] * (y + 1))), (byte*)(this.TexPointer + frame->linesize[0] * y), frame->linesize[0], frame->linesize[0]);
			}

		}

		public double Time 
		{
			get;
			private set;
		}
		public IntPtr TexPointer
		{
			get;
			private set;
		}
		public Size TexSize
		{
			get;
			private set;
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(this.TexPointer);
		}
	}
}
