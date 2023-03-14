using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏Drumsコンボ吹き出し : CActivity
	{
		// コンストラクタ

		/// <summary>
		/// 100コンボごとに出る吹き出し。
		/// 本当は「10000点」のところも動かしたいけど、技術不足だし保留。
		/// </summary>
		public CAct演奏Drumsコンボ吹き出し()
		{
			base.b活性化してない = true;
		}
		
		
		// メソッド
		public virtual void Start( int nCombo, int player )
		{
			this.ct進行[ player ] = new CCounter( 1, 103, 20, TJAPlayer3.Timer );
			this.nCombo_渡[ player ] = nCombo;
		}

		// CActivity 実装

		public override void On活性化()
		{
			for( int i = 0; i < 2; i++ )
			{
				this.nCombo_渡[ i ] = 0;
				this.ct進行[ i ] = new CCounter();
			}

			base.On活性化();
		}
		public override void On非活性化()
		{
			for( int i = 0; i < 2; i++ )
			{
				this.ct進行[ i ] = null;
			}
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
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				for( int i = 0; i < 2; i++ )
				{
					if( !this.ct進行[ i ].b停止中 )
					{
						this.ct進行[ i ].t進行();
						if( this.ct進行[ i ].b終了値に達した )
						{
							this.ct進行[ i ].t停止();
						}
					}

					if( TJAPlayer3.Tx.Balloon_Combo[ i ] != null )
					{
						//半透明4f
						if( this.ct進行[ i ].n現在の値 == 1 || this.ct進行[ i ].n現在の値 == 103 )
						{
							TJAPlayer3.Tx.Balloon_Combo[ i ].Opacity = 64;
							TJAPlayer3.Tx.Balloon_Number_Combo.Opacity = 64;
						}
						else if( this.ct進行[ i ].n現在の値 == 2 || this.ct進行[ i ].n現在の値 == 102 )
						{
							TJAPlayer3.Tx.Balloon_Combo[ i ].Opacity = 128;
							TJAPlayer3.Tx.Balloon_Number_Combo.Opacity = 128;
						}
						else if( this.ct進行[ i ].n現在の値 == 3 || this.ct進行[ i ].n現在の値 == 101 )
						{
							TJAPlayer3.Tx.Balloon_Combo[ i ].Opacity = 192;
							TJAPlayer3.Tx.Balloon_Number_Combo.Opacity = 192;
						}
						else if( this.ct進行[ i ].n現在の値 >= 4 && this.ct進行[ i ].n現在の値 <= 100 )
						{
							TJAPlayer3.Tx.Balloon_Combo[ i ].Opacity = 255;
							TJAPlayer3.Tx.Balloon_Number_Combo.Opacity = 255;
						}

						if( this.ct進行[ i ].b進行中 )
						{
							TJAPlayer3.Tx.Balloon_Combo[ i ].t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboX[ i ], TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboY[ i ] );
							if( this.nCombo_渡[ i ] < 1000 ) //2016.08.23 kairera0467 仮実装。
							{
								this.t小文字表示( TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboNumberX[ i ], TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboNumberY[ i ], string.Format( "{0,4:###0}", this.nCombo_渡[ i ] ) );
								TJAPlayer3.Tx.Balloon_Number_Combo.t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboTextX[ i ], TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboTextY[ i ], new Rectangle( 0, 54, 77, 32 ) );
							}
							else
							{
								this.t小文字表示( TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboNumberExX[ i ], TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboNumberExY[ i ], string.Format( "{0,4:###0}", this.nCombo_渡[ i ] ) );
								TJAPlayer3.Tx.Balloon_Number_Combo.t2D描画( TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboTextExX[ i ], TJAPlayer3.Skin.SkinConfig.Game.Balloon.ComboTextExY[ i ], new Rectangle( 0, 54, 77, 32 ) );
							}
						}
					}
				}
			}
			return 0;
		}
		

		// その他

		#region [ private ]
		//-----------------
		private CCounter[] ct進行 = new CCounter[ 2 ];
		//private CTexture[] tx吹き出し本体 = new CTexture[ 2 ];
		//private CTexture tx数字;
		private int[] nCombo_渡 = new int[ 2 ];

		[StructLayout(LayoutKind.Sequential)]
		private struct ST文字位置
		{
			public char ch;
			public Point pt;
			public ST文字位置( char ch, Point pt )
			{
				this.ch = ch;
				this.pt = pt;
			}
		}
		private ST文字位置[] st小文字位置 = new ST文字位置[]{
			new ST文字位置( '0', new Point( 0, 0 ) ),
			new ST文字位置( '1', new Point( 44, 0 ) ),
			new ST文字位置( '2', new Point( 88, 0 ) ),
			new ST文字位置( '3', new Point( 132, 0 ) ),
			new ST文字位置( '4', new Point( 176, 0 ) ),
			new ST文字位置( '5', new Point( 220, 0 ) ),
			new ST文字位置( '6', new Point( 264, 0 ) ),
			new ST文字位置( '7', new Point( 308, 0 ) ),
			new ST文字位置( '8', new Point( 352, 0 ) ),
			new ST文字位置( '9', new Point( 396, 0 ) )
		};

		private void t小文字表示( int x, int y, string str )
		{
			foreach( char ch in str )
			{
				for( int i = 0; i < this.st小文字位置.Length; i++ )
				{
					if( this.st小文字位置[ i ].ch == ch )
					{
						Rectangle rectangle = new Rectangle( this.st小文字位置[ i ].pt.X, this.st小文字位置[ i ].pt.Y, 44, 54 );
						if(TJAPlayer3.Tx.Balloon_Number_Combo != null )
						{
							TJAPlayer3.Tx.Balloon_Number_Combo.t2D描画( TJAPlayer3.app.Device, x, y, rectangle );
						}
						break;
					}
				}
				x += 40;
			}
		}
		//-----------------
		#endregion
	}
}
