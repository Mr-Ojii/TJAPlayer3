using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TJAPlayer3.C曲リストノードComparers;
using FDK;

namespace TJAPlayer3
{
	[Serializable]
	internal class CSongs管理
	{
		// プロパティ

		public int nSongsDBから取得できたスコア数
		{
			get;
			set;
		}
		public int nSongsDBへ出力できたスコア数
		{
			get;
			set;
		}
		public int nスコアキャッシュから反映できたスコア数
		{
			get;
			set;
		}
		public int nファイルから反映できたスコア数
		{
			get;
			set;
		}
		public int n検索されたスコア数
		{
			get;
			set;
		}
		public int n検索された曲ノード数
		{
			get;
			set;
		}
		[NonSerialized]
		public List<Cスコア> listSongsDB;                  // songs.dbから構築されるlist
		public List<C曲リストノード> list曲ルート;         // 起動時にフォルダ検索して構築されるlist
		public bool bIsSuspending                           // 外部スレッドから、内部スレッドのsuspendを指示する時にtrueにする
		{                                                   // 再開時は、これをfalseにしてから、次のautoReset.Set()を実行する
			get;
			set;
		}
		[NonSerialized]
		private AutoResetEvent autoReset;
		public AutoResetEvent AutoReset
		{
			get
			{
				return autoReset;
			}
			private set
			{
				autoReset = value;
			}
		}

		// コンストラクタ

		public CSongs管理()
		{
			this.listSongsDB = new List<Cスコア>();
			this.list曲ルート = new List<C曲リストノード>();
			this.n検索された曲ノード数 = 0;
			this.n検索されたスコア数 = 0;
			this.bIsSuspending = false;                     // #27060
			this.autoReset = new AutoResetEvent(true);  // #27060
		}


		// メソッド

		#region [ SongsDB(songs.db) を読み込む ]
		//-----------------
		public void tSongsDBを読み込む(string SongsDBファイル名)
		{
			this.nSongsDBから取得できたスコア数 = 0;
			if (File.Exists(SongsDBファイル名))
			{
				BinaryReader br = null;
				try
				{
					br = new BinaryReader(File.OpenRead(SongsDBファイル名));
					if (!br.ReadString().Equals(SONGSDB_VERSION))
					{
						throw new InvalidDataException("ヘッダが異なります。");
					}
					this.listSongsDB = new List<Cスコア>();

					while (true)
					{
						try
						{
							Cスコア item = this.tSongsDBからスコアを１つ読み込む(br);
							this.listSongsDB.Add(item);
							this.nSongsDBから取得できたスコア数++;
						}
						catch (EndOfStreamException)
						{
							break;
						}
					}
				}
				finally
				{
					if (br != null)
						br.Close();
				}
			}
		}
		//-----------------
		#endregion

