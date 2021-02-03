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
			this.list入力イベント = new List<STInputEvent>(32);
			this.eInputDeviceType = EInputDeviceType.MidiIn;
			this.GUID = "";
			this.ID = (int)nID;
			this.strDeviceName = "";    // CInput管理で初期化する
		}

		// メソッド

		public unsafe void tメッセージからMIDI信号のみ受信(int dev, long time, IntPtr buffer, int length, IntPtr user)
		{
			Debug.Print(length.ToString());
			byte* buf = (byte*)buffer;
			int nMIDIevent = buf[0];
			int nPara1 = buf[1];
			int nPara2 = buf[2];

			if ((nMIDIevent == 0x90) && (nPara2 != 0))      // Note ON
			{
				STInputEvent item = new STInputEvent();
				item.nKey = nPara1;
				item.b押された = true;
				item.nTimeStamp = time;
				this.listEventBuffer.Add(item);
			}
		}

		#region [ IInputDevice 実装 ]
		//-----------------
		public EInputDeviceType eInputDeviceType { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> list入力イベント { get; private set; }
		public string strDeviceName { get; set; }

		public void tポーリング(bool bWindowがアクティブ中, bool bバッファ入力有効)
		{
			// this.list入力イベント = new List<STInputEvent>( 32 );
			this.list入力イベント.Clear();                                // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();

			for (int i = 0; i < this.listEventBuffer.Count; i++)
				this.list入力イベント.Add(this.listEventBuffer[i]);

			this.listEventBuffer.Clear();
		}
		public bool bキーが押された(int nKey)
		{
			foreach (STInputEvent event2 in this.list入力イベント)
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
			if (this.list入力イベント != null)
			{
				this.list入力イベント = null;
			}
		}
		//-----------------
		#endregion
	}
}
