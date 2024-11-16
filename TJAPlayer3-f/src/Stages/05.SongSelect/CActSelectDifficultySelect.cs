using FDK;

namespace TJAPlayer3;

/// <summary>
/// DifficultySelect。
/// </summary>
internal class CActSelectDifficultySelect : CActivity
{
    // プロパティ

    // CActivity 実装

    public override void On活性化()
    {
        if (this.b活性化してる)
            return;
        try
        {
            this.ct分岐表示用タイマー = new CCounter(1, 2, 2500, TJAPlayer3.app.Timer);
            選択済み = new bool[2] { false, false };
            裏カウント = new int[2] { 0, 0 };
        }
        finally
        {

        }

        base.On活性化();
    }
    public override void On非活性化()
    {
        if (this.b活性化してない)
            return;

        try
        {
            this.ct分岐表示用タイマー = null;
            this.b開いた直後 = true;
        }
        finally
        {
        }

        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (this.b活性化してない)
            return 0;

        #region [ 初めての進行描画 ]
        //-----------------
        if (base.b初めての進行描画)
        {
            base.b初めての進行描画 = false;
        }

        if (b開いた直後)
        {

            for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
            {
                this.ct難易度拡大用[nPlayer].n現在の値 = 0;
                this.ct難易度拡大用[nPlayer].t時間Reset();
            }
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND難易度選択]?.t再生する();
            b開いた直後 = false;
        }
        //-----------------
        #endregion

        this.ct分岐表示用タイマー.t進行Loop();
        this.ct難易度拡大用[0].t進行();
        this.ct難易度拡大用[1].t進行();

        // 描画。


