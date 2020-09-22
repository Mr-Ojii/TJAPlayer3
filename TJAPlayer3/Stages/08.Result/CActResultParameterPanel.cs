using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using FDK;

namespace TJAPlayer3
{
	internal class CActResultParameterPanel : CActivity
	{
		// コンストラクタ

		public CActResultParameterPanel()
		{
			ST文字位置[] st文字位置Array = new ST文字位置[ 11 ];
			ST文字位置 st文字位置 = new ST文字位置();
			st文字位置.ch = '0';
			st文字位置.pt = new Point( 0, 0 );
			st文字位置Array[ 0 ] = st文字位置;
			ST文字位置 st文字位置2 = new ST文字位置();
			st文字位置2.ch = '1';
			st文字位置2.pt = new Point( 32, 0 );
			st文字位置Array[ 1 ] = st文字位置2;
			ST文字位置 st文字位置3 = new ST文字位置();
			st文字位置3.ch = '2';
			st文字位置3.pt = new Point( 64, 0 );
			st文字位置Array[ 2 ] = st文字位置3;
			ST文字位置 st文字位置4 = new ST文字位置();
			st文字位置4.ch = '3';
			st文字位置4.pt = new Point( 96, 0 );
			st文字位置Array[ 3 ] = st文字位置4;
			ST文字位置 st文字位置5 = new ST文字位置();
			st文字位置5.ch = '4';
			st文字位置5.pt = new Point( 128, 0 );
			st文字位置Array[ 4 ] = st文字位置5;
			ST文字位置 st文字位置6 = new ST文字位置();
			st文字位置6.ch = '5';
			st文字位置6.pt = new Point( 160, 0 );
			st文字位置Array[ 5 ] = st文字位置6;
			ST文字位置 st文字位置7 = new ST文字位置();
			st文字位置7.ch = '6';
			st文字位置7.pt = new Point( 192, 0 );
			st文字位置Array[ 6 ] = st文字位置7;
			ST文字位置 st文字位置8 = new ST文字位置();
			st文字位置8.ch = '7';
			st文字位置8.pt = new Point( 224, 0 );
			st文字位置Array[ 7 ] = st文字位置8;
			ST文字位置 st文字位置9 = new ST文字位置();
			st文字位置9.ch = '8';
			st文字位置9.pt = new Point( 256, 0 );
			st文字位置Array[ 8 ] = st文字位置9;
			ST文字位置 st文字位置10 = new ST文字位置();
			st文字位置10.ch = '9';
			st文字位置10.pt = new Point( 288, 0 );
			st文字位置Array[ 9 ] = st文字位置10;
			ST文字位置 st文字位置11 = new ST文字位置();
			st文字位置11.ch = ' ';
			st文字位置11.pt = new Point( 0, 0 );
			st文字位置Array[ 10 ] = st文字位置11;
			this.st小文字位置 = st文字位置Array;

			ST文字位置[] stScore文字位置Array = new ST文字位置[10];
			ST文字位置 stScore文字位置 = new ST文字位置();
			stScore文字位置.ch = '0';
			stScore文字位置.pt = new Point(0, 0);
			stScore文字位置Array[0] = stScore文字位置;
			ST文字位置 stScore文字位置2 = new ST文字位置();
			stScore文字位置2.ch = '1';
			stScore文字位置2.pt = new Point(24, 0);
			stScore文字位置Array[1] = stScore文字位置2;
			ST文字位置 stScore文字位置3 = new ST文字位置();
			stScore文字位置3.ch = '2';
			stScore文字位置3.pt = new Point(48, 0);
			stScore文字位置Array[2] = stScore文字位置3;
			ST文字位置 stScore文字位置4 = new ST文字位置();
			stScore文字位置4.ch = '3';
			stScore文字位置4.pt = new Point(72, 0);
			stScore文字位置Array[3] = stScore文字位置4;
			ST文字位置 stScore文字位置5 = new ST文字位置();
			stScore文字位置5.ch = '4';
			stScore文字位置5.pt = new Point(96, 0);
			stScore文字位置Array[4] = stScore文字位置5;
			ST文字位置 stScore文字位置6 = new ST文字位置();
			stScore文字位置6.ch = '5';
			stScore文字位置6.pt = new Point(120, 0);
			stScore文字位置Array[5] = stScore文字位置6;
			ST文字位置 stScore文字位置7 = new ST文字位置();
			stScore文字位置7.ch = '6';
			stScore文字位置7.pt = new Point(144, 0);
			stScore文字位置Array[6] = stScore文字位置7;
			ST文字位置 stScore文字位置8 = new ST文字位置();
			stScore文字位置8.ch = '7';
			stScore文字位置8.pt = new Point(168, 0);
			stScore文字位置Array[7] = stScore文字位置8;
			ST文字位置 stScore文字位置9 = new ST文字位置();
			stScore文字位置9.ch = '8';
			stScore文字位置9.pt = new Point(192, 0);
			stScore文字位置Array[8] = stScore文字位置9;
			ST文字位置 stScore文字位置10 = new ST文字位置();
			stScore文字位置10.ch = '9';
			stScore文字位置10.pt = new Point(216, 0);
			stScore文字位置Array[9] = stScore文字位置10;
			this.stScoreFont = stScore文字位置Array;

			base.b活性化してない = true;
		}


