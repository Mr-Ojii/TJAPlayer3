using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading;
using FDK.ExtensionMethods;
using SharpDX.Multimedia;
using Un4seen.Bass;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Fx;
using OpenTK.Audio.OpenAL;

namespace FDK
{
	#region [ DTXMania用拡張 ]
	public class CSound管理	// : CSound
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
		public static bool bUseOSTimer = false;		// OSのタイマーを使うか、CSoundTimerを使うか。DTXCではfalse, DTXManiaではtrue。
													// DTXCでCSoundTimerを使うと、内部で無音のループサウンドを再生するため
													// サウンドデバイスを占有してしまい、Viewerとして呼び出されるDTXManiaで、ASIOが使えなくなる。

													// DTXMania単体でこれをtrueにすると、WASAPI/ASIO時に演奏タイマーとしてFDKタイマーではなく
													// システムのタイマーを使うようになる。こうするとスクロールは滑らかになるが、音ズレが出るかもしれない。
		
		public static IntPtr WindowHandle;

		public static bool bIsTimeStretch = false;

		private static int _nMasterVolume;
		public int nMasterVolume
		{
			get
			{
				return _nMasterVolume;
			}
			//get
			//{
			//    if ( SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI || SoundDeviceType == ESoundDeviceType.ASIO )
			//    {
			//        return Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM ) / 100;
			//    }
			//    else
			//    {
			//        return 100;
			//    }
			//}
			//set
			//{
			//    if ( SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI )
			//    {
			//			// LINEARでなくWINDOWS(2)を使う必要があるが、exclusive時は使用不可、またデバイス側が対応してないと使用不可
			//        bool b = BassWasapi.BASS_WASAPI_SetVolume( BASSWASAPIVolume.BASS_WASAPI_CURVE_LINEAR, value / 100.0f );
			//        if ( !b )
			//        {
			//            BASSError be = Bass.BASS_ErrorGetCode();
			//            Trace.TraceInformation( "WASAPI Master Volume Set Error: " + be.ToString() );
			//        }
			//    }
			//}
			//set
			//{
			//    if ( SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI || SoundDeviceType == ESoundDeviceType.ASIO )
			//    {
			//        bool b = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, value * 100 );
			//        if ( !b )
			//        {
			//            BASSError be = Bass.BASS_ErrorGetCode();
			//            Trace.TraceInformation( "Master Volume Set Error: " + be.ToString() );
			//        }
			//    }
			//}
			//set
			//{
			//    if ( SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI || SoundDeviceType == ESoundDeviceType.ASIO )
			//    {
			//        var nodes = new BASS_MIXER_NODE[ 1 ] { new BASS_MIXER_NODE( 0, (float) value ) };
			//        BassMix.BASS_Mixer_ChannelSetEnvelope( SoundDevice.hMixer, BASSMIXEnvelope.BASS_MIXER_ENV_VOL, nodes );
			//    }
			//}
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
		public static int SoundDelayExclusiveWASAPI = 0;		// SSTでは、50ms
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
		///// <summary>
		///// <para>ASIO 出力における再生遅延[ms]（の希望値）。最終的にはこの数値を基にドライバが決定する）。</para>
		///// </summary>
		//public static int SoundDelayASIO = 0;					// SSTでは50ms。0にすると、デバイスの設定値をそのまま使う。
		/// <summary>
		/// <para>ASIO 出力におけるバッファサイズ。</para>
		/// </summary>
		public static int SoundDelayASIO = 0;						// 0にすると、デバイスの設定値をそのまま使う。
		public static int ASIODevice = 0;
		public int GetASIODevice()
		{
			return ASIODevice;
		}
		public void SetASIODevice(int value)
		{
			ASIODevice = value;
		}
		/// <summary>
		/// <para>OpenAL 出力における再生遅延[ms]。ユーザが決定する。</para>
		/// </summary>
		public static int SoundDelayOpenAL = 100;

		public long GetSoundDelay()
		{
			if ( SoundDevice != null )
			{
				return SoundDevice.n実バッファサイズms;
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
		public CSound管理( IntPtr handle, ESoundDeviceType soundDeviceType, int nSoundDelayExclusiveWASAPI, int nSoundDelayASIO, int nASIODevice, bool _bUseOSTimer )
		{
			WindowHandle = handle;
			SoundDevice = null;
			//bUseOSTimer = false;
			t初期化( soundDeviceType, nSoundDelayExclusiveWASAPI, nSoundDelayASIO, nASIODevice, _bUseOSTimer );
		}
		public void Dispose()
		{
			t終了();
		}
		public void t初期化( ESoundDeviceType soundDeviceType, int _nSoundDelayExclusiveWASAPI, int _nSoundDelayASIO, int _nASIODevice, bool _bUseOSTimer )
		{
			//SoundDevice = null;						// 後で再初期化することがあるので、null初期化はコンストラクタに回す
			rc演奏用タイマ = null;						// Global.Bass 依存（つまりユーザ依存）
			nMixing = 0;

			SoundDelayExclusiveWASAPI = _nSoundDelayExclusiveWASAPI;
			SoundDelayASIO = _nSoundDelayASIO;
			ASIODevice = _nASIODevice;
			bUseOSTimer = _bUseOSTimer;

			ESoundDeviceType[] ESoundDeviceTypes = new ESoundDeviceType[ 5 ]
			{
				ESoundDeviceType.SharedWASAPI,
				ESoundDeviceType.ExclusiveWASAPI,
				ESoundDeviceType.ASIO,
				ESoundDeviceType.OpenAL,
				ESoundDeviceType.Unknown
			};

			int n初期デバイス;
			switch ( soundDeviceType )
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
				case ESoundDeviceType.OpenAL:
					n初期デバイス = 3;
					break;
				default:
					n初期デバイス = 4;
					break;
			}
			for ( SoundDeviceType = ESoundDeviceTypes[ n初期デバイス ]; ; SoundDeviceType = ESoundDeviceTypes[ ++n初期デバイス ] )
			{
				try
				{
					t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する();
					break;
				}
				catch ( Exception e )
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceError( "例外が発生しましたが処理を継続します。 (2609806d-23e8-45c2-9389-b427e80915bc)" );
					if ( ESoundDeviceTypes[ n初期デバイス ] == ESoundDeviceType.Unknown )
					{
						Trace.TraceError( string.Format( "サウンドデバイスの初期化に失敗しました。" ) );
						break;
					}
				}
			}
			if ( SoundDeviceType == ESoundDeviceType.ExclusiveWASAPI || SoundDeviceType == ESoundDeviceType.SharedWASAPI || SoundDeviceType == ESoundDeviceType.ASIO )
			{
				//Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_UPDATETHREADS, 4 );
				//Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0 );

				Trace.TraceInformation( "BASS_CONFIG_UpdatePeriod=" + Bass.BASS_GetConfig( BASSConfig.BASS_CONFIG_UPDATEPERIOD ) );
				Trace.TraceInformation( "BASS_CONFIG_UpdateThreads=" + Bass.BASS_GetConfig( BASSConfig.BASS_CONFIG_UPDATETHREADS ) );
			}
		}

		public static void t終了()
		{
			C共通.tDisposeする( SoundDevice ); SoundDevice = null;
			C共通.tDisposeする( ref rc演奏用タイマ );	// Global.Bass を解放した後に解放すること。（Global.Bass で参照されているため）
		}


		public static void t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する()
		{
			#region [ すでにサウンドデバイスと演奏タイマが構築されていれば解放する。]
			//-----------------
			if ( SoundDevice != null )
			{
				// すでに生成済みのサウンドがあれば初期状態に戻す。

				CSound.tすべてのサウンドを初期状態に戻す();		// リソースは解放するが、CSoundのインスタンスは残す。


				// サウンドデバイスと演奏タイマを解放する。

				C共通.tDisposeする( SoundDevice ); SoundDevice = null;
				C共通.tDisposeする( ref rc演奏用タイマ );	// Global.SoundDevice を解放した後に解放すること。（Global.SoundDevice で参照されているため）
			}
			//-----------------
			#endregion

			#region [ 新しいサウンドデバイスを構築する。]
			//-----------------
			switch ( SoundDeviceType )
			{
				case ESoundDeviceType.ExclusiveWASAPI:
					SoundDevice = new CSoundDeviceWASAPI( CSoundDeviceWASAPI.Eデバイスモード.排他, SoundDelayExclusiveWASAPI, SoundUpdatePeriodExclusiveWASAPI );
					break;

				case ESoundDeviceType.SharedWASAPI:
					SoundDevice = new CSoundDeviceWASAPI( CSoundDeviceWASAPI.Eデバイスモード.共有, SoundDelaySharedWASAPI, SoundUpdatePeriodSharedWASAPI );
					break;

				case ESoundDeviceType.ASIO:
					SoundDevice = new CSoundDeviceASIO( SoundDelayASIO, ASIODevice );
					break;

				case ESoundDeviceType.OpenAL:
					SoundDevice = new CSoundDeviceOpenAL( WindowHandle, SoundDelayOpenAL, bUseOSTimer );
					break;

				default:
					throw new Exception( string.Format( "未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString() ) );
			}
			//-----------------
			#endregion
			#region [ 新しい演奏タイマを構築する。]
			//-----------------
			rc演奏用タイマ = new CSoundTimer( SoundDevice );
			//-----------------
			#endregion

			SoundDevice.nMasterVolume = _nMasterVolume;					// サウンドデバイスに対して、マスターボリュームを再設定する

			CSound.tすべてのサウンドを再構築する( SoundDevice );		// すでに生成済みのサウンドがあれば作り直す。
		}
		public CSound tサウンドを生成する( string filename, ESoundGroup soundGroup )
		{
			if( !File.Exists( filename ) )
			{
				Trace.TraceWarning($"[i18n] File does not exist: {filename}");
				return null;
			}

			if ( SoundDeviceType == ESoundDeviceType.Unknown )
			{
				throw new Exception( string.Format( "未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString() ) );
			}
			return SoundDevice.tサウンドを作成する( filename, soundGroup );
		}

		public void tサウンドを破棄する( CSound csound )
		{
			csound?.t解放する( true );			// インスタンスは存続→破棄にする。
		}

		public float GetCPUusage()
		{
			float f;
			switch ( SoundDeviceType )
			{
				case ESoundDeviceType.ExclusiveWASAPI:
				case ESoundDeviceType.SharedWASAPI:
					f = BassWasapi.BASS_WASAPI_GetCPU();
					break;
				case ESoundDeviceType.ASIO:
					f = BassAsio.BASS_ASIO_GetCPU();
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
			switch ( SoundDeviceType )
			{
				case ESoundDeviceType.ExclusiveWASAPI:
					return "WASAPI(Exclusive)";
				case ESoundDeviceType.SharedWASAPI:
					return "WASAPI(Shared)";
				case ESoundDeviceType.ASIO:
					return "ASIO";
				case ESoundDeviceType.OpenAL:
					return "OpenAL";
				default:
					return "Unknown";
			}
		}

		public void AddMixer( CSound cs, double db再生速度, bool _b演奏終了後も再生が続くチップである )
		{
			cs.b演奏終了後も再生が続くチップである = _b演奏終了後も再生が続くチップである;
			cs.db再生速度 = db再生速度;
			cs.tBASSサウンドをミキサーに追加する();
		}
		public void RemoveMixer( CSound cs )
		{
			cs.tBASSサウンドをミキサーから削除する();
		}
	}
	#endregion

	// CSound は、サウンドデバイスが変更されたときも、インスタンスを再作成することなく、新しいデバイスで作り直せる必要がある。
	// そのため、デバイスごとに別のクラスに分割するのではなく、１つのクラスに集約するものとする。

	public class CSound : IDisposable
	{
		public const int MinimumSongVol = 0;
		public const int MaximumSongVol = 200; // support an approximate doubling in volume.
		public const int DefaultSongVol = 100;

		// 2018-08-19 twopointzero: Note the present absence of a MinimumAutomationLevel.
		// We will revisit this if/when song select BGM fade-in/fade-out needs
		// updating due to changing the type or range of AutomationLevel
		public const int MaximumAutomationLevel = 100;
		public const int DefaultAutomationLevel = 100;

		public const int MinimumGroupLevel = 0;
		public const int MaximumGroupLevel = 100;
		public const int DefaultGroupLevel = 100;
		public const int DefaultSoundEffectLevel = 80;
		public const int DefaultVoiceLevel = 90;
		public const int DefaultSongPreviewLevel = 75;
		public const int DefaultSongPlaybackLevel = 90;

		public static readonly Lufs MinimumLufs = new Lufs(-100.0);
		public static readonly Lufs MaximumLufs = new Lufs(10.0); // support an approximate doubling in volume.

		private static readonly Lufs DefaultGain = new Lufs(0.0);

		public readonly ESoundGroup SoundGroup;

		#region [ DTXMania用拡張 ]

		public int n総演奏時間ms
		{
			get;
			private set;
		}
		public int nサウンドバッファサイズ		// 取りあえず0固定★★★★★★★★★★★★★★★★★★★★
		{
			get { return 0; }
		}
		public bool bストリーム再生する			// 取りあえずfalse固定★★★★★★★★★★★★★★★★★★★★
												// trueにすると同一チップ音の多重再生で問題が出る(4POLY音源として動かない)
		{
			get { return false; }
		}
		public double db再生速度
		{
			get
			{
				return _db再生速度;
			}
			set
			{
				if ( _db再生速度 != value )
				{
					_db再生速度 = value;
					bIs1倍速再生 = ( _db再生速度 == 1.000f );
					if ( bBASSサウンドである )
					{
						if ( _hTempoStream != 0 && !this.bIs1倍速再生 )	// 再生速度がx1.000のときは、TempoStreamを用いないようにして高速化する
						{
							this.hBassStream = _hTempoStream;
						}
						else
						{
							this.hBassStream = _hBassStream;
						}

						if ( CSound管理.bIsTimeStretch )
						{
							Bass.BASS_ChannelSetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_TEMPO, (float) ( _db再生速度 * 100 - 100 ) );
							//double seconds = Bass.BASS_ChannelBytes2Seconds( this.hTempoStream, nBytes );
							//this.n総演奏時間ms = (int) ( seconds * 1000 );
						}
						else
						{
							Bass.BASS_ChannelSetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_FREQ, ( float ) ( _db再生速度 * nオリジナルの周波数 ) );
						}
					}
					else
					{
						try
						{
							for (int i = 0; i < this.SourceOpen.Length; i++)
							{
								AL.Source(this.SourceOpen[i], ALSourcef.Pitch, (float)db再生速度);
							}
						}
						catch
						{
							//例外処理は出さない
							this.b速度上げすぎ問題 = true;
						}
					}
				}
			}
		}
		#endregion

		public bool b速度上げすぎ問題 = false;
		public bool b演奏終了後も再生が続くチップである = false;	// これがtrueなら、本サウンドの再生終了のコールバック時に自動でミキサーから削除する

		//private STREAMPROC _cbStreamXA;		// make it global, so that the GC can not remove it
		private SYNCPROC _cbEndofStream;	// ストリームの終端まで再生されたときに呼び出されるコールバック
//		private WaitCallback _cbRemoveMixerChannel;

		/// <summary>
		/// Gain is applied "first" to the audio data, much as in a physical or
		/// software mixer. Later steps in the flow of audio apply "channel" level
		/// (e.g. AutomationLevel) and mixing group level (e.g. GroupLevel) before
		/// the audio is output.
		/// 
		/// This method, taking an integer representing a percent value, is used
		/// for mixing in the SONGVOL value, when available. It is also used for
		/// DTXViewer preview mode.
		/// </summary>
		public void SetGain(int songVol)
		{
			SetGain(LinearIntegerPercentToLufs(songVol), null);
		}

		private static Lufs LinearIntegerPercentToLufs(int percent)
		{
			// 2018-08-27 twopointzero: We'll use the standard conversion until an appropriate curve can be selected
			return new Lufs(20.0 * Math.Log10(percent / 100.0));
		}

		/// <summary>
		/// Gain is applied "first" to the audio data, much as in a physical or
		/// software mixer. Later steps in the flow of audio apply "channel" level
		/// (e.g. AutomationLevel) and mixing group level (e.g. GroupLevel) before
		/// the audio is output.
		/// 
		/// This method, taking a LUFS gain value and a LUFS true audio peak value,
		/// is used for mixing in the loudness-metadata-base gain value, when available.
		/// </summary>
		public void SetGain(Lufs gain, Lufs? truePeak)
		{
			if (Equals(_gain, gain))
			{
				return;
			}

			_gain = gain;
			_truePeak = truePeak;

			if (SoundGroup == ESoundGroup.SongPlayback)
			{
				Trace.TraceInformation($"{nameof(CSound)}.{nameof(SetGain)}: Gain: {_gain}. True Peak: {_truePeak}");
			}

			SetVolume();
		}

		/// <summary>
		/// AutomationLevel is applied "second" to the audio data, much as in a
		/// physical or sofware mixer and its channel level. Before this Gain is
		/// applied, and after this the mixing group level is applied.
		///
		/// This is currently used only for automated fade in and out as is the
		/// case right now for the song selection screen background music fade
		/// in and fade out.
		/// </summary>
		public int AutomationLevel
		{
			get => _automationLevel;
			set
			{
				if (_automationLevel == value)
				{
					return;
				}

				_automationLevel = value;

				if (SoundGroup == ESoundGroup.SongPlayback)
				{
					Trace.TraceInformation($"{nameof(CSound)}.{nameof(AutomationLevel)} set: {AutomationLevel}");
				}

				SetVolume();
			}
		}

		/// <summary>
		/// GroupLevel is applied "third" to the audio data, much as in the sub
		/// mixer groups of a physical or software mixer. Before this both the
		/// Gain and AutomationLevel are applied, and after this the audio
		/// flows into the audio subsystem for mixing and output based on the
		/// master volume.
		///
		/// This is currently automatically managed for each sound based on the
		/// configured and dynamically adjustable sound group levels for each of
		/// sound effects, voice, song preview, and song playback.
		///
		/// See the SoundGroupLevelController and related classes for more.
		/// </summary>
		public int GroupLevel
		{
			private get => _groupLevel;
			set
			{
				if (_groupLevel == value)
				{
					return;
				}

				_groupLevel = value;

				if (SoundGroup == ESoundGroup.SongPlayback)
				{
					Trace.TraceInformation($"{nameof(CSound)}.{nameof(GroupLevel)} set: {GroupLevel}");
				}

				SetVolume();
			}
		}

		private void SetVolume()
		{
			var automationLevel = LinearIntegerPercentToLufs(AutomationLevel);
			var groupLevel = LinearIntegerPercentToLufs(GroupLevel);

			var gain =
				_gain +
				automationLevel +
				groupLevel;

			var safeTruePeakGain = _truePeak?.Negate() ?? new Lufs(0);
			var finalGain = gain.Min(safeTruePeakGain);

			if (SoundGroup == ESoundGroup.SongPlayback)
			{
				Trace.TraceInformation(
					$"{nameof(CSound)}.{nameof(SetVolume)}: Gain:{_gain}. Automation Level: {automationLevel}. Group Level: {groupLevel}. Summed Gain: {gain}. Safe True Peak Gain: {safeTruePeakGain}. Final Gain: {finalGain}.");
			}

			lufs音量 = finalGain;
		}

		private Lufs lufs音量
		{
			set
			{
				if (this.bBASSサウンドである)
				{
					var db音量 = ((value.ToDouble() / 100.0) + 1.0).Clamp(0, 1);
					Bass.BASS_ChannelSetAttribute(this.hBassStream, BASSAttribute.BASS_ATTRIB_VOL, (float) db音量);
				}
				else if (this.bOpenALである)
				{
					var db音量 = ((value.ToDouble() / 100.0) + 1.0).Clamp(0, 1);

					for (int i = 0; i < this.SourceOpen.Length; i++)
					{
						AL.Source(this.SourceOpen[i], ALSourcef.Gain, (float)db音量);
					}
				}
			}
		}

		/// <summary>
		/// <para>左:-100～中央:0～100:右。set のみ。</para>
		/// </summary>
		public int n位置
		{
			get
			{
				if( this.bBASSサウンドである )
				{
					float f位置 = 0.0f;
					if ( !Bass.BASS_ChannelGetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_PAN, ref f位置 ) )
						//if( BassMix.BASS_Mixer_ChannelGetEnvelopePos( this.hBassStream, BASSMIXEnvelope.BASS_MIXER_ENV_PAN, ref f位置 ) == -1 )
						return 0;
					return (int) ( f位置 * 100 );
				}
				else if( this.bOpenALである )
				{
					return this._n位置;
				}
				return -9999;
			}
			set
			{
				if( this.bBASSサウンドである )
				{
					float f位置 = Math.Min( Math.Max( value, -100 ), 100 ) / 100.0f;	// -100～100 → -1.0～1.0
					//var nodes = new BASS_MIXER_NODE[ 1 ] { new BASS_MIXER_NODE( 0, f位置 ) };
					//BassMix.BASS_Mixer_ChannelSetEnvelope( this.hBassStream, BASSMIXEnvelope.BASS_MIXER_ENV_PAN, nodes );
					Bass.BASS_ChannelSetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_PAN, f位置 );
				}
				else if( this.bOpenALである )
				{
					float f位置 = (Math.Min(Math.Max(value, -100), 100) / 100.0f);  // -100～100 → -1.0～1.0
					for (int i = 0; i < this.SourceOpen.Length; i++)
					{
						float tmppan = Math.Min(Math.Max(f位置 * 2 + defaultPan[i], -1f), 1f);//もっとよい数式ください

						AL.Source(this.SourceOpen[i], ALSource3f.Position, tmppan, 0f, 0f);
					}
					_n位置 = value;
				}
			}
		}

		/// <summary>
		/// <para>全インスタンスリスト。</para>
		/// <para>～を作成する() で追加され、t解放する() or Dispose() で解放される。</para>
		/// </summary>
		public static readonly ObservableCollection<CSound> listインスタンス = new ObservableCollection<CSound>();

		public static void ShowAllCSoundFiles()
		{
			int i = 0;
			foreach ( CSound cs in listインスタンス )
			{
				Debug.WriteLine( i++.ToString( "d3" ) + ": " + Path.GetFileName( cs.strファイル名 ) );
			}
		}

		public CSound(ESoundGroup soundGroup)
		{
			SoundGroup = soundGroup;
			this.n位置 = 0;
			this._db再生速度 = 1.0;
//			this._cbRemoveMixerChannel = new WaitCallback( RemoveMixerChannelLater );
			this._hBassStream = -1;
			this._hTempoStream = 0;
		}

		public void tASIOサウンドを作成する( string strファイル名, int hMixer )
		{
			this.eデバイス種別 = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strファイル名, hMixer, BASSFlag.BASS_STREAM_DECODE );
		}
		public void tASIOサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer )
		{
			this.eデバイス種別 = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BASSFlag.BASS_STREAM_DECODE );
		}
		public void tWASAPIサウンドを作成する( string strファイル名, int hMixer, ESoundDeviceType eデバイス種別 )
		{
			this.eデバイス種別 = eデバイス種別;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strファイル名, hMixer, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT );
		}
		public void tWASAPIサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer, ESoundDeviceType eデバイス種別 )
		{
			this.eデバイス種別 = eデバイス種別;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT );
		}
		public void tOpenALサウンドを作成する( string strファイル名 )
		{
			this.e作成方法 = E作成方法.ファイルから;
			this.strファイル名 = strファイル名;
			// すべてのファイルを FFmpeg でデコードすると時間がかかるので、ファイルが WAV かつ PCM フォーマットでない場合のみ FFmpeg でデコードする。

			byte[] byArrWAVファイルイメージ = null;
			bool bファイルがWAVかつPCMフォーマットである = true;

			{
				#region [ ファイルがWAVかつPCMフォーマットか否か調べる。]
				//-----------------
				try
				{
					using (var ws = new SoundStream(new FileStream(strファイル名, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
					{
						if (ws.Format.Encoding != WaveFormatEncoding.Pcm)
							bファイルがWAVかつPCMフォーマットである = false;
					}
				}
				catch
				{
					bファイルがWAVかつPCMフォーマットである = false;
				}
				//-----------------
				#endregion

				if (bファイルがWAVかつPCMフォーマットである)
				{
					#region [ ファイルを読み込んで byArrWAVファイルイメージへ格納。]
					//-----------------
					var fs = File.Open(strファイル名, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					var br = new BinaryReader(fs);

					byArrWAVファイルイメージ = new byte[fs.Length];
					br.Read(byArrWAVファイルイメージ, 0, (int)fs.Length);

					br.Close();
					fs.Close();
					//-----------------
					#endregion
				}
				else
				{
					try
					{
						this.e作成方法 = E作成方法.ファイルから;
						this.strファイル名 = strファイル名;

						int nPCMデータの先頭インデックス = 0;
						//			int nPCMサイズbyte = (int) ( xa.xaheader.nSamples * xa.xaheader.nChannels * 2 );	// nBytes = Bass.BASS_ChannelGetLength( this.hBassStream );

						int nPCMサイズbyte;
						CWin32.WAVEFORMATEX cw32wfx;
						tオンメモリ方式でデコードする(strファイル名, out this.byArrWAVファイルイメージ,
						out nPCMデータの先頭インデックス, out nPCMサイズbyte, out cw32wfx, false);

						// セカンダリバッファを作成し、PCMデータを書き込む。
						tOpenALサウンドを作成する_セカンダリバッファの作成とWAVデータ書き込み
							(ref this.byArrWAVファイルイメージ, cw32wfx, nPCMサイズbyte, nPCMデータの先頭インデックス);
						return;
					}
					catch (Exception e)
					{
						string s = Path.GetFileName(strファイル名);
						Trace.TraceWarning($"Failed to create OpenAL buffer by using libav({s}: {e.Message})");
					}
				}
			}

			// あとはあちらで。

			this.tOpenALサウンドを作成する(byArrWAVファイルイメージ);
		}

		public void tOpenALサウンドを作成する( byte[] byArrWAVファイルイメージ )
		{
			if( this.e作成方法 == E作成方法.Unknown )
				this.e作成方法 = E作成方法.WAVファイルイメージから;

			bool EnableData = false;
			CWin32.WAVEFORMATEX c32wfx = new CWin32.WAVEFORMATEX();
			int nPCMデータの先頭インデックス = -1;
			int nPCMサイズbyte = -1;
	
			#region [ byArrWAVファイルイメージ[] から上記３つのデータを取得。]
			//-----------------
			var ms = new MemoryStream( byArrWAVファイルイメージ );
			var br = new BinaryReader( ms );

			try
			{
				// 'RIFF'＋RIFFデータサイズ

				if( br.ReadUInt32() != 0x46464952 )
					throw new InvalidDataException( "RIFFファイルではありません。" );
				br.ReadInt32();

				// 'WAVE'
				if( br.ReadUInt32() != 0x45564157 )
					throw new InvalidDataException( "WAVEファイルではありません。" );

				// チャンク
				while( ( ms.Position + 8 ) < ms.Length )	// +8 は、チャンク名＋チャンクサイズ。残り8バイト未満ならループ終了。
				{
					uint chunkName = br.ReadUInt32();

					// 'fmt '
					if( chunkName == 0x20746D66 )
					{
						long chunkSize = (long) br.ReadUInt32();

						var tag = br.ReadInt16();
						int Channels = br.ReadInt16();
						int SamplesPerSecond = br.ReadInt32();
						int AverageBytesPerSecond = br.ReadInt32();
						int BlockAlignment = br.ReadInt16();
						int BitsPerSample = br.ReadInt16();


						if (tag == (short)WaveFormatEncoding.Pcm || tag == (short)WaveFormatEncoding.Extensible) EnableData = true;
						else
							throw new InvalidDataException(string.Format("未対応のWAVEフォーマットタグです。(Tag:{0})", tag.ToString()));

						c32wfx = new CWin32.WAVEFORMATEX((short)tag, (ushort)Channels, (uint)SamplesPerSecond, (uint)AverageBytesPerSecond, (ushort)BlockAlignment, (ushort)BitsPerSample);
						
						long nフォーマットサイズbyte = 16;

						if( tag == (short)WaveFormatEncoding.Extensible )
						{
							br.ReadUInt16();    // 拡張領域サイズbyte
							br.ReadInt16();//ValidBitsPerSample	読み捨て
							br.ReadInt32();//ChannelMask	読み捨て
							new Guid(br.ReadBytes(16)); // GUID は 16byte (128bit)	GuidSubFormat	読み捨て

							nフォーマットサイズbyte += 24;
						}

						ms.Seek( chunkSize - nフォーマットサイズbyte, SeekOrigin.Current );
						continue;
					}

					// 'data'
					else if( chunkName == 0x61746164 )
					{
						nPCMサイズbyte = br.ReadInt32();
						nPCMデータの先頭インデックス = (int) ms.Position;

						ms.Seek( nPCMサイズbyte, SeekOrigin.Current );
						continue;
					}

					// その他
					else
					{
						long chunkSize = (long) br.ReadUInt32();
						ms.Seek( chunkSize, SeekOrigin.Current );
						continue;
					}
				}

				if( !EnableData )
					throw new InvalidDataException( "fmt チャンクが存在しません。不正なサウンドデータです。" );
				if( nPCMサイズbyte < 0 )
					throw new InvalidDataException( "data チャンクが存在しません。不正なサウンドデータです。" );
			}
			finally
			{
				ms.Close();
				br.Close();
			}
			//-----------------
			#endregion


			// セカンダリバッファを作成し、PCMデータを書き込む。
			tOpenALサウンドを作成する_セカンダリバッファの作成とWAVデータ書き込み(
				ref byArrWAVファイルイメージ, c32wfx, nPCMサイズbyte, nPCMデータの先頭インデックス);
		}

		private void tOpenALサウンドを作成する_セカンダリバッファの作成とWAVデータ書き込み
			( ref byte[] byArrWAVファイルイメージ, CWin32.WAVEFORMATEX wfx,
			int nPCMサイズbyte, int nPCMデータの先頭インデックス )
		{
			byte[] tmp = new byte[byArrWAVファイルイメージ.Length - nPCMデータの先頭インデックス];
			Array.Copy(byArrWAVファイルイメージ, nPCMデータの先頭インデックス, tmp, 0, byArrWAVファイルイメージ.Length - nPCMデータの先頭インデックス);
			byArrWAVファイルイメージ = tmp;
			
			this.SourceOpen = new int[wfx.nChannels];
			this.BufferOpen = new int[wfx.nChannels];
			this.defaultPan = new float[wfx.nChannels];

			for (int i = 0; i < wfx.nChannels; i++)
			{
				this.SourceOpen[i] = AL.GenSource();
				this.BufferOpen[i] = AL.GenBuffer();
			}

			ALFormat alformat;
			if (wfx.wBitsPerSample == 8)
			{
				alformat = ALFormat.Mono8;
			}
			else
			{
				alformat = ALFormat.Mono16;
			}

			int BytesPerSample = (wfx.wBitsPerSample / 8);

			{
				for (int i = 0; i < wfx.nChannels; i++)
				{
					byte[] wavdat = new byte[byArrWAVファイルイメージ.Length / wfx.nChannels];
					for (int j = 0; j < wavdat.Length; j += BytesPerSample)
					{
						for (int k = 0; k < BytesPerSample; k++)
						{
							wavdat[j + k] = byArrWAVファイルイメージ[(j * wfx.nChannels) + (i * BytesPerSample) + k];
						}
					}


					AL.BufferData(this.BufferOpen[i], alformat, wavdat, wavdat.Length, (int)wfx.nSamplesPerSec);
					AL.BindBufferToSource(this.SourceOpen[i], this.BufferOpen[i]);
				}
			}

			switch (wfx.nChannels)//強制2Dパン(面倒くさいだけです。すみません。)
			{
				case 1://FC
					this.defaultPan[0] = 0;
					break;
				case 2://FL+FR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					break;
				case 3://FL+FR+FC
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = 0;
					break;
				case 4://FL+FR+BL+BR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = -1;
					this.defaultPan[3] = 1;
					break;
				case 5://FL+FR+FC+SL+SR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = 0;
					this.defaultPan[3] = -1;
					this.defaultPan[4] = 1;
					break;
				case 6://FL+FR+FC+BC+SL+SR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = 0;
					this.defaultPan[3] = 0;
					this.defaultPan[4] = -1;
					this.defaultPan[5] = 1;
					break;
				case 7://FL+FR+FC+BL+BR+SL+SR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = 0;
					this.defaultPan[3] = -1;
					this.defaultPan[4] = 1;
					this.defaultPan[5] = -1;
					this.defaultPan[6] = 1;
					break;
				case 8://FL+FR+FC+BL+BR+BC+SL+SR
					this.defaultPan[0] = -1;
					this.defaultPan[1] = 1;
					this.defaultPan[2] = 0;
					this.defaultPan[3] = -1;
					this.defaultPan[4] = 1;
					this.defaultPan[5] = 0;
					this.defaultPan[6] = -1;
					this.defaultPan[7] = 1;
					break;
			}
			// 作成完了。

			this.eデバイス種別 = ESoundDeviceType.OpenAL;
			this.byArrWAVファイルイメージ = byArrWAVファイルイメージ;

			// DTXMania用に追加
			this.nオリジナルの周波数 = (int)wfx.nSamplesPerSec;
			n総演奏時間ms = (int)(((double)nPCMサイズbyte) / (wfx.nAvgBytesPerSec * 0.001));

			for (int i = 0; i < wfx.nChannels; i++)
			{
				AL.Source(this.SourceOpen[i], ALSource3f.Position, defaultPan[i], 0f, 0f);//デフォルトパンの適用
			}

			// インスタンスリストに登録。

			CSound.listインスタンス.Add( this );
		}

		#region [ DTXMania用の変換 ]

		public void tサウンドを破棄する( CSound cs )
		{
			cs.t解放する();
		}
		public void t再生を開始する()
		{
			t再生位置を先頭に戻す();
			tサウンドを再生する();
		}
		public void t再生を開始する( bool bループする)
		{
			if ( bBASSサウンドである )
			{
				if ( bループする )
				{
					Bass.BASS_ChannelFlags(this.hBassStream, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
				}
				else
				{
					Bass.BASS_ChannelFlags( this.hBassStream, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_DEFAULT );
				}
			}
			t再生位置を先頭に戻す();
			tサウンドを再生する( bループする);
		}
		public void t再生を停止する()
		{
			tサウンドを停止する();
			t再生位置を先頭に戻す();
		}
		public void t再生を一時停止する()
		{
			tサウンドを停止する(true);
			this.n一時停止回数++;
		}
		public void t再生を再開する( long t )	// ★★★★★★★★★★★★★★★★★★★★★★★★★★★★
		{
			Debug.WriteLine( "t再生を再開する(long " + t + ")" );
			t再生位置を変更する( t );
			tサウンドを再生する();
			this.n一時停止回数--;
		}
		public bool b一時停止中
		{
			get
			{
				if ( this.bBASSサウンドである )
				{
					bool ret = ( BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) == BASSActive.BASS_ACTIVE_PAUSED ) &
								( BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) > 0 );
					return ret;
				}
				else
				{
					return ( this.n一時停止回数 > 0 );
				}
			}
		}
		public bool b再生中
		{
			get
			{
				if ( this.eデバイス種別 == ESoundDeviceType.OpenAL )
				{
					return AL.GetSourceState(SourceOpen[0]) == ALSourceState.Playing;//すべてのチャンネルで同期させているはずなので、0で取得
				}
				else
				{
					// 基本的にはBASS_ACTIVE_PLAYINGなら再生中だが、最後まで再生しきったchannelも
					// BASS_ACTIVE_PLAYINGのままになっているので、小細工が必要。
					bool ret = ( BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) == BASSActive.BASS_ACTIVE_PLAYING );
					if ( BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) >= nBytes )
					{
						ret = false;
					}
					return ret;
				}
			}
		}
		//public lint t時刻から位置を返す( long t )
		//{
		//    double num = ( n時刻 * this.db再生速度 ) * this.db周波数倍率;
		//    return (int) ( ( num * 0.01 ) * this.nSamplesPerSecond );
		//}
		#endregion


		public void t解放する()
		{
			t解放する( false );
		}

		public void t解放する( bool _bインスタンス削除 )
		{
			if ( this.bBASSサウンドである )		// stream数の削減用
			{
				tBASSサウンドをミキサーから削除する();
				_cbEndofStream = null;
				//_cbStreamXA = null;
				CSound管理.nStreams--;
			}
			bool bManagedも解放する = true;
			bool bインスタンス削除 = _bインスタンス削除;	// CSoundの再初期化時は、インスタンスは存続する。
			this.Dispose( bManagedも解放する, bインスタンス削除 );
//Debug.WriteLine( "Disposed: " + _bインスタンス削除 + " : " + Path.GetFileName( this.strファイル名 ) );
		}
		public void tサウンドを再生する()
		{
			if (!b速度上げすぎ問題)
				tサウンドを再生する(false);
		}
		private void tサウンドを再生する( bool bループする)
		{
			if ( this.bBASSサウンドである )			// BASSサウンド時のループ処理は、t再生を開始する()側に実装。ここでは「bループする」は未使用。
			{
//Debug.WriteLine( "再生中?: " +  System.IO.Path.GetFileName(this.strファイル名) + " status=" + BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) + " current=" + BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) + " nBytes=" + nBytes );
				bool b = BassMix.BASS_Mixer_ChannelPlay( this.hBassStream );
				if ( !b )
				{
//Debug.WriteLine( "再生しようとしたが、Mixerに登録されていなかった: " + Path.GetFileName( this.strファイル名 ) + ", stream#=" + this.hBassStream + ", ErrCode=" + Bass.BASS_ErrorGetCode() );

					bool bb = tBASSサウンドをミキサーに追加する();
					if ( !bb )
					{
Debug.WriteLine( "Mixerへの登録に失敗: " + Path.GetFileName( this.strファイル名 ) + ", ErrCode=" + Bass.BASS_ErrorGetCode() );
					}
					else
					{
//Debug.WriteLine( "Mixerへの登録に成功: " + Path.GetFileName( this.strファイル名 ) + ": " + Bass.BASS_ErrorGetCode() );
					}
					//this.t再生位置を先頭に戻す();

					bool bbb = BassMix.BASS_Mixer_ChannelPlay( this.hBassStream );
					if (!bbb)
					{
Debug.WriteLine("更に再生に失敗: " + Path.GetFileName(this.strファイル名) + ", ErrCode=" + Bass.BASS_ErrorGetCode() );
					}
					else
					{
//						Debug.WriteLine("再生成功(ミキサー追加後)                       : " + Path.GetFileName(this.strファイル名));
					}
				}
				else
				{
//Debug.WriteLine( "再生成功: " + Path.GetFileName( this.strファイル名 ) + " (" + hBassStream + ")" );
				}
			}
			else if( this.bOpenALである )
			{
				for (int i = 0; i < this.SourceOpen.Length; i++)
				{
					AL.Source(this.SourceOpen[i], ALSourceb.Looping, bループする);
					AL.SourcePlay(this.SourceOpen[i]);
				}
			}
		}
		public void tサウンドを停止してMixerからも削除する()
		{
			tサウンドを停止する( false );
			if ( bBASSサウンドである )
			{
				tBASSサウンドをミキサーから削除する();
			}
		}
		public void tサウンドを停止する()
		{
			tサウンドを停止する( false );
		}
		public void tサウンドを停止する( bool pause )
		{
			if( this.bBASSサウンドである )
			{
//Debug.WriteLine( "停止: " + System.IO.Path.GetFileName( this.strファイル名 ) + " status=" + BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) + " current=" + BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) + " nBytes=" + nBytes );
				BassMix.BASS_Mixer_ChannelPause( this.hBassStream );
				if ( !pause )
				{
			//		tBASSサウンドをミキサーから削除する();		// PAUSEと再生停止を区別できるようにすること!!
				}
			}
			else if( this.bOpenALである )
			{
				for (int i = 0; i < this.SourceOpen.Length; i++)
				{
					AL.SourceStop(this.SourceOpen[i]);
				}
			}
			this.n一時停止回数 = 0;
		}
		
		public void t再生位置を先頭に戻す()
		{
			if( this.bBASSサウンドである )
			{
				BassMix.BASS_Mixer_ChannelSetPosition( this.hBassStream, 0 );
				//pos = 0;
			}
			else if( this.bOpenALである )
			{
				for (int i = 0; i < this.SourceOpen.Length; i++)
				{
					AL.Source(this.SourceOpen[i], ALSourcef.SecOffset, 0f);
				}
			}
		}
		public void t再生位置を変更する( long n位置ms )
		{
			if( this.bBASSサウンドである )
			{
				bool b = true;
				try
				{
					b = BassMix.BASS_Mixer_ChannelSetPosition( this.hBassStream, Bass.BASS_ChannelSeconds2Bytes( this.hBassStream, n位置ms * _db再生速度 / 1000.0 ), BASSMode.BASS_POS_BYTES );
				}
				catch( Exception e )
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceInformation( Path.GetFileName( this.strファイル名 ) + ": Seek error: " + e.ToString() + ": " + n位置ms + "ms" );
				}
				finally
				{
					if ( !b )
					{
						BASSError be = Bass.BASS_ErrorGetCode();
						Trace.TraceInformation( Path.GetFileName( this.strファイル名 ) + ": Seek error: " + be.ToString() + ": " + n位置ms + "MS" );
					}
				}
				//if ( this.n総演奏時間ms > 5000 )
				//{
				//    Trace.TraceInformation( Path.GetFileName( this.strファイル名 ) + ": Seeked to " + n位置ms + "ms = " + Bass.BASS_ChannelSeconds2Bytes( this.hBassStream, n位置ms * this.db周波数倍率 * this.db再生速度 / 1000.0 ) );
				//}
			}
			else if( this.bOpenALである )
			{
				try
				{
					for (int i = 0; i < this.SourceOpen.Length; i++)
					{
						AL.Source(this.SourceOpen[i], ALSourcef.SecOffset, (float)(n位置ms * 0.001f * this.db再生速度));
					}
				}
				catch
				{
					Trace.TraceError("{0}: Seek error: {1}", Path.GetFileName(this.strファイル名), n位置ms);
					Trace.TraceError("例外が発生しましたが処理を継続します。 (95dee242-1f92-4fcf-aaf6-b162ad2bfc03)");
				}
			}
		}
		/// <summary>
		/// デバッグ用
		/// </summary>
		/// <param name="n位置byte"></param>
		/// <param name="db位置ms"></param>
		public void t再生位置を取得する( out long n位置byte, out double db位置ms )
		{
			if ( this.bBASSサウンドである )
			{
				n位置byte = BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream );
				db位置ms = Bass.BASS_ChannelBytes2Seconds( this.hBassStream, n位置byte );
			}
			else if ( this.bOpenALである )
			{
				//すべてのチャンネルで長さは同じはず0で取得する
				AL.GetSource(this.SourceOpen[0], ALGetSourcei.ByteOffset, out int n位置bytei);
				n位置byte = (long)n位置bytei;
				AL.GetSource(this.SourceOpen[0], ALSourcef.SecOffset, out float ms);

				db位置ms = ms / _db再生速度;
			}
			else
			{
				n位置byte = 0;
				db位置ms = 0.0;
			}
		}


		public static void tすべてのサウンドを初期状態に戻す()
		{
			foreach ( var sound in CSound.listインスタンス )
			{
				sound.t解放する( false );
			}
		}
		internal static void tすべてのサウンドを再構築する( ISoundDevice device )
		{
			if( CSound.listインスタンス.Count == 0 )
				return;


			// サウンドを再生する際にインスタンスリストも更新されるので、配列にコピーを取っておき、リストはクリアする。

			var sounds = CSound.listインスタンス.ToArray();
			CSound.listインスタンス.Clear();
			

			// 配列に基づいて個々のサウンドを作成する。

			for( int i = 0; i < sounds.Length; i++ )
			{
				switch( sounds[ i ].e作成方法 )
				{
					#region [ ファイルから ]
					case E作成方法.ファイルから:
						string strファイル名 = sounds[ i ].strファイル名;
						sounds[ i ].Dispose( true, false );
						device.tサウンドを作成する( strファイル名, sounds[ i ] );
						break;
					#endregion
					#region [ WAVファイルイメージから ]
					case E作成方法.WAVファイルイメージから:
						if( sounds[ i ].bBASSサウンドである )
						{
							byte[] byArrWaveファイルイメージ = sounds[ i ].byArrWAVファイルイメージ;
							sounds[ i ].Dispose( true, false );
							device.tサウンドを作成する( byArrWaveファイルイメージ, sounds[ i ] );
						}
						else if( sounds[ i ].bOpenALである )
						{
							byte[] byArrWaveファイルイメージ = sounds[ i ].byArrWAVファイルイメージ;
							sounds[ i ].Dispose( true, false );
							( (CSoundDeviceOpenAL) device ).tサウンドを作成する( byArrWaveファイルイメージ, sounds[ i ] );
						}
						break;
					#endregion
				}
			}
		}

		#region [ Dispose-Finalizeパターン実装 ]
		//-----------------
		public void Dispose()
		{
			this.Dispose( true, true );
			GC.SuppressFinalize( this );
		}
		private void Dispose( bool bManagedも解放する, bool bインスタンス削除 )
		{
			if( this.bBASSサウンドである )
			{
				#region [ ASIO, WASAPI の解放 ]
				//-----------------
				if ( _hTempoStream != 0 )
				{
					BassMix.BASS_Mixer_ChannelRemove( this._hTempoStream );
					Bass.BASS_StreamFree( this._hTempoStream );
				}
				BassMix.BASS_Mixer_ChannelRemove( this._hBassStream );
				Bass.BASS_StreamFree( this._hBassStream );
				this.hBassStream = -1;
				this._hBassStream = -1;
				this._hTempoStream = 0;
				//-----------------
				#endregion
			}

			if( bManagedも解放する )
			{
				//int freeIndex = -1;

				//if ( CSound.listインスタンス != null )
				//{
				//    freeIndex = CSound.listインスタンス.IndexOf( this );
				//    if ( freeIndex == -1 )
				//    {
				//        Debug.WriteLine( "ERR: freeIndex==-1 : Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strファイル名 ) );
				//    }
				//}

				if( this.eデバイス種別 == ESoundDeviceType.OpenAL )
				{
					#region [ OpenAL の解放 ]
					//-----------------
					for (int i = 0; i < this.SourceOpen.Length; i++)
					{
						AL.SourceStop(this.SourceOpen[i]);
					}

					for (int i = 0; i < this.SourceOpen.Length; i++)//SourceOpenとBufferOpenは同じ長さでないといけない
					{
						AL.DeleteSource(this.SourceOpen[i]);
						AL.DeleteBuffer(this.BufferOpen[i]);
					}
					//-----------------
					#endregion
				}

				if( this.e作成方法 == E作成方法.WAVファイルイメージから &&
					this.eデバイス種別 != ESoundDeviceType.OpenAL )	// OpenAL は hGC 未使用。
				{
					if ( this.hGC != null && this.hGC.IsAllocated )
					{
						this.hGC.Free();
						this.hGC = default( GCHandle );
					}
				}
				if ( this.byArrWAVファイルイメージ != null )
				{
					this.byArrWAVファイルイメージ = null;
				}

				this.eデバイス種別 = ESoundDeviceType.Unknown;

				if ( bインスタンス削除 )
				{
					//try
					//{
					//    CSound.listインスタンス.RemoveAt( freeIndex );
					//}
					//catch
					//{
					//    Debug.WriteLine( "FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strファイル名 ) );
					//}
					bool b = CSound.listインスタンス.Remove( this );	// これだと、Clone()したサウンドのremoveに失敗する
					if ( !b )
					{
						Debug.WriteLine( "FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strファイル名 ) );
					}

				}
			}
		}
		~CSound()
		{
			this.Dispose( false, true );
		}
		//-----------------
		#endregion

		#region [ protected ]
		//-----------------
		protected enum E作成方法 { ファイルから, WAVファイルイメージから, Unknown }
		protected E作成方法 e作成方法 = E作成方法.Unknown;
		protected ESoundDeviceType eデバイス種別 = ESoundDeviceType.Unknown;
		public string strファイル名 = null;
		protected byte[] byArrWAVファイルイメージ = null;	// WAVファイルイメージ、もしくはchunkのDATA部のみ
		protected GCHandle hGC;
		protected int _hTempoStream = 0;
		protected int _hBassStream = -1;					// ASIO, WASAPI 用
		protected int hBassStream = 0;						// #31076 2013.4.1 yyagi; プロパティとして実装すると動作が低速になったため、
															// tBASSサウンドを作成する_ストリーム生成後の共通処理()のタイミングと、
															// 再生速度を変更したタイミングでのみ、
															// hBassStreamを更新するようにした。
		//{
		//    get
		//    {
		//        if ( _hTempoStream != 0 && !this.bIs1倍速再生 )	// 再生速度がx1.000のときは、TempoStreamを用いないようにして高速化する
		//        {
		//            return _hTempoStream;
		//        }
		//        else
		//        {
		//            return _hBassStream;
		//        }
		//    }
		//    set
		//    {
		//        _hBassStream = value;
		//    }
		//}
		protected int hMixer = -1;	// 設計壊してゴメン Mixerに後で登録するときに使う
		//-----------------
		#endregion

		#region [ private ]
		//-----------------
		private bool bOpenALである
		{
			get { return ( this.eデバイス種別 == ESoundDeviceType.OpenAL ); }
		}
		private bool bBASSサウンドである
		{
			get
			{
				return (
					this.eデバイス種別 == ESoundDeviceType.ASIO ||
					this.eデバイス種別 == ESoundDeviceType.ExclusiveWASAPI ||
					this.eデバイス種別 == ESoundDeviceType.SharedWASAPI );
			}
		}
		public int[] BufferOpen;
		public int[] SourceOpen;
		public float[] defaultPan;
		private int _n位置 = 0;
		private Lufs _gain = DefaultGain;
		private Lufs? _truePeak = null;
		private int _automationLevel = DefaultAutomationLevel;
		private int _groupLevel = DefaultGroupLevel;
		private long nBytes = 0;
		private int n一時停止回数 = 0;
		private int nオリジナルの周波数 = 0;
		private double _db再生速度 = 1.0;
		private bool bIs1倍速再生 = true;

		private void tBASSサウンドを作成する( string strファイル名, int hMixer, BASSFlag flags )
		{
			#region [ wav(RIFF chunked vorbis)に対しては専用の処理をする ]
			switch ( Path.GetExtension( strファイル名 ).ToLower() )
			{
				case ".wav":
					if ( tRIFFchunkedVorbisならFFmpegでDecodeする( strファイル名, ref byArrWAVファイルイメージ ) )
					{
						tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, flags );
						return;
					}
					break;

				default:
					break;
			}
			#endregion

			this.e作成方法 = E作成方法.ファイルから;
			this.strファイル名 = strファイル名;


			// BASSファイルストリームを作成。

			this._hBassStream = Bass.BASS_StreamCreateFile( strファイル名, 0, 0, flags );
			if (this._hBassStream == 0) 
			{
				//ファイルからのサウンド生成に失敗した場合にデコードする。(時間がかかるのはしょうがないね)
				tオンメモリ方式でデコードする(strファイル名, out byArrWAVファイルイメージ, out _, out _, out _, true);
				tBASSサウンドを作成する(byArrWAVファイルイメージ, hMixer, flags);
				return;
			}
			
			nBytes = Bass.BASS_ChannelGetLength( this._hBassStream );
			
			tBASSサウンドを作成する_ストリーム生成後の共通処理( hMixer );
		}
		private void tBASSサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer, BASSFlag flags )
		{
			this.e作成方法 = E作成方法.WAVファイルイメージから;
			this.byArrWAVファイルイメージ = byArrWAVファイルイメージ;
			this.hGC = GCHandle.Alloc( byArrWAVファイルイメージ, GCHandleType.Pinned );		// byte[] をピン留め


			// BASSファイルストリームを作成。

			this._hBassStream = Bass.BASS_StreamCreateFile( hGC.AddrOfPinnedObject(), 0, byArrWAVファイルイメージ.Length, flags );
			if ( this._hBassStream == 0 )
				throw new Exception( string.Format( "サウンドストリームの生成に失敗しました。(BASS_StreamCreateFile)[{0}]", Bass.BASS_ErrorGetCode().ToString() ) );

			nBytes = Bass.BASS_ChannelGetLength( this._hBassStream );
	
			tBASSサウンドを作成する_ストリーム生成後の共通処理( hMixer );
		}

		/// <summary>
		/// Decode "RIFF chunked Vorbis" to "raw wave"
		/// because BASE.DLL has two problems for RIFF chunked Vorbis;
		/// 1. time seek is not fine  2. delay occurs (about 10ms)
		/// </summary>
		/// <param name="strファイル名">wave filename</param>
		/// <param name="byArrWAVファイルイメージ">wav file image</param>
		/// <returns></returns>
		private bool tRIFFchunkedVorbisならFFmpegでDecodeする( string strファイル名, ref byte[] byArrWAVファイルイメージ )
		{
			bool bファイルにVorbisコンテナが含まれている = false;

			#region [ ファイルがWAVかつ、Vorbisコンテナが含まれているかを調べ、それに該当するなら、FFmpegでデコードする。]
			//-----------------
			try
			{
				Stream str = File.Open(strファイル名, FileMode.Open, FileAccess.Read);
				using ( var ws = new SoundStream( str ) )
				{
					if ( ws.Format.Encoding == WaveFormatEncoding.OggVorbisMode2Plus ||	// Ogg Vorbis Mode 2+
						 ws.Format.Encoding == WaveFormatEncoding.OggVorbisMode3Plus)	// Ogg Vorbis Mode 3+
					{
						Trace.TraceInformation( Path.GetFileName( strファイル名 ) + ": RIFF chunked Vorbis. Decode to raw Wave first, to avoid BASS.DLL troubles" );
						try
						{
							tオンメモリ方式でデコードする(strファイル名, out byArrWAVファイルイメージ, out _, out _, out _, true);
							bファイルにVorbisコンテナが含まれている = true;
						}
						catch
						{
							Trace.TraceWarning( "Warning: " + Path.GetFileName( strファイル名 ) + " : RIFF chunked Vorbisのデコードに失敗しました。" );
						}
					}
				}
			}
			catch ( InvalidDataException )
			{
				// DirectShowのデコードに失敗したら、次はACMでのデコードを試すことになるため、ここではエラーログを出さない。
				// Trace.TraceWarning( "Warning: " + Path.GetFileName( strファイル名 ) + " : デコードに失敗しました。" );
			}
			catch ( Exception e )
			{
				Trace.TraceWarning( e.ToString() );
				Trace.TraceWarning( "Warning: " + Path.GetFileName( strファイル名 ) + " : 読み込みに失敗しました。" );
			}
			#endregion

			return bファイルにVorbisコンテナが含まれている;
		}

		private void tBASSサウンドを作成する_ストリーム生成後の共通処理( int hMixer )
		{
			CSound管理.nStreams++;

			// 個々のストリームの出力をテンポ変更のストリームに入力する。テンポ変更ストリームの出力を、Mixerに出力する。

//			if ( CSound管理.bIsTimeStretch )	// TimeStretchのON/OFFに関わりなく、テンポ変更のストリームを生成する。後からON/OFF切り替え可能とするため。
			{
				this._hTempoStream = BassFx.BASS_FX_TempoCreate( this._hBassStream, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE );
				if ( this._hTempoStream == 0 )
				{
					hGC.Free();
					throw new Exception( string.Format( "サウンドストリームの生成に失敗しました。(BASS_FX_TempoCreate)[{0}]", Bass.BASS_ErrorGetCode().ToString() ) );
				}
				else
				{
					Bass.BASS_ChannelSetAttribute( this._hTempoStream, BASSAttribute.BASS_ATTRIB_TEMPO_OPTION_USE_QUICKALGO, 1f );	// 高速化(音の品質は少し落ちる)
				}
			}

			if ( _hTempoStream != 0 && !this.bIs1倍速再生 )	// 再生速度がx1.000のときは、TempoStreamを用いないようにして高速化する
			{
				this.hBassStream = _hTempoStream;
			}
			else
			{
				this.hBassStream = _hBassStream;
			}

			// #32248 再生終了時に発火するcallbackを登録する (演奏終了後に再生終了するチップを非同期的にミキサーから削除するため。)
			_cbEndofStream = new SYNCPROC( CallbackEndofStream );
			Bass.BASS_ChannelSetSync( hBassStream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, _cbEndofStream, IntPtr.Zero );

			// n総演奏時間の取得; DTXMania用に追加。
			double seconds = Bass.BASS_ChannelBytes2Seconds( this._hBassStream, nBytes );
			this.n総演奏時間ms = (int) ( seconds * 1000 );
			//this.pos = 0;
			this.hMixer = hMixer;
			float freq = 0.0f;
			if ( !Bass.BASS_ChannelGetAttribute( this._hBassStream, BASSAttribute.BASS_ATTRIB_FREQ, ref freq ) )
			{
				hGC.Free();
				throw new Exception( string.Format( "サウンドストリームの周波数取得に失敗しました。(BASS_ChannelGetAttribute)[{0}]", Bass.BASS_ErrorGetCode().ToString() ) );
			}
			this.nオリジナルの周波数 = (int) freq;

			// インスタンスリストに登録。

			CSound.listインスタンス.Add( this );
		}
		//-----------------

		//private int pos = 0;
		//private int CallbackPlayingXA( int handle, IntPtr buffer, int length, IntPtr user )
		//{
		//    int bytesread = ( pos + length > Convert.ToInt32( nBytes ) ) ? Convert.ToInt32( nBytes ) - pos : length;

		//    Marshal.Copy( byArrWAVファイルイメージ, pos, buffer, bytesread );
		//    pos += bytesread;
		//    if ( pos >= nBytes )
		//    {
		//        // set indicator flag
		//        bytesread |= (int) BASSStreamProc.BASS_STREAMPROC_END;
		//    }
		//    return bytesread;
		//}
		/// <summary>
		/// ストリームの終端まで再生したときに呼び出されるコールバック
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="channel"></param>
		/// <param name="data"></param>
		/// <param name="user"></param>
		private void CallbackEndofStream( int handle, int channel, int data, IntPtr user )	// #32248 2013.10.14 yyagi
		{
// Trace.TraceInformation( "Callback!(remove): " + Path.GetFileName( this.strファイル名 ) );
			if ( b演奏終了後も再生が続くチップである )			// 演奏終了後に再生終了するチップ音のミキサー削除は、再生終了のコールバックに引っ掛けて、自前で行う。
			{													// そうでないものは、ミキサー削除予定時刻に削除する。
				tBASSサウンドをミキサーから削除する( channel );
			}
		}

