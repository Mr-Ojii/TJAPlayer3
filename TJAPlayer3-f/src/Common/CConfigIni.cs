using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using FDK;
using FDK.ExtensionMethods;

namespace TJAPlayer3;

internal class CConfigIni : INotifyPropertyChanged
{
	private const int MinimumKeyboardSoundLevelIncrement = 1;
	private const int MaximumKeyboardSoundLevelIncrement = 20;
	private const int DefaultKeyboardSoundLevelIncrement = 5;

	// クラス

	#region [ CKeyAssign ]
	public class CKeyAssign
	{
		public CConfigIni.CKeyAssign.STKEYASSIGN[] FullScreen;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] Capture;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftRed;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] RightRed;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftBlue;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] RightBlue;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftRed2P;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] RightRed2P;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftBlue2P;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] RightBlue2P;
		public CConfigIni.CKeyAssign.STKEYASSIGN[] this[int index]
		{
			get
			{
				switch (index)
				{
					case (int)EKeyConfigPad.LRed:
						return this.LeftRed;

					case (int)EKeyConfigPad.RRed:
						return this.RightRed;

					case (int)EKeyConfigPad.LBlue:
						return this.LeftBlue;

					case (int)EKeyConfigPad.RBlue:
						return this.RightBlue;

					case (int)EKeyConfigPad.LRed2P:
						return this.LeftRed2P;

					case (int)EKeyConfigPad.RRed2P:
						return this.RightRed2P;

					case (int)EKeyConfigPad.LBlue2P:
						return this.LeftBlue2P;

					case (int)EKeyConfigPad.RBlue2P:
						return this.RightBlue2P;

					case (int)EKeyConfigPad.Capture:
						return this.Capture;

					case (int)EKeyConfigPad.FullScreen:
						return this.FullScreen;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch (index)
				{
					case (int)EKeyConfigPad.LRed:
						this.LeftRed = value;
						return;

					case (int)EKeyConfigPad.RRed:
						this.RightRed = value;
						return;

					case (int)EKeyConfigPad.LBlue:
						this.LeftBlue = value;
						return;

					case (int)EKeyConfigPad.RBlue:
						this.RightBlue = value;
						return;

					case (int)EKeyConfigPad.LRed2P:
						this.LeftRed2P = value;
						return;

					case (int)EKeyConfigPad.RRed2P:
						this.RightRed2P = value;
						return;

					case (int)EKeyConfigPad.LBlue2P:
						this.LeftBlue2P = value;
						return;

					case (int)EKeyConfigPad.RBlue2P:
						this.RightBlue2P = value;
						return;

					case (int)EKeyConfigPad.Capture:
						this.Capture = value;
						return;

					case (int)EKeyConfigPad.FullScreen:
						this.FullScreen = value;
						return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct STKEYASSIGN
		{
			public EInputDevice 入力デバイス;
			public int ID;
			public int Code;
			public STKEYASSIGN(EInputDevice DeviceType, int nID, int nCode)
			{
				this.入力デバイス = DeviceType;
				this.ID = nID;
				this.Code = nCode;
			}
		}
	}
	#endregion

	//
	public enum ESoundDeviceTypeForConfig
	{
		BASS = 0,
		ASIO,
		WASAPI_Exclusive,
		WASAPI_Shared,
		Unknown=99
	}
	// プロパティ

	public bool bWave再生位置自動調整機能有効;
	public Dictionary<int, string> dicJoystick;
	public CKeyAssign KeyAssign;
	public bool b演奏速度が一倍速であるとき以外音声を再生しない;

	private bool _applyLoudnessMetadata;

	public bool ApplyLoudnessMetadata
	{
		get => _applyLoudnessMetadata;
		set => SetProperty(ref _applyLoudnessMetadata, value, nameof(ApplyLoudnessMetadata));
	}

	private double _targetLoudness;

	public double TargetLoudness
	{
		get => _targetLoudness;
		set => SetProperty(ref _targetLoudness, value, nameof(TargetLoudness));
	}

	private bool _applySongVol;

	public bool ApplySongVol
	{
		get => _applySongVol;
		set => SetProperty(ref _applySongVol, value, nameof(ApplySongVol));
	}

	private int _soundEffectLevel;

	public int SoundEffectLevel
	{
		get => _soundEffectLevel;
		set => SetProperty(ref _soundEffectLevel, value, nameof(SoundEffectLevel));
	}

	private int _voiceLevel;

	public int VoiceLevel
	{
		get => _voiceLevel;
		set => SetProperty(ref _voiceLevel, value, nameof(VoiceLevel));
	}

	private int _songPreviewLevel;

	public int SongPreviewLevel
	{
		get => _songPreviewLevel;
		set => SetProperty(ref _songPreviewLevel, value, nameof(SongPreviewLevel));
	}

	private int _songPlaybackLevel;

	public int SongPlaybackLevel
	{
		get => _songPlaybackLevel;
		set => SetProperty(ref _songPlaybackLevel, value, nameof(SongPlaybackLevel));
	}

	private int _keyboardSoundLevelIncrement;

	public int KeyboardSoundLevelIncrement
	{
		get => _keyboardSoundLevelIncrement;
		set => SetProperty(
			ref _keyboardSoundLevelIncrement,
			Math.Clamp(value, MinimumKeyboardSoundLevelIncrement, MaximumKeyboardSoundLevelIncrement),
			nameof(KeyboardSoundLevelIncrement));
	}

	public string Version;
	public bool bBranchGuide;
	public int nScoreMode;
	public int nDefaultCourse; //2017.01.30 DD デフォルトでカーソルをあわせる難易度

	public bool b大音符判定;
	public int n両手判定の待ち時間;
	public bool b両手判定待ち時間中に大音符を判定枠に合わせるか;
	public int nBranchAnime;

	public bool bJudgeCountDisplay;

	public bool bEnableSkinV2;

	// 各画像の表示・非表示設定
	public bool ShowChara;
	public bool ShowDancer;
	public bool ShowRunner;
	public bool ShowFooter;
	public bool ShowMob;
	public bool ShowPuchiChara; // リザーブ
								//

	public ESubtitleDispMode eSubtitleDispMode = ESubtitleDispMode.Compliant;

	public EScrollMode eScrollMode = EScrollMode.Normal;
	public int nRegSpeedBPM = 120;
	public bool bスクロールモードを上書き = false;

	public bool bHispeedRandom;
	public EStealthMode[] eSTEALTH = new EStealthMode[4];
	public bool bNoInfo;

	public int nDefaultSongSort;

	public EGame eGameMode;
	public int TokkunSkipMeasures;
	public int TokkunMashInterval;
	public bool bSuperHard = false;

	public string strSystemSkinSubfolderFullName;	// #28195 2012.5.2 yyagi Skin切替用 System/以下のサブフォルダ名
	public bool bEnterがキー割り当てのどこにも使用されていない
	{
		get
		{
			for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
			{
				for (int k = 0; k < 0x10; k++)
				{
					if ((this.KeyAssign[j][k].入力デバイス == EInputDevice.KeyBoard) && (this.KeyAssign[j][k].Code == (int)SlimDXKeys.Key.Return))
					{
						return false;
					}
				}
			}

			return true;
		}
	}
	public bool b2P演奏時のSEの左右;
	public bool bTimeStretch;					// #23664 2013.2.24 yyagi ピッチ変更無しで再生速度を変更するかどうか

	//public bool bNoMP3Streaming;				// 2014.4.14 yyagi; mp3のシーク位置がおかしくなる場合は、これをtrueにすることで、wavにデコードしてからオンメモリ再生する
	public int nMasterVolume;
	public int MusicPreTimeMs; // 音源再生前の待機時間ms
	/// <summary>
	/// DiscordのRitch Presenceに再生中の.tjaファイルの情報を送信するかどうか。
	/// </summary>
	public bool SendDiscordPlayingInformation;


	// コンストラクタ

	public CConfigIni()
	{
		this.Version = "Unknown";
		//this.bWave再生位置自動調整機能有効 = true;
		this.bWave再生位置自動調整機能有効 = false;
		this.bEnableSkinV2 = false;
		this.ApplyLoudnessMetadata = true;

		// 2018-08-28 twopointzero:
		// There exists a particular large, well-known, well-curated, and
		// regularly-updated collection of content for use with Taiko no
		// Tatsujin simulators. A statistical analysis was performed on the
		// the integrated loudness and true peak loudness of the thousands
		// of songs within this collection as of late August 2018.
		//
		// The analysis allows us to select a target loudness which
		// results in the smallest total amount of loudness adjustment
		// applied to the songs of that collection. The selected target
		// loudness should result in the least-noticeable average
		// adjustment for the most users, assuming their collection is
		// similar to the exemplar.
		//
		// The target loudness which achieves this is -7.4 LUFS.
		this.TargetLoudness = -7.4;

		this.ApplySongVol = false;
		this.SoundEffectLevel = CSound.DefaultSoundEffectLevel;
		this.VoiceLevel = CSound.DefaultVoiceLevel;
		this.SongPreviewLevel = CSound.DefaultSongPreviewLevel;
		this.SongPlaybackLevel = CSound.DefaultSongPlaybackLevel;
		this.KeyboardSoundLevelIncrement = DefaultKeyboardSoundLevelIncrement;

		this.b演奏速度が一倍速であるとき以外音声を再生しない = false;
		this.ConfigIniファイル名 = "";
		this.dicJoystick = new Dictionary<int, string>( 10 );
		this.tデフォルトのキーアサインに設定する();

		this.strSystemSkinSubfolderFullName = "";	// #28195 2012.5.2 yyagi 使用中のSkinサブフォルダ名
		this.bTimeStretch = false;					// #23664 2013.2.24 yyagi 初期値はfalse (再生速度変更を、ピッチ変更にて行う)
		
		this.bBranchGuide = false;
		this.nScoreMode = 2;
		this.nDefaultCourse = 3;
		this.nBranchAnime = 1;

		this.b大音符判定 = true;
		this.n両手判定の待ち時間 = 25;
		this.b両手判定待ち時間中に大音符を判定枠に合わせるか = true;

		this.bJudgeCountDisplay = false;

		ShowChara = true;
		ShowDancer = true;
		ShowRunner = true;
		ShowFooter = true;
		ShowMob = true;
		ShowPuchiChara = true;

		this.eSTEALTH[0] = EStealthMode.OFF;
		this.eSTEALTH[1] = EStealthMode.OFF;
		this.bNoInfo = false;
		
		//this.bNoMP3Streaming = false;
		this.nMasterVolume = 100;                   // #33700 2014.4.26 yyagi マスターボリュームの設定(WASAPI/ASIO用)
		this.b2P演奏時のSEの左右 = true;

		this.bHispeedRandom = false;
		this.nDefaultSongSort = 1;
		this.eGameMode = EGame.OFF;
		this.TokkunSkipMeasures = 5;
		this.TokkunMashInterval = 750;
		MusicPreTimeMs = 1000; // 一秒
		SendDiscordPlayingInformation = true;
	}
	public CConfigIni( string iniファイル名 )
		: this()
	{
		this.tファイルから読み込み( iniファイル名 );
	}


	// メソッド

	public void t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice DeviceType, int nID, int nCode)
	{
		for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
		{
			for (int k = 0; k < 0x10; k++)
			{
				if (((this.KeyAssign[j][k].入力デバイス == DeviceType) && (this.KeyAssign[j][k].ID == nID)) && (this.KeyAssign[j][k].Code == nCode))
				{
					for (int m = k; m < 15; m++)
					{
						this.KeyAssign[j][m] = this.KeyAssign[j][m + 1];
					}
					this.KeyAssign[j][15].入力デバイス = EInputDevice.Unknown;
					this.KeyAssign[j][15].ID = 0;
					this.KeyAssign[j][15].Code = 0;
					k--;
				}
			}
		}
	}
	public void t書き出し( string iniファイル名 )
	{
		StreamWriter sw = new StreamWriter( iniファイル名, false, new UTF8Encoding(false));

		sw.WriteLine("[System]");
		#region [ スキン関連 ]
		#region [ Skinパスの絶対パス→相対パス変換 ]
		Uri uriRoot = new Uri( System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System/" ) );
		if ( strSystemSkinSubfolderFullName != null && strSystemSkinSubfolderFullName.Length == 0 )
		{
			// Config.iniが空の状態でDTXManiaをViewerとして起動_終了すると、strSystemSkinSubfolderFullName が空の状態でここに来る。
			// → 初期値として Default/ を設定する。
			strSystemSkinSubfolderFullName = System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System/Default/" );
		}
		Uri uriPath = new Uri( System.IO.Path.Combine( this.strSystemSkinSubfolderFullName, "./" ) );
		string relPath = uriRoot.MakeRelativeUri( uriPath ).ToString();				// 相対パスを取得
		relPath = System.Web.HttpUtility.UrlDecode( relPath );						// デコードする
		#endregion
		sw.WriteLine( "; 使用するSkinのフォルダ名。" );
		sw.WriteLine( "; 例えば System/Default/Graphics/... などの場合は、SkinPath=./Default/ を指定します。" );
		sw.WriteLine( "; Skin folder path." );
		sw.WriteLine( "; e.g. System/Default/Graphics/... -> Set SkinPath=./Default/" );
		sw.WriteLine( "SkinPath={0}", relPath );
		sw.WriteLine();
		#endregion
		sw.WriteLine("; BS1770GAIN によるラウドネスメータの測量を適用する (0:OFF, 1:ON)");
		sw.WriteLine( "; Apply BS1770GAIN loudness metadata (0:OFF, 1:ON)" );
		sw.WriteLine( "{0}={1}", nameof(ApplyLoudnessMetadata), this.ApplyLoudnessMetadata ? 1 : 0 );
		sw.WriteLine();
		sw.WriteLine( $"; BS1770GAIN によるラウドネスメータの目標値 (0). ({CSound.MinimumLufs}-{CSound.MaximumLufs})" );
		sw.WriteLine( $"; Loudness Target in dB (decibels) relative to full scale (0). ({CSound.MinimumLufs}-{CSound.MaximumLufs})" );
		sw.WriteLine( "{0}={1}", nameof(TargetLoudness), TargetLoudness );
		sw.WriteLine();
		sw.WriteLine("; .tjaファイルのSONGVOLヘッダを音源の音量に適用する (0:OFF, 1:ON)");
		sw.WriteLine( "; Apply SONGVOL (0:OFF, 1:ON)" );
		sw.WriteLine( "{0}={1}", nameof(ApplySongVol), this.ApplySongVol ? 1 : 0 );
		sw.WriteLine();
		sw.WriteLine( $"; 効果音の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( $"; Sound effect level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( "{0}={1}", nameof(SoundEffectLevel), SoundEffectLevel );
		sw.WriteLine();
		sw.WriteLine( $"; 各ボイス、コンボボイスの音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( $"; Voice level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( "{0}={1}", nameof(VoiceLevel), VoiceLevel );
		sw.WriteLine();
		sw.WriteLine( $"; 選曲画面のプレビュー時の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( $"; Song preview level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( "{0}={1}", nameof(SongPreviewLevel), SongPreviewLevel );
		sw.WriteLine();
		sw.WriteLine( $"; ゲーム中の音源の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( $"; Song playback level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)" );
		sw.WriteLine( "{0}={1}", nameof(SongPlaybackLevel), SongPlaybackLevel );
		sw.WriteLine();
		sw.WriteLine( $"; KeyBoardによる音量変更の増加量、減少量 ({MinimumKeyboardSoundLevelIncrement}-{MaximumKeyboardSoundLevelIncrement})" );
		sw.WriteLine( $"; Keyboard sound level increment ({MinimumKeyboardSoundLevelIncrement}-{MaximumKeyboardSoundLevelIncrement})" );
		sw.WriteLine( "{0}={1}", nameof(KeyboardSoundLevelIncrement), KeyboardSoundLevelIncrement );
		sw.WriteLine();
		sw.WriteLine($"; 2P演奏時に叩いた音を左右別々にならすか (0:OFF, 1:ON)");
		sw.WriteLine($"; Use panning for SE (0:OFF, 1:ON)");
		sw.WriteLine("UsePanning={0}", b2P演奏時のSEの左右 ? 1 : 0);
		sw.WriteLine();
		sw.WriteLine($"; 音源再生前の空白時間 (ms)");
		sw.WriteLine($"; Blank time before music source to play. (ms)");
		sw.WriteLine("{0}={1}", nameof(MusicPreTimeMs), MusicPreTimeMs);
		sw.WriteLine();
		sw.WriteLine("; Discordに再生中の譜面情報を送信する(0:OFF, 1:ON)");                        // #25399 2011.6.9 yyagi
		sw.WriteLine("; Share Playing .tja file infomation on Discord.");                     //
		sw.WriteLine("{0}={1}", nameof(SendDiscordPlayingInformation), SendDiscordPlayingInformation ? 1 : 0);       //
		sw.WriteLine();
		sw.WriteLine( "; 再生速度変更を、ピッチ変更で行うかどうか(0:ピッチ変更, 1:タイムストレッチ)" );	// #23664 2013.2.24 yyagi
		sw.WriteLine( "; Set \"0\" if you'd like to use pitch shift with PlaySpeed." );	//
		sw.WriteLine( "; Set \"1\" for time stretch." );								//
		sw.WriteLine( "TimeStretch={0}", this.bTimeStretch ? 1 : 0 );					//
		sw.WriteLine();
		sw.WriteLine( ";-------------------" );


#region [ PlayOption ]
		sw.WriteLine( "[PlayOption]" );
		sw.WriteLine();
		sw.WriteLine("; サブタイトルの表示モード(0:表示しない,1:譜面準拠,2:表示する)");   // 2020.10.18 Mr-Ojii
		sw.WriteLine("; SubtitleDisplayMode(0:Off,1:Compliant,2:On)");
		sw.WriteLine("SubtitleDispMode={0}", (int)this.eSubtitleDispMode);
		sw.WriteLine();
		sw.WriteLine("; 各画像の表示設定");
		sw.WriteLine("; キャラクター画像を表示する (0:OFF, 1:ON)");
		sw.WriteLine("ShowChara={0}", ShowChara ? 1 : 0);
		sw.WriteLine("; ダンサー画像を表示する (0:OFF, 1:ON)");
		sw.WriteLine("ShowDancer={0}", ShowDancer ? 1 : 0);
		sw.WriteLine("; ランナー画像を表示する (0:OFF, 1:ON)");
		sw.WriteLine("ShowRunner={0}", ShowRunner ? 1 : 0);
		sw.WriteLine("; モブ画像を表示する (0:OFF, 1:ON)");
		sw.WriteLine("ShowMob={0}", ShowMob ? 1 : 0);
		sw.WriteLine("; フッター画像 (0:OFF, 1:ON)");
		sw.WriteLine("ShowFooter={0}", ShowFooter ? 1 : 0);
		sw.WriteLine("; ぷちキャラ画像 (0:OFF, 1:ON)");
		sw.WriteLine("ShowPuchiChara={0}", ShowPuchiChara ? 1 : 0);
		sw.WriteLine();
		sw.WriteLine( "; 演奏速度が一倍速であるときのみBGMを再生する(0:OFF, 1:ON)");
		sw.WriteLine( "PlaySpeedNotEqualOneNoSound={0}", this.b演奏速度が一倍速であるとき以外音声を再生しない ? 1 : 0);
		sw.WriteLine();
		sw.WriteLine("; デフォルトで選択される難易度");
		sw.WriteLine("DefaultCourse={0}", this.nDefaultCourse);
		sw.WriteLine();
		sw.WriteLine( "; スコア計算方法(0:ドンだフルモード, 1:~AC14, 2:AC15, 3:AC16)");
		sw.WriteLine( "ScoreMode={0}", this.nScoreMode );
		sw.WriteLine();
		sw.WriteLine("; 両手判定の待ち時間中に大音符を判定枠に合わせる(0:OFF, 1:ON)");
		sw.WriteLine("BigNotesJudgeFrame={0}", this.b両手判定待ち時間中に大音符を判定枠に合わせるか ? 1 : 0);
		sw.WriteLine();
		sw.WriteLine( "; 大音符の両手入力待機時間(ms)" );
		sw.WriteLine( "BigNotesWaitTime={0}", this.n両手判定の待ち時間 );
		sw.WriteLine();
		sw.WriteLine( "; 大音符の両手判定(0:OFF, 1:ON)" );
		sw.WriteLine( "BigNotesJudge={0}", this.b大音符判定 ? 1 : 0 );
		sw.WriteLine();
		sw.WriteLine( "; NoInfo(0:OFF, 1:ON)" );
		sw.WriteLine( "NoInfo={0}", this.bNoInfo ? 1 : 0 );
		sw.WriteLine();
		sw.WriteLine( "; 譜面分岐のアニメーション(0:7～14, 1:15)" );
		sw.WriteLine( "BranchAnime={0}", this.nBranchAnime );
		sw.WriteLine();
		sw.WriteLine( "; デフォルトの曲ソート(0:絶対パス順, 1:ジャンル名ソートRENEWED )" );
		sw.WriteLine( "0:Path, 1:GenreName" );
		sw.WriteLine( "DefaultSongSort={0}", this.nDefaultSongSort );
		sw.WriteLine();
		sw.WriteLine( "; 1PSTEALTHモード(0:OFF, 1:ドロン, 2:ステルス)" );
		sw.WriteLine( "1PTaikoStealth={0}", (int) this.eSTEALTH[0] );
		sw.WriteLine( "2PTaikoStealth={0}", (int) this.eSTEALTH[1] );
		sw.WriteLine();
		sw.WriteLine( "; ゲーム(0:OFF, 1:完走!叩ききりまショー!, 2:完走!叩ききりまショー!(激辛), 3:特訓モード)" );
		sw.WriteLine( "GameMode={0}", (int) this.eGameMode );
		sw.WriteLine();
		sw.WriteLine("; 特訓モード時にPgUp/PgDnで何小節飛ばすか");
		sw.WriteLine("TokkunSkipMeasures={0}", this.TokkunSkipMeasures);
		sw.WriteLine();
		sw.WriteLine("; 特訓モード時にジャンプポイントに飛ばすための時間(ms)");
		sw.WriteLine("; 指定ms以内に5回縁を叩きましょう");
		sw.WriteLine("{1}={0}", this.TokkunMashInterval, nameof(this.TokkunMashInterval));
		sw.WriteLine();
		sw.WriteLine( "; 判定数の表示(0:OFF, 1:ON)" );
		sw.WriteLine( "JudgeCountDisplay={0}", this.bJudgeCountDisplay ? 1 : 0 );
		sw.WriteLine();

		sw.WriteLine( ";-------------------" );
#endregion
#region [ GUID ]
		sw.WriteLine( "[GUID]" );
		sw.WriteLine();
		foreach( KeyValuePair<int, string> pair in this.dicJoystick )
		{
			sw.WriteLine( "JoystickID={0},{1}", pair.Key, pair.Value );
		}
#endregion
#region [ DrumsKeyAssign ]
		sw.WriteLine();
		sw.WriteLine( ";-------------------" );
		sw.WriteLine( "; キーアサイン" );
		sw.WriteLine( ";   項　目：Keyboard → 'K'＋'0'＋KeyCode(10進数)" );
		sw.WriteLine( ";           Mouse    → 'N'＋'0'＋ボタン番号(0～13)" );
		sw.WriteLine( ";           MIDI In  → 'M'＋デバイス番号1桁(0～9,A～Z)＋ノート番号(10進数)" );
		sw.WriteLine( ";           Joystick → 'J'＋デバイス番号1桁(0～9,A～Z)＋ 0 ...... Ｘ減少(左)ボタン" );
		sw.WriteLine( ";                                                         1 ...... Ｘ増加(右)ボタン" );
		sw.WriteLine( ";                                                         2 ...... Ｙ減少(上)ボタン" );
		sw.WriteLine( ";                                                         3 ...... Ｙ増加(下)ボタン" );
		sw.WriteLine( ";                                                         4 ...... Ｚ減少(前)ボタン" );
		sw.WriteLine( ";                                                         5 ...... Ｚ増加(後)ボタン" );
		sw.WriteLine( ";                                                         6 ...... Ｚ回転(ＣＣＷ)");
		sw.WriteLine( ";                                                         7 ...... Ｚ回転(ＣＷ)");
		sw.WriteLine( ";                                                         8～135.. ボタン1～128" );
		sw.WriteLine( ";           これらの項目を 16 個まで指定可能(',' で区切って記述）。" );
		sw.WriteLine( ";" );
		sw.WriteLine( ";   表記例：LeftRed=K044,M042,J18");
		sw.WriteLine( ";           → LeftRed を Keyboard の 44 ('Z'), MidiIn#0 の 42, JoyPad#1 の 8(ボタン1) に割当て" );
		sw.WriteLine( ";" );
		sw.WriteLine( ";   ※Joystick のデバイス番号とデバイスとの関係は [GUID] セクションに記してあるものが有効。" );
		sw.WriteLine( ";" );
		sw.WriteLine( ";   ※改造者はJoystickと呼べるようなものを所持していないため、Ｚ回転のＣＷ、ＣＣＷは逆である可能性があります。");
		sw.WriteLine();
		sw.WriteLine( "[DrumsKeyAssign]" );
		sw.WriteLine();
		sw.Write( "LeftRed=" );
		this.tキーの書き出し( sw, this.KeyAssign.LeftRed );
		sw.WriteLine();
		sw.Write( "RightRed=" );
		this.tキーの書き出し( sw, this.KeyAssign.RightRed );
		sw.WriteLine();
		sw.Write( "LeftBlue=" );										// #27029 2012.1.4 from
		this.tキーの書き出し( sw, this.KeyAssign.LeftBlue );	//
		sw.WriteLine();											//
		sw.Write( "RightBlue=" );										// #27029 2012.1.4 from
		this.tキーの書き出し( sw, this.KeyAssign.RightBlue );	//
		sw.WriteLine();
		sw.Write( "LeftRed2P=" );
		this.tキーの書き出し( sw, this.KeyAssign.LeftRed2P );
		sw.WriteLine();
		sw.Write( "RightRed2P=" );
		this.tキーの書き出し( sw, this.KeyAssign.RightRed2P );
		sw.WriteLine();
		sw.Write( "LeftBlue2P=" );										// #27029 2012.1.4 from
		this.tキーの書き出し( sw, this.KeyAssign.LeftBlue2P );	//
		sw.WriteLine();											        //
		sw.Write( "RightBlue2P=" );										// #27029 2012.1.4 from
		this.tキーの書き出し( sw, this.KeyAssign.RightBlue2P );	//
		sw.WriteLine();
		sw.WriteLine();
#endregion
#region [ SystemkeyAssign ]
		sw.WriteLine( "[SystemKeyAssign]" );
		sw.WriteLine();
		sw.Write( "Capture=" );
		this.tキーの書き出し( sw, this.KeyAssign.Capture );
		sw.WriteLine();
		sw.Write("FullScreen=");
		this.tキーの書き出し(sw, this.KeyAssign.FullScreen);
		sw.WriteLine();
		sw.WriteLine();
#endregion

		sw.Close();
	}

	public void tファイルから読み込み( string iniファイル名 )
	{
		this.ConfigIniファイル名 = iniファイル名;
		this.bConfigIniが存在している = File.Exists( this.ConfigIniファイル名 );
		if( this.bConfigIniが存在している )
		{
			string str = CJudgeTextEncoding.ReadTextFile(ConfigIniファイル名);
			this.tキーアサインを全部クリアする();
			t文字列から読み込み( str );
		}
	}

	private void t文字列から読み込み( string strAllSettings )	// 2011.4.13 yyagi; refactored to make initial KeyConfig easier.
	{
		Eセクション種別 unknown = Eセクション種別.Unknown;
		string[] delimiter = { "\n" };
		string[] strSingleLine = strAllSettings.Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
		foreach ( string s in strSingleLine )
		{
			string str = s.Replace( '\t', ' ' ).TrimStart( new char[] { '\t', ' ' } );
			if (str.Length == 0 || str[0] == ';')
				continue;

			try
			{
				string str3;
				string str4;
				if ( str[ 0 ] == '[' )
				{
#region [ セクションの変更 ]
					//-----------------------------
					int index = str.IndexOf(']');
					string str2;
					if (index >= 0)
						str2 = str.Substring(1, index - 1);
					else
						str2 = str.Substring(1);

					if ( Enum.TryParse(typeof(Eセクション種別), str2, out var eType) )
						unknown = (Eセクション種別)eType;
					else
						unknown = Eセクション種別.Unknown;
					//-----------------------------
#endregion
				}
				else
				{
					string[] strArray = str.Split( new char[] { '=' } );
					if( strArray.Length == 2 )
					{
						str3 = strArray[ 0 ].Trim();
						str4 = strArray[ 1 ].Trim();
						switch( unknown )
						{
#region [ [System] ]
							//-----------------------------
							case Eセクション種別.System:
								{
#region [ skin関係 ]
									if ( str3.Equals( "SkinPath" ) )
									{
										string absSkinPath = str4;
										if ( !System.IO.Path.IsPathRooted( str4 ) )
										{
											absSkinPath = System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System" );
											absSkinPath = System.IO.Path.Combine( absSkinPath, str4 );
											Uri u = new Uri( absSkinPath );
											absSkinPath = u.AbsolutePath.ToString();	// str4内に相対パスがある場合に備える
											absSkinPath = System.Web.HttpUtility.UrlDecode( absSkinPath );						// デコードする
										}
										if ( absSkinPath[ absSkinPath.Length - 1 ] != '/' )	// フォルダ名末尾に\を必ずつけて、CSkin側と表記を統一する
										{
											absSkinPath += '/';
										}
										this.strSystemSkinSubfolderFullName = absSkinPath;
									}
#endregion
									else if( str3.Equals( nameof(ApplyLoudnessMetadata) ) )
									{
										this.ApplyLoudnessMetadata = str4[0].ToBool();
									}
									else if( str3.Equals( nameof(TargetLoudness) ) )
									{
										this.TargetLoudness = str4.ToDouble(CSound.MinimumLufs.ToDouble(), CSound.MaximumLufs.ToDouble(), this.TargetLoudness);
									}
									else if( str3.Equals( nameof(ApplySongVol) ) )
									{
										this.ApplySongVol = str4[0].ToBool();
									}
									else if( str3.Equals( nameof(SoundEffectLevel) ) )
									{
										this.SoundEffectLevel = str4.ToInt32(CSound.MinimumGroupLevel, CSound.MaximumGroupLevel, this.SoundEffectLevel);
									}
									else if( str3.Equals( nameof(VoiceLevel) ) )
									{
										this.VoiceLevel = str4.ToInt32(CSound.MinimumGroupLevel, CSound.MaximumGroupLevel, this.VoiceLevel);
									}
									else if( str3.Equals( nameof(SongPreviewLevel) ) )
									{
										this.SongPreviewLevel = str4.ToInt32(CSound.MinimumGroupLevel, CSound.MaximumGroupLevel, this.SongPreviewLevel);
									}
									else if( str3.Equals( nameof(SongPlaybackLevel) ) )
									{
										this.SongPlaybackLevel = str4.ToInt32(CSound.MinimumGroupLevel, CSound.MaximumGroupLevel, this.SongPlaybackLevel);
									}
									else if( str3.Equals( nameof(KeyboardSoundLevelIncrement) ) )
									{
										this.KeyboardSoundLevelIncrement = str4.ToInt32(MinimumKeyboardSoundLevelIncrement, MaximumKeyboardSoundLevelIncrement, this.KeyboardSoundLevelIncrement);
									}
									else if (str3.Equals("UsePanning"))
									{
										this.b2P演奏時のSEの左右 = str4[0].ToBool();
									}
									else if( str3.Equals(nameof(MusicPreTimeMs)))
									{
										MusicPreTimeMs = int.Parse(str4);
									}
									else if (str3.Equals(nameof(SendDiscordPlayingInformation)))
									{
										SendDiscordPlayingInformation = str4[0].ToBool();
									}
									else if ( str3.Equals( "TimeStretch" ) )				// #23664 2013.2.24 yyagi
									{
										this.bTimeStretch = str4[0].ToBool();
									}

									continue;
								}
							//-----------------------------
#endregion

#region [ [PlayOption] ]
							//-----------------------------
							case Eセクション種別.PlayOption:
								{
									if (str3.Equals("ShowChara"))
									{
										ShowChara = str4[0].ToBool();
									}
									else if( str3.Equals("ShowDancer"))
									{
										ShowDancer = str4[0].ToBool();
									}
									else if (str3.Equals("ShowRunner"))
									{
										ShowRunner = str4[0].ToBool();
									}
									else if (str3.Equals("ShowMob"))
									{
										ShowMob = str4[0].ToBool();
									}
									else if (str3.Equals("ShowFooter"))
									{
										ShowFooter = str4[0].ToBool();
									}
									else if (str3.Equals("ShowPuchiChara"))
									{
										ShowPuchiChara = str4[0].ToBool();
									}
									else if (str3.Equals("SubtitleDispMode"))
									{
										this.eSubtitleDispMode = (ESubtitleDispMode)str4.ToInt32(0, 2, (int)this.eSubtitleDispMode);
									}
									else if( str3.Equals( "ScrollMode" ) )
									{
										this.eScrollMode = (EScrollMode)str4.ToInt32(0, 2, 0);
									}
									else if (str3.Equals("PlaySpeedNotEqualOneNoSound"))
									{
										this.b演奏速度が一倍速であるとき以外音声を再生しない = str4[0].ToBool();
									}
									else if ( str3.Equals( "BranchGuide" ) )
									{
										this.bBranchGuide = str4[0].ToBool();
									}
									else if ( str3.Equals( "DefaultCourse" ) ) //2017.01.30 DD
									{
										this.nDefaultCourse = str4.ToInt32(0, 4, this.nDefaultCourse);
									}
									else if ( str3.Equals( "ScoreMode" ) )
									{
										this.nScoreMode = str4.ToInt32(0, 3, this.nScoreMode);
									}
									else if ( str3.Equals( "HispeedRandom" ) )
									{
										this.bHispeedRandom = str4[0].ToBool();
									}
									else if (str3.Equals("BigNotesJudgeFrame"))
									{
										this.b両手判定待ち時間中に大音符を判定枠に合わせるか = str4[0].ToBool();
									}
									else if ( str3.Equals( "BigNotesWaitTime" ) )
									{
										this.n両手判定の待ち時間 = str4.ToInt32(1, 100, this.n両手判定の待ち時間);
									}
									else if ( str3.Equals( "BigNotesJudge" ) )
									{
										this.b大音符判定 = str4[0].ToBool();
									}
									else if ( str3.Equals( "BranchAnime" ) )
									{
										this.nBranchAnime = str4.ToInt32(0, 1, this.nBranchAnime);
									}
									else if ( str3.Equals( "NoInfo" ) )
									{
										this.bNoInfo = str4[0].ToBool();
									}
									else if ( str3.Equals( "DefaultSongSort" ) )
									{
										this.nDefaultSongSort = str4.ToInt32(0, 1, this.nDefaultSongSort);
									}
									else if( str3.Equals( "1PTaikoStealth" ) )
									{
										this.eSTEALTH[0] = (EStealthMode) str4.ToInt32(0, 3, (int) this.eSTEALTH[0]);
									}
									else if (str3.Equals("2PTaikoStealth"))
									{
										this.eSTEALTH[1] = (EStealthMode)str4.ToInt32(0, 3, (int)this.eSTEALTH[1]);
									}
									else if( str3.Equals( "GameMode" ) )
									{
										this.eGameMode = (EGame)str4.ToInt32(0, 3, (int) this.eGameMode);
									}
									else if (str3.Equals("TokkunSkipMeasures"))
									{
										this.TokkunSkipMeasures = str4.ToInt32(0, 9999, this.TokkunSkipMeasures);
									}
									else if (str3.Equals(nameof(TokkunMashInterval)))
									{
										this.TokkunMashInterval = str4.ToInt32(0, 9999, this.TokkunMashInterval);
									}
									else if( str3.Equals( "JudgeCountDisplay" ) )
									{
										this.bJudgeCountDisplay = str4[0].ToBool();
									}
									continue;
								}
							//-----------------------------
#endregion

#region [ [GUID] ]
							//-----------------------------
							case Eセクション種別.GUID:
								if( str3.Equals( "JoystickID" ) )
								{
									this.tJoystickIDの取得( str4 );
								}
								continue;
							//-----------------------------
#endregion

#region [ [DrumsKeyAssign] ]
							//-----------------------------
							case Eセクション種別.DrumsKeyAssign:
								{
									if( str3.Equals( "LeftRed" ) )
									{
										this.tキーの読み出しと設定( str4, this.KeyAssign.LeftRed );
									}
									else if( str3.Equals( "RightRed" ) )
									{
										this.tキーの読み出しと設定( str4, this.KeyAssign.RightRed );
									}
									else if( str3.Equals( "LeftBlue" ) )										// #27029 2012.1.4 from
									{																	//
										this.tキーの読み出しと設定( str4, this.KeyAssign.LeftBlue );	//
									}																	//
									else if( str3.Equals( "RightBlue" ) )										// #27029 2012.1.4 from
									{																	//
										this.tキーの読み出しと設定( str4, this.KeyAssign.RightBlue );	//
									}

									else if( str3.Equals( "LeftRed2P" ) )
									{
										this.tキーの読み出しと設定( str4, this.KeyAssign.LeftRed2P );
									}
									else if( str3.Equals( "RightRed2P" ) )
									{
										this.tキーの読み出しと設定( str4, this.KeyAssign.RightRed2P );
									}
									else if( str3.Equals( "LeftBlue2P" ) )										// #27029 2012.1.4 from
									{																	//
										this.tキーの読み出しと設定( str4, this.KeyAssign.LeftBlue2P );	//
									}																	//
									else if( str3.Equals( "RightBlue2P" ) )										// #27029 2012.1.4 from
									{																	//
										this.tキーの読み出しと設定( str4, this.KeyAssign.RightBlue2P );	//
									}

									continue;
								}
							//-----------------------------
#endregion

#region [ [SystemKeyAssign] ]
							//-----------------------------
							case Eセクション種別.SystemKeyAssign:
								if( str3.Equals( "Capture" ) )
								{
									this.tキーの読み出しと設定( str4, this.KeyAssign.Capture );
								}
								else if (str3.Equals("FullScreen"))
								{
									this.tキーの読み出しと設定(str4, this.KeyAssign.FullScreen);
								}
								continue;
							//-----------------------------
#endregion
						}
					}
				}
				continue;
			}
			catch ( Exception exception )
			{
				Trace.TraceError( exception.ToString() );
				Trace.TraceError( "An exception has occurred, but processing continues." );
				continue;
			}
		}
	}

	// その他

#region [ private ]
	//-----------------
	private enum Eセクション種別
	{
		Unknown,
		System,
		PlayOption,
		GUID,
		DrumsKeyAssign,
		SystemKeyAssign,
		Temp,
	}

	private bool bConfigIniが存在している;
	private string ConfigIniファイル名;

	private void tJoystickIDの取得( string strキー記述 )
	{
		string[] strArray = strキー記述.Split( new char[] { ',' } );
		if( strArray.Length >= 2 )
		{
			int result = 0;
			if( ( int.TryParse( strArray[ 0 ], out result ) && ( result >= 0 ) ) && ( result <= 9 ) )
			{
				if( this.dicJoystick.ContainsKey( result ) )
				{
					this.dicJoystick.Remove( result );
				}
				this.dicJoystick.Add( result, strArray[ 1 ] );
			}
		}
	}
	private void tキーアサインを全部クリアする()
	{
		this.KeyAssign = new CKeyAssign();
		for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
		{
			this.KeyAssign[j] = new CKeyAssign.STKEYASSIGN[16];
			for (int k = 0; k < 16; k++)
			{
				this.KeyAssign[j][k] = new CKeyAssign.STKEYASSIGN(EInputDevice.Unknown, 0, 0);
			}
		}
	}
	private void tキーの書き出し( StreamWriter sw, CKeyAssign.STKEYASSIGN[] assign )
	{
		bool flag = true;
		for( int i = 0; i < 0x10; i++ )
		{
			if( assign[ i ].入力デバイス == EInputDevice.Unknown )
			{
				continue;
			}
			if( !flag )
			{
				sw.Write( ',' );
			}
			flag = false;
			switch( assign[ i ].入力デバイス )
			{
				case EInputDevice.KeyBoard:
					sw.Write( 'K' );
					break;

				case EInputDevice.MIDIInput:
					sw.Write( 'M' );
					break;

				case EInputDevice.Joypad:
					sw.Write( 'J' );
					break;

				case EInputDevice.Mouse:
					sw.Write( 'N' );
					break;
			}
			sw.Write( "{0}{1}", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring( assign[ i ].ID, 1 ), assign[ i ].Code );	// #24166 2011.1.15 yyagi: to support ID > 10, change 2nd character from Decimal to 36-numeral system. (e.g. J1023 -> JA23)
		}
	}
	private void tキーの読み出しと設定( string strキー記述, CKeyAssign.STKEYASSIGN[] assign )
	{
		string[] strArray = strキー記述.Split( new char[] { ',' } );
		for( int i = 0; ( i < strArray.Length ) && ( i < 0x10 ); i++ )
		{
			EInputDevice e入力デバイス;
			int id;
			int code;
			string str = strArray[ i ].Trim().ToUpper();
			if ( str.Length >= 3 )
			{
				e入力デバイス = EInputDevice.Unknown;
				switch ( str[ 0 ] )
				{
					case 'J':
						e入力デバイス = EInputDevice.Joypad;
						break;

					case 'K':
						e入力デバイス = EInputDevice.KeyBoard;
						break;

					case 'L':
						continue;

					case 'M':
						e入力デバイス = EInputDevice.MIDIInput;
						break;

					case 'N':
						e入力デバイス = EInputDevice.Mouse;
						break;
				}
			}
			else
			{
				continue;
			}
			id = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf( str[ 1 ] );	// #24166 2011.1.15 yyagi: to support ID > 10, change 2nd character from Decimal to 36-numeral system. (e.g. J1023 -> JA23)
			if( ( ( id >= 0 ) && int.TryParse( str.Substring( 2 ), out code ) ) && ( ( code >= 0 ) && ( code <= 0xff ) ) )
			{
				this.t指定した入力が既にアサイン済みである場合はそれを全削除する( e入力デバイス, id, code );
				assign[ i ].入力デバイス = e入力デバイス;
				assign[ i ].ID = id;
				assign[ i ].Code = code;
			}
		}
	}
	private void tデフォルトのキーアサインに設定する()
	{
		this.tキーアサインを全部クリアする();

		string strDefaultKeyAssign = @"
[DrumsKeyAssign]
LeftRed=K015
RightRed=K019
LeftBlue=K013
RightBlue=K020
LeftRed2P=K031
RightRed2P=K022
LeftBlue2P=K012
RightBlue2P=K047

[SystemKeyAssign]
Capture=K065
FullScreen=K064
";
		t文字列から読み込み( strDefaultKeyAssign );
	}
	//-----------------
#endregion


	public event PropertyChangedEventHandler PropertyChanged;

	private bool SetProperty<T>(ref T storage, T value, string propertyName = null)
	{
		if (Equals(storage, value))
		{
			return false;
		}

		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
