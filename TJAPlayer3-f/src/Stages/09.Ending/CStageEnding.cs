using FDK;

namespace TJAPlayer3;

internal class CStageEnding : CStage
{
    // コンストラクタ

    public CStageEnding()
    {
        base.eStageID = CStage.EStage.Ending;
        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
    }


    // CStage 実装

    public override void On活性化()
    {
        Trace.TraceInformation("終了ステージを活性化します。");
        Trace.Indent();
        try
        {
            this.ctAnimation = new CCounter(0, 3000, 1, TJAPlayer3.app.Timer);
            TJAPlayer3.app.Discord.Update("Ending");
            base.On活性化();
        }
        finally
        {
            Trace.TraceInformation("終了ステージの活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override void On非活性化()
    {
        Trace.TraceInformation("終了ステージを非活性化します。");
        Trace.Indent();
        try
        {
            base.On非活性化();
        }
        finally
        {
            Trace.TraceInformation("終了ステージの非活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override int On進行描画()
    {
        if ((EEndingAnime)TJAPlayer3.app.ConfigToml.Ending.EndingAnime == EEndingAnime.Off || (this.ctAnimation is null) || (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))) //2017.01.27 DD
        {
            return 1;
        }
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDゲーム終了音].t再生する();
                base.b初めての進行描画 = false;
            }

            this.ctAnimation.t進行();

            if (TJAPlayer3.app.Tx.Exit_Curtain is not null && TJAPlayer3.app.Tx.Exit_Text is not null)
            {
                double t = this.ctAnimation.n現在の値, c = -1300, b = 1300, d = this.ctAnimation.n終了値;
                t = t / d - 1;
                int x = (int)(-c * (Math.Pow(t, 4) - 1) + b);

                const double n = 1500.0;
                double t2 = Math.Min(Math.Max(this.ctAnimation.n現在の値 - 1000, 0), n), c2 = 1000, b2 = -1000, d2 = n;
                int y;
                t2 = t2 / d2;
                if (t2 < 1.0 / 2.75)
                {
                    y = (int)(c2 * (7.5625 * t2 * t2) + b2);
                }
                else if (t2 < 2.0 / 2.75)
                {
                    t2 = t2 - (1.5 / 2.75);
                    y = (int)((c2 * (7.5625 * t2 * t2 + 0.75) + b2) * 0.5);
                }
                else if (t2 < 2.5 / 2.75)
                {
                    t2 = t2 - (2.25 / 2.75);
                    y = (int)((c2 * (7.5625 * t2 * t2 + 0.9375) + b2) * 0.5);
                }
                else
                {
                    t2 = t2 - (2.625 / 2.75);
                    y = (int)((c2 * (7.5625 * t2 * t2 + 0.984375) + b2) * 0.5);
                }

                TJAPlayer3.app.Tx.Exit_Curtain.t2D描画(TJAPlayer3.app.Device, x, 0);

                TJAPlayer3.app.Tx.Exit_Text.t2D描画(TJAPlayer3.app.Device, 0, y);
            }

            if (this.ctAnimation.b終了値に達した && !TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDゲーム終了音].b再生中)
            {
                return 1;
            }
        }
        return 0;
    }

    // その他

    #region [ private ]
    //-----------------
    private CCounter? ctAnimation;
    //-----------------
    #endregion
}
