using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace FDK
{
	public class CWin32
	{
		#region [ Win32 定数 ]
		//-----------------

		#region [ MIDIメッセージ ]
		public const uint MIM_CLOSE = 0x3c2;
		public const uint MIM_DATA = 0x3c3;
		public const uint MIM_ERROR = 0x3c5;
		public const uint MIM_LONGDATA = 0x3c4;
		public const uint MIM_LONGERROR = 0x3c6;
		public const uint MIM_OPEN = 0x3c1;
		#endregion
		
		//-----------------
		#endregion

		#region [ Win32 関数 ]
		//-----------------
		[DllImport("winmm.dll")]
		public static extern uint midiInClose(IntPtr hMidiIn);
		[DllImport("winmm.dll")]
		public static extern uint midiInGetDevCaps(uint uDeviceID, ref MIDIINCAPS lpMidiInCaps, uint cbMidiInCaps);
		[DllImport("winmm.dll")]
		public static extern uint midiInGetID(IntPtr hMidiIn, ref IntPtr puDeviceID);
		[DllImport("winmm.dll")]
		public static extern uint midiInGetNumDevs();
		[DllImport("winmm.dll")]
		public static extern uint midiInOpen(ref IntPtr phMidiIn, uint uDeviceID, MidiInProc dwCallback, IntPtr dwInstance, int fdwOpen);
		[DllImport("winmm.dll")]
		public static extern uint midiInReset(IntPtr hMidiIn);
		[DllImport("winmm.dll")]
		public static extern uint midiInStart(IntPtr hMidiIn);
		[DllImport("winmm.dll")]
		public static extern uint midiInStop(IntPtr hMidiIn);
		//-----------------
		#endregion

		#region [ Win32 構造体 ]
		//-----------------
		[StructLayout( LayoutKind.Sequential )]
		public struct MIDIINCAPS
		{
			public ushort wMid;
			public ushort wPid;
			public uint vDriverVersion;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 0x20 )]
			public string szPname;
			public uint dwSupport;
		}

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

		// Win32 メッセージ処理デリゲート

		public delegate void MidiInProc(IntPtr hMidiIn, uint wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
	}
}
