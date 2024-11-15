using FDK;

namespace TJAPlayer3;

internal class CActJudgeString : CActivity
{
    // プロパティ

    private STSTATUS[] st状態 = new STSTATUS[12];
    [StructLayout(LayoutKind.Sequential)]
    private struct STSTATUS
    {
        public bool b使用中;
        public CCounter ct進行;
        public EJudge judge;
        public int n相対X座標;
        public int n相対Y座標;
        public int n透明度;
        public int nPlayer;                             // 2017.08.15 kairera0467
    }

    private readonly Rectangle[] st判定文字列;

    // コンストラクタ

    public CActJudgeString()
    {
        this.st判定文字列 = new Rectangle[] {
            new Rectangle( 0, 0,    90, 60 ),		// Perfect
            new Rectangle( 0, 60,   90, 60 ),		// Good
            new Rectangle( 0, 120,   90, 60 ),		// Bad
            new Rectangle( 0, 120,   90, 60 ),		// Miss
            new Rectangle( 0, 0,    90, 60 )		// Auto
        };
    }


    // メソッド

    public void Start(EJudge judge, int lag, CDTX.CChip pChip, int player)
    {
        // When performing calibration, reduce visual distraction
        // and current judgment feedback near the judgment position.
        if (TJAPlayer3.IsPerformingCalibration)
        {
            return;
        }

        if (pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x19)
        {
            return;
        }


        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (this.st状態[j].b使用中 == false)
                {
                    this.st状態[j].ct進行 = new CCounter(0, 300, 1, TJAPlayer3.app.Timer);
                    this.st状態[j].b使用中 = true;
                    this.st状態[j].judge = judge;
                    this.st状態[j].n相対X座標 = 0;
                    this.st状態[j].n相対Y座標 = 0;
                    this.st状態[j].n透明度 = 0xff;
                    this.st状態[j].nPlayer = player;
                    break;
                }

            }
        }


    }


    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 12; i++)
        {
            this.st状態[i].ct進行 = new CCounter();
            this.st状態[i].b使用中 = false;
        }
        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 12; i++)
        {
            this.st状態[i].ct進行 = null;
        }
        base.On非活性化();
    }

    public int t進行描画()
    {
        for (int i = 0; i < 12; i++)
        {
            if (!this.st状態[i].ct進行.b停止中)
            {
                this.st状態[i].ct進行.t進行();
                if (this.st状態[i].ct進行.b終了値に達した)
                {
                    this.st状態[i].ct進行.t停止();
                    this.st状態[i].b使用中 = false;
                }
                int num2 = this.st状態[i].ct進行.n現在の値;
                if (this.st状態[i].judge != EJudge.Good)
                {
                    this.st状態[i].n相対X座標 = 0;
                    this.st状態[i].n相対Y座標 = 15;
                    this.st状態[i].n透明度 = 0xff;
                }
                if ((this.st状態[i].judge != EJudge.Miss) && (this.st状態[i].judge != EJudge.Bad))
                {
                    if (num2 < 20)
                    {
                        this.st状態[i].n相対X座標 = 0;
                        this.st状態[i].n相対Y座標 = 0;
                        this.st状態[i].n透明度 = 0xff;
                    }
                    else if (num2 < 40)
                    {
                        this.st状態[i].n相対X座標 = 0;
                        this.st状態[i].n相対Y座標 = 5;
                        this.st状態[i].n透明度 = 0xff;
                    }
                    else if (num2 >= 60)
                    {
                        this.st状態[i].n相対X座標 = 0;
                        this.st状態[i].n相対Y座標 = 10;
                        this.st状態[i].n透明度 = 0xff;
                    }
                    else
                    {
                        this.st状態[i].n相対X座標 = 0;
                        this.st状態[i].n相対Y座標 = 15;
                        this.st状態[i].n透明度 = 0xff;
                    }
                }
                if (num2 < 20)
                {
                    this.st状態[i].n相対X座標 = 0;
                    this.st状態[i].n相対Y座標 = 0;
                    this.st状態[i].n透明度 = 0xff;
                }
                else if (num2 < 40)
                {
                    this.st状態[i].n相対X座標 = 0;
                    this.st状態[i].n相対Y座標 = 5;
                    this.st状態[i].n透明度 = 0xff;
                }
                else if (num2 >= 60)
                {
                    this.st状態[i].n相対X座標 = 0;
                    this.st状態[i].n相対Y座標 = 10;
                    this.st状態[i].n透明度 = 0xff;
                }
                else
                {
                    this.st状態[i].n相対X座標 = 0;
                    this.st状態[i].n相対Y座標 = 15;
                    this.st状態[i].n透明度 = 0xff;
                }
            }
        }
        for (int j = 0; j < 12; j++)
        {
            if (!this.st状態[j].ct進行.b停止中 && TJAPlayer3.app.Tx.Judge is not null)
            {
                int baseY = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[this.st状態[j].nPlayer] - 53;
                int x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態[j].nPlayer] - TJAPlayer3.app.Tx.Judge.szTextureSize.Width / 2;
                int y = (baseY + this.st状態[j].n相対Y座標);
                TJAPlayer3.app.Tx.Judge.t2D描画(TJAPlayer3.app.Device, x, y, this.st判定文字列[(int)this.st状態[j].judge]);
            }
        }
        return 0;
    }
}
