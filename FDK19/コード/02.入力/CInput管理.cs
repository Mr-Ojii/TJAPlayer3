using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FDK
{
	public class CInput管理 : IDisposable
	{
		// プロパティ

		public List<IInputDevice> list入力デバイス
		{
			get;
			private set;
		}
		public IInputDevice Keyboard
		{
			get
			{
				if (this._Keyboard != null)
				{
					return this._Keyboard;
				}
				foreach (IInputDevice device in this.list入力デバイス)
				{
					if (device.e入力デバイス種別 == E入力デバイス種別.Keyboard)
					{
						this._Keyboard = device;
						return device;
					}
				}
				return null;
			}
		}
		public IInputDevice Mouse
		{
			get
			{
				if (this._Mouse != null)
				{
					return this._Mouse;
				}
				foreach (IInputDevice device in this.list入力デバイス)
				{
					if (device.e入力デバイス種別 == E入力デバイス種別.Mouse)
					{
						this._Mouse = device;
						return device;
					}
				}
				return null;
			}
		}


		// コンストラクタ
		public CInput管理(IntPtr hWnd)
		{
			// this.timer = new CTimer( CTimer.E種別.MultiMedia );

			this.list入力デバイス = new List<IInputDevice>(10);
			#region [ Enumerate keyboard/mouse: exception is masked if keyboard/mouse is not connected ]
			CInputKeyboard cinputkeyboard = null;
			CInputMouse cinputmouse = null;
			try
			{
				cinputkeyboard = new CInputKeyboard();
				cinputmouse = new CInputMouse();
			}
			catch
			{
			}
			if (cinputkeyboard != null)
			{
				this.list入力デバイス.Add(cinputkeyboard);
			}
			if (cinputmouse != null)
			{
				this.list入力デバイス.Add(cinputmouse);
			}
			#endregion
			#region [ Enumerate joypad ]
			try
			{
				for (int joynum = 0; joynum < 8; joynum++)//2020.06.28 Mr-Ojii joystickの検出数を返す関数が見つからないので適当に8個で
				{
					if (OpenTK.Input.Joystick.GetState(joynum).IsConnected)
						this.list入力デバイス.Add(new CInputJoystick(joynum));
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.ToString());
			}
			#endregion
			this.proc = new CWin32.MidiInProc(this.MidiInCallback);
			uint nMidiDevices = CWin32.midiInGetNumDevs();
			Trace.TraceInformation("MIDI入力デバイス数: {0}", nMidiDevices);
			for (uint i = 0; i < nMidiDevices; i++)
			{
				CInputMIDI item = new CInputMIDI(i);
				CWin32.MIDIINCAPS lpMidiInCaps = new CWin32.MIDIINCAPS();
				uint num3 = CWin32.midiInGetDevCaps(i, ref lpMidiInCaps, (uint)Marshal.SizeOf(lpMidiInCaps));
				if (num3 != 0)
				{
					Trace.TraceError("MIDI In: Device{0}: midiInDevCaps(): {1:X2}: ", i, num3);
				}
				else
				{
					uint ret = CWin32.midiInOpen(ref item.hMidiIn, i, this.proc, IntPtr.Zero, 0x30000);
					Trace.TraceInformation("midiInOpen()==" + ret);
					Trace.TraceInformation("item.hMidiIn==" + item.hMidiIn.ToString());
					if ((ret == 0) && (item.hMidiIn != IntPtr.Zero))
					{
						CWin32.midiInStart(item.hMidiIn);
						Trace.TraceInformation("MIDI In: [{0}] \"{1}\" の入力受付を開始しました。", i, lpMidiInCaps.szPname);
						item.strDeviceName = lpMidiInCaps.szPname;
						this.list入力デバイス.Add(item);
						continue;
					}
				}
				Trace.TraceError("MIDI In: [{0}] \"{1}\" の入力受付の開始に失敗しました。", i, lpMidiInCaps.szPname);
			}
			
		}


		// メソッド

		public IInputDevice Joystick(int ID)
		{
			foreach (IInputDevice device in this.list入力デバイス)
			{
				if ((device.e入力デバイス種別 == E入力デバイス種別.Joystick) && (device.ID == ID))
				{
					return device;
				}
			}
			return null;
		}
		public IInputDevice Joystick(string GUID)
		{
			foreach (IInputDevice device in this.list入力デバイス)
			{
				if ((device.e入力デバイス種別 == E入力デバイス種別.Joystick) && device.GUID.Equals(GUID))
				{
					return device;
				}
			}
			return null;
		}
		public IInputDevice MidiIn(int ID)
		{
			foreach (IInputDevice device in this.list入力デバイス)
			{
				if ((device.e入力デバイス種別 == E入力デバイス種別.MidiIn) && (device.ID == ID))
				{
					return device;
				}
			}
			return null;
		}
		public void tポーリング(bool bWindowがアクティブ中)
		{
			lock (this.objMidiIn排他用)
			{
				//				foreach( IInputDevice device in this.list入力デバイス )
				for (int i = this.list入力デバイス.Count - 1; i >= 0; i--)    // #24016 2011.1.6 yyagi: change not to use "foreach" to avoid InvalidOperation exception by Remove().
				{
					IInputDevice device = this.list入力デバイス[i];
					try
					{
						device.tポーリング(bWindowがアクティブ中);
					}
					catch (Exception e)                                      // #24016 2011.1.6 yyagi: catch exception for unplugging USB joystick, and remove the device object from the polling items.
					{
						this.list入力デバイス.Remove(device);
						device.Dispose();
						Trace.TraceError("tポーリング時に例外発生。該当deviceをポーリング対象からRemoveしました。");
						Trace.TraceError(e.ToString());
					}
				}
			}
		}

		#region [ IDisposable＋α ]
		//-----------------
		public void Dispose()
		{
			this.Dispose(true);
		}
		public void Dispose(bool disposeManagedObjects)
		{
			if (!this.bDisposed済み)
			{
				if (disposeManagedObjects)
				{
					foreach (IInputDevice device in this.list入力デバイス)
					{
						CInputMIDI tmidi = device as CInputMIDI;
						if (tmidi != null)
						{
							CWin32.midiInStop(tmidi.hMidiIn);
							CWin32.midiInReset(tmidi.hMidiIn);
							CWin32.midiInClose(tmidi.hMidiIn);
							Trace.TraceInformation("MIDI In: [{0}] を停止しました。", new object[] { tmidi.ID });
						}
					}
					foreach (IInputDevice device2 in this.list入力デバイス)
					{
						device2.Dispose();
					}
					lock (this.objMidiIn排他用)
					{
						this.list入力デバイス.Clear();
					}

					//if( this.timer != null )
					//{
					//    this.timer.Dispose();
					//    this.timer = null;
					//}
				}
				this.bDisposed済み = true;
			}
		}
		~CInput管理()
		{
			this.Dispose(false);
			GC.KeepAlive(this);
		}
		//-----------------
		#endregion


		// その他

		#region [ private ]
		//-----------------
		private IInputDevice _Keyboard;
		private IInputDevice _Mouse;
		private bool bDisposed済み;
		private List<uint> listHMIDIIN = new List<uint>(8);
		private object objMidiIn排他用 = new object();
		private CWin32.MidiInProc proc;
		//		private CTimer timer;

		private void MidiInCallback(IntPtr hMidiIn, uint wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
		{
			int p = (int) dwParam1 & 0xF0;
			if (wMsg != CWin32.MIM_DATA || (p != 0x80 && p != 0x90 && p != 0xB0))
				return;

			long time = CSound管理.rc演奏用タイマ.nシステム時刻;  // lock前に取得。演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。

			lock (this.objMidiIn排他用)
			{
				if ((this.list入力デバイス != null) && (this.list入力デバイス.Count != 0))
				{
					foreach (IInputDevice device in this.list入力デバイス)
					{
						CInputMIDI tmidi = device as CInputMIDI;
						if ((tmidi != null) && (tmidi.hMidiIn == hMidiIn))
						{
							tmidi.tメッセージからMIDI信号のみ受信(wMsg, dwInstance, dwParam1, dwParam2, time);
							break;
						}
					}
				}
			}
		}
		//-----------------
		#endregion
	}
}
