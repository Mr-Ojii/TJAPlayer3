using FDK;

namespace TJAPlayer3;

internal class CActScore : CActivity
{
    // プロパティ

    protected long[] nスコアの増分;
    protected double[] n現在の本当のスコア;
    protected long[] n現在表示中のスコア;
    //protected CTexture txScore;

    //      protected CTexture txScore_1P;
    protected CCounter ctTimer;
    public CCounter[] ct点数アニメタイマ;

    public CCounter[] ctボーナス加算タイマ;

    protected STスコア[] stScore;
    protected int n現在表示中のAddScore;

    [StructLayout(LayoutKind.Sequential)]
    protected struct STスコア
    {
        public bool b使用中;
        public bool b表示中;
        public bool bBonusScore;
        public CCounter ctTimer;
        public int nAddScore;
        public int nPlayer;
    }

    public long GetScore(int player)
    {
        return n現在表示中のスコア[player];
    }

    // コンストラクタ

    public CActScore()
    {
        this.stScore = new STスコア[256];
    }


    // メソッド

    private float[] ScoreScale = new float[]
    {
        1.000f,
        1.111f, // リピート
        1.222f,
        1.185f,
        1.148f,
        1.129f,
        1.111f,
        1.074f,
        1.065f,
        1.033f,
        1.015f,
        1.000f
    };

    public double Get(int player)
    {
        return this.n現在の本当のスコア[player];
    }
    public void Set(double nScore, int player)
    {
        if (this.n現在の本当のスコア[player] != nScore)
        {
            this.n現在の本当のスコア[player] = nScore;
            this.nスコアの増分[player] = (long)(((double)(this.n現在の本当のスコア[player] - this.n現在表示中のスコア[player])) / 20.0);
            if (this.nスコアの増分[player] < 1L)
            {
                this.nスコアの増分[player] = 1L;
            }
        }
    }

    /// <summary>
    /// 点数を加える(各種AUTO補正つき)
    /// </summary>
    /// <param name="part"></param>
    /// <param name="bAutoPlay"></param>
    /// <param name="delta"></param>
    public void Add(long delta, int player)
    {
        double rev = 1.0;

        this.ctTimer = new CCounter(0, 400, 1, TJAPlayer3.app.Timer);

        for (int sc = 0; sc < 1; sc++)
        {
            for (int i = 0; i < 256; i++)
            {
                if (this.stScore[i].b使用中 == false)
                {
                    this.stScore[i].b使用中 = true;
                    this.stScore[i].b表示中 = true;
                    this.stScore[i].nAddScore = (int)delta;
                    this.stScore[i].ctTimer = new CCounter(0, 500, 1, TJAPlayer3.app.Timer);
                    this.stScore[i].bBonusScore = false;
                    this.stScore[i].nPlayer = player;
                    this.n現在表示中のAddScore++;
                    break;
                }
            }
        }

        this.Set(this.Get(player) + delta * rev, player);
    }

    public void BonusAdd(int player)
    {
        for (int sc = 0; sc < 1; sc++)
        {
            for (int i = 0; i < 256; i++)
            {
                if (this.stScore[i].b使用中 == false)
                {
                    this.stScore[i].b使用中 = true;
                    this.stScore[i].b表示中 = true;
                    this.stScore[i].nAddScore = 10000;
                    this.stScore[i].ctTimer = new CCounter(0, 400, 1, TJAPlayer3.app.Timer);
                    this.stScore[i].bBonusScore = true;
                    this.stScore[i].nPlayer = player;
                    this.n現在表示中のAddScore++;
                    break;
                }
            }
        }

        this.Set(this.Get(player) + 10000, player);
    }

    // CActivity 実装

