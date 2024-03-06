using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drums連打キャラ : CActivity
{
    // コンストラクタ

    public CAct演奏Drums連打キャラ()
    {
    }


    // メソッド
    public virtual void Start(int player)
    {
        for (int i = 0; i < 128; i++)
        {
            if (!RollCharas[i].IsUsing)
            {
                RollCharas[i].IsUsing = true;
                RollCharas[i].Type = random.Next(0, TJAPlayer3.app.Skin.Game_Effect_Roll_Ptn);
                RollCharas[i].OldValue = 0;
                RollCharas[i].Counter = new CCounter(0, 5000, 1, TJAPlayer3.app.Timer);
                if (TJAPlayer3.stage演奏ドラム画面.bDoublePlay)
                {
                    RollCharas[i].X = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointMultiX[player][random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointMultiX[player].Length)];
                    RollCharas[i].Y = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointMultiY[player][random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointMultiY[player].Length)];
                    RollCharas[i].XAdd = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedMultiX[player][random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedMultiX[player].Length)];
                    RollCharas[i].YAdd = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedMultiY[player][random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedMultiY[player].Length)];
                }
                else
                {
                    RollCharas[i].X = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointX[random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointX.Length)];
                    RollCharas[i].Y = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointY[random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.StartPointY.Length)];
                    RollCharas[i].XAdd = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedX[random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedX.Length)];
                    RollCharas[i].YAdd = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedY[random.Next(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Roll.SpeedY.Length)];
                }
                break;
            }
        }

    }

    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            RollCharas[i] = new RollChara();
            RollCharas[i].IsUsing = false;
            RollCharas[i].Counter = new CCounter();
        }
        // SkinConfigで指定されたいくつかの変数からこのクラスに合ったものに変換していく

        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            RollCharas[i].Counter = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            for (int i = 0; i < 128; i++)
            {
                if (RollCharas[i].IsUsing)
                {
                    RollCharas[i].OldValue = RollCharas[i].Counter.n現在の値;
                    RollCharas[i].Counter.t進行();
                    if (RollCharas[i].Counter.b終了値に達した)
                    {
                        RollCharas[i].Counter.t停止();
                        RollCharas[i].IsUsing = false;
                    }
                    for (int l = RollCharas[i].OldValue; l < RollCharas[i].Counter.n現在の値; l++)
                    {
                        RollCharas[i].X += RollCharas[i].XAdd;
                        RollCharas[i].Y += RollCharas[i].YAdd;
                    }
                    if (RollCharas[i].Type < TJAPlayer3.app.Tx.Effects_Roll.Length)
                    {
                        TJAPlayer3.app.Tx.Effects_Roll[RollCharas[i].Type]?.t2D描画(TJAPlayer3.app.Device, RollCharas[i].X, RollCharas[i].Y);
                        // 画面外にいたら描画をやめさせる
                        if (RollCharas[i].X < 0 - TJAPlayer3.app.Tx.Effects_Roll[RollCharas[i].Type].szTextureSize.Width || RollCharas[i].X > TJAPlayer3.app.LogicalSize.Width)
                        {
                            RollCharas[i].Counter.t停止();
                            RollCharas[i].IsUsing = false;
                        }
                        if (RollCharas[i].Y < 0 - TJAPlayer3.app.Tx.Effects_Roll[RollCharas[i].Type].szTextureSize.Height || RollCharas[i].Y > TJAPlayer3.app.LogicalSize.Height)
                        {
                            RollCharas[i].Counter.t停止();
                            RollCharas[i].IsUsing = false;
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
    //private CTexture[] txChara;

    [StructLayout(LayoutKind.Sequential)]
    private struct RollChara
    {
        public CCounter Counter;
        public int Type;
        public bool IsUsing;
        public float X;
        public float Y;
        public float XAdd;
        public float YAdd;
        public int OldValue;
    }

    private RollChara[] RollCharas = new RollChara[128];

    private Random random = new Random();
    //-----------------
    #endregion
}
