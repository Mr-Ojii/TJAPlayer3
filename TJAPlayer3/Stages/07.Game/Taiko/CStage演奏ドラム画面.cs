using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using FDK;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CStage演奏ドラム画面 : CStage演奏画面共通
	{
		// コンストラクタ

		public CStage演奏ドラム画面()
		{
			base.eステージID = CStage.Eステージ.演奏;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			base.list子Activities.Add( this.actCombo = new CAct演奏Combo共通() );
			base.list子Activities.Add( this.actDANGER = new CAct演奏DrumsDanger() );
			base.list子Activities.Add( this.actChipFireD = new CAct演奏DrumsチップファイアD() );
			base.list子Activities.Add( this.Rainbow = new Rainbow() );
			base.list子Activities.Add( this.actGauge = new CAct演奏Drumsゲージ() );
			base.list子Activities.Add( this.actJudgeString = new CAct演奏Drums判定文字列() );
			base.list子Activities.Add( this.actTaikoLaneFlash = new TaikoLaneFlash() );
			base.list子Activities.Add( this.actScore = new CAct演奏Drumsスコア() );
			base.list子Activities.Add( this.act譜面スクロール速度 = new CAct演奏スクロール速度() );
			base.list子Activities.Add( this.actAVI = new CAct演奏AVI() );
			base.list子Activities.Add( this.actPanel = new CAct演奏パネル文字列() );
			base.list子Activities.Add( this.actStageFailed = new CAct演奏ステージ失敗() );
			base.list子Activities.Add( this.actPlayInfo = new CAct演奏演奏情報() );
			//base.list子Activities.Add( this.actFI = new CActFIFOBlack() );
			base.list子Activities.Add( this.actFI = new CActFIFOStart() );
			base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
			base.list子Activities.Add( this.actFOClear = new CActFIFOResult() );
			base.list子Activities.Add( this.actLane = new CAct演奏Drumsレーン() );
			base.list子Activities.Add( this.actEnd = new CAct演奏Drums演奏終了演出() );
			base.list子Activities.Add( this.actDancer = new CAct演奏DrumsDancer() );
			base.list子Activities.Add( this.actMtaiko = new CAct演奏DrumsMtaiko() );
			base.list子Activities.Add( this.actLaneTaiko = new CAct演奏Drumsレーン太鼓() );
			base.list子Activities.Add( this.actRoll = new CAct演奏Drums連打() );
			base.list子Activities.Add( this.actBalloon = new CAct演奏Drums風船() );
			base.list子Activities.Add( this.actChara = new CAct演奏Drumsキャラクター() );
			base.list子Activities.Add( this.actGame = new CAct演奏Drumsゲームモード() );
			base.list子Activities.Add( this.actBackground = new CAct演奏Drums背景() );
			base.list子Activities.Add( this.actRollChara = new CAct演奏Drums連打キャラ() );
			base.list子Activities.Add( this.actComboBalloon = new CAct演奏Drumsコンボ吹き出し() );
			base.list子Activities.Add( this.actComboVoice = new CAct演奏Combo音声() );
			base.list子Activities.Add( this.actPauseMenu = new CAct演奏PauseMenu() );
			base.list子Activities.Add(this.actChipEffects = new CAct演奏Drumsチップエフェクト());
			base.list子Activities.Add(this.actRunner = new CAct演奏DrumsRunner());
			base.list子Activities.Add(this.actMob = new CAct演奏DrumsMob());
			base.list子Activities.Add(this.GoGoSplash = new GoGoSplash());
			base.list子Activities.Add(this.FlyingNotes = new FlyingNotes());
			base.list子Activities.Add(this.FireWorks = new FireWorks());
			base.list子Activities.Add(this.PuchiChara = new PuchiChara());

			base.list子Activities.Add(this.actDan = new Dan_Cert());
			base.list子Activities.Add(this.actTokkun = new CAct演奏Drums特訓モード());
			#region[ 文字初期化 ]
			ST文字位置[] st文字位置Array = new ST文字位置[ 12 ];
			ST文字位置 st文字位置 = new ST文字位置();
			st文字位置.ch = '0';
			st文字位置.pt = new Point( 0, 0 );
			st文字位置Array[ 0 ] = st文字位置;
			ST文字位置 st文字位置2 = new ST文字位置();
			st文字位置2.ch = '1';
			st文字位置2.pt = new Point( 32, 0 );
			st文字位置Array[ 1 ] = st文字位置2;
			ST文字位置 st文字位置3 = new ST文字位置();
			st文字位置3.ch = '2';
			st文字位置3.pt = new Point( 64, 0 );
			st文字位置Array[ 2 ] = st文字位置3;
			ST文字位置 st文字位置4 = new ST文字位置();
			st文字位置4.ch = '3';
			st文字位置4.pt = new Point( 96, 0 );
			st文字位置Array[ 3 ] = st文字位置4;
			ST文字位置 st文字位置5 = new ST文字位置();
			st文字位置5.ch = '4';
			st文字位置5.pt = new Point( 128, 0 );
			st文字位置Array[ 4 ] = st文字位置5;
			ST文字位置 st文字位置6 = new ST文字位置();
			st文字位置6.ch = '5';
			st文字位置6.pt = new Point( 160, 0 );
			st文字位置Array[ 5 ] = st文字位置6;
			ST文字位置 st文字位置7 = new ST文字位置();
			st文字位置7.ch = '6';
			st文字位置7.pt = new Point( 192, 0 );
			st文字位置Array[ 6 ] = st文字位置7;
			ST文字位置 st文字位置8 = new ST文字位置();
			st文字位置8.ch = '7';
			st文字位置8.pt = new Point( 224, 0 );
			st文字位置Array[ 7 ] = st文字位置8;
			ST文字位置 st文字位置9 = new ST文字位置();
			st文字位置9.ch = '8';
			st文字位置9.pt = new Point( 256, 0 );
			st文字位置Array[ 8 ] = st文字位置9;
			ST文字位置 st文字位置10 = new ST文字位置();
			st文字位置10.ch = '9';
			st文字位置10.pt = new Point( 288, 0 );
			st文字位置Array[ 9 ] = st文字位置10;
			ST文字位置 st文字位置11 = new ST文字位置();
			st文字位置11.ch = '%';
			st文字位置11.pt = new Point( 320, 0 );
			st文字位置Array[ 10 ] = st文字位置11;
			ST文字位置 st文字位置12 = new ST文字位置();
			st文字位置12.ch = ' ';
			st文字位置12.pt = new Point( 0, 0 );
			st文字位置Array[ 11 ] = st文字位置12;
			this.st小文字位置 = st文字位置Array;

			st文字位置Array = new ST文字位置[ 12 ];
			st文字位置 = new ST文字位置();
			st文字位置.ch = '0';
			st文字位置.pt = new Point( 0, 0 );
			st文字位置Array[ 0 ] = st文字位置;
			st文字位置2 = new ST文字位置();
			st文字位置2.ch = '1';
			st文字位置2.pt = new Point( 32, 0 );
			st文字位置Array[ 1 ] = st文字位置2;
			st文字位置3 = new ST文字位置();
			st文字位置3.ch = '2';
			st文字位置3.pt = new Point( 64, 0 );
			st文字位置Array[ 2 ] = st文字位置3;
			st文字位置4 = new ST文字位置();
			st文字位置4.ch = '3';
			st文字位置4.pt = new Point( 96, 0 );
			st文字位置Array[ 3 ] = st文字位置4;
			st文字位置5 = new ST文字位置();
			st文字位置5.ch = '4';
			st文字位置5.pt = new Point( 128, 0 );
			st文字位置Array[ 4 ] = st文字位置5;
			st文字位置6 = new ST文字位置();
			st文字位置6.ch = '5';
			st文字位置6.pt = new Point( 160, 0 );
			st文字位置Array[ 5 ] = st文字位置6;
			st文字位置7 = new ST文字位置();
			st文字位置7.ch = '6';
			st文字位置7.pt = new Point( 192, 0 );
			st文字位置Array[ 6 ] = st文字位置7;
			st文字位置8 = new ST文字位置();
			st文字位置8.ch = '7';
			st文字位置8.pt = new Point( 224, 0 );
			st文字位置Array[ 7 ] = st文字位置8;
			st文字位置9 = new ST文字位置();
			st文字位置9.ch = '8';
			st文字位置9.pt = new Point( 256, 0 );
			st文字位置Array[ 8 ] = st文字位置9;
			st文字位置10 = new ST文字位置();
			st文字位置10.ch = '9';
			st文字位置10.pt = new Point( 288, 0 );
			st文字位置Array[ 9 ] = st文字位置10;
			st文字位置11 = new ST文字位置();
			st文字位置11.ch = '%';
			st文字位置11.pt = new Point( 320, 0 );
			st文字位置Array[ 10 ] = st文字位置11;
			st文字位置12 = new ST文字位置();
			st文字位置12.ch = ' ';
			st文字位置12.pt = new Point( 0, 0 );
			st文字位置Array[ 11 ] = st文字位置12;
			this.st小文字位置 = st文字位置Array;
			#endregion
		}


		// メソッド

		public void t演奏結果を格納する( out CScoreIni.C演奏記録 Drums ,int nPlayer)
		{
			base.t演奏結果を格納する_ドラム( out Drums , nPlayer);
		}


		// CStage 実装

		public override void On活性化()
		{
			LoudnessMetadataScanner.StopBackgroundScanning(joinImmediately: false);

			this.n待機中の大音符の座標 = 0;
			this.actGame.t叩ききりまショー_初期化();
			for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
				base.ReSetScore(TJAPlayer3.DTX[nPlayer].nScoreInit[0, TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]], TJAPlayer3.DTX[nPlayer].nScoreDiff[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]], nPlayer);
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;//初期化すれば、リザルト変遷は止まる。
			
			base.On活性化();

			dtLastQueueOperation = DateTime.MinValue;

			//int nUnit = (int)( ( ( 60.0 / ( CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM ) ) ) * 1000.0 / this.actChara.arモーション番号.Length );
			//int nUnit_gogo = (int)((60.0 / ( CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM ) / this.actChara.arゴーゴーモーション番号.Length ) * 1000 );

			//double dbUnit = ( ( ( 60.0 / ( CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM ) ) ) );
			//double dbUnit_gogo = ( ( ( 60.0 / ( CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM ) ) ) / this.actChara.arゴーゴーモーション番号[nPlayer].Length );

			PuchiChara.InitializeBPM(60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM / (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));

			//dbUnit = Math.Ceiling( dbUnit * 1000.0 );
			//dbUnit = dbUnit / 1000.0;

			//this.actChara.ctChara_Normal = new CCounter( 0, this.actChara.arモーション番号[nPlayer].Length - 1, dbPtn_Normal, CSound管理.rc演奏用タイマ );
			////this.actChara.ct通常モーション = new CCounter( 0, this.actChara.arモーション番号[nPlayer].Length - 1, 0.07, CSound管理.rc演奏用タイマ );
			//this.actChara.ctChara_GoGo = new CCounter( 0, this.actChara.arゴーゴーモーション番号[nPlayer].Length - 1, dbPtn_GoGo, CSound管理.rc演奏用タイマ );
			for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
			{
				double dbPtn_Normal = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.Skin.Game_Chara_Beat_Normal[nPlayer] / this.actChara.arモーション番号[nPlayer].Length / (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0);
				double dbPtn_Clear = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.Skin.Game_Chara_Beat_Clear[nPlayer] / this.actChara.arクリアモーション番号[nPlayer].Length / (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0);
				double dbPtn_GoGo = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.Skin.Game_Chara_Beat_GoGo[nPlayer] / this.actChara.arゴーゴーモーション番号[nPlayer].Length / (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0);
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Normal[nPlayer] != 0)
				{
					this.actChara.ctChara_Normal[nPlayer] = new CCounter(0, this.actChara.arモーション番号[nPlayer].Length - 1, dbPtn_Normal, CSound管理.rc演奏用タイマ);
				}
				else
				{
					this.actChara.ctChara_Normal[nPlayer] = new CCounter();
				}
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
				{
					this.actChara.ctChara_Clear[nPlayer] = new CCounter(0, this.actChara.arクリアモーション番号[nPlayer].Length - 1, dbPtn_Clear, CSound管理.rc演奏用タイマ);
				}
				else
				{
					this.actChara.ctChara_Clear[nPlayer] = new CCounter();
				}
				if (TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
				{
					this.actChara.ctChara_GoGo[nPlayer] = new CCounter(0, this.actChara.arゴーゴーモーション番号[nPlayer].Length - 1, dbPtn_GoGo, CSound管理.rc演奏用タイマ);
				}
				else
				{
					this.actChara.ctChara_GoGo[nPlayer] = new CCounter();
				}
			}

			//if (this.actChara.ctキャラクターアクションタイマ != null) this.actChara.ctキャラクターアクションタイマ = new CCounter();

			//this.actDancer.ct通常モーション = new CCounter( 0, this.actDancer.arモーション番号_通常.Length - 1, ( dbUnit * 4.0) / this.actDancer.arモーション番号_通常.Length, CSound管理.rc演奏用タイマ );
			//this.actDancer.ctモブ = new CCounter( 1.0, 16.0, ((60.0 / CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM / 16.0 )), CSound管理.rc演奏用タイマ );

			if(this.actDancer.ct踊り子モーション != null)
			{
				double dbUnit_dancer = (((60 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))) / this.actDancer.ar踊り子モーション番号.Length);
				this.actDancer.ct踊り子モーション = new CCounter(0, this.actDancer.ar踊り子モーション番号.Length - 1, dbUnit_dancer * TJAPlayer3.Skin.Game_Dancer_Beat, CSound管理.rc演奏用タイマ);
			}else
			{
				this.actDancer.ct踊り子モーション = new CCounter();
			}

			this.ct手つなぎ = new CCounter( 0, 60, 20, TJAPlayer3.Timer );
			this.ShownLyric2 = 0;

			// Discord Presence の更新
			var difficultyName = TJAPlayer3.DifficultyNumberToEnum(TJAPlayer3.stage選曲.n確定された曲の難易度[0]).ToString();
			Discord.UpdatePresence(TJAPlayer3.ConfigIni.SendDiscordPlayingInformation ? TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION : "",
				Properties.Discord.Stage_InGame + (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == true ? " (" + Properties.Discord.Info_IsAuto + ")" : ""),
				0,
				Discord.GetUnixTime() + (long)TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms / 1000,
				TJAPlayer3.ConfigIni.SendDiscordPlayingInformation ? difficultyName.ToLower() : "",
				TJAPlayer3.ConfigIni.SendDiscordPlayingInformation ? String.Format("COURSE:{0} ({1})", difficultyName, TJAPlayer3.stage選曲.n確定された曲の難易度[0]) : "");
		}
		public override void On非活性化()
		{
			this.ct手つなぎ = null;
			base.On非活性化();

			LoudnessMetadataScanner.StartBackgroundScanning();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				// When performing calibration, reduce audio distraction from user input.
				// For users who play primarily by listening to the music,
				// you might think that we want them to hear drum sound effects during
				// calibration, but we do not. Humans are remarkably good at adjusting
				// the timing of their own physical movement, even without realizing it.
				// We are calibrating their input timing for the purposes of judgment.
				// We do not want them subconsciously playing early so as to line up
				// their drum sound effects with the sounds of the input calibration file.
				// Instead, we want them focused on the sounds of their keyboard, tatacon,
				// other controller, etc. and the sounds of the input calibration audio file.
				if (!TJAPlayer3.IsPerformingCalibration)
				{
					for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++) {
						if (File.Exists(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\dong.ogg")))
							this.soundRed[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\dong.ogg"), ESoundGroup.SoundEffect);
						else
							this.soundRed[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\dong.ogg"), ESoundGroup.SoundEffect);

						if (File.Exists(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\ka.ogg")))
							this.soundBlue[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\ka.ogg"), ESoundGroup.SoundEffect);
						else
							this.soundBlue[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\ka.ogg"), ESoundGroup.SoundEffect);

						if (File.Exists(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\Adlib.ogg")))
							this.soundAdlib[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\" + TJAPlayer3.Skin.NowSENum[i].ToString() + @"\Adlib.ogg"), ESoundGroup.SoundEffect);
						else
							this.soundAdlib[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(@"Sounds\Taiko\Adlib.ogg"), ESoundGroup.SoundEffect);
					}

					if (TJAPlayer3.ConfigIni.nPlayerCount >= 2 && TJAPlayer3.ConfigIni.b2P演奏時のSEの左右)//2020.05.06 Mr-Ojii 左右に出したかったから、追加。
					{
						if (this.soundRed[0] != null)
							this.soundRed[0].n位置 = -100;
						if (this.soundBlue[0] != null)
							this.soundBlue[0].n位置 = -100;
						if (this.soundAdlib[0] != null)
							this.soundAdlib[0].n位置 = -100;
						if (this.soundRed[1] != null)
							this.soundRed[1].n位置 = 100;
						if (this.soundBlue[1] != null)
							this.soundBlue[1].n位置 = 100;
						if (this.soundAdlib[1] != null)
							this.soundAdlib[1].n位置 = 100;
					}
				}

				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				for (int i = 0; i < 2; i++)
				{
					if (this.soundRed[i] != null)
						this.soundRed[i].t解放する();
					if (this.soundBlue[i] != null)
						this.soundBlue[i].t解放する();
					if (this.soundAdlib[i] != null)
						this.soundAdlib[i].t解放する();
				}
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			base.sw.Start();
			if( !base.b活性化してない )
			{
				bool bIsFinishedPlaying = false;
				bool bIsFinishedEndAnime = false;
				bool bIsFinishedFadeout = false;
				#region [ 初めての進行描画 ]
				if ( base.b初めての進行描画 )
				{
					CSound管理.rc演奏用タイマ.tリセット();
					//CSound管理.rc演奏用タイマ.n現在時刻ms += 50000;
					TJAPlayer3.Timer.tリセット();

					// this.actChipFireD.Start( Eレーン.HH );	// #31554 2013.6.12 yyagi
					// 初チップヒット時のもたつき回避。最初にactChipFireD.Start()するときにJITが掛かって？
					// ものすごく待たされる(2回目以降と比べると2,3桁tick違う)。そこで最初の画面フェードインの間に
					// 一発Start()を掛けてJITの結果を生成させておく。

					base.eフェーズID = CStage.Eフェーズ.共通_フェードイン;
					this.actFI.tフェードイン開始();


					TJAPlayer3.Sound管理.tDisableUpdateBufferAutomatically();
					base.b初めての進行描画 = false;
				}
				#endregion
				if ( ( ( TJAPlayer3.ConfigIni.nRisky != 0 && this.actGauge.IsFailed( 0 ) ) || this.actGame.st叩ききりまショー.ct残り時間.b終了値に達した ) && ( base.eフェーズID == CStage.Eフェーズ.共通_通常状態 ) )
				{
					this.actStageFailed.Start();
					TJAPlayer3.DTX[0].t全チップの再生停止();
					base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_FAILED;
				}
				if( (!String.IsNullOrEmpty( TJAPlayer3.DTX[0].strBGIMAGE_PATH ) || ( TJAPlayer3.DTX[0].listVD.Count == 0 )) || !TJAPlayer3.ConfigIni.bAVI有効  ) //背景動画があったら背景画像を描画しない。
				{
					this.t進行描画_背景();
				}

				if (TJAPlayer3.ConfigIni.bAVI有効 && TJAPlayer3.DTX[0].listVD.Count > 0 && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード)
				{
					this.t進行描画_AVI();
				}
				else if (TJAPlayer3.ConfigIni.bBGA有効)
				{
					if (TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード) actTokkun.On進行描画_背景();
					else actBackground.On進行描画();
				}

				if (!TJAPlayer3.ConfigIni.bAVI有効 && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード)
				{
					actRollChara.On進行描画();
				}

				if (!bDoublePlay && TJAPlayer3.ConfigIni.ShowDancer && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード)
				{
					actDancer.On進行描画();
				}

				if(!bDoublePlay && TJAPlayer3.ConfigIni.ShowFooter && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード && TJAPlayer3.Tx.Mob_Footer != null)
					TJAPlayer3.Tx.Mob_Footer.t2D描画(TJAPlayer3.app.Device, 0, 720 - TJAPlayer3.Tx.Mob_Footer.szテクスチャサイズ.Height);

				//this.t進行描画_グラフ();   // #24074 2011.01.23 add ikanick


				//this.t進行描画_DANGER();
				//this.t進行描画_判定ライン();
				if ( !TJAPlayer3.ConfigIni.bNoInfo )
					this.t進行描画_ネームプレート();
				if( TJAPlayer3.ConfigIni.ShowChara )
					this.actChara.On進行描画();

				if(TJAPlayer3.ConfigIni.ShowMob && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード)
					this.actMob.On進行描画();

				if ( TJAPlayer3.ConfigIni.eGameMode != EGame.OFF )
					this.actGame.On進行描画();

				this.act譜面スクロール速度.On進行描画();
				this.t進行描画_チップアニメ();

				if(TJAPlayer3.ConfigIni.ShowRunner)
					this.actRunner.On進行描画();


				if (!TJAPlayer3.ConfigIni.bNoInfo)
					this.t進行描画_パネル文字列();

				this.actLaneTaiko.On進行描画();
				//this.t進行描画_レーン();
				//this.t進行描画_レーンフラッシュD();

				if( ( TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.ウィンドウのみ || TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.両方 ) && TJAPlayer3.ConfigIni.nPlayerCount == 1 )
					this.actAVI.t窓表示();

				if (!TJAPlayer3.ConfigIni.bNoInfo && TJAPlayer3.ConfigIni.eGameMode != EGame.特訓モード)
					this.t進行描画_ゲージ();

				this.actLaneTaiko.ゴーゴー炎();

				this.actDan.On進行描画();

				for ( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
				{
					bIsFinishedPlaying = this.t進行描画_チップ( i );
					this.t進行描画_チップ_連打( i );
				}


				this.actMtaiko.On進行描画();
				//if (this.txNamePlate != null)
				//{
				//    this.txNamePlate.t2D描画(CDTXMania.app.Device, 0, 288);
				//}
				//if (this.txPlayerNumber != null)
				//{
				//    this.txPlayerNumber.t2D描画(CDTXMania.app.Device, 5, 233);
				//}
				this.GoGoSplash.On進行描画();
				this.t進行描画_リアルタイム判定数表示();
				if (TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード)
					this.actTokkun.On進行描画_小節_速度();

				if ( !TJAPlayer3.ConfigIni.bNoInfo )
					this.t進行描画_コンボ();
				if( !TJAPlayer3.ConfigIni.bNoInfo )
					this.t進行描画_スコア();

				if (TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード)
					actTokkun.On進行描画();
				


				this.Rainbow.On進行描画();
				this.FireWorks.On進行描画();
				this.actChipEffects.On進行描画();
				this.FlyingNotes.On進行描画();
				this.t進行描画_チップファイアD();

				this.actComboBalloon.On進行描画();

				for ( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
				{
					this.actRoll.On進行描画( this.n現在の連打数[ i ], i );
				}


				if( !TJAPlayer3.ConfigIni.bNoInfo )
					this.actJudgeString.t進行描画();

				this.t進行描画_演奏情報();
				
				if (TJAPlayer3.DTX[0].listLyric2.Count > ShownLyric2 && TJAPlayer3.DTX[0].listLyric2[ShownLyric2].Time + TJAPlayer3.DTX[0].nBGMAdjust < (long)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
				{
					this.actPanel.t歌詞テクスチャを生成する(TJAPlayer3.DTX[0].listLyric2[ShownLyric2++].TextTex);
				}

				this.actPanel.t歌詞テクスチャを描画する();

				if (TJAPlayer3.ConfigIni.ShowChara)
					actChara.OnDraw_Balloon();

				this.t全体制御メソッド();

				this.actPauseMenu.t進行描画();

				this.t進行描画_STAGEFAILED();

				bIsFinishedEndAnime = this.actEnd.On進行描画() == 1 ? true : false;
				bIsFinishedFadeout = this.t進行描画_フェードイン_アウト();

				//演奏終了→演出表示→フェードアウト
				if( bIsFinishedPlaying && base.eフェーズID == CStage.Eフェーズ.共通_通常状態 )
				{
					if (TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード)
					{
						bIsFinishedPlaying = false;
						TJAPlayer3.Skin.sound特訓停止音.t再生する();
						actTokkun.t演奏を停止する();

						actTokkun.n現在の小節線 = TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0];
						actTokkun.t譜面の表示位置を合わせる(false);
					}
					else
					{
						base.eフェーズID = CStage.Eフェーズ.演奏_演奏終了演出;
						this.actEnd.Start();
						for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
						{
							if (TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] != 0)
							{
								if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] >= 100)
								{
									double dbUnit = (((60.0 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))));
									this.actChara.アクションタイマーリセット(nPlayer);
									this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer] = new CCounter(0, TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] - 1, (dbUnit / TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer]) * 2, CSound管理.rc演奏用タイマ);
									this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].t進行db();
									this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].db現在の値 = 0D;
									this.actChara.bマイどんアクション中[nPlayer] = true;
								}
							}
						}
					}
				}
				else if( bIsFinishedEndAnime && base.eフェーズID == Eフェーズ.演奏_演奏終了演出 )
				{
					this.eフェードアウト完了時の戻り値 = E演奏画面の戻り値.ステージクリア;
					base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_CLEAR_フェードアウト;
					this.actFOClear.tフェードアウト開始();
				}


				//if( bIsFinishedPlaying && ( base.eフェーズID == CStage.Eフェーズ.共通_通常状態 ) )
				//{
				//    this.eフェードアウト完了時の戻り値 = E演奏画面の戻り値.ステージクリア;
				//    base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_CLEAR_フェードアウト;
				//    this.actFOClear.tフェードアウト開始();
				//}

				if( bIsFinishedFadeout )
				{
					Debug.WriteLine( "Total On進行描画=" + sw.ElapsedMilliseconds + "ms" );
					return (int) this.eフェードアウト完了時の戻り値;
				}

				ManageMixerQueue();

				// キー入力

				this.tキー入力();

				// 確認用 18_04_26(AioiLight)
				//CDTXMania.act文字コンソール.tPrint(0, 0, C文字コンソール.Eフォント種別.白, this.actGauge.db現在のゲージ値[0].ToString());

			}
			base.sw.Stop();
			return 0;
		}

		// その他

		#region [ private ]
		//-----------------
		[StructLayout( LayoutKind.Sequential )]
		private struct ST文字位置
		{
			public char ch;
			public Point pt;
		}
		public CAct演奏DrumsチップファイアD actChipFireD;

		public CAct演奏Drumsレーン actLane;
		public CAct演奏DrumsMtaiko actMtaiko;
		public CAct演奏Drumsレーン太鼓 actLaneTaiko;
		public CAct演奏Drums演奏終了演出 actEnd;
		private CAct演奏Drumsゲームモード actGame;
		public CAct演奏Drums背景 actBackground;
		public CAct演奏Drums特訓モード actTokkun;
		public GoGoSplash GoGoSplash;
		public FlyingNotes FlyingNotes;
		public FireWorks FireWorks;
		public PuchiChara PuchiChara;
		private CCounter ct手つなぎ;

		public float nGauge = 0.0f;

		private int ShownLyric2 = 0 ;

		private int n待機中の大音符の座標;
		private readonly ST文字位置[] st小文字位置;
		//-----------------

		private bool tドラムヒット処理( long nHitTime, Eパッド type, CDTX.CChip pChip, bool b両手入力, int nPlayer )
		{
			int nInput = 0;

			switch( type )
			{
				case Eパッド.LRed:
				case Eパッド.RRed:
				case Eパッド.LRed2P:
				case Eパッド.RRed2P:
					nInput = 0;
					if( b両手入力 )
						nInput = 2;
					break;
				case Eパッド.LBlue:
				case Eパッド.RBlue:
				case Eパッド.LBlue2P:
				case Eパッド.RBlue2P:
					nInput = 1;
					if( b両手入力 )
						nInput = 3;
					break;
			}


			if( pChip == null )
			{
				return false;
			}
			int index = pChip.nチャンネル番号;
			if ( ( index >= 0x11 ) && ( index <= 0x12 ) )
			{
				index -= 0x11;
			}
			else if ( ( index >= 0x13 ) && ( index <= 0x14 ) )
			{
				index -= 0x13;
			}
			else if ( ( index >= 0x1A ) && ( index <= 0x1B ) )
			{
				index -= 0x1A;
			}
			else if( index == 0x1F )
			{
				index = 0x11 - 0x11;
			}
			else if( pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x17 )
			{
				this.tチップのヒット処理( nHitTime, pChip, true, nInput, nPlayer );
				return true;
			}
			else
			{
				return false;
			}
			

			int nLane = index;
			int nPad = index;

			E判定 e判定 = this.e指定時刻からChipのJUDGEを返す( nHitTime, pChip );
			//if( pChip.nコース == this.n現在のコース )
				this.actGame.t叩ききりまショー_判定から各数値を増加させる( e判定, (int)( nHitTime - pChip.n発声時刻ms ) );
			if( e判定 == E判定.Miss )
			{
				return false;
			}
			this.tチップのヒット処理( nHitTime, pChip, true, nInput, nPlayer );
			if( ( e判定 != E判定.Poor ) && ( e判定 != E判定.Miss ) )
			{
				TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.Start( pChip.nチャンネル番号, e判定, b両手入力, nPlayer );

				int nFly = 0;
				switch(pChip.nチャンネル番号)
				{
					case 0x11:
						nFly = 1;
						break;
					case 0x12:
						nFly = 2;
						break;
					case 0x13:
					case 0x1A:
						nFly = b両手入力 ? 3 : 1;
						break;
					case 0x14:
					case 0x1B:
						nFly = b両手入力 ? 4 : 2;
						break;
					case 0x1F:
						nFly = nInput == 0 ? 1 : 2;
						break;
					default:
						nFly = 1;
						break;
				}


				//this.actChipFireTaiko.Start( nFly, nPlayer );
				this.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Hit);
				this.FlyingNotes.Start(nFly, nPlayer);
			}

			return true;
		}

		protected override void ドラムスクロール速度アップ(int nPlayer)
		{
			TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] = Math.Min( TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] + 1, 1999 );
		}
		protected override void ドラムスクロール速度ダウン(int nPlayer)
		{
			TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] = Math.Max( TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] - 1, 0 );
		}

	
		protected override void t進行描画_AVI()
		{
			base.t進行描画_AVI();
		}
		protected override void t進行描画_DANGER()
		{
			this.actDANGER.t進行描画( this.actGauge.IsDanger(0) );
		}

		private void t進行描画_チップファイアD()
		{
			this.actChipFireD.On進行描画();
		}

		protected override void t進行描画_演奏情報()
		{
			base.t進行描画_演奏情報( 1000, 257 );
		}

		protected override void t紙吹雪_開始()
		{
			//if( this.actCombo.n現在のコンボ数.Drums % 10 == 0 && this.actCombo.n現在のコンボ数.Drums > 0 )
			{
				//this.actChipFireD.Start紙吹雪();
			}
		}

		protected override void t入力処理_ドラム()
		{
			var nInputAdjustTimeMs = TJAPlayer3.ConfigIni.nInputAdjustTimeMs;

			for( int nPad = 0; nPad < (int) Eパッド.MAX; nPad++ )		// #27029 2012.1.4 from: <10 to <=10; Eパッドの要素が１つ（HP）増えたため。
																		//		  2012.1.5 yyagi: (int)Eパッド.MAX に変更。Eパッドの要素数への依存を無くすため。
			{
				List<STInputEvent> listInputEvent = TJAPlayer3.Pad.GetEvents((Eパッド) nPad );

				if( ( listInputEvent == null ) || ( listInputEvent.Count == 0 ) )
					continue;

				this.t入力メソッド記憶();

				foreach( STInputEvent inputEvent in listInputEvent )
				{
					if( !inputEvent.b押された )
						continue;

					long nTime = (long)(((inputEvent.nTimeStamp + nInputAdjustTimeMs - CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻) * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)));
					//int nPad09 = ( nPad == (int) Eパッド.HP ) ? (int) Eパッド.BD : nPad;		// #27029 2012.1.5 yyagi

					bool bHitted = false;

					int nLane = 0;
					int nHand = 0;
					int nChannel = 0;

					//連打チップを検索してから通常音符検索
					//連打チップの検索は、
					//一番近くの連打音符を探す→時刻チェック
					//発声 < 現在時刻 && 終わり > 現在時刻

					//2015.03.19 kairera0467 Chipを1つにまとめて1つのレーン扱いにする。
					int nUsePlayer = 0;
					if(nPad >= (int)Eパッド.LRed && nPad <= (int)Eパッド.RBlue) {
						nUsePlayer = 0;
					} else if(nPad >= (int)Eパッド.LRed2P && nPad <= (int)Eパッド.RBlue2P) {
						nUsePlayer = 1;
						if( TJAPlayer3.ConfigIni.nPlayerCount < 2 ) //プレイ人数が2人以上でなければ入力をキャンセル
							break;
					}

					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] && (nPad >= (int)Eパッド.LRed && nPad <= (int)Eパッド.RBlue))//2020.05.18 Mr-Ojii オート時の入力キャンセル
						break;
					else if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[1] && (nPad >= (int)Eパッド.LRed2P && nPad <= (int)Eパッド.RBlue2P))
						break;

					var padTo = nPad - (nUsePlayer * 4);
					var isDon = padTo < 2 ? true : false;

					CDTX.CChip chipNoHit = chip現在処理中の連打チップ[nUsePlayer] == null ? GetChipOfNearest( nTime, nUsePlayer, isDon) : GetChipOfNearest(nTime, nUsePlayer);
					E判定 e判定 = ( chipNoHit != null ) ? this.e指定時刻からChipのJUDGEを返す( nTime, chipNoHit ) : E判定.Miss;

					bool b太鼓音再生フラグ = true;
					if( chipNoHit != null )
					{
						if( chipNoHit.nチャンネル番号 == 0x1F && ( e判定 == E判定.Perfect || e判定 == E判定.Good ) )
							b太鼓音再生フラグ = false;

						if( chipNoHit.nチャンネル番号 == 0x1F && ( e判定 != E判定.Miss && e判定 != E判定.Poor ))
							this.soundAdlib[chipNoHit.nPlayerSide]?.t再生を開始する();
					}

					switch (nPad)
					{
						case (int)Eパッド.LRed:
							nLane = 0;
							nHand = 0;
							nChannel = 0x11;
							if( b太鼓音再生フラグ )
							{
								this.soundRed[0]?.t再生を開始する();
							}
							break;
						case (int)Eパッド.RRed:
							nLane = 0;
							nHand = 1;
							nChannel = 0x11;
							if( b太鼓音再生フラグ )
							{
								this.soundRed[0]?.t再生を開始する();
							}
							break;
						case (int)Eパッド.LBlue:
							nLane = 1;
							nHand = 0;
							nChannel = 0x12;
							if( b太鼓音再生フラグ )
								this.soundBlue[0]?.t再生を開始する();
							break;
						case (int)Eパッド.RBlue:
							nLane = 1;
							nHand = 1;
							nChannel = 0x12;
							if( b太鼓音再生フラグ )
								this.soundBlue[0]?.t再生を開始する();
							break;
						//以下2P
						case (int)Eパッド.LRed2P:
							nLane = 0;
							nHand = 0;
							nChannel = 0x11;
							if( b太鼓音再生フラグ )
							{
								this.soundRed[1]?.t再生を開始する();
							}
							break;
						case (int)Eパッド.RRed2P:
							nLane = 0;
							nHand = 1;
							nChannel = 0x11;
							if( b太鼓音再生フラグ )
							{
								this.soundRed[1]?.t再生を開始する();
							}
							break;
						case (int)Eパッド.LBlue2P:
							nLane = 1;
							nHand = 0;
							nChannel = 0x12;
							if( b太鼓音再生フラグ)
								this.soundBlue[1]?.t再生を開始する();
							break;
						case (int)Eパッド.RBlue2P:
							nLane = 1;
							nHand = 1;
							nChannel = 0x12;
							if( b太鼓音再生フラグ)
								this.soundBlue[1]?.t再生を開始する();
							break;
					}

					TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nUsePlayer].Start((PlayerLane.FlashType)nLane);
					TJAPlayer3.stage演奏ドラム画面.actMtaiko.tMtaikoEvent(nChannel, nHand, nUsePlayer );

					if( this.b連打中[ nUsePlayer ] )
					{
						chipNoHit = this.chip現在処理中の連打チップ[ nUsePlayer ];
						e判定 = E判定.Perfect;
					}



					if( chipNoHit == null )
					{
						break;
					}

					//if (this.stream != null)
					//    this.stream.WriteLine("channel:{0} course:{1} judge:{2} DBSCROLL:{3}", chipNoHit.nチャンネル番号, chipNoHit.nコース, e判定, chipNoHit.fBMSCROLLTime );

					#region [ (A) ヒットしていればヒット処理して次の inputEvent へ ]
					//-----------------------------
					switch( ( (Eパッド) nPad ) )
					{
						case Eパッド.LRed:
						case Eパッド.LRed2P:
							#region[ 面のヒット処理 ]
							//-----------------------------
							{
								if( e判定 != E判定.Miss && chipNoHit.nチャンネル番号 == 0x11 )
								{
									this.tドラムヒット処理( nTime, Eパッド.LRed, chipNoHit, false, nUsePlayer );
									bHitted = true;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A ) && !TJAPlayer3.ConfigIni.b大音符判定 )
								{
									this.tドラムヒット処理( nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer );
									bHitted = true;
									this.nWaitButton = 0;
									break;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A ) && TJAPlayer3.ConfigIni.b大音符判定 )
								{
									if (e判定 == E判定.Poor) 
									{
										this.tドラムヒット処理(nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer);
										bHitted = true;
										this.nWaitButton = 0;
									}
									else if( chipNoHit.eNoteState == ENoteState.none )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										if (time <= 110)
										{
											chipNoHit.nProcessTime = (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
											chipNoHit.eNoteState = ENoteState.wait;
											this.nWaitButton = 2;
										}
									}
									else if (chipNoHit.eNoteState == ENoteState.wait)
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										int nWaitTime = TJAPlayer3.ConfigIni.n両手判定の待ち時間;
										if( this.nWaitButton == 1 && time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
										else if (this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理(nTime, Eパッド.LRed, chipNoHit, false, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
									}
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 || chipNoHit.nチャンネル番号 == 0x17 ) )
								{
									this.tドラムヒット処理( nTime, Eパッド.LRed, chipNoHit, false, nUsePlayer );
								}

								if( !bHitted )
									break;
								continue;
							}
							//-----------------------------
							#endregion

						case Eパッド.RRed:
						case Eパッド.RRed2P:
							#region[ 面のヒット処理 ]
							//-----------------------------
							{
								if( e判定 != E判定.Miss && chipNoHit.nチャンネル番号 == 0x11 )
								{
									this.tドラムヒット処理( nTime, Eパッド.RRed, chipNoHit, false, nUsePlayer );
									bHitted = true;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A ) && !TJAPlayer3.ConfigIni.b大音符判定 )
								{
									this.tドラムヒット処理( nTime, Eパッド.RRed, chipNoHit, true, nUsePlayer );
									bHitted = true;
									this.nWaitButton = 0;
									break;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A ) && TJAPlayer3.ConfigIni.b大音符判定 )
								{
									if (e判定 == E判定.Poor)
									{
										this.tドラムヒット処理(nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer);
										bHitted = true;
										this.nWaitButton = 0;
									}
									else if ( chipNoHit.eNoteState == ENoteState.none )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										if( time <= 110 )
										{
											chipNoHit.nProcessTime = (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
											this.n待機中の大音符の座標 = chipNoHit.nバーからの距離dot.Taiko;
											chipNoHit.eNoteState = ENoteState.wait;
											this.nWaitButton = 1;
										}
									}
									else if( chipNoHit.eNoteState == ENoteState.wait )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										int nWaitTime = TJAPlayer3.ConfigIni.n両手判定の待ち時間;
										if( this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.RRed, chipNoHit, true, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
											break;
										}
										else if( this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.RRed, chipNoHit, false, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
									}
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 || chipNoHit.nチャンネル番号 == 0x17 ) )
								{
									this.tドラムヒット処理( nTime, Eパッド.RRed, chipNoHit, false, nUsePlayer );
								}

								if( !bHitted )
									break;

								continue;
							}
							//-----------------------------
							#endregion

						case Eパッド.LBlue:
						case Eパッド.LBlue2P:
							#region[ ふちのヒット処理 ]
							//-----------------------------
							{
								if( e判定 != E判定.Miss && chipNoHit.nチャンネル番号 == 0x12 )
								{
									this.tドラムヒット処理( nTime, Eパッド.LBlue, chipNoHit, false, nUsePlayer );
									bHitted = true;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B ) && !TJAPlayer3.ConfigIni.b大音符判定 )
								{
									this.tドラムヒット処理(nTime, Eパッド.LBlue, chipNoHit, true, nUsePlayer );
									bHitted = true;
									this.nWaitButton = 0;
									break;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B ) && TJAPlayer3.ConfigIni.b大音符判定 )
								{
									if (e判定 == E判定.Poor)
									{
										this.tドラムヒット処理(nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer);
										bHitted = true;
										this.nWaitButton = 0;
									}
									else if ( chipNoHit.eNoteState == ENoteState.none )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										if( time <= 110 )
										{
											chipNoHit.nProcessTime = (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
											chipNoHit.eNoteState = ENoteState.wait;
											this.nWaitButton = 2;
										}
									}
									else if( chipNoHit.eNoteState == ENoteState.wait )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										int nWaitTime = TJAPlayer3.ConfigIni.n両手判定の待ち時間;
										if( this.nWaitButton == 1 && time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.LBlue, chipNoHit, true, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
										else if( this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.LBlue, chipNoHit, false, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
									}
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 ) )
								{
									this.tドラムヒット処理( nTime, Eパッド.LBlue, chipNoHit, false, nUsePlayer );
								}

								if( !bHitted )
									break;
								continue;
							}
							//-----------------------------
							#endregion

						case Eパッド.RBlue:
						case Eパッド.RBlue2P:
							#region[ ふちのヒット処理 ]
							//-----------------------------
							{
								if( e判定 != E判定.Miss && chipNoHit.nチャンネル番号 == 0x12 )
								{
									this.tドラムヒット処理( nTime, Eパッド.RBlue, chipNoHit, false, nUsePlayer );
									bHitted = true;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B ) && !TJAPlayer3.ConfigIni.b大音符判定 )
								{
									this.tドラムヒット処理( nTime, Eパッド.RBlue, chipNoHit, true, nUsePlayer );
									bHitted = true;
									this.nWaitButton = 0;
									break;
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B ) && TJAPlayer3.ConfigIni.b大音符判定 )
								{
									if (e判定 == E判定.Poor)
									{
										this.tドラムヒット処理(nTime, Eパッド.LRed, chipNoHit, true, nUsePlayer);
										bHitted = true;
										this.nWaitButton = 0;
									}
									else if ( chipNoHit.eNoteState == ENoteState.none )
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										if( time <= 110 )
										{
											chipNoHit.nProcessTime = (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
											this.n待機中の大音符の座標 = chipNoHit.nバーからの距離dot.Taiko;
											chipNoHit.eNoteState = ENoteState.wait;
											this.nWaitButton = 1;
										}
									}
									else if (chipNoHit.eNoteState == ENoteState.wait)
									{
										float time = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
										int nWaitTime = TJAPlayer3.ConfigIni.n両手判定の待ち時間;
										if( this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.RBlue, chipNoHit, true, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
											break;
										}
										else if( this.nWaitButton == 2 && time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
										{
											this.tドラムヒット処理( nTime, Eパッド.RBlue, chipNoHit, false, nUsePlayer );
											bHitted = true;
											this.nWaitButton = 0;
										}
									}
								}
								if( e判定 != E判定.Miss && ( chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 ) )
								{
									this.tドラムヒット処理( nTime, Eパッド.RBlue, chipNoHit, false, nUsePlayer );
								}

								if( !bHitted )
									break;
								continue;
							}
							//-----------------------------
							#endregion
					}
					//2016.07.14 kairera0467 Adlibの場合、一括して処理を行う。
					if( e判定 != E判定.Miss && chipNoHit.nチャンネル番号 == 0x1F )
					{
						this.tドラムヒット処理( nTime, (Eパッド)nPad, chipNoHit, false, nUsePlayer );
						bHitted = true;
					}

					//-----------------------------
					#endregion
					#region [ (B) ヒットしてなかった場合は、レーンフラッシュ、パッドアニメ、空打ち音再生を実行 ]
					//-----------------------------
					int pad = nPad;	// 以下、nPad の代わりに pad を用いる。（成りすまし用）
					// BAD or TIGHT 時の処理。
					if( TJAPlayer3.ConfigIni.bTight && !b連打中[nUsePlayer]) // 18/8/13 - 連打時にこれが発動すると困る!!! (AioiLight)
						this.tチップのヒット処理_BadならびにTight時のMiss(chipNoHit.nコース );
					//-----------------------------
					#endregion
				}
			}
		}

		// t入力処理_ドラム()からメソッドを抽出したもの。
		/// <summary>
		/// chipArrayの中を, n発生位置の小さい順に並べる + nullを大きい方に退かす。セットでe判定Arrayも並べ直す。
		/// </summary>
		/// <param name="chipArray">ソート対象chip群</param>
		/// <param name="e判定Array">ソート対象e判定群</param>
		/// <param name="NumOfChips">チップ数</param>
		private static void SortChipsByNTime( CDTX.CChip[] chipArray, E判定[] e判定Array, int NumOfChips )
		{
			for ( int i = 0; i < NumOfChips - 1; i++ )
			{
				//num9 = 2;
				//while( num9 > num8 )
				for ( int j = NumOfChips - 1; j > i; j-- )
				{
					if ( ( chipArray[ j - 1 ] == null ) || ( ( chipArray[ j ] != null ) && ( chipArray[ j - 1 ].n発声位置 > chipArray[ j ].n発声位置 ) ) )
					{
						// swap
						CDTX.CChip chipTemp = chipArray[ j - 1 ];
						chipArray[ j - 1 ] = chipArray[ j ];
						chipArray[ j ] = chipTemp;
						E判定 e判定Temp = e判定Array[ j - 1 ];
						e判定Array[ j - 1 ] = e判定Array[ j ];
						e判定Array[ j ] = e判定Temp;
					}
					//num9--;
				}
				//num8++;
			}
		}

		protected override void t背景テクスチャの生成()
		{
			Rectangle bgrect = new Rectangle( 0, 0, 1280, 720 );
			string DefaultBgFilename = @"Graphics\5_Game\5_Background\0\Background.png";
			string BgFilename = "";
			if( !String.IsNullOrEmpty( TJAPlayer3.DTX[0].strBGIMAGE_PATH ) )
				BgFilename = TJAPlayer3.DTX[0].strBGIMAGE_PATH;
			base.t背景テクスチャの生成( DefaultBgFilename, bgrect, BgFilename );
		}
		protected override void t進行描画_チップ_Taiko( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer )
		{
			int nLane = 0;

			#region[ 作り直したもの ]
			if( pChip.b可視 )
			{
				if( !pChip.bHit)
				{
					long nPlayTime = (long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
					if ((!pChip.bHit) && (pChip.n発声時刻ms <= nPlayTime))
					{
						bool bAutoPlay = false;
						switch (nPlayer)
						{
							case 0:
								bAutoPlay = TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0];
								break;
							case 1:
								bAutoPlay = TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[1];
								break;
							case 2:
							case 3:
								bAutoPlay = true;
								break;
						}

						if (bAutoPlay && !this.bPAUSE)
						{
							pChip.bHit = true;
							if (pChip.nチャンネル番号 != 0x1F)
								this.FlyingNotes.Start(pChip.nチャンネル番号 < 0x1A ? (pChip.nチャンネル番号 - 0x10) : (pChip.nチャンネル番号 - 0x17), nPlayer);
							//this.actChipFireTaiko.Start(pChip.nチャンネル番号 < 0x1A ? (pChip.nチャンネル番号 - 0x10) : (pChip.nチャンネル番号 - 0x17), nPlayer);
							if (pChip.nチャンネル番号 == 0x12 || pChip.nチャンネル番号 == 0x14 || pChip.nチャンネル番号 == 0x1B) nLane = 1;
							TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start((nLane == 0 ? PlayerLane.FlashType.Red : PlayerLane.FlashType.Blue));
							TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Hit);
							this.actMtaiko.tMtaikoEvent(pChip.nチャンネル番号, this.nHand[nPlayer], nPlayer);

							int n大音符 = (pChip.nチャンネル番号 == 0x11 || pChip.nチャンネル番号 == 0x12 ? 2 : 0);

							this.tチップのヒット処理(pChip.n発声時刻ms, pChip, true, nLane + n大音符, nPlayer);
							this.tサウンド再生(pChip, nPlayer);
							return;
						}
					}


					if ( pChip.nノーツ出現時刻ms != 0 && ( nPlayTime < pChip.n発声時刻ms - pChip.nノーツ出現時刻ms ) )
						pChip.bShow = false;
					else
						pChip.bShow = true;


					switch (nPlayer)
					{
						case 0:
							break;
						case 1:
							break;
					}
					switch( pChip.nPlayerSide )
					{
						case 1:
							break;
					}

					int x = 0;
					int y = TJAPlayer3.Skin.nScrollFieldY[nPlayer];

					if ( pChip.nノーツ移動開始時刻ms != 0 && ( nPlayTime < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) )
					{
						x = (int)( ( ( ( pChip.n発声時刻ms ) - ( pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) ) * pChip.dbBPM * pChip.dbSCROLL * ( this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0 ) ) / 502.8594 / 5.0); // 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
					}
					else
					{
						x = pChip.nバーからの距離dot.Taiko;
					}

					int xTemp = 0;
					int yTemp = 0;

					#region[ スクロール方向変更 ]
					if( pChip.nスクロール方向 != 0 )
					{
						xTemp = x;
						yTemp = y;
					}
					switch ( pChip.nスクロール方向 )
					{
						case 0:
							x += ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] );
							break;
						case 1:
							x = ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] );
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] - xTemp;
							break;
						case 2:
							x = ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] + 3 );
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] + xTemp;
							break;
						case 3:
							x += ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] );
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] - xTemp;
							break;
						case 4:
							x += ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] );
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] + xTemp;
							break;
						case 5:
							x = ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] + 10 ) - xTemp;
							break;
						case 6:
							x = ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] ) - xTemp;
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] - xTemp;
							break;
						case 7:
							x = ( TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] ) - xTemp;
							y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ] + xTemp;
							break;
					}
					#endregion

					#region[ 両手待ち時 ]
					if( pChip.eNoteState == ENoteState.wait && TJAPlayer3.ConfigIni.b両手判定待ち時間中に大音符を判定枠に合わせるか)
					{
						x = (TJAPlayer3.Skin.nScrollFieldX[pChip.nPlayerSide]);
					}
					#endregion

					if (pChip.dbSCROLL_Y != 0.0)
					{

						var dbSCROLL = configIni.eScrollMode == EScrollMode.BMSCROLL ? 1.0 : pChip.dbSCROLL;

						y = TJAPlayer3.Skin.nScrollFieldY[nPlayer];
						if (TJAPlayer3.ConfigIni.eScrollMode == EScrollMode.Normal)
							y += (int)(((pChip.n発声時刻ms - ((CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0))) * pChip.dbBPM * pChip.dbSCROLL_Y * (this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0)) / 502.8594 / 5.0); // 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
						else if (TJAPlayer3.ConfigIni.eScrollMode == EScrollMode.BMSCROLL || TJAPlayer3.ConfigIni.eScrollMode == EScrollMode.HBSCROLL)
						{
							float? play_bpm_time = null;
							if (!play_bpm_time.HasValue)
							{
								play_bpm_time = this.GetNowPBMTime(dTX);
							}
							var dbSCROLL_Y = configIni.eScrollMode == EScrollMode.BMSCROLL ? 1.0 : pChip.dbSCROLL_Y;

							y += (int)(3 * 0.8335 * ((pChip.fBMSCROLLTime * NOTE_GAP) - (play_bpm_time * NOTE_GAP)) * dbSCROLL_Y * (this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0) / 2 / 5.0);
						} 
					}

					if( pChip.nバーからの距離dot.Drums < 0 )
					{
						this.actGame.st叩ききりまショー.b最初のチップが叩かれた = true;
					}

					if( ( 1400 > x ))
					{
						if( TJAPlayer3.Tx.Notes != null )
						{
							//int num9 = this.actCombo.n現在のコンボ数.Drums >= 50 ? this.ctチップ模様アニメ.Drums.n現在の値 * 130 : 0;
							int num9 = 0;
							if (TJAPlayer3.Skin.Game_Notes_Anime)
							{
								if (this.actCombo.n現在のコンボ数[nPlayer] >= 300 && ctChipAnimeLag[nPlayer].b終了値に達した)
								{
									//num9 = ctChipAnime[nPlayer].db現在の値 != 0 ? 260 : 0;
									if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
									{
										num9 = 260;
									}
									else
									{
										num9 = 0;
									}
								}
								else if (this.actCombo.n現在のコンボ数[nPlayer] >= 300 && !ctChipAnimeLag[nPlayer].b終了値に達した)
								{
									//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
									if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
									{
										num9 = 130;
									}
									else
									{
										num9 = 0;
									}
								}
								else if (this.actCombo.n現在のコンボ数[nPlayer] >= 150)
								{
									//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
									if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
									{
										num9 = 130;
									}
									else
									{
										num9 = 0;
									}
								}
								else if (this.actCombo.n現在のコンボ数[nPlayer] >= 50 && ctChipAnimeLag[nPlayer].b終了値に達した)
								{
									//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
									if ((int)ctChipAnime[nPlayer].db現在の値 <= 1)
									{
										num9 = 130;
									}
									else
									{
										num9 = 0;
									}
								}
								else if (this.actCombo.n現在のコンボ数[nPlayer] >= 50 && !ctChipAnimeLag[nPlayer].b終了値に達した)
								{
									//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
									num9 = 0;
								}
								else
								{
									num9 = 0;
								}
							}



							int nSenotesY = TJAPlayer3.Skin.nSENotesY[nPlayer];
							this.ct手つなぎ.t進行Loop();
							int nHand = this.ct手つなぎ.n現在の値 < 30 ? this.ct手つなぎ.n現在の値 : 60 - this.ct手つなぎ.n現在の値;


							x = ( x ) - ( ( int ) ( ( 130.0 * pChip.dbチップサイズ倍率 ) / 2.0 ) );
							TJAPlayer3.Tx.Notes.b加算合成 = false;
							TJAPlayer3.Tx.SENotes.b加算合成 = false;
							var device = TJAPlayer3.app.Device;
							switch ( pChip.nチャンネル番号 )
							{
								case 0x11:

									if( TJAPlayer3.Tx.Notes != null && pChip.bShow )
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 130, num9, 130, 130 ) );
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 30 * pChip.nSenote, 136, 30 ) );
										//CDTXMania.act文字コンソール.tPrint( x + 60, y + 140, C文字コンソール.Eフォント種別.白, pChip.nSenote.ToString() );
									}
									break;

								case 0x12:
									if( TJAPlayer3.Tx.Notes != null && pChip.bShow)
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 260, num9, 130, 130) );
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 30 * pChip.nSenote, 136, 30 ) );
										//CDTXMania.act文字コンソール.tPrint( x + 60, y + 140, C文字コンソール.Eフォント種別.白, pChip.nSenote.ToString() );
									}
									nLane = 1;
									break;
								  
								case 0x13:
									if( TJAPlayer3.Tx.Notes != null && pChip.bShow)
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
										{
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 390, num9, 130, 130 ) );
											//CDTXMania.Tx.Notes.t3D描画( device, mat, new Rectangle( 390, num9, 130, 130 ) );
										}
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 30 * pChip.nSenote, 136, 30 ) );
										//CDTXMania.act文字コンソール.tPrint( x + 60, y + 140, C文字コンソール.Eフォント種別.白, pChip.nSenote.ToString() );
									}
									break;
								case 0x14:
									if( TJAPlayer3.Tx.Notes != null && pChip.bShow)
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 520, num9, 130, 130 ) );
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 30 * pChip.nSenote, 136, 30 ) );
										//CDTXMania.act文字コンソール.tPrint( x + 60, y + 140, C文字コンソール.Eフォント種別.白, pChip.nSenote.ToString() );
									}
									nLane = 1;
									break;

								case 0x1A:
									if( TJAPlayer3.Tx.Notes != null )
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
										{
											if( nPlayer == 0 )
											{
												TJAPlayer3.Tx.Notes_Arm.t2D上下反転描画( device, x + 25, ( y + 74 ) + nHand );
												TJAPlayer3.Tx.Notes_Arm.t2D上下反転描画( device, x + 60, ( y + 104 ) - nHand );
											}
											else if( nPlayer == 1 )
											{
												TJAPlayer3.Tx.Notes_Arm.t2D描画( device, x + 25, ( y - 44 ) + nHand );
												TJAPlayer3.Tx.Notes_Arm.t2D描画( device, x + 60, ( y - 14 ) - nHand );
											}
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 1690, num9, 130, 130 ) );
											//CDTXMania.Tx.Notes.t3D描画( device, mat, new Rectangle( 390, num9, 130, 130 ) );
										}
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 390, 136, 30 ) );
									}
									break;

								case 0x1B:
									if( TJAPlayer3.Tx.Notes != null )
									{
										if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
										{
											if( nPlayer == 0 )
											{
												TJAPlayer3.Tx.Notes_Arm.t2D上下反転描画( device, x + 25, ( y + 74 ) + nHand );
												TJAPlayer3.Tx.Notes_Arm.t2D上下反転描画( device, x + 60, ( y + 104 ) - nHand );
											}
											else if( nPlayer == 1 )
											{
												TJAPlayer3.Tx.Notes_Arm.t2D描画( device, x + 25, ( y - 44 ) + nHand );
												TJAPlayer3.Tx.Notes_Arm.t2D描画( device, x + 60, ( y - 14 ) - nHand );
											}
											TJAPlayer3.Tx.Notes.t2D描画( device, x, y, new Rectangle( 1820, num9, 130, 130 ) );
										}
										if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
											TJAPlayer3.Tx.SENotes.t2D描画( device, x - 2, y + nSenotesY, new Rectangle( 0, 420, 136, 30 ) );
									}
									nLane = 1;
									break;

								case 0x1F:
									break;
							}
							//CDTXMania.act文字コンソール.tPrint( x + 60, y + 160, C文字コンソール.Eフォント種別.白, pChip.nPlayerSide.ToString() );
						}
					}
					else if( x < -1000 )
					{
						//pChip.bHit = true;
					}
				}
			}
			else
			{
				return;
			}
			#endregion
		}
		protected override void t進行描画_チップ_Taiko連打( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer )
		{
			int nSenotesY = TJAPlayer3.Skin.nSENotesY[ nPlayer ];
			int nノート座標 = 0;
			int nノート末端座標 = 0;
			int n先頭発声位置 = 0;

			#region[ 作り直したもの ]
			if( pChip.b可視 )
			{
				if( pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x18 )
				{
					if( pChip.nノーツ出現時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ出現時刻ms ) )
						pChip.bShow = false;
					else if( pChip.nノーツ出現時刻ms != 0 && pChip.nノーツ移動開始時刻ms != 0 )
						pChip.bShow = true;

					if( pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) )
					{
						nノート座標 = (int)( ( ( ( pChip.n発声時刻ms ) - ( pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) ) * pChip.dbBPM * pChip.dbSCROLL * ( this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0 ) ) / 502.8594 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
						nノート末端座標 = (int)( ( ( pChip.nノーツ終了時刻ms - ( pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) ) * pChip.db末端BPM * pChip.db末端SCROLL * ( this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0 ) ) / 502.8594 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
					}
					else
					{
						nノート座標 = 0;
						nノート末端座標 = 0;
					}
				}
				if( pChip.nチャンネル番号 == 0x18 )
				{
					if( pChip.nノーツ出現時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < n先頭発声位置 - pChip.nノーツ出現時刻ms ) )
						pChip.bShow = false;
					else
						pChip.bShow = true;

					CDTX.CChip cChip = null;
					if( pChip.nノーツ移動開始時刻ms != 0 ) // n先頭発声位置 value is only used when this condition is met
					{
						cChip = TJAPlayer3.stage演奏ドラム画面.r指定時刻に一番近い連打Chip_ヒット未済問わず不可視考慮( pChip.n発声時刻ms, 0x10 + pChip.n連打音符State, 0, nPlayer );
						if( cChip != null )
						{
							n先頭発声位置 = cChip.n発声時刻ms;
						}
					}

					//連打音符先頭の開始時刻を取得しなければならない。
					//そうしなければ連打先頭と連打末端の移動開始時刻にズレが出てしまう。
					if( pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < n先頭発声位置 - pChip.nノーツ移動開始時刻ms ) )
					{
						nノート座標 = (int)( ( ( pChip.n発声時刻ms - ( n先頭発声位置 - pChip.nノーツ移動開始時刻ms ) ) * pChip.dbBPM * pChip.dbSCROLL * ( this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0 ) ) / 502.8594 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
					}
					else
					{
						nノート座標 = 0;
					}
				}
				//2020.05.06 Mr-Ojii ここらへんから349って書いてあったところを　TJAPlayer3.Skin.nScrollFieldX[nPlayer] - 55に置き換えた。
				int x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからの距離dot.Taiko - 55;
				int x末端 = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからのノーツ末端距離dot - 55;
				int y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ];

				if( pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x17 )
				{
					if( pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms ) )
					{
						x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + nノート座標;
						x末端 = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + nノート末端座標;
					}
					else
					{
						x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからの距離dot.Taiko - 55;
						x末端 = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからのノーツ末端距離dot - 55;
					}
				}
				else if( pChip.nチャンネル番号 == 0x18 )
				{
					if( pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < n先頭発声位置 - pChip.nノーツ移動開始時刻ms ) )
					{
						x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + nノート座標;
					}
					else
					{
						x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからの距離dot.Taiko - 55;
					}
				}

				//if( CDTXMania.ConfigIni.eScrollMode != EScrollMode.Normal )
					x -= 10;

				if (1400 > Math.Min(x, x末端))
				{
					if( TJAPlayer3.Tx.Notes != null )
					{
						//int num9 = this.actCombo.n現在のコンボ数.Drums >= 50 ? this.ctチップ模様アニメ.Drums.n現在の値 * 130 : 0;
						//int num9 = this.actCombo.n現在のコンボ数.Drums >= 50 ? base.n現在の音符の顔番号 * 130 : 0;
						int num9 = 0;
						//if( this.actCombo.n現在のコンボ数[ nPlayer ] >= 300 )
						//{
						//    num9 = base.n現在の音符の顔番号 != 0 ? 260 : 0;
						//}
						//else if( this.actCombo.n現在のコンボ数[ nPlayer ] >= 50 )
						//{
						//    num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
						//}
						if (TJAPlayer3.Skin.Game_Notes_Anime)
						{
							if (this.actCombo.n現在のコンボ数[nPlayer] >= 300 && ctChipAnimeLag[nPlayer].b終了値に達した)
							{
								//num9 = ctChipAnime[nPlayer].db現在の値 != 0 ? 260 : 0;
								if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
								{
									num9 = 260;
								}
								else
								{
									num9 = 0;
								}
							}
							else if (this.actCombo.n現在のコンボ数[nPlayer] >= 300 && !ctChipAnimeLag[nPlayer].b終了値に達した)
							{
								//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
								if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
								{
									num9 = 130;
								}
								else
								{
									num9 = 0;
								}
							}
							else if (this.actCombo.n現在のコンボ数[nPlayer] >= 150)
							{
								//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
								if ((int)ctChipAnime[nPlayer].db現在の値 == 1 || (int)ctChipAnime[nPlayer].db現在の値 == 3)
								{
									num9 = 130;
								}
								else
								{
									num9 = 0;
								}
							}
							else if (this.actCombo.n現在のコンボ数[nPlayer] >= 50 && ctChipAnimeLag[nPlayer].b終了値に達した)
							{
								//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
								if ((int)ctChipAnime[nPlayer].db現在の値 <= 1)
								{
									num9 = 130;
								}
								else
								{
									num9 = 0;
								}
							}
							else if (this.actCombo.n現在のコンボ数[nPlayer] >= 50 && !ctChipAnimeLag[nPlayer].b終了値に達した)
							{
								//num9 = base.n現在の音符の顔番号 != 0 ? base.n現在の音符の顔番号 * 130 : 0;
								num9 = 0;
							}
							else
							{
								num9 = 0;
							}
						}

						//kairera0467氏 の TJAPlayer2forPC のコードを参考にし、打数に応じて色を変える(打数の変更以外はほとんどそのまんま) ろみゅ～？ 2018/8/20
						pChip.RollInputTime?.t進行();
						pChip.RollDelay?.t進行();

						if (pChip.RollInputTime != null && pChip.RollInputTime.b終了値に達した)
						{
							pChip.RollInputTime.t停止();
							pChip.RollInputTime.n現在の値 = 0;
							pChip.RollDelay = new CCounter(0, 1, 1, TJAPlayer3.Timer);
						}
						
						if (pChip.RollDelay != null && pChip.RollDelay.b終了値に達した && pChip.RollEffectLevel > 0)
						{
							pChip.RollEffectLevel--;
							pChip.RollDelay = new CCounter(0, 1, 1, TJAPlayer3.Timer);
							pChip.RollDelay.n現在の値 = 0;
						}

						float f減少するカラー = 1.0f - ((0.95f / 100) * pChip.RollEffectLevel);
						var effectedColor = new Color4( f減少するカラー, f減少するカラー, 1.0f , 1f);
						var normalColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
						float f末端ノーツのテクスチャ位置調整 = 65f;

						if ( pChip.nチャンネル番号 == 0x15 && pChip.bShow) //連打(小)
						{
							int index = x末端 - x; //連打の距離
							if ( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
							{
								#region[末端をテクスチャ側でつなげる場合の方式]

								if (TJAPlayer3.Skin.Game_RollColorMode != CSkin.RollColorMode.None)
									TJAPlayer3.Tx.Notes.color4 = effectedColor;
								else
									TJAPlayer3.Tx.Notes.color4 = normalColor;
								TJAPlayer3.Tx.Notes.vc拡大縮小倍率.X = (index - 65.0f + f末端ノーツのテクスチャ位置調整 + 1) / 128.0f;
								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x + 64, y, new Rectangle(781, 0, 128, 130));
								TJAPlayer3.Tx.Notes.vc拡大縮小倍率.X = 1.0f;
								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + f末端ノーツのテクスチャ位置調整, y, 0, new Rectangle(910, num9, 130, 130));
								if (TJAPlayer3.Skin.Game_RollColorMode == CSkin.RollColorMode.All)
									TJAPlayer3.Tx.Notes.color4 = effectedColor;
								else
									TJAPlayer3.Tx.Notes.color4 = normalColor;
								
								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, 0, new Rectangle(650, num9, 130, 130));
								TJAPlayer3.Tx.Notes.color4 = normalColor;
								#endregion
							}
							if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
							{
								TJAPlayer3.Tx.SENotes.vc拡大縮小倍率.X = index - 44;
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 90, y + nSenotesY, new Rectangle(60, 240, 1, 30));
								TJAPlayer3.Tx.SENotes.vc拡大縮小倍率.X = 1.0f;
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 30, y + nSenotesY, new Rectangle(0, 240, 60, 30));
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
							}
						}
						if( pChip.nチャンネル番号 == 0x16 && pChip.bShow)
						{
							int index = x末端 - x; //連打の距離

							if( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
							{
								#region[末端をテクスチャ側でつなげる場合の方式]

								if (TJAPlayer3.Skin.Game_RollColorMode != CSkin.RollColorMode.None)
									TJAPlayer3.Tx.Notes.color4 = effectedColor;
								else
									TJAPlayer3.Tx.Notes.color4 = normalColor;

								TJAPlayer3.Tx.Notes.vc拡大縮小倍率.X = (index - 65 + f末端ノーツのテクスチャ位置調整 + 1) / 128f;
								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x + 64, y, new Rectangle(1171, 0, 128, 130));

								TJAPlayer3.Tx.Notes.vc拡大縮小倍率.X = 1.0f;
								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + f末端ノーツのテクスチャ位置調整, y, 0, new Rectangle(1300, num9, 130, 130));
								if (TJAPlayer3.Skin.Game_RollColorMode == CSkin.RollColorMode.All)
									TJAPlayer3.Tx.Notes.color4 = effectedColor;
								else
									TJAPlayer3.Tx.Notes.color4 = normalColor;

								TJAPlayer3.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(1040, num9, 130, 130));
								TJAPlayer3.Tx.Notes.color4 = normalColor;
								#endregion
							}
							if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
							{
								TJAPlayer3.Tx.SENotes.vc拡大縮小倍率.X = index - 70;
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 116, y + nSenotesY, new Rectangle(60, 240, 1, 30));
								TJAPlayer3.Tx.SENotes.vc拡大縮小倍率.X = 1.0f;
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 56, y + nSenotesY, new Rectangle(0, 240, 60, 30));
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
							}
						}
						if( pChip.nチャンネル番号 == 0x17 && pChip.bShow)
						{
							if ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) >= pChip.n発声時刻ms && (long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) < pChip.nノーツ終了時刻ms)
								x = TJAPlayer3.Skin.nScrollFieldX[nPlayer] - 55;
							else if ((long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) >= pChip.nノーツ終了時刻ms)
								x = (TJAPlayer3.Skin.nScrollFieldX[nPlayer] + pChip.nバーからのノーツ末端距離dot - 55);

							if ( TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] == EStealthMode.OFF )
								TJAPlayer3.Tx.Notes.t2D描画( TJAPlayer3.app.Device, x, y, new Rectangle( 1430, num9, 260, 130 ) );

							if(TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
								TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
						}
						if( pChip.nチャンネル番号 == 0x18 )
						{
							//大きい連打か小さい連打かの区別方法を考えてなかったよちくしょう
							TJAPlayer3.Tx.Notes.vc拡大縮小倍率.X = 1.0f;

							if( pChip.n連打音符State != 7 )
							{
								//if( CDTXMania.ConfigIni.eSTEALTH != EStealthMode.DORON )
								//    CDTXMania.Tx.Notes.t2D描画( CDTXMania.app.Device, x, y, new Rectangle( n, num9, 130, 130 ) );//大音符:1170
								if (TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] != EStealthMode.STEALTH)
									TJAPlayer3.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 56, y + nSenotesY, new Rectangle( 58, 270, 78, 30 ) );
							}


							//CDTXMania.act文字コンソール.tPrint(x, y - 10, C文字コンソール.Eフォント種別.白, pChip.n発声時刻ms.ToString());
							//CDTXMania.act文字コンソール.tPrint(x, y - 26, C文字コンソール.Eフォント種別.白, pChip.n連打音符State.ToString());
							//CDTXMania.act文字コンソール.tPrint(x, y - 42, C文字コンソール.Eフォント種別.白, pChip.dbSCROLL.ToString());
							//CDTXMania.act文字コンソール.tPrint(x, y - 58, C文字コンソール.Eフォント種別.白, pChip.dbBPM.ToString());

						}
					}
				}
			}
			if( pChip.n発声時刻ms < (CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)) && pChip.nノーツ終了時刻ms > (CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
			{
				//時間内でかつ0x9Aじゃないならならヒット処理
				if (pChip.nチャンネル番号 != 0x18 && TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[nPlayer])
					this.tチップのヒット処理( pChip.n発声時刻ms, pChip, false, 0, nPlayer );
			}
			#endregion
		}

		protected override void t進行描画_チップ_小節線( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer )
		{
			if( pChip.nコース != this.n現在のコース[ nPlayer ] )
				return;

			//int n小節番号plus1 = pChip.n発声位置 / 384;
			int n小節番号plus1 = this.actPlayInfo.NowMeasure[nPlayer];
			int x = TJAPlayer3.Skin.nScrollFieldX[ nPlayer ] + pChip.nバーからの距離dot.Taiko;
			int y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ];

			if( pChip.dbSCROLL_Y != 0.0 )
			{
				y = TJAPlayer3.Skin.nScrollFieldY[ nPlayer ];
				y += (int) ( ( ( pChip.n発声時刻ms - (CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0))) * pChip.dbBPM * pChip.dbSCROLL_Y * ( this.act譜面スクロール速度.db現在の譜面スクロール速度[nPlayer] + 1.0 ) ) / 502.8594 / 5.0 );
			}

			if ( !pChip.bHit && pChip.n発声時刻ms > CSound管理.rc演奏用タイマ.n現在時刻 )
			{
				//pChip.bHit = true;
				//this.actPlayInfo.n小節番号 = n小節番号plus1 - 1;
				//this.actPlayInfo.n小節番号++;
				if ( configIni.bWave再生位置自動調整機能有効 && ( bIsOpenAL || bUseOSTimer ) )
				{
					dTX.tWave再生位置自動補正();
				}
			}
			if ( configIni.b演奏情報を表示する || TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード)
			{
				var nowMeasure = pChip.n整数値_内部番号;
				if ( x >= 310 )
				{
					TJAPlayer3.act文字コンソール.tPrint(x + 8, y - 26, C文字コンソール.Eフォント種別.白, nowMeasure.ToString());
				}
			}
			if ( ( pChip.b可視 ) && (TJAPlayer3.Tx.Bar != null ) )
			{
				if( x >= 0 )
				{
					Matrix4 mat = Matrix4.Identity;
					mat *= Matrix4.CreateRotationZ(C変換.DegreeToRadian(-(90.0f * (float)pChip.dbSCROLL_Y)));
					mat *= Matrix4.CreateTranslation((float)(x - 640.0f) - 1.5f, -(y - 360.0f + 65.0f), 0f);

					if( pChip.bBranch )
					{
						//this.tx小節線_branch.t2D描画( CDTXMania.app.Device, x - 3, y, new Rectangle( 0, 0, 3, 130 ) );
						TJAPlayer3.Tx.Bar_Branch.t3D描画( TJAPlayer3.app.Device, mat, new Rectangle( 0, 0, 3, 130 ) );
					}
					else
					{
						//this.tx小節線.t2D描画( CDTXMania.app.Device, x - 3, y, new Rectangle( 0, 0, 3, 130 ) );
						TJAPlayer3.Tx.Bar.t3D描画( TJAPlayer3.app.Device, mat, new Rectangle( 0, 0, 3, 130 ) );
					}
				}
			}
		}

		protected void t進行描画_レーン()
		{
			this.actLane.On進行描画();
		}

		/// <summary>
		/// 全体にわたる制御をする。
		/// </summary>
		public void t全体制御メソッド()
		{
			int time = (int)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
			//CDTXMania.act文字コンソール.tPrint( 0, 16, C文字コンソール.Eフォント種別.白, t.ToString() );

			for ( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
			{
				if( this.chip現在処理中の連打チップ[ i ] != null )
				{
					int n = this.chip現在処理中の連打チップ[ i ].nチャンネル番号;


					if( this.chip現在処理中の連打チップ[ i ].nチャンネル番号 == 0x17 && this.b連打中[ i ] == true )
					{
						//if (this.chip現在処理中の連打チップ.n発声時刻ms <= (int)CSound管理.rc演奏用タイマ.n現在時刻ms && this.chip現在処理中の連打チップ.nノーツ終了時刻ms >= (int)CSound管理.rc演奏用タイマ.n現在時刻ms)
						if( this.chip現在処理中の連打チップ[ i ].n発声時刻ms <= time && this.chip現在処理中の連打チップ[ i ].nノーツ終了時刻ms + 500 >= time)
						{
							this.chip現在処理中の連打チップ[ i ].bShow = false;
							this.actBalloon.On進行描画( this.chip現在処理中の連打チップ[ i ].nBalloon, this.n風船残り[ i ], i );
						}
						else
						{
							this.n現在の連打数[i] = 0;
						}
					}
					else
					{
						if (actChara.CharaAction_Balloon_Breaking[i].b進行中 && chip現在処理中の連打チップ[i].nPlayerSide == 0)
						{
						}
					}
				}
			}



			#region[ 片手判定をこっちに持ってきてみる。]
			//常時イベントが発生しているメソッドのほうがいいんじゃないかという予想。
			//CDTX.CChip chipNoHit = this.r指定時刻に一番近い未ヒットChip((int)CSound管理.rc演奏用タイマ.n現在時刻ms, 0);
			for( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
			{
				CDTX.CChip chipNoHit = GetChipOfNearest((long)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)), i );

				if( chipNoHit != null && ( chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1A || chipNoHit.nチャンネル番号 == 0x1B ) )
				{
					float timeC = chipNoHit.n発声時刻ms - (float)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
					int nWaitTime = TJAPlayer3.ConfigIni.n両手判定の待ち時間;
					if (chipNoHit.eNoteState == ENoteState.wait && timeC <= 110 && chipNoHit.nProcessTime + nWaitTime <= (int)(CSound管理.rc演奏用タイマ.n現在時刻 * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0)))
					{
						this.tドラムヒット処理(chipNoHit.nProcessTime, Eパッド.RRed, chipNoHit, false, i);
						this.nWaitButton = 0;
						chipNoHit.eNoteState = ENoteState.none;
						chipNoHit.bHit = true;
						chipNoHit.IsHitted = true;
					}
				}
			}

			#endregion

			string strNull = "Found";
			if( TJAPlayer3.Input管理.Keyboard.bキーが押された( (int)SlimDXKeys.Key.F1 ) )
			{
				if( !this.actPauseMenu.bIsActivePopupMenu && this.bPAUSE == false)
				{
					TJAPlayer3.Skin.sound変更音.t再生する();

					CSound管理.rc演奏用タイマ.t一時停止();
					TJAPlayer3.Timer.t一時停止();
					TJAPlayer3.DTX[0].t全チップの再生一時停止();
					this.actAVI.tPauseControl();

					this.bPAUSE = true;
					this.actPauseMenu.tActivatePopupMenu(0);
				}

			}

			if (!this.bStartFlag)
			{
				//CSound管理.rc演奏用タイマ.n現在時刻ms = 80000;

				this.bStartFlag = true;
			}
		}
		private bool bStartFlag;
		private void t進行描画_ネームプレート()
		{
			//if( this.txNamePlate != null )
				//this.txNamePlate.t2D描画( CDTXMania.app.Device, 0, 100 );
			
			//if(CDTXMania.Tx.Taiko_NamePlate[0] != null) CDTXMania.Tx.Taiko_NamePlate[0].t2D描画(CDTXMania.app.Device, 0, 100);
			//if(CDTXMania.stage演奏ドラム画面.bDoublePlay && CDTXMania.Tx.Taiko_NamePlate[1] != null) CDTXMania.Tx.Taiko_NamePlate[0].t2D描画(CDTXMania.app.Device, 0, 600);

			//if ( CDTXMania.stage演奏ドラム画面.bDoublePlay )
			//{
			//    if( this.txNamePlate2P != null )
			//        this.txNamePlate2P.t2D描画( CDTXMania.app.Device, 314, 636 );
			//}
		}

		private void t進行描画_リアルタイム判定数表示()
		{
			bool ShowJudgeInfo = false;

			if (TJAPlayer3.ConfigIni.nPlayerCount == 1 ? (TJAPlayer3.ConfigIni.bJudgeCountDisplay && !TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0]) : false) ShowJudgeInfo = true;
			if (TJAPlayer3.ConfigIni.eGameMode == EGame.特訓モード) ShowJudgeInfo = true;

			if (ShowJudgeInfo)
			{
				//ボードの横幅は333px
				//数字フォントの小さいほうはリザルトのものと同じ。
				if( TJAPlayer3.Tx.Judge_Meter != null )
					TJAPlayer3.Tx.Judge_Meter.t2D描画( TJAPlayer3.app.Device, 0, 360 );

				this.t小文字表示( 102, 494, string.Format( "{0,4:###0}", this.nヒット数_Auto含まない[0].Perfect.ToString() ), false );
				this.t小文字表示( 102, 532, string.Format( "{0,4:###0}", this.nヒット数_Auto含まない[0].Great.ToString() ), false );
				this.t小文字表示( 102, 570, string.Format( "{0,4:###0}", this.nヒット数_Auto含まない[0].Miss.ToString() ), false );
				this.t小文字表示(102, 634, string.Format("{0,4:###0}", GetRoll(0)), false);

				int nNowTotal = this.nヒット数_Auto含まない[0].Perfect + this.nヒット数_Auto含まない[0].Great + this.nヒット数_Auto含まない[0].Miss;
				double dbたたけた率 = Math.Round((100.0 * ( TJAPlayer3.stage演奏ドラム画面.nヒット数_Auto含まない[0].Perfect + TJAPlayer3.stage演奏ドラム画面.nヒット数_Auto含まない[0].Great)) / (double)nNowTotal);
				double dbPERFECT率 = Math.Round((100.0 * TJAPlayer3.stage演奏ドラム画面.nヒット数_Auto含まない[0].Perfect) / (double)nNowTotal);
				double dbGREAT率 = Math.Round((100.0 * TJAPlayer3.stage演奏ドラム画面.nヒット数_Auto含まない[0].Great / (double)nNowTotal));
				double dbMISS率 = Math.Round((100.0 * TJAPlayer3.stage演奏ドラム画面.nヒット数_Auto含まない[0].Miss / (double)nNowTotal));

				if (double.IsNaN(dbたたけた率))
					dbたたけた率 = 0;
				if (double.IsNaN(dbPERFECT率))
					dbPERFECT率 = 0;
				if (double.IsNaN(dbGREAT率))
					dbGREAT率 = 0;
				if (double.IsNaN(dbMISS率))
					dbMISS率 = 0;

				this.t大文字表示( 202, 436, string.Format( "{0,3:##0}%", dbたたけた率 ) );
				this.t小文字表示( 206, 494, string.Format( "{0,3:##0}%", dbPERFECT率 ), false );
				this.t小文字表示( 206, 532, string.Format( "{0,3:##0}%", dbGREAT率 ), false );
				this.t小文字表示( 206, 570, string.Format( "{0,3:##0}%", dbMISS率 ), false );
			}
		}

		private void t小文字表示( int x, int y, string str, bool bOrange )
		{
			foreach( char ch in str )
			{
				for( int i = 0; i < this.st小文字位置.Length; i++ )
				{
					if( ch == ' ' )
					{
						break;
					}

					if( this.st小文字位置[ i ].ch == ch )
					{
						Rectangle rectangle = new Rectangle( this.st小文字位置[ i ].pt.X, this.st小文字位置[ i ].pt.Y, 32, 38 );
						if( TJAPlayer3.Tx.Result_Number != null )
						{
							TJAPlayer3.Tx.Result_Number.t2D描画( TJAPlayer3.app.Device, x, y, rectangle );
						}
						break;
					}
				}
				x += 22;
			}
		}

		private void t大文字表示( int x, int y, string str )
		{
			foreach( char ch in str )
			{
				for( int i = 0; i < this.st小文字位置.Length; i++ )
				{
					if( ch == ' ' )
					{
						break;
					}

					if( this.st小文字位置[ i ].ch == ch )
					{
						Rectangle rectangle = new Rectangle( this.st小文字位置[ i ].pt.X, 38, 32, 42 );
						if(TJAPlayer3.Tx.Result_Number != null )
						{
							TJAPlayer3.Tx.Result_Number.t2D描画( TJAPlayer3.app.Device, x, y, rectangle );
						}
						break;
					}
				}
				x += 28;
			}
		}
		#endregion
	}
}
