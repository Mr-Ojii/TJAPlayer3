using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing.Text;
using FDK;
using OpenTK;
using OpenTK.Graphics;
using System.Reflection;

using Rectangle = System.Drawing.Rectangle;

namespace TJAPlayer3
{
	/// <summary>
	/// 難易度選択画面。
	/// </summary>
	internal class CActSelect難易度選択画面 : CActivity
	{
		// プロパティ

		// CActivity 実装

		public override void On活性化()
		{
			if( this.b活性化してる )
				return;
			try
			{
				this.ct分岐表示用タイマー = new CCounter(1, 2, 2500, TJAPlayer3.Timer);
				選択済み = new bool[2] { false, false };
				裏カウント = new int[2] { 0, 0 };
			}
			finally { 
			
			}

			base.On活性化();
		}
		public override void On非活性化()
		{
			if( this.b活性化してない )
				return;

			try
			{
				this.ct分岐表示用タイマー = null;
				this.b開いた直後 = true;
			}
			finally { 
			}

			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( this.b活性化してない )
				return;

			base.OnManagedリソースの作成();
		}
		public override void OnManagedリソースの解放()
		{
			if( this.b活性化してない )
				return;

			base.OnManagedリソースの解放();
		}
		public override int On進行描画()
		{
			if (this.b活性化してない)
				return 0;

			#region [ 初めての進行描画 ]
			//-----------------
			if (base.b初めての進行描画)
			{
				base.b初めての進行描画 = false;
			}

			if (b開いた直後) {

				for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
				{
					this.ct難易度拡大用[nPlayer].n現在の値 = 0;
					this.ct難易度拡大用[nPlayer].t時間Reset();
				}
				TJAPlayer3.Skin.sound難易度選択音?.t再生する();
				b開いた直後 = false;
			}
			//-----------------
			#endregion

			this.ct分岐表示用タイマー.t進行Loop();
			this.ct難易度拡大用[0].t進行();
			this.ct難易度拡大用[1].t進行();

			// 描画。


			#region[難易度マーク]

			for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
			{
				if (選択済み[i])
				{
					if (TJAPlayer3.Tx.Difficulty_Mark[確定された難易度[i]] != null)
					{
						TJAPlayer3.Tx.Difficulty_Mark[確定された難易度[i]].Opacity = 100;
						TJAPlayer3.Tx.Difficulty_Mark[確定された難易度[i]].vc拡大縮小倍率 = new Vector3(0.75f);
						TJAPlayer3.Tx.Difficulty_Mark[確定された難易度[i]].t2D拡大率考慮下基準描画(TJAPlayer3.app.Device, i * 1075 - 30, TJAPlayer3.Skin.Difficulty_Mark_Y);
					}
				}else if (現在の選択行[i] >= 3)
                {
					if (裏表示 && 現在の選択行[i] - 3 == 3)
					{
						if (TJAPlayer3.Tx.Difficulty_Mark[4] != null)
						{
							TJAPlayer3.Tx.Difficulty_Mark[4].Opacity = 100;
							TJAPlayer3.Tx.Difficulty_Mark[4].vc拡大縮小倍率 = new Vector3(0.75f);
							TJAPlayer3.Tx.Difficulty_Mark[4].vc拡大縮小倍率.Y = 0.75f * (float)(1 + Math.Sin(ct難易度拡大用[i].n現在の値 * Math.PI / 180) * 0.25);
							TJAPlayer3.Tx.Difficulty_Mark[4].t2D拡大率考慮下基準描画(TJAPlayer3.app.Device, i * 1075 - 30, TJAPlayer3.Skin.Difficulty_Mark_Y);
						}
					}
					else
					{
						if (TJAPlayer3.Tx.Difficulty_Mark[現在の選択行[i] - 3] != null)
						{
							TJAPlayer3.Tx.Difficulty_Mark[現在の選択行[i] - 3].Opacity = 100;
							TJAPlayer3.Tx.Difficulty_Mark[現在の選択行[i] - 3].vc拡大縮小倍率 = new Vector3(0.75f);
							TJAPlayer3.Tx.Difficulty_Mark[現在の選択行[i] - 3].vc拡大縮小倍率.Y = 0.75f * (float)(1 + Math.Sin(ct難易度拡大用[i].n現在の値 * Math.PI / 180) * 0.25);
							TJAPlayer3.Tx.Difficulty_Mark[現在の選択行[i] - 3].t2D拡大率考慮下基準描画(TJAPlayer3.app.Device, i * 1075 - 30, TJAPlayer3.Skin.Difficulty_Mark_Y);
						}
					}
				}
			}
			#endregion
			#region[難易度選択裏バー描画]
			if (TJAPlayer3.Tx.Difficulty_Center_Bar != null)
			{
				int width = TJAPlayer3.Skin.Difficulty_Bar_Center_X_WH_WH_Y_Y[3];
				int height = TJAPlayer3.Skin.Difficulty_Bar_Center_X_WH_WH_Y_Y[4];

				int xdiff = TJAPlayer3.Skin.Difficulty_Bar_Center_X_WH_WH_Y_Y[0] - TJAPlayer3.Skin.Difficulty_Bar_Center_X_WH_WH_Y_Y[3] / 2;
				int ydiff = TJAPlayer3.Skin.Difficulty_Bar_Center_X_WH_WH_Y_Y[6];


				int wh = Math.Min(TJAPlayer3.Tx.Difficulty_Center_Bar.szテクスチャサイズ.Width / 3, TJAPlayer3.Tx.Difficulty_Center_Bar.szテクスチャサイズ.Height / 3);

				for (int i = 0; i < width / wh + 1; i++)
				{
					for (int j = 0; j < height / wh + 1; j++)
					{
						if (i == 0 && j == 0)
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(0, 0, wh, wh));
						}
						else if (i == 0 && j == (height / wh))
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(0, wh*2, wh, wh));
						}
						else if (i == (width / wh) && j == 0)
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh + ydiff, new Rectangle(wh*2, 0, wh, wh));
						}
						else if (i == (width / wh) && j == (height / wh))
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(wh*2, wh*2, wh, wh));
						}
						else if (i == 0)
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(0, wh, wh, wh));
						}
						else if (j == 0)
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(wh, 0, wh, wh));
						}
						else if (i == (width / wh))
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh + ydiff, new Rectangle(wh*2, wh, wh, wh));
						}
						else if (j == (height / wh))
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(wh, wh*2, wh, wh));
						}
						else
						{
							TJAPlayer3.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(wh, wh, wh, wh));
						}
					}
				}
			}
			#endregion
			#region[タイトル文字列]
			int xAnime = 200;
			int yAnime = 30;

			if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲のサブタイトル != null)
			{
				TJAPlayer3.stage選曲.act曲リスト.サブタイトルtmp.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, 707 + (TJAPlayer3.stage選曲.act曲リスト.サブタイトルtmp.szテクスチャサイズ.Width / 2) + xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y + 440 - yAnime);
				if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲の曲名 != null)
				{
					TJAPlayer3.stage選曲.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y + 23 - yAnime);
				}
			}
			else if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲の曲名 != null)
			{
				TJAPlayer3.stage選曲.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y + 23 - yAnime);
			}
			#endregion
			#region[バーテクスチャ]
			for (int i = 0; i < 3; i++)
			{
				if (TJAPlayer3.Tx.Difficulty_Bar_Etc[i] != null)
					TJAPlayer3.Tx.Difficulty_Bar_Etc[i].t2D描画(TJAPlayer3.app.Device, i * TJAPlayer3.Skin.Difficulty_BarEtc_Padding + TJAPlayer3.Skin.Difficulty_BarEtc_XY[0], TJAPlayer3.Skin.Difficulty_BarEtc_XY[1]);
			}

			for (int i = 0; i < 4; i++)
			{
				if (裏表示 && i == 3)
				{
					if (TJAPlayer3.Tx.Difficulty_Bar[4] != null)
					{
						if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[4])
							TJAPlayer3.Tx.Difficulty_Bar[4].color4 = new Color4(1f, 1f, 1f, 1f);
						else
							TJAPlayer3.Tx.Difficulty_Bar[4].color4 = new Color4(0.5f, 0.5f, 0.5f, 1f);
						if (TJAPlayer3.Tx.Difficulty_Bar[4] != null)
							TJAPlayer3.Tx.Difficulty_Bar[4].t2D描画(TJAPlayer3.app.Device, i * TJAPlayer3.Skin.Difficulty_Bar_Padding + TJAPlayer3.Skin.Difficulty_Bar_XY[0], TJAPlayer3.Skin.Difficulty_Bar_XY[1]);
					}
				}
				else
				{
					if (TJAPlayer3.Tx.Difficulty_Bar[i] != null)
					{
						if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[i])
							TJAPlayer3.Tx.Difficulty_Bar[i].color4 = new Color4(1f, 1f, 1f, 1f);
						else
							TJAPlayer3.Tx.Difficulty_Bar[i].color4 = new Color4(0.5f, 0.5f, 0.5f, 1f);
						if (TJAPlayer3.Tx.Difficulty_Bar[i] != null)
							TJAPlayer3.Tx.Difficulty_Bar[i].t2D描画(TJAPlayer3.app.Device, i * TJAPlayer3.Skin.Difficulty_Bar_Padding + TJAPlayer3.Skin.Difficulty_Bar_XY[0], TJAPlayer3.Skin.Difficulty_Bar_XY[1]);
					}
				}
			}
            #endregion
            #region[星]
            if (TJAPlayer3.Tx.Difficulty_Star != null)//Difficulty_Starがないなら、通す必要なし！
			{
				for (int i = 0; i < 4; i++)
				{
					if (裏表示 && i == 3)
					{
						for (int j = 0; j < TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nレベル[4]; j++)
						{
							TJAPlayer3.Tx.Difficulty_Star.t2D描画(TJAPlayer3.app.Device, i * 100 + 475, 483 - (j * 20), new Rectangle(TJAPlayer3.Tx.Difficulty_Star.szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.Difficulty_Star.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Star.szテクスチャサイズ.Height));
						}
					}
					else
					{
						for (int j = 0; j < TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nレベル[i]; j++)
						{
							TJAPlayer3.Tx.Difficulty_Star.t2D描画(TJAPlayer3.app.Device, i * 100 + 475, 483 - (j * 20), new Rectangle(0, 0, TJAPlayer3.Tx.Difficulty_Star.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Star.szテクスチャサイズ.Height));
						}
					}
				}
			}
			#endregion
			#region[譜面分岐]
			if (TJAPlayer3.Tx.Difficulty_Branch != null)//Difficulty_Branchがないなら、通す必要なし！
			{
				TJAPlayer3.Tx.Difficulty_Branch.Opacity = (int)((ct分岐表示用タイマー.n現在の値 % 2) * 255.0);
				for (int i = 0; i < 4; i++)
				{
					if (裏表示 && i == 3)
					{
						if(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面分岐[4])
							TJAPlayer3.Tx.Difficulty_Branch.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(TJAPlayer3.Tx.Difficulty_Branch.szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.Difficulty_Branch.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Branch.szテクスチャサイズ.Height));
					}
					else
					{
						if(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面分岐[i])
							TJAPlayer3.Tx.Difficulty_Branch.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(0, 0, TJAPlayer3.Tx.Difficulty_Branch.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Branch.szテクスチャサイズ.Height));
					}
				}
			}
			#endregion
			#region[パパママサポート]
			if (TJAPlayer3.Tx.Difficulty_PapaMama != null)//Difficulty_PapaMamaがないなら、通す必要なし！
			{
				TJAPlayer3.Tx.Difficulty_PapaMama.Opacity = (int)((ct分岐表示用タイマー.n現在の値 % 2) * 255.0);
				for (int i = 0; i < 4; i++)
				{
					if (裏表示 && i == 3)
					{
						if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.bPapaMamaSupport[4])
							TJAPlayer3.Tx.Difficulty_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(TJAPlayer3.Tx.Difficulty_PapaMama.szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.Difficulty_PapaMama.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_PapaMama.szテクスチャサイズ.Height));
					}
					else
					{
						if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.bPapaMamaSupport[i])
							TJAPlayer3.Tx.Difficulty_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(0, 0, TJAPlayer3.Tx.Difficulty_PapaMama.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_PapaMama.szテクスチャサイズ.Height));
					}
				}
			}
			#endregion
			#region[王冠]
			if (TJAPlayer3.Tx.Crown_t != null)//王冠テクスチャがないなら、通す必要なし！
			{
				TJAPlayer3.Tx.Crown_t.Opacity = 255;
				TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率 = new Vector3(0.35f);
				for (int i = 0; i < 4; i++)
				{
					if (裏表示 && i == 3)
					{
						if(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[4])
							TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, i * TJAPlayer3.Skin.Difficulty_Bar_Padding + TJAPlayer3.Skin.Difficulty_Bar_XY[0] + 35, TJAPlayer3.Skin.Difficulty_Bar_XY[1] - 30, new Rectangle(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.n王冠[4] * 100, 0, 100, 100));
					}
					else
					{
						if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[i])
							TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, i * TJAPlayer3.Skin.Difficulty_Bar_Padding + TJAPlayer3.Skin.Difficulty_Bar_XY[0] + 35, TJAPlayer3.Skin.Difficulty_Bar_XY[1] - 30, new Rectangle(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.n王冠[i] * 100, 0, 100, 100));
					}
				}
			}
			#endregion
			#region[プレイヤーアンカー]
			for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++) {
				if (TJAPlayer3.ConfigIni.nPlayerCount >= 2 && 現在の選択行[0] == 現在の選択行[1] && !選択済み[0] && !選択済み[1]) {
					if (現在の選択行[i] < 3)
					{
						if (TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i] != null)
							TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].t2D描画(TJAPlayer3.app.Device, 現在の選択行[i] * TJAPlayer3.Skin.Difficulty_AncBoxEtc_Padding + TJAPlayer3.Skin.Difficulty_AncBoxEtc_XY[0] + i * TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].szテクスチャサイズ.Width / 2, TJAPlayer3.Skin.Difficulty_AncBoxEtc_XY[1], new Rectangle(i * TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].szテクスチャサイズ.Height)) ;

						if (TJAPlayer3.Tx.Difficulty_Anc_Same[i] != null)
							TJAPlayer3.Tx.Difficulty_Anc_Same[i].t2D描画(TJAPlayer3.app.Device, 現在の選択行[i] * TJAPlayer3.Skin.Difficulty_AncEtc_Padding + TJAPlayer3.Skin.Difficulty_AncEtc_XY[0] + (int)(TJAPlayer3.Tx.Difficulty_Anc_Same[i].szテクスチャサイズ.Width * (i - 0.5)), TJAPlayer3.Skin.Difficulty_AncEtc_XY[1]);
					}
					else
					{
						if (TJAPlayer3.Tx.Difficulty_Anc_Box[i] != null)
							TJAPlayer3.Tx.Difficulty_Anc_Box[i].t2D描画(TJAPlayer3.app.Device, (現在の選択行[i] - 3) * TJAPlayer3.Skin.Difficulty_AncBox_Padding + TJAPlayer3.Skin.Difficulty_AncBox_XY[0] + i * TJAPlayer3.Tx.Difficulty_Anc_Box[i].szテクスチャサイズ.Width / 2, TJAPlayer3.Skin.Difficulty_AncBox_XY[1], new Rectangle(i * TJAPlayer3.Tx.Difficulty_Anc_Box[i].szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.Difficulty_Anc_Box[i].szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.Difficulty_Anc_Box[i].szテクスチャサイズ.Height));

						if (TJAPlayer3.Tx.Difficulty_Anc_Same[i] != null)
							TJAPlayer3.Tx.Difficulty_Anc_Same[i].t2D描画(TJAPlayer3.app.Device, (現在の選択行[i] - 3) * TJAPlayer3.Skin.Difficulty_Anc_Padding + TJAPlayer3.Skin.Difficulty_Anc_XY[0] + (int)(TJAPlayer3.Tx.Difficulty_Anc_Same[i].szテクスチャサイズ.Width * (i - 0.5)), TJAPlayer3.Skin.Difficulty_Anc_XY[1]);
					}
				}
				else
				{
					if (!選択済み[i])
					{
						if (現在の選択行[i] < 3)
						{
							if (TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i] != null)
								TJAPlayer3.Tx.Difficulty_Anc_Box_Etc[i].t2D描画(TJAPlayer3.app.Device, 現在の選択行[i] * TJAPlayer3.Skin.Difficulty_AncBoxEtc_Padding + TJAPlayer3.Skin.Difficulty_AncBoxEtc_XY[0], TJAPlayer3.Skin.Difficulty_AncBoxEtc_XY[1]);

							if (TJAPlayer3.Tx.Difficulty_Anc[i] != null)
								TJAPlayer3.Tx.Difficulty_Anc[i].t2D描画(TJAPlayer3.app.Device, 現在の選択行[i] * TJAPlayer3.Skin.Difficulty_AncEtc_Padding + TJAPlayer3.Skin.Difficulty_AncEtc_XY[0], TJAPlayer3.Skin.Difficulty_AncEtc_XY[1]);
						}
						else
						{
							if (TJAPlayer3.Tx.Difficulty_Anc_Box[i] != null)
								TJAPlayer3.Tx.Difficulty_Anc_Box[i].t2D描画(TJAPlayer3.app.Device, (現在の選択行[i] - 3) * TJAPlayer3.Skin.Difficulty_AncBox_Padding + TJAPlayer3.Skin.Difficulty_AncBox_XY[0], TJAPlayer3.Skin.Difficulty_AncBox_XY[1]);

							if (TJAPlayer3.Tx.Difficulty_Anc[i] != null)
								TJAPlayer3.Tx.Difficulty_Anc[i].t2D描画(TJAPlayer3.app.Device, (現在の選択行[i] - 3) * TJAPlayer3.Skin.Difficulty_Anc_Padding + TJAPlayer3.Skin.Difficulty_Anc_XY[0], TJAPlayer3.Skin.Difficulty_Anc_XY[1]);
						}
					}
				}
			}
            #endregion

            //裏鬼表示用
            if ((裏カウント[0] >= 10 || 裏カウント[1] >= 10) && TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[4]) {
				裏表示 = !裏表示;
				裏カウント[0] = 0;
				裏カウント[1] = 0;
				if(裏表示)
					TJAPlayer3.stage選曲.act曲リスト.n現在のアンカ難易度レベル[0] = 4;
				else
					TJAPlayer3.stage選曲.act曲リスト.n現在のアンカ難易度レベル[0] = 3;
			}
			return 0;
		}


		

		// その他

		#region [ private ]
		//-----------------
		internal int[] 現在の選択行 = new int[2] { TJAPlayer3.ConfigIni.nDefaultCourse + 3, TJAPlayer3.ConfigIni.nDefaultCourse + 3 };
		internal bool[] 選択済み = new bool[2];
		internal int[] 確定された難易度 = new int[2];
		internal int[] 裏カウント = new int[2];
		internal bool 裏表示 = false;
		internal bool b開いた直後 = true;
		private CCounter ct分岐表示用タイマー;
		internal CCounter[] ct難易度拡大用 = { new CCounter(0, 180, 1, TJAPlayer3.Timer), new CCounter(0, 180, 1, TJAPlayer3.Timer) };
		//-----------------
		#endregion
	}
}
