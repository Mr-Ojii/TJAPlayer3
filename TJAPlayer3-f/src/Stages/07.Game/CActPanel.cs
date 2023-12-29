using FDK;

namespace TJAPlayer3;

internal class CActPanel : CActivity
{

    // コンストラクタ

    public CActPanel()
    {
        base.b活性化してない = true;
        this.Start();
    }


    // メソッド

    /// <summary>
    /// 右上の曲名、曲数表示の更新を行います。
    /// </summary>
    /// <param name="songName">曲名</param>
    /// <param name="genreName">ジャンル名</param>
    /// <param name="stageText">曲数</param>
    public void SetPanelString(string songName, string subtitle, string genreName, string stageText = null)
    {
        if (!base.b活性化してる)
            return;

        TJAPlayer3.t安全にDisposeする(ref this.txPanel);
        if (!string.IsNullOrEmpty(songName))
        {
            try
            {
                TJAPlayer3.t安全にDisposeする(ref txMusicName);
                TJAPlayer3.t安全にDisposeする(ref txSubTitleName);
                using (var bmpSongTitle = pfMusicName.DrawText(songName, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameForeColor, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameBackColor, TJAPlayer3.Skin.SkinConfig.Font.EdgeRatio))
                {
                    this.txMusicName = TJAPlayer3.tCreateTexture(bmpSongTitle);
                }
                if (txMusicName != null)
                {
                    this.txMusicName.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref txMusicName);
                }
                if (!string.IsNullOrEmpty(subtitle))
                {
                    using (var bmpSubTitle = pfSubTitleName.DrawText(subtitle, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameForeColor, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameBackColor, TJAPlayer3.Skin.SkinConfig.Font.EdgeRatio))
                    {
                        this.txSubTitleName = TJAPlayer3.tCreateTexture(bmpSubTitle);
                    }
                    if (txSubTitleName != null)
                    {
                        this.txSubTitleName.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref txSubTitleName, 520);
                    }
                }

                using (var bmpDiff = pfMusicName.DrawText(stageText, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._StageTextForeColor, TJAPlayer3.Skin.SkinConfig.Game.PanelFont._StageTextBackColor, TJAPlayer3.Skin.SkinConfig.Font.EdgeRatio))
                {
                    this.tx難易度とステージ数 = TJAPlayer3.tCreateTexture(bmpDiff);
                }
            }
            catch (CTextureCreateFailedException e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("パネル文字列テクスチャの生成に失敗しました。");
                this.txPanel = null;
            }
        }
        if (!string.IsNullOrEmpty(genreName))
        {
            this.txGENRE = TJAPlayer3.Tx.TxCGen(TJAPlayer3.Skin.nStrジャンルtoNum(genreName).ToString());
        }

        this.ct進行用 = new CCounter(0, 2000, 2, TJAPlayer3.Timer);
        this.Start();
    }

    public void Stop()
    {
        this.bMute = true;
    }
    public void Start()
    {
        this.bMute = false;
    }


    // CActivity 実装

    public override void On活性化()
    {
        this.pfMusicName = new CCachedFontRenderer(TJAPlayer3.ConfigToml.General.FontName, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameFontSize);
        this.pfSubTitleName = new CCachedFontRenderer(TJAPlayer3.ConfigToml.General.FontName, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameFontSize);

        this.txPanel = null;
        this.ct進行用 = new CCounter();
        this.Start();
        this.bFirst = true;
        base.On活性化();
    }
    public override void On非活性化()
    {
        this.ct進行用 = null;

        TJAPlayer3.t安全にDisposeする(ref this.txPanel);
        TJAPlayer3.t安全にDisposeする(ref this.txMusicName);
        TJAPlayer3.t安全にDisposeする(ref this.txSubTitleName);
        TJAPlayer3.t安全にDisposeする(ref this.txGENRE);
        TJAPlayer3.t安全にDisposeする(ref this.txPanel);
        TJAPlayer3.t安全にDisposeする(ref this.pfMusicName);
        TJAPlayer3.t安全にDisposeする(ref this.pfSubTitleName);
        TJAPlayer3.t安全にDisposeする(ref this.tx難易度とステージ数);
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (TJAPlayer3.stage演奏ドラム画面.actDan.IsAnimating) return 0;
        if (!base.b活性化してない && !this.bMute)
        {
            this.ct進行用.t進行Loop();
            if (this.bFirst)
            {
                this.ct進行用.n現在の値 = 300;
            }
            if (this.txGENRE != null)
                this.txGENRE.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.GenreX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.GenreY);

            if (!TJAPlayer3.Skin.SkinConfig.Game.PanelFont.StageTextDisp)
            {
                if (this.txMusicName != null)
                {
                    float fRate = 660.0f / this.txMusicName.szTextureSize.Width;
                    if (this.txMusicName.szTextureSize.Width <= 660.0f)
                        fRate = 1.0f;
                    this.txMusicName.vcScaling.X = fRate;
                    if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Center)
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - ((this.txMusicName.szTextureSize.Width * fRate) / 2), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - (this.txMusicName.szTextureSize.Width * fRate), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    if (this.txSubTitleName != null)
                    {
                        fRate = 600.0f / this.txSubTitleName.szTextureSize.Width;
                        if (this.txSubTitleName.szTextureSize.Width <= 600.0f)
                            fRate = 1.0f;
                        this.txSubTitleName.vcScaling.X = fRate;
                        if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._SubTitleNameReferencePoint == CSkin.EReferencePoint.Center)
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX - ((this.txSubTitleName.szTextureSize.Width * fRate) / 2), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                        else if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._SubTitleNameReferencePoint == CSkin.EReferencePoint.Left)
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                        else
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX - (this.txSubTitleName.szTextureSize.Width * fRate), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                    }
                }
            }
            else
            {
                #region[ 透明度制御 ]

                if (this.txMusicName != null)
                {
                    if (this.ct進行用.n現在の値 < 745)
                    {
                        this.bFirst = false;
                        this.txMusicName.Opacity = 255;
                        if (this.txSubTitleName != null)
                            this.txSubTitleName.Opacity = 255;
                        if (this.txGENRE != null)
                            this.txGENRE.Opacity = 255;
                        this.tx難易度とステージ数.Opacity = 0;
                    }
                    else if (this.ct進行用.n現在の値 >= 745 && this.ct進行用.n現在の値 < 1000)
                    {
                        this.txMusicName.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
                        if (this.txSubTitleName != null)
                            this.txSubTitleName.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
                        if (this.txGENRE != null)
                            this.txGENRE.Opacity = 255 - (this.ct進行用.n現在の値 - 745);
                        this.tx難易度とステージ数.Opacity = this.ct進行用.n現在の値 - 745;
                    }
                    else if (this.ct進行用.n現在の値 >= 1000 && this.ct進行用.n現在の値 <= 1745)
                    {
                        this.txMusicName.Opacity = 0;
                        if (this.txSubTitleName != null)
                            this.txSubTitleName.Opacity = 0;
                        if (this.txGENRE != null)
                            this.txGENRE.Opacity = 0;
                        this.tx難易度とステージ数.Opacity = 255;
                    }
                    else if (this.ct進行用.n現在の値 >= 1745)
                    {
                        this.txMusicName.Opacity = this.ct進行用.n現在の値 - 1745;
                        if (this.txSubTitleName != null)
                            this.txSubTitleName.Opacity = this.ct進行用.n現在の値 - 1745;
                        if (this.txGENRE != null)
                            this.txGENRE.Opacity = this.ct進行用.n現在の値 - 1745;
                        this.tx難易度とステージ数.Opacity = 255 - (this.ct進行用.n現在の値 - 1745);
                    }
                    #endregion
                    if (this.b初めての進行描画)
                    {
                        b初めての進行描画 = false;
                    }
                    if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Center)
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - ((this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X) / 2), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else
                    {
                        this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - (this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    if (this.txSubTitleName != null)
                    {
                        if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._SubTitleNameReferencePoint == CSkin.EReferencePoint.Center)
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX - ((this.txSubTitleName.szTextureSize.Width * this.txSubTitleName.vcScaling.X) / 2), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                        else if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._SubTitleNameReferencePoint == CSkin.EReferencePoint.Left)
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                        else
                        {
                            this.txSubTitleName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameX - (this.txSubTitleName.szTextureSize.Width * this.txSubTitleName.vcScaling.X), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.SubTitleNameY);
                        }
                    }
                }
                if (this.tx難易度とステージ数 != null)
                    if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Center)
                    {
                        this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - (this.tx難易度とステージ数.szTextureSize.Width / 2), TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else if (TJAPlayer3.Skin.SkinConfig.Game.PanelFont._MusicNameReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
                    else
                    {
                        this.tx難易度とステージ数.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameX - this.tx難易度とステージ数.szTextureSize.Width, TJAPlayer3.Skin.SkinConfig.Game.PanelFont.MusicNameY);
                    }
            }
        }
        return base.On進行描画();
    }


    // その他

    #region [ private ]
    //-----------------
    private CCounter ct進行用;

    private CTexture txPanel;
    private bool bMute;
    private bool bFirst;

    private CTexture txMusicName;
    private CTexture txSubTitleName;
    private CTexture tx難易度とステージ数;
    private CTexture txGENRE;
    private CCachedFontRenderer pfMusicName;
    private CCachedFontRenderer pfSubTitleName;
    //-----------------
    #endregion
}
