using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FDK;
using FDK.ExtensionMethods;
using System.Drawing;
using System.Linq;
using Tomlyn;

namespace TJAPlayer3
{
	// グローバル定数

	public enum Eシステムサウンド : int
	{
		SOUNDカーソル移動音 = 0,
		SOUND決定音,
		SOUND変更音,
		SOUND取消音,
		SOUNDステージ失敗音,
		SOUNDゲーム開始音,
		SOUNDゲーム終了音,
		SOUND曲読込開始音,
		SOUNDタイトル音,
		BGM起動画面,
		BGMコンフィグ画面,
		BGM選曲画面,
		SOUND風船,
		SOUND曲決定音,
		SOUND成績発表,
		SOUNDDANするカッ,
		SOUND特訓再生,
		SOUND特訓停止,
		SOUND特訓スクロール,
		SOUND選曲スキップ,
		SOUND音色選択,
		SOUND難易度選択,
		SOUND自己ベスト更新,
		SOUND回転音,
		Count				// システムサウンド総数の計算用
	}

	internal class CSkin : IDisposable
	{
		// クラス

		public class Cシステムサウンド : IDisposable
		{
			// static フィールド

			public static CSkin.Cシステムサウンド r最後に再生した排他システムサウンド;

			private readonly ESoundGroup _soundGroup;

			// フィールド、プロパティ

