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

		public List<STInputEvent> listEventBuffer;

		public CInputKeyboard()
		{
			this.eInputDeviceType = EInputDeviceType.Keyboard;
			this.GUID = "";
			this.ID = 0;

			for (int i = 0; i < this.bKeyState.Length; i++)
				this.bKeyState[i] = false;

			this.listInputEvents = new List<STInputEvent>(32);
			this.listEventBuffer = new List<STInputEvent>(32);
		}

		public void Key押された受信(Key Code)
		{
			var key = DeviceConstantConverter.TKKtoKey(Code);
			if (SlimDXKey.Unknown == key)
				return;   // 未対応キーは無視。

			if (this.bKeyStateForBuff[(int)key] == false)
			{
				STInputEvent item = new STInputEvent()
				{
					nKey = (int)key,
					bPressed = true,
					bReleased = false,
					nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms,
				};
				this.listEventBuffer.Add(item);

				this.bKeyStateForBuff[(int)key] = true;
			}
		}
		public void Key離された受信(Key Code)
		{
			var key = DeviceConstantConverter.TKKtoKey(Code);
			if (SlimDXKey.Unknown == key)
				return;   // 未対応キーは無視。

			if (this.bKeyStateForBuff[(int)key] == true)
			{
				STInputEvent item = new STInputEvent()
				{
					nKey = (int)key,
					bPressed = false,
					bReleased = true,
					nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms,
				};

				this.listEventBuffer.Add(item);
				this.bKeyStateForBuff[(int)key] = false;
			}
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
			for (int i = 0; i < 256; i++)
			{
				this.bKeyPushDown[i] = false;
				this.bKeyPullUp[i] = false;
			}

			if (bIsWindowActive)
			{
				if (bEnableBufferInput)
				{
					this.listInputEvents.Clear();

					for (int i = 0; i < this.listEventBuffer.Count; i++)
					{
						if (this.listEventBuffer[i].bPressed)
						{
							this.bKeyState[this.listEventBuffer[i].nKey] = true;
							this.bKeyPushDown[this.listEventBuffer[i].nKey] = true;
						}
						else if(this.listEventBuffer[i].bReleased)
						{
							this.bKeyState[this.listEventBuffer[i].nKey] = false;
							this.bKeyPullUp[this.listEventBuffer[i].nKey] = true;
						}
						this.listInputEvents.Add(this.listEventBuffer[i]);
					}

					this.listEventBuffer.Clear();
				}
				else
				{
					this.listInputEvents.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();

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

								if (this.bKeyState[(int)key] == false)
								{
									if (key != SlimDXKey.Return || (bKeyState[(int)SlimDXKey.LeftAlt] == false && bKeyState[(int)SlimDXKey.RightAlt] == false))    // #23708 2016.3.19 yyagi
									{
										var ev = new STInputEvent()
										{
											nKey = (int)key,
											bPressed = true,
											bReleased = false,
											nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
										};
										this.listInputEvents.Add(ev);

										this.bKeyState[(int)key] = true;
										this.bKeyPushDown[(int)key] = true;
									}
								}
							}
							{
								// #xxxxx: 2017.5.7: from: TKK (OpenTK.Input.Key) を SlimDX.DirectInput.Key に変換。
								var key = DeviceConstantConverter.TKKtoKey((Key)index);
								if (SlimDXKey.Unknown == key)
									continue;   // 未対応キーは無視。

								if (this.bKeyState[(int)key] == true && !currentState.IsKeyDown((Key)index)) // 前回は押されているのに今回は押されていない → 離された
								{
									var ev = new STInputEvent()
									{
										nKey = (int)key,
										bPressed = false,
										bReleased = true,
										nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
									};
									this.listInputEvents.Add(ev);

									this.bKeyState[(int)key] = false;
									this.bKeyPullUp[(int)key] = true;
								}
							}
						}
					}
				}
				//-----------------------------
			}
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
		private bool[] bKeyStateForBuff = new bool[256];
		//-----------------
		#endregion
	}
}
