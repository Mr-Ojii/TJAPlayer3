using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using FDK;
using DiscordRPC;

using Rectangle = System.Drawing.Rectangle;

namespace TJAPlayer3
{
	internal class CStageResult : CStage
	{
		// プロパティ

		public float fPerfect率;
		public float fGreat率;
		public float fGood率;
		public float fPoor率;
		public float fMiss率;
		public CScoreIni.C演奏記録[] st演奏記録;


		// コンストラクタ

		public CStageResult()
		{
			this.st演奏記録 = new CScoreIni.C演奏記録[2];
			base.eStageID = CStage.EStage.Result;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			base.list子Activities.Add( this.actParameterPanel = new CActResultParameterPanel() );
			base.list子Activities.Add( this.actSongBar = new CActResultSongBar() );
			base.list子Activities.Add( this.actFI = new CActFIFOResult() );
			base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
		}

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "結果ステージを活性化します。" );
			Trace.Indent();
			try
			{
				#region [ 初期化 ]
				//---------------------
				this.eFadeOut完了時の戻り値 = E戻り値.継続;
				this.bアニメが完了 = false;
				//---------------------
				#endregion

				#region [ 結果の計算 ]
				//---------------------
				for( int i = 0; i < 1; i++ )
				{
					this.fPerfect率 = this.fGreat率 = this.fGood率 = this.fPoor率 = this.fMiss率 = 0.0f;	// #28500 2011.5.24 yyagi
				}
				//---------------------
				#endregion

				#region [ .score.ini の作成と出力 ]
				//---------------------
				string str = TJAPlayer3.DTX[0].strFilenameの絶対パス + ".score.ini";
				CScoreIni ini = new CScoreIni( str );


				for( int i = 0; i < 1; i++ )
				{
					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false && this.st演奏記録[0].b途中でAutoを切り替えたか == false)
					ini.stセクション.HiScore = this.st演奏記録[0];

					// ラストプレイ #23595 2011.1.9 ikanick
					// オートじゃなければプレイ結果を書き込む
					if( TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false ) {
						ini.stセクション.LastPlay = this.st演奏記録[0];
					}

					// #23596 10.11.16 add ikanick オートじゃないならクリア回数を1増やす
					//        11.02.05 bオート to t更新条件を取得する use      ikanick
					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false)
					{	
						ini.stファイル.ClearCountDrums++;
					}
					//---------------------------------------------------------------------/
				}
				if (TJAPlayer3.ConfigIni.bScoreIniを出力する)
				{
					ini.t書き出し(str);
				}
				//---------------------
				#endregion

				#region [ 選曲画面の譜面情報の更新 ]
				//---------------------

				{ 
					Cスコア cスコア = TJAPlayer3.stage選曲.r確定されたスコア;

					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false && this.st演奏記録[0].b途中でAutoを切り替えたか == false)
					{
						cスコア.譜面情報.nCrown = st演奏記録[0].nCrown;//2020.05.22 Mr-Ojii データが保存されない問題の解決策。
						cスコア.譜面情報.nハイスコア = st演奏記録[0].nハイスコア;
						cスコア.譜面情報.nSecondScore = st演奏記録[0].nSecondScore;
						cスコア.譜面情報.nThirdScore = st演奏記録[0].nThirdScore;

						cスコア.譜面情報.strHiScorerName = st演奏記録[0].strHiScorerName;
						cスコア.譜面情報.strSecondScorerName = st演奏記録[0].strSecondScorerName;
						cスコア.譜面情報.strThirdScorerName = st演奏記録[0].strThirdScorerName;
					}
					
					TJAPlayer3.stage選曲.r確定されたスコア = cスコア;
				}
				//---------------------
				#endregion

				string Details = TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION;

				// Discord Presenseの更新
				TJAPlayer3.DiscordClient?.SetPresence(new RichPresence()
				{
					Details = Details.Substring(0, Math.Min(127, Details.Length)),
					State = "Result" + (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == true ? " (Auto)" : ""),
					Timestamps = new Timestamps(TJAPlayer3.StartupTime),
					Assets = new Assets()
					{
						LargeImageKey = TJAPlayer3.LargeImageKey,
						LargeImageText = TJAPlayer3.LargeImageText,
					}
				});

				this.ctMountainAndClear = new CCounter(0, 1655, 1, TJAPlayer3.Timer);

				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation( "結果ステージの活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				if( this.ct登場用 != null )
				{
					this.ct登場用 = null;
				}
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				if( base.b初めての進行描画 )
				{
					this.ct登場用 = new CCounter( 0, 100, 5, TJAPlayer3.Timer );
					this.actFI.tFadeIn開始();
					base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
					base.b初めての進行描画 = false;
				}
				this.bアニメが完了 = true;
				if( this.ct登場用.b進行中 )
				{
					this.ct登場用.t進行();
					if( this.ct登場用.b終了値に達した )
					{
						this.ct登場用.t停止();
					}
					else
					{
						this.bアニメが完了 = false;
					}
				}

				// 描画
				if (TJAPlayer3.ConfigIni.bEnableSkinV2)
				{
					if (TJAPlayer3.Tx.Result_v2_Background != null)
					{
						if (TJAPlayer3.Tx.Result_v2_Background[0] != null)
							TJAPlayer3.Tx.Result_v2_Background[0].t2D描画(TJAPlayer3.app.Device, 0, 0);
						for (int ind = 0; ind < TJAPlayer3.ConfigIni.nPlayerCount; ind++)
						{
							if (this.st演奏記録[ind].fゲージ >= 80.0 && TJAPlayer3.Tx.Result_v2_Background[1] != null)
							{
								TJAPlayer3.Tx.Result_v2_Background[1].Opacity = Math.Min(this.ctMountainAndClear.n現在の値, 255);
								TJAPlayer3.Tx.Result_v2_Background[1].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.ConfigIni.nPlayerCount * ind, 0, new Rectangle(TJAPlayer3.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.ConfigIni.nPlayerCount * ind, 0, TJAPlayer3.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.ConfigIni.nPlayerCount, TJAPlayer3.Tx.Result_v2_Background[1].szTextureSize.Height));
							}
						}
					}
					if (TJAPlayer3.Tx.Result_v2_Mountain != null && TJAPlayer3.ConfigIni.nPlayerCount == 1)
					{
						if (TJAPlayer3.Tx.Result_v2_Mountain[0] != null)
							if (this.ctMountainAndClear.n現在の値 <= 255 || this.st演奏記録[0].fゲージ < 80.0)
								TJAPlayer3.Tx.Result_v2_Mountain[0].t2D描画(TJAPlayer3.app.Device, 0, 0);
						if (this.st演奏記録[0].fゲージ >= 80.0 && TJAPlayer3.Tx.Result_v2_Mountain[1] != null)
						{
							TJAPlayer3.Tx.Result_v2_Mountain[1].Opacity = Math.Min(this.ctMountainAndClear.n現在の値, 255);
							if (this.ctMountainAndClear.n現在の値 <= 255 || this.ctMountainAndClear.n現在の値 == this.ctMountainAndClear.n終了値)
							{
								TJAPlayer3.Tx.Result_v2_Mountain[1].vcScaling.Y = 1f;
							}
							else if (this.ctMountainAndClear.n現在の値 <= 555)
							{
								TJAPlayer3.Tx.Result_v2_Mountain[1].vcScaling.Y = 1.0f - (this.ctMountainAndClear.n現在の値 - 255) / 300f * 0.4f;
							}
							else if (this.ctMountainAndClear.n現在の値 <= 1155)
							{
								//600msで150degなので4で割る
								TJAPlayer3.Tx.Result_v2_Mountain[1].vcScaling.Y = (float)((Math.Sin((this.ctMountainAndClear.n現在の値 - 555) / 4.0 / 180.0 * Math.PI) * 0.8f) + 0.6f);
							}
							else 
							{
								TJAPlayer3.Tx.Result_v2_Mountain[1].vcScaling.Y = (float)Math.Sin((this.ctMountainAndClear.n現在の値 - 1155) / 500f * Math.PI) * 0.3f + 1f;
							}
							TJAPlayer3.Tx.Result_v2_Mountain[1].t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640, 720);
						}
					}
					if (TJAPlayer3.Tx.Result_v2_Header != null)
					{
						TJAPlayer3.Tx.Result_v2_Header.t2D描画(TJAPlayer3.app.Device, 0, 0);
					}
				}
				else
				{
					if (TJAPlayer3.Tx.Result_Background != null)
					{
						TJAPlayer3.Tx.Result_Background.t2D描画(TJAPlayer3.app.Device, 0, 0);
					}
					if (TJAPlayer3.Tx.Result_Header != null)
					{
						TJAPlayer3.Tx.Result_Header.t2D描画(TJAPlayer3.app.Device, 0, 0);
					}
				}
				if (this.actParameterPanel.On進行描画() == 0)
				{
					this.bアニメが完了 = false;
					this.ctMountainAndClear.n現在の値 = 0;
					this.ctMountainAndClear.t時間Reset();
				}
				else 
				{
					this.ctMountainAndClear.t進行();
					if (!this.ctMountainAndClear.b終了値に達した)
						this.bアニメが完了 = false;
				}

				if ( this.actSongBar.On進行描画() == 0 )
				{
					this.bアニメが完了 = false;
				}

				#region ネームプレート
				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
				{
					if (TJAPlayer3.Tx.NamePlate[i] != null)
					{
						TJAPlayer3.Tx.NamePlate[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.ConfigIni.bEnableSkinV2 ? TJAPlayer3.Skin.SkinConfig.Result.v2NamePlateX[i] : TJAPlayer3.Skin.Result_NamePlate_X[i], TJAPlayer3.ConfigIni.bEnableSkinV2 ? TJAPlayer3.Skin.SkinConfig.Result.v2NamePlateY[i] : TJAPlayer3.Skin.Result_NamePlate_Y[i]);
					}
				}
				#endregion

				if ( base.eフェーズID == CStage.Eフェーズ.共通_FadeIn )
				{
					if( this.actFI.On進行描画() != 0 )
					{
						base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
					}
				}
				else if( ( base.eフェーズID == CStage.Eフェーズ.共通_FadeOut ) )			//&& ( this.actFO.On進行描画() != 0 ) )
				{
					return (int) this.eFadeOut完了時の戻り値;
				}

				// キー入力

				if ((TJAPlayer3.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.bPressed(EPad.LRed) || TJAPlayer3.Pad.bPressed(EPad.RRed) || (TJAPlayer3.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && !this.bアニメが完了)
				{
					this.actFI.tFadeIn完了();                 // #25406 2011.6.9 yyagi
					this.actParameterPanel.tアニメを完了させる();
					this.actSongBar.tアニメを完了させる();
					this.ct登場用.t停止();
				}
				if (base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
				{
					if (TJAPlayer3.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
                    {
                        TJAPlayer3.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                        this.actFO.tFadeOut開始();
						base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
						this.eFadeOut完了時の戻り値 = E戻り値.完了;
					}
					if ((TJAPlayer3.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.bPressed(EPad.LRed) || TJAPlayer3.Pad.bPressed(EPad.RRed) || (TJAPlayer3.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && this.bアニメが完了)
                    {
                        TJAPlayer3.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                        //							this.actFO.tFadeOut開始();
                        base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
						this.eFadeOut完了時の戻り値 = E戻り値.完了;
					}
				}
				
			}
			return 0;
		}

		public enum E戻り値 : int
		{
			継続,
			完了
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter ct登場用;
		private CCounter ctMountainAndClear;
		private E戻り値 eFadeOut完了時の戻り値;
		private CActFIFOResult actFI;
		private CActFIFOBlack actFO;
		private CActResultParameterPanel actParameterPanel;
		private CActResultSongBar actSongBar;
		private bool bアニメが完了;

		#endregion
	}
}
