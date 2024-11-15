using FDK;

namespace TJAPlayer3;

internal class CActFIFOResult : CActivity
{
    // メソッド

    public void tFadeOut開始()
    {
        this.mode = EFIFOMode.FadeOut;
        this.counter = new CCounter(0, 500, 2, TJAPlayer3.app.Timer);
        if (TJAPlayer3.app.Tx.Result_FadeIn is not null)
            TJAPlayer3.app.Tx.Result_FadeIn.Opacity = 255;
    }
    public void tFadeIn開始()
    {
        this.mode = EFIFOMode.FadeIn;
        this.counter = new CCounter(0, 100, 5, TJAPlayer3.app.Timer);
        if (TJAPlayer3.app.Tx.Result_FadeIn is not null)
            TJAPlayer3.app.Tx.Result_FadeIn.Opacity = 255;
    }
    public void tFadeIn完了()		// #25406 2011.6.9 yyagi
    {
        if (this.counter is not null)
            this.counter.n現在の値 = this.counter.n終了値;
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
        if (base.b活性化してない || (this.counter is null))
        {
            return 0;
        }
        this.counter.t進行();

        if (TJAPlayer3.app.ConfigToml.EnableSkinV2)
        {
            if (TJAPlayer3.app.Tx.Tile_Black is not null)
            {
                TJAPlayer3.app.Tx.Tile_Black.Opacity = (this.mode == EFIFOMode.FadeIn) ? (((100 - this.counter.n現在の値) * 0xff) / 100) : ((this.counter.n現在の値 * 0xff) / 500);
                for (int i = 0; i <= (TJAPlayer3.app.LogicalSize.Width / 64); i++)      // #23510 2010.10.31 yyagi: change "clientSize.Width" to "640" to fix FIFO drawing size
                {
                    for (int j = 0; j <= (TJAPlayer3.app.LogicalSize.Height / 64); j++) // #23510 2010.10.31 yyagi: change "clientSize.Height" to "480" to fix FIFO drawing size
                    {
                        TJAPlayer3.app.Tx.Tile_Black.t2D描画(TJAPlayer3.app.Device, i * 64, j * 64);
                    }
                }
            }
        }
        else
        {
            if (TJAPlayer3.app.Tx.Result_FadeIn is not null)
            {
                if (this.mode == EFIFOMode.FadeOut)
                {
                    int y = Math.Min(360, this.counter.n現在の値);
                    TJAPlayer3.app.Tx.Result_FadeIn.t2D描画(TJAPlayer3.app.Device, 0, -360 + y, new Rectangle(0, 0, 1280, 380));
                    TJAPlayer3.app.Tx.Result_FadeIn.t2D描画(TJAPlayer3.app.Device, 0, 720 - y, new Rectangle(0, 380, 1280, 360));
                }
                else
                {
                    TJAPlayer3.app.Tx.Result_FadeIn.Opacity = (((100 - this.counter.n現在の値) * 0xff) / 100);
                    TJAPlayer3.app.Tx.Result_FadeIn.t2D描画(TJAPlayer3.app.Device, 0, 0, new Rectangle(0, 0, 1280, 360));
                    TJAPlayer3.app.Tx.Result_FadeIn.t2D描画(TJAPlayer3.app.Device, 0, 360, new Rectangle(0, 380, 1280, 360));
                }
            }
        }
        if (this.mode == EFIFOMode.FadeOut)
        {
            if (this.counter.n現在の値 != 500)
            {
                return 0;
            }
        }
        else if (this.mode == EFIFOMode.FadeIn)
        {
            if (this.counter.n現在の値 != 100)
            {
                return 0;
            }
        }
        return 1;
    }


    // その他

    #region [ private ]
    //-----------------
    private CCounter? counter = null;
    private EFIFOMode mode;
    //-----------------
    #endregion
}
