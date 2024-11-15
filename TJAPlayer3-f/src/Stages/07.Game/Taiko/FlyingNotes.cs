using FDK;

namespace TJAPlayer3;

internal class FlyingNotes : CActivity
{
    // コンストラクタ

    public FlyingNotes()
    {
    }


    // メソッド
    public virtual void Start(int nLane, int nPlayer, bool isRoll = false)
    {
        if (TJAPlayer3.app.Tx.Notes is null)
            return;

        for (int i = 0; i < 128; i++)
        {
            if (!Flying[i].IsUsing)
            {
                // 初期化
                Flying[i].IsUsing = true;
                Flying[i].Lane = nLane;
                Flying[i].Player = nPlayer;
                Flying[i].X = StartPointX[nPlayer];
                Flying[i].Y = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer];
                Flying[i].StartPointX = StartPointX[nPlayer];
                Flying[i].StartPointY = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer];
                Flying[i].OldValue = 0;
                Flying[i].IsRoll = isRoll;
                // 角度の決定
                Flying[i].Height = Math.Abs(TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer] - TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer]);
                Flying[i].Width = Math.Abs((TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer] - StartPointX[nPlayer])) / 2;
                //Console.WriteLine("{0}, {1}", width2P, height2P);
                Flying[i].Counter = new CCounter(0, (180), TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.Timer, TJAPlayer3.app.Timer);
                //Flying[i].Counter = new CCounter(0, 200000, CDTXMania.Skin.Game_Effect_FlyingNotes_Timer, CDTXMania.Timer);

                Flying[i].IncreaseX = (1.00 * Math.Abs((TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer] - StartPointX[nPlayer]))) / (180);
                Flying[i].IncreaseY = (1.00 * Math.Abs((TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer] - TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer]))) / (180);
                break;
            }
        }
    }

    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            Flying[i] = new Status();
            Flying[i].IsUsing = false;
            Flying[i].Counter = new CCounter();
        }
        for (int i = 0; i < 2; i++)
        {
            StartPointX[i] = TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointX[i];
        }
        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            Flying[i].Counter = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            for (int i = 0; i < 128; i++)
            {
                if (Flying[i].IsUsing)
                {
                    Flying[i].OldValue = Flying[i].Counter.n現在の値;
                    Flying[i].Counter.t進行();
                    if (Flying[i].Counter.b終了値に達した)
                    {
                        Flying[i].Counter.t停止();
                        Flying[i].IsUsing = false;
                        TJAPlayer3.stage演奏ドラム画面.actChipEffects.Start(Flying[i].Player, Flying[i].Lane);
                    }
                    for (int n = Flying[i].OldValue; n < Flying[i].Counter.n現在の値; n++)
                    {
                        if (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.IsUsingEasing)
                        {
                            Flying[i].X = Flying[i].StartPointX + Flying[i].Width + ((-Math.Cos(Flying[i].Counter.n現在の値 * (Math.PI / 180)) * Flying[i].Width));
                        }
                        else
                        {
                            Flying[i].X += Flying[i].IncreaseX;
                        }

                        if (n % TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FireWorks.Timing == 0 && !Flying[i].IsRoll && Flying[i].Counter.n現在の値 > 18)
                        {
                            if (Flying[i].Lane == 3 || Flying[i].Lane == 4)
                            {
                                TJAPlayer3.stage演奏ドラム画面.FireWorks.Start(Flying[i].Lane, Flying[i].Player, Flying[i].X, Flying[i].Y);
                            }
                        }


                        if (Flying[i].Player == 0)
                        {
                            Flying[i].Y = (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[Flying[i].Player]) - Math.Sin(Flying[i].Counter.n現在の値 * (Math.PI / 180)) * TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.Sine;
                            Flying[i].Y -= Flying[i].IncreaseY * Flying[i].Counter.n現在の値;
                        }
                        else
                        {
                            Flying[i].Y = (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[Flying[i].Player]) + Math.Sin(Flying[i].Counter.n現在の値 * (Math.PI / 180)) * TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.Sine;
                            Flying[i].Y += Flying[i].IncreaseY * Flying[i].Counter.n現在の値;
                        }

                    }

                    TJAPlayer3.app.Tx.Notes?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, (int)Flying[i].X, (int)Flying[i].Y, new Rectangle(Flying[i].Lane * 130, 0, 130, 130));

                }
            }
        }
        return base.On進行描画();
    }


    #region [ private ]
    //-----------------

    [StructLayout(LayoutKind.Sequential)]
    private struct Status
    {
        public int Lane;
        public int Player;
        public bool IsUsing;
        public CCounter Counter;
        public int OldValue;
        public double X;
        public double Y;
        public int Height;
        public int Width;
        public double IncreaseX;
        public double IncreaseY;
        public bool IsRoll;
        public int StartPointX;
        public int StartPointY;
    }

    private Status[] Flying = new Status[128];

    public readonly int[] StartPointX = new int[2];

    //-----------------
    #endregion
}
