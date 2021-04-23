using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace FDK
{
	public class CSoundDeviceBASS : ISoundDevice
	{
		// プロパティ

		public ESoundDeviceType eOutputDevice
		{
			get;
			protected set;
		}
		public long nOutPutDelayms
		{
			get;
			protected set;
		}
		public long nBufferSizems
		{
			get;
			protected set;
		}

		// CSoundTimer 用に公開しているプロパティ

		public long nElapsedTimems
		{
			get;
			protected set;
		}
		public long SystemTimemsWhenUpdatingElapsedTime
		{
			get;
			protected set;
		}
		public CTimer tmSystemTimer
		{
			get;
			protected set;
		}

		public float CPUUsage
		{
			get
			{
				return Bass.BASS_GetCPU();
			}
		}


		// マスターボリュームの制御コードは、WASAPI/ASIOで全く同じ。
		public int nMasterVolume
		{
			get
			{
				float f音量 = 0.0f;
				bool b = Bass.BASS_ChannelGetAttribute(this.hMixer, BASSAttribute.BASS_ATTRIB_VOL, ref f音量);
				if (!b)
				{
					BASSError be = Bass.BASS_ErrorGetCode();
					Trace.TraceInformation("ASIO Master Volume Get Error: " + be.ToString());
				}
				else
				{
					//Trace.TraceInformation( "ASIO Master Volume Get Success: " + (f音量 * 100) );

				}
				return (int)(f音量 * 100);
			}
			set
			{
				bool b = Bass.BASS_ChannelSetAttribute(this.hMixer, BASSAttribute.BASS_ATTRIB_VOL, (float)(value / 100.0));
				if (!b)
				{
					BASSError be = Bass.BASS_ErrorGetCode();
					Trace.TraceInformation("ASIO Master Volume Set Error: " + be.ToString());
				}
				else
				{
					// int n = this.nMasterVolume;	
					// Trace.TraceInformation( "ASIO Master Volume Set Success: " + value );
				}
			}
		}

		public CSoundDeviceBASS(int UpdatePeriod, int BufferSizems)
		{
			Trace.TraceInformation("BASS の初期化を開始します。");
			this.eOutputDevice = ESoundDeviceType.Unknown;
			this.nOutPutDelayms = 0;
			this.nElapsedTimems = 0;
			this.SystemTimemsWhenUpdatingElapsedTime = CTimer.nUnused;
			this.tmSystemTimer = new CTimer();

			#region [ BASS registration ]
			// BASS.NET ユーザ登録（BASSスプラッシュが非表示になる）。
			BassNet.Registration("dtx2013@gmail.com", "2X9181017152222");
			#endregion

			#region [ BASS Version Check ]
			// BASS のバージョンチェック。
			int nBASSVersion = Utils.HighWord(Bass.BASS_GetVersion());
			if (nBASSVersion != Bass.BASSVERSION)
				throw new DllNotFoundException(string.Format("bass.dll のバージョンが異なります({0})。このプログラムはバージョン{1}で動作します。", nBASSVersion, Bass.BASSVERSION));

			int nBASSMixVersion = Utils.HighWord(BassMix.BASS_Mixer_GetVersion());
			if (nBASSMixVersion != BassMix.BASSMIXVERSION)
				throw new DllNotFoundException(string.Format("bassmix.dll のバージョンが異なります({0})。このプログラムはバージョン{1}で動作します。", nBASSMixVersion, BassMix.BASSMIXVERSION));
			#endregion

			this.bIsBASSSoundFree = true;

			// BASS の初期化。

			int n周波数 = 44100;
			if (!Bass.BASS_Init(-1, n周波数, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
				throw new Exception(string.Format("BASS の初期化に失敗しました。(BASS_Init)[{0}]", Bass.BASS_ErrorGetCode().ToString()));

			Bass.BASS_SetDevice(-1);
			
			if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, UpdatePeriod))
			{
				Trace.TraceWarning($"BASS_SetConfig({nameof(BASSConfig.BASS_CONFIG_UPDATEPERIOD)}) に失敗しました。[{Bass.BASS_ErrorGetCode()}]");
			}
			
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, BufferSizems);
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_CURVE_VOL, true);

			this.tSTREAMPROC = new STREAMPROC(Stream処理);
			this.hMainStream = Bass.BASS_StreamCreate(n周波数, 2, BASSFlag.BASS_DEFAULT, this.tSTREAMPROC, IntPtr.Zero);

			var flag = BASSFlag.BASS_MIXER_NONSTOP| BASSFlag.BASS_STREAM_DECODE;   // デコードのみ＝発声しない。
			this.hMixer = BassMix.BASS_Mixer_StreamCreate(n周波数, 2, flag);

			if (this.hMixer == 0)
			{
				BASSError err = Bass.BASS_ErrorGetCode();
				Bass.BASS_Free();
				this.bIsBASSSoundFree = true;
				throw new Exception(string.Format("BASSミキサ(mixing)の作成に失敗しました。[{0}]", err));
			}

			// BASS ミキサーの1秒あたりのバイト数を算出。

			this.bIsBASSSoundFree = false;

			var mixerInfo = Bass.BASS_ChannelGetInfo(this.hMixer);
			int nサンプルサイズbyte = 2;
			//long nミキサーの1サンプルあたりのバイト数 = /*mixerInfo.chans*/ 2 * nサンプルサイズbyte;
			long nミキサーの1サンプルあたりのバイト数 = mixerInfo.chans * nサンプルサイズbyte;
			this.nミキサーの1秒あたりのバイト数 = nミキサーの1サンプルあたりのバイト数 * mixerInfo.freq;

			// 単純に、hMixerの音量をMasterVolumeとして制御しても、
			// ChannelGetData()の内容には反映されない。
			// そのため、もう一段mixerを噛ませて、一段先のmixerからChannelGetData()することで、
			// hMixerの音量制御を反映させる。
			this.hMixer_DeviceOut = BassMix.BASS_Mixer_StreamCreate(
				n周波数, 2, flag);
			if (this.hMixer_DeviceOut == 0)
			{
				BASSError errcode = Bass.BASS_ErrorGetCode();
				Bass.BASS_Free();
				this.bIsBASSSoundFree = true;
				throw new Exception(string.Format("BASSミキサ(最終段)の作成に失敗しました。[{0}]", errcode));
			}
			{
				bool b1 = BassMix.BASS_Mixer_StreamAddChannel(this.hMixer_DeviceOut, this.hMixer, BASSFlag.BASS_DEFAULT);
				if (!b1)
				{
					BASSError errcode = Bass.BASS_ErrorGetCode();
					Bass.BASS_Free();
					this.bIsBASSSoundFree = true;
					throw new Exception(string.Format("BASSミキサ(最終段とmixing)の接続に失敗しました。[{0}]", errcode));
				};
			}

			this.eOutputDevice = ESoundDeviceType.BASS;

			// 出力を開始。

			if (!Bass.BASS_Start())     // 範囲外の値を指定した場合は自動的にデフォルト値に設定される。
			{
				BASSError err = Bass.BASS_ErrorGetCode();
				Bass.BASS_Free();
				this.bIsBASSSoundFree = true;
				throw new Exception("BASS デバイス出力開始に失敗しました。" + err.ToString());
			}
			else
			{
				var info = Bass.BASS_GetInfo();

				this.nBufferSizems = this.nOutPutDelayms = info.latency + BufferSizems;//求め方があっているのだろうか…

				Trace.TraceInformation("BASS デバイス出力開始:[{0}ms]", this.nOutPutDelayms);
			}

			Bass.BASS_ChannelPlay(this.hMainStream, false);

		}

		#region [ tCreateSound() ]
		public CSound tCreateSound(string strファイル名, ESoundGroup soundGroup)
		{
			var sound = new CSound(soundGroup);
			sound.tBASSサウンドを作成する(strファイル名, this.hMixer);
			return sound;
		}

		public void tCreateSound(string strファイル名, CSound sound)
		{
			sound.tBASSサウンドを作成する(strファイル名, this.hMixer);
		}
		public void tCreateSound(byte[] byArrWAVファイルイメージ, CSound sound)
		{
			sound.tBASSサウンドを作成する(byArrWAVファイルイメージ, this.hMixer);
		}
		#endregion


		#region [ Dispose-Finallizeパターン実装 ]
		//-----------------
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected void Dispose(bool bManagedDispose)
		{
			this.eOutputDevice = ESoundDeviceType.Unknown;      // まず出力停止する(Dispose中にクラス内にアクセスされることを防ぐ)
			if (hMainStream != -1)
			{
				Bass.BASS_StreamFree(this.hMainStream);
			}
			if (hMixer != -1)
			{
				Bass.BASS_StreamFree(this.hMixer);
			}
			if (!this.bIsBASSSoundFree)
			{
				Bass.BASS_Stop();
				Bass.BASS_Free();// システムタイマより先に呼び出すこと。（Stream処理() の中でシステムタイマを参照してるため）
			}

			if (bManagedDispose)
			{
				CCommon.tDispose(this.tmSystemTimer);
				this.tmSystemTimer = null;
			}
		}
		~CSoundDeviceBASS()
		{
			this.Dispose(false);
		}
		//-----------------
		#endregion

		public int Stream処理(int handle, IntPtr buffer, int length, IntPtr user)
		{
			// BASSミキサからの出力データをそのまま ASIO buffer へ丸投げ。

			int num = Bass.BASS_ChannelGetData(this.hMixer_DeviceOut, buffer, length);      // num = 実際に転送した長さ

			if (num == -1) num = 0;

			// 経過時間を更新。
			// データの転送差分ではなく累積転送バイト数から算出する。

			this.nElapsedTimems = (this.n累積転送バイト数 * 1000 / this.nミキサーの1秒あたりのバイト数) - this.nOutPutDelayms;
			this.SystemTimemsWhenUpdatingElapsedTime = this.tmSystemTimer.nシステム時刻ms;


			// 経過時間を更新後に、今回分の累積転送バイト数を反映。

			this.n累積転送バイト数 += num;
			return num;
		}
		private long nミキサーの1秒あたりのバイト数 = 0;
		private long n累積転送バイト数 = 0;

		protected int hMainStream = -1;
		protected int hMixer = -1;
		protected int hMixer_DeviceOut = -1;
		protected STREAMPROC tSTREAMPROC = null;
		private bool bIsBASSSoundFree = true;

	}
}
