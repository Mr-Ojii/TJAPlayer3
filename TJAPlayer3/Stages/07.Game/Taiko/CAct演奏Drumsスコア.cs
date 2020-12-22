using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏Drumsスコア : CAct演奏スコア共通
	{
		// CActivity 実装（共通クラスからの差分のみ）

		public override int On進行描画()
		{
			if (!base.b活性化してない)
			{
				if (base.b初めての進行描画)
				{
					base.b初めての進行描画 = false;
				}
				if( !this.ctTimer.b停止中 )
				{
					this.ctTimer.t進行();
					if( this.ctTimer.b終了値に達した )
					{
						this.ctTimer.t停止();
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (!this.ct点数アニメタイマ[i].b停止中)
					{
						this.ct点数アニメタイマ[i].t進行();
						if (this.ct点数アニメタイマ[i].b終了値に達した)
						{
							this.ct点数アニメタイマ[i].t停止();
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (!this.ctボーナス加算タイマ[i].b停止中)
					{
						this.ctボーナス加算タイマ[i].t進行();
						if (this.ctボーナス加算タイマ[i].b終了値に達した)
						{
							TJAPlayer3.stage演奏ドラム画面.actScore.BonusAdd(i);
							this.ctボーナス加算タイマ[i].t停止();
						}
					}
				}

				base.t小文字表示(TJAPlayer3.Skin.Game_Score_X[0], TJAPlayer3.Skin.Game_Score_Y[0], string.Format( "{0,7:######0}", this.n現在表示中のスコア[ 0 ] ), 0 , 256, 0);
				if( TJAPlayer3.stage演奏ドラム画面.bDoublePlay ) base.t小文字表示(TJAPlayer3.Skin.Game_Score_X[1], TJAPlayer3.Skin.Game_Score_Y[1], string.Format( "{0,7:######0}", this.n現在表示中のスコア[ 1 ] ), 0 , 256, 1);

				for( int i = 0; i < 256; i++ )
				{
					if( this.stScore[ i ].b使用中 )
					{
						if( !this.stScore[ i ].ctTimer.b停止中 )
						{
							this.stScore[ i ].ctTimer.t進行();
							if( this.stScore[ i ].ctTimer.b終了値に達した )
							{
								this.n現在表示中のスコア[ this.stScore[ i ].nPlayer ] += (long)this.stScore[ i ].nAddScore;
								if( this.stScore[ i ].b表示中 == true )
									this.n現在表示中のAddScore--;
								this.stScore[ i ].ctTimer.t停止();
								this.stScore[ i ].b使用中 = false;
								if (ct点数アニメタイマ[stScore[i].nPlayer].b終了値に達してない)
								{
									this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 11, 12, TJAPlayer3.Timer);
									this.ct点数アニメタイマ[stScore[i].nPlayer].n現在の値 = 1;
								}
								else
								{
									this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 11, 12, TJAPlayer3.Timer);
								}
								TJAPlayer3.stage演奏ドラム画面.actDan.Update();
							}

							int xAdd = 0;
							int yAdd = 0;
							int alpha = 0;

							if ( this.stScore[i].ctTimer.n現在の値 < 10)
							{
								xAdd = 25;
								alpha = 150;
							} else if (this.stScore[i].ctTimer.n現在の値 < 20)
							{
								xAdd = 10;
								alpha = 200;
							} else if (this.stScore[i].ctTimer.n現在の値 < 30)
							{
								xAdd = -5;
								alpha = 250;
							} else if (this.stScore[i].ctTimer.n現在の値 < 40)
							{
								xAdd = -9;
								alpha = 256;
							} else if (this.stScore[i].ctTimer.n現在の値 < 50)
							{
								xAdd = -10;
								alpha = 256;
							} else if (this.stScore[i].ctTimer.n現在の値 < 60)
							{
								xAdd = -9;
								alpha = 256;
							} else if (this.stScore[i].ctTimer.n現在の値 < 70)
							{
								xAdd = -5;
								alpha = 256;
							} else if (this.stScore[i].ctTimer.n現在の値 < 80)
							{
								xAdd = -3;
								alpha = 256;
							} else
							{
								xAdd = 0;
								alpha = 256;
							}



							if ( this.stScore[ i ].ctTimer.n現在の値 > 300 )
							{
								yAdd = -1;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 310)
							{
								yAdd = -5;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 320)
							{
								yAdd = -7;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 330)
							{
								yAdd = -8;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 340)
							{
								yAdd = -8;
								alpha = 256;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 350)
							{
								yAdd = -6;
								alpha = 256;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 360)
							{
								yAdd = 0;
								alpha = 256;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 370)
							{
								yAdd = 5;
								alpha = 200;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 380)
							{
								yAdd = 12;
								alpha = 150;
							}
							if (this.stScore[i].ctTimer.n現在の値 > 390)
							{
								yAdd = 20;
								alpha = 0;
							}


							if ( this.n現在表示中のAddScore < 10 && this.stScore[ i ].bBonusScore == false )
								base.t小文字表示(TJAPlayer3.Skin.Game_Score_Add_X[this.stScore[i].nPlayer] + xAdd, this.stScore[ i ].nPlayer == 0 ? TJAPlayer3.Skin.Game_Score_Add_Y[ this.stScore[ i ].nPlayer ] + yAdd : TJAPlayer3.Skin.Game_Score_Add_Y[ this.stScore[ i ].nPlayer ] - yAdd, string.Format( "{0,7:######0}", this.stScore[ i ].nAddScore ), this.stScore[ i ].nPlayer + 1 , alpha, stScore[i].nPlayer);
							if( this.n現在表示中のAddScore < 10 && this.stScore[ i ].bBonusScore == true )
								base.t小文字表示(TJAPlayer3.Skin.Game_Score_AddBonus_X[this.stScore[i].nPlayer] + xAdd, TJAPlayer3.Skin.Game_Score_AddBonus_Y[ this.stScore[ i ].nPlayer ], string.Format( "{0,7:######0}", this.stScore[ i ].nAddScore ), this.stScore[ i ].nPlayer + 1 , alpha, stScore[i].nPlayer);
							else
							{
								this.n現在表示中のAddScore--;
								this.stScore[ i ].b表示中 = false;
							}
						}
					}
				}
			}
			return 0;
		}
	}
}