			public bool bループ;
			public bool b読み込み未試行;
			public bool b読み込み成功;
			public bool b排他;
			public string strFilename = "";
			public bool b再生中
			{
				get
				{
					if (this.rSound[1 - this.n次に鳴るサウンド番号] == null)
						return false;

					return this.rSound[1 - this.n次に鳴るサウンド番号].bPlaying;
				}
			}
			public int n位置_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.nPanning;
				}
				set
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.nPanning = value;
				}
			}
			public int n位置_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.nPanning;
				}
				set
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.nPanning = value;
				}
			}
			public int nAutomationLevel_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.AutomationLevel;
				}
				set
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound != null)
					{
						sound.AutomationLevel = value;
					}
				}
			}
			public int n長さ_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
					{
						return 0;
					}
					return sound.nDurationms;
				}
			}
			public int n長さ_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
					{
						return 0;
					}
					return sound.nDurationms;
				}
			}


			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="strFilename"></param>
			/// <param name="bループ"></param>
			/// <param name="b排他"></param>
			/// <param name="bCompact対象"></param>
			public Cシステムサウンド(string strFilename, bool bループ, bool b排他, ESoundGroup soundGroup)
			{
				this.strFilename = strFilename;
				this.bループ = bループ;
				this.b排他 = b排他;
				_soundGroup = soundGroup;
				this.b読み込み未試行 = true;
			}


			// メソッド

			public void tLoad()
			{
				this.b読み込み未試行 = false;
				this.b読み込み成功 = false;
				if (string.IsNullOrEmpty(this.strFilename))
					throw new InvalidOperationException("ファイル名が無効です。");

				if (!File.Exists(CSkin.Path(this.strFilename)))
				{
					Trace.TraceWarning($"ファイルが存在しません。: {this.strFilename}");
					return;
				}
				if (TJAPlayer3.SoundManager == null)
					throw new Exception("SoundManagerがnullのため、サウンド生成ができません。");
				for (int i = 0; i < 2; i++)     // 一旦Cloneを止めてASIO対応に専念
				{
					try
					{
						this.rSound[i] = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(this.strFilename), _soundGroup);
					}
					catch
					{
						this.rSound[i] = null;
						throw;
					}
				}
				this.b読み込み成功 = true;
			}
			public void t再生する()
			{
				if (this.b読み込み未試行)
				{
					try
					{
						tLoad();
					}
					catch (Exception e)
					{
						Trace.TraceError(e.ToString());
						Trace.TraceError("An exception has occurred, but processing continues. (17668977-4686-4aa7-b3f0-e0b9a44975b8)");
						this.b読み込み未試行 = false;
					}
				}
				if (this.b排他)
				{
					if (r最後に再生した排他システムサウンド != null)
						r最後に再生した排他システムサウンド.t停止する();

					r最後に再生した排他システムサウンド = this;
				}
				CSound sound = this.rSound[this.n次に鳴るサウンド番号];
				if (sound != null)
					sound.t再生を開始する(this.bループ);

				this.n次に鳴るサウンド番号 = 1 - this.n次に鳴るサウンド番号;
			}
			public void t停止する()
			{
				if (this.rSound[0] != null)
					this.rSound[0].t再生を停止する();

				if (this.rSound[1] != null)
					this.rSound[1].t再生を停止する();

				if (r最後に再生した排他システムサウンド == this)
					r最後に再生した排他システムサウンド = null;
			}

			public void tRemoveMixer()
			{
				for (int i = 0; i < 2; i++)
				{
					if (this.rSound[i] != null)
					{
						TJAPlayer3.SoundManager.RemoveMixer(this.rSound[i]);
					}
				}
			}

			#region [ IDisposable 実装 ]
			//-----------------
			public void Dispose()
			{
				if (!this.bDisposed済み)
				{
					for (int i = 0; i < 2; i++)
					{
						if (this.rSound[i] != null)
						{
							this.rSound[i].t解放する();
							this.rSound[i] = null;
						}
					}
					this.b読み込み成功 = false;
					this.bDisposed済み = true;
				}
			}
			//-----------------
			#endregion

			#region [ private ]
			//-----------------
			private bool bDisposed済み;
			private int n次に鳴るサウンド番号;
			private CSound[] rSound = new CSound[2];
			//-----------------
			#endregion
		}

		private struct SystemSoundInfo
		{
			public SystemSoundInfo(string strFilePath, bool bLoop, bool bExclusive, ESoundGroup eSoundGroup)
			{
				this.strFilePath = strFilePath;
				this.bLoop = bLoop;
				this.bExclusive = bExclusive;
				this.eSoundGroup = eSoundGroup;
			}
			public readonly string strFilePath;
			public readonly bool bLoop;
			public readonly bool bExclusive;
			public readonly ESoundGroup eSoundGroup;
		}

		// プロパティ
		public Dictionary<Eシステムサウンド, Cシステムサウンド> SystemSounds = new Dictionary<Eシステムサウンド, Cシステムサウンド>();
		private readonly Dictionary<Eシステムサウンド, SystemSoundInfo> SystemSoundsInfo = new Dictionary<Eシステムサウンド, SystemSoundInfo>()
		{
			{ Eシステムサウンド.SOUNDカーソル移動音, new SystemSoundInfo(@"Sounds/Move.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND決定音, new SystemSoundInfo(@"Sounds/Decide.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND変更音, new SystemSoundInfo(@"Sounds/Change.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND取消音, new SystemSoundInfo(@"Sounds/Cancel.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUNDステージ失敗音, new SystemSoundInfo(@"Sounds/Stage failed.ogg", false, true, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUNDゲーム開始音, new SystemSoundInfo(@"Sounds/Game start.ogg", false, false, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUNDゲーム終了音, new SystemSoundInfo(@"Sounds/Game end.ogg", false, true, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUND曲読込開始音, new SystemSoundInfo(@"Sounds/Now loading.ogg", false, true, ESoundGroup.Unknown) },
			{ Eシステムサウンド.SOUNDタイトル音, new SystemSoundInfo(@"Sounds/Title.ogg", false, true, ESoundGroup.SongPlayback) },
			{ Eシステムサウンド.BGM起動画面, new SystemSoundInfo(@"Sounds/Setup BGM.ogg", true, true, ESoundGroup.SongPlayback) },
			{ Eシステムサウンド.BGMコンフィグ画面, new SystemSoundInfo(@"Sounds/Config BGM.ogg", true, true, ESoundGroup.SongPlayback) },
			{ Eシステムサウンド.BGM選曲画面, new SystemSoundInfo(@"Sounds/Select BGM.ogg", true, true, ESoundGroup.SongPreview) },
			{ Eシステムサウンド.SOUND風船, new SystemSoundInfo(@"Sounds/balloon.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND曲決定音, new SystemSoundInfo(@"Sounds/SongDecide.ogg", false, false, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUND成績発表, new SystemSoundInfo(@"Sounds/ResultIn.ogg", false, false, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUNDDANするカッ, new SystemSoundInfo(@"Sounds/Dan_Select.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND特訓再生, new SystemSoundInfo(@"Sounds/Resume.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND特訓停止, new SystemSoundInfo(@"Sounds/Pause.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND特訓スクロール, new SystemSoundInfo(@"Sounds/Scroll.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND選曲スキップ, new SystemSoundInfo(@"Sounds/Skip.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND音色選択, new SystemSoundInfo(@"Sounds/Timbre.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND難易度選択, new SystemSoundInfo(@"Sounds/DifficultySelect.ogg", false, false, ESoundGroup.SoundEffect) },
			{ Eシステムサウンド.SOUND自己ベスト更新, new SystemSoundInfo(@"Sounds/NewRecord.ogg", false, false, ESoundGroup.Voice) },
			{ Eシステムサウンド.SOUND回転音, new SystemSoundInfo(@"Sounds/Rotate.ogg", false, false, ESoundGroup.SoundEffect) },
		};

		public readonly int nシステムサウンド数 = (int)Eシステムサウンド.Count;
		public Cシステムサウンド this[int index]
		{
			get
			{
				if (SystemSounds.TryGetValue((Eシステムサウンド)index, out var cSystemSound))
					return cSystemSound;
				return null;
			}
			private set
			{
				SystemSounds[(Eシステムサウンド)index] = value;
			}
		}


		public string strSystemSkinRoot = null;
		public string[] strSystemSkinSubfolders = null;     // List<string>だとignoreCaseな検索が面倒なので、配列に逃げる :-)

		private static string strSystemSkinSubfolderFullName;           // Config画面で設定されたスキン

		/// <summary>
		/// スキンパス名をフルパスで取得する
		/// </summary>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		/// <returns></returns>
		public string GetCurrentSkinSubfolderFullName(bool bFromUserConfig)
		{
			return strSystemSkinSubfolderFullName;
		}
		/// <summary>
		/// スキンパス名をフルパスで設定する
		/// </summary>
		/// <param name="value">スキンパス名</param>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		public void SetCurrentSkinSubfolderFullName(string value, bool bFromUserConfig)
		{
			strSystemSkinSubfolderFullName = value;
		}


		// コンストラクタ
		public CSkin(string _strSkinSubfolderFullName)
		{
			strSystemSkinSubfolderFullName = _strSkinSubfolderFullName;
			InitializeSkinPathRoot();
			ReloadSkinPaths();
			PrepareReloadSkin();
			SEloader();
			GenreLoader();
			SortLoader();
		}
		public CSkin()
		{
			InitializeSkinPathRoot();
			ReloadSkinPaths();
			PrepareReloadSkin();
			SEloader();
			GenreLoader();
			SortLoader();
		}
		private string InitializeSkinPathRoot()
		{
			strSystemSkinRoot = System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System/");
			return strSystemSkinRoot;
		}


		/// <summary>
		/// 音色用文字列の読み込み用
		/// </summary>
		public void SEloader()
		{
			this.SECount = TJAPlayer3.t連番フォルダの個数を数える(CSkin.Path(@"Sounds/Taiko/"));
			string strFilename = CSkin.Path(@"Sounds/Taiko/SElist.csv");

			if (!File.Exists(strFilename))
			{
				string[] splitstr = new string[this.SECount];
				for (int i = 0; i < this.SECount; i++)
					splitstr[i] = "無名";
				this.SENames = splitstr;
			}
			else
			{
				string str = CJudgeTextEncoding.ReadTextFile(strFilename);
				str = str.Replace(CJudgeTextEncoding.JudgeNewLine(str), "\n");
				str = str.Replace(',', '\n');
				string[] splitstr = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);

				if (splitstr.Length < this.SECount)//SEの数より配列数が少なかったとき
				{
					string[] splitstrtmp = new string[this.SECount];

					for (int i = 0; i < splitstrtmp.Length; i++)
					{
						if (i < splitstr.Length)
						{
							splitstrtmp[i] = splitstr[i];
						}
						else
						{
							splitstrtmp[i] = "無名";
						}
					}
					splitstr = splitstrtmp;
				}
				this.SENames = splitstr;
			}
		}

		/// <summary>
		/// ジャンルファイルの読み込み
		/// </summary>
		public void GenreLoader()
		{
			string strFileName = Path(@"GenreConfig.csv");
			if (File.Exists(strFileName))
			{
				Dictionary<string, int> tmp = new Dictionary<string, int>();
				string str = CJudgeTextEncoding.ReadTextFile(strFileName);
				string[] splitstr = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < splitstr.Length; i++)
				{
					string[] genres = splitstr[i].Split(',');
					for (int j = 0; j < genres.Length; j++)
					{
						tmp.Add(genres[j], i);
					}
				}

				if (tmp.Count != 0)
					this.GenreKeyPairs = tmp;
			}

			int max = -1;
			foreach (KeyValuePair<string, int> i in GenreKeyPairs)
			{
				max = Math.Max(i.Value, max);
			}
			this.MaxKeyNum = max;
		}

		/// <summary>
		/// ソート指定ファイルの読み込み
		/// </summary>
		public void SortLoader()
		{
			string strFileName = Path(@"SortConfig.ini");
			if (File.Exists(strFileName))
			{
				string str = CJudgeTextEncoding.ReadTextFile(strFileName);
				string[] splitstr = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, Dictionary<string, int>> tmpSortList = new();

                Dictionary<string, int> tmpDic = null;
				string tmpSectionName = null;

				foreach (string sstr in splitstr)
				{
					if (sstr.Length == 0 || sstr[0] == ';')
						continue;

					if (sstr[0] == '[')//セクション
					{
						if (!string.IsNullOrEmpty(tmpSectionName) && tmpDic != null && tmpDic.Count != 0)
							tmpSortList.Add(tmpSectionName, tmpDic);
						tmpDic = new Dictionary<string, int>();
						tmpSectionName = sstr.Substring(1, sstr.Length - 2); //最初と最後の2文字を消す。
					}
					else
					{
						if (tmpDic != null && sstr.IndexOf('=') != -1)
						{
                            string sKey = sstr.Substring(0, sstr.IndexOf('='));
                            string sValue = sstr.Substring(sstr.IndexOf('=') + 1, sstr.Length - sstr.IndexOf('=') - 1);
							if(int.TryParse(sValue, out int nValue))
								tmpDic.Add(sKey, nValue);
                        }

					}
                }
                if (!string.IsNullOrEmpty(tmpSectionName) && tmpDic != null && tmpDic.Count != 0)
                    tmpSortList.Add(tmpSectionName, tmpDic);

				if (tmpSortList.Count != 0)
				{
					this.SortList = tmpSortList;
				}
			}
		}

		public int nStrジャンルtoNum(string strGenre)
		{
			if (this.GenreKeyPairs.TryGetValue(strGenre, out int num))
				return (num + 1);
			else
				return 0;
		}

		/// <summary>
		/// Skin(Sounds)を再読込する準備をする(再生停止,Dispose,ファイル名再設定)。
		/// あらかじめstrSkinSubfolderを適切に設定しておくこと。
		/// その後、ReloadSkinPaths()を実行し、strSkinSubfolderの正当性を確認した上で、本メソッドを呼び出すこと。
		/// 本メソッド呼び出し後に、ReloadSkin()を実行することで、システムサウンドを読み込み直す。
		/// ReloadSkin()の内容は本メソッド内に含めないこと。起動時はReloadSkin()相当の処理をCEnumSongsで行っているため。
		/// </summary>
		public void PrepareReloadSkin()
		{
			Trace.TraceInformation("SkinPath設定: {0}",
				strSystemSkinSubfolderFullName
			);

			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (this[i] != null && this[i].b読み込み成功)
				{
					this[i].t停止する();
					this[i].Dispose();
				}
			}
			for (int i = 0; i < nシステムサウンド数; i++)
			{
				SystemSoundInfo info = SystemSoundsInfo[(Eシステムサウンド)i];
				this[i] = new Cシステムサウンド(info.strFilePath, info.bLoop, info.bExclusive, info.eSoundGroup);
			}

			ReloadSkin();
			tReadSkinConfig();
		}

		public void ReloadSkin()
		{
			for (int i = 0; i < nシステムサウンド数; i++)
			{
				Cシステムサウンド cシステムサウンド = this[i];
				if (cシステムサウンド.b排他)
				{
					continue;
				}

				try
				{
					cシステムサウンド.tLoad();
					Trace.TraceInformation("システムサウンドを読み込みました。({0})", cシステムサウンド.strFilename);
				}
				catch (FileNotFoundException e)
				{
					Trace.TraceWarning(e.ToString());
					Trace.TraceWarning("システムサウンドが存在しません。({0})", cシステムサウンド.strFilename);
				}
				catch (Exception e)
				{
					Trace.TraceWarning(e.ToString());
					Trace.TraceWarning("システムサウンドの読み込みに失敗しました。({0})", cシステムサウンド.strFilename);
				}

			}
		}


		/// <summary>
		/// Skinの一覧を再取得する。
		/// System/*****/Graphics (やSounds/) というフォルダ構成を想定している。
		/// もし再取得の結果、現在使用中のSkinのパス(strSystemSkinSubfloderFullName)が消えていた場合は、
		/// 以下の優先順位で存在確認の上strSystemSkinSubfolderFullNameを再設定する。
		/// 1. System/Default/
		/// 2. System/*****/ で最初にenumerateされたもの
		/// 3. System/ (従来互換)
		/// </summary>
		public void ReloadSkinPaths()
		{
			#region [ まず System/*** をenumerateする ]
			string[] tempSkinSubfolders = System.IO.Directory.GetDirectories(strSystemSkinRoot, "*");
			strSystemSkinSubfolders = new string[tempSkinSubfolders.Length];
			int size = 0;
			for (int i = 0; i < tempSkinSubfolders.Length; i++)
			{
				#region [ 検出したフォルダがスキンフォルダかどうか確認する]
				if (!bIsValid(tempSkinSubfolders[i]))
					continue;
				#endregion
				#region [ スキンフォルダと確認できたものを、strSkinSubfoldersに入れる ]
				// フォルダ名末尾に必ず\をつけておくこと。さもないとConfig読み出し側(必ず\をつける)とマッチできない
				if (tempSkinSubfolders[i][tempSkinSubfolders[i].Length - 1] != '/')
				{
					tempSkinSubfolders[i] += '/';
				}
				strSystemSkinSubfolders[size] = tempSkinSubfolders[i];
				Trace.TraceInformation("SkinPath検出: {0}", strSystemSkinSubfolders[size]);
				size++;
				#endregion
			}
			Trace.TraceInformation("SkinPath入力: {0}", strSystemSkinSubfolderFullName);
			Array.Resize(ref strSystemSkinSubfolders, size);
			Array.Sort(strSystemSkinSubfolders);    // BinarySearch実行前にSortが必要
			#endregion

			#region [ 次に、現在のSkinパスが存在するか調べる。あれば終了。]
			if (Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
				StringComparer.InvariantCultureIgnoreCase) >= 0)
				return;
			#endregion
			#region [ カレントのSkinパスが消滅しているので、以下で再設定する。]
			/// 以下の優先順位で現在使用中のSkinパスを再設定する。
			/// 1. System/Default/
			/// 2. System/*****/ で最初にenumerateされたもの
			/// 3. System/ (従来互換)
			#region [ System/Default/ があるなら、そこにカレントSkinパスを設定する]
			string tempSkinPath_default = System.IO.Path.Combine(strSystemSkinRoot, "Default/");
			if (Array.BinarySearch(strSystemSkinSubfolders, tempSkinPath_default,
				StringComparer.InvariantCultureIgnoreCase) >= 0)
			{
				strSystemSkinSubfolderFullName = tempSkinPath_default;
				return;
			}
			#endregion
			#region [ System/SkinFiles.*****/ で最初にenumerateされたものを、カレントSkinパスに再設定する ]
			if (strSystemSkinSubfolders.Length > 0)
			{
				strSystemSkinSubfolderFullName = strSystemSkinSubfolders[0];
				return;
			}
			#endregion
			#region [ System/ に、カレントSkinパスを再設定する。]
			strSystemSkinSubfolderFullName = strSystemSkinRoot;
			strSystemSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
			#endregion
			#endregion
		}

		// メソッド

		public static string Path(string strファイルの相対パス)
		{
			return System.IO.Path.Combine(strSystemSkinSubfolderFullName, strファイルの相対パス);
		}

		/// <summary>
		/// フルパス名を与えると、スキン名として、ディレクトリ名末尾の要素を返す
		/// 例: C:/foo/bar/ なら、barを返す
		/// </summary>
		/// <param name="skinpath">スキンが格納されたパス名(フルパス)</param>
		/// <returns>スキン名</returns>
		public static string GetSkinName(string skinPathFullName)
		{
			if (skinPathFullName != null)
			{
				if (skinPathFullName == "")     // 「box.defで未定義」用
					skinPathFullName = strSystemSkinSubfolderFullName;
				string[] tmp = skinPathFullName.Split('/');
				return tmp[tmp.Length - 2];     // ディレクトリ名の最後から2番目の要素がスキン名(最後の要素はnull。元stringの末尾が/なので。)
			}
			return null;
		}
		public static string[] GetSkinName(string[] skinPathFullNames)
		{
			string[] ret = new string[skinPathFullNames.Length];
			for (int i = 0; i < skinPathFullNames.Length; i++)
			{
				ret[i] = GetSkinName(skinPathFullNames[i]);
			}
			return ret;
		}


		public string GetSkinSubfolderFullNameFromSkinName(string skinName)
		{
			foreach (string s in strSystemSkinSubfolders)
			{
				if (GetSkinName(s) == skinName)
					return s;
			}
			return null;
		}

		/// <summary>
		/// スキンパス名が妥当かどうか
		/// (タイトル画像にアクセスできるかどうかで判定する)
		/// </summary>
		/// <param name="skinPathFullName">妥当性を確認するスキンパス(フルパス)</param>
		/// <returns>妥当ならtrue</returns>
		public bool bIsValid(string skinPathFullName)
		{
			string filePathTitle;
			filePathTitle = System.IO.Path.Combine(skinPathFullName, @"Graphics/1_Title/Background.png");
			return (File.Exists(filePathTitle));
		}


		public void tRemoveMixerAll()
		{
			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (this[i] != null && this[i].b読み込み成功)
				{
					this[i].t停止する();
					this[i].tRemoveMixer();
				}
			}

		}

		public void tReadSkinConfig()
		{
			var str = "";
			LoadSkinConfigFromFile(Path(@"SkinConfig.ini"), ref str);
			this.t文字列から読み込み(str);
			
			string strToml =  CJudgeTextEncoding.ReadTextFile(Path(@"SkinConfig.toml"));
			TomlModelOptions tomlModelOptions = new()
			{
				ConvertPropertyName = (x) => x,
				ConvertFieldName = (x) => x,
			};
			CSkinConfig cSC = Toml.ToModel<CSkinConfig>(strToml, null, tomlModelOptions);
			this.SkinConfig = cSC;
			Program.SkinName = this.SkinConfig.General.Name;
			Program.SkinCreator = this.SkinConfig.General.Creator;
			Program.SkinVersion = this.SkinConfig.General.Version;
			CFontRenderer.SetRotate_Chara_List_Vertical(this.SkinConfig.SongSelect.RotateChara);
			CFontRenderer.SetTextCorrectionX_Chara_List_Vertical(this.SkinConfig.SongSelect.CorrectionXChara);
			CFontRenderer.SetTextCorrectionY_Chara_List_Vertical(this.SkinConfig.SongSelect.CorrectionYChara);
			CFontRenderer.SetTextCorrectionX_Chara_List_Value_Vertical(this.SkinConfig.SongSelect.CorrectionXCharaValue);
			CFontRenderer.SetTextCorrectionY_Chara_List_Value_Vertical(this.SkinConfig.SongSelect.CorrectionYCharaValue);

			void LoadSkinConfigFromFile(string path, ref string work)
			{
				if (!File.Exists(Path(path))) return;
				Encoding enc = CJudgeTextEncoding.JudgeFileEncoding(Path(path));
				using (var streamReader = new StreamReader(Path(path), enc))
				{
					while (streamReader.Peek() > -1) // 一行ずつ読み込む。
					{
						var nowLine = streamReader.ReadLine();
						if (nowLine.StartsWith("#include"))
						{
							// #include hogehoge.iniにぶち当たった
							var includePath = nowLine.Substring("#include ".Length).Trim();
							LoadSkinConfigFromFile(includePath, ref work); // 再帰的に読み込む
						}
						else
						{
							work += nowLine + "\n";
						}
					}
				}
			}
		}

		private void t文字列から読み込み(string strAllSettings)  // 2011.4.13 yyagi; refactored to make initial KeyConfig easier.
		{
			string[] delimiter = { "\n" };
			string[] strSingleLine = strAllSettings.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in strSingleLine)
			{
				string str = s.Replace('\t', ' ').TrimStart(new char[] { '\t', ' ' });
				if ((str.Length != 0) && (str[0] != ';'))
				{
					try
					{
						string strCommand;
						string strParam;
						string[] strArray = str.Split(new char[] { '=' });
						if (strArray.Length == 2)
						{
							strCommand = strArray[0].Trim();
							strParam = strArray[1].Trim();

							#region スキン設定

							void ParseInt32(Action<int> setValue)
							{
								if (int.TryParse(strParam, out var unparsedValue))
								{
									setValue(unparsedValue);
								}
								else
								{
									Trace.TraceWarning($"SkinConfigの値 {strCommand} は整数値である必要があります。現在の値: {strParam}");
								}
							}
							#endregion

							#region 背景(スクロール)
							if (strCommand == nameof(Background_Scroll_Y))
							{
								this.Background_Scroll_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion

							#region[ 演奏 ]
							//-----------------------------
							else if (strCommand == "ScrollFieldP1Y")
							{
								this.nScrollFieldY[0] = int.Parse(strParam);
							}
							else if (strCommand == "ScrollFieldP2Y")
							{
								this.nScrollFieldY[1] = int.Parse(strParam);
							}
							else if (strCommand == "SENotesP1Y")
							{
								this.nSENotesY[0] = int.Parse(strParam);
							}
							else if (strCommand == "SENotesP2Y")
							{
								this.nSENotesY[1] = int.Parse(strParam);
							}
							else if (strCommand == "JudgePointP1Y")
							{
								this.nJudgePointY[0] = int.Parse(strParam);
							}
							else if (strCommand == "JudgePointP2Y")
							{
								this.nJudgePointY[1] = int.Parse(strParam);
							}
							else if (strCommand == "NowStageDisp")
							{
								this.b現在のステージ数を表示しない = strParam[0].ToBool();
							}

							//-----------------------------
							#endregion

							#region 新・SkinConfig
							#region SongSelect
							else if (strCommand == nameof(SongSelect_ForeColor))
							{
								SongSelect_ForeColor = strParam.Split(',').Select(ColorTranslator.FromHtml).ToArray();
							}
							else if (strCommand == nameof(SongSelect_BackColor))
							{
								SongSelect_BackColor = strParam.Split(',').Select(ColorTranslator.FromHtml).ToArray();
							}

							#endregion
							#region SongLoading
							else if (strCommand == nameof(SongLoading_Plate_ReferencePoint))
							{
								SongLoading_Plate_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_ReferencePoint))
							{
								SongLoading_Title_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_ReferencePoint))
							{
								SongLoading_SubTitle_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_ForeColor))
							{
								SongLoading_Title_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_BackColor))
							{
								SongLoading_Title_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_ForeColor))
							{
								SongLoading_SubTitle_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_BackColor))
							{
								SongLoading_SubTitle_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Plate_X))
							{
								SongLoading_v2_Plate_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Plate_Y))
							{
								SongLoading_v2_Plate_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Title_X))
							{
								SongLoading_v2_Title_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Title_Y))
							{
								SongLoading_v2_Title_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_SubTitle_X))
							{
								SongLoading_v2_SubTitle_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_SubTitle_Y))
							{
								SongLoading_v2_SubTitle_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Plate_ReferencePoint))
							{
								SongLoading_v2_Plate_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_Title_ReferencePoint))
							{
								SongLoading_v2_Title_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_v2_SubTitle_ReferencePoint))
							{
								SongLoading_v2_SubTitle_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							#endregion
							#region Game
							else if (strCommand == nameof(Game_RollColorMode))
							{
								Game_RollColorMode = (RollColorMode)int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_JudgeFrame_AddBlend))
							{
								Game_JudgeFrame_AddBlend = strParam[0].ToBool();
							}

							#region CourseSymbol
							else if (strCommand == nameof(Game_CourseSymbol_X))
							{
								Game_CourseSymbol_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_CourseSymbol_Y))
							{
								Game_CourseSymbol_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion
							#region PanelFont
							else if (strCommand == nameof(Game_MusicName_X))
							{
								Game_MusicName_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_MusicName_Y))
							{
								Game_MusicName_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_MusicName_FontSize))
							{
								if (int.Parse(strParam) > 0)
									Game_MusicName_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_MusicName_ReferencePoint))
							{
								Game_MusicName_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_SubTitleName_X))
							{
								Game_SubTitleName_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_SubTitleName_Y))
							{
								Game_SubTitleName_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_SubTitleName_FontSize))
							{
								if (int.Parse(strParam) > 0)
									Game_SubTitleName_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_MusicName_ReferencePoint))
							{
								Game_SubTitleName_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Genre_X))
							{
								Game_Genre_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Genre_Y))
							{
								Game_Genre_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_X))
							{
								Game_Lyric_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_Y))
							{
								Game_Lyric_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_FontName))
							{
								Game_Lyric_FontName = strParam;
							}
							else if (strCommand == nameof(Game_Lyric_FontSize))
							{
								if (int.Parse(strParam) > 0)
									Game_Lyric_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_ReferencePoint))
							{
								Game_Lyric_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}

							else if (strCommand == nameof(Game_MusicName_ForeColor))
							{
								Game_MusicName_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_StageText_ForeColor))
							{
								Game_StageText_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_ForeColor))
							{
								Game_Lyric_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_MusicName_BackColor))
							{
								Game_MusicName_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_StageText_BackColor))
							{
								Game_StageText_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_Lyric_BackColor))
							{
								Game_Lyric_BackColor = ColorTranslator.FromHtml(strParam);
							}
							#endregion
							#region Score
							else if (strCommand == nameof(Game_Score_X))
							{
								this.Game_Score_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_Y))
							{
								this.Game_Score_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_Add_X))
							{
								this.Game_Score_Add_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_Add_Y))
							{
								this.Game_Score_Add_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_AddBonus_X))
							{
								this.Game_Score_AddBonus_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_AddBonus_Y))
							{
								this.Game_Score_AddBonus_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Score_Padding))
							{
								ParseInt32(value => Game_Score_Padding = value);
							}
							else if (strCommand == nameof(Game_Score_Size))
							{
								this.Game_Score_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion
							#region Taiko
							else if (strCommand == nameof(Game_Taiko_NamePlate_X))
							{
								this.Game_Taiko_NamePlate_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_NamePlate_Y))
							{
								this.Game_Taiko_NamePlate_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_PlayerNumber_X))
							{
								this.Game_Taiko_PlayerNumber_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_PlayerNumber_Y))
							{
								this.Game_Taiko_PlayerNumber_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_X))
							{
								this.Game_Taiko_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Y))
							{
								this.Game_Taiko_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_X))
							{
								this.Game_Taiko_Combo_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Y))
							{
								this.Game_Taiko_Combo_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Ex_X))
							{
								this.Game_Taiko_Combo_Ex_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Ex_Y))
							{
								this.Game_Taiko_Combo_Ex_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Ex4_X))
							{
								this.Game_Taiko_Combo_Ex4_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Ex4_Y))
							{
								this.Game_Taiko_Combo_Ex4_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Padding))
							{
								this.Game_Taiko_Combo_Padding = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Size))
							{
								this.Game_Taiko_Combo_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Size_Ex))
							{
								this.Game_Taiko_Combo_Size_Ex = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Scale))
							{
								this.Game_Taiko_Combo_Scale = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Text_X))
							{
								this.Game_Taiko_Combo_Text_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Text_Y))
							{
								this.Game_Taiko_Combo_Text_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Text_Size))
							{
								this.Game_Taiko_Combo_Text_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Taiko_Combo_Ex_IsJumping))
							{
								Game_Taiko_Combo_Ex_IsJumping = strParam[0].ToBool();
							}
							#endregion
							#region Gauge
							else if (strCommand == nameof(Game_Gauge_Rainbow_Timer))
							{
								if (int.Parse(strParam) != 0)
									Game_Gauge_Rainbow_Timer = int.Parse(strParam);
							}
							#endregion
							#region Balloon
							else if (strCommand == nameof(Game_Balloon_Combo_X))
							{
								this.Game_Balloon_Combo_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Y))
							{
								this.Game_Balloon_Combo_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Number_X))
							{
								this.Game_Balloon_Combo_Number_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Number_Y))
							{
								this.Game_Balloon_Combo_Number_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Number_Ex_X))
							{
								this.Game_Balloon_Combo_Number_Ex_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Number_Ex_Y))
							{
								this.Game_Balloon_Combo_Number_Ex_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Text_X))
							{
								this.Game_Balloon_Combo_Text_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Text_Y))
							{
								this.Game_Balloon_Combo_Text_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Text_Ex_X))
							{
								this.Game_Balloon_Combo_Text_Ex_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Combo_Text_Ex_Y))
							{
								this.Game_Balloon_Combo_Text_Ex_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_Balloon_Balloon_X))
							{
								this.Game_Balloon_Balloon_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Y))
							{
								this.Game_Balloon_Balloon_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Frame_X))
							{
								this.Game_Balloon_Balloon_Frame_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Frame_Y))
							{
								this.Game_Balloon_Balloon_Frame_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Number_X))
							{
								this.Game_Balloon_Balloon_Number_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Number_Y))
							{
								this.Game_Balloon_Balloon_Number_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_Balloon_Roll_Frame_X))
							{
								this.Game_Balloon_Roll_Frame_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Roll_Frame_Y))
							{
								this.Game_Balloon_Roll_Frame_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Roll_Number_X))
							{
								this.Game_Balloon_Roll_Number_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Roll_Number_Y))
							{
								this.Game_Balloon_Roll_Number_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Number_Size))
							{
								this.Game_Balloon_Number_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Balloon_Number_Padding))
							{
								ParseInt32(value => Game_Balloon_Number_Padding = value);
							}
							else if (strCommand == nameof(Game_Balloon_Roll_Number_Scale))
							{
								ParseInt32(value => Game_Balloon_Roll_Number_Scale = value);
							}
							else if (strCommand == nameof(Game_Balloon_Balloon_Number_Scale))
							{
								ParseInt32(value => Game_Balloon_Balloon_Number_Scale = value);
							}

							#endregion
							#region Effects
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_X))
							{
								Game_Effect_Roll_StartPoint_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_Y))
							{
								Game_Effect_Roll_StartPoint_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_1P_X))
							{
								Game_Effect_Roll_StartPoint_1P_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_1P_Y))
							{
								Game_Effect_Roll_StartPoint_1P_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_2P_X))
							{
								Game_Effect_Roll_StartPoint_2P_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_StartPoint_2P_Y))
							{
								Game_Effect_Roll_StartPoint_2P_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_X))
							{
								Game_Effect_Roll_Speed_X = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_Y))
							{
								Game_Effect_Roll_Speed_Y = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_1P_X))
							{
								Game_Effect_Roll_Speed_1P_X = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_1P_Y))
							{
								Game_Effect_Roll_Speed_1P_Y = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_2P_X))
							{
								Game_Effect_Roll_Speed_2P_X = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_Roll_Speed_2P_Y))
							{
								Game_Effect_Roll_Speed_2P_Y = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_NotesFlash))
							{
								Game_Effect_NotesFlash = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_NotesFlash_Timer))
							{
								Game_Effect_NotesFlash_Timer = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash))
							{
								Game_Effect_GoGoSplash = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_X))
							{
								Game_Effect_GoGoSplash_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_Y))
							{
								Game_Effect_GoGoSplash_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_Rotate))
							{
								Game_Effect_GoGoSplash_Rotate = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_Timer))
							{
								Game_Effect_GoGoSplash_Timer = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_Fire))
							{
								Game_Effect_Fire = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_StartPoint_X))
							{
								Game_Effect_FlyingNotes_StartPoint_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_StartPoint_Y))
							{
								Game_Effect_FlyingNotes_StartPoint_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_EndPoint_X))
							{
								Game_Effect_FlyingNotes_EndPoint_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_EndPoint_Y))
							{
								Game_Effect_FlyingNotes_EndPoint_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_Sine))
							{
								Game_Effect_FlyingNotes_Sine = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_IsUsingEasing))
							{
								Game_Effect_FlyingNotes_IsUsingEasing = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_FlyingNotes_Timer))
							{
								Game_Effect_FlyingNotes_Timer = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_FireWorks))
							{
								Game_Effect_FireWorks = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Effect_FireWorks_Timer))
							{
								Game_Effect_FireWorks_Timer = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_Rainbow_Timer))
							{
								Game_Effect_Rainbow_Timer = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Effect_HitExplosion_AddBlend))
							{
								Game_Effect_HitExplosion_AddBlend = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_HitExplosionBig_AddBlend))
							{
								Game_Effect_HitExplosionBig_AddBlend = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_FireWorks_AddBlend))
							{
								Game_Effect_FireWorks_AddBlend = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_Fire_AddBlend))
							{
								Game_Effect_Fire_AddBlend = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_AddBlend))
							{
								Game_Effect_GoGoSplash_AddBlend = strParam[0].ToBool();
							}
							else if (strCommand == nameof(Game_Effect_FireWorks_Timing))
							{
								Game_Effect_FireWorks_Timing = int.Parse(strParam);
							}
							#endregion
							#region Runner
							else if (strCommand == nameof(this.Game_Runner_Size))
							{
								this.Game_Runner_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Runner_Ptn))
							{
								ParseInt32(value => Game_Runner_Ptn = value);
							}
							else if (strCommand == nameof(Game_Runner_Type))
							{
								ParseInt32(value => Game_Runner_Type = value);
							}
							else if (strCommand == nameof(Game_Runner_StartPoint_X))
							{
								this.Game_Runner_StartPoint_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Runner_StartPoint_Y))
							{
								this.Game_Runner_StartPoint_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Runner_Timer))
							{
								if (int.Parse(strParam) != 0)
									Game_Runner_Timer = int.Parse(strParam);
							}
							#endregion
							#region Dan_C
							else if (strCommand == nameof(Game_DanC_Title_ForeColor))
							{
								Game_DanC_Title_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_DanC_Title_BackColor))
							{
								Game_DanC_Title_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_DanC_SubTitle_ForeColor))
							{
								Game_DanC_SubTitle_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_DanC_SubTitle_BackColor))
							{
								Game_DanC_SubTitle_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Game_DanC_X))
							{
								Game_DanC_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Y))
							{
								Game_DanC_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_DanC_Y_Padding))
							{
								ParseInt32(value => Game_DanC_Y_Padding = value);
							}
							else if (strCommand == nameof(Game_DanC_Offset))
							{
								Game_DanC_Offset = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Number_Padding))
							{
								ParseInt32(value => Game_DanC_Number_Padding = value);
							}

							else if (strCommand == nameof(Game_DanC_Number_Small_Scale))
							{
								Game_DanC_Number_Small_Scale = float.Parse(strParam);
							}

							else if (strCommand == nameof(Game_DanC_Number_Small_Padding))
							{
								ParseInt32(value => Game_DanC_Number_Small_Padding = value);
							}

							else if (strCommand == nameof(Game_DanC_Number_XY))
							{
								Game_DanC_Number_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Number_Small_Number_Offset))
							{
								Game_DanC_Number_Small_Number_Offset = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_ExamType_Size))
							{
								Game_DanC_ExamType_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Percent_Hit_Score_Padding))
							{
								Game_DanC_Percent_Hit_Score_Padding = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_ExamUnit_Size))
							{
								Game_DanC_ExamUnit_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Exam_Offset))
							{
								Game_DanC_Exam_Offset = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_DanC_Dan_Plate))
							{
								Game_DanC_Dan_Plate = strParam.Split(',').Select(int.Parse).ToArray();
							}

							#endregion
							#region PuchiChara
							else if (strCommand == nameof(Game_PuchiChara_X))
							{
								Game_PuchiChara_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara_Y))
							{
								Game_PuchiChara_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara_BalloonX))
							{
								Game_PuchiChara_BalloonX = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara_BalloonY))
							{
								Game_PuchiChara_BalloonY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara_Scale))
							{
								Game_PuchiChara_Scale = strParam.Split(',').Select(float.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara))
							{
								Game_PuchiChara = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_PuchiChara_Sine))
							{
								ParseInt32(value => Game_PuchiChara_Sine = value);
							}
							else if (strCommand == nameof(Game_PuchiChara_Timer))
							{
								ParseInt32(value => Game_PuchiChara_Timer = value);
							}
							else if (strCommand == nameof(Game_PuchiChara_SineTimer))
							{
								Game_PuchiChara_SineTimer = double.Parse(strParam);
							}
							#endregion
							#region Training
							else if (strCommand == nameof(Game_Training_ScrollTime))
							{
								Game_Training_ScrollTime = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Training_ProgressBar_XY))
							{
								Game_Training_ProgressBar_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Training_GoGoPoint_Y))
							{
								Game_Training_GoGoPoint_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Training_JumpPoint_Y))
							{
								Game_Training_JumpPoint_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Training_MaxMeasureCount_XY))
							{
								Game_Training_MaxMeasureCount_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Training_CurrentMeasureCount_XY))
							{
								Game_Training_CurrentMeasureCount_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Training_SpeedDisplay_XY))
							{
								Game_Training_CurrentMeasureCount_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Training_SmallNumber_Width))
							{
								Game_Training_SmallNumber_Width = int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_Training_BigNumber_Width))
							{
								Game_Training_BigNumber_Width = int.Parse(strParam);
							}
							#endregion
							#region Background

							else if (strCommand == nameof(this.Background_Scroll_PatternY))
							{
								this.Background_Scroll_PatternY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion
							#endregion
							#region Result
							else if (strCommand == nameof(Result_MusicName_X))
							{
								Result_MusicName_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_MusicName_Y))
							{
								Result_MusicName_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_MusicName_FontSize))
							{
								if (int.Parse(strParam) > 0)
									Result_MusicName_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_MusicName_ReferencePoint))
							{
								Result_MusicName_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_StageText_X))
							{
								Result_StageText_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_StageText_Y))
							{
								Result_StageText_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_StageText_FontSize))
							{
								if (int.Parse(strParam) > 0)
									Result_StageText_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(Result_StageText_ReferencePoint))
							{
								Result_StageText_ReferencePoint = (ReferencePoint)int.Parse(strParam);
							}

							else if (strCommand == nameof(Result_MusicName_ForeColor))
							{
								Result_MusicName_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Result_StageText_ForeColor))
							{
								Result_StageText_ForeColor = ColorTranslator.FromHtml(strParam);
							}
							//else if (strCommand == nameof(Result_StageText_ForeColor_Red))
							//{
							//    Result_StageText_ForeColor_Red = ColorTranslator.FromHtml(strParam);
							//}
							else if (strCommand == nameof(Result_MusicName_BackColor))
							{
								Result_MusicName_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Result_StageText_BackColor))
							{
								Result_StageText_BackColor = ColorTranslator.FromHtml(strParam);
							}
							//else if (strCommand == nameof(Result_StageText_BackColor_Red))
							//{
							//    Result_StageText_BackColor_Red = ColorTranslator.FromHtml(strParam);
							//}

							else if (strCommand == nameof(Result_NamePlate_X))
							{
								Result_NamePlate_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_NamePlate_Y))
							{
								Result_NamePlate_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Result_Dan))
							{
								Result_Dan = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_Dan_XY))
							{
								Result_Dan_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_Dan_Plate_XY))
							{
								Result_Dan_Plate_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_Crown_X))
							{
								Result_Crown_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_Crown_Y))
							{
								Result_Crown_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Result_RotateInterval))
							{
								if (int.Parse(strParam) != 0)
									Result_RotateInterval = int.Parse(strParam);
							}
							#endregion
							#endregion
						}
						continue;
					}
					catch (Exception exception)
					{
						Trace.TraceError(exception.ToString());
						Trace.TraceError("An exception has occurred, but processing continues.");
						continue;
					}
				}
			}
