using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.IO;
using FDK;
using System.Runtime.InteropServices;

namespace TJAPlayer3
{
	internal class CActSelectPlayOption : CActivity
	{
		// コンストラクタ

		private List<CItemBase> MakeListCItemBase(int nPlayer)
		{
			List<CItemBase> l = new List<CItemBase>();

			#region [ 個別 ScrollSpeed ]
			l.Add(new CItemInteger("ばいそく", 0, 1999, TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer],
				"演奏時のドラム譜面のスクロールの\n" +
				"速度を指定します。\n" +
				"x0.1 ～ x200.0 を指定可能です。",
				"To change the scroll speed for the\n" +
				"drums lanes.\n" +
				"You can set it from x0.1 to x200.0.\n" +
				"(ScrollSpeed=x0.5 means half speed)"));
			#endregion
			#region [ 共通 Dark/Risky/PlaySpeed ]
			if (nPlayer == 0)//1Pのときのみ表示
			{
				l.Add(new CItemInteger("演奏速度", 5, 400, TJAPlayer3.ConfigIni.n演奏速度,
					"曲の演奏速度を、速くしたり遅くした\n" +
					"りすることができます。\n" +
					"（※一部のサウンドカードでは正しく\n" +
					"再生できない可能性があります。）",
					"It changes the song speed.\n" +
					"For example, you can play in half\n" +
					" speed by setting PlaySpeed = 0.500\n" +
					" for your practice.\n" +
					"Note: It also changes the songs' pitch."));

			#endregion
			#region [ 個別 Sud/Hid ]
				l.Add(new CItemList("ゲーム", (int)TJAPlayer3.ConfigIni.eGameMode,
					"ゲームモード\n" +
					"TYPE-A: 完走!叩ききりまショー!\n" +
					"TYPE-B: 完走!叩ききりまショー!(激辛)\n" +
					" \n",
					" \n" +
					" \n" +
					" ",
					new string[] { "OFF", "完走!", "完走!激辛", "特訓" }));

			}
			l.Add(new CItemList("ランダム", (int)TJAPlayer3.ConfigIni.eRandom[nPlayer],
				"いわゆるランダム。\n  RANDOM: ちょっと変わる\n  MIRROR: あべこべ \n  SUPER: そこそこヤバい\n  HYPER: 結構ヤバい\nなお、実装は適当な模様",
				"Drums chips come randomly.\n\n Part: swapping lanes randomly for each\n  measures.\n Super: swapping chip randomly\n Hyper: swapping randomly\n  (number of lanes also changes)",
				new string[] { "OFF", "RANDOM", "あべこべ", "SUPER", "HYPER" }));
			l.Add(new CItemList("ドロン", (int)TJAPlayer3.ConfigIni.eSTEALTH[nPlayer],
				"",
				new string[] { "OFF", "ドロン", "ステルス" }));

			l.Add(new CItemList("真打", TJAPlayer3.ConfigIni.ShinuchiMode[nPlayer] ? 1 : 0, "", "", new string[] { "OFF", "ON" }));
			if (nPlayer == 0)
			{
				l.Add(new CItemInteger("プレイ人数", 1, 2, TJAPlayer3.ConfigIni.nPlayerCount,
					"プレイヤー人数",
					"PlayerCount"));
			}
			#endregion

			return l;
		}

