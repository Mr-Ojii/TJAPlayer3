using FDK;

namespace TJAPlayer3;

internal class Dan_Cert : CActivity
{
    /// <summary>
    /// 段位認定
    /// </summary>
    public Dan_Cert()
    {
    }

    //
    Dan_C[] Challenge;
    Dan_C Gauge;
    private bool IsVer2 = false;
    //

    public void Start(int number)
    {
        NowShowingNumber = number;

        for (int i = 0; i < 3; i++)
        {
            if (Challenge[i] is not null)
                if (Challenge[i].IsEnable)
                    Challenge[i].SetNowSongNum(number);
        }
        if (Gauge is not null)
            if (Gauge.IsEnable)
                Gauge.SetNowSongNum(number);

        Counter_In = new CCounter(0, 999, 1, TJAPlayer3.app.Timer);
        ScreenPoint = new double[] { TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] - (TJAPlayer3.app.Tx.DanC_Screen?.szTextureSize.Width ?? 1280) / 2, 1280 }; //2020.06.06 Mr-Ojii twopointzero氏のソースコードをもとに改良
        TJAPlayer3.stage演奏ドラム画面.ReSetScore(TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].ScoreInit, TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].ScoreDiff, 0);
        IsAnimating = true;

        string subtitle = (TJAPlayer3.app.ConfigToml.Game._SubtitleDispMode == ESubtitleDispMode.On || (TJAPlayer3.app.ConfigToml.Game._SubtitleDispMode == ESubtitleDispMode.Compliant && TJAPlayer3.DTX[0].SUBTITLEDisp)) ? TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitle : null;

        TJAPlayer3.stage演奏ドラム画面.actPanel.SetPanelString(TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].Title, subtitle, TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].Genre, 1 + NowShowingNumber + "曲目");
        Sound_Section?.t再生を開始する();
    }

    public override void On活性化()
    {
        NowShowingNumber = 0;
        Challenge = new Dan_C[3];
        Gauge = new Dan_C();
        for (int i = 0; i < 3; i++)
        {
            if (TJAPlayer3.DTX[0].Dan_C[i] is not null) Challenge[i] = new Dan_C(TJAPlayer3.DTX[0].Dan_C[i]);
        }
        if (TJAPlayer3.DTX[0].Dan_C_Gauge is not null) Gauge = new Dan_C(TJAPlayer3.DTX[0].Dan_C_Gauge);
        // 始点を決定する。
        ExamCount = 0;
        this.IsVer2 = false;
        for (int i = 0; i < 3; i++)
        {
            if (Challenge[i] is not null)
            {
                if (Challenge[i].IsEnable == true)
                    this.ExamCount++;
                if (Challenge[i].IsForEachSongs)
                    this.IsVer2 = true;
            }
        }

        if (Gauge.IsEnable == true)
        {
            this.IsVer2 = true;
        }

        for (int i = 0; i < 3; i++)
        {
            Status[i] = new ChallengeStatus();
            Status[i].Timer_Amount = new CCounter();
            Status[i].Timer_Gauge = new CCounter();
            Status[i].Timer_Failed = new CCounter();
        }
        IsEnded = false;

        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan)
            IsAnimating = true;

        Dan_Plate = TJAPlayer3.app.tCreateTexture(Path.GetDirectoryName(TJAPlayer3.DTX[0].strFilenameの絶対パス) + @"/Dan_Plate.png");
        Sound_Section = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Dan/Section.ogg"), ESoundGroup.SoundEffect);
        Sound_Failed = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Dan/Failed.ogg"), ESoundGroup.SoundEffect);
        base.On活性化();
    }

    public void Update()
    {
        if (Gauge is not null)
            if (Gauge.IsEnable)
            {
                Gauge.Update((int)TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[0]);
                var notesRemain = TJAPlayer3.DTX[0].nノーツ数[3] - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Bad) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Miss);
                // 残り音符数が0になったときに判断されるやつ
                if (notesRemain <= 0)
                {
                    if (Gauge.GetAmount() < Gauge.GetValue(false)) Gauge.SetReached(true);
                }
            }

        for (int i = 0; i < 3; i++)
        {
            if (Challenge[i] is null || !Challenge[i].IsEnable) return;
            var oldReached = Challenge[i].GetReached();
            var isChangedAmount = false;
            switch (Challenge[i].Type)
            {
                case Exam.Type.Gauge:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[0]);
                    break;
                case Exam.Type.JudgePerfect:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect);
                    break;
                case Exam.Type.JudgeGood:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good);
                    break;
                case Exam.Type.JudgeBad:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Miss + TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Bad);
                    break;
                case Exam.Type.Score:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.actScore.GetScore(0));
                    break;
                case Exam.Type.Roll:
                    isChangedAmount = Challenge[i].Update((int)(TJAPlayer3.stage演奏ドラム画面.GetRoll(0)));
                    break;
                case Exam.Type.Hit:
                    isChangedAmount = Challenge[i].Update((int)(TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect + TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good + TJAPlayer3.stage演奏ドラム画面.GetRoll(0)));
                    break;
                case Exam.Type.Combo:
                    isChangedAmount = Challenge[i].Update((int)TJAPlayer3.stage演奏ドラム画面.actCombo.n現在のコンボ数.Max[0]);
                    break;
                default:
                    break;
            }

            // 値が変更されていたらアニメーションを行う。
            if (isChangedAmount)
            {
                if (Status[i].Timer_Amount is not null && Status[i].Timer_Amount.b終了値に達してない)
                {
                    Status[i].Timer_Amount = new CCounter(0, 11, 12, TJAPlayer3.app.Timer);
                    Status[i].Timer_Amount.n現在の値 = 1;
                }
                else
                {
                    Status[i].Timer_Amount = new CCounter(0, 11, 12, TJAPlayer3.app.Timer);
                }
            }

            // 条件の達成見込みがあるかどうか判断する。
            if (Challenge[i].Range == Exam.Range.Less)
            {
                Challenge[i].SetReached(!Challenge[i].GetNowCleared(false));
            }
            else
            {
                var notesRemain = TJAPlayer3.DTX[0].nノーツ数[3] - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Bad) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Miss);
                // 残り音符数が0になったときに判断されるやつ
                if (notesRemain <= 0)
                {
                    // 残り音符数ゼロ
                    switch (Challenge[i].Type)
                    {
                        case Exam.Type.Gauge:
                            if (Challenge[i].GetAmount() < Challenge[i].GetValue(false)) Challenge[i].SetReached(true);
                            break;
                        case Exam.Type.Score:
                            if (Challenge[i].GetAmount() < Challenge[i].GetValue(false)) Challenge[i].SetReached(true);
                            break;
                        default:
                            // 何もしない
                            break;
                    }
                }
                // 常に監視されるやつ。
                switch (Challenge[i].Type)
                {
                    case Exam.Type.JudgePerfect:
                    case Exam.Type.JudgeGood:
                    case Exam.Type.JudgeBad:
                        if (notesRemain < (Challenge[i].GetValue(false) - Challenge[i].GetAmount())) Challenge[i].SetReached(true);
                        break;
                    case Exam.Type.Combo:
                        if (notesRemain + TJAPlayer3.stage演奏ドラム画面.actCombo.n現在のコンボ数[0] < ((Challenge[i].GetValue(false))) && TJAPlayer3.stage演奏ドラム画面.actCombo.n現在のコンボ数.Max[0] < (Challenge[i].GetValue(false))) Challenge[i].SetReached(true);
                        break;
                    default:
                        break;
                }


                // 音源が終了したやつの分岐。
                if (!IsEnded)
                {
                    if (TJAPlayer3.DTX[0].listChip.Count <= 0) continue;

                    //次の音符が段位幕かENDだったらtrue
                    //次の音符がドン・カッ・連打・風船だったら、false
                    bool bNotesFin = true;
                    for (int index = 0; index < TJAPlayer3.DTX[0].listChip.Count; index++)
                    {
                        if (TJAPlayer3.DTX[0].listChip[index].n発声時刻ms > (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                        {
                            if (TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 == 0xff || (Challenge[i].IsForEachSongs && TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 == 0x9B))
                                break;
                            else if (TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 >= 0x10 && TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 <= 0x1f)
                            {
                                bNotesFin = false;
                                break;
                            }
                        }
                    }

                    if (bNotesFin)
                    {
                        switch (Challenge[i].Type)
                        {
                            case Exam.Type.Score:
                            case Exam.Type.Roll:
                            case Exam.Type.Hit:
                                if (Challenge[i].GetAmount() < Challenge[i].GetValue(false)) Challenge[i].SetReached(true);
                                break;
                            default:
                                break;
                        }
                        IsEnded = true;
                    }
                }
            }
            if (oldReached == false && Challenge[i].GetReached() == true)
            {
                Sound_Failed?.t再生を開始する();
            }
        }
    }

    public override void On非活性化()
    {
        for (int i = 0; i < 3; i++)
        {
            Challenge[i] = null;
        }

        for (int i = 0; i < 3; i++)
        {
            Status[i].Timer_Amount = null;
            Status[i].Timer_Gauge = null;
            Status[i].Timer_Failed = null;
        }
        IsEnded = false;

        TJAPlayer3.t安全にDisposeする(ref Dan_Plate);
        Sound_Section?.t解放する();
        Sound_Failed?.t解放する();
        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan) return base.On進行描画();
        Counter_In?.t進行();
        Counter_Wait?.t進行();
        Counter_Out?.t進行();
        Counter_Text?.t進行();

        if (Counter_Text is not null)
        {
            if (Counter_Text.n現在の値 >= 2000)
            {
                for (int i = Counter_Text_Old; i < Counter_Text.n現在の値; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].TitleTex is not null)
                        {
                            TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].TitleTex.Opacity--;
                        }
                        if (TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitleTex is not null)
                        {
                            TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitleTex.Opacity--;
                        }
                    }
                }
            }
            else
            {
                if (TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].TitleTex is not null)
                {
                    TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].TitleTex.Opacity = 255;
                }
                if (TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitleTex is not null)
                {
                    TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitleTex.Opacity = 255;
                }
            }
            Counter_Text_Old = Counter_Text.n現在の値;
        }

        for (int i = 0; i < 3; i++)
        {
            Status[i].Timer_Amount?.t進行();
        }


        if (TJAPlayer3.app.ConfigToml.EnableSkinV2 || this.IsVer2)
        {
            // 背景を描画する。
            TJAPlayer3.app.Tx.DanC_V2_Background?.t2D描画(TJAPlayer3.app.Device, 0, 0);

            // 段プレートを描画する。
            Dan_Plate?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2DanPlateXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2DanPlateXY[1]);

            DrawExamV2(Challenge, Gauge);
        }
        else
        {
            // 背景を描画する。
            TJAPlayer3.app.Tx.DanC_Background?.t2D描画(TJAPlayer3.app.Device, 0, 0);

            // 残り音符数を描画する。
            var notesRemain = TJAPlayer3.DTX[0].nノーツ数[3] - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Bad) - (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Miss);

            DrawNumber(notesRemain, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberXY[1], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberPadding);

            // 段プレートを描画する。
            Dan_Plate?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.DanPlateXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.DanPlateXY[1]);
            DrawExam(Challenge);
        }

        // 幕のアニメーション
        if (Counter_In is not null)
        {
            if (Counter_In.b終了値に達してない)
            {
                for (int i = Counter_In_Old; i < Counter_In.n現在の値; i++)
                {
                    ScreenPoint[0] += (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] - ScreenPoint[0]) / 180.0;
                    ScreenPoint[1] += ((1280 / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] / 2) - ScreenPoint[1]) / 180.0;
                }
                Counter_In_Old = Counter_In.n現在の値;
                TJAPlayer3.app.Tx.DanC_Screen?.t2D描画(TJAPlayer3.app.Device, (int)ScreenPoint[0], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0], new Rectangle(0, 0, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Height));
                TJAPlayer3.app.Tx.DanC_Screen?.t2D描画(TJAPlayer3.app.Device, (int)ScreenPoint[1], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0], new Rectangle(TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, 0, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Height));
                //CDTXMania.act文字コンソール.tPrint(0, 420, C文字コンソール.EFontType.白, String.Format("{0} : {1}", ScreenPoint[0], ScreenPoint[1]));
            }
            if (Counter_In.b終了値に達した)
            {
                Counter_In = null;
                Counter_Wait = new CCounter(0, 2299, 1, TJAPlayer3.app.Timer);
            }
        }
        if (Counter_Wait is not null)
        {
            if (Counter_Wait.b終了値に達してない)
            {
                TJAPlayer3.app.Tx.DanC_Screen?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0]);
            }
            if (Counter_Wait.b終了値に達した)
            {
                Counter_Wait = null;
                Counter_Out = new CCounter(0, 499, 1, TJAPlayer3.app.Timer);
                Counter_Text = new CCounter(0, 2899, 1, TJAPlayer3.app.Timer);
            }
        }
        if (Counter_Text is not null)
        {
            if (Counter_Text.b終了値に達してない)
            {
                var title = TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].TitleTex;
                var subTitle = TJAPlayer3.DTX[0].List_DanSongs[NowShowingNumber].SubTitleTex;
                if (subTitle is null)
                    title?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, 1280 / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] / 2, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0] + 65);
                else
                {
                    title?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, 1280 / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] / 2, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0] + 45);
                    subTitle?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, 1280 / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[0] / 2, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0] + 85);
                }
            }
            if (Counter_Text.b終了値に達した)
            {
                Counter_Text = null;
                IsAnimating = false;
            }
        }
        if (Counter_Out is not null)
        {
            if (Counter_Out.b終了値に達してない)
            {
                for (int i = Counter_Out_Old; i < Counter_Out.n現在の値; i++)
                {
                    ScreenPoint[0] += -3;
                    ScreenPoint[1] += 3;
                }
                Counter_Out_Old = Counter_Out.n現在の値;
                TJAPlayer3.app.Tx.DanC_Screen?.t2D描画(TJAPlayer3.app.Device, (int)ScreenPoint[0], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0], new Rectangle(0, 0, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Height));
                TJAPlayer3.app.Tx.DanC_Screen?.t2D描画(TJAPlayer3.app.Device, (int)ScreenPoint[1], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0], new Rectangle(TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, 0, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Width / 2, TJAPlayer3.app.Tx.DanC_Screen.szTextureSize.Height));
                //CDTXMania.act文字コンソール.tPrint(0, 420, C文字コンソール.EFontType.白, String.Format("{0} : {1}", ScreenPoint[0], ScreenPoint[1]));
            }
            if (Counter_Out.b終了値に達した)
            {
                Counter_Out = null;
            }
        }
        return base.On進行描画();
    }

    public void DrawExam(Dan_C[] dan_C)
    {
        var count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (dan_C[i] is not null && dan_C[i].IsEnable == true)
                count++;
        }
        for (int i = 0; i < count; i++)
        {
            float PanelOffset = (count - 1) / 2.0f;
            int PanelY = 500 + 90 * i;

            #region ゲージの土台を描画する。
            if (TJAPlayer3.app.Tx.DanC_Base is not null)
            {
                PanelY = (int)(TJAPlayer3.app.Skin.SkinConfig.Game.DanC.Y - (TJAPlayer3.app.Skin.SkinConfig.Game.DanC.YPadding * PanelOffset) + (TJAPlayer3.app.Skin.SkinConfig.Game.DanC.YPadding * i) - (TJAPlayer3.app.Tx.DanC_Base.szTextureSize.Height / 2));
                TJAPlayer3.app.Tx.DanC_Base?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1], PanelY);
            }
            #endregion

            #region ゲージを描画する。
            var drawGaugeType = 0;
            if (dan_C[i].Range == Exam.Range.More)
            {
                if (dan_C[i].GetAmountToPercent() >= 100)
                    drawGaugeType = 2;
                else if (dan_C[i].GetAmountToPercent() >= 70)
                    drawGaugeType = 1;
                else
                    drawGaugeType = 0;
            }
            else
            {
                if (dan_C[i].GetAmountToPercent() >= 100)
                    drawGaugeType = 2;
                else if (dan_C[i].GetAmountToPercent() > 70)
                    drawGaugeType = 1;
                else
                    drawGaugeType = 0;
            }
            TJAPlayer3.app.Tx.DanC_Gauge[drawGaugeType]?.t2D描画(TJAPlayer3.app.Device,
                TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.Offset[0], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.Offset[1], new Rectangle(0, 0, (int)(dan_C[i].GetAmountToPercent() * (TJAPlayer3.app.Tx.DanC_Gauge[drawGaugeType].szTextureSize.Width / 100.0)), TJAPlayer3.app.Tx.DanC_Gauge[drawGaugeType].szTextureSize.Height));
            #endregion

            #region 現在の値を描画する。
            var nowAmount = 0;
            if (dan_C[i].Range == Exam.Range.Less)
            {
                nowAmount = dan_C[i].GetValue(false) - dan_C[i].GetAmount();
            }
            else
            {
                nowAmount = dan_C[i].GetAmount();
            }
            if (nowAmount < 0) nowAmount = 0;

            DrawNumber(nowAmount, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[0], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[1], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallScale, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallScale, (Status[i].Timer_Amount is not null ? ScoreScale[Status[i].Timer_Amount.n現在の値] : 0f));

            if (TJAPlayer3.app.Tx.DanC_Number is not null)
            {
                // 単位(あれば)
                switch (dan_C[i].Type)
                {
                    case Exam.Type.Gauge:
                        // パーセント
                        TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[0], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));
                        break;
                    case Exam.Type.Score:
                        TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[2], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 2, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));

                        // 点
                        break;
                    case Exam.Type.Roll:
                    case Exam.Type.Hit:
                        TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[1], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 1, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));

                        // 打
                        break;
                    case Exam.Type.Combo:
                        TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[3], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 3, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));
                        //コンボ
                        break;
                    default:
                        // 何もしない
                        break;
                }
            }

            #endregion

            #region 条件の文字を描画する。
            var offset = TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[0];
            //offset -= CDTXMania.Skin.Game_DanC_ExamRange_Padding;
            // 条件の範囲
            if (TJAPlayer3.app.Tx.DanC_ExamRange is not null)
            {
                TJAPlayer3.app.Tx.DanC_ExamRange?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset - TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Width, PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1] + (TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Height / 2), new Rectangle(0, (TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Height / 2) * (int)dan_C[i].Range, TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Width, (TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Height / 2)));
                offset -= TJAPlayer3.app.Tx.DanC_ExamRange.szTextureSize.Width;
            }

            // 単位(あれば)
            switch (dan_C[i].Type)
            {
                case Exam.Type.Gauge:
                    // パーセント
                    TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[0], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));
                    offset -= TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[0];
                    break;
                case Exam.Type.Score:
                    TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[2], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 2, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));
                    offset -= TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[2];

                    // 点
                    break;
                case Exam.Type.Roll:
                case Exam.Type.Hit:
                    TJAPlayer3.app.Tx.DanC_ExamUnit?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[1], PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1] * 1, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamUnitSize[1]));
                    offset -= TJAPlayer3.app.Skin.SkinConfig.Game.DanC.PercentHitScorePadding[1];

                    // 打
                    break;
                default:
                    // 何もしない
                    break;
            }

            // 条件の数字
            DrawNumber(dan_C[i].GetValue(false), TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset, PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallScale, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallScale);
            offset -= TJAPlayer3.app.Skin.SkinConfig.Game.DanC.NumberSmallPadding * (dan_C[i].GetValue(false).ToString().Length);

            // 条件の種類
            TJAPlayer3.app.Tx.DanC_ExamType?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1] + offset - TJAPlayer3.app.Tx.DanC_ExamType.szTextureSize.Width, PanelY - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamTypeSize[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamTypeSize[1] * (int)dan_C[i].Type, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamTypeSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.ExamTypeSize[1]));
            #endregion

            #region 条件達成失敗の画像を描画する。
            if (dan_C[i].GetReached())
            {
                TJAPlayer3.app.Tx.DanC_Failed?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.X[count - 1], PanelY);
            }
            #endregion
        }
    }

    public void DrawExamV2(Dan_C[] dan_C, Dan_C DanCGauge)
    {
        if (Gauge is not null)
            if (Gauge.IsEnable)
            {
                int soulgaugeboxx = (int)((TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxX[1] - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxX[0]) * DanCGauge.GetValue(false) / 100.0) + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxX[0];
                TJAPlayer3.app.Tx.DanC_V2_SoulGauge_Box.t2D描画(TJAPlayer3.app.Device, soulgaugeboxx, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxY);

                if (TJAPlayer3.app.Tx.DanC_V2_ExamRange is not null)
                    TJAPlayer3.app.Tx.DanC_V2_ExamRange?.t2D描画(TJAPlayer3.app.Device, soulgaugeboxx + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamRangeOffset[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamRangeOffset[1], new Rectangle(0, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Height / 2 * (int)Gauge.Range, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Width, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Height / 2));

                // 条件の種類
                if (TJAPlayer3.app.Tx.DanC_V2_ExamType is not null)
                {
                    if (TJAPlayer3.app.Tx.DanC_V2_ExamType_Box is not null)
                    {
                        TJAPlayer3.app.Tx.DanC_V2_ExamType_Box.vcScaling.X = 0.5f;
                        TJAPlayer3.app.Tx.DanC_V2_ExamType_Box?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, soulgaugeboxx + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamTypeOffset[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamTypeOffset[1]);
                    }
                    TJAPlayer3.app.Tx.DanC_V2_ExamType?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, soulgaugeboxx + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamTypeOffset[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamTypeOffset[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[1] * (int)Gauge.Type, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[1]));
                }

                DrawNumberV2(DanCGauge.GetValue(false), soulgaugeboxx + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamRangeOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeNumOffset[0] - TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxPersentWidth, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SoulGaugeBoxExamRangeOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeNumOffset[1], true, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2NumberSmallScale, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2NumberSmallScale);
            }

        var count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (dan_C[i] is not null && dan_C[i].IsEnable == true)
                count++;
        }
        for (int i = 0; i < count; i++)
        {
            float PanelOffset = (count - 1) / 2.0f;
            int PanelY = 500 + 90 * i;
            #region[パネルを描画する]
            if (TJAPlayer3.app.Tx.DanC_V2_Panel is not null)
            {
                PanelY = TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelY[i];
                TJAPlayer3.app.Tx.DanC_V2_Panel.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i], PanelY);
            }
            #endregion

            float GaugeXRatio = 1f;
            if (dan_C[i].IsForEachSongs)
                GaugeXRatio = 0.675f;
            else
                GaugeXRatio = 1.0f;

            #region ゲージの土台を描画する。
            if (TJAPlayer3.app.Tx.DanC_V2_Base is not null)
            {
                TJAPlayer3.app.Tx.DanC_V2_Base.vcScaling.X = GaugeXRatio;
                TJAPlayer3.app.Tx.DanC_V2_Base.vcScaling.Y = 1.0f;
                TJAPlayer3.app.Tx.DanC_V2_Base?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1]);
            }
            #endregion

            #region ゲージを描画する。
            var drawGaugeType = 0;
            if (dan_C[i].Range == Exam.Range.More)
            {
                if (dan_C[i].GetAmountToPercent() >= 100)
                    drawGaugeType = 2;
                else if (dan_C[i].GetAmountToPercent() >= 70)
                    drawGaugeType = 1;
                else
                    drawGaugeType = 0;
            }
            else
            {
                if (dan_C[i].GetAmountToPercent() >= 100)
                    drawGaugeType = 2;
                else if (dan_C[i].GetAmountToPercent() > 70)
                    drawGaugeType = 1;
                else
                    drawGaugeType = 0;
            }
            if (TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType] is not null)
            {
                TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].vcScaling.X = GaugeXRatio;
                TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].vcScaling.Y = 1.0f;
                TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0] + (int)(TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2GaugeOffset[0] * GaugeXRatio), PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2GaugeOffset[1], new Rectangle(0, 0, (int)(dan_C[i].GetAmountToPercent() * (TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].szTextureSize.Width / 100.0)), TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].szTextureSize.Height));
            }
            #endregion

            #region 現在の値を描画する。
            var nowAmount = 0;
            if (dan_C[i].Range == Exam.Range.Less)
            {
                nowAmount = dan_C[i].GetValue(false) - dan_C[i].GetAmount();
            }
            else
            {
                nowAmount = dan_C[i].GetAmount();
            }
            if (nowAmount < 0) nowAmount = 0;

            DrawNumberV2(nowAmount, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountOffset[1],
                false, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountScale, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountScale, (Status[i].Timer_Amount is not null ? ScoreScale[Status[i].Timer_Amount.n現在の値] : 0f));
            #endregion

            #region 条件の文字を描画する。
            // 条件の範囲
            if (TJAPlayer3.app.Tx.DanC_V2_ExamRange is not null)
                TJAPlayer3.app.Tx.DanC_V2_ExamRange?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeOffset[1], new Rectangle(0, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Height / 2 * (int)dan_C[i].Range, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Width, TJAPlayer3.app.Tx.DanC_V2_ExamRange.szTextureSize.Height / 2));

            // 条件の数字
            DrawNumberV2(dan_C[i].GetValue(false), TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeNumOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamRangeNumOffset[1], true, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2NumberSmallScale, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2NumberSmallScale);
            // 条件の種類
            if (TJAPlayer3.app.Tx.DanC_V2_ExamType is not null)
            {
                if (TJAPlayer3.app.Tx.DanC_V2_ExamType_Box is not null)
                {
                    TJAPlayer3.app.Tx.DanC_V2_ExamType_Box.vcScaling.X = 1f;
                    TJAPlayer3.app.Tx.DanC_V2_ExamType_Box?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeOffset[1]);
                }
                TJAPlayer3.app.Tx.DanC_V2_ExamType?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeOffset[1], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[1] * (int)dan_C[i].Type, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2ExamTypeSize[1]));
            }
            #endregion

            #region 条件達成失敗の画像を描画する。
            if (dan_C[i].GetReached())
            {
                if (TJAPlayer3.app.Tx.DanC_V2_Failed_Cover is not null)
                {
                    TJAPlayer3.app.Tx.DanC_V2_Failed_Cover.vcScaling.X = GaugeXRatio;
                    TJAPlayer3.app.Tx.DanC_V2_Failed_Cover.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1]);

                    if (TJAPlayer3.app.Tx.DanC_V2_Failed_Text is not null)
                    {
                        int textx = TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0] + (int)(TJAPlayer3.app.Tx.DanC_V2_Failed_Cover.szTextureSize.Width * GaugeXRatio / 2) - (TJAPlayer3.app.Tx.DanC_V2_Failed_Text.szTextureSize.Width / 2);
                        int texty = PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + (int)(TJAPlayer3.app.Tx.DanC_V2_Failed_Cover.szTextureSize.Height / 2) - (TJAPlayer3.app.Tx.DanC_V2_Failed_Text.szTextureSize.Height / 2);
                        TJAPlayer3.app.Tx.DanC_V2_Failed_Text.t2D描画(TJAPlayer3.app.Device, textx, texty);
                    }
                }
            }
            #endregion


            #region[過去ゲージの描画]
            if (dan_C[i].IsForEachSongs)
            {
                GaugeXRatio = 0.675f * 0.36f;
                float GaugeYRatio = 0.36f;
                for (int examindex = 0; examindex < this.NowShowingNumber; examindex++)
                {
                    //ベース
                    if (TJAPlayer3.app.Tx.DanC_V2_Base is not null)
                    {
                        TJAPlayer3.app.Tx.DanC_V2_Base.vcScaling.X = GaugeXRatio;
                        TJAPlayer3.app.Tx.DanC_V2_Base.vcScaling.Y = GaugeYRatio;
                        TJAPlayer3.app.Tx.DanC_V2_Base?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[1] + (examindex * TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffsetYPadding));
                    }
                    //ゲージ
                    drawGaugeType = 0;
                    if (dan_C[i].Range == Exam.Range.More)
                    {
                        if (dan_C[i].GetAmountToPercent(examindex) >= 100)
                            drawGaugeType = 2;
                        else if (dan_C[i].GetAmountToPercent(examindex) >= 70)
                            drawGaugeType = 1;
                        else
                            drawGaugeType = 0;
                    }
                    else
                    {
                        if (dan_C[i].GetAmountToPercent(examindex) >= 100)
                            drawGaugeType = 2;
                        else if (dan_C[i].GetAmountToPercent(examindex) > 70)
                            drawGaugeType = 1;
                        else
                            drawGaugeType = 0;
                    }
                    if (TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType] is not null)
                    {
                        TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].vcScaling.X = GaugeXRatio;
                        TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].vcScaling.Y = GaugeYRatio;
                        TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0] + (int)(TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2GaugeOffset[0] * GaugeXRatio), PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[1] + (int)(TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2GaugeOffset[1] * GaugeYRatio) + (examindex * TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffsetYPadding), new Rectangle(0, 0, (int)(dan_C[i].GetAmountToPercent(examindex) * (TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].szTextureSize.Width / 100.0)), TJAPlayer3.app.Tx.DanC_V2_Gauge[drawGaugeType].szTextureSize.Height));
                    }
                    //値
                    nowAmount = 0;
                    if (dan_C[i].Range == Exam.Range.Less)
                    {
                        nowAmount = dan_C[i].GetValue(false, examindex) - dan_C[i].GetAmount(examindex);
                    }
                    else
                    {
                        nowAmount = dan_C[i].GetAmount(examindex);
                    }
                    if (nowAmount < 0) nowAmount = 0;

                    DrawNumberV2(nowAmount, TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2PanelX[i] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[0] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountOffset[0], PanelY + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2BaseOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffset[1] + TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2AmountOffset[1] + (examindex * TJAPlayer3.app.Skin.SkinConfig.Game.DanC.v2SmallGaugeOffsetYPadding),
                        false, 0.37f, 0.37f, 0f);
                }
            }
            #endregion
        }
    }


    /// <summary>
    /// 段位チャレンジの数字フォントで数字を描画します。
    /// </summary>
    /// <param name="value">値。</param>
    /// <param name="x">一桁目のX座標。</param>
    /// <param name="y">一桁目のY座標</param>
    /// <param name="padding">桁数間の字間</param>
    /// <param name="scaleX">拡大率X</param>
    /// <param name="scaleY">拡大率Y</param>
    /// <param name="scaleJump">アニメーション用拡大率(Yに加算される)。</param>
    private static void DrawNumber(int value, int x, int y, int padding, float scaleX = 1.0f, float scaleY = 1.0f, float scaleJump = 0.0f)
    {
        for (int i = 0; i < value.ToString().Length; i++)
        {
            if (TJAPlayer3.app.Tx.DanC_Number is not null)
            {
                var number = (int)(value / Math.Pow(10, i) % 10);
                Rectangle rectangle = new Rectangle(TJAPlayer3.app.Tx.DanC_Number.szTextureSize.Width / 10 * number, 0, TJAPlayer3.app.Tx.DanC_Number.szTextureSize.Width / 10, TJAPlayer3.app.Tx.DanC_Number.szTextureSize.Height);
                TJAPlayer3.app.Tx.DanC_Number.vcScaling.X = scaleX;
                TJAPlayer3.app.Tx.DanC_Number.vcScaling.Y = scaleY + scaleJump;
                TJAPlayer3.app.Tx.DanC_Number.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownRight, x + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 20) - ((i + 1) * padding), y + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Height * scaleY), rectangle);
            }
        }
    }


    /// <summary>
    /// 段位チャレンジ(ニジイロ)の数字フォントで数字を描画します。
    /// </summary>
    /// <param name="value">値。</param>
    /// <param name="x">一桁目のX座標。</param>
    /// <param name="y">一桁目のY座標</param>
    /// <param name="padding">桁数間の字間</param>
    /// <param name="IsRPRight">右揃えか</param>
    /// <param name="scaleX">拡大率X</param>
    /// <param name="scaleY">拡大率Y</param>
    /// <param name="scaleJump">アニメーション用拡大率(Yに加算される)。</param>
    private static void DrawNumberV2(int value, int x, int y, bool IsRPRight, float scaleX = 1.0f, float scaleY = 1.0f, float scaleJump = 0.0f)
    {
        if (TJAPlayer3.app.Tx.DanC_V2_Number is not null)
        {
            int padding = (int)(TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 10f * scaleX);
            if (IsRPRight)
            {
                for (int i = 0; i < value.ToString().Length; i++)
                {
                    var number = (int)(value / Math.Pow(10, i) % 10);
                    Rectangle rect = new Rectangle(TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 10 * number, 0, TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 10, TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Height);
                    TJAPlayer3.app.Tx.DanC_V2_Number.vcScaling.X = scaleX;
                    TJAPlayer3.app.Tx.DanC_V2_Number.vcScaling.Y = scaleY + scaleJump;
                    TJAPlayer3.app.Tx.DanC_V2_Number.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 20) - ((i + 1) * padding), y + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Height * scaleY), rect);
                }
            }
            else
            {
                for (int i = 0; i < value.ToString().Length; i++)
                {
                    var number = (int)(value / Math.Pow(10, value.ToString().Length - 1 - i) % 10);
                    Rectangle rect = new Rectangle(TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 10 * number, 0, TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 10, TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Height);
                    TJAPlayer3.app.Tx.DanC_V2_Number.vcScaling.X = scaleX;
                    TJAPlayer3.app.Tx.DanC_V2_Number.vcScaling.Y = scaleY + scaleJump;
                    TJAPlayer3.app.Tx.DanC_V2_Number.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Width / 20) + (i * padding), y + (TJAPlayer3.app.Tx.DanC_V2_Number.szTextureSize.Height * scaleY), rect);
                }

            }
        }
    }

    /// <summary>
    /// n個の条件がひとつ以上達成失敗しているかどうかを返します。
    /// </summary>
    /// <returns>n個の条件がひとつ以上達成失敗しているか。</returns>
    public bool GetFailedAllChallenges()
    {
        var isFailed = false;
        for (int i = 0; i < this.ExamCount; i++)
        {
            if (Challenge[i].IsEnable && Challenge[i].GetReached()) isFailed = true;
        }
        if (Gauge.IsEnable && Gauge.GetReached()) isFailed = true;
        return isFailed;
    }

    /// <summary>
    /// n個の条件で段位認定モードのステータスを返します。
    /// </summary>
    /// <param name="dan_C">条件。</param>
    /// <returns>ExamStatus。</returns>
    public Exam.Status GetExamStatus(Dan_C[] dan_C, Dan_C Gauge)
    {
        var status = Exam.Status.Better_Success;
        var count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (dan_C[i] is not null && dan_C[i].IsEnable == true)
                count++;
        }
        for (int i = 0; i < count; i++)
        {
            if (!dan_C[i].GetCleared(true)) status = Exam.Status.Success;
        }
        if (Gauge.IsEnable)
            if (!Gauge.GetCleared(true))
                status = Exam.Status.Success;
        for (int i = 0; i < count; i++)
        {
            if (!dan_C[i].GetCleared(false)) status = Exam.Status.Failure;
        }
        if (Gauge.IsEnable)
            if (!Gauge.GetCleared(false))
                status = Exam.Status.Failure;
        return status;
    }

    public Dan_C[] GetExam()
    {
        return Challenge;
    }

    public Dan_C GetGaugeExam()
    {
        return Gauge;
    }

    private readonly float[] ScoreScale = new float[]
    {
        0.000f,
        0.111f, // リピート
        0.222f,
        0.185f,
        0.148f,
        0.129f,
        0.111f,
        0.074f,
        0.065f,
        0.033f,
        0.015f,
        0.000f
    };

    [StructLayout(LayoutKind.Sequential)]
    struct ChallengeStatus
    {
        public CCounter Timer_Gauge;
        public CCounter Timer_Amount;
        public CCounter Timer_Failed;
    }

    #region[ private ]
    //-----------------
    private int ExamCount;
    private ChallengeStatus[] Status = new ChallengeStatus[3];
    private CTexture Dan_Plate;
    private bool IsEnded;

    // アニメ関連
    private int NowShowingNumber;
    private CCounter Counter_In, Counter_Wait, Counter_Out, Counter_Text;
    private double[] ScreenPoint;
    private int Counter_In_Old, Counter_Out_Old, Counter_Text_Old;
    public bool IsAnimating;

    //音声関連
    private CSound Sound_Section;
    private CSound Sound_Failed;


    //-----------------
    #endregion
}
