using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDK
{
	public class CDecodedFrame : IDisposable
	{
		public CDecodedFrame(double time, byte[] tex, Size texsize)
		{
			this.Time = time;
			this.Tex = tex;
			this.TexSize = texsize;
		}

		public bool Using
		{
			get;
			private set;
		}
		public double Time 
		{
			get;
			private set;
		}
		public byte[] Tex
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
			//GCヨロシク！
		}
	}
}