		#region [ 曲を検索してリストを作成する ]
		//-----------------
		public void t曲を検索してリストを作成する(string str基点フォルダ, bool b子BOXへ再帰する)
		{
			this.t曲を検索してリストを作成する(str基点フォルダ, b子BOXへ再帰する, this.list曲ルート, null);
		}
		private void t曲を検索してリストを作成する(string str基点フォルダ, bool b子BOXへ再帰する, List<C曲リストノード> listノードリスト, C曲リストノード node親)
		{
			if (!str基点フォルダ.EndsWith(@"\"))
				str基点フォルダ = str基点フォルダ + @"\";

			DirectoryInfo info = new DirectoryInfo(str基点フォルダ);

			if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
				Trace.TraceInformation("基点フォルダ: " + str基点フォルダ);

			#region [ 個別ファイルからノード作成 ]
			//-----------------------------
			foreach (FileInfo fileinfo in info.GetFiles())
			{
				SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす
				string strExt = fileinfo.Extension.ToLower();

				if ((strExt.Equals(".tja")))
				{
					#region[ 新処理 ]
					CDTX dtx = new CDTX(str基点フォルダ + fileinfo.Name, false, 1.0, 0);
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.SCORE;

					c曲リストノード.r親ノード = node親;
					c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード == null) ?
						str基点フォルダ + fileinfo.Name : c曲リストノード.r親ノード.strBreadcrumbs + " > " + str基点フォルダ + fileinfo.Name;

					c曲リストノード.strタイトル = dtx.TITLE;
					c曲リストノード.strサブタイトル = dtx.SUBTITLE;
					if (!string.IsNullOrEmpty(dtx.GENRE))
					{
						c曲リストノード.strジャンル = dtx.GENRE;
					}
					else
					{
						if (c曲リストノード.r親ノード != null && c曲リストノード.r親ノード.strジャンル != "")
						{
							// .tjaのジャンルが存在しなくて、かつ親ノードにジャンルが指定されていればそちらを読み込む。
							c曲リストノード.strジャンル = c曲リストノード.r親ノード.strジャンル;
						}
					}

					if (c曲リストノード.r親ノード != null)
					{
						if (c曲リストノード.r親ノード.IsChangedForeColor)
						{
							c曲リストノード.ForeColor = c曲リストノード.r親ノード.ForeColor;
							c曲リストノード.IsChangedForeColor = true;
						}
						if (c曲リストノード.r親ノード.IsChangedBackColor)
						{
							c曲リストノード.BackColor = c曲リストノード.r親ノード.BackColor;
							c曲リストノード.IsChangedBackColor = true;
						}
					}


					switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
					{
						case 0:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
							break;
						case 1:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
							break;
						case 2:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
							break;
						case 3:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
							break;
						case 4:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
							break;
						case 5:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
							break;
						case 6:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
							break;
						case 7:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
							break;
						default:
							break;
					}


					c曲リストノード.nLevel = dtx.LEVELtaiko;


					bool b = false;

					c曲リストノード.arスコア = new Cスコア();
					c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス = str基点フォルダ + fileinfo.Name;
					c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス = str基点フォルダ;
					c曲リストノード.arスコア.ファイル情報.ファイルサイズ = fileinfo.Length;
					c曲リストノード.arスコア.ファイル情報.最終更新日時 = fileinfo.LastWriteTime;
					string strFileNameScoreIni = c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス + ".score.ini";
					if (File.Exists(strFileNameScoreIni))
					{
						FileInfo infoScoreIni = new FileInfo(strFileNameScoreIni);
						c曲リストノード.arスコア.ScoreIni情報.ファイルサイズ = infoScoreIni.Length;
						c曲リストノード.arスコア.ScoreIni情報.最終更新日時 = infoScoreIni.LastWriteTime;
					}

					for (int n = 0; n < (int)Difficulty.Total; n++)
					{
						if (dtx.b譜面が存在する[n])
						{
							c曲リストノード.nスコア数++;
							c曲リストノード.arスコア.譜面情報.b譜面が存在する[n] = true;
							if (b == false)
							{
								this.n検索されたスコア数++;
								listノードリスト.Add(c曲リストノード);
								this.n検索された曲ノード数++;
								b = true;
							}
						}
					}
					dtx = null;
					#endregion
				}
				else if (strExt.Equals(".tcm"))
				{
					#region[ tcm処理 ]
					CDTX dtx = new CDTX(str基点フォルダ + fileinfo.Name, false, 1.0, 0);
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.SCORE;

					c曲リストノード.r親ノード = node親;
					c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード == null) ?
						str基点フォルダ + fileinfo.Name : c曲リストノード.r親ノード.strBreadcrumbs + " > " + str基点フォルダ + fileinfo.Name;

					c曲リストノード.strタイトル = dtx.TITLE;
					c曲リストノード.strサブタイトル = dtx.SUBTITLE;
					if (!string.IsNullOrEmpty(dtx.GENRE))
					{
						c曲リストノード.strジャンル = dtx.GENRE;
					}
					else
					{
						if (c曲リストノード.r親ノード != null && c曲リストノード.r親ノード.strジャンル != "")
						{
							// .tjaのジャンルが存在しなくて、かつ親ノードにジャンルが指定されていればそちらを読み込む。
							c曲リストノード.strジャンル = c曲リストノード.r親ノード.strジャンル;
						}
					}

					if (c曲リストノード.r親ノード != null)
					{
						if (c曲リストノード.r親ノード.IsChangedForeColor)
						{
							c曲リストノード.ForeColor = c曲リストノード.r親ノード.ForeColor;
							c曲リストノード.IsChangedForeColor = true;
						}
						if (c曲リストノード.r親ノード.IsChangedBackColor)
						{
							c曲リストノード.BackColor = c曲リストノード.r親ノード.BackColor;
							c曲リストノード.IsChangedBackColor = true;
						}
					}


					switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
					{
						case 0:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
							break;
						case 1:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
							break;
						case 2:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
							break;
						case 3:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
							break;
						case 4:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
							break;
						case 5:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
							break;
						case 6:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
							break;
						case 7:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
							break;
						default:
							break;
					}


					c曲リストノード.nLevel = dtx.LEVELtaiko;


					bool b = false;

					c曲リストノード.arスコア = new Cスコア();
					c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス = str基点フォルダ + fileinfo.Name;
					c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス = str基点フォルダ;
					c曲リストノード.arスコア.ファイル情報.ファイルサイズ = fileinfo.Length;
					c曲リストノード.arスコア.ファイル情報.最終更新日時 = fileinfo.LastWriteTime;
					string strFileNameScoreIni = c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス + ".score.ini";
					if (File.Exists(strFileNameScoreIni))
					{
						FileInfo infoScoreIni = new FileInfo(strFileNameScoreIni);
						c曲リストノード.arスコア.ScoreIni情報.ファイルサイズ = infoScoreIni.Length;
						c曲リストノード.arスコア.ScoreIni情報.最終更新日時 = infoScoreIni.LastWriteTime;
					}

					for (int n = 0; n < (int)Difficulty.Total; n++)
					{
						if (dtx.b譜面が存在する[n])
						{
							c曲リストノード.nスコア数++;
							c曲リストノード.arスコア.譜面情報.b譜面が存在する[n] = true;
							if (b == false)
							{
								this.n検索されたスコア数++;
								listノードリスト.Add(c曲リストノード);
								this.n検索された曲ノード数++;
								b = true;
							}
						}
					}
					dtx = null;
					#endregion
				}
				else if (strExt.Equals(".tci"))
				{
					#region[ tci処理 ]
					CDTX dtx = new CDTX(str基点フォルダ + fileinfo.Name, false, 1.0, 0);
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.SCORE;

					c曲リストノード.r親ノード = node親;
					c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード == null) ?
						str基点フォルダ + fileinfo.Name : c曲リストノード.r親ノード.strBreadcrumbs + " > " + str基点フォルダ + fileinfo.Name;

					c曲リストノード.strタイトル = dtx.TITLE;
					c曲リストノード.strサブタイトル = dtx.SUBTITLE;
					if (!string.IsNullOrEmpty(dtx.GENRE))
					{
						c曲リストノード.strジャンル = dtx.GENRE;
					}
					else
					{
						if (c曲リストノード.r親ノード != null && c曲リストノード.r親ノード.strジャンル != "")
						{
							// .tjaのジャンルが存在しなくて、かつ親ノードにジャンルが指定されていればそちらを読み込む。
							c曲リストノード.strジャンル = c曲リストノード.r親ノード.strジャンル;
						}
					}

					if (c曲リストノード.r親ノード != null)
					{
						if (c曲リストノード.r親ノード.IsChangedForeColor)
						{
							c曲リストノード.ForeColor = c曲リストノード.r親ノード.ForeColor;
							c曲リストノード.IsChangedForeColor = true;
						}
						if (c曲リストノード.r親ノード.IsChangedBackColor)
						{
							c曲リストノード.BackColor = c曲リストノード.r親ノード.BackColor;
							c曲リストノード.IsChangedBackColor = true;
						}
					}


					switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
					{
						case 0:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
							break;
						case 1:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
							break;
						case 2:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
							break;
						case 3:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
							break;
						case 4:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
							break;
						case 5:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
							break;
						case 6:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
							break;
						case 7:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
							break;
						default:
							break;
					}


					c曲リストノード.nLevel = dtx.LEVELtaiko;


					bool b = false;

					c曲リストノード.arスコア = new Cスコア();
					c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス = str基点フォルダ + fileinfo.Name;
					c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス = str基点フォルダ;
					c曲リストノード.arスコア.ファイル情報.ファイルサイズ = fileinfo.Length;
					c曲リストノード.arスコア.ファイル情報.最終更新日時 = fileinfo.LastWriteTime;
					string strFileNameScoreIni = c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス + ".score.ini";
					if (File.Exists(strFileNameScoreIni))
					{
						FileInfo infoScoreIni = new FileInfo(strFileNameScoreIni);
						c曲リストノード.arスコア.ScoreIni情報.ファイルサイズ = infoScoreIni.Length;
						c曲リストノード.arスコア.ScoreIni情報.最終更新日時 = infoScoreIni.LastWriteTime;
					}

					for (int n = 0; n < (int)Difficulty.Total; n++)
					{
						if (dtx.b譜面が存在する[n])
						{
							c曲リストノード.nスコア数++;
							c曲リストノード.arスコア.譜面情報.b譜面が存在する[n] = true;
							if (b == false)
							{
								this.n検索されたスコア数++;
								listノードリスト.Add(c曲リストノード);
								this.n検索された曲ノード数++;
								b = true;
							}
						}
					}
					dtx = null;
					#endregion
				}

			}
			//-----------------------------
			#endregion

