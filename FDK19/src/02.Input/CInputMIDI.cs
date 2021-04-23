using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FDK
{
	public class CInputMIDI : IInputDevice, IDisposable
	{
		// プロパティ

		public IntPtr hMidiIn;
		public List<STInputEvent> listEventBuffer;

		// コンストラクタ

		public CInputMIDI(uint nID)
		{
			this.hMidiIn = IntPtr.Zero;
			this.listEventBuffer = new List<STInputEvent>(32);
			this.listInputEvents = new List<STInputEvent>(32);
			this.eInputDeviceType = EInputDeviceType.MidiIn;
			this.GUID = "";
			this.ID = (int)nID;
			this.strDeviceName = "";    // CInputManagerで初期化する
		}

		// メソッド

		public unsafe void tメッセージからMIDI信号のみ受信(int dev, long time, byte[] buf, int count) 
		{
			if (this.ID == dev)
			{
				int nMIDIevent = buf[count * 3];
				int nPara1 = buf[count * 3 + 1];
				int nPara2 = buf[count * 3 + 2];

				if ((nMIDIevent == 0x90) && (nPara2 != 0))      // Note ON
				{
					STInputEvent item = new STInputEvent();
					item.nKey = nPara1;
					item.b押された = true;
					item.nTimeStamp = time;
					this.listEventBuffer.Add(item);
				}
			}
		}

		#region [ IInputDevice 実装 ]
		//-----------------
		public EInputDeviceType eInputDeviceType { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> listInputEvents { get; private set; }
		public string strDeviceName { get; set; }

		public void tPolling(bool bIsWindowActive, bool bEnableBufferInput)
		{
			// this.listInputEvents = new List<STInputEvent>( 32 );
			this.listInputEvents.Clear();                                // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();

			for (int i = 0; i < this.listEventBuffer.Count; i++)
				this.listInputEvents.Add(this.listEventBuffer[i]);

			this.listEventBuffer.Clear();
		}
		public bool bキーが押された(int nKey)
		{
			foreach (STInputEvent event2 in this.listInputEvents)
			{
				if ((event2.nKey == nKey) && event2.b押された)
				{
					return true;
				}
			}
			return false;
		}
		public bool bキーが押されている(int nKey)
		{
			return false;
		}
		public bool bキーが離された(int nKey)
		{
			return false;
		}
		public bool bキーが離されている(int nKey)
		{
			return false;
		}
		//-----------------
		#endregion

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (this.listEventBuffer != null)
			{
				this.listEventBuffer = null;
			}
			if (this.listInputEvents != null)
			{
				this.listInputEvents = null;
			}
		}
		//-----------------
		#endregion
	}
}
