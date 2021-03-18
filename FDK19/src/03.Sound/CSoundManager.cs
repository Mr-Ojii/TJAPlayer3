using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Un4seen.Bass;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;

namespace FDK
{
	#region [ DTXMania用拡張 ]
	public class CSoundManager  // : CSound
	{
		private static ISoundDevice SoundDevice
		{
			get; set;
		}
		private static ESoundDeviceType SoundDeviceType
		{
			get; set;
		}
		public static CSoundTimer rc演奏用タイマ = null;
		public static bool bUseOSTimer = false;     // OSのタイマーを使うか、CSoundTimerを使うか。DTXCではfalse, DTXManiaではtrue。
													// DTXCでCSoundTimerを使うと、内部で無音のループサウンドを再生するため
													// サウンドデバイスを占有してしまい、Viewerとして呼び出されるDTXManiaで、ASIOが使えなくなる。

		// DTXMania単体でこれをtrueにすると、WASAPI/ASIO時に演奏タイマーとしてFDKタイマーではなく
		// システムのタイマーを使うようになる。こうするとスクロールは滑らかになるが、音ズレが出るかもしれない。

		public static bool bIsTimeStretch = false;

		private static int _nMasterVolume;
		public int nMasterVolume
		{
			get
			{
				return _nMasterVolume;
			}
			set
			{
				SoundDevice.nMasterVolume = value;
				_nMasterVolume = value;
			}
		}

		///// <summary>
		///// BASS時、mp3をストリーミング再生せずに、デコードしたraw wavをオンメモリ再生する場合はtrueにする。
		///// 特殊なmp3を使用時はシークが乱れるので、必要に応じてtrueにすること。(Config.iniのNoMP3Streamingで設定可能。)
		///// ただし、trueにすると、その分再生開始までの時間が長くなる。
		///// </summary>
		//public static bool bIsMP3DecodeByWindowsCodec = false;

		public static int nMixing = 0;
		public static int nStreams = 0;
		#region [ WASAPI/ASIO/OpenAL設定値 ]
		/// <summary>
		/// <para>WASAPI 排他モード出力における再生遅延[ms]（の希望値）。最終的にはこの数値を基にドライバが決定する）。</para>
		/// <para>0以下の値を指定すると、この数値はWASAPI初期化時に自動設定する。正数を指定すると、その値を設定しようと試みる。</para>
		/// </summary>
		public static int SoundDelayExclusiveWASAPI = 0;        // SSTでは、50ms
		/// <summary>
		/// <para>WASAPI 共有モード出力における再生遅延[ms]。ユーザが決定する。</para>
		/// </summary>
		public static int SoundDelaySharedWASAPI = 100;
		/// <summary>
		/// <para>排他WASAPIバッファの更新間隔。出力間隔ではないので注意。</para>
		/// <para>→ 自動設定されるのでSoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
		/// </summary>
		public static int SoundUpdatePeriodExclusiveWASAPI = 6;
		/// <summary>
		/// <para>共有WASAPIバッファの更新間隔。出力間隔ではないので注意。</para>
		/// <para>SoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
		/// </summary>
		public static int SoundUpdatePeriodSharedWASAPI = 6;
		/// <summary>
		/// <para>WASAPI BASS出力における再生遅延[ms]。ユーザが決定する。</para>
		/// </summary>
		public static int SoundDelayBASS = 15;
		/// <para>BASSバッファの更新間隔。出力間隔ではないので注意。</para>
		/// <para>SoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
		/// </summary>
		public static int SoundUpdatePeriodBASS = 1;
		///// <summary>
		///// <para>ASIO 出力における再生遅延[ms]（の希望値）。最終的にはこの数値を基にドライバが決定する）。</para>
		///// </summary>
		//public static int SoundDelayASIO = 0;					// SSTでは50ms。0にすると、デバイスの設定値をそのまま使う。
		/// <summary>
		/// <para>ASIO 出力におけるバッファサイズ。</para>
		/// </summary>
		public static int SoundDelayASIO = 0;                       // 0にすると、デバイスの設定値をそのまま使う。
		public static int ASIODevice = 0;
		/// <summary>
		/// <para>OpenAL 出力における再生遅延[ms]。ユーザが決定する。</para>
		/// </summary>
		public static int SoundDelayOpenAL = 100;

