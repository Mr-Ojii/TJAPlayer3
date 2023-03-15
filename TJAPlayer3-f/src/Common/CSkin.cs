using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FDK;
using FDK.ExtensionMethods;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
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
#if DEBUG
			Console.WriteLine(Tomlyn.Toml.FromModel(this.SkinConfig, tomlModelOptions));
#endif
			this.SkinConfig = Toml.ToModel<CSkinConfig>(strToml, null, tomlModelOptions);
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

							#region[ 演奏 ]
							//-----------------------------
							if (strCommand == "ScrollFieldP1Y")
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

							//-----------------------------
							#endregion

							#region 新・SkinConfig
							#region Game
							else if (strCommand == nameof(Game_RollColorMode))
							{
								Game_RollColorMode = (RollColorMode)int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_JudgeFrame_AddBlend))
							{
								Game_JudgeFrame_AddBlend = strParam[0].ToBool();
							}
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
							else if (strCommand == nameof(Game_Effect_FireWorks_Timing))
							{
								Game_Effect_FireWorks_Timing = int.Parse(strParam);
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
							else if (strCommand == nameof(Result_MusicName_BackColor))
							{
								Result_MusicName_BackColor = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(Result_StageText_BackColor))
							{
								Result_StageText_BackColor = ColorTranslator.FromHtml(strParam);
							}

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
				public string[] ForeColor { get{ return this._ForeColor.Select(ColorTranslator.ToHtml).ToArray(); } set{ this._ForeColor = value.Select(ColorTranslator.FromHtml).ToArray(); } }
				public string[] BackColor { get{ return this._BackColor.Select(ColorTranslator.ToHtml).ToArray(); } set{ this._BackColor = value.Select(ColorTranslator.FromHtml).ToArray(); } }
				[IgnoreDataMember]
				public Color[] _ForeColor { get; set; } = new Color[] { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White };
				[IgnoreDataMember]
				public Color[] _BackColor { get; set; } = new Color[] { Color.Black, ColorTranslator.FromHtml("#01455B"), ColorTranslator.FromHtml("#9D3800"), ColorTranslator.FromHtml("#412080"), ColorTranslator.FromHtml("#980E00"), ColorTranslator.FromHtml("#875600"), ColorTranslator.FromHtml("#366600"), ColorTranslator.FromHtml("#99001F"), ColorTranslator.FromHtml("#5B6278") };
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
				public int PlateReferencePoint { get{ return (int)this._PlateReferencePoint; } set{ this._PlateReferencePoint = (ReferencePoint)value; } }
				public int TitleReferencePoint { get{ return (int)this._TitleReferencePoint; } set{ this._TitleReferencePoint = (ReferencePoint)value; } }
				public int SubTitleReferencePoint { get{ return (int)this._SubTitleReferencePoint; } set{ this._SubTitleReferencePoint = (ReferencePoint)value; } }
				[IgnoreDataMember]
				public ReferencePoint _PlateReferencePoint { get; set; } = ReferencePoint.Center;
				[IgnoreDataMember]
				public ReferencePoint _TitleReferencePoint { get; set; } = ReferencePoint.Center;
				[IgnoreDataMember]
				public ReferencePoint _SubTitleReferencePoint { get; set; } = ReferencePoint.Center;
				public string TitleForeColor { get{ return ColorTranslator.ToHtml(this._TitleForeColor); } set{ this._TitleForeColor = ColorTranslator.FromHtml(value); } }
				public string TitleBackColor { get{ return ColorTranslator.ToHtml(this._TitleBackColor); } set{ this._TitleBackColor = ColorTranslator.FromHtml(value); } }
				public string SubTitleForeColor { get{ return ColorTranslator.ToHtml(this._SubTitleForeColor); } set{ this._SubTitleForeColor = ColorTranslator.FromHtml(value); } }
				public string SubTitleBackColor { get{ return ColorTranslator.ToHtml(this._SubTitleBackColor); } set{ this._SubTitleBackColor = ColorTranslator.FromHtml(value); } }
				[IgnoreDataMember]
				public Color _TitleForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
				[IgnoreDataMember]
				public Color _TitleBackColor { get; set; } = ColorTranslator.FromHtml("#000000");
				[IgnoreDataMember]
				public Color _SubTitleForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
				[IgnoreDataMember]
				public Color _SubTitleBackColor { get; set; } = ColorTranslator.FromHtml("#000000");

				public int v2PlateX { get; set; } = 640;
				public int v2PlateY { get; set; } = 200;
				public int v2TitleX { get; set; } = 640;
				public int v2TitleY { get; set; } = 180;
				public int v2SubTitleX { get; set; } = 640;
				public int v2SubTitleY { get; set; } = 230;
				public int v2PlateReferencePoint { get{ return (int)this._v2PlateReferencePoint; } set{ this._v2PlateReferencePoint = (ReferencePoint)value; } }
				public int v2TitleReferencePoint { get{ return (int)this._v2TitleReferencePoint; } set{ this._v2TitleReferencePoint = (ReferencePoint)value; } }
				public int v2SubTitleReferencePoint { get{ return (int)this._v2SubTitleReferencePoint; } set{ this._v2SubTitleReferencePoint = (ReferencePoint)value; } }
				[IgnoreDataMember]
				public ReferencePoint _v2PlateReferencePoint { get; set; } = ReferencePoint.Center;
				[IgnoreDataMember]
				public ReferencePoint _v2TitleReferencePoint { get; set; } = ReferencePoint.Center;
				[IgnoreDataMember]
				public ReferencePoint _v2SubTitleReferencePoint { get; set; } = ReferencePoint.Center;
			}
			public CGame Game { get; set; } = new();
			public class CGame
			{
				public bool NotesAnime { get; set; } = false;
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
				public CCourseSymbol CourseSymbol { get; set; } = new();
				public class CCourseSymbol
				{
					public int[] X { get; set; } = new int[] { 64, 64 };
					public int[] Y { get; set; } = new int[] { 232, 432 };
				}
				public CPanelFont PanelFont { get; set; } = new();
				public class CPanelFont
				{
					public int MusicNameX { get; set; } = 1254;
					public int MusicNameY { get; set; } = 14;
					public int MusicNameFontSize { get; set; } = 30;
					public int MusicNameReferencePoint { get{ return (int)this._MusicNameReferencePoint; } set{ this._MusicNameReferencePoint = (ReferencePoint)value; } }
					[IgnoreDataMember]
					public ReferencePoint _MusicNameReferencePoint { get; set; } = ReferencePoint.Right;
					
					public int SubTitleNameX { get; set; } = 1114;
					public int SubTitleNameY { get; set; }= 70;
					public int SubTitleNameFontSize { get; set; } = 15;
					public int SubTitleNameReferencePoint { get{ return (int)this._SubTitleNameReferencePoint; } set{ this._SubTitleNameReferencePoint = (ReferencePoint)value; } }
					[IgnoreDataMember]
					public ReferencePoint _SubTitleNameReferencePoint { get; set; } = ReferencePoint.Right;

					public int GenreX { get; set; } = 1114;
					public int GenreY { get; set; } = 74;
					public int LyricX { get; set; } = 640;
					public int LyricY { get; set; } = 630;
					public string LyricFontName { get; set; } = CFontRenderer.DefaultFontName;
					public int LyricFontSize { get; set; } = 38;
					public int LyricReferencePoint { get{ return (int)this._LyricReferencePoint; } set{ this._LyricReferencePoint = (ReferencePoint)value; } }
					[IgnoreDataMember]
					public ReferencePoint _LyricReferencePoint { get; set; } = ReferencePoint.Center;
					
					public string StageText { get; set; } = "1曲目";
					public bool StageTextDisp { get; set; } = true;

					public string MusicNameForeColor { get{ return ColorTranslator.ToHtml(this._MusicNameForeColor); } set{ this._MusicNameForeColor = ColorTranslator.FromHtml(value); } }
					public string StageTextForeColor { get{ return ColorTranslator.ToHtml(this._StageTextForeColor); } set{ this._StageTextForeColor = ColorTranslator.FromHtml(value); } }
					public string LyricForeColor { get{ return ColorTranslator.ToHtml(this._LyricForeColor); } set{ this._LyricForeColor = ColorTranslator.FromHtml(value); } }
					public string MusicNameBackColor { get{ return ColorTranslator.ToHtml(this._MusicNameBackColor); } set{ this._MusicNameBackColor = ColorTranslator.FromHtml(value); } }
					public string StageTextBackColor { get{ return ColorTranslator.ToHtml(this._StageTextBackColor); } set{ this._StageTextBackColor = ColorTranslator.FromHtml(value); } }
					public string LyricBackColor { get{ return ColorTranslator.ToHtml(this._LyricBackColor); } set{ this._LyricBackColor = ColorTranslator.FromHtml(value); } }
					[IgnoreDataMember]
					public Color _MusicNameForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
					[IgnoreDataMember]
					public Color _StageTextForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
					[IgnoreDataMember]
					public Color _LyricForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
					[IgnoreDataMember]
					public Color _MusicNameBackColor { get; set; } = ColorTranslator.FromHtml("#000000");
					[IgnoreDataMember]
					public Color _StageTextBackColor { get; set; } = ColorTranslator.FromHtml("#000000");
					[IgnoreDataMember]
					public Color _LyricBackColor { get; set; } = ColorTranslator.FromHtml("#0000FF");
				}
				public CScore Score { get; set; } = new();
				public class CScore
				{
					public int[] X { get; set; } = new int[] { 20, 20, 0, 0 };
					public int[] Y { get; set; } = new int[] { 226, 530, 0, 0 };
					public int[] AddX { get; set; } = new int[] { 20, 20, 0, 0 };
					public int[] AddY { get; set; } = new int[] { 186, 570, 0, 0 };
					public int[] AddBonusX { get; set; } = new int[] { 20, 20, 0, 0 };
					public int[] AddBonusY { get; set; }= new int[] { 136, 626, 0, 0 };
					public int Padding { get; set; } = 20;
					public int[] Size { get; set; } = new int[] { 24, 40 };
				}
				public CTaiko Taiko { get; set; } = new();
				public class CTaiko
				{
					public int[] NamePlateX { get; set; } = new int[] { 0, 0 };
					public int[] NamePlateY { get; set; } = new int[] { 288, 368 };
					public int[] PlayerNumberX { get; set; } = new int[] { 4, 4 };
					public int[] PlayerNumberY { get; set; } = new int[] { 233, 435 };
					public int[] X { get; set; } = new int[] { 190, 190 };
					public int[] Y { get; set; } = new int[] { 190, 366 };
					public int[] ComboX { get; set; } = new int[] { 268, 268 };
					public int[] ComboY { get; set; } = new int[] { 270, 448 };
					public int[] ComboExX { get; set; } = new int[] { 268, 268 };
					public int[] ComboExY { get; set; } = new int[] { 270, 448 };
					public int[] ComboEx4X { get; set; } = new int[] { 268, 268 };
					public int[] ComboEx4Y { get; set; } = new int[] { 270, 448 };
					public int[] ComboPadding { get; set; } = new int[] { 28, 30, 24 };
					public int[] ComboSize { get; set; } = new int[] { 42, 48 };
					public int[] ComboSizeEx { get; set; } = new int[] { 42, 56 };
					public float[] ComboScale { get; set; } = new float[] { 1.0f, 1.0f, 0.8f };
					public int[] ComboTextX { get; set; } = new int[] { 268, 268 };
					public int[] ComboTextY { get; set; } = new int[] { 295, 472 };
					public int[] ComboTextSize { get; set; } = new int[] { 100, 50 };
					public bool ComboExIsJumping { get; set; } = true;
				}
				public CGauge Gauge { get; set; } = new();
				public class CGauge
				{
					public int RainbowTimer { get; set; } = 50;
				}
				public CBalloon Balloon { get; set; } = new();
				public class CBalloon
				{
					public int[] ComboX { get; set; } = new int[] { 253, 253 };
					public int[] ComboY { get; set; }= new int[] { -11, 498 };
					public int[] ComboNumberX { get; set; } = new int[] { 312, 312 };
					public int[] ComboNumberY { get; set; } = new int[] { 34, 540 };
					public int[] ComboNumberExX { get; set; } = new int[] { 335, 335 };
					public int[] ComboNumberExY { get; set; } = new int[] { 34, 540 };
					public int[] ComboTextX { get; set; } = new int[] { 471, 471 };
					public int[] ComboTextY { get; set; } = new int[] { 55, 561 };
					public int[] ComboTextExX { get; set; } = new int[] { 491, 491 };
					public int[] ComboTextExY { get; set; } = new int[] { 55, 561 };

					public int[] BalloonX { get; set; } = new int[] { 382, 382 };
					public int[] BalloonY { get; set; } = new int[] { 115, 290 };
					public int[] BalloonFrameX { get; set; } = new int[] { 382, 382 };
					public int[] BalloonFrameY { get; set; } = new int[] { 80, 260 };
					public int[] BalloonNumberX { get; set; } = new int[] { 486, 486 };
					public int[] BalloonNumberY { get; set; } = new int[] { 187, 373 };
					public int[] RollFrameX { get; set; } = new int[] { 218, 218 };
					public int[] RollFrameY { get; set; } = new int[] { -3, 514 };
					public int[] RollNumberX { get; set; } = new int[] { 392, 392 };
					public int[] RollNumberY { get; set; } = new int[] { 128, 639 };
					public int[] NumberSize { get; set; } = new int[] { 62, 80 };
					public int NumberPadding { get; set; } = 60;
					public float RollNumberScale { get; set; } = 1.000f;
					public float BalloonNumberScale { get; set; } = 0.879f;
				}
				public CEffect Effect { get; set; } = new();
				public class CEffect
				{
					public CRoll Roll { get; set; } = new();
					public class CRoll
					{

					}
					public CFireWorks FireWorks { get; set; } = new();
					public class CFireWorks
					{

					}
					public CGoGoSplash GoGoSplash { get; set; } = new();
					public class CGoGoSplash
					{
						public int Width { get; set; } = 300;
						public int Height { get; set; } = 400;
						public int Ptn { get; set; } = 10;
						public int[] X = new int[] { 120, 300, 520, 760, 980, 1160 };
						public int[] Y = new int[] { 740, 730, 720, 720, 730, 740 };
						public bool Rotate = true;
						public int Timer = 25;
						public bool AddBlend = true;
					}
					public CFlyingNotes FlyingNotes { get; set; } = new();
					public class CFlyingNotes
					{

					}
				}
				public CRunner Runner { get; set; } = new();
				public class CRunner
				{
					public int[] Size { get; set; } = new int[] { 60, 125 };
					public int Ptn { get; set; } = 48;
					public int Type { get; set; } = 4;
					public int[] StartPointX { get; set; } = new int[] { 175, 175 };
					public int[] StartPointY { get; set; } = new int[] { 40, 560 };
					public int Timer { get; set; } = 16;
				}
				public CPuchiChara PuchiChara { get; set; } = new();
				public class CPuchiChara
				{
					public int[] X { get; set; } = new int[] { 100, 100 };
					public int[] Y { get; set; } = new int[] { 140, 600 };
					public int[] BalloonX { get; set; } = new int[] { 300, 300 };
					public int[] BalloonY { get; set; } = new int[] { 240, 500 };
					public float[] Scale { get; set; } = new float[] { 0.7f, 1.0f }; // 通常時、 ふうせん連打時
					public int Width { get; set; } = 180;
					public int Height { get; set; } = 180;
					public int Ptn { get; set; } = 2;
					public int Sine { get; set; } = 20;
					public int Timer { get; set; } = 4800;
					public double SineTimer { get; set; } = 2;
				}
				public CBackground Background { get; set; } = new();
				public class CBackground
				{
					public int[] ScrollY { get; set; } = new int[] { 0, 536 };
					public int[] ScrollPattern { get; set; } = new int[] { 0, 0 };
				}
				public CDanC DanC { get; set; } = new();
				public class CDanC
				{
					public string TitleForeColor { get{ return ColorTranslator.ToHtml(this._TitleForeColor); } set{ this._TitleForeColor = ColorTranslator.FromHtml(value); } }
					public string TitleBackColor { get{ return ColorTranslator.ToHtml(this._TitleBackColor); } set{ this._TitleBackColor = ColorTranslator.FromHtml(value); } }
					public string SubTitleForeColor { get{ return ColorTranslator.ToHtml(this._SubTitleForeColor); } set{ this._SubTitleForeColor = ColorTranslator.FromHtml(value); } }
					public string SubTitleBackColor { get{ return ColorTranslator.ToHtml(this._SubTitleBackColor); } set{ this._SubTitleBackColor = ColorTranslator.FromHtml(value); } }
					[IgnoreDataMember]
					public Color _TitleForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
					[IgnoreDataMember]
					public Color _TitleBackColor { get; set; } = ColorTranslator.FromHtml("#000000");
					[IgnoreDataMember]
					public Color _SubTitleForeColor { get; set; } = ColorTranslator.FromHtml("#FFFFFF");
					[IgnoreDataMember]
					public Color _SubTitleBackColor { get; set; } = ColorTranslator.FromHtml("#000000");
					
					public int[] X { get; set; } = new int[] { 302, 302, 302 };
					public int Y { get; set; } = 520;
					public int YPadding { get; set; } = 100;
					public int[] Offset { get; set; } = new int[] { 15, 10 };
					public int NumberPadding { get; set; } = 50;
					public int[] NumberXY { get; set; } = new int[] { 250, 550 };
					public float NumberSmallScale { get; set; } = 0.5f;
					public int NumberSmallPadding { get; set; } = 26;
					public int[] NumberSmallOffset { get; set; } = new int[] { 178, -15 };
					public int[] DanPlateXY { get; set; } = new int[] { 149, 416 };
					public int[] ExamTypeSize { get; set; } = new int[] { 100, 36 };
					public int[] ExamUnitSize { get; set; } = new int[] { 60, 36 };
					public int[] PercentHitScorePadding { get; set; } = new int[] { 20, 20, 20, 20 };
					public int[] ExamOffset { get; set; } = new int[] { 932, -40 };

					public int[] v2PanelX { get; set; } = new int[] { 90, 90, 90 };
					public int[] v2PanelY { get; set; } = new int[] { 385, 495, 605 };
					public int[] v2BaseOffset { get; set; } = new int[] { 260, 17 };
					public int[] v2GaugeOffset { get; set; } = new int[] { 5, 5 };
					public int[] v2AmountOffset { get; set; } = new int[] { 0, 0 };
					public float v2AmountScale { get; set; } = 1f;
					public int[] v2ExamTypeOffset { get; set; } = new int[] { 122, 10 };
					public int[] v2ExamTypeSize { get; set; } = new int[] { 100, 25 };
					public int[] v2ExamRangeOffset { get; set; } = new int[] { 180, 30 };
					public int[] v2ExamRangeNumOffset { get; set; } = new int[] { -15, 3 };
					public int[] v2DanPlateXY { get; set; } = new int[] { 1000, 416 };
					public int[] v2SoulGaugeBoxX { get; set; } = new int[] { 110, 800 };//0%,100%のX座標
					public int v2SoulGaugeBoxY { get; set; } = 70;
					public int v2SoulGaugeBoxPersentWidth { get; set; } = 25;
					public int[] v2SoulGaugeBoxExamTypeOffset { get; set; } = new int[] { 65, 55 };
					public float v2SoulGaugeBoxExamTypeBoxXRatio { get; set; } = 0.7f;
					public int[] v2SoulGaugeBoxExamRangeOffset { get; set; } = new int[] { 300, 50 };
					public int[] v2SmallGaugeOffset { get; set; } = new int[] { 487, 2 };
					public int v2SmallGaugeOffsetYPadding { get; set; } = 32;
					public float v2NumberSmallScale { get; set; } = 0.5f;
				}
				public CTraining Training { get; set; } = new();
				public class CTraining
				{
					public int ScrollTime { get; set; } = 350;
					public int[] ProgressBarXY { get; set; } = { 333, 378 };
					public int GoGoPointY { get; set; } = 396;
					public int JumpPointY { get; set; } = 375;
					public int[] MaxMeasureCountXY { get; set; } = { 284, 377 };
					public int[] CurrentMeasureCountXY { get; set; } = { 254, 370 };
					public int[] SpeedDisplayXY { get; set; } = { 110, 370 };
					public int SmallNumberWidth { get; set; } = 17;
					public int BigNumberWidth { get; set; } = 20;
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
		#region Gauge
		public int Game_Gauge_Rainbow_Ptn;
		public int Game_Gauge_Rainbow_Danc_Ptn;
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
		public int Game_Effect_FireWorks_Timing = 8;
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