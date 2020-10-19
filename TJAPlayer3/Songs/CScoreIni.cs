using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using FDK;

namespace TJAPlayer3
{
	public class CScoreIni
	{
		// プロパティ

		// [File] セクション
		public STファイル stファイル;
		[StructLayout( LayoutKind.Sequential )]
		public struct STファイル
		{
			public string Title;
			public string Name;
			public string Hash;
			public int PlayCountDrums;
			// #23596 10.11.16 add ikanick-----/
			public int ClearCountDrums;
			// #24459 2011.2.24 yyagi----------/
			public int BestRank;
			// --------------------------------/
			public int HistoryCount;
			public string[] History;
			public int BGMAdjust;
		}

		// 演奏記録セクション（2種類）
		public STセクション stセクション;
		[StructLayout( LayoutKind.Sequential )]
		public struct STセクション
		{
			public CScoreIni.C演奏記録 HiScore;
			public CScoreIni.C演奏記録 LastPlay;   // #23595 2011.1.9 ikanick
			public CScoreIni.C演奏記録 this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.HiScore;

						case 1:
							return this.LastPlay;
						//------------
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.HiScore = value;
							return;

						case 1:
							this.LastPlay = value;
							return;
						//------------------
					}
					throw new IndexOutOfRangeException();
				}
			}
		}
		public enum Eセクション種別 : int
		{
			Unknown = -2,
			File = -1,
			HiScore = 0,
			LastPlayDrums = 1,
		}
		public enum ERANK : int		// #24459 yyagi
		{
			SS = 0,
			S = 1,
			A = 2,
			B = 3,
			C = 4,
			D = 5,
			E = 6,
			UNKNOWN = 99
		}
		public class C演奏記録
		{
			public bool bDrums有効;
			public bool bSTAGEFAILED有効;
			public bool bTight;
			public bool b演奏にMIDI入力を使用した;
			public bool b演奏にキーボードを使用した;
			public bool b演奏にジョイパッドを使用した;
			public bool b演奏にマウスを使用した;
			public double dbゲーム型スキル値;
			public double db演奏型スキル値;
			public ERandomMode eRandom;
			public float f譜面スクロール速度;
			public string Hash;
			public int nGoodになる範囲ms;
			public int nGood数;
			public int nGreatになる範囲ms;
			public int nGreat数;
			public int nMiss数;
			public int nPerfectになる範囲ms;
			public int nPerfect数;
			public int nPoorになる範囲ms;
			public int nPoor数;
			public int nPerfect数_Auto含まない;
			public int nGreat数_Auto含まない;
			public int nGood数_Auto含まない;
			public int nPoor数_Auto含まない;
			public int nMiss数_Auto含まない;
			public long nスコア;
			public int n連打数;
			public int n演奏速度分子;
			public int n演奏速度分母;
			public int n最大コンボ数;
			public int n全チップ数;
			public string strDTXManiaのバージョン;
			public bool レーン9モード;
			public int nRisky;		// #23559 2011.6.20 yyagi 0=OFF, 1-10=Risky
			public string 最終更新日時;
			public float fゲージ;
			public bool b途中でAutoを切り替えたか;
			public int[] n良 = new int[(int)Difficulty.Total];
			public int[] n可 = new int[(int)Difficulty.Total];
			public int[] n不可 = new int[(int)Difficulty.Total];
			public int[] nハイスコア = new int[(int)Difficulty.Total];
			public int[] nSecondScore = new int[(int)Difficulty.Total];
			public int[] nThirdScore = new int[(int)Difficulty.Total];
			public string[] strHiScorerName = new string[(int)Difficulty.Total];
			public string[] strSecondScorerName = new string[(int)Difficulty.Total];
			public string[] strThirdScorerName = new string[(int)Difficulty.Total];
			public int[] n王冠 = new int[(int)Difficulty.Total];
			public Dan_C[] Dan_C;
			public Dan_C Dan_C_Gauge;

			public C演奏記録()
			{
				this.eRandom = ERandomMode.OFF;
				this.f譜面スクロール速度 = new float();
				this.f譜面スクロール速度 = 1f;
				this.n演奏速度分子 = 20;
				this.n演奏速度分母 = 20;
				this.bDrums有効 = true;
				this.bSTAGEFAILED有効 = true;
				this.nPerfectになる範囲ms = 34;
				this.nGreatになる範囲ms = 67;
				this.nGoodになる範囲ms = 84;
				this.nPoorになる範囲ms = 117;
				this.strDTXManiaのバージョン = "Unknown";
				this.最終更新日時 = "";
				this.Hash = "00000000000000000000000000000000";
				this.レーン9モード = true;
				this.nRisky = 0;									// #23559 2011.6.20 yyagi
				this.fゲージ = 0.0f;
				this.b途中でAutoを切り替えたか = false;
				Dan_C = new Dan_C[3];
				Dan_C_Gauge = new Dan_C();
			}

			public bool bフルコンボじゃない
			{
				get
				{
					return !this.bフルコンボである;
				}
			}
			public bool bフルコンボである
			{
				get
				{
					return ( ( this.n最大コンボ数 > 0 ) && ( this.n最大コンボ数 == ( this.nPerfect数 + this.nGreat数 + this.nGood数 + this.nPoor数 + this.nMiss数 ) ) );
				}
			}

			public bool b全AUTOじゃない
			{
				get
				{
					return !b全AUTOである;
				}
			}
			public bool b全AUTOである
			{
				get
				{
					return (this.n全チップ数 - this.nPerfect数_Auto含まない - this.nGreat数_Auto含まない - this.nGood数_Auto含まない - this.nPoor数_Auto含まない - this.nMiss数_Auto含まない) == this.n全チップ数;
				}
			}
		}

		/// <summary>
		/// <para>.score.ini の存在するフォルダ（絶対パス；末尾に '\' はついていない）。</para>
		/// <para>未保存などでファイル名がない場合は null。</para>
		/// </summary>
		public string iniファイルのあるフォルダ名
		{
			get;
			private set;
		}

		/// <summary>
		/// <para>.score.ini のファイル名（絶対パス）。</para>
		/// <para>未保存などでファイル名がない場合は null。</para>
		/// </summary>
		public string iniファイル名
		{
			get; 
			private set;
		}


		// コンストラクタ

		public CScoreIni()
		{
			this.iniファイルのあるフォルダ名 = null;
			this.iniファイル名 = null;
			this.stファイル = new STファイル();
			stファイル.Title = "";
			stファイル.Name = "";
			stファイル.Hash = "";
			stファイル.History = new string[] { "", "", "", "", "" };
			stファイル.BestRank =  (int)ERANK.UNKNOWN;		// #24459 2011.2.24 yyagi
	
			this.stセクション = new STセクション();
			stセクション.HiScore = new C演奏記録();
			stセクション.LastPlay = new C演奏記録();
		}

		/// <summary>
		/// <para>初期化後にiniファイルを読み込むコンストラクタ。</para>
		/// <para>読み込んだiniに不正値があれば、それが含まれるセクションをリセットする。</para>
		/// </summary>
		public CScoreIni( string str読み込むiniファイル )
			: this()
		{
			this.t読み込み( str読み込むiniファイル );
			this.t全演奏記録セクションの整合性をチェックし不整合があればリセットする();
		}


		// メソッド

		/// <summary>
		/// <para>現在の this.Record[] オブジェクトの、指定されたセクションの情報が正当であるか否かを判定する。
		/// 真偽どちらでも、その内容は書き換えない。</para>
		/// </summary>
		/// <param name="eセクション">判定するセクション。</param>
		/// <returns>正当である（整合性がある）場合は true。</returns>
		public bool b整合性がある( Eセクション種別 eセクション )
		{
			return true;	// オープンソース化に伴い、整合性チェックを無効化。（2010.10.21）
		}
		
		/// <summary>
		/// 指定されたファイルの内容から MD5 値を求め、それを16進数に変換した文字列を返す。
		/// </summary>
		/// <param name="ファイル名">MD5 を求めるファイル名。</param>
		/// <returns>算出結果の MD5 を16進数で並べた文字列。</returns>
		public static string tファイルのMD5を求めて返す( string ファイル名 )
		{
			byte[] buffer = null;
			FileStream stream = new FileStream( ファイル名, FileMode.Open, FileAccess.Read );
			buffer = new byte[ stream.Length ];
			stream.Read( buffer, 0, (int) stream.Length );
			stream.Close();
			StringBuilder builder = new StringBuilder(0x21);
			{
				MD5CryptoServiceProvider m = new MD5CryptoServiceProvider();
				byte[] buffer2 = m.ComputeHash(buffer);
				foreach (byte num in buffer2)
					builder.Append(num.ToString("x2"));
			}
			return builder.ToString();
		}
		
		/// <summary>
		/// 指定された .score.ini を読み込む。内容の真偽は判定しない。
		/// </summary>
		/// <param name="iniファイル名">読み込む .score.ini ファイルを指定します（絶対パスが安全）。</param>
		public void t読み込み( string iniファイル名 )
		{
			this.iniファイルのあるフォルダ名 = Path.GetDirectoryName( iniファイル名 );
			this.iniファイル名 = Path.GetFileName( iniファイル名 );

			Eセクション種別 section = Eセクション種別.Unknown;
			if( File.Exists( iniファイル名 ) )
			{
				string str;
				Encoding inienc = CJudgeTextEncoding.JudgeFileEncoding(iniファイル名);
				using (StreamReader reader = new StreamReader(iniファイル名, inienc))
				{
					while ((str = reader.ReadLine()) != null)
					{
						str = str.Replace('\t', ' ').TrimStart(new char[] { '\t', ' ' });
						if ((str.Length != 0) && (str[0] != ';'))
						{
							try
							{
								string item;
								string para;
								C演奏記録 c演奏記録;
								#region [ section ]
								if (str[0] == '[')
								{
									StringBuilder builder = new StringBuilder(0x20);
									int num = 1;
									while ((num < str.Length) && (str[num] != ']'))
									{
										builder.Append(str[num++]);
									}
									string str2 = builder.ToString();
									if (str2.Equals("File"))
									{
										section = Eセクション種別.File;
									}
									else if (str2.Equals("HiScore.Drums"))
									{
										section = Eセクション種別.HiScore;
									}
									// #23595 2011.1.9 ikanick
									else if (str2.Equals("LastPlay.Drums"))
									{
										section = Eセクション種別.LastPlayDrums;
									}
									//----------------------------------------------------
									else
									{
										section = Eセクション種別.Unknown;
									}
								}
								#endregion
								else
								{
									string[] strArray = str.Split(new char[] { '=' });
									if (strArray.Length == 2)
									{
										item = strArray[0].Trim();
										para = strArray[1].Trim();
										switch (section)
										{
											case Eセクション種別.File:
												{
													if (!item.Equals("Title"))
													{
														goto Label_01C7;
													}
													this.stファイル.Title = para;
													continue;
												}
											case Eセクション種別.HiScore:
											case Eセクション種別.LastPlayDrums:// #23595 2011.1.9 ikanick
												{
													c演奏記録 = this.stセクション[(int)section];
													if (!item.Equals("Score"))
													{
														goto Label_03B9;
													}
													c演奏記録.nスコア = long.Parse(para);


													continue;
												}
										}
									}
								}
								continue;
							#region [ File section ]
							Label_01C7:
								if (item.Equals("Name"))
								{
									this.stファイル.Name = para;
								}
								else if (item.Equals("Hash"))
								{
									this.stファイル.Hash = para;
								}
								else if (item.Equals("PlayCountDrums"))
								{
									this.stファイル.PlayCountDrums = C変換.n値を文字列から取得して範囲内に丸めて返す(para, 0, 99999999, 0);
								}
								// #23596 10.11.16 add ikanick------------------------------------/
								else if (item.Equals("ClearCountDrums"))
								{
									this.stファイル.ClearCountDrums = C変換.n値を文字列から取得して範囲内に丸めて返す(para, 0, 99999999, 0);
								}
								// #24459 2011.2.24 yyagi-----------------------------------------/
								else if (item.Equals("BestRankDrums"))
								{
									this.stファイル.BestRank = C変換.n値を文字列から取得して範囲内に丸めて返す(para, (int)ERANK.SS, (int)ERANK.E, (int)ERANK.UNKNOWN);
								}
								//----------------------------------------------------------------/
								else if (item.Equals("History0"))
								{
									this.stファイル.History[0] = para;
								}
								else if (item.Equals("History1"))
								{
									this.stファイル.History[1] = para;
								}
								else if (item.Equals("History2"))
								{
									this.stファイル.History[2] = para;
								}
								else if (item.Equals("History3"))
								{
									this.stファイル.History[3] = para;
								}
								else if (item.Equals("History4"))
								{
									this.stファイル.History[4] = para;
								}
								else if (item.Equals("HistoryCount"))
								{
									this.stファイル.HistoryCount = C変換.n値を文字列から取得して範囲内に丸めて返す(para, 0, 99999999, 0);
								}
								else if (item.Equals("BGMAdjust"))
								{
									this.stファイル.BGMAdjust = C変換.n値を文字列から取得して返す(para, 0);
								}
								continue;
							#endregion
							#region [ Score section ]
							Label_03B9:
								if (item.Equals("HiScore1"))
								{
									c演奏記録.nハイスコア[0] = int.Parse(para);
								}
								else if (item.Equals("HiScore2"))
								{
									c演奏記録.nハイスコア[1] = int.Parse(para);
								}
								else if (item.Equals("HiScore3"))
								{
									c演奏記録.nハイスコア[2] = int.Parse(para);
								}
								else if (item.Equals("HiScore4"))
								{
									c演奏記録.nハイスコア[3] = int.Parse(para);
								}
								else if (item.Equals("HiScore5"))
								{
									c演奏記録.nハイスコア[4] = int.Parse(para);
								}
								else if (item.Equals("HiScore6"))
								{
									c演奏記録.nハイスコア[5] = int.Parse(para);
								}
								else if (item.Equals("HiScore7"))
								{
									c演奏記録.nハイスコア[6] = int.Parse(para);
								}
								else if (item.Equals("SecondScore1"))
								{
									c演奏記録.nSecondScore[0] = int.Parse(para);
								}
								else if (item.Equals("SecondScore2"))
								{
									c演奏記録.nSecondScore[1] = int.Parse(para);
								}
								else if (item.Equals("SecondScore3"))
								{
									c演奏記録.nSecondScore[2] = int.Parse(para);
								}
								else if (item.Equals("SecondScore4"))
								{
									c演奏記録.nSecondScore[3] = int.Parse(para);
								}
								else if (item.Equals("SecondScore5"))
								{
									c演奏記録.nSecondScore[4] = int.Parse(para);
								}
								else if (item.Equals("SecondScore6"))
								{
									c演奏記録.nSecondScore[5] = int.Parse(para);
								}
								else if (item.Equals("SecondScore7"))
								{
									c演奏記録.nSecondScore[6] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore1"))
								{
									c演奏記録.nThirdScore[0] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore2"))
								{
									c演奏記録.nThirdScore[1] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore3"))
								{
									c演奏記録.nThirdScore[2] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore4"))
								{
									c演奏記録.nThirdScore[3] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore5"))
								{
									c演奏記録.nThirdScore[4] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore6"))
								{
									c演奏記録.nThirdScore[5] = int.Parse(para);
								}
								else if (item.Equals("ThirdScore7"))
								{
									c演奏記録.nThirdScore[6] = int.Parse(para);
								}
								if (item.Equals("PlaySkill"))
								{
									try
									{
										c演奏記録.db演奏型スキル値 = (double)decimal.Parse(para);
									}
									catch
									{
										c演奏記録.db演奏型スキル値 = 0.0;
									}
								}
								else if (item.Equals("Skill"))
								{
									try
									{
										c演奏記録.dbゲーム型スキル値 = (double)decimal.Parse(para);
									}
									catch
									{
										c演奏記録.dbゲーム型スキル値 = 0.0;
									}
								}
								else if (item.Equals("Perfect"))
								{
									c演奏記録.nPerfect数 = int.Parse(para);
								}
								else if (item.Equals("Great"))
								{
									c演奏記録.nGreat数 = int.Parse(para);
								}
								else if (item.Equals("Good"))
								{
									c演奏記録.nGood数 = int.Parse(para);
								}
								else if (item.Equals("Poor"))
								{
									c演奏記録.nPoor数 = int.Parse(para);
								}
								else if (item.Equals("Miss"))
								{
									c演奏記録.nMiss数 = int.Parse(para);
								}
								else if (item.Equals("Roll"))
								{
									c演奏記録.n連打数 = int.Parse(para);
								}
								else if (item.Equals("MaxCombo"))
								{
									c演奏記録.n最大コンボ数 = int.Parse(para);
								}
								else if (item.Equals("TotalChips"))
								{
									c演奏記録.n全チップ数 = int.Parse(para);
								}
								else if (item.Equals("Risky"))
								{
									c演奏記録.nRisky = int.Parse(para);
								}
								else if (item.Equals("TightDrums"))
								{
									c演奏記録.bTight = C変換.bONorOFF(para[0]);
								}
								#endregion
								else
								{
									#region [ ScrollSpeedDrums ]
									if (item.Equals("ScrollSpeedDrums"))
									{
										c演奏記録.f譜面スクロール速度 = (float)decimal.Parse(para);
									}
									#endregion
									#region [ PlaySpeed ]
									else if (item.Equals("PlaySpeed"))
									{
										string[] strArray2 = para.Split(new char[] { '/' });
										if (strArray2.Length == 2)
										{
											c演奏記録.n演奏速度分子 = int.Parse(strArray2[0]);
											c演奏記録.n演奏速度分母 = int.Parse(strArray2[1]);
										}
									}
									#endregion
									else
									{
										#region [ Drums ]
										if (item.Equals("Drums"))
										{
											c演奏記録.bDrums有効 = C変換.bONorOFF(para[0]);
										}
										#endregion
										#region [ StageFailed ]
										else if (item.Equals("StageFailed"))
										{
											c演奏記録.bSTAGEFAILED有効 = C変換.bONorOFF(para[0]);
										}
										#endregion
										else
										{
											if (item.Equals("UseKeyboard"))
											{
												c演奏記録.b演奏にキーボードを使用した = C変換.bONorOFF(para[0]);
											}
											else if (item.Equals("UseMIDIIN"))
											{
												c演奏記録.b演奏にMIDI入力を使用した = C変換.bONorOFF(para[0]);
											}
											else if (item.Equals("UseJoypad"))
											{
												c演奏記録.b演奏にジョイパッドを使用した = C変換.bONorOFF(para[0]);
											}
											else if (item.Equals("UseMouse"))
											{
												c演奏記録.b演奏にマウスを使用した = C変換.bONorOFF(para[0]);
											}
											else if (item.Equals("PerfectRange"))
											{
												c演奏記録.nPerfectになる範囲ms = int.Parse(para);
											}
											else if (item.Equals("GreatRange"))
											{
												c演奏記録.nGreatになる範囲ms = int.Parse(para);
											}
											else if (item.Equals("GoodRange"))
											{
												c演奏記録.nGoodになる範囲ms = int.Parse(para);
											}
											else if (item.Equals("PoorRange"))
											{
												c演奏記録.nPoorになる範囲ms = int.Parse(para);
											}
											else if (item.Equals("DTXManiaVersion"))
											{
												c演奏記録.strDTXManiaのバージョン = para;
											}
											else if (item.Equals("DateTime"))
											{
												c演奏記録.最終更新日時 = para;
											}
											else if (item.Equals("Hash"))
											{
												c演奏記録.Hash = para;
											}
											else if (item.Equals("9LaneMode"))
											{
												c演奏記録.レーン9モード = C変換.bONorOFF(para[0]);
											}
											else if (item.Equals("HiScore1"))
											{
												c演奏記録.nハイスコア[0] = int.Parse(para);
											}
											else if (item.Equals("HiScore2"))
											{
												c演奏記録.nハイスコア[1] = int.Parse(para);
											}
											else if (item.Equals("HiScore3"))
											{
												c演奏記録.nハイスコア[2] = int.Parse(para);
											}
											else if (item.Equals("HiScore4"))
											{
												c演奏記録.nハイスコア[3] = int.Parse(para);
											}
											else if (item.Equals("HiScore5"))
											{
												c演奏記録.nハイスコア[4] = int.Parse(para);
											}
											else if (item.Equals("HiScore6"))
											{
												c演奏記録.nハイスコア[5] = int.Parse(para);
											}
											else if (item.Equals("HiScore7"))
											{
												c演奏記録.nハイスコア[6] = int.Parse(para);
											}
											else if (item.Equals("SecondScore1"))
											{
												c演奏記録.nSecondScore[0] = int.Parse(para);
											}
											else if (item.Equals("SecondScore2"))
											{
												c演奏記録.nSecondScore[1] = int.Parse(para);
											}
											else if (item.Equals("SecondScore3"))
											{
												c演奏記録.nSecondScore[2] = int.Parse(para);
											}
											else if (item.Equals("SecondScore4"))
											{
												c演奏記録.nSecondScore[3] = int.Parse(para);
											}
											else if (item.Equals("SecondScore5"))
											{
												c演奏記録.nSecondScore[4] = int.Parse(para);
											}
											else if (item.Equals("SecondScore6"))
											{
												c演奏記録.nSecondScore[5] = int.Parse(para);
											}
											else if (item.Equals("SecondScore7"))
											{
												c演奏記録.nSecondScore[6] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore1"))
											{
												c演奏記録.nThirdScore[0] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore2"))
											{
												c演奏記録.nThirdScore[1] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore3"))
											{
												c演奏記録.nThirdScore[2] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore4"))
											{
												c演奏記録.nThirdScore[3] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore5"))
											{
												c演奏記録.nThirdScore[4] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore6"))
											{
												c演奏記録.nThirdScore[5] = int.Parse(para);
											}
											else if (item.Equals("ThirdScore7"))
											{
												c演奏記録.nThirdScore[6] = int.Parse(para);
											}
											else if (item.Equals("HiScorerName1"))
											{
												c演奏記録.strHiScorerName[0] = para;
											}
											else if (item.Equals("HiScorerName2"))
											{
												c演奏記録.strHiScorerName[1] = para;
											}
											else if (item.Equals("HiScorerName3"))
											{
												c演奏記録.strHiScorerName[2] = para;
											}
											else if (item.Equals("HiScorerName4"))
											{
												c演奏記録.strHiScorerName[3] = para;
											}
											else if (item.Equals("HiScorerName5"))
											{
												c演奏記録.strHiScorerName[4] = para;
											}
											else if (item.Equals("HiScorerName6"))
											{
												c演奏記録.strHiScorerName[5] = para;
											}
											else if (item.Equals("HiScorerName7"))
											{
												c演奏記録.strHiScorerName[6] = para;
											}
											else if (item.Equals("SecondScorerName1"))
											{
												c演奏記録.strSecondScorerName[0] = para;
											}
											else if (item.Equals("SecondScorerName2"))
											{
												c演奏記録.strSecondScorerName[1] = para;
											}
											else if (item.Equals("SecondScorerName3"))
											{
												c演奏記録.strSecondScorerName[2] = para;
											}
											else if (item.Equals("SecondScorerName4"))
											{
												c演奏記録.strSecondScorerName[3] = para;
											}
											else if (item.Equals("SecondScorerName5"))
											{
												c演奏記録.strSecondScorerName[4] = para;
											}
											else if (item.Equals("SecondScorerName6"))
											{
												c演奏記録.strSecondScorerName[5] = para;
											}
											else if (item.Equals("SecondScorerName7"))
											{
												c演奏記録.strSecondScorerName[6] = para;
											}
											else if (item.Equals("ThirdScorerName1"))
											{
												c演奏記録.strThirdScorerName[0] = para;
											}
											else if (item.Equals("ThirdScorerName2"))
											{
												c演奏記録.strThirdScorerName[1] = para;
											}
											else if (item.Equals("ThirdScorerName3"))
											{
												c演奏記録.strThirdScorerName[2] = para;
											}
											else if (item.Equals("ThirdScorerName4"))
											{
												c演奏記録.strThirdScorerName[3] = para;
											}
											else if (item.Equals("ThirdScorerName5"))
											{
												c演奏記録.strThirdScorerName[4] = para;
											}
											else if (item.Equals("ThirdScorerName6"))
											{
												c演奏記録.strThirdScorerName[5] = para;
											}
											else if (item.Equals("ThirdScorerName7"))
											{
												c演奏記録.strThirdScorerName[6] = para;
											}
											else if (item.Equals("Crown1"))
											{
												c演奏記録.n王冠[0] = int.Parse(para);
											}
											else if (item.Equals("Crown2"))
											{
												c演奏記録.n王冠[1] = int.Parse(para);
											}
											else if (item.Equals("Crown3"))
											{
												c演奏記録.n王冠[2] = int.Parse(para);
											}
											else if (item.Equals("Crown4"))
											{
												c演奏記録.n王冠[3] = int.Parse(para);
											}
											else if (item.Equals("Crown5"))
											{
												c演奏記録.n王冠[4] = int.Parse(para);
											}
											else if (item.Equals("Crown6"))
											{
												c演奏記録.n王冠[5] = int.Parse(para);
											}
											else if (item.Equals("Crown7"))
											{
												c演奏記録.n王冠[6] = int.Parse(para);
											}
										}
									}

								}
								continue;
							}
							catch (Exception exception)
							{
								Trace.TraceError(exception.ToString());
								Trace.TraceError("読み込みを中断します。({0})", iniファイル名);
								break;
							}
						}
					}
				}
			}
		}

		internal void tヒストリを追加する( string str追加文字列 )
		{
			this.stファイル.HistoryCount++;
			for( int i = 3; i >= 0; i-- )
				this.stファイル.History[ i + 1 ] = this.stファイル.History[ i ];
			DateTime now = DateTime.Now;
			this.stファイル.History[ 0 ] = string.Format( "{0:0}.{1:D2}/{2}/{3} {4}", this.stファイル.HistoryCount, now.Year % 100, now.Month, now.Day, str追加文字列 );
		}
		internal void t書き出し( string iniファイル名 )
		{
			this.iniファイルのあるフォルダ名 = Path.GetDirectoryName( iniファイル名 );
			this.iniファイル名 = Path.GetFileName( iniファイル名 );

			StreamWriter writer = new StreamWriter( iniファイル名, false, new UTF8Encoding(false));
			writer.WriteLine( "[File]" );
			writer.WriteLine( "Title={0}", this.stファイル.Title );
			writer.WriteLine( "Name={0}", this.stファイル.Name );
			writer.WriteLine( "Hash={0}", this.stファイル.Hash );
			writer.WriteLine( "PlayCountDrums={0}", this.stファイル.PlayCountDrums );
			writer.WriteLine( "ClearCountDrums={0}", this.stファイル.ClearCountDrums );       // #23596 10.11.16 add ikanick
			writer.WriteLine( "BestRankDrums={0}", this.stファイル.BestRank );		// #24459 2011.2.24 yyagi
			writer.WriteLine( "HistoryCount={0}", this.stファイル.HistoryCount );
			writer.WriteLine( "History0={0}", this.stファイル.History[ 0 ] );
			writer.WriteLine( "History1={0}", this.stファイル.History[ 1 ] );
			writer.WriteLine( "History2={0}", this.stファイル.History[ 2 ] );
			writer.WriteLine( "History3={0}", this.stファイル.History[ 3 ] );
			writer.WriteLine( "History4={0}", this.stファイル.History[ 4 ] );
			writer.WriteLine( "BGMAdjust={0}", this.stファイル.BGMAdjust );
			writer.WriteLine();

			for (int i = 0; i < 2; i++)
			{
				string[] strArray = { "HiScore.Drums", "LastPlay.Drums" };
				writer.WriteLine("[{0}]", strArray[i]);
				writer.WriteLine("Score={0}", this.stセクション[i].nスコア);
				writer.WriteLine("PlaySkill={0}", this.stセクション[i].db演奏型スキル値);
				writer.WriteLine("Skill={0}", this.stセクション[i].dbゲーム型スキル値);
				writer.WriteLine("Perfect={0}", this.stセクション[i].nPerfect数);
				writer.WriteLine("Great={0}", this.stセクション[i].nGreat数);
				writer.WriteLine("Good={0}", this.stセクション[i].nGood数);
				writer.WriteLine("Poor={0}", this.stセクション[i].nPoor数);
				writer.WriteLine("Miss={0}", this.stセクション[i].nMiss数);
				writer.WriteLine("MaxCombo={0}", this.stセクション[i].n最大コンボ数);
				writer.WriteLine("TotalChips={0}", this.stセクション[i].n全チップ数);
				writer.WriteLine();
				writer.WriteLine("Risky={0}", this.stセクション[i].nRisky);
				writer.WriteLine("TightDrums={0}", this.stセクション[i].bTight ? 1 : 0);
				writer.WriteLine("RandomDrums={0}", (int)this.stセクション[i].eRandom);
				writer.WriteLine("ScrollSpeedDrums={0}", this.stセクション[i].f譜面スクロール速度);
				writer.WriteLine("PlaySpeed={0}/{1}", this.stセクション[i].n演奏速度分子, this.stセクション[i].n演奏速度分母);
				writer.WriteLine("Drums={0}", this.stセクション[i].bDrums有効 ? 1 : 0);
				writer.WriteLine("StageFailed={0}", this.stセクション[i].bSTAGEFAILED有効 ? 1 : 0);
				writer.WriteLine("UseKeyboard={0}", this.stセクション[i].b演奏にキーボードを使用した ? 1 : 0);
				writer.WriteLine("UseMIDIIN={0}", this.stセクション[i].b演奏にMIDI入力を使用した ? 1 : 0);
				writer.WriteLine("UseJoypad={0}", this.stセクション[i].b演奏にジョイパッドを使用した ? 1 : 0);
				writer.WriteLine("UseMouse={0}", this.stセクション[i].b演奏にマウスを使用した ? 1 : 0);
				writer.WriteLine("PerfectRange={0}", this.stセクション[i].nPerfectになる範囲ms);
				writer.WriteLine("GreatRange={0}", this.stセクション[i].nGreatになる範囲ms);
				writer.WriteLine("GoodRange={0}", this.stセクション[i].nGoodになる範囲ms);
				writer.WriteLine("PoorRange={0}", this.stセクション[i].nPoorになる範囲ms);
				writer.WriteLine("DTXManiaVersion={0}", this.stセクション[i].strDTXManiaのバージョン);
				writer.WriteLine("DateTime={0}", this.stセクション[i].最終更新日時);
				writer.WriteLine("Hash={0}", this.stセクション[i].Hash);
				writer.WriteLine("HiScore1={0}", this.stセクション[i].nハイスコア[0]);
				writer.WriteLine("HiScore2={0}", this.stセクション[i].nハイスコア[1]);
				writer.WriteLine("HiScore3={0}", this.stセクション[i].nハイスコア[2]);
				writer.WriteLine("HiScore4={0}", this.stセクション[i].nハイスコア[3]);
				writer.WriteLine("HiScore5={0}", this.stセクション[i].nハイスコア[4]);
				writer.WriteLine("HiScore6={0}", this.stセクション[i].nハイスコア[5]);
				writer.WriteLine("HiScore7={0}", this.stセクション[i].nハイスコア[6]);
				writer.WriteLine("SecondScore1={0}", this.stセクション[i].nSecondScore[0]);
				writer.WriteLine("SecondScore2={0}", this.stセクション[i].nSecondScore[1]);
				writer.WriteLine("SecondScore3={0}", this.stセクション[i].nSecondScore[2]);
				writer.WriteLine("SecondScore4={0}", this.stセクション[i].nSecondScore[3]);
				writer.WriteLine("SecondScore5={0}", this.stセクション[i].nSecondScore[4]);
				writer.WriteLine("SecondScore6={0}", this.stセクション[i].nSecondScore[5]);
				writer.WriteLine("SecondScore7={0}", this.stセクション[i].nSecondScore[6]);
				writer.WriteLine("ThirdScore1={0}", this.stセクション[i].nThirdScore[0]);
				writer.WriteLine("ThirdScore2={0}", this.stセクション[i].nThirdScore[1]);
				writer.WriteLine("ThirdScore3={0}", this.stセクション[i].nThirdScore[2]);
				writer.WriteLine("ThirdScore4={0}", this.stセクション[i].nThirdScore[3]);
				writer.WriteLine("ThirdScore5={0}", this.stセクション[i].nThirdScore[4]);
				writer.WriteLine("ThirdScore6={0}", this.stセクション[i].nThirdScore[5]);
				writer.WriteLine("ThirdScore7={0}", this.stセクション[i].nThirdScore[6]);
				writer.WriteLine("HiScorerName1={0}", this.stセクション[i].strHiScorerName[0]);
				writer.WriteLine("HiScorerName2={0}", this.stセクション[i].strHiScorerName[1]);
				writer.WriteLine("HiScorerName3={0}", this.stセクション[i].strHiScorerName[2]);
				writer.WriteLine("HiScorerName4={0}", this.stセクション[i].strHiScorerName[3]);
				writer.WriteLine("HiScorerName5={0}", this.stセクション[i].strHiScorerName[4]);
				writer.WriteLine("HiScorerName6={0}", this.stセクション[i].strHiScorerName[5]);
				writer.WriteLine("HiScorerName7={0}", this.stセクション[i].strHiScorerName[6]);
				writer.WriteLine("SecondScorerName1={0}", this.stセクション[i].strSecondScorerName[0]);
				writer.WriteLine("SecondScorerName2={0}", this.stセクション[i].strSecondScorerName[1]);
				writer.WriteLine("SecondScorerName3={0}", this.stセクション[i].strSecondScorerName[2]);
				writer.WriteLine("SecondScorerName4={0}", this.stセクション[i].strSecondScorerName[3]);
				writer.WriteLine("SecondScorerName5={0}", this.stセクション[i].strSecondScorerName[4]);
				writer.WriteLine("SecondScorerName6={0}", this.stセクション[i].strSecondScorerName[5]);
				writer.WriteLine("SecondScorerName7={0}", this.stセクション[i].strSecondScorerName[6]);
				writer.WriteLine("ThirdScorerName1={0}", this.stセクション[i].strThirdScorerName[0]);
				writer.WriteLine("ThirdScorerName2={0}", this.stセクション[i].strThirdScorerName[1]);
				writer.WriteLine("ThirdScorerName3={0}", this.stセクション[i].strThirdScorerName[2]);
				writer.WriteLine("ThirdScorerName4={0}", this.stセクション[i].strThirdScorerName[3]);
				writer.WriteLine("ThirdScorerName5={0}", this.stセクション[i].strThirdScorerName[4]);
				writer.WriteLine("ThirdScorerName6={0}", this.stセクション[i].strThirdScorerName[5]);
				writer.WriteLine("ThirdScorerName7={0}", this.stセクション[i].strThirdScorerName[6]);
				writer.WriteLine("Crown1={0}", this.stセクション[i].n王冠[0]);
				writer.WriteLine("Crown2={0}", this.stセクション[i].n王冠[1]);
				writer.WriteLine("Crown3={0}", this.stセクション[i].n王冠[2]);
				writer.WriteLine("Crown4={0}", this.stセクション[i].n王冠[3]);
				writer.WriteLine("Crown5={0}", this.stセクション[i].n王冠[4]);
				writer.WriteLine("Crown6={0}", this.stセクション[i].n王冠[5]);
				writer.WriteLine("Crown7={0}", this.stセクション[i].n王冠[6]);
			}

			writer.Close();
		}
		internal void t全演奏記録セクションの整合性をチェックし不整合があればリセットする()
		{
			for( int i = 0; i < 2; i++ )
			{ 
				if (!this.b整合性がある((Eセクション種別)i))
					this.stセクション[i] = new C演奏記録();
			}
		}
		internal static int tランク値を計算して返す( C演奏記録 part )
		{
			if( part.b演奏にMIDI入力を使用した || part.b演奏にキーボードを使用した || part.b演奏にジョイパッドを使用した || part.b演奏にマウスを使用した )	// 2010.9.11
			{
				int nTotal = part.nPerfect数 + part.nGreat数 + part.nGood数 + part.nPoor数 + part.nMiss数;
				return tランク値を計算して返す( nTotal, part.nPerfect数, part.nGreat数, part.nGood数, part.nPoor数, part.nMiss数 );
			}
			return (int)ERANK.UNKNOWN;
		}
		internal static int tランク値を計算して返す( int nTotal, int nPerfect, int nGreat, int nGood, int nPoor, int nMiss )
		{
			if( nTotal <= 0 )
				return (int)ERANK.UNKNOWN;

			//int nRank = (int)ERANK.E;
			int nAuto = nTotal - ( nPerfect + nGreat + nGood + nPoor + nMiss );
			if( nTotal == nAuto )
			{
				return (int)ERANK.SS;
			}
			double dRate = ( (double) ( nPerfect + nGreat ) ) / ( (double) ( nTotal - nAuto ) );
			if( dRate == 1.0 )
			{
				return (int)ERANK.SS;
			}
			if( dRate >= 0.95 )
			{
				return (int)ERANK.S;
			}
			if( dRate >= 0.9 )
			{
				return (int)ERANK.A;
			}
			if( dRate >= 0.85 )
			{
				return (int)ERANK.B;
			}
			if( dRate >= 0.8 )
			{
				return (int)ERANK.C;
			}
			if( dRate >= 0.7 )
			{
				return (int)ERANK.D;
			}
			return (int)ERANK.E;
		}
		internal static double tゲーム型スキルを計算して返す( int nLevel, int nTotal, int nPerfect, int nCombo)
		{
			double ret;
			if( ( nTotal == 0 ) || ( ( nPerfect == 0 ) && ( nCombo == 0 ) ) )
				ret = 0.0;

			ret = ( ( nLevel * ( ( nPerfect * 0.8 + nCombo * 0.2 ) / ( (double) nTotal ) ) ) / 2.0 );

			return ret;
		}
		internal static double t演奏型スキルを計算して返す( int nTotal, int nPerfect, int nGreat, int nGood, int nPoor, int nMiss)
		{
			if( nTotal == 0 )
				return 0.0;

			int nAuto = nTotal - ( nPerfect + nGreat + nGood + nPoor  + nMiss );
			double y = ( ( nPerfect * 1.0 + nGreat * 0.8 + nGood * 0.5 + nPoor * 0.2 + nMiss * 0.0 + nAuto * 0.0 ) * 100.0 ) / ( (double) nTotal );
			double ret = ( 100.0 * ( ( Math.Pow( 1.03, y ) - 1.0 ) / ( Math.Pow( 1.03, 100.0 ) - 1.0 ) ) );

			return ret;
		}
		internal static string t演奏セクションのMD5を求めて返す( C演奏記録 cc )
		{
			StringBuilder builder = new StringBuilder();
			builder.Append( cc.nスコア.ToString() );
			builder.Append( cc.dbゲーム型スキル値.ToString( ".000000" ) );
			builder.Append( cc.db演奏型スキル値.ToString( ".000000" ) );
			builder.Append( cc.nPerfect数 );
			builder.Append( cc.nGreat数 );
			builder.Append( cc.nGood数 );
			builder.Append( cc.nPoor数 );
			builder.Append( cc.nMiss数 );
			builder.Append( cc.n最大コンボ数 );
			builder.Append( cc.n全チップ数 );
			builder.Append( boolToChar( cc.bTight ) );
			builder.Append( cc.f譜面スクロール速度.ToString( ".000000" ) );
			builder.Append( cc.n演奏速度分子 );
			builder.Append( cc.n演奏速度分母 );
			builder.Append( boolToChar( cc.bDrums有効 ) );
			builder.Append( boolToChar( cc.bSTAGEFAILED有効 ) );
			builder.Append( boolToChar( cc.b演奏にキーボードを使用した ) );
			builder.Append( boolToChar( cc.b演奏にMIDI入力を使用した ) );
			builder.Append( boolToChar( cc.b演奏にジョイパッドを使用した ) );
			builder.Append( boolToChar( cc.b演奏にマウスを使用した ) );
			builder.Append( cc.nPerfectになる範囲ms );
			builder.Append( cc.nGreatになる範囲ms );
			builder.Append( cc.nGoodになる範囲ms );
			builder.Append( cc.nPoorになる範囲ms );
			builder.Append( cc.strDTXManiaのバージョン );
			builder.Append( cc.最終更新日時 );

			byte[] bytes = Encoding.GetEncoding( "Shift_JIS" ).GetBytes( builder.ToString() );
			StringBuilder builder2 = new StringBuilder(0x21);
			{
				MD5CryptoServiceProvider m = new MD5CryptoServiceProvider();
				byte[] buffer2 = m.ComputeHash(bytes);
				foreach (byte num2 in buffer2)
					builder2.Append(num2.ToString("x2"));
			}
			return builder2.ToString();
		}
		internal static int t総合ランク値を計算して返す( C演奏記録 Drums )
		{
			int nTotal   = Drums.n全チップ数;
			int nPerfect = Drums.nPerfect数_Auto含まない;	// #24569 2011.3.1 yyagi: to calculate result rank without AUTO chips
			int nGreat =   Drums.nGreat数_Auto含まない;		//
			int nGood =    Drums.nGood数_Auto含まない;		//
			int nPoor =    Drums.nPoor数_Auto含まない;		//
			int nMiss =    Drums.nMiss数_Auto含まない;		//
			return tランク値を計算して返す( nTotal, nPerfect, nGreat, nGood, nPoor, nMiss );
		}

		// その他

		#region [ private ]
		//-----------------
		private bool ONorOFF( char c )
		{
			return ( c != '0' );
		}
		private static char boolToChar( bool b )
		{
			if( !b )
			{
				return '0';
			}
			return '1';
		}
		//-----------------
		#endregion
	}
}
