using FDK;

namespace TJAPlayer3;

internal class CStage選曲 : CStage
{
    // プロパティ
    public int[] n確定された曲の難易度
    {
        get;
        private set;
    }
    public Cスコア r確定されたスコア
    {
        get;
        internal set;
    }
    public C曲リストノード r確定された曲
    {
        get;
        private set;
    }
    public int[] n現在選択中の曲の難易度
    {
        get
        {
            return this.act曲リスト.n現在選択中の曲の難易度レベル;
        }
    }

    // コンストラクタ
    public CStage選曲()
    {
        base.eStageID = CStage.EStage.SongSelect;
        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
        base.listChildren.Add(this.actFIFO = new CActFIFOBlack());
        base.listChildren.Add(this.actFIfromResult = new CActFIFOBlack());
        base.listChildren.Add(this.actFOtoNowLoading = new CActFIFOStart());
        base.listChildren.Add(this.act曲リスト = new CActSelect曲リスト());
        base.listChildren.Add(this.actDifficultySelect = new CActSelectDifficultySelect());
        base.listChildren.Add(this.actHistoryPanel = new CActSelectHistoryPanel());
        base.listChildren.Add(this.actPresound = new CActSelectPresound());
        base.listChildren.Add(this.actSortSongs = new CActSortSongs());
        base.listChildren.Add(this.actPlayOption = new CActSelectPlayOption());
        base.listChildren.Add(this.actChangeSE = new CActSelectChangeSE());
    }


    // メソッド

    public void t選択曲変更通知()
    {
        this.actPresound.t選択曲が変更された();
    }

    // CStage 実装

    /// <summary>
    /// 曲リストをリセットする
    /// </summary>
    /// <param name="cs"></param>
    public void Refresh(CSongsManager cs, bool bRemakeSongTitleBar)
    {
        this.act曲リスト.Refresh(cs, bRemakeSongTitleBar);
    }

    public override void On活性化()
    {
        Trace.TraceInformation("選曲ステージを活性化します。");
        Trace.Indent();
        try
        {
            this.n確定された曲の難易度 = new int[4];
            this.eFadeOut完了時の戻り値 = E戻り値.継続;
            this.bBGM再生済み = false;
            for (int i = 0; i < 4; i++)
                this.ctキー反復用[i] = new CCounter(0, 0, 0, TJAPlayer3.app.Timer);

            if (TJAPlayer3.app.Tx.SongSelect_Background is not null)
                this.ct背景スクロール用タイマー = new CCounter(0, TJAPlayer3.app.Tx.SongSelect_Background.szTextureSize.Width, 30, TJAPlayer3.app.Timer);
            this.ctカウントダウン用タイマー = new CCounter(0, 100, 1000, TJAPlayer3.app.Timer);
            this.ctDifficultySelectIN用タイマー = new CCounter(0, 750, 1, TJAPlayer3.app.Timer);
            this.ctDifficultySelectINバー拡大用タイマー = new CCounter(0, 750, 1, TJAPlayer3.app.Timer);
            this.ctDifficultySelectOUT用タイマー = new CCounter(0, 500, 1, TJAPlayer3.app.Timer);

            //this.actDifficultySelect.bIsDifficltSelect = true;
            base.On活性化();

            現在の選曲画面状況 = E選曲画面.通常;
            完全に選択済み = false;
            // Discord Presenceの更新

            TJAPlayer3.app.Discord.Update("SongSelect");
        }
        finally
        {
            TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.Normal;
            TJAPlayer3.app.ConfigToml.OverrideScrollMode = false;
            Trace.TraceInformation("選曲ステージの活性化を完了しました。");
            Trace.Unindent();
        }
    }

