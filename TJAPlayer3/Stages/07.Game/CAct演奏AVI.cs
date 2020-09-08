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
using System.Runtime.CompilerServices;
using System.Windows.Forms;

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

		public void Start( int nチャンネル番号, CVideoDecoder rVD )
		{
			if ( ( nチャンネル番号 == 0x54 || nチャンネル番号 == 0x5A ) && TJAPlayer3.ConfigIni.bAVI有効 )
			{
				this.rVD = rVD;
				if (this.rVD != null)
				{
					this.frameheight = (uint)rVD.FrameSize.Height;
					this.framewidth = (uint)rVD.FrameSize.Width;

					#region[ リサイズ処理 ]
					if (((float)this.framewidth) / ((float)this.frameheight) < 1.77f)
					{
						//旧企画クリップだった場合
						this.ratio1 = (float)GameWindowSize.Height / ((float)this.frameheight);
					}
					else
					{
						//ワイドクリップの処理
						this.ratio1 = (float)GameWindowSize.Width / ((float)this.framewidth);
					}
					#endregion
					this.rVD.Start();
				}
			}
		}
		public void SkipStart( int n移動開始時刻ms )
		{
			if (this.rVD != null)
			{
				this.rVD.SkipStart(n移動開始時刻ms);
			}
		}

		public void Stop()
		{
			if (this.rVD != null)
			{
				this.rVD.Stop();
			}
		}

		public unsafe int t進行描画( int x, int y )
		{
			if ( !base.b活性化してない )
			{
				if (this.rVD == null)
					return 0;

				if (this.rVD != null)
				{
					#region[ ワイドクリップ ]
					this.tx描画用 = this.rVD.GetNowFrame(TJAPlayer3.app.Device);
					this.tx窓描画用 = this.rVD.GetNowFrame(TJAPlayer3.app.Device);

					this.tx描画用.vc拡大縮小倍率.X = this.ratio1;
					this.tx描画用.vc拡大縮小倍率.Y = this.ratio1;

					if (TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.背景のみ || TJAPlayer3.ConfigIni.eClipDispType == EClipDispType.両方)
					{
						this.tx描画用.t2D描画(TJAPlayer3.app.Device, x, y);
					}
					#endregion
				}
				
			}
			return 0;
		}

		public void t窓表示()
		{
			if( this.rVD != null )
			{
				#region[ ワイドクリップ ]

				if( this.tx窓描画用 != null )
				{
					float[] fRatio = new float[] { 640.0f - 4.0f, 360.0f - 4.0f, 322, 362 }; //中央下表示

					this.tx窓描画用.vc拡大縮小倍率.X = (float)(fRatio[0] / this.rVD.FrameSize.Width);
					this.tx窓描画用.vc拡大縮小倍率.Y = (float)(fRatio[1] / this.rVD.FrameSize.Height);
						
					this.tx窓描画用.t2D描画( TJAPlayer3.app.Device, (int)fRatio[ 2 ], (int)fRatio[ 3 ] );
				}

				#endregion
			}
		}

		public void tPauseControl()
		{
			if (this.rVD != null)
			{
				this.rVD.PauseControl();
			}
		}

		public void tReset()
		{
			if (this.rVD != null) 
			{
				this.rVD.Reset();
			}
		}

		// CActivity 実装

		public override void On活性化()
		{
			base.On活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if ( !base.b活性化してない )
			{
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

		private uint frameheight;
		private uint framewidth;
		private float ratio1;

		private CTexture tx描画用;
		private CTexture tx窓描画用;

		public CVideoDecoder rVD;

		//-----------------
		#endregion
	}
}
