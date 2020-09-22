using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using FDK;
using FDK.ExtensionMethods;

namespace TJAPlayer3
{
	internal class CDTX : CActivity
	{
		// 定数

		// クラス


		public class CBPM
		{
			public double dbBPM値;
			public double bpm_change_time;
			public double bpm_change_bmscroll_time;
			public int bpm_change_course = 0;
			public int n内部番号;
			public int n表記上の番号;

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder(0x80);
				if (this.n内部番号 != this.n表記上の番号)
				{
					builder.Append(string.Format("CBPM{0}(内部{1})", CDTX.tZZ(this.n表記上の番号), this.n内部番号));
				}
				else
				{
					builder.Append(string.Format("CBPM{0}", CDTX.tZZ(this.n表記上の番号)));
				}
				builder.Append(string.Format(", BPM:{0}", this.dbBPM値));
				return builder.ToString();
			}
		}
		public class CSCROLL
		{
			public double dbSCROLL値;
			public double dbSCROLL値Y;
			public int n内部番号;
			public int n表記上の番号;

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder(0x80);
				if (this.n内部番号 != this.n表記上の番号)
				{
					builder.Append(string.Format("CSCROLL{0}(内部{1})", CDTX.tZZ(this.n表記上の番号), this.n内部番号));
				}
				else
				{
					builder.Append(string.Format("CSCROLL{0}", CDTX.tZZ(this.n表記上の番号)));
				}
				builder.Append(string.Format(", SCROLL:{0}", this.dbSCROLL値));
				return builder.ToString();
			}
		}
		/// <summary>
		/// 判定ライン移動命令
		/// </summary>
		public class CJPOSSCROLL
		{
			public double db移動時間;
			public int n移動距離px;
			public int n内部番号;
			public int n表記上の番号;

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder(0x80);
				if (this.n内部番号 != this.n表記上の番号)
				{
					builder.Append(string.Format("CJPOSSCROLL{0}(内部{1})", CDTX.tZZ(this.n表記上の番号), this.n内部番号));
				}
				else
				{
					builder.Append(string.Format("CJPOSSCROLL{0}", CDTX.tZZ(this.n表記上の番号)));
				}
				builder.Append(string.Format(", JPOSSCROLL:{0}", this.db移動時間));
				return builder.ToString();
			}
		}

		public class CDELAY
		{
			public int nDELAY値; //格納時にはmsになっているため、doubleにはしない。
			public int n内部番号;
			public int n表記上の番号;
			public double delay_time;
			public double delay_bmscroll_time;
			public double delay_bpm;
			public int delay_course = 0;

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder(0x80);
				if (this.n内部番号 != this.n表記上の番号)
				{
					builder.Append(string.Format("CDELAY{0}(内部{1})", CDTX.tZZ(this.n表記上の番号), this.n内部番号));
				}
				else
				{
					builder.Append(string.Format("CDELAY{0}", CDTX.tZZ(this.n表記上の番号)));
				}
				builder.Append(string.Format(", DELAY:{0}", this.nDELAY値));
				return builder.ToString();
			}
		}

		public class CBRANCH
		{
			public int n分岐の種類; //0:精度分岐 1:連打分岐 2:スコア分岐 3:大音符のみの精度分岐
			public double db条件数値A;
			public double db条件数値B;
			public double db分岐時刻ms;

			public int n番号;

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder(0x80);

				builder.Append(string.Format("CBRANCH{0}, BRANCH:{1}", CDTX.tZZ(this.n番号), this.n分岐の種類));

				return builder.ToString();
			}
		}


		public class CChip : IComparable<CDTX.CChip>, ICloneable
		{
			public bool bHit;
			public bool b可視 = true;
			public bool bShow;
			public bool bBranch = false;
			public double dbチップサイズ倍率 = 1.0;
			public double db実数値;
			public double dbBPM;
			public double db末端BPM;
			public float fNow_Measure_s = 4.0f; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加//強制分岐のために追加.2020.04.21.akasoko26
			public float fNow_Measure_m = 4.0f; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加//強制分岐のために追加.2020.04.21.akasoko26
			public bool IsEndedBranching = false; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加//分岐が終わった時の連打譜面が非可視化になってしまうためフラグを追加.2020.04.21.akasoko26
			public double dbSCROLL;
			public double dbSCROLL_Y;
			public double db末端SCROLL;
			public int nコース;
			public int nSenote;
			public int nRollCount;
			public int nBalloon;
			public int nProcessTime;
			public int nスクロール方向;
			public int n描画優先度; //(特殊)現状連打との判断目的で使用
			public ENoteState eNoteState;
			public EAVI種別 eAVI種別;
			public E楽器パート e楽器パート = E楽器パート.UNKNOWN;
			public int nチャンネル番号;
			public STDGBVALUE<int> nバーからの距離dot;
			public int nバーからのノーツ末端距離dot;
			public int n整数値;
			public int n整数値_内部番号;
			public int n発声位置;
			public double db発声位置;  // 発声時刻を格納していた変数のうちの１つをfloat型からdouble型に変更。(kairera0467)
			public double fBMSCROLLTime;
			public double fBMSCROLLTime_end;
			public int n発声時刻ms;
			public double db発声時刻ms;
			public int nノーツ終了位置;
			public int nノーツ終了時刻ms;
			public int nノーツ出現時刻ms;
			public int nノーツ移動開始時刻ms;
			public int n連打音符State;
			public int nLag;                // 2011.2.1 yyagi
			public CVideoDecoder rVD;
			public int nPlayerSide;
			public bool bGOGOTIME = false; //2018.03.11 k1airera0467 ゴーゴータイム内のチップであるか
			public bool IsFixedSENote;
			public bool IsHitted = false;
			public bool IsMissed = false; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加

			public bool b演奏終了後も再生が続くチップである;	// #32248 2013.10.14 yyagi
			public CCounter RollDelay; // 18.9.22 AioiLight Add 連打時に赤くなるやつのタイマー
			public CCounter RollInputTime; // 18.9.22 AioiLight Add  連打入力後、RollDelayが作動するまでのタイマー
			public int RollEffectLevel; // 18.9.22 AioiLight Add 連打時に赤くなるやつの度合い

			public CChip()
			{
				this.nバーからの距離dot = new STDGBVALUE<int>()
				{
					Drums = 0,
					Taiko = 0
				};
			}
			public override string ToString()
			{

				//2016.10.07 kairera0467 近日中に再編成予定
				string[] chToStr =
				{
					//システム
					"??", "バックコーラス", "小節長変更", "BPM変更", "??", "??", "??", "??",
					"BPM変更(拡張)", "??", "??", "??", "??", "??", "??", "??",

					//太鼓1P(移動予定)
					"??", "ドン", "カツ", "ドン(大)", "カツ(大)", "連打", "連打(大)", "ふうせん連打",
					"連打終点", "芋", "ドン(手)", "カッ(手)", "??", "??", "??", "AD-LIB",

					//太鼓予備
					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??",

					//太鼓予備
					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??",

					//太鼓予備
					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??", 

					//システム
					"小節線", "拍線", "??", "??", "AVI", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??", 

					//システム(移動予定)
					"SCROLL", "DELAY", "GOGOSTART", "GOGOEND", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??",

					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??",

					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??", 

					//太鼓1P、システム(現行)
					"??", "??", "??", "太鼓_赤", "太鼓_青", "太鼓_赤(大)", "太鼓_青(大)", "太鼓_黄",
					"太鼓_黄(大)", "太鼓_風船", "太鼓_連打末端", "太鼓_芋", "??", "SCROLL", "GOGOSTART", "GOGOEND",

					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "太鼓 AD-LIB",

					"??", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??",

					"??", "??", "??", "??", "0xC4", "0xC5", "0xC6", "??",
					"??", "??", "0xCA", "??", "??", "??", "??", "0xCF", 

					//システム(現行)
					"0xD0", "??", "??", "??", "??", "??", "??", "??",
					"??", "??", "ミキサー追加", "ミキサー削除", "DELAY", "譜面分岐リセット", "譜面分岐アニメ", "譜面分岐内部処理", 

					//システム(現行)
					"小節線ON/OFF", "分岐固定", "判定枠移動", "", "", "", "", "",
					"", "", "", "", "", "", "", "",

					"0xF0", "歌詞", "??", "SUDDEN", "??", "??", "??", "??",
					"??", "??", "??", "??", "??", "??", "??", "??", "譜面終了"
				};
				return string.Format("CChip: Position:{0:D4}.{1:D3}, Time{2:D6}, Ch:{3:X2}({4}), Pn:{5}({11})(Internal{6}), Pd:{7}, Sz:{8}, BMScroll:{9}, Cource:{10}",
					this.n発声位置 / 384, this.n発声位置 % 384,
					this.n発声時刻ms,
					this.nチャンネル番号, chToStr[this.nチャンネル番号],
					this.n整数値, this.n整数値_内部番号,
					this.db実数値,
					this.dbチップサイズ倍率,
					this.fBMSCROLLTime,
					this.nコース,
					CDTX.tZZ(this.n整数値));
			}
			/// <summary>
			/// チップの再生長を取得する。現状、WAVチップとBGAチップでのみ使用可能。
			/// </summary>
			/// <returns>再生長(ms)</returns>
			public int GetDuration()
			{
				int nDuration = 0;

				if (this.nチャンネル番号 == 0x01)       // WAV
				{
					CDTX.CWAV wc;
					TJAPlayer3.DTX[0].listWAV.TryGetValue(this.n整数値_内部番号, out wc);
					if (wc == null)
					{
						nDuration = 0;
					}
					else
					{
						nDuration = (wc.rSound[0] == null) ? 0 : wc.rSound[0].n総演奏時間ms;
					}
				}

				return (int)(nDuration / TJAPlayer3.DTX[0].db再生速度);
			}

			#region [ IComparable 実装 ]
			//-----------------

			private static readonly byte[] n優先度 = new byte[] {
				5, 5, 3, 7, 5, 5, 5, 5, 3, 5, 5, 5, 5, 5, 5, 5, //0x00
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x10
				7, 7, 7, 7, 7, 7, 7, 7, 5, 5, 5, 5, 5, 5, 5, 5, //0x20
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x30
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x40
				9, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x50
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x60
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x70
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0x80
				5, 5, 5, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 9, 9, 9, //0x90
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0xA0
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0xB0
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0xC0
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 3, 4, 4, //0xD0
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0xE0
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //0xF0
			};

			public int CompareTo(CDTX.CChip other)
			{
				// まずは位置で比較。

				//BGMチップだけ発声位置
				//if( this.nチャンネル番号 == 0x01 || this.nチャンネル番号 == 0x02 )
				//{
				//    if( this.n発声位置 < other.n発声位置 )
				//        return -1;

				//    if( this.n発声位置 > other.n発声位置 )
				//        return 1;
				//}

				//if( this.n発声位置 < other.n発声位置 )
				//    return -1;

				//if( this.n発声位置 > other.n発声位置 )
				//    return 1;

				//譜面解析メソッドV4では発声時刻msで比較する。
				var n発声時刻msCompareToResult = 0;
				n発声時刻msCompareToResult = this.n発声時刻ms.CompareTo(other.n発声時刻ms);
				if (n発声時刻msCompareToResult != 0)
				{
					return n発声時刻msCompareToResult;
				}

				n発声時刻msCompareToResult = this.db発声時刻ms.CompareTo(other.db発声時刻ms);
				if (n発声時刻msCompareToResult != 0)
				{
					return n発声時刻msCompareToResult;
				}

				// 位置が同じなら優先度で比較。
				return n優先度[this.nチャンネル番号].CompareTo(n優先度[other.nチャンネル番号]);
			}
			//-----------------
			#endregion
			/// <summary>
			/// shallow copyです。
			/// </summary>
			/// <returns></returns>
			public object Clone()
			{
				return MemberwiseClone();
			}
		}
		public class CWAV : IDisposable
		{
			public bool bBGMとして使う;
			public List<int> listこのWAVを使用するチャンネル番号の集合 = new List<int>(16);
			public int nチップサイズ = 100;
			public int n位置;
			public long[] n一時停止時刻 = new long[TJAPlayer3.ConfigIni.nPoliphonicSounds];    // 4
			public int SongVol = CSound.DefaultSongVol;
			public LoudnessMetadata? SongLoudnessMetadata = null;
			public int n現在再生中のサウンド番号;
			public long[] n再生開始時刻 = new long[TJAPlayer3.ConfigIni.nPoliphonicSounds];    // 4
			public int n内部番号;
			public int n表記上の番号;
			public CSound[] rSound = new CSound[TJAPlayer3.ConfigIni.nPoliphonicSounds];     // 4
			public string strコメント文 = "";
			public string strファイル名 = "";
			public bool bBGMとして使わない
			{
				get
				{
					return !this.bBGMとして使う;
				}
				set
				{
					this.bBGMとして使う = !value;
				}
			}
			public bool bIsBassSound = false;
			public bool bIsGuitarSound = false;
			public bool bIsDrumsSound = false;
			public bool bIsSESound = false;
			public bool bIsBGMSound = false;

			public override string ToString()
			{
				var sb = new StringBuilder(128);

				if (this.n表記上の番号 == this.n内部番号)
				{
					sb.Append(string.Format("CWAV{0}: ", CDTX.tZZ(this.n表記上の番号)));
				}
				else
				{
					sb.Append(string.Format("CWAV{0}(内部{1}): ", CDTX.tZZ(this.n表記上の番号), this.n内部番号));
				}
				sb.Append(
					$"{nameof(SongVol)}:{this.SongVol}, {nameof(LoudnessMetadata.Integrated)}:{this.SongLoudnessMetadata?.Integrated}, {nameof(LoudnessMetadata.TruePeak)}:{this.SongLoudnessMetadata?.TruePeak}, 位置:{this.n位置}, サイズ:{this.nチップサイズ}, BGM:{(this.bBGMとして使う ? 'Y' : 'N')}, File:{this.strファイル名}, Comment:{this.strコメント文}");

				return sb.ToString();
			}

			#region [ Dispose-Finalize パターン実装 ]
			//-----------------
			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}
			public void Dispose(bool bManagedリソースの解放も行う)
			{
				if (this.bDisposed済み)
					return;

				if (bManagedリソースの解放も行う)
				{
					for (int i = 0; i < TJAPlayer3.ConfigIni.nPoliphonicSounds; i++) // 4
					{
						if (this.rSound[i] != null)
							TJAPlayer3.Sound管理.tサウンドを破棄する(this.rSound[i]);
						this.rSound[i] = null;

						if ((i == 0) && TJAPlayer3.ConfigIni.bLog作成解放ログ出力)
							Trace.TraceInformation("サウンドを解放しました。({0})({1})", this.strコメント文, this.strファイル名);
					}
				}

				this.bDisposed済み = true;
			}
			~CWAV()
			{
				this.Dispose(false);
			}
			//-----------------
			#endregion

			#region [ private ]
			//-----------------
			private bool bDisposed済み;
			//-----------------
			#endregion
		}

		public class DanSongs
		{
			public CTexture TitleTex;
			public CTexture SubTitleTex;
			public string Title;
			public string SubTitle;
			public string FileName;
			public string Genre;
			public int ScoreInit;
			public int ScoreDiff;
			public static int Number;
			public CWAV Wave;

			public DanSongs()
			{
				Number++;
			}
		}

		public struct STLYRIC
		{
			public long Time;
			public Bitmap TextTex;
			public string Text;
			public int index;
		}

		// 構造体

		public struct STLANEINT
		{
			public int Taiko_Red;
			public int Taiko_Blue;

			public int Drums
			{
				get
				{
					return this.Taiko_Red + this.Taiko_Blue;
				}
			}

			public int this[int index]
			{
				get
				{
					switch (index)
					{
						case 0:
							return this.Taiko_Red;

						case 1:
							return this.Taiko_Blue;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					if (value < 0)
					{
						throw new ArgumentOutOfRangeException();
					}
					switch (index)
					{
						case 0:
							this.Taiko_Red = value;
							return;

						case 1:
							this.Taiko_Blue = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}
		public struct STチップがある
		{
			public bool Drums;
			
			public bool Branch;
		}

		public class CLine
		{
			public int n小節番号;
			public int n文字数;
			public int nコース = 0;
		}

		// プロパティ

		//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
		public class CBranchStartInfo
		{
			public int nMeasureCount;
			public double dbTime;
			public double dbBPM;
			public double dbSCROLL;
			public double dbSCROLLY;
			public double dbBMScollTime;
			public double db移動待機時刻;
			public double db出現時刻;
			public double db再生速度;
			public float fMeasure_s;
			public float fMeasure_m;
		}

		/// <summary>
		/// 分岐開始時の情報を記録するためのあれ 2020.04.21
		/// </summary>
		public CBranchStartInfo cBranchStart = new CBranchStartInfo();
		//---------------------

		public int nBGMAdjust
		{
			get;
			private set;
		}
		public bool b分岐を一回でも開始した = false; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加 //2020.04.22 akasoko26 分岐譜面のみ値を代入するように。
		public int nPlayerSide; //2017.08.14 kairera0467 引数で指定する
		public bool bSession譜面を読み込む;

		public string ARTIST;
		public string BACKGROUND;
		public string BACKGROUND_GR;
		public double BASEBPM;
		public double BPM;
		public STチップがある bチップがある;
		public string COMMENT;
		public double db再生速度;
		public string GENRE;
		public bool bLyrics;
		public int[] LEVELtaiko = new int[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1 };
		public Dictionary<int, CVideoDecoder> listVD;
		public Dictionary<int, CBPM> listBPM;
		public List<CChip> listChip;
		public Dictionary<int, CWAV> listWAV;
		public Dictionary<int, CSCROLL> listSCROLL;
		public Dictionary<int, CSCROLL> listSCROLL_Normal;
		public Dictionary<int, CSCROLL> listSCROLL_Expert;
		public Dictionary<int, CSCROLL> listSCROLL_Master;
		public Dictionary<int, CJPOSSCROLL> listJPOSSCROLL;
		public List<DanSongs> List_DanSongs;

		private double[] dbNowSCROLL_Normal;
		private double[] dbNowSCROLL_Expert;
		private double[] dbNowSCROLL_Master;


		public Dictionary<int, CDELAY> listDELAY;
		public Dictionary<int, CBRANCH> listBRANCH;
		public STLANEINT n可視チップ数;
		public string PANEL;
		public string PATH_WAV;
		public string strファイル名;
		public string strファイル名の絶対パス;
		public string strフォルダ名;
		public string EXTENSION;
		public string SUBTITLE;
		public string TITLE;
		public bool SUBTITLEDisp;
		public double dbScrollSpeed;
		public int nデモBGMオフセット;

		private int n現在の小節数 = 1;

		private int nNowRoll = 0;
		private int nNowRollCount = 0;

		private int[] n連打チップ_temp = new int[3];

		public int nOFFSET = 0;
		private bool bOFFSETの値がマイナスである = false;
		private int nMOVIEOFFSET = 0;
		private bool bMOVIEOFFSETの値がマイナスである = false;
		private double dbNowBPM = 120.0;
		public bool[] bHasBranch = new bool[(int)Difficulty.Total] { false, false, false, false, false, false, false };

		//分岐関連
		private int n現在のコース = 0; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加

		private bool b最初の分岐である;
		public int[] nノーツ数 = new int[4]; //0～2:各コース 3:共通
		public int[] nノーツ数_Branch = new int[4]; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
		public int[] n風船数 = new int[4]; //0～2:各コース 3:共通

		private List<CLine> listLine;
		private int nLineCountTemp; //分岐開始時の小節数を記録。
		private int nLineCountCourseTemp = 0; //現在カウント中のコースを記録。

		public int n参照中の難易度 = 3;
		public int nScoreModeTmp = 99; //2017.01.28 DD
		public int[,] nScoreInit = new int[2, (int)Difficulty.Total]; //[ x, y ] x=通常or真打 y=コース
		public int[] nScoreDiff = new int[(int)Difficulty.Total]; //[y]
		public bool[,] b配点が指定されている = new bool[3, (int)Difficulty.Total]; //2017.06.04 kairera0467 [ x, y ] x=通常(Init)or真打orDiff y=コース

		private double dbBarLength;
		public float fNow_Measure_s = 4.0f;
		public float fNow_Measure_m = 4.0f;
		public double dbNowTime = 0.0;
		public double dbNowBMScollTime = 0.0;
		public double dbNowScroll = 1.0;
		public double dbNowScrollY = 0.0; //2016.08.13 kairera0467 複素数スクロール
		public double dbLastTime = 0.0; //直前の小節の開始時間
		public double dbLastBMScrollTime = 0.0;

		public int[] bBARLINECUE = new int[2]; //命令を入れた次の小節の操作を実現するためのフラグ。0 = mainflag, 1 = cuetype
		public bool b小節線を挿入している = false;

		//Normal Regular Masterにしたいけどここは我慢。
		private List<int> listBalloon_Normal;
		private List<int> listBalloon_Expert;
		private List<int> listBalloon_Master;

		public List<Bitmap> listLyric; //2020.05.13 Mr-Ojii 曲読み込み時にテクスチャを生成するために変更
		public List<STLYRIC> listLyric2;

		private int listBalloon_Normal_数値管理;
		private int listBalloon_Expert_数値管理;
		private int listBalloon_Master_数値管理;

		private int nBRANCH現在番号;

		public bool[] b譜面が存在する = new bool[(int)Difficulty.Total];

		private string[] dlmtSpace = { " " };
		private string[] dlmtEnter = { "\n" };
		private string[] dlmtCOURSE = { "COURSE:" };

		private int nスクロール方向 = 0;
		//2015.09.18 kairera0467
		//バタフライスライドみたいなアレをやりたいがために実装。
		//次郎2みたいな複素数とかは意味不明なので、方向を指定してスクロールさせることにした。
		//0:通常
		//1:上
		//2:下
		//3:右上
		//4:右下
		//5:左
		//6:左上
		//7:左下

		public string strBGIMAGE_PATH;
		public string strBGVIDEO_PATH;

		public double db出現時刻;
		public double db移動待機時刻;

		public string strBGM_PATH;
		public int SongVol;
		public LoudnessMetadata? SongLoudnessMetadata;

		public bool bHIDDENBRANCH; //2016.04.01 kairera0467 選曲画面上、譜面分岐開始前まで譜面分岐の表示を隠す
		public bool bGOGOTIME; //2018.03.11 kairera0467

		public bool[] IsBranchBarDraw = new bool[4]; //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加 // 仕様変更により、黄色lineの表示法を変更.2020.04.21.akasoko26
		public bool IsEndedBranching; // BRANCHENDが呼び出されたかどうか
		public Dan_C[] Dan_C;

		public bool IsEnabledFixSENote;
		public int FixSENote;
		public GaugeIncreaseMode GaugeIncreaseMode;

		// コンストラクタ

		public CDTX()
		{
			this.nPlayerSide = 0;
			this.EXTENSION = "";
			this.TITLE = "";
			this.SUBTITLE = "";
			this.SUBTITLEDisp = false;
			this.ARTIST = "";
			this.COMMENT = "";
			this.PANEL = "";
			this.GENRE = "";
			this.bLyrics = false;
			this.BACKGROUND = "";
			this.BACKGROUND_GR = "";
			this.PATH_WAV = "";
			this.BPM = 120.0;
			this.bHIDDENBRANCH = false;
			this.db再生速度 = 1.0;
			this.bチップがある = new STチップがある();
			this.bチップがある.Drums = false;
			this.bチップがある.Branch = false;
			this.strファイル名 = "";
			this.strフォルダ名 = "";
			this.strファイル名の絶対パス = "";
			this.n無限管理SIZE = new int[36 * 36];
			this.listBalloon_Normal_数値管理 = 0;
			this.listBalloon_Expert_数値管理 = 0;
			this.listBalloon_Master_数値管理 = 0;
			this.nBRANCH現在番号 = 0;

			this.nBGMAdjust = 0;
			this.nPolyphonicSounds = TJAPlayer3.ConfigIni.nPoliphonicSounds;

			//this.nScoreModeTmp = 1;
			for (int y = 0; y < (int)Difficulty.Total; y++)
			{
				this.nScoreInit[0, y] = 300;
				this.nScoreInit[1, y] = 1000;
				this.nScoreDiff[y] = 120;
				this.b配点が指定されている[0, y] = false;
				this.b配点が指定されている[1, y] = false;
				this.b配点が指定されている[2, y] = false;
			}

			this.dbBarLength = 1.0;

			this.b最初の分岐である = true;

			this.SongVol = CSound.DefaultSongVol;
			this.SongLoudnessMetadata = null;

			GaugeIncreaseMode = GaugeIncreaseMode.Normal;

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Change default culture to invariant, fixes (Purota)
			Dan_C = new Dan_C[3];
		}
		public CDTX(string strファイル名, bool bヘッダのみ)
			: this()
		{
			this.On活性化();
			this.t入力(strファイル名, bヘッダのみ);
		}
		public CDTX(string strファイル名, bool bヘッダのみ, double db再生速度, int nBGMAdjust)
			: this()
		{
			this.On活性化();
			this.t入力(strファイル名, bヘッダのみ, db再生速度, nBGMAdjust, 0, false);
		}
		public CDTX(string strファイル名, bool bヘッダのみ, double db再生速度, int nBGMAdjust, int nPlayerSide, bool bSession)
			: this()
		{
			this.On活性化();
			this.t入力(strファイル名, bヘッダのみ, db再生速度, nBGMAdjust, nPlayerSide, bSession);
		}


		// メソッド

		public void tAVIの読み込み()
		{
			if (this.listVD != null)
			{
				foreach (CVideoDecoder cvd in this.listVD.Values)
				{
					cvd.InitRead();
					cvd.dbPlaySpeed = ((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0;
				}
			}
			if (!this.bヘッダのみ)//&& this.b動画読み込み )
			{
				foreach (CChip chip in this.listChip)
				{
					if (chip.nチャンネル番号 == 0x54 || chip.nチャンネル番号 == 0x5A)
					{
						chip.eAVI種別 = EAVI種別.Unknown;
						chip.rVD = null;

						CVideoDecoder vd;
						if ((this.listVD.TryGetValue(chip.n整数値, out vd)))
						{
							chip.eAVI種別 = EAVI種別.AVI;
							chip.rVD = vd;
						}
					}
				}
			}
		}

		public void tWave再生位置自動補正()
		{
			foreach (CWAV cwav in this.listWAV.Values)
			{
				this.tWave再生位置自動補正(cwav);
			}
		}
		public void tWave再生位置自動補正(CWAV wc)
		{
			if (wc.rSound[0] != null && wc.rSound[0].n総演奏時間ms >= 5000)
			{
				for (int i = 0; i < nPolyphonicSounds; i++)
				{
					if ((wc.rSound[i] != null) && (wc.rSound[i].b再生中))
					{
						long nCurrentTime = CSound管理.rc演奏用タイマ.nシステム時刻ms;
						if (nCurrentTime > wc.n再生開始時刻[i])
						{
							long nAbsTimeFromStartPlaying = nCurrentTime - wc.n再生開始時刻[i];
							//Trace.TraceInformation( "再生位置自動補正: {0}, seek先={1}ms, 全音長={2}ms",
							//    Path.GetFileName( wc.rSound[ 0 ].strファイル名 ),
							//    nAbsTimeFromStartPlaying,
							//    wc.rSound[ 0 ].n総演奏時間ms
							//);
							// wc.rSound[ i ].t再生位置を変更する( wc.rSound[ i ].t時刻から位置を返す( nAbsTimeFromStartPlaying ) );

							// WASAPI/ASIO用↓
							if (!TJAPlayer3.stage演奏ドラム画面.bPAUSE)
							{
								if (wc.rSound[i].b一時停止中) wc.rSound[i].t再生を再開する(nAbsTimeFromStartPlaying);
								else wc.rSound[i].t再生位置を変更する(nAbsTimeFromStartPlaying);
							}
							else
							{
								wc.rSound[i].t再生を一時停止する();
							}
						}
					}
				}
			}
		}
		public void tWavの再生停止(int nWaveの内部番号)
		{
			tWavの再生停止(nWaveの内部番号, false);
		}
		public void tWavの再生停止(int nWaveの内部番号, bool bミキサーからも削除する)
		{
			if (this.listWAV.TryGetValue(nWaveの内部番号, out CWAV cwav))
			{
				for (int i = 0; i < nPolyphonicSounds; i++)
				{
					if (cwav.rSound[i] != null && cwav.rSound[i].b再生中)
					{
						if (bミキサーからも削除する)
						{
							cwav.rSound[i].tサウンドを停止してMixerからも削除する();
						}
						else
						{
							cwav.rSound[i].t再生を停止する();
						}
					}
				}
			}
		}
		public void tWAVの読み込み(CWAV cwav)
		{
			string str = string.IsNullOrEmpty(this.PATH_WAV) ? this.strフォルダ名 : this.PATH_WAV;
			str = str + cwav.strファイル名;

			try
			{
				#region [ 同時発音数を、チャンネルによって変える ]

				int nPoly = nPolyphonicSounds;
				if (TJAPlayer3.Sound管理.GetCurrentSoundDeviceType() != "OpenAL") // DShowでの再生の場合はミキシング負荷が高くないため、
				{
					// チップのライフタイム管理を行わない
					if (cwav.bIsBassSound) nPoly = (nPolyphonicSounds >= 2) ? 2 : 1;
					else if (cwav.bIsGuitarSound) nPoly = (nPolyphonicSounds >= 2) ? 2 : 1;
					else if (cwav.bIsSESound) nPoly = 1;
					else if (cwav.bIsBGMSound) nPoly = 1;
				}

				if (cwav.bIsBGMSound) nPoly = 1;

				#endregion

				for (int i = 0; i < nPoly; i++)
				{
					try
					{
						cwav.rSound[i] = TJAPlayer3.Sound管理.tサウンドを生成する(str, ESoundGroup.SongPlayback);

						if (!TJAPlayer3.ConfigIni.bDynamicBassMixerManagement)
						{
							cwav.rSound[i].tBASSサウンドをミキサーに追加する();
						}

						if (TJAPlayer3.ConfigIni.bLog作成解放ログ出力)
						{
							Trace.TraceInformation("サウンドを作成しました。({3})({0})({1})({2}bytes)", cwav.strコメント文, str,
								cwav.rSound[0].nサウンドバッファサイズ, cwav.rSound[0].bストリーム再生する ? "Stream" : "OnMemory");
						}
					}
					catch (Exception e)
					{
						cwav.rSound[i] = null;
						Trace.TraceError("サウンドの作成に失敗しました。({0})({1})", cwav.strコメント文, str);
						Trace.TraceError(e.ToString());
					}
				}
			}
			catch (Exception exception)
			{
				Trace.TraceError("サウンドの生成に失敗しました。({0})({1})", cwav.strコメント文, str);
				Trace.TraceError(exception.ToString());

				for (int j = 0; j < nPolyphonicSounds; j++)
				{
					cwav.rSound[j] = null;
				}

				//continue;
			}
		}

		public static string tZZ(int n)
		{
			if (n < 0 || n >= 36 * 36)
				return "!!";    // オーバー／アンダーフロー。

			// n を36進数2桁の文字列にして返す。

			string str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			return new string(new char[] { str[n / 36], str[n % 36] });
		}
		public void t太鼓チップのランダム化(ERandomMode eRandom)
		{
			//2016.02.11 kairera0467
			//なんだよこのクソ実装は(怒)
			Random rnd = new System.Random();

			switch (eRandom)
			{
				case ERandomMode.MIRROR:
					foreach (var chip in this.listChip)
					{
						switch (chip.nチャンネル番号)
						{
							case 0x11:
								chip.nチャンネル番号 = 0x12;
								break;
							case 0x12:
								chip.nチャンネル番号 = 0x11;
								break;
							case 0x13:
								chip.nチャンネル番号 = 0x14;
								chip.nSenote = 6;
								break;
							case 0x14:
								chip.nチャンネル番号 = 0x13;
								chip.nSenote = 5;
								break;
						}
					}
					break;
				case ERandomMode.RANDOM:
					foreach (var chip in this.listChip)
					{
						int n = rnd.Next(50);

						if (n >= 5 && n <= 10)
						{
							switch (chip.nチャンネル番号)
							{
								case 0x11:
									chip.nチャンネル番号 = 0x12;
									break;
								case 0x12:
									chip.nチャンネル番号 = 0x11;
									break;
								case 0x13:
									chip.nチャンネル番号 = 0x14;
									chip.nSenote = 6;
									break;
								case 0x14:
									chip.nチャンネル番号 = 0x13;
									chip.nSenote = 5;
									break;
							}
						}
					}
					break;
				case ERandomMode.SUPERRANDOM:
					foreach (var chip in this.listChip)
					{
						int n = rnd.Next(80);

						if (n >= 3 && n <= 43)
						{
							switch (chip.nチャンネル番号)
							{
								case 0x11:
									chip.nチャンネル番号 = 0x12;
									break;
								case 0x12:
									chip.nチャンネル番号 = 0x11;
									break;
								case 0x13:
									chip.nチャンネル番号 = 0x14;
									chip.nSenote = 6;
									break;
								case 0x14:
									chip.nチャンネル番号 = 0x13;
									chip.nSenote = 5;
									break;
							}
						}
					}
					break;
				case ERandomMode.HYPERRANDOM:
					foreach (var chip in this.listChip)
					{
						int n = rnd.Next(100);

						if (n >= 20 && n <= 80)
						{
							switch (chip.nチャンネル番号)
							{
								case 0x11:
									chip.nチャンネル番号 = 0x12;
									break;
								case 0x12:
									chip.nチャンネル番号 = 0x11;
									break;
								case 0x13:
									chip.nチャンネル番号 = 0x14;
									chip.nSenote = 6;
									break;
								case 0x14:
									chip.nチャンネル番号 = 0x13;
									chip.nSenote = 5;
									break;
							}
						}
					}
					break;
				case ERandomMode.OFF:
				default:
					break;
			}
			if (eRandom != ERandomMode.OFF)
			{
				#region[ list作成 ]
				//ひとまずチップだけのリストを作成しておく。
				List<CDTX.CChip> list音符のみのリスト;
				list音符のみのリスト = new List<CChip>();
				int nCount = 0;
				int dkdkCount = 0;

				foreach (CChip chip in this.listChip)
				{
					if (chip.nチャンネル番号 >= 0x11 && chip.nチャンネル番号 < 0x18)
					{
						list音符のみのリスト.Add(chip);
					}
				}
				#endregion

				this.tSenotes_Core_V2(list音符のみのリスト);

			}


		}

		#region [ チップの再生と停止 ]
		public void tチップの再生(CChip pChip, long n再生開始システム時刻ms)
		{
			if (TJAPlayer3.ConfigIni.b演奏速度が一倍速であるとき以外音声を再生しない && TJAPlayer3.ConfigIni.n演奏速度 != 20)
				return;

			if (pChip.n整数値_内部番号 >= 0)
			{
				if (this.listWAV.TryGetValue(pChip.n整数値_内部番号, out CWAV wc))
				{
					int index = wc.n現在再生中のサウンド番号 = (wc.n現在再生中のサウンド番号 + 1) % nPolyphonicSounds;
					if ((wc.rSound[0] != null) &&
						(wc.rSound[0].bストリーム再生する || wc.rSound[index] == null))
					{
						index = wc.n現在再生中のサウンド番号 = 0;
					}
					CSound sound = wc.rSound[index];
					if (sound != null)
					{
						sound.db再生速度 = ((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0;
						// 再生速度によって、WASAPI/ASIOで使う使用mixerが決まるため、付随情報の設定(音量/PAN)は、再生速度の設定後に行う

						// 2018-08-27 twopointzero - DON'T attempt to load (or queue scanning) loudness metadata here.
						//                           This code is called right after loading the .tja, and that code
						//                           will have just made such an attempt.
						TJAPlayer3.SongGainController.Set(wc.SongVol, wc.SongLoudnessMetadata, sound);

						sound.n位置 = wc.n位置;
						sound.t再生を開始する();
					}
					wc.n再生開始時刻[wc.n現在再生中のサウンド番号] = n再生開始システム時刻ms;
					this.tWave再生位置自動補正(wc);
				}
			}
		}
		public void t各自動再生音チップの再生時刻を変更する(int nBGMAdjustの増減値)
		{
			this.nBGMAdjust += nBGMAdjustの増減値;
			for (int i = 0; i < this.listChip.Count; i++)
			{
				int nChannelNumber = this.listChip[i].nチャンネル番号;
				if (((
						(nChannelNumber == 1) ||
						((0x61 <= nChannelNumber) && (nChannelNumber <= 0x69))
					  ) ||
						((0x70 <= nChannelNumber) && (nChannelNumber <= 0x79))
					) ||
					(((0x80 <= nChannelNumber) && (nChannelNumber <= 0x89)) || ((0x90 <= nChannelNumber) && (nChannelNumber <= 0x92)))
				  )
				{
					this.listChip[i].n発声時刻ms += nBGMAdjustの増減値;
				}
			}
			foreach (CWAV cwav in this.listWAV.Values)
			{
				for (int j = 0; j < nPolyphonicSounds; j++)
				{
					if ((cwav.rSound[j] != null) && cwav.rSound[j].b再生中)
					{
						cwav.n再生開始時刻[j] += nBGMAdjustの増減値;
					}
				}
			}
		}
		public void t全チップの再生一時停止()
		{
			foreach (CWAV cwav in this.listWAV.Values)
			{
				for (int i = 0; i < nPolyphonicSounds; i++)
				{
					if ((cwav.rSound[i] != null) && cwav.rSound[i].b再生中)
					{
						cwav.rSound[i].t再生を一時停止する();
						cwav.n一時停止時刻[i] = CSound管理.rc演奏用タイマ.nシステム時刻ms;
					}
				}
			}
		}
		public void t全チップの再生再開()
		{
			foreach (CWAV cwav in this.listWAV.Values)
			{
				for (int i = 0; i < nPolyphonicSounds; i++)
				{
					if ((cwav.rSound[i] != null) && cwav.rSound[i].b一時停止中)
					{
						//long num1 = cwav.n一時停止時刻[ i ];
						//long num2 = cwav.n再生開始時刻[ i ];
						cwav.rSound[i].t再生を再開する(cwav.n一時停止時刻[i] - cwav.n再生開始時刻[i]);
						cwav.n再生開始時刻[i] += CSound管理.rc演奏用タイマ.nシステム時刻ms - cwav.n一時停止時刻[i];
					}
				}
			}
		}
		public void t全チップの再生停止()
		{
			foreach (CWAV cwav in this.listWAV.Values)
			{
				this.tWavの再生停止(cwav.n内部番号);
			}
		}
		public void t全チップの再生停止とミキサーからの削除()
		{
			foreach (CWAV cwav in this.listWAV.Values)
			{
				this.tWavの再生停止(cwav.n内部番号, true);
			}
		}
		#endregion

		public void t入力(string strファイル名, bool bヘッダのみ)
		{
			this.t入力(strファイル名, bヘッダのみ, 1.0, 0, 0, false);
		}
		public void t入力(string strファイル名, bool bヘッダのみ, double db再生速度, int nBGMAdjust, int nPlayerSide, bool bSession)
		{
			this.bヘッダのみ = bヘッダのみ;
			this.strファイル名の絶対パス = Path.GetFullPath(strファイル名);
			this.strファイル名 = Path.GetFileName(this.strファイル名の絶対パス);
			this.strフォルダ名 = Path.GetDirectoryName(this.strファイル名の絶対パス) + @"\";
			//if ( this.e種別 != E種別.SMF )

			try
			{
				this.EXTENSION = Path.GetExtension(strファイル名);
				this.nPlayerSide = nPlayerSide;
				this.bSession譜面を読み込む = bSession;
				if (Path.GetExtension(strファイル名).Equals(".tci"))
				{
					Encoding ファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(strファイル名);
					StreamReader reader = new StreamReader(strファイル名, ファイルenc);
					string tcistr = reader.ReadToEnd();
					reader.Close();

					this.t入力tci(tcistr, db再生速度, nBGMAdjust);

				}
				else if (Path.GetExtension(strファイル名).Equals(".tcm"))
				{
					Encoding ファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(strファイル名);
					StreamReader reader = new StreamReader(strファイル名, ファイルenc);
					string tcistr = reader.ReadToEnd();
					reader.Close();

					this.t入力tcm(tcistr, db再生速度, nBGMAdjust);

				}
				else
				{
					//次郎方式
					Encoding ファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(strファイル名);

					StreamReader reader = new StreamReader(strファイル名, ファイルenc);
					string str2 = reader.ReadToEnd();
					reader.Close();

					this.t入力_全入力文字列から(str2, db再生速度, nBGMAdjust);
				}

			}
			catch (Exception ex)
			{
				//MessageBox.Show( "おや?エラーが出たようです。お兄様。" );
				Trace.TraceError("おや?エラーが出たようです。お兄様。");
				Trace.TraceError(ex.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (79ff8639-9b3c-477f-bc4a-f2eea9784860)");

			}
		}
		public void t入力_全入力文字列から( string str1, double db再生速度, int nBGMAdjust)
		{
			//DateTime timeBeginLoad = DateTime.Now;
			//TimeSpan span;

			if (!string.IsNullOrEmpty(str1))
			{
				#region [ 入力/行解析 ]
				#region[初期化]
				this.db再生速度 = db再生速度;
				for (int j = 0; j < 36 * 36; j++)
				{
					this.n無限管理SIZE[j] = -j;
				}
				this.n内部番号WAV1to = 1;
				this.n内部番号BPM1to = 1;
				this.dbNowScroll = 1.0;
				this.dbNowSCROLL_Normal = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Expert = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Master = new double[] { 1.0, 0.0 };
				this.n現在のコース = 0;
				#endregion
				if (this.listChip.Count == 0)
				{
					try
					{
						this.t入力_V4(str1);
					}
					catch (Exception e)
					{
						Trace.WriteLine(e.ToString());
					}
				}

				#endregion

				チップについての共通部分(nBGMAdjust);
				
			}
		}

		private void チップについての共通部分(int nBGMAdjust)
		{
			#region[コピペ]
			//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
			//Trace.TraceInformation( "抜き出し時間:             {0}", span.ToString() );
			//timeBeginLoad = DateTime.Now;
			this.n無限管理SIZE = null;
			//this.t入力_行解析ヘッダ( str1 );
			if (!this.bヘッダのみ)
			{
				#region [ BPM/BMP初期化 ]
				int ch;
				CBPM cbpm = null;
				foreach (CBPM cbpm2 in this.listBPM.Values)
				{
					if (cbpm2.n表記上の番号 == 0)
					{
						cbpm = cbpm2;
						break;
					}
				}
				if (cbpm == null)
				{
					cbpm = new CBPM();
					cbpm.n内部番号 = this.n内部番号BPM1to++;
					cbpm.n表記上の番号 = 0;
					cbpm.dbBPM値 = 120.0;
					this.listBPM.Add(cbpm.n内部番号, cbpm);
					CChip chip = new CChip();
					chip.n発声位置 = 0;
					chip.nチャンネル番号 = 8;      // 拡張BPM
					chip.n整数値 = 0;
					chip.n整数値_内部番号 = cbpm.n内部番号;
					this.listChip.Insert(0, chip);
				}
				else
				{
					CChip chip = new CChip();
					chip.n発声位置 = 0;
					chip.nチャンネル番号 = 8;      // 拡張BPM
					chip.n整数値 = 0;
					chip.n整数値_内部番号 = cbpm.n内部番号;
					this.listChip.Insert(0, chip);
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "前準備完了時間:           {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ CWAV初期化 ]
				foreach (CWAV cwav in this.listWAV.Values)
				{
					if (cwav.nチップサイズ < 0)
					{
						cwav.nチップサイズ = 100;
					}
					if (cwav.n位置 <= -10000)
					{
						cwav.n位置 = 0;
					}
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "CWAV前準備時間:           {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ チップ倍率設定 ]						// #28145 2012.4.22 yyagi 二重ループを1重ループに変更して高速化)
				//foreach ( CWAV cwav in this.listWAV.Values )
				//{
				//    foreach( CChip chip in this.listChip )
				//    {
				//        if( chip.n整数値_内部番号 == cwav.n内部番号 )
				//        {
				//            chip.dbチップサイズ倍率 = ( (double) cwav.nチップサイズ ) / 100.0;
				//            if (chip.nチャンネル番号 == 0x01 )	// BGMだったら
				//            {
				//                cwav.bIsOnBGMLane = true;
				//            }
				//        }
				//    }
				//}
				foreach (CChip chip in this.listChip)
				{
					if (this.listWAV.TryGetValue(chip.n整数値_内部番号, out CWAV cwav))
					//foreach ( CWAV cwav in this.listWAV.Values )
					{
						//	if ( chip.n整数値_内部番号 == cwav.n内部番号 )
						//	{
						chip.dbチップサイズ倍率 = ((double)cwav.nチップサイズ) / 100.0;
						//if ( chip.nチャンネル番号 == 0x01 )	// BGMだったら
						//{
						//	cwav.bIsOnBGMLane = true;
						//}
						//	}
					}
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "CWAV全準備時間:           {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ 必要に応じて空打ち音を0小節に定義する ]
				//for ( int m = 0xb1; m <= 0xbc; m++ )			// #28146 2012.4.21 yyagi; bb -> bc
				//{
				//    foreach ( CChip chip in this.listChip )
				//    {
				//        if ( chip.nチャンネル番号 == m )
				//        {
				//            CChip c = new CChip();
				//            c.n発声位置 = 0;
				//            c.nチャンネル番号 = chip.nチャンネル番号;
				//            c.n整数値 = chip.n整数値;
				//            c.n整数値_内部番号 = chip.n整数値_内部番号;
				//            this.listChip.Insert( 0, c );
				//            break;
				//        }
				//    }
				//}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "空打確認時間:             {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ 拍子_拍線の挿入 ]
				if (this.listChip.Count > 0)
				{
					this.listChip.Sort();       // 高速化のためにはこれを削りたいが、listChipの最後がn発声位置の終端である必要があるので、
												// 保守性確保を優先してここでのソートは残しておく
												// なお、093時点では、このソートを削除しても動作するようにはしてある。
												// (ここまでの一部チップ登録を、listChip.Add(c)から同Insert(0,c)に変更してある)
												// これにより、数ms程度ながらここでのソートも高速化されている。

					//double barlength = 1.0;
					//int nEndOfSong = ( this.listChip[ this.listChip.Count - 1 ].n発声位置 + 384 ) - ( this.listChip[ this.listChip.Count - 1 ].n発声位置 % 384 );
					//for ( int tick384 = 0; tick384 <= nEndOfSong; tick384 += 384 )	// 小節線の挿入　(後に出てくる拍子線とループをまとめようとするなら、forループの終了条件の微妙な違いに注意が必要)
					//{
					//    CChip chip = new CChip();
					//    chip.n発声位置 = tick384;
					//    chip.nチャンネル番号 = 0x50;	// 小節線
					//    chip.n整数値 = 36 * 36 - 1;
					//    chip.dbSCROLL = 1.0;
					//    this.listChip.Add( chip );
					//}
					////this.listChip.Sort();				// ここでのソートは不要。ただし最後にソートすること
					//int nChipNo_BarLength = 0;
					//int nChipNo_C1 = 0;

					//this.listChip.Sort();
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "拍子_拍線挿入時間:       {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ C2 [拍線_小節線表示指定] の処理 ]		// #28145 2012.4.21 yyagi; 2重ループをほぼ1重にして高速化
				bool bShowBeatBarLine = true;
				for (int i = 0; i < this.listChip.Count; i++)
				{
					bool bChangedBeatBarStatus = false;
					if ((this.listChip[i].nチャンネル番号 == 0xc2))
					{
						if (this.listChip[i].n整数値 == 1)             // BAR/BEAT LINE = ON
						{
							bShowBeatBarLine = true;
							bChangedBeatBarStatus = true;
						}
						else if (this.listChip[i].n整数値 == 2)            // BAR/BEAT LINE = OFF
						{
							bShowBeatBarLine = false;
							bChangedBeatBarStatus = true;
						}
					}
					int startIndex = i;
					if (bChangedBeatBarStatus)                          // C2チップの前に50/51チップが来ている可能性に配慮
					{
						while (startIndex > 0 && this.listChip[startIndex].n発声位置 == this.listChip[i].n発声位置)
						{
							startIndex--;
						}
						startIndex++;   // 1つ小さく過ぎているので、戻す
					}
					for (int j = startIndex; j <= i; j++)
					{
						if (((this.listChip[j].nチャンネル番号 == 0x50) || (this.listChip[j].nチャンネル番号 == 0x51)) &&
							(this.listChip[j].n整数値 == (36 * 36 - 1)))
						{
							this.listChip[j].b可視 = bShowBeatBarLine;
						}
					}
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "C2 [拍線_小節線表示指定]:  {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				this.n内部番号JSCROLL1to = 0;
				#region [ 発声時刻の計算 ]
				double bpm = 120.0;
				//double dbBarLength = 1.0;
				int n発声位置 = 0;
				int ms = 0;
				int nBar = 0;
				int nCount = 0;
				this.nNowRollCount = 0;

				List<STLYRIC> tmplistlyric = new List<STLYRIC>();
				int BGM番号 = 0;

				foreach (CChip chip in this.listChip)
				{
					if (chip.nチャンネル番号 == 0x02) { }
					//else if( chip.nチャンネル番号 == 0x03 ){}
					else if (chip.nチャンネル番号 == 0x01) { }
					else if (chip.nチャンネル番号 == 0x08) { }
					else if (chip.nチャンネル番号 >= 0x11 && chip.nチャンネル番号 <= 0x1F) { }
					else if (chip.nチャンネル番号 == 0x50) { }
					else if (chip.nチャンネル番号 == 0x51) { }
					else if (chip.nチャンネル番号 == 0x54) { }
					else if (chip.nチャンネル番号 == 0x08) { }
					else if (chip.nチャンネル番号 == 0xF1) { }
					else if (chip.nチャンネル番号 == 0xF2) { }
					else if (chip.nチャンネル番号 == 0xFF) { }
					else if (chip.nチャンネル番号 == 0xDD) { chip.n発声時刻ms = ms + ((int)(((625 * (chip.n発声位置 - n発声位置)) * this.dbBarLength) / bpm)); }
					else if (chip.nチャンネル番号 == 0xDF) { chip.n発声時刻ms = ms + ((int)(((625 * (chip.n発声位置 - n発声位置)) * this.dbBarLength) / bpm)); }
					else if (chip.nチャンネル番号 < 0x93)
						chip.n発声時刻ms = ms + ((int)(((625 * (chip.n発声位置 - n発声位置)) * this.dbBarLength) / bpm));
					else if ((chip.nチャンネル番号 > 0x9F && chip.nチャンネル番号 < 0xA0) || (chip.nチャンネル番号 >= 0xF0 && chip.nチャンネル番号 < 0xFE))
						chip.n発声時刻ms = ms + ((int)(((625 * (chip.n発声位置 - n発声位置)) * this.dbBarLength) / bpm));
					nBar = chip.n発声位置 / 384;
					ch = chip.nチャンネル番号;

					nCount++;
					this.nNowRollCount++;



					switch (ch)
					{
						case 0x01:  // BGM
							{
								n発声位置 = chip.n発声位置;

								if (this.bOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nOFFSET;
								ms = chip.n発声時刻ms;

								#region[listlyric2の時間合わせ]
								for (int ind = 0; ind < listLyric2.Count; ind++)
								{
									if (listLyric2[ind].index == BGM番号)
									{
										STLYRIC lyrictmp = this.listLyric2[ind];

										lyrictmp.Time += chip.n発声時刻ms;

										tmplistlyric.Add(lyrictmp);
									}
								}


								BGM番号++;
								#endregion
								continue;
							}
						case 0x02:  // BarLength
							{
								n発声位置 = chip.n発声位置;
								if (this.bOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nOFFSET;
								ms = chip.n発声時刻ms;
								dbBarLength = chip.db実数値;
								continue;
							}
						case 0x03:  // BPM
							{
								n発声位置 = chip.n発声位置;
								if (this.bOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nOFFSET;
								ms = chip.n発声時刻ms;
								bpm = this.BASEBPM + chip.n整数値;
								this.dbNowBPM = bpm;
								continue;
							}
						case 0x04:  // BGA (レイヤBGA1)
						case 0x07:  // レイヤBGA2
							break;

						case 0x15:
						case 0x16:
						case 0x17:
							{
								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
									chip.nノーツ終了時刻ms += this.nOFFSET;
								}

								this.nNowRoll = this.nNowRollCount - 1;
								continue;
							}
						case 0x18:
							{
								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
								}
								continue;
							}

						case 0x55:
						case 0x56:
						case 0x57:
						case 0x58:
						case 0x59:
						case 0x60:
							break;

						case 0x50:
							{
								if (this.bOFFSETの値がマイナスである)
									chip.n発声時刻ms += this.nOFFSET;

								continue;
							}

						case 0x05:  // Extended Object (非対応)
						case 0x06:  // Missアニメ (非対応)
						case 0x5A:  // 未定義
						case 0x5b:  // 未定義
						case 0x5c:  // 未定義
						case 0x5d:  // 未定義
						case 0x5e:  // 未定義
						case 0x5f:  // 未定義
							{
								continue;
							}
						case 0x08:  // 拡張BPM
							{
								n発声位置 = chip.n発声位置;
								if (this.bOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nOFFSET;
								ms = chip.n発声時刻ms;
								if (this.listBPM.TryGetValue(chip.n整数値_内部番号, out CBPM cBPM))
								{
									bpm = (cBPM.n表記上の番号 == 0 ? 0.0 : this.BASEBPM) + cBPM.dbBPM値;
									this.dbNowBPM = bpm;
								}
								continue;
							}
						case 0x54:  // 動画再生
							{
								if (this.bOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nOFFSET;
								if (this.bMOVIEOFFSETの値がマイナスである == false)
									chip.n発声時刻ms += this.nMOVIEOFFSET;
								else
									chip.n発声時刻ms -= this.nMOVIEOFFSET;
								continue;
							}
						case 0x97:
						case 0x98:
						case 0x99:
							{
								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
									chip.nノーツ終了時刻ms += this.nOFFSET;
								}

								//chip.dbBPM = this.dbNowBPM;
								//chip.dbSCROLL = this.dbNowSCROLL;
								this.nNowRoll = this.nNowRollCount - 1;

								//chip.nノーツ終了時刻ms = ms + ( (int) ( ( ( 0x271 * ( chip.nノーツ終了位置 - n発声位置 ) ) * dbBarLength ) / bpm ) );

								#region[チップ番号を記録]
								//switch(chip.nコース)
								//{
								//    case 0:
								//        this.n連打チップ_temp[0] = this.nNowRoll;
								//        this.dbSCROLL_temp[0] = this.dbNowSCROLL;
								//        break;
								//    case 1:
								//        this.n連打チップ_temp[1] = this.nNowRoll;
								//        this.dbSCROLL_temp[1] = this.dbNowSCROLL;
								//        break;
								//    case 2:
								//        this.n連打チップ_temp[2] = this.nNowRoll;
								//        this.dbSCROLL_temp[2] = this.dbNowSCROLL;
								//        break;
								//}

								#endregion

								continue;
							}
						case 0x9A:
							{

								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
								}
								//chip.n発声時刻ms += this.nDELAY;
								//chip.dbBPM = this.dbNowBPM;
								//chip.dbSCROLL = this.dbNowSCROLL;

								#region[チップ番号を記録]
								//風船は現時点では未実装のため処理しない。


								//switch (chip.nコース)
								//{
								//    case 0:
								//        if (this.listChip[this.n連打チップ_temp[0]].nチャンネル番号 == 0x99) break;
								//        this.listChip[this.n連打チップ_temp[0]].nノーツ終了時刻ms = chip.n発声時刻ms;
								//        this.listChip[this.n連打チップ_temp[0]].dbSCROLL = this.dbSCROLL_temp[0];
								//        break;
								//    case 1:
								//        if (this.listChip[this.n連打チップ_temp[1]].nチャンネル番号 == 0x99) break;
								//        this.listChip[this.n連打チップ_temp[1]].nノーツ終了時刻ms = chip.n発声時刻ms;
								//        this.listChip[this.n連打チップ_temp[1]].dbSCROLL = this.dbSCROLL_temp[1];
								//        break;
								//    case 2:
								//        if (this.listChip[this.n連打チップ_temp[2]].nチャンネル番号 == 0x99) break;
								//        this.listChip[this.n連打チップ_temp[2]].nノーツ終了時刻ms = chip.n発声時刻ms;
								//        this.listChip[this.n連打チップ_temp[2]].dbSCROLL = this.dbSCROLL_temp[2];
								//        break;
								//}

								#endregion

								//this.listChip[this.nNowRoll].nノーツ終了時刻ms = chip.n発声時刻ms;
								//this.listChip[this.nNowRoll].dbSCROLL = this.dbNowSCROLL;
								//this.listChip[this.nNowRoll].dbBPM = this.dbNowBPM;
								continue;
							}
						case 0x9D:
							{
								//if ( this.listSCROLL.ContainsKey( chip.n整数値_内部番号 ) )
								//{
								//this.dbNowSCROLL = ( ( this.listSCROLL[ chip.n整数値_内部番号 ].n表記上の番号 == 0 ) ? 0.0 : 1.0 ) + this.listSCROLL[ chip.n整数値_内部番号 ].dbSCROLL値;
								//}

								//switch (chip.nコース)
								//{
								//    case 0:
								//        this.dbNowSCROLL_Normal = this.dbNowSCROLL;
								//        this.n現在のコース = 0;
								//        break;
								//    case 1:
								//        this.dbNowSCROLL_Expert = this.dbNowSCROLL;
								//        this.n現在のコース = 1;
								//        break;
								//    case 2:
								//        this.dbNowSCROLL_Master = this.dbNowSCROLL;
								//        this.n現在のコース = 2;
								//        break;
								//}

								continue;
							}
						case 0xDC:
							{
								if (this.bOFFSETの値がマイナスである)
									chip.n発声時刻ms += this.nOFFSET;
								//if ( this.listDELAY.ContainsKey( chip.n整数値_内部番号 ) )
								//{
								//    this.nDELAY = ( ( this.listDELAY[ chip.n整数値_内部番号 ].n表記上の番号 == 0 ) ? 0 : 0 ) + this.listDELAY[ chip.n整数値_内部番号 ].nDELAY値;
								//}
								continue;
							}
						case 0xDE:
							{
								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
									if (this.listBRANCH.ContainsKey(chip.n整数値_内部番号))
									{
										this.listBRANCH[chip.n整数値_内部番号].db分岐時刻ms += this.nOFFSET; 
									}
								}
								this.n現在のコース = chip.nコース;
								continue;
							}
						case 0x52: //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
							{
								if (this.bOFFSETの値がマイナスである)
								{
									chip.n発声時刻ms += this.nOFFSET;
								}
								this.n現在のコース = chip.nコース;
								continue;
							}
						case 0xDF:
							{
								if (this.bOFFSETの値がマイナスである)
									chip.n発声時刻ms += this.nOFFSET;
								//if ( this.listBRANCH.ContainsKey( chip.n整数値_内部番号 ) )
								//{
								//this.listBRANCH[chip.n整数値_内部番号].db分岐時間ms = chip.n発声時刻ms + ( this.bOFFSETの値がマイナスである ? this.nOFFSET : 0 );
								//}

								continue;
							}
						case 0xE0:
							{
								//if (this.bOFFSETの値がマイナスである)
								//    chip.n発声時刻ms += this.nOFFSET;

								//chip.dbBPM = this.dbNowBPM;
								//chip.dbSCROLL = this.dbNowSCROLL;
								//if( chip.n整数値_内部番号 == 1 )
								//    this.bBarLine = false;
								//else
								//    this.bBarLine = true;
								continue;
							}
						default:
							{
								if (this.bOFFSETの値がマイナスである)
									chip.n発声時刻ms += this.nOFFSET;
								//chip.n発声時刻ms += this.nDELAY;
								chip.dbBPM = this.dbNowBPM;
								//chip.dbSCROLL = this.dbNowSCROLL;
								continue;
							}
					}
				}
				if (this.db再生速度 > 0.0)
				{
					foreach (CChip chip in this.listChip)
					{
						chip.n発声時刻ms = (int)(((double)chip.n発声時刻ms) / this.db再生速度);
						chip.db発声時刻ms = (((double)chip.n発声時刻ms) / this.db再生速度);
						chip.nノーツ終了時刻ms = (int)(((double)chip.nノーツ終了時刻ms) / this.db再生速度);
					}
				}
				#endregion

				#region[listlyricを時間順に並び替え。]
				this.listLyric2 = tmplistlyric;
				this.listLyric2.Sort((a, b) => a.Time.CompareTo(b.Time));
				#endregion

				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "発声時刻計算:             {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				this.nBGMAdjust = 0;
				this.t各自動再生音チップの再生時刻を変更する(nBGMAdjust);
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "再生時刻変更:             {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ 可視チップ数カウント ]
				for (int n = 0; n < 2; n++)
				{
					this.n可視チップ数[n] = 0;
				}
				foreach (CChip chip in this.listChip)
				{
					int c = chip.nチャンネル番号;
					if ((0x11 <= c) && (c <= 0x14))
					{
						if (c == 0x11 || c == 0x13)
							this.n可視チップ数.Taiko_Red++;
						else if (c == 0x12 || c == 0x14)
							this.n可視チップ数.Taiko_Blue++;
					}
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "可視チップ数カウント      {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region [ チップの種類を分類し、対応するフラグを立てる ]
				foreach (CChip chip in this.listChip)
				{
					if ((chip.nチャンネル番号 == 0x01 && this.listWAV.TryGetValue(chip.n整数値_内部番号, out CWAV cwav)) && !cwav.listこのWAVを使用するチャンネル番号の集合.Contains(chip.nチャンネル番号))
					{
						cwav.listこのWAVを使用するチャンネル番号の集合.Add(chip.nチャンネル番号);

						int c = chip.nチャンネル番号 >> 4;
						switch (c)
						{
							case 0x01:
								cwav.bIsDrumsSound = true; break;
							case 0x02:
								cwav.bIsGuitarSound = true; break;
							case 0x0A:
								cwav.bIsBassSound = true; break;
							case 0x06:
							case 0x07:
							case 0x08:
							case 0x09:
								cwav.bIsSESound = true; break;
							case 0x00:
								if (chip.nチャンネル番号 == 0x01)
								{
									cwav.bIsBGMSound = true; break;
								}
								break;
						}
					}
				}
				#endregion
				//span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
				//Trace.TraceInformation( "ch番号集合確認:           {0}", span.ToString() );
				//timeBeginLoad = DateTime.Now;
				#region[ seNotes計算 ]
				if (this.listBRANCH.Count != 0)
					this.tSetSenotes_branch();
				else
					this.tSetSenotes();

				#endregion
				#region [ bLogDTX詳細ログ出力 ]
				if (TJAPlayer3.ConfigIni.bLogDTX詳細ログ出力)
				{
					foreach (CWAV cwav in this.listWAV.Values)
					{
						Trace.TraceInformation(cwav.ToString());
					}
					foreach (CBPM cbpm3 in this.listBPM.Values)
					{
						Trace.TraceInformation(cbpm3.ToString());
					}
					foreach (CChip chip in this.listChip)
					{
						Trace.TraceInformation(chip.ToString());
					}
				}
				#endregion

				//ソートっぽい
				//this.listChip.Sort(delegate(CChip pchipA, CChip pchipB) { return pchipA.n発声時刻ms - pchipB.n発声時刻ms; } );
				//Random ran1 = new Random();
				//for (int n = 0; n < this.listChip.Count; n++ )
				//{

				//    if (CDTXMania.ConfigIni.bHispeedRandom)
				//    {

				//        int nRan = ran1.Next(5, 40);
				//        this.listChip[n].dbSCROLL = nRan / 10.0;
				//    }
				//}
				int n整数値管理 = 0;
				foreach (CChip chip in this.listChip)
				{
					if (chip.nチャンネル番号 != 0x54)
						chip.n整数値 = n整数値管理;
					n整数値管理++;
				}

			}
			#endregion
		}

		private void t入力tcm(string 入力文字列,　double 再生速度,int nBGMAdjust) {
			if (!string.IsNullOrEmpty(入力文字列))
			{
				#region [ 初期化 ]
				this.db再生速度 = 再生速度;
				for (int j = 0; j < 36 * 36; j++)
				{
					this.n無限管理SIZE[j] = -j;
				}
				this.n内部番号WAV1to = 1;
				this.n内部番号BPM1to = 1;
				this.dbNowScroll = 1.0;
				this.dbNowSCROLL_Normal = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Expert = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Master = new double[] { 1.0, 0.0 };
				this.n現在のコース = 0;
				#endregion
				
				OTCMedley obj = JsonConvert.DeserializeObject<OTCMedley>(入力文字列);
				
				if (obj.Jouken != null)
					for (int joukenindex = 0; joukenindex < Math.Min(obj.Jouken.Length,3); joukenindex++)
					{
						if (!string.IsNullOrEmpty(obj.Jouken[joukenindex].Type) && !string.IsNullOrEmpty(obj.Jouken[joukenindex].Range) && obj.Jouken[joukenindex].Value[0] != null)
						{
							Exam.Type examType;
							Exam.Range examRange;
							int[] examValue;
							switch (obj.Jouken[joukenindex].Type)
							{
								case "gauge":
									examType = Exam.Type.Gauge;
									break;
								case "judgeperfect":
									examType = Exam.Type.JudgePerfect;
									break;
								case "judgegood":
									examType = Exam.Type.JudgeGood;
									break;
								case "judgebad":
									examType = Exam.Type.JudgeBad;
									break;
								case "score":
									examType = Exam.Type.Score;
									break;
								case "roll":
									examType = Exam.Type.Roll;
									break;
								case "hit":
									examType = Exam.Type.Hit;
									break;
								case "combo":
									examType = Exam.Type.Combo;
									break;
								default:
									examType = Exam.Type.Gauge;
									break;
							}
							switch (obj.Jouken[joukenindex].Range)
							{
								case "more":
									examRange = Exam.Range.More;
									break;
								case "less":
									examRange = Exam.Range.Less;
									break;
								default:
									examRange = Exam.Range.More;
									break;
							}
							if (obj.Jouken[joukenindex].Value[1] != null)
							{
								try
								{
									examValue = new int[] {(int) obj.Jouken[joukenindex].Value[0], (int)obj.Jouken[joukenindex].Value[1] };
								}
								catch (Exception)
								{
									try
									{
										examValue = new int[] { (int)obj.Jouken[joukenindex].Value[0], (int) obj.Jouken[joukenindex].Value[0] };
									}
									catch (Exception)
									{
										examValue = new int[] { 100, 100 };
									}
								}
							}
							else
							{
								try
								{
									examValue = new int[] {(int) obj.Jouken[joukenindex].Value[0], (int)obj.Jouken[joukenindex].Value[0] };
								}
								catch (Exception)
								{
									examValue = new int[] { 100, 100 };
								}
							}

							Dan_C[joukenindex] = new Dan_C(examType, examValue, examRange);
						}
					}
				
				this.b譜面が存在する[(int)Difficulty.Dan] = true;

				#region[ 最初の処理 ]
				//1小節の時間を挿入して開始時間を調節。
				this.dbNowTime += ((15000.0 / 120.0 * (4.0 / 4.0)) * 16.0);
				#endregion

				#region[#START命令の挿入]
				//#STARTと同時に鳴らすのはどうかと思うけどしゃーなしだな。
				AddMusicPreTimeMs(); // 音源を鳴らす前に遅延。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x01;
				chip.n発声位置 = 384;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.fBMSCROLLTime = this.dbNowBMScollTime;
				chip.n整数値 = 0x01;
				chip.n整数値_内部番号 = 1;

				// チップを配置。
				this.listChip.Add(chip);

				var chip1 = new CChip();
				chip1.nチャンネル番号 = 0x54;
				//chip1.n発声位置 = 384;
				//chip1.n発声時刻ms = (int)this.dbNowTime;
				if (this.nMOVIEOFFSET == 0)
					chip1.n発声時刻ms = (int)this.dbNowTime;
				else
					chip1.n発声時刻ms = (int)this.nMOVIEOFFSET;
				chip1.dbBPM = this.dbNowBPM;
				chip1.dbSCROLL = this.dbNowScroll;
				chip1.fNow_Measure_m = this.fNow_Measure_m;
				chip1.fNow_Measure_s = this.fNow_Measure_s;
				chip1.n整数値 = 0x01;
				chip1.n整数値_内部番号 = 1;
				chip1.nPlayerSide = this.nPlayerSide;
				chip1.eAVI種別 = EAVI種別.AVI;

				// チップを配置。

				this.listChip.Add(chip1);
				#endregion

				#region[一応、TJAで最初に入ってるやつ]
				#region[BPM]
				double dbBPM = Convert.ToDouble(120);
				this.BPM = dbBPM;
				this.BASEBPM = dbBPM;
				this.dbNowBPM = dbBPM;

				this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = this.n内部番号BPM1to - 1, dbBPM値 = dbBPM, });
				this.n内部番号BPM1to++;


				//チップ追加して割り込んでみる。
				var chipbpm = new CChip();

				chipbpm.nチャンネル番号 = 0x03;
				chipbpm.n発声位置 = ((this.n現在の小節数 - 1) * 384);
				chipbpm.n整数値 = 0x00;
				chipbpm.n整数値_内部番号 = 1;

				this.listChip.Add(chip);
				#endregion

				#region[DEMOSTART]
				this.nデモBGMオフセット = 0;
				#endregion

				#region[LEVEL]
				var level = (int)Convert.ToDouble(5);
				this.LEVELtaiko[(int)Difficulty.Dan] = level;
				#endregion
				#endregion

				if (obj.Humen != null)
					for (int humenindex = 0; humenindex < obj.Humen.Length; humenindex++) {


						string 読み込みtci名 = this.strフォルダ名 + obj.Humen[humenindex].File;
						int diff = strConvertCourse(obj.Humen[humenindex].Diff);

						Encoding ファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(読み込みtci名);
						StreamReader reader = new StreamReader(読み込みtci名, ファイルenc);
						string tcistr = reader.ReadToEnd();
						reader.Close();

						#region[段位道場の幕]
						var delayTime = 6200.0; // 6.2秒ディレイ
												//チップ追加して割り込んでみる。
						chip = new CChip();

						chip.nチャンネル番号 = 0x9B;
						chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
						chip.n発声時刻ms = (int)this.dbNowTime;
						chip.fNow_Measure_m = this.fNow_Measure_m;
						chip.fNow_Measure_s = this.fNow_Measure_s;
						this.dbNowTime += delayTime;
						this.dbNowBMScollTime += delayTime * this.dbNowBPM / 15000;
						chip.n整数値_内部番号 = 0;
						chip.nコース = this.n現在のコース;

						// チップを配置。
						this.listChip.Add(chip);

						AddMusicPreTimeMs(); // 段位の幕が開いてからの遅延。


						OTCInfomation objtci = JsonConvert.DeserializeObject<OTCInfomation>(tcistr);

						int n譜面数 = 0;

						int[] coursesindex = new int[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1 };

						for (int i = 0; i < objtci.Courses.Length; i++)
						{
							this.n参照中の難易度 = strConvertCourse(objtci.Courses[i].Diffculty);
							coursesindex[n参照中の難易度] = i;
							n譜面数++;
						}

						string 読み込むtccファイル = this.strフォルダ名 + objtci.Courses[coursesindex[diff]].Single;

						Encoding ファイルenctcc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(読み込むtccファイル);
						StreamReader readertcc = new StreamReader(読み込むtccファイル, ファイルenctcc);
						string tccstr = readertcc.ReadToEnd();
						readertcc.Close();

						OTCCource objtcc = JsonConvert.DeserializeObject<OTCCource>(tccstr);

						var dansongs = new DanSongs();
						if (objtci.SubTitle != null)
						{
							if (objtci.SubTitle.StartsWith("++") || objtci.SubTitle.StartsWith("--"))
							{
								objtci.SubTitle = objtci.SubTitle.Substring(3);
							}
						}
						else {
							objtci.SubTitle = "";
						}
						dansongs.Title = objtci.Title;
						dansongs.SubTitle = objtci.SubTitle;
						dansongs.Genre = "";
						dansongs.FileName = objtci.WAVFile;
						dansongs.ScoreInit = (int)objtcc.ScoreInit;
						dansongs.ScoreDiff = (int)objtcc.ScoreDiff;
						dansongs.Wave = new CWAV
						{
							n内部番号 = this.n内部番号WAV1to,
							n表記上の番号 = this.n内部番号WAV1to,
							nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
							n位置 = 0,
							SongVol = this.SongVol,
							SongLoudnessMetadata = this.SongLoudnessMetadata,
							strファイル名 = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, dansongs.FileName),
							strコメント文 = "TJA BGM"
						};
						dansongs.Wave.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(dansongs.Wave.strファイル名);
						List_DanSongs.Add(dansongs);
						this.listWAV.Add(this.n内部番号WAV1to, dansongs.Wave);
						this.n内部番号WAV1to++;

						var nextSongnextSongChip = new CChip();

						nextSongnextSongChip.nチャンネル番号 = 0x01;
						nextSongnextSongChip.n発声位置 = 384;
						nextSongnextSongChip.n発声時刻ms = (int)this.dbNowTime;
						nextSongnextSongChip.fNow_Measure_m = this.fNow_Measure_m;
						nextSongnextSongChip.fNow_Measure_s = this.fNow_Measure_s;
						nextSongnextSongChip.n整数値 = 0x01;
						nextSongnextSongChip.n整数値_内部番号 = 1 + List_DanSongs.Count;

						this.listWAV[1].strファイル名 = "";

						// チップを配置。
						this.listChip.Add(nextSongnextSongChip);
						#endregion


						t入力tci_tcm用(tcistr, 再生速度, coursesindex[diff]);
					}
				
				#region[#END命令の挿入]
				//ためしに割り込む。
				chip = new CChip();

				chip.nチャンネル番号 = 0xFF;
				chip.n発声位置 = ((this.n現在の小節数 + 2) * 384);
				//chip.n発声時刻ms = (int)( this.dbNowTime + ((15000.0 / this.dbNowBPM * ( 4.0 / 4.0 )) * 16.0) * 2  );
				chip.n発声時刻ms = (int)(this.dbNowTime + 1000); //2016.07.16 kairera0467 終了時から1秒後に設置するよう変更。
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値 = 0xFF;
				chip.n整数値_内部番号 = 1;
				// チップを配置。

				this.listChip.Add(chip);
				#endregion

				チップについての共通部分(nBGMAdjust);
				this.TITLE = obj.Title;
			}
		}

		private void t入力tci_tcm用(string 入力文字列, double 再生速度, int cindex) {

			OTCInfomation obj = JsonConvert.DeserializeObject<OTCInfomation>(入力文字列);

			#region[BGM]
			if (strBGM_PATH != null)
			{
				Trace.TraceWarning($"{nameof(CDTX)} is ignoring an extra WAVE header in {this.strファイル名の絶対パス}");
			}
			else
			{
				this.strBGM_PATH = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, obj.WAVFile);
				//tbWave.Text = strCommandParam;
				if (this.listWAV != null)
				{
					// 2018-08-27 twopointzero - DO attempt to load (or queue scanning) loudness metadata here.
					//                           TJAP3 is either launching, enumerating songs, or is about to
					//                           begin playing a song. If metadata is available, we want it now.
					//                           If is not yet available then we wish to queue scanning.
					var absoluteBgmPath = Path.Combine(this.strフォルダ名, this.strBGM_PATH);
					this.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(absoluteBgmPath);

					var wav = new CWAV()
					{
						n内部番号 = this.n内部番号WAV1to,
						n表記上の番号 = 1,
						nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
						n位置 = 0,
						SongVol = this.SongVol,
						SongLoudnessMetadata = this.SongLoudnessMetadata,
						strファイル名 = this.strBGM_PATH,
						strコメント文 = "TJA BGM",
					};

					this.listWAV.Add(this.n内部番号WAV1to, wav);
					this.n内部番号WAV1to++;
				}
			}
			#endregion

			if (obj.BPM != null) //BPMCHANGEもかませる
			{
				double dbBPM = Convert.ToDouble(obj.BPM);
				this.dbNowBPM = dbBPM;

				this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = 0, dbBPM値 = dbBPM, bpm_change_time = this.dbNowTime, bpm_change_bmscroll_time = this.dbNowBMScollTime, bpm_change_course = this.n現在のコース });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x08;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip.dbBPM = dbBPM;
				chip.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip);

				var chip1 = new CChip();
				chip1.nチャンネル番号 = 0x9C;
				chip1.n発声位置 = ((this.n現在の小節数) * 384);
				chip1.n発声時刻ms = (int)this.dbNowTime;
				chip1.fNow_Measure_m = this.fNow_Measure_m;
				chip1.fNow_Measure_s = this.fNow_Measure_s;
				chip1.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip1.dbBPM = dbBPM;
				chip1.dbSCROLL = this.dbNowScroll;
				chip1.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip1);

				this.n内部番号BPM1to++;
			}

			if (obj.Offset != null) //TJAと同じようにDELAYをかませる
			{
				double nDELAY = (Convert.ToDouble(obj.Offset) * 1000.0);


				this.listDELAY.Add(this.n内部番号DELAY1to, new CDELAY() { n内部番号 = this.n内部番号DELAY1to, n表記上の番号 = 0, nDELAY値 = (int)nDELAY, delay_bmscroll_time = this.dbLastBMScrollTime, delay_bpm = this.dbNowBPM, delay_course = this.n現在のコース, delay_time = this.dbLastTime });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xDC;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.db発声時刻ms = this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.nコース = this.n現在のコース;
				chip.n整数値_内部番号 = this.n内部番号DELAY1to;
				chip.fBMSCROLLTime = this.dbNowBMScollTime;
				// チップを配置。

				this.dbNowTime += nDELAY;
				this.dbNowBMScollTime += nDELAY * this.dbNowBPM / 15000;

				this.listChip.Add(chip);
				this.n内部番号DELAY1to++;
			}

			#region[4/4拍子にする]
			string[] strArray = "4/4".Split('/');
			WarnSplitLength("#MEASURE subsplit", strArray, 2);

			double[] dbLength = new double[2];
			dbLength[0] = Convert.ToDouble(strArray[0]);
			dbLength[1] = Convert.ToDouble(strArray[1]);

			double db小節長倍率 = dbLength[0] / dbLength[1];
			this.dbBarLength = db小節長倍率;
			this.fNow_Measure_m = (float)dbLength[1];
			this.fNow_Measure_s = (float)dbLength[0];

			var chipme = new CChip();

			chipme.nチャンネル番号 = 0x02;
			chipme.n発声位置 = ((this.n現在の小節数) * 384);
			chipme.n発声時刻ms = (int)this.dbNowTime;
			chipme.fNow_Measure_m = this.fNow_Measure_m;
			chipme.fNow_Measure_s = this.fNow_Measure_s;
			chipme.dbSCROLL = this.dbNowScroll;
			chipme.db実数値 = db小節長倍率;
			chipme.n整数値_内部番号 = 1;
			// チップを配置。

			this.listChip.Add(chipme);

			//lbMaster.Items.Add( ";拍子変更 " + strArray[0] + "/" + strArray[1] );
			#endregion

			#region[scrollを1にする]
			double dbSCROLL = Convert.ToDouble(1);
			this.dbNowScroll = dbSCROLL;
			this.dbNowScrollY = 0.0;

			this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = dbSCROLL, dbSCROLL値Y = 0.0 });

			switch (this.n現在のコース)
			{
				case 0:
					this.dbNowSCROLL_Normal[0] = dbSCROLL;
					break;
				case 1:
					this.dbNowSCROLL_Expert[0] = dbSCROLL;
					break;
				case 2:
					this.dbNowSCROLL_Master[0] = dbSCROLL;
					break;
			}

			//チップ追加して割り込んでみる。
			var chipsc = new CChip();

			chipsc.nチャンネル番号 = 0x9D;
			chipsc.n発声位置 = ((this.n現在の小節数) * 384) - 1;
			chipsc.n発声時刻ms = (int)this.dbNowTime;
			chipsc.fNow_Measure_m = this.fNow_Measure_m;
			chipsc.fNow_Measure_s = this.fNow_Measure_s;
			chipsc.n整数値_内部番号 = this.n内部番号SCROLL1to;
			chipsc.dbSCROLL = dbSCROLL;
			chipsc.dbSCROLL_Y = 0.0;
			chipsc.nコース = this.n現在のコース;

			// チップを配置。

			this.listChip.Add(chipsc);

			this.n内部番号SCROLL1to++;
			#endregion

			t入力tccファイル(obj.Courses[cindex].Single);
		}


		private void t入力tci(string 入力文字列, double 再生速度,int nBGMAdjust)
		{
			if (!string.IsNullOrEmpty(入力文字列))
			{
				#region [ 初期化 ]
				this.db再生速度 = 再生速度;
				for (int j = 0; j < 36 * 36; j++)
				{
					this.n無限管理SIZE[j] = -j;
				}
				this.n内部番号WAV1to = 1;
				this.n内部番号BPM1to = 1;
				this.dbNowScroll = 1.0;
				this.dbNowSCROLL_Normal = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Expert = new double[] { 1.0, 0.0 };
				this.dbNowSCROLL_Master = new double[] { 1.0, 0.0 };
				this.n現在のコース = 0;
				#endregion

				if (this.listChip.Count == 0)
				{
					try
					{
						this.t入力tciファイル(入力文字列);
					}
					catch (Exception ex)
					{
						Debug.Print(ex.ToString());
					}
				}
				チップについての共通部分(nBGMAdjust);
			}
		}

		private void t入力tciファイル(string 全入力文字列) 
		{
			OTCInfomation obj = JsonConvert.DeserializeObject<OTCInfomation>(全入力文字列);

			#region[タイトル&サブタイ]
			this.TITLE = obj.Title;
			if (obj.SubTitle != null) {
				if (obj.SubTitle.StartsWith("--"))
				{
					this.SUBTITLE = obj.SubTitle.Substring(2);
				}
				else if (obj.SubTitle.StartsWith("++"))
				{
					this.SUBTITLEDisp = true;
					this.SUBTITLE = obj.SubTitle.Substring(2);
				}
				else
				{
					this.SUBTITLEDisp = true;
					this.SUBTITLE = obj.SubTitle;
				}
			}
			#endregion

			#region[BGM]
			if (strBGM_PATH != null)
			{
				Trace.TraceWarning($"{nameof(CDTX)} is ignoring an extra WAVE header in {this.strファイル名の絶対パス}");
			}
			else
			{
				this.strBGM_PATH = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, obj.WAVFile);
				//tbWave.Text = strCommandParam;
				if (this.listWAV != null)
				{
					// 2018-08-27 twopointzero - DO attempt to load (or queue scanning) loudness metadata here.
					//                           TJAP3 is either launching, enumerating songs, or is about to
					//                           begin playing a song. If metadata is available, we want it now.
					//                           If is not yet available then we wish to queue scanning.
					var absoluteBgmPath = Path.Combine(this.strフォルダ名, this.strBGM_PATH);
					this.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(absoluteBgmPath);

					var wav = new CWAV()
					{
						n内部番号 = this.n内部番号WAV1to,
						n表記上の番号 = 1,
						nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
						n位置 = 0,
						SongVol = this.SongVol,
						SongLoudnessMetadata = this.SongLoudnessMetadata,
						strファイル名 = this.strBGM_PATH,
						strコメント文 = "TJA BGM",
					};

					this.listWAV.Add(this.n内部番号WAV1to, wav);
					this.n内部番号WAV1to++;
				}
			}
			#endregion

			#region[BPM]
			if (obj.BPM != null)
			{
				this.BPM = (double)obj.BPM;
				this.BASEBPM = (double)obj.BPM;
				this.dbNowBPM = (double)obj.BPM;
			}
			else 
			{
				this.BPM = 120.0;
				this.BASEBPM = 120.0;
				this.dbNowBPM = 120.0;
			}

			this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = this.n内部番号BPM1to - 1, dbBPM値 = this.dbNowBPM, });
			this.n内部番号BPM1to++;

			//チップ追加して割り込んでみる。
			var chip = new CChip();

			chip.nチャンネル番号 = 0x03;
			chip.n発声位置 = ((this.n現在の小節数 - 1) * 384);
			chip.n整数値 = 0x00;
			chip.n整数値_内部番号 = 1;

			this.listChip.Add(chip);
			#endregion

			#region[OFFSET]
			if (obj.Offset != null)
				this.nOFFSET = (int)((double)obj.Offset * 1000);
			else
				this.nOFFSET = 0;
			this.bOFFSETの値がマイナスである = this.nOFFSET < 0 ? false : true;	//2020.07.04 Mr-Ojii OTC規格ではOFFSETがTJAと逆なので真と偽が逆になる。

			this.listBPM[0].bpm_change_bmscroll_time = -2000 * this.dbNowBPM / 15000;
			if (this.bOFFSETの値がマイナスである == false)						//↑trueとfalseが逆になるのでここの比較も逆
				this.nOFFSET = this.nOFFSET * -1; //OFFSETは秒を加算するので、必ず正の数にすること。 
			#endregion

			#region[MOVIEOFFSET]
			if (obj.MVOffset != null)
				this.nMOVIEOFFSET = (int)(obj.MVOffset * 1000);
			else
				this.nMOVIEOFFSET = 0;
			this.bMOVIEOFFSETの値がマイナスである = this.nMOVIEOFFSET < 0 ? true : false;
			
			if (this.bMOVIEOFFSETの値がマイナスである == true)
				this.nMOVIEOFFSET = this.nMOVIEOFFSET * -1; //OFFSETは秒を加算するので、必ず正の数にすること。
			#endregion

			#region[DEMOSTART]
			this.nデモBGMオフセット = 0;
			#endregion

			#region[BGIMAGE or BGMOVIE]
			Regex IMAGEEX = new Regex(@"\.png|\.jpg|\.jpeg", RegexOptions.Multiline | RegexOptions.Compiled);
			Regex MOVIEEX = new Regex(@"\.avi|\.mp4", RegexOptions.Multiline | RegexOptions.Compiled);
			Match im =IMAGEEX.Match(Path.GetExtension(this.strフォルダ名 + obj.BGFile));
			if (im.Success)
			{
				if (!string.IsNullOrEmpty(obj.BGFile))
				{
					this.strBGIMAGE_PATH = obj.BGFile;
				}
			}
			else { 
				Match mo = MOVIEEX.Match(Path.GetExtension(this.strフォルダ名 + obj.BGFile));
				if (mo.Success) {
					if (!string.IsNullOrEmpty(obj.BGFile))
					{
						this.strBGVIDEO_PATH =
							CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, obj.BGFile);
					}

					string strVideoFilename;
					if (!string.IsNullOrEmpty(this.PATH_WAV))
						strVideoFilename = this.PATH_WAV + this.strBGVIDEO_PATH;
					else
						strVideoFilename = this.strフォルダ名 + this.strBGVIDEO_PATH;

					CVideoDecoder vd = new CVideoDecoder(TJAPlayer3.app.Device, strVideoFilename);

					if (this.listVD.ContainsKey(1))
						this.listVD.Remove(1);

					this.listVD.Add(1, vd);
				}
			}

			//ここにIMAGEやMOVIEの

			#endregion

			for (int i = 0; i < b譜面が存在する.Length; i++)
				this.b譜面が存在する[i] = false;

			int n読み込むコース = 3;
			int n譜面数 = 0;

			int[] coursesindex = new int[(int)Difficulty.Total] {-1,-1,-1,-1,-1,-1,-1};
			
			for (int i = 0; i < obj.Courses.Length; i++)
			{
				this.n参照中の難易度 = strConvertCourse(obj.Courses[i].Diffculty);
				coursesindex[n参照中の難易度] = i;
				this.b譜面が存在する[n参照中の難易度] = true;
				if (obj.Courses[i].Level != null)
					this.LEVELtaiko[this.n参照中の難易度] = (int)obj.Courses[i].Level;
				else
					this.LEVELtaiko[this.n参照中の難易度] = 10;
				n譜面数++;
			}

			#region[ 読み込ませるコースを決定 ]
			if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//2020.05.12 Mr-Ojii 起動直後の曲読み込みでエラーを吐くので対策
				n読み込むコース = TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayerSide];
			
			if (this.b譜面が存在する[n読み込むコース] == false)
			{
				n読み込むコース++;
				for (int n = 1; n < (int)Difficulty.Total; n++)
				{
					if (this.b譜面が存在する[n読み込むコース] == false)
					{
						n読み込むコース++;
						if (n読み込むコース > (int)Difficulty.Total - 1)
							n読み込むコース = 0;
					}
					else
						break;
				}
			}

			//Seseragi255様のプルリクより変更

			//多難易度選択が可能になったので、セッション譜面は同じ難易度再生の時以外はお預けにしておく
			int n読み込むセッション譜面パート = 0;
			if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//2020.05.12 Mr-Ojii 起動直後の曲読み込みでエラーを吐くので対策
				if (TJAPlayer3.ConfigIni.nPlayerCount >= 2 && TJAPlayer3.stage選曲.n確定された曲の難易度[0] == TJAPlayer3.stage選曲.n確定された曲の難易度[1])
					n読み込むセッション譜面パート = nPlayerSide + 1;

			if (n読み込むセッション譜面パート >= 1) 
			{
				if (obj.Courses[coursesindex[n読み込むコース]].Multiple == null ||  obj.Courses[coursesindex[n読み込むコース]].Multiple.Length < n読み込むセッション譜面パート || string.IsNullOrEmpty(obj.Courses[coursesindex[n読み込むコース]].Multiple[n読み込むセッション譜面パート - 1])) {
					n読み込むセッション譜面パート = 0;
				}
			}
			#endregion

			this.n参照中の難易度 = strConvertCourse(obj.Courses[coursesindex[n読み込むコース]].Diffculty);

			//2020.06.09 tcmにtccのコードを再利用するには、ここにつけるしかなかったんだ～許して～
			#region[ 最初の処理 ]
			//1小節の時間を挿入して開始時間を調節。
			this.dbNowTime += ((15000.0 / 120.0 * (4.0 / 4.0)) * 16.0);
			#endregion

			#region[#START命令の挿入]
			//#STARTと同時に鳴らすのはどうかと思うけどしゃーなしだな。
			AddMusicPreTimeMs(); // 音源を鳴らす前に遅延。
			chip = new CChip();

			chip.nチャンネル番号 = 0x01;
			chip.n発声位置 = 384;
			chip.n発声時刻ms = (int)this.dbNowTime;
			chip.fNow_Measure_m = this.fNow_Measure_m;
			chip.fNow_Measure_s = this.fNow_Measure_s;
			chip.fBMSCROLLTime = this.dbNowBMScollTime;
			chip.n整数値 = 0x01;
			chip.n整数値_内部番号 = 1;

			// チップを配置。
			this.listChip.Add(chip);

			var chip1 = new CChip();
			chip1.nチャンネル番号 = 0x54;
			//chip1.n発声位置 = 384;
			//chip1.n発声時刻ms = (int)this.dbNowTime;
			if (this.nMOVIEOFFSET == 0)
				chip1.n発声時刻ms = (int)this.dbNowTime;
			else
				chip1.n発声時刻ms = (int)this.nMOVIEOFFSET;
			chip1.dbBPM = this.dbNowBPM;
			chip1.dbSCROLL = this.dbNowScroll;
			chip1.fNow_Measure_m = this.fNow_Measure_m;
			chip1.fNow_Measure_s = this.fNow_Measure_s;
			chip1.n整数値 = 0x01;
			chip1.n整数値_内部番号 = 1;
			chip1.nPlayerSide = this.nPlayerSide;
			chip1.eAVI種別 = EAVI種別.AVI;

			// チップを配置。

			this.listChip.Add(chip1);
			#endregion

			if (n読み込むセッション譜面パート == 0)
			{
				t入力tccファイル(obj.Courses[coursesindex[n読み込むコース]].Single);
			}
			else
			{
				t入力tccファイル(obj.Courses[coursesindex[n読み込むコース]].Multiple[n読み込むセッション譜面パート - 1]);
			}

			#region[#END命令の挿入]
			//ためしに割り込む。
			chip = new CChip();

			chip.nチャンネル番号 = 0xFF;
			chip.n発声位置 = ((this.n現在の小節数 + 2) * 384);
			//chip.n発声時刻ms = (int)( this.dbNowTime + ((15000.0 / this.dbNowBPM * ( 4.0 / 4.0 )) * 16.0) * 2  );
			chip.n発声時刻ms = (int)(this.dbNowTime + 1000); //2016.07.16 kairera0467 終了時から1秒後に設置するよう変更。
			chip.fNow_Measure_m = this.fNow_Measure_m;
			chip.fNow_Measure_s = this.fNow_Measure_s;
			chip.n整数値 = 0xFF;
			chip.n整数値_内部番号 = 1;
			// チップを配置。

			this.listChip.Add(chip);
			#endregion

		}

		private void t入力tccファイル(string strファイル相対パス) 
		{
			string 読み込むtccファイル = this.strフォルダ名 + strファイル相対パス;

			Encoding ファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(読み込むtccファイル);
			StreamReader reader = new StreamReader(読み込むtccファイル, ファイルenc);
			string tccstr = reader.ReadToEnd();
			reader.Close();

			OTCCource obj = JsonConvert.DeserializeObject<OTCCource>(tccstr);

			this.nScoreModeTmp = 2;

			if (obj.ScoreInit != null)
			{
				this.nScoreInit[0, this.n参照中の難易度] = Convert.ToInt16(obj.ScoreInit);
				this.b配点が指定されている[0, this.n参照中の難易度] = true;
			}

			if (obj.ScoreDiff != null)
			{
				this.nScoreDiff[this.n参照中の難易度] = Convert.ToInt16(obj.ScoreDiff);
				this.b配点が指定されている[1, this.n参照中の難易度] = true;
			}

			if (obj.Balloon != null)
			{
				for (int n = 0; n < obj.Balloon.Length; n++)
				{
					if (obj.Balloon[n] != null)
						listBalloon_Normal.Add((int)obj.Balloon[n]);
					else
						listBalloon_Normal.Add(5);
				}
			}
			this.n現在の小節数 = 1;

			try
			{

				for (int i = 0; obj.Measures.Length > i; i++)//小節ごとに分けてInputする
				{
					this.t入力tcc小節(obj.Measures[i]);
				}

			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (2da1e880-6b63-4e82-b018-bf18c3568335)");
			}
		}

		private void t入力tcc小節(string[] 小節ごとInput) {

			int n文字数 = 0;
			for (int i = 0; i < 小節ごとInput.Length; i++) {//1小節にいくつの音符があるかどうかの計算は先にする。
				if (!小節ごとInput[i].StartsWith("#"))
					n文字数 += 小節ごとInput[i].Length;
			}

			#region[小節線など]
			if (this.b小節線を挿入している == false)
			{
				// 小節線にもやってあげないと
				// IsEndedBranchingがfalseで1回
				// trueで3回だよ3回
				for (int i = 0; i < (IsEndedBranching == true ? 3 : 1); i++)
				{
					CChip chip = new CChip();
					chip.n発声位置 = ((this.n現在の小節数) * 384);
					chip.nチャンネル番号 = 0x50;
					chip.n発声時刻ms = (int)this.dbNowTime;
					chip.n整数値 = this.n現在の小節数;
					chip.n整数値_内部番号 = this.n現在の小節数;
					chip.dbBPM = this.dbNowBPM;
					chip.fNow_Measure_m = this.fNow_Measure_m;
					chip.fNow_Measure_s = this.fNow_Measure_s;
					chip.IsEndedBranching = IsEndedBranching;
					chip.dbSCROLL = this.dbNowScroll;
					chip.dbSCROLL_Y = this.dbNowScrollY;
					chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;

					if (IsEndedBranching)
						chip.nコース = i;
					else
						chip.nコース = n現在のコース;

					if (this.bBARLINECUE[0] == 1)
					{
						chip.b可視 = false;
					}
					#region [ 作り直し ]  
					if (IsEndedBranching)
					{
						if (this.IsBranchBarDraw[i])
							chip.bBranch = true;
					}
					else
					{
						if (this.IsBranchBarDraw[(int)n現在のコース])
							chip.bBranch = true;
					}
					#endregion

					this.listChip.Add(chip);

					#region [ 作り直し ]  
					if (IsEndedBranching)
						this.IsBranchBarDraw[i] = false;
					else this.IsBranchBarDraw[(int)n現在のコース] = false;
					#endregion
				}

				this.dbLastTime = this.dbNowTime;

				#region[ 拍線チップテスト ]
				//1拍の時間を計算
				double db1拍 = (60.0 / this.dbNowBPM) / 4.0;
				//forループ(拍数)
				for (int measure = 1; measure < this.fNow_Measure_s; measure++)
				{
					CChip hakusen = new CChip();
					hakusen.n発声位置 = ((this.n現在の小節数) * 384);
					hakusen.n発声時刻ms = (int)(this.dbNowTime + (((db1拍 * 4.0)) * measure) * 1000.0);
					hakusen.nチャンネル番号 = 0x51;
					//hakusen.n発声時刻ms = (int)this.dbNowTime;
					hakusen.fBMSCROLLTime = this.dbNowBMScollTime;
					hakusen.n整数値_内部番号 = this.n現在の小節数;
					hakusen.n整数値 = 0;
					hakusen.dbBPM = this.dbNowBPM;
					hakusen.dbSCROLL = this.dbNowScroll;
					hakusen.fNow_Measure_m = this.fNow_Measure_m;
					hakusen.fNow_Measure_s = this.fNow_Measure_s;
					hakusen.dbSCROLL_Y = this.dbNowScrollY;
					hakusen.nコース = this.n現在のコース;

					this.listChip.Add(hakusen);
					//--全ての拍線の時間を出力する--
					//Trace.WriteLine( string.Format( "|| {0,3:##0} Time:{1} Beat:{2}", this.n現在の小節数, hakusen.n発声時刻ms, measure ) );
					//--------------------------------
				}

				#endregion
			}
			#endregion

			for (int i = 0; i < 小節ごとInput.Length; i++)
			{
				string 今回の文字列 = 小節ごとInput[i];
				if (今回の文字列.StartsWith("#"))
				{
					this.t命令を挿入するtcc(今回の文字列);
				}
				else 
				{
					this.t入力tcc音符(今回の文字列 , n文字数);
				}
			}
			this.n現在の小節数++;

		}

		private void t入力tcc音符(string str音符,int n文字数) {

			for (int n = 0; n < str音符.Length; n++)
			{

				int nObjectNum = this.CharConvertNote(str音符.Substring(n, 1));

				if (nObjectNum != 0)
				{
					if ((nObjectNum >= 5 && nObjectNum <= 7) || nObjectNum == 9)
					{
						if (nNowRoll != 0)
						{
							this.dbNowTime += (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));
							this.dbNowBMScollTime += (double)((this.dbBarLength) * (16.0 / n文字数));
							continue;
						}
						else
						{
							this.nNowRollCount = listChip.Count;
							nNowRoll = nObjectNum;
						}
					}

					for (int i = 0; i < (IsEndedBranching == true ? 3 : 1); i++) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに修正
					{
						// IsEndedBranchingがfalseで1回
						// trueで3回だよ3回
						var chip = new CChip();

						chip.IsMissed = false;
						chip.bHit = false;
						chip.b可視 = true;
						chip.bShow = true;
						chip.nチャンネル番号 = 0x10 + nObjectNum;
						//chip.n発声位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
						chip.n発声位置 = (int)((this.n現在の小節数 * 384.0) + ((384.0 * n) / n文字数));
						chip.db発声位置 = this.dbNowTime;
						chip.n発声時刻ms = (int)this.dbNowTime;
						//chip.fBMSCROLLTime = (float)(( this.dbBarLength ) * (16.0f / this.n各小節の文字数[this.n現在の小節数]));
						chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;
						chip.n整数値 = nObjectNum;
						chip.n整数値_内部番号 = 1;
						chip.IsEndedBranching = IsEndedBranching;
						chip.fNow_Measure_m = this.fNow_Measure_m;
						chip.fNow_Measure_s = this.fNow_Measure_s;
						chip.dbBPM = this.dbNowBPM;
						chip.dbSCROLL = this.dbNowScroll;
						chip.dbSCROLL_Y = this.dbNowScrollY;
						chip.nスクロール方向 = this.nスクロール方向;
						if (IsEndedBranching)
							chip.nコース = i;
						else
							chip.nコース = n現在のコース;
						chip.e楽器パート = E楽器パート.TAIKO;
						chip.nノーツ出現時刻ms = (int)(this.db出現時刻 * 1000.0);
						chip.nノーツ移動開始時刻ms = (int)(this.db移動待機時刻 * 1000.0);
						chip.nPlayerSide = this.nPlayerSide;
						chip.bGOGOTIME = this.bGOGOTIME;

						if (nObjectNum == 7 || nObjectNum == 9)
						{
							//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに修正
							switch (chip.nコース)
							{
								case 0:
									if (this.listBalloon_Normal.Count == 0)
									{
										chip.nBalloon = 5;
										break;
									}

									if (this.listBalloon_Normal.Count > this.listBalloon_Normal_数値管理)
									{
										chip.nBalloon = this.listBalloon_Normal[this.listBalloon_Normal_数値管理];
										this.listBalloon_Normal_数値管理++;
										break;
									}
									//else if( this.listBalloon.Count != 0 )
									//{
									//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
									//    this.listBalloon_Normal_数値管理++;
									//    break;
									//}
									break;
								case 1:
									if (this.listBalloon_Expert.Count == 0)
									{
										chip.nBalloon = 5;
										break;
									}

									if (this.listBalloon_Expert.Count > this.listBalloon_Expert_数値管理)
									{
										chip.nBalloon = this.listBalloon_Expert[this.listBalloon_Expert_数値管理];
										this.listBalloon_Expert_数値管理++;
										break;
									}
									//else if( this.listBalloon.Count != 0 )
									//{
									//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
									//    this.listBalloon_Normal_数値管理++;
									//    break;
									//}
									break;
								case 2:
									if (this.listBalloon_Master.Count == 0)
									{
										chip.nBalloon = 5;
										break;
									}

									if (this.listBalloon_Master.Count > this.listBalloon_Master_数値管理)
									{
										chip.nBalloon = this.listBalloon_Master[this.listBalloon_Master_数値管理];
										this.listBalloon_Master_数値管理++;
										break;
									}
									//else if( this.listBalloon.Count != 0 )
									//{
									//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
									//    this.listBalloon_Normal_数値管理++;
									//    break;
									//}
									break;
							}
						}
						if (nObjectNum == 8)
						{
							chip.nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
							chip.nノーツ終了時刻ms = (int)this.dbNowTime;
							chip.fBMSCROLLTime_end = (float)this.dbNowBMScollTime;

							chip.nノーツ出現時刻ms = listChip[nNowRollCount].nノーツ出現時刻ms;
							chip.nノーツ移動開始時刻ms = listChip[nNowRollCount].nノーツ移動開始時刻ms;

							chip.n連打音符State = nNowRoll;
							listChip[nNowRollCount].nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
							listChip[nNowRollCount].nノーツ終了時刻ms = (int)this.dbNowTime;
							listChip[nNowRollCount].fBMSCROLLTime_end = (int)this.dbNowBMScollTime;
							//listChip[ nNowRollCount ].dbBPM = this.dbNowBPM;
							//listChip[ nNowRollCount ].dbSCROLL = this.dbNowSCROLL;
							nNowRoll = 0;
							//continue;
						}

						if (IsEnabledFixSENote)
						{
							chip.IsFixedSENote = true;
							chip.nSenote = FixSENote - 1;
						}

						#region[ 固定される種類のsenotesはここで設定しておく。 ]
						switch (nObjectNum)
						{
							case 3:
								chip.nSenote = 5;
								break;
							case 4:
								chip.nSenote = 6;
								break;
							case 5:
								chip.nSenote = 7;
								break;
							case 6:
								chip.nSenote = 0xA;
								break;
							case 7:
								chip.nSenote = 0xB;
								break;
							case 8:
								chip.nSenote = 0xC;
								break;
							case 9:
								chip.nSenote = 0xD;
								break;
							case 10:
								chip.nSenote = 0xE;
								break;
							case 11:
								chip.nSenote = 0xF;
								break;
						}
						#endregion

						if (nObjectNum < 5) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
						{
							#region [ 作り直し ]
							//譜面分岐がない譜面でも値は加算されてしまうがしゃあない
							//分岐を開始しない間は共通譜面としてみなす。

							if (this.b分岐を一回でも開始した)//一回も分岐していないのに加算させるのはおかしいだろ
							{
								if (IsEndedBranching)
									this.nノーツ数_Branch[i]++;
								else this.nノーツ数_Branch[chip.nコース]++;
							}
							if (!this.b分岐を一回でも開始した)
							{
								//IsEndedBranching==false = forloopが行われていないときのみ
								for (int l = 0; l < 3; l++)
									this.nノーツ数_Branch[l]++;
							}

							this.nノーツ数[3]++;
							#endregion
						}
						else if (nObjectNum == 7)
						{
							if (this.b最初の分岐である == false)
								this.n風船数[this.n現在のコース]++;
							else
								this.n風船数[3]++;
						}


						this.listChip.Add(chip);

					}
				}

				if (IsEnabledFixSENote) IsEnabledFixSENote = false;

				this.dbLastTime = this.dbNowTime;
				this.dbLastBMScrollTime = this.dbNowBMScollTime;
				this.dbNowTime += (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));
				this.dbNowBMScollTime += (((this.fNow_Measure_s / this.fNow_Measure_m)) * (16.0 / (double)n文字数));
			}
		}

		private void t命令を挿入するtcc(string InputText) {

			string[] InputArr = InputText.Split(' ');

			if (InputArr[0].Equals("#bpm"))
			{
				double dbBPM = Convert.ToDouble(InputArr[1]);
				this.dbNowBPM = dbBPM;

				this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = 0, dbBPM値 = dbBPM, bpm_change_time = this.dbNowTime, bpm_change_bmscroll_time = this.dbNowBMScollTime, bpm_change_course = this.n現在のコース });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x08;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip.dbBPM = dbBPM;
				chip.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip);

				var chip1 = new CChip();
				chip1.nチャンネル番号 = 0x9C;
				chip1.n発声位置 = ((this.n現在の小節数) * 384);
				chip1.n発声時刻ms = (int)this.dbNowTime;
				chip1.fNow_Measure_m = this.fNow_Measure_m;
				chip1.fNow_Measure_s = this.fNow_Measure_s;
				chip1.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip1.dbBPM = dbBPM;
				chip1.dbSCROLL = this.dbNowScroll;
				chip1.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip1);

				this.n内部番号BPM1to++;
			}
			else if (InputArr[0].Equals("#scroll"))
			{
				double dbSCROLL = Convert.ToDouble(InputArr[1]);
				this.dbNowScroll = dbSCROLL;
				this.dbNowScrollY = 0.0;

				this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = dbSCROLL, dbSCROLL値Y = 0.0 });

				switch (this.n現在のコース)
				{
					case 0:
						this.dbNowSCROLL_Normal[0] = dbSCROLL;
						break;
					case 1:
						this.dbNowSCROLL_Expert[0] = dbSCROLL;
						break;
					case 2:
						this.dbNowSCROLL_Master[0] = dbSCROLL;
						break;
				}

				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9D;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = this.n内部番号SCROLL1to;
				chip.dbSCROLL = dbSCROLL;
				chip.dbSCROLL_Y = 0.0;
				chip.nコース = this.n現在のコース;

				// チップを配置。

				this.listChip.Add(chip);

				this.n内部番号SCROLL1to++;
			}
			else if (InputArr[0].Equals("#tsign"))
			{
				string[] strArray = InputArr[1].Split('/');
				WarnSplitLength("#MEASURE subsplit", strArray, 2);

				double[] dbLength = new double[2];
				dbLength[0] = Convert.ToDouble(strArray[0]);
				dbLength[1] = Convert.ToDouble(strArray[1]);

				double db小節長倍率 = dbLength[0] / dbLength[1];
				this.dbBarLength = db小節長倍率;
				this.fNow_Measure_m = (float)dbLength[1];
				this.fNow_Measure_s = (float)dbLength[0];

				var chip = new CChip();

				chip.nチャンネル番号 = 0x02;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.dbSCROLL = this.dbNowScroll;
				chip.db実数値 = db小節長倍率;
				chip.n整数値_内部番号 = 1;
				// チップを配置。

				this.listChip.Add(chip);

				//lbMaster.Items.Add( ";拍子変更 " + strArray[0] + "/" + strArray[1] );
			}
			else if (InputArr[0].Equals("#delay"))
			{
				double nDELAY = (Convert.ToDouble(InputArr[1]) * 1000.0);


				this.listDELAY.Add(this.n内部番号DELAY1to, new CDELAY() { n内部番号 = this.n内部番号DELAY1to, n表記上の番号 = 0, nDELAY値 = (int)nDELAY, delay_bmscroll_time = this.dbLastBMScrollTime, delay_bpm = this.dbNowBPM, delay_course = this.n現在のコース, delay_time = this.dbLastTime });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xDC;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.db発声時刻ms = this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.nコース = this.n現在のコース;
				chip.n整数値_内部番号 = this.n内部番号DELAY1to;
				chip.fBMSCROLLTime = this.dbNowBMScollTime;
				// チップを配置。

				this.dbNowTime += nDELAY;
				this.dbNowBMScollTime += nDELAY * this.dbNowBPM / 15000;

				this.listChip.Add(chip);
				this.n内部番号DELAY1to++;
			}

			else if (InputArr[0].Equals("#gogobegin"))
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9E;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.dbBPM = this.dbNowBPM;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				this.bGOGOTIME = true;

				// チップを配置。
				this.listChip.Add(chip);
			}
			else if (InputArr[0].Equals("#gogoend"))
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9F;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.dbBPM = this.dbNowBPM;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				this.bGOGOTIME = false;

				// チップを配置。
				this.listChip.Add(chip);
			}

		}

		private string tコメントを削除する(string input)
		{
			string strOutput = Regex.Replace(input, @" *//.*", ""); //2017.01.28 DD コメント前のスペースも削除するように修正

			return strOutput;
		}

		private string[] tコマンド行を削除したTJAを返す(string[] input)
		{
			return this.tコマンド行を削除したTJAを返す(input, 0);
		}

		private string[] tコマンド行を削除したTJAを返す(string[] input, int nMode)
		{
			var sb = new StringBuilder();
			
			// 18/11/11 AioiLight 譜面にSpace、スペース、Tab等が入っているとおかしくなるので修正。
			// 多分コマンドもスペースが抜かれちゃっているが、コマンド行を除く譜面を返すので大丈夫(たぶん)。
			for (int i = 0; i < input.Length; i++)
			{
				input[i] = input[i].Trim();
			}

			for (int n = 0; n < input.Length; n++)
			{
				if (nMode == 0)
				{
					if (!string.IsNullOrEmpty(input[n]) && this.CharConvertNote(input[n].Substring(0, 1)) != -1)
					{
						sb.Append(input[n] + "\n");
					}
				}
				else if (nMode == 1)
				{
					if (!string.IsNullOrEmpty(input[n]) && (input[n].Substring(0, 1) == "#" || this.CharConvertNote(input[n].Substring(0, 1)) != -1))
					{
						if (input[n].StartsWith("BALLOON") || input[n].StartsWith("BPM"))
						{
							//A～Fで始まる命令が削除されない不具合の対策
						}
						else
						{
							sb.Append(input[n] + "\n");
						}
					}
				}
				else if (nMode == 2)
				{
					if (!string.IsNullOrEmpty(input[n]) && this.CharConvertNote(input[n].Substring(0, 1)) != -1)
					{
						if (input[n].StartsWith("BALLOON") || input[n].StartsWith("BPM"))
						{
							//A～Fで始まる命令が削除されない不具合の対策
						}
						else
						{
							sb.Append(input[n] + "\n");
						}
					}
					else
					{
						if (input[n].StartsWith("#BRANCHSTART") || input[n] == "#N" || input[n] == "#E" || input[n] == "#M")
						{
							sb.Append(input[n] + "\n");
						}

					}
				}
			}

			string[] strOutput = sb.ToString().Split(this.dlmtEnter, StringSplitOptions.None);

			return strOutput;
		}

		private string[] t空のstring配列を詰めたstring配列を返す(string[] input)
		{
			var sb = new StringBuilder();

			for (int n = 0; n < input.Length; n++)
			{
				if (!string.IsNullOrEmpty(input[n]))
				{
					sb.Append(input[n] + "\n");
				}
			}

			string[] strOutput = sb.ToString().Split(this.dlmtEnter, StringSplitOptions.None);

			return strOutput;
		}

		private string StringArrayToString(string[] input)
		{
			return this.StringArrayToString(input, "");
		}
		private string StringArrayToString(string[] input, string strデリミタ文字)
		{
			var sb = new StringBuilder();

			for (int n = 0; n < input.Length; n++)
			{
				sb.Append(input[n] + strデリミタ文字);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InputText"></param>
		/// <returns>1小節内の文字数</returns>
		private int t1小節の文字数をカウントする(string InputText)
		{
			return InputText.Length - 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InputText"></param>
		/// <returns>1小節内の文字数</returns>
		private void t1小節の文字数をカウントしてリストに追加する(string InputText)
		{
			int nCount = 0;

			if (InputText.StartsWith("#BRANCHSTART"))
			{
				this.nLineCountTemp = this.n現在の小節数;
				return;
			}
			else if (InputText.StartsWith("#N"))
			{
				this.nLineCountCourseTemp = 0;
				this.n現在の小節数 = this.nLineCountTemp;
				return;
			}
			else if (InputText.StartsWith("#E"))
			{
				this.nLineCountCourseTemp = 1;
				this.n現在の小節数 = this.nLineCountTemp;
				return;
			}
			else if (InputText.StartsWith("#M"))
			{
				this.nLineCountCourseTemp = 2;
				this.n現在の小節数 = this.nLineCountTemp;
				return;
			}


			var line = new CLine();
			line.nコース = this.nLineCountCourseTemp;
			line.n文字数 = InputText.Length - 1;
			line.n小節番号 = this.n現在の小節数;

			this.listLine.Add(line);

			this.n現在の小節数++;

		}

		/// <summary>
		/// 0:改行文字を削除して、デリミタとしてスペースを入れる。(返り値:string)
		/// 1:改行文字を削除、さらにSplitして返す(返り値:string[n])
		/// </summary>
		/// <param name="strInput"></param>
		/// <param name="nMode"></param>
		/// <returns></returns>
		private object str改行文字を削除する(string strInput, int nMode)
		{
			string str = "";
			str = strInput.Replace(Environment.NewLine, "\n");
			str = str.Replace('\t', ' ');

			if (nMode == 0)
			{
				str = str.Replace("\n", " ");
			}
			else if (nMode == 1)
			{
				str = str + "\n";

				string[] strArray;
				strArray = str.Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);

				return strArray;
			}

			return str;
		}

		/// <summary>
		/// コースごとに譜面を分割する。
		/// </summary>
		/// <param name="strTJA"></param>
		/// <returns>各コースの譜面(string[5])</returns>
		private string[] tコースで譜面を分割する(string strTJA)
		{
			string[] strCourseTJA = new string[(int)Difficulty.Total];

			if (strTJA.IndexOf("COURSE", 0) != -1)
			{
				//tja内に「COURSE」があればここを使う。
				string[] strTemp = strTJA.Split(this.dlmtCOURSE, StringSplitOptions.RemoveEmptyEntries);

				for (int n = 1; n < strTemp.Length; n++)
				{
					int nCourse = 0;
					string nNC = "";
					while (strTemp[n].Substring(0, 1) != "\n") //2017.01.29 DD COURSE単語表記に対応
					{
						nNC += strTemp[n].Substring(0, 1);
						strTemp[n] = strTemp[n].Remove(0, 1);
					}

					if (this.strConvertCourse(nNC) != -1)
					{
						nCourse = this.strConvertCourse(nNC);
						strCourseTJA[nCourse] = strTemp[n];
					}
					else
					{

					}
					//strCourseTJA[ ];

				}
			}
			else
			{
				strCourseTJA[3] = strTJA;
			}

			return strCourseTJA;
		}

		private static readonly Regex regexForPrefixingCommaStartingLinesWithZero = new Regex(@"^,", RegexOptions.Multiline | RegexOptions.Compiled);
		private static readonly Regex regexForStrippingHeadingLines = new Regex(
			@"^(?!(TITLE|LEVEL|BPM|WAVE|OFFSET|BALLOON|EXAM1|EXAM2|EXAM3|BALLOONNOR|BALLOONEXP|BALLOONMAS|SONGVOL|SEVOL|SCOREINIT|SCOREDIFF|COURSE|STYLE|GAME|LIFE|DEMOSTART|SIDE|SUBTITLE|SCOREMODE|GENRE|MOVIEOFFSET|BGIMAGE|BGMOVIE|HIDDENBRANCH|GAUGEINCR|#HBSCROLL|#BMSCROLL|LYRICFILE)).+\n",
			RegexOptions.Multiline | RegexOptions.Compiled);

		/// <summary>
		/// 新型。
		/// ○未実装
		/// _「COURSE」定義が無い譜面は未対応
		/// 　→ver2015082200で対応完了。
		/// 
		/// </summary>
		/// <param name="strInput">譜面のデータ</param>
		private void t入力_V4(string strInput)
		{
			if (!String.IsNullOrEmpty(strInput)) //空なら通さない
			{
				//StreamWriter stream = null;
				//bool bLog = true;
				//try
				//{
				//    stream = new StreamWriter("noteTest.txt", false);
				//}
				//catch (Exception ex)
				//{
				//    Trace.TraceError( ex.StackTrace );
				//}

				//2017.01.31 DD カンマのみの行を0,に置き換え
				strInput = regexForPrefixingCommaStartingLinesWithZero.Replace(strInput, "0,");

				//2017.02.03 DD ヘッダ内にある命令以外の文字列を削除
				var startIndex = strInput.IndexOf("#START");
				if (startIndex < 0)
				{
					Trace.TraceWarning($"#START命令が少なくとも1つは必要です。 ({strファイル名の絶対パス})");
				}
				string strInputHeader = strInput.Remove(startIndex);
				strInput = strInput.Remove(0, startIndex);
				strInputHeader = regexForStrippingHeadingLines.Replace(strInputHeader, "");
				strInput = strInputHeader + "\n" + strInput;

				//どうせ使わないので先にSplitしてコメントを削除。
				var strSplitした譜面 = (string[])this.str改行文字を削除する(strInput, 1);
				for (int i = 0; strSplitした譜面.Length > i; i++)
				{
					strSplitした譜面[i] = this.tコメントを削除する(strSplitした譜面[i]);
				}
				//空のstring配列を詰める
				strSplitした譜面 = this.t空のstring配列を詰めたstring配列を返す(strSplitした譜面);

				#region[ヘッダ]

				//2015.05.21 kairera0467
				//ヘッダの読み込みは譜面全体から該当する命令を探す。
				//少し処理が遅くなる可能性はあるが、ここは正確性を重視する。
				//点数などの指定は後から各コースで行うので問題は無いだろう。

				//SplitしたヘッダのLengthの回数だけ、forで回して各種情報を読み取っていく。
				for (int i = 0; strSplitした譜面.Length > i; i++)
				{
					this.t入力_行解析ヘッダ(strSplitした譜面[i]);
				}
				#endregion

				#region[譜面]

				int n読み込むコース = 3;
				int n譜面数 = 0; //2017.07.22 kairera0467 tjaに含まれる譜面の数



				//まずはコースごとに譜面を分割。
				strSplitした譜面 = this.tコースで譜面を分割する(this.StringArrayToString(strSplitした譜面, "\n"));
				string strTest = "";
				//存在するかのフラグ作成。
				for (int i = 0; i < strSplitした譜面.Length; i++)
				{
					if (!String.IsNullOrEmpty(strSplitした譜面[i]))
					{
						this.b譜面が存在する[i] = true;
						n譜面数++;
					}
					else
						this.b譜面が存在する[i] = false;
				}
				#region[ 読み込ませるコースを決定 ]
				if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//2020.05.12 Mr-Ojii 起動直後の曲読み込みでエラーを吐くので対策
					n読み込むコース = TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayerSide];
				if (this.b譜面が存在する[n読み込むコース] == false)
				{
					n読み込むコース++;
					for (int n = 1; n < (int)Difficulty.Total; n++)
					{
						if (this.b譜面が存在する[n読み込むコース] == false)
						{
							n読み込むコース++;
							if (n読み込むコース > (int)Difficulty.Total - 1)
								n読み込むコース = 0;
						}
						else
							break;
					}
				}
				#endregion

				//Seseragi255様のプルリクより変更

				//多難易度選択が可能になったので、セッション譜面は同じ難易度再生の時以外はお預けにしておく
				int n読み込むセッション譜面パート = 0;
				if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//2020.05.12 Mr-Ojii 起動直後の曲読み込みでエラーを吐くので対策
					if (TJAPlayer3.ConfigIni.nPlayerCount >= 2 && TJAPlayer3.stage選曲.n確定された曲の難易度[0] == TJAPlayer3.stage選曲.n確定された曲の難易度[1])
						n読み込むセッション譜面パート = nPlayerSide + 1;

				//指定したコースの譜面の命令を消去する。
				strSplitした譜面[n読み込むコース] = CDTXStyleExtractor.tセッション譜面がある(
					strSplitした譜面[n読み込むコース],
					n読み込むセッション譜面パート,
					this.strファイル名の絶対パス);

				//------

				//命令をすべて消去した譜面
				var str命令消去譜面 = strSplitした譜面[n読み込むコース].Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);
				//if( bLog && stream != null )
				//{
				//    stream.WriteLine( "-------------------------------------------------" );
				//    stream.WriteLine( ">>this.str命令消去譜面(コマンド削除前)" );
				//    for( int i = 0; i < this.str命令消去譜面.Length; i++ )
				//    {
				//        stream.WriteLine( this.str命令消去譜面[ i ] );
				//    }
				//    stream.WriteLine( "-------------------------------------------------" );
				//}
				str命令消去譜面 = this.tコマンド行を削除したTJAを返す(str命令消去譜面, 2);

				//if( bLog && stream != null )
				//{
				//    stream.WriteLine( "-------------------------------------------------" );
				//    stream.WriteLine( ">>this.str命令消去譜面" );
				//    for( int i = 0; i < this.str命令消去譜面.Length; i++ )
				//    {
				//        stream.WriteLine( this.str命令消去譜面[ i ] );
				//    }
				//    stream.WriteLine( "-------------------------------------------------" );
				//}


				//ここで1行の文字数をカウント。配列にして返す。
				var strSplit読み込むコース = strSplitした譜面[n読み込むコース].Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);
				string str = "";
				try
				{
					if (n譜面数 > 1)
					{
						//2017.07.22 kairera0467 譜面が2つ以上ある場合はCOURSE以下のBALLOON命令を使う
						this.listBalloon_Normal.Clear();
						this.listBalloon_Expert.Clear();
						this.listBalloon_Master.Clear();
						this.listBalloon_Normal_数値管理 = 0;
						this.listBalloon_Expert_数値管理 = 0;
						this.listBalloon_Master_数値管理 = 0;
					}

					for (int i = 0; i < strSplit読み込むコース.Length; i++)
					{
						if (!String.IsNullOrEmpty(strSplit読み込むコース[i]))
						{
							this.t難易度別ヘッダ(strSplit読み込むコース[i]);
						}
					}
					for (int i = 0; i < str命令消去譜面.Length; i++)
					{
						if (str命令消去譜面[i].IndexOf(',', 0) == -1 && !String.IsNullOrEmpty(str命令消去譜面[i]))
						{
							if (str命令消去譜面[i].Substring(0, 1) == "#")
							{
								this.t1小節の文字数をカウントしてリストに追加する(str + str命令消去譜面[i]);
							}

							if (this.CharConvertNote(str命令消去譜面[i].Substring(0, 1)) != -1)
								str += str命令消去譜面[i];
						}
						else
						{
							this.t1小節の文字数をカウントしてリストに追加する(str + str命令消去譜面[i]);
							str = "";
						}
					}
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (9e401212-0b78-4073-88d0-f7e791f36a91)");
				}

				//if( bLog && stream != null )
				//{
				//    stream.WriteLine( "-------------------------------------------------" );
				//    stream.WriteLine( ">>this.str命令消去譜面 (命令消去した後)" );
				//    for( int i = 0; i < this.str命令消去譜面.Length; i++ )
				//    {
				//        stream.WriteLine( this.str命令消去譜面[ i ] );
				//    }
				//    stream.WriteLine( "-------------------------------------------------" );
				//}

				//読み込み部分本体に渡す譜面を作成。
				//0:ヘッダー情報 1:#START以降 となる。個数の定義は後からされるため、ここでは省略。
				var strSplitした後の譜面 = strSplit読み込むコース; //strSplitした譜面[ n読み込むコース ].Split( this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries );
				strSplitした後の譜面 = this.tコマンド行を削除したTJAを返す(strSplitした後の譜面, 1);
				//string str命令消去譜面temp = this.StringArrayToString( str命令消去譜面 );
				//string[] strDelimiter = { "," };
				//this.str命令消去譜面 = str命令消去譜面temp.Split( strDelimiter, StringSplitOptions.RemoveEmptyEntries );

				//if( bLog && stream != null )
				//{
				//    stream.WriteLine( "-------------------------------------------------" );
				//    stream.WriteLine( ">>this.str命令消去譜面 (Splitした後)" );
				//    for( int i = 0; i < this.str命令消去譜面.Length; i++ )
				//    {
				//        stream.WriteLine( this.str命令消去譜面[ i ] );
				//    }
				//    stream.WriteLine( "-------------------------------------------------" );
				//}

				this.n現在の小節数 = 1;
				try
				{
					#region[ 最初の処理 ]
					//1小節の時間を挿入して開始時間を調節。
					this.dbNowTime += ((15000.0 / 120.0 * (4.0 / 4.0)) * 16.0);
					//this.dbNowBMScollTime += (( this.dbBarLength ) * 16.0 );
					#endregion
					//string strWrite = "";
					for (int i = 0; strSplitした後の譜面.Length > i; i++)
					{
						str = strSplitした後の譜面[i];
						//strWrite += str;
						//if( !str.StartsWith( "#" ) && !string.IsNullOrEmpty( this.strTemp ) )
						//{
						//    str = this.strTemp + str;
						//}

						this.t入力_行解析譜面_V4(str);
					}
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (2da1e880-6b63-4e82-b018-bf18c3568335)");
				}
				//if( stream != null )
				//{
				//    stream.Flush();
				//    stream.Close();
				//}
				#endregion
			}
		}

		private CChip t発声位置から過去方向で一番近くにある指定チャンネルのチップを返す(int n発声時刻, int nチャンネル番号)
		{
			//過去方向への検索
			for (int i = this.listChip.Count - 1; i >= 0; i--)
			{
				if (this.listChip[i].nチャンネル番号 == nチャンネル番号)
				{
					return this.listChip[i];
				}
			}

			return null;
		}

		//現在、以下のような行には対応できていません。
		//_パラメータを持つ命令がある
		//_行の途中に命令がある
		private int t文字数解析(string InputText)
		{
			int n文字数 = 0;

			for (int i = 0; i < InputText.Length; i++)
			{
				if (this.CharConvertNote(InputText.Substring(i, 1)) != -1)
				{
					n文字数++;
				}
			}


			return n文字数;
		}

		private static readonly Regex CommandAndArgumentRegex =
			new Regex(@"^(#[A-Z]+)(?:\s?)(.+?)?$", RegexOptions.Compiled);

		private static readonly Regex BranchStartArgumentRegex =
			new Regex(@"^([^,\s]+)\s*,\s*([^,\s]+)\s*,\s*([^,\s]+)$", RegexOptions.Compiled);

		private string[] SplitComma(string input)
		{
			var result = new List<string>();
			var workingIndex = 0;
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i].Equals(',')) // カンマにぶち当たった
				{
					if (i - 1 >= 0)//2020.08.04 Mr-Ojii &&演算子でも、例外が起きるので...
					{
						if (input[i - 1].Equals('\\')) // 1文字前がバックスラッシュ
						{
							input = input.Remove(i - 1, 1);
						}
						else
						{
							// workingIndexから今の位置までをリストにブチ込む
							result.Add(input.Substring(workingIndex, i - workingIndex));
							// workingIndexに今の位置+1を代入
							workingIndex = i + 1;
						}
					}
					else
					{
						// workingIndexから今の位置までをリストにブチ込む
						result.Add(input.Substring(workingIndex, i - workingIndex));
						// workingIndexに今の位置+1を代入
						workingIndex = i + 1;
					}
				}
				if (i + 1 == input.Length) // 最後に
				{
					result.Add(input.Substring(workingIndex, input.Length - workingIndex));
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// 譜面読み込みメソッドV4で使用。
		/// </summary>
		/// <param name="InputText"></param>
		private void t命令を挿入する(string InputText)
		{
			var match = CommandAndArgumentRegex.Match(InputText);
			if (!match.Success)
			{
				return;
			}

			var command = match.Groups[1].Value;
			var argumentMatchGroup = match.Groups[2];
			var argument = argumentMatchGroup.Success ? argumentMatchGroup.Value : null;

			while (true) {//2020.05.29 Mr-Ojii 間違えて、命令の最後の所に,を入れてしまった時の対応
				if (argument != null && argument[argument.Length - 1] == ',')
					argument = argument.Substring(0, argument.Length - 1);
				else
					break;
			}


			char[] chDelimiter = new char[] { ' ' };
			string[] strArray = null;

			if (command == "#START")
			{
				//#STARTと同時に鳴らすのはどうかと思うけどしゃーなしだな。
				AddMusicPreTimeMs(); // 音源を鳴らす前に遅延。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x01;
				chip.n発声位置 = 384;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.fBMSCROLLTime = this.dbNowBMScollTime;
				chip.n整数値 = 0x01;
				chip.n整数値_内部番号 = 1;

				// チップを配置。
				this.listChip.Add(chip);

				var chip1 = new CChip();
				chip1.nチャンネル番号 = 0x54;
				//chip1.n発声位置 = 384;
				//chip1.n発声時刻ms = (int)this.dbNowTime;
				if (this.nMOVIEOFFSET == 0)
					chip1.n発声時刻ms = (int)this.dbNowTime;
				else
					chip1.n発声時刻ms = (int)this.nMOVIEOFFSET;
				chip1.dbBPM = this.dbNowBPM;
				chip1.dbSCROLL = this.dbNowScroll;
				chip1.fNow_Measure_m = this.fNow_Measure_m;
				chip1.fNow_Measure_s = this.fNow_Measure_s;
				chip1.n整数値 = 0x01;
				chip1.n整数値_内部番号 = 1;
				chip1.nPlayerSide = this.nPlayerSide;
				chip1.eAVI種別 = EAVI種別.AVI;

				// チップを配置。

				this.listChip.Add(chip1);
			}
			else if (command == "#END")
			{
				//ためしに割り込む。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xFF;
				chip.n発声位置 = ((this.n現在の小節数 + 2) * 384);
				//chip.n発声時刻ms = (int)( this.dbNowTime + ((15000.0 / this.dbNowBPM * ( 4.0 / 4.0 )) * 16.0) * 2  );
				chip.n発声時刻ms = (int)(this.dbNowTime + 1000); //2016.07.16 kairera0467 終了時から1秒後に設置するよう変更。
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値 = 0xFF;
				chip.n整数値_内部番号 = 1;
				// チップを配置。

				this.listChip.Add(chip);
			}

			else if (command == "#BPMCHANGE")
			{
				double dbBPM = Convert.ToDouble(argument);
				this.dbNowBPM = dbBPM;

				this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = 0, dbBPM値 = dbBPM, bpm_change_time = this.dbNowTime, bpm_change_bmscroll_time = this.dbNowBMScollTime, bpm_change_course = this.n現在のコース });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x08;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip.dbBPM = dbBPM;
				chip.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip);

				var chip1 = new CChip();
				chip1.nチャンネル番号 = 0x9C;
				chip1.n発声位置 = ((this.n現在の小節数) * 384);
				chip1.n発声時刻ms = (int)this.dbNowTime;
				chip1.fNow_Measure_m = this.fNow_Measure_m;
				chip1.fNow_Measure_s = this.fNow_Measure_s;
				chip1.fBMSCROLLTime = (float)this.dbNowBMScollTime;
				chip1.dbBPM = dbBPM;
				chip1.dbSCROLL = this.dbNowScroll;
				chip1.n整数値_内部番号 = this.n内部番号BPM1to - 1;

				// チップを配置。

				this.listChip.Add(chip1);

				this.n内部番号BPM1to++;
			}
			else if (command == "#SCROLL")
			{
				//2016.08.13 kairera0467 複素数スクロールもどきのテスト
				if (argument.IndexOf('i') != -1)
				{
					//iが入っていた場合、複素数スクロールとみなす。

					double[] dbComplexNum = new double[2];
					this.tParsedComplexNumber(argument, ref dbComplexNum);

					this.dbNowScroll = dbComplexNum[0];
					this.dbNowScrollY = dbComplexNum[1];

					this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = dbComplexNum[0], dbSCROLL値Y = dbComplexNum[1] });

					switch (this.n現在のコース)
					{
						case 0:
							this.dbNowSCROLL_Normal[0] = dbComplexNum[0];
							this.dbNowSCROLL_Normal[1] = dbComplexNum[1];
							break;
						case 1:
							this.dbNowSCROLL_Expert[0] = dbComplexNum[0];
							this.dbNowSCROLL_Expert[1] = dbComplexNum[1];
							break;
						case 2:
							this.dbNowSCROLL_Master[0] = dbComplexNum[0];
							this.dbNowSCROLL_Master[1] = dbComplexNum[1];
							break;
						default:
							this.dbNowSCROLL_Normal[0] = dbComplexNum[0];
							this.dbNowSCROLL_Normal[1] = dbComplexNum[1];
							break;
					}

					//チップ追加して割り込んでみる。
					var chip = new CChip();

					chip.nチャンネル番号 = 0x9D;
					chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
					chip.n発声時刻ms = (int)this.dbNowTime;
					chip.fNow_Measure_m = this.fNow_Measure_m;
					chip.fNow_Measure_s = this.fNow_Measure_s;
					chip.n整数値_内部番号 = this.n内部番号SCROLL1to;
					chip.dbSCROLL = dbComplexNum[0];
					chip.dbSCROLL_Y = dbComplexNum[1];
					chip.nコース = this.n現在のコース;

					// チップを配置。

					this.listChip.Add(chip);
				}
				else
				{
					double dbSCROLL = Convert.ToDouble(argument);
					this.dbNowScroll = dbSCROLL;
					this.dbNowScrollY = 0.0;

					this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = dbSCROLL, dbSCROLL値Y = 0.0 });

					switch (this.n現在のコース)
					{
						case 0:
							this.dbNowSCROLL_Normal[0] = dbSCROLL;
							break;
						case 1:
							this.dbNowSCROLL_Expert[0] = dbSCROLL;
							break;
						case 2:
							this.dbNowSCROLL_Master[0] = dbSCROLL;
							break;
					}

					//チップ追加して割り込んでみる。
					var chip = new CChip();

					chip.nチャンネル番号 = 0x9D;
					chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
					chip.n発声時刻ms = (int)this.dbNowTime;
					chip.fNow_Measure_m = this.fNow_Measure_m;
					chip.fNow_Measure_s = this.fNow_Measure_s;
					chip.n整数値_内部番号 = this.n内部番号SCROLL1to;
					chip.dbSCROLL = dbSCROLL;
					chip.dbSCROLL_Y = 0.0;
					chip.nコース = this.n現在のコース;

					// チップを配置。

					this.listChip.Add(chip);
				}




				this.n内部番号SCROLL1to++;
			}
			else if (command == "#MEASURE")
			{
				strArray = argument.Split(new char[] { '/' });
				WarnSplitLength("#MEASURE subsplit", strArray, 2);

				double[] dbLength = new double[2];
				dbLength[0] = Convert.ToDouble(strArray[0]);
				dbLength[1] = Convert.ToDouble(strArray[1]);

				double db小節長倍率 = dbLength[0] / dbLength[1];
				this.dbBarLength = db小節長倍率;
				this.fNow_Measure_m = (float)dbLength[1];
				this.fNow_Measure_s = (float)dbLength[0];

				var chip = new CChip();

				chip.nチャンネル番号 = 0x02;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.dbSCROLL = this.dbNowScroll;
				chip.db実数値 = db小節長倍率;
				chip.n整数値_内部番号 = 1;
				// チップを配置。

				this.listChip.Add(chip);

				//lbMaster.Items.Add( ";拍子変更 " + strArray[0] + "/" + strArray[1] );
			}
			else if (command == "#DELAY")
			{
				double nDELAY = (Convert.ToDouble(argument) * 1000.0);


				this.listDELAY.Add(this.n内部番号DELAY1to, new CDELAY() { n内部番号 = this.n内部番号DELAY1to, n表記上の番号 = 0, nDELAY値 = (int)nDELAY, delay_bmscroll_time = this.dbLastBMScrollTime, delay_bpm = this.dbNowBPM, delay_course = this.n現在のコース, delay_time = this.dbLastTime });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xDC;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.db発声時刻ms = this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.nコース = this.n現在のコース;
				chip.n整数値_内部番号 = this.n内部番号DELAY1to;
				chip.fBMSCROLLTime = this.dbNowBMScollTime;
				// チップを配置。

				this.dbNowTime += nDELAY;
				this.dbNowBMScollTime += nDELAY * this.dbNowBPM / 15000;

				this.listChip.Add(chip);
				this.n内部番号DELAY1to++;
			}

			else if (command == "#GOGOSTART")
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9E;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.dbBPM = this.dbNowBPM;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				this.bGOGOTIME = true;

				// チップを配置。
				this.listChip.Add(chip);
			}
			else if (command == "#GOGOEND")
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9F;
				chip.n発声位置 = ((this.n現在の小節数) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.dbBPM = this.dbNowBPM;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				this.bGOGOTIME = false;

				// チップを配置。
				this.listChip.Add(chip);
			}
			else if (command == "#SECTION")
			{
				//分岐:条件リセット
				var chip = new CChip();

				chip.nチャンネル番号 = 0xDD;
				chip.n発声位置 = ((this.n現在の小節数 - 1) * 384);
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				chip.db発声時刻ms = this.dbNowTime;
				// チップを配置。
				this.listChip.Add(chip);
			}
			else if (command == "#BRANCHSTART")
			{
				#region [ 譜面分岐のパース方法を作り直し ]   //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに結構な修正
				this.bチップがある.Branch = true;
				this.b最初の分岐である = false;
				this.b分岐を一回でも開始した = true;

				//分岐:分岐スタート
				int n条件;

				//条件数値。
				double[] nNum = new double[2];

				//名前と条件Aの間に,が無いと正常に動作しなくなる.2020.04.23.akasoko26
				#region [ 名前と条件Aの間に,が無いと正常に動作しなくなる ]
				//空白を削除する。
				argument = Regex.Replace(argument, @"\s", "");
				//2文字目が,か数値かをチェック
				var IsNumber = bIsNumber(argument[1]);
				//IsNumber == true であったら,が無いということなので,を2文字目にぶち込む・・・
				if (IsNumber)
					argument = argument.Insert(1, ",");
				#endregion

				var branchStartArgumentMatch = BranchStartArgumentRegex.Match(argument);
				nNum[0] = Convert.ToDouble(branchStartArgumentMatch.Groups[2].Value);
				nNum[1] = Convert.ToDouble(branchStartArgumentMatch.Groups[3].Value);
				switch (branchStartArgumentMatch.Groups[1].Value)
				{
					case "p":
						n条件 = 0;
						break;
					case "s":
						n条件 = 1;
						break;
					case "r":
						n条件 = 2;
						break;
					case "d":
						n条件 = 3;
						break;
					default:
						n条件 = 0;
						break;
				}

				#region [ 分岐開始時のチップ情報を記録 ]
				//現在のチップ情報を記録する必要がある。
				this.t現在のチップ情報を記録する(true);
				#endregion

				#region [ 一小節前の分岐開始Chip ]
				//16分前に戻す計算なんか当てにしちゃだめよ。。(by Akasoko)
				var c小節前の小節線情報 = c一小節前の小節線情報を返す(listChip, n条件);
				CChip c小節前の連打開始位置 = null;

				var chip = new CChip();

				if (n条件 == 2)
				{
					/*c小節前の連打開始位置 = c一小節前の小節線情報を返す(listChip, n条件, true);

					//連打分岐の位置を再現
					//この計算式はあてにならないと思うが、まあどうしようもないんでこれで
					//なるべく連打のケツの部分に
					var f連打の長さの半分 = (c小節前の小節線情報.n発声時刻ms - c小節前の連打開始位置.n発声時刻ms) / 2.0f;
					*/

					chip.n発声時刻ms = c小節前の小節線情報.n発声時刻ms;
				}
				else chip.n発声時刻ms = c小節前の小節線情報.n発声時刻ms;

				chip.nチャンネル番号 = 0xDE;
				chip.fNow_Measure_m = c小節前の小節線情報.fNow_Measure_m;
				chip.fNow_Measure_s = c小節前の小節線情報.fNow_Measure_s;

				chip.dbSCROLL = c小節前の小節線情報.dbSCROLL;
				chip.dbBPM = c小節前の小節線情報.dbBPM;
				chip.n整数値_内部番号 = nBRANCH現在番号;
				this.listChip.Add(chip);
				this.listBRANCH.Add(nBRANCH現在番号,
					new CBRANCH { db条件数値A = nNum[0],
						db条件数値B = nNum[1],
						n分岐の種類 = n条件,
						n番号 = nBRANCH現在番号,
						//ノーツ * 0.5分後ろにして、ノーツが残らないようにする
						db分岐時刻ms = this.dbNowTime - ((15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m)) * 0.5) 
					});
				this.nBRANCH現在番号++;
				#endregion

				for (int i = 0; i < 3; i++)
					IsBranchBarDraw[i] = true;//3コース分の黄色小説線表示㋫ラブ

				IsEndedBranching = false;
				#endregion
			}
			else if (command == "#N" || command == "#E" || command == "#M") //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更//これCourseを全部集めてあとから分岐させればいい件
			{
				//開始時の情報にセット
				t現在のチップ情報を記録する(false);

				if (command == "#N")
					this.n現在のコース = 0;//分岐:普通譜面
				else if (command == "#E")
					this.n現在のコース = 1;//分岐:玄人譜面
				else if (command == "#M")
					this.n現在のコース = 2;//分岐:達人譜面
			}
			else if (command == "#LEVELHOLD")
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0xE1;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;

				this.listChip.Add(chip);
			}
			else if (command == "#BRANCHEND") //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
			{
				var GoBranch = new CChip();

				//End用チャンネルをEmptyから引っ張ってきた。
				GoBranch.nチャンネル番号 = 0x52;
				GoBranch.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				GoBranch.n発声時刻ms = (int)this.dbNowTime;
				GoBranch.fNow_Measure_m = this.fNow_Measure_m;
				GoBranch.fNow_Measure_s = this.fNow_Measure_s;
				GoBranch.dbSCROLL = this.dbNowScroll;
				GoBranch.dbBPM = this.dbNowBPM;
				GoBranch.n整数値_内部番号 = 1;

				this.listChip.Add(GoBranch);

				//End時にも黄色い小節線あったべ？
				for (int i = 0; i < 3; i++)
					IsBranchBarDraw[i] = true;//3コース分の黄色小説線表示㋫ラブ
				IsEndedBranching = true;
			}
			else if (command == "#BARLINEOFF")
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0xE0;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime + 1;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 1;
				chip.nコース = this.n現在のコース;
				this.bBARLINECUE[0] = 1;

				this.listChip.Add(chip);
			}
			else if (command == "#BARLINEON")
			{
				var chip = new CChip();

				chip.nチャンネル番号 = 0xE0;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime + 1;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 2;
				chip.nコース = this.n現在のコース;
				this.bBARLINECUE[0] = 0;

				this.listChip.Add(chip);
			}
			else if (command == "#LYRIC")
			{
				if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//起動時に重たくなってしまう問題の修正用
					this.listLyric.Add(this.pf歌詞フォント.DrawPrivateFont(argument, TJAPlayer3.Skin.Game_Lyric_ForeColor, TJAPlayer3.Skin.Game_Lyric_BackColor));

				var chip = new CChip();


				chip.nチャンネル番号 = 0xF1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 0;
				chip.nコース = this.n現在のコース;

				// チップを配置。

				this.listChip.Add(chip);
				this.bLyrics = true;
			}
			else if (command == "#DIRECTION")
			{
				double dbSCROLL = Convert.ToDouble(argument);
				this.nスクロール方向 = (int)dbSCROLL;

				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xF2;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 0;
				chip.nスクロール方向 = (int)dbSCROLL;
				chip.nコース = this.n現在のコース;

				// チップを配置。

				this.listChip.Add(chip);
			}
			else if (command == "#SUDDEN")
			{
				strArray = argument.Split(chDelimiter);
				WarnSplitLength("#SUDDEN", strArray, 2);
				double db出現時刻 = Convert.ToDouble(strArray[0]);
				double db移動待機時刻 = Convert.ToDouble(strArray[1]);
				this.db出現時刻 = db出現時刻;
				this.db移動待機時刻 = db移動待機時刻;

				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xF3;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 0;
				chip.nノーツ出現時刻ms = (int)this.db出現時刻;
				chip.nノーツ移動開始時刻ms = (int)this.db移動待機時刻;
				chip.nコース = this.n現在のコース;

				// チップを配置。

				this.listChip.Add(chip);
			}
			else if (command == "#JPOSSCROLL")
			{
				strArray = argument.Split(chDelimiter);
				WarnSplitLength("#JPOSSCROLL", strArray, 3);
				double db移動時刻 = Convert.ToDouble(strArray[0]);
				int n移動px = Convert.ToInt32(strArray[1]);
				int n移動方向 = Convert.ToInt32(strArray[2]);
				if (n移動方向 == 0)
					n移動px = -n移動px;

				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0xE2;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				chip.n整数値_内部番号 = 0;
				chip.nコース = this.n現在のコース;

				// チップを配置。

				this.listJPOSSCROLL.Add(this.n内部番号JSCROLL1to, new CJPOSSCROLL() { n内部番号 = this.n内部番号JSCROLL1to, n表記上の番号 = 0, db移動時間 = db移動時刻, n移動距離px = n移動px });
				this.listChip.Add(chip);
				this.n内部番号JSCROLL1to++;
			}
			else if (command == "#SENOTECHANGE")
			{
				FixSENote = int.Parse(argument);
				IsEnabledFixSENote = true;
			}
			else if (command == "#NEXTSONG")
			{
				var delayTime = 6200.0; // 6.2秒ディレイ
				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9B;
				chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
				chip.n発声時刻ms = (int)this.dbNowTime;
				chip.fNow_Measure_m = this.fNow_Measure_m;
				chip.fNow_Measure_s = this.fNow_Measure_s;
				this.dbNowTime += delayTime;
				this.dbNowBMScollTime += delayTime * this.dbNowBPM / 15000;
				chip.n整数値_内部番号 = 0;
				chip.nコース = this.n現在のコース;

				// チップを配置。
				this.listChip.Add(chip);

				AddMusicPreTimeMs(); // 段位の幕が開いてからの遅延。

				strArray = SplitComma(argument); // \,をエスケープ処理するメソッドだぞっ
				
				WarnSplitLength("#NEXTSONG", strArray, 6);
				var dansongs = new DanSongs();
				dansongs.Title = strArray[0];
				dansongs.SubTitle = strArray[1];
				dansongs.Genre = strArray[2];
				dansongs.FileName = strArray[3];
				dansongs.ScoreInit = int.Parse(strArray[4]);
				dansongs.ScoreDiff = int.Parse(strArray[5]);
				dansongs.Wave = new CWAV
				{
					n内部番号 = this.n内部番号WAV1to,
					n表記上の番号 = this.n内部番号WAV1to,
					nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
					n位置 = 0,
					SongVol = this.SongVol,
					SongLoudnessMetadata = this.SongLoudnessMetadata,
					strファイル名 = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, dansongs.FileName),
					strコメント文 = "TJA BGM"
				};
				dansongs.Wave.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(dansongs.Wave.strファイル名);
				List_DanSongs.Add(dansongs);
				this.listWAV.Add(this.n内部番号WAV1to, dansongs.Wave);
				this.n内部番号WAV1to++;

				var nextSongnextSongChip = new CChip();

				nextSongnextSongChip.nチャンネル番号 = 0x01;
				nextSongnextSongChip.n発声位置 = 384;
				nextSongnextSongChip.n発声時刻ms = (int)this.dbNowTime;
				nextSongnextSongChip.fNow_Measure_m = this.fNow_Measure_m;
				nextSongnextSongChip.fNow_Measure_s = this.fNow_Measure_s;
				nextSongnextSongChip.n整数値 = 0x01;
				nextSongnextSongChip.n整数値_内部番号 = 1 + List_DanSongs.Count;

				this.listWAV[1].strファイル名 = "";

				// チップを配置。
				this.listChip.Add(nextSongnextSongChip);

			}
		}

		void t現在のチップ情報を記録する(bool bInPut) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
		{
			//2020.04.21 こうなってしまったのは仕方がないな。。 
			if (bInPut)
			{
				#region [ 記録する ]
				cBranchStart.dbTime = this.dbNowTime;
				cBranchStart.dbSCROLL = this.dbNowScroll;
				cBranchStart.dbSCROLLY = this.dbNowScrollY;
				cBranchStart.dbBMScollTime = this.dbNowBMScollTime;
				cBranchStart.dbBPM = this.dbNowBPM;
				cBranchStart.fMeasure_s = this.fNow_Measure_s;
				cBranchStart.fMeasure_m = this.fNow_Measure_m;
				cBranchStart.nMeasureCount = this.n現在の小節数;
				cBranchStart.db移動待機時刻 = this.db移動待機時刻;
				cBranchStart.db再生速度 = this.db再生速度;
				cBranchStart.db出現時刻 = this.db出現時刻;
				#endregion
			}
			else
			{
				#region [ 記録した情報をNow~に適応 ]
				this.dbNowTime = cBranchStart.dbTime;
				this.dbNowScroll = cBranchStart.dbSCROLL;
				this.dbNowScrollY = cBranchStart.dbSCROLLY;
				this.dbNowBMScollTime = cBranchStart.dbBMScollTime;
				this.dbNowBPM = cBranchStart.dbBPM;
				this.fNow_Measure_s = cBranchStart.fMeasure_s;
				this.fNow_Measure_m = cBranchStart.fMeasure_m;
				this.n現在の小節数 = cBranchStart.nMeasureCount;
				this.db移動待機時刻 = cBranchStart.db移動待機時刻;
				this.db再生速度 = cBranchStart.db再生速度;
				this.db出現時刻 = cBranchStart.db出現時刻;
				#endregion
			}
		}

		/// <summary>
		/// 一小節前の小節線情報を返すMethod 2020.04.21.akasoko26
		/// </summary>
		/// <param name="listChips"></param>
		/// <returns></returns>
		private CChip c一小節前の小節線情報を返す(List<CChip> listChips, int n分岐種類, bool b分岐前の連打開始 = false) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
		{
			//2020.04.20 c一小節前の小節線情報を返すMethodを追加
			//連打分岐時は現在の小節以降の連打の終わり部分の時刻を取得する

			int? nReturnChip = null;

			//--して取得しないとだめよ～ダメダメ💛
			for (int i = listChips.Count - 1; i >= 0; i--)
			{
				if (b分岐前の連打開始)
				{
					if (listChips[i].nチャンネル番号 == 0x15 || listChips[i].nチャンネル番号 == 0x16)
					{
						if (nReturnChip == null)
							nReturnChip = i;

						//ReturnChipがnullであったら適応
					}
				}
				else
				{
					var Flag = n分岐種類 == 2 ? 0x18 : 0x50;

					if (listChips[i].nチャンネル番号 == Flag)
					{
						if (nReturnChip == null)
							nReturnChip = i;
						//ReturnChipがnullであったら適応
					}
				}
			}

			//もし、nReturnChipがnullだったらlistChipのCount - 1にセットする。
			return listChips[nReturnChip == null ? listChips.Count - 1 : (int)nReturnChip];
		}

		private void WarnSplitLength(string name, string[] strArray, int minimumLength)
		{
			if (strArray.Length < minimumLength)
			{
				Trace.TraceWarning(
					$"命令 {name} のパラメータが足りません。少なくとも {minimumLength} つのパラメータが必要です。 (現在のパラメータ数: {strArray.Length}). ({strファイル名の絶対パス})");
			}
		}

		private void t入力_行解析譜面_V4(string InputText)
		{
			if (!String.IsNullOrEmpty(InputText))
			{
				int n文字数 = 16;

				//現在のコース、小節に当てはまるものをリストから探して文字数を返す。
				for (int i = 0; i < this.listLine.Count; i++)
				{
					if (this.listLine[i].n小節番号 == this.n現在の小節数 && this.listLine[i].nコース == this.n現在のコース)
					{
						n文字数 = this.listLine[i].n文字数;
					}

				}

				if (InputText.StartsWith("#"))
				{
					try
					{
						this.t命令を挿入する(InputText);
					}
					catch(Exception e) 
					{
						Trace.WriteLine(e);
						Trace.WriteLine("命令挿入中にエラーが発生しましたが、処理を継続します。");
					}
					return;

				}
				else
				{
					if (this.b小節線を挿入している == false)
					{
						// 小節線にもやってあげないと
						// IsEndedBranchingがfalseで1回
						// trueで3回だよ3回
						for (int i = 0; i < (IsEndedBranching == true ? 3 : 1); i++)
						{
							CChip chip = new CChip();
							chip.n発声位置 = ((this.n現在の小節数) * 384);
							chip.nチャンネル番号 = 0x50;
							chip.n発声時刻ms = (int)this.dbNowTime;
							chip.n整数値 = this.n現在の小節数;
							chip.n整数値_内部番号 = this.n現在の小節数;
							chip.dbBPM = this.dbNowBPM;
							chip.fNow_Measure_m = this.fNow_Measure_m;
							chip.fNow_Measure_s = this.fNow_Measure_s;
							chip.IsEndedBranching = IsEndedBranching;
							chip.dbSCROLL = this.dbNowScroll;
							chip.dbSCROLL_Y = this.dbNowScrollY;
							chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;

							if (IsEndedBranching)
								chip.nコース = i;
							else
								chip.nコース = n現在のコース;

							if (this.bBARLINECUE[0] == 1)
							{
								chip.b可視 = false;
							}
							#region [ 作り直し ]  
							if (IsEndedBranching)
							{
								if (this.IsBranchBarDraw[i])
									chip.bBranch = true;
							}
							else
							{
								if (this.IsBranchBarDraw[(int)n現在のコース])
									chip.bBranch = true;
							}
							#endregion

							this.listChip.Add(chip);

							#region [ 作り直し ]  
							if (IsEndedBranching)
								this.IsBranchBarDraw[i] = false;
							else this.IsBranchBarDraw[(int)n現在のコース] = false;
							#endregion
						}

						this.dbLastTime = this.dbNowTime;
						this.b小節線を挿入している = true;

						#region[ 拍線チップテスト ]
						//1拍の時間を計算
						double db1拍 = (60.0 / this.dbNowBPM) / 4.0;
						//forループ(拍数)
						for (int measure = 1; measure < this.fNow_Measure_s; measure++)
						{
							CChip hakusen = new CChip();
							hakusen.n発声位置 = ((this.n現在の小節数) * 384);
							hakusen.n発声時刻ms = (int)(this.dbNowTime + (((db1拍 * 4.0)) * measure) * 1000.0);
							hakusen.nチャンネル番号 = 0x51;
							//hakusen.n発声時刻ms = (int)this.dbNowTime;
							hakusen.fBMSCROLLTime = this.dbNowBMScollTime;
							hakusen.n整数値_内部番号 = this.n現在の小節数;
							hakusen.n整数値 = 0;
							hakusen.dbBPM = this.dbNowBPM;
							hakusen.dbSCROLL = this.dbNowScroll;
							hakusen.fNow_Measure_m = this.fNow_Measure_m;
							hakusen.fNow_Measure_s = this.fNow_Measure_s;
							hakusen.dbSCROLL_Y = this.dbNowScrollY;
							hakusen.nコース = this.n現在のコース;

							this.listChip.Add(hakusen);
							//--全ての拍線の時間を出力する--
							//Trace.WriteLine( string.Format( "|| {0,3:##0} Time:{1} Beat:{2}", this.n現在の小節数, hakusen.n発声時刻ms, measure ) );
							//--------------------------------
						}

						#endregion
					}

					for (int n = 0; n < InputText.Length; n++)
					{
						if (InputText.Substring(n, 1) == ",")
						{
							this.n現在の小節数++;
							this.b小節線を挿入している = false;
							return;
						}

						if (InputText.Substring(0, 1) == "F")
						{
							bool bTest = true;
						}


						int nObjectNum = this.CharConvertNote(InputText.Substring(n, 1));

						if (nObjectNum != 0)
						{
							if ((nObjectNum >= 5 && nObjectNum <= 7) || nObjectNum == 9)
							{
								if (nNowRoll != 0)
								{
									this.dbNowTime += (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));
									this.dbNowBMScollTime += (double)((this.dbBarLength) * (16.0 / n文字数));
									continue;
								}
								else
								{
									this.nNowRollCount = listChip.Count;
									nNowRoll = nObjectNum;
								}
							}

							for (int i = 0; i < (IsEndedBranching == true ? 3 : 1); i++) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに修正
							{
								// IsEndedBranchingがfalseで1回
								// trueで3回だよ3回
								var chip = new CChip();

								chip.IsMissed = false;
								chip.bHit = false;
								chip.b可視 = true;
								chip.bShow = true;
								chip.nチャンネル番号 = 0x10 + nObjectNum;
								//chip.n発声位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
								chip.n発声位置 = (int)((this.n現在の小節数 * 384.0) + ((384.0 * n) / n文字数));
								chip.db発声位置 = this.dbNowTime;
								chip.n発声時刻ms = (int)this.dbNowTime;
								//chip.fBMSCROLLTime = (float)(( this.dbBarLength ) * (16.0f / this.n各小節の文字数[this.n現在の小節数]));
								chip.fBMSCROLLTime = (float)this.dbNowBMScollTime;
								chip.n整数値 = nObjectNum;
								chip.n整数値_内部番号 = 1;
								chip.IsEndedBranching = IsEndedBranching;
								chip.fNow_Measure_m = this.fNow_Measure_m;
								chip.fNow_Measure_s = this.fNow_Measure_s;
								chip.dbBPM = this.dbNowBPM;
								chip.dbSCROLL = this.dbNowScroll;
								chip.dbSCROLL_Y = this.dbNowScrollY;
								chip.nスクロール方向 = this.nスクロール方向;
								if (IsEndedBranching)
									chip.nコース = i;
								else
									chip.nコース = n現在のコース;
								chip.e楽器パート = E楽器パート.TAIKO;
								chip.nノーツ出現時刻ms = (int)(this.db出現時刻 * 1000.0);
								chip.nノーツ移動開始時刻ms = (int)(this.db移動待機時刻 * 1000.0);
								chip.nPlayerSide = this.nPlayerSide;
								chip.bGOGOTIME = this.bGOGOTIME;

								if (nObjectNum == 7 || nObjectNum == 9)
								{ 
									//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに修正
									switch (chip.nコース)
									{
										case 0:
											if (this.listBalloon_Normal.Count == 0)
											{
												chip.nBalloon = 5;
												break;
											}

											if (this.listBalloon_Normal.Count > this.listBalloon_Normal_数値管理)
											{
												chip.nBalloon = this.listBalloon_Normal[this.listBalloon_Normal_数値管理];
												this.listBalloon_Normal_数値管理++;
												break;
											}
											//else if( this.listBalloon.Count != 0 )
											//{
											//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
											//    this.listBalloon_Normal_数値管理++;
											//    break;
											//}
											break;
										case 1:
											if (this.listBalloon_Expert.Count == 0)
											{
												chip.nBalloon = 5;
												break;
											}

											if (this.listBalloon_Expert.Count > this.listBalloon_Expert_数値管理)
											{
												chip.nBalloon = this.listBalloon_Expert[this.listBalloon_Expert_数値管理];
												this.listBalloon_Expert_数値管理++;
												break;
											}
											//else if( this.listBalloon.Count != 0 )
											//{
											//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
											//    this.listBalloon_Normal_数値管理++;
											//    break;
											//}
											break;
										case 2:
											if (this.listBalloon_Master.Count == 0)
											{
												chip.nBalloon = 5;
												break;
											}

											if (this.listBalloon_Master.Count > this.listBalloon_Master_数値管理)
											{
												chip.nBalloon = this.listBalloon_Master[this.listBalloon_Master_数値管理];
												this.listBalloon_Master_数値管理++;
												break;
											}
											//else if( this.listBalloon.Count != 0 )
											//{
											//    chip.nBalloon = this.listBalloon[ this.listBalloon_Normal_数値管理 ];
											//    this.listBalloon_Normal_数値管理++;
											//    break;
											//}
											break;
									}
								}
								if (nObjectNum == 8)
								{
									chip.nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
									chip.nノーツ終了時刻ms = (int)this.dbNowTime;
									chip.fBMSCROLLTime_end = (float)this.dbNowBMScollTime;

									chip.nノーツ出現時刻ms = listChip[nNowRollCount].nノーツ出現時刻ms;
									chip.nノーツ移動開始時刻ms = listChip[nNowRollCount].nノーツ移動開始時刻ms;

									chip.n連打音符State = nNowRoll;
									listChip[nNowRollCount].nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
									listChip[nNowRollCount].nノーツ終了時刻ms = (int)this.dbNowTime;
									listChip[nNowRollCount].fBMSCROLLTime_end = (int)this.dbNowBMScollTime;
									listChip[nNowRollCount].db末端BPM = dbNowBPM;
									listChip[nNowRollCount].db末端SCROLL = this.dbNowScroll;
									nNowRoll = 0;
									//continue;
								}

								if (IsEnabledFixSENote)
								{
									chip.IsFixedSENote = true;
									chip.nSenote = FixSENote - 1;
								}

								#region[ 固定される種類のsenotesはここで設定しておく。 ]
								switch (nObjectNum)
								{
									case 3:
										chip.nSenote = 5;
										break;
									case 4:
										chip.nSenote = 6;
										break;
									case 5:
										chip.nSenote = 7;
										break;
									case 6:
										chip.nSenote = 0xA;
										break;
									case 7:
										chip.nSenote = 0xB;
										break;
									case 8:
										chip.nSenote = 0xC;
										break;
									case 9:
										chip.nSenote = 0xD;
										break;
									case 10:
										chip.nSenote = 0xE;
										break;
									case 11:
										chip.nSenote = 0xF;
										break;
								}
								#endregion

								if (nObjectNum < 5) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
								{
									#region [ 作り直し ]
									//譜面分岐がない譜面でも値は加算されてしまうがしゃあない
									//分岐を開始しない間は共通譜面としてみなす。

									if (this.b分岐を一回でも開始した)//一回も分岐していないのに加算させるのはおかしいだろ
									{
										if (IsEndedBranching)
											this.nノーツ数_Branch[i]++;
										else this.nノーツ数_Branch[chip.nコース]++;
									}
									if (!this.b分岐を一回でも開始した)
									{
										//IsEndedBranching==false = forloopが行われていないときのみ
										for (int l = 0; l < 3; l++)
											this.nノーツ数_Branch[l]++;
									}

									this.nノーツ数[3]++;
									#endregion
								}
								else if (nObjectNum == 7)
								{
									if (this.b最初の分岐である == false)
										this.n風船数[this.n現在のコース]++;
									else
										this.n風船数[3]++;
								}


								this.listChip.Add(chip);

							}
						}

						if (IsEnabledFixSENote) IsEnabledFixSENote = false;

						this.dbLastTime = this.dbNowTime;
						this.dbLastBMScrollTime = this.dbNowBMScollTime;
						this.dbNowTime += (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));
						this.dbNowBMScollTime += (((this.fNow_Measure_s / this.fNow_Measure_m)) * (16.0 / (double)n文字数));
					}
				}
			}
		}

		/// <summary>
		/// 難易度ごとによって変わるヘッダ値を読み込む。
		/// (BALLOONなど。)
		/// </summary>
		/// <param name="InputText"></param>
		private void t難易度別ヘッダ(string InputText)
		{
			if (TJAPlayer3.actEnumSongs.b活性化してない)
			{
				if (InputText.Equals("#HBSCROLL") && TJAPlayer3.ConfigIni.bスクロールモードを上書き == false)
				{
					TJAPlayer3.ConfigIni.eScrollMode = EScrollMode.HBSCROLL;
				}
				if (InputText.Equals("#BMSCROLL") && TJAPlayer3.ConfigIni.bスクロールモードを上書き == false)
				{
					TJAPlayer3.ConfigIni.eScrollMode = EScrollMode.BMSCROLL;
				}
			}

			string[] strArray = InputText.Split(new char[] { ':' });
			string strCommandName = "";
			string strCommandParam = "";

			if (strArray.Length == 2)
			{
				strCommandName = strArray[0].Trim();
				strCommandParam = strArray[1].Trim();
			}

			void ParseOptionalInt16(Action<short> setValue)
			{
				this.ParseOptionalInt16(strCommandName, strCommandParam, setValue);
			}

			if (strCommandName.Equals("BALLOON") || strCommandName.Equals("BALLOONNOR"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Normal);
			}
			else if (strCommandName.Equals("BALLOONEXP"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Expert);
				//tbBALLOON.Text = strCommandParam;
			}
			else if (strCommandName.Equals("BALLOONMAS"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Master);
				//tbBALLOON.Text = strCommandParam;
			}
			else if (strCommandName.Equals("SCOREMODE"))
			{
				ParseOptionalInt16(value => this.nScoreModeTmp = value);
			}
			else if (strCommandName.Equals("SCOREINIT"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					string[] scoreinit = strCommandParam.Split(',');

					this.ParseOptionalInt16("SCOREINIT first value", scoreinit[0], value =>
					{
						this.nScoreInit[0, this.n参照中の難易度] = value;
						this.b配点が指定されている[0, this.n参照中の難易度] = true;
					});

					if (scoreinit.Length == 2)
					{
						this.ParseOptionalInt16("SCOREINIT second value", scoreinit[1], value =>
						{
							this.nScoreInit[1, this.n参照中の難易度] = value;
							this.b配点が指定されている[2, this.n参照中の難易度] = true;
						});
					}
				}
			}
			else if (strCommandName.Equals("SCOREDIFF"))
			{
				ParseOptionalInt16(value =>
				{
					this.nScoreDiff[this.n参照中の難易度] = value;
					this.b配点が指定されている[1, this.n参照中の難易度] = true;
				});
			}

			//if( this.nScoreModeTmp == 99 ) //2017.01.28 DD SCOREMODEを入力していない場合のみConfigで設定したモードにする
			//{
			//    this.nScoreModeTmp = CDTXMania.ConfigIni.nScoreMode;
			//}
			//if( CDTXMania.ConfigIni.nScoreMode == 3 && !this.b配点が指定されている[ 2, this.n参照中の難易度 ] ){ //2017.06.04 kairera0467
			//    this.nScoreModeTmp = 3;
			//}



			else if (strCommandName.Equals("SCOREMODE"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.nScoreModeTmp = Convert.ToInt16(strCommandParam);
				}
			}
			else if (strCommandName.Equals("SCOREINIT"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					string[] scoreinit = strCommandParam.Split(',');

					this.nScoreInit[0, this.n参照中の難易度] = Convert.ToInt16(scoreinit[0]);
					this.b配点が指定されている[0, this.n参照中の難易度] = true;
					if (scoreinit.Length == 2)
					{
						this.nScoreInit[1, this.n参照中の難易度] = Convert.ToInt16(scoreinit[1]);
						this.b配点が指定されている[2, this.n参照中の難易度] = true;
					}
				}
			}
			else if (strCommandName.Equals("SCOREDIFF"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.nScoreDiff[this.n参照中の難易度] = Convert.ToInt16(strCommandParam);
					this.b配点が指定されている[1, this.n参照中の難易度] = true;
				}
			}
			else if (strCommandName.Equals("EXAM1") || strCommandName.Equals("EXAM2") || strCommandName.Equals("EXAM3"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					Exam.Type examType;
					int[] examValue;
					Exam.Range examRange;
					var splitExam = strCommandParam.Split(',');
					switch (splitExam[0])
					{
						case "g":
							examType = Exam.Type.Gauge;
							break;
						case "jp":
							examType = Exam.Type.JudgePerfect;
							break;
						case "jg":
							examType = Exam.Type.JudgeGood;
							break;
						case "jb":
							examType = Exam.Type.JudgeBad;
							break;
						case "s":
							examType = Exam.Type.Score;
							break;
						case "r":
							examType = Exam.Type.Roll;
							break;
						case "h":
							examType = Exam.Type.Hit;
							break;
						case "c":
							examType = Exam.Type.Combo;
							break;
						default:
							examType = Exam.Type.Gauge;
							break;
					}
					try
					{
						examValue = new int[] { int.Parse(splitExam[1]), int.Parse(splitExam[2]) };
					}
					catch (Exception)
					{
						examValue = new int[] { 100, 100 };
					}
					switch (splitExam[3])
					{
						case "m":
							examRange = Exam.Range.More;
							break;
						case "l":
							examRange = Exam.Range.Less;
							break;
						default:
							examRange = Exam.Range.More;
							break;
					}
					Dan_C[int.Parse(strCommandName.Substring(4)) - 1] = new Dan_C(examType, examValue, examRange);
				}
			}
			if (this.nScoreModeTmp == 99) //2017.01.28 DD SCOREMODEを入力していない場合のみConfigで設定したモードにする
			{
				this.nScoreModeTmp = TJAPlayer3.ConfigIni.nScoreMode;
			}
			if (TJAPlayer3.ConfigIni.nScoreMode == 3 && !this.b配点が指定されている[2, this.n参照中の難易度])
			{ //2017.06.04 kairera0467
				this.nScoreModeTmp = 3;
			}
		}

		private void ParseOptionalInt16(string name, string unparsedValue, Action<short> setValue)
		{
			if (string.IsNullOrEmpty(unparsedValue))
			{
				return;
			}

			if (short.TryParse(unparsedValue, out var value))
			{
				setValue(value);
			}
			else
			{
				Trace.TraceWarning($"命令名: {name} のパラメータの値が正しくないことを検知しました。値: {unparsedValue} ({strファイル名の絶対パス})");
			}
		}


		private void ParseBalloon(string strCommandParam, List<int> listBalloon)
		{
			string[] strParam = strCommandParam.Split(',');
			for (int n = 0; n < strParam.Length; n++)
			{
				int n打数;
				try
				{
					if (strParam[n] == null || strParam[n] == "")
						break;

					n打数 = Convert.ToInt32(strParam[n]);
				}
				catch (Exception ex)
				{
					Trace.TraceError($"おや?エラーが出たようです。お兄様。 ({strファイル名の絶対パス})");
					Trace.TraceError(ex.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (95327158-4e83-4fa9-b5e9-ad3c3d4c2a22)");
					break;
				}

				listBalloon.Add(n打数);
			}
		}
		private void t入力_行解析ヘッダ(string InputText)
		{
			//やべー。先頭にコメント行あったらやばいやん。
			string[] strArray = InputText.Split(new char[] { ':' });
			string strCommandName = "";
			string strCommandParam = "";

			if (InputText.StartsWith("#BRANCHSTART"))
			{
				//2015.08.18 kairera0467
				//本来はヘッダ命令ではありませんが、難易度ごとに違う項目なのでここで読み込ませます。
				//Lengthのチェックをされる前ににif文を入れています。
				this.bHasBranch[this.n参照中の難易度] = true;
			}

			//まずは「:」でSplitして割り当てる。
			if (strArray.Length == 2)
			{
				strCommandName = strArray[0].Trim();
				strCommandParam = strArray[1].Trim();
			}
			else if (strArray.Length > 2)
			{
				//strArrayが2じゃない場合、ヘッダのSplitを通していない可能性がある。
				//この処理自体は「t入力」を改造したもの。STARTでSplitしていない等、一部の処理が異なる。

				#region[ヘッダ]
				InputText = InputText.Replace(Environment.NewLine, "\n"); //改行文字を別の文字列に差し替え。
				InputText = InputText.Replace('\t', ' '); //何の文字か知らないけどスペースに差し替え。
				InputText = InputText + "\n";

				string[] strDelimiter = { "#START" };
				strArray = InputText.Split(strDelimiter, StringSplitOptions.RemoveEmptyEntries);

				string[] strDelimiter2 = { "\n" };
				strArray = InputText.Split(strDelimiter2, StringSplitOptions.RemoveEmptyEntries);


				strArray = strArray[0].Split(new char[] { ':' });

				strCommandName = strArray[0].Trim();
				strCommandParam = strArray[1].Trim();

				#endregion
				//lblMessage.Text = "おや?strArrayのLengthが2じゃないようですね。お兄様。";
			}

			void ParseOptionalInt16(Action<short> setValue)
			{
				this.ParseOptionalInt16(strCommandName, strCommandParam, setValue);
			}

			//パラメータを分別、そこから割り当てていきます。
			if (strCommandName.Equals("TITLE"))
			{
				//this.TITLE = strCommandParam;
				this.TITLE = InputText.Substring(InputText.IndexOf(":") + 1);
				//tbTitle.Text = strCommandParam;
				if (InputText.Contains("\n"))
				{
					Debug.Print("");
				}
			}
			if (strCommandName.Equals("SUBTITLE"))
			{
				if (strCommandParam.StartsWith("--"))
				{
					this.SUBTITLE = InputText.Substring(InputText.IndexOf(":") + 3);
				}
				else if (strCommandParam.StartsWith("++"))
				{
					this.SUBTITLEDisp = true;
					this.SUBTITLE = InputText.Substring(InputText.IndexOf(":") + 3);
				}
				else
				{
					this.SUBTITLEDisp = true;
					this.SUBTITLE = InputText.Substring(InputText.IndexOf(":") + 1);
				}
			}
			else if (strCommandName.Equals("LEVEL"))
			{
				var level = (int)Convert.ToDouble(strCommandParam);
				this.LEVELtaiko[this.n参照中の難易度] = level;
			}
			else if (strCommandName.Equals("BPM"))
			{
				if (strCommandParam.IndexOf(",") != -1)
					strCommandParam = strCommandParam.Replace(',', '.');

				double dbBPM = Convert.ToDouble(strCommandParam);
				this.BPM = dbBPM;
				this.BASEBPM = dbBPM;
				this.dbNowBPM = dbBPM;

				this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = this.n内部番号BPM1to - 1, dbBPM値 = dbBPM, });
				this.n内部番号BPM1to++;


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x03;
				chip.n発声位置 = ((this.n現在の小節数 - 1) * 384);
				chip.n整数値 = 0x00;
				chip.n整数値_内部番号 = 1;

				this.listChip.Add(chip);
				//tbBPM.Text = strCommandParam;
			}
			else if (strCommandName.Equals("WAVE"))
			{
				if (strBGM_PATH != null)
				{
					Trace.TraceWarning($"{nameof(CDTX)} is ignoring an extra WAVE header in {this.strファイル名の絶対パス}");
				}
				else
				{
					this.strBGM_PATH = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, strCommandParam);
					//tbWave.Text = strCommandParam;
					if (this.listWAV != null)
					{
						// 2018-08-27 twopointzero - DO attempt to load (or queue scanning) loudness metadata here.
						//                           TJAP3 is either launching, enumerating songs, or is about to
						//                           begin playing a song. If metadata is available, we want it now.
						//                           If is not yet available then we wish to queue scanning.
						var absoluteBgmPath = Path.Combine(this.strフォルダ名, this.strBGM_PATH);
						this.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(absoluteBgmPath);

						var wav = new CWAV()
						{
							n内部番号 = this.n内部番号WAV1to,
							n表記上の番号 = 1,
							nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
							n位置 = 0,
							SongVol = this.SongVol,
							SongLoudnessMetadata = this.SongLoudnessMetadata,
							strファイル名 = this.strBGM_PATH,
							strコメント文 = "TJA BGM",
						};

						this.listWAV.Add(this.n内部番号WAV1to, wav);
						this.n内部番号WAV1to++;
					}
				}
			}
			else if (strCommandName.Equals("OFFSET") && !string.IsNullOrEmpty(strCommandParam))
			{
				this.nOFFSET = (int)(Convert.ToDouble(strCommandParam) * 1000);
				this.bOFFSETの値がマイナスである = this.nOFFSET < 0 ? true : false;

				this.listBPM[0].bpm_change_bmscroll_time = -2000 * this.dbNowBPM / 15000;
				if (this.bOFFSETの値がマイナスである == true)
					this.nOFFSET = this.nOFFSET * -1; //OFFSETは秒を加算するので、必ず正の数にすること。
				//tbOFFSET.Text = strCommandParam;
			}
			else if (strCommandName.Equals("MOVIEOFFSET"))
			{
				this.nMOVIEOFFSET = (int)(Convert.ToDouble(strCommandParam) * 1000);
				this.bMOVIEOFFSETの値がマイナスである = this.nMOVIEOFFSET < 0 ? true : false;

				if (this.bMOVIEOFFSETの値がマイナスである == true)
					this.nMOVIEOFFSET = this.nMOVIEOFFSET * -1; //OFFSETは秒を加算するので、必ず正の数にすること。
				//tbOFFSET.Text = strCommandParam;
			}
			#region[移動→不具合が起こるのでここも一応復活させておく]
			else if (strCommandName.Equals("BALLOON") || strCommandName.Equals("BALLOONNOR"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Normal);
			}
			else if (strCommandName.Equals("BALLOONEXP"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Expert);
				//tbBALLOON.Text = strCommandParam;
			}
			else if (strCommandName.Equals("BALLOONMAS"))
			{
				ParseBalloon(strCommandParam, this.listBalloon_Master);
				//tbBALLOON.Text = strCommandParam;
			}
			else if (strCommandName.Equals("SCOREMODE"))
			{
				ParseOptionalInt16(value => this.nScoreModeTmp = value);
			}
			else if (strCommandName.Equals("SCOREINIT"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					string[] scoreinit = strCommandParam.Split(',');

					this.ParseOptionalInt16("SCOREINIT first value", scoreinit[0], value =>
					{
						this.nScoreInit[0, this.n参照中の難易度] = value;
					});

					if (scoreinit.Length == 2)
					{
						this.ParseOptionalInt16("SCOREINIT second value", scoreinit[1], value =>
						{
							this.nScoreInit[1, this.n参照中の難易度] = value;
						});
					}
				}
			}
			else if (strCommandName.Equals("GAUGEINCR"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					switch (strCommandParam.ToLower())
					{
						case "normal":
							GaugeIncreaseMode = GaugeIncreaseMode.Normal;
							break;
						case "floor":
							GaugeIncreaseMode = GaugeIncreaseMode.Floor;
							break;
						case "round":
							GaugeIncreaseMode = GaugeIncreaseMode.Round;
							break;
						case "ceiling":
							GaugeIncreaseMode = GaugeIncreaseMode.Ceiling;
							break;
						case "notfix":
							GaugeIncreaseMode = GaugeIncreaseMode.NotFix;
							break;
						default:
							GaugeIncreaseMode = GaugeIncreaseMode.Normal;
							break;
					}
				}
			}
			else if (strCommandName.Equals("SCOREDIFF"))
			{
				ParseOptionalInt16(value => this.nScoreDiff[this.n参照中の難易度] = value);
			}
			#endregion
			else if (strCommandName.Equals("SONGVOL") && !string.IsNullOrEmpty(strCommandParam))
			{
				this.SongVol = Convert.ToInt32(strCommandParam).Clamp(CSound.MinimumSongVol, CSound.MaximumSongVol);

				foreach (var kvp in this.listWAV)
				{
					kvp.Value.SongVol = this.SongVol;
				}
			}
			else if (strCommandName.Equals("SEVOL"))
			{
				//tbSeVol.Text = strCommandParam;
			}
			else if (strCommandName.Equals("COURSE"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					//this.n参照中の難易度 = Convert.ToInt16( strCommandParam );
					this.n参照中の難易度 = this.strConvertCourse(strCommandParam);
				}
			}

			else if (strCommandName.Equals("HEADSCROLL"))
			{
				//新定義:初期スクロール速度設定(というよりこのシステムに合わせるには必須。)
				//どうしても一番最初に1小節挿入されるから、こうするしかなかったんだ___

				this.dbScrollSpeed = Convert.ToDouble(strCommandParam);

				this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = this.dbScrollSpeed, });


				//チップ追加して割り込んでみる。
				var chip = new CChip();

				chip.nチャンネル番号 = 0x9D;
				chip.n発声位置 = ((this.n現在の小節数 - 2) * 384);
				chip.n整数値 = 0x00;
				chip.n整数値_内部番号 = this.n内部番号SCROLL1to;
				chip.dbSCROLL = this.dbScrollSpeed;

				// チップを配置。

				this.listChip.Add(chip);
				this.n内部番号SCROLL1to++;

				//this.nScoreDiff = Convert.ToInt16( strCommandParam );
				//tbScoreDiff.Text = strCommandParam;
			}
			else if (strCommandName.Equals("GENRE"))
			{
				//2015.03.28 kairera0467
				//ジャンルの定義。DTXから入力もできるが、tjaからも入力できるようにする。
				//日本語名だと選曲画面でバグが出るので、そこもどうにかしていく予定。

				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.GENRE = strCommandParam;
				}
			}
			else if (strCommandName.Equals("DEMOSTART"))
			{
				//2015.04.10 kairera0467

				if (!string.IsNullOrEmpty(strCommandParam))
				{
					int nOFFSETms;
					try
					{
						nOFFSETms = (int)(Convert.ToDouble(strCommandParam) * 1000.0);
					}
					catch
					{
						nOFFSETms = 0;
					}


					this.nデモBGMオフセット = nOFFSETms;
				}
			}
			else if (strCommandName.Equals("BGMOVIE"))
			{
				//2016.02.02 kairera0467
				//背景動画の定義。DTXから入力もできるが、tjaからも入力できるようにする。

				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.strBGVIDEO_PATH =
						CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, strCommandParam);
				}

				string strVideoFilename;
				if (!string.IsNullOrEmpty(this.PATH_WAV))
					strVideoFilename = this.PATH_WAV + this.strBGVIDEO_PATH;
				else
					strVideoFilename = this.strフォルダ名 + this.strBGVIDEO_PATH;

				CVideoDecoder vd = new CVideoDecoder(TJAPlayer3.app.Device, strVideoFilename);

				if (this.listVD.ContainsKey(1))
					this.listVD.Remove(1);

				this.listVD.Add(1, vd);
			}
			else if (strCommandName.Equals("BGIMAGE"))
			{
				//2016.02.02 kairera0467
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.strBGIMAGE_PATH = strCommandParam;
				}
			}
			else if (strCommandName.Equals("HIDDENBRANCH"))
			{
				//2016.04.01 kairera0467 パラメーターは
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					this.bHIDDENBRANCH = true;
				}
			}
			else if (strCommandName.Equals("LYRICFILE"))
			{
				if (!string.IsNullOrEmpty(strCommandParam))
				{
					string[] strFiles = SplitComma(strCommandParam);
					string[] strFilePath = new string[strFiles.Length];
					for (int index = 0; index < strFiles.Length; index++)
					{
						strFilePath[index] = this.strフォルダ名 + strFiles[index];
						if (File.Exists(strFilePath[index]))
						{
							try
							{
								if (TJAPlayer3.r現在のステージ.eステージID == CStage.Eステージ.曲読み込み)//2020.06.06 Mr-Ojii 起動時に重たくなってしまう問題の修正用
									this.LyricFileParser(strFilePath[index],index);
								this.bLyrics = true;
							}
							catch
							{
								Console.WriteLine("lrcファイルNo.{0}の読み込みに失敗しましたが、", index);
								Console.WriteLine("処理を続行します。");
							}
						}
					}
				}
			}
			if (this.nScoreModeTmp == 99)
			{
				//2017.01.28 DD 
				this.nScoreModeTmp = TJAPlayer3.ConfigIni.nScoreMode;
			}
		}

		/// <summary>
		/// 指定した文字が数値かを返すメソッド
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public bool bIsNumber(char Char) //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
		{
			if ((Char >= '0') && (Char <= '9'))
				return true;
			else
				return false;
		}

		/// <summary>
		/// string型からint型に変換する。
		/// TJAP2から持ってきた。
		/// </summary>
		private int CharConvertNote(string str)
		{
			switch (str)
			{
				case "0":
					return 0;
				case "1":
					return 1;
				case "2":
					return 2;
				case "3":
					return 3;
				case "4":
					return 4;
				case "5":
					return 5;
				case "6":
					return 6;
				case "7":
					return 7;
				case "8":
					return 8;
				case "9":
					return 7; //2017.01.30 DD 芋連打を風船連打扱いに
				case "A": //2017.08.22 kairera0467 手つなぎ
					return 10;
				case "F":
					return 15;
				default:
					return -1;
			}
		}

		private int strConvertCourse(string str)
		{
			//2016.08.24 kairera0467
			//正規表現を使っているため、easyでもEASYでもOK。

			// 小文字大文字区別しない正規表現で仮対応。 (AioiLight)
			// 相変わらず原始的なやり方だが、正常に動作した。
			string[] Matchptn = new string[7] { "easy", "normal", "hard", "oni", "edit", "tower", "dan" };
			for (int i = 0; i < Matchptn.Length; i++)
			{
				if (Regex.IsMatch(str, Matchptn[i], RegexOptions.IgnoreCase))
				{
					return i;
				}
			}

			switch (str)
			{
				case "0":
					return 0;
				case "1":
					return 1;
				case "2":
					return 2;
				case "3":
					return 3;
				case "4":
					return 4;
				case "5":
					return 5;
				case "6":
					return 6;
				default:
					return 3;
			}
		}

		/// <summary>
		/// Lyricファイルのパースもどき
		/// 自力で作ったので、うまくパースしてくれないかも
		/// </summary>
		/// <param name="strFilePath">lrcファイルのパス</param>
		private void LyricFileParser(string strFilePath ,int ordnumber)//2020.06.04 Mr-Ojii lrcファイルのパース用
		{
			Encoding lrcファイルenc = TJAPlayer3.JudgeTextEncoding.JudgeFileEncoding(strFilePath);
			StreamReader reader = new StreamReader(strFilePath, lrcファイルenc);
			string str = reader.ReadToEnd();
			reader.Close();
			var strSplit後 = str.Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);
			Regex timeRegex = new Regex(@"^(\[)(\d{2})(:)(\d{2})([:.])(\d{2})(\])", RegexOptions.Multiline | RegexOptions.Compiled);
			Regex timeRegexO = new Regex(@"^(\[)(\d{2})(:)(\d{2})(\])", RegexOptions.Multiline | RegexOptions.Compiled);
			List<long> list;
			for (int i = 0; i < strSplit後.Length; i++)
			{ 
				list = new List<long>();
				if (!String.IsNullOrEmpty(strSplit後[i]))
				{
					if (strSplit後[i].StartsWith("[")) 
					{
						Match timestring = timeRegex.Match(strSplit後[i]) , timestringO = timeRegexO.Match(strSplit後[i]);
						while ( timestringO.Success || timestring.Success)
						{
							long time;
							if (timestring.Success)
							{
								time = Int32.Parse(timestring.Groups[2].Value) * 60000 + Int32.Parse(timestring.Groups[4].Value) * 1000 + Int32.Parse(timestring.Groups[6].Value) * 10;
								strSplit後[i] = strSplit後[i].Remove(0, 10);
							}
							else if (timestringO.Success)
							{
								time = Int32.Parse(timestringO.Groups[2].Value) * 60000 + Int32.Parse(timestringO.Groups[4].Value) * 1000;
								strSplit後[i] = strSplit後[i].Remove(0, 7);
							}
							else
								break;
							list.Add(time);
							timestring = timeRegex.Match(strSplit後[i]);
							timestringO = timeRegexO.Match(strSplit後[i]);

						}
						strSplit後[i] = strSplit後[i].Replace("\r", "").Replace("\n", "");

						for (int listindex = 0; listindex < list.Count; listindex++) 
						{
							STLYRIC stlrc;
							stlrc.Text = strSplit後[i];
							stlrc.TextTex = this.pf歌詞フォント.DrawPrivateFont(strSplit後[i], TJAPlayer3.Skin.Game_Lyric_ForeColor, TJAPlayer3.Skin.Game_Lyric_BackColor);
							stlrc.Time = list[listindex];
							stlrc.index = ordnumber;
							this.listLyric2.Add(stlrc);
						}
					}
				}
			}
		}


		/// <summary>
		/// 複素数のパースもどき
		/// </summary>
		private void tParsedComplexNumber(string strScroll, ref double[] dbScroll)
		{
			bool bFirst = true; //最初の数値か
			bool bUse = false; //数値扱い中
			string[] arScroll = new string[2];
			char[] c = strScroll.ToCharArray();
			//1.0-1.0i
			for (int i = 0; i < strScroll.Length; i++)
			{
				if (bFirst)
					arScroll[0] += c[i];
				else
					arScroll[1] += c[i];

				//次の文字が'i'なら脱出。
				if (c[i + 1] == 'i')
					break;
				else if (c[i + 1] == '-' || c[i + 1] == '+')
					bFirst = false;

			}

			dbScroll[0] = Convert.ToDouble(arScroll[0]);
			dbScroll[1] = Convert.ToDouble(arScroll[1]);
			return;
		}

		private void tSetSenotes()
		{
			#region[ list作成 ]
			//ひとまずチップだけのリストを作成しておく。
			List<CDTX.CChip> list音符のみのリスト;
			list音符のみのリスト = new List<CChip>();
			int nCount = 0;
			int dkdkCount = 0;

			foreach (CChip chip in this.listChip)
			{
				if (chip.nチャンネル番号 >= 0x11 && chip.nチャンネル番号 < 0x18)
				{
					list音符のみのリスト.Add(chip);
				}
			}
			#endregion

			//時間判定は、「次のチップの発声時刻」から「現在(過去)のチップの発声時刻」で引く必要がある。
			//逆にしてしまうと計算がとてつもないことになるので注意。

			try
			{
				//this.tSenotes_Core( list音符のみのリスト );
				this.tSenotes_Core_V2(list音符のみのリスト, true);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (b67473e4-1930-44f1-b320-4ead5786e74c)");
			}


			#region[統合前]
			//foreach( CChip pChip in list音符のみのリスト )
			//{
			//    int dbUnitTime = ( int )( ( ( 60.0 / this.dbNowBPM ) / 4.0 ) * 1000.0 );
			//    int nUnit4 = dbUnitTime * 4;
			//    int nUnit8 = dbUnitTime * 2;
			//    int nUnit16 = dbUnitTime;

			//    if( nCount == 0  )
			//    {
			//        nCount++;
			//        continue;
			//    }

			//    double db1個前の発生時刻ms = list音符のみのリスト[nCount - 1].n発声時刻ms * 1;

			//    if( nCount == 1 )
			//    {
			//        //nCount - 1は一番最初のノーツになる。

			//        if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms >= nUnit4 )
			//        {
			//            if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x93 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 0;
			//            else if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x94 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 3;

			//            if( list音符のみのリスト[ nCount + 1 ].n発声時刻ms - pChip.n発声時刻ms < nUnit4 )
			//            {
			//                if( list音符のみのリスト[ nCount + 1 ].n発声時刻ms - pChip.n発声時刻ms < nUnit8 )
			//                {
			//                    //16分なら「ド」
			//                    pChip.nSenote = 1;
			//                }
			//                else
			//                {
			//                    if( dkdkCount == 0 )
			//                    {
			//                        pChip.nSenote = 1;
			//                        dkdkCount++;
			//                    }
			//                    else if( dkdkCount == 1 )
			//                    {
			//                        pChip.nSenote = 2;
			//                        dkdkCount = 0;
			//                    }

			//                }
			//            }
			//            else
			//            {
			//                //次も4分なら「ドン」か「カッ」
			//                if( pChip.nチャンネル番号 == 0x93 )
			//                {
			//                    pChip.nSenote = 0;
			//                }
			//                else if( pChip.nチャンネル番号 == 0x94 )
			//                {
			//                    pChip.nSenote = 3;
			//                }
			//            }
			//        }
			//        else if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms <= nUnit4 && pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms >= nUnit8 )
			//        {
			//            if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x93 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 1;
			//            else if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x94 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 4;

			//            if( pChip.nチャンネル番号 == 0x93 )
			//            {
			//                pChip.nSenote = 1;
			//            }
			//            else if( pChip.nチャンネル番号 == 0x94 )
			//            {
			//                pChip.nSenote = 4;
			//            }
			//        }
			//        else if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms < nUnit8 )
			//        {
			//            if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x93 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 1;
			//            else if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x94 )
			//                list音符のみのリスト[ nCount - 1 ].nSenote = 4;

			//            if( pChip.nチャンネル番号 == 0x93 )
			//            {
			//                pChip.nSenote = 1;
			//            }
			//            else if( pChip.nチャンネル番号 == 0x94 )
			//            {
			//                pChip.nSenote = 4;
			//            }
			//        }

			//        nCount++;
			//        continue;
			//    }

			//    double db2個前の発声時刻ms = list音符のみのリスト[ nCount - 2 ].n発声時刻ms * 1;

			//    #region[新しいやつ]
			//    if( nCount + 1 >= list音符のみのリスト.Count )
			//        break;

			//    if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms >= nUnit4 )
			//    {
			//        if( pChip.nチャンネル番号 == 0x93 )
			//        {
			//            pChip.nSenote = 0;
			//        }
			//        else if( pChip.nチャンネル番号 == 0x94 )
			//        {
			//            pChip.nSenote = 3;
			//        }

			//        if( list音符のみのリスト[ nCount + 1 ].n発声時刻ms - pChip.n発声時刻ms <= nUnit4 )
			//        {
			//            if( pChip.nチャンネル番号 == 0x93 )
			//                pChip.nSenote = 1;
			//            else if( pChip.nチャンネル番号 == 0x94 )
			//                pChip.nSenote = 4;
			//        }
			//    }
			//    else if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms < nUnit4 && pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms >= nUnit8 )
			//    {
			//        if( pChip.nチャンネル番号 == 0x93 )
			//        {
			//            pChip.nSenote = 1;
			//        }
			//        else if( pChip.nチャンネル番号 == 0x94 )
			//        {
			//            pChip.nSenote = 4;
			//        }

			//        if( list音符のみのリスト[ nCount + 1 ].n発声時刻ms - pChip.n発声時刻ms <= nUnit4 )
			//        {
			//            if( pChip.nチャンネル番号 == 0x93 )
			//                pChip.nSenote = 0;
			//            else if( pChip.nチャンネル番号 == 0x94 )
			//                pChip.nSenote = 3;

			//            if( list音符のみのリスト[ nCount + 2 ].n発声時刻ms - list音符のみのリスト[ nCount + 1 ].n発声時刻ms >= nUnit8 )
			//            {
			//                if( pChip.nチャンネル番号 == 0x93 )
			//                    pChip.nSenote = 1;
			//                else if( pChip.nチャンネル番号 == 0x94 )
			//                    pChip.nSenote = 4;
			//            }
			//            else if( list音符のみのリスト[ nCount + 2 ].n発声時刻ms - list音符のみのリスト[ nCount + 1 ].n発声時刻ms < nUnit8 )
			//            {
			//                if( pChip.nチャンネル番号 == 0x93 )
			//                    pChip.nSenote = 1;
			//                else if( pChip.nチャンネル番号 == 0x94 )
			//                    pChip.nSenote = 4;
			//            }

			//        }
			//        else
			//        {
			//            if( pChip.nチャンネル番号 == 0x93 )
			//                pChip.nSenote = 0;
			//            else if( pChip.nチャンネル番号 == 0x94 )
			//                pChip.nSenote = 3;
			//        }
			//    }
			//    else if( pChip.n発声時刻ms - list音符のみのリスト[ nCount - 1 ].n発声時刻ms < nUnit8 ) //8分以下
			//    {
			//        if( pChip.nチャンネル番号 == 0x93 )
			//        {
			//            pChip.nSenote = 1;
			//        }
			//        else if( pChip.nチャンネル番号 == 0x94 )
			//        {
			//            pChip.nSenote = 4;
			//        }

			//        //後ろが4分
			//        try
			//        {
			//            if( nCount + 1 >= list音符のみのリスト.Count )
			//                break;

			//            if( list音符のみのリスト[ nCount + 1 ].n発声時刻ms - pChip.n発声時刻ms >= nUnit8 ) //分岐があるとここがバグるっぽい?(Indexエラー)
			//            {
			//                if( pChip.nチャンネル番号 == 0x93 )
			//                {
			//                    pChip.nSenote = 0;
			//                }
			//                else if( pChip.nチャンネル番号 == 0x94 )
			//                {
			//                    pChip.nSenote = 3;
			//                }
			//            }
			//        }
			//        catch( Exception ex )
			//        {

			//        }


			//    }
			//    #endregion

			//    #region[古いやつ]
			//    ////2つ前と1つ前のチップのSenoteを決めていく。
			//    ////連打、大音符などはチップ配置の際に決めます。
			//    //if (( db1個前の発生時刻ms - db2個前の発声時刻ms ) >= nUnit4)
			//    //{
			//    //    //2つ前の音符と1つ前の音符の間が4分以上でかつ、その音符がドンなら2つ前のSenoteは「ドン」で確定。
			//    //    //同時にdkdkをリセット
			//    //    dkdkCount = false;
			//    //    if( list音符のみのリスト[nCount - 2].nチャンネル番号 == 0x93 )
			//    //        list音符のみのリスト[nCount - 2].nSenote = 0;
			//    //    else if( list音符のみのリスト[nCount - 2].nチャンネル番号 == 0x94 )
			//    //        list音符のみのリスト[nCount - 2].nSenote = 3;

			//    //    if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) >= nUnit4 )
			//    //    {
			//    //        //1つ前の音符と現在の音符の間が4分以上かつ、その音符がドンなら1つ前の音符は「ドン」で確定。
			//    //        if( list音符のみのリスト[nCount - 1].nチャンネル番号 == 0x93 )
			//    //            list音符のみのリスト[nCount - 1].nSenote = 0;
			//    //        else if( list音符のみのリスト[nCount - 1].nチャンネル番号 == 0x94 )
			//    //            list音符のみのリスト[nCount - 1].nSenote = 3;
			//    //    }
			//    //    else if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) <= nUnit4 )
			//    //    {
			//    //        //4分
			//    //        if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) >= nUnit8 )
			//    //        {
			//    //            dkdkCount = false;
			//    //            //1つ前の音符と現在の音符の間が8分以内で16分以上でかつ、その音符が赤なら1つ前の音符は「ド」で確定。
			//    //            if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x94 )
			//    //                list音符のみのリスト[ nCount - 1 ].nSenote = 2;
			//    //            else if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x94 )
			//    //                list音符のみのリスト[ nCount - 1 ].nSenote = 4;
			//    //        }
			//    //        else if( ( db1個前の発生時刻ms - db2個前の発声時刻ms ) <= nUnit8 )
			//    //        {
			//    //            dkdkCount = false;
			//    //            if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //            {
			//    //                list音符のみのリスト[ nCount - 2 ].nSenote = 1;

			//    //                //ドコドン
			//    //                if( list音符のみのリスト[ nCount - 1 ].nチャンネル番号 == 0x93 )
			//    //                {
			//    //                    if( pChip.nチャンネル番号 == 0x93 )
			//    //                        pChip.nSenote = dkdkCount ? 2 : 1;
			//    //                    if( dkdkCount == false )
			//    //                        dkdkCount = true;
			//    //                    else
			//    //                        dkdkCount = false;
			//    //                }
			//    //            }
			//    //            else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //                list音符のみのリスト[ nCount - 2 ].nSenote = 4;
			//    //        }

			//    //    }
			//    //}
			//    //else if ( ( db1個前の発生時刻ms - db2個前の発声時刻ms ) <= nUnit4 && ( db1個前の発生時刻ms - db2個前の発声時刻ms ) >= nUnit8)
			//    //{
			//    //    //2つ前の音符と1つ前の音符の間が8分以上でかつ、16分以内

			//    //    if( ( db1個前の発生時刻ms - db2個前の発声時刻ms ) >= nUnit8 && ( db1個前の発生時刻ms - db2個前の発声時刻ms ) > nUnit16 )
			//    //    {
			//    //        //2つ前の音符と1つ前の音符の間が8分以上でかつ、16分以内なら「ド」
			//    //        if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //        {
			//    //            list音符のみのリスト[ nCount - 2 ].nSenote = 1;
			//    //        }
			//    //        else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //        {
			//    //            list音符のみのリスト[ nCount - 2 ].nSenote = 4;
			//    //        }
			//    //    }
			//    //    else if( ( db1個前の発生時刻ms - db2個前の発声時刻ms ) < nUnit8 )
			//    //    {
			//    //        //2つ前の音符と1つ前の音符の間が16分以内なら「ド」で確定
			//    //        if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //        {
			//    //            list音符のみのリスト[ nCount - 2 ].nSenote = 1;
			//    //        }
			//    //        else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //            list音符のみのリスト[ nCount - 2 ].nSenote = 4;
			//    //    }

			//    //    if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) >= nUnit16 )
			//    //    {
			//    //        if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) >= nUnit8 )
			//    //        {
			//    //            if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //            {
			//    //                list音符のみのリスト[ nCount - 1 ].nSenote = 0;
			//    //            }
			//    //            else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //                list音符のみのリスト[ nCount - 1 ].nSenote = 3;
			//    //        }
			//    //    }
			//    //}
			//    //else if ( ( db1個前の発生時刻ms - db2個前の発声時刻ms ) >= nUnit16 && ( db1個前の発生時刻ms - db2個前の発声時刻ms ) <= nUnit8 )
			//    //{
			//    //    //2つ前の音符と1つ前の音符の間が16分以上
			//    //    if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //    {
			//    //        list音符のみのリスト[ nCount - 2 ].nSenote = 1;
			//    //    }
			//    //    else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //        list音符のみのリスト[ nCount - 2 ].nSenote = 4;

			//    //    if( ( pChip.n発声時刻ms - db1個前の発生時刻ms ) >= nUnit8 )
			//    //    {
			//    //        if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x93 )
			//    //        {
			//    //            list音符のみのリスト[ nCount - 1 ].nSenote = 0;
			//    //        }
			//    //        else if( list音符のみのリスト[ nCount - 2 ].nチャンネル番号 == 0x94 )
			//    //            list音符のみのリスト[ nCount - 1 ].nSenote = 3;
			//    //    }


			//    //}
			//    #endregion

			//    nCount++;
			//}
			#endregion


		}

		/// <summary>
		/// 譜面分岐がある場合はこちらを使う
		/// </summary>
		private void tSetSenotes_branch()
		{
			#region[ list作成 ]
			//ひとまずチップだけのリストを作成しておく。
			List<CDTX.CChip> list音符のみのリスト;
			List<CDTX.CChip> list普通譜面のみのリスト;
			List<CDTX.CChip> list玄人譜面のみのリスト;
			List<CDTX.CChip> list達人譜面のみのリスト;
			list音符のみのリスト = new List<CChip>();
			list普通譜面のみのリスト = new List<CChip>();
			list玄人譜面のみのリスト = new List<CChip>();
			list達人譜面のみのリスト = new List<CChip>();
			int nCount = 0;
			int dkdkCount = 0;

			foreach (CChip chip in this.listChip)
			{
				if (chip.nチャンネル番号 >= 0x11 && chip.nチャンネル番号 < 0x18)
				{
					list音符のみのリスト.Add(chip);

					switch (chip.nコース)
					{
						case 0:
							list普通譜面のみのリスト.Add(chip);
							break;
						case 1:
							list玄人譜面のみのリスト.Add(chip);
							break;
						case 2:
							list達人譜面のみのリスト.Add(chip);
							break;
					}
				}
			}
			#endregion

			//forで処理。
			for (int n = 0; n < 3; n++)
			{
				switch (n)
				{
					case 0:
						list音符のみのリスト = list普通譜面のみのリスト;
						break;
					case 1:
						list音符のみのリスト = list玄人譜面のみのリスト;
						break;
					case 2:
						list音符のみのリスト = list達人譜面のみのリスト;
						break;
				}

				//this.tSenotes_Core( list音符のみのリスト );
				this.tSenotes_Core_V2(list音符のみのリスト, true);
			}

		}

		/// <summary>
		/// コア部分Ver2。TJAP2から移植しただけ。
		/// </summary>
		/// <param name="list音符のみのリスト"></param>
		private void tSenotes_Core_V2(List<CChip> list音符のみのリスト, bool ignoreSENote = false)
		{
			const int DATA = 3;
			int doco_count = 0;
			int[] sort = new int[7];
			double[] time = new double[7];
			double[] scroll = new double[7];
			double time_tmp;

			for (int i = 0; i < list音符のみのリスト.Count; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					if (i + (j - 3) < 0)
					{
						sort[j] = -1;
						time[j] = -1000000000;
						scroll[j] = 1.0;
					}
					else if (i + (j - 3) >= list音符のみのリスト.Count)
					{
						sort[j] = -1;
						time[j] = 1000000000;
						scroll[j] = 1.0;
					}
					else
					{
						sort[j] = list音符のみのリスト[i + (j - 3)].nチャンネル番号;
						time[j] = list音符のみのリスト[i + (j - 3)].fBMSCROLLTime;
						scroll[j] = list音符のみのリスト[i + (j - 3)].dbSCROLL;
					}
				}
				time_tmp = time[DATA];
				for (int j = 0; j < 7; j++)
				{
					time[j] = (time[j] - time_tmp) * scroll[j];
					if (time[j] < 0) time[j] *= -1;
				}

				if (ignoreSENote && list音符のみのリスト[i].IsFixedSENote) continue;

				switch (list音符のみのリスト[i].nチャンネル番号)
				{
					case 0x11:

						//（左2より離れている｜）_右2_右ドン_右右4_右右ドン…
						if ((time[DATA - 1] > 2/* || (sort[DATA-1] != 1 && time[DATA-1] >= 2 && time[DATA-2] >= 4 && time[DATA-3] <= 5)*/) && time[DATA + 1] == 2 && sort[DATA + 1] == 1 && time[DATA + 2] == 4 && sort[DATA + 2] == 0x11 && time[DATA + 3] == 6 && sort[DATA + 3] == 0x11)
						{
							list音符のみのリスト[i].nSenote = 1;
							doco_count = 1;
							break;
						}
						//ドコドコ中_左2_右2_右ドン
						else if (doco_count != 0 && time[DATA - 1] == 2 && time[DATA + 1] == 2 && (sort[DATA + 1] == 0x11 || sort[DATA + 1] == 0x11))
						{
							if (doco_count % 2 == 0)
								list音符のみのリスト[i].nSenote = 1;
							else
								list音符のみのリスト[i].nSenote = 2;
							doco_count++;
							break;
						}
						else
						{
							doco_count = 0;
						}

						//8分ドコドン
						if ((time[DATA - 2] >= 4.1 && time[DATA - 1] == 2 && time[DATA + 1] == 2 && time[DATA + 2] >= 4.1) && (sort[DATA - 1] == 0x11 && sort[DATA + 1] == 0x11))
						{
							if (list音符のみのリスト[i].dbBPM >= 120.0)
							{
								list音符のみのリスト[i - 1].nSenote = 1;
								list音符のみのリスト[i].nSenote = 2;
								list音符のみのリスト[i + 1].nSenote = 0;
								break;
							}
							else if (list音符のみのリスト[i].dbBPM < 120.0)
							{
								list音符のみのリスト[i - 1].nSenote = 0;
								list音符のみのリスト[i].nSenote = 0;
								list音符のみのリスト[i + 1].nSenote = 0;
								break;
							}
						}

						//BPM120以下のみ
						//8分間隔の「ドドド」→「ドンドンドン」

						if (time[DATA - 1] >= 2 && time[DATA + 1] >= 2)
						{
							if (list音符のみのリスト[i].dbBPM < 120.0)
							{
								list音符のみのリスト[i].nSenote = 0;
								break;
							}
						}

						//ドコドコドン
						if (time[DATA - 3] >= 3.4 && time[DATA - 2] == 2 && time[DATA - 1] == 1 && time[DATA + 1] == 1 && time[DATA + 2] == 2 && time[DATA + 3] >= 3.4 && sort[DATA - 2] == 0x93 && sort[DATA - 1] == 0x11 && sort[DATA + 1] == 0x11 && sort[DATA + 2] == 0x11)
						{
							list音符のみのリスト[i - 2].nSenote = 1;
							list音符のみのリスト[i - 1].nSenote = 2;
							list音符のみのリスト[i + 0].nSenote = 1;
							list音符のみのリスト[i + 1].nSenote = 2;
							list音符のみのリスト[i + 2].nSenote = 0;
							i += 2;
							//break;
						}
						//ドコドン
						else if (time[DATA - 2] >= 2.4 && time[DATA - 1] == 1 && time[DATA + 1] == 1 && time[DATA + 2] >= 2.4 && sort[DATA - 1] == 0x11 && sort[DATA + 1] == 0x11)
						{
							list音符のみのリスト[i].nSenote = 2;
						}
						//右の音符が2以上離れている
						else if (time[DATA + 1] > 2)
						{
							list音符のみのリスト[i].nSenote = 0;
						}
						//右の音符が1.4以上_左の音符が1.4以内
						else if (time[DATA + 1] >= 1.4 && time[DATA - 1] <= 1.4)
						{
							list音符のみのリスト[i].nSenote = 0;
						}
						//右の音符が2以上_右右の音符が3以内
						else if (time[DATA + 1] >= 2 && time[DATA + 2] <= 3)
						{
							list音符のみのリスト[i].nSenote = 0;
						}
						//右の音符が2以上_大音符
						else if (time[DATA + 1] >= 2 && (sort[DATA + 1] == 0x13 || sort[DATA + 1] == 0x14))
						{
							list音符のみのリスト[i].nSenote = 0;
						}
						else
						{
							list音符のみのリスト[i].nSenote = 1;
						}
						break;
					case 0x12:
						doco_count = 0;

						//BPM120以下のみ
						//8分間隔の「ドドド」→「ドンドンドン」
						if (time[DATA - 1] == 2 && time[DATA + 1] == 2)
						{
							if (list音符のみのリスト[i - 1].dbBPM < 120.0 && list音符のみのリスト[i].dbBPM < 120.0 && list音符のみのリスト[i + 1].dbBPM < 120.0)
							{
								list音符のみのリスト[i].nSenote = 3;
								break;
							}
						}

						//右の音符が2以上離れている
						if (time[DATA + 1] > 2)
						{
							list音符のみのリスト[i].nSenote = 3;
						}
						//右の音符が1.4以上_左の音符が1.4以内
						else if (time[DATA + 1] >= 1.4 && time[DATA - 1] <= 1.4)
						{
							list音符のみのリスト[i].nSenote = 3;
						}
						//右の音符が2以上_右右の音符が3以内
						else if (time[DATA + 1] >= 2 && time[DATA + 2] <= 3)
						{
							list音符のみのリスト[i].nSenote = 3;
						}
						//右の音符が2以上_大音符
						else if (time[DATA + 1] >= 2 && (sort[DATA + 1] == 0x13 || sort[DATA + 1] == 0x14))
						{
							list音符のみのリスト[i].nSenote = 3;
						}
						else
						{
							list音符のみのリスト[i].nSenote = 4;
						}
						break;
					default:
						doco_count = 0;
						break;
				}
			}
		}

		/// <summary>
		/// サウンドミキサーにサウンドを登録_削除する時刻を事前に算出する
		/// </summary>
		public void PlanToAddMixerChannel()
		{
			if (TJAPlayer3.Sound管理.GetCurrentSoundDeviceType() == "OpenAL") // DShowでの再生の場合はミキシング負荷が高くないため、
			{                                                                       // チップのライフタイム管理を行わない
				return;
			}

			List<CChip> listAddMixerChannel = new List<CChip>(128); ;
			List<CChip> listRemoveMixerChannel = new List<CChip>(128);
			List<CChip> listRemoveTiming = new List<CChip>(128);

			foreach (CChip pChip in listChip)
			{
				switch (pChip.nチャンネル番号)
				{
					// BGM, 演奏チャネル, 不可視サウンド, フィルインサウンド, 空打ち音はミキサー管理の対象
					// BGM:
					case 0x01:
						// Dr演奏チャネル
						//case 0x11:	case 0x12:	case 0x13:	case 0x14:	case 0x15:	case 0x16:	case 0x17:	case 0x18:	case 0x19:	case 0x1A:  case 0x1B:  case 0x1C:
						// Gt演奏チャネル
						//case 0x20:	case 0x21:	case 0x22:	case 0x23:	case 0x24:	case 0x25:	case 0x26:	case 0x27:	case 0x28:
						// Bs演奏チャネル
						//case 0xA0:	case 0xA1:	case 0xA2:	case 0xA3:	case 0xA4:	case 0xA5:	case 0xA6:	case 0xA7:	case 0xA8:
						// Dr不可視チップ
						//case 0x31:	case 0x32:	case 0x33:	case 0x34:	case 0x35:	case 0x36:	case 0x37:
						//case 0x38:	case 0x39:	case 0x3A:
						// Dr/Gt/Bs空打ち
						//case 0xB1:	case 0xB2:	case 0xB3:	case 0xB4:	case 0xB5:	case 0xB6:	case 0xB7:	case 0xB8:
						//case 0xB9:	case 0xBA:	case 0xBB:	case 0xBC:
						// フィルインサウンド
						//case 0x1F:	case 0x2F:	case 0xAF:
						// 自動演奏チップ
						//case 0x61:	case 0x62:	case 0x63:	case 0x64:	case 0x65:	case 0x66:	case 0x67:	case 0x68:	case 0x69:
						//case 0x70:	case 0x71:	case 0x72:	case 0x73:	case 0x74:	case 0x75:	case 0x76:	case 0x77:	case 0x78:	case 0x79:
						//case 0x80:	case 0x81:	case 0x82:	case 0x83:	case 0x84:	case 0x85:	case 0x86:	case 0x87:	case 0x88:	case 0x89:
						//case 0x90:	case 0x91:	case 0x92:

						#region [ 発音1秒前のタイミングを算出 ]
						int n発音前余裕ms = 1000, n発音後余裕ms = 800;
						{
							int ch = pChip.nチャンネル番号 >> 4;
							if (ch == 0x02 || ch == 0x0A)
							{
								n発音前余裕ms = 800;
								n発音前余裕ms = 500;
							}
							if (ch == 0x06 || ch == 0x07 || ch == 0x08 || ch == 0x09)
							{
								n発音前余裕ms = 200;
								n発音前余裕ms = 500;
							}
						}
						#endregion
						#region [ BGMチップならば即ミキサーに追加 ]
						//if ( pChip.nチャンネル番号 == 0x01 )	// BGMチップは即ミキサーに追加
						//{
						//    if ( listWAV.ContainsKey( pChip.n整数値_内部番号 ) )
						//    {
						//        CDTX.CWAV wc = CDTXMania.DTX.listWAV[ pChip.n整数値_内部番号 ];
						//        if ( wc.rSound[ 0 ] != null )
						//        {
						//            CDTXMania.Sound管理.AddMixer( wc.rSound[ 0 ] );	// BGMは多重再生しない仕様としているので、1個目だけミキサーに登録すればよい
						//        }
						//    }
						//}
						#endregion
						#region [ 発音1秒前のタイミングを算出 ]
						int nAddMixer時刻ms, nAddMixer位置 = 0;
						//Debug.WriteLine("==================================================================");
						//Debug.WriteLine( "Start: ch=" + pChip.nチャンネル番号.ToString("x2") + ", nWAV番号=" + pChip.n整数値 + ", time=" + pChip.n発声時刻ms + ", lasttime=" + listChip[ listChip.Count - 1 ].n発声時刻ms );
						t発声時刻msと発声位置を取得する(pChip.n発声時刻ms - n発音前余裕ms, out nAddMixer時刻ms, out nAddMixer位置);
						//Debug.WriteLine( "nAddMixer時刻ms=" + nAddMixer時刻ms + ",nAddMixer位置=" + nAddMixer位置 );

						CChip c_AddMixer = new CChip()
						{
							nチャンネル番号 = 0xDA,
							n整数値 = pChip.n整数値,
							n整数値_内部番号 = pChip.n整数値_内部番号,
							n発声時刻ms = nAddMixer時刻ms,
							n発声位置 = nAddMixer位置,
							b演奏終了後も再生が続くチップである = false
						};
						listAddMixerChannel.Add(c_AddMixer);
						//Debug.WriteLine("listAddMixerChannel:" );
						//DebugOut_CChipList( listAddMixerChannel );
						#endregion

						int duration = 0;
						if (listWAV.TryGetValue(pChip.n整数値_内部番号, out CDTX.CWAV wc))
						{
							duration = (wc.rSound[0] == null) ? 0 : (int)(wc.rSound[0].n総演奏時間ms / this.db再生速度); // #23664 durationに再生速度が加味されておらず、低速再生でBGMが途切れる問題を修正 (発声時刻msは、DTX読み込み時に再生速度加味済)
						}
						//Debug.WriteLine("duration=" + duration );
						int n新RemoveMixer時刻ms, n新RemoveMixer位置;
						t発声時刻msと発声位置を取得する(pChip.n発声時刻ms + duration + n発音後余裕ms, out n新RemoveMixer時刻ms, out n新RemoveMixer位置);
						//Debug.WriteLine( "n新RemoveMixer時刻ms=" + n新RemoveMixer時刻ms + ",n新RemoveMixer位置=" + n新RemoveMixer位置 );
						if (n新RemoveMixer時刻ms < pChip.n発声時刻ms + duration)   // 曲の最後でサウンドが切れるような場合は
						{
							CChip c_AddMixer_noremove = c_AddMixer;
							c_AddMixer_noremove.b演奏終了後も再生が続くチップである = true;
							listAddMixerChannel[listAddMixerChannel.Count - 1] = c_AddMixer_noremove;
							//continue;												// 発声位置の計算ができないので、Mixer削除をあきらめる___のではなく
							// #32248 2013.10.15 yyagi 演奏終了後も再生を続けるチップであるというフラグをpChip内に立てる
							break;
						}
						#region [ 未使用コード ]
						//if ( n新RemoveMixer時刻ms < pChip.n発声時刻ms + duration )	// 曲の最後でサウンドが切れるような場合
						//{
						//    n新RemoveMixer時刻ms = pChip.n発声時刻ms + duration;
						//    // 「位置」は比例計算で求めてお茶を濁す...このやり方だと誤動作したため対応中止
						//    n新RemoveMixer位置 = listChip[ listChip.Count - 1 ].n発声位置 * n新RemoveMixer時刻ms / listChip[ listChip.Count - 1 ].n発声時刻ms;
						//}
						#endregion

						#region [ 発音終了2秒後にmixerから削除するが、その前に再発音することになるのかを確認(再発音ならmixer削除タイミングを延期) ]
						int n整数値 = pChip.n整数値;
						int index = listRemoveTiming.FindIndex(
							delegate (CChip cchip) { return cchip.n整数値 == n整数値; }
						);
						//Debug.WriteLine( "index=" + index );
						if (index >= 0)                                                 // 過去に同じチップで発音中のものが見つかった場合
						{                                                                   // 過去の発音のmixer削除を確定させるか、延期するかの2択。
							int n旧RemoveMixer時刻ms = listRemoveTiming[index].n発声時刻ms;
							int n旧RemoveMixer位置 = listRemoveTiming[index].n発声位置;

							//Debug.WriteLine( "n旧RemoveMixer時刻ms=" + n旧RemoveMixer時刻ms + ",n旧RemoveMixer位置=" + n旧RemoveMixer位置 );
							if (pChip.n発声時刻ms - n発音前余裕ms <= n旧RemoveMixer時刻ms)  // mixer削除前に、同じ音の再発音がある場合は、
							{                                                                   // mixer削除時刻を遅延させる(if-else後に行う)
																								//Debug.WriteLine( "remove TAIL of listAddMixerChannel. TAIL INDEX=" + listAddMixerChannel.Count );
																								//DebugOut_CChipList( listAddMixerChannel );
								listAddMixerChannel.RemoveAt(listAddMixerChannel.Count - 1);    // また、同じチップ音の「mixerへの再追加」は削除する
																								//Debug.WriteLine( "removed result:" );
																								//DebugOut_CChipList( listAddMixerChannel );
							}
							else                                                            // 逆に、時間軸上、mixer削除後に再発音するような流れの場合は
							{
								//Debug.WriteLine( "Publish the value(listRemoveTiming[index] to listRemoveMixerChannel." );
								listRemoveMixerChannel.Add(listRemoveTiming[index]);    // mixer削除を確定させる
																						//Debug.WriteLine( "listRemoveMixerChannel:" );
																						//DebugOut_CChipList( listRemoveMixerChannel );
																						//listRemoveTiming.RemoveAt( index );
							}
							CChip c = new CChip()                                           // mixer削除時刻を更新(遅延)する
							{
								nチャンネル番号 = 0xDB,
								n整数値 = listRemoveTiming[index].n整数値,
								n整数値_内部番号 = listRemoveTiming[index].n整数値_内部番号,
								n発声時刻ms = n新RemoveMixer時刻ms,
								n発声位置 = n新RemoveMixer位置
							};
							listRemoveTiming[index] = c;
							//listRemoveTiming[ index ].n発声時刻ms = n新RemoveMixer時刻ms;	// mixer削除時刻を更新(遅延)する
							//listRemoveTiming[ index ].n発声位置 = n新RemoveMixer位置;
							//Debug.WriteLine( "listRemoveTiming: modified" );
							//DebugOut_CChipList( listRemoveTiming );
						}
						else                                                                // 過去に同じチップを発音していないor
						{                                                                   // 発音していたが既にmixer削除確定していたなら
							CChip c = new CChip()                                           // 新しくmixer削除候補として追加する
							{
								nチャンネル番号 = 0xDB,
								n整数値 = pChip.n整数値,
								n整数値_内部番号 = pChip.n整数値_内部番号,
								n発声時刻ms = n新RemoveMixer時刻ms,
								n発声位置 = n新RemoveMixer位置
							};
							//Debug.WriteLine( "Add new chip to listRemoveMixerTiming: " );
							//Debug.WriteLine( "ch=" + c.nチャンネル番号.ToString( "x2" ) + ", nWAV番号=" + c.n整数値 + ", time=" + c.n発声時刻ms + ", lasttime=" + listChip[ listChip.Count - 1 ].n発声時刻ms );
							listRemoveTiming.Add(c);
							//Debug.WriteLine( "listRemoveTiming:" );
							//DebugOut_CChipList( listRemoveTiming );
						}
						#endregion
						break;
				}
			}
			//Debug.WriteLine("==================================================================");
			//Debug.WriteLine( "Result:" );
			//Debug.WriteLine( "listAddMixerChannel:" );
			//DebugOut_CChipList( listAddMixerChannel );
			//Debug.WriteLine( "listRemoveMixerChannel:" );
			//DebugOut_CChipList( listRemoveMixerChannel );
			//Debug.WriteLine( "listRemoveTiming:" );
			//DebugOut_CChipList( listRemoveTiming );
			//Debug.WriteLine( "==================================================================" );

			listChip.AddRange(listAddMixerChannel);
			listChip.AddRange(listRemoveMixerChannel);
			listChip.AddRange(listRemoveTiming);
			listChip.Sort();
		}
		private void DebugOut_CChipList(List<CChip> c)
		{
			//Debug.WriteLine( "Count=" + c.Count );
			for (int i = 0; i < c.Count; i++)
			{
				Debug.WriteLine(i + ": ch=" + c[i].nチャンネル番号.ToString("x2") + ", WAV番号=" + c[i].n整数値 + ", time=" + c[i].n発声時刻ms);
			}
		}
		private bool t発声時刻msと発声位置を取得する(int n希望発声時刻ms, out int n新発声時刻ms, out int n新発声位置)
		{
			// 発声時刻msから発声位置を逆算することはできないため、近似計算する。
			// 具体的には、希望発声位置前後の2つのチップの発声位置の中間を取る。

			if (n希望発声時刻ms < 0)
			{
				n希望発声時刻ms = 0;
			}
			//else if ( n希望発声時刻ms > listChip[ listChip.Count - 1 ].n発声時刻ms )		// BGMの最後の余韻を殺してしまうので、この条件は外す
			//{
			//    n希望発声時刻ms = listChip[ listChip.Count - 1 ].n発声時刻ms;
			//}

			int index_min = -1, index_max = -1;
			for (int i = 0; i < listChip.Count; i++)        // 希望発声位置前後の「前」の方のチップを検索
			{
				if (listChip[i].n発声時刻ms >= n希望発声時刻ms)
				{
					index_min = i;
					break;
				}
			}
			if (index_min < 0)  // 希望発声時刻に至らずに曲が終了してしまう場合
			{
				// listの最終項目の時刻をそのまま使用する
				//___のではダメ。BGMが尻切れになる。
				// そこで、listの最終項目の発声時刻msと発生位置から、希望発声時刻に相当する希望発声位置を比例計算して求める。
				//n新発声時刻ms = n希望発声時刻ms;
				//n新発声位置 = listChip[ listChip.Count - 1 ].n発声位置 * n希望発声時刻ms / listChip[ listChip.Count - 1 ].n発声時刻ms;
				n新発声時刻ms = listChip[listChip.Count - 1].n発声時刻ms;
				n新発声位置 = listChip[listChip.Count - 1].n発声位置;
				return false;
			}
			index_max = index_min + 1;
			if (index_max >= listChip.Count)
			{
				index_max = index_min;
			}
			n新発声時刻ms = (listChip[index_max].n発声時刻ms + listChip[index_min].n発声時刻ms) / 2;
			n新発声位置 = (listChip[index_max].n発声位置 + listChip[index_min].n発声位置) / 2;

			return true;
		}

		// CActivity 実装

		private CPrivateFastFont pf歌詞フォント;
		public override void On活性化()
		{
			if (!string.IsNullOrEmpty(TJAPlayer3.Skin.Game_Lyric_FontName))
			{
				this.pf歌詞フォント = new CPrivateFastFont(new FontFamily(TJAPlayer3.Skin.Game_Lyric_FontName), TJAPlayer3.Skin.Game_Lyric_FontSize);
			}
			else
			{
				this.pf歌詞フォント = new CPrivateFastFont(new FontFamily("MS UI Gothic"), TJAPlayer3.Skin.Game_Lyric_FontSize);
			}
			this.listWAV = new Dictionary<int, CWAV>();
			this.listBPM = new Dictionary<int, CBPM>();
			this.listSCROLL = new Dictionary<int, CSCROLL>();
			this.listSCROLL_Normal = new Dictionary<int, CSCROLL>();
			this.listSCROLL_Expert = new Dictionary<int, CSCROLL>();
			this.listSCROLL_Master = new Dictionary<int, CSCROLL>();
			this.listJPOSSCROLL = new Dictionary<int, CJPOSSCROLL>();
			this.listDELAY = new Dictionary<int, CDELAY>();
			this.listBRANCH = new Dictionary<int, CBRANCH>();
			this.listVD = new Dictionary<int, CVideoDecoder>();
			this.listChip = new List<CChip>();
			this.listBalloon_Normal = new List<int>();
			this.listBalloon_Expert = new List<int>();
			this.listBalloon_Master = new List<int>();
			this.listLine = new List<CLine>();
			this.listLyric = new List<Bitmap>();
			this.listLyric2 = new List<STLYRIC>();
			this.List_DanSongs = new List<DanSongs>();
			base.On活性化();
		}
		public override void On非活性化()
		{
			if (this.listWAV != null)
			{
				foreach (CWAV cwav in this.listWAV.Values)
				{
					cwav.Dispose();
				}
				this.listWAV = null;
			}
			if (this.listVD != null)
			{
				foreach (CVideoDecoder cvd in this.listVD.Values)
				{
					cvd.Dispose();
				}
				this.listVD = null;
			}
			if (this.listBPM != null)
			{
				this.listBPM.Clear();
				this.listBPM = null;
			}
			if (this.listDELAY != null)
			{
				this.listDELAY.Clear();
				this.listDELAY = null;
			}
			if (this.listBRANCH != null)
			{
				this.listBRANCH.Clear();
				this.listBRANCH = null;
			}
			if (this.listSCROLL != null)
			{
				this.listSCROLL.Clear();
				this.listSCROLL = null;
			}

			if (this.listSCROLL_Normal != null)
			{
				this.listSCROLL_Normal.Clear();
				this.listSCROLL_Normal = null;
			}
			if (this.listSCROLL_Expert != null)
			{
				this.listSCROLL_Expert.Clear();
				this.listSCROLL_Expert = null;
			}
			if (this.listSCROLL_Master != null)
			{
				this.listSCROLL_Master.Clear();
				this.listSCROLL_Master = null;
			}
			if (this.listJPOSSCROLL != null)
			{
				this.listJPOSSCROLL.Clear();
				this.listJPOSSCROLL = null;
			}
			if (this.List_DanSongs != null)
			{
				this.List_DanSongs.Clear();
				this.List_DanSongs = null;
			}

			if (this.listChip != null)
			{
				this.listChip.Clear();
			}

			if (this.listBalloon_Normal != null)
			{
				this.listBalloon_Normal.Clear();
			}
			if (this.listBalloon_Expert != null)
			{
				this.listBalloon_Expert.Clear();
			}
			if (this.listBalloon_Master != null)
			{
				this.listBalloon_Master.Clear();
			}
			if (this.listLyric != null)
			{
				this.listLyric.Clear();
			}
			if (this.listLyric2 != null)
			{
				this.listLyric2.Clear();
			}

			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				this.tAVIの読み込み();
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				TJAPlayer3.t安全にDisposeする(ref this.pf歌詞フォント);
				if (this.listVD != null)
				{
					foreach (CVideoDecoder cvd in this.listVD.Values)
					{
						cvd.Dispose();
					}
					this.listVD = null;
				}
				base.OnManagedリソースの解放();
			}
		}

		// その他

		#region [ private ]
		//-----------------

		private bool bヘッダのみ;

		private int nPolyphonicSounds = 4;                          // #28228 2012.5.1 yyagi

		private int n内部番号BPM1to;
		private int n内部番号SCROLL1to;
		private int n内部番号JSCROLL1to;
		private int n内部番号DELAY1to;
		private int n内部番号WAV1to;
		private int[] n無限管理SIZE;

		[JsonObject("infomation")]
		private class OTCInfomation
		{
			[JsonProperty("title")]
			public string Title { get; set; }
			[JsonProperty("subtitle")]
			public string SubTitle { get; set; }
			[JsonProperty("albumart")]
			public string Albumart { get; set; }
			[JsonProperty("artist")]
			public string[] Artist { get; set; }
			[JsonProperty("creator")]
			public string[] Creator { get; set; }
			[JsonProperty("audio")]
			public string WAVFile { get; set; }
			[JsonProperty("background")]
			public string BGFile { get; set; }
			[JsonProperty("movieoffset")]
			public double? MVOffset { get; set; }
			[JsonProperty("bpm")]
			public double? BPM { get; set; }
			[JsonProperty("offset")]
			public double? Offset { get; set; }
			[JsonProperty("courses")]
			public OTCInfomationHumen[] Courses { get; set; }
		}

		[JsonObject("infohumen")]
		private class OTCInfomationHumen
		{
			[JsonProperty("difficulty")]
			public string Diffculty { get; set; }
			[JsonProperty("level")]
			public int? Level { get; set; }
			[JsonProperty("single")]
			public string Single { get; set; }
			[JsonProperty("multiple")]
			public string[] Multiple { get; set; }
		}

		[JsonObject("cource")]
		private class OTCCource
		{
			[JsonProperty("scoreinit")]
			public int? ScoreInit { get; set; }
			[JsonProperty("scorediff")]
			public int? ScoreDiff { get; set; }
			[JsonProperty("scoreshinuchi")]
			public int? ScoreShinuchi { get; set; }
			[JsonProperty("balloon")]
			public int?[] Balloon { get; set; }
			[JsonProperty("measures")]
			public string[][] Measures { get; set; }
		}

		[JsonObject("medley")]
		private class OTCMedley
		{
			[JsonProperty("title")]
			public string Title { get; set; }
			[JsonProperty("exams")]
			public OTCMedleyJouken[] Jouken { get; set; }
			[JsonProperty("charts")]
			public OTCMedleyHumen[] Humen { get; set; }
		}

		[JsonObject("medleyjouken")]
		private class OTCMedleyJouken
		{
			[JsonProperty("type")]
			public string Type { get; set; }
			[JsonProperty("range")]
			public string Range { get; set; }
			[JsonProperty("value")]
			public int?[] Value { get; set; }
		}

		[JsonObject("medleyHumen")]
		private class OTCMedleyHumen
		{
			[JsonProperty("file")]
			public string File { get; set; }
			[JsonProperty("difficulty")]
			public string Diff { get; set; }
		}

		/// <summary>
		/// 音源再生前の空白を追加するメソッド。
		/// </summary>
		private void AddMusicPreTimeMs()
		{
			this.dbNowTime += TJAPlayer3.ConfigIni.MusicPreTimeMs;
			this.dbNowBMScollTime += TJAPlayer3.ConfigIni.MusicPreTimeMs * this.dbNowBPM / 15000;
		}
		//-----------------
		#endregion
	}
}
