using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using FDK;

using Rectangle = System.Drawing.Rectangle;

namespace TJAPlayer3
{
	internal class CActMtaiko : CActivity
	{
		/// <summary>
		/// mtaiko部分を描画するクラス。左側だけ。
		/// 
		/// </summary>
		public CActMtaiko()
		{
			base.b活性化してない = true;
		}

		public override void On活性化()
		{
			for( int i = 0; i < 16; i++ )
			{
				STパッド状態 stパッド状態 = new STパッド状態();
				stパッド状態.n明るさ = 0;
				this.stパッド状態[ i ] = stパッド状態;
			}
			base.On活性化();
		}

		public override void On非活性化()
		{
			base.On非活性化();
		}

		public override void OnManagedリソースの作成()
		{
			this.ctレベルアップダウン = new CCounter[ 4 ];
			this.After = new int[ 4 ];
			this.Before = new int[ 4 ];
			for( int i = 0; i < 4; i++ )
			{
				this.ctレベルアップダウン[ i ] = new CCounter();
			}


			base.OnManagedリソースの作成();
		}

		public override void OnManagedリソースの解放()
		{
			this.ctレベルアップダウン = null;

			base.OnManagedリソースの解放();
		}

		public override int On進行描画()
		{
			if( base.b初めての進行描画 )
			{
				this.nフラッシュ制御タイマ = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
				base.b初めての進行描画 = false;
			}
			
			long num = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
			if( num < this.nフラッシュ制御タイマ )
			{
				this.nフラッシュ制御タイマ = num;
			}
			while( ( num - this.nフラッシュ制御タイマ ) >= 20 )
			{
				for( int j = 0; j < 16; j++ )
				{
					if( this.stパッド状態[ j ].n明るさ > 0 )
					{
						this.stパッド状態[ j ].n明るさ--;
					}
				}
				this.nフラッシュ制御タイマ += 20;
			}

			if(TJAPlayer3.Tx.Taiko_Background[0] != null )
				TJAPlayer3.Tx.Taiko_Background[0].t2D描画( TJAPlayer3.app.Device, 0, 184 );

			if ( TJAPlayer3.stage演奏ドラム画面.bDoublePlay )
			{
				if(TJAPlayer3.Tx.Taiko_Background[1] != null )
					TJAPlayer3.Tx.Taiko_Background[1].t2D描画( TJAPlayer3.app.Device, 0, 360 );
			}
			
			if(TJAPlayer3.Tx.Taiko_Base != null )
			{
				TJAPlayer3.Tx.Taiko_Base.t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[0], TJAPlayer3.Skin.Game_Taiko_Y[0]);
				if( TJAPlayer3.stage演奏ドラム画面.bDoublePlay )
					TJAPlayer3.Tx.Taiko_Base.t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[1], TJAPlayer3.Skin.Game_Taiko_Y[1]);
			}
			if( TJAPlayer3.Tx.Taiko_Don_Left != null && TJAPlayer3.Tx.Taiko_Don_Right != null && TJAPlayer3.Tx.Taiko_Ka_Left != null && TJAPlayer3.Tx.Taiko_Ka_Right != null )
			{
				TJAPlayer3.Tx.Taiko_Ka_Left.Opacity = this.stパッド状態[0].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Ka_Right.Opacity = this.stパッド状態[1].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Don_Left.Opacity = this.stパッド状態[2].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Don_Right.Opacity = this.stパッド状態[3].n明るさ * 73;

				TJAPlayer3.Tx.Taiko_Ka_Left.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[0], TJAPlayer3.Skin.Game_Taiko_Y[0], new Rectangle(0, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Ka_Right.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[0] + TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Skin.Game_Taiko_Y[0], new Rectangle(TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Don_Left.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[0], TJAPlayer3.Skin.Game_Taiko_Y[0], new Rectangle(0, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Don_Right.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[0] + TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Skin.Game_Taiko_Y[0], new Rectangle(TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
			}

			if( TJAPlayer3.Tx.Taiko_Don_Left != null && TJAPlayer3.Tx.Taiko_Don_Right != null && TJAPlayer3.Tx.Taiko_Ka_Left != null && TJAPlayer3.Tx.Taiko_Ka_Right != null )
			{
				TJAPlayer3.Tx.Taiko_Ka_Left.Opacity = this.stパッド状態[4].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Ka_Right.Opacity = this.stパッド状態[5].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Don_Left.Opacity = this.stパッド状態[6].n明るさ * 73;
				TJAPlayer3.Tx.Taiko_Don_Right.Opacity = this.stパッド状態[7].n明るさ * 73;

				TJAPlayer3.Tx.Taiko_Ka_Left.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[1], TJAPlayer3.Skin.Game_Taiko_Y[1], new Rectangle(0, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Ka_Right.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[1] + TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Skin.Game_Taiko_Y[1], new Rectangle(TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Don_Left.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[1], TJAPlayer3.Skin.Game_Taiko_Y[1], new Rectangle(0, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
				TJAPlayer3.Tx.Taiko_Don_Right.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_X[1] + TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Skin.Game_Taiko_Y[1], new Rectangle(TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, TJAPlayer3.Tx.Taiko_Ka_Right.szTextureSize.Height));
			}

			int[] nLVUPY = new int[] { 127, 127, 0, 0 };

			for ( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
			{
				if( !this.ctレベルアップダウン[ i ].b停止中 )
				{
					this.ctレベルアップダウン[ i ].t進行();
					if( this.ctレベルアップダウン[ i ].b終了値に達した ) {
						this.ctレベルアップダウン[ i ].t停止();
					}
				}

				if( ( this.ctレベルアップダウン[ i ].b進行中 && ( TJAPlayer3.Tx.Taiko_LevelUp != null && TJAPlayer3.Tx.Taiko_LevelDown != null ) ) && !TJAPlayer3.ConfigIni.bNoInfo )
				{
					//this.ctレベルアップダウン[ i ].n現在の値 = 110;

					float fScale = 1.0f;
					int nAlpha = 255;
					float[] fY = new float[] { -206, 206, 0, 0 };
					if( this.ctレベルアップダウン[ i ].n現在の値 >= 0 && this.ctレベルアップダウン[ i ].n現在の値 <= 20 )
					{
						nAlpha = 60;
						fScale = 1.14f;
					}
					else if( this.ctレベルアップダウン[ i ].n現在の値 >= 21 && this.ctレベルアップダウン[ i ].n現在の値 <= 40 )
					{
						nAlpha = 60;
						fScale = 1.19f;
					}
					else if( this.ctレベルアップダウン[ i ].n現在の値 >= 41 && this.ctレベルアップダウン[ i ].n現在の値 <= 60 )
					{
						nAlpha = 220;
						fScale = 1.23f;
					}
					else if( this.ctレベルアップダウン[ i ].n現在の値 >= 61 && this.ctレベルアップダウン[ i ].n現在の値 <= 80 )
					{
						nAlpha = 230;
						fScale = 1.19f;
					}
					else if( this.ctレベルアップダウン[ i ].n現在の値 >= 81 && this.ctレベルアップダウン[ i ].n現在の値 <= 100 )
					{
						nAlpha = 240;
						fScale = 1.14f;
					}
					else if( this.ctレベルアップダウン[ i ].n現在の値 >= 101 && this.ctレベルアップダウン[ i ].n現在の値 <= 120 )
					{
						nAlpha = 255;
						fScale = 1.04f;
					}
					else
					{
						nAlpha = 255;
						fScale = 1.0f;
					}

					if( this.After[ i ] - this.Before[ i ] >= 0 )
					{
						//レベルアップ
						TJAPlayer3.Tx.Taiko_LevelUp.Opacity = nAlpha;
						TJAPlayer3.Tx.Taiko_LevelUp.vcScaling = new Vector3( fScale, fScale, 1.0f );
						TJAPlayer3.Tx.Taiko_LevelUp.t2D拡大率考慮描画( TJAPlayer3.app.Device, CTexture.RefPnt.Center, -329 + GameWindowSize.Width / 2, fY[ i ] + GameWindowSize.Height / 2);
					}
					else
					{
						TJAPlayer3.Tx.Taiko_LevelDown.Opacity = nAlpha;
						TJAPlayer3.Tx.Taiko_LevelDown.vcScaling = new Vector3(fScale, fScale, 1.0f);
						TJAPlayer3.Tx.Taiko_LevelDown.t2D拡大率考慮描画( TJAPlayer3.app.Device, CTexture.RefPnt.Center, -329 + GameWindowSize.Width / 2, fY[ i ] + GameWindowSize.Height / 2 );
					}
				}
			}

			for( int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++ )
			{
				if (TJAPlayer3.Tx.Couse_Symbol[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]] != null)
				{
					TJAPlayer3.Tx.Couse_Symbol[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]].t2D描画(TJAPlayer3.app.Device,
						TJAPlayer3.Skin.Game_CourseSymbol_X[nPlayer],
						TJAPlayer3.Skin.Game_CourseSymbol_Y[nPlayer]
						);
				}

				if (TJAPlayer3.ConfigIni.ShinuchiMode[nPlayer])
				{
					if (TJAPlayer3.Tx.Couse_Symbol[(int)Difficulty.Total] != null)
					{
						TJAPlayer3.Tx.Couse_Symbol[(int)Difficulty.Total].t2D描画(TJAPlayer3.app.Device,
							TJAPlayer3.Skin.Game_CourseSymbol_X[nPlayer],
							TJAPlayer3.Skin.Game_CourseSymbol_Y[nPlayer]
							);
					}

				}

				if (TJAPlayer3.Tx.Taiko_NamePlate[nPlayer] != null)
				{
					TJAPlayer3.Tx.Taiko_NamePlate[nPlayer].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_NamePlate_X[nPlayer], TJAPlayer3.Skin.Game_Taiko_NamePlate_Y[nPlayer]);
				}

				if (TJAPlayer3.Tx.Taiko_PlayerNumber[nPlayer] != null)
				{
					TJAPlayer3.Tx.Taiko_PlayerNumber[nPlayer].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Taiko_PlayerNumber_X[nPlayer], TJAPlayer3.Skin.Game_Taiko_PlayerNumber_Y[nPlayer]);
				}
			}

			return base.On進行描画();
		}

		public void tMtaikoEvent( int nChannel, int nHand, int nPlayer )
		{
			if( !TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[nPlayer] )
			{
				switch( nChannel )
				{
					case 0x11:
					case 0x13:
					case 0x15:
					case 0x16:
					case 0x17:
						{
							this.stパッド状態[ 2 + nHand + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;
					case 0x12:
					case 0x14:
						{
							this.stパッド状態[ nHand + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;

				}
			}
			else
			{
				switch( nChannel )
				{
					case 0x11:
					case 0x15:
					case 0x16:
					case 0x17:
						{
							this.stパッド状態[ 2 + nHand + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;
							
					case 0x13:
						{
							this.stパッド状態[ 2 + ( 4 * nPlayer ) ].n明るさ = 8;
							this.stパッド状態[ 3 + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;

					case 0x12:
						{
							this.stパッド状態[ nHand + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;

					case 0x14:
						{
							this.stパッド状態[ 0 + ( 4 * nPlayer ) ].n明るさ = 8;
							this.stパッド状態[ 1 + ( 4 * nPlayer ) ].n明るさ = 8;
						}
						break;
				}
			}

		}

		public void tBranchEvent( int Before, int After, int player )
		{
			if( After != Before )
				this.ctレベルアップダウン[ player ] = new CCounter( 0, 1000, 1, TJAPlayer3.Timer );

			this.After[ player ] = After;
			this.Before[ player ] = Before;
		}


		#region[ private ]
		//-----------------
		//構造体
		[StructLayout(LayoutKind.Sequential)]
		private struct STパッド状態
		{
			public int n明るさ;
		}

		//太鼓
		private STパッド状態[] stパッド状態 = new STパッド状態[ 4 * 4 ];
		private long nフラッシュ制御タイマ;

		//譜面分岐
		private CCounter[] ctレベルアップダウン;
		public int[] After;
		public int[] Before;
		//-----------------
		#endregion

	}
}
　
