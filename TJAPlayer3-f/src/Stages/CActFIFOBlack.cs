using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayer3;

internal class CActFIFOBlack : CActivity
{
    // メソッド

    public void tFadeOut開始()
    {
        this.mode = EFIFOMode.FadeOut;
        this.counter = new CCounter(0, 100, 5, TJAPlayer3.Timer);
    }
    public void tFadeIn開始()
    {
        this.mode = EFIFOMode.FadeIn;
        this.counter = new CCounter(0, 100, 5, TJAPlayer3.Timer);
    }


    // CActivity 実装

    public override void On非活性化()
    {
        if (!base.b活性化してない)
        {
            base.On非活性化();
        }
    }
    public override int On進行描画()
    {
        if (base.b活性化してない || (this.counter == null))
        {
            return 0;
        }
        this.counter.t進行();
        // Size clientSize = CDTXMania.app.Window.ClientSize;	// #23510 2010.10.31 yyagi: delete as of no one use this any longer.
        if (TJAPlayer3.Tx.Tile_Black != null)
        {
            TJAPlayer3.Tx.Tile_Black.Opacity = (this.mode == EFIFOMode.FadeIn) ? (((100 - this.counter.n現在の値) * 0xff) / 100) : ((this.counter.n現在の値 * 0xff) / 100);
            for (int i = 0; i <= (TJAPlayer3.app.LogicalSize.Width / 64); i++)		// #23510 2010.10.31 yyagi: change "clientSize.Width" to "640" to fix FIFO drawing size
            {
                for (int j = 0; j <= (TJAPlayer3.app.LogicalSize.Height / 64); j++)	// #23510 2010.10.31 yyagi: change "clientSize.Height" to "480" to fix FIFO drawing size
                {
                    TJAPlayer3.Tx.Tile_Black.t2D描画(TJAPlayer3.app.Device, i * 64, j * 64);
                }
            }
        }
        if (this.counter.n現在の値 != 100)
        {
            return 0;
        }
        return 1;
    }


    // その他

    #region [ private ]
    //-----------------
    private CCounter counter;
    private EFIFOMode mode;
    //-----------------
    #endregion
}
