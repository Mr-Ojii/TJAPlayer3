using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading;
using FDK.ExtensionMethods;
using FDK.BassMixExtension;
using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Fx;

namespace FDK
{
	// CSound は、サウンドデバイスが変更されたときも、インスタンスを再作成することなく、新しいデバイスで作り直せる必要がある。
	// そのため、デバイスごとに別のクラスに分割するのではなく、１つのクラスに集約するものとする。

	public class CSound : IDisposable
	{
		public const ushort PCM = 1;
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

		public int nDurationms
		{
			get;
			private set;
		}
		public double dbPlaySpeed
		{
			get
			{
				return _dbPlaySpeed;
			}
			set
			{
				if (_dbPlaySpeed != value)
				{
					_dbPlaySpeed = value;
					bIs1倍速再生 = (_dbPlaySpeed == 1.000f);

					if (_hTempoStream != 0 && !this.bIs1倍速再生)   // PlaySpeedがx1.000のときは、TempoStreamを用いないようにして高速化する
					{
						this.hBassStream = _hTempoStream;
					}
					else
					{
						this.hBassStream = _hBassStream;
					}

					if (CSoundManager.bIsTimeStretch)
					{
						Bass.ChannelSetAttribute(this.hBassStream, ChannelAttribute.Tempo, (float)(_dbPlaySpeed * 100 - 100));
						//double seconds = Bass.BASS_ChannelBytes2Seconds( this.hTempoStream, nBytes );
						//this.n総演奏時間ms = (int) ( seconds * 1000 );
					}
					else
					{
						Bass.ChannelSetAttribute(this.hBassStream, ChannelAttribute.Frequency, (float)(_dbPlaySpeed * nFrequency));
					}
				}
			}
		}
		#endregion
		public bool b演奏終了後も再生が続くチップである = false;	// これがtrueなら、本サウンドの再生終了のコールバック時に自動でミキサーから削除する

		//private STREAMPROC _cbStreamXA;		// make it global, so that the GC can not remove it
		private SyncProcedure _cbEndofStream;	// ストリームの終端まで再生されたときに呼び出されるコールバック
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

			lufsVolume = finalGain;
		}

		private Lufs lufsVolume
		{
			set
			{
				var dbVolume = ((value.ToDouble() / 100.0) + 1.0).Clamp(0, 1);
				Bass.ChannelSetAttribute(this.hBassStream, ChannelAttribute.Volume, (float)dbVolume);
			}
		}

		/// <summary>
		/// <para>左:-100～中央:0～100:右。set のみ。</para>
		/// </summary>
		public int nPanning
		{
			get
			{
				float fPan = 0.0f;
				if (!Bass.ChannelGetAttribute(this.hBassStream, ChannelAttribute.Pan, out fPan))
					return 0;
				return (int)(fPan * 100);
			}
			set
			{
				float fPan = Math.Min(Math.Max(value, -100), 100) / 100.0f; // -100～100 → -1.0～1.0
				Bass.ChannelSetAttribute(this.hBassStream, ChannelAttribute.Pan, fPan);
			}
		}

		/// <summary>
		/// <para>全インスタンスリスト。</para>
		/// <para>～を作成する() で追加され、t解放する() or Dispose() で解放される。</para>
		/// </summary>
		public static readonly ObservableCollection<CSound> listインスタンス = new ObservableCollection<CSound>();

		public CSound(ESoundGroup soundGroup)
		{
			SoundGroup = soundGroup;
			this.nPanning = 0;
			this._dbPlaySpeed = 1.0;
//			this._cbRemoveMixerChannel = new WaitCallback( RemoveMixerChannelLater );
			this._hBassStream = -1;
			this._hTempoStream = 0;
		}

