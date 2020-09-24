using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using FDK;

namespace TJAPlayer3
{
	[Serializable]
	internal class Cスコア
	{
		// プロパティ

		public STScoreIni情報 ScoreIni情報;
		[Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct STScoreIni情報
		{
			public DateTime 最終更新日時;
			public long ファイルサイズ;

			public STScoreIni情報( DateTime 最終更新日時, long ファイルサイズ )
			{
				this.最終更新日時 = 最終更新日時;
				this.ファイルサイズ = ファイルサイズ;
			}
		}

		public STファイル情報 ファイル情報;
		[Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct STファイル情報
		{
			public string ファイルの絶対パス;
			public string フォルダの絶対パス;
			public DateTime 最終更新日時;
			public long ファイルサイズ;

			public STファイル情報( string ファイルの絶対パス, string フォルダの絶対パス, DateTime 最終更新日時, long ファイルサイズ )
			{
				this.ファイルの絶対パス = ファイルの絶対パス;
				this.フォルダの絶対パス = フォルダの絶対パス;
				this.最終更新日時 = 最終更新日時;
				this.ファイルサイズ = ファイルサイズ;
			}
		}

		public ST譜面情報 譜面情報;
		[Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ST譜面情報
		{
			public string タイトル;
			public string アーティスト名;
			public string コメント;
			public string ジャンル;
			public string Backgound;
			public int 最大ランク;
			public double 最大スキル;
			public bool フルコンボ;
			public int 演奏回数;
			public STHISTORY 演奏履歴;
			public double Bpm;
			public int Duration;
			public string strBGMファイル名;
			public int SongVol;
			public LoudnessMetadata? SongLoudnessMetadata;
			public bool b歌詞あり;
			public int nデモBGMオフセット;
			public bool[] b譜面が存在する;
			public bool[] b譜面分岐;
			public bool[] bPapaMamaSupport;
			public int[] nハイスコア;
			public int[] nSecondScore;
			public int[] nThirdScore;
			public string[] strHiScorerName;
			public string[] strSecondScorerName;
			public string[] strThirdScorerName;
			public int[] n王冠;
			public string strサブタイトル;
			public int[] nレベル;

			[Serializable]
			[StructLayout( LayoutKind.Sequential )]
			public struct STHISTORY
			{
				public string 行1;
				public string 行2;
				public string 行3;
				public string 行4;
				public string 行5;
				public string 行6;
				public string 行7;
				public string this[ int index ]
				{
					get
					{
						switch( index )
						{
							case 0:
								return this.行1;

							case 1:
								return this.行2;

							case 2:
								return this.行3;

							case 3:
								return this.行4;

							case 4:
								return this.行5;
							case 5:
								return this.行6;
							case 6:
								return this.行7;
						}
						throw new IndexOutOfRangeException();
					}
					set
					{
						switch( index )
						{
							case 0:
								this.行1 = value;
								return;

							case 1:
								this.行2 = value;
								return;

							case 2:
								this.行3 = value;
								return;

							case 3:
								this.行4 = value;
								return;

							case 4:
								this.行5 = value;
								return;
							case 5:
								this.行6 = value;
								return;
							case 6:
								this.行7 = value;
								return;
						}
						throw new IndexOutOfRangeException();
					}
				}
			}
		}

		public bool bSongDBにキャッシュがあった;


		// コンストラクタ

		public Cスコア()
		{
			this.ScoreIni情報 = new STScoreIni情報( DateTime.MinValue, 0L );
			this.bSongDBにキャッシュがあった = false;
			this.ファイル情報 = new STファイル情報( "", "", DateTime.MinValue, 0L );
			this.譜面情報 = new ST譜面情報();
			this.譜面情報.タイトル = "";
			this.譜面情報.アーティスト名 = "";
			this.譜面情報.コメント = "";
			this.譜面情報.ジャンル = "";
			this.譜面情報.Backgound = "";
			this.譜面情報.最大ランク =  (int)CScoreIni.ERANK.UNKNOWN;
			this.譜面情報.フルコンボ = false;
			this.譜面情報.演奏回数 = new int();
			this.譜面情報.演奏履歴 = new ST譜面情報.STHISTORY();
			this.譜面情報.演奏履歴.行1 = "";
			this.譜面情報.演奏履歴.行2 = "";
			this.譜面情報.演奏履歴.行3 = "";
			this.譜面情報.演奏履歴.行4 = "";
			this.譜面情報.演奏履歴.行5 = "";
			this.譜面情報.演奏履歴.行6 = "";
			this.譜面情報.演奏履歴.行7 = "";
			this.譜面情報.最大スキル = new double();
			this.譜面情報.Bpm = 120.0;
			this.譜面情報.Duration = 0;
			this.譜面情報.strBGMファイル名 = "";
			this.譜面情報.SongVol = CSound.DefaultSongVol;
			this.譜面情報.SongLoudnessMetadata = null;
			this.譜面情報.nデモBGMオフセット = 0;
			this.譜面情報.b譜面が存在する = new bool[(int)Difficulty.Total];
			this.譜面情報.b譜面分岐 = new bool[(int)Difficulty.Total];
			this.譜面情報.bPapaMamaSupport = new bool[(int)Difficulty.Total];
			this.譜面情報.b歌詞あり = false;
			this.譜面情報.nハイスコア = new int[(int)Difficulty.Total];
			this.譜面情報.nSecondScore = new int[(int)Difficulty.Total];
			this.譜面情報.nThirdScore = new int[(int)Difficulty.Total];
			this.譜面情報.strHiScorerName = new string[(int)Difficulty.Total];
			this.譜面情報.strSecondScorerName = new string[(int)Difficulty.Total];
			this.譜面情報.strThirdScorerName = new string[(int)Difficulty.Total];
			this.譜面情報.n王冠 = new int[(int)Difficulty.Total];
			this.譜面情報.strサブタイトル = "";
			this.譜面情報.nレベル = new int[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1};
		}
	}
}