		public long GetSoundDelay()
		{
			if (SoundDevice != null)
			{
				return SoundDevice.nBufferSizems;
			}
			else
			{
				return -1;
			}
		}

		#endregion


		/// <summary>
		/// DTXMania用コンストラクタ
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="soundDeviceType"></param>
		/// <param name="nSoundDelayExclusiveWASAPI"></param>
		/// <param name="nSoundDelayASIO"></param>
		/// <param name="nASIODevice"></param>
		public CSoundManager(ESoundDeviceType soundDeviceType, int nSoundDelayExclusiveWASAPI, int nSoundDelayASIO, int nASIODevice, int nSoundDelayBASS, bool _bUseOSTimer)
		{
			SoundDevice = null;
			//bUseOSTimer = false;
			tInitialize(soundDeviceType, nSoundDelayExclusiveWASAPI, nSoundDelayASIO, nASIODevice, nSoundDelayBASS, _bUseOSTimer);
		}
		public void Dispose()
		{
			t終了();
		}
		public void tInitialize(ESoundDeviceType soundDeviceType, int _nSoundDelayExclusiveWASAPI, int _nSoundDelayASIO, int _nASIODevice, int _nSoundDelayBASS, bool _bUseOSTimer)
		{
			//SoundDevice = null;						// 後で再初期化することがあるので、null初期化はコンストラクタに回す
			rc演奏用タイマ = null;                        // Global.Bass 依存（つまりユーザ依存）
			nMixing = 0;

			SoundDelayExclusiveWASAPI = _nSoundDelayExclusiveWASAPI;
			SoundDelayASIO = _nSoundDelayASIO;
			SoundDelayBASS = _nSoundDelayBASS;
			ASIODevice = _nASIODevice;
			bUseOSTimer = _bUseOSTimer;

			ESoundDeviceType[] ESoundDeviceTypes = new ESoundDeviceType[6]
			{
				ESoundDeviceType.SharedWASAPI,
				ESoundDeviceType.ExclusiveWASAPI,
				ESoundDeviceType.ASIO,
				ESoundDeviceType.BASS,
				ESoundDeviceType.OpenAL,
				ESoundDeviceType.Unknown
			};

			int n初期デバイス;
			switch (soundDeviceType)
			{
				case ESoundDeviceType.SharedWASAPI:
					n初期デバイス = 0;
					break;
				case ESoundDeviceType.ExclusiveWASAPI:
					n初期デバイス = 1;
					break;
				case ESoundDeviceType.ASIO:
					n初期デバイス = 2;
					break;
				case ESoundDeviceType.BASS:
					n初期デバイス = 3;
					break;
				case ESoundDeviceType.OpenAL:
					n初期デバイス = 4;
					break;
				default:
					n初期デバイス = 5;
					break;
			}
			for (SoundDeviceType = ESoundDeviceTypes[n初期デバイス]; ; SoundDeviceType = ESoundDeviceTypes[++n初期デバイス])
			{
				try
				{
					t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する();
					break;
				}
				catch (Exception e)
				{
					Trace.TraceError(e.ToString());
					Trace.TraceError("An exception has occurred, but processing continues.");
					if (ESoundDeviceTypes[n初期デバイス] == ESoundDeviceType.Unknown)
					{
						Trace.TraceError(string.Format("サウンドデバイスの初期化に失敗しました。"));
						break;
					}
				}
			}
			if (SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI || SoundDeviceType == ESoundDeviceType.SharedWASAPI || SoundDeviceType == ESoundDeviceType.ASIO || SoundDeviceType == ESoundDeviceType.BASS)
			{
				Trace.TraceInformation("BASS_CONFIG_UpdatePeriod=" + Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD));
				Trace.TraceInformation("BASS_CONFIG_UpdateThreads=" + Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS));
			}
		}

		public static void t終了()
		{
			C共通.tDisposeする(SoundDevice); SoundDevice = null;
			C共通.tDisposeする(ref rc演奏用タイマ);   // Global.Bass を解放した後に解放すること。（Global.Bass で参照されているため）
		}


		public static void t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する()
		{
			#region [ すでにサウンドデバイスと演奏タイマが構築されていれば解放する。]
			//-----------------
			if (SoundDevice != null)
			{
				// すでに生成済みのサウンドがあれば初期状態に戻す。

				CSound.tすべてのサウンドを初期状態に戻す();     // リソースは解放するが、CSoundのインスタンスは残す。


				// サウンドデバイスと演奏タイマを解放する。

				C共通.tDisposeする(SoundDevice); SoundDevice = null;
				C共通.tDisposeする(ref rc演奏用タイマ);   // Global.SoundDevice を解放した後に解放すること。（Global.SoundDevice で参照されているため）
			}
			//-----------------
			#endregion

			#region [ 新しいサウンドデバイスを構築する。]
			//-----------------
			switch (SoundDeviceType)
			{
				case ESoundDeviceType.ExclusiveWASAPI:
					SoundDevice = new CSoundDeviceWASAPI(CSoundDeviceWASAPI.Eデバイスモード.Exclusive, SoundDelayExclusiveWASAPI, SoundUpdatePeriodExclusiveWASAPI);
					break;

				case ESoundDeviceType.SharedWASAPI:
					SoundDevice = new CSoundDeviceWASAPI(CSoundDeviceWASAPI.Eデバイスモード.Shared, SoundDelaySharedWASAPI, SoundUpdatePeriodSharedWASAPI);
					break;

				case ESoundDeviceType.ASIO:
					SoundDevice = new CSoundDeviceASIO(SoundDelayASIO, ASIODevice);
					break;

				case ESoundDeviceType.BASS:
					SoundDevice = new CSoundDeviceBASS(SoundUpdatePeriodBASS, SoundDelayBASS);
					break;

				case ESoundDeviceType.OpenAL:
					SoundDevice = new CSoundDeviceOpenAL(SoundDelayOpenAL, bUseOSTimer);
					break;

				default:
					throw new Exception(string.Format("未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString()));
			}
			//-----------------
			#endregion
			#region [ 新しい演奏タイマを構築する。]
			//-----------------
			rc演奏用タイマ = new CSoundTimer(SoundDevice);
			//-----------------
			#endregion

			SoundDevice.nMasterVolume = _nMasterVolume;                 // サウンドデバイスに対して、マスターボリュームを再設定する

			CSound.tすべてのサウンドを再構築する(SoundDevice);        // すでに生成済みのサウンドがあれば作り直す。
		}
		public CSound tCreateSound(string filename, ESoundGroup soundGroup)
		{
			if (!File.Exists(filename))
			{
				Trace.TraceWarning($"[i18n] File does not exist: {filename}");
				return null;
			}

			if (SoundDeviceType == ESoundDeviceType.Unknown)
			{
				throw new Exception(string.Format("未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString()));
			}
			return SoundDevice.tCreateSound(filename, soundGroup);
		}

		public float GetCPUusage()
		{
			float f;
			switch (SoundDeviceType)
			{
				case ESoundDeviceType.ExclusiveWASAPI:
				case ESoundDeviceType.SharedWASAPI:
					f = BassWasapi.BASS_WASAPI_GetCPU();
					break;
				case ESoundDeviceType.ASIO:
					f = BassAsio.BASS_ASIO_GetCPU();
					break;
				case ESoundDeviceType.BASS:
					f = Bass.BASS_GetCPU();
					break;
				case ESoundDeviceType.OpenAL:
					f = 0.0f;
					break;
				default:
					f = 0.0f;
					break;
			}
			return f;
		}

		public string GetCurrentSoundDeviceType()
		{
			switch (SoundDeviceType)
			{
				case ESoundDeviceType.ExclusiveWASAPI:
					return "WASAPI(Exclusive)";
				case ESoundDeviceType.SharedWASAPI:
					return "WASAPI(Shared)";
				case ESoundDeviceType.ASIO:
					return "ASIO";
				case ESoundDeviceType.BASS:
					return "BASS";
				case ESoundDeviceType.OpenAL:
					return "OpenAL";
				default:
					return "Unknown";
			}
		}

		public void AddMixer(CSound cs, double db再生速度, bool _b演奏終了後も再生が続くチップである)
		{
			cs.b演奏終了後も再生が続くチップである = _b演奏終了後も再生が続くチップである;
			cs.db再生速度 = db再生速度;
			cs.tBASSサウンドをミキサーに追加する();
		}
		public void RemoveMixer(CSound cs)
		{
			cs.tBASSサウンドをミキサーから削除する();
		}
	}
	#endregion
}
