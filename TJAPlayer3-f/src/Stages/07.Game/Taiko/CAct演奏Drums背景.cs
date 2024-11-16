using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drums背景 : CActivity
{
    // 本家っぽい背景を表示させるメソッド。
    //
    // 拡張性とかないんで。はい、ヨロシクゥ!
    //
    public CAct演奏Drums背景()
    {
    }

    public void ClearIn(int player)
    {
        this.ct上背景クリアインタイマー[player] = new CCounter(0, 100, 2, TJAPlayer3.app.Timer);
        this.ct上背景クリアインタイマー[player].n現在の値 = 0;
        this.ct上背景FIFOタイマー = new CCounter(0, 100, 2, TJAPlayer3.app.Timer);
        this.ct上背景FIFOタイマー.n現在の値 = 0;
    }

    public override void On活性化()
    {

        this.ct上背景スクロール用タイマー = new CCounter[2];
        this.ct上背景上下用タイマー = new CCounter[2];
        this.ct上背景桜用タイマー = new CCounter[2];
        this.ct上背景桜スクロール用タイマー = new CCounter[2];
        this.ct上背景クリアインタイマー = new CCounter[2];
        for (int i = 0; i < 2; i++)
        {
            CTexture? up = TJAPlayer3.app.Tx.Background_Up[i];
            if (up is not null)
            {
                this.ct上背景スクロール用タイマー[i] = new CCounter(1, up.szTextureSize.Width, 16, TJAPlayer3.app.Timer);

                switch (TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollPattern[i])
                {
                    case 0:
                        this.ct上背景上下用タイマー[i] = new CCounter(1, 100, 30, TJAPlayer3.app.Timer);
                        break;

                    case 1:
                    case 2:
                        this.ct上背景上下用タイマー[i] = new CCounter(0, 3140, 1, TJAPlayer3.app.Timer);
                        break;
                    case 3:
                    default:
                        if (TJAPlayer3.app.Tx.Background_Up_YMove is not null)
                        {
                            CTexture? up_ymove = TJAPlayer3.app.Tx.Background_Up_YMove[i];
                            if (up_ymove is not null)
                                this.ct上背景上下用タイマー[i] = new CCounter(1, up_ymove.szTextureSize.Width, 6, TJAPlayer3.app.Timer);
                        }
                        break;
                }


                this.ct上背景桜用タイマー[i] = new CCounter(0, 400, 8, TJAPlayer3.app.Timer);
                CTexture? up_sakura = TJAPlayer3.app.Tx.Background_Up_Sakura[i];
                if (up_sakura is not null)
                {
                    this.ct上背景桜スクロール用タイマー[i] = new CCounter(0, up_sakura.szTextureSize.Width, 8, TJAPlayer3.app.Timer);
                }
                this.ct上背景クリアインタイマー[i] = new CCounter();
            }
        }
        if (TJAPlayer3.app.Tx.Background_Down_Scroll is not null)
            this.ct下背景スクロール用タイマー1 = new CCounter(1, TJAPlayer3.app.Tx.Background_Down_Scroll.szTextureSize.Width, 4, TJAPlayer3.app.Timer);

        this.ct上背景FIFOタイマー = new CCounter();
        base.On活性化();
    }

    public override void On非活性化()
    {
        this.ct上背景FIFOタイマー = null;
        for (int i = 0; i < 2; i++)
        {
            ct上背景スクロール用タイマー[i] = null;
        }
        for (int i = 0; i < 2; i++)
        {
            ct上背景上下用タイマー[i] = null;
        }
        for (int i = 0; i < 2; i++)
        {
            ct上背景桜用タイマー[i] = null;
        }
        for (int i = 0; i < 2; i++)
        {
            ct上背景桜スクロール用タイマー[i] = null;
        }
        this.ct下背景スクロール用タイマー1 = null;
        base.On非活性化();
    }

    public override int On進行描画()
    {
        this.ct上背景FIFOタイマー.t進行();

        for (int i = 0; i < 2; i++)
        {
            if (this.ct上背景クリアインタイマー[i] is not null)
                this.ct上背景クリアインタイマー[i].t進行();
        }
        for (int i = 0; i < 2; i++)
        {
            if (this.ct上背景スクロール用タイマー[i] is not null)
                this.ct上背景スクロール用タイマー[i].t進行Loop();
        }
        for (int i = 0; i < 2; i++)
        {
            if (this.ct上背景上下用タイマー[i] is not null)
                this.ct上背景上下用タイマー[i].t進行Loop();
        }
        for (int i = 0; i < 2; i++)
        {
            if (this.ct上背景桜用タイマー[i] is not null)
                this.ct上背景桜用タイマー[i].t進行Loop();
        }
        for (int i = 0; i < 2; i++)
        {
            if (this.ct上背景桜スクロール用タイマー[i] is not null)
                this.ct上背景桜スクロール用タイマー[i].t進行Loop();
        }
        if (this.ct下背景スクロール用タイマー1 is not null)
            this.ct下背景スクロール用タイマー1.t進行Loop();



        #region 1P-2P-上背景
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (this.ct上背景スクロール用タイマー[i] is not null)
            {
                CTexture? up = TJAPlayer3.app.Tx.Background_Up[i];
                if (up is not null)
                {
                    double TexSize = TJAPlayer3.app.LogicalSize.Width / up.szTextureSize.Width;
                    // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                    int ForLoop = (int)Math.Ceiling(TexSize) + 1;
                    //int nループ幅 = 328;
                    up.t2D描画(TJAPlayer3.app.Device, 0 - this.ct上背景スクロール用タイマー[i].n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i]);
                    for (int l = 1; l < ForLoop + 1; l++)
                    {
                        up.t2D描画(TJAPlayer3.app.Device, +(l * up.szTextureSize.Width) - this.ct上背景スクロール用タイマー[i].n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i]);
                    }
                }
                CTexture? up_clear = TJAPlayer3.app.Tx.Background_Up_Clear[i];
                if (up_clear is not null)
                {
                    if (TJAPlayer3.stage演奏ドラム画面.bIsAlreadyCleared[i])
                        up_clear.Opacity = ((this.ct上背景クリアインタイマー[i].n現在の値 * 0xff) / 100);
                    else
                        up_clear.Opacity = 0;

                    double TexSize = TJAPlayer3.app.LogicalSize.Width / up_clear.szTextureSize.Width;
                    // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                    int ForLoop = (int)Math.Ceiling(TexSize) + 1;

                    up_clear.t2D描画(TJAPlayer3.app.Device, 0 - this.ct上背景スクロール用タイマー[i].n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i]);
                    for (int l = 1; l < ForLoop + 1; l++)
                    {
                        up_clear.t2D描画(TJAPlayer3.app.Device, (l * up_clear.szTextureSize.Width) - this.ct上背景スクロール用タイマー[i].n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i]);
                    }
                }
                if (this.ct上背景桜用タイマー[i] is not null && this.ct上背景桜スクロール用タイマー[i] is not null)
                {
                    CTexture? up_sakura = TJAPlayer3.app.Tx.Background_Up_Sakura[i];
                    if (up_sakura is not null)
                    {
                        int xy = (int)(this.ct上背景桜用タイマー[i].n現在の値 - (this.ct上背景桜用タイマー[i].n終了値 / 2.0));

                        double TexSize = TJAPlayer3.app.LogicalSize.Width / up_sakura.szTextureSize.Width;
                        // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                        int ForLoop = (int)Math.Ceiling(TexSize) + 1;
                        //int nループ幅 = 328;
                        up_sakura.t2D描画(TJAPlayer3.app.Device, 0 - this.ct上背景桜スクロール用タイマー[i].n現在の値 - xy, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + xy);
                        for (int l = 1; l < ForLoop + 1; l++)
                        {
                            up_sakura.t2D描画(TJAPlayer3.app.Device, +(l * up_sakura.szTextureSize.Width) - this.ct上背景桜スクロール用タイマー[i].n現在の値 - xy, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + xy);
                        }
                    }
                    CTexture? up_sakura_clear = TJAPlayer3.app.Tx.Background_Up_Sakura_Clear[i];
                    if (up_sakura_clear is not null)
                    {
                        if (TJAPlayer3.stage演奏ドラム画面.bIsAlreadyCleared[i])
                            up_sakura_clear.Opacity = ((this.ct上背景クリアインタイマー[i].n現在の値 * 0xff) / 100);
                        else
                            up_sakura_clear.Opacity = 0;

                        int xy = (int)(this.ct上背景桜用タイマー[i].n現在の値 - this.ct上背景桜用タイマー[i].n終了値 / 2.0);

                        double TexSize = TJAPlayer3.app.LogicalSize.Width / up_sakura_clear.szTextureSize.Width;
                        // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                        int ForLoop = (int)Math.Ceiling(TexSize) + 1;
                        //int nループ幅 = 328;
                        up_sakura_clear.t2D描画(TJAPlayer3.app.Device, 0 - this.ct上背景桜スクロール用タイマー[i].n現在の値 - xy, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + xy);
                        for (int l = 1; l < ForLoop + 1; l++)
                        {
                            up_sakura_clear.t2D描画(TJAPlayer3.app.Device, +(l * up_sakura_clear.szTextureSize.Width) - this.ct上背景桜スクロール用タイマー[i].n現在の値 - xy, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + xy);
                        }
                    }
                }
                if (this.ct上背景上下用タイマー[i] is not null)
                {
                    CTexture? up_ymove = TJAPlayer3.app.Tx.Background_Up_YMove[i];
                    if (up_ymove is not null)
                    {
                        int ym;
                        int xm;

                        switch (TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollPattern[i])
                        {
                            case 0:
                                if (ct上背景上下用タイマー[i].n現在の値 <= ct上背景上下用タイマー[i].n終了値 * 0.5)
                                    ym = (int)((-ct上背景上下用タイマー[i].n現在の値) * 0.5);
                                else
                                    ym = (int)((ct上背景上下用タイマー[i].n現在の値 - ct上背景上下用タイマー[i].n終了値) * 0.5);
                                ym -= (int)(ct上背景上下用タイマー[i].n終了値 * 0.0625);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 1:
                                ym = (int)(Math.Sin(ct上背景上下用タイマー[i].n現在の値 / 1000.0) * 100.0);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 2:
                                ym = (int)(Math.Min(Math.Sin(ct上背景上下用タイマー[i].n現在の値 / 1000.0), 0.2) * 100.0);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 3:
                                const double seams = 0.4;
                                if (ct上背景上下用タイマー[i].n現在の値 <= ct上背景上下用タイマー[i].n終了値 * seams)
                                {
                                    ym = -(int)(Math.Sin((ct上背景上下用タイマー[i].n現在の値 / (ct上背景上下用タイマー[i].n終了値 * seams)) * Math.PI) * 20.0);
                                    xm = (int)this.ct上背景上下用タイマー[i].n現在の値;
                                }
                                else
                                {
                                    ym = -(int)(Math.Sin(((ct上背景上下用タイマー[i].n現在の値 - (ct上背景上下用タイマー[i].n終了値 * seams)) / (ct上背景上下用タイマー[i].n終了値 * (1.0 - seams))) * Math.PI) * 50.0);

                                    xm = (int)((this.ct上背景上下用タイマー[i].n現在の値 - (ct上背景上下用タイマー[i].n終了値 * seams)) * (2.0 - seams) / (1.0 - seams) + (ct上背景上下用タイマー[i].n終了値 * seams));
                                }
                                ym += 100;
                                break;
                            default:
                                ym = 0;
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                        }

                        double TexSize = TJAPlayer3.app.LogicalSize.Width / up_ymove.szTextureSize.Width;
                        // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+2足す。
                        int ForLoop = (int)Math.Ceiling(TexSize) + 2;
                        //int nループ幅 = 328;
                        up_ymove.t2D描画(TJAPlayer3.app.Device, 0 - xm, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + ym);
                        for (int l = 1; l < ForLoop + 1; l++)
                        {
                            up_ymove.t2D描画(TJAPlayer3.app.Device, +(l * up_ymove.szTextureSize.Width) - xm, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + ym);
                        }
                    }
                    CTexture? up_ymove_clear = TJAPlayer3.app.Tx.Background_Up_YMove_Clear[i];
                    if (up_ymove_clear is not null)
                    {
                        int ym;
                        int xm;

                        switch (TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollPattern[i])
                        {
                            case 0:
                                if (ct上背景上下用タイマー[i].n現在の値 <= ct上背景上下用タイマー[i].n終了値 * 0.5)
                                    ym = (int)((-ct上背景上下用タイマー[i].n現在の値) * 0.5);
                                else
                                    ym = (int)((ct上背景上下用タイマー[i].n現在の値 - ct上背景上下用タイマー[i].n終了値) * 0.5);
                                ym -= (int)(ct上背景上下用タイマー[i].n終了値 * 0.0625);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 1:
                                ym = (int)(Math.Sin(ct上背景上下用タイマー[i].n現在の値 / 1000.0) * 100.0);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 2:
                                ym = (int)(Math.Min(Math.Sin(ct上背景上下用タイマー[i].n現在の値 / 1000.0), 0.2) * 100.0);
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                            case 3:
                                const double seams = 0.4;
                                if (ct上背景上下用タイマー[i].n現在の値 <= ct上背景上下用タイマー[i].n終了値 * seams)
                                {
                                    ym = -(int)(Math.Sin((ct上背景上下用タイマー[i].n現在の値 / (ct上背景上下用タイマー[i].n終了値 * seams)) * Math.PI) * 20.0);
                                    xm = (int)this.ct上背景上下用タイマー[i].n現在の値;
                                }
                                else
                                {
                                    ym = -(int)(Math.Sin(((ct上背景上下用タイマー[i].n現在の値 - (ct上背景上下用タイマー[i].n終了値 * seams)) / (ct上背景上下用タイマー[i].n終了値 * (1.0 - seams))) * Math.PI) * 50.0);

                                    xm = (int)((this.ct上背景上下用タイマー[i].n現在の値 - (ct上背景上下用タイマー[i].n終了値 * seams)) * (2.0 - seams) / (1.0 - seams) + (ct上背景上下用タイマー[i].n終了値 * seams));
                                }
                                ym += 100;
                                break;
                            default:
                                ym = 0;
                                xm = this.ct上背景スクロール用タイマー[i].n現在の値;
                                break;
                        }

                        if (TJAPlayer3.stage演奏ドラム画面.bIsAlreadyCleared[i])
                            up_ymove_clear.Opacity = ((this.ct上背景クリアインタイマー[i].n現在の値 * 0xff) / 100);
                        else
                            up_ymove_clear.Opacity = 0;

                        double TexSize = TJAPlayer3.app.LogicalSize.Width / up_ymove_clear.szTextureSize.Width;
                        // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+2足す。
                        int ForLoop = (int)Math.Ceiling(TexSize) + 2;
                        //int nループ幅 = 328;
                        up_ymove_clear.t2D描画(TJAPlayer3.app.Device, 0 - xm, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + ym);
                        for (int l = 1; l < ForLoop + 1; l++)
                        {
                            up_ymove_clear.t2D描画(TJAPlayer3.app.Device, +(l * up_ymove_clear.szTextureSize.Width) - xm, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[i] + ym);
                        }
                    }
                }
            }
        }
        #endregion
        #region 1P-下背景
        if (!TJAPlayer3.stage演奏ドラム画面.bDoublePlay)
        {
            {
                if (TJAPlayer3.app.Tx.Background_Down is not null)
                {
                    TJAPlayer3.app.Tx.Background_Down.t2D描画(TJAPlayer3.app.Device, 0, 360);
                }
            }
            if (TJAPlayer3.stage演奏ドラム画面.bIsAlreadyCleared[0])
            {
                if (TJAPlayer3.app.Tx.Background_Down_Clear is not null && TJAPlayer3.app.Tx.Background_Down_Scroll is not null)
                {
                    TJAPlayer3.app.Tx.Background_Down_Clear.Opacity = ((this.ct上背景FIFOタイマー.n現在の値 * 0xff) / 100);
                    TJAPlayer3.app.Tx.Background_Down_Scroll.Opacity = ((this.ct上背景FIFOタイマー.n現在の値 * 0xff) / 100);
                    TJAPlayer3.app.Tx.Background_Down_Clear.t2D描画(TJAPlayer3.app.Device, 0, 360);

                    //int nループ幅 = 1257;
                    //CDTXMania.Tx.Background_Down_Scroll.t2D描画( CDTXMania.app.Device, 0 - this.ct下背景スクロール用タイマー1.n現在の値, 360 );
                    //CDTXMania.Tx.Background_Down_Scroll.t2D描画(CDTXMania.app.Device, (1 * nループ幅) - this.ct下背景スクロール用タイマー1.n現在の値, 360);
                    double TexSize = TJAPlayer3.app.LogicalSize.Width / TJAPlayer3.app.Tx.Background_Down_Scroll.szTextureSize.Width;
                    // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                    int ForLoop = (int)Math.Ceiling(TexSize) + 1;

                    //int nループ幅 = 328;
                    TJAPlayer3.app.Tx.Background_Down_Scroll.t2D描画(TJAPlayer3.app.Device, 0 - this.ct下背景スクロール用タイマー1.n現在の値, 360);
                    for (int l = 1; l < ForLoop + 1; l++)
                    {
                        TJAPlayer3.app.Tx.Background_Down_Scroll.t2D描画(TJAPlayer3.app.Device, +(l * TJAPlayer3.app.Tx.Background_Down_Scroll.szTextureSize.Width) - this.ct下背景スクロール用タイマー1.n現在の値, 360);
                    }

                }
            }
        }
        #endregion
        return base.On進行描画();
    }

    #region[ private ]
    //-----------------
    private CCounter[] ct上背景スクロール用タイマー; //上背景のX方向スクロール用
    private CCounter[] ct上背景上下用タイマー; //上背景のY方向上下用
    private CCounter[] ct上背景桜用タイマー; //上背景の桜用
    private CCounter[] ct上背景桜スクロール用タイマー; //上背景の桜用
    private CCounter ct下背景スクロール用タイマー1; //下背景パーツ1のX方向スクロール用
    private CCounter ct上背景FIFOタイマー;
    private CCounter[] ct上背景クリアインタイマー;
    //private CTexture tx上背景メイン;
    //private CTexture tx上背景クリアメイン;
    //private CTexture tx下背景メイン;
    //private CTexture tx下背景クリアメイン;
    //private CTexture tx下背景クリアサブ1;
    //-----------------
    #endregion
}
