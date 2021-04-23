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
			this.eInputDeviceType = EInputDeviceType.Mouse;
			this.GUID = "";
			this.ID = 0;

			for (int i = 0; i < this.bMouseState.Length; i++)
				this.bMouseState[i] = false;
			this.listInputEvents = new List<STInputEvent>(32);
		}

		// メソッド

		#region [ IInputDevice 実装 ]
		//-----------------
		public EInputDeviceType eInputDeviceType { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> listInputEvents { get; private set; }

		public void tPolling(bool bIsWindowActive, bool bEnableBufferInput)
		{
			for (int i = 0; i < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length; i++)
			{
				this.bMousePushDown[i] = false;
				this.bMousePullUp[i] = false;
			}

			if (bIsWindowActive)
			{

				this.listInputEvents.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();
				
				//-----------------------------
				MouseState currentState = Mouse.GetState();

				if (currentState.IsConnected)
				{
					for (int j = 0; j < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length; j++)
					{
						if (this.bMouseState[j] == false && currentState[(MouseButton)j] == true)
						{
							var ev = new STInputEvent()
							{
								nKey = j,
								bPressed = true,
								bReleased = false,
								nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
							};
							this.listInputEvents.Add(ev);

							this.bMouseState[j] = true;
							this.bMousePushDown[j] = true;
						}
						else if (this.bMouseState[j] == true && currentState[(MouseButton)j] == false)
						{
							var ev = new STInputEvent()
							{
								nKey = j,
								bPressed = false,
								bReleased = true,
								nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
							};
							this.listInputEvents.Add(ev);

							this.bMouseState[j] = false;
							this.bMousePullUp[j] = true;
						}
					}
				}
				//-----------------------------
				
			}
		}
		public bool bIsKeyPressed(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length)) && this.bMousePushDown[nButton]);
		}
		public bool bIsKeyDown(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length)) && this.bMouseState[nButton]);
		}
		public bool bIsKeyReleased(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length)) && this.bMousePullUp[nButton]);
		}
		public bool bIsKeyUp(int nButton)
		{
			return (((0 <= nButton) && (nButton < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length)) && !this.bMouseState[nButton]);
		}
		//-----------------
		#endregion

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDisposed)
			{
				if (this.listInputEvents != null)
				{
					this.listInputEvents = null;
				}
				this.bDisposed = true;
			}
		}
		//-----------------
		#endregion


		// その他

		#region [ private ]
		//-----------------
		private bool bDisposed;
		private bool[] bMousePullUp = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
		private bool[] bMousePushDown = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
		private bool[] bMouseState = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
		//-----------------
		#endregion
	}
}
