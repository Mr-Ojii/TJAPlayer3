using FDK;

namespace TJAPlayer3;

internal class CActRunner : CActivity
{
    /// <summary>
    /// ランナー
    /// </summary>
    public CActRunner()
    {
    }

    public void Start(int Player, bool IsMiss, CDTX.CChip pChip)
    {
        if (TJAPlayer3.app.Tx.Runner is null)
            return;

        if (pChip.nチャンネル番号 < 0x15 || (pChip.nチャンネル番号 >= 0x1A))
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stRunners[i].b使用中)
                {
                    stRunners[i].b使用中 = true;
                    stRunners[i].nPlayer = Player;

                    if (IsMiss)
                        stRunners[i].nType = 0;
                    else
                        stRunners[i].nType = random.Next(1, Type + 1);

                    stRunners[i].ct進行 = new CCounter(0, TJAPlayer3.app.LogicalSize.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Runner.Timer, TJAPlayer3.app.Timer);
                    stRunners[i].nOldValue = 0;
                    stRunners[i].nNowPtn = 0;
                    stRunners[i].fX = 0;
                    break;
                }
            }
        }
    }

    public override void On活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            stRunners[i] = new STRunner();
            stRunners[i].b使用中 = false;
            stRunners[i].ct進行 = new CCounter();
        }

        // フィールド上で代入してたためこちらへ移動。
        Size = TJAPlayer3.app.Skin.SkinConfig.Game.Runner.Size;
        Ptn = TJAPlayer3.app.Skin.SkinConfig.Game.Runner.Ptn;
        Type = TJAPlayer3.app.Skin.SkinConfig.Game.Runner.Type;
        StartPoint_X = TJAPlayer3.app.Skin.SkinConfig.Game.Runner.StartPointX;
        StartPoint_Y = TJAPlayer3.app.Skin.SkinConfig.Game.Runner.StartPointY;
        base.On活性化();
    }

    public override void On非活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            stRunners[i].ct進行 = null;
        }
        base.On非活性化();
    }

    public override int On進行描画()
    {
        for (int i = 0; i < 128; i++)
        {
            if (stRunners[i].b使用中)
            {
                stRunners[i].nOldValue = stRunners[i].ct進行.n現在の値;
                stRunners[i].ct進行.t進行();
                if (stRunners[i].ct進行.b終了値に達した || stRunners[i].fX > TJAPlayer3.app.LogicalSize.Width)
                {
                    stRunners[i].ct進行.t停止();
                    stRunners[i].b使用中 = false;
                }
                for (int n = stRunners[i].nOldValue; n < stRunners[i].ct進行.n現在の値; n++)
                {
                    //AkasokoPullyou様のソースコードを参考にして、ランナーの逆流を防止
                    double dbBPM = Math.Abs(TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM);
                    stRunners[i].fX += (float)dbBPM / 18;
                    int Width = TJAPlayer3.app.LogicalSize.Width / Ptn;
                    stRunners[i].nNowPtn = (int)stRunners[i].fX / Width;
                }
                TJAPlayer3.app.Tx.Runner?.t2D描画(TJAPlayer3.app.Device, (int)(StartPoint_X[stRunners[i].nPlayer] + stRunners[i].fX), StartPoint_Y[stRunners[i].nPlayer], new Rectangle(stRunners[i].nNowPtn * Size[0], stRunners[i].nType * Size[1], Size[0], Size[1]));
            }
        }
        return base.On進行描画();
    }

    #region[ private ]
    //-----------------
    [StructLayout(LayoutKind.Sequential)]
    private struct STRunner
    {
        public bool b使用中;
        public int nPlayer;
        public int nType;
        public int nOldValue;
        public int nNowPtn;
        public float fX;
        public CCounter ct進行;
    }
    private STRunner[] stRunners = new STRunner[128];
    Random random = new Random();

    // ランナー画像のサイズ。 X, Y
    private int[] Size;
    // ランナーのコマ数
    private int Ptn;
    // ランナーのキャラクターのバリエーション(ミス時を含まない)。
    private int Type;
    // スタート地点のX座標 1P, 2P
    private int[] StartPoint_X;
    // スタート地点のY座標 1P, 2P
    private int[] StartPoint_Y;
    //-----------------
    #endregion
}
