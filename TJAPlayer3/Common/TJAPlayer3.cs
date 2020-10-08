using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
using FDK;
using System.Reflection;
using DiscordRPC;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class TJAPlayer3 : Game
	{
		// プロパティ
		#region [ properties ]
		public static readonly string VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, Assembly.GetExecutingAssembly().GetName().Version.ToString().Length - 2);

		public static TJAPlayer3 app
		{
			get;
			private set;
		}
		public static C文字コンソール act文字コンソール
		{
			get;
			private set;
		}
		public static CConfigIni ConfigIni
		{
			get;
			private set;
		}
		public static CDTX[] DTX
		{
			get
			{
				return dtx;
			}
			set
			{
				for (int nPlayer = 0; nPlayer < 2; nPlayer++)
				{
					if ((dtx[nPlayer] != null) && (app != null))
					{
						dtx[nPlayer].On非活性化();
						app.listトップレベルActivities.Remove(dtx[nPlayer]);
					}
				}
				dtx = value;
				for (int nPlayer = 0; nPlayer < 2; nPlayer++)
				{
					if ((dtx[nPlayer] != null) && (app != null))
					{
						app.listトップレベルActivities.Add(dtx[nPlayer]);
					}
				}
			}
		}

		public static bool IsPerformingCalibration;

		public static CFPS FPS
		{
			get;
			private set;
		}

		public static CInput管理 Input管理
		{
			get;
			private set;
		}
		#region [ 入力範囲ms ]
		public static int nPerfect範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nPerfect範囲ms >= 0))
					{
						return c曲リストノード.nPerfect範囲ms;
					}
				}
				return ConfigIni.nヒット範囲ms.Perfect;
			}
		}
		public static int nGreat範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nGreat範囲ms >= 0))
					{
						return c曲リストノード.nGreat範囲ms;
					}
				}
				return ConfigIni.nヒット範囲ms.Great;
			}
		}
		public static int nGood範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nGood範囲ms >= 0))
					{
						return c曲リストノード.nGood範囲ms;
					}
				}
				return ConfigIni.nヒット範囲ms.Good;
			}
		}
		public static int nPoor範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nPoor範囲ms >= 0))
					{
						return c曲リストノード.nPoor範囲ms;
					}
				}
				return ConfigIni.nヒット範囲ms.Poor;
			}
		}
		#endregion
		public static CPad Pad
		{
			get;
			private set;
		}
		public static Random Random
		{
			get;
			private set;
		}
		public static CSkin Skin
		{
			get;
			private set;
		}
		public static CSongs管理 Songs管理
		{
			get;
			set;    // 2012.1.26 yyagi private解除 CStage起動でのdesirialize読み込みのため
		}
		public static CEnumSongs EnumSongs
		{
			get;
			private set;
		}
		public static CActEnumSongs actEnumSongs
		{
			get;
			private set;
		}
		public static CActScanningLoudness actScanningLoudness
		{
			get;
			private set;
		}
		public static CActFlushGPU actFlushGPU
		{
			get;
			private set;
		}

		public static CSound管理 Sound管理
		{
			get;
			private set;
		}

		public static SongGainController SongGainController
		{
			get;
			private set;
		}

		public static SoundGroupLevelController SoundGroupLevelController
		{
			get;
			private set;
		}

		public static CStage起動 stage起動
		{
			get;
			private set;
		}
		public static CStageタイトル stageタイトル
		{
			get;
			private set;
		}
		public static CStageコンフィグ stageコンフィグ
		{
			get;
			private set;
		}
		public static CStage選曲 stage選曲
		{
			get;
			private set;
		}
		public static CStage曲読み込み stage曲読み込み
		{
			get;
			private set;
		}
		public static CStage演奏ドラム画面 stage演奏ドラム画面
		{
			get;
			private set;
		}
		public static CStage結果 stage結果
		{
			get;
			private set;
		}
		public static CStageChangeSkin stageChangeSkin
		{
			get;
			private set;
		}
		public static CStage終了 stage終了
		{
			get;
			private set;
		}
		public static CStageメンテナンス stageメンテ
		{
			get;
			private set;
		}
		public static CStage r現在のステージ = null;
		public static CStage r直前のステージ = null;
		public static string strEXEのあるフォルダ
		{
			get;
			private set;
		}
		public static CTimer Timer
		{
			get;
			private set;
		}

		public bool bApplicationActive
		{
			get 
			{
				return this.Window.Focused;
			}
		}
		public bool b次のタイミングで垂直帰線同期切り替えを行う
		{
			get;
			set;
		}
		public bool b次のタイミングで全画面_ウィンドウ切り替えを行う
		{
			get;
			set;
		}
		public Device Device
		{
			get
			{
				return base.GraphicsDeviceManager.Direct3D9.Device;
			}
		}
		private static Size currentClientSize       // #23510 2010.10.27 add yyagi to keep current window size
		{
			get;
			set;
		}
		//		public static CTimer ct;
		public IntPtr WindowHandle                  // 2012.10.24 yyagi; to add ASIO support
		{
			get { return base.Window.Handle; }
		}

		#endregion

		// コンストラクタ

		public TJAPlayer3()
		{
			TJAPlayer3.app = this;
			this.t起動処理();
		}


		// メソッド

		public void t全画面_ウィンドウモード切り替え()
		{
			DeviceSettings settings = base.GraphicsDeviceManager.CurrentSettings.Clone();
			if ((ConfigIni != null) && (ConfigIni.bウィンドウモード != settings.Windowed))
			{
				settings.Windowed = ConfigIni.bウィンドウモード;

				if (ConfigIni.bウィンドウモード == false)   // #23510 2010.10.27 yyagi: backup current window size before going fullscreen mode
				{
					currentClientSize = this.Window.ClientSize;
					ConfigIni.nウインドウwidth = this.Window.ClientSize.Width;
					ConfigIni.nウインドウheight = this.Window.ClientSize.Height;
					//					FDK.CTaskBar.ShowTaskBar( false );
				}
				base.GraphicsDeviceManager.ChangeDevice(settings);
				if (ConfigIni.bウィンドウモード == true)    // #23510 2010.10.27 yyagi: to resume window size from backuped value
				{
					base.Window.ClientSize =
						new Size(currentClientSize.Width, currentClientSize.Height);
					base.Window.Icon = Properties.Resources.tjap3;
					//					FDK.CTaskBar.ShowTaskBar( true );
				}
			}
		}

		// Game 実装

		protected override void Initialize()
		{
			//			new GCBeep();
			//sw.Start();
			//swlist1 = new List<int>( 8192 );
			//swlist2 = new List<int>( 8192 );
			//swlist3 = new List<int>( 8192 );
			//swlist4 = new List<int>( 8192 );
			//swlist5 = new List<int>( 8192 );
			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnManagedリソースの作成();
			}
		}
		protected override void LoadContent()
		{
			if (ConfigIni.bウィンドウモード)
			{
				if (!this.bマウスカーソル表示中)
				{
					Cursor.Show();
					this.bマウスカーソル表示中 = true;
				}
			}
			else if (this.bマウスカーソル表示中)
			{
				Cursor.Hide();
				this.bマウスカーソル表示中 = false;
			}

			CAction.LoadContentAction(this.Device);

			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnUnmanagedリソースの作成();
			}
		}
		protected override void UnloadContent()
		{
			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnUnmanagedリソースの解放();
			}
		}
		protected override void OnExiting(EventArgs e)
		{
			this.t終了処理();
			base.OnExiting(e);
		}
		protected override void Update(GameTime gameTime)
		{
		}
		protected override void Draw(GameTime gameTime)
		{
			Timer?.t更新();
			CSound管理.rc演奏用タイマ?.t更新();
			Input管理?.tポーリング(this.bApplicationActive, TJAPlayer3.ConfigIni.bバッファ入力);
			FPS?.tカウンタ更新();

			if (this.Device == null)
				return;

			// #xxxxx 2013.4.8 yyagi; sleepの挿入位置を、EndScnene～Present間から、BeginScene前に移動。描画遅延を小さくするため。
			#region [ スリープ ]
			if (ConfigIni.nフレーム毎スリープms >= 0)            // #xxxxx 2011.11.27 yyagi
			{
				Thread.Sleep(ConfigIni.nフレーム毎スリープms);
			}
			#endregion

			CAction.BeginScene(this.Device);

			if (r現在のステージ != null)
			{
				this.n進行描画の戻り値 = (r現在のステージ != null) ? r現在のステージ.On進行描画() : 0;

				CScoreIni scoreIni = null;

				if (Control.IsKeyLocked(Keys.CapsLock))             // #30925 2013.3.11 yyagi; capslock=ON時は、EnumSongsしないようにして、起動負荷とASIOの音切れの関係を確認する
				{                                                       // → songs.db等の書き込み時だと音切れするっぽい
					actEnumSongs.On非活性化();
					EnumSongs.SongListEnumCompletelyDone();
					TJAPlayer3.stage選曲.bIsEnumeratingSongs = false;
				}
				#region [ 曲検索スレッドの起動/終了 ]					// ここに"Enumerating Songs..."表示を集約
				actEnumSongs.On進行描画();                          // "Enumerating Songs..."アイコンの描画
				switch (r現在のステージ.eステージID)
				{
					case CStage.Eステージ.タイトル:
					case CStage.Eステージ.コンフィグ:
					case CStage.Eステージ.選曲:
					case CStage.Eステージ.曲読み込み:
						if (EnumSongs != null)
						{
							#region [ (特定条件時) 曲検索スレッドの起動_開始 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.タイトル &&
								 r直前のステージ.eステージID == CStage.Eステージ.起動 &&
								 this.n進行描画の戻り値 == (int)CStageタイトル.E戻り値.継続 &&
								 !EnumSongs.IsSongListEnumStarted)
							{
								actEnumSongs.On活性化();
								TJAPlayer3.stage選曲.bIsEnumeratingSongs = true;
								EnumSongs.Init(TJAPlayer3.Songs管理.listSongsDB, TJAPlayer3.Songs管理.nSongsDBから取得できたスコア数); // songs.db情報と、取得した曲数を、新インスタンスにも与える
								EnumSongs.StartEnumFromDisk();      // 曲検索スレッドの起動_開始
								if (TJAPlayer3.Songs管理.nSongsDBから取得できたスコア数 == 0)    // もし初回起動なら、検索スレッドのプライオリティをLowestでなくNormalにする
								{
									EnumSongs.ChangeEnumeratePriority(ThreadPriority.Normal);
								}
							}
							#endregion

							#region [ 曲検索の中断と再開 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.選曲 && !EnumSongs.IsSongListEnumCompletelyDone)
							{
								switch (this.n進行描画の戻り値)
								{
									case 0:     // 何もない
										EnumSongs.Resume();                     // #27060 2012.2.6 yyagi 中止していたバックグランド曲検索を再開
										actEnumSongs.On活性化();
										break;

									case 2:     // 曲決定
										EnumSongs.Suspend();                        // #27060 バックグラウンドの曲検索を一時停止
										actEnumSongs.On非活性化();
										break;
								}
							}
							#endregion

							#region [ 曲探索中断待ち待機 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.曲読み込み && !EnumSongs.IsSongListEnumCompletelyDone &&
								EnumSongs.thDTXFileEnumerate != null)                           // #28700 2012.6.12 yyagi; at Compact mode, enumerating thread does not exist.
							{
								EnumSongs.WaitUntilSuspended();                                 // 念のため、曲検索が一時中断されるまで待機
							}
							#endregion

							#region [ 曲検索が完了したら、実際の曲リストに反映する ]
							// CStage選曲.On活性化() に回した方がいいかな？
							if (EnumSongs.IsSongListEnumerated)
							{
								actEnumSongs.On非活性化();
								TJAPlayer3.stage選曲.bIsEnumeratingSongs = false;

								bool bRemakeSongTitleBar = (r現在のステージ.eステージID == CStage.Eステージ.選曲) ? true : false;
								TJAPlayer3.stage選曲.Refresh(EnumSongs.Songs管理, bRemakeSongTitleBar);
								EnumSongs.SongListEnumCompletelyDone();
							}
							#endregion
						}
						break;
				}
				#endregion

				switch (r現在のステージ.eステージID)
				{
					case CStage.Eステージ.何もしない:
						break;

					case CStage.Eステージ.起動:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							r現在のステージ.On非活性化();
							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ タイトル");
							stageタイトル.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stageタイトル;

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.タイトル:
						#region [ *** ]
						//-----------------------------
						switch (this.n進行描画の戻り値)
						{
							case (int)CStageタイトル.E戻り値.GAMESTART:
								#region [ 選曲処理へ ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;
								//-----------------------------
								#endregion
								break;

							case (int)CStageタイトル.E戻り値.CONFIG:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ コンフィグ");
								stageコンフィグ.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageコンフィグ;
								//-----------------------------
								#endregion
								break;

							case (int)CStageタイトル.E戻り値.EXIT:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 終了");
								stage終了.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage終了;
								//-----------------------------
								#endregion
								break;

							case (int)CStageタイトル.E戻り値.MAINTENANCE:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ メンテ");
								stageメンテ.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageメンテ;
								//-----------------------------
								#endregion
								break;
						}

						//this.tガベージコレクションを実行する();		// #31980 2013.9.3 yyagi タイトル画面でだけ、毎フレームGCを実行して重くなっていた問題の修正
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.コンフィグ:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							switch (r直前のステージ.eステージID)
							{
								case CStage.Eステージ.タイトル:
									#region [ *** ]
									//-----------------------------
									r現在のステージ.On非活性化();
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ タイトル");
									stageタイトル.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stageタイトル;

									this.tガベージコレクションを実行する();
									break;
								//-----------------------------
								#endregion

								case CStage.Eステージ.選曲:
									#region [ *** ]
									//-----------------------------
									r現在のステージ.On非活性化();
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ 選曲");
									stage選曲.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stage選曲;

									this.tガベージコレクションを実行する();
									break;
									//-----------------------------
									#endregion
							}
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.選曲:
						#region [ *** ]
						//-----------------------------
						switch (this.n進行描画の戻り値)
						{
							case (int)CStage選曲.E戻り値.タイトルに戻る:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ タイトル");
								stageタイトル.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageタイトル;

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.選曲した:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 曲読み込み");
								stage曲読み込み.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage曲読み込み;

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							//							case (int) CStage選曲.E戻り値.オプション呼び出し:
							#region [ *** ]
							//								//-----------------------------
							//								r現在のステージ.On非活性化();
							//								Trace.TraceInformation( "----------------------" );
							//								Trace.TraceInformation( "■ オプション" );
							//								stageオプション.On活性化();
							//								r直前のステージ = r現在のステージ;
							//								r現在のステージ = stageオプション;
							//
							//								this.tガベージコレクションを実行する();
							//								break;
							//							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.コンフィグ呼び出し:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ コンフィグ");
								stageコンフィグ.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageコンフィグ;

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.スキン変更:

								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ スキン切り替え");
								stageChangeSkin.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageChangeSkin;
								break;
								//-----------------------------
								#endregion
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.曲読み込み:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							TJAPlayer3.Pad.st検知したデバイス.Clear();  // 入力デバイスフラグクリア(2010.9.11)
							r現在のステージ.On非活性化();
							#region [ ESC押下時は、曲の読み込みを中止して選曲画面に戻る ]
							if (this.n進行描画の戻り値 == (int)E曲読込画面の戻り値.読込中止)
							{
								//DTX.t全チップの再生停止();
								if (DTX[0] != null)
									DTX[0].On非活性化();
								Trace.TraceInformation("曲の読み込みを中止しました。");
								this.tガベージコレクションを実行する();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;
								break;
							}
							#endregion

							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 演奏（ドラム画面）");
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage演奏ドラム画面;

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.演奏:
						#region [ *** ]
						//-----------------------------
						//long n1 = FDK.CSound管理.rc演奏用タイマ.nシステム時刻ms;
						//long n2 = FDK.CSound管理.SoundDevice.n経過時間ms;
						//long n3 = FDK.CSound管理.SoundDevice.tmシステムタイマ.nシステム時刻ms;
						//long n4 = FDK.CSound管理.rc演奏用タイマ.n現在時刻;
						//long n5 = FDK.CSound管理.SoundDevice.n経過時間を更新したシステム時刻ms;

						//swlist1.Add( Convert.ToInt32(n1) );
						//swlist2.Add( Convert.ToInt32(n2) );
						//swlist3.Add( Convert.ToInt32( n3 ) );
						//swlist4.Add( Convert.ToInt32( n4 ) );
						//swlist5.Add( Convert.ToInt32( n5 ) );

						switch (this.n進行描画の戻り値)
						{
							case (int)E演奏画面の戻り値.再読込_再演奏:
								#region [ DTXファイルを再読み込みして、再演奏 ]
								DTX[0].t全チップの再生停止();
								DTX[0].On非活性化();
								r現在のステージ.On非活性化();
								stage曲読み込み.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage曲読み込み;
								this.tガベージコレクションを実行する();
								break;
							#endregion

							//case (int) E演奏画面の戻り値.再演奏:
							#region [ 再読み込み無しで、再演奏 ]
							#endregion
							//	break;

							case (int)E演奏画面の戻り値.継続:
								break;

							case (int)E演奏画面の戻り値.演奏中断:
								#region [ 演奏キャンセル ]
								//-----------------------------
								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新("Play canceled");


								DTX[0].t全チップの再生停止();
								DTX[0].On非活性化();
								r現在のステージ.On非活性化();

								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;

								this.tガベージコレクションを実行する();

								break;
							//-----------------------------
							#endregion

							case (int)E演奏画面の戻り値.ステージ失敗:
								#region [ 演奏失敗(StageFailed) ]
								//-----------------------------
								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新("Stage failed");

								DTX[0].t全チップの再生停止();
								DTX[0].On非活性化();
								r現在のステージ.On非活性化();

								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)E演奏画面の戻り値.ステージクリア:
								#region [ 演奏クリア ]
								//-----------------------------
								CScoreIni.C演奏記録[] c演奏記録_Drums = new CScoreIni.C演奏記録[4];
								stage演奏ドラム画面.t演奏結果を格納する(out c演奏記録_Drums[0], 0);
								if (ConfigIni.nPlayerCount == 2)
								{
									stage演奏ドラム画面.t演奏結果を格納する(out c演奏記録_Drums[1], 1);
								}

								double ps = 0.0, gs = 0.0;
								if (!TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] && c演奏記録_Drums[0].n全チップ数 > 0) {
									ps = c演奏記録_Drums[0].db演奏型スキル値;
									gs = c演奏記録_Drums[0].dbゲーム型スキル値;
								}
								string str = "Cleared";
								switch (CScoreIni.t総合ランク値を計算して返す(c演奏記録_Drums[0]))
								{
									case (int)CScoreIni.ERANK.SS:
										str = string.Format("Cleared (SS: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.S:
										str = string.Format("Cleared (S: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.A:
										str = string.Format("Cleared (A: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.B:
										str = string.Format("Cleared (B: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.C:
										str = string.Format("Cleared (C: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.D:
										str = string.Format("Cleared (D: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.E:
										str = string.Format("Cleared (E: {0:F2})", ps);
										break;

									case (int)CScoreIni.ERANK.UNKNOWN:  // #23534 2010.10.28 yyagi add: 演奏チップが0個のとき
										str = "Cleared (No chips)";
										break;
								}

								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新(str);

								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 結果");
								stage結果.st演奏記録[0] = c演奏記録_Drums[0];
								if (ConfigIni.nPlayerCount == 2)
								{
									stage結果.st演奏記録[1] = c演奏記録_Drums[1];
								}
								stage結果.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage結果;

								break;
								//-----------------------------
								#endregion
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.結果:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							//DTX.t全チップの再生一時停止();
							DTX[0].t全チップの再生停止とミキサーからの削除();
							DTX[0].On非活性化();
							r現在のステージ.On非活性化();
							this.tガベージコレクションを実行する();

							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 選曲");
							stage選曲.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage選曲;

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.ChangeSkin:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							r現在のステージ.On非活性化();
							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 選曲");
							stage選曲.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage選曲;
							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.終了:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							base.Exit();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.メンテ:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0) {
							r現在のステージ.On非活性化();
							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 選曲");
							stage選曲.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage選曲;
							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;
				}

				actScanningLoudness.On進行描画();

				if (r現在のステージ != null && r現在のステージ.eステージID != CStage.Eステージ.起動 && TJAPlayer3.Tx.Overlay_Online != null && TJAPlayer3.Tx.Overlay_Offline != null)
				{
					if (Math.Abs(CSound管理.rc演奏用タイマ.nシステム時刻ms - this.前回のシステム時刻ms) > 10000)
					{
						this.前回のシステム時刻ms = CSound管理.rc演奏用タイマ.nシステム時刻ms;
						Task.Factory.StartNew(() =>
						{
							//IPv4 8.8.8.8にPingを送信する(timeout 5000ms)
							PingReply reply = new Ping().Send("8.8.8.8", 5000);
							this.bネットワークに接続中 = reply.Status == IPStatus.Success;
						});
					}
					if (this.bネットワークに接続中)
						TJAPlayer3.Tx.Overlay_Online.t2D描画(app.Device, 0, 0);
					else
						TJAPlayer3.Tx.Overlay_Offline.t2D描画(app.Device, 0, 0);
				}
				// オーバレイを描画する(テクスチャの生成されていない起動ステージは例外
				if (r現在のステージ != null && r現在のステージ.eステージID != CStage.Eステージ.起動 && TJAPlayer3.Tx.Overlay != null)
				{
					TJAPlayer3.Tx.Overlay.t2D描画(app.Device, 0, 0);
				}
			}
			CAction.EndScene(this.Device);  // Present()は game.csのOnFrameEnd()に登録された、GraphicsDeviceManager.game_FrameEnd() 内で実行されるので不要
											// (つまり、Present()は、Draw()完了後に実行される)

			actFlushGPU?.On進行描画();      // Flush GPU	// EndScene()～Present()間 (つまりVSync前) でFlush実行
#if OpenGL
			OpenTK.Graphics.OpenGL.GL.Flush();
#endif

			#region [ 全画面_ウインドウ切り替え ]
			if (this.b次のタイミングで全画面_ウィンドウ切り替えを行う)
			{
				ConfigIni.b全画面モード = !ConfigIni.b全画面モード;
				app.t全画面_ウィンドウモード切り替え();
				this.b次のタイミングで全画面_ウィンドウ切り替えを行う = false;
			}
#endregion
#region [ 垂直基線同期切り替え ]
			if (this.b次のタイミングで垂直帰線同期切り替えを行う)
			{
				bool bIsMaximized = this.Window.IsMaximized;                                            // #23510 2010.11.3 yyagi: to backup current window mode before changing VSyncWait
				currentClientSize = this.Window.ClientSize;                                             // #23510 2010.11.3 yyagi: to backup current window size before changing VSyncWait
				DeviceSettings currentSettings = app.GraphicsDeviceManager.CurrentSettings;
				currentSettings.EnableVSync = ConfigIni.b垂直帰線待ちを行う;
				app.GraphicsDeviceManager.ChangeDevice(currentSettings);
				this.b次のタイミングで垂直帰線同期切り替えを行う = false;
				base.Window.ClientSize = new Size(currentClientSize.Width, currentClientSize.Height);   // #23510 2010.11.3 yyagi: to resume window size after changing VSyncWait
				if (bIsMaximized)
				{
					this.Window.WindowState = FormWindowState.Maximized;                                // #23510 2010.11.3 yyagi: to resume window mode after changing VSyncWait
				}
			}
#endregion
		}

		// その他

#region [ 汎用ヘルパー ]
		//-----------------
		public static CTexture tテクスチャの生成(string fileName)
		{
			return tテクスチャの生成(fileName, false);
		}
		public static CTexture tテクスチャの生成(string fileName, bool b黒を透過する)
		{
			if (app == null)
			{
				return null;
			}
			try
			{
				return new CTexture(app.Device, fileName, b黒を透過する);
			}
			catch (CTextureCreateFailedException e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("テクスチャの生成に失敗しました。({0})", fileName);
				return null;
			}
			catch (FileNotFoundException)
			{
				Trace.TraceWarning("テクスチャファイルが見つかりませんでした。({0})", fileName);
				return null;
			}
		}
		public static CTexture tテクスチャの生成(Bitmap bitmap)
		{
			return tテクスチャの生成(bitmap, false);
		}
		public static CTexture tテクスチャの生成(Bitmap bitmap, bool b黒を透過する)
		{
			if (app == null)
			{
				return null;
			}
			if (bitmap == null)
			{
				Trace.TraceError("テクスチャの生成に失敗しました。(bitmap==null)");
				return null;
			}
			try
			{
				return new CTexture(app.Device, bitmap, b黒を透過する);
			}
			catch (CTextureCreateFailedException e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("テクスチャの生成に失敗しました。(txData)");
				return null;
			}
		}

		public static CTexture ColorTexture(string htmlcolor, int width, int height)//2020.05.31 Mr-Ojii 単色塗りつぶしテクスチャの生成。必要かって？Tile_Black・Tile_Whiteがいらなくなるじゃん。あと、メンテモードの画像生成に便利かなって。
		{
			if (htmlcolor.Length == 7 && htmlcolor.StartsWith("#"))
			{
				Color color = ColorTranslator.FromHtml(htmlcolor);
				Brush brush = new SolidBrush(color);
				return ColorTexture(brush, width, height);
			}
			else
				return ColorTexture(Brushes.Black);
		}
		public static CTexture ColorTexture(string htmlcolor)
		{
			if (htmlcolor.Length == 7 && htmlcolor.StartsWith("#"))
			{
				Color color = ColorTranslator.FromHtml(htmlcolor);
				Brush brush = new SolidBrush(color);
				return ColorTexture(brush);
			}
			else
				return ColorTexture(Brushes.Black);
		}
		public static CTexture ColorTexture(Brush brush)
		{
			return ColorTexture(brush, 64, 64);
		}
		/// <summary>
		/// 単色塗りつぶしテクスチャの生成
		/// </summary>
		/// <param name="brush">ブラシの色とかの指定</param>
		/// <param name="width">幅</param>
		/// <param name="height">高さ</param>
		/// <returns></returns>
		public static CTexture ColorTexture(Brush brush, int width, int height)
		{
			Bitmap bmp = new Bitmap(width, height);
			Graphics gra = Graphics.FromImage(bmp);
			gra.FillRectangle(brush, 0, 0, width, height);
			gra.Dispose();
			return TJAPlayer3.tテクスチャの生成(bmp);
		}

		/// <summary>プロパティ、インデクサには ref は使用できないので注意。</summary>
		public static void t安全にDisposeする<T>(ref T obj) where T : class, IDisposable //2020.06.06 Mr-Ojii twopointzero氏のソースコードをもとに改良
		{
			if (obj == null)
				return;

			obj.Dispose();
			obj = null;
		}
		
		public static void t安全にDisposeする<T>(ref T[] array) where T : class, IDisposable //2020.08.01 Mr-Ojii twopointzero氏のソースコードをもとに追加
		{
			if (array == null)
			{
				return;
			}

			for (var i = 0; i < array.Length; i++)
			{
				array[i]?.Dispose();
				array[i] = null;
			}
		}

		/// <summary>
		/// そのフォルダの連番画像の最大値を返す。
		/// </summary>
		public static int t連番画像の枚数を数える(string ディレクトリ名, string プレフィックス = "", string 拡張子 = ".png")
		{
			int num = 0;
			while (File.Exists(ディレクトリ名 + プレフィックス + num + 拡張子))
			{
				num++;
			}
			return num;
		}

		/// <summary>
		/// そのフォルダの連番フォルダの最大値を返す。
		/// </summary>
		public static int t連番フォルダの個数を数える(string ディレクトリ名, string プレフィックス = "")
		{
			int num = 0;
			while (Directory.Exists(ディレクトリ名 + プレフィックス + num))
			{
				num++;
			}
			return num;
		}

		/// <summary>
		/// 曲名テクスチャの縮小倍率を返す。
		/// </summary>
		/// <param name="cTexture">曲名テクスチャ。</param>
		/// <param name="samePixel">等倍で表示するピクセル数の最大値(デフォルト値:645)</param>
		/// <returns>曲名テクスチャの縮小倍率。そのテクスチャがnullならば一倍(1f)を返す。</returns>
		public static float GetSongNameXScaling(ref CTexture cTexture, int samePixel = 660)
		{
			if (cTexture == null) return 1f;
			float scalingRate = (float)samePixel / (float)cTexture.szテクスチャサイズ.Width;
			if (cTexture.szテクスチャサイズ.Width <= samePixel)
				scalingRate = 1.0f;
			return scalingRate;
		}

		/// <summary>
		/// 難易度を表す数字を列挙体に変換します。
		/// </summary>
		/// <param name="number">難易度を表す数字。</param>
		/// <returns>Difficulty 列挙体</returns>
		public static Difficulty DifficultyNumberToEnum(int number)
		{
			switch (number)
			{
				case 0:
					return Difficulty.Easy;
				case 1:
					return Difficulty.Normal;
				case 2:
					return Difficulty.Hard;
				case 3:
					return Difficulty.Oni;
				case 4:
					return Difficulty.Edit;
				case 5:
					return Difficulty.Tower;
				case 6:
					return Difficulty.Dan;
				default:
					throw new IndexOutOfRangeException();
			}
		}

		//-----------------
#endregion

#region [ private ]
		//-----------------
		private bool bマウスカーソル表示中 = true;
		private bool b終了処理完了済み;
		private bool bネットワークに接続中 = false;
		private long 前回のシステム時刻ms = long.MinValue;
		public static DiscordRpcClient DiscordClient;
		private static CDTX[] dtx = new CDTX[4];

		public static TextureLoader Tx = new TextureLoader();

		private List<CActivity> listトップレベルActivities;
		private int n進行描画の戻り値;
		private MouseButtons mb = System.Windows.Forms.MouseButtons.Left;
		public static DateTime StartupTime
		{
			get;
			private set;
		}

		private void t起動処理()
		{
#region [ strEXEのあるフォルダを決定する ]
			//-----------------
			// BEGIN #23629 2010.11.13 from: デバッグ時は Application.ExecutablePath が ($SolutionDir)/bin/x86/Debug/ などになり System/ の読み込みに失敗するので、カレントディレクトリを採用する。（プロジェクトのプロパティ→デバッグ→作業ディレクトリが有効になる）

			strEXEのあるフォルダ = Path.GetDirectoryName( Assembly.GetEntryAssembly().Location ) + @"\";

			// END #23629 2010.11.13 from
			//-----------------
#endregion
#region [ Config.ini の読込み ]
			//---------------------
			ConfigIni = new CConfigIni();
			string path = strEXEのあるフォルダ + "Config.ini";
			if (File.Exists(path))
			{
				try
				{
					ConfigIni.tファイルから読み込み(path);
				}
				catch (Exception e)
				{
					//ConfigIni = new CConfigIni();	// 存在してなければ新規生成
					Trace.TraceError(e.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (b8d93255-bbe4-4ca3-8264-7ee5175b19f3)");
				}
			}
			this.Window.EnableSystemMenu = TJAPlayer3.ConfigIni.bIsEnabledSystemMenu;   // #28200 2011.5.1 yyagi
																						// 2012.8.22 Config.iniが無いときに初期値が適用されるよう、この設定行をifブロック外に移動

			//---------------------
#endregion
#region [ ログ出力開始 ]
			//---------------------
			Trace.AutoFlush = true;
			if (ConfigIni.bログ出力)
			{ 
				bool log出力ok = false;
				int num = 0;
				while (!log出力ok)
				{
					try
					{
						string logname;
						if (num == 0)
							logname = "TJAPlayer3-f.log";
						else
							logname = "TJAPlayer3-f_" + num.ToString() + ".log";
						Trace.Listeners.Add(new CTraceLogListener(new StreamWriter(System.IO.Path.Combine(strEXEのあるフォルダ, logname), false, new UTF8Encoding(false))));
						log出力ok = true;
					}
					catch (Exception)
					{
						num++;
					}
				}
			}
			Trace.WriteLine("");
			Trace.WriteLine("DTXMania powered by YAMAHA Silent Session Drums");
			Trace.WriteLine(string.Format("Release: {0}", VERSION));
			Trace.WriteLine("");
			Trace.TraceInformation("----------------------");
			Trace.TraceInformation("■ アプリケーションの初期化");
			Trace.TraceInformation("OS Version: " + Environment.OSVersion);
			Trace.TraceInformation("ProcessorCount: " + Environment.ProcessorCount.ToString());
			Trace.TraceInformation("CLR Version: " + Environment.Version.ToString());
			//---------------------
#endregion


#region [ ウィンドウ初期化 ]
			//---------------------
			base.Window.StartPosition = FormStartPosition.Manual;                                                       // #30675 2013.02.04 ikanick add
			base.Window.Location = new Point(ConfigIni.n初期ウィンドウ開始位置X, ConfigIni.n初期ウィンドウ開始位置Y);   // #30675 2013.02.04 ikanick add


			base.Window.Text = "";

			base.Window.ClientSize = new Size(ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight);   // #34510 yyagi 2010.10.31 to change window size got from Config.ini

			if (!ConfigIni.bウィンドウモード)                       // #23510 2010.11.02 yyagi: add; to recover window size in case bootup with fullscreen mode
			{                                                       // #30666 2013.02.02 yyagi: currentClientSize should be always made
				currentClientSize = new Size(ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight);
			}

			base.Window.MaximizeBox = true;                         // #23510 2010.11.04 yyagi: to support maximizing window
			base.Window.FormBorderStyle = FormBorderStyle.Sizable;  // #23510 2010.10.27 yyagi: changed from FixedDialog to Sizable, to support window resize
																	// #30666 2013.02.02 yyagi: moved the code to t全画面_ウインドウモード切り替え()
			base.Window.ShowIcon = true;
			base.Window.Icon = global::TJAPlayer3.Properties.Resources.tjap3;
			base.Window.KeyDown += new KeyEventHandler(this.Window_KeyDown);
			base.Window.MouseUp += new MouseEventHandler(this.Window_MouseUp);
			base.Window.MouseDoubleClick += new MouseEventHandler(this.Window_MouseDoubleClick);    // #23510 2010.11.13 yyagi: to go fullscreen mode
			base.Window.ResizeEnd += new EventHandler(this.Window_ResizeEnd);                       // #23510 2010.11.20 yyagi: to set resized window size in Config.ini
			//---------------------
#endregion
#region [ Direct3D9 デバイスの生成 ]
			//---------------------
			DeviceSettings settings = new DeviceSettings();
			settings.Windowed = ConfigIni.bウィンドウモード;

			settings.BackBufferWidth = GameWindowSize.Width;
			settings.BackBufferHeight = GameWindowSize.Height;
			//			settings.BackBufferCount = 3;
			settings.EnableVSync = ConfigIni.b垂直帰線待ちを行う;
			//			settings.BackBufferFormat = Format.A8R8G8B8;
			//			settings.MultisampleType = MultisampleType.FourSamples;
			//			settings.MultisampleQuality = 4;
			//			settings.MultisampleType = MultisampleType.None;
			//			settings.MultisampleQuality = 0;

			try
			{
				base.GraphicsDeviceManager.ChangeDevice(settings);
			}
			catch (DeviceCreationException e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show(e.ToString(), "DTXMania failed to boot: DirectX9 Initialize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(-1);
			}

			base.IsFixedTimeStep = false;
			//			base.TargetElapsedTime = TimeSpan.FromTicks( 10000000 / 75 );
			base.Window.ClientSize = new Size(ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight);   // #23510 2010.10.31 yyagi: to recover window size. width and height are able to get from Config.ini.
			base.InactiveSleepTime = TimeSpan.FromMilliseconds((float)(ConfigIni.n非フォーカス時スリープms));  // #23568 2010.11.3 yyagi: to support valiable sleep value when !IsActive
																									// #23568 2010.11.4 ikanick changed ( 1 -> ConfigIni )

			actFlushGPU = new CActFlushGPU();
			//---------------------
#endregion

			DTX[0] = null;
			DTX[1] = null;

#region [ Skin の初期化 ]
			//---------------------
			Trace.TraceInformation("スキンの初期化を行います。");
			Trace.Indent();
			try
			{
				Skin = new CSkin(TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName, false);
				TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName = TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName(true);    // 旧指定のSkinフォルダが消滅していた場合に備える
				Trace.TraceInformation("スキンの初期化を完了しました。");
			}
			catch
			{
				Trace.TraceInformation("スキンの初期化に失敗しました。");
				throw;
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
			//-----------
#region [ Timer の初期化 ]
			//---------------------
			Trace.TraceInformation("タイマの初期化を行います。");
			Trace.Indent();
			try
			{
				Timer = new CTimer();
				Trace.TraceInformation("タイマの初期化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
			//-----------

#region [ FPS カウンタの初期化 ]
			//---------------------
			Trace.TraceInformation("FPSカウンタの初期化を行います。");
			Trace.Indent();
			try
			{
				FPS = new CFPS();
				Trace.TraceInformation("FPSカウンタを生成しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ act文字コンソールの初期化 ]
			//---------------------
			Trace.TraceInformation("文字コンソールの初期化を行います。");
			Trace.Indent();
			try
			{
				act文字コンソール = new C文字コンソール();
				Trace.TraceInformation("文字コンソールを生成しました。");
				act文字コンソール.On活性化();
				Trace.TraceInformation("文字コンソールを活性化しました。");
				Trace.TraceInformation("文字コンソールの初期化を完了しました。");
			}
			catch (Exception exception)
			{
				Trace.TraceError(exception.ToString());
				Trace.TraceError("文字コンソールの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ Input管理 の初期化 ]
			//---------------------
			Trace.TraceInformation("DirectInput, MIDI入力の初期化を行います。");
			Trace.Indent();
			try
			{
				Input管理 = new CInput管理(base.Window.Handle);
				foreach (IInputDevice device in Input管理.list入力デバイス)
				{
					if ((device.e入力デバイス種別 == E入力デバイス種別.Joystick) && !ConfigIni.dicJoystick.ContainsValue(device.GUID))
					{
						int key = 0;
						while (ConfigIni.dicJoystick.ContainsKey(key))
						{
							key++;
						}
						ConfigIni.dicJoystick.Add(key, device.GUID);
					}
				}
				base.Window.KeyDown += new KeyEventHandler(Input管理.KeyDownEvent);
				base.Window.KeyUp += new KeyEventHandler(Input管理.KeyUpEvent);
				Trace.TraceInformation("DirectInput の初期化を完了しました。");
			}
			catch
			{
				Trace.TraceError("DirectInput, MIDI入力の初期化に失敗しました。");
				throw;
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ Pad の初期化 ]
			//---------------------
			Trace.TraceInformation("パッドの初期化を行います。");
			Trace.Indent();
			try
			{
				Pad = new CPad(ConfigIni, Input管理);
				Trace.TraceInformation("パッドの初期化を完了しました。");
			}
			catch (Exception exception3)
			{
				Trace.TraceError(exception3.ToString());
				Trace.TraceError("パッドの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ Sound管理 の初期化 ]
			//---------------------
			Trace.TraceInformation("サウンドデバイスの初期化を行います。");
			Trace.Indent();
			try
			{
				ESoundDeviceType soundDeviceType;
				switch (TJAPlayer3.ConfigIni.nSoundDeviceType)
				{
					case 0:
						soundDeviceType = ESoundDeviceType.OpenAL;
						break;
					case 1:
						soundDeviceType = ESoundDeviceType.ASIO;
						break;
					case 2:
						soundDeviceType = ESoundDeviceType.ExclusiveWASAPI;
						break;
					case 3:
						soundDeviceType = ESoundDeviceType.SharedWASAPI;
						break;
					default:
						soundDeviceType = ESoundDeviceType.Unknown;
						break;
				}
				Sound管理 = new CSound管理(base.Window.Handle,
											soundDeviceType,
											TJAPlayer3.ConfigIni.nWASAPIBufferSizeMs,
											// CDTXMania.ConfigIni.nASIOBufferSizeMs,
											0,
											TJAPlayer3.ConfigIni.nASIODevice,
											TJAPlayer3.ConfigIni.bUseOSTimer
				);
				//Sound管理 = FDK.CSound管理.Instance;
				//Sound管理.t初期化( soundDeviceType, 0, 0, CDTXMania.ConfigIni.nASIODevice, base.Window.Handle );


				Trace.TraceInformation("Initializing loudness scanning, song gain control, and sound group level control...");
				Trace.Indent();
				try
				{
					actScanningLoudness = new CActScanningLoudness();
					actScanningLoudness.On活性化();
					LoudnessMetadataScanner.ScanningStateChanged +=
						(_, args) => actScanningLoudness.bIsActivelyScanning = args.IsActivelyScanning;
					LoudnessMetadataScanner.StartBackgroundScanning();

					SongGainController = new SongGainController();
					ConfigIniToSongGainControllerBinder.Bind(ConfigIni, SongGainController);

					SoundGroupLevelController = new SoundGroupLevelController(CSound.listインスタンス);
					ConfigIniToSoundGroupLevelControllerBinder.Bind(ConfigIni, SoundGroupLevelController);
				}
				finally
				{
					Trace.Unindent();
					Trace.TraceInformation("Initialized loudness scanning, song gain control, and sound group level control.");
				}

				ShowWindowTitleWithSoundType();
				CSound管理.bIsTimeStretch = TJAPlayer3.ConfigIni.bTimeStretch;
				Sound管理.nMasterVolume = TJAPlayer3.ConfigIni.nMasterVolume;
				//FDK.CSound管理.bIsMP3DecodeByWindowsCodec = CDTXMania.ConfigIni.bNoMP3Streaming;
				Trace.TraceInformation("サウンドデバイスの初期化を完了しました。");
			}
			catch (Exception e)
			{
				throw new NullReferenceException("サウンドデバイスがひとつも有効になっていないため、サウンドデバイスの初期化ができませんでした。", e);
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ Songs管理 の初期化 ]
			//---------------------
			Trace.TraceInformation("曲リストの初期化を行います。");
			Trace.Indent();
			try
			{
				Songs管理 = new CSongs管理();
				//				Songs管理_裏読 = new CSongs管理();
				EnumSongs = new CEnumSongs();
				actEnumSongs = new CActEnumSongs();
				Trace.TraceInformation("曲リストの初期化を完了しました。");
			}
			catch (Exception e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("曲リストの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
#endregion
#region [ Random の初期化 ]
			//---------------------
			Random = new Random((int)Timer.nシステム時刻ms);
			//---------------------
#endregion
#region [ ステージの初期化 ]
			//---------------------
			r現在のステージ = null;
			r直前のステージ = null;
			stage起動 = new CStage起動();
			stageタイトル = new CStageタイトル();
			//			stageオプション = new CStageオプション();
			stageコンフィグ = new CStageコンフィグ();
			stage選曲 = new CStage選曲();
			stage曲読み込み = new CStage曲読み込み();
			stage演奏ドラム画面 = new CStage演奏ドラム画面();
			stage結果 = new CStage結果();
			stageChangeSkin = new CStageChangeSkin();
			stage終了 = new CStage終了();
			stageメンテ = new CStageメンテナンス();
			this.listトップレベルActivities = new List<CActivity>();
			this.listトップレベルActivities.Add(actEnumSongs);
			this.listトップレベルActivities.Add(act文字コンソール);
			this.listトップレベルActivities.Add(stage起動);
			this.listトップレベルActivities.Add(stageタイトル);
			//			this.listトップレベルActivities.Add( stageオプション );
			this.listトップレベルActivities.Add(stageコンフィグ);
			this.listトップレベルActivities.Add(stage選曲);
			this.listトップレベルActivities.Add(stage曲読み込み);
			this.listトップレベルActivities.Add(stage演奏ドラム画面);
			this.listトップレベルActivities.Add(stage結果);
			this.listトップレベルActivities.Add(stageChangeSkin);
			this.listトップレベルActivities.Add(stage終了);
			this.listトップレベルActivities.Add(stageメンテ);
			this.listトップレベルActivities.Add(actFlushGPU);
			//---------------------
			#endregion
#region Discordの処理
			DiscordClient = new DiscordRpcClient("692578108997632051");
			DiscordClient?.Initialize();
			StartupTime = DateTime.UtcNow;
			DiscordClient?.SetPresence(new RichPresence()
			{
				Details = "",
				State = "Startup",
				Timestamps = new Timestamps(TJAPlayer3.StartupTime),
				Assets = new Assets()
				{
					LargeImageKey = "tjaplayer3-f",
					LargeImageText = "Ver." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
				}
			});
#endregion


			Trace.TraceInformation("アプリケーションの初期化を完了しました。");


#region [ 最初のステージの起動 ]
			//---------------------
			Trace.TraceInformation("----------------------");
			Trace.TraceInformation("■ 起動");

			r現在のステージ = stage起動;

			r現在のステージ.On活性化();

			//---------------------
#endregion
		}

		public void ShowWindowTitleWithSoundType()
		{
			string delay = "";
			if (Sound管理.GetCurrentSoundDeviceType() != "OpenAL")
			{
				delay = "(" + Sound管理.GetSoundDelay() + "ms)";
			}
			AssemblyName asmApp = Assembly.GetExecutingAssembly().GetName();
			base.Window.Text = asmApp.Name + " Ver." + VERSION + " (" + Sound管理.GetCurrentSoundDeviceType() + delay + ")";
		}

		public void ChangeWindowTitle(string Name, bool StringInitialize = true, bool Concat = true) {
			if(StringInitialize)
				this.ShowWindowTitleWithSoundType();
			if (Concat)
				Name = base.Window.Text + Name;
			base.Window.Text = Name;
		}

		private void t終了処理()
		{
			if( !this.b終了処理完了済み )
			{
				Trace.TraceInformation( "----------------------" );
				Trace.TraceInformation( "■ アプリケーションの終了" );
#region [ 曲検索の終了処理 ]
				//---------------------
				if ( actEnumSongs != null )
				{
					Trace.TraceInformation( "曲検索actの終了処理を行います。" );
					Trace.Indent();
					try
					{
						actEnumSongs.On非活性化();
						actEnumSongs= null;
						Trace.TraceInformation( "曲検索actの終了処理を完了しました。" );
					}
					catch ( Exception e )
					{
						Trace.TraceError( e.ToString() );
						Trace.TraceError( "曲検索actの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ 現在のステージの終了処理 ]
				//---------------------
				if( TJAPlayer3.r現在のステージ != null && TJAPlayer3.r現在のステージ.b活性化してる )		// #25398 2011.06.07 MODIFY FROM
				{
					Trace.TraceInformation( "現在のステージを終了します。" );
					Trace.Indent();
					try
					{
						r現在のステージ.On非活性化();
						Trace.TraceInformation( "現在のステージの終了処理を完了しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
#region Discordの処理
				DiscordClient?.Dispose();
#endregion
#region [ 曲リストの終了処理 ]
				//---------------------
				if (Songs管理 != null)
				{
					Trace.TraceInformation( "曲リストの終了処理を行います。" );
					Trace.Indent();
					try
					{
						Songs管理 = null;
						Trace.TraceInformation( "曲リストの終了処理を完了しました。" );
					}
					catch( Exception exception )
					{
						Trace.TraceError( exception.ToString() );
						Trace.TraceError( "曲リストの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region TextureLoaderの処理
				Tx.DisposeTexture();
#endregion
#region [ スキンの終了処理 ]
				//---------------------
				if (Skin != null)
				{
					Trace.TraceInformation( "スキンの終了処理を行います。" );
					Trace.Indent();
					try
					{
						Skin.Dispose();
						Skin = null;
						Trace.TraceInformation( "スキンの終了処理を完了しました。" );
					}
					catch( Exception exception2 )
					{
						Trace.TraceError( exception2.ToString() );
						Trace.TraceError( "スキンの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ サウンドの終了処理 ]
				//---------------------
				if (Sound管理 != null)
				{
					Trace.TraceInformation( "サウンド の終了処理を行います。" );
					Trace.Indent();
					try
					{
						Sound管理.Dispose();
						Sound管理 = null;
						Trace.TraceInformation( "サウンド の終了処理を完了しました。" );
					}
					catch( Exception exception3 )
					{
						Trace.TraceError( exception3.ToString() );
						Trace.TraceError( "サウンド の終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ パッドの終了処理 ]
				//---------------------
				if (Pad != null)
				{
					Trace.TraceInformation( "パッドの終了処理を行います。" );
					Trace.Indent();
					try
					{
						Pad = null;
						Trace.TraceInformation( "パッドの終了処理を完了しました。" );
					}
					catch( Exception exception4 )
					{
						Trace.TraceError( exception4.ToString() );
						Trace.TraceError( "パッドの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ DirectInput, MIDI入力の終了処理 ]
				//---------------------
				if (Input管理 != null)
				{
					Trace.TraceInformation( "DirectInput, MIDI入力の終了処理を行います。" );
					Trace.Indent();
					try
					{
						Input管理.Dispose();
						Input管理 = null;
						Trace.TraceInformation( "DirectInput, MIDI入力の終了処理を完了しました。" );
					}
					catch( Exception exception5 )
					{
						Trace.TraceError( exception5.ToString() );
						Trace.TraceError( "DirectInput, MIDI入力の終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ 文字コンソールの終了処理 ]
				//---------------------
				if (act文字コンソール != null)
				{
					Trace.TraceInformation( "文字コンソールの終了処理を行います。" );
					Trace.Indent();
					try
					{
						act文字コンソール.On非活性化();
						act文字コンソール = null;
						Trace.TraceInformation( "文字コンソールの終了処理を完了しました。" );
					}
					catch( Exception exception6 )
					{
						Trace.TraceError( exception6.ToString() );
						Trace.TraceError( "文字コンソールの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
#endregion
#region [ FPSカウンタの終了処理 ]
				//---------------------
				Trace.TraceInformation("FPSカウンタの終了処理を行います。");
				Trace.Indent();
				try
				{
					if( FPS != null )
					{
						FPS = null;
					}
					Trace.TraceInformation( "FPSカウンタの終了処理を完了しました。" );
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
#endregion
#region [ タイマの終了処理 ]
				//---------------------
				Trace.TraceInformation("タイマの終了処理を行います。");
				Trace.Indent();
				try
				{
					if( Timer != null )
					{
						Timer.Dispose();
						Timer = null;
						Trace.TraceInformation( "タイマの終了処理を完了しました。" );
					}
					else
					{
						Trace.TraceInformation( "タイマは使用されていません。" );
					}
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
#endregion
#region [ Config.iniの出力 ]
				//---------------------
				Trace.TraceInformation("Config.ini を出力します。");
				string str = strEXEのあるフォルダ + "Config.ini";
				Trace.Indent();
				try
				{
					ConfigIni.t書き出し( str );
					Trace.TraceInformation( "保存しました。({0})", str );	
				}
				catch( Exception e )
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceError( "Config.ini の出力に失敗しました。({0})", str );
				}
				finally
				{
					Trace.Unindent();
				}

				Trace.TraceInformation("Deinitializing loudness scanning, song gain control, and sound group level control...");
				Trace.Indent();
				try
				{
					SoundGroupLevelController = null;
					SongGainController = null;
					LoudnessMetadataScanner.StopBackgroundScanning(joinImmediately: true);
					actScanningLoudness.On非活性化();
					actScanningLoudness = null;
				}
				finally
				{
					Trace.Unindent();
					Trace.TraceInformation("Deinitialized loudness scanning, song gain control, and sound group level control.");
				}

				ConfigIni = null;

				//---------------------
#endregion
#region [ DirectXの終了処理 ]
				base.GraphicsDeviceManager.Dispose();
#endregion
				Trace.TraceInformation( "アプリケーションの終了処理を完了しました。" );

				this.b終了処理完了済み = true;
			}
		}
		private CScoreIni tScoreIniへBGMAdjustとHistoryとPlayCountを更新(string str新ヒストリ行)
		{
			bool bIsUpdatedDrums;
			string strFilename = DTX[0].strファイル名の絶対パス + ".score.ini";
			CScoreIni ini = new CScoreIni( strFilename );
			if( !File.Exists( strFilename ) )
			{
				ini.stファイル.Title = DTX[0].TITLE;
				ini.stファイル.Name = DTX[0].strファイル名;
				ini.stファイル.Hash = CScoreIni.tファイルのMD5を求めて返す( DTX[0].strファイル名の絶対パス );
				for( int i = 0; i < 2; i++ )
				{
					ini.stセクション[i].nPerfectになる範囲ms = nPerfect範囲ms;
					ini.stセクション[i].nGreatになる範囲ms = nGreat範囲ms;
					ini.stセクション[i].nGoodになる範囲ms = nGood範囲ms;
					ini.stセクション[i].nPoorになる範囲ms = nPoor範囲ms;
				}
			}
			ini.stファイル.BGMAdjust = DTX[0].nBGMAdjust;
			CScoreIni.t更新条件を取得する(out bIsUpdatedDrums);
			
			if( bIsUpdatedDrums )
			{
				ini.stファイル.PlayCountDrums++;
			}
			ini.tヒストリを追加する( str新ヒストリ行 );
			
			stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.演奏回数 = ini.stファイル.PlayCountDrums;
			for (int j = 0; j < ini.stファイル.History.Length; j++)
			{
				stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.演奏履歴[j] = ini.stファイル.History[j];

			}
			
			if( ConfigIni.bScoreIniを出力する )
			{
				ini.t書き出し( strFilename );
			}

			return ini;
		}
		private void tガベージコレクションを実行する()
		{
			GC.Collect(GC.MaxGeneration);
			GC.WaitForPendingFinalizers();
			GC.Collect(GC.MaxGeneration);
		}

		public void RefleshSkin()
		{
			Trace.TraceInformation("スキン変更:" + TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName(false));

			TJAPlayer3.act文字コンソール.On非活性化();

			TJAPlayer3.Skin.Dispose();
			TJAPlayer3.Skin = null;
			TJAPlayer3.Skin = new CSkin(TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName, false);


			TJAPlayer3.Tx.DisposeTexture();
			TJAPlayer3.Tx.LoadTexture();

			TJAPlayer3.act文字コンソール.On活性化();
		}
#region [ Windowイベント処理 ]
		//-----------------
		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode == Keys.Menu )
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if ( ( e.KeyCode == Keys.Return ) && e.Alt )
			{
				if ( ConfigIni != null )
				{
					ConfigIni.bウィンドウモード = !ConfigIni.bウィンドウモード;
					this.t全画面_ウィンドウモード切り替え();
				}
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else
			{
				for ( int i = 0; i < 0x10; i++ )
				{
					if ( ConfigIni.KeyAssign.System.Capture[ i ].コード > 0 &&
						 e.KeyCode == DeviceConstantConverter.KeyToKeyCode( (SlimDXKeys.Key) ConfigIni.KeyAssign.System.Capture[ i ].コード ) )
					{
						// Debug.WriteLine( "capture: " + string.Format( "{0:2x}", (int) e.KeyCode ) + " " + (int) e.KeyCode );
						string strFullPath =
						   Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "Capture_img" );
						strFullPath = Path.Combine( strFullPath, DateTime.Now.ToString( "yyyyMMddHHmmss" ) + ".png" );
						CSaveScreen.CSaveFromDevice(TJAPlayer3.app.Device, strFullPath);
					}
				}
			}
		}
		private void Window_MouseUp( object sender, MouseEventArgs e )
		{
			mb = e.Button;
		}

		private void Window_MouseDoubleClick( object sender, MouseEventArgs e)	// #23510 2010.11.13 yyagi: to go full screen mode
		{
			if ( mb.Equals(MouseButtons.Left) && ConfigIni.bIsAllowedDoubleClickFullscreen )	// #26752 2011.11.27 yyagi
			{
				ConfigIni.bウィンドウモード = false;
				this.t全画面_ウィンドウモード切り替え();
			}
		}
		private void Window_ResizeEnd(object sender, EventArgs e)				// #23510 2010.11.20 yyagi: to get resized window size
		{
			if ( ConfigIni.bウィンドウモード )
			{
				ConfigIni.n初期ウィンドウ開始位置X = base.Window.Location.X;	// #30675 2013.02.04 ikanick add
				ConfigIni.n初期ウィンドウ開始位置Y = base.Window.Location.Y;	//
			}

			ConfigIni.nウインドウwidth = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Width : currentClientSize.Width;	// #23510 2010.10.31 yyagi add
			ConfigIni.nウインドウheight = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Height : currentClientSize.Height;
		}
#endregion
#endregion
	}
}
