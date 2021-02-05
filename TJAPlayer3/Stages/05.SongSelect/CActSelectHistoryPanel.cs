using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics;
using FDK;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CActSelectHistoryPanel : CActivity
	{
		// メソッド

		public CActSelectHistoryPanel()
		{
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
				this.Font = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 40);
				base.OnManagedリソースの作成();
				tSongChange();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする(ref this.First);
				TJAPlayer3.t安全にDisposeする(ref this.Second);
				TJAPlayer3.t安全にDisposeする(ref this.Third);
				if (Font != null)
                {
                    Font.Dispose();
					Font = null;
                }
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
				int[] x = TJAPlayer3.Skin.SongSelect_ScoreWindow_X;
				int[] y = TJAPlayer3.Skin.SongSelect_ScoreWindow_Y;
				int xdiff = 170;
				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++) {
					if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア != null && this.ct登場アニメ用.b終了値に達した && TJAPlayer3.stage選曲.act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.SCORE)
					{
						//CDTXMania.Tx.SongSelect_ScoreWindow_Text.n透明度 = ct登場アニメ用.n現在の値 - 1745;
						if (TJAPlayer3.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]] != null)
						{
							//CDTXMania.Tx.SongSelect_ScoreWindow[CDTXMania.stage選曲.n現在選択中の曲の難易度].n透明度 = ct登場アニメ用.n現在の値 - 1745;
							TJAPlayer3.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]].t2D描画(TJAPlayer3.app.Device, x[i], y[i]);
							this.First[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.UpRight, x[i] + xdiff + 50, y[i] + 90 - 25);
							this.t小文字表示(x[i] + xdiff, y[i] + 90, TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nハイスコア[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]);
							TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x[i] + xdiff + 15, y[i] + 95, new Rectangle(0, 36, 32, 30));

							this.Second[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.UpRight, x[i] + xdiff + 50, y[i] + 90 + 45);
							this.t小文字表示(x[i] + xdiff, y[i] + 90 + 70, TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nSecondScore[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]);
							TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x[i] + xdiff + 15, y[i] + 95 + 70, new Rectangle(0, 36, 32, 30));

							this.Third[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.UpRight, x[i] + xdiff + 50, y[i] + 90 + 115);
							this.t小文字表示(x[i] + xdiff, y[i] + 90 + 140, TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nThirdScore[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]);
							TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x[i] + xdiff + 15, y[i] + 95 + 140, new Rectangle(0, 36, 32, 30));
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
		private CTexture[] First = new CTexture[(int)Difficulty.Total];
		private CTexture[] Second = new CTexture[(int)Difficulty.Total];
		private CTexture[] Third = new CTexture[(int)Difficulty.Total];
		private CPrivateFastFont Font;
		//-----------------

		private void t小文字表示(int x, int y, long n)
		{
			for (int index = 0; index < n.ToString().Length; index++) {
				int Num = (int)(n / Math.Pow(10, index) % 10);
				Rectangle rectangle = new Rectangle((TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10) * Num, 0, TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10, TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Height / 2);
				if (TJAPlayer3.Tx.SongSelect_ScoreWindow_Text != null)
				{
					TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
				}
				x -= TJAPlayer3.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10;
			}
		}

		public void tSongChange()
		{
			this.ct登場アニメ用 = new CCounter( 0, 2000, 1, TJAPlayer3.Timer );

			//Dispose
			TJAPlayer3.t安全にDisposeする(ref this.First);
			TJAPlayer3.t安全にDisposeする(ref this.Second);
			TJAPlayer3.t安全にDisposeする(ref this.Third);

			if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア != null)
			{
				string[] First = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.strHiScorerName;
				string[] Second = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.strSecondScorerName;
				string[] Third = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.strThirdScorerName;

				if (Font != null)
					for (int index = 0; index < (int)Difficulty.Total; index++)
					{
						if (!string.IsNullOrEmpty(First[index]))
						{
							this.First[index] = TJAPlayer3.tCreateTexture(Font.DrawPrivateFont(First[index], Color.Black));
							this.First[index].vcScaling = new Vector3(0.5f);
						}
						if (!string.IsNullOrEmpty(Second[index]))
						{
							this.Second[index] = TJAPlayer3.tCreateTexture(Font.DrawPrivateFont(Second[index], Color.Black));
							this.Second[index].vcScaling = new Vector3(0.5f);
						}
						if (!string.IsNullOrEmpty(Third[index]))
						{
							this.Third[index] = TJAPlayer3.tCreateTexture(Font.DrawPrivateFont(Third[index], Color.Black));
							this.Third[index].vcScaling = new Vector3(0.5f);
						}
					}
			}
		}


		#endregion
	}
}
