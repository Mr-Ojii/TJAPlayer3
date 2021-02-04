using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏Drumsゲージ : CAct演奏ゲージ共通
	{
		// プロパティ

//		public double db現在のゲージ値
//		{
//			get
//			{
//				return this.dbゲージ値;
//			}
//			set
//			{
//				this.dbゲージ値 = value;
//				if( this.dbゲージ値 > 1.0 )
//				{
//					this.dbゲージ値 = 1.0;
//				}
//			}
//		}

		
		// コンストラクタ
		/// <summary>
		/// ゲージの描画クラス。ドラム側。
		/// 
		/// 課題
		/// _ゲージの実装。
		/// _Danger時にゲージの色が変わる演出の実装。
		/// _Danger、MAX時のアニメーション実装。
		/// </summary>
		public CAct演奏Drumsゲージ()
		{
			base.b活性化してない = true;
		}

		// CActivity 実装

		public override void On活性化()
		{
			this.ct炎 = new CCounter( 0, 6, 50, TJAPlayer3.Timer );

			base.On活性化();
		}
		public override void On非活性化()
		{
			this.ct炎 = null;
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				if(TJAPlayer3.Skin.Game_Gauge_Rainbow_Timer <= 1)
				{
					throw new DivideByZeroException("SkinConfigの設定\"Game_Gauge_Rainbow_Timer\"を1以下にすることは出来ません。");
				}
				this.ct虹アニメ = new CCounter( 0, TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn -1, TJAPlayer3.Skin.Game_Gauge_Rainbow_Timer, TJAPlayer3.Timer );
				this.ct虹透明度 = new CCounter(0, TJAPlayer3.Skin.Game_Gauge_Rainbow_Timer-1, 1, TJAPlayer3.Timer);
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				this.ct虹アニメ = null;
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if ( !base.b活性化してない )
			{
				//CDTXMania.act文字コンソール.tPrint( 20, 150, C文字コンソール.Eフォント種別.白, this.db現在のゲージ値.Taiko.ToString() );

				#region [ 初めての進行描画 ]
				if ( base.b初めての進行描画 )
				{
					base.b初めての進行描画 = false;
				}
				#endregion


				int nRectX = (int)( this.db現在のゲージ値[ 0 ] / 2 ) * 14;
				int nRectX2P = (int)( this.db現在のゲージ値[ 1 ] / 2 ) * 14;
				int 虹ベース = ct虹アニメ.n現在の値 + 1;
				if (虹ベース == ct虹アニメ.n終了値+1) 虹ベース = 0;
				/*

				新虹ゲージの仕様  2018/08/10 ろみゅ～？
				 
				 フェードで動く虹ゲージが、ある程度強化できたので放出。
				 透明度255の虹ベースを描画し、その上から透明度可変式の虹ゲージを描画する。
				 ゲージのパターン枚数は、読み込み枚数によって決定する。
				 ゲージ描画の切り替え速度は、タイマーの値をSkinConfigで指定して行う(初期値50,1にするとエラーを吐く模様)。進行速度は1ms、高フレームレートでの滑らかさを重視。
				 虹ゲージの透明度調整値は、「255/パターン数」で算出する。
				 こんな簡単なことを考えるのに30分(60f/s換算で108000f)を費やす。
				 
				*/
				if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
				{
					int[] ypos = new int[] { 144, 532 };
					for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
					{
						if (TJAPlayer3.Tx.Gauge_Base[nPlayer] != null)
						{
							TJAPlayer3.Tx.Gauge_Base[nPlayer].t2D描画(TJAPlayer3.app.Device, 492, ypos[nPlayer], new Rectangle(0, 0, 700, 44));
						}
					}
				}
				else
				{
					if (TJAPlayer3.Tx.Gauge_Base_Danc != null)
					{
						TJAPlayer3.Tx.Gauge_Base_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, 700, 44));
					}
				}
				#region[ ゲージ1P ]				
				if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
				{
					if (TJAPlayer3.Tx.Gauge[0] != null)
					{
						TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, nRectX, 44));

						if (TJAPlayer3.Tx.Gauge_Line[0] != null)
						{
							if (this.db現在のゲージ値[0] >= 100.0)
							{
								this.ct虹アニメ.t進行Loop();
								this.ct虹透明度.t進行Loop();
								if (TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値] != null)
								{
									TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値].Opacity = 255;
									TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値].t2D描画(TJAPlayer3.app.Device, 492, 144);
									TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
									TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].t2D描画(TJAPlayer3.app.Device, 492, 144);
								}
							}
							TJAPlayer3.Tx.Gauge_Line[0].t2D描画(TJAPlayer3.app.Device, 492, 144);
						}
						#region[ 「クリア」文字 ]
						if (this.db現在のゲージ値[0] >= 80.0)
						{
							TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 1038, 144, new Rectangle(0, 44, 58, 24));
						}
						else
						{
							TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 1038, 144, new Rectangle(58, 44, 58, 24));
						}
						#endregion
					}
				}
				else {

					if (TJAPlayer3.Tx.Gauge_Danc != null) 
					{
						TJAPlayer3.Tx.Gauge_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, nRectX, 44));

						if (TJAPlayer3.Tx.Gauge_Line_Danc != null)
						{
							if (this.db現在のゲージ値[0] >= 100.0)
							{
								this.ct虹アニメ.t進行Loop();
								this.ct虹透明度.t進行Loop();
								if (TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値] != null)
								{
									TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値].Opacity = 255;
									TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値].t2D描画(TJAPlayer3.app.Device, 492, 144);
									TJAPlayer3.Tx.Gauge_Rainbow_Danc[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
									TJAPlayer3.Tx.Gauge_Rainbow_Danc[虹ベース].t2D描画(TJAPlayer3.app.Device, 492, 144);
								}
							}
							TJAPlayer3.Tx.Gauge_Line_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144);
						}
					}
				}
				#endregion
				#region[ ゲージ2P ]
				if( TJAPlayer3.stage演奏ドラム画面.bDoublePlay && TJAPlayer3.Tx.Gauge[1] != null )
				{
					TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 492, 532, new Rectangle( 0, 0, nRectX2P, 44 ) );
					if(TJAPlayer3.Tx.Gauge[1] != null )
					{
						if (this.db現在のゲージ値[1] >= 100.0)
						{
							this.ct虹アニメ.t進行Loop();
							this.ct虹透明度.t進行Loop();
							if (TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値] != null)
							{
								TJAPlayer3.Tx.Gauge_Rainbow[ct虹アニメ.n現在の値].Opacity = 255;
								TJAPlayer3.Tx.Gauge_Rainbow[ct虹アニメ.n現在の値].t2D上下反転描画(TJAPlayer3.app.Device, 492, 532);
								TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
								TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].t2D上下反転描画(TJAPlayer3.app.Device, 492, 532);
							}
						}
						TJAPlayer3.Tx.Gauge_Line[1].t2D描画( TJAPlayer3.app.Device, 492, 532 );
					}
					#region[ 「クリア」文字 ]
					if( this.db現在のゲージ値[ 1 ] >= 80.0 )
					{
						TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 1038, 554, new Rectangle( 0, 44, 58, 24 ) );
					}
					else
					{
						TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 1038, 554, new Rectangle( 58, 44, 58, 24 ) );
					}
					#endregion
				}
				#endregion


				if(TJAPlayer3.Tx.Gauge_Soul_Fire != null )
				{
					//仮置き
					int[] nSoulFire = new int[] { 52, 443, 0, 0 };
					for( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
					{
						if( this.db現在のゲージ値[ i ] >= 100.0 )
						{
							this.ct炎.t進行Loop();
							TJAPlayer3.Tx.Gauge_Soul_Fire.t2D描画( TJAPlayer3.app.Device, 1112, nSoulFire[ i ], new Rectangle( 230 * ( this.ct炎.n現在の値 ), 0, 230, 230 ) );
						}
					}
				}
				if(TJAPlayer3.Tx.Gauge_Soul != null )
				{
					//仮置き
					int[] nSoulY = new int[] { 125, 516, 0, 0 };
					for( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
					{
						if( this.db現在のゲージ値[ i ] >= 80.0 )
						{
							TJAPlayer3.Tx.Gauge_Soul.t2D描画( TJAPlayer3.app.Device, 1184, nSoulY[ i ], new Rectangle( 0, 0, 80, 80 ) );
						}
						else
						{
							TJAPlayer3.Tx.Gauge_Soul.t2D描画( TJAPlayer3.app.Device, 1184, nSoulY[ i ], new Rectangle( 0, 80, 80, 80 ) );
						}
					}
				}

			}
			return 0;
		}
	}
}
