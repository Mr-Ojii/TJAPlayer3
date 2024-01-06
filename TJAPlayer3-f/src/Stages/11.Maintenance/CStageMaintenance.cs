using FDK;

namespace TJAPlayer3;

class CStageMaintenance : CStage
{
    // コンストラクタ

    public CStageMaintenance()
    {
        base.eStageID = CStage.EStage.Maintenance;
    }
    // CStage 実装

    public override void On活性化()
    {
        Trace.TraceInformation("メンテナンスステージを活性化します。");
        Trace.Indent();
        try
        {
            //表示用テクスチャの生成
            don = TJAPlayer3.app.ColorTexture("#ff4000", Width, Height);
            ka = TJAPlayer3.app.ColorTexture("#00c8ff", Width, Height);
            string[] txt = new string[4] { "左ふち", "左面", "右面", "右ふち" };
            using (var pf = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, fontsize))
            {
                for (int ind = 0; ind < 4; ind++)
                {
                    using (var bmp = pf.DrawText(txt[ind], Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                        str[ind] = TJAPlayer3.app.tCreateTexture(bmp);
                }
            }
            TJAPlayer3.app.Discord.Update("Maintenance");
            base.On活性化();
        }
        finally
        {
            Trace.TraceInformation("メンテナンスの活性化を完了しました。");
            Trace.Unindent();
        }
    }

    public override void On非活性化()
    {
        Trace.TraceInformation("メンテナンスステージを非活性化します。");
        Trace.Indent();
        try
        {
            //表示用テクスチャの解放
            TJAPlayer3.t安全にDisposeする(ref str);
            don?.Dispose();
            don = null;
            ka?.Dispose();
            ka = null;
        }
        finally
        {
            Trace.TraceInformation("メンテナンスステージの非活性化を完了しました。");
            Trace.Unindent();
        }
        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (base.b初めての進行描画)
        {
            base.b初めての進行描画 = false;
        }

        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
            return 1;

        if ((don is null) || (ka is null))
            return 0;

        //入力信号に合わせて色を描画
        if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue))
            ka.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 4, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.LRed))
            don.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 3, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.RRed))
            don.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 2, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue))
            ka.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 1, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue2P))
            ka.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 1, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P))
            don.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 2, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.RRed2P))
            don.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 3, Y);
        if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue2P))
            ka.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 4, Y);

        for (int index = 0; index < 4; index++)
        {
            CTexture? str_i = str[index];
            if(str_i is not null)
            {
                //文字の描画
                str_i.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * (4 - index), strY);
                str_i.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * (index + 1), strY);
            }
        }

        return 0;
    }

    #region[private]
    private CTexture? don;
    private CTexture? ka;
    private CTexture?[] str = new CTexture?[4];

    private const int Width = 100;
    private const int Height = 100;
    private const int Y = 550;
    private const int strY = 450;
    private const int fontsize = 20;

    private const int Diff = 16;
    #endregion
}