		// メソッド

		public void tアニメを完了させる()
		{
			this.ct表示用.n現在の値 = this.ct表示用.n終了値;
		}


		// CActivity 実装

		public override void On活性化()
		{
			this.sdDTXで指定されたフルコンボ音 = null;
			base.On活性化();
		}
		public override void On非活性化()
		{
			if( this.ct表示用 != null )
			{
				this.ct表示用 = null;
			}
			if( this.sdDTXで指定されたフルコンボ音 != null )
			{
				TJAPlayer3.Sound管理.tサウンドを破棄する( this.sdDTXで指定されたフルコンボ音 );
				this.sdDTXで指定されたフルコンボ音 = null;
			}
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				Dan_Plate = TJAPlayer3.tテクスチャの生成(Path.GetDirectoryName(TJAPlayer3.DTX[0].strファイル名の絶対パス) + @"\Dan_Plate.png");
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする(ref Dan_Plate);
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( base.b活性化してない )
			{
				return 0;
			}
			if( base.b初めての進行描画 )
			{
				this.ct表示用 = new CCounter( 0, 0x3e7, 2, TJAPlayer3.Timer );
				base.b初めての進行描画 = false;
			}
			this.ct表示用.t進行();
			for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
			{
				if (TJAPlayer3.Tx.Result_Panel != null)
				{
					TJAPlayer3.Tx.Result_Panel.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultPanelX[i], TJAPlayer3.Skin.nResultPanelY[i]);
				}
				if (TJAPlayer3.Tx.Result_Score_Text != null)
				{
					int[] s_y = { 249, 609 };
					TJAPlayer3.Tx.Result_Score_Text.t2D描画(TJAPlayer3.app.Device, 753, s_y[i]); //点
				}
				if (TJAPlayer3.Tx.Result_Judge != null)
				{
					TJAPlayer3.Tx.Result_Judge.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultJudge_X[i], TJAPlayer3.Skin.nResultJudge_Y[i]);
				}
				if (TJAPlayer3.Tx.Result_Gauge_Base != null && TJAPlayer3.Tx.Result_Gauge != null)
				{
					double Rate = TJAPlayer3.stage結果.st演奏記録[i].fゲージ;
					TJAPlayer3.Tx.Result_Gauge_Base.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultGaugeBaseX[i], TJAPlayer3.Skin.nResultGaugeBaseY[i], new Rectangle(0, 0, 691, 47));
					#region[ ゲージ本体 ]
					int[] y_tmp= { 145, 505 };
					int y = y_tmp[i];
					if (Rate > 2)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 4)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 12, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 6)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 24, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 8)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 36, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 10)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 49, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 12)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 62, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 14)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 74, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 16)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 86, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 18)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 99, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 20)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 112, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 22)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 125, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 24)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 138, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 26)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 150, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 28)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 162, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 30)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 175, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 32)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 187, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 34)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 200, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 36)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 212, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 38)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 225, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 40)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 238, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 42)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 251, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 44)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 263, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 46)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 276, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 48)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 288, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 50)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 301, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 52)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 313, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 54)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 326, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 56)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 339, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 58)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 352, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 60)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 364, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 62)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 377, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 64)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 390, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 66)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 402, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 68)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 415, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 70)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 427, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 72)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 440, y, new Rectangle(0, 20, 12, 20));
					if (Rate > 74)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 452, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 76)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 465, y, new Rectangle(12, 20, 13, 20));
					if (Rate > 78)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 478, y, new Rectangle(12, 20, 13, 20));

					if (Rate > 80)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 491, y - 20, new Rectangle(25, 0, 12, 40));
					if (Rate > 82)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 503, y - 20, new Rectangle(49, 0, 13, 40));
					if (Rate > 84)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 516, y - 20, new Rectangle(37, 0, 12, 40));
					if (Rate > 86)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 528, y - 20, new Rectangle(49, 0, 13, 40));
					if (Rate > 88)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 541, y - 20, new Rectangle(37, 0, 12, 40));
					if (Rate > 90)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 553, y - 20, new Rectangle(49, 0, 13, 40));
					if (Rate > 92)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 566, y - 20, new Rectangle(49, 0, 13, 40));
					if (Rate > 94)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 579, y - 20, new Rectangle(37, 0, 12, 40));
					if (Rate > 96)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 591, y - 20, new Rectangle(49, 0, 13, 40));
					if (Rate > 98)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 604, y - 20, new Rectangle(37, 0, 12, 40));
					if (Rate >= 100)
						TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 616, y - 20, new Rectangle(49, 0, 10, 40));

					#endregion
				}
				if (TJAPlayer3.Tx.Gauge_Soul != null)
				{
					int[] y_Fire = { 34, 394 };
					int[] y_Soul = { 107, 467 };
					if (TJAPlayer3.Tx.Gauge_Soul_Fire != null && TJAPlayer3.stage結果.st演奏記録[i].fゲージ >= 100.0f)
						TJAPlayer3.Tx.Gauge_Soul_Fire.t2D描画(TJAPlayer3.app.Device, 1100, y_Fire[i], new Rectangle(0, 0, 230, 230));
					TJAPlayer3.Tx.Gauge_Soul.t2D描画(TJAPlayer3.app.Device, 1174, y_Soul[i], new Rectangle(0, 0, 80, 80));
				}
				//演奏中のやつ使いまわせなかった。ファック。
				this.tスコア文字表示(TJAPlayer3.Skin.nResultScoreX[i], TJAPlayer3.Skin.nResultScoreY[i], string.Format("{0,7:######0}", TJAPlayer3.stage結果.st演奏記録[i].nスコア));
				this.t小文字表示(TJAPlayer3.Skin.nResultGreatX[i], TJAPlayer3.Skin.nResultGreatY[i], string.Format("{0,4:###0}", TJAPlayer3.stage結果.st演奏記録[i].nPerfect数.ToString()));
				this.t小文字表示(TJAPlayer3.Skin.nResultGoodX[i], TJAPlayer3.Skin.nResultGoodY[i], string.Format("{0,4:###0}", TJAPlayer3.stage結果.st演奏記録[i].nGreat数.ToString()));
				this.t小文字表示(TJAPlayer3.Skin.nResultBadX[i], TJAPlayer3.Skin.nResultBadY[i], string.Format("{0,4:###0}", TJAPlayer3.stage結果.st演奏記録[i].nMiss数.ToString()));

				this.t小文字表示(TJAPlayer3.Skin.nResultComboX[i], TJAPlayer3.Skin.nResultComboY[i], string.Format("{0,4:###0}", TJAPlayer3.stage結果.st演奏記録[i].n最大コンボ数.ToString()));
				this.t小文字表示(TJAPlayer3.Skin.nResultRollX[i], TJAPlayer3.Skin.nResultRollY[i], string.Format("{0,4:###0}", TJAPlayer3.stage結果.st演奏記録[i].n連打数.ToString()));

				#region 段位認定モード用+王冠
				if (TJAPlayer3.stage選曲.n確定された曲の難易度[i] == (int)Difficulty.Dan)
				{
					TJAPlayer3.stage演奏ドラム画面.actDan.DrawExam(TJAPlayer3.stage結果.st演奏記録[i].Dan_C);
					switch (TJAPlayer3.stage演奏ドラム画面.actDan.GetExamStatus(TJAPlayer3.stage結果.st演奏記録[i].Dan_C))
					{
						case Exam.Status.Failure:
							TJAPlayer3.Tx.Result_Dan?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_XY[0], TJAPlayer3.Skin.Result_Dan_XY[1], new Rectangle(0, 0, TJAPlayer3.Skin.Result_Dan[0], TJAPlayer3.Skin.Result_Dan[1]));
							break;
						case Exam.Status.Success:
							TJAPlayer3.Tx.Result_Dan?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_XY[0], TJAPlayer3.Skin.Result_Dan_XY[1], new Rectangle(TJAPlayer3.Skin.Result_Dan[0], 0, TJAPlayer3.Skin.Result_Dan[0], TJAPlayer3.Skin.Result_Dan[1]));
							break;
						case Exam.Status.Better_Success:
							TJAPlayer3.Tx.Result_Dan?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_XY[0], TJAPlayer3.Skin.Result_Dan_XY[1], new Rectangle(TJAPlayer3.Skin.Result_Dan[0] * 2, 0, TJAPlayer3.Skin.Result_Dan[0], TJAPlayer3.Skin.Result_Dan[1]));
							break;
						default:
							break;
					}
					// Dan_Plate
					Dan_Plate?.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_Plate_XY[0], TJAPlayer3.Skin.Result_Dan_Plate_XY[1]);
				}
				else
				{
					int j;
					if (TJAPlayer3.stage結果.st演奏記録[i].fゲージ < 80)
					{
						j = 0;
					}
					else if (TJAPlayer3.stage結果.st演奏記録[i].nMiss数_Auto含まない != 0)
					{
						j = 1;
					}
					else if (TJAPlayer3.stage結果.st演奏記録[i].nGreat数_Auto含まない != 0)
					{
						j = 2;
					}
					else
					{
						j = 3;
					}
					if (j != 0 && TJAPlayer3.Tx.Crown_t != null)
					{
						TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.X = 1.0f;
						TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.Y = 1.0f;
						TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Crown_X[i], TJAPlayer3.Skin.Result_Crown_Y[i], new Rectangle(j * 100, 0, 100, 100));
					}
				}
				#endregion
			}

			if ( !this.ct表示用.b終了値に達した )
			{
				return 0;
			}
			return 1;
		}
		

		// その他

		#region [ private ]
		//-----------------
		[StructLayout( LayoutKind.Sequential )]
		private struct ST文字位置
		{
			public char ch;
			public Point pt;
		}

		private CCounter ct表示用;
		private CSound sdDTXで指定されたフルコンボ音;
		private readonly ST文字位置[] st小文字位置;
		private ST文字位置[] stScoreFont;

		private CTexture Dan_Plate;

		private void t小文字表示( int x, int y, string str )
		{
			foreach( char ch in str )
			{
				for( int i = 0; i < this.st小文字位置.Length; i++ )
				{
					if( ch == ' ' )
					{
						break;
					}

					if( this.st小文字位置[ i ].ch == ch )
					{
						Rectangle rectangle = new Rectangle( this.st小文字位置[ i ].pt.X, this.st小文字位置[ i ].pt.Y, 32, 38 );
						if(TJAPlayer3.Tx.Result_Number != null )
						{
							TJAPlayer3.Tx.Result_Number.t2D描画( TJAPlayer3.app.Device, x, y, rectangle );
						}
						break;
					}
				}
				x += 22;
			}
		}

		private void tスコア文字表示(int x, int y, string str)
		{
			foreach (char ch in str)
			{
				for (int i = 0; i < this.stScoreFont.Length; i++)
				{
					if (this.stScoreFont[i].ch == ch)
					{
						Rectangle rectangle = new Rectangle(this.stScoreFont[ i ].pt.X, 0, 24, 40);
						if (TJAPlayer3.Tx.Result_Score_Number != null)
						{
							TJAPlayer3.Tx.Result_Score_Number.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
						}
						break;
					}
				}
				x += 24;
			}
		}
		//-----------------
		#endregion
	}
}
