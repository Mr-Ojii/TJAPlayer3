using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Text;
using FDK;

using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CStage曲読み込み : CStage
	{
		// コンストラクタ

		public CStage曲読み込み()
		{
			base.eステージID = CStage.Eステージ.曲読み込み;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			//base.list子Activities.Add( this.actFI = new CActFIFOBlack() );	// #27787 2012.3.10 yyagi 曲読み込み画面のフェードインの省略
			//base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
		}


		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "曲読み込みステージを活性化します。" );
			Trace.Indent();
			try
			{
				this.str曲タイトル = "";
				
				this.nBGM再生開始時刻 = -1;
				this.nBGMの総再生時間ms = 0;
				if( this.sd読み込み音 != null )
				{
					TJAPlayer3.Sound管理.tサウンドを破棄する( this.sd読み込み音 );
					this.sd読み込み音 = null;
				}

				
				string strDTXファイルパス = TJAPlayer3.stage選曲.r確定されたスコア.ファイル情報.ファイルの絶対パス;

				var 譜面情報 = TJAPlayer3.stage選曲.r確定されたスコア.譜面情報;
				this.str曲タイトル = 譜面情報.タイトル;
				this.strサブタイトル = 譜面情報.strサブタイトル;
				
				

				// For the moment, detect that we are performing
				// calibration via there being an actual single
				// player and the special song title and subtitle
				// of the .tja used to perform input calibration
				TJAPlayer3.IsPerformingCalibration =
					!TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] &&
					TJAPlayer3.ConfigIni.nPlayerCount == 1 &&
					str曲タイトル == "Input Calibration" &&
					strサブタイトル == "TJAPlayer3 Developers";

				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation( "曲読み込みステージの活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			Trace.TraceInformation( "曲読み込みステージを非活性化します。" );
			Trace.Indent();
			try
			{
				base.On非活性化();
			}
			finally
			{
				Trace.TraceInformation( "曲読み込みステージの非活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				this.ct待機 = new CCounter( 0, 600, 5, TJAPlayer3.Timer );
				this.ct曲名表示 = new CCounter( 1, 30, 30, TJAPlayer3.Timer );
				try
				{
					// When performing calibration, inform the player that
					// calibration is about to begin, rather than
					// displaying the song title and subtitle as usual.

					var タイトル = TJAPlayer3.IsPerformingCalibration
						? "Input calibration is about to begin."
						: this.str曲タイトル;

					var サブタイトル = TJAPlayer3.IsPerformingCalibration
						? "Please play as accurately as possible."
						: this.strサブタイトル;

					if( !string.IsNullOrEmpty(タイトル) )
					{
						using (CPrivateFont pfTITLE = new CPrivateFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), TJAPlayer3.Skin.SongLoading_Title_FontSize))
						{
							using (var bmpSongTitle = pfTITLE.DrawPrivateFont(タイトル, TJAPlayer3.Skin.SongLoading_Title_ForeColor, TJAPlayer3.Skin.SongLoading_Title_BackColor))
							{
								this.txタイトル = TJAPlayer3.tテクスチャの生成(bmpSongTitle);
								this.txタイトル.vc拡大縮小倍率.X = TJAPlayer3.GetSongNameXScaling(ref txタイトル, 710);
							}
						}

						using (CPrivateFont pfSUBTITLE = new CPrivateFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), TJAPlayer3.Skin.SongLoading_SubTitle_FontSize))
						{
							using (var bmpSongSubTitle = pfSUBTITLE.DrawPrivateFont(サブタイトル, TJAPlayer3.Skin.SongLoading_SubTitle_ForeColor, TJAPlayer3.Skin.SongLoading_SubTitle_BackColor))
							{
								this.txサブタイトル = TJAPlayer3.tテクスチャの生成(bmpSongSubTitle);
							}
						}
					}
					else
					{
						this.txタイトル = null;
						this.txサブタイトル = null;
					}

				}
				catch ( CTextureCreateFailedException e )
				{
					Trace.TraceError( e.ToString() );
					this.txタイトル = null;
					this.txサブタイトル = null;
				}
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする( ref this.txタイトル );
				//CDTXMania.t安全にDisposeする( ref this.txSongnamePlate );
				TJAPlayer3.t安全にDisposeする( ref this.txサブタイトル );
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( base.b活性化してない )
				return 0;

			#region [ 初めての進行描画 ]
			//-----------------------------
			if( base.b初めての進行描画 )
			{
				if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
				{
					if (this.sd読み込み音 != null)
					{
						if (TJAPlayer3.Skin.sound曲読込開始音.b排他 && (CSkin.Cシステムサウンド.r最後に再生した排他システムサウンド != null))
						{
							CSkin.Cシステムサウンド.r最後に再生した排他システムサウンド.t停止する();
						}
						this.sd読み込み音.t再生を開始する();
						this.nBGM再生開始時刻 = CSound管理.rc演奏用タイマ.n現在時刻ms;
						this.nBGMの総再生時間ms = this.sd読み込み音.n総演奏時間ms;
					}
					else
					{
						TJAPlayer3.Skin.sound曲読込開始音.t再生する();
						this.nBGM再生開始時刻 = CSound管理.rc演奏用タイマ.n現在時刻ms;
						this.nBGMの総再生時間ms = TJAPlayer3.Skin.sound曲読込開始音.n長さ_現在のサウンド;
					}
				}
				//this.actFI.tフェードイン開始();							// #27787 2012.3.10 yyagi 曲読み込み画面のフェードインの省略
				base.eフェーズID = CStage.Eフェーズ.共通_フェードイン;
				base.b初めての進行描画 = false;

				nWAVcount = 1;
				bitmapFilename = new Bitmap( 640, 24 );
				graphicsFilename = Graphics.FromImage( bitmapFilename );
				graphicsFilename.TextRenderingHint = TextRenderingHint.AntiAlias;
				ftFilename = new Font("MS UI Gothic", 24f, FontStyle.Bold, GraphicsUnit.Pixel );
			}
			//-----------------------------
			#endregion
			this.ct待機.t進行();



			#region [ ESC押下時は選曲画面に戻る ]
			if ( tキー入力() )
			{
				if ( this.sd読み込み音 != null )
				{
					this.sd読み込み音.tサウンドを停止する();
					this.sd読み込み音.t解放する();
				}
				return (int) E曲読込画面の戻り値.読込中止;
			}
			#endregion

			#region [ 背景、音符＋タイトル表示 ]
			//-----------------------------
			this.ct曲名表示.t進行();
			if (TJAPlayer3.ConfigIni.bEnableSkinV2)
			{
				if (TJAPlayer3.Tx.SongLoading_v2_BG != null)
					TJAPlayer3.Tx.SongLoading_v2_BG.t2D描画(TJAPlayer3.app.Device, 0, 0);
			}
			else
			{
				if (TJAPlayer3.Tx.SongLoading_BG != null)
					TJAPlayer3.Tx.SongLoading_BG.t2D描画(TJAPlayer3.app.Device, 0, 0);
			}

			if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
			{
				if (TJAPlayer3.ConfigIni.bEnableSkinV2) 
				{
					if (TJAPlayer3.Tx.SongLoading_v2_Plate != null)
					{
						TJAPlayer3.Tx.SongLoading_v2_Plate.bスクリーン合成 = TJAPlayer3.Skin.SongLoading_Plate_ScreenBlend; //あまりにも出番が無い
						TJAPlayer3.Tx.SongLoading_v2_Plate.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_v2_Plate_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							TJAPlayer3.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_Plate_X, TJAPlayer3.Skin.SongLoading_v2_Plate_Y - (TJAPlayer3.Tx.SongLoading_v2_Plate.sz画像サイズ.Height / 2));
						}
						else if (TJAPlayer3.Skin.SongLoading_Plate_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							TJAPlayer3.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_Plate_X - TJAPlayer3.Tx.SongLoading_v2_Plate.sz画像サイズ.Width, TJAPlayer3.Skin.SongLoading_v2_Plate_Y - (TJAPlayer3.Tx.SongLoading_v2_Plate.sz画像サイズ.Height / 2));
						}
						else
						{
							TJAPlayer3.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_Plate_X - (TJAPlayer3.Tx.SongLoading_v2_Plate.sz画像サイズ.Width / 2), TJAPlayer3.Skin.SongLoading_v2_Plate_Y - (TJAPlayer3.Tx.SongLoading_v2_Plate.sz画像サイズ.Height / 2));
						}
					}

					if (this.txタイトル != null)
					{
						int nサブタイトル補正 = string.IsNullOrEmpty(TJAPlayer3.stage選曲.r確定されたスコア.譜面情報.strサブタイトル) ? 15 : 0;

						this.txタイトル.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_v2_Title_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_Title_X, TJAPlayer3.Skin.SongLoading_v2_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
						else if (TJAPlayer3.Skin.SongLoading_v2_Title_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_Title_X - (this.txタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X), TJAPlayer3.Skin.SongLoading_v2_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
						else
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.Skin.SongLoading_v2_Title_X - ((this.txタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X) / 2)), TJAPlayer3.Skin.SongLoading_v2_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
					}
					if (this.txサブタイトル != null)
					{
						this.txサブタイトル.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_v2_SubTitle_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_SubTitle_X, TJAPlayer3.Skin.SongLoading_v2_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
						else if (TJAPlayer3.Skin.SongLoading_v2_SubTitle_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_v2_SubTitle_X - (this.txサブタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X), TJAPlayer3.Skin.SongLoading_v2_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
						else
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.Skin.SongLoading_v2_SubTitle_X - ((this.txサブタイトル.sz画像サイズ.Width * txサブタイトル.vc拡大縮小倍率.X) / 2)), TJAPlayer3.Skin.SongLoading_v2_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
					}
				}
				else
				{
					if (TJAPlayer3.Tx.SongLoading_Plate != null)
					{
						TJAPlayer3.Tx.SongLoading_Plate.bスクリーン合成 = TJAPlayer3.Skin.SongLoading_Plate_ScreenBlend; //あまりにも出番が無い
						TJAPlayer3.Tx.SongLoading_Plate.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_Plate_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							TJAPlayer3.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_Plate_X, TJAPlayer3.Skin.SongLoading_Plate_Y - (TJAPlayer3.Tx.SongLoading_Plate.sz画像サイズ.Height / 2));
						}
						else if (TJAPlayer3.Skin.SongLoading_Plate_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							TJAPlayer3.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_Plate_X - TJAPlayer3.Tx.SongLoading_Plate.sz画像サイズ.Width, TJAPlayer3.Skin.SongLoading_Plate_Y - (TJAPlayer3.Tx.SongLoading_Plate.sz画像サイズ.Height / 2));
						}
						else
						{
							TJAPlayer3.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_Plate_X - (TJAPlayer3.Tx.SongLoading_Plate.sz画像サイズ.Width / 2), TJAPlayer3.Skin.SongLoading_Plate_Y - (TJAPlayer3.Tx.SongLoading_Plate.sz画像サイズ.Height / 2));
						}
					}

					if (this.txタイトル != null)
					{
						int nサブタイトル補正 = string.IsNullOrEmpty(TJAPlayer3.stage選曲.r確定されたスコア.譜面情報.strサブタイトル) ? 15 : 0;

						this.txタイトル.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_Title_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_Title_X, TJAPlayer3.Skin.SongLoading_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
						else if (TJAPlayer3.Skin.SongLoading_Title_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_Title_X - (this.txタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X), TJAPlayer3.Skin.SongLoading_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
						else
						{
							this.txタイトル.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.Skin.SongLoading_Title_X - ((this.txタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X) / 2)), TJAPlayer3.Skin.SongLoading_Title_Y - (this.txタイトル.sz画像サイズ.Height / 2) + nサブタイトル補正);
						}
					}
					if (this.txサブタイトル != null)
					{
						this.txサブタイトル.Opacity = C変換.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
						if (TJAPlayer3.Skin.SongLoading_SubTitle_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_SubTitle_X, TJAPlayer3.Skin.SongLoading_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
						else if (TJAPlayer3.Skin.SongLoading_SubTitle_ReferencePoint == CSkin.ReferencePoint.Right)
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SongLoading_SubTitle_X - (this.txサブタイトル.sz画像サイズ.Width * txタイトル.vc拡大縮小倍率.X), TJAPlayer3.Skin.SongLoading_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
						else
						{
							this.txサブタイトル.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.Skin.SongLoading_SubTitle_X - ((this.txサブタイトル.sz画像サイズ.Width * txサブタイトル.vc拡大縮小倍率.X) / 2)), TJAPlayer3.Skin.SongLoading_SubTitle_Y - (this.txサブタイトル.sz画像サイズ.Height / 2));
						}
					}
				}
			}
			//-----------------------------
			#endregion

			switch( base.eフェーズID )
			{
				case CStage.Eフェーズ.共通_フェードイン:
					//if( this.actFI.On進行描画() != 0 )			    // #27787 2012.3.10 yyagi 曲読み込み画面のフェードインの省略
																		// 必ず一度「CStaeg.Eフェーズ.共通_フェードイン」フェーズを経由させること。
																		// さもないと、曲読み込みが完了するまで、曲読み込み画面が描画されない。
						base.eフェーズID = CStage.Eフェーズ.NOWLOADING_DTXファイルを読み込む;
					return (int) E曲読込画面の戻り値.継続;

				case CStage.Eフェーズ.NOWLOADING_DTXファイルを読み込む:
					{
						timeBeginLoad = DateTime.Now;
						string str = TJAPlayer3.stage選曲.r確定されたスコア.ファイル情報.ファイルの絶対パス;


						CScoreIni ini = new CScoreIni( str + ".score.ini" );

						if( ( TJAPlayer3.DTX[0] != null ) && TJAPlayer3.DTX[0].b活性化してる )
							TJAPlayer3.DTX[0].On非活性化();

						//if( CDTXMania.DTX == null )
						{
							TJAPlayer3.DTX[0] = new CDTX( str, false, 1.0, ini.stファイル.BGMAdjust, 0, true );
							if( TJAPlayer3.ConfigIni.nPlayerCount == 2 )
								TJAPlayer3.DTX[1] = new CDTX( str, false, 1.0, ini.stファイル.BGMAdjust, 1, true );

							Trace.TraceInformation( "----曲情報-----------------" );
							Trace.TraceInformation( "TITLE: {0}", TJAPlayer3.DTX[0].TITLE );
							Trace.TraceInformation( "FILE: {0}",  TJAPlayer3.DTX[0].strファイル名の絶対パス );
							Trace.TraceInformation( "---------------------------" );

							TimeSpan span = (TimeSpan) ( DateTime.Now - timeBeginLoad );
							Trace.TraceInformation( "DTX読込所要時間:           {0}", span.ToString() );

							// 段位認定モード用。
							if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan && TJAPlayer3.DTX[0].List_DanSongs != null)
							{
								for (int i = 0; i < TJAPlayer3.DTX[0].List_DanSongs.Count; i++)
								{
									if (!string.IsNullOrEmpty(TJAPlayer3.DTX[0].List_DanSongs[i].Title))
									{
										using (CPrivateFastFont pfTitle = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 32))
										{
											using (var bmpSongTitle = pfTitle.DrawPrivateFont(TJAPlayer3.DTX[0].List_DanSongs[i].Title, TJAPlayer3.Skin.Game_DanC_Title_ForeColor, TJAPlayer3.Skin.Game_DanC_Title_BackColor))
											{
												TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex = TJAPlayer3.tテクスチャの生成(bmpSongTitle);
												TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex.vc拡大縮小倍率.X = TJAPlayer3.GetSongNameXScaling(ref TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex, 710);
											}
										}
									}

									if (!string.IsNullOrEmpty(TJAPlayer3.DTX[0].List_DanSongs[i].SubTitle))
									{
										using (CPrivateFastFont pfSubTitle = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 19))
										{
											using (var bmpSongSubTitle = pfSubTitle.DrawPrivateFont(TJAPlayer3.DTX[0].List_DanSongs[i].SubTitle, TJAPlayer3.Skin.Game_DanC_SubTitle_ForeColor, TJAPlayer3.Skin.Game_DanC_SubTitle_BackColor))
											{
												TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex = TJAPlayer3.tテクスチャの生成(bmpSongSubTitle);
												TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex.vc拡大縮小倍率.X = TJAPlayer3.GetSongNameXScaling(ref TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex, 710);
											}
										}
									}

								}
							}
						}

						base.eフェーズID = CStage.Eフェーズ.NOWLOADING_WAV読み込み待機;
						timeBeginLoadWAV = DateTime.Now;
						return (int) E曲読込画面の戻り値.継続;
					}

				case CStage.Eフェーズ.NOWLOADING_WAV読み込み待機:
					{
						if( this.ct待機.n現在の値 > 260 )
						{
							base.eフェーズID = CStage.Eフェーズ.NOWLOADING_WAVファイルを読み込む;
						}
						return (int) E曲読込画面の戻り値.継続;
					}

				case CStage.Eフェーズ.NOWLOADING_WAVファイルを読み込む:
					{
						if ( nWAVcount == 1 && TJAPlayer3.DTX[0].listWAV.Count > 0 )			// #28934 2012.7.7 yyagi (added checking Count)
						{
							ShowProgressByFilename( TJAPlayer3.DTX[0].listWAV[ nWAVcount ].strファイル名 );
						}
						int looptime = (TJAPlayer3.ConfigIni.b垂直帰線待ちを行う)? 3 : 1;	// VSyncWait=ON時は1frame(1/60s)あたり3つ読むようにする
						for ( int i = 0; i < looptime && nWAVcount <= TJAPlayer3.DTX[0].listWAV.Count; i++ )
						{
							if ( TJAPlayer3.DTX[0].listWAV[ nWAVcount ].listこのWAVを使用するチャンネル番号の集合.Count > 0 )	// #28674 2012.5.8 yyagi
							{
								TJAPlayer3.DTX[0].tWAVの読み込み( TJAPlayer3.DTX[0].listWAV[ nWAVcount ] );
							}
							nWAVcount++;
						}
						if ( nWAVcount <= TJAPlayer3.DTX[0].listWAV.Count )
						{
							ShowProgressByFilename( TJAPlayer3.DTX[0].listWAV[ nWAVcount ].strファイル名 );
						}
						if ( nWAVcount > TJAPlayer3.DTX[0].listWAV.Count )
						{
							TimeSpan span = ( TimeSpan ) ( DateTime.Now - timeBeginLoadWAV );
							Trace.TraceInformation( "WAV読込所要時間({0,4}):     {1}", TJAPlayer3.DTX[0].listWAV.Count, span.ToString() );
							timeBeginLoadWAV = DateTime.Now;

							if ( TJAPlayer3.ConfigIni.bDynamicBassMixerManagement )
							{
								TJAPlayer3.DTX[0].PlanToAddMixerChannel();
							}

							for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
							{
								TJAPlayer3.DTX[nPlayer].t太鼓チップのランダム化(TJAPlayer3.ConfigIni.eRandom[nPlayer]);
								//太鼓チップをランダム化する際、1P後2Pの処理をすぐに行うと、
								//1Pと酷似した、もしくは全く同じチップが流れてしまうので、
								//1Pのランダム化後、ある程度時間を空ける。
								Stopwatch sw = new Stopwatch();
								sw.Start();
								for (int i = 0; i < 1;)
								{
									if (sw.ElapsedMilliseconds >= 500)
									{
										sw.Reset();
										i++;
									}
								}
							}

							TJAPlayer3.stage演奏ドラム画面.On活性化();

							span = (TimeSpan) ( DateTime.Now - timeBeginLoadWAV );

							base.eフェーズID = CStage.Eフェーズ.NOWLOADING_BMPファイルを読み込む;
						}
						return (int) E曲読込画面の戻り値.継続;
					}

				case CStage.Eフェーズ.NOWLOADING_BMPファイルを読み込む:
					{
						TimeSpan span;
						DateTime timeBeginLoadBMPAVI = DateTime.Now;

						if ( TJAPlayer3.ConfigIni.bAVI有効 )
							TJAPlayer3.DTX[0].tAVIの読み込み();
						span = ( TimeSpan ) ( DateTime.Now - timeBeginLoadBMPAVI );

						span = ( TimeSpan ) ( DateTime.Now - timeBeginLoad );
						Trace.TraceInformation( "総読込時間:                {0}", span.ToString() );

						if(TJAPlayer3.ConfigIni.FastRender)
						{
							FastRender.Render();//staticに変更 インスタンス作成不要
						}

						if ( bitmapFilename != null )
						{
							bitmapFilename.Dispose();
							bitmapFilename = null;
						}
						if ( graphicsFilename != null )
						{
							graphicsFilename.Dispose();
							graphicsFilename = null;
						}
						if ( ftFilename != null )
						{
							ftFilename.Dispose();
							ftFilename = null;
						}
						TJAPlayer3.Timer.t更新();
						//CSound管理.rc演奏用タイマ.t更新();
						base.eフェーズID = CStage.Eフェーズ.NOWLOADING_システムサウンドBGMの完了を待つ;
						return (int) E曲読込画面の戻り値.継続;
					}

				case CStage.Eフェーズ.NOWLOADING_システムサウンドBGMの完了を待つ:
					{
						long nCurrentTime = TJAPlayer3.Timer.n現在時刻ms;
						if( nCurrentTime < this.nBGM再生開始時刻 )
							this.nBGM再生開始時刻 = nCurrentTime;

//						if ( ( nCurrentTime - this.nBGM再生開始時刻 ) > ( this.nBGMの総再生時間ms - 1000 ) )
						if ( ( nCurrentTime - this.nBGM再生開始時刻 ) >= ( this.nBGMの総再生時間ms ) )	// #27787 2012.3.10 yyagi 1000ms == フェードイン分の時間
						{
							base.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						}
						return (int) E曲読込画面の戻り値.継続;
					}

				case CStage.Eフェーズ.共通_フェードアウト:
					if ( this.ct待機.b終了値に達してない )
						return (int)E曲読込画面の戻り値.継続;

					if ( txFilename != null )
					{
						txFilename.Dispose();
					}
					if ( this.sd読み込み音 != null )
					{
						this.sd読み込み音.t解放する();
					}
					return (int) E曲読込画面の戻り値.読込完了;
			}
			return (int) E曲読込画面の戻り値.継続;
		}

		/// <summary>
		/// ESC押下時、trueを返す
		/// </summary>
		/// <returns></returns>
		protected bool tキー入力()
		{
			IInputDevice keyboard = TJAPlayer3.Input管理.Keyboard;
			if 	( keyboard.bキーが押された( (int)SlimDXKeys.Key.Escape ) )		// escape (exit)
			{
				return true;
			}
			return false;
		}


		private void ShowProgressByFilename(string strファイル名 )
		{
			if ( graphicsFilename != null && ftFilename != null )
			{
				graphicsFilename.Clear( Color.Transparent );
				graphicsFilename.DrawString( strファイル名, ftFilename, Brushes.White, new RectangleF( 0, 0, 640, 24 ) );
				if ( txFilename != null )
				{
					txFilename.Dispose();
				}
				txFilename = TJAPlayer3.tテクスチャの生成(bitmapFilename, true);
				txFilename.vc拡大縮小倍率 = new System.Numerics.Vector3( 0.5f, 0.5f, 1f );
				txFilename.t2D描画( TJAPlayer3.app.Device, 0, 720 - 16 );
			}
		}

		// その他

		#region [ private ]
		//-----------------
		private long nBGMの総再生時間ms;
		private long nBGM再生開始時刻;
		private CSound sd読み込み音;
		private string str曲タイトル;
		private string strサブタイトル;
		private CTexture txタイトル;
		private CTexture txサブタイトル;
		private DateTime timeBeginLoad;
		private DateTime timeBeginLoadWAV;
		private int nWAVcount;
		private CTexture txFilename;
		private Bitmap bitmapFilename;
		private Graphics graphicsFilename;
		private Font ftFilename;
		private CCounter ct待機;
		private CCounter ct曲名表示;
		//-----------------
		#endregion
	}
}
