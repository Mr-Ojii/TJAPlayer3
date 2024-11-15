using FDK;

namespace TJAPlayer3;

class GoGoSplash : CActivity
{
    public GoGoSplash()
    {
    }

    public override void On活性化()
    {
        Splash = new CCounter();
        base.On活性化();
    }

    public override void On非活性化()
    {
        base.On非活性化();
    }

    /// <summary>
    /// ゴーゴースプラッシュの描画処理です。
    /// SkinCofigで本数を変更することができます。
    /// </summary>
    /// <returns></returns>
    public override int On進行描画()
    {
        if (Splash is null) return base.On進行描画();
        Splash.t進行();
        if (Splash.b終了値に達した)
        {
            Splash.n現在の値 = 0;
            Splash.t停止();
        }
        if (Splash.b進行中)
        {
            for (int i = 0; i < TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.X.Length; i++)
            {
                if (i > TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Y.Length) break;
                // Yの配列がiよりも小さかったらそこでキャンセルする。
                if (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Rotate && TJAPlayer3.app.Tx.Effects_GoGoSplash is not null)
                {
                    // Switch文を使いたかったが、定数じゃないから使えねぇ!!!!
                    if (i == 0)
                    {
                        TJAPlayer3.app.Tx.Effects_GoGoSplash.fRotation = -0.2792526803190927f;
                    }
                    else if (i == 1)
                    {
                        TJAPlayer3.app.Tx.Effects_GoGoSplash.fRotation = -0.13962634015954636f;
                    }
                    else if (i == TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.X.Length - 2)
                    {
                        TJAPlayer3.app.Tx.Effects_GoGoSplash.fRotation = 0.13962634015954636f;
                    }
                    else if (i == TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.X.Length - 1)
                    {
                        TJAPlayer3.app.Tx.Effects_GoGoSplash.fRotation = 0.2792526803190927f;
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.Effects_GoGoSplash.fRotation = 0.0f;
                    }
                }
                TJAPlayer3.app.Tx.Effects_GoGoSplash?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.X[i], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Y[i], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Width * Splash.n現在の値, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Height));
            }
        }
        return base.On進行描画();
    }

    public void StartSplash()
    {
        Splash = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Ptn - 1, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.GoGoSplash.Timer, TJAPlayer3.app.Timer);
    }

    private CCounter? Splash;
}