			foreach (DirectoryInfo infoDir in info.GetDirectories())
			{
				SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

				#region [ a. "dtxfiles." で始まるフォルダの場合 ]
				//-----------------------------
				if (infoDir.Name.ToLower().StartsWith("dtxfiles."))
				{
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.BOX;
					c曲リストノード.bDTXFilesで始まるフォルダ名のBOXである = true;
					c曲リストノード.strタイトル = infoDir.Name.Substring(9);
					c曲リストノード.nスコア数 = 1;
					c曲リストノード.r親ノード = node親;
					c曲リストノード.Openindex = 1;



					// 一旦、上位BOXのスキン情報をコピー (後でbox.defの記載にて上書きされる場合がある)
					c曲リストノード.strSkinPath = (c曲リストノード.r親ノード == null) ?
						"" : c曲リストノード.r親ノード.strSkinPath;

					c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード == null) ?
						c曲リストノード.strタイトル : c曲リストノード.r親ノード.strBreadcrumbs + " > " + c曲リストノード.strタイトル;


					c曲リストノード.list子リスト = new List<C曲リストノード>();
					c曲リストノード.arスコア = new Cスコア();
					c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス = infoDir.FullName + @"\";
					c曲リストノード.arスコア.譜面情報.タイトル = c曲リストノード.strタイトル;
					c曲リストノード.arスコア.譜面情報.コメント =
						(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja") ?
						"BOX に移動します。" :
						"Enter into the BOX.";
					listノードリスト.Add(c曲リストノード);
					c曲リストノード.arスコア.譜面情報.b譜面が存在する[0] = true;
					if (File.Exists(infoDir.FullName + @"\box.def"))
					{
						CBoxDef boxdef = new CBoxDef(infoDir.FullName + @"\box.def");
						if ((boxdef.Title != null) && (boxdef.Title.Length > 0))
						{
							c曲リストノード.strタイトル = boxdef.Title;
						}
						if ((boxdef.Genre != null) && (boxdef.Genre.Length > 0))
						{
							c曲リストノード.strジャンル = boxdef.Genre;
						}
						if (boxdef.IsChangedForeColor)
						{
							c曲リストノード.ForeColor = boxdef.ForeColor;
						}
						if (boxdef.IsChangedBackColor)
						{
							c曲リストノード.BackColor = boxdef.BackColor;
						}
					}
					if (b子BOXへ再帰する)
					{
						this.t曲を検索してリストを作成する(infoDir.FullName + @"\", b子BOXへ再帰する, c曲リストノード.list子リスト, c曲リストノード);
					}
				}
				//-----------------------------
				#endregion

				#region [ b.box.def を含むフォルダの場合  ]
				//-----------------------------
				else if (File.Exists(infoDir.FullName + @"\box.def"))
				{
					CBoxDef boxdef = new CBoxDef(infoDir.FullName + @"\box.def");
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.BOX;
					c曲リストノード.bDTXFilesで始まるフォルダ名のBOXである = false;
					c曲リストノード.strタイトル = boxdef.Title;
					c曲リストノード.strジャンル = boxdef.Genre;

					if (boxdef.IsChangedForeColor)
					{
						c曲リストノード.ForeColor = boxdef.ForeColor;
						c曲リストノード.IsChangedForeColor = true;
					}
					if (boxdef.IsChangedBackColor)
					{
						c曲リストノード.BackColor = boxdef.BackColor;
						c曲リストノード.IsChangedBackColor = true;
					}

					switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
					{
						case 0:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
							break;
						case 1:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
							break;
						case 2:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
							break;
						case 3:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
							break;
						case 4:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
							break;
						case 5:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
							break;
						case 6:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
							break;
						case 7:
							c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
							c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
							break;
						default:
							break;
					}



					c曲リストノード.nスコア数 = 1;
					c曲リストノード.arスコア = new Cスコア();
					c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス = infoDir.FullName + @"\";
					c曲リストノード.arスコア.譜面情報.タイトル = boxdef.Title;
					c曲リストノード.arスコア.譜面情報.ジャンル = boxdef.Genre;
					c曲リストノード.r親ノード = node親;
					c曲リストノード.Openindex = 1;

					c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード == null) ?
						c曲リストノード.strタイトル : c曲リストノード.r親ノード.strBreadcrumbs + " > " + c曲リストノード.strタイトル;


					c曲リストノード.list子リスト = new List<C曲リストノード>();
					listノードリスト.Add(c曲リストノード);
					if (b子BOXへ再帰する)
					{
						this.t曲を検索してリストを作成する(infoDir.FullName + @"\", b子BOXへ再帰する, c曲リストノード.list子リスト, c曲リストノード);
					}
				}
				//-----------------------------
				#endregion

				#region [ c.通常フォルダの場合 ]
				//-----------------------------
				else
				{
					this.t曲を検索してリストを作成する(infoDir.FullName + @"\", b子BOXへ再帰する, listノードリスト, node親);
				}
				//-----------------------------
				#endregion
			}
		}
		//-----------------
		#endregion
		#region [ スコアキャッシュを曲リストに反映する ]
		//-----------------
		public void tスコアキャッシュを曲リストに反映する()
		{
			this.nスコアキャッシュから反映できたスコア数 = 0;
			this.tスコアキャッシュを曲リストに反映する(this.list曲ルート);
		}
		private void tスコアキャッシュを曲リストに反映する(List<C曲リストノード> ノードリスト)
		{
			using (List<C曲リストノード>.Enumerator enumerator = ノードリスト.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

					C曲リストノード node = enumerator.Current;
					if (node.eノード種別 == C曲リストノード.Eノード種別.BOX)
					{
						this.tスコアキャッシュを曲リストに反映する(node.list子リスト);
					}
					else if ((node.eノード種別 == C曲リストノード.Eノード種別.SCORE))
					{
						Predicate<Cスコア> match = null;
						if (match == null)
						{
							match = delegate (Cスコア sc)
							{
								return
									(
								(sc.ファイル情報.ファイルの絶対パス.Equals(node.arスコア.ファイル情報.ファイルの絶対パス)
									&& sc.ファイル情報.ファイルサイズ.Equals(node.arスコア.ファイル情報.ファイルサイズ))
									&& (sc.ファイル情報.最終更新日時.Equals(node.arスコア.ファイル情報.最終更新日時)
									&& sc.ScoreIni情報.ファイルサイズ.Equals(node.arスコア.ScoreIni情報.ファイルサイズ)))
									&& sc.ScoreIni情報.最終更新日時.Equals(node.arスコア.ScoreIni情報.最終更新日時);
							};
						}
						int nMatched = this.listSongsDB.FindIndex(match);
						if (nMatched == -1)
						{
							//Trace.TraceInformation( "songs.db に存在しません。({0})", node.arスコア[ lv ].ファイル情報.ファイルの絶対パス );
							if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
							{
								Trace.TraceInformation("songs.db に存在しません。({0})", node.arスコア.ファイル情報.ファイルの絶対パス);
							}
						}
						else
						{
							node.arスコア.譜面情報 = this.listSongsDB[nMatched].譜面情報;
							node.arスコア.bSongDBにキャッシュがあった = true;
							if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
							{
								Trace.TraceInformation("songs.db から転記しました。({0})", node.arスコア.ファイル情報.ファイルの絶対パス);
							}
							this.nスコアキャッシュから反映できたスコア数++;
						}
					}
				}
			}
		}
		private Cスコア tSongsDBからスコアを１つ読み込む(BinaryReader br)
		{
			Cスコア cスコア = new Cスコア();
			cスコア.ファイル情報.ファイルの絶対パス = br.ReadString();
			cスコア.ファイル情報.フォルダの絶対パス = br.ReadString();
			cスコア.ファイル情報.最終更新日時 = new DateTime(br.ReadInt64());
			cスコア.ファイル情報.ファイルサイズ = br.ReadInt64();
			cスコア.ScoreIni情報.最終更新日時 = new DateTime(br.ReadInt64());
			cスコア.ScoreIni情報.ファイルサイズ = br.ReadInt64();
			cスコア.譜面情報.タイトル = br.ReadString();
			cスコア.譜面情報.アーティスト名 = br.ReadString();
			cスコア.譜面情報.コメント = br.ReadString();
			cスコア.譜面情報.ジャンル = br.ReadString();
			cスコア.譜面情報.Backgound = br.ReadString();
			cスコア.譜面情報.最大ランク = br.ReadInt32();
			cスコア.譜面情報.最大スキル = br.ReadDouble();
			cスコア.譜面情報.フルコンボ = br.ReadBoolean();
			cスコア.譜面情報.演奏回数 = br.ReadInt32();
			cスコア.譜面情報.演奏履歴.行1 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行2 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行3 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行4 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行5 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行6 = br.ReadString();
			cスコア.譜面情報.演奏履歴.行7 = br.ReadString();
			cスコア.譜面情報.Bpm = br.ReadDouble();
			cスコア.譜面情報.Duration = br.ReadInt32();
			cスコア.譜面情報.strBGMファイル名 = br.ReadString();
			cスコア.譜面情報.SongVol = br.ReadInt32();
			var hasSongIntegratedLoudness = br.ReadBoolean();
			var songIntegratedLoudness = br.ReadDouble();
			var integratedLoudness = hasSongIntegratedLoudness ? new Lufs(songIntegratedLoudness) : default(Lufs?);
			var hasSongPeakLoudness = br.ReadBoolean();
			var songPeakLoudness = br.ReadDouble();
			var peakLoudness = hasSongPeakLoudness ? new Lufs(songPeakLoudness) : default(Lufs?);
			var songLoudnessMetadata = hasSongIntegratedLoudness
				? new LoudnessMetadata(integratedLoudness.Value, peakLoudness)
				: default(LoudnessMetadata?);
			cスコア.譜面情報.SongLoudnessMetadata = songLoudnessMetadata;
			cスコア.譜面情報.nデモBGMオフセット = br.ReadInt32();
			cスコア.譜面情報.b譜面分岐[0] = br.ReadBoolean();//2020.04.19 Mr-Ojii
			cスコア.譜面情報.b譜面分岐[1] = br.ReadBoolean();//ここら辺の配列になってるやつscore.iniに出力する方とdbに出力する方で
			cスコア.譜面情報.b譜面分岐[2] = br.ReadBoolean();//配列の数が違うから、[0]~[6]で統一。
			cスコア.譜面情報.b譜面分岐[3] = br.ReadBoolean();
			cスコア.譜面情報.b譜面分岐[4] = br.ReadBoolean();
			cスコア.譜面情報.b譜面分岐[5] = br.ReadBoolean();
			cスコア.譜面情報.b譜面分岐[6] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[0] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[1] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[2] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[3] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[4] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[5] = br.ReadBoolean();
			cスコア.譜面情報.bPapaMamaSupport[6] = br.ReadBoolean();
			cスコア.譜面情報.nハイスコア[0] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[1] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[2] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[3] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[4] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[5] = br.ReadInt32();
			cスコア.譜面情報.nハイスコア[6] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[0] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[1] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[2] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[3] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[4] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[5] = br.ReadInt32();
			cスコア.譜面情報.nSecondScore[6] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[0] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[1] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[2] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[3] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[4] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[5] = br.ReadInt32();
			cスコア.譜面情報.nThirdScore[6] = br.ReadInt32();
			cスコア.譜面情報.strHiScorerName[0] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[1] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[2] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[3] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[4] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[5] = br.ReadString();
			cスコア.譜面情報.strHiScorerName[6] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[0] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[1] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[2] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[3] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[4] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[5] = br.ReadString();
			cスコア.譜面情報.strSecondScorerName[6] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[0] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[1] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[2] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[3] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[4] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[5] = br.ReadString();
			cスコア.譜面情報.strThirdScorerName[6] = br.ReadString();
			cスコア.譜面情報.strサブタイトル = br.ReadString();
			cスコア.譜面情報.nレベル[0] = br.ReadInt32();
			cスコア.譜面情報.nレベル[1] = br.ReadInt32();
			cスコア.譜面情報.nレベル[2] = br.ReadInt32();
			cスコア.譜面情報.nレベル[3] = br.ReadInt32();
			cスコア.譜面情報.nレベル[4] = br.ReadInt32();
			cスコア.譜面情報.nレベル[5] = br.ReadInt32();
			cスコア.譜面情報.nレベル[6] = br.ReadInt32();
			cスコア.譜面情報.n王冠[0] = br.ReadInt32();
			cスコア.譜面情報.n王冠[1] = br.ReadInt32();
			cスコア.譜面情報.n王冠[2] = br.ReadInt32();
			cスコア.譜面情報.n王冠[3] = br.ReadInt32();
			cスコア.譜面情報.n王冠[4] = br.ReadInt32();
			cスコア.譜面情報.n王冠[5] = br.ReadInt32();
			cスコア.譜面情報.n王冠[6] = br.ReadInt32();
			cスコア.譜面情報.b譜面が存在する[0] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[1] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[2] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[3] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[4] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[5] = br.ReadBoolean();
			cスコア.譜面情報.b譜面が存在する[6] = br.ReadBoolean();
			cスコア.譜面情報.b歌詞あり = br.ReadBoolean();


			//Debug.WriteLine( "songs.db: " + cスコア.ファイル情報.ファイルの絶対パス );
			return cスコア;
		}
		//-----------------
		#endregion
		#region [ SongsDBになかった曲をファイルから読み込んで反映する ]
		//-----------------
		public void tSongsDBになかった曲をファイルから読み込んで反映する()
		{
			this.nファイルから反映できたスコア数 = 0;
			this.tSongsDBになかった曲をファイルから読み込んで反映する(this.list曲ルート);
		}
		private void tSongsDBになかった曲をファイルから読み込んで反映する(List<C曲リストノード> ノードリスト)
		{
			foreach (C曲リストノード c曲リストノード in ノードリスト)
			{
				SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

				if (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)
				{
					this.tSongsDBになかった曲をファイルから読み込んで反映する(c曲リストノード.list子リスト);
				}
				else if ((c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.SCORE))
				{
					for (int i = 0; i < (int)Difficulty.Total; i++)
					{
						if ((c曲リストノード.arスコア.譜面情報.b譜面が存在する[i] != false) && !c曲リストノード.arスコア.bSongDBにキャッシュがあった)
						{
							#region [ DTX ファイルのヘッダだけ読み込み、Cスコア.譜面情報 を設定する ]
							//-----------------
							string path = c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス;
							if (File.Exists(path))
							{
								try
								{
									CDTX cdtx = new CDTX(c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス, true, 0, 0);

									c曲リストノード.arスコア.譜面情報.タイトル = cdtx.TITLE;


									c曲リストノード.arスコア.譜面情報.アーティスト名 = cdtx.ARTIST;
									c曲リストノード.arスコア.譜面情報.コメント = cdtx.COMMENT;
									c曲リストノード.arスコア.譜面情報.ジャンル = cdtx.GENRE;
									c曲リストノード.arスコア.譜面情報.Backgound = ((cdtx.BACKGROUND != null) && (cdtx.BACKGROUND.Length > 0)) ? cdtx.BACKGROUND : "";
									c曲リストノード.arスコア.譜面情報.Bpm = cdtx.BPM;
									c曲リストノード.arスコア.譜面情報.Duration = 0;   //  (cdtx.listChip == null)? 0 : cdtx.listChip[ cdtx.listChip.Count - 1 ].n発声時刻ms;
									c曲リストノード.arスコア.譜面情報.strBGMファイル名 = cdtx.strBGM_PATH == null ? "" : cdtx.strBGM_PATH;//ここのnullをどうにか--------
									c曲リストノード.arスコア.譜面情報.SongVol = cdtx.SongVol;
									c曲リストノード.arスコア.譜面情報.SongLoudnessMetadata = cdtx.SongLoudnessMetadata;
									c曲リストノード.arスコア.譜面情報.nデモBGMオフセット = cdtx.nデモBGMオフセット;
									c曲リストノード.arスコア.譜面情報.b譜面分岐[0] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[0];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[1] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[1];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[2] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[2];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[3] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[3];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[4] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[4];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[5] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[5];
									c曲リストノード.arスコア.譜面情報.b譜面分岐[6] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[6];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[0] = cdtx.bPapaMamaSupport[0];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[1] = cdtx.bPapaMamaSupport[1];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[2] = cdtx.bPapaMamaSupport[2];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[3] = cdtx.bPapaMamaSupport[3];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[4] = cdtx.bPapaMamaSupport[4];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[5] = cdtx.bPapaMamaSupport[5];
									c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[6] = cdtx.bPapaMamaSupport[6];
									c曲リストノード.arスコア.譜面情報.strサブタイトル = cdtx.SUBTITLE;
									c曲リストノード.arスコア.譜面情報.nレベル[0] = cdtx.LEVELtaiko[0];
									c曲リストノード.arスコア.譜面情報.nレベル[1] = cdtx.LEVELtaiko[1];
									c曲リストノード.arスコア.譜面情報.nレベル[2] = cdtx.LEVELtaiko[2];
									c曲リストノード.arスコア.譜面情報.nレベル[3] = cdtx.LEVELtaiko[3];
									c曲リストノード.arスコア.譜面情報.nレベル[4] = cdtx.LEVELtaiko[4];
									c曲リストノード.arスコア.譜面情報.nレベル[5] = cdtx.LEVELtaiko[5];
									c曲リストノード.arスコア.譜面情報.nレベル[6] = cdtx.LEVELtaiko[6];
									c曲リストノード.arスコア.譜面情報.b歌詞あり = cdtx.bLyrics;
									this.nファイルから反映できたスコア数++;
									cdtx.On非活性化();
									//Debug.WriteLine( "★" + this.nファイルから反映できたスコア数 + " " + c曲リストノード.arスコア[ i ].譜面情報.タイトル );
									#region [ 曲検索ログ出力 ]
									//-----------------
									if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
									{
										StringBuilder sb = new StringBuilder(0x400);
										sb.Append(string.Format("曲データファイルから譜面情報を転記しました。({0})", path));
										sb.Append("(title=" + c曲リストノード.arスコア.譜面情報.タイトル);
										sb.Append(", artist=" + c曲リストノード.arスコア.譜面情報.アーティスト名);
										sb.Append(", comment=" + c曲リストノード.arスコア.譜面情報.コメント);
										sb.Append(", genre=" + c曲リストノード.arスコア.譜面情報.ジャンル);
										sb.Append(", background=" + c曲リストノード.arスコア.譜面情報.Backgound);
										sb.Append(", bpm=" + c曲リストノード.arスコア.譜面情報.Bpm);
										sb.Append(", lyrics=" + c曲リストノード.arスコア.譜面情報.b歌詞あり);
										Trace.TraceInformation(sb.ToString());
									}
									//-----------------
									#endregion
								}
								catch (Exception exception)
								{
									Trace.TraceError(exception.ToString());
									c曲リストノード.arスコア = null;
									c曲リストノード.nスコア数--;
									this.n検索されたスコア数--;
									Trace.TraceError("曲データファイルの読み込みに失敗しました。({0})", path);
								}
							}
							//-----------------
							#endregion

							#region [ 対応する .score.ini が存在していれば読み込み、Cスコア.譜面情報 に追加設定する ]
							//-----------------
							try
							{
								var scoreIniPath = c曲リストノード.arスコア.ファイル情報.ファイルの絶対パス + ".score.ini";
								if (File.Exists(scoreIniPath))
									this.tScoreIniを読み込んで譜面情報を設定する(scoreIniPath, c曲リストノード.arスコア);
								else
								{
									string[] dtxscoreini = Directory.GetFiles(c曲リストノード.arスコア.ファイル情報.フォルダの絶対パス, "*.dtx.score.ini");
									if (dtxscoreini.Length != 0 && File.Exists(dtxscoreini[0]))
									{
										this.tScoreIniを読み込んで譜面情報を設定する(dtxscoreini[0], c曲リストノード.arスコア);
									}
								}
							}
							catch (Exception e)
							{
								Trace.TraceError(e.ToString());
								Trace.TraceError("例外が発生しましたが処理を継続します。 (c8b6538c-46a1-403e-8cc3-fc7e7ff914fb)");
							}

							//-----------------
							#endregion
						}
					}
				}
			}
		}
		//-----------------
		#endregion
		#region [ 曲リストへ後処理を適用する ]
		//-----------------
		public void t曲リストへ後処理を適用する()
		{
			listStrBoxDefSkinSubfolderFullName = new List<string>();
			if (TJAPlayer3.Skin.strBoxDefSkinSubfolders != null)
			{
				foreach (string b in TJAPlayer3.Skin.strBoxDefSkinSubfolders)
				{
					listStrBoxDefSkinSubfolderFullName.Add(b);
				}
			}

			this.t曲リストへ後処理を適用する(this.list曲ルート);

			#region [ skin名で比較して、systemスキンとboxdefスキンに重複があれば、boxdefスキン側を削除する ]
			string[] systemSkinNames = CSkin.GetSkinName(TJAPlayer3.Skin.strSystemSkinSubfolders);
			List<string> l = new List<string>(listStrBoxDefSkinSubfolderFullName);
			foreach (string boxdefSkinSubfolderFullName in l)
			{
				if (Array.BinarySearch(systemSkinNames,
					CSkin.GetSkinName(boxdefSkinSubfolderFullName),
					StringComparer.InvariantCultureIgnoreCase) >= 0)
				{
					listStrBoxDefSkinSubfolderFullName.Remove(boxdefSkinSubfolderFullName);
				}
			}
			#endregion
			string[] ba = listStrBoxDefSkinSubfolderFullName.ToArray();
			Array.Sort(ba);
			TJAPlayer3.Skin.strBoxDefSkinSubfolders = ba;
		}
		private void t曲リストへ後処理を適用する(List<C曲リストノード> ノードリスト)
		{
			#region [ リストに１つ以上の曲があるなら RANDOM BOX を入れる ]

			//-----------------------------
			if (ノードリスト.Count > 0 && TJAPlayer3.ConfigIni.RandomPresence)
			{
				C曲リストノード itemRandom = new C曲リストノード();
				itemRandom.eノード種別 = C曲リストノード.Eノード種別.RANDOM;
				itemRandom.strタイトル = "ランダムに曲をえらぶ";
				itemRandom.nスコア数 = (int)Difficulty.Total;
				itemRandom.r親ノード = ノードリスト[0].r親ノード;

				itemRandom.strBreadcrumbs = (itemRandom.r親ノード == null) ?
					itemRandom.strタイトル : itemRandom.r親ノード.strBreadcrumbs + " > " + itemRandom.strタイトル;

				itemRandom.arスコア = new Cスコア();
				for (int i = 0; i < (int)Difficulty.Total; i++)
				{
					itemRandom.arスコア.譜面情報.タイトル = string.Format("< RANDOM SELECT Lv.{0} >", i + 1);
					itemRandom.arスコア.譜面情報.コメント =
						 (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja") ?
						 string.Format("難易度レベル {0} 付近の曲をランダムに選択します。難易度レベルを持たない曲も選択候補となります。", i + 1) :
						 string.Format("Random select from the songs which has the level about L{0}. Non-leveled songs may also selected.", i + 1);
					itemRandom.ar難易度ラベル[i] = string.Format("L{0}", i + 1);
					itemRandom.arスコア.譜面情報.b譜面が存在する[i] = true;
				}
				ノードリスト.Add(itemRandom);

				#region [ ログ出力 ]
				//-----------------------------
				if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
				{
					StringBuilder sb = new StringBuilder(0x100);
					sb.Append(string.Format("nID#{0:D3}", itemRandom.nID));
					if (itemRandom.r親ノード != null)
					{
						sb.Append(string.Format("(in#{0:D3}):", itemRandom.r親ノード.nID));
					}
					else
					{
						sb.Append("(onRoot):");
					}
					sb.Append(" RANDOM");
					Trace.TraceInformation(sb.ToString());
				}
				//-----------------------------
				#endregion
			}
			//-----------------------------
			#endregion

			// すべてのノードについて…
			foreach (C曲リストノード c曲リストノード in ノードリスト)
			{
				SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

				#region [ BOXノードなら子リストに <<BACK を入れ、子リストに後処理を適用する ]
				//-----------------------------
				if (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)
				{
					int 曲数 = c曲リストノード.list子リスト.Count;//for文に直接書くと、もどるもカウントされてしまう。
					for (int index = 0; index < ((曲数 - 1) / TJAPlayer3.ConfigIni.n閉じる差し込み間隔) + 2; index++)
					{
						C曲リストノード itemBack = new C曲リストノード();
						itemBack.eノード種別 = C曲リストノード.Eノード種別.BACKBOX;
						itemBack.strタイトル = "とじる";
						itemBack.nスコア数 = 1;
						itemBack.r親ノード = c曲リストノード;

						itemBack.strSkinPath = (c曲リストノード.r親ノード == null) ?
							"" : c曲リストノード.r親ノード.strSkinPath;

						if (itemBack.strSkinPath != "" && !listStrBoxDefSkinSubfolderFullName.Contains(itemBack.strSkinPath))
						{
							listStrBoxDefSkinSubfolderFullName.Add(itemBack.strSkinPath);
						}

						itemBack.strBreadcrumbs = (itemBack.r親ノード == null) ?
							itemBack.strタイトル : itemBack.r親ノード.strBreadcrumbs + " > " + itemBack.strタイトル;

						itemBack.arスコア = new Cスコア();
						itemBack.arスコア.ファイル情報.フォルダの絶対パス = "";
						itemBack.arスコア.譜面情報.タイトル = itemBack.strタイトル;
						itemBack.arスコア.譜面情報.コメント =
							(CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja") ?
							"BOX を出ます。" :
							"Exit from the BOX.";
						c曲リストノード.arスコア.譜面情報.b譜面が存在する[0] = true;
						c曲リストノード.list子リスト.Insert(Math.Min(index * (TJAPlayer3.ConfigIni.n閉じる差し込み間隔 + 1), c曲リストノード.list子リスト.Count), itemBack);


						#region [ ログ出力 ]
						//-----------------------------
						if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
						{
							StringBuilder sb = new StringBuilder(0x100);
							sb.Append(string.Format("nID#{0:D3}", itemBack.nID));
							if (itemBack.r親ノード != null)
							{
								sb.Append(string.Format("(in#{0:D3}):", itemBack.r親ノード.nID));
							}
							else
							{
								sb.Append("(onRoot):");
							}
							sb.Append(" BACKBOX");
							Trace.TraceInformation(sb.ToString());
						}
						//-----------------------------
						#endregion
					}

					this.t曲リストへ後処理を適用する(c曲リストノード.list子リスト);
					continue;
				}
				//-----------------------------
				#endregion

				#region [ ノードにタイトルがないなら、最初に見つけたスコアのタイトルを設定する ]
				//-----------------------------
				if (string.IsNullOrEmpty(c曲リストノード.strタイトル))
				{
					for (int j = 0; j < (int)Difficulty.Total; j++)
					{
						if ((c曲リストノード.arスコア.譜面情報.b譜面が存在する[j] != false) && !string.IsNullOrEmpty(c曲リストノード.arスコア.譜面情報.タイトル))
						{
							c曲リストノード.strタイトル = c曲リストノード.arスコア.譜面情報.タイトル;

							if (TJAPlayer3.ConfigIni.bLog曲検索ログ出力)
								Trace.TraceInformation("タイトルを設定しました。(nID#{0:D3}, title={1})", c曲リストノード.nID, c曲リストノード.strタイトル);

							break;
						}
					}
				}
				//-----------------------------
				#endregion
			}

			#region [ ノードをソートする ]
			//-----------------------------
			if (TJAPlayer3.ConfigIni.nDefaultSongSort == 0)
			{
				t曲リストのソート1_絶対パス順(ノードリスト);
			}
			else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 1)
			{
				t曲リストのソート9_ジャンル順(ノードリスト, 1, 0);
			}
			else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 2)
			{
				t曲リストのソート9_ジャンル順(ノードリスト, 2, 0);
			}
			//-----------------------------
			#endregion
		}
		//-----------------
		#endregion
		#region [ スコアキャッシュをSongsDBに出力する ]
		//-----------------
		public void tスコアキャッシュをSongsDBに出力する(string SongsDBファイル名)
		{
			this.nSongsDBへ出力できたスコア数 = 0;
			try
			{
				BinaryWriter bw = new BinaryWriter(new FileStream(SongsDBファイル名, FileMode.Create, FileAccess.Write));
				bw.Write(SONGSDB_VERSION);
				this.tSongsDBにリストを１つ出力する(bw, this.list曲ルート);
				bw.Close();
			}
			catch (Exception e)
			{
				Trace.TraceError("songs.dbの出力に失敗しました。");
				Trace.TraceError(e.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (ca70d133-f092-4351-8ebd-0906d8f1cffa)");
			}
		}
		private void tSongsDBにノードを１つ出力する(BinaryWriter bw, C曲リストノード node)
		{
			// ここではsuspendに応じないようにしておく(深い意味はない。ファイルの書き込みオープン状態を長時間維持したくないだけ)
			//if ( this.bIsSuspending )		// #27060 中断要求があったら、解除要求が来るまで待機
			//{
			//	autoReset.WaitOne();
			//}

			bw.Write(node.arスコア.ファイル情報.ファイルの絶対パス);
			bw.Write(node.arスコア.ファイル情報.フォルダの絶対パス);
			bw.Write(node.arスコア.ファイル情報.最終更新日時.Ticks);
			bw.Write(node.arスコア.ファイル情報.ファイルサイズ);
			bw.Write(node.arスコア.ScoreIni情報.最終更新日時.Ticks);
			bw.Write(node.arスコア.ScoreIni情報.ファイルサイズ);
			bw.Write(node.arスコア.譜面情報.タイトル);
			bw.Write(node.arスコア.譜面情報.アーティスト名);
			bw.Write(node.arスコア.譜面情報.コメント);
			bw.Write(node.arスコア.譜面情報.ジャンル);
			bw.Write(node.arスコア.譜面情報.Backgound);
			bw.Write(node.arスコア.譜面情報.最大ランク);
			bw.Write(node.arスコア.譜面情報.最大スキル);
			bw.Write(node.arスコア.譜面情報.フルコンボ);
			bw.Write(node.arスコア.譜面情報.演奏回数);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行1);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行2);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行3);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行4);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行5);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行6);
			bw.Write(node.arスコア.譜面情報.演奏履歴.行7);
			bw.Write(node.arスコア.譜面情報.Bpm);
			bw.Write(node.arスコア.譜面情報.Duration);
			bw.Write(node.arスコア.譜面情報.strBGMファイル名);
			bw.Write(node.arスコア.譜面情報.SongVol);
			var songLoudnessMetadata = node.arスコア.譜面情報.SongLoudnessMetadata;
			bw.Write(songLoudnessMetadata.HasValue);
			bw.Write(songLoudnessMetadata?.Integrated.ToDouble() ?? 0.0);
			bw.Write(songLoudnessMetadata?.TruePeak.HasValue ?? false);
			bw.Write(songLoudnessMetadata?.TruePeak?.ToDouble() ?? 0.0);
			bw.Write(node.arスコア.譜面情報.nデモBGMオフセット);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[0]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[1]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[2]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[3]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[4]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[5]);
			bw.Write(node.arスコア.譜面情報.b譜面分岐[6]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[0]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[1]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[2]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[3]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[4]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[5]);
			bw.Write(node.arスコア.譜面情報.bPapaMamaSupport[6]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[0]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[1]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[2]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[3]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[4]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[5]);
			bw.Write(node.arスコア.譜面情報.nハイスコア[6]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[0]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[1]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[2]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[3]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[4]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[5]);
			bw.Write(node.arスコア.譜面情報.nSecondScore[6]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[0]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[1]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[2]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[3]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[4]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[5]);
			bw.Write(node.arスコア.譜面情報.nThirdScore[6]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[0]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[1]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[2]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[3]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[4]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[5]);
			bw.Write(node.arスコア.譜面情報.strHiScorerName[6]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[0]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[1]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[2]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[3]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[4]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[5]);
			bw.Write(node.arスコア.譜面情報.strSecondScorerName[6]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[0]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[1]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[2]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[3]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[4]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[5]);
			bw.Write(node.arスコア.譜面情報.strThirdScorerName[6]);
			bw.Write(node.arスコア.譜面情報.strサブタイトル);
			bw.Write(node.arスコア.譜面情報.nレベル[0]);
			bw.Write(node.arスコア.譜面情報.nレベル[1]);
			bw.Write(node.arスコア.譜面情報.nレベル[2]);
			bw.Write(node.arスコア.譜面情報.nレベル[3]);
			bw.Write(node.arスコア.譜面情報.nレベル[4]);
			bw.Write(node.arスコア.譜面情報.nレベル[5]);
			bw.Write(node.arスコア.譜面情報.nレベル[6]);
			bw.Write(node.arスコア.譜面情報.n王冠[0]);
			bw.Write(node.arスコア.譜面情報.n王冠[1]);
			bw.Write(node.arスコア.譜面情報.n王冠[2]);
			bw.Write(node.arスコア.譜面情報.n王冠[3]);
			bw.Write(node.arスコア.譜面情報.n王冠[4]);
			bw.Write(node.arスコア.譜面情報.n王冠[5]);
			bw.Write(node.arスコア.譜面情報.n王冠[6]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[0]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[1]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[2]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[3]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[4]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[5]);
			bw.Write(node.arスコア.譜面情報.b譜面が存在する[6]);
			bw.Write(node.arスコア.譜面情報.b歌詞あり);
			this.nSongsDBへ出力できたスコア数++;
		}
		private void tSongsDBにリストを１つ出力する(BinaryWriter bw, List<C曲リストノード> list)
		{
			foreach (C曲リストノード c曲リストノード in list)
			{
				if ((c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.SCORE))
				{
					this.tSongsDBにノードを１つ出力する(bw, c曲リストノード);
				}
				if (c曲リストノード.list子リスト != null)
				{
					this.tSongsDBにリストを１つ出力する(bw, c曲リストノード.list子リスト);
				}
			}
		}
		//-----------------
		#endregion

		#region [ 曲リストソート ]
		//-----------------

		public static void t曲リストのソート1_絶対パス順(List<C曲リストノード> ノードリスト)
		{
			t曲リストのソート1_絶対パス順(ノードリスト, 1, 0);

			foreach (C曲リストノード c曲リストノード in ノードリスト)
			{
				if ((c曲リストノード.list子リスト != null) && (c曲リストノード.list子リスト.Count > 1))
				{
					t曲リストのソート1_絶対パス順(c曲リストノード.list子リスト);
				}
			}
		}

		public static void t曲リストのソート1_絶対パス順(List<C曲リストノード> ノードリスト, int order, params object[] p)
		{
			var comparer = new ComparerChain<C曲リストノード>(
				new C曲リストノードComparerノード種別(),
				new C曲リストノードComparer絶対パス(order),
				new C曲リストノードComparerタイトル(order));

			ノードリスト.Sort(comparer);
			tとじるノードの等間隔追加(ノードリスト);
		}

		public static void t曲リストのソート2_タイトル順(List<C曲リストノード> ノードリスト, int order, params object[] p)
		{
			var comparer = new ComparerChain<C曲リストノード>(
				new C曲リストノードComparerノード種別(),
				new C曲リストノードComparerタイトル(order),
				new C曲リストノードComparer絶対パス(order));

			ノードリスト.Sort(comparer);
			tとじるノードの等間隔追加(ノードリスト);
		}

		public static void t曲リストのソート9_ジャンル順(List<C曲リストノード> ノードリスト, int order, params object[] p)
		{
			try
			{
				var acGenreComparer = order == 1
					? (IComparer<C曲リストノード>)new C曲リストノードComparerAC8_14()
					: new C曲リストノードComparerAC15();

				var comparer = new ComparerChain<C曲リストノード>(
					new C曲リストノードComparerノード種別(),
					acGenreComparer,
					new C曲リストノードComparer絶対パス(1),
					new C曲リストノードComparerタイトル(1));

				ノードリスト.Sort(comparer);
				tとじるノードの等間隔追加(ノードリスト);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (bca6dda7-76ad-42fc-a415-250f52c0b17d)");
			}
		}

		public static void tとじるノードの等間隔追加(List<C曲リストノード> ノードリスト)
		{
			int 戻るノード数 = 0;
			for (int index = 0; index < ノードリスト.Count; index++)
			{
				if (ノードリスト[index].eノード種別 == C曲リストノード.Eノード種別.BACKBOX)
				{
					C曲リストノード tmp = new C曲リストノード();//今、入ってるBACKBOXを使いまわす。
					tmp = ノードリスト[index];
					ノードリスト.RemoveAt(index);
					ノードリスト.Insert(Math.Min((戻るノード数) * (TJAPlayer3.ConfigIni.n閉じる差し込み間隔 + 1), ノードリスト.Count), tmp);
					戻るノード数++;
				}
			}
		}

