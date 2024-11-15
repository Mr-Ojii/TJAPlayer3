using FDK;

namespace TJAPlayer3;

internal class CStageResult : CStage
{
    // プロパティ

    public CScoreJson.CRecord[] cRecords;


    // コンストラクタ

    public CStageResult()
    {
        this.cRecords = new CScoreJson.CRecord[2];
        base.eStageID = CStage.EStage.Result;
        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
        base.listChildren.Add(this.actParameterPanel = new CActResultParameterPanel());
        base.listChildren.Add(this.actSongBar = new CActResultSongBar());
        base.listChildren.Add(this.actFI = new CActFIFOResult());
        base.listChildren.Add(this.actFO = new CActFIFOBlack());
    }

    // CStage 実装

    public override void On活性化()
    {
        Trace.TraceInformation("結果ステージを活性化します。");
        Trace.Indent();
        try
        {
            #region [ 初期化 ]
            //---------------------
            this.eFadeOut完了時の戻り値 = E戻り値.継続;
            this.bアニメが完了 = false;
            //---------------------
            #endregion

            string str = TJAPlayer3.DTX[0].strFilenameの絶対パス + ".score.json";
            CScoreJson json = CScoreJson.Load(str);

            for (int i = 0; i < 1; i++)
            {
                if (!this.cRecords[i].Auto)
                {
                    #region [ .score.json の作成と出力 ]
                    //王冠の更新
                    if (TJAPlayer3.stage選曲.n確定された曲の難易度[i] != (int)Difficulty.Dan)
                    {
                        if (this.cRecords[i].Gauge < 80)
                            json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 0);
                        else if (this.cRecords[i].MissCount != 0 && this.cRecords[i].BadCount != 0)
                            json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 1);
                        else if (this.cRecords[i].GoodCount != 0)
                            json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 2);
                        else
                            json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 3);
                    }
                    else
                    {
                        switch (TJAPlayer3.stage演奏ドラム画面.actDan.GetExamStatus(this.cRecords[i].DanC, this.cRecords[i].DanCGauge))
                        {
                            case Exam.Status.Success:
                                json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 1);
                                break;
                            case Exam.Status.Better_Success:
                                json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 2);
                                break;
                            default:
                                json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown = Math.Max(json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown, 0);
                                break;
                        }
                    }

                    //LastPlayの更新
                    json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].LastPlay = this.cRecords[i];

                    //HiScoreの更新
                    {
                        int j;
                        for (j = 0; j < json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore.Count; j++)
                            if (this.cRecords[i].Score >= json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore[j].Score)
                                break;

                        json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore.Insert(j, this.cRecords[i]);
                    }

                    //3個以上だった場合、3個に丸める
                    while (json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore.Count > 3)
                        json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore.RemoveAt(3);

                    //クリアしていた場合、クリアのカウントを増やす
                    if (this.cRecords[i].Gauge >= 80)
                        json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].ClearCount++;

                    //書き出し
                    json.Save(str);
                    #endregion

                    #region [ 選曲画面の譜面情報の更新 ]
                    Cスコア cスコア = TJAPlayer3.stage選曲.r確定されたスコア;
                    cスコア.譜面情報.nCrown[TJAPlayer3.stage選曲.n確定された曲の難易度[i]] = json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].Crown;
                    for (int k = 0; k < json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore.Count; k++)
                    {
                        cスコア.譜面情報.nHiScore[TJAPlayer3.stage選曲.n確定された曲の難易度[i]][k] = (int)json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore[k].Score;
                        cスコア.譜面情報.strHiScorerName[TJAPlayer3.stage選曲.n確定された曲の難易度[i]][k] = json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[i]].HiScore[k].PlayerName;
                    }
                    TJAPlayer3.stage選曲.r確定されたスコア = cスコア;
                    #endregion

                }
            }

            string Details = TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION;

            // Discord Presenseの更新
            TJAPlayer3.app.Discord.Update(
                Details.Substring(0, Math.Min(127, Details.Length)),
                "Result" + (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] == true ? " (Auto)" : "")
            );

            this.ctMountainAndClear = new CCounter(0, 1655, 1, TJAPlayer3.app.Timer);

            base.On活性化();
        }
        finally
        {
            Trace.TraceInformation("結果ステージの活性化を完了しました。");
            Trace.Unindent();
        }
    }
    public override void On非活性化()
    {
        if (this.ct登場用 is not null)
        {
            this.ct登場用 = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                this.ct登場用 = new CCounter(0, 100, 5, TJAPlayer3.app.Timer);
                this.actFI.tFadeIn開始();
                base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
                base.b初めての進行描画 = false;
            }
            this.bアニメが完了 = true;
            if (this.ct登場用.b進行中)
            {
                this.ct登場用.t進行();
                if (this.ct登場用.b終了値に達した)
                {
                    this.ct登場用.t停止();
                }
                else
                {
                    this.bアニメが完了 = false;
                }
            }

            // 描画
            if (TJAPlayer3.app.ConfigToml.EnableSkinV2)
            {
                if (TJAPlayer3.app.Tx.Result_v2_Background is not null)
                {
                    if (TJAPlayer3.app.Tx.Result_v2_Background[0] is not null)
                        TJAPlayer3.app.Tx.Result_v2_Background[0].t2D描画(TJAPlayer3.app.Device, 0, 0);
                    for (int ind = 0; ind < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; ind++)
                    {
                        if (this.cRecords[ind].Gauge >= 80.0 && TJAPlayer3.app.Tx.Result_v2_Background[1] is not null)
                        {
                            TJAPlayer3.app.Tx.Result_v2_Background[1].Opacity = Math.Min(this.ctMountainAndClear.n現在の値, 255);
                            TJAPlayer3.app.Tx.Result_v2_Background[1].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount * ind, 0, new Rectangle(TJAPlayer3.app.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount * ind, 0, TJAPlayer3.app.Tx.Result_v2_Background[1].szTextureSize.Width / TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount, TJAPlayer3.app.Tx.Result_v2_Background[1].szTextureSize.Height));
                        }
                    }
                }
                if (TJAPlayer3.app.Tx.Result_v2_Mountain is not null && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1)
                {
                    if (TJAPlayer3.app.Tx.Result_v2_Mountain[0] is not null)
                        if (this.ctMountainAndClear.n現在の値 <= 255 || this.cRecords[0].Gauge < 80.0)
                            TJAPlayer3.app.Tx.Result_v2_Mountain[0].t2D描画(TJAPlayer3.app.Device, 0, 0);
                    if (this.cRecords[0].Gauge >= 80.0 && TJAPlayer3.app.Tx.Result_v2_Mountain[1] is not null)
                    {
                        TJAPlayer3.app.Tx.Result_v2_Mountain[1].Opacity = Math.Min(this.ctMountainAndClear.n現在の値, 255);
                        if (this.ctMountainAndClear.n現在の値 <= 255 || this.ctMountainAndClear.n現在の値 == this.ctMountainAndClear.n終了値)
                        {
                            TJAPlayer3.app.Tx.Result_v2_Mountain[1].vcScaling.Y = 1f;
                        }
                        else if (this.ctMountainAndClear.n現在の値 <= 555)
                        {
                            TJAPlayer3.app.Tx.Result_v2_Mountain[1].vcScaling.Y = 1.0f - (this.ctMountainAndClear.n現在の値 - 255) / 300f * 0.4f;
                        }
                        else if (this.ctMountainAndClear.n現在の値 <= 1155)
                        {
                            //600msで150degなので4で割る
                            TJAPlayer3.app.Tx.Result_v2_Mountain[1].vcScaling.Y = (float)((Math.Sin((this.ctMountainAndClear.n現在の値 - 555) / 4.0 / 180.0 * Math.PI) * 0.8f) + 0.6f);
                        }
                        else
                        {
                            TJAPlayer3.app.Tx.Result_v2_Mountain[1].vcScaling.Y = (float)Math.Sin((this.ctMountainAndClear.n現在の値 - 1155) / 500f * Math.PI) * 0.3f + 1f;
                        }
                        TJAPlayer3.app.Tx.Result_v2_Mountain[1].t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, TJAPlayer3.app.LogicalSize.Width / 2, TJAPlayer3.app.LogicalSize.Height);
                    }
                }
                if (TJAPlayer3.app.Tx.Result_v2_Header is not null)
                {
                    TJAPlayer3.app.Tx.Result_v2_Header.t2D描画(TJAPlayer3.app.Device, 0, 0);
                }
            }
            else
            {
                if (TJAPlayer3.app.Tx.Result_Background is not null)
                {
                    TJAPlayer3.app.Tx.Result_Background.t2D描画(TJAPlayer3.app.Device, 0, 0);
                }
                if (TJAPlayer3.app.Tx.Result_Header is not null)
                {
                    TJAPlayer3.app.Tx.Result_Header.t2D描画(TJAPlayer3.app.Device, 0, 0);
                }
            }
            if (this.actParameterPanel.On進行描画() == 0)
            {
                this.bアニメが完了 = false;
                this.ctMountainAndClear.n現在の値 = 0;
                this.ctMountainAndClear.t時間Reset();
            }
            else
            {
                this.ctMountainAndClear.t進行();
                if (!this.ctMountainAndClear.b終了値に達した)
                    this.bアニメが完了 = false;
            }

            if (this.actSongBar.On進行描画() == 0)
            {
                this.bアニメが完了 = false;
            }

            #region ネームプレート
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                if (TJAPlayer3.app.Tx.NamePlate[i] is not null)
                {
                    TJAPlayer3.app.Tx.NamePlate[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.ConfigToml.EnableSkinV2 ? TJAPlayer3.app.Skin.SkinConfig.Result.v2NamePlateX[i] : TJAPlayer3.app.Skin.SkinConfig.Result.NamePlateX[i], TJAPlayer3.app.ConfigToml.EnableSkinV2 ? TJAPlayer3.app.Skin.SkinConfig.Result.v2NamePlateY[i] : TJAPlayer3.app.Skin.SkinConfig.Result.NamePlateY[i]);
                }
            }
            #endregion

            if (base.eフェーズID == CStage.Eフェーズ.共通_FadeIn)
            {
                if (this.actFI.On進行描画() != 0)
                {
                    base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
                }
            }
            else if ((base.eフェーズID == CStage.Eフェーズ.共通_FadeOut))			//&& ( this.actFO.On進行描画() != 0 ) )
            {
                return (int)this.eFadeOut完了時の戻り値;
            }

            // キー入力

            if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return) || TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed) || (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2) && !this.bアニメが完了)
            {
                this.actFI.tFadeIn完了();                 // #25406 2011.6.9 yyagi
                this.actParameterPanel.tアニメを完了させる();
                this.actSongBar.tアニメを完了させる();
                this.ct登場用.t停止();
            }
            if (base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
            {
                if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                    this.actFO.tFadeOut開始();
                    base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
                    this.eFadeOut完了時の戻り値 = E戻り値.完了;
                }
                if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return) || TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed) || (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2) && this.bアニメが完了)
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                    //							this.actFO.tFadeOut開始();
                    base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
                    this.eFadeOut完了時の戻り値 = E戻り値.完了;
                }
            }

        }
        return 0;
    }

    public enum E戻り値 : int
    {
        継続,
        完了
    }


    // その他

    #region [ private ]
    //-----------------
    private CCounter ct登場用;
    private CCounter ctMountainAndClear;
    private E戻り値 eFadeOut完了時の戻り値;
    private CActFIFOResult actFI;
    private CActFIFOBlack actFO;
    private CActResultParameterPanel actParameterPanel;
    private CActResultSongBar actSongBar;
    private bool bアニメが完了;

    #endregion
}