// mixerからの削除

		public bool tBASSサウンドをミキサーから削除する()
		{
			return tBASSサウンドをミキサーから削除する( this.hBassStream );
		}
		public bool tBASSサウンドをミキサーから削除する( int channel )
		{
			bool b = BassMix.BASS_Mixer_ChannelRemove( channel );
			if ( b )
			{
				Interlocked.Decrement( ref CSound管理.nMixing );
//				Debug.WriteLine( "Removed: " + Path.GetFileName( this.strファイル名 ) + " (" + channel + ")" + " MixedStreams=" + CSound管理.nMixing );
			}
			return b;
		}


		// mixer への追加
			public bool tBASSサウンドをミキサーに追加する()
		{
			if ( BassMix.BASS_Mixer_ChannelGetMixer( hBassStream ) == 0 )
			{
				BASSFlag bf = BASSFlag.BASS_SPEAKER_FRONT | BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_PAUSE;
				Interlocked.Increment( ref CSound管理.nMixing );

				// preloadされることを期待して、敢えてflagからはBASS_MIXER_PAUSEを外してAddChannelした上で、すぐにPAUSEする
				// -> ChannelUpdateでprebufferできることが分かったため、BASS_MIXER_PAUSEを使用することにした
				bool b1 = BassMix.BASS_Mixer_StreamAddChannel( this.hMixer, this.hBassStream, bf );
				//bool b2 = BassMix.BASS_Mixer_ChannelPause( this.hBassStream );
				t再生位置を先頭に戻す();	// StreamAddChannelの後で再生位置を戻さないとダメ。逆だと再生位置が変わらない。
//Trace.TraceInformation( "Add Mixer: " + Path.GetFileName( this.strファイル名 ) + " (" + hBassStream + ")" + " MixedStreams=" + CSound管理.nMixing );
				Bass.BASS_ChannelUpdate( this.hBassStream, 0 );	// pre-buffer
				return b1;	// &b2;
			}
			return true;
		}

		#region [ tオンメモリ方式でデコードする() ]
		public void tオンメモリ方式でデコードする(string strファイル名, out byte[] buffer,
			out int nPCMデータの先頭インデックス, out int totalPCMSize, out CWin32.WAVEFORMATEX wfx, bool enablechunk)
		{
			nPCMデータの先頭インデックス = 0;

			CAudioDecoder sounddecoder = new CAudioDecoder();

			if ( !File.Exists( strファイル名 ) )
				throw new FileNotFoundException( string.Format( "File Not Found...({0})", strファイル名 ) );

			//丸投げ
			int rtn = sounddecoder.AudioDecode(strファイル名, out buffer, out nPCMデータの先頭インデックス, out totalPCMSize, out wfx, enablechunk);

			//正常にDecodeできなかった場合、例外
			if ( rtn < 0 )
				throw new Exception( string.Format( "Decoded Failed...({0})({1})", rtn, strファイル名 ) );
			
			sounddecoder = null;
			
		}
		#endregion
		#endregion
	}
}