#if DEBUG
			Tomlyn.TomlModelOptions tm = new()
			{
				ConvertPropertyName = (x) => x,
				ConvertFieldName = (x) => x,
			};
			Console.WriteLine(Tomlyn.Toml.FromModel(this.SkinConfig, tm));
#endif
		}

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDisposed済み)
			{
				for (int i = 0; i < this.nシステムサウンド数; i++)
					this[i].Dispose();

				this.bDisposed済み = true;
			}
		}
		//-----------------
		#endregion


		// その他

		#region[ Genre ]
		public Dictionary<string, int> GenreKeyPairs = new Dictionary<string, int>
		{
			{ "J-POP", 0 },
			{ "アニメ", 1 },
			{ "ゲームミュージック", 2 },
			{ "ナムコオリジナル", 3 },
			{ "クラシック", 4 },
			{ "バラエティ", 5 },
			{ "どうよう", 6 },
			{ "ボーカロイド", 7 },
			{ "VOCALOID", 7 },
		};

		public int MaxKeyNum = 7;


		public Dictionary<string, Dictionary<string, int>> SortList = new Dictionary<string, Dictionary<string, int>>
		{
			{ "AC15", new Dictionary<string, int>
                {
					{ "J-POP", 0 },
					{ "アニメ", 1 },
					{ "ボーカロイド", 2 },
					{ "VOCALOID", 2 },
					{ "どうよう", 3 },
					{ "バラエティ", 4 },
					{ "クラシック", 5 },
					{ "ゲームミュージック", 6 },
					{ "ナムコオリジナル", 7 },
                }
			},
            { "AC8-14", new Dictionary<string, int>
                {
					{ "アニメ", 0 },
					{ "J-POP", 1 },
					{ "ゲームミュージック", 2 },
					{ "ナムコオリジナル", 3 },
					{ "クラシック", 4 },
					{ "どうよう", 5 },
					{ "バラエティ", 6 },
					{ "ボーカロイド", 7 },
					{ "VOCALOID", 7 },
                }
            },
        };

        #endregion

        #region [ private ]
        //-----------------
        private bool bDisposed済み;
		//-----------------
		#endregion

		public CSkinConfig SkinConfig = new();
		public class CSkinConfig
		{
			public CGeneral General { get; set; } = new();
			public class CGeneral
			{
				public string Name { get; set; } = "Unknown";
				public string Version { get; set; } = "Unknown";
				public string Creator { get; set; } = "Unknown";
			}
			public CFont Font { get; set; } = new();
			public class CFont
			{
				public int EdgeRatio { get; set; } = 30;
				public int EdgeRatioVertical { get; set; } = 30;
			}
			public CTitle Title { get; set; } = new();
			public class CTitle
			{

			}
			public CConfig Config { get; set; } = new();
			public class CConfig
			{
				public int ItemTextCorrectionX { get; set; } = 0;
				public int ItemTextCorrectionY { get; set; } = 0;
			}
			public CSongSelect SongSelect { get; set; } = new();
			public class CSongSelect
			{
				public int OverallY { get; set; } = 123;
				public int[] NamePlateX { get; set; } = new int[2] { 60, 950 };
				public int[] NamePlateY { get; set; } = new int[2] { 650, 650 };
				public int[] NamePlateAutoX { get; set; } = new int[2] { 60, 950 };
				public int[] NamePlateAutoY { get; set; } = new int[2] { 650, 650 };
				public int CounterX { get; set; } = 1145;
				public int CounterY { get; set; } = 55;
				public int[] ScoreWindowX { get; set; } = { 0, 1030 };
				public int[] ScoreWindowY { get; set; }= { 160, 160 };
				public int BackBoxTextCorrectionY { get; set; } = 0;
				public int BoxHeaderCorrectionY { get; set; }= 0;
				public string[] RotateChara { get; set; } = new string[] { };
				public string[] CorrectionXChara { get; set; } = new string[] { };
				public string[] CorrectionYChara { get; set; } = new string[] { };
				public int[] CorrectionXCharaValue { get; set; } = new int[] { };
				public int[] CorrectionYCharaValue { get; set; } = new int[] { };
				public CDifficultySelect Difficulty { get; set; } = new();
				public class CDifficultySelect
				{
					public int MarkY { get; set; } = 600;
					public int[] ChangeSEBoxX { get; set; } = new int[] { 220, 1050 };
					public int[] ChangeSEBoxY { get; set; } = new int[] { 740, 740 };
					public int[] PlayOptionBoxX { get; set; } = new int[] { 220, 1050 };
					public int[] PlayOptionBoxY { get; set; } = new int[] { 750, 750 };
					public int[] PlayOptionBoxSectionY { get; set; } = new int[] { 0, 72, 118 };
					public int PlayOptionNameCorrectionX { get; set; } = -150;
					public int PlayOptionNameCorrectionY { get; set; } = -2;
					public int PlayOptionListCorrectionX { get; set; } = 90;
					public int PlayOptionListCorrectionY { get; set; } = -2;
					public int[] BarX { get; set; } = new int[] { 440, 540, 640, 740 };
					public int[] BarY { get; set; } = new int[] { 90, 90, 90, 90 };
					public int[] BarEtcX { get; set; } = new int[] { 225, 300, 375 };
					public int[] BarEtcY { get; set; } = new int[] { 150, 150, 150 };
					public int[] AncX { get; set; } = new int[] { 441, 541, 641, 741 };
					public int[] AncY { get; set; } = new int[] { -10, -10, -10, -10 };
					public int[] AncBoxX { get; set; } = new int[] { 441, 541, 641, 741 };
					public int[] AncBoxY { get; set; } = new int[] { 138, 138, 138, 138 };
					public int[] AncEtcX { get; set; } = new int[] { 210, 285, 360 };
					public int[] AncEtcY { get; set; } = new int[] { 0, 0, 0 };
					public int[] AncBoxEtcX { get; set; } = new int[] { 210, 285, 360 };
					public int[] AncBoxEtcY { get; set; } = new int[] { 105, 105, 105 };
					public int BarCenterX { get; set; } = 643;
					public int BarCenterNormalW { get; set; } = 387;
					public int BarCenterNormalH { get; set; } = 439;
					public int BarCenterNormalY { get; set; } = 125;
					public int BarCenterExpandW { get; set; } = 880;
					public int BarCenterExpandH { get; set; } = 540;
					public int BarCenterExpandY { get; set; } = 25;
				}
			}
			public CSongLoading SongLoading { get; set; } = new();
			public class CSongLoading
			{
				public int PlateX { get; set; } = 640;
				public int PlateY { get; set; } = 360;
				public int TitleX { get; set; } = 640;
				public int TitleY { get; set; } = 340;
				public int SubTitleX { get; set; } = 640;
				public int SubTitleY { get; set; } = 390;
				public int TitleFontSize { get; set; } = 30;
				public int SubTitleFontSize { get; set; } = 22;	
			}
			public CGame Game { get; set; } = new();
			public class CGame
			{
				public bool NotesAnime { get; set; } = false;
				public string StageText { get; set; } = "1曲目";
				public CChara Chara { get; set; } = new();
				public class CChara
				{
					public int[] X { get; set; } = new int[] { 0, 0 };
					public int[] Y { get; set; } = new int[] { 0, 537 };
					public int[] BalloonX { get; set; } = new int[] { 240, 240, 0, 0 };
					public int[] BalloonY { get; set; } = new int[] { 0, 297, 0, 0 };
					public int[] BalloonTimer { get; set; } = new int[] { 28, 28 };
					public int[] BalloonDelay { get; set; } = new int[] { 500, 500 };
					public int[] BalloonFadeOut { get; set; } = new int[] { 84, 84 };
					public int[] BeatNormal { get; set; } = new int[] { 1, 1 };
					public int[] BeatClear { get; set; } = new int[] { 2, 2 };
					public int[] BeatGoGo { get; set; } = new int[] { 2, 2 };
					public int[][] MotionNormal { get; set; } = new int[][] { new int[] { 0 }, new int[] { 0 }};
					public int[][] MotionClear { get; set; } = new int[][] { new int[] { 0 }, new int[] { 0 }};
					public int[][] MotionGoGo { get; set; } = new int[][] { new int[] { 0 }, new int[] { 0 }};
				}
				public CDancer Dancer { get; set; } = new();
				public class CDancer
				{
					public int[] X { get; set; } = new int[] { 640, 430, 856, 215, 1070 };
					public int[] Y { get; set; } = new int[] { 500, 500, 500, 500, 500 };
					public int[] Motion { get; set; } = new int[] { 0 };
					public int Beat { get; set; } = 8;
					public int[] Gauge { get; set; } = new int[] { 0, 20, 40, 60, 80 };
				}
				public CMob Mob { get; set; } = new();
				public class CMob
				{
					public int Beat { get; set; } = 1;
					public int PtnBeat { get; set; } = 1;
				}
			}
			public CResult Result { get; set; } = new();
			public class CResult
			{

			}
			public CEnding Ending { get; set; } = new();
			public class CEnding
			{

			}
		}

		#region 背景(スクロール)
		public int[] Background_Scroll_Y = new int[] { 0, 536 };
		#endregion


		#region[ 座標 ]
		//2017.08.11 kairera0467 DP実用化に向けてint配列に変更

		//フィールド位置　Xは判定枠部分の位置。Yはフィールドの最上部の座標。
		//現時点ではノーツ画像、Senotes画像、判定枠が連動する。
		//Xは中央基準描画、Yは左上基準描画
		public int[] nScrollFieldX = new int[] { 414, 414 };
		public int[] nScrollFieldY = new int[] { 192, 368 };

		//中心座標指定
		public int[] nJudgePointX = new int[] { 413, 413, 413, 413 };
		public int[] nJudgePointY = new int[] { 256, 433, 0, 0 };

		//フィールド背景画像
		//ScrollField座標への追従設定が可能。
		//分岐背景、ゴーゴー背景が連動する。(全て同じ大きさ、位置で作成すること。)
		//左上基準描画
		public int[] nScrollFieldBGX = new int[] { 333, 333, 333, 333 };
		public int[] nScrollFieldBGY = new int[] { 192, 368, 0, 0 };

		//SEnotes
		//音符座標に加算
		public int[] nSENotesY = new int[] { 131, 131 };

		//光る太鼓部分
		public int nMtaikoBackgroundX = 0;
		public int nMtaikoBackgroundY = 184;
		public int nMtaikoFieldX = 0;
		public int nMtaikoFieldY = 184;
		public int nMtaikoMainX = 0;
		public int nMtaikoMainY = 0;

		//コンボ
		public int[] nComboNumberX = new int[] { 0, 0, 0, 0 };
		public int[] nComboNumberY = new int[] { 212, 388, 0, 0 };
		public int[] nComboNumberTextY = new int[] { 271, 447, 0, 0 };
		public int[] nComboNumberTextLargeY = new int[] { 270, 446, 0, 0 };
		public float fComboNumberSpacing = 0;
		public float fComboNumberSpacing_l = 0;

		public bool b現在のステージ数を表示しない = false;

		//リザルト画面
		//現在のデフォルト値はダミーです。
		public int[] nResultPanelX = { 515, 515 };
		public int[] nResultPanelY = { 75, 369 };
		public int[] nResultScoreX = { 730, 730 };
		public int[] nResultScoreY = { 252, 546 };
		public int[] nResultJudge_X = { 815, 815 };
		public int[] nResultJudge_Y = { 182, 476 };
		public int[] nResultGreatX = { 960, 960 };
		public int[] nResultGreatY = { 188, 482 };
		public int[] nResultGoodX = { 960, 960 };
		public int[] nResultGoodY = { 226, 520 };
		public int[] nResultBadX = { 960, 960 };
		public int[] nResultBadY = { 266, 560 };
		public int[] nResultComboX = { 1225, 1225 };
		public int[] nResultComboY = { 188, 482 };
		public int[] nResultRollX = { 1225, 1225 };
		public int[] nResultRollY = { 226, 520 };
		public int[] nResultGaugeBaseX = { 555, 555 };
		public int[] nResultGaugeBaseY = { 122, 416 };
		public int[] nResultGaugeBodyX = { 559, 559 };
		public int[] nResultGaugeBodyY = { 125, 419 };

		public int[] nResultV2PanelX = { 0, 640 };
		public int[] nResultV2PanelY = { 0, 0 };
		public int[] nResultV2ScoreX = { 300, 940 };
		public int[] nResultV2ScoreY = { 240, 240 };
		public int[] nResultV2GreatX = { 560, 1200 };
		public int[] nResultV2GreatY = { 206, 206 };
		public int[] nResultV2GoodX = { 560, 1200 };
		public int[] nResultV2GoodY = { 248, 248 };
		public int[] nResultV2BadX = { 560, 1200 };
		public int[] nResultV2BadY = { 290, 290 };
		public int[] nResultV2RollX = { 560, 1200 };
		public int[] nResultV2RollY = { 332, 332 };
		public int[] nResultV2ComboX = { 560, 1200 };
		public int[] nResultV2ComboY = { 374, 374 };
		public int[] nResultV2GaugeBackX = { 3, 643 };
		public int[] nResultV2GaugeBackY = { 122, 122 };
		public int[] nResultV2GaugeBodyX = { 60, 700 };
		public int[] nResultV2GaugeBodyY = { 130, 130 };
		#endregion

		public enum RollColorMode : int
		{
			None = 0, // PS4, Switchなど
			All = 1, // 旧筐体(旧作含む)
			WithoutStart = 2, // 新筐体
		}
		public enum ReferencePoint : int //テクスチャ描画の基準点を変更可能にするための値(rhimm)
		{
			Center = 0,
			Left = 1,
			Right = 2,
		}

		#region 新・SkinConfig
		#region SongSelect
		public Color[] SongSelect_ForeColor = new Color[] { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White };
		public Color[] SongSelect_BackColor = new Color[] { Color.Black, ColorTranslator.FromHtml("#01455B"), ColorTranslator.FromHtml("#9D3800"), ColorTranslator.FromHtml("#412080"), ColorTranslator.FromHtml("#980E00"), ColorTranslator.FromHtml("#875600"), ColorTranslator.FromHtml("#366600"), ColorTranslator.FromHtml("#99001F"), ColorTranslator.FromHtml("#5B6278") };

		#endregion
		#region SongLoading
		public ReferencePoint SongLoading_Plate_ReferencePoint = ReferencePoint.Center;
		public ReferencePoint SongLoading_Title_ReferencePoint = ReferencePoint.Center;
		public ReferencePoint SongLoading_SubTitle_ReferencePoint = ReferencePoint.Center;
		public Color SongLoading_Title_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongLoading_Title_BackColor = ColorTranslator.FromHtml("#000000");
		public Color SongLoading_SubTitle_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongLoading_SubTitle_BackColor = ColorTranslator.FromHtml("#000000");
		public int SongLoading_v2_Plate_X = 640;
		public int SongLoading_v2_Plate_Y = 200;
		public int SongLoading_v2_Title_X = 640;
		public int SongLoading_v2_Title_Y = 180;
		public int SongLoading_v2_SubTitle_X = 640;
		public int SongLoading_v2_SubTitle_Y = 230;
		public ReferencePoint SongLoading_v2_Plate_ReferencePoint = ReferencePoint.Center;
		public ReferencePoint SongLoading_v2_Title_ReferencePoint = ReferencePoint.Center;
		public ReferencePoint SongLoading_v2_SubTitle_ReferencePoint = ReferencePoint.Center;

		#endregion
		#region Game
		public RollColorMode Game_RollColorMode = RollColorMode.All;
		public bool Game_JudgeFrame_AddBlend = true;
		#region Chara
		public int[] Game_Chara_Ptn_Normal = new int[2],
			Game_Chara_Ptn_GoGo = new int[2],
			Game_Chara_Ptn_Clear = new int[2],
			Game_Chara_Ptn_10combo = new int[2],
			Game_Chara_Ptn_10combo_Max = new int[2],
			Game_Chara_Ptn_GoGoStart = new int[2],
			Game_Chara_Ptn_GoGoStart_Max = new int[2],
			Game_Chara_Ptn_ClearIn = new int[2],
			Game_Chara_Ptn_SoulIn = new int[2],
			Game_Chara_Ptn_Balloon_Breaking = new int[2],
			Game_Chara_Ptn_Balloon_Broke = new int[2],
			Game_Chara_Ptn_Balloon_Miss = new int[2];

		#endregion
		#region Dancer
		public int Game_Dancer_Ptn = 0;
		#endregion
		#region Mob
		public int Game_Mob_Ptn = 0;
		#endregion
		#region CourseSymbol
		public int[] Game_CourseSymbol_X = new int[] { 64, 64 };
		public int[] Game_CourseSymbol_Y = new int[] { 232, 432 };
		#endregion
		#region PanelFont
		public int Game_MusicName_X = 1254;
		public int Game_MusicName_Y = 14;
		public int Game_MusicName_FontSize = 30;
		public ReferencePoint Game_MusicName_ReferencePoint = ReferencePoint.Right;
		public int Game_SubTitleName_X = 1114;
		public int Game_SubTitleName_Y = 70;
		public int Game_SubTitleName_FontSize = 15;
		public ReferencePoint Game_SubTitleName_ReferencePoint = ReferencePoint.Right;
		public int Game_Genre_X = 1114;
		public int Game_Genre_Y = 74;
		public int Game_Lyric_X = 640;
		public int Game_Lyric_Y = 630;
		public string Game_Lyric_FontName = CFontRenderer.DefaultFontName;
		public int Game_Lyric_FontSize = 38;
		public ReferencePoint Game_Lyric_ReferencePoint = ReferencePoint.Center;

		public Color Game_MusicName_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Game_StageText_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Game_Lyric_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Game_MusicName_BackColor = ColorTranslator.FromHtml("#000000");
		public Color Game_StageText_BackColor = ColorTranslator.FromHtml("#000000");
		public Color Game_Lyric_BackColor = ColorTranslator.FromHtml("#0000FF");

		#endregion
		#region Score
		public int[] Game_Score_X = new int[] { 20, 20, 0, 0 };
		public int[] Game_Score_Y = new int[] { 226, 530, 0, 0 };
		public int[] Game_Score_Add_X = new int[] { 20, 20, 0, 0 };
		public int[] Game_Score_Add_Y = new int[] { 186, 570, 0, 0 };
		public int[] Game_Score_AddBonus_X = new int[] { 20, 20, 0, 0 };
		public int[] Game_Score_AddBonus_Y = new int[] { 136, 626, 0, 0 };
		public int Game_Score_Padding = 20;
		public int[] Game_Score_Size = new int[] { 24, 40 };
		#endregion
		#region Taiko
		public int[] Game_Taiko_NamePlate_X = new int[] { 0, 0 };
		public int[] Game_Taiko_NamePlate_Y = new int[] { 288, 368 };
		public int[] Game_Taiko_PlayerNumber_X = new int[] { 4, 4 };
		public int[] Game_Taiko_PlayerNumber_Y = new int[] { 233, 435 };
		public int[] Game_Taiko_X = new int[] { 190, 190 };
		public int[] Game_Taiko_Y = new int[] { 190, 366 };
		public int[] Game_Taiko_Combo_X = new int[] { 268, 268 };
		public int[] Game_Taiko_Combo_Y = new int[] { 270, 448 };
		public int[] Game_Taiko_Combo_Ex_X = new int[] { 268, 268 };
		public int[] Game_Taiko_Combo_Ex_Y = new int[] { 270, 448 };
		public int[] Game_Taiko_Combo_Ex4_X = new int[] { 268, 268 };
		public int[] Game_Taiko_Combo_Ex4_Y = new int[] { 270, 448 };
		public int[] Game_Taiko_Combo_Padding = new int[] { 28, 30, 24 };
		public int[] Game_Taiko_Combo_Size = new int[] { 42, 48 };
		public int[] Game_Taiko_Combo_Size_Ex = new int[] { 42, 56 };
		public float[] Game_Taiko_Combo_Scale = new float[] { 1.0f, 1.0f, 0.8f };
		public int[] Game_Taiko_Combo_Text_X = new int[] { 268, 268 };
		public int[] Game_Taiko_Combo_Text_Y = new int[] { 295, 472 };
		public int[] Game_Taiko_Combo_Text_Size = new int[] { 100, 50 };
		public bool Game_Taiko_Combo_Ex_IsJumping = true;
		#endregion
		#region Gauge
		public int Game_Gauge_Rainbow_Ptn;
		public int Game_Gauge_Rainbow_Danc_Ptn;
		public int Game_Gauge_Rainbow_Timer = 50;
		#endregion
		#region Balloon
		public int[] Game_Balloon_Combo_X = new int[] { 253, 253 };
		public int[] Game_Balloon_Combo_Y = new int[] { -11, 498 };
		public int[] Game_Balloon_Combo_Number_X = new int[] { 312, 312 };
		public int[] Game_Balloon_Combo_Number_Y = new int[] { 34, 540 };
		public int[] Game_Balloon_Combo_Number_Ex_X = new int[] { 335, 335 };
		public int[] Game_Balloon_Combo_Number_Ex_Y = new int[] { 34, 540 };
		public int[] Game_Balloon_Combo_Text_X = new int[] { 471, 471 };
		public int[] Game_Balloon_Combo_Text_Y = new int[] { 55, 561 };
		public int[] Game_Balloon_Combo_Text_Ex_X = new int[] { 491, 491 };
		public int[] Game_Balloon_Combo_Text_Ex_Y = new int[] { 55, 561 };

		public int[] Game_Balloon_Balloon_X = new int[] { 382, 382 };
		public int[] Game_Balloon_Balloon_Y = new int[] { 115, 290 };
		public int[] Game_Balloon_Balloon_Frame_X = new int[] { 382, 382 };
		public int[] Game_Balloon_Balloon_Frame_Y = new int[] { 80, 260 };
		public int[] Game_Balloon_Balloon_Number_X = new int[] { 486, 486 };
		public int[] Game_Balloon_Balloon_Number_Y = new int[] { 187, 373 };
		public int[] Game_Balloon_Roll_Frame_X = new int[] { 218, 218 };
		public int[] Game_Balloon_Roll_Frame_Y = new int[] { -3, 514 };
		public int[] Game_Balloon_Roll_Number_X = new int[] { 392, 392 };
		public int[] Game_Balloon_Roll_Number_Y = new int[] { 128, 639 };
		public int[] Game_Balloon_Number_Size = new int[] { 62, 80 };
		public int Game_Balloon_Number_Padding = 60;
		public float Game_Balloon_Roll_Number_Scale = 1.000f;
		public float Game_Balloon_Balloon_Number_Scale = 0.879f;
		#endregion
		#region Effects
		public int[] Game_Effect_Roll_StartPoint_X = new int[] { 56, -10, 200, 345, 100, 451, 600, 260, -30, 534, 156, 363 };
		public int[] Game_Effect_Roll_StartPoint_Y = new int[] { 720 };
		public int[] Game_Effect_Roll_StartPoint_1P_X = new int[] { 56, -10, 200, 345, 100, 451, 600, 260, -30, 534, 156, 363 };
		public int[] Game_Effect_Roll_StartPoint_1P_Y = new int[] { 240 };
		public int[] Game_Effect_Roll_StartPoint_2P_X = new int[] { 56, -10, 200, 345, 100, 451, 600, 260, -30, 534, 156, 363 };
		public int[] Game_Effect_Roll_StartPoint_2P_Y = new int[] { 360 };
		public float[] Game_Effect_Roll_Speed_X = new float[] { 0.6f };
		public float[] Game_Effect_Roll_Speed_Y = new float[] { -0.6f };
		public float[] Game_Effect_Roll_Speed_1P_X = new float[] { 0.6f };
		public float[] Game_Effect_Roll_Speed_1P_Y = new float[] { -0.6f };
		public float[] Game_Effect_Roll_Speed_2P_X = new float[] { 0.6f };
		public float[] Game_Effect_Roll_Speed_2P_Y = new float[] { 0.6f };
		public int Game_Effect_Roll_Ptn;
		public int[] Game_Effect_NotesFlash = new int[] { 180, 180, 12 }; // Width, Height, Ptn
		public int Game_Effect_NotesFlash_Timer = 20;
		public int[] Game_Effect_GoGoSplash = new int[] { 300, 400, 10 };
		public int[] Game_Effect_GoGoSplash_X = new int[] { 120, 300, 520, 760, 980, 1160 };
		public int[] Game_Effect_GoGoSplash_Y = new int[] { 740, 730, 720, 720, 730, 740 };
		public bool Game_Effect_GoGoSplash_Rotate = true;
		public int Game_Effect_GoGoSplash_Timer = 25;
		public int[] Game_Effect_Fire = new int[] { 230, 230, 8 };
		// super-flying-notes AioiLight
		public int[] Game_Effect_FlyingNotes_StartPoint_X = new int[] { 414, 414 };
		public int[] Game_Effect_FlyingNotes_StartPoint_Y = new int[] { 260, 434 };
		public int[] Game_Effect_FlyingNotes_EndPoint_X = new int[] { 1222, 1222 }; // 1P, 2P
		public int[] Game_Effect_FlyingNotes_EndPoint_Y = new int[] { 164, 554 };

		public int Game_Effect_FlyingNotes_Sine = 220;
		public bool Game_Effect_FlyingNotes_IsUsingEasing = true;
		public int Game_Effect_FlyingNotes_Timer = 3;
		public int[] Game_Effect_FireWorks = new int[] { 180, 180, 10 };
		public int Game_Effect_FireWorks_Timer = 5;
		public int Game_Effect_Rainbow_Timer = 7;

		public bool Game_Effect_HitExplosion_AddBlend = true;
		public bool Game_Effect_HitExplosionBig_AddBlend = true;
		public bool Game_Effect_FireWorks_AddBlend = true;
		public bool Game_Effect_Fire_AddBlend = true;
		public bool Game_Effect_GoGoSplash_AddBlend = true;
		public int Game_Effect_FireWorks_Timing = 8;
		#endregion
		#region Runner
		public int[] Game_Runner_Size = new int[] { 60, 125 };
		public int Game_Runner_Ptn = 48;
		public int Game_Runner_Type = 4;
		public int[] Game_Runner_StartPoint_X = new int[] { 175, 175 };
		public int[] Game_Runner_StartPoint_Y = new int[] { 40, 560 };
		public int Game_Runner_Timer = 16;
		#endregion
		#region PuchiChara
		public int[] Game_PuchiChara_X = new int[] { 100, 100 };
		public int[] Game_PuchiChara_Y = new int[] { 140, 600 };
		public int[] Game_PuchiChara_BalloonX = new int[] { 300, 300 };
		public int[] Game_PuchiChara_BalloonY = new int[] { 240, 500 };
		public float[] Game_PuchiChara_Scale = new float[] { 0.7f, 1.0f }; // 通常時、 ふうせん連打時
		public int[] Game_PuchiChara = new int[] { 180, 180, 2}; // Width, Height, Ptn
		public int Game_PuchiChara_Sine = 20;
		public int Game_PuchiChara_Timer = 4800;
		public double Game_PuchiChara_SineTimer = 2;
		#endregion
		#region Dan-C
		public Color Game_DanC_Title_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Game_DanC_Title_BackColor = ColorTranslator.FromHtml("#000000");
		public Color Game_DanC_SubTitle_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Game_DanC_SubTitle_BackColor = ColorTranslator.FromHtml("#000000");


		public int[] Game_DanC_X = new int[] { 302, 302, 302 };
		public int Game_DanC_Y = 520;//変更
		public int Game_DanC_Y_Padding = 100;
		public int[] Game_DanC_Offset = new int[] { 15, 10 };
		public int Game_DanC_Number_Padding = 50;
		public int[] Game_DanC_Number_XY = new int[] { 250, 550 };
		public float Game_DanC_Number_Small_Scale = 0.5f;
		public int Game_DanC_Number_Small_Padding = 26;
		public int[] Game_DanC_Dan_Plate = new int[] { 149, 416 };
		public int[] Game_DanC_ExamType_Size = new int[] { 100, 36 };
		public int[] Game_DanC_ExamUnit_Size = new int[] { 60, 36 };
		public int[] Game_DanC_Number_Small_Number_Offset = new int[] { 178, -15 };
		public int[] Game_DanC_Percent_Hit_Score_Padding = new int[] { 20, 20, 20, 20 };
		public int[] Game_DanC_Exam_Offset = new int[] { 932, -40 };


		public int[] Game_DanC_v2_Panel_X = new int[] { 90, 90, 90 };
		public int[] Game_DanC_v2_Panel_Y = new int[] { 385, 495, 605 };
		public int[] Game_DanC_v2_Base_Offset = new int[] { 260, 17 };
		public int[] Game_DanC_v2_Gauge_Offset = new int[] { 5, 5 };
		public int[] Game_DanC_v2_Amount_Offset = new int[] { 0, 0 };
		public float Game_DanC_v2_Amount_Scale = 1f;
		public int[] Game_DanC_v2_ExamType_Offset = new int[] { 122, 10 };
		public int[] Game_DanC_v2_ExamType_Size = new int[] { 100, 25 };
		public int[] Game_DanC_v2_ExamRange_Offset = new int[] { 180, 30 };
		public int[] Game_DanC_v2_ExamRangeNum_Offset = new int[] { -15, 3 };
		public int[] Game_DanC_v2_Dan_Plate = new int[] { 1000, 416 };

		public int[] Game_DanC_v2_SoulGauge_Box_X = new int[] { 110, 800 };//0%,100%のX座標
		public int Game_DanC_v2_SoulGauge_Box_Y = 70;
		public int Game_DanC_v2_SoulGauge_Box_Persent_Width = 25;
		public int[] Game_DanC_v2_SoulGauge_Box_ExamType_Offset = new int[] { 65, 55 };
		public float Game_DanC_v2_SoulGauge_Box_ExamType_Box_XRatio = 0.7f;
		public int[] Game_DanC_v2_SoulGauge_Box_ExamRange_Offset = new int[] { 300, 50 };


		public int[] Game_DanC_v2_SmallGauge_Offset = new int[] { 487, 2 };
		public int Game_DanC_v2_SmallGauge_Offset_Y_Padding = 32;

		public float Game_DanC_v2_Number_Small_Scale = 0.5f;
		#endregion
		#region Training
		public int Game_Training_ScrollTime = 350;
		public int[] Game_Training_ProgressBar_XY = { 333, 378 };
		public int Game_Training_GoGoPoint_Y = 396;
		public int Game_Training_JumpPoint_Y = 375;
		public int[] Game_Training_MaxMeasureCount_XY = { 284, 377 };
		public int[] Game_Training_CurrentMeasureCount_XY = { 254, 370 };
		public int[] Game_Training_SpeedDisplay_XY = { 110, 370 };
		public int Game_Training_SmallNumber_Width = 17;
		public int Game_Training_BigNumber_Width = 20;
		#endregion
		#region Background
		public int[] Background_Scroll_PatternY = new int[] { 0, 0 };
		#endregion
		#endregion
		#region Result
		public int Result_MusicName_X = 1254;
		public int Result_MusicName_Y = 6;
		public int Result_MusicName_FontSize = 30;
		public ReferencePoint Result_MusicName_ReferencePoint = ReferencePoint.Right;
		public int Result_StageText_X = 230;
		public int Result_StageText_Y = 6;
		public int Result_StageText_FontSize = 30;
		public ReferencePoint Result_StageText_ReferencePoint = ReferencePoint.Left;
		public int Result_v2_MusicName_X = 640;
		public int Result_v2_MusicName_Y = 6;
		public ReferencePoint Result_v2_MusicName_ReferencePoint = ReferencePoint.Center;

		public Color Result_MusicName_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Result_StageText_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Result_MusicName_BackColor = ColorTranslator.FromHtml("#000000");
		public Color Result_StageText_BackColor = ColorTranslator.FromHtml("#000000");

		public int[] Result_NamePlate_X = new int[] { 260, 260 };
		public int[] Result_NamePlate_Y = new int[] { 96, 390 };

		public int[] Result_Dan = new int[] { 500, 500 };
		public int[] Result_Dan_XY = new int[] { 100, 0 };
		public int[] Result_Dan_Plate_XY = new int[] { 149, 416 };

		public int[] Result_Crown_X = new int[] { 400, 400 };
		public int[] Result_Crown_Y = new int[] { 250, 544 };
		public int Result_RotateInterval = 50;

		public int[] Result_v2_NamePlate_X = new int[] { 20, 1000 };
		public int[] Result_v2_NamePlate_Y = new int[] { 610, 610 };

		public int[] Result_v2_Crown_X = new int[] { 270, 910 };
		public int[] Result_v2_Crown_Y = new int[] { 340, 340 };
		#endregion
		public int SECount = 0;
		public int[] NowSENum = { 0, 0 };
		public string[] SENames;
		#endregion

	}
}