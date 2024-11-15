using FDK;

namespace TJAPlayer3;

internal class CAct演奏DrumsチップファイアD : CActivity
{
    // コンストラクタ

    public CAct演奏DrumsチップファイアD()
    {
    }


    // メソッド

    public virtual void Start(int nLane, EJudge judge, int player)
    {
        for (int j = 0; j < 3 * 4; j++)
        {
            if (!this.st状態[j].b使用中)
            //for( int n = 0; n < 1; n++ )
            {
                this.st状態[j].b使用中 = true;
                //this.st状態[ n ].ct進行 = new CCounter( 0, 9, 20, CDTXMania.Timer );
                this.st状態[j].ct進行 = new CCounter(0, 6, 25, TJAPlayer3.app.Timer);
                this.st状態[j].judge = judge;
                this.st状態[j].nPlayer = player;
                this.st状態_大[j].nPlayer = player;

                switch (nLane)
                {
                    case 0x11:
                    case 0x12:
                        this.st状態[j].nIsBig = 0;
                        break;
                    case 0x13:
                    case 0x14:
                    case 0x1A:
                    case 0x1B:
                        this.st状態_大[j].ct進行 = new CCounter(0, 9, 20, TJAPlayer3.app.Timer);
                        this.st状態_大[j].judge = judge;
                        this.st状態_大[j].nIsBig = 1;
                        break;
                }
                break;
            }
        }
    }

    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 3 * 4; i++)
        {
            this.st状態[i].ct進行 = new CCounter();
            this.st状態[i].b使用中 = false;
            this.st状態_大[i].ct進行 = new CCounter();
        }
        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 3 * 4; i++)
        {
            this.st状態[i].ct進行 = null;
            this.st状態_大[i].ct進行 = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            for (int i = 0; i < 3 * 4; i++)
            {
                if (this.st状態[i].b使用中)
                {
                    if (!this.st状態[i].ct進行.b停止中)
                    {
                        this.st状態[i].ct進行.t進行();
                        if (this.st状態[i].ct進行.b終了値に達した)
                        {
                            this.st状態[i].ct進行.t停止();
                            this.st状態[i].b使用中 = false;
                        }

                        // (When performing calibration, reduce visual distraction
                        // and current judgment feedback near the judgment position.)
                        if (TJAPlayer3.app.Tx.Effects_Hit_Explosion is not null && !TJAPlayer3.IsPerformingCalibration)
                        {
                            int n = this.st状態[i].nIsBig == 1 ? 520 : 0;
                            int nX = (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態[i].nPlayer]) - ((TJAPlayer3.app.Tx.Effects_Hit_Explosion.szTextureSize.Width / 7) / 2);
                            int nY = (TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[this.st状態[i].nPlayer]) - ((TJAPlayer3.app.Tx.Effects_Hit_Explosion.szTextureSize.Height / 4) / 2);

                            switch (st状態[i].judge)
                            {
                                case EJudge.Perfect:
                                case EJudge.AutoPerfect:
                                    if (!this.st状態_大[i].ct進行.b停止中 && TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big is not null && this.st状態_大[i].nIsBig == 1)
                                        TJAPlayer3.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n + 520, 260, 260));
                                    else
                                        TJAPlayer3.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n, 260, 260));
                                    break;
                                case EJudge.Good:
                                    if (!this.st状態_大[i].ct進行.b停止中 && TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big is not null && this.st状態_大[i].nIsBig == 1)
                                        TJAPlayer3.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n + 780, 260, 260));
                                    else
                                        TJAPlayer3.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayer3.app.Device, nX, nY, new Rectangle(this.st状態[i].ct進行.n現在の値 * 260, n + 260, 260, 260));
                                    break;
                                case EJudge.Miss:
                                case EJudge.Bad:
                                    break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < 3 * 4; i++)
            {
                if (!this.st状態_大[i].ct進行.b停止中)
                {
                    this.st状態_大[i].ct進行.t進行();
                    if (this.st状態_大[i].ct進行.b終了値に達した)
                    {
                        this.st状態_大[i].ct進行.t停止();
                    }
                    if (TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big is not null && this.st状態_大[i].nIsBig == 1)
                    {

                        switch (st状態_大[i].judge)
                        {
                            case EJudge.Perfect:
                            case EJudge.AutoPerfect:
                                if (this.st状態_大[i].nIsBig == 1)
                                {
                                    float f倍率 = 0.5f + ((this.st状態_大[i].ct進行.n現在の値 * 0.5f) / 10.0f);

                                    TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big.Opacity = 255;
                                    TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big.vcScaling = new Vector2(f倍率);
                                    TJAPlayer3.app.Tx.Effects_Hit_Explosion_Big.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態_大[i].nPlayer], TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[this.st状態[i].nPlayer]);
                                }
                                break;

                            case EJudge.Good:
                                break;

                            case EJudge.Miss:
                            case EJudge.Bad:
                                break;
                        }
                    }
                }
            }
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    protected STSTATUS[] st状態 = new STSTATUS[3 * 4];
    protected STSTATUS_B[] st状態_大 = new STSTATUS_B[3 * 4];

    protected int[] nX座標 = new int[] { 450, 521, 596, 686, 778, 863, 970, 1070, 1150 };
    protected int[] nY座標 = new int[] { 172, 108, 50, 8, -10, -60, -5, 30, 90 };
    protected int[] nY座標P2 = new int[] { 172, 108, 50, 8, -10, -60, -5, 30, 90 };

    [StructLayout(LayoutKind.Sequential)]
    protected struct STSTATUS
    {
        public bool b使用中;
        public CCounter ct進行;
        public EJudge judge;
        public int nIsBig;
        public int n透明度;
        public int nPlayer;
    }
    [StructLayout(LayoutKind.Sequential)]
    protected struct STSTATUS_B
    {
        public CCounter ct進行;
        public EJudge judge;
        public int nIsBig;
        public int n透明度;
        public int nPlayer;
    }
    //-----------------
    #endregion
}