        #region[難易度マーク]

        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (選択済み[i])
            {
                CTexture? diff_mark = TJAPlayer3.app.Tx.Difficulty_Mark[確定された難易度[i]];
                if (diff_mark is not null)
                {
                    diff_mark.Opacity = 100;
                    diff_mark.vcScaling = new Vector2(0.75f);
                    diff_mark.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, i * 1075 - 30, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.MarkY);
                }
            }
            else if (現在の選択行[i] >= 3)
            {
                CTexture? diff_mark = TJAPlayer3.app.Tx.Difficulty_Mark[現在の選択行[i] - 3];
                if (裏表示 && 現在の選択行[i] - 3 == 3)
                {
                    diff_mark = TJAPlayer3.app.Tx.Difficulty_Mark[4];
                }
                if (diff_mark is not null)
                {
                    diff_mark.Opacity = 100;
                    diff_mark.vcScaling.X = 0.75f;
                    diff_mark.vcScaling.Y = 0.75f * (float)(1 + Math.Sin(ct難易度拡大用[i].n現在の値 * Math.PI / 180) * 0.25);
                    diff_mark.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, i * 1075 - 30, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.MarkY);
                }
            }
        }
        #endregion
        #region[難易度選択裏バー描画]
        if (TJAPlayer3.app.Tx.Difficulty_Center_Bar is not null)
        {
            int width = TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandW;
            int height = TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandH;

            int xdiff = TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterX - width / 2;
            int ydiff = TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandY;


            int wh = Math.Min(TJAPlayer3.app.Tx.Difficulty_Center_Bar.szTextureSize.Width / 3, TJAPlayer3.app.Tx.Difficulty_Center_Bar.szTextureSize.Height / 3);

            for (int i = 0; i < width / wh + 1; i++)
            {
                for (int j = 0; j < height / wh + 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(0, 0, wh, wh));
                    }
                    else if (i == 0 && j == (height / wh))
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(0, wh * 2, wh, wh));
                    }
                    else if (i == (width / wh) && j == 0)
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh + ydiff, new Rectangle(wh * 2, 0, wh, wh));
                    }
                    else if (i == (width / wh) && j == (height / wh))
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(wh * 2, wh * 2, wh, wh));
                    }
                    else if (i == 0)
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(0, wh, wh, wh));
                    }
                    else if (j == 0)
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(wh, 0, wh, wh));
                    }
                    else if (i == (width / wh))
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh - (wh - width % wh) + xdiff, j * wh + ydiff, new Rectangle(wh * 2, wh, wh, wh));
                    }
                    else if (j == (height / wh))
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh - (wh - height % wh) + ydiff, new Rectangle(wh, wh * 2, wh, wh));
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.Difficulty_Center_Bar.t2D描画(TJAPlayer3.app.Device, i * wh + xdiff, j * wh + ydiff, new Rectangle(wh, wh, wh, wh));
                    }
                }
            }
        }
        #endregion
        #region[タイトル文字列]
        int xAnime = 200;
        int yAnime = 60;

        if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲のサブタイトル is not null)
        {
            TJAPlayer3.stage選曲.act曲リスト.サブタイトルtmp.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 707 + (TJAPlayer3.stage選曲.act曲リスト.サブタイトルtmp.szTextureSize.Width / 2) + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 430 - yAnime);
            if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲の曲名 is not null)
            {
                TJAPlayer3.stage選曲.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 23 - yAnime);
            }
        }
        else if (TJAPlayer3.stage選曲.act曲リスト.ttk選択している曲の曲名 is not null)
        {
            TJAPlayer3.stage選曲.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 23 - yAnime);
        }
        #endregion
        #region[バーテクスチャ]
        for (int i = 0; i < 3; i++)
            TJAPlayer3.app.Tx.Difficulty_Bar_Etc[i]?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarEtcX[i], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarEtcY[i]);

        for (int i = 0; i < 4; i++)
        {
            CTexture? diff_bar = TJAPlayer3.app.Tx.Difficulty_Bar[i];
            int j = i;
            if (裏表示 && i == 3)
            {
                j = 4;
                diff_bar = TJAPlayer3.app.Tx.Difficulty_Bar[4];
            }
            if (diff_bar is not null)
            {
                if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[j])
                    diff_bar.color = Color.FromArgb(255, 255, 255, 255);
                else
                    diff_bar.color = Color.FromArgb(255, 127, 127, 127);
                diff_bar.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarX[i], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarY[i]);
            }
        }
        #endregion
        #region[星]
        if (TJAPlayer3.app.Tx.Difficulty_Star is not null)//Difficulty_Starがないなら、通す必要なし！
        {
            for (int i = 0; i < 4; i++)
            {
                int level = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.Level[i];
                Rectangle rect = new Rectangle(0, 0, TJAPlayer3.app.Tx.Difficulty_Star.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_Star.szTextureSize.Height);
                if (裏表示 && i == 3)
                {
                    level = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.Level[4];
                    rect.X = TJAPlayer3.app.Tx.Difficulty_Star.szTextureSize.Width / 2;
                }
                for (int j = 0; j < level; j++)
                {
                    TJAPlayer3.app.Tx.Difficulty_Star.t2D描画(TJAPlayer3.app.Device, i * 100 + 475, 483 - (j * 20), rect);
                }
            }
        }
        #endregion
        #region[譜面分岐]
        if (TJAPlayer3.app.Tx.Difficulty_Branch is not null)//Difficulty_Branchがないなら、通す必要なし！
        {
            TJAPlayer3.app.Tx.Difficulty_Branch.Opacity = (int)((ct分岐表示用タイマー.n現在の値 % 2) * 255.0);
            for (int i = 0; i < 4; i++)
            {
                if (裏表示 && i == 3)
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面分岐[4])
                        TJAPlayer3.app.Tx.Difficulty_Branch.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(TJAPlayer3.app.Tx.Difficulty_Branch.szTextureSize.Width / 2, 0, TJAPlayer3.app.Tx.Difficulty_Branch.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_Branch.szTextureSize.Height));
                }
                else
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面分岐[i])
                        TJAPlayer3.app.Tx.Difficulty_Branch.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(0, 0, TJAPlayer3.app.Tx.Difficulty_Branch.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_Branch.szTextureSize.Height));
                }
            }
        }
        #endregion
        #region[パパママサポート]
        if (TJAPlayer3.app.Tx.Difficulty_PapaMama is not null)//Difficulty_PapaMamaがないなら、通す必要なし！
        {
            TJAPlayer3.app.Tx.Difficulty_PapaMama.Opacity = (int)((ct分岐表示用タイマー.n現在の値 % 2) * 255.0);
            for (int i = 0; i < 4; i++)
            {
                if (裏表示 && i == 3)
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.bPapaMamaSupport[4])
                        TJAPlayer3.app.Tx.Difficulty_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(TJAPlayer3.app.Tx.Difficulty_PapaMama.szTextureSize.Width / 2, 0, TJAPlayer3.app.Tx.Difficulty_PapaMama.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_PapaMama.szTextureSize.Height));
                }
                else
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.bPapaMamaSupport[i])
                        TJAPlayer3.app.Tx.Difficulty_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 100 + 470, 310, new Rectangle(0, 0, TJAPlayer3.app.Tx.Difficulty_PapaMama.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_PapaMama.szTextureSize.Height));
                }
            }
        }
        #endregion
        #region[王冠]
        if (TJAPlayer3.app.Tx.Crown_t is not null)//王冠テクスチャがないなら、通す必要なし！
        {
            TJAPlayer3.app.Tx.Crown_t.Opacity = 255;
            TJAPlayer3.app.Tx.Crown_t.vcScaling = new Vector2(0.35f);
            for (int i = 0; i < 4; i++)
            {
                if (裏表示 && i == 3)
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[4])
                        TJAPlayer3.app.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarX[i] + 35, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarY[i] - 30, new Rectangle(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nCrown[4] * 100, 0, 100, 100));
                }
                else
                {
                    if (TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[i])
                        TJAPlayer3.app.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarX[i] + 35, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarY[i] - 30, new Rectangle(TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.nCrown[i] * 100, 0, 100, 100));
                }
            }
        }
        #endregion
        #region[プレイヤーアンカー]
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 && 現在の選択行[0] == 現在の選択行[1] && !選択済み[0] && !選択済み[1])
            {
                if (現在の選択行[i] < 3)
                {
                    CTexture? anc_box_etc = TJAPlayer3.app.Tx.Difficulty_Anc_Box_Etc[i];
                    if (anc_box_etc is not null)
                        anc_box_etc.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxEtcX[現在の選択行[i]] + i * anc_box_etc.szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxEtcY[現在の選択行[i]], new Rectangle(i * anc_box_etc.szTextureSize.Width / 2, 0, anc_box_etc.szTextureSize.Width / 2, anc_box_etc.szTextureSize.Height));

                    CTexture? anc_same = TJAPlayer3.app.Tx.Difficulty_Anc_Same[i];
                    if (anc_same is not null)
                        anc_same.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncEtcX[現在の選択行[i]] + (int)(anc_same.szTextureSize.Width * (i - 0.5)), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncEtcY[現在の選択行[i]]);
                }
                else
                {
                    CTexture? anc_box = TJAPlayer3.app.Tx.Difficulty_Anc_Box[i];
                    if (anc_box is not null)
                        anc_box.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxX[現在の選択行[i] - 3] + i * anc_box.szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxY[現在の選択行[i] - 3], new Rectangle(i * anc_box.szTextureSize.Width / 2, 0, anc_box.szTextureSize.Width / 2, anc_box.szTextureSize.Height));

                    CTexture? anc_same = TJAPlayer3.app.Tx.Difficulty_Anc_Same[i];
                    if (anc_same is not null)
                        anc_same.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncX[現在の選択行[i] - 3] + (int)(anc_same.szTextureSize.Width * (i - 0.5)), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncY[現在の選択行[i] - 3]);
                }
            }
            else
            {
                if (!選択済み[i])
                {
                    if (現在の選択行[i] < 3)
                    {
                        CTexture? anc_box_etc = TJAPlayer3.app.Tx.Difficulty_Anc_Box_Etc[i];
                        if (anc_box_etc is not null)
                            anc_box_etc.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxEtcX[現在の選択行[i]], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxEtcY[現在の選択行[i]]);

                        CTexture? anc = TJAPlayer3.app.Tx.Difficulty_Anc[i];
                        if (anc is not null)
                            anc.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncEtcX[現在の選択行[i]], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncEtcY[現在の選択行[i]]);
                    }
                    else
                    {
                        CTexture? anc_box = TJAPlayer3.app.Tx.Difficulty_Anc_Box[i];
                        if (anc_box is not null)
                            anc_box.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxX[現在の選択行[i] - 3], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncBoxY[現在の選択行[i] - 3]);

                        CTexture? anc = TJAPlayer3.app.Tx.Difficulty_Anc[i];
                        if (anc is not null)
                            anc.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncX[現在の選択行[i] - 3], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.AncY[現在の選択行[i] - 3]);
                    }
                }
            }
        }
        #endregion
        #region[BPM]
        if (TJAPlayer3.app.ConfigToml.SongSelect.TCCLikeStyle && TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア is not null && TJAPlayer3.app.Tx.Difficulty_BPMBox is not null && TJAPlayer3.app.Tx.Difficulty_BPMNumber is not null)
        {
            const int cx = 1000, cy = 500;

            TJAPlayer3.app.Tx.Difficulty_BPMBox.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.UpRight, cx + (TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Width / 10) + 10, cy - (TJAPlayer3.app.Tx.Difficulty_BPMBox.szTextureSize.Height - TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Height) / 2);
            this.tBPM小文字表示(cx, cy, (long)TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.Bpm);
        }
        #endregion

        //裏鬼表示用
        if (((裏カウント[0] >= 10 || 裏カウント[1] >= 10) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Tab)) && TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[4])
        {
            裏表示 = !裏表示;
            裏カウント[0] = 0;
            裏カウント[1] = 0;
            if (裏表示)
                TJAPlayer3.stage選曲.act曲リスト.n現在のアンカ難易度レベル[0] = 4;
            else
                TJAPlayer3.stage選曲.act曲リスト.n現在のアンカ難易度レベル[0] = 3;
        }
        return 0;
    }




    // その他

    #region [ private ]
    //-----------------
    internal int[] 現在の選択行 = new int[2] { TJAPlayer3.app.ConfigToml.PlayOption.DefaultCourse + 3, TJAPlayer3.app.ConfigToml.PlayOption.DefaultCourse + 3 };
    internal bool[] 選択済み = new bool[2];
    internal int[] 確定された難易度 = new int[2];
    internal int[] 裏カウント = new int[2];
    internal bool 裏表示 = false;
    internal bool b開いた直後 = true;
    private CCounter ct分岐表示用タイマー;
    internal CCounter[] ct難易度拡大用 = { new CCounter(0, 180, 1, TJAPlayer3.app.Timer), new CCounter(0, 180, 1, TJAPlayer3.app.Timer) };

    private void tBPM小文字表示(int x, int y, long n)
    {
        if (TJAPlayer3.app.Tx.Difficulty_BPMNumber is not null)
        {
            for (int index = 0; index < n.ToString().Length; index++)
            {
                int Num = (int)(n / Math.Pow(10, index) % 10);
                Rectangle rectangle = new Rectangle((TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Width / 10) * Num, 0, TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Width / 10, TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Height);

                TJAPlayer3.app.Tx.Difficulty_BPMNumber.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
                x -= TJAPlayer3.app.Tx.Difficulty_BPMNumber.szTextureSize.Width / 10;
            }
        }
    }
    //-----------------
    #endregion
}