    public override void On非活性化()
    {
        Trace.TraceInformation("選曲ステージを非活性化します。");
        Trace.Indent();
        try
        {
            for (int i = 0; i < 4; i++)
            {
                this.ctキー反復用[i] = null;
            }
            base.On非活性化();
        }
        finally
        {
            Trace.TraceInformation("選曲ステージの非活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            this.ct背景スクロール用タイマー?.t進行Loop();
            this.ctカウントダウン用タイマー.t進行Loop();
            #region [ 初めての進行描画 ]
            //---------------------
            if (base.b初めての進行描画)
            {
                this.ct登場時アニメ用共通 = new CCounter(0, 100, 3, TJAPlayer3.app.Timer);
                if (TJAPlayer3.r直前のステージ == TJAPlayer3.stageResult)
                {
                    this.actFIfromResult.tFadeIn開始();
                    base.eフェーズID = CStage.Eフェーズ.選曲_結果画面からのFadeIn;
                }
                else
                {
                    this.actFIFO.tFadeIn開始();
                    base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
                }
                this.t選択曲変更通知();
                base.b初めての進行描画 = false;
            }
            //---------------------
            #endregion

            this.ct登場時アニメ用共通.t進行();

            TJAPlayer3.app.Tx.SongSelect_Background?.t2D描画(TJAPlayer3.app.Device, 0, 0);

            if (act曲リスト.r現在選択中の曲 is not null)
            {
                int nGenreBack = 0;
                if (act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.BOX || act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.SCORE)
                {
                    nGenreBack = TJAPlayer3.app.Skin.nStrジャンルtoNum(act曲リスト.r現在選択中の曲.strGenre);
                }
                else if (act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.BACKBOX)
                {
                    nGenreBack = TJAPlayer3.app.Skin.nStrジャンルtoNum(act曲リスト.r現在選択中の曲.r親ノード.strGenre);
                }
                if (TJAPlayer3.app.Tx.SongSelect_GenreBack[nGenreBack] is not null)
                {
                    for (int i = 0; i < (TJAPlayer3.app.LogicalSize.Width / TJAPlayer3.app.Tx.SongSelect_GenreBack[nGenreBack].szTextureSize.Width) + 2; i++)
                    {
                        if (TJAPlayer3.app.Tx.SongSelect_GenreBack[nGenreBack] is not null && ct背景スクロール用タイマー is not null)
                        {
                            TJAPlayer3.app.Tx.SongSelect_GenreBack[nGenreBack].t2D描画(TJAPlayer3.app.Device, -ct背景スクロール用タイマー.n現在の値 + TJAPlayer3.app.Tx.SongSelect_GenreBack[nGenreBack].szTextureSize.Width * i, 0);
                        }
                    }
                }
            }

            if (現在の選曲画面状況 != E選曲画面.難易度選択)
            {
                this.act曲リスト.On進行描画();
                this.actDifficultySelect.裏表示 = false;
                this.actDifficultySelect.裏カウント[0] = 0;
            }

            if (現在の選曲画面状況 == E選曲画面.難易度選択In)
            {
                this.ctDifficultySelectIN用タイマー.t進行();
                if (this.ctDifficultySelectIN用タイマー.b終了値に達した)
                {
                    this.ctDifficultySelectINバー拡大用タイマー.t進行();
                    if (this.ctDifficultySelectINバー拡大用タイマー.b終了値に達した)
                    {

                        現在の選曲画面状況 = E選曲画面.難易度選択;
                    }
                }
                else
                {
                    this.ctDifficultySelectINバー拡大用タイマー.n現在の値 = 0;
                    this.ctDifficultySelectINバー拡大用タイマー.t時間Reset();
                }

                if (TJAPlayer3.app.Tx.Difficulty_Center_Bar is not null)
                {
                    //Bar_Centerの拡大アニメーション
                    int width = Math.Max(Math.Min(
                        TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalW +
                        (int)((TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandW - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalW) * (((double)ctDifficultySelectINバー拡大用タイマー.n現在の値 * 3) / ctDifficultySelectINバー拡大用タイマー.n終了値)),
                        TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandW), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalW);

                    int height = Math.Max(Math.Min(
                        TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalH +
                        (int)((TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandH - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalH) * (((double)ctDifficultySelectINバー拡大用タイマー.n現在の値 * 2 - ctDifficultySelectINバー拡大用タイマー.n終了値 / 2) / ctDifficultySelectINバー拡大用タイマー.n終了値)),
                        TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandH), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalH);

                    int ydiff = Math.Min(Math.Max(TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalY + (int)((TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandY - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalY) * (((double)ctDifficultySelectINバー拡大用タイマー.n現在の値 * 2 - ctDifficultySelectINバー拡大用タイマー.n終了値 / 2) / ctDifficultySelectINバー拡大用タイマー.n終了値)), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterExpandY), TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterNormalY);

                    int xdiff = TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.BarCenterX - width / 2;

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

                int xAnime = Math.Min((int)(200 * Math.Max((((double)ctDifficultySelectINバー拡大用タイマー.n現在の値 * 3) / ctDifficultySelectINバー拡大用タイマー.n終了値), 0)), 200);
                int yAnime = Math.Min((int)(60 * Math.Max((((double)ctDifficultySelectINバー拡大用タイマー.n現在の値 * 2 - ctDifficultySelectINバー拡大用タイマー.n終了値 / 2) / ctDifficultySelectINバー拡大用タイマー.n終了値), 0)), 60);

                if (this.act曲リスト.ttk選択している曲のサブタイトル is not null)
                {
                    this.act曲リスト.サブタイトルtmp.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 707 + (this.act曲リスト.サブタイトルtmp.szTextureSize.Width / 2) + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 430 - yAnime);
                    if (this.act曲リスト.ttk選択している曲の曲名 is not null)
                    {
                        this.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 23 - yAnime);
                    }
                }
                else if (this.act曲リスト.ttk選択している曲の曲名 is not null)
                {
                    this.act曲リスト.タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750 + xAnime, TJAPlayer3.app.Skin.SkinConfig.SongSelect.OverallY + 23 - yAnime);
                }

            }
            else
            {
                this.ctDifficultySelectIN用タイマー.n現在の値 = 0;
                this.ctDifficultySelectIN用タイマー.t時間Reset();
            }

            if (現在の選曲画面状況 == E選曲画面.難易度選択Out)
            {
                this.ctDifficultySelectOUT用タイマー.t進行();
                if (this.ctDifficultySelectOUT用タイマー.b終了値に達した)
                {
                    現在の選曲画面状況 = E選曲画面.通常;
                }
            }
            else
            {
                this.ctDifficultySelectOUT用タイマー.n現在の値 = 0;
                this.ctDifficultySelectOUT用タイマー.t時間Reset();
            }


            //this.actPreimageパネル.On進行描画();
            //	this.bIsEnumeratingSongs = !this.actPreimageパネル.bIsPlayingPremovie;				// #27060 2011.3.2 yyagi: #PREMOVIE再生中は曲検索を中断する
            if (現在の選曲画面状況 == E選曲画面.難易度選択)
            {
                this.actDifficultySelect.On進行描画();
            }

            if (TJAPlayer3.app.Tx.SongSelect_Header is not null)
                TJAPlayer3.app.Tx.SongSelect_Header.t2D描画(TJAPlayer3.app.Device, 0, 0);

            if (TJAPlayer3.app.Tx.SongSelect_Footer is not null)
                TJAPlayer3.app.Tx.SongSelect_Footer.t2D描画(TJAPlayer3.app.Device, 0, TJAPlayer3.app.LogicalSize.Height - TJAPlayer3.app.Tx.SongSelect_Footer.szTextureSize.Height);

            #region ネームプレート
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                if (TJAPlayer3.app.Tx.NamePlate[i] is not null)
                {
                    TJAPlayer3.app.Tx.NamePlate[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.NamePlateX[i], TJAPlayer3.app.Skin.SkinConfig.SongSelect.NamePlateY[i]);
                }
            }
            if (TJAPlayer3.app.Tx.SongSelect_Auto is not null)
            {
                for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
                {
                    if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[i])
                    {
                        TJAPlayer3.app.Tx.SongSelect_Auto.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongSelect.NamePlateAutoX[i], TJAPlayer3.app.Skin.SkinConfig.SongSelect.NamePlateAutoY[i]);
                    }
                }
            }
            #endregion

            #region[ 下部テキスト ]
            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, "GAME: SURVIVAL");
            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー激辛)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, "GAME: SURVIVAL HARD");
            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, "GAME: TRAINING MODE");
            if (TJAPlayer3.app.ConfigToml.SuperHard)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 16, C文字コンソール.EFontType.赤, "SUPER HARD MODE : ON");
            if (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.BMSCROLL)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 32, C文字コンソール.EFontType.赤, "BMSCROLL : ON");
            else if (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.HBSCROLL)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 32, C文字コンソール.EFontType.赤, "HBSCROLL : ON");
            else if (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.REGULSPEED)
                TJAPlayer3.app.act文字コンソール.tPrint(0, 32, C文字コンソール.EFontType.赤, "Reg.Speed : " + TJAPlayer3.app.ConfigToml.RegSpeedBPM.ToString());
            #endregion

            if (TJAPlayer3.app.ConfigToml.SongSelect.CountDownTimer && TJAPlayer3.app.Tx.SongSelect_Counter_Back[0] is not null && TJAPlayer3.app.Tx.SongSelect_Counter_Back[1] is not null && TJAPlayer3.app.Tx.SongSelect_Counter_Num[0] is not null && TJAPlayer3.app.Tx.SongSelect_Counter_Num[1] is not null)
            {
                This_counter = (100 - this.ctカウントダウン用タイマー.n現在の値);
                int dotinum = 1;
                if (This_counter >= 10)
                    dotinum = 0;
                TJAPlayer3.app.Tx.SongSelect_Counter_Back[dotinum].t2D描画(TJAPlayer3.app.Device, 880, 0);
                for (int countdig = 0; countdig < This_counter.ToString().Length; countdig++)
                    TJAPlayer3.app.Tx.SongSelect_Counter_Num[dotinum].t2D描画(TJAPlayer3.app.Device, (int)(((countdig + (This_counter.ToString().Length - 1) / 2.0) - (This_counter.ToString().Length - 1)) * 48.0) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.CounterX, TJAPlayer3.app.Skin.SkinConfig.SongSelect.CounterY, new Rectangle((TJAPlayer3.app.Tx.SongSelect_Counter_Num[dotinum].szTextureSize.Width / 10) * (This_counter / (int)Math.Pow(10, This_counter.ToString().Length - countdig - 1) % 10), 0, TJAPlayer3.app.Tx.SongSelect_Counter_Num[dotinum].szTextureSize.Width / 10, TJAPlayer3.app.Tx.SongSelect_Counter_Num[dotinum].szTextureSize.Height));
            }

            if (this.act曲リスト.n現在選択中の曲の難易度レベル[0] != (int)Difficulty.Dan)
                this.actPresound.On進行描画();

            this.actHistoryPanel.On進行描画();

            if (act曲リスト.r現在選択中の曲 is not null && TJAPlayer3.app.Tx.SongSelect_Difficulty is not null)
                TJAPlayer3.app.Tx.SongSelect_Difficulty.t2D描画(TJAPlayer3.app.Device, 830, 40, new Rectangle(0, 70 * this.n現在選択中の曲の難易度[0], 260, 70));

            if (!this.bBGM再生済み && (base.eフェーズID == CStage.Eフェーズ.共通_通常状態))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].t再生する();
                this.bBGM再生済み = true;
            }

            if (現在の選曲画面状況 == E選曲画面.Dan選択)
            {
                if (TJAPlayer3.app.Tx.Difficulty_Dan_Box is not null && TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting is not null)
                {
                    TJAPlayer3.app.Tx.Difficulty_Dan_Box.t2D描画(TJAPlayer3.app.Device, 0, 0);
                    TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting.szTextureSize.Width / 2 * DanSelectingRow, 0, new Rectangle(TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting.szTextureSize.Width / 2 * DanSelectingRow, 0, TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting.szTextureSize.Height));
                }
            }

            if ((act曲リスト.r現在選択中の曲 is not null) && TJAPlayer3.app.ConfigToml.SongSelect.TCCLikeStyle && act曲リスト.r現在選択中の曲.eNodeType == C曲リストノード.ENodeType.SCORE)
                this.act曲リスト.tアイテム数の描画();

            this.actChangeSE.On進行描画();
            this.actPlayOption.On進行描画();

            // キー入力
            if (base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
            {
                if (popupbool[0])
                {
                    //クイックコンフィグの呼び出し
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                    this.actPlayOption.tActivatePopupMenu(0);
                    popupbool[0] = false;
                    popupbool[1] = false;
                }
                else if (popupbool[1])
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                    this.actPlayOption.tActivatePopupMenu(1);
                    popupbool[0] = false;
                    popupbool[1] = false;
                }

                #region[もし段位道場の確認状態だったら]
                if (現在の選曲画面状況 == E選曲画面.Dan選択)
                {//2020.05.25 Mr-Ojii 段位道場の確認を追加
                    #region [ ESC ]
                    if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape) && (this.act曲リスト.r現在選択中の曲 is not null))
                    {
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                        現在の選曲画面状況 = E選曲画面.通常;
                    }
                    #endregion
                    #region[Decide]
                    if (((TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed)) || (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 ||
                                    (TJAPlayer3.app.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return))))
                    {
                        if (DanSelectingRow == 1)
                        {
                            if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].b読み込み成功)
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].t再生する();
                            else
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                            this.t曲を選択する();
                            現在の選曲画面状況 = E選曲画面.通常;
                        }
                        else
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                            現在の選曲画面状況 = E選曲画面.通常;
                        }
                    }
                    #endregion
                    #region [ Up ]
                    if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow))
                    {
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                        DanSelectingRow = 0;
                    }
                    #endregion
                    #region [ Down ]
                    if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow))
                    {
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                        DanSelectingRow = 1;
                    }
                    #endregion
                }
                #endregion
                #region[DifficultySelectのキー入力]
                else if (現在の選曲画面状況 == E選曲画面.難易度選択)
                {//2020.06.02 Mr-Ojii DifficultySelectの追加
                    if (!this.actSortSongs.bIsActivePopupMenu)
                    {
                        #region [ ESC ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape) && (this.act曲リスト.r現在選択中の曲 is not null))
                        {
                            if (this.actChangeSE.bIsActive[0])
                                this.actChangeSE.tDeativateChangeSE(0);
                            else if (this.actPlayOption.bIsActive[0])
                                this.actPlayOption.tDeativatePopupMenu(0);
                            else if (this.actDifficultySelect.選択済み[0] && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
                            {
                                if (this.actChangeSE.bIsActive[1])
                                    this.actChangeSE.tDeativateChangeSE(1);
                                else if (this.actPlayOption.bIsActive[1])
                                    this.actPlayOption.tDeativatePopupMenu(1);
                                else
                                    難易度から選曲へ戻る();
                            }
                            else
                                難易度から選曲へ戻る();
                        }
                        #endregion
                        #region [ Shift-F1: CONFIG画面 ]
                        if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightShift) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftShift)) &&
                            TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F1))
                        {   // [SHIFT] + [F1] CONFIG
                            this.GotoConfig();
                            return 0;
                        }
                        #endregion
