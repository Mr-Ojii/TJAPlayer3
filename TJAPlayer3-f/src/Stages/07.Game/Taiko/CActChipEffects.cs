using FDK;

namespace TJAPlayer3;

internal class CActChipEffects : CActivity
{
    // コンストラクタ

    public CActChipEffects()
    {
    }

    // メソッド
    public virtual void Start(int nPlayer, int Lane)
    {
        if (TJAPlayer3.app.Tx.Gauge_Soul_Explosion is null)
            return;

        for (int i = 0; i < 128; i++)
        {
            var si = chipEffects[i];
            if (!si.b使用中)
            {
                si.b使用中 = true;
                si.ct進行 = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Ptn, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Timer, TJAPlayer3.app.Timer);
                si.nプレイヤー = nPlayer;
                si.Lane = Lane;
                break;
            }
        }
    }

    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            chipEffects[i] = new CChipEffect
            {
                b使用中 = false,
                ct進行 = new CCounter()
            };
        }
        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            chipEffects[i].ct進行 = null;
            chipEffects[i].b使用中 = false;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        for (int i = 0; i < 128; i++)
        {
            var si = chipEffects[i];
            if (!si.b使用中 || si.ct進行 is null)
                continue;

            si.ct進行.t進行();
            if (si.ct進行.b終了値に達した)
            {
                si.ct進行.t停止();
                si.b使用中 = false;
            }
            if (TJAPlayer3.app.Tx.Notes is not null)
                TJAPlayer3.app.Tx.Notes.Opacity = 255 - (int)Math.Min((500.0 * (si.ct進行.n現在の値 / (double)si.ct進行.n終了値)), 255.0);
            if (TJAPlayer3.app.Tx.Notes_White is not null)
                TJAPlayer3.app.Tx.Notes_White.Opacity = (int)Math.Min((500.0 * (si.ct進行.n現在の値 / (double)si.ct進行.n終了値)), 255.0);//2020.05.15 Mr-Ojii ノーツを白くするために追加。
            switch (si.nプレイヤー)
            {
                case 0:
                    TJAPlayer3.app.Tx.Gauge_Soul_Explosion[0]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[0], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[0], new Rectangle(si.ct進行.n現在の値 * TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Width, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Height));
                    TJAPlayer3.app.Tx.Notes?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[0], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[0], new Rectangle(si.Lane * 130, 0, 130, 130));
                    TJAPlayer3.app.Tx.Notes_White?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[0], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[0], new Rectangle(si.Lane * 130, 0, 130, 130));
                    break;
                case 1:
                    TJAPlayer3.app.Tx.Gauge_Soul_Explosion[1]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[1], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[1], new Rectangle(si.ct進行.n現在の値 * TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Width, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.NotesFlash.Height));
                    TJAPlayer3.app.Tx.Notes?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[1], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[1], new Rectangle(si.Lane * 130, 0, 130, 130));
                    TJAPlayer3.app.Tx.Notes_White?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[1], TJAPlayer3.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[1], new Rectangle(si.Lane * 130, 0, 130, 130));
                    break;
            }
            if (TJAPlayer3.app.Tx.Notes is not null)
                TJAPlayer3.app.Tx.Notes.Opacity = 255; //2020.05.15 Mr-Ojii これ書いとかないと、流れるノーツも半透明化されてしまう。
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    private class CChipEffect
    {
        public bool b使用中;
        public CCounter? ct進行;
        public int nプレイヤー;
        public int Lane;
    }
    private CChipEffect[] chipEffects = new CChipEffect[128];
    //-----------------
    #endregion
}
