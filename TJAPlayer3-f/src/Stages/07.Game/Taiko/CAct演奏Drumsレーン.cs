using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drumsレーン : CActivity
{
    public CAct演奏Drumsレーン()
    {
    }

    public override void On活性化()
    {
        this.ct分岐アニメ進行 = new CCounter[4];
        this.nBefore = new int[4];
        this.nAfter = new int[4];
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            this.ct分岐アニメ進行[i] = new CCounter();
            this.nBefore[i] = 0;
            this.nAfter[i] = 0;
            this.bState[i] = false;
        }
        CTexture? lane_base0 = TJAPlayer3.app.Tx.Lane_Base[0];
        if (lane_base0 is not null)
            lane_base0.Opacity = 255;
        base.On活性化();
    }

    public override void On非活性化()
    {
        this.ct分岐アニメ進行 = null;
        base.On非活性化();
    }

    public override int On進行描画()
    {
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (!this.ct分岐アニメ進行[i].b停止中)
            {
                this.ct分岐アニメ進行[i].t進行();
                if (this.ct分岐アニメ進行[i].b終了値に達した)
                {
                    this.bState[i] = false;
                    this.ct分岐アニメ進行[i].t停止();
                }
            }
        }


        //アニメーション中の分岐レイヤー(背景)の描画を行う。
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.stage演奏ドラム画面.bUseBranch[i] == true)
            {
                if (this.ct分岐アニメ進行[i].b進行中)
                {
                    CTexture? lane_base_0 = TJAPlayer3.app.Tx.Lane_Base[0];
                    CTexture? lane_base_1 = TJAPlayer3.app.Tx.Lane_Base[1];
                    CTexture? lane_base_2 = TJAPlayer3.app.Tx.Lane_Base[2];
                    #region[ 普通譜面_レベルアップ ]
                    //普通→玄人
                    if (nBefore[i] == 0 && nAfter[i] == 1 && lane_base_1 is not null && lane_base_0 is not null)
                    {
                        lane_base_1.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                        lane_base_0.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                    }
                    //普通→達人
                    if (nBefore[i] == 0 && nAfter[i] == 2 && lane_base_0 is not null && lane_base_1 is not null && lane_base_2 is not null)
                    {
                        lane_base_0.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        if (this.ct分岐アニメ進行[i].n現在の値 < 100)
                        {
                            lane_base_1.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                        else if (this.ct分岐アニメ進行[i].n現在の値 >= 100 && this.ct分岐アニメ進行[i].n現在の値 < 150)
                        {
                            lane_base_1.Opacity = 255;
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                        else if (this.ct分岐アニメ進行[i].n現在の値 >= 150)
                        {
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            lane_base_2.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 250 ? 255 : (((this.ct分岐アニメ進行[i].n現在の値 - 150) * 0xff) / 100);
                            lane_base_2.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                    }
                    #endregion
                    #region[ 玄人譜面_レベルアップ ]
                    if (nBefore[i] == 1 && nAfter[i] == 2 && lane_base_1 is not null && lane_base_2 is not null)
                    {
                        lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        lane_base_2.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                        lane_base_2.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                    }
                    #endregion
                    #region[ 玄人譜面_レベルダウン ]
                    if (nBefore[i] == 1 && nAfter[i] == 0 && lane_base_0 is not null && lane_base_1 is not null)
                    {
                        lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        lane_base_0.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                        lane_base_0.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                    }
                    #endregion
                    #region[ 達人譜面_レベルダウン ]
                    if (nBefore[i] == 2 && nAfter[i] == 0 && lane_base_0 is not null && lane_base_1 is not null && lane_base_2 is not null)
                    {
                        lane_base_2.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        if (this.ct分岐アニメ進行[i].n現在の値 < 100)
                        {
                            lane_base_1.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                        else if (this.ct分岐アニメ進行[i].n現在の値 >= 100 && this.ct分岐アニメ進行[i].n現在の値 < 150)
                        {
                            lane_base_1.Opacity = 255;
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                        else if (this.ct分岐アニメ進行[i].n現在の値 >= 150)
                        {
                            lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            lane_base_0.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 250 ? 255 : (((this.ct分岐アニメ進行[i].n現在の値 - 150) * 0xff) / 100);
                            lane_base_0.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                    }
                    if (nBefore[i] == 2 && nAfter[i] == 1 && lane_base_2 is not null && lane_base_1 is not null)
                    {
                        lane_base_2.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        lane_base_1.Opacity = this.ct分岐アニメ進行[i].n現在の値 > 100 ? 255 : ((this.ct分岐アニメ進行[i].n現在の値 * 0xff) / 100);
                        lane_base_1.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                    }
                    #endregion
                }
            }
        }
        return base.On進行描画();
    }

    public virtual void t分岐レイヤー_コース変化(int n現在, int n次回, int player)
    {
        if (n現在 == n次回)
        {
            return;
        }
        this.ct分岐アニメ進行[player] = new CCounter(0, 300, 2, TJAPlayer3.app.Timer);
        this.bState[player] = true;

        this.nBefore[player] = n現在;
        this.nAfter[player] = n次回;

    }

    #region[ private ]
    //-----------------
    public bool[] bState = new bool[4];
    public CCounter[] ct分岐アニメ進行 = new CCounter[4];
    private int[] nBefore;
    private int[] nAfter;
    private int[] n透明度 = new int[4];
    //private CTexture[] tx普通譜面 = new CTexture[2];
    //private CTexture[] tx玄人譜面 = new CTexture[2];
    //private CTexture[] tx達人譜面 = new CTexture[2];
    //-----------------
    #endregion
}
