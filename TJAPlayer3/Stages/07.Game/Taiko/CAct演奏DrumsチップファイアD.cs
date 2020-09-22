using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using FDK;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CAct演奏DrumsチップファイアD : CActivity
	{
		// コンストラクタ

		public CAct演奏DrumsチップファイアD()
		{
			base.b活性化してない = true;
		}
		
		
		// メソッド

		public virtual void Start( int nLane, E判定 judge, int player )
		{
			for (int j = 0; j < 3 * 4; j++)
			{
				if( !this.st状態[ j ].b使用中 )
				//for( int n = 0; n < 1; n++ )
				{
					this.st状態[ j ].b使用中 = true;
					//this.st状態[ n ].ct進行 = new CCounter( 0, 9, 20, CDTXMania.Timer );
					this.st状態[ j ].ct進行 = new CCounter( 0, 6, 25, TJAPlayer3.Timer );
					this.st状態[ j ].judge = judge;
					this.st状態[ j ].nPlayer = player;
					this.st状態_大[ j ].nPlayer = player;

					switch( nLane )
					{
						case 0x11:
						case 0x12:
							this.st状態[ j ].nIsBig = 0;
							break;
						case 0x13:
						case 0x14:
						case 0x1A:
						case 0x1B:
							this.st状態_大[ j ].ct進行 = new CCounter( 0, 9, 20, TJAPlayer3.Timer );
							this.st状態_大[ j ].judge = judge;
							this.st状態_大[ j ].nIsBig = 1;
							break;
					}
					break;
				}
			}
		}

		// CActivity 実装

		public override void On活性化()
		{
			for( int i = 0; i < 3 * 4; i++ )
			{
				this.st状態[ i ].ct進行 = new CCounter();
				this.st状態[ i ].b使用中 = false;
				this.st状態_大[ i ].ct進行 = new CCounter();
			}
			for( int i = 0; i < 256; i++ )
			{
				this.st紙吹雪[ i ] = new ST紙吹雪();
				this.st紙吹雪[ i ].b使用中 = false;
				this.st紙吹雪[ i ].ct進行 = new CCounter();
			}
			base.On活性化();
		}
		public override void On非活性化()
		{
			for( int i = 0; i < 3 * 4; i++ )
			{
				this.st状態[ i ].ct進行 = null;
				this.st状態_大[ i ].ct進行 = null;
			}
			for( int i = 0; i < 256; i++ )
			{
				this.st紙吹雪[ i ].ct進行 = null;
			}
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
	//            this.txアタックエフェクトUpper = CDTXMania.tテクスチャの生成Af( CSkin.Path( @"Graphics\7_explosion_upper.png" ) );
	//            this.txアタックエフェクトUpper_big = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\7_explosion_upper_big.png" ) );
				//if( this.txアタックエフェクトUpper != null )
				//{
				//	this.txアタックエフェクトUpper.b加算合成 = true;
				//}
	//            this.tx大音符花火[0] = CDTXMania.tテクスチャの生成Af( CSkin.Path( @"Graphics\7_explosion_bignotes_red.png" ) );
	//            this.tx大音符花火[0].b加算合成 = true;
	//            this.tx大音符花火[1] = CDTXMania.tテクスチャの生成Af( CSkin.Path( @"Graphics\7_explosion_bignotes_blue.png" ) );
	//            this.tx大音符花火[1].b加算合成 = true;
				//this.tx紙吹雪 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\7_particle paper.png" ) );
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				//CDTXMania.t安全にDisposeする( ref this.txアタックエフェクトUpper );
				//CDTXMania.t安全にDisposeする( ref this.txアタックエフェクトUpper_big );
	//            CDTXMania.t安全にDisposeする( ref this.tx大音符花火[ 0 ] );
	//            CDTXMania.t安全にDisposeする( ref this.tx大音符花火[ 1 ] );
				//CDTXMania.t安全にDisposeする( ref this.tx紙吹雪 );
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				for( int i = 0; i < 3 * 4; i++ )
				{
					if( this.st状態[ i ].b使用中 )
					{
						if( !this.st状態[ i ].ct進行.b停止中 )
						{
							this.st状態[ i ].ct進行.t進行();
							if( this.st状態[ i ].ct進行.b終了値に達した )
							{
								this.st状態[ i ].ct進行.t停止();
								this.st状態[ i ].b使用中 = false;
							}

							// (When performing calibration, reduce visual distraction
							// and current judgment feedback near the judgment position.)
							if( TJAPlayer3.Tx.Effects_Hit_Explosion != null && !TJAPlayer3.IsPerformingCalibration )
							{
								int n = this.st状態[ i ].nIsBig == 1 ? 520 : 0;
								int nX = ( TJAPlayer3.Skin.nScrollFieldX[ this.st状態[ i ].nPlayer ] ) - ( (TJAPlayer3.Tx.Effects_Hit_Explosion.sz画像サイズ.Width / 7 ) / 2 );
								int nY = ( TJAPlayer3.Skin.nJudgePointY[ this.st状態[ i ].nPlayer ] ) - ( (TJAPlayer3.Tx.Effects_Hit_Explosion.sz画像サイズ.Height / 4 ) / 2 );

								switch( st状態[ i ].judge )
								{
									case E判定.Perfect:
									case E判定.Great:
									case E判定.AutoPerfect:
										if (!this.st状態_大[i].ct進行.b停止中 && TJAPlayer3.Tx.Effects_Hit_Explosion_Big != null && this.st状態_大[i].nIsBig == 1)  
												TJAPlayer3.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n + 520, 260, 260));
										else
											TJAPlayer3.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n, 260, 260));
										break;                                    
									case E判定.Good:
										if (!this.st状態_大[i].ct進行.b停止中 && TJAPlayer3.Tx.Effects_Hit_Explosion_Big != null && this.st状態_大[i].nIsBig == 1)
											TJAPlayer3.Tx.Effects_Hit_Explosion.t2D描画( TJAPlayer3.app.Device, nX, nY, new Rectangle( this.st状態[ i ].ct進行.n現在の値 * 260, n + 780, 260, 260 ) );
										else
											TJAPlayer3.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n + 260, 260, 260));
										break;
									case E判定.Miss:
									case E判定.Bad:
										break;
								}
							}
						}
					}
				}

				for( int i = 0; i < 3 * 4; i++ )
				{
					if( !this.st状態_大[ i ].ct進行.b停止中 )
					{
						this.st状態_大[ i ].ct進行.t進行();
						if( this.st状態_大[ i ].ct進行.b終了値に達した )
						{
							this.st状態_大[ i ].ct進行.t停止();
						}
						if(TJAPlayer3.Tx.Effects_Hit_Explosion_Big != null && this.st状態_大[ i ].nIsBig == 1 )
						{

							switch( st状態_大[ i ].judge )
							{
								case E判定.Perfect:
								case E判定.Great:
								case E判定.AutoPerfect:
									if( this.st状態_大[ i ].nIsBig == 1 )
									{
										float fX = 415 - ((TJAPlayer3.Tx.Effects_Hit_Explosion_Big.sz画像サイズ.Width * TJAPlayer3.Tx.Effects_Hit_Explosion_Big.vc拡大縮小倍率.X ) / 2.0f);
										float fY = TJAPlayer3.Skin.nJudgePointY[ this.st状態_大[ i ].nPlayer ] - ((TJAPlayer3.Tx.Effects_Hit_Explosion_Big.sz画像サイズ.Height * TJAPlayer3.Tx.Effects_Hit_Explosion_Big.vc拡大縮小倍率.Y ) / 2.0f);
										//float fY = 257 - ((this.txアタックエフェクトUpper_big.sz画像サイズ.Height * this.txアタックエフェクトUpper_big.vc拡大縮小倍率.Y ) / 2.0f);

										////7
										float f倍率 = 0.5f + ( (this.st状態_大[ i ].ct進行.n現在の値 * 0.5f) / 10.0f);
										//this.txアタックエフェクトUpper_big.vc拡大縮小倍率.X = f倍率;
										//this.txアタックエフェクトUpper_big.vc拡大縮小倍率.Y = f倍率;
										//this.txアタックエフェクトUpper_big.n透明度 = (int)(255 * f倍率);
										//this.txアタックエフェクトUpper_big.t2D描画( CDTXMania.app.Device, fX, fY );

										Matrix4 mat = Matrix4.Identity;
										mat *= Matrix4.CreateScale( f倍率, f倍率, f倍率 );
										mat *= Matrix4.CreateTranslation(TJAPlayer3.Skin.nScrollFieldX[this.st状態_大[i].nPlayer] - GameWindowSize.Width / 2.0f, -(TJAPlayer3.Skin.nJudgePointY[this.st状態[i].nPlayer] - GameWindowSize.Height / 2.0f), 0f);
										//mat *= Matrix.Billboard( new Vector3( 15, 15, 15 ), new Vector3(0, 0, 0), new Vector3( 0, 0, 0 ), new Vector3( 0, 0, 0 ) );
										//mat *= Matrix.Translation( 0f, 0f, 0f );


										TJAPlayer3.Tx.Effects_Hit_Explosion_Big.Opacity = 255;
										TJAPlayer3.Tx.Effects_Hit_Explosion_Big.t3D描画( TJAPlayer3.app.Device, mat );
									}
									break;
									
								case E判定.Good:
									break;

								case E判定.Miss:
								case E判定.Bad:
									break;
							}
						}
					}
				}

				for (int i = 0; i < 45; i++)
				{
					if( TJAPlayer3.Skin.nScrollFieldX[0] != 414 )
						break;

					if (this.st大音符花火[i].b使用中)
					{
						this.st大音符花火[i].n前回のValue = this.st大音符花火[i].ct進行.n現在の値;
						this.st大音符花火[i].ct進行.t進行();
						if (this.st大音符花火[i].ct進行.b終了値に達した)
						{
							this.st大音符花火[i].ct進行.t停止();
							this.st大音符花火[i].b使用中 = false;
						}
						Matrix4 mat = Matrix4.Identity;

						mat *= Matrix4.CreateTranslation(this.st大音符花火[i].fX - GameWindowSize.Width / 2, -(this.st大音符花火[i].fY - GameWindowSize.Height / 2), 0f);
						float fX = this.st大音符花火[i].fX - ( 192 / 2 );
						float fY = this.st大音符花火[i].fY - ( 192 / 2 );

						//if(CDTXMania.Tx.Effects_Hit_FireWorks[ 0 ] != null && this.st大音符花火[ i ].nColor == 0 )
						//{
						//    if( this.st大音符花火[ i ].n開始フレーム <= this.st大音符花火[ i ].ct進行.n現在の値 && this.st大音符花火[ i ].n終了フレーム > this.st大音符花火[ i ].ct進行.n現在の値 )
						//    {
						//        //this.tx大音符花火[ 0 ].t3D描画(CDTXMania.app.Device, mat, new Rectangle( ( this.st大音符花火[i].ct進行.n現在の値 - this.st大音符花火[ i ].n開始フレーム ) * 192, 0, 192, 192 ));
						//        //this.tx大音符花火[ 0 ].t3D描画( CDTXMania.app.Device, mat, fX, fY, new Rectangle( ( this.st大音符花火[i].ct進行.n現在の値 - this.st大音符花火[ i ].n開始フレーム ) * 192, 0, 192, 192 ) );
						//        CDTXMania.Tx.Effects_Hit_FireWorks[ 0 ].t2D描画( CDTXMania.app.Device, (int)fX, (int)fY, new Rectangle( ( this.st大音符花火[i].ct進行.n現在の値 - this.st大音符花火[ i ].n開始フレーム ) * 192, 0, 192, 192 ) );
						//    }
						//}
						////if(CDTXMania.Tx.Effects_Hit_FireWorks[ 1 ] != null && this.st大音符花火[ i ].nColor == 1 )
						//{
						//    if( this.st大音符花火[ i ].n開始フレーム <= this.st大音符花火[ i ].ct進行.n現在の値 && this.st大音符花火[ i ].n終了フレーム > this.st大音符花火[ i ].ct進行.n現在の値 )
						//    {
						//        //this.tx大音符花火[ 1 ].t3D描画( CDTXMania.app.Device, mat, fX, fY, );
						//        //CDTXMania.Tx.Effects_Hit_FireWorks[ 1 ].t2D描画( CDTXMania.app.Device, (int)fX, (int)fY, new Rectangle( ( this.st大音符花火[i].ct進行.n現在の値 - this.st大音符花火[ i ].n開始フレーム ) * 192, 0, 192, 192 ) );
						//    }
						//}
					}

				}

				for (int i = 0; i < 256; i++)
				{
					if (this.st紙吹雪[i].b使用中)
					{
						this.st紙吹雪[i].n前回のValue = this.st紙吹雪[i].ct進行.n現在の値;
						this.st紙吹雪[i].ct進行.t進行();
						if (this.st紙吹雪[i].ct進行.b終了値に達した)
						{
							this.st紙吹雪[i].ct進行.t停止();
							this.st紙吹雪[i].b使用中 = false;
						}
						else if( this.st紙吹雪[ i ].fX > 1300 || this.st紙吹雪[ i ].fX < -20 )
						{
							this.st紙吹雪[i].ct進行.t停止();
							this.st紙吹雪[i].b使用中 = false;
						}
						for (int n = this.st紙吹雪[i].n前回のValue; n < this.st紙吹雪[i].ct進行.n現在の値; n++)
						{
							this.st紙吹雪[i].fX -= this.st紙吹雪[i].f加速度X;
							this.st紙吹雪[i].fY -= this.st紙吹雪[i].f加速度Y;
							this.st紙吹雪[i].f加速度X *= this.st紙吹雪[i].f加速度の加速度X;
							this.st紙吹雪[i].f加速度Y *= this.st紙吹雪[i].f加速度の加速度Y;
							this.st紙吹雪[i].f加速度Y -= this.st紙吹雪[i].f重力加速度;
						}
						Matrix4 mat = Matrix4.Identity;

						float x = (float)(this.st紙吹雪[i].f半径 * Math.Cos((Math.PI / 2 * this.st紙吹雪[i].ct進行.n現在の値) / 100.0)) * 2.3f;
						mat *= Matrix4.CreateScale(x, x, 1f);
						mat *= Matrix4.CreateTranslation(this.st紙吹雪[i].fX - GameWindowSize.Width / 2, -(this.st紙吹雪[i].fY - GameWindowSize.Height / 2), 0f);

						/*if (this.tx紙吹雪 != null)
						{
							this.tx紙吹雪.t3D描画(CDTXMania.app.Device, mat, new Rectangle( 32 * this.st紙吹雪[ i ].nGraphic, 32 * this.st紙吹雪[ i ].nColor, 32, 32 ));

						} */
					}

				}
			}
			return 0;
		}
		

		// その他

		#region [ private ]
		//-----------------
		//private CTextureAf txアタックエフェクトUpper;
		//private CTexture txアタックエフェクトUpper_big;
		//private CTextureAf[] tx大音符花火 = new CTextureAf[2];
		//private CTexture tx紙吹雪;

		protected STSTATUS[] st状態 = new STSTATUS[ 3 * 4 ];
		protected STSTATUS_B[] st状態_大 = new STSTATUS_B[ 3 * 4 ];
		private ST大音符花火[] st大音符花火 = new ST大音符花火[45];

		protected int[] nX座標 = new int[] { 450, 521, 596, 686, 778, 863, 970, 1070, 1150 };
		protected int[] nY座標 = new int[] { 172, 108,  50,   8, -10, -60,  -5,   30,   90 };
		protected int[] nY座標P2 = new int[] { 172, 108,  50,   8, -10, -60,  -5,   30,   90 };

		[StructLayout(LayoutKind.Sequential)]
		protected struct STSTATUS
		{
			public bool b使用中;
			public CCounter ct進行;
			public E判定 judge;
			public int nIsBig;
			public int n透明度;
			public int nPlayer;
		}
		[StructLayout(LayoutKind.Sequential)]
		protected struct STSTATUS_B
		{
			public CCounter ct進行;
			public E判定 judge;
			public int nIsBig;
			public int n透明度;
			public int nPlayer;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct ST大音符花火
		{
			public int nColor;
			public bool b使用中;
			public CCounter ct進行;
			public int n前回のValue;
			public float fX;
			public float fY;
			public int n開始フレーム;
			public int n終了フレーム;
		}

		private ST紙吹雪[] st紙吹雪 = new ST紙吹雪[ 256 ];
		[StructLayout( LayoutKind.Sequential )]
		private struct ST紙吹雪
		{
			public int nGraphic;
			public int nColor;
			public bool b使用中;
			public CCounter ct進行;
			public int n前回のValue;
			public float fX;
			public float fY;
			public float f加速度X;
			public float f加速度Y;
			public float f加速度の加速度X;
			public float f加速度の加速度Y;
			public float f重力加速度;
			public float f半径;
			public float f角度;
		}
		//-----------------
		#endregion
	}
}
　
