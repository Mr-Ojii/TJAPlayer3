using System;
using System.Collections.Generic;
using System.Text;
using FDK;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TJAPlayer3
{
	internal class CAct演奏スコア共通 : CActivity
	{
		// プロパティ

		protected long[] nスコアの増分;
		protected double[] n現在の本当のスコア;
		protected long[] n現在表示中のスコア;
		//protected CTexture txScore;

  //      protected CTexture txScore_1P;
		protected CCounter ctTimer;
		public CCounter[] ct点数アニメタイマ;

		public CCounter[] ctボーナス加算タイマ;

		protected STスコア[] stScore;
		protected int n現在表示中のAddScore;

		[StructLayout( LayoutKind.Sequential )]
		protected struct STスコア
		{
			public bool b使用中;
			public bool b表示中;
			public bool bBonusScore;
			public CCounter ctTimer;
			public int nAddScore;
			public int nPlayer;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct ST文字位置
		{
			public char ch;
			public Point pt;
		}
		private ST文字位置[] stFont;


		public long GetScore(int player)
		{
			return n現在表示中のスコア[player];
		}

		// コンストラクタ

		public CAct演奏スコア共通()
		{
			ST文字位置[] st文字位置Array = new ST文字位置[11];
			ST文字位置 st文字位置 = new ST文字位置();
			st文字位置.ch = '0';
			st文字位置.pt = new Point(0, 0);
			st文字位置Array[0] = st文字位置;
			ST文字位置 st文字位置2 = new ST文字位置();
			st文字位置2.ch = '1';
			st文字位置2.pt = new Point(24, 0);
			st文字位置Array[1] = st文字位置2;
			ST文字位置 st文字位置3 = new ST文字位置();
			st文字位置3.ch = '2';
			st文字位置3.pt = new Point(48, 0);
			st文字位置Array[2] = st文字位置3;
			ST文字位置 st文字位置4 = new ST文字位置();
			st文字位置4.ch = '3';
			st文字位置4.pt = new Point(72, 0);
			st文字位置Array[3] = st文字位置4;
			ST文字位置 st文字位置5 = new ST文字位置();
			st文字位置5.ch = '4';
			st文字位置5.pt = new Point(96, 0);
			st文字位置Array[4] = st文字位置5;
			ST文字位置 st文字位置6 = new ST文字位置();
			st文字位置6.ch = '5';
			st文字位置6.pt = new Point(120, 0);
			st文字位置Array[5] = st文字位置6;
			ST文字位置 st文字位置7 = new ST文字位置();
			st文字位置7.ch = '6';
			st文字位置7.pt = new Point(144, 0);
			st文字位置Array[6] = st文字位置7;
			ST文字位置 st文字位置8 = new ST文字位置();
			st文字位置8.ch = '7';
			st文字位置8.pt = new Point(168, 0);
			st文字位置Array[7] = st文字位置8;
			ST文字位置 st文字位置9 = new ST文字位置();
			st文字位置9.ch = '8';
			st文字位置9.pt = new Point(192, 0);
			st文字位置Array[8] = st文字位置9;
			ST文字位置 st文字位置10 = new ST文字位置();
			st文字位置10.ch = '9';
			st文字位置10.pt = new Point(216, 0);
			st文字位置Array[9] = st文字位置10;
			this.stFont = st文字位置Array;

			this.stScore = new STスコア[ 256 ];
			base.b活性化してない = true;
		}


		// メソッド

		private float[] ScoreScale = new float[]
		{
			1.000f,
			1.111f, // リピート
			1.222f,
			1.185f,
			1.148f,
			1.129f,
			1.111f,
			1.074f,
			1.065f,
			1.033f,
			1.015f,
			1.000f
		};

		public double Get( int player )
		{
			return this.n現在の本当のスコア[ player ];
		}
		public void Set( double nScore, int player )
		{
			if( this.n現在の本当のスコア[ player ] != nScore )
			{
				this.n現在の本当のスコア[ player ] = nScore;
				this.nスコアの増分[ player ] = (long) ( ( (double) ( this.n現在の本当のスコア[ player ] - this.n現在表示中のスコア[ player ] ) ) / 20.0 );
				if( this.nスコアの増分[ player ] < 1L )
				{
					this.nスコアの増分[ player ] = 1L;
				}
			}			
		}

		/// <summary>
		/// 点数を加える(各種AUTO補正つき)
		/// </summary>
		/// <param name="part"></param>
		/// <param name="bAutoPlay"></param>
		/// <param name="delta"></param>
		public void Add( long delta, int player )
		{
			double rev = 1.0;

			this.ctTimer = new CCounter( 0, 400, 1, TJAPlayer3.Timer );

			for( int sc = 0; sc < 1; sc++ )
			{
				for( int i = 0; i < 256; i++ )
				{
					if( this.stScore[ i ].b使用中 == false )
					{
						this.stScore[ i ].b使用中 = true;
						this.stScore[ i ].b表示中 = true;
						this.stScore[ i ].nAddScore = (int)delta;
						this.stScore[ i ].ctTimer = new CCounter( 0, 500, 1, TJAPlayer3.Timer );
						this.stScore[ i ].bBonusScore = false;
						this.stScore[ i ].nPlayer = player;
						this.n現在表示中のAddScore++;
						break;
					}
				}
			}

			this.Set( this.Get( player ) + delta * rev, player );
		}

		public void BonusAdd( int player )
		{
			for( int sc = 0; sc < 1; sc++ )
			{
				for( int i = 0; i < 256; i++ )
				{
					if( this.stScore[ i ].b使用中 == false )
					{
						this.stScore[ i ].b使用中 = true;
						this.stScore[ i ].b表示中 = true;
						this.stScore[ i ].nAddScore = 10000;
						this.stScore[ i ].ctTimer = new CCounter( 0, 400, 1, TJAPlayer3.Timer );
						this.stScore[ i ].bBonusScore = true;
						this.stScore[ i ].nPlayer = player;
						this.n現在表示中のAddScore++;
						break;
					}
				}
			}

			this.Set( this.Get( player ) + 10000, player );
		}

		// CActivity 実装

		public override void On活性化()
		{
			this.n現在表示中のスコア = new long[ 4 ];
			this.n現在の本当のスコア = new double[ 4 ];
			this.nスコアの増分 = new long[ 4 ];

			for (int i = 0; i < 4; i++)
			{
				this.n現在表示中のスコア[i] = 0L;
				this.n現在の本当のスコア[i] = 0L;
				this.nスコアの増分[i] = 0L;
			}

			for( int sc = 0; sc < 256; sc++ )
			{
				this.stScore[ sc ].b使用中 = false;
				this.stScore[ sc ].ctTimer = new CCounter();
				this.stScore[ sc ].nAddScore = 0;
				this.stScore[ sc ].bBonusScore = false;
			}

			this.n現在表示中のAddScore = 0;

			this.ctTimer = new CCounter();

			this.ct点数アニメタイマ = new CCounter[4];
			for (int i = 0; i < 4; i++)
			{
				this.ct点数アニメタイマ[i] = new CCounter();
			}
			this.ctボーナス加算タイマ = new CCounter[4];
			for (int i = 0; i < 4; i++)
			{
				this.ctボーナス加算タイマ[i] = new CCounter();
			}
			base.On活性化();
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

		protected void t小文字表示( int x, int y, string str, int mode , int alpha, int player )
		{
			foreach( char ch in str )
			{
				for( int i = 0; i < this.stFont.Length; i++ )
				{
					if( this.stFont[ i ].ch == ch )
					{
						Rectangle rectangle = new Rectangle(TJAPlayer3.Skin.Game_Score_Size[0] * i, 0, TJAPlayer3.Skin.Game_Score_Size[0], TJAPlayer3.Skin.Game_Score_Size[1]);
						switch( mode )
						{
							case 0:
								if( TJAPlayer3.Tx.Taiko_Score[0] != null )
								{
									TJAPlayer3.Tx.Taiko_Score[0].Opacity = alpha;
									TJAPlayer3.Tx.Taiko_Score[0].vc拡大縮小倍率.Y = ScoreScale[this.ct点数アニメタイマ[player].n現在の値];
									TJAPlayer3.Tx.Taiko_Score[0].t2D拡大率考慮下基準描画( TJAPlayer3.app.Device, x , y, rectangle );
									
								}
								break;
							case 1:
								if(TJAPlayer3.Tx.Taiko_Score[1] != null )
								{
									TJAPlayer3.Tx.Taiko_Score[1].Opacity = alpha;
									TJAPlayer3.Tx.Taiko_Score[1].vc拡大縮小倍率.Y = 1;
									TJAPlayer3.Tx.Taiko_Score[1].t2D拡大率考慮下基準描画( TJAPlayer3.app.Device, x, y, rectangle );
								}
								break;
							case 2:
								if(TJAPlayer3.Tx.Taiko_Score[2] != null )
								{
									TJAPlayer3.Tx.Taiko_Score[2].Opacity = alpha;
									TJAPlayer3.Tx.Taiko_Score[2].vc拡大縮小倍率.Y = 1;
									TJAPlayer3.Tx.Taiko_Score[2].t2D拡大率考慮下基準描画( TJAPlayer3.app.Device, x, y, rectangle );
								}
								break;
						}
						break;
					}
				}
				x += TJAPlayer3.Skin.Game_Score_Padding;
			}
		}
	}
}