    public override void On活性化()
    {
        this.n現在表示中のスコア = new long[4];
        this.n現在の本当のスコア = new double[4];
        this.nスコアの増分 = new long[4];

        for (int i = 0; i < 4; i++)
        {
            this.n現在表示中のスコア[i] = 0L;
            this.n現在の本当のスコア[i] = 0L;
            this.nスコアの増分[i] = 0L;
        }

        for (int sc = 0; sc < 256; sc++)
        {
            this.stScore[sc].b使用中 = false;
            this.stScore[sc].ctTimer = new CCounter();
            this.stScore[sc].nAddScore = 0;
            this.stScore[sc].bBonusScore = false;
        }

        this.n現在表示中のAddScore = 0;

        this.ctTimer = new CCounter();

        this.ct点数アニメタイマ = new CCounter[4];
        for (int i = 0; i < 4; i++)
        {
            this.ct点数アニメタイマ[i] = new CCounter();
        }
        this.ctボーナス加算タイマ = new CCounter[4];
        for (int i = 0; i < 4; i++)
        {
            this.ctボーナス加算タイマ[i] = new CCounter();
        }
        base.On活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                base.b初めての進行描画 = false;
            }
            if (!this.ctTimer.b停止中)
            {
                this.ctTimer.t進行();
                if (this.ctTimer.b終了値に達した)
                {
                    this.ctTimer.t停止();
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (!this.ct点数アニメタイマ[i].b停止中)
                {
                    this.ct点数アニメタイマ[i].t進行();
                    if (this.ct点数アニメタイマ[i].b終了値に達した)
                    {
                        this.ct点数アニメタイマ[i].t停止();
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (!this.ctボーナス加算タイマ[i].b停止中)
                {
                    this.ctボーナス加算タイマ[i].t進行();
                    if (this.ctボーナス加算タイマ[i].b終了値に達した)
                    {
                        TJAPlayer3.stage演奏ドラム画面.actScore.BonusAdd(i);
                        this.ctボーナス加算タイマ[i].t停止();
                    }
                }
            }

            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                this.t小文字表示(TJAPlayer3.app.Skin.SkinConfig.Game.Score.X[i], TJAPlayer3.app.Skin.SkinConfig.Game.Score.Y[i], string.Format("{0,7:######0}", this.n現在表示中のスコア[i]), 0, 256, i);
            }

            for (int i = 0; i < 256; i++)
            {
                if (this.stScore[i].b使用中)
                {
                    if (!this.stScore[i].ctTimer.b停止中)
                    {
                        this.stScore[i].ctTimer.t進行();
                        if (this.stScore[i].ctTimer.b終了値に達した)
                        {
                            this.n現在表示中のスコア[this.stScore[i].nPlayer] += (long)this.stScore[i].nAddScore;
                            if (this.stScore[i].b表示中 == true)
                                this.n現在表示中のAddScore--;
                            this.stScore[i].ctTimer.t停止();
                            this.stScore[i].b使用中 = false;
                            if (ct点数アニメタイマ[stScore[i].nPlayer].b終了値に達してない)
                            {
                                this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 11, 12, TJAPlayer3.app.Timer);
                                this.ct点数アニメタイマ[stScore[i].nPlayer].n現在の値 = 1;
                            }
                            else
                            {
                                this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 11, 12, TJAPlayer3.app.Timer);
                            }
                            TJAPlayer3.stage演奏ドラム画面.actDan.Update();
                        }

                        int xAdd = 0;
                        int yAdd = 0;
                        int alpha = 0;

                        if (this.stScore[i].ctTimer.n現在の値 < 10)
                        {
                            xAdd = 25;
                            alpha = 150;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 20)
                        {
                            xAdd = 10;
                            alpha = 200;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 30)
                        {
                            xAdd = -5;
                            alpha = 250;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 40)
                        {
                            xAdd = -9;
                            alpha = 256;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 50)
                        {
                            xAdd = -10;
                            alpha = 256;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 60)
                        {
                            xAdd = -9;
                            alpha = 256;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 70)
                        {
                            xAdd = -5;
                            alpha = 256;
                        }
                        else if (this.stScore[i].ctTimer.n現在の値 < 80)
                        {
                            xAdd = -3;
                            alpha = 256;
                        }
                        else
                        {
                            xAdd = 0;
                            alpha = 256;
                        }



                        if (this.stScore[i].ctTimer.n現在の値 > 300)
                        {
                            yAdd = -1;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 310)
                        {
                            yAdd = -5;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 320)
                        {
                            yAdd = -7;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 330)
                        {
                            yAdd = -8;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 340)
                        {
                            yAdd = -8;
                            alpha = 256;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 350)
                        {
                            yAdd = -6;
                            alpha = 256;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 360)
                        {
                            yAdd = 0;
                            alpha = 256;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 370)
                        {
                            yAdd = 5;
                            alpha = 200;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 380)
                        {
                            yAdd = 12;
                            alpha = 150;
                        }
                        if (this.stScore[i].ctTimer.n現在の値 > 390)
                        {
                            yAdd = 20;
                            alpha = 0;
                        }


                        if (this.n現在表示中のAddScore < 10 && this.stScore[i].bBonusScore == false)
                            this.t小文字表示(TJAPlayer3.app.Skin.SkinConfig.Game.Score.AddX[this.stScore[i].nPlayer] + xAdd, this.stScore[i].nPlayer == 0 ? TJAPlayer3.app.Skin.SkinConfig.Game.Score.AddY[this.stScore[i].nPlayer] + yAdd : TJAPlayer3.app.Skin.SkinConfig.Game.Score.AddY[this.stScore[i].nPlayer] - yAdd, string.Format("{0,7:######0}", this.stScore[i].nAddScore), this.stScore[i].nPlayer + 1, alpha, stScore[i].nPlayer);
                        if (this.n現在表示中のAddScore < 10 && this.stScore[i].bBonusScore == true)
                            this.t小文字表示(TJAPlayer3.app.Skin.SkinConfig.Game.Score.AddBonusX[this.stScore[i].nPlayer] + xAdd, TJAPlayer3.app.Skin.SkinConfig.Game.Score.AddBonusY[this.stScore[i].nPlayer], string.Format("{0,7:######0}", this.stScore[i].nAddScore), this.stScore[i].nPlayer + 1, alpha, stScore[i].nPlayer);
                        else
                        {
                            this.n現在表示中のAddScore--;
                            this.stScore[i].b表示中 = false;
                        }
                    }
                }
            }
        }
        return 0;
    }

    protected void t小文字表示(int x, int y, string str, int mode, int alpha, int player)
    {
        foreach (char ch in str)
        {
            if (int.TryParse(ch.ToString(), out var i))
            {
                Rectangle rectangle = new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[0] * i, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[0], TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[1]);
                switch (mode)
                {
                    case 0:
                        {
                            CTexture? tx_score = TJAPlayer3.app.Tx.Taiko_Score[0];
                            if (tx_score is not null)
                            {
                                tx_score.Opacity = alpha;
                                tx_score.vcScaling.Y = ScoreScale[this.ct点数アニメタイマ[player].n現在の値];
                                tx_score.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, x, y, rectangle);
                            }
                            break;
                        }
                    case 1:
                        {
                            CTexture? tx_score = TJAPlayer3.app.Tx.Taiko_Score[1];
                            if (tx_score is not null)
                            {
                                tx_score.Opacity = alpha;
                                tx_score.vcScaling.Y = 1;
                                tx_score.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, x, y, rectangle);
                            }
                            break;
                        }
                    case 2:
                        {
                            CTexture? tx_score = TJAPlayer3.app.Tx.Taiko_Score[2];
                            if (tx_score is not null)
                            {
                                tx_score.Opacity = alpha;
                                tx_score.vcScaling.Y = 1;
                                tx_score.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, x, y, rectangle);
                            }
                            break;
                        }
                }
                x += TJAPlayer3.app.Skin.SkinConfig.Game.Score.Padding;
            }
        }
    }
}