		public void tBASSサウンドを作成する(string strFilename, int hMixer)
		{
			this.eSoundDeviceType = ESoundDeviceType.BASS;      // 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する(strFilename, hMixer, BassFlags.Decode);
		}
		public void tBASSサウンドを作成する(byte[] byArrWAVファイルイメージ, int hMixer)
		{
			this.eSoundDeviceType = ESoundDeviceType.BASS;      // 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する(byArrWAVファイルイメージ, hMixer, BassFlags.Decode);
		}
		public void tASIOサウンドを作成する( string strFilename, int hMixer )
		{
			this.eSoundDeviceType = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strFilename, hMixer, BassFlags.Decode );
		}
		public void tASIOサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer )
		{
			this.eSoundDeviceType = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BassFlags.Decode );
		}
		public void tWASAPIサウンドを作成する( string strFilename, int hMixer, ESoundDeviceType eSoundDeviceType )
		{
			this.eSoundDeviceType = eSoundDeviceType;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strFilename, hMixer, BassFlags.Decode | BassFlags.Float );
		}
		public void tWASAPIサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer, ESoundDeviceType eSoundDeviceType )
		{
			this.eSoundDeviceType = eSoundDeviceType;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BassFlags.Decode | BassFlags.Float );
		}

		#region [ DTXMania用の変換 ]

		public void t再生を開始する()
		{
			t再生位置を先頭に戻す();
			tサウンドを再生する();
		}
		public void t再生を開始する(bool bループする)
		{
			if (bループする)
			{
				Bass.ChannelFlags(this.hBassStream, BassFlags.Loop, BassFlags.Loop);
			}
			else
			{
				Bass.ChannelFlags(this.hBassStream, BassFlags.Default, BassFlags.Default);
			}
			t再生位置を先頭に戻す();
			tサウンドを再生する(bループする);
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
				bool ret = (!BassMixExtensions.ChannelIsPlaying(this.hBassStream)) &
							(BassMix.ChannelGetPosition(this.hBassStream) > 0);
				return ret;
			}
		}
		public bool bPlaying
		{
			get
			{
				// 基本的にはBASS_ACTIVE_PLAYINGなら再生中だが、最後まで再生しきったchannelも
				// BASS_ACTIVE_PLAYINGのままになっているので、小細工が必要。
				bool ret = (BassMixExtensions.ChannelIsPlaying(this.hBassStream));
				if (BassMix.ChannelGetPosition(this.hBassStream) >= nBytes)
				{
					ret = false;
				}
				return ret;
			}
		}
		#endregion


		public void t解放する()
		{
			t解放する( false );
		}

		public void t解放する(bool _bインスタンス削除)
		{
			tBASSサウンドをミキサーから削除する();
			_cbEndofStream = null;
			CSoundManager.nStreams--;

			this.Dispose(true, _bインスタンス削除);   // CSoundの再初期化時は、インスタンスは存続する。
		}
		public void tサウンドを再生する()
		{
			tサウンドを再生する(false);
		}
		private void tサウンドを再生する(bool bループする)
		{
			// BASSサウンド時のループ処理は、t再生を開始する()側に実装。ここでは「bループする」は未使用。

			//Debug.WriteLine( "再生中?: " +  System.IO.Path.GetFileName(this.strFilename) + " status=" + BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) + " current=" + BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) + " nBytes=" + nBytes );
			bool b = BassMixExtensions.ChannelPlay(this.hBassStream);
			if (!b)
			{
				//Debug.WriteLine( "再生しようとしたが、Mixerに登録されていなかった: " + Path.GetFileName( this.strFilename ) + ", stream#=" + this.hBassStream + ", ErrCode=" + Bass.BASS_ErrorGetCode() );

				bool bb = tBASSサウンドをミキサーに追加する();
				if (!bb)
				{
					Debug.WriteLine("Mixerへの登録に失敗: " + Path.GetFileName(this.strFilename) + ", ErrCode=" + Bass.LastError);
				}
				else
				{
					//Debug.WriteLine( "Mixerへの登録に成功: " + Path.GetFileName( this.strFilename ) + ": " + Bass.BASS_ErrorGetCode() );
				}
				//this.t再生位置を先頭に戻す();

				bool bbb = BassMixExtensions.ChannelPlay(this.hBassStream);
				if (!bbb)
				{
					Debug.WriteLine("更に再生に失敗: " + Path.GetFileName(this.strFilename) + ", ErrCode=" + Bass.LastError);
				}
				else
				{
					//Debug.WriteLine("再生成功(ミキサー追加後): " + Path.GetFileName(this.strFilename));
				}
			}
			else
			{
				//Debug.WriteLine( "再生成功: " + Path.GetFileName( this.strFilename ) + " (" + hBassStream + ")" );
			}
		}
		public void tサウンドを停止してMixerからも削除する()
		{
			tサウンドを停止する(false);
			tBASSサウンドをミキサーから削除する();
		}
		public void tサウンドを停止する()
		{
			tサウンドを停止する( false );
		}
		public void tサウンドを停止する(bool pause)
		{
			BassMixExtensions.ChannelPause(this.hBassStream);
			this.n一時停止回数 = 0;
		}

		public void t再生位置を先頭に戻す()
		{
			BassMix.ChannelSetPosition(this.hBassStream, 0);
		}
		public void t再生位置を変更する(long n位置ms)
		{
			bool b = true;
			try
			{
				b = BassMix.ChannelSetPosition(this.hBassStream, Bass.ChannelSeconds2Bytes(this.hBassStream, n位置ms * _dbPlaySpeed / 1000.0), PositionFlags.Bytes);
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceInformation(Path.GetFileName(this.strFilename) + ": Seek error: " + e.ToString() + ": " + n位置ms + "ms");
			}
			finally
			{
				if (!b)
				{
					Errors be = Bass.LastError;
					Trace.TraceInformation(Path.GetFileName(this.strFilename) + ": Seek error: " + be.ToString() + ": " + n位置ms + "MS");
				}
			}
		}
		/// <summary>
		/// デバッグ用
		/// </summary>
		/// <param name="n位置byte"></param>
		/// <param name="db位置ms"></param>
		public void t再生位置を取得する(out long n位置byte, out double db位置ms)
		{
			n位置byte = BassMix.ChannelGetPosition(this.hBassStream);
			db位置ms = Bass.ChannelBytes2Seconds(this.hBassStream, n位置byte);
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
				switch (sounds[i].e作成方法)
				{
					#region [ ファイルから ]
					case E作成方法.ファイルから:
						string strFilename = sounds[i].strFilename;
						sounds[i].Dispose(true, false);
						device.tCreateSound(strFilename, sounds[i]);
						break;
					#endregion
					#region [ WAVファイルイメージから ]
					case E作成方法.WAVファイルイメージから:
						byte[] byArrWaveファイルイメージ = sounds[i].byArrWAVファイルイメージ;
						sounds[i].Dispose(true, false);
						device.tCreateSound(byArrWaveファイルイメージ, sounds[i]);
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
		private void Dispose(bool bManagedも解放する, bool bインスタンス削除)
		{
			#region [ Stream の解放 ]
			//-----------------
			if (_hTempoStream != 0)
			{
				BassMix.MixerRemoveChannel(this._hTempoStream);
				Bass.StreamFree(this._hTempoStream);
			}
			BassMix.MixerRemoveChannel(this._hBassStream);
			Bass.StreamFree(this._hBassStream);
			this.hBassStream = -1;
			this._hBassStream = -1;
			this._hTempoStream = 0;
			//-----------------
			#endregion

			if (bManagedも解放する)
			{
				if (this.e作成方法 == E作成方法.WAVファイルイメージから)
				{
					if (this.hGC.IsAllocated)
					{
						this.hGC.Free();
						this.hGC = default(GCHandle);
					}
				}
				if (this.byArrWAVファイルイメージ != null)
				{
					this.byArrWAVファイルイメージ = null;
				}

				this.eSoundDeviceType = ESoundDeviceType.Unknown;

				if (bインスタンス削除)
				{
					//try
					//{
					//    CSound.listインスタンス.RemoveAt( freeIndex );
					//}
					//catch
					//{
					//    Debug.WriteLine( "FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strFilename ) );
					//}
					bool b = CSound.listインスタンス.Remove(this);    // これだと、Clone()したサウンドのremoveに失敗する
					if (!b)
					{
						Debug.WriteLine("FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName(this.strFilename));
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
		protected ESoundDeviceType eSoundDeviceType = ESoundDeviceType.Unknown;
		public string strFilename = null;
		protected byte[] byArrWAVファイルイメージ = null;	// WAVファイルイメージ、もしくはchunkのDATA部のみ
		protected GCHandle hGC;
		protected int _hTempoStream = 0;
		protected int _hBassStream = -1;					// ASIO, WASAPI 用
		protected int hBassStream = 0;						// #31076 2013.4.1 yyagi; プロパティとして実装すると動作が低速になったため、
															// tBASSサウンドを作成する_ストリーム生成後の共通処理()のタイミングと、
															// PlaySpeedを変更したタイミングでのみ、
															// hBassStreamを更新するようにした。
		//{
		//    get
		//    {
		//        if ( _hTempoStream != 0 && !this.bIs1倍速再生 )	// PlaySpeedがx1.000のときは、TempoStreamを用いないようにして高速化する
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
		public int[] BufferOpen;
		public int[] SourceOpen;
		public float[] defaultPan;
		private Lufs _gain = DefaultGain;
		private Lufs? _truePeak = null;
		private int _automationLevel = DefaultAutomationLevel;
		private int _groupLevel = DefaultGroupLevel;
		private long nBytes = 0;
		private int n一時停止回数 = 0;
		private int nFrequency = 0;
		private double _dbPlaySpeed = 1.0;
		private bool bIs1倍速再生 = true;

		private void tBASSサウンドを作成する( string strFilename, int hMixer, BassFlags flags )
		{
			this.e作成方法 = E作成方法.ファイルから;
			this.strFilename = strFilename;

			// BASSファイルストリームを作成。

			this._hBassStream = Bass.CreateStream( strFilename, 0, 0, flags );
			if (this._hBassStream == 0) 
			{
				//ファイルからのサウンド生成に失敗した場合にデコードする。(時間がかかるのはしょうがないね)
				tDecodeAudioFile(strFilename, out byArrWAVファイルイメージ, out _, out _, true);
				tBASSサウンドを作成する(byArrWAVファイルイメージ, hMixer, flags);
				return;
			}
			
			nBytes = Bass.ChannelGetLength( this._hBassStream );
			
			tBASSサウンドを作成する_ストリーム生成後の共通処理( hMixer );
		}
		private void tBASSサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer, BassFlags flags )
		{
			this.e作成方法 = E作成方法.WAVファイルイメージから;
			this.byArrWAVファイルイメージ = byArrWAVファイルイメージ;
			this.hGC = GCHandle.Alloc( byArrWAVファイルイメージ, GCHandleType.Pinned );		// byte[] をピン留め


			// BASSファイルストリームを作成。

			this._hBassStream = Bass.CreateStream( hGC.AddrOfPinnedObject(), 0, byArrWAVファイルイメージ.Length, flags );
			if ( this._hBassStream == 0 )
				throw new Exception( string.Format( "サウンドストリームの生成に失敗しました。(BASS_StreamCreateFile)[{0}]", Bass.LastError.ToString() ) );

			nBytes = Bass.ChannelGetLength( this._hBassStream );
	
			tBASSサウンドを作成する_ストリーム生成後の共通処理( hMixer );
		}

		private void tBASSサウンドを作成する_ストリーム生成後の共通処理( int hMixer )
		{
			CSoundManager.nStreams++;

			// 個々のストリームの出力をテンポ変更のストリームに入力する。テンポ変更ストリームの出力を、Mixerに出力する。

//			if ( CSoundManager.bIsTimeStretch )	// TimeStretchのON/OFFに関わりなく、テンポ変更のストリームを生成する。後からON/OFF切り替え可能とするため。
			{
				this._hTempoStream = BassFx.TempoCreate( this._hBassStream, BassFlags.Decode | BassFlags.FxFreeSource );
				if ( this._hTempoStream == 0 )
				{
					hGC.Free();
					throw new Exception( string.Format( "サウンドストリームの生成に失敗しました。(BASS_FX_TempoCreate)[{0}]", Bass.LastError.ToString() ) );
				}
				else
				{
					Bass.ChannelSetAttribute( this._hTempoStream, ChannelAttribute.TempoUseQuickAlgorithm, 1f );	// 高速化(音の品質は少し落ちる)
				}
			}

			if ( _hTempoStream != 0 && !this.bIs1倍速再生 )	// PlaySpeedがx1.000のときは、TempoStreamを用いないようにして高速化する
			{
				this.hBassStream = _hTempoStream;
			}
			else
			{
				this.hBassStream = _hBassStream;
			}

			// #32248 再生終了時に発火するcallbackを登録する (演奏終了後に再生終了するチップを非同期的にミキサーから削除するため。)
			_cbEndofStream = new SyncProcedure( CallbackEndofStream );
			Bass.ChannelSetSync( hBassStream, SyncFlags.End | SyncFlags.Mixtime, 0, _cbEndofStream, IntPtr.Zero );

			// n総演奏時間の取得; DTXMania用に追加。
			double seconds = Bass.ChannelBytes2Seconds( this._hBassStream, nBytes );
			this.nDurationms = (int) ( seconds * 1000 );
			//this.pos = 0;
			this.hMixer = hMixer;
			float freq = 0.0f;
			if ( !Bass.ChannelGetAttribute( this._hBassStream, ChannelAttribute.Frequency, out freq ) )
			{
				hGC.Free();
				throw new Exception( string.Format( "サウンドストリームの周波数取得に失敗しました。(BASS_ChannelGetAttribute)[{0}]", Bass.LastError.ToString() ) );
			}
			this.nFrequency = (int) freq;

			// インスタンスリストに登録。

			CSound.listインスタンス.Add( this );
		}

		/// <summary>
		/// ストリームの終端まで再生したときに呼び出されるコールバック
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="channel"></param>
		/// <param name="data"></param>
		/// <param name="user"></param>
		private void CallbackEndofStream( int handle, int channel, int data, IntPtr user )	// #32248 2013.10.14 yyagi
		{
			if ( b演奏終了後も再生が続くチップである )			// 演奏終了後に再生終了するチップ音のミキサー削除は、再生終了のコールバックに引っ掛けて、自前で行う。
			{													// そうでないものは、ミキサー削除予定時刻に削除する。
				tBASSサウンドをミキサーから削除する( channel );
			}
		}

		// mixerからの削除

		public bool tBASSサウンドをミキサーから削除する()
		{
			return tBASSサウンドをミキサーから削除する(this.hBassStream);
		}
		public static bool tBASSサウンドをミキサーから削除する( int channel )
		{
			bool b = BassMix.MixerRemoveChannel( channel );
			if ( b )
			{
				Interlocked.Decrement( ref CSoundManager.nMixing );
			}
			return b;
		}


		// mixer への追加
			public bool tBASSサウンドをミキサーに追加する()
		{
			if ( BassMix.ChannelGetMixer( hBassStream ) == 0 )
			{
				BassFlags bf = BassFlags.SpeakerFront | BassFlags.MixerChanNoRampin | BassFlags.MixerChanPause;
				Interlocked.Increment( ref CSoundManager.nMixing );

				// preloadされることを期待して、敢えてflagからはBASS_MIXER_PAUSEを外してAddChannelした上で、すぐにPAUSEする
				// -> ChannelUpdateでprebufferできることが分かったため、BASS_MIXER_PAUSEを使用することにした
				bool b1 = BassMix.MixerAddChannel( this.hMixer, this.hBassStream, bf );
				t再生位置を先頭に戻す();	// StreamAddChannelの後で再生位置を戻さないとダメ。逆だと再生位置が変わらない。
				Bass.ChannelUpdate( this.hBassStream, 0 );	// pre-buffer
				return b1;	// &b2;
			}
			return true;
		}

		#region [ tDecodeAudioFile() ]
		public void tDecodeAudioFile(string strFilename, out byte[] buffer,
			out int nPCMDataIndex, out int totalPCMSize, bool enablechunk)
		{
			nPCMDataIndex = 0;

			if ( !File.Exists( strFilename ) )
				throw new FileNotFoundException( string.Format( "File Not Found...({0})", strFilename ) );

			//丸投げ
			int rtn = CAudioDecoder.AudioDecode(strFilename, out buffer, out nPCMDataIndex, out totalPCMSize, enablechunk);

			//正常にDecodeできなかった場合、例外
			if ( rtn < 0 )
				throw new Exception( string.Format( "Decoded Failed...({0})({1})", rtn, strFilename ) );			
		}
		#endregion
		#endregion
	}
}
