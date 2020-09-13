using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏Drumsチップエフェクト : CActivity
	{
		// コンストラクタ

		public CAct演奏Drumsチップエフェクト()
		{
			//base.b活性化してない = true;
		}
		
		
		// メソッド
		public virtual void Start(int nPlayer, int Lane)
		{
			if(TJAPlayer3.Tx.Gauge_Soul_Explosion != null)
			{
				for (int i = 0; i < 128; i++)
				{
					if(!st[i].b使用中)
					{
						st[i].b使用中 = true;
						st[i].ct進行 = new CCounter(0, TJAPlayer3.Skin.Game_Effect_NotesFlash[2], TJAPlayer3.Skin.Game_Effect_NotesFlash_Timer, TJAPlayer3.Timer);
						st[i].nプレイヤー = nPlayer;
						st[i].Lane = Lane;
						break;
					}
				}
			}
		}

		// CActivity 実装

		public override void On活性化()
		{
			for (int i = 0; i < 128; i++)
			{
				st[i] = new STチップエフェクト
				{
					b使用中 = false,
					ct進行 = new CCounter()
				};
			}
			base.On活性化();
		}
		public override void On非活性化()
		{
			for (int i = 0; i < 128; i++)
			{
				st[i].ct進行 = null;
				st[i].b使用中 = false;
			}
			base.On非活性化();
		}
		public override int On進行描画()
		{
			for (int i = 0; i < 128; i++)
			{
				if (st[i].b使用中)
				{
					st[i].ct進行.t進行();
					if (st[i].ct進行.b終了値に達した)
					{
						st[i].ct進行.t停止();
						st[i].b使用中 = false;
					}
					TJAPlayer3.Tx.Notes.Opacity = 255 - (int)Math.Min((500.0 * (st[i].ct進行.n現在の値 / (double)st[i].ct進行.n終了値)), 255.0);
					if(TJAPlayer3.Tx.Notes_White!=null)
						TJAPlayer3.Tx.Notes_White.Opacity = (int)Math.Min((500.0 * (st[i].ct進行.n現在の値 / (double)st[i].ct進行.n終了値)), 255.0);//2020.05.15 Mr-Ojii ノーツを白くするために追加。 
					switch (st[i].nプレイヤー)
					{
						case 0:
							if (TJAPlayer3.Tx.Gauge_Soul_Explosion[0] != null)
								TJAPlayer3.Tx.Gauge_Soul_Explosion[0].t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[0], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[0], new Rectangle(st[i].ct進行.n現在の値 * TJAPlayer3.Skin.Game_Effect_NotesFlash[0], 0, TJAPlayer3.Skin.Game_Effect_NotesFlash[0], TJAPlayer3.Skin.Game_Effect_NotesFlash[1]));
							TJAPlayer3.Tx.Notes.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[0], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[0], new Rectangle(st[i].Lane * 130, 0, 130, 130));
							if (TJAPlayer3.Tx.Notes_White != null)
								TJAPlayer3.Tx.Notes_White.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[0], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[0], new Rectangle(st[i].Lane * 130, 0, 130, 130));
							break;
						case 1:
							if (TJAPlayer3.Tx.Gauge_Soul_Explosion[1] != null)
								TJAPlayer3.Tx.Gauge_Soul_Explosion[1].t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[1], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[1], new Rectangle(st[i].ct進行.n現在の値 * TJAPlayer3.Skin.Game_Effect_NotesFlash[0], 0, TJAPlayer3.Skin.Game_Effect_NotesFlash[0], TJAPlayer3.Skin.Game_Effect_NotesFlash[1]));
							TJAPlayer3.Tx.Notes.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[1], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[1], new Rectangle(st[i].Lane * 130, 0, 130, 130));
							if (TJAPlayer3.Tx.Notes_White != null)
								TJAPlayer3.Tx.Notes_White.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_X[1], TJAPlayer3.Skin.Game_Effect_FlyingNotes_EndPoint_Y[1], new Rectangle(st[i].Lane * 130, 0, 130, 130));
							break;
					}
					TJAPlayer3.Tx.Notes.Opacity = 255;//2020.05.15 Mr-Ojii これ書いとかないと、流れるノーツも半透明化されてしまう。
				}
			}
			return 0;
		}
		

		// その他

		#region [ private ]
		//-----------------
		//private CTexture[] txChara;

		[StructLayout(LayoutKind.Sequential)]
		private struct STチップエフェクト
		{
			public bool b使用中;
			public CCounter ct進行;
			public int nプレイヤー;
			public int Lane;
		}
		private STチップエフェクト[] st = new STチップエフェクト[128];
		//private struct ST連打キャラ
		//{
		//    public int nColor;
		//    public bool b使用中;
		//    public CCounter ct進行;
		//    public int n前回のValue;
		//    public float fX;
		//    public float fY;
		//    public float fX開始点;
		//    public float fY開始点;
		//    public float f進行方向; //進行方向 0:左→右 1:左下→右上 2:右→左
		//    public float fX加速度;
		//    public float fY加速度;
		//}
		//private ST連打キャラ[] st連打キャラ = new ST連打キャラ[64];
		//-----------------
		#endregion
	}
}
