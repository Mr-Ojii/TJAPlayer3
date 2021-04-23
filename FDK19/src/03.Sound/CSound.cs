using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading;
using FDK.ExtensionMethods;
using Un4seen.Bass;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Fx;
using OpenTK.Audio.OpenAL;

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

		public int n総演奏時間ms
		{
			get;
			private set;
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
					if (bIsBASSSound)
					{
						if (_hTempoStream != 0 && !this.bIs1倍速再生)   // 再生速度がx1.000のときは、TempoStreamを用いないようにして高速化する
						{
							this.hBassStream = _hTempoStream;
						}
						else
						{
							this.hBassStream = _hBassStream;
						}

						if (CSoundManager.bIsTimeStretch)
						{
							Bass.BASS_ChannelSetAttribute(this.hBassStream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)(_db再生速度 * 100 - 100));
							//double seconds = Bass.BASS_ChannelBytes2Seconds( this.hTempoStream, nBytes );
							//this.n総演奏時間ms = (int) ( seconds * 1000 );
						}
						else
						{
							Bass.BASS_ChannelSetAttribute(this.hBassStream, BASSAttribute.BASS_ATTRIB_FREQ, (float)(_db再生速度 * nオリジナルの周波数));
						}
					}
					else
					{
						for (int i = 0; i < this.SourceOpen.Length; i++)
						{
							AL.Source(this.SourceOpen[i], ALSourcef.Pitch, (float)db再生速度);
						}
					}
				}
			}
		}
		#endregion
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
				if (this.bIsBASSSound)
				{
					var db音量 = ((value.ToDouble() / 100.0) + 1.0).Clamp(0, 1);
					Bass.BASS_ChannelSetAttribute(this.hBassStream, BASSAttribute.BASS_ATTRIB_VOL, (float) db音量);
				}
				else if (this.bIsOpenALSound)
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
				if( this.bIsBASSSound )
				{
					float f位置 = 0.0f;
					if ( !Bass.BASS_ChannelGetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_PAN, ref f位置 ) )
						return 0;
					return (int) ( f位置 * 100 );
				}
				else if( this.bIsOpenALSound )
				{
					return this._n位置;
				}
				return -9999;
			}
			set
			{
				if( this.bIsBASSSound )
				{
					float f位置 = Math.Min( Math.Max( value, -100 ), 100 ) / 100.0f;	// -100～100 → -1.0～1.0
					Bass.BASS_ChannelSetAttribute( this.hBassStream, BASSAttribute.BASS_ATTRIB_PAN, f位置 );
				}
				else if( this.bIsOpenALSound )
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

		public CSound(ESoundGroup soundGroup)
		{
			SoundGroup = soundGroup;
			this.n位置 = 0;
			this._db再生速度 = 1.0;
//			this._cbRemoveMixerChannel = new WaitCallback( RemoveMixerChannelLater );
			this._hBassStream = -1;
			this._hTempoStream = 0;
		}

		public void tBASSサウンドを作成する(string strFilename, int hMixer)
		{
			this.eSoundDeviceType = ESoundDeviceType.BASS;      // 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する(strFilename, hMixer, BASSFlag.BASS_STREAM_DECODE);
		}
		public void tBASSサウンドを作成する(byte[] byArrWAVファイルイメージ, int hMixer)
		{
			this.eSoundDeviceType = ESoundDeviceType.BASS;      // 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する(byArrWAVファイルイメージ, hMixer, BASSFlag.BASS_STREAM_DECODE);
		}
		public void tASIOサウンドを作成する( string strFilename, int hMixer )
		{
			this.eSoundDeviceType = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strFilename, hMixer, BASSFlag.BASS_STREAM_DECODE );
		}
		public void tASIOサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer )
		{
			this.eSoundDeviceType = ESoundDeviceType.ASIO;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BASSFlag.BASS_STREAM_DECODE );
		}
		public void tWASAPIサウンドを作成する( string strFilename, int hMixer, ESoundDeviceType eSoundDeviceType )
		{
			this.eSoundDeviceType = eSoundDeviceType;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( strFilename, hMixer, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT );
		}
		public void tWASAPIサウンドを作成する( byte[] byArrWAVファイルイメージ, int hMixer, ESoundDeviceType eSoundDeviceType )
		{
			this.eSoundDeviceType = eSoundDeviceType;		// 作成後に設定する。（作成に失敗してると例外発出されてここは実行されない）
			this.tBASSサウンドを作成する( byArrWAVファイルイメージ, hMixer, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT );
		}
		public void tOpenALサウンドを作成する(string strFilename)
		{
			this.e作成方法 = E作成方法.ファイルから;
			this.strFilename = strFilename;
			// すべてのファイルを FFmpeg でデコードすると時間がかかるので、ファイルが WAV かつ PCM フォーマットでない場合のみ FFmpeg でデコードする。

			byte[] byArrWAVファイルイメージ = null;

			try
			{
				this.e作成方法 = E作成方法.ファイルから;
				this.strFilename = strFilename;

				int nPCMデータの先頭インデックス = 0;
				//			int nPCMサイズbyte = (int) ( xa.xaheader.nSamples * xa.xaheader.nChannels * 2 );	// nBytes = Bass.BASS_ChannelGetLength( this.hBassStream );

				int nPCMサイズbyte;
				CWin32.WAVEFORMATEX cw32wfx;
				tオンメモリ方式でデコードする(strFilename, out this.byArrWAVファイルイメージ,
				out nPCMデータの先頭インデックス, out nPCMサイズbyte, out cw32wfx, false);

				// セカンダリバッファを作成し、PCMデータを書き込む。
				tOpenALサウンドを作成する_セカンダリバッファの作成とWAVデータ書き込み
					(ref this.byArrWAVファイルイメージ, cw32wfx, nPCMサイズbyte, nPCMデータの先頭インデックス);
				return;
			}
			catch (Exception e)
			{
				string s = Path.GetFileName(strFilename);
				Trace.TraceWarning($"Failed to create OpenAL buffer by using libav({s}: {e.Message})");
			}

			// あとはあちらで。

			this.tOpenALサウンドを作成する(byArrWAVファイルイメージ);
		}

		public void tOpenALサウンドを作成する( byte[] byArrWAVファイルイメージ )
		{
			if (byArrWAVファイルイメージ == null)
				return;

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

						var tag = br.ReadUInt16();
						int Channels = br.ReadInt16();
						int SamplesPerSecond = br.ReadInt32();
						int AverageBytesPerSecond = br.ReadInt32();
						int BlockAlignment = br.ReadInt16();
						int BitsPerSample = br.ReadInt16();


						if (tag == PCM) EnableData = true;
						else
							throw new InvalidDataException(string.Format("未対応のWAVEフォーマットタグです。(Tag:{0})", tag.ToString()));

						c32wfx = new CWin32.WAVEFORMATEX((short)tag, (ushort)Channels, (uint)SamplesPerSecond, (uint)AverageBytesPerSecond, (ushort)BlockAlignment, (ushort)BitsPerSample);
						
						long nフォーマットサイズbyte = 16;

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

			this.eSoundDeviceType = ESoundDeviceType.OpenAL;
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

		public void t再生を開始する()
		{
			t再生位置を先頭に戻す();
			tサウンドを再生する();
		}
		public void t再生を開始する( bool bループする)
		{
			if ( bIsBASSSound )
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
				if ( this.bIsBASSSound )
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
				if ( this.eSoundDeviceType == ESoundDeviceType.OpenAL )
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
		#endregion


		public void t解放する()
		{
			t解放する( false );
		}

		public void t解放する( bool _bインスタンス削除 )
		{
			if ( this.bIsBASSSound )		// stream数の削減用
			{
				tBASSサウンドをミキサーから削除する();
				_cbEndofStream = null;
				CSoundManager.nStreams--;
			}
			this.Dispose( true, _bインスタンス削除 );   // CSoundの再初期化時は、インスタンスは存続する。
		}
		public void tサウンドを再生する()
		{
			tサウンドを再生する(false);
		}
		private void tサウンドを再生する( bool bループする)
		{
			if ( this.bIsBASSSound )			// BASSサウンド時のループ処理は、t再生を開始する()側に実装。ここでは「bループする」は未使用。
			{
//Debug.WriteLine( "再生中?: " +  System.IO.Path.GetFileName(this.strFilename) + " status=" + BassMix.BASS_Mixer_ChannelIsActive( this.hBassStream ) + " current=" + BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream ) + " nBytes=" + nBytes );
				bool b = BassMix.BASS_Mixer_ChannelPlay( this.hBassStream );
				if ( !b )
				{
//Debug.WriteLine( "再生しようとしたが、Mixerに登録されていなかった: " + Path.GetFileName( this.strFilename ) + ", stream#=" + this.hBassStream + ", ErrCode=" + Bass.BASS_ErrorGetCode() );

					bool bb = tBASSサウンドをミキサーに追加する();
					if ( !bb )
					{
Debug.WriteLine( "Mixerへの登録に失敗: " + Path.GetFileName( this.strFilename ) + ", ErrCode=" + Bass.BASS_ErrorGetCode() );
					}
					else
					{
//Debug.WriteLine( "Mixerへの登録に成功: " + Path.GetFileName( this.strFilename ) + ": " + Bass.BASS_ErrorGetCode() );
					}
					//this.t再生位置を先頭に戻す();

					bool bbb = BassMix.BASS_Mixer_ChannelPlay( this.hBassStream );
					if (!bbb)
					{
Debug.WriteLine("更に再生に失敗: " + Path.GetFileName(this.strFilename) + ", ErrCode=" + Bass.BASS_ErrorGetCode() );
					}
					else
					{
//						Debug.WriteLine("再生成功(ミキサー追加後)                       : " + Path.GetFileName(this.strFilename));
					}
				}
				else
				{
//Debug.WriteLine( "再生成功: " + Path.GetFileName( this.strFilename ) + " (" + hBassStream + ")" );
				}
			}
			else if( this.bIsOpenALSound )
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
			if ( bIsBASSSound )
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
			if( this.bIsBASSSound )
			{
				BassMix.BASS_Mixer_ChannelPause( this.hBassStream );
			}
			else if( this.bIsOpenALSound )
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
			if( this.bIsBASSSound )
			{
				BassMix.BASS_Mixer_ChannelSetPosition( this.hBassStream, 0 );
				//pos = 0;
			}
			else if( this.bIsOpenALSound )
			{
				for (int i = 0; i < this.SourceOpen.Length; i++)
				{
					AL.Source(this.SourceOpen[i], ALSourcef.SecOffset, 0f);
				}
			}
		}
		public void t再生位置を変更する( long n位置ms )
		{
			if( this.bIsBASSSound )
			{
				bool b = true;
				try
				{
					b = BassMix.BASS_Mixer_ChannelSetPosition( this.hBassStream, Bass.BASS_ChannelSeconds2Bytes( this.hBassStream, n位置ms * _db再生速度 / 1000.0 ), BASSMode.BASS_POS_BYTES );
				}
				catch( Exception e )
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceInformation( Path.GetFileName( this.strFilename ) + ": Seek error: " + e.ToString() + ": " + n位置ms + "ms" );
				}
				finally
				{
					if ( !b )
					{
						BASSError be = Bass.BASS_ErrorGetCode();
						Trace.TraceInformation( Path.GetFileName( this.strFilename ) + ": Seek error: " + be.ToString() + ": " + n位置ms + "MS" );
					}
				}
			}
			else if( this.bIsOpenALSound )
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
					Trace.TraceError("{0}: Seek error: {1}", Path.GetFileName(this.strFilename), n位置ms);
					Trace.TraceError("An exception has occurred, but processing continues. (95dee242-1f92-4fcf-aaf6-b162ad2bfc03)");
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
			if ( this.bIsBASSSound )
			{
				n位置byte = BassMix.BASS_Mixer_ChannelGetPosition( this.hBassStream );
				db位置ms = Bass.BASS_ChannelBytes2Seconds( this.hBassStream, n位置byte );
			}
			else if ( this.bIsOpenALSound )
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
						string strFilename = sounds[ i ].strFilename;
						sounds[ i ].Dispose( true, false );
						device.tCreateSound( strFilename, sounds[ i ] );
						break;
					#endregion
					#region [ WAVファイルイメージから ]
					case E作成方法.WAVファイルイメージから:
						if( sounds[ i ].bIsBASSSound )
						{
							byte[] byArrWaveファイルイメージ = sounds[ i ].byArrWAVファイルイメージ;
							sounds[ i ].Dispose( true, false );
							device.tCreateSound( byArrWaveファイルイメージ, sounds[ i ] );
						}
						else if( sounds[ i ].bIsOpenALSound )
						{
							byte[] byArrWaveファイルイメージ = sounds[ i ].byArrWAVファイルイメージ;
							sounds[ i ].Dispose( true, false );
							( (CSoundDeviceOpenAL) device ).tCreateSound( byArrWaveファイルイメージ, sounds[ i ] );
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
			if( this.bIsBASSSound )
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
				//        Debug.WriteLine( "ERR: freeIndex==-1 : Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strFilename ) );
				//    }
				//}

				if( this.eSoundDeviceType == ESoundDeviceType.OpenAL )
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
					this.eSoundDeviceType != ESoundDeviceType.OpenAL )	// OpenAL は hGC 未使用。
				{
					if ( this.hGC.IsAllocated )
					{
						this.hGC.Free();
						this.hGC = default( GCHandle );
					}
				}
				if ( this.byArrWAVファイルイメージ != null )
				{
					this.byArrWAVファイルイメージ = null;
				}

				this.eSoundDeviceType = ESoundDeviceType.Unknown;

				if ( bインスタンス削除 )
				{
					//try
					//{
					//    CSound.listインスタンス.RemoveAt( freeIndex );
					//}
					//catch
					//{
					//    Debug.WriteLine( "FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strFilename ) );
					//}
					bool b = CSound.listインスタンス.Remove( this );	// これだと、Clone()したサウンドのremoveに失敗する
					if ( !b )
					{
						Debug.WriteLine( "FAILED to remove CSound.listインスタンス: Count=" + CSound.listインスタンス.Count + ", filename=" + Path.GetFileName( this.strFilename ) );
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
		private bool bIsOpenALSound
		{
			get { return ( this.eSoundDeviceType == ESoundDeviceType.OpenAL ); }
		}
		private bool bIsBASSSound
		{
			get
			{
				return (
					this.eSoundDeviceType == ESoundDeviceType.BASS ||
					this.eSoundDeviceType == ESoundDeviceType.ASIO ||
					this.eSoundDeviceType == ESoundDeviceType.ExclusiveWASAPI ||
					this.eSoundDeviceType == ESoundDeviceType.SharedWASAPI );
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

		private void tBASSサウンドを作成する( string strFilename, int hMixer, BASSFlag flags )
		{
			this.e作成方法 = E作成方法.ファイルから;
			this.strFilename = strFilename;

			// BASSファイルストリームを作成。

			this._hBassStream = Bass.BASS_StreamCreateFile( strFilename, 0, 0, flags );
			if (this._hBassStream == 0) 
			{
				//ファイルからのサウンド生成に失敗した場合にデコードする。(時間がかかるのはしょうがないね)
				tオンメモリ方式でデコードする(strFilename, out byArrWAVファイルイメージ, out _, out _, out _, true);
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

		private void tBASSサウンドを作成する_ストリーム生成後の共通処理( int hMixer )
		{
			CSoundManager.nStreams++;

			// 個々のストリームの出力をテンポ変更のストリームに入力する。テンポ変更ストリームの出力を、Mixerに出力する。

//			if ( CSoundManager.bIsTimeStretch )	// TimeStretchのON/OFFに関わりなく、テンポ変更のストリームを生成する。後からON/OFF切り替え可能とするため。
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
			if (this.bIsBASSSound)
				return tBASSサウンドをミキサーから削除する(this.hBassStream);
			else
				return false;
		}
		public static bool tBASSサウンドをミキサーから削除する( int channel )
		{
			bool b = BassMix.BASS_Mixer_ChannelRemove( channel );
			if ( b )
			{
				Interlocked.Decrement( ref CSoundManager.nMixing );
			}
			return b;
		}


		// mixer への追加
			public bool tBASSサウンドをミキサーに追加する()
		{
			if ( BassMix.BASS_Mixer_ChannelGetMixer( hBassStream ) == 0 )
			{
				BASSFlag bf = BASSFlag.BASS_SPEAKER_FRONT | BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_PAUSE;
				Interlocked.Increment( ref CSoundManager.nMixing );

				// preloadされることを期待して、敢えてflagからはBASS_MIXER_PAUSEを外してAddChannelした上で、すぐにPAUSEする
				// -> ChannelUpdateでprebufferできることが分かったため、BASS_MIXER_PAUSEを使用することにした
				bool b1 = BassMix.BASS_Mixer_StreamAddChannel( this.hMixer, this.hBassStream, bf );
				t再生位置を先頭に戻す();	// StreamAddChannelの後で再生位置を戻さないとダメ。逆だと再生位置が変わらない。
				Bass.BASS_ChannelUpdate( this.hBassStream, 0 );	// pre-buffer
				return b1;	// &b2;
			}
			return true;
		}

		#region [ tオンメモリ方式でデコードする() ]
		public void tオンメモリ方式でデコードする(string strFilename, out byte[] buffer,
			out int nPCMデータの先頭インデックス, out int totalPCMSize, out CWin32.WAVEFORMATEX wfx, bool enablechunk)
		{
			nPCMデータの先頭インデックス = 0;

			if ( !File.Exists( strFilename ) )
				throw new FileNotFoundException( string.Format( "File Not Found...({0})", strFilename ) );

			//丸投げ
			int rtn = CAudioDecoder.AudioDecode(strFilename, out buffer, out nPCMデータの先頭インデックス, out totalPCMSize, out wfx, enablechunk);

			//正常にDecodeできなかった場合、例外
			if ( rtn < 0 )
				throw new Exception( string.Format( "Decoded Failed...({0})({1})", rtn, strFilename ) );			
		}
		#endregion
		#endregion
	}
}
