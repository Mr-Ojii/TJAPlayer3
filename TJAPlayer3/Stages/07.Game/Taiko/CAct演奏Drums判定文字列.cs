using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace TJAPlayer3
{
	internal class CAct演奏Drums判定文字列 : CAct演奏判定文字列共通
	{
		// コンストラクタ

		public CAct演奏Drums判定文字列()
		{
			this.stレーンサイズ = new STレーンサイズ[ 12 ];
			base.b活性化してない = true;
		}
		
		
		// CActivity 実装（共通クラスからの差分のみ）
		public override int On進行描画()
		{
			throw new InvalidOperationException( "t進行描画() のほうを使用してください。" );
		}
		public override int t進行描画()
		{
			if( !base.b活性化してない )
			{
				for( int i = 0; i < 12; i++ )
				{
					if( !base.st状態[ i ].ct進行.b停止中 )
					{
						base.st状態[ i ].ct進行.t進行();
						if( base.st状態[ i ].ct進行.b終了値に達した )
						{
							base.st状態[ i ].ct進行.t停止();
							base.st状態[ i ].b使用中 = false;
						}
						int num2 = base.st状態[ i ].ct進行.n現在の値;
						if( base.st状態[ i ].judge != E判定.Great )
						{
							base.st状態[ i ].n相対X座標 = 0;
							base.st状態[ i ].n相対Y座標 = 15;
							base.st状態[ i ].n透明度 = 0xff;
						}
						if( ( base.st状態[ i ].judge != E判定.Miss ) && ( base.st状態[ i ].judge != E判定.Bad ) )
						{
							if( num2 < 20 )
							{
								base.st状態[ i ].n相対X座標 = 0;
								base.st状態[ i ].n相対Y座標 = 0;
								base.st状態[ i ].n透明度 = 0xff;
							}
							else if( num2 < 40 )
							{
								base.st状態[ i ].n相対X座標 = 0;
								base.st状態[ i ].n相対Y座標 = 5;
								base.st状態[ i ].n透明度 = 0xff;
							}
							else if( num2 >= 60 )
							{
								base.st状態[ i ].n相対X座標 = 0;
								base.st状態[ i ].n相対Y座標 = 10;
								base.st状態[ i ].n透明度 = 0xff;
							}
							else
							{
								base.st状態[ i ].n相対X座標 = 0;
								base.st状態[ i ].n相対Y座標 = 15;
								base.st状態[ i ].n透明度 = 0xff;
							}
						}
						if( num2 < 20 )
						{
							base.st状態[ i ].n相対X座標 = 0;
							base.st状態[ i ].n相対Y座標 = 0;
							base.st状態[ i ].n透明度 = 0xff;
						}
						else if( num2 < 40 )
						{
							base.st状態[ i ].n相対X座標 = 0;
							base.st状態[ i ].n相対Y座標 = 5;
							base.st状態[ i ].n透明度 = 0xff;
						}
						else if( num2 >= 60 )
						{
							base.st状態[ i ].n相対X座標 = 0;
							base.st状態[ i ].n相対Y座標 = 10;
							base.st状態[ i ].n透明度 = 0xff;
						}
						else
						{
							base.st状態[ i ].n相対X座標 = 0;
							base.st状態[ i ].n相対Y座標 = 15;
							base.st状態[ i ].n透明度 = 0xff;
						}
					}
				}
				for( int j = 0; j < 12; j++ )
				{
					if (!base.st状態[j].ct進行.b停止中 && TJAPlayer3.Tx.Judge != null)
					{
						int baseX = 370;
						//int baseY = 135;
						int baseY = TJAPlayer3.Skin.nScrollFieldY[base.st状態[j].nPlayer] - 53;
						int x = TJAPlayer3.Skin.nScrollFieldX[base.st状態[j].nPlayer] - TJAPlayer3.Tx.Judge.szテクスチャサイズ.Width / 2;
						int y = ( baseY + base.st状態[ j ].n相対Y座標 );
						TJAPlayer3.Tx.Judge.t2D描画( TJAPlayer3.app.Device, x, y, base.st判定文字列[ (int) base.st状態[ j ].judge ].rc );
					}
				}
			}
			return 0;
		}
		

		// その他

		#region [ private ]
		//-----------------
		[StructLayout( LayoutKind.Sequential )]
		private struct STレーンサイズ
		{
			public int x;
			public int w;
		}

		private readonly int[] n文字の縦表示位置 = new int[] { 1, 2, 0, 1, 3, 2, 1, 0, 0, 0, 1, 1 };
		private STレーンサイズ[] stレーンサイズ;
		//-----------------
		#endregion
	}
}