#if PLAYABLE
                        #region [ F3 1PオートON/OFF ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F3))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0];
                        }
                        #endregion
                        #region [ F4 2PオートON/OFF ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F4))
                        {
                            if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount > 1)
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                                TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1];
                            }
                        }
                        #endregion
#endif
                        #region [ F5 スーパーハード ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F5))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.SuperHard = !TJAPlayer3.app.ConfigToml.SuperHard;
                        }
                        #endregion
                        #region [ F6 SCROLL ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F6))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.OverrideScrollMode = true;
                            switch ((int)TJAPlayer3.app.ConfigToml.ScrollMode)
                            {
                                case 0:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.BMSCROLL;
                                    break;
                                case 1:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.HBSCROLL;
                                    break;
                                case 2:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.REGULSPEED;
                                    break;
                                case 3:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.Normal;
                                    TJAPlayer3.app.ConfigToml.OverrideScrollMode = false;
                                    break;
                            }
                        }
                        #endregion
                        #region[ F7 Reg.Speed DOWN ]
                        this.ctキー反復用.Left.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.F7) && (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.REGULSPEED),
                            new CCounter.DGキー処理(
                            () =>
                            {
                                TJAPlayer3.app.ConfigToml.RegSpeedBPM = Math.Max(TJAPlayer3.app.ConfigToml.RegSpeedBPM - 1, 1);
                            }));
                        #endregion
                        #region[ F8 Reg.Speed UP ]
                        this.ctキー反復用.Right.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.F8) && (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.REGULSPEED),
                            new CCounter.DGキー処理(
                            () =>
                            {
                                TJAPlayer3.app.ConfigToml.RegSpeedBPM = Math.Min(TJAPlayer3.app.ConfigToml.RegSpeedBPM + 1, 9999);
                            }));
                        #endregion
                        #region [ Decide ]
                        if (((TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed)) ||
                                (TJAPlayer3.app.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return))) && !this.actDifficultySelect.選択済み[0] && !this.actChangeSE.bIsActive[0] && !this.actPlayOption.bIsActive[0])
                        {
                            if (this.actDifficultySelect.現在の選択行[0] == 0)
                            {
                                難易度から選曲へ戻る();
                            }
                            else if (this.actDifficultySelect.現在の選択行[0] == 1)
                            {
                                this.popupbool[0] = true;
                            }
                            else if (this.actDifficultySelect.現在の選択行[0] == 2)
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND音色選択]?.t再生する();
                                this.actChangeSE.tActivateChangeSE(0);
                            }
                            else
                            {
                                if (!(TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2))
                                {
                                    if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[0] == 6)
                                    {
                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                        this.actDifficultySelect.選択済み[0] = true;
                                        this.actDifficultySelect.確定された難易度[0] = (int)Difficulty.Edit;
                                    }
                                    else
                                    {


                                        if (this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[0] - 3])
                                        {
                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                            this.actDifficultySelect.選択済み[0] = true;
                                            this.actDifficultySelect.確定された難易度[0] = this.actDifficultySelect.現在の選択行[0] - 3;
                                        }
                                    }
                                }
                            }
                            this.難易度選択完了したか();
                        }
                        else if (((TJAPlayer3.app.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return)) && this.actDifficultySelect.選択済み[0] || TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 && !this.actDifficultySelect.選択済み[1] && !this.actChangeSE.bIsActive[1] && !this.actPlayOption.bIsActive[1])
                        {
                            if (this.actDifficultySelect.現在の選択行[1] == 0)
                            {
                                難易度から選曲へ戻る();
                            }
                            else if (this.actDifficultySelect.現在の選択行[1] == 1)
                            {
                                this.popupbool[1] = true;
                            }
                            else if (this.actDifficultySelect.現在の選択行[1] == 2)
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND音色選択]?.t再生する();
                                this.actChangeSE.tActivateChangeSE(1);
                            }
                            else
                            {
                                if (!(TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2))
                                {
                                    if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[1] == 6)
                                    {
                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                        this.actDifficultySelect.選択済み[1] = true;
                                        this.actDifficultySelect.確定された難易度[1] = (int)Difficulty.Edit;
                                    }
                                    else
                                    {
                                        if (this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[1] - 3])
                                        {
                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();

                                            this.actDifficultySelect.選択済み[1] = true;
                                            this.actDifficultySelect.確定された難易度[1] = this.actDifficultySelect.現在の選択行[1] - 3;
                                        }
                                    }
                                }
                            }
                            this.難易度選択完了したか();
                        }
                        #endregion
                        #region [ Right ]
                        if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow) || TJAPlayer3.app.Pad.bPressed(EPad.RBlue)) && !this.actDifficultySelect.選択済み[0] && !this.actChangeSE.bIsActive[0] && !this.actPlayOption.bIsActive[0])
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                            this.actDifficultySelect.現在の選択行[0]++;

                            if (this.actDifficultySelect.現在の選択行[0] > 6)
                            {
                                this.actDifficultySelect.現在の選択行[0] = 6;
                                this.actDifficultySelect.裏カウント[0]++;
                            }
                            else
                            {
                                this.actDifficultySelect.裏カウント[0] = 0;
                                this.actDifficultySelect.ct難易度拡大用[0].n現在の値 = 0;
                                this.actDifficultySelect.ct難易度拡大用[0].t時間Reset();
                            }
                            if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[0] == 6)
                            {
                                this.act曲リスト.n現在のアンカ難易度レベル[0] = 4;
                            }
                            else
                            {
                                if (this.actDifficultySelect.現在の選択行[0] >= 3 && this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[0] - 3])
                                    this.act曲リスト.n現在のアンカ難易度レベル[0] = this.actDifficultySelect.現在の選択行[0] - 3;
                            }
                        }
                        if (((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow) && this.actDifficultySelect.選択済み[0]) || TJAPlayer3.app.Pad.bPressed(EPad.RBlue2P)) && !this.actDifficultySelect.選択済み[1] && !this.actChangeSE.bIsActive[1] && !this.actPlayOption.bIsActive[1] && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                            this.actDifficultySelect.現在の選択行[1]++;

                            if (this.actDifficultySelect.現在の選択行[1] > 6)
                            {
                                this.actDifficultySelect.現在の選択行[1] = 6;
                                this.actDifficultySelect.裏カウント[1]++;
                            }
                            else
                            {
                                this.actDifficultySelect.裏カウント[1] = 0;
                                this.actDifficultySelect.ct難易度拡大用[1].n現在の値 = 0;
                                this.actDifficultySelect.ct難易度拡大用[1].t時間Reset();
                            }
                            if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[1] == 6)
                            {
                                this.act曲リスト.n現在のアンカ難易度レベル[1] = 4;
                            }
                            else
                            {
                                if (this.actDifficultySelect.現在の選択行[1] >= 3 && this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[1] - 3])
                                    this.act曲リスト.n現在のアンカ難易度レベル[1] = this.actDifficultySelect.現在の選択行[1] - 3;
                            }
                        }
                        #endregion
                        #region [ Left ]
                        if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow) || TJAPlayer3.app.Pad.bPressed(EPad.LBlue)) && !this.actDifficultySelect.選択済み[0] && !this.actChangeSE.bIsActive[0] && !this.actPlayOption.bIsActive[0])
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                            this.actDifficultySelect.現在の選択行[0]--;
                            if (this.actDifficultySelect.現在の選択行[0] < 0)
                            {
                                this.actDifficultySelect.現在の選択行[0] = 0;
                            }
                            else
                            {
                                this.actDifficultySelect.ct難易度拡大用[0].n現在の値 = 0;
                                this.actDifficultySelect.ct難易度拡大用[0].t時間Reset();
                            }

                            this.actDifficultySelect.裏カウント[0] = 0;

                            if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[0] == 6)
                            {
                                this.act曲リスト.n現在のアンカ難易度レベル[0] = 4;
                            }
                            else
                            {
                                if (this.actDifficultySelect.現在の選択行[0] >= 3 && this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[0] - 3])
                                    this.act曲リスト.n現在のアンカ難易度レベル[0] = this.actDifficultySelect.現在の選択行[0] - 3;
                            }
                        }
                        if (((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow) && this.actDifficultySelect.選択済み[0]) || TJAPlayer3.app.Pad.bPressed(EPad.LBlue2P)) && !this.actDifficultySelect.選択済み[1] && !this.actChangeSE.bIsActive[1] && !this.actPlayOption.bIsActive[1] && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
                            this.actDifficultySelect.現在の選択行[1]--;
                            if (this.actDifficultySelect.現在の選択行[1] < 0)
                            {
                                this.actDifficultySelect.現在の選択行[1] = 0;
                            }
                            else
                            {
                                this.actDifficultySelect.ct難易度拡大用[1].n現在の値 = 0;
                                this.actDifficultySelect.ct難易度拡大用[1].t時間Reset();
                            }

                            this.actDifficultySelect.裏カウント[1] = 0;

                            if (this.actDifficultySelect.裏表示 && this.actDifficultySelect.現在の選択行[1] == 6)
                            {
                                this.act曲リスト.n現在のアンカ難易度レベル[1] = 4;
                            }
                            else
                            {
                                if (this.actDifficultySelect.現在の選択行[1] >= 3 && this.act曲リスト.r現在選択中のスコア.譜面情報.b譜面が存在する[this.actDifficultySelect.現在の選択行[1] - 3])
                                    this.act曲リスト.n現在のアンカ難易度レベル[1] = this.actDifficultySelect.現在の選択行[1] - 3;
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                #region[通常状態のキー入力]
                else if (現在の選曲画面状況 == E選曲画面.通常)
                {
                    if (!this.actSortSongs.bIsActivePopupMenu)
                    {
                        #region [ ESC ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape) && (this.act曲リスト.r現在選択中の曲 is not null))
                        {
                            if (this.act曲リスト.r現在選択中の曲.r親ノード is null)
                            {   // [ESC]
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                                this.eFadeOut完了時の戻り値 = E戻り値.タイトルに戻る;
                                this.actFIFO.tFadeOut開始();
                                base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
                                return 0;
                            }
                            else
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                                this.act曲リスト.tBOXを出る();
                            }
                            this.actPresound.tサウンドの停止MT();
                        }

                        #endregion
                        #region [ Shift-F1: CONFIG画面 ]
                        if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightShift) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftShift)) &&
                            TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F1))
                        {   // [SHIFT] + [F1] CONFIG
                            this.GotoConfig();
                            return 0;
                        }
                        #endregion
