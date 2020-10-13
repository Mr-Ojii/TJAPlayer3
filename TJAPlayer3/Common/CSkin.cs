using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using FDK;
using System.Drawing;
using System.Linq;

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
			public string strファイル名 = "";
			public bool b再生中
			{
				get
				{
					if (this.rSound[1 - this.n次に鳴るサウンド番号] == null)
						return false;

					return this.rSound[1 - this.n次に鳴るサウンド番号].b再生中;
				}
			}
			public int n位置_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.n位置;
				}
				set
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.n位置 = value;
				}
			}
			public int n位置_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.n位置;
				}
				set
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.n位置 = value;
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
					return sound.n総演奏時間ms;
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
					return sound.n総演奏時間ms;
				}
			}


			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="strファイル名"></param>
			/// <param name="bループ"></param>
			/// <param name="b排他"></param>
			/// <param name="bCompact対象"></param>
			public Cシステムサウンド(string strファイル名, bool bループ, bool b排他, ESoundGroup soundGroup)
			{
				this.strファイル名 = strファイル名;
				this.bループ = bループ;
				this.b排他 = b排他;
				_soundGroup = soundGroup;
				this.b読み込み未試行 = true;
			}


			// メソッド

			public void t読み込み()
			{
				this.b読み込み未試行 = false;
				this.b読み込み成功 = false;
				if (string.IsNullOrEmpty(this.strファイル名))
					throw new InvalidOperationException("ファイル名が無効です。");

				if (!File.Exists(CSkin.Path(this.strファイル名)))
				{
					Trace.TraceWarning($"ファイルが存在しません。: {this.strファイル名}");
					return;
				}
				////				for( int i = 0; i < 2; i++ )		// #27790 2012.3.10 yyagi 2回読み出しを、1回読みだし＋1回メモリコピーに変更
				////				{
				//                    try
				//                    {
				//                        this.rSound[ 0 ] = CDTXMania.Sound管理.tサウンドを生成する( CSkin.Path( this.strファイル名 ) );
				//                    }
				//                    catch
				//                    {
				//                        this.rSound[ 0 ] = null;
				//                        throw;
				//                    }
				//                    if ( this.rSound[ 0 ] == null )	// #28243 2012.5.3 yyagi "this.rSound[ 0 ].bストリーム再生する"時もCloneするようにし、rSound[1]がnullにならないよう修正→rSound[1]の再生正常化
				//                    {
				//                        this.rSound[ 1 ] = null;
				//                    }
				//                    else
				//                    {
				//                        this.rSound[ 1 ] = ( CSound ) this.rSound[ 0 ].Clone();	// #27790 2012.3.10 yyagi add: to accelerate loading chip sounds
				//                        CDTXMania.Sound管理.tサウンドを登録する( this.rSound[ 1 ] );	// #28243 2012.5.3 yyagi add (登録漏れによりストリーム再生処理が発生していなかった)
				//                    }

				////				}

				for (int i = 0; i < 2; i++)     // 一旦Cloneを止めてASIO対応に専念
				{
					try
					{
						this.rSound[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(this.strファイル名), _soundGroup);
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
						t読み込み();
					}
					catch (Exception e)
					{
						Trace.TraceError(e.ToString());
						Trace.TraceError("例外が発生しましたが処理を継続します。 (17668977-4686-4aa7-b3f0-e0b9a44975b8)");
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
				if (TJAPlayer3.Sound管理.GetCurrentSoundDeviceType() != "DirectShow")
				{
					for (int i = 0; i < 2; i++)
					{
						if (this.rSound[i] != null)
						{
							TJAPlayer3.Sound管理.RemoveMixer(this.rSound[i]);
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
					for (int i = 0; i < 2; i++)
					{
						if (this.rSound[i] != null)
						{
							TJAPlayer3.Sound管理.tサウンドを破棄する(this.rSound[i]);
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


		// プロパティ

		public Cシステムサウンド bgmコンフィグ画面 = null;
		public Cシステムサウンド bgm起動画面 = null;
		public Cシステムサウンド bgm選曲画面 = null;
		public Cシステムサウンド soundSTAGEFAILED音 = null;
		public Cシステムサウンド soundカーソル移動音 = null;
		public Cシステムサウンド soundゲーム開始音 = null;
		public Cシステムサウンド soundゲーム終了音 = null;
		public Cシステムサウンド soundタイトル音 = null;
		public Cシステムサウンド sound曲読込開始音 = null;
		public Cシステムサウンド sound決定音 = null;
		public Cシステムサウンド sound取消音 = null;
		public Cシステムサウンド sound変更音 = null;
		//add
		public Cシステムサウンド bgmリザルト = null;
		public Cシステムサウンド bgmリザルトループ = null;
		public Cシステムサウンド sound曲決定音 = null;
		public Cシステムサウンド sound成績発表 = null;
		public Cシステムサウンド soundDanするカッ = null;
		public Cシステムサウンド sound特訓再生音 = null;
		public Cシステムサウンド sound特訓停止音 = null;
		public Cシステムサウンド sound特訓スクロール音 = null;
		public Cシステムサウンド sound選曲スキップ音 = null;
		public Cシステムサウンド sound音色選択音 = null;
		public Cシステムサウンド sound難易度選択音 = null;
		public Cシステムサウンド sound自己ベスト更新音 = null;
		public Cシステムサウンド sound回転音 = null;

		//public Cシステムサウンド soundRed = null;
		//public Cシステムサウンド soundBlue = null;
		public Cシステムサウンド soundBalloon = null;


		public readonly int nシステムサウンド数 = (int)Eシステムサウンド.Count;
		public Cシステムサウンド this[int index]
		{
			get
			{
				switch (index)
				{
					case (int)Eシステムサウンド.SOUNDカーソル移動音:
						return this.soundカーソル移動音;

					case (int)Eシステムサウンド.SOUND決定音:
						return this.sound決定音;

					case (int)Eシステムサウンド.SOUND変更音:
						return this.sound変更音;

					case (int)Eシステムサウンド.SOUND取消音:
						return this.sound取消音;

					case (int)Eシステムサウンド.SOUNDステージ失敗音:
						return this.soundSTAGEFAILED音;

					case (int)Eシステムサウンド.SOUNDゲーム開始音:
						return this.soundゲーム開始音;

					case (int)Eシステムサウンド.SOUNDゲーム終了音:
						return this.soundゲーム終了音;

					case (int)Eシステムサウンド.SOUND曲読込開始音:
						return this.sound曲読込開始音;

					case (int)Eシステムサウンド.SOUNDタイトル音:
						return this.soundタイトル音;

					case (int)Eシステムサウンド.BGM起動画面:
						return this.bgm起動画面;

					case (int)Eシステムサウンド.BGMコンフィグ画面:
						return this.bgmコンフィグ画面;

					case (int)Eシステムサウンド.BGM選曲画面:
						return this.bgm選曲画面;

					case (int)Eシステムサウンド.SOUND風船:
						return this.soundBalloon;

					case (int)Eシステムサウンド.SOUND曲決定音:
						return this.sound曲決定音;

					case (int)Eシステムサウンド.SOUND成績発表:
						return this.sound成績発表;

					case (int)Eシステムサウンド.SOUNDDANするカッ:
						return this.soundDanするカッ;

					case (int)Eシステムサウンド.SOUND特訓再生:
						return this.sound特訓再生音;

					case (int)Eシステムサウンド.SOUND特訓停止:
						return this.sound特訓停止音;

					case (int)Eシステムサウンド.SOUND特訓スクロール:
						return this.sound特訓スクロール音;

					case (int)Eシステムサウンド.SOUND選曲スキップ:
						return this.sound選曲スキップ音;

					case (int)Eシステムサウンド.SOUND音色選択:
						return this.sound音色選択音;

					case (int)Eシステムサウンド.SOUND難易度選択:
						return this.sound難易度選択音;

					case (int)Eシステムサウンド.SOUND自己ベスト更新:
						return this.sound自己ベスト更新音;

					case (int)Eシステムサウンド.SOUND回転音:
						return this.sound回転音;
				}
				throw new IndexOutOfRangeException();
			}
		}


		// スキンの切り替えについて___
		//
		// _スキンの種類は大きく分けて2種類。Systemスキンとboxdefスキン。
		// 　前者はSystem/フォルダにユーザーが自らインストールしておくスキン。
		// 　後者はbox.defで指定する、曲データ制作者が提示するスキン。
		//
		// _Config画面で、2種のスキンを区別無く常時使用するよう設定することができる。
		// _box.defの#SKINPATH 設定により、boxdefスキンを一時的に使用するよう設定する。
		// 　(box.defの効果の及ばない他のmuxic boxでは、当該boxdefスキンの有効性が無くなる)
		//
		// これを実現するために___
		// _Systemスキンの設定情報と、boxdefスキンの設定情報は、分離して持つ。
		// 　(strSystem～～ と、strBoxDef～～～)
		// _Config画面からは前者のみ書き換えできるようにし、
		// 　選曲画面からは後者のみ書き換えできるようにする。(SetCurrent...())
		// _読み出しは両者から行えるようにすると共に
		// 　選曲画面用に二種の情報を区別しない読み出し方法も提供する(GetCurrent...)

		private object lockBoxDefSkin;
		public static bool bUseBoxDefSkin = true;                       // box.defからのスキン変更を許容するか否か

		public string strSystemSkinRoot = null;
		public string[] strSystemSkinSubfolders = null;     // List<string>だとignoreCaseな検索が面倒なので、配列に逃げる :-)
		private string[] _strBoxDefSkinSubfolders = null;
		public string[] strBoxDefSkinSubfolders
		{
			get
			{
				lock (lockBoxDefSkin)
				{
					return _strBoxDefSkinSubfolders;
				}
			}
			set
			{
				lock (lockBoxDefSkin)
				{
					_strBoxDefSkinSubfolders = value;
				}
			}
		}           // 別スレッドからも書き込みアクセスされるため、スレッドセーフなアクセス法を提供

		private static string strSystemSkinSubfolderFullName;           // Config画面で設定されたスキン
		private static string strBoxDefSkinSubfolderFullName = "";      // box.defで指定されているスキン

		/// <summary>
		/// スキンパス名をフルパスで取得する
		/// </summary>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		/// <returns></returns>
		public string GetCurrentSkinSubfolderFullName(bool bFromUserConfig)
		{
			if (!bUseBoxDefSkin || bFromUserConfig == true || strBoxDefSkinSubfolderFullName == "")
			{
				return strSystemSkinSubfolderFullName;
			}
			else
			{
				return strBoxDefSkinSubfolderFullName;
			}
		}
		/// <summary>
		/// スキンパス名をフルパスで設定する
		/// </summary>
		/// <param name="value">スキンパス名</param>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		public void SetCurrentSkinSubfolderFullName(string value, bool bFromUserConfig)
		{
			if (bFromUserConfig)
			{
				strSystemSkinSubfolderFullName = value;
			}
			else
			{
				strBoxDefSkinSubfolderFullName = value;
			}
		}


		// コンストラクタ
		public CSkin(string _strSkinSubfolderFullName, bool _bUseBoxDefSkin)
		{
			lockBoxDefSkin = new object();
			strSystemSkinSubfolderFullName = _strSkinSubfolderFullName;
			bUseBoxDefSkin = _bUseBoxDefSkin;
			InitializeSkinPathRoot();
			ReloadSkinPaths();
			PrepareReloadSkin();
			SEloader(out this.SENames);
		}
		public CSkin()
		{
			lockBoxDefSkin = new object();
			InitializeSkinPathRoot();
			bUseBoxDefSkin = true;
			ReloadSkinPaths();
			PrepareReloadSkin();
			SEloader(out this.SENames);
		}
		private string InitializeSkinPathRoot()
		{
			strSystemSkinRoot = System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar);
			return strSystemSkinRoot;
		}


		/// <summary>
		/// 音色用文字列の読み込み用
		/// </summary>
		public void SEloader(out string[] Name)
		{
			this.SECount = TJAPlayer3.t連番フォルダの個数を数える(CSkin.Path(@"Sounds\Taiko\"));
			string strファイル名 = CSkin.Path(@"Sounds\Taiko\SElist.csv");

			if (!File.Exists(strファイル名))
			{
				string[] splitstr = new string[this.SECount];
				for (int i = 0; i < this.SECount; i++)
					splitstr[i] = "無名";
				Name = splitstr;
			}
			else
			{
				Encoding ファイルenc = CJudgeTextEncoding.JudgeFileEncoding(strファイル名);
				StreamReader reader = new StreamReader(strファイル名, ファイルenc);
				string str = reader.ReadToEnd();
				reader.Close();
				str = str.Replace(',', '\n');
				string[] splitstr = str.Split('\n');

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
				Name = splitstr;
			}

			return;
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
				(strBoxDefSkinSubfolderFullName == "") ?
				strSystemSkinSubfolderFullName :
				strBoxDefSkinSubfolderFullName
			);

			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (this[i] != null && this[i].b読み込み成功)
				{
					this[i].t停止する();
					this[i].Dispose();
				}
			}
			this.soundカーソル移動音 = new Cシステムサウンド(@"Sounds\Move.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound決定音 = new Cシステムサウンド(@"Sounds\Decide.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound変更音 = new Cシステムサウンド(@"Sounds\Change.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound取消音 = new Cシステムサウンド(@"Sounds\Cancel.ogg", false, false, ESoundGroup.SoundEffect);
			this.soundSTAGEFAILED音 = new Cシステムサウンド(@"Sounds\Stage failed.ogg", false, true, ESoundGroup.Voice);
			this.soundゲーム開始音 = new Cシステムサウンド(@"Sounds\Game start.ogg", false, false, ESoundGroup.Voice);
			this.soundゲーム終了音 = new Cシステムサウンド(@"Sounds\Game end.ogg", false, true, ESoundGroup.Voice);
			this.sound曲読込開始音 = new Cシステムサウンド(@"Sounds\Now loading.ogg", false, true, ESoundGroup.Unknown);
			this.soundタイトル音 = new Cシステムサウンド(@"Sounds\Title.ogg", false, true, ESoundGroup.SongPlayback);
			this.bgm起動画面 = new Cシステムサウンド(@"Sounds\Setup BGM.ogg", true, true, ESoundGroup.SongPlayback);
			this.bgmコンフィグ画面 = new Cシステムサウンド(@"Sounds\Config BGM.ogg", true, true, ESoundGroup.SongPlayback);
			this.bgm選曲画面 = new Cシステムサウンド(@"Sounds\Select BGM.ogg", true, true, ESoundGroup.SongPreview);

			this.soundBalloon = new Cシステムサウンド(@"Sounds\balloon.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound曲決定音 = new Cシステムサウンド(@"Sounds\SongDecide.ogg", false, false, ESoundGroup.Voice);
			this.sound成績発表 = new Cシステムサウンド(@"Sounds\ResultIn.ogg", false, false, ESoundGroup.Voice);
			this.soundDanするカッ = new Cシステムサウンド(@"Sounds\Dan_Select.ogg", false, false, ESoundGroup.SoundEffect);

			this.sound特訓再生音 = new Cシステムサウンド(@"Sounds\Resume.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound特訓停止音 = new Cシステムサウンド(@"Sounds\Pause.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound特訓スクロール音 = new Cシステムサウンド(@"Sounds\Scroll.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound選曲スキップ音 = new Cシステムサウンド(@"Sounds\Skip.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound音色選択音 = new Cシステムサウンド(@"Sounds\Timbre.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound難易度選択音 = new Cシステムサウンド(@"Sounds\DifficultySelect.ogg", false, false, ESoundGroup.SoundEffect);
			this.sound自己ベスト更新音 = new Cシステムサウンド(@"Sounds\NewRecord.ogg", false, false, ESoundGroup.Voice);
			this.sound回転音 = new Cシステムサウンド(@"Sounds\Rotate.ogg", false, false, ESoundGroup.SoundEffect);

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
					cシステムサウンド.t読み込み();
					Trace.TraceInformation("システムサウンドを読み込みました。({0})", cシステムサウンド.strファイル名);
				}
				catch (FileNotFoundException e)
				{
					Trace.TraceWarning(e.ToString());
					Trace.TraceWarning("システムサウンドが存在しません。({0})", cシステムサウンド.strファイル名);
				}
				catch (Exception e)
				{
					Trace.TraceWarning(e.ToString());
					Trace.TraceWarning("システムサウンドの読み込みに失敗しました。({0})", cシステムサウンド.strファイル名);
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
				if (tempSkinSubfolders[i][tempSkinSubfolders[i].Length - 1] != System.IO.Path.DirectorySeparatorChar)
				{
					tempSkinSubfolders[i] += System.IO.Path.DirectorySeparatorChar;
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

			#region [ 現在のSkinパスがbox.defスキンをCONFIG指定していた場合のために、最初にこれが有効かチェックする。有効ならこれを使う。 ]
			if (bIsValid(strSystemSkinSubfolderFullName) &&
				Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
				StringComparer.InvariantCultureIgnoreCase) < 0)
			{
				strBoxDefSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
				return;
			}
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
			string tempSkinPath_default = System.IO.Path.Combine(strSystemSkinRoot, "Default" + System.IO.Path.DirectorySeparatorChar);
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
			if (strBoxDefSkinSubfolderFullName == "" || !bUseBoxDefSkin)
			{
				return System.IO.Path.Combine(strSystemSkinSubfolderFullName, strファイルの相対パス);
			}
			else
			{
				return System.IO.Path.Combine(strBoxDefSkinSubfolderFullName, strファイルの相対パス);
			}
		}

		/// <summary>
		/// フルパス名を与えると、スキン名として、ディレクトリ名末尾の要素を返す
		/// 例: C:\foo\bar\ なら、barを返す
		/// </summary>
		/// <param name="skinpath">スキンが格納されたパス名(フルパス)</param>
		/// <returns>スキン名</returns>
		public static string GetSkinName(string skinPathFullName)
		{
			if (skinPathFullName != null)
			{
				if (skinPathFullName == "")     // 「box.defで未定義」用
					skinPathFullName = strSystemSkinSubfolderFullName;
				string[] tmp = skinPathFullName.Split(System.IO.Path.DirectorySeparatorChar);
				return tmp[tmp.Length - 2];     // ディレクトリ名の最後から2番目の要素がスキン名(最後の要素はnull。元stringの末尾が\なので。)
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
			foreach (string b in strBoxDefSkinSubfolders)
			{
				if (GetSkinName(b) == skinName)
					return b;
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
			filePathTitle = System.IO.Path.Combine(skinPathFullName, @"Graphics\1_Title\Background.png");
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

		/// <summary>
		/// 変数の初期化
		/// </summary>
		public void tSkinConfigInit()
		{
			this.eDiffDispMode = E難易度表示タイプ.mtaikoに画像で表示;
			this.b現在のステージ数を表示しない = false;
		}

		public void tReadSkinConfig()
		{
			var str = "";
			LoadSkinConfigFromFile(Path(@"SkinConfig.ini"), ref str);
			this.t文字列から読み込み(str);

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
							work += nowLine + Environment.NewLine;
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

							if (strCommand == "Name")
							{
								this.Skin_Name = strParam;
							}
							else if (strCommand == "Version")
							{
								this.Skin_Version = strParam;
							}
							else if (strCommand == "Creator")
							{
								this.Skin_Creator = strParam;
							}
							#endregion

							#region 背景(スクロール)
							else if (strCommand == nameof(Background_Scroll_Y))
							{
								this.Background_Scroll_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion

							#region[ 演奏 ]
							//-----------------------------
							else if (strCommand == "ScrollFieldP1Y")
							{
								this.nScrollFieldY[0] = C変換.n値を文字列から取得して返す(strParam, 192);
							}
							else if (strCommand == "ScrollFieldP2Y")
							{
								this.nScrollFieldY[1] = C変換.n値を文字列から取得して返す(strParam, 192);
							}
							else if (strCommand == "SENotesP1Y")
							{
								this.nSENotesY[0] = C変換.n値を文字列から取得して返す(strParam, this.nSENotesY[0]);
							}
							else if (strCommand == "SENotesP2Y")
							{
								this.nSENotesY[1] = C変換.n値を文字列から取得して返す(strParam, this.nSENotesY[1]);
							}
							else if (strCommand == "JudgePointP1Y")
							{
								this.nJudgePointY[0] = C変換.n値を文字列から取得して返す(strParam, this.nJudgePointY[0]);
							}
							else if (strCommand == "JudgePointP2Y")
							{
								this.nJudgePointY[1] = C変換.n値を文字列から取得して返す(strParam, this.nJudgePointY[1]);
							}

							else if (strCommand == "DiffDispMode")
							{
								this.eDiffDispMode = (E難易度表示タイプ)C変換.n値を文字列から取得して範囲内に丸めて返す(strParam, 0, 2, (int)this.eDiffDispMode);
							}
							else if (strCommand == "NowStageDisp")
							{
								this.b現在のステージ数を表示しない = C変換.bONorOFF(strParam[0]);
							}

							//-----------------------------
							#endregion

							#region 新・SkinConfig
							#region Config
							else if (strCommand == nameof(Config_ItemText_Correction_X))
							{
								Config_ItemText_Correction_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Config_ItemText_Correction_Y))
							{
								Config_ItemText_Correction_Y = int.Parse(strParam);
							}
							#endregion
							#region SongSelect
							else if (strCommand == nameof(SongSelect_Overall_Y))
							{
								SongSelect_Overall_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongSelect_NamePlate_X))
							{
								SongSelect_NamePlate_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_NamePlate_Y))
							{
								SongSelect_NamePlate_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_Auto_X))
							{
								SongSelect_Auto_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_Auto_Y))
							{
								SongSelect_Auto_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_ForeColor_JPOP))
							{
								SongSelect_ForeColor_JPOP = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_Anime))
							{
								SongSelect_ForeColor_Anime = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_VOCALOID))
							{
								SongSelect_ForeColor_VOCALOID = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_Children))
							{
								SongSelect_ForeColor_Children = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_Variety))
							{
								SongSelect_ForeColor_Variety = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_Classic))
							{
								SongSelect_ForeColor_Classic = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_GameMusic))
							{
								SongSelect_ForeColor_GameMusic = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_ForeColor_Namco))
							{
								SongSelect_ForeColor_Namco = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_JPOP))
							{
								SongSelect_BackColor_JPOP = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_Anime))
							{
								SongSelect_BackColor_Anime = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_VOCALOID))
							{
								SongSelect_BackColor_VOCALOID = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_Children))
							{
								SongSelect_BackColor_Children = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_Variety))
							{
								SongSelect_BackColor_Variety = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_Classic))
							{
								SongSelect_BackColor_Classic = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_GameMusic))
							{
								SongSelect_BackColor_GameMusic = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_BackColor_Namco))
							{
								SongSelect_BackColor_Namco = ColorTranslator.FromHtml(strParam);
							}
							else if (strCommand == nameof(SongSelect_CorrectionX_Chara))
							{
								SongSelect_CorrectionX_Chara = strParam.Split(',').ToArray();
							}
							else if (strCommand == nameof(SongSelect_CorrectionY_Chara))
							{
								SongSelect_CorrectionY_Chara = strParam.Split(',').ToArray();
							}
							else if (strCommand == nameof(SongSelect_CorrectionX_Chara_Value))
							{
								SongSelect_CorrectionX_Chara_Value = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_CorrectionY_Chara_Value))
							{
								SongSelect_CorrectionY_Chara_Value = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(SongSelect_Rotate_Chara))
							{
								SongSelect_Rotate_Chara = strParam.Split(',').ToArray();
							}
							else if (strCommand == nameof(SongSelect_BackBoxText_Y_Diff))
							{
								SongSelect_BackBoxText_Y_Diff = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongSelect_Box_Center_Header_Y_Diff))
							{
								SongSelect_Box_Center_Header_Y_Diff = int.Parse(strParam);
							}

							#region Difficulty
							else if (strCommand == nameof(Difficulty_Bar_Center_X_WH_WH_Y_Y))
							{
								Difficulty_Bar_Center_X_WH_WH_Y_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_Bar_XY))
							{
								Difficulty_Bar_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_Bar_Padding))
							{
								Difficulty_Bar_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_BarEtc_XY))
							{
								Difficulty_BarEtc_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_BarEtc_Padding))
							{
								Difficulty_BarEtc_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_Anc_XY))
							{
								Difficulty_Anc_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_AncEtc_XY))
							{
								Difficulty_AncEtc_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_AncBox_XY))
							{
								Difficulty_AncBox_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_AncBoxEtc_XY))
							{
								Difficulty_AncBoxEtc_XY = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Difficulty_Anc_Padding))
							{
								Difficulty_Anc_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_AncEtc_Padding))
							{
								Difficulty_Anc_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_AncBox_Padding))
							{
								Difficulty_Anc_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_AncBoxEtc_Padding))
							{
								Difficulty_Anc_Padding = int.Parse(strParam);
							}
							else if (strCommand == nameof(Difficulty_Mark_Y))
							{
								Difficulty_Mark_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(ChangeSE_Box_X))
							{
								ChangeSE_Box_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(ChangeSE_Box_Y))
							{
								ChangeSE_Box_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(PlayOption_Box_X))
							{
								PlayOption_Box_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(PlayOption_Box_Y))
							{
								PlayOption_Box_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(PlayOption_Box_Section_Y))
							{
								PlayOption_Box_Section_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(PlayOption_Name_XY_Diff))
							{
								PlayOption_Name_XY_Diff = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(PlayOption_List_XY_Diff))
							{
								PlayOption_List_XY_Diff = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion

							#endregion
							#region SongLoading
							else if (strCommand == nameof(SongLoading_Plate_X))
							{
								SongLoading_Plate_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Plate_Y))
							{
								SongLoading_Plate_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_X))
							{
								SongLoading_Title_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_Y))
							{
								SongLoading_Title_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_X))
							{
								SongLoading_SubTitle_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_Y))
							{
								SongLoading_SubTitle_Y = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_Title_FontSize))
							{
								if (int.Parse(strParam) > 0)
									SongLoading_Title_FontSize = int.Parse(strParam);
							}
							else if (strCommand == nameof(SongLoading_SubTitle_FontSize))
							{
								if (int.Parse(strParam) > 0)
									SongLoading_SubTitle_FontSize = int.Parse(strParam);
							}
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
							else if (strCommand == nameof(SongLoading_Plate_ScreenBlend))
							{
								SongLoading_Plate_ScreenBlend = C変換.bONorOFF(strParam[0]);
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
							else if (strCommand == nameof(Game_Notes_Anime))
							{
								Game_Notes_Anime = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_StageText))
							{
								Game_StageText = strParam;
							}
							else if (strCommand == nameof(Game_RollColorMode))
							{
								Game_RollColorMode = (RollColorMode)int.Parse(strParam);
							}
							else if (strCommand == nameof(Game_JudgeFrame_AddBlend))
							{
								Game_JudgeFrame_AddBlend = C変換.bONorOFF(strParam[0]);
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
							#region Chara
							else if (strCommand == nameof(Game_Chara_X))
							{
								Game_Chara_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Y))
							{
								Game_Chara_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Balloon_X))
							{
								Game_Chara_Balloon_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Balloon_Y))
							{
								Game_Chara_Balloon_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Balloon_Timer))
							{
								Game_Chara_Balloon_Timer = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Balloon_Delay))
							{
								Game_Chara_Balloon_Delay = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Balloon_FadeOut))
							{
								Game_Chara_Balloon_FadeOut = strParam.Split(',').Select(int.Parse).ToArray();
							}
							// パターン数の設定はTextureLoader.csで反映されます。
							else if (strCommand == nameof(Game_Chara_Motion_Normal))
							{
								Game_Chara_Motion_Normal[0] = strParam;
							}
							else if (strCommand == nameof(Game_Chara_Motion_Clear))
							{
								Game_Chara_Motion_Clear[0] = strParam;
							}
							else if (strCommand == nameof(Game_Chara_Motion_GoGo))
							{
								Game_Chara_Motion_GoGo[0] = strParam;
							}
							else if (strCommand == "Game_Chara_Motion_Normal_2P")
							{
								Game_Chara_Motion_Normal[1] = strParam;
							}
							else if (strCommand == "Game_Chara_Motion_Clear_2P")
							{
								Game_Chara_Motion_Clear[1] = strParam;
							}
							else if (strCommand == "Game_Chara_Motion_GoGo_2P")
							{
								Game_Chara_Motion_GoGo[1] = strParam;
							}
							else if (strCommand == nameof(Game_Chara_Beat_Normal))
							{
								Game_Chara_Beat_Normal = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Beat_Clear))
							{
								Game_Chara_Beat_Clear = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Chara_Beat_GoGo))
							{
								Game_Chara_Beat_GoGo = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion
							#region Dancer
							else if (strCommand == nameof(Game_Dancer_X))
							{
								this.Game_Dancer_X = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Dancer_Y))
							{
								this.Game_Dancer_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}
							else if (strCommand == nameof(Game_Dancer_Motion))
							{
								Game_Dancer_Motion = strParam;
							}
							// Game_Dancer_PtnはTextrueLoader.csで反映されます。
							else if (strCommand == nameof(Game_Dancer_Beat))
							{
								ParseInt32(value => Game_Dancer_Beat = value);
							}
							else if (strCommand == nameof(Game_Dancer_Gauge))
							{
								this.Game_Dancer_Gauge = strParam.Split(',').Select(int.Parse).ToArray();
							}
							#endregion
							#region Mob
							else if (strCommand == nameof(Game_Mob_Beat))
							{
								ParseInt32(value => Game_Mob_Beat = value);
							}
							else if (strCommand == nameof(Game_Mob_Ptn_Beat))
							{
								ParseInt32(value => Game_Mob_Ptn_Beat = value);
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
								Game_Taiko_Combo_Ex_IsJumping = C変換.bONorOFF(strParam[0]);
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
								Game_Effect_GoGoSplash_Rotate = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_Timer))
							{
								Game_Effect_GoGoSplash_Timer = int.Parse(strParam);
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
								Game_Effect_FlyingNotes_IsUsingEasing = C変換.bONorOFF(strParam[0]);
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
								Game_Effect_HitExplosion_AddBlend = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_Effect_HitExplosionBig_AddBlend))
							{
								Game_Effect_HitExplosionBig_AddBlend = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_Effect_FireWorks_AddBlend))
							{
								Game_Effect_FireWorks_AddBlend = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_Effect_Fire_AddBlend))
							{
								Game_Effect_Fire_AddBlend = C変換.bONorOFF(strParam[0]);
							}
							else if (strCommand == nameof(Game_Effect_GoGoSplash_AddBlend))
							{
								Game_Effect_GoGoSplash_AddBlend = C変換.bONorOFF(strParam[0]);
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
								Game_DanC_Y = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_DanC_Size))
							{
								Game_DanC_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_DanC_Padding))
							{
								ParseInt32(value => Game_DanC_Padding = value);
							}

							else if (strCommand == nameof(Game_DanC_Offset))
							{
								Game_DanC_Offset = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_DanC_Number_Size))
							{
								Game_DanC_Number_Size = strParam.Split(',').Select(int.Parse).ToArray();
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
							else if (strCommand == nameof(Game_DanC_ExamRange_Size))
							{
								Game_DanC_ExamRange_Size = strParam.Split(',').Select(int.Parse).ToArray();
							}

							else if (strCommand == nameof(Game_DanC_ExamRange_Padding))
							{
								ParseInt32(value => Game_DanC_ExamRange_Padding = value);
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
							#region Font
							else if (strCommand == nameof(Font_Edge_Ratio)) //Config画面や簡易メニューのフォントについて(rhimm)
							{
								if (int.Parse(strParam) > 0)
									Font_Edge_Ratio = int.Parse(strParam);
							}
							else if (strCommand == nameof(Font_Edge_Ratio_Vertical)) //TITLEやSUBTITLEなど、縦に書かれることのあるフォントについて(rhimm)
							{
								if (int.Parse(strParam) > 0)
									Font_Edge_Ratio_Vertical = int.Parse(strParam);
							}
							else if (strCommand == nameof(Text_Correction_X))
							{
								Text_Correction_X = int.Parse(strParam);
							}
							else if (strCommand == nameof(Text_Correction_Y))
							{
								Text_Correction_Y = int.Parse(strParam);
							}
							#endregion
							#endregion
						}
						continue;
					}
					catch (Exception exception)
					{
						Trace.TraceError(exception.ToString());
						Trace.TraceError("例外が発生しましたが処理を継続します。 (6a32cc37-1527-412e-968a-512c1f0135cd)");
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

		#region [ private ]
		//-----------------
		private bool bDisposed済み;
		//-----------------
		#endregion

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

		public E難易度表示タイプ eDiffDispMode;
		public bool b現在のステージ数を表示しない;

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
		#endregion

		public enum RollColorMode
		{
			None, // PS4, Switchなど
			All, // 旧筐体(旧作含む)
			WithoutStart // 新筐体
		}
		public enum ReferencePoint //テクスチャ描画の基準点を変更可能にするための値(rhimm)
		{
			Center,
			Left,
			Right
		}

		#region 新・SkinConfig
		#region General
		public string Skin_Name = "Unknown";
		public string Skin_Version = "Unknown";
		public string Skin_Creator = "Unknown";
		#endregion
		#region Config
		public int Config_ItemText_Correction_X = 0;
		public int Config_ItemText_Correction_Y = 0;
		#endregion
		#region SongSelect
		public int SongSelect_Overall_Y = 123;
		public int[] SongSelect_NamePlate_X = new int[] { 60, 950 };
		public int[] SongSelect_NamePlate_Y = new int[] { 650, 650 };
		public int[] SongSelect_Auto_X = new int[] { 60, 950 };
		public int[] SongSelect_Auto_Y = new int[] { 650, 650 };
		public Color SongSelect_ForeColor_JPOP = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_Anime = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_VOCALOID = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_Children = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_Variety = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_Classic = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_GameMusic = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_ForeColor_Namco = ColorTranslator.FromHtml("#FFFFFF");
		public Color SongSelect_BackColor_JPOP = ColorTranslator.FromHtml("#01455B");
		public Color SongSelect_BackColor_Anime = ColorTranslator.FromHtml("#9D3800");
		public Color SongSelect_BackColor_VOCALOID = ColorTranslator.FromHtml("#5B6278");
		public Color SongSelect_BackColor_Children = ColorTranslator.FromHtml("#99001F");
		public Color SongSelect_BackColor_Variety = ColorTranslator.FromHtml("#366600");
		public Color SongSelect_BackColor_Classic = ColorTranslator.FromHtml("#875600");
		public Color SongSelect_BackColor_GameMusic = ColorTranslator.FromHtml("#412080");
		public Color SongSelect_BackColor_Namco = ColorTranslator.FromHtml("#980E00");
		public string[] SongSelect_CorrectionX_Chara = { "ここにX座標を補正したい文字をカンマで区切って記入" };
		public string[] SongSelect_CorrectionY_Chara = { "ここにY座標を補正したい文字をカンマで区切って記入" };
		public int[] SongSelect_CorrectionX_Chara_Value;
		public int[] SongSelect_CorrectionY_Chara_Value;
		public string[] SongSelect_Rotate_Chara = { "ここに90℃回転させたい文字をカンマで区切って記入" };
		public int SongSelect_BackBoxText_Y_Diff = 0;
		public int SongSelect_Box_Center_Header_Y_Diff = 0;
		public int[] SongSelect_Counter_XY = { 1145, 55 };

		#region[Difficulty]
		public int[] Difficulty_Bar_Center_X_WH_WH_Y_Y = new int[7] { 643, 387, 439, 880, 540, 125, 25 };
		public int[] Difficulty_Bar_XY = new int[2] { 440, 90 };
		public int Difficulty_Bar_Padding = 100;
		public int[] Difficulty_BarEtc_XY = new int[2] { 225, 150 };
		public int Difficulty_BarEtc_Padding = 75;

		public int[] Difficulty_Anc_XY = new int[2] { 441, -10 };
		public int Difficulty_Anc_Padding = 100;
		public int[] Difficulty_AncBox_XY = new int[2] { 441, 138 };
		public int Difficulty_AncBox_Padding = 100;

		public int[] Difficulty_AncEtc_XY = new int[2] { 210, 0 };
		public int Difficulty_AncEtc_Padding = 75;
		public int[] Difficulty_AncBoxEtc_XY = new int[2] { 210, 105 };
		public int Difficulty_AncBoxEtc_Padding = 75;

		public int Difficulty_Mark_Y = 600;

		public int[] ChangeSE_Box_X = { 220, 1050 };
		public int[] ChangeSE_Box_Y = { 740, 740 };
		public int[] PlayOption_Box_X = { 220, 1050 };
		public int[] PlayOption_Box_Y = { 750, 750 };
		public int[] PlayOption_Box_Section_Y = { 0, 72, 118 };
		public int[] PlayOption_Name_XY_Diff = { -150, -2 };
		public int[] PlayOption_List_XY_Diff = { 90, -2 };
		#endregion
		#endregion
		#region SongLoading
		public int SongLoading_Plate_X = 640;
		public int SongLoading_Plate_Y = 360;
		public int SongLoading_Title_X = 640;
		public int SongLoading_Title_Y = 340;
		public int SongLoading_SubTitle_X = 640;
		public int SongLoading_SubTitle_Y = 390;
		public int SongLoading_Title_FontSize = 30;
		public int SongLoading_SubTitle_FontSize = 22;
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
		public bool SongLoading_Plate_ScreenBlend = true;

		#endregion
		#region Game
		public bool Game_Notes_Anime = false;
		public string Game_StageText = "1曲目";
		public RollColorMode Game_RollColorMode = RollColorMode.All;
		public bool Game_JudgeFrame_AddBlend = true;
		#region Chara
		public int[] Game_Chara_X = new int[] { 0, 0 };
		public int[] Game_Chara_Y = new int[] { 0, 537 };
		public int[] Game_Chara_Balloon_X = new int[] { 240, 240, 0, 0 };
		public int[] Game_Chara_Balloon_Y = new int[] { 0, 297, 0, 0 };
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
		public string[] Game_Chara_Motion_Normal = { "0", "0" },
			Game_Chara_Motion_Clear = { "0", "0" },
			Game_Chara_Motion_GoGo = { "0", "0" };
		public int[] Game_Chara_Beat_Normal = { 1, 1 };
		public int[] Game_Chara_Beat_Clear = { 2, 2 };
		public int[] Game_Chara_Beat_GoGo = { 2, 2 };
		public int[] Game_Chara_Balloon_Timer = { 28, 28 };
		public int[] Game_Chara_Balloon_Delay = { 500, 500 };
		public int[] Game_Chara_Balloon_FadeOut = { 84, 84 };
		#endregion
		#region Dancer
		public int[] Game_Dancer_X = new int[] { 640, 430, 856, 215, 1070 };
		public int[] Game_Dancer_Y = new int[] { 500, 500, 500, 500, 500 };
		public string Game_Dancer_Motion = "0";
		public int Game_Dancer_Ptn = 0;
		public int Game_Dancer_Beat = 8;
		public int[] Game_Dancer_Gauge = new int[] { 0, 20, 40, 60, 80 };
		#endregion
		#region Mob
		public int Game_Mob_Ptn = 0;
		public int Game_Mob_Beat,
			Game_Mob_Ptn_Beat = 1;
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
		public string Game_Lyric_FontName = "MS UI Gothic";
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
		public int[] Game_Effect_GoGoSplash = new int[] { 300, 400, 30 };
		public int[] Game_Effect_GoGoSplash_X = new int[] { 120, 300, 520, 760, 980, 1160 };
		public int[] Game_Effect_GoGoSplash_Y = new int[] { 740, 730, 720, 720, 730, 740 };
		public bool Game_Effect_GoGoSplash_Rotate = true;
		public int Game_Effect_GoGoSplash_Timer = 18;
		// super-flying-notes AioiLight
		public int[] Game_Effect_FlyingNotes_StartPoint_X = new int[] { 414, 414 };
		public int[] Game_Effect_FlyingNotes_StartPoint_Y = new int[] { 260, 434 };
		public int[] Game_Effect_FlyingNotes_EndPoint_X = new int[] { 1222, 1222 }; // 1P, 2P
		public int[] Game_Effect_FlyingNotes_EndPoint_Y = new int[] { 164, 554 };

		public int Game_Effect_FlyingNotes_Sine = 220;
		public bool Game_Effect_FlyingNotes_IsUsingEasing = true;
		public int Game_Effect_FlyingNotes_Timer = 3;
		public int[] Game_Effect_FireWorks = new int[] { 180, 180, 30 };
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
		public int[] Game_DanC_Y = new int[] { 473, 419, 365 };
		public int[] Game_DanC_Size = new int[] { 956, 92 };
		public int Game_DanC_Padding = 5;
		public int[] Game_DanC_Offset = new int[] { 15, 17 };
		public int[] Game_DanC_Number_Size = new int[] { 50, 62 };
		public int Game_DanC_Number_Padding = 50;
		public float Game_DanC_Number_Small_Scale = 0.5f;
		public int Game_DanC_Number_Small_Padding = 26;
		public int[] Game_DanC_Number_XY = new int[] { 218, 610 };
		public int[] Game_DanC_Number_Small_Number_Offset = new int[] { 178, 43 };
		public int[] Game_DanC_ExamType_Size = new int[] { 100, 36 };
		public int[] Game_DanC_ExamRange_Size = new int[] { 60, 36 };
		public int Game_DanC_ExamRange_Padding = 46;
		public int[] Game_DanC_Percent_Hit_Score_Padding = new int[] { 20, 20, 20 };
		public int[] Game_DanC_ExamUnit_Size = new int[] { 30, 36 };
		public int[] Game_DanC_Exam_Offset = new int[] { 932, 17 };
		public int[] Game_DanC_Dan_Plate = new int[] { 149, 416 };
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

		public Color Result_MusicName_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		public Color Result_StageText_ForeColor = ColorTranslator.FromHtml("#FFFFFF");
		//public Color Result_StageText_ForeColor_Red = ColorTranslator.FromHtml("#FFFFFF");
		public Color Result_MusicName_BackColor = ColorTranslator.FromHtml("#000000");
		public Color Result_StageText_BackColor = ColorTranslator.FromHtml("#000000");
		//public Color Result_StageText_BackColor_Red = ColorTranslator.FromHtml("#FF0000");

		public int[] Result_NamePlate_X = new int[] { 260, 260 };
		public int[] Result_NamePlate_Y = new int[] { 96, 390 };

		public int[] Result_Dan = new int[] { 500, 500 };
		public int[] Result_Dan_XY = new int[] { 100, 0 };
		public int[] Result_Dan_Plate_XY = new int[] { 149, 416 };

		public int[] Result_Crown_X = new int[] { 400, 400 };
		public int[] Result_Crown_Y = new int[] { 250, 544 };
		public int Result_RotateInterval = 50;
		#endregion
		#region Font
		public int Font_Edge_Ratio = 30;
		public int Font_Edge_Ratio_Vertical = 30;
		public int Text_Correction_X = 0;
		public int Text_Correction_Y = 0;
		#endregion
		public int SECount = 0;
		public int[] NowSENum = { 0, 0 };
		public string[] SENames;
		#endregion
	}
}