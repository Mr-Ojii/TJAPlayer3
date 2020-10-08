using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
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
			this.e入力デバイス種別 = E入力デバイス種別.Keyboard;
			this.GUID = "";
			this.ID = 0;

			for (int i = 0; i < this.bKeyState.Length; i++)
				this.bKeyState[i] = false;

			this.list入力イベント = new List<STInputEvent>(32);
			this.listEventBuffer = new List<STInputEvent>(32);
		}

		public void Key押された受信(Keys Code)
		{
			var key = DeviceConstantConverter.KeysToKey(Code);
			if (SlimDXKey.Unknown == key)
				return;   // 未対応キーは無視。

			if (this.bKeyStateForBuff[(int)key] == false)
			{
				STInputEvent item = new STInputEvent()
				{
					nKey = (int)key,
					b押された = true,
					b離された = false,
					nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻ms,
				};
				this.listEventBuffer.Add(item);

				this.bKeyStateForBuff[(int)key] = true;
			}
		}
		public void Key離された受信(Keys Code)
		{
			var key = DeviceConstantConverter.KeysToKey(Code);
			if (SlimDXKey.Unknown == key)
				return;   // 未対応キーは無視。

			if (this.bKeyStateForBuff[(int)key] == true)
			{
				STInputEvent item = new STInputEvent()
				{
					nKey = (int)key,
					b押された = false,
					b離された = true,
					nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻ms,
				};

				this.listEventBuffer.Add(item);
				this.bKeyStateForBuff[(int)key] = false;
			}
		}

		// メソッド

		#region [ IInputDevice 実装 ]
		//-----------------
		public E入力デバイス種別 e入力デバイス種別 { get; private set; }
		public string GUID { get; private set; }
		public int ID { get; private set; }
		public List<STInputEvent> list入力イベント { get; private set; }

		public void tポーリング(bool bWindowがアクティブ中, bool bバッファ入力有効)
		{
			for (int i = 0; i < 256; i++)
			{
				this.bKeyPushDown[i] = false;
				this.bKeyPullUp[i] = false;
			}

			if (bWindowがアクティブ中)
			{
				if (bバッファ入力有効)
				{
					this.list入力イベント.Clear();

					for (int i = 0; i < this.listEventBuffer.Count; i++)
					{
						if (this.listEventBuffer[i].b押された)
						{
							this.bKeyState[this.listEventBuffer[i].nKey] = true;
							this.bKeyPushDown[this.listEventBuffer[i].nKey] = true;
						}
						else if(this.listEventBuffer[i].b離された)
						{
							this.bKeyState[this.listEventBuffer[i].nKey] = false;
							this.bKeyPullUp[this.listEventBuffer[i].nKey] = true;
						}
						this.list入力イベント.Add(this.listEventBuffer[i]);
					}

					this.listEventBuffer.Clear();
				}
				else
				{
					this.list入力イベント.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();

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
											b押された = true,
											b離された = false,
											nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
										};
										this.list入力イベント.Add(ev);

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
										b押された = false,
										b離された = true,
										nTimeStamp = CSound管理.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
									};
									this.list入力イベント.Add(ev);

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
		public bool bキーが押された(int nKey)
		{
			return this.bKeyPushDown[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bキーが押されている(int nKey)
		{
			return this.bKeyState[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bキーが離された(int nKey)
		{
			return this.bKeyPullUp[nKey];
		}

		/// <param name="nKey">
		///		調べる SlimDX.DirectInput.Key を int にキャストした値。
		/// </param>
		public bool bキーが離されている(int nKey)
		{
			return !this.bKeyState[nKey];
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
		private bool[] bKeyPullUp = new bool[256];
		private bool[] bKeyPushDown = new bool[256];
		private bool[] bKeyState = new bool[256];
		private bool[] bKeyStateForBuff = new bool[256];
		//-----------------
		#endregion
	}
}
