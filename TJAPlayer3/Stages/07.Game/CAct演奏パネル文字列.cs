using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏パネル文字列 : CActivity
	{

		// コンストラクタ

		public CAct演奏パネル文字列()
		{
			base.b活性化してない = true;
			this.Start();
		}


		// メソッド

		/// <summary>
		/// 右上の曲名、曲数表示の更新を行います。
		/// </summary>
		/// <param name="songName">曲名</param>
		/// <param name="genreName">ジャンル名</param>
		/// <param name="stageText">曲数</param>
		public void SetPanelString(string songName, string subtitle, string genreName, string stageText = null)
		{
			if( base.b活性化してる )
			{
				TJAPlayer3.t安全にDisposeする( ref this.txPanel );
				if( (songName != null ) && (songName.Length > 0 ) )
				{
					try
					{
						TJAPlayer3.t安全にDisposeする(ref txMusicName);
						TJAPlayer3.t安全にDisposeする(ref txSubTitleName);
						using (var bmpSongTitle = pfMusicName.DrawPrivateFont(songName, TJAPlayer3.Skin.Game_MusicName_ForeColor, TJAPlayer3.Skin.Game_MusicName_BackColor))
						{
							this.txMusicName = TJAPlayer3.tテクスチャの生成( bmpSongTitle );
						}
						if (txMusicName != null)
						{
							this.txMusicName.vc拡大縮小倍率.X = TJAPlayer3.GetSongNameXScaling(ref txMusicName);
						}
						if (!string.IsNullOrEmpty(subtitle))
						{
							using (var bmpSubTitle = pfSubTitleName.DrawPrivateFont(subtitle, TJAPlayer3.Skin.Game_MusicName_ForeColor, TJAPlayer3.Skin.Game_MusicName_BackColor))
							{
								this.txSubTitleName = TJAPlayer3.tテクスチャの生成(bmpSubTitle);
							}
							if (txSubTitleName != null)
							{
								this.txSubTitleName.vc拡大縮小倍率.X = TJAPlayer3.GetSongNameXScaling(ref txSubTitleName, 520);
							}
						}
					
						Bitmap bmpDiff;
						string strDiff = "";
						if (TJAPlayer3.Skin.eDiffDispMode == E難易度表示タイプ.n曲目に表示)
						{
							switch (TJAPlayer3.stage選曲.n確定された曲の難易度[0])
							{
								case 0:
									strDiff = "かんたん ";
									break;
								case 1:
									strDiff = "ふつう ";
									break;
								case 2:
									strDiff = "むずかしい ";
									break;
								case 3:
									strDiff = "おに ";
									break;
								case 4:
									strDiff = "えでぃと ";
									break;
								default:
									strDiff = "おに ";
									break;
							}
							bmpDiff = pfMusicName.DrawPrivateFont(strDiff + stageText, TJAPlayer3.Skin.Game_StageText_ForeColor, TJAPlayer3.Skin.Game_StageText_BackColor );
						}
						else
						{
							bmpDiff = pfMusicName.DrawPrivateFont(stageText, TJAPlayer3.Skin.Game_StageText_ForeColor, TJAPlayer3.Skin.Game_StageText_BackColor );
						}

						using (bmpDiff)
						{
							this.tx難易度とステージ数 = TJAPlayer3.tテクスチャの生成( bmpDiff );
						}
					}
					catch( CTextureCreateFailedException e )
					{
						Trace.TraceError( e.ToString() );
						Trace.TraceError( "パネル文字列テクスチャの生成に失敗しました。" );
						this.txPanel = null;
					}
				}
				if( !string.IsNullOrEmpty(genreName) )
				{
					this.txGENRE = TJAPlayer3.Tx.TxCGen(TJAPlayer3.Skin.nStrジャンルtoNum(genreName).ToString());	
				}

				this.ct進行用 = new CCounter( 0, 2000, 2, TJAPlayer3.Timer );
				this.Start();

			}
		}

		public void t歌詞テクスチャを生成する( Bitmap bmplyric )
		{
			TJAPlayer3.t安全にDisposeする(ref this.tx歌詞テクスチャ);
			this.tx歌詞テクスチャ = TJAPlayer3.tテクスチャの生成( bmplyric );
		}

		public void t歌詞テクスチャを削除する()
		{
			TJAPlayer3.t安全にDisposeする(ref this.tx歌詞テクスチャ);
		}

		/// <summary>
		/// レイヤー管理のため、On進行描画から分離。
		/// </summary>
		public void t歌詞テクスチャを描画する()
		{
			if( this.tx歌詞テクスチャ != null )
			{
				if (TJAPlayer3.Skin.Game_Lyric_ReferencePoint == CSkin.ReferencePoint.Left)
				{
				this.tx歌詞テクスチャ.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Lyric_X , TJAPlayer3.Skin.Game_Lyric_Y);
				}
				else if (TJAPlayer3.Skin.Game_Lyric_ReferencePoint == CSkin.ReferencePoint.Right)
				{
				this.tx歌詞テクスチャ.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Lyric_X - this.tx歌詞テクスチャ.szテクスチャサイズ.Width, TJAPlayer3.Skin.Game_Lyric_Y);
				}
				else
				{
				this.tx歌詞テクスチャ.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Lyric_X - (this.tx歌詞テクスチャ.szテクスチャサイズ.Width / 2), TJAPlayer3.Skin.Game_Lyric_Y);
				}
			}
		}

		public void Stop()
		{
			this.bMute = true;
		}
		public void Start()
		{
			this.bMute = false;
		}


		// CActivity 実装

		public override void On活性化()
		{
			this.pfMusicName = new CPrivateFastFont( new FontFamily( TJAPlayer3.ConfigIni.FontName), TJAPlayer3.Skin.Game_MusicName_FontSize);
			this.pfSubTitleName = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), TJAPlayer3.Skin.Game_SubTitleName_FontSize);

			this.txPanel = null;
			this.ct進行用 = new CCounter();
			this.Start();
			this.bFirst = true;
			base.On活性化();
		}
		public override void On非活性化()
		{
			this.ct進行用 = null;
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
				TJAPlayer3.t安全にDisposeする( ref this.txPanel );
				TJAPlayer3.t安全にDisposeする( ref this.txMusicName );
				TJAPlayer3.t安全にDisposeする( ref this.txSubTitleName);
				TJAPlayer3.t安全にDisposeする( ref this.txGENRE );
				TJAPlayer3.t安全にDisposeする(ref this.txPanel);
				TJAPlayer3.t安全にDisposeする(ref this.tx歌詞テクスチャ);
				TJAPlayer3.t安全にDisposeする(ref this.pfMusicName);
				TJAPlayer3.t安全にDisposeする(ref this.pfSubTitleName);
				TJAPlayer3.t安全にDisposeする(ref this.tx難易度とステージ数);
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			throw new InvalidOperationException( "t進行描画(x,y)のほうを使用してください。" );
		}
		public int t進行描画()
		{
			if (TJAPlayer3.stage演奏ドラム画面.actDan.IsAnimating) return 0;
			if( !base.b活性化してない && !this.bMute )
			{
				this.ct進行用.t進行Loop();
				if( this.bFirst )
				{
					this.ct進行用.n現在の値 = 300;
				}
				if( this.txGENRE != null )
					this.txGENRE.t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Genre_X, TJAPlayer3.Skin.Game_Genre_Y );

				if (TJAPlayer3.Skin.b現在のステージ数を表示しない)
				{
					if (this.txMusicName != null)
					{
						float fRate = 660.0f / this.txMusicName.szテクスチャサイズ.Width;
						if (this.txMusicName.szテクスチャサイズ.Width <= 660.0f)
							fRate = 1.0f;
						this.txMusicName.vc拡大縮小倍率.X = fRate;
						if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Center)
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - ((this.txMusicName.szテクスチャサイズ.Width * fRate) / 2), TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X, TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - (this.txMusicName.szテクスチャサイズ.Width * fRate), TJAPlayer3.Skin.Game_MusicName_Y);
						}
						if (this.txSubTitleName != null)
						{
							fRate = 600.0f / this.txSubTitleName.szテクスチャサイズ.Width;
							if (this.txSubTitleName.szテクスチャサイズ.Width <= 600.0f)
								fRate = 1.0f;
							this.txSubTitleName.vc拡大縮小倍率.X = fRate;
							if (TJAPlayer3.Skin.Game_SubTitleName_ReferencePoint == CSkin.ReferencePoint.Center)
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X - ((this.txSubTitleName.szテクスチャサイズ.Width * fRate) / 2), TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
							else if (TJAPlayer3.Skin.Game_SubTitleName_ReferencePoint == CSkin.ReferencePoint.Left)
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X, TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
							else
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X - (this.txSubTitleName.szテクスチャサイズ.Width * fRate), TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
						}
					}
				}
				else
				{
					#region[ 透明度制御 ]

					if (this.txMusicName != null)
					{
						if (this.ct進行用.n現在の値 < 745)
						{
							this.bFirst = false;
							this.txMusicName.Opacity = 255;
							if (this.txSubTitleName != null)
								this.txSubTitleName.Opacity = 255;
							if (this.txGENRE != null)
								this.txGENRE.Opacity = 255;
							this.tx難易度とステージ数.Opacity = 0;
						}
						else if (this.ct進行用.n現在の値 >= 745 && this.ct進行用.n現在の値 < 1000)
						{
							this.txMusicName.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
							if (this.txSubTitleName != null)
								this.txSubTitleName.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
							if (this.txGENRE != null)
								this.txGENRE.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
							this.tx難易度とステージ数.Opacity = this.ct進行用.n現在の値 - 745;
						}
						else if (this.ct進行用.n現在の値 >= 1000 && this.ct進行用.n現在の値 <= 1745)
						{
							this.txMusicName.Opacity = 0;
							if (this.txSubTitleName != null)
								this.txSubTitleName.Opacity = 0;
							if (this.txGENRE != null)
								this.txGENRE.Opacity = 0;
							this.tx難易度とステージ数.Opacity = 255;
						}
						else if (this.ct進行用.n現在の値 >= 1745)
						{
							this.txMusicName.Opacity = this.ct進行用.n現在の値 - 1745;
							if (this.txSubTitleName != null)
								this.txSubTitleName.Opacity = this.ct進行用.n現在の値 - 1745;
							if (this.txGENRE != null)
								this.txGENRE.Opacity = this.ct進行用.n現在の値 - 1745;
							this.tx難易度とステージ数.Opacity = 255 - (this.ct進行用.n現在の値 - 1745);
						}
						#endregion
						if (this.b初めての進行描画)
						{
							b初めての進行描画 = false;
						}
						if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Center)
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - ((this.txMusicName.szテクスチャサイズ.Width * txMusicName.vc拡大縮小倍率.X) / 2), TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X, TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else
						{
							this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - (this.txMusicName.szテクスチャサイズ.Width * txMusicName.vc拡大縮小倍率.X), TJAPlayer3.Skin.Game_MusicName_Y);
						}
						if (this.txSubTitleName != null)
						{
							if (TJAPlayer3.Skin.Game_SubTitleName_ReferencePoint == CSkin.ReferencePoint.Center)
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X - ((this.txSubTitleName.szテクスチャサイズ.Width * this.txSubTitleName.vc拡大縮小倍率.X) / 2), TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
							else if (TJAPlayer3.Skin.Game_SubTitleName_ReferencePoint == CSkin.ReferencePoint.Left)
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X, TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
							else
							{
								this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_SubTitleName_X - (this.txSubTitleName.szテクスチャサイズ.Width * this.txSubTitleName.vc拡大縮小倍率.X), TJAPlayer3.Skin.Game_SubTitleName_Y);
							}
						}
					}
					if (this.tx難易度とステージ数 != null)
						if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Center)
						{
							this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - (this.tx難易度とステージ数.szテクスチャサイズ.Width / 2), TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else if (TJAPlayer3.Skin.Game_MusicName_ReferencePoint == CSkin.ReferencePoint.Left)
						{
							this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X, TJAPlayer3.Skin.Game_MusicName_Y);
						}
						else
						{
							this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_MusicName_X - this.tx難易度とステージ数.szテクスチャサイズ.Width, TJAPlayer3.Skin.Game_MusicName_Y);
						}
				}
			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter ct進行用;

		private CTexture txPanel;
		private bool bMute;
		private bool bFirst;

		private CTexture txMusicName;
		private CTexture txSubTitleName;
		private CTexture tx難易度とステージ数;
		private CTexture txGENRE;
		private CTexture tx歌詞テクスチャ;
		private CPrivateFastFont pfMusicName;
		private CPrivateFastFont pfSubTitleName;
		//-----------------
		#endregion
	}
}
　
