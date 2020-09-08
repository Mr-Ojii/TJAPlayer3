using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D9;
using FDK;
using DirectShowLib;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CAct演奏AVI : CActivity
	{
		// コンストラクタ

		public CAct演奏AVI()
		{
			base.b活性化してない = true;
		}


		// メソッド

		public void Start( int nチャンネル番号, CDTX.CDirectShow dsBGV )
		{
			if ( ( nチャンネル番号 == 0x54 || nチャンネル番号 == 0x5A ) && TJAPlayer3.ConfigIni.bAVI有効 )
			{
				this.dsBGV = dsBGV;
				if (this.dsBGV != null && this.dsBGV.dshow != null)
				{
					this.framewidth = (uint)this.dsBGV.dshow.n幅px;
					this.frameheight = (uint)this.dsBGV.dshow.n高さpx;
					this.fAVIアスペクト比 = ((float)this.framewidth) / ((float)this.frameheight);

					if ( fAVIアスペクト比 < 1.77f )
					{
						#region[ 旧規格クリップ時の処理。結果的には面倒な処理なんだよな____ ]
						this.n前回表示したフレーム番号 = -1;

						this.dsBGV = null;
						#endregion
					}
					if (this.tx描画用 == null)
					{
						this.tx描画用 = new CTexture(TJAPlayer3.app.Device, (int)this.framewidth, (int)this.frameheight, TJAPlayer3.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Managed);
					}

					#region[ リサイズ処理 ]
					if (fAVIアスペクト比 < 1.77f)
					{
						//旧企画クリップだった場合
						this.ratio1 = 720f / ((float)this.frameheight);
						this.tx描画用.vc拡大縮小倍率.X = this.ratio1;
						this.tx描画用.vc拡大縮小倍率.Y = this.ratio1;
					}
					else
					{
						//ワイドクリップの処理
						this.ratio1 = 1280f / ((float)this.framewidth);
						this.tx描画用.vc拡大縮小倍率.X = this.ratio1;
						this.tx描画用.vc拡大縮小倍率.Y = this.ratio1;
					}

					#endregion
				}

				if ( fAVIアスペクト比 > 1.77f && this.dsBGV != null && this.dsBGV.dshow != null )
				{
					this.dsBGV.dshow.t再生開始();
					this.bDShowクリップを再生している = true;
					this.b再生トグル = true;
				}
			}
		}
		public void SkipStart( int n移動開始時刻ms )
		{
			foreach ( CDTX.CChip chip in TJAPlayer3.DTX[0].listChip )
			{
				if ( chip.n発声時刻ms > n移動開始時刻ms )
				{
					break;
				}
				switch ( chip.eAVI種別 )
				{
					case EAVI種別.AVI:
						{
							this.Start( chip.nチャンネル番号, chip.rDShow );
							continue;
						}
				}
			}
		}
		public void Stop()
		{
			if( this.dsBGV != null )
			{
				if( this.dsBGV.dshow != null )
					this.dsBGV.dshow.MediaCtrl.Stop();
				this.bDShowクリップを再生している = false;
			}
		}
		public unsafe int t進行描画( int x, int y )
		{
			if ( !base.b活性化してない )
			{
				if (!this.bDShowクリップを再生している || (this.dsBGV.dshow == null || this.dsBGV == null))
				{
					return 0;
				}
				if ( this.tx描画用 == null )
				{
					return 0;
				}
				int time = (int) ( ( CSound管理.rc演奏用タイマ.n現在時刻 - (-1) ) * ( ( (double) TJAPlayer3.ConfigIni.n演奏速度 ) / 20.0 ) );
				int frameNoFromTime = 0;

				#region[ frameNoFromTime ]
				if ( (this.dsBGV != null) )
				{
					this.dsBGV.dshow.MediaSeeking.GetPositions(out this.lDshowPosition, out this.lStopPosition);
					frameNoFromTime = (int)lDshowPosition;	
				}
				#endregion

				if((((this.n前回表示したフレーム番号 != frameNoFromTime) || !this.bフレームを作成した)) && (fAVIアスペクト比 < 1.77f ))
				{
					this.n前回表示したフレーム番号 = frameNoFromTime;
					this.bフレームを作成した = true;
				}

				//ループ防止
				if ( this.lDshowPosition >= this.lStopPosition && this.dsBGV != null )
				{
					this.dsBGV.dshow.MediaSeeking.SetPositions(
					DsLong.FromInt64((long)(0)),
					AMSeekingSeekingFlags.AbsolutePositioning,
					null,
					AMSeekingSeekingFlags.NoPositioning);
					this.dsBGV.dshow.MediaCtrl.Stop();
					this.bDShowクリップを再生している = false;
				}

				if( ( this.tx描画用 != null ))
				{
					if ( ( this.bDShowクリップを再生している == true ) && this.dsBGV.dshow != null )
					{
						#region[ ワイドクリップ ]
						this.dsBGV.dshow.t現時点における最新のスナップイメージをTextureに転写する( this.tx描画用 );
						this.dsBGV.dshow.t現時点における最新のスナップイメージをTextureに転写する( this.tx窓描画用 );

						if( TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.背景のみ || TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.両方 )
						{
							if( this.dsBGV.dshow.b上下反転 )
								this.tx描画用.t2D上下反転描画( TJAPlayer3.app.Device, x, y );
							else
								this.tx描画用.t2D描画( TJAPlayer3.app.Device, x, y );
						}
						#endregion
					}
				}
			}
			return 0;
		}

		public void t窓表示()
		{
			//return;

			if( ( this.bDShowクリップを再生している == true ) && this.dsBGV.dshow != null )
			{
				#region[ ワイドクリップ ]
				float fRet = this.dsBGV.dshow.n幅px / this.dsBGV.dshow.n高さpx;

				if( this.tx窓描画用 != null && fRet == 1.0 )
				{
					float[] fRatio = new float[] { 640.0f - 4.0f, 360.0f - 4.0f, 322, 362 }; //中央下表示

					this.tx窓描画用.vc拡大縮小倍率.X = (float)( fRatio[ 0 ] / this.dsBGV.dshow.n幅px );
					this.tx窓描画用.vc拡大縮小倍率.Y = (float)( fRatio[ 1 ] / this.dsBGV.dshow.n高さpx );
					if( this.dsBGV.dshow.b上下反転 )
						this.tx窓描画用.t2D上下反転描画( TJAPlayer3.app.Device, (int)fRatio[ 2 ], (int)fRatio[ 3 ] );
					else
						this.tx窓描画用.t2D描画( TJAPlayer3.app.Device, (int)fRatio[ 2 ], (int)fRatio[ 3 ] );
				}

				#endregion
			}
		}

		public void tPauseControl()
		{
			if( this.b再生トグル == true )
			{
				if( this.dsBGV != null )
				{
					if( this.dsBGV.dshow != null )
						this.dsBGV.dshow.MediaCtrl.Pause();
				}
				this.b再生トグル = false;
			}
			else if( this.b再生トグル == false )
			{
				if(this.dsBGV != null )
				{
					if( this.dsBGV.dshow != null )
						this.dsBGV.dshow.MediaCtrl.Run();
				}
				this.b再生トグル = true;
			}
		}

		public void tReset()
		{
			if( this.dsBGV != null )
			{
				if( this.dsBGV.dshow != null )
				{
					this.dsBGV.dshow.MediaSeeking.SetPositions(
					DsLong.FromInt64((long)(0)),
					AMSeekingSeekingFlags.AbsolutePositioning,
					null,
					AMSeekingSeekingFlags.NoPositioning);
					this.dsBGV.dshow.MediaCtrl.Stop();
				}
			}
			if( this.tx描画用 != null && this.tx窓描画用 != null )
			{
				//2016.12.22 kairera0467 解放→生成というのもどうなのだろうか...
				TJAPlayer3.t安全にDisposeする( ref this.tx描画用 );
				TJAPlayer3.t安全にDisposeする( ref this.tx窓描画用 );

				this.tx描画用 = new CTexture( TJAPlayer3.app.Device, 1280, 720, TJAPlayer3.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Managed );
				this.tx窓描画用 = new CTexture( TJAPlayer3.app.Device, 1280, 720, TJAPlayer3.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Managed );
			}
		}

		// CActivity 実装

		public override void On活性化()
		{
			this.n前回表示したフレーム番号 = -1;
			this.bフレームを作成した = false;
			base.On活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if ( !base.b活性化してない )
			{
				this.tx描画用 = new CTexture( TJAPlayer3.app.Device, 1280, 720, TJAPlayer3.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Managed );
				this.tx窓描画用 = new CTexture( TJAPlayer3.app.Device, 1280, 720, TJAPlayer3.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Managed );

				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if ( !base.b活性化してない )
			{
				if ( this.tx描画用 != null )
				{
					this.tx描画用.Dispose();
					this.tx描画用 = null;
				}
				if( this.tx窓描画用 != null )
				{
					this.tx窓描画用.Dispose();
					this.tx窓描画用 = null;
				}
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			throw new InvalidOperationException( "t進行描画(int,int)のほうを使用してください。" );
		}


		// その他

		#region [ private ]
		//-----------------
		private bool bフレームを作成した;
		private int n前回表示したフレーム番号;

		public float fAVIアスペクト比;
		private uint frameheight;
		private uint framewidth;
		private float ratio1;

		private CTexture tx描画用;
		private CTexture tx窓描画用;

		//DirectShow用
		private bool b再生トグル;
		private bool bDShowクリップを再生している;
		private long lDshowPosition;
		private long lStopPosition;

		public CDTX.CDirectShow dsBGV;

		//-----------------
		#endregion
	}
}
