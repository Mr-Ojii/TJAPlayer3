using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TJAPlayer3
{

	/// <summary>
	/// 難易度。
	/// </summary>
	public enum Difficulty
	{
		Easy,
		Normal,
		Hard,
		Oni,
		Edit,
		Tower,
		Dan,
		Total
	}

	public enum EScrollMode
	{
		Normal,
		BMSCROLL,
		HBSCROLL,
		REGULSPEED
	}
	public enum EGame
	{
		OFF = 0,
		完走叩ききりまショー = 1,
		完走叩ききりまショー激辛 = 2,
		特訓モード = 3
	}
	public enum E難易度表示タイプ
	{
		OFF = 0,
		n曲目に表示 = 1,
		mtaikoに画像で表示 = 2,
	}
	public enum Eパッド			// 演奏用のenum。ここを修正するときは、次に出てくる EKeyConfigPad と EパッドFlag もセットで修正すること。
	{
		LRed    = 0,
		RRed    = 1,
		LBlue   = 2,
		RBlue   = 3,
		LRed2P  = 4,
		RRed2P  = 5,
		LBlue2P = 6,
		RBlue2P = 7,
		MAX,			// 門番用として定義
		UNKNOWN = 99
	}
	public enum EKeyConfigPad		// #24609 キーコンフィグで使うenum。capture要素あり。
	{
		LRed    = Eパッド.LRed,
		RRed    = Eパッド.RRed,
		LBlue   = Eパッド.LBlue,
		RBlue   = Eパッド.RBlue,
		LRed2P  = Eパッド.LRed2P,
		RRed2P  = Eパッド.RRed2P,
		LBlue2P = Eパッド.LBlue2P,
		RBlue2P = Eパッド.RBlue2P,
		Capture,
		UNKNOWN = Eパッド.UNKNOWN
	}
	[Flags]
	public enum EパッドFlag		// #24063 2011.1.16 yyagi コマンド入力用 パッド入力のフラグ化
	{
		LRed    = 0,
		RRed    = 1,
		LBlue   = 2,
		RBlue   = 4,
		LRed2P  = 8,
		RRed2P  = 16,
		LBlue2P = 32,
		RBlue2P = 64,
		UNKNOWN = 4096
	}
	public enum ERandomMode
	{
		OFF,
		RANDOM,
		MIRROR,
		SUPERRANDOM,
		HYPERRANDOM
	}
	public enum E楽器パート		// ここを修正するときは、セットで次の EKeyConfigPart も修正すること。
	{
		DRUMS	= 0,
		TAIKO   = 1,
		UNKNOWN	= 99
	}
	public enum EKeyConfigPart	// : E楽器パート
	{
		DRUMS	= E楽器パート.DRUMS,
		SYSTEM,
		UNKNOWN	= E楽器パート.UNKNOWN
	}
	internal enum E入力デバイス
	{
		キーボード		= 0,
		MIDI入力		= 1,
		ジョイパッド	= 2,
		マウス			= 3,
		不明			= -1
	}
	public enum E判定
	{
		Perfect	= 0,
		Great	= 1,
		Good	= 2,
		Poor	= 3,
		Miss	= 4,
		Bad		= 5,
		AutoPerfect = 6,
		AutoGreat   = 7,
		AutoGood    = 8,
	}
	internal enum EFIFOモード
	{
		フェードイン,
		フェードアウト
	}
	internal enum E演奏画面の戻り値
	{
		継続,
		演奏中断,
		ステージ失敗,
		ステージクリア,
		再読込_再演奏,
		再演奏
	}
	internal enum E曲読込画面の戻り値
	{
		継続 = 0,
		読込完了,
		読込中止
	}

	public enum ENoteState
	{
		none,
		wait,
		perfect,
		grade,
		bad
	}

	public enum EStealthMode
	{
		OFF = 0,
		DORON = 1,
		STEALTH = 2
	}

	/// <summary>
	/// Drum/Taiko の値を扱う汎用の構造体。
	/// </summary>
	/// <typeparam name="T">値の型。</typeparam>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct STDGBVALUE<T>			// indexはE楽器パートと一致させること
	{
		public T Drums;
		public T Taiko;
		public T Unknown;
		public T this[ int index ]
		{
			get
			{
				switch( index )
				{
					case (int) E楽器パート.DRUMS:
						return this.Drums;

					case (int) E楽器パート.TAIKO:
						return this.Taiko;

					case (int) E楽器パート.UNKNOWN:
						return this.Unknown;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch( index )
				{
					case (int) E楽器パート.DRUMS:
						this.Drums = value;
						return;

					case (int) E楽器パート.TAIKO:
						this.Taiko = value;
						return;

					case (int) E楽器パート.UNKNOWN:
						this.Unknown = value;
						return;
				}
				throw new IndexOutOfRangeException();
			}
		}
	}

	#region[Ver.K追加]
	public enum EClipDispType
	{
		背景のみ           = 1,
		ウィンドウのみ     = 2,
		両方               = 3,
		OFF                = 0
	}
	#endregion

}