#if PLAYABLE
                        #region [ F3 1PオートON/OFF ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F3))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0];
                        }
                        #endregion
                        #region [ F4 2PオートON/OFF ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F4))
                        {
                            if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount > 1)
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                                TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1];
                            }
                        }
                        #endregion
#endif
                        #region [ F5 スーパーハード ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F5))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.SuperHard = !TJAPlayer3.app.ConfigToml.SuperHard;
                        }
                        #endregion
                        #region [ F6 SCROLL ]
                        if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F6))
                        {
                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            TJAPlayer3.app.ConfigToml.OverrideScrollMode = true;
                            switch ((int)TJAPlayer3.app.ConfigToml.ScrollMode)
                            {
                                case 0:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.BMSCROLL;
                                    break;
                                case 1:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.HBSCROLL;
                                    break;
                                case 2:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.REGULSPEED;
                                    break;
                                case 3:
                                    TJAPlayer3.app.ConfigToml.ScrollMode = EScrollMode.Normal;
                                    TJAPlayer3.app.ConfigToml.OverrideScrollMode = false;
                                    break;
                            }
                        }
                        #endregion
                        #region[ F7 Reg.Speed DOWN ]
                        this.ctキー反復用.Left.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.F7) && (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.REGULSPEED),
                            new CCounter.DGキー処理(
                            () =>
                            {
                                TJAPlayer3.app.ConfigToml.RegSpeedBPM = Math.Max(TJAPlayer3.app.ConfigToml.RegSpeedBPM - 1, 1);
                            }));
                        #endregion
                        #region[ F8 Reg.Speed UP ]
                        this.ctキー反復用.Right.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.F8) && (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.REGULSPEED),
                            new CCounter.DGキー処理(
                            () =>
                            {
                                TJAPlayer3.app.ConfigToml.RegSpeedBPM = Math.Min(TJAPlayer3.app.ConfigToml.RegSpeedBPM + 1, 9999);
                            }));
                        #endregion
                        if (this.act曲リスト.r現在選択中の曲 is not null)
                        {
                            #region [ Decide ]
                            if (((TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed)) || (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 ||
                                    (TJAPlayer3.app.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return))))
                            {
                                if (this.act曲リスト.r現在選択中の曲 is not null)
                                {
                                    switch (this.act曲リスト.r現在選択中の曲.eNodeType)
                                    {
                                        case C曲リストノード.ENodeType.SCORE:
                                            if (!((this.n現在選択中の曲の難易度[0] == (int)Difficulty.Dan || this.n現在選択中の曲の難易度[0] == (int)Difficulty.Tower) && (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 || TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)))
                                            {
                                                if (this.n現在選択中の曲の難易度[0] == (int)Difficulty.Dan && TJAPlayer3.app.Tx.Difficulty_Dan_Box is not null && TJAPlayer3.app.Tx.Difficulty_Dan_Box_Selecting is not null)
                                                {
                                                    if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDDANするカッ].b読み込み成功)
                                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDDANするカッ].t再生する();
                                                    else
                                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                    DanSelectingRow = 0;
                                                    現在の選曲画面状況 = E選曲画面.Dan選択;
                                                }
                                                else if (this.n現在選択中の曲の難易度[0] == (int)Difficulty.Tower || this.n現在選択中の曲の難易度[0] == (int)Difficulty.Dan)
                                                {
                                                    if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].b読み込み成功)
                                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].t再生する();
                                                    else
                                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                    this.t曲を選択する();
                                                }
                                                else
                                                {
                                                    if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.Tab) && !(TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 && TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード))
                                                    {
                                                        if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].b読み込み成功)
                                                        {
                                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].t再生する();
                                                        }
                                                        else
                                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                        this.t曲を選択する();
                                                    }
                                                    else
                                                    {
                                                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                        現在の選曲画面状況 = E選曲画面.難易度選択In;
                                                    }
                                                }
                                            }
                                            break;
                                        case C曲リストノード.ENodeType.BOX:
                                            {
                                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                                this.act曲リスト.tBOXに入る();
                                            }
                                            break;
                                        case C曲リストノード.ENodeType.BACKBOX:
                                            {
                                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                                                this.act曲リスト.tBOXを出る();
                                            }
                                            break;
                                        case C曲リストノード.ENodeType.RANDOM:
                                            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                                            this.t曲をランダム選択する();
                                            break;
                                    }
                                }
                            }
                            #endregion
                            #region [ Up ]
                            this.ctキー反復用.Up.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftArrow), new CCounter.DGキー処理(this.tカーソルを上へ移動する));
                            if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue) || TJAPlayer3.app.Pad.bPressed(EPad.LBlue2P) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
                            {
                                this.tカーソルを上へ移動する();
                            }
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.PageDown))
                            {
                                this.tカーソルを上へスキップする();
                            }
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Home))
                            {
                                this.tカーソルをフォルダのはじめへスキップする();
                            }
                            #endregion
                            #region [ Down ]
                            this.ctキー反復用.Down.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightArrow), new CCounter.DGキー処理(this.tカーソルを下へ移動する));
                            if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue) || TJAPlayer3.app.Pad.bPressed(EPad.RBlue2P) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
                            {
                                this.tカーソルを下へ移動する();
                            }
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.PageUp))
                            {
                                this.tカーソルを下へスキップする();
                            }
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.End))
                            {
                                this.tカーソルをフォルダの最後へスキップする();
                            }
                            #endregion
                            #region [ Sort ]
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Space))
                            {
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                                this.actSortSongs.tActivatePopupMenu(ref this.act曲リスト);
                            }
                            #endregion
                            #region [ 上: 難易度変更(上) ]
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.UpArrow))
                            {
                                Debug.WriteLine("ドラムス難易度変更");
                                if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl))
                                    this.act曲リスト.t難易度レベルをひとつ進める(1);
                                else
                                    this.act曲リスト.t難易度レベルをひとつ進める(0);
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            }
                            #endregion
                            #region [ 下: 難易度変更(下) ]
                            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.DownArrow))
                            {
                                Debug.WriteLine("ドラムス難易度変更");
                                if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl))
                                    this.act曲リスト.t難易度レベルをひとつ戻す(1);
                                else
                                    this.act曲リスト.t難易度レベルをひとつ戻す(0);
                                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();
                            }
                            #endregion
                        }
                    }
                }
                #endregion

                #region [ Minus & Equals Sound Group Level ]
                KeyboardSoundGroupLevelControlHandler.Handle(
                    TJAPlayer3.app.InputManager.Keyboard, TJAPlayer3.SoundGroupLevelController, TJAPlayer3.app.Skin, true);
                #endregion

                this.actSortSongs.t進行描画();
            }
            switch (base.eフェーズID)
            {
                case CStage.Eフェーズ.共通_FadeIn:
                    if (this.actFIFO.On進行描画() != 0)
                    {
                        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
                    }
                    break;

                case CStage.Eフェーズ.共通_FadeOut:
                    if (this.actFIFO.On進行描画() == 0)
                    {
                        break;
                    }
                    return (int)this.eFadeOut完了時の戻り値;

                case CStage.Eフェーズ.選曲_結果画面からのFadeIn:
                    if (this.actFIfromResult.On進行描画() != 0)
                    {
                        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
                    }
                    break;

                case CStage.Eフェーズ.選曲_NowLoading画面へのFadeOut:
                    if (this.actFOtoNowLoading.On進行描画() == 0)
                    {
                        break;
                    }
                    return (int)this.eFadeOut完了時の戻り値;
            }
        }
        return 0;
    }

    public enum E戻り値 : int
    {
        継続,
        タイトルに戻る,
        選曲した,
        オプション呼び出し,
        コンフィグ呼び出し,
        スキン変更
    }
    public enum E選曲画面 : int
    {
        通常,
        Dan選択,//2020.05.25 Mr-Ojii Danの選択用
        難易度選択In,//2020.05.25 Mr-Ojii DifficultySelectを追加したとき用
        難易度選択,
        難易度選択Out
    }
    // その他

    #region [ private ]
    //-----------------
    internal E選曲画面 現在の選曲画面状況 = E選曲画面.通常;
    private int DanSelectingRow = 0;

    private void GotoConfig()
    {
        actChangeSE.tDeativateChangeSE(0);
        actChangeSE.tDeativateChangeSE(1);
        actPlayOption.tDeativatePopupMenu(0);
        actPlayOption.tDeativatePopupMenu(1);
        this.actPresound.tサウンドの停止MT();
        this.eFadeOut完了時の戻り値 = E戻り値.コンフィグ呼び出し;  // #24525 2011.3.16 yyagi: [SHIFT]-[F1]でCONFIG呼び出し
        this.actFIFO.tFadeOut開始();
        base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
    }

    private void 難易度選択完了したか()
    {
        if (!完全に選択済み)
        {
            if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)
            {
                if (this.actDifficultySelect.選択済み[0] && this.actDifficultySelect.選択済み[1])
                {
                    if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].b読み込み成功)
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].t再生する();
                    else
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                    this.t曲を選択する(this.actDifficultySelect.確定された難易度[0], this.actDifficultySelect.確定された難易度[1]);
                    完全に選択済み = true;
                }
            }
            else
            {
                if (this.actDifficultySelect.選択済み[0])
                {
                    if (TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].b読み込み成功)
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲決定音].t再生する();
                    else
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();
                    this.t曲を選択する(this.actDifficultySelect.確定された難易度[0]);
                    完全に選択済み = true;
                }
            }
        }
    }

    private void 難易度から選曲へ戻る()
    {
        if (!this.actChangeSE.bIsActive[0] && !this.actChangeSE.bIsActive[1] && !this.actPlayOption.bIsActive[0] && !this.actPlayOption.bIsActive[1])
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
            this.actDifficultySelect.選択済み[0] = false;
            this.actDifficultySelect.選択済み[1] = false;
            this.actDifficultySelect.b開いた直後 = true;
            現在の選曲画面状況 = E選曲画面.難易度選択Out;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STキー反復用カウンタ
    {
        public CCounter Up;
        public CCounter Down;
        public CCounter Left;
        public CCounter Right;
        public CCounter this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.Up;

                    case 1:
                        return this.Down;

                    case 2:
                        return this.Left;

                    case 3:
                        return this.Right;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.Up = value;
                        return;

                    case 1:
                        this.Down = value;
                        return;

                    case 2:
                        this.Left = value;
                        return;

                    case 3:
                        this.Right = value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }
    }
    internal CActFIFOBlack actFIFO;
    private CActFIFOBlack actFIfromResult;
    //private CActFIFOBlack actFOtoNowLoading;
    private CActFIFOStart actFOtoNowLoading;
    private CActSelectPresound actPresound;
    public CActSelectHistoryPanel actHistoryPanel;
    public CActSelect曲リスト act曲リスト;
    internal CActSelectDifficultySelect actDifficultySelect;
    private bool 完全に選択済み = false;

    private CActSortSongs actSortSongs;
    private CActSelectPlayOption actPlayOption;
    internal CActSelectChangeSE actChangeSE;

    private bool bBGM再生済み;
    private STキー反復用カウンタ ctキー反復用;
    public CCounter ct登場時アニメ用共通;
    private CCounter ct背景スクロール用タイマー;
    private CCounter ctカウントダウン用タイマー;
    internal CCounter ctDifficultySelectIN用タイマー;
    internal CCounter ctDifficultySelectINバー拡大用タイマー;
    internal CCounter ctDifficultySelectOUT用タイマー;
    internal E戻り値 eFadeOut完了時の戻り値;

    public void MouseWheel(float i)
    {
        if (this.現在の選曲画面状況 == E選曲画面.通常)
        {
            if (i < 0)
                this.tカーソルを上へ移動する();
            else
                this.tカーソルを下へ移動する();
        }
    }
    private void tカーソルを下へ移動する()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
        this.act曲リスト.t次に移動();
    }
    private void tカーソルを上へ移動する()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
        this.act曲リスト.t前に移動();

    }
    private void tカーソルを下へスキップする()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND選曲スキップ].t再生する();
        this.act曲リスト.tかなり次に移動();
    }
    private void tカーソルを上へスキップする()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND選曲スキップ].t再生する();
        this.act曲リスト.tかなり前に移動();
    }
    private void tカーソルをフォルダのはじめへスキップする()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND選曲スキップ].t再生する();
        this.act曲リスト.tフォルダのはじめに移動();
    }
    private void tカーソルをフォルダの最後へスキップする()
    {
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND選曲スキップ].t再生する();
        this.act曲リスト.tフォルダの最後に移動();
    }
    private void t曲をランダム選択する()
    {
        List<C曲リストノード> list = this.t指定された曲が存在する場所の曲を列挙する_子リスト含む(this.act曲リスト.r現在選択中の曲);
        this.act曲リスト.RandomSelect(list[Random.Shared.Next(0, list.Count - 1)]);

    }
    private void t曲を選択する()
    {
        this.r確定された曲 = this.act曲リスト.r現在選択中の曲;
        this.r確定されたスコア = this.act曲リスト.r現在選択中のスコア;
        this.n確定された曲の難易度[0] = this.act曲リスト.n現在選択中の曲の難易度レベル[0];
        this.n確定された曲の難易度[1] = this.act曲リスト.n現在選択中の曲の難易度レベル[1];
        if ((this.r確定された曲 is not null) && (this.r確定されたスコア is not null))
        {
            this.eFadeOut完了時の戻り値 = E戻り値.選曲した;
            this.actFOtoNowLoading.tFadeOut開始();				// #27787 2012.3.10 yyagi 曲決定時の画面FadeOutの省略
            base.eフェーズID = CStage.Eフェーズ.選曲_NowLoading画面へのFadeOut;
        }
        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].t再生する();
    }
    public void t曲を選択する(int nCurrentLevel)
    {
        this.r確定された曲 = this.act曲リスト.r現在選択中の曲;
        this.r確定されたスコア = this.act曲リスト.r現在選択中のスコア;
        this.n確定された曲の難易度[0] = nCurrentLevel;
        this.n確定された曲の難易度[1] = nCurrentLevel;
        if ((this.r確定された曲 is not null) && (this.r確定されたスコア is not null))
        {
            this.eFadeOut完了時の戻り値 = E戻り値.選曲した;
            this.actFOtoNowLoading.tFadeOut開始();				// #27787 2012.3.10 yyagi 曲決定時の画面FadeOutの省略
            base.eフェーズID = CStage.Eフェーズ.選曲_NowLoading画面へのFadeOut;
        }

        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].t停止する();
    }
    public void t曲を選択する(int nCurrentLevel, int nCurrentLevel2)
    {
        this.r確定された曲 = this.act曲リスト.r現在選択中の曲;
        this.r確定されたスコア = this.act曲リスト.r現在選択中のスコア;
        this.n確定された曲の難易度[0] = nCurrentLevel;
        this.n確定された曲の難易度[1] = nCurrentLevel2;
        if ((this.r確定された曲 is not null) && (this.r確定されたスコア is not null))
        {
            this.eFadeOut完了時の戻り値 = E戻り値.選曲した;
            this.actFOtoNowLoading.tFadeOut開始();                // #27787 2012.3.10 yyagi 曲決定時の画面FadeOutの省略
            base.eフェーズID = CStage.Eフェーズ.選曲_NowLoading画面へのFadeOut;
        }

        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].t停止する();
    }
    private List<C曲リストノード> t指定された曲が存在する場所の曲を列挙する_子リスト含む(C曲リストノード song)
    {
        List<C曲リストノード> list = new List<C曲リストノード>();
        song = song.r親ノード;
        if ((song is null) && (TJAPlayer3.SongsManager.list曲ルート.Count > 0))
        {
            foreach (C曲リストノード c曲リストノード in TJAPlayer3.SongsManager.list曲ルート)
            {
                if ((c曲リストノード.eNodeType == C曲リストノード.ENodeType.SCORE))
                {
                    list.Add(c曲リストノード);
                }
                if ((c曲リストノード.list子リスト is not null) && TJAPlayer3.app.ConfigToml.SongSelect.RandomIncludeSubBox)
                {
                    this.t指定された曲の子リストの曲を列挙する_孫リスト含む(c曲リストノード, ref list);
                }
            }
            return list;
        }
        this.t指定された曲の子リストの曲を列挙する_孫リスト含む(song, ref list);
        return list;
    }
    private void t指定された曲の子リストの曲を列挙する_孫リスト含む(C曲リストノード r親, ref List<C曲リストノード> list)
    {
        if ((r親 is not null) && (r親.list子リスト is not null))
        {
            foreach (C曲リストノード c曲リストノード in r親.list子リスト)
            {
                if ((c曲リストノード.eNodeType == C曲リストノード.ENodeType.SCORE))
                {
                    list.Add(c曲リストノード);
                }
                if ((c曲リストノード.list子リスト is not null) && TJAPlayer3.app.ConfigToml.SongSelect.RandomIncludeSubBox)
                {
                    this.t指定された曲の子リストの曲を列挙する_孫リスト含む(c曲リストノード, ref list);
                }
            }
        }
    }

    private int This_counter;

    private bool[] popupbool = { false, false };

    //-----------------
    #endregion
}
