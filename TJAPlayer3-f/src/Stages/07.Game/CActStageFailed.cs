using FDK;

namespace TJAPlayer3;

internal class CActStageFailed : CActivity
{
    // コンストラクタ

    public CActStageFailed()
    {
    }


    // メソッド

    public void Start()
    {
        this.dbFailedTime = TJAPlayer3.app.Timer.n現在時刻ms;
        this.ct進行 = new CCounter(0, 1000, 2, TJAPlayer3.app.Timer);
        if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.OFF)
        {
            this.ct進行 = new CCounter(0, 4000, 2, TJAPlayer3.app.Timer);
        }
    }


    // CActivity 実装

    public override void On活性化()
    {
        this.b効果音再生済み = false;
        this.ct進行 = new CCounter();
        base.On活性化();
    }
    public override void On非活性化()
    {
        this.ct進行 = null;
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (base.b活性化してない)
        {
            return 0;
        }
        if ((this.ct進行 is null) || this.ct進行.b停止中)
        {
            return 0;
        }
        this.ct進行.t進行();

        if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー || TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー激辛)
        {
            if (TJAPlayer3.app.Tx.Tile_Black is not null)
            {
                for (int i = 0; i <= (TJAPlayer3.app.LogicalSize.Width / 64); i++)
                {
                    for (int j = 0; j <= (TJAPlayer3.app.LogicalSize.Height / 64); j++)
                    {
                        TJAPlayer3.app.Tx.Tile_Black.t2D描画(TJAPlayer3.app.Device, i * 64, j * 64);
                    }
                }
            }
            if (this.ct進行.n現在の値 > 1500)
            {
                if (TJAPlayer3.app.Tx.Failed_Game is not null)
                    TJAPlayer3.app.Tx.Failed_Game.t2D描画(TJAPlayer3.app.Device, 0, 0);

                int num = (TJAPlayer3.DTX[0].listChip.Count > 0) ? TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms : 0;
                this.t文字表示(640, 520, (((this.dbFailedTime) / 1000.0) / (((double)num) / 1000.0) * 100).ToString("##0") + "%");
            }


        }
        else
        {
            if (this.ct進行.n現在の値 < 100)
            {
                int x = (int)(640.0 * Math.Cos((Math.PI / 2 * this.ct進行.n現在の値) / 100.0));
                if ((x != 640) && (TJAPlayer3.app.Tx.Failed_Stage is not null))
                {
                    TJAPlayer3.app.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 0, 0, new Rectangle(x, 0, 640 - x, 720));
                    TJAPlayer3.app.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 640 + x, 0, new Rectangle(640, 0, 640 - x, 720));
                }
            }
            else
            {
                if (TJAPlayer3.app.Tx.Failed_Stage is not null)
                {
                    TJAPlayer3.app.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 0, 0);
                }
                if (this.ct進行.n現在の値 <= 250)
                {
                    int num2 = Random.Shared.Next(5) - 2;
                    int y = Random.Shared.Next(5) - 2;
                    if (TJAPlayer3.app.Tx.Failed_Stage is not null)
                    {
                        TJAPlayer3.app.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, num2, y);
                    }
                }
                if (!this.b効果音再生済み)
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDステージ失敗音].t再生する();
                    this.b効果音再生済み = true;
                }
            }
        }

        if (!this.ct進行.b終了値に達した)
        {
            return 0;
        }
        return 1;
    }


    // その他

    #region [ private ]
    //-----------------
    private bool b効果音再生済み;
    private CCounter? ct進行;
    private double dbFailedTime;
    //-----------------
    private readonly FrozenDictionary<char, Point> st文字位置 = new Dictionary<char, Point>()
    {
        {'0', new Point(0, 0)},
        {'1', new Point(62, 0)},
        {'2', new Point(124, 0)},
        {'3', new Point(186, 0)},
        {'4', new Point(248, 0)},
        {'5', new Point(310, 0)},
        {'6', new Point(372, 0)},
        {'7', new Point(434, 0)},
        {'8', new Point(496, 0)},
        {'9', new Point(558, 0)},
        {'%', new Point(558 + 62, 0)},
    }.ToFrozenDictionary();

    private void t文字表示(int x, int y, string str)
    {
        //描画するテクスチャがないなら、以後の計算は無駄
        if (TJAPlayer3.app.Tx.Balloon_Number_Roll is null)
            return;

        foreach (char ch in str)
        {
            if (this.st文字位置.TryGetValue(ch, out var pt))
            {
                Rectangle rectangle = new Rectangle(pt.X, pt.Y, 62, 80);
                if (ch == '%')
                    rectangle.Width = 80;
                TJAPlayer3.app.Tx.Balloon_Number_Roll.t2D描画(TJAPlayer3.app.Device, x - (62 * str.Length / 2), y, rectangle);
            }
            x += 62;
        }
    }


    #endregion
}
