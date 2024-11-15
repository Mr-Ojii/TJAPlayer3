using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drums風船 : CActivity
{


    public CAct演奏Drums風船()
    {
    }

    public override void On活性化()
    {
        this.ct風船ふきだしアニメ = new CCounter();
        this.ct風船アニメ = new CCounter[4];
        for (int i = 0; i < 4; i++)
        {
            this.ct風船アニメ[i] = new CCounter();
        }

        this.ct風船ふきだしアニメ = new CCounter(0, 1, 100, TJAPlayer3.app.Timer);
        base.On活性化();
    }

    public override void On非活性化()
    {
        this.ct風船ふきだしアニメ = null;
        base.On非活性化();
    }

    public override int On進行描画()
    {
        return base.On進行描画();
    }

    public int On進行描画(int n連打ノルマ, int n連打数, int nPlayer)
    {
        this.ct風船ふきだしアニメ.t進行Loop();
        this.ct風船アニメ[nPlayer].t進行();

        //CDTXMania.act文字コンソール.tPrint( 0, 16, C文字コンソール.EFontType.赤, this.ct風船終了.n現在の値.ToString() );
        int[] n残り打数 = new int[] { 0, 0, 0, 0, 0 };
        #region[  ]
        if (n連打ノルマ > 0)
        {
            if (n連打ノルマ < 5)
            {
                n残り打数 = new int[] { 4, 3, 2, 1, 0 };
            }
            else
            {
                n残り打数[0] = (n連打ノルマ / 5) * 4;
                n残り打数[1] = (n連打ノルマ / 5) * 3;
                n残り打数[2] = (n連打ノルマ / 5) * 2;
                n残り打数[3] = (n連打ノルマ / 5) * 1;
            }
        }
        #endregion

        if (n連打数 != 0)
        {
            for (int j = 0; j < 5; j++)
            {
                if (n残り打数[j] < n連打数)
                {
                    if (TJAPlayer3.app.Tx.Balloon_Breaking[j] is not null)
                        TJAPlayer3.app.Tx.Balloon_Breaking[j].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonX[nPlayer] + (this.ct風船ふきだしアニメ.n現在の値 == 1 ? 3 : 0) + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - 414, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonY[nPlayer]);
                    break;
                }
            }
            //1P:31 2P:329
            if (TJAPlayer3.app.Tx.Balloon_Balloon is not null)
                TJAPlayer3.app.Tx.Balloon_Balloon.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonFrameX[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - 414, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonFrameY[nPlayer]);
            this.t文字表示(TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonNumberX[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - 414, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonNumberY[nPlayer], n連打数, nPlayer);
            //CDTXMania.act文字コンソール.tPrint( 0, 0, C文字コンソール.EFontType.白, n連打数.ToString() );
        }
        if (n連打数 == 0 && TJAPlayer3.stage演奏ドラム画面.actChara.b風船連打中[nPlayer])
        {
            TJAPlayer3.stage演奏ドラム画面.actChara.b風船連打中[nPlayer] = false;
            TJAPlayer3.stage演奏ドラム画面.b連打中[nPlayer] = false;
        }
        return base.On進行描画();
    }



    //private CTexture tx連打枠;
    //private CTexture tx連打数字;

    //private CTexture txキャラクター;
    //private CTexture txキャラクター_風船終了;

    //private CTexture[] tx風船枠 = new CTexture[6];

    private CCounter ct風船ふきだしアニメ;

    public CCounter[] ct風船アニメ;
    private float[] RollScale = new float[]
    {
        0.000f,
        0.123f, // リピート
        0.164f,
        0.164f,
        0.164f,
        0.137f,
        0.110f,
        0.082f,
        0.055f,
        0.000f
    };

    private void t文字表示(int x, int y, int n連打, int nPlayer)
    {
        int n桁数 = n連打.ToString().Length;

        for (int index = n連打.ToString().Length - 1; index >= 0; index--)
        {
            int i = (int)(n連打 / Math.Pow(10, index) % 10);
            Rectangle rectangle = new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.NumberSize[0] * i, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.NumberSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.NumberSize[1]);

            if (TJAPlayer3.app.Tx.Balloon_Number_Roll is not null)
            {
                TJAPlayer3.app.Tx.Balloon_Number_Roll.Opacity = 255;
                TJAPlayer3.app.Tx.Balloon_Number_Roll.vcScaling.X = TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonNumberScale;
                TJAPlayer3.app.Tx.Balloon_Number_Roll.vcScaling.Y = TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.BalloonNumberScale + RollScale[this.ct風船アニメ[nPlayer].n現在の値];
                TJAPlayer3.app.Tx.Balloon_Number_Roll.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, x - (((TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.NumberPadding + 2) * n桁数) / 2), y, rectangle);
            }
            x += (TJAPlayer3.app.Skin.SkinConfig.Game.Balloon.NumberPadding - (n桁数 > 2 ? n桁数 * 2 : 0));
        }
    }
}
