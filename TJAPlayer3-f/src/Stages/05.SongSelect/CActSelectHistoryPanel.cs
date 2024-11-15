using FDK;

namespace TJAPlayer3;

internal class CActSelectHistoryPanel : CActivity
{
    // メソッド

    public CActSelectHistoryPanel()
    {
    }

    // CActivity 実装

    public override void On活性化()
    {
        this.Font = new CCachedFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 40);
        tSongChange();
        base.On活性化();
    }
    public override void On非活性化()
    {
        this.ct登場アニメ用 = null;

        for (int i = 0; i < (int)Difficulty.Total; i++)
            for (int j = 0; j < 3; j++)
                TJAPlayer3.t安全にDisposeする(ref this.Names[i, j]);
        if (Font is not null)
        {
            Font.Dispose();
            Font = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                this.ct登場アニメ用 = new CCounter(0, 3000, 1, TJAPlayer3.app.Timer);
                base.b初めての進行描画 = false;
            }
            if (ct登場アニメ用 is null)
                return 0;
            this.ct登場アニメ用.t進行();
            int[] x = TJAPlayer3.app.Skin.SkinConfig.SongSelect.ScoreWindowX;
            int[] y = TJAPlayer3.app.Skin.SkinConfig.SongSelect.ScoreWindowY;
            int xdiff = 170;
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア is not null && this.ct登場アニメ用.b終了値に達した && TJAPlayer3.stage選曲.act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.SCORE)
                {
                    if (TJAPlayer3.app.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]] is not null && TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text is not null)
                    {
                        TJAPlayer3.app.Tx.SongSelect_ScoreWindow[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]]?.t2D描画(TJAPlayer3.app.Device, x[i], y[i]);
                        for (int j = 0; j < 3; j++)
                        {
                            this.Names[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i], j]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.UpRight, x[i] + xdiff + 50, y[i] + 65 + j * 70);
                            this.t小文字表示(x[i] + xdiff, y[i] + 90 + j * 70, TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nHiScore[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[i]][j]);
                            TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x[i] + xdiff + 15, y[i] + 95 + j * 70, new Rectangle(0, 36, 32, 30));
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
    private CCounter? ct登場アニメ用;
    private CTexture?[,] Names = new CTexture?[(int)Difficulty.Total, 3];
    private CCachedFontRenderer? Font;
    //-----------------

    private void t小文字表示(int x, int y, long n)
    {
        if (TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text is null)
            return;

        for (int index = 0; index < n.ToString().Length; index++)
        {
            int Num = (int)(n / Math.Pow(10, index) % 10);
            Rectangle rectangle = new Rectangle((TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10) * Num, 0, TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10, TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Height / 2);
            TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
            x -= TJAPlayer3.app.Tx.SongSelect_ScoreWindow_Text.szTextureSize.Width / 10;
        }
    }

    public void tSongChange()
    {
        this.ct登場アニメ用 = new CCounter(0, 2000, 1, TJAPlayer3.app.Timer);

        //Dispose
        for (int i = 0; i < (int)Difficulty.Total; i++)
            for (int j = 0; j < 3; j++)
                TJAPlayer3.t安全にDisposeする(ref this.Names[i, j]);

        if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア is not null)
        {
            string[][] HiScorerName = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.strHiScorerName;

            if (Font is not null)
                for (int index = 0; index < (int)Difficulty.Total; index++)
                {
                    for (int j = 0; j < 3; j++)
                        if (!string.IsNullOrEmpty(HiScorerName[index][j]))
                        {
                            var name = this.Names[index, j] = TJAPlayer3.app.tCreateTexture(Font.DrawText(HiScorerName[index][j], Color.Black));
                            if (name is not null)
                                name.vcScaling = new Vector2(0.5f);
                        }
                }
        }
    }


    #endregion
}
