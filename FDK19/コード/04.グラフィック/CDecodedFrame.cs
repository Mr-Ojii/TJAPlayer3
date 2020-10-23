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
		public double Time;
		public byte[] Bitmap;

		public void Dispose() 
		{
			//未実装
			//仕様変更を行う際に必要となるため、今のうちに形だけ実装してしまう。
		}
	}
}
