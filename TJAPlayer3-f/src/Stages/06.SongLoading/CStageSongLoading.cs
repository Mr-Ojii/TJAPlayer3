using FDK;

namespace TJAPlayer3;

internal class CStageSongLoading : CStage
{
    // コンストラクタ

    public CStageSongLoading()
    {
        base.eStageID = CStage.EStage.SongLoading;
        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
    }

    // CStage 実装

    public override void On活性化()
    {
        Trace.TraceInformation("曲読み込みステージを活性化します。");
        Trace.Indent();
        try
        {
            this.strTitle = "";

            this.nBGM再生開始時刻 = -1;
            this.nBGMの総再生時間ms = 0;

            var 譜面情報 = TJAPlayer3.stage選曲.r確定されたスコア.譜面情報;
            this.strTitle = 譜面情報.Title;
            this.strSubTitle = 譜面情報.SubTitle;



            // For the moment, detect that we are performing
            // calibration via there being an actual single
            // player and the special song title and subtitle
            // of the .tja used to perform input calibration
            TJAPlayer3.IsPerformingCalibration =
                !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] &&
                TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1 &&
                strTitle == "Input Calibration" &&
                strSubTitle == "TJAPlayer3 Developers";


            this.ct待機 = new CCounter(0, 600, 5, TJAPlayer3.app.Timer);
            this.ct曲名表示 = new CCounter(1, 30, 30, TJAPlayer3.app.Timer);
            try
            {
                // When performing calibration, inform the player that
                // calibration is about to begin, rather than
                // displaying the song title and subtitle as usual.

                var タイトル = TJAPlayer3.IsPerformingCalibration
                    ? "Input calibration is about to begin."
                    : this.strTitle;

                var サブタイトル = TJAPlayer3.IsPerformingCalibration
                    ? "Please play as accurately as possible."
                    : this.strSubTitle;

                this.txTitle?.Dispose();
                this.txTitle = null;
                this.txSubTitle?.Dispose();
                this.txSubTitle = null;

                if (!string.IsNullOrEmpty(タイトル))
                {
                    using (CFontRenderer pfTITLE = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleFontSize))
                    {
                        using (var bmpSongTitle = pfTITLE.DrawText(タイトル, TJAPlayer3.app.Skin.SkinConfig.SongLoading._TitleForeColor, TJAPlayer3.app.Skin.SkinConfig.SongLoading._TitleBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                        {
                            this.txTitle = TJAPlayer3.app.tCreateTexture(bmpSongTitle);
                            this.txTitle.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref txTitle, 710);
                        }
                    }

                    if (!string.IsNullOrEmpty(サブタイトル))
                    {
                        using (CFontRenderer pfSUBTITLE = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleFontSize))
                        {
                            using (var bmpSongSubTitle = pfSUBTITLE.DrawText(サブタイトル, TJAPlayer3.app.Skin.SkinConfig.SongLoading._SubTitleForeColor, TJAPlayer3.app.Skin.SkinConfig.SongLoading._SubTitleBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                            {
                                this.txSubTitle = TJAPlayer3.app.tCreateTexture(bmpSongSubTitle);
                            }
                        }
                    }
                }

            }
            catch (CTextureCreateFailedException e)
            {
                Trace.TraceError(e.ToString());
                this.txTitle?.Dispose();
                this.txTitle = null;
                this.txSubTitle?.Dispose();
                this.txSubTitle = null;
            }

            base.On活性化();
        }
        finally
        {
            Trace.TraceInformation("曲読み込みステージの活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override void On非活性化()
    {
        Trace.TraceInformation("曲読み込みステージを非活性化します。");
        Trace.Indent();
        try
        {
            TJAPlayer3.t安全にDisposeする(ref this.txTitle);
            TJAPlayer3.t安全にDisposeする(ref this.txSubTitle);
            base.On非活性化();
        }
        finally
        {
            Trace.TraceInformation("曲読み込みステージの非活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override int On進行描画()
    {
        if (base.b活性化してない)
            return 0;

        #region [ 初めての進行描画 ]
        //-----------------------------
        if (base.b初めての進行描画)
        {
            if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲読込開始音].t再生する();
                this.nBGM再生開始時刻 = CSoundManager.rc演奏用タイマ.n現在時刻ms;
                this.nBGMの総再生時間ms = TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND曲読込開始音].n長さ_現在のサウンド;
            }
            //this.actFI.tFadeIn開始();							// #27787 2012.3.10 yyagi 曲読み込み画面のFadeInの省略
            base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
            base.b初めての進行描画 = false;

            nWAVcount = 1;
        }
        //-----------------------------
        #endregion

        #region [ ESC押下時は選曲画面に戻る ]
        if (this.ct待機 is null || this.ct曲名表示 is null || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
        {
            return (int)E曲読込画面の戻り値.読込中止;
        }
        #endregion

        this.ct待機.t進行();

        #region [ 背景、音符＋タイトル表示 ]
        //-----------------------------
        this.ct曲名表示.t進行();
        if (TJAPlayer3.app.ConfigToml.EnableSkinV2)
        {
            if (TJAPlayer3.app.Tx.SongLoading_v2_BG is not null)
                TJAPlayer3.app.Tx.SongLoading_v2_BG.t2D描画(TJAPlayer3.app.Device, 0, 0);
        }
        else
        {
            if (TJAPlayer3.app.Tx.SongLoading_BG is not null)
                TJAPlayer3.app.Tx.SongLoading_BG.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.LogicalSize.Width / 2, TJAPlayer3.app.LogicalSize.Height / 2);
        }

        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
        {
            if (TJAPlayer3.app.ConfigToml.EnableSkinV2)
            {
                if (TJAPlayer3.app.Tx.SongLoading_v2_Plate is not null)
                {
                    TJAPlayer3.app.Tx.SongLoading_v2_Plate.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2PlateReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        TJAPlayer3.app.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateY - (TJAPlayer3.app.Tx.SongLoading_v2_Plate.szTextureSize.Height / 2));
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2PlateReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        TJAPlayer3.app.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateX - TJAPlayer3.app.Tx.SongLoading_v2_Plate.szTextureSize.Width, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateY - (TJAPlayer3.app.Tx.SongLoading_v2_Plate.szTextureSize.Height / 2));
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.SongLoading_v2_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateX - (TJAPlayer3.app.Tx.SongLoading_v2_Plate.szTextureSize.Width / 2), TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2PlateY - (TJAPlayer3.app.Tx.SongLoading_v2_Plate.szTextureSize.Height / 2));
                    }
                }

                if (this.txTitle is not null)
                {
                    int nサブタイトル補正 = string.IsNullOrEmpty(TJAPlayer3.stage選曲.r確定されたスコア.譜面情報.SubTitle) ? 15 : 0;

                    this.txTitle.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2TitleReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2TitleReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleX - (this.txTitle.szTextureSize.Width * txTitle.vcScaling.X), TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                    else
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleX - ((this.txTitle.szTextureSize.Width * txTitle.vcScaling.X) / 2)), TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                }
                if (this.txSubTitle is not null)
                {
                    this.txSubTitle.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2SubTitleReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._v2SubTitleReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleX - (this.txSubTitle.szTextureSize.Width * txSubTitle.vcScaling.X), TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                    else
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleX - ((this.txSubTitle.szTextureSize.Width * txSubTitle.vcScaling.X) / 2)), TJAPlayer3.app.Skin.SkinConfig.SongLoading.v2SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                }
            }
            else
            {
                if (TJAPlayer3.app.Tx.SongLoading_Plate is not null)
                {
                    TJAPlayer3.app.Tx.SongLoading_Plate.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._PlateReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        TJAPlayer3.app.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateY - (TJAPlayer3.app.Tx.SongLoading_Plate.szTextureSize.Height / 2));
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._PlateReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        TJAPlayer3.app.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateX - TJAPlayer3.app.Tx.SongLoading_Plate.szTextureSize.Width, TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateY - (TJAPlayer3.app.Tx.SongLoading_Plate.szTextureSize.Height / 2));
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.SongLoading_Plate.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateX - (TJAPlayer3.app.Tx.SongLoading_Plate.szTextureSize.Width / 2), TJAPlayer3.app.Skin.SkinConfig.SongLoading.PlateY - (TJAPlayer3.app.Tx.SongLoading_Plate.szTextureSize.Height / 2));
                    }
                }

                if (this.txTitle is not null)
                {
                    int nサブタイトル補正 = string.IsNullOrEmpty(TJAPlayer3.stage選曲.r確定されたスコア.譜面情報.SubTitle) ? 15 : 0;

                    this.txTitle.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._TitleReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._TitleReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleX - (this.txTitle.szTextureSize.Width * txTitle.vcScaling.X), TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                    else
                    {
                        this.txTitle.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleX - ((this.txTitle.szTextureSize.Width * txTitle.vcScaling.X) / 2)), TJAPlayer3.app.Skin.SkinConfig.SongLoading.TitleY - (this.txTitle.szTextureSize.Height / 2) + nサブタイトル補正);
                    }
                }
                if (this.txSubTitle is not null)
                {
                    this.txSubTitle.Opacity = CConvert.nParsentTo255((this.ct曲名表示.n現在の値 / 30.0));
                    if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._SubTitleReferencePoint == CSkin.EReferencePoint.Left)
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleX, TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                    else if (TJAPlayer3.app.Skin.SkinConfig.SongLoading._SubTitleReferencePoint == CSkin.EReferencePoint.Right)
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleX - (this.txSubTitle.szTextureSize.Width * txSubTitle.vcScaling.X), TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                    else
                    {
                        this.txSubTitle.t2D描画(TJAPlayer3.app.Device, (TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleX - ((this.txSubTitle.szTextureSize.Width * txSubTitle.vcScaling.X) / 2)), TJAPlayer3.app.Skin.SkinConfig.SongLoading.SubTitleY - (this.txSubTitle.szTextureSize.Height / 2));
                    }
                }
            }
        }
        //-----------------------------
        #endregion

        switch (base.eフェーズID)
        {
            case CStage.Eフェーズ.共通_FadeIn:
                //if( this.actFI.On進行描画() != 0 )			    // #27787 2012.3.10 yyagi 曲読み込み画面のFadeInの省略
                // 必ず一度「CStaeg.Eフェーズ.共通_FadeIn」フェーズを経由させること。
                // さもないと、曲読み込みが完了するまで、曲読み込み画面が描画されない。
                base.eフェーズID = CStage.Eフェーズ.NOWLOADING_DTXファイルを読み込む;
                return (int)E曲読込画面の戻り値.継続;

            case CStage.Eフェーズ.NOWLOADING_DTXファイルを読み込む:
                {
                    timeBeginLoad = DateTime.Now;
                    string str = TJAPlayer3.stage選曲.r確定されたスコア.FileInfo.FileAbsolutePath;

                    CScoreJson json = CScoreJson.Load(str + ".score.json");

                    if ((TJAPlayer3.DTX[0] is not null) && TJAPlayer3.DTX[0].b活性化してる)
                        TJAPlayer3.DTX[0].On非活性化();

                    //if( CDTXMania.DTX is null )
                    {
                        bool bSession = TJAPlayer3.app.ConfigToml.PlayOption.Session &&
                                        TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 2 &&
                                        TJAPlayer3.stage選曲.n確定された曲の難易度[0] == TJAPlayer3.stage選曲.n確定された曲の難易度[1];

                        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
                            TJAPlayer3.DTX[i] = new CDTX(str, false, json.BGMAdjust, i, bSession);

                        if (TJAPlayer3.app.ConfigToml.OverrideScrollMode == false)
                            TJAPlayer3.app.ConfigToml.ScrollMode = TJAPlayer3.DTX[0].eScrollMode;

                        Trace.TraceInformation("----曲情報-----------------");
                        Trace.TraceInformation("TITLE: {0}", TJAPlayer3.DTX[0].TITLE);
                        Trace.TraceInformation("FILE: {0}", TJAPlayer3.DTX[0].strFilenameの絶対パス);
                        Trace.TraceInformation("---------------------------");

                        TimeSpan span = (TimeSpan)(DateTime.Now - timeBeginLoad);
                        Trace.TraceInformation("DTX読込所要時間:           {0}", span.ToString());

                        // 段位認定モード用。
                        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan && TJAPlayer3.DTX[0].List_DanSongs is not null)
                        {
                            for (int i = 0; i < TJAPlayer3.DTX[0].List_DanSongs.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(TJAPlayer3.DTX[0].List_DanSongs[i].Title))
                                {
                                    using (var pfTitle = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 32))
                                    {
                                        using (var bmpSongTitle = pfTitle.DrawText(TJAPlayer3.DTX[0].List_DanSongs[i].Title, TJAPlayer3.app.Skin.SkinConfig.Game.DanC._TitleForeColor, TJAPlayer3.app.Skin.SkinConfig.Game.DanC._TitleBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                                        {
                                            TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex = TJAPlayer3.app.tCreateTexture(bmpSongTitle);
                                            TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref TJAPlayer3.DTX[0].List_DanSongs[i].TitleTex, 710);
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(TJAPlayer3.DTX[0].List_DanSongs[i].SubTitle))
                                {
                                    using (var pfSubTitle = new CFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 19))
                                    {
                                        using (var bmpSongSubTitle = pfSubTitle.DrawText(TJAPlayer3.DTX[0].List_DanSongs[i].SubTitle, TJAPlayer3.app.Skin.SkinConfig.Game.DanC._SubTitleForeColor, TJAPlayer3.app.Skin.SkinConfig.Game.DanC._SubTitleBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                                        {
                                            TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex = TJAPlayer3.app.tCreateTexture(bmpSongSubTitle);
                                            TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex.vcScaling.X = TJAPlayer3.GetSongNameXScaling(ref TJAPlayer3.DTX[0].List_DanSongs[i].SubTitleTex, 710);
                                        }
                                    }
                                }

                            }
                        }
                    }

                    base.eフェーズID = CStage.Eフェーズ.NOWLOADING_WAVファイルを読み込む;
                    timeBeginLoadWAV = DateTime.Now;
                    return (int)E曲読込画面の戻り値.継続;
                }

            case CStage.Eフェーズ.NOWLOADING_WAVファイルを読み込む:
                {
                    int looptime = (TJAPlayer3.app.ConfigToml.Window.VSyncWait) ? 3 : 1;	// VSyncWait=ON時は1frame(1/60s)あたり3つ読むようにする
                    for (int i = 0; i < looptime && nWAVcount <= TJAPlayer3.DTX[0].listWAV.Count; i++)
                    {
                        if (TJAPlayer3.DTX[0].listWAV[nWAVcount].bUse)	// #28674 2012.5.8 yyagi
                        {
                            TJAPlayer3.DTX[0].tWAVの読み込み(TJAPlayer3.DTX[0].listWAV[nWAVcount]);
                        }
                        nWAVcount++;
                    }
                    if (nWAVcount > TJAPlayer3.DTX[0].listWAV.Count)
                    {
                        TimeSpan span = (TimeSpan)(DateTime.Now - timeBeginLoadWAV);
                        Trace.TraceInformation("WAV読込所要時間({0,4}):     {1}", TJAPlayer3.DTX[0].listWAV.Count, span.ToString());
                        timeBeginLoadWAV = DateTime.Now;

                        TJAPlayer3.DTX[0].PlanToAddMixerChannel();

                        for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
                        {
                            TJAPlayer3.DTX[nPlayer].t太鼓チップのランダム化(TJAPlayer3.app.ConfigToml.PlayOption._Random[nPlayer]);
                        }

                        TJAPlayer3.stage演奏ドラム画面.On活性化();

                        span = (TimeSpan)(DateTime.Now - timeBeginLoadWAV);

                        base.eフェーズID = CStage.Eフェーズ.NOWLOADING_BMPファイルを読み込む;
                    }
                    return (int)E曲読込画面の戻り値.継続;
                }

            case CStage.Eフェーズ.NOWLOADING_BMPファイルを読み込む:
                {
                    if (TJAPlayer3.app.ConfigToml.Game.Background.Movie)
                        TJAPlayer3.DTX[0].tAVIの読み込み();

                    TimeSpan span = (TimeSpan)(DateTime.Now - timeBeginLoad);
                    Trace.TraceInformation("総読込時間:                {0}", span.ToString());

                    TJAPlayer3.app.Timer.t更新();

                    base.eフェーズID = CStage.Eフェーズ.NOWLOADING_システムサウンドBGMの完了を待つ;
                    return (int)E曲読込画面の戻り値.継続;
                }

            case CStage.Eフェーズ.NOWLOADING_システムサウンドBGMの完了を待つ:
                {
                    long nCurrentTime = TJAPlayer3.app.Timer.n現在時刻ms;
                    if (nCurrentTime < this.nBGM再生開始時刻)
                        this.nBGM再生開始時刻 = nCurrentTime;

                    //						if ( ( nCurrentTime - this.nBGM再生開始時刻 ) > ( this.nBGMの総再生時間ms - 1000 ) )
                    if ((nCurrentTime - this.nBGM再生開始時刻) >= (this.nBGMの総再生時間ms))	// #27787 2012.3.10 yyagi 1000ms == FadeIn分の時間
                    {
                        base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
                    }
                    return (int)E曲読込画面の戻り値.継続;
                }

            case CStage.Eフェーズ.共通_FadeOut:
                if (this.ct待機.b終了値に達してない)
                    return (int)E曲読込画面の戻り値.継続;

                return (int)E曲読込画面の戻り値.読込完了;
        }
        return (int)E曲読込画面の戻り値.継続;
    }

    // その他

    #region [ private ]
    //-----------------
    private long nBGMの総再生時間ms;
    private long nBGM再生開始時刻;
    private string? strTitle;
    private string? strSubTitle;
    private CTexture? txTitle;
    private CTexture? txSubTitle;
    private DateTime timeBeginLoad;
    private DateTime timeBeginLoadWAV;
    private int nWAVcount;
    private CCounter? ct待機;
    private CCounter? ct曲名表示;
    //-----------------
    #endregion
}
