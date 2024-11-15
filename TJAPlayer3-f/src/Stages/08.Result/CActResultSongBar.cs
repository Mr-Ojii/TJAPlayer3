using FDK;

namespace TJAPlayer3;

internal class CActResultSongBar : CActivity
{
    // コンストラクタ

    public CActResultSongBar()
    {
    }


    // メソッド

    public void tアニメを完了させる()
    {
        this.ct登場用.n現在の値 = this.ct登場用.n終了値;
    }


    // CActivity 実装

    public override void On活性化()
    {
        // After performing calibration, inform the player that
        // calibration has been completed, rather than
        // displaying the song title as usual.


        var title = TJAPlayer3.IsPerformingCalibration
            ? $"Calibration complete. InputAdjustTime is now {TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs}ms"
            : TJAPlayer3.DTX[0].TITLE;

        using (var pfMusicName = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameFontSize))
        {

            using (var bmpSongTitle = pfMusicName.DrawText(title, TJAPlayer3.app.Skin.SkinConfig.Result._MusicNameForeColor, TJAPlayer3.app.Skin.SkinConfig.Result._MusicNameBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
            {
                this.txMusicName = TJAPlayer3.app.tCreateTexture(bmpSongTitle);
                txMusicName.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref txMusicName);
            }
        }

        using (var pfStageText = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextFontSize))
        {
            using (var bmpStageText = pfStageText.DrawText(TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont.StageText, TJAPlayer3.app.Skin.SkinConfig.Result._StageTextForeColor, TJAPlayer3.app.Skin.SkinConfig.Result._StageTextBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
            {
                this.txStageText = TJAPlayer3.app.tCreateTexture(bmpStageText);
            }
        }

        base.On活性化();
    }
    public override void On非活性化()
    {
        if (this.ct登場用 is not null)
        {
            this.ct登場用 = null;
        }
        TJAPlayer3.t安全にDisposeする(ref this.txMusicName);

        TJAPlayer3.t安全にDisposeする(ref this.txStageText);
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (base.b活性化してない)
        {
            return 0;
        }
        if (base.b初めての進行描画)
        {
            this.ct登場用 = new CCounter(0, 270, 4, TJAPlayer3.app.Timer);
            base.b初めての進行描画 = false;
        }
        this.ct登場用.t進行();

        if (TJAPlayer3.app.ConfigToml.EnableSkinV2)
        {
            if (TJAPlayer3.app.Skin.SkinConfig.Result._v2MusicNameReferencePoint == CSkin.EReferencePoint.Center)
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameX - ((this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X) / 2), TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameY);
            }
            else if (TJAPlayer3.app.Skin.SkinConfig.Result._v2MusicNameReferencePoint == CSkin.EReferencePoint.Left)
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameX, TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameY);
            }
            else
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameX - this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X, TJAPlayer3.app.Skin.SkinConfig.Result.v2MusicNameY);
            }
        }
        else
        {
            if (TJAPlayer3.app.Skin.SkinConfig.Result._MusicNameReferencePoint == CSkin.EReferencePoint.Center)
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameX - ((this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X) / 2), TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameY);
            }
            else if (TJAPlayer3.app.Skin.SkinConfig.Result._MusicNameReferencePoint == CSkin.EReferencePoint.Left)
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameX, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameY);
            }
            else
            {
                this.txMusicName.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameX - this.txMusicName.szTextureSize.Width * txMusicName.vcScaling.X, TJAPlayer3.app.Skin.SkinConfig.Result.MusicNameY);
            }

            if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
            {
                if (TJAPlayer3.app.Skin.SkinConfig.Result._StageTextReferencePoint == CSkin.EReferencePoint.Center)
                {
                    this.txStageText.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextX - ((this.txStageText.szTextureSize.Width * txStageText.vcScaling.X) / 2), TJAPlayer3.app.Skin.SkinConfig.Result.StageTextY);
                }
                else if (TJAPlayer3.app.Skin.SkinConfig.Result._StageTextReferencePoint == CSkin.EReferencePoint.Right)
                {
                    this.txStageText.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextX - this.txStageText.szTextureSize.Width, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextY);
                }
                else
                {
                    this.txStageText.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextX, TJAPlayer3.app.Skin.SkinConfig.Result.StageTextY);
                }
            }
        }


        if (!this.ct登場用.b終了値に達した)
        {
            return 0;
        }
        return 1;
    }


    // その他

    #region [ private ]
    //-----------------
    private CCounter ct登場用;

    private CTexture txMusicName;

    private CTexture txStageText;
    //-----------------
    #endregion
}
