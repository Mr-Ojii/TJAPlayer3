using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;
using FDK.ExtensionMethods;

namespace TJAPlayer3;

internal class CActDancer : CActivity
{
	/// <summary>
	/// 踊り子
	/// </summary>
	public CActDancer()
	{
		base.b活性化してない = true;
	}

	public override void On活性化()
	{
		this.ct踊り子モーション = new CCounter();
		base.On活性化();
	}

	public override void On非活性化()
	{
		this.ct踊り子モーション = null;
		base.On非活性化();
	}

	public override void OnManagedリソースの作成()
	{
		this.ar踊り子モーション番号 = TJAPlayer3.Skin.SkinConfig.Game.Dancer.Motion;
		if(this.ar踊り子モーション番号 == null) ar踊り子モーション番号 = new int[] { 0, 0 };
		this.ct踊り子モーション = new CCounter(0, this.ar踊り子モーション番号.Length - 1, 0.01, CSoundManager.rc演奏用タイマ);
		base.OnManagedリソースの作成();
	}

	public override void OnManagedリソースの解放()
	{
		base.OnManagedリソースの解放();
	}

	public override int On進行描画()
	{
		if( this.b初めての進行描画 )
		{
			this.b初めての進行描画 = false;
		}

		if (this.ct踊り子モーション != null || TJAPlayer3.Skin.Game_Dancer_Ptn != 0) this.ct踊り子モーション.t進行LoopDb();

		if (TJAPlayer3.ConfigIni.ShowDancer && this.ct踊り子モーション != null && TJAPlayer3.Skin.Game_Dancer_Ptn != 0)
		{
			for (int i = 0; i < TJAPlayer3.Tx.Dancer.Length; i++)
			{
				if (TJAPlayer3.Tx.Dancer[i][this.ar踊り子モーション番号[(int)this.ct踊り子モーション.db現在の値]] != null)
				{
					if ((int)TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[0] >= TJAPlayer3.Skin.SkinConfig.Game.Dancer.Gauge[i])
						TJAPlayer3.Tx.Dancer[i][this.ar踊り子モーション番号[(int)this.ct踊り子モーション.db現在の値]].t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.Skin.SkinConfig.Game.Dancer.X[i], TJAPlayer3.Skin.SkinConfig.Game.Dancer.Y[i]);
				}
			}
		}
		return base.On進行描画();
	}

	#region[ private ]
	//-----------------
	public int[] ar踊り子モーション番号;
	public CCounter ct踊り子モーション;
	//-----------------
	#endregion
}
