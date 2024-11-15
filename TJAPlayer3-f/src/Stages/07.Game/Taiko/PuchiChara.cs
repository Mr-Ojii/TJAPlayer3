using FDK;

namespace TJAPlayer3;

class PuchiChara : CActivity
{
    public PuchiChara()
    {
    }

    public override void On活性化()
    {
        Counter = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Ptn - 1, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Timer, TJAPlayer3.app.Timer);
        SineCounter = new CCounter(0, 360, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.SineTimer, CSoundManager.rc演奏用タイマ);
        base.On活性化();
    }
    public override void On非活性化()
    {
        Counter = null;
        SineCounter = null;
        base.On非活性化();
    }

    public void ChangeBPM(double bpm)
    {
        int n値 = Counter.n現在の値;
        Counter = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Ptn - 1, (int)(TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Timer * bpm / TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Ptn), TJAPlayer3.app.Timer);
        Counter.t時間Reset();
        Counter.n現在の値 = n値;

        double db値 = SineCounter.db現在の値;
        SineCounter = new CCounter(1, 360, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.SineTimer * bpm / 180, CSoundManager.rc演奏用タイマ);
        SineCounter.db現在の値 = db値;
        SineCounter.t時間Resetdb();
    }

    public void InitializeBPM(double bpm)
    {
        Counter = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Ptn - 1, (int)(TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Timer * bpm / TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Ptn), TJAPlayer3.app.Timer);

        SineCounter = new CCounter(1, 360, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.SineTimer * bpm / 180, CSoundManager.rc演奏用タイマ);
    }

    /// <summary>
    /// ぷちキャラを描画する。(オーバーライドじゃないよ)
    /// </summary>
    /// <param name="x">X座標(中央)</param>
    /// <param name="y">Y座標(中央)</param>
    /// <param name="alpha">不透明度</param>
    /// <returns></returns>
    public int On進行描画(int x, int y, bool isGrowing, int nPlayer, int alpha = 255, bool isBalloon = false)
    {
        if (!TJAPlayer3.app.ConfigToml.Game.ShowPuchiChara) return base.On進行描画();
        if (Counter is null || SineCounter is null || TJAPlayer3.app.Tx.PuchiChara[nPlayer] is null) return base.On進行描画();
        Counter.t進行Loop();
        SineCounter.t進行LoopDb();
        var sineY = Math.Sin(SineCounter.db現在の値 * (Math.PI / 180)) * (TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Sine * (isBalloon ? TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Scale[1] : TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Scale[0]));
        TJAPlayer3.app.Tx.PuchiChara[nPlayer].vcScaling = new Vector2((isBalloon ? TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Scale[1] : TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Scale[0]));
        TJAPlayer3.app.Tx.PuchiChara[nPlayer].Opacity = alpha;
        TJAPlayer3.app.Tx.PuchiChara[nPlayer].t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, x, y + (int)sineY, new Rectangle(Counter.n現在の値 * TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Width, (isGrowing ? TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Height : 0), TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Width, TJAPlayer3.app.Skin.SkinConfig.Game.PuchiChara.Height));
        return base.On進行描画();
    }

    private CCounter Counter;
    private CCounter SineCounter;
}
