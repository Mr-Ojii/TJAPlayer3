using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CActSelect演奏履歴パネル : CActivity
	{
		// メソッド

		public CActSelect演奏履歴パネル()
		{
			ST文字位置[] st文字位置Array = new ST文字位置[10];

			ST文字位置 st文字位置 = new ST文字位置();
			st文字位置.ch = '0';
			st文字位置.pt = new Point(0, 0);
			st文字位置Array[0] = st文字位置;
			ST文字位置 st文字位置2 = new ST文字位置();
			st文字位置2.ch = '1';
			st文字位置2.pt = new Point(26, 0);
			st文字位置Array[1] = st文字位置2;
			ST文字位置 st文字位置3 = new ST文字位置();
			st文字位置3.ch = '2';
			st文字位置3.pt = new Point(52, 0);
			st文字位置Array[2] = st文字位置3;
			ST文字位置 st文字位置4 = new ST文字位置();
			st文字位置4.ch = '3';
			st文字位置4.pt = new Point(78, 0);
			st文字位置Array[3] = st文字位置4;
			ST文字位置 st文字位置5 = new ST文字位置();
			st文字位置5.ch = '4';
			st文字位置5.pt = new Point(104, 0);
			st文字位置Array[4] = st文字位置5;
			ST文字位置 st文字位置6 = new ST文字位置();
			st文字位置6.ch = '5';
			st文字位置6.pt = new Point(130, 0);
			st文字位置Array[5] = st文字位置6;
			ST文字位置 st文字位置7 = new ST文字位置();
			st文字位置7.ch = '6';
			st文字位置7.pt = new Point(156, 0);
			st文字位置Array[6] = st文字位置7;
			ST文字位置 st文字位置8 = new ST文字位置();
			st文字位置8.ch = '7';
			st文字位置8.pt = new Point(182, 0);
			st文字位置Array[7] = st文字位置8;
			ST文字位置 st文字位置9 = new ST文字位置();
			st文字位置9.ch = '8';
			st文字位置9.pt = new Point(208, 0);
			st文字位置Array[8] = st文字位置9;
			ST文字位置 st文字位置10 = new ST文字位置();
			st文字位置10.ch = '9';
			st文字位置10.pt = new Point(234, 0);
			st文字位置Array[9] = st文字位置10;
			this.st小文字位置 = st文字位置Array;

			base.b活性化してない = true;
		}

		// CActivity 実装

		public override void On活性化()
		{
			base.On活性化();
		}
		public override void On非活性化()
		{
			this.ct登場アニメ用 = null;
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
				if( base.b初めての進行描画 )
				{
					this.ct登場アニメ用 = new CCounter( 0, 3000, 1, TJAPlayer3.Timer );
					base.b初めての進行描画 = false;
				}
				this.ct登場アニメ用.t進行();
				int[] x = new int[2] { 0, 980 };
				int[] y = new int[2] { 350, 350 };
				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++) {
					if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア != null && this.ct登場アニメ用.n現在の値 >= 2000 && TJAPlayer3.stage選曲.act曲リスト.r現在選択中の曲.eノード種別 == C曲リストノード.Eノード種別.SCORE)
					{
						//CDTXMania.Tx.SongSelect_ScoreWindow_Text.n透明度 = ct登場アニメ用.n現在の値 - 1745;
						if (TJAPlayer3.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]] != null)
						{
							//CDTXMania.Tx.SongSelect_ScoreWindow[CDTXMania.stage選曲.n現在選択中の曲の難易度].n透明度 = ct登場アニメ用.n現在の値 - 1745;
							TJAPlayer3.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]].t2D描画(TJAPlayer3.app.Device, x[i], y[i]);
							this.t小文字表示(x[i] + 56, y[i] + 160, string.Format("{0,7:######0}", TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nハイスコア[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]].ToString()));
							this.t小文字表示(x[i] + 56, y[i] + 160+50, string.Format("{0,7:######0}", TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nSecondScore[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]].ToString()));
							this.t小文字表示(x[i] + 56, y[i] + 160+100, string.Format("{0,7:######0}", TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nThirdScore[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]].ToString()));
							TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x[i] + 236, y[i] + 166, new Rectangle(0, 36, 32, 30));
						}
					} 
				}
			}
			return 0;
		}
		

		// その他

		#region [ private ]
		//-----------------
		private CCounter ct登場アニメ用;
		//-----------------

		[StructLayout(LayoutKind.Sequential)]
		private struct ST文字位置
		{
			public char ch;
			public Point pt;
		}
		private readonly ST文字位置[] st小文字位置;
		private void t小文字表示(int x, int y, string str)
		{
			foreach (char ch in str)
			{
				for (int i = 0; i < this.st小文字位置.Length; i++)
				{
					if (this.st小文字位置[i].ch == ch)
					{
						Rectangle rectangle = new Rectangle( this.st小文字位置[i].pt.X, this.st小文字位置[i].pt.Y, 26, 36 );
						if (TJAPlayer3.Tx.SongSelect_ScoreWindow_Text != null)
						{
							TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
						}
						break;
					}
				}
				x += 26;
			}
		}

		public void tSongChange()
		{
			this.ct登場アニメ用 = new CCounter( 0, 3000, 1, TJAPlayer3.Timer );
		}


		#endregion
	}
}
