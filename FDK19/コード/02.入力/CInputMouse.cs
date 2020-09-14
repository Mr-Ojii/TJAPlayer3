using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK.Input;

namespace FDK
{
	public class CInputMouse : IInputDevice, IDisposable
	{
		// コンストラクタ

		public CInputMouse()
		{
			this.e入力デバイス種別 = E入力デバイス種別.Mouse;
			this.GUID = "";
			this.ID = 0;

			for (int i = 0; i < this.bMouseState.Length; i++)
				this.bMouseState[i] = false;

			//this.timer = new CTimer( CTimer.E種別.MultiMedia );
			this.list入力イベント = new List<STInputEvent>(32);
		}

		// メソッド

		#region [ IInputDevice 実装 ]
		//-----------------
		public E入力デバイス種別 e入力デバイス種別 { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> list入力イベント { get; private set; }

		public void tポーリング(bool bWindowがアクティブ中)
		{
			for (int i = 0; i < Enum.GetNames(typeof(MouseButton)).Length; i++)
			{
				this.bMousePushDown[i] = false;
				this.bMousePullUp[i] = false;
			}

			if (bWindowがアクティブ中)
			{

				this.list入力イベント.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();
				
				//-----------------------------
				MouseState currentState = Mouse.GetState();

				if (currentState.IsConnected)
				{
					for (int j = 0; j < Enum.GetNames(typeof(MouseButton)).Length; j++)
					{
						if (this.bMouseState[j] == false && currentState[(MouseButton)j] == true)
						{
							var ev = new STInputEvent()
							{
								nKey = j,
								b押された = true,
								b離された = false,
								nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
							};
							this.list入力イベント.Add(ev);

							this.bMouseState[j] = true;
							this.bMousePushDown[j] = true;
						}
						else if (this.bMouseState[j] == true && currentState[(MouseButton)j] == false)
						{
							var ev = new STInputEvent()
							{
								nKey = j,
								b押された = false,
								b離された = true,
								nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
							};
							this.list入力イベント.Add(ev);

							this.bMouseState[j] = false;
							this.bMousePullUp[j] = true;
						}
					}
				}
				//-----------------------------
				
			}
		}
		public bool bキーが押された(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(MouseButton)).Length)) && this.bMousePushDown[nButton]);
		}
		public bool bキーが押されている(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(MouseButton)).Length)) && this.bMouseState[nButton]);
		}
		public bool bキーが離された(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(MouseButton)).Length)) && this.bMousePullUp[nButton]);
		}
		public bool bキーが離されている(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(MouseButton)).Length)) && !this.bMouseState[nButton]);
		}
		//-----------------
		#endregion

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDispose完了済み)
			{
				if (this.list入力イベント != null)
				{
					this.list入力イベント = null;
				}
				this.bDispose完了済み = true;
			}
		}
		//-----------------
		#endregion


		// その他

		#region [ private ]
		//-----------------
		private bool bDispose完了済み;
		private bool[] bMousePullUp = new bool[Enum.GetNames(typeof(MouseButton)).Length];
		private bool[] bMousePushDown = new bool[Enum.GetNames(typeof(MouseButton)).Length];
		private bool[] bMouseState = new bool[Enum.GetNames(typeof(MouseButton)).Length];
		//-----------------
		#endregion
	}
}