#if TEST_SORTBGM
		public static void t曲リストのソート9_BPM順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			order = -order;
			int nL12345 = (int) p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
		#region [ 共通処理 ]
					if ( n1 == n2 )
					{
						return 0;
					}
					int num = this.t比較0_共通( n1, n2 );
					if ( num != 0 )
					{
						return num;
					}
					if ( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
		#endregion
					double dBPMn1 = 0.0, dBPMn2 = 0.0;
					if ( n1.arスコア[ nL12345 ] != null )
					{
						dBPMn1 = n1.arスコア[ nL12345 ].譜面情報.bpm;
					}
					if ( n2.arスコア[ nL12345 ] != null )
					{
						dBPMn2 = n2.arスコア[ nL12345 ].譜面情報.bpm;
					}
					double d = dBPMn1- dBPMn2;
					if ( d != 0 )
					{
						return order * System.Math.Sign( d );
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					double dBPM = 0;
					if ( c曲リストノード.arスコア[ nL12345 ] != null )
					{
						dBPM = c曲リストノード.arスコア[ nL12345 ].譜面情報.bpm;
					}
Debug.WriteLine( dBPM + ":" + c曲リストノード.strタイトル );
				}
			}
		}
#endif
		//-----------------
		#endregion
		#region [ .score.ini を読み込んで Cスコア.譜面情報に設定する ]
		//-----------------
		public void tScoreIniを読み込んで譜面情報を設定する(string strScoreIniファイルパス, Cスコア score)
		{
			if (!File.Exists(strScoreIniファイルパス))
				return;

			try
			{
				var ini = new CScoreIni(strScoreIniファイルパス);

				for (int n楽器番号 = 0; n楽器番号 < 1; n楽器番号++)
				{
					#region socre.譜面情報.最大ランク.Drums = ... 
					//-----------------
					if (ini.stセクション.HiScore.b演奏にMIDI入力を使用した ||
						ini.stセクション.HiScore.b演奏にキーボードを使用した ||
						ini.stセクション.HiScore.b演奏にジョイパッドを使用した ||
						ini.stセクション.HiScore.b演奏にマウスを使用した)
					{
						// (A) 全オートじゃないようなので、演奏結果情報を有効としてランクを算出する。

						score.譜面情報.最大ランク =
							CScoreIni.tランク値を計算して返す(
								ini.stセクション.HiScore.n全チップ数,
								ini.stセクション.HiScore.nPerfect数,
								ini.stセクション.HiScore.nGreat数,
								ini.stセクション.HiScore.nGood数,
								ini.stセクション.HiScore.nPoor数,
								ini.stセクション.HiScore.nMiss数);
					}
					else
					{
						// (B) 全オートらしいので、ランクは無効とする。

						score.譜面情報.最大ランク = (int)CScoreIni.ERANK.UNKNOWN;
					}
					//-----------------
					#endregion
					score.譜面情報.最大スキル = ini.stセクション.HiScore.db演奏型スキル値;
					score.譜面情報.フルコンボ = ini.stセクション.HiScore.bフルコンボである;
					for (int i = 0; i < (int)Difficulty.Total; i++)
					{
						score.譜面情報.nハイスコア[i] = (int)ini.stセクション.HiScore.nハイスコア[i];
						score.譜面情報.nSecondScore[i] = (int)ini.stセクション.HiScore.nSecondScore[i];
						score.譜面情報.nThirdScore[i] = (int)ini.stセクション.HiScore.nThirdScore[i];
						score.譜面情報.strHiScorerName[i] = ini.stセクション.HiScore.strHiScorerName[i];
						score.譜面情報.strSecondScorerName[i] = ini.stセクション.HiScore.strSecondScorerName[i];
						score.譜面情報.strThirdScorerName[i] = ini.stセクション.HiScore.strThirdScorerName[i];
						score.譜面情報.n王冠[i] = (int)ini.stセクション.HiScore.n王冠[i];
					}
				}
				score.譜面情報.演奏回数 = ini.stファイル.PlayCountDrums;
				//for( int i = 0; i < (int)Difficulty.Total; i++ )
				//	score.譜面情報.演奏履歴[ i ] = ini.stファイル.History[ i ];//2020.04.18 Mr-Ojii ここで例外処理起こすしなぜこのコードが必要かがわからなかったので、コメントアウト化
			}
			catch (Exception e)
			{
				Trace.TraceError("演奏記録ファイルの読み込みに失敗しました。[{0}]", strScoreIniファイルパス);
				Trace.TraceError(e.ToString());
				Trace.TraceError("例外が発生しましたが処理を継続します。 (801f823d-a952-4809-a1bb-cf6a56194f5c)");
			}
		}
		//-----------------
		#endregion

		// その他

		#region [ private ]
		//-----------------
		private const string SONGSDB_VERSION = "SongsDB5-f";
		private List<string> listStrBoxDefSkinSubfolderFullName;

		/// <summary>
		/// 検索を中断_スローダウンする
		/// </summary>
		private void SlowOrSuspendSearchTask()
		{
			if (this.bIsSuspending)     // #27060 中断要求があったら、解除要求が来るまで待機
			{
				autoReset.WaitOne();
			}
		}

		//-----------------
		#endregion
	}
}

