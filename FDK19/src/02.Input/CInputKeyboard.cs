using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK.Input;

using SlimDXKey = SlimDXKeys.Key;

namespace FDK
{
	public class CInputKeyboard : IInputDevice, IDisposable
	{
		// コンストラクタ

		public CInputKeyboard()
		{
			this.eInputDeviceType = EInputDeviceType.Keyboard;
			this.GUID = "";
			this.ID = 0;

			for (int i = 0; i < this.bKeyState.Length; i++)
				this.bKeyState[i] = false;

			this.listInputEvents = new List<STInputEvent>();
			this.listtmpInputEvents = new List<STInputEvent>();
		}

		// メソッド

		#region [ IInputDevice 実装 ]
		//-----------------
		public EInputDeviceType eInputDeviceType { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> listInputEvents { get; private set; }

		public void tPolling(bool bIsWindowActive)
		{
			if (bIsWindowActive)
			{
				{
					//-----------------------------
					KeyboardState currentState = Keyboard.GetState();

					if (currentState.IsConnected)
					{
						for (int index = 0; index < Enum.GetNames(typeof(Key)).Length; index++)
						{
							if (currentState[(Key)index])
							{
								// #xxxxx: 2017.5.7: from: TKK (OpenTK.Input.Key) を SlimDX.DirectInput.Key に変換。
								var key = DeviceConstantConverter.TKKtoKey((Key)index);
								if (SlimDXKey.Unknown == key)
									continue;   // 未対応キーは無視。

								if (this.btmpKeyState[(int)key] == false)
								{
									if (key != SlimDXKey.Return || (btmpKeyState[(int)SlimDXKey.LeftAlt] == false && btmpKeyState[(int)SlimDXKey.RightAlt] == false))    // #23708 2016.3.19 yyagi
									{
										var ev = new STInputEvent()
										{
											nKey = (int)key,
											bPressed = true,
											bReleased = false,
											nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
										};
										this.listtmpInputEvents.Add(ev);

										this.btmpKeyState[(int)key] = true;
										this.btmpKeyPushDown[(int)key] = true;
									}
								}
							}
							else
							{
								// #xxxxx: 2017.5.7: from: TKK (OpenTK.Input.Key) を SlimDX.DirectInput.Key に変換。
								var key = DeviceConstantConverter.TKKtoKey((Key)index);
								if (SlimDXKey.Unknown == key)
									continue;   // 未対応キーは無視。

								if (this.btmpKeyState[(int)key] == true) // 前回は押されているのに今回は押されていない → 離された
								{
									var ev = new STInputEvent()
									{
										nKey = (int)key,
										bPressed = false,
										bReleased = true,
										nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
									};
									this.listtmpInputEvents.Add(ev);

									this.btmpKeyState[(int)key] = false;
									this.btmpKeyPullUp[(int)key] = true;
								}
							}
						}
					}
				}
				//-----------------------------
			}
		}
		public void tSwapEventList()
        {
			this.listInputEvents.Clear();
			for (int i = 0; i < 256; i++)
			{
				//Swap
				this.bKeyPullUp[i] = this.btmpKeyPullUp[i];
				this.bKeyPushDown[i] = this.btmpKeyPushDown[i];
				this.bKeyState[i] = this.btmpKeyState[i];

				//Clear
				this.btmpKeyPushDown[i] = false;
				this.btmpKeyPullUp[i] = false;
			}
			for (int i = 0; i < this.listtmpInputEvents.Count; i++)
            {
				this.listInputEvents.Add(this.listtmpInputEvents[i]);
			}
			this.listtmpInputEvents.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();
		}


		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bIsKeyPressed(int nKey)
		{
			return this.bKeyPushDown[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bIsKeyDown(int nKey)
		{
			return this.bKeyState[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bIsKeyReleased(int nKey)
		{
			return this.bKeyPullUp[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bIsKeyUp(int nKey)
		{
			return !this.bKeyState[nKey];
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
		private bool[] bKeyPullUp = new bool[256];
		private bool[] bKeyPushDown = new bool[256];
		private bool[] bKeyState = new bool[256];
		private bool[] btmpKeyPullUp = new bool[256];
		private bool[] btmpKeyPushDown = new bool[256];
		private bool[] btmpKeyState = new bool[256];
		private List<STInputEvent> listtmpInputEvents;
		//-----------------
		#endregion
	}
}