		private List<ItemTextureList> MakeItemnameTexture(List<CItemBase> cItemBases)
		{
			List<ItemTextureList> textures = new List<ItemTextureList>();
			for (int i = 0; i < cItemBases.Count; i++)
			{
				CTexture NameTexture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont(cItemBases[i].str項目名, Color.White, Color.Black));
				CTexture[] ListTexture;
				if (cItemBases[i].e種別 == CItemBase.E種別.リスト) {
					ListTexture = new CTexture[((CItemList)cItemBases[i]).list項目値.Count];
					for (int index = 0; index < ((CItemList)cItemBases[i]).list項目値.Count; index++) {
						ListTexture[index] = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont(((CItemList)cItemBases[i]).list項目値[index], Color.White, Color.Black));
					}
				}
				else
				{
					ListTexture = null;
				}
				textures.Add(new ItemTextureList(NameTexture, ListTexture));
			}
			return textures;
		}

		private void SaveValue(int nPlayer)
		{
			if (nPlayer == 0)
			{
				switch (NowRow[0])
				{
					case (int)EItemList1P.ScrollSpeed:
						TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] = lci[nPlayer][(int)EItemList1P.ScrollSpeed].GetIndex();
						break;
					case (int)EItemList1P.PlaySpeed:
						TJAPlayer3.ConfigIni.n演奏速度 = lci[nPlayer][(int)EItemList1P.PlaySpeed].GetIndex();
						break;
					case (int)EItemList1P.GameMode:
						TJAPlayer3.ConfigIni.eGameMode = (EGame)lci[nPlayer][(int)EItemList1P.GameMode].GetIndex();
						break;
					case (int)EItemList1P.Random:
						TJAPlayer3.ConfigIni.eRandom[nPlayer] = (ERandomMode)lci[nPlayer][(int)EItemList1P.Random].GetIndex();
						break;
					case (int)EItemList1P.Stealth:
						TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] = (EStealthMode)lci[nPlayer][(int)EItemList1P.Stealth].GetIndex();
						break;
					case (int)EItemList1P.Shinuchi:
						TJAPlayer3.ConfigIni.ShinuchiMode[nPlayer] = lci[nPlayer][(int)EItemList1P.Shinuchi].GetIndex() == 1;
						break;
					case (int)EItemList1P.PlayerCount:
						TJAPlayer3.ConfigIni.nPlayerCount = lci[nPlayer][(int)EItemList1P.PlayerCount].GetIndex();
						if (TJAPlayer3.ConfigIni.nPlayerCount == 1) 
						{
							this.tDeativatePopupMenu(1);
							TJAPlayer3.stage選曲.actChangeSE.tDeativateChangeSE(1);
						}
						break;
				}
			}
			else if(nPlayer == 1) 
			{
				switch (NowRow[1])
				{
					case (int)EItemList2P.ScrollSpeed:
						TJAPlayer3.ConfigIni.n譜面スクロール速度[nPlayer] = lci[nPlayer][(int)EItemList2P.ScrollSpeed].GetIndex();
						break;
					case (int)EItemList2P.Random:
						TJAPlayer3.ConfigIni.eRandom[nPlayer] = (ERandomMode)lci[nPlayer][(int)EItemList2P.Random].GetIndex();
						break;
					case (int)EItemList2P.Stealth:
						TJAPlayer3.ConfigIni.eSTEALTH[nPlayer] = (EStealthMode)lci[nPlayer][(int)EItemList2P.Stealth].GetIndex();
						break;
					case (int)EItemList2P.Shinuchi:
						TJAPlayer3.ConfigIni.ShinuchiMode[nPlayer] = lci[nPlayer][(int)EItemList2P.Shinuchi].GetIndex() == 1;
						break;
				}
			}
		}

		private enum EItemList1P : int
		{
			ScrollSpeed = 0,
			PlaySpeed,
			GameMode,
			Random,
			Stealth,
			Shinuchi,
			PlayerCount
		}
		private enum EItemList2P : int
		{
			ScrollSpeed = 0,
			Random,
			Stealth,
			Shinuchi
		}

		// メソッド
		public void tActivatePopupMenu(int nPlayer)
		{
			if (ePhase[nPlayer] == EChangeSEPhase.Inactive)
			{
				this.NowRow[nPlayer] = 0;
				ePhase[nPlayer] = EChangeSEPhase.AnimationIn;
				ct登場退場アニメ用[nPlayer].t時間Reset();
				ct登場退場アニメ用[nPlayer].n現在の値 = 0;
			}
		}

		public void tDeativatePopupMenu(int nPlayer)
		{
			if (ePhase[nPlayer] == EChangeSEPhase.Active)
			{
				ePhase[nPlayer] = EChangeSEPhase.AnimationOut;
				ct登場退場アニメ用[nPlayer].t時間Reset();
				ct登場退場アニメ用[nPlayer].n現在の値 = 0;
			}
		}

		// CActivity 実装

		public override void On活性化()
		{
			this.Font = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 20);
			lci = new List<CItemBase>[2];
			lci[0] = MakeListCItemBase(0);
			lci[1] = MakeListCItemBase(1);
			NameTexture[0] = MakeItemnameTexture(lci[0]);
			NameTexture[1] = MakeItemnameTexture(lci[1]);
			base.On活性化();
		}

		public override void On非活性化()
		{
			if (this.Font != null)
				this.Font.Dispose();
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				base.OnManagedリソースの解放();
			}
		}

		public override int On進行描画()
		{
			if (base.b初めての進行描画)
			{
				base.b初めての進行描画 = false;
			}

			for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
			{
				this.ct登場退場アニメ用[nPlayer].t進行();
				if (this.ePhase[nPlayer] == EChangeSEPhase.Active)
				{
					this.tDrawBox(TJAPlayer3.Skin.PlayOption_Box_X[nPlayer], TJAPlayer3.Skin.PlayOption_Box_Y[nPlayer], nPlayer);
				}
				else if (this.ePhase[nPlayer] == EChangeSEPhase.AnimationIn)
				{
					int y = (int)((TJAPlayer3.Skin.PlayOption_Box_Y[nPlayer]) + (float)((Math.Sin(this.ct登場退場アニメ用[nPlayer].n現在の値 / 100.0) - 0.95) * -500f));
					this.tDrawBox(TJAPlayer3.Skin.PlayOption_Box_X[nPlayer], y, nPlayer);

					if (this.ct登場退場アニメ用[nPlayer].b終了値に達した)
						this.ePhase[nPlayer] = EChangeSEPhase.Active;
				}
				else if (this.ePhase[nPlayer] == EChangeSEPhase.AnimationOut)
				{
					int y = (int)((TJAPlayer3.Skin.PlayOption_Box_Y[nPlayer]) + (float)((Math.Sin((this.ct登場退場アニメ用[nPlayer].n終了値 - this.ct登場退場アニメ用[nPlayer].n現在の値) / 100.0) - 0.95) * -500f));
					this.tDrawBox(TJAPlayer3.Skin.PlayOption_Box_X[nPlayer], y, nPlayer);

					if (this.ct登場退場アニメ用[nPlayer].b終了値に達した)
						this.ePhase[nPlayer] = EChangeSEPhase.Inactive;
				}
			}

			if (this.ePhase[0] == EChangeSEPhase.Active)
			{
				if (TJAPlayer3.Pad.b押された(Eパッド.LRed) || TJAPlayer3.Pad.b押された(Eパッド.RRed))
				{
					TJAPlayer3.Skin.sound決定音?.t再生する();
					SaveValue(0);
					NowRow[0]++;
					if (NowRow[0] >= lci[0].Count)
						tDeativatePopupMenu(0);
				}
				if (TJAPlayer3.Pad.b押された(Eパッド.LBlue))
				{
					TJAPlayer3.Skin.sound変更音?.t再生する();
					lci[0][NowRow[0]].t項目値を前へ移動();
				}
				if (TJAPlayer3.Pad.b押された(Eパッド.RBlue))
				{
					TJAPlayer3.Skin.sound変更音?.t再生する();
					lci[0][NowRow[0]].t項目値を次へ移動();
				}
			}

			if (this.ePhase[1] == EChangeSEPhase.Active)
			{
				if (TJAPlayer3.Pad.b押された(Eパッド.LRed2P) || TJAPlayer3.Pad.b押された(Eパッド.RRed2P))
				{
					TJAPlayer3.Skin.sound決定音?.t再生する();
					SaveValue(1);
					NowRow[1]++;
					if (NowRow[1] >= lci[1].Count)
						tDeativatePopupMenu(1);
				}
				if (TJAPlayer3.Pad.b押された(Eパッド.LBlue2P))
				{
					TJAPlayer3.Skin.sound変更音?.t再生する();
					lci[1][NowRow[1]].t項目値を前へ移動();
				}
				if (TJAPlayer3.Pad.b押された(Eパッド.RBlue2P))
				{
					TJAPlayer3.Skin.sound変更音?.t再生する();
					lci[1][NowRow[1]].t項目値を次へ移動();
				}
			}

			return base.On進行描画();
		}


		/// <param name="x">下中央基準のX</param>
		/// <param name="y">下中央基準のY</param>
		private void tDrawBox(float x, float y, int nPlayer) {
			if (TJAPlayer3.Tx.PlayOption_List != null && TJAPlayer3.Tx.PlayOption_Active != null)
			{
				TJAPlayer3.Tx.PlayOption_List.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, x, y, new Rectangle(0, TJAPlayer3.Skin.PlayOption_Box_Section_Y[2], TJAPlayer3.Tx.PlayOption_List.szテクスチャサイズ.Width, TJAPlayer3.Tx.PlayOption_List.szテクスチャサイズ.Height - TJAPlayer3.Skin.PlayOption_Box_Section_Y[2]));//下部
				y -= TJAPlayer3.Tx.PlayOption_List.szテクスチャサイズ.Height - TJAPlayer3.Skin.PlayOption_Box_Section_Y[2];

				for (int i = 0; i < lci[nPlayer].Count; i++)
				{
					TJAPlayer3.Tx.PlayOption_List.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, x, y, new Rectangle(0, TJAPlayer3.Skin.PlayOption_Box_Section_Y[1], TJAPlayer3.Tx.PlayOption_List.szテクスチャサイズ.Width, TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]));//リスト本体
					if (lci[nPlayer].Count - i == NowRow[nPlayer] + 1)
						TJAPlayer3.Tx.PlayOption_Active.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, x, y);

					this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemNameTexture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_Name_XY_Diff[0], y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_Name_XY_Diff[1]);

					if (lci[nPlayer][lci[nPlayer].Count - i - 1].str項目名.Equals("ばいそく"))
					{
						using (CTexture texture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont(((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() + 1) * 0.1).ToString("0.0"), Color.White, Color.Black)))
						{
							texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_List_XY_Diff[0] - texture.szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
						}
					}
					else if (lci[nPlayer][lci[nPlayer].Count - i - 1].str項目名.Equals("演奏速度"))
					{
						using (CTexture texture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() * 0.05).ToString("0.00"), Color.White, Color.Black)))
						{
							texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_List_XY_Diff[0] - texture.szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
						}
					}
					else if (lci[nPlayer][lci[nPlayer].Count - i - 1].e種別 == CItemBase.E種別.整数)
					{
						using (CTexture texture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()).ToString(), Color.White, Color.Black)))
						{
							texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_List_XY_Diff[0] - texture.szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
						}
					}
					else if (lci[nPlayer][lci[nPlayer].Count - i - 1].e種別 == CItemBase.E種別.リスト)
					{
						this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].t2D描画(TJAPlayer3.app.Device, x + 90 - this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
					}

					y -= TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1];
				}

				TJAPlayer3.Tx.PlayOption_List.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, x, y, new Rectangle(0, TJAPlayer3.Skin.PlayOption_Box_Section_Y[0], TJAPlayer3.Tx.PlayOption_List.szテクスチャサイズ.Width, TJAPlayer3.Skin.PlayOption_Box_Section_Y[1] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[0]));//上部
			}
			else//項目名だけは表示
			{
				y -= 100;
				for (int i = 0; i < lci[nPlayer].Count; i++)
				{
					this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemNameTexture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_Name_XY_Diff[0], y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_Name_XY_Diff[1]);

					if (lci[nPlayer][lci[nPlayer].Count - i - 1].str項目名.Equals("ばいそく"))
					{
						using (CTexture texture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont(((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() + 1) * 0.1).ToString("0.0"), Color.White, Color.Black)))
						{
							texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_List_XY_Diff[0] - texture.szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
						}
					}
					else if (lci[nPlayer][lci[nPlayer].Count - i - 1].str項目名.Equals("演奏速度"))
					{
						using (CTexture texture = TJAPlayer3.tテクスチャの生成(this.Font.DrawPrivateFont((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() * 0.05).ToString("0.00"), Color.White, Color.Black)))
						{
							texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.Skin.PlayOption_List_XY_Diff[0] - texture.szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
						}
					}
					else if (lci[nPlayer][lci[nPlayer].Count - i - 1].e種別 == CItemBase.E種別.リスト)
					{
						this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].t2D描画(TJAPlayer3.app.Device, x + 90 - this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].szテクスチャサイズ.Width / 2, y - (TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1]) + TJAPlayer3.Skin.PlayOption_List_XY_Diff[1]);
					}

					y -= TJAPlayer3.Skin.PlayOption_Box_Section_Y[2] - TJAPlayer3.Skin.PlayOption_Box_Section_Y[1];
				}
			}
		}

		public bool[] bIsActive
		{
			get
			{
				return new bool[] { (ePhase[0] != EChangeSEPhase.Inactive), (ePhase[1] != EChangeSEPhase.Inactive) };
			}
		}

		#region [ private ]
		//-----------------
		private class ItemTextureList
		{
			public CTexture ItemNameTexture;
			public CTexture[] ItemListTexture;

			public ItemTextureList(CTexture NameTexture, CTexture[] ListTexture) 
			{
				this.ItemNameTexture = NameTexture;
				this.ItemListTexture = ListTexture;
			}
		}

		private CCounter[] ct登場退場アニメ用 = { new CCounter(0, 188, 2, TJAPlayer3.Timer), new CCounter(0, 188, 2, TJAPlayer3.Timer) };//Math.PI-Math.Asin(0.95)
		private EChangeSEPhase[] ePhase = { EChangeSEPhase.Inactive, EChangeSEPhase.Inactive };
		private int[] NowRow = { 0, 0 };
		private List<ItemTextureList>[] NameTexture = new List<ItemTextureList>[2];
		private List<CItemBase>[] lci;
		private CPrivateFastFont Font;

		private enum EChangeSEPhase
		{
			Inactive,
			AnimationIn,
			Active,
			AnimationOut
		}
		//-----------------
		#endregion
	}


}
