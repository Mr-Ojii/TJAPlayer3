using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace FDK
{
	public class CWin32
	{
		#region [ Win32 構造体 ]
		//-----------------
		[StructLayout(LayoutKind.Sequential)]
		public struct WAVEFORMATEX
		{
			public short wFormatTag;
			public ushort nChannels;
			public uint nSamplesPerSec;
			public uint nAvgBytesPerSec;
			public ushort nBlockAlign;
			public ushort wBitsPerSample;

			public WAVEFORMATEX(
				short _wFormatTag,
				ushort _nChannels,
				uint _nSamplesPerSec,
				uint _nAvgBytesPerSec,
				ushort _nBlockAlign,
				ushort _wBitsPerSample)
				: this()
			{
				wFormatTag = _wFormatTag;
				nChannels = _nChannels;
				nSamplesPerSec = _nSamplesPerSec;
				nAvgBytesPerSec = _nAvgBytesPerSec;
				nBlockAlign = _nBlockAlign;
				wBitsPerSample = _wBitsPerSample;
			}
		}
		//-----------------
		#endregion
	}
}
