using FDK;
using SkiaSharp;

namespace TJAPlayer3;

/// <summary>
/// 演奏画面のクラス
/// </summary>
internal class CStage演奏画面共通 : CStage
{
    // プロパティ

    // メソッド
    public CStage演奏画面共通()
    {
        base.eStageID = CStage.EStage.Playing;
        base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
        base.listChildren.Add(this.actCombo = new CAct演奏Combo共通());
        base.listChildren.Add(this.actChipFireD = new CAct演奏DrumsチップファイアD());
        base.listChildren.Add(this.Rainbow = new Rainbow());
        base.listChildren.Add(this.actGauge = new CAct演奏ゲージ共通());
        base.listChildren.Add(this.actJudgeString = new CActJudgeString());
        base.listChildren.Add(this.actTaikoLaneFlash = new TaikoLaneFlash());
        base.listChildren.Add(this.actScore = new CActScore());
        base.listChildren.Add(this.actScrollSpeed = new CActScrollSpeed());
        base.listChildren.Add(this.actAVI = new CAct演奏AVI());
        base.listChildren.Add(this.actPanel = new CActPanel());
        base.listChildren.Add(this.actLyric = new CActLyric());
        base.listChildren.Add(this.actStageFailed = new CActStageFailed());
        base.listChildren.Add(this.actPlayInfo = new CActPlayInfo());
        base.listChildren.Add(this.actFI = new CActFIFOStart());
        base.listChildren.Add(this.actFO = new CActFIFOBlack());
        base.listChildren.Add(this.actFOClear = new CActFIFOResult());
        base.listChildren.Add(this.actLane = new CAct演奏Drumsレーン());
        base.listChildren.Add(this.actEnd = new CAct演奏Drums演奏終了演出());
        base.listChildren.Add(this.actDancer = new CActDancer());
        base.listChildren.Add(this.actMtaiko = new CActMtaiko());
        base.listChildren.Add(this.actLaneTaiko = new CAct演奏Drumsレーン太鼓());
        base.listChildren.Add(this.actRoll = new CActRoll());
        base.listChildren.Add(this.actBalloon = new CAct演奏Drums風船());
        base.listChildren.Add(this.actChara = new CActChara());
        base.listChildren.Add(this.actGame = new CAct演奏Drumsゲームモード());
        base.listChildren.Add(this.actBackground = new CAct演奏Drums背景());
        base.listChildren.Add(this.actRollChara = new CAct演奏Drums連打キャラ());
        base.listChildren.Add(this.actComboBalloon = new CAct演奏Drumsコンボ吹き出し());
        base.listChildren.Add(this.actComboVoice = new CActComboVoice());
        base.listChildren.Add(this.actPauseMenu = new CActPauseMenu());
        base.listChildren.Add(this.actChipEffects = new CActChipEffects());
        base.listChildren.Add(this.actRunner = new CActRunner());
        base.listChildren.Add(this.actMob = new CActMob());
        base.listChildren.Add(this.GoGoSplash = new GoGoSplash());
        base.listChildren.Add(this.FlyingNotes = new FlyingNotes());
        base.listChildren.Add(this.FireWorks = new FireWorks());
        base.listChildren.Add(this.PuchiChara = new PuchiChara());

        base.listChildren.Add(this.actDan = new Dan_Cert());
        base.listChildren.Add(this.actTraining = new CAct演奏Drums特訓モード());
    }

    #region [ tSaveToCRecord ]
    public void tSaveToCRecord(out CScoreJson.CRecord Record, int nPlayer)
    {
        Record = new();

        Record.Version = TJAPlayer3.VERSION;
        Record.DateTime = DateTime.Now;
        Record.Tight = TJAPlayer3.app.ConfigToml.PlayOption.Tight;
        Record.Risky = TJAPlayer3.app.ConfigToml.PlayOption.Risky;
        Record.Just = TJAPlayer3.app.ConfigToml.PlayOption.Just;
        Record.InputMIDI = this.b演奏にMIDIInputを使った;
        Record.InputKeyboard = this.b演奏にKeyBoardを使った;
        Record.InputJoystick = this.b演奏にJoypadを使った;
        Record.InputMouse = this.b演奏にMouseを使った;
        Record.Random = TJAPlayer3.app.ConfigToml.PlayOption._Random[nPlayer];
        Record.ScrollSpeed = TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] * 0.1;
        Record.PlaySpeed = TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed / 20.0;
        Record.PerfectRange = TJAPlayer3.app.ConfigToml.HitRange.Perfect;
        Record.GoodRange = TJAPlayer3.app.ConfigToml.HitRange.Good;
        Record.BadRange = TJAPlayer3.app.ConfigToml.HitRange.Bad;
        Record.PerfectCount = this.nヒット数[nPlayer].Perfect;
        Record.GoodCount = this.nヒット数[nPlayer].Good;
        Record.BadCount = this.nヒット数[nPlayer].Bad;
        Record.MissCount = this.nヒット数[nPlayer].Miss;
        Record.RollCount = this.n合計連打数[nPlayer];
        Record.Score = (long)this.actScore.Get(nPlayer);
        Record.MaxCombo = this.actCombo.n現在のコンボ数.Max[nPlayer];
        Record.Gauge = this.actGauge.db現在のゲージ値[nPlayer];
        Record.Auto = this.b途中でAutoを切り替えたか[nPlayer] || TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[nPlayer];
        Record.PlayerName = TJAPlayer3.app.ConfigToml.PlayOption.PlayerName[nPlayer];
        var danC = TJAPlayer3.stage演奏ドラム画面.actDan.GetExam();
        for (int i = 0; i < danC.Length; i++)
        {
            Record.DanC[i] = danC[i];
        }
        Record.DanCGauge = TJAPlayer3.stage演奏ドラム画面.actDan.GetGaugeExam();
    }
    #endregion

    // CStage 実装

    public override void On活性化()
    {
        LoudnessMetadataScanner.StopBackgroundScanning(joinImmediately: false);

        this.actGame.t叩ききりまショー_初期化();
        for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
            this.ReSetScore(TJAPlayer3.DTX[nPlayer].nScoreInit[0, TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]], TJAPlayer3.DTX[nPlayer].nScoreDiff[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]], nPlayer);
        this.eフェーズID = CStage.Eフェーズ.共通_通常状態;//初期化する。

        for (int index = TJAPlayer3.DTX[0].listChip.Count - 1; index >= 0; index--)
        {
            if (TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 == 0x01)
            {
                this.bgmlength = TJAPlayer3.DTX[0].listChip[index].GetDuration() + TJAPlayer3.DTX[0].listChip[index].n発声時刻ms;
                break;
            }
        }

        ctChipAnime = new CCounter[2];
        ctChipAnimeLag = new CCounter[2];
        for (int i = 0; i < 2; i++)
        {
            ctChipAnime[i] = new CCounter();
            ctChipAnimeLag[i] = new CCounter();
        }

        this.eFadeOut完了時の戻り値 = E演奏画面の戻り値.継続;
        this.n現在のトップChip = (TJAPlayer3.DTX[0].listChip.Count > 0) ? 0 : -1;

        this.nヒット数[0] = new CHITCOUNTOFRANK();
        this.nヒット数[1] = new CHITCOUNTOFRANK();

        this.b演奏にKeyBoardを使った = false;
        this.b演奏にJoypadを使った = false;
        this.b演奏にMIDIInputを使った = false;
        this.b演奏にMouseを使った = false;

        this.ShownLyric2 = 0;

        // When performing calibration, reduce audio distraction from user input.
        // For users who play primarily by listening to the music,
        // you might think that we want them to hear drum sound effects during
        // calibration, but we do not. Humans are remarkably good at adjusting
        // the timing of their own physical movement, even without realizing it.
        // We are calibrating their input timing for the purposes of judgment.
        // We do not want them subconsciously playing early so as to line up
        // their drum sound effects with the sounds of the input calibration file.
        // Instead, we want them focused on the sounds of their keyboard, tatacon,
        // other controller, etc. and the sounds of the input calibration audio file.
        if (!TJAPlayer3.IsPerformingCalibration)
        {
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                this.soundRed[i] = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Taiko/" + TJAPlayer3.app.Skin.NowSENum[i].ToString() + @"/dong.ogg"), ESoundGroup.SoundEffect);
                this.soundBlue[i] = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Taiko/" + TJAPlayer3.app.Skin.NowSENum[i].ToString() + @"/ka.ogg"), ESoundGroup.SoundEffect);
                this.soundAdlib[i] = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Taiko/" + TJAPlayer3.app.Skin.NowSENum[i].ToString() + @"/Adlib.ogg"), ESoundGroup.SoundEffect);
            }

            if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 && TJAPlayer3.app.ConfigToml.PlayOption.UsePanning)//2020.05.06 Mr-Ojii 左右に出したかったから、追加。
            {
                if (this.soundRed[0] is not null)
                    this.soundRed[0].nPanning = -100;
                if (this.soundBlue[0] is not null)
                    this.soundBlue[0].nPanning = -100;
                if (this.soundAdlib[0] is not null)
                    this.soundAdlib[0].nPanning = -100;
                if (this.soundRed[1] is not null)
                    this.soundRed[1].nPanning = 100;
                if (this.soundBlue[1] is not null)
                    this.soundBlue[1].nPanning = 100;
                if (this.soundAdlib[1] is not null)
                    this.soundAdlib[1].nPanning = 100;
            }
        }

        this.t背景テクスチャの生成();

        base.On活性化();
        this.tパネル文字列の設定();
        //this.演奏判定ライン座標();
        this.bIsGOGOTIME = new bool[] { false, false, false, false };
        this.bUseBranch = new bool[] { false, false, false, false };
        this.n現在のコース = new int[4];
        this.n次回のコース = new int[4];
        for (int i = 0; i < 2; i++)
        {
            this.b強制的に分岐させた[i] = false;

            TJAPlayer3.stage演奏ドラム画面.actMtaiko.After[i] = 0;
            TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.stBranch[i].nAfter = 0;
            TJAPlayer3.stage演奏ドラム画面.actMtaiko.Before[i] = 0;
            TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.stBranch[i].nBefore = 0;
        }
        for (int i = 0; i < CBranchScore.Length; i++)
        {
            this.CBranchScore[i] = new CBRANCHSCORE();

            //大音符分岐時の情報をまとめるため
            this.CBranchScore[i].cBigNotes = new CBRANCHSCORE();
        }
        this.b連打中 = new bool[] { false, false, false, false };
        this.n現在の連打数 = new int[] { 0, 0, 0, 0 };
        this.n合計連打数 = new int[] { 0, 0, 0, 0 };
        this.n分岐した回数 = new int[4];
        this.ShownLyric = 0;
        this.ShownLyric2 = 0;
        using (var fontLyric = new CCachedFontRenderer(TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont.LyricFontName, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont.LyricFontSize))
        {
            foreach (var lyric in TJAPlayer3.DTX[0].listLyric)
            {
                this.listLyric.Add(fontLyric.DrawText(lyric, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont._LyricForeColor, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont._LyricBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio));
            }
            foreach (var lyric in TJAPlayer3.DTX[0].listLyric2)
            {
                this.listLyric2.Add(fontLyric.DrawText(lyric.Text, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont._LyricForeColor, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont._LyricBackColor, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio));
            }
        }
        this.nJPOSSCROLL = new int[4];
        this.bLEVELHOLD = new bool[] { false, false, false, false };

        this.bDoublePlay = TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2 ? true : false;

        this.tBranchReset(0);
        this.tBranchReset(1);

        //			this.nRisky = CDTXMania.ConfigIni.nRisky;											// #23559 2011.7.28 yyagi
        actGauge.Init(TJAPlayer3.app.ConfigToml.PlayOption.Risky);									// #23559 2011.7.28 yyagi

        TJAPlayer3.app.Skin.tRemoveMixerAll();	// 効果音のストリームをミキサーから解除しておく

        queueMixerSound = new Queue<STMixer>(64);
        this.bPAUSE = false;

        db再生速度 = ((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0;

        #region [ 演奏開始前にmixer登録しておくべきサウンド(開幕してすぐに鳴らすことになるチップ音)を登録しておく ]
        {
            CDTX.CChip pChip = TJAPlayer3.DTX[0].listChip[0];
            //				Debug.WriteLine( "CH=" + pChip.nチャンネル番号.ToString( "x2" ) + ", 整数値=" + pChip.n整数値 +  ", time=" + pChip.n発声時刻ms );
            if (pChip.n発声時刻ms <= 0)
            {
                if (pChip.nチャンネル番号 == 0xDA)
                {
                    pChip.bHit = true;
                    //						Trace.TraceInformation( "first [DA] BAR=" + pChip.n発声位置 / 384 + " ch=" + pChip.nチャンネル番号.ToString( "x2" ) + ", wav=" + pChip.n整数値 + ", time=" + pChip.n発声時刻ms );
                    if (TJAPlayer3.DTX[0].listWAV.TryGetValue(pChip.n整数値_内部番号, out CDTX.CWAV wc))
                    {
                        if (wc.rSound is not null)
                        {
                            TJAPlayer3.SoundManager.AddMixer(wc.rSound, db再生速度, pChip.b演奏終了後も再生が続くチップである);
                        }
                    }
                }
            }
        }
        #endregion

        this.sw = new Stopwatch();
        //          this.sw2 = new Stopwatch();
        //			this.gclatencymode = GCSettings.LatencyMode;
        //			GCSettings.LatencyMode = GCLatencyMode.Batch;	// 演奏画面中はGCを抑止する
        this.bIsAlreadyCleared = new bool[2];
        this.bIsAlreadyMaxed = new bool[2];

        this.ListDan_Number = 0;
        this.IsDanFailed = false;
        this.b途中でAutoを切り替えたか = new bool[] { false, false };

        dtLastQueueOperation = DateTime.MinValue;

        PuchiChara.InitializeBPM(60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));

        for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
        {
            double dbPtn_Normal = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatNormal[nPlayer] / this.actChara.arモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
            double dbPtn_Clear = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatClear[nPlayer] / this.actChara.arクリアモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
            double dbPtn_GoGo = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatGoGo[nPlayer] / this.actChara.arゴーゴーモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Normal[nPlayer] != 0)
            {
                this.actChara.ctChara_Normal[nPlayer] = new CCounter(0, this.actChara.arモーション番号[nPlayer].Length - 1, dbPtn_Normal, CSoundManager.rc演奏用タイマ);
            }
            else
            {
                this.actChara.ctChara_Normal[nPlayer] = new CCounter();
            }
            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
            {
                this.actChara.ctChara_Clear[nPlayer] = new CCounter(0, this.actChara.arクリアモーション番号[nPlayer].Length - 1, dbPtn_Clear, CSoundManager.rc演奏用タイマ);
            }
            else
            {
                this.actChara.ctChara_Clear[nPlayer] = new CCounter();
            }
            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
            {
                this.actChara.ctChara_GoGo[nPlayer] = new CCounter(0, this.actChara.arゴーゴーモーション番号[nPlayer].Length - 1, dbPtn_GoGo, CSoundManager.rc演奏用タイマ);
            }
            else
            {
                this.actChara.ctChara_GoGo[nPlayer] = new CCounter();
            }
        }

        if (this.actDancer.ct踊り子モーション is not null)
        {
            double dbUnit_dancer = (((60 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))) / this.actDancer.ar踊り子モーション番号.Length);
            this.actDancer.ct踊り子モーション = new CCounter(0, this.actDancer.ar踊り子モーション番号.Length - 1, dbUnit_dancer * TJAPlayer3.app.Skin.SkinConfig.Game.Dancer.Beat, CSoundManager.rc演奏用タイマ);
        }
        else
        {
            this.actDancer.ct踊り子モーション = new CCounter();
        }

        this.ct手つなぎ = new CCounter(0, 60, 20, TJAPlayer3.app.Timer);

        // Discord Presence の更新
        var difficultyName = ((Difficulty)TJAPlayer3.stage選曲.n確定された曲の難易度[0]).ToString();

        string Details = TJAPlayer3.app.ConfigToml.Game.SendDiscordPlayingInformation ? TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION : "";


        TJAPlayer3.app.Discord.Update(
            Details.Substring(0, Math.Min(127, Details.Length)),
            "Playing" + (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] == true ? " (Auto)" : ""),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMilliseconds(TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms / (TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed / 20.0)),
            TJAPlayer3.app.ConfigToml.Game.SendDiscordPlayingInformation ? difficultyName.ToLowerInvariant() : "",
            TJAPlayer3.app.ConfigToml.Game.SendDiscordPlayingInformation ? String.Format("COURSE:{0} ({1})", difficultyName, TJAPlayer3.stage選曲.n確定された曲の難易度[0]) : ""
        );
    }
    public override void On非活性化()
    {
        this.ct手つなぎ = null;
        this.bgmlength = 1;

        for (int i = 0; i < 2; i++)
        {
            ctChipAnime[i] = null;
            ctChipAnimeLag[i] = null;
        }

        queueMixerSound.Clear();
        queueMixerSound = null;
        //			GCSettings.LatencyMode = this.gclatencymode;

        var meanLag = CLagLogger.LogAndReturnMeanLag();

        if (TJAPlayer3.IsPerformingCalibration && meanLag is not null)
        {
            var oldInputAdjustTimeMs = TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs;
            var newInputAdjustTimeMs = oldInputAdjustTimeMs - (int)Math.Round(meanLag.Value);
            Trace.TraceInformation($"Calibration complete. Updating InputAdjustTime from {oldInputAdjustTimeMs}ms to {newInputAdjustTimeMs}ms.");
            TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs = newInputAdjustTimeMs;
        }

        this.actDan.IsAnimating = false;//2020.07.03 Mr-Ojii IsAnimating=trueのときにそのまま選曲画面に戻ると、文字列が描画されない問題修正用。

        for (int i = 0; i < 2; i++)
        {
            if (this.soundRed[i] is not null)
                this.soundRed[i].t解放する();
            if (this.soundBlue[i] is not null)
                this.soundBlue[i].t解放する();
            if (this.soundAdlib[i] is not null)
                this.soundAdlib[i].t解放する();
        }
        TJAPlayer3.t安全にDisposeする(ref this.tx背景);

        if (this.listLyric is not null)
        {
            for (int i = 0; i < this.listLyric.Count; i++)
                listLyric[i].Dispose();
            listLyric.Clear();
        }

        if (this.listLyric2 is not null)
        {
            for (int i = 0; i < this.listLyric2.Count; i++)
                listLyric2[i].Dispose();
            listLyric2.Clear();
        }

        base.On非活性化();
        LoudnessMetadataScanner.StartBackgroundScanning();
    }

    public override int On進行描画()
    {
        this.sw.Start();
        if (!base.b活性化してない)
        {
            bool bIsFinishedPlaying = false;
            bool bIsFinishedEndAnime = false;
            bool bIsFinishedFadeout = false;
            #region [ 初めての進行描画 ]
            if (base.b初めての進行描画)
            {
                CSoundManager.rc演奏用タイマ.tリセット();
                //CSoundManager.rc演奏用タイマ.n現在時刻ms += 50000;
                TJAPlayer3.app.Timer.tリセット();

                // this.actChipFireD.Start( Eレーン.HH );	// #31554 2013.6.12 yyagi
                // 初チップヒット時のもたつき回避。最初にactChipFireD.Start()するときにJITが掛かって？
                // ものすごく待たされる(2回目以降と比べると2,3桁tick違う)。そこで最初の画面FadeInの間に
                // 一発Start()を掛けてJITの結果を生成させておく。

                base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
                this.actFI.tFadeIn開始();

                base.b初めての進行描画 = false;
            }
            #endregion
            this.actPauseMenu.t選択後();
            if (((TJAPlayer3.app.ConfigToml.PlayOption.Risky != 0 && this.actGauge.IsFailed(0)) || this.actGame.st叩ききりまショー.ct残り時間.b終了値に達した) && (base.eフェーズID == CStage.Eフェーズ.共通_通常状態))
            {
                this.actStageFailed.Start();
                TJAPlayer3.DTX[0].t全チップの再生停止();
                base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_FAILED;
            }
            if ((!String.IsNullOrEmpty(TJAPlayer3.DTX[0].strBGIMAGE_PATH) || (TJAPlayer3.DTX[0].listVD.Count == 0)) || !TJAPlayer3.app.ConfigToml.Game.Background.Movie) //背景動画があったら背景画像を描画しない。
            {
                if (this.tx背景 is not null)
                {
                    float ratio = Math.Min((TJAPlayer3.app.LogicalSize.Width / (float)this.tx背景.szTextureSize.Width), (TJAPlayer3.app.LogicalSize.Height / (float)this.tx背景.szTextureSize.Height));
                    this.tx背景.vcScaling.X = ratio;
                    this.tx背景.vcScaling.Y = ratio;
                    this.tx背景.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.LogicalSize.Width / 2, TJAPlayer3.app.LogicalSize.Height / 2);
                }
            }

            if (TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0 && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
            {
                this.t進行描画_AVI();
            }
            else if (TJAPlayer3.app.ConfigToml.Game.Background.BGA)
            {
                if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード) actTraining.On進行描画_背景();
                else actBackground.On進行描画();
            }

            if (!(TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0) && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
            {
                actRollChara.On進行描画();
            }

            if (!(TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0) && !bDoublePlay && TJAPlayer3.app.ConfigToml.Game.ShowDancer && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
            {
                actDancer.On進行描画();
            }

            if (!(TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0) && !bDoublePlay && TJAPlayer3.app.ConfigToml.Game.ShowFooter && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード && TJAPlayer3.app.Tx.Mob_Footer is not null)
                TJAPlayer3.app.Tx.Mob_Footer.t2D描画(TJAPlayer3.app.Device, 0, TJAPlayer3.app.LogicalSize.Height - TJAPlayer3.app.Tx.Mob_Footer.szTextureSize.Height);

            if (!(TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0) && TJAPlayer3.app.ConfigToml.Game.ShowChara)
                this.actChara.On進行描画();

            if (TJAPlayer3.app.ConfigToml.Game.ShowMob && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
                this.actMob.On進行描画();

            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.OFF)
                this.actGame.On進行描画();

            this.actScrollSpeed.On進行描画();
            this.t進行描画_チップアニメ();

            if (TJAPlayer3.app.ConfigToml.Game.ShowRunner)
                this.actRunner.On進行描画();

            this.actLaneTaiko.On進行描画();

            if (TJAPlayer3.app.ConfigToml.Game.Background._ClipDispType.HasFlag(EClipDispType.Window) && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1)
                this.actAVI.t窓表示();

            if (!TJAPlayer3.app.ConfigToml.Game.NoInfo && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
                this.t進行描画_ゲージ();

            this.actLaneTaiko.ゴーゴー炎();

            this.actDan.On進行描画();

            bIsFinishedPlaying = true;
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                bool tmp = this.t進行描画_チップ(i);
                bIsFinishedPlaying = bIsFinishedPlaying && tmp;
                this.t進行描画_チップ_連打(i);
            }

            this.actMtaiko.On進行描画();

            this.GoGoSplash.On進行描画();
            this.t進行描画_リアルタイム判定数表示();
            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
                this.actTraining.On進行描画_小節_速度();

            if (!TJAPlayer3.app.ConfigToml.Game.NoInfo)
                this.actCombo.On進行描画();
            if (!TJAPlayer3.app.ConfigToml.Game.NoInfo && TJAPlayer3.app.ConfigToml.PlayOption._GameMode != EGame.特訓モード)
                this.actScore.On進行描画();

            if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
                actTraining.On進行描画();

            this.Rainbow.On進行描画();
            this.FireWorks.On進行描画();
            this.actChipEffects.On進行描画();
            this.FlyingNotes.On進行描画();
            this.actChipFireD.On進行描画();

            this.actComboBalloon.On進行描画();

            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                this.actRoll.On進行描画(this.n現在の連打数[i], i);
            }

            if (!TJAPlayer3.app.ConfigToml.Game.NoInfo)
                this.actJudgeString.t進行描画();

            if (!TJAPlayer3.app.ConfigToml.Game.NoInfo)
                this.t進行描画_パネル文字列();

            if (TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus)
                this.actPlayInfo.t進行描画(1000, 400);

            if (TJAPlayer3.DTX[0].listLyric2.Count > ShownLyric2 && TJAPlayer3.DTX[0].listLyric2[ShownLyric2].Time + TJAPlayer3.DTX[0].nBGMAdjust < (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
            {
                this.actLyric.tSetLyricTexture(this.listLyric2[ShownLyric2++]);
            }

            this.actLyric.On進行描画();

            if (TJAPlayer3.app.ConfigToml.Game.ShowChara)
                actChara.OnDraw_Balloon();

            this.t全体制御メソッド();

            this.actPauseMenu.t進行描画();

            this.t進行描画_STAGEFAILED();

            bIsFinishedEndAnime = this.actEnd.On進行描画() == 1 ? true : false;
            bIsFinishedFadeout = this.t進行描画_FadeIn_アウト();

            //演奏終了→演出表示→FadeOut
            if (bIsFinishedPlaying && base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
            {
                if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓停止].t再生する();
                    actTraining.t演奏を停止する();

                    actTraining.n現在の小節線 = TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0];
                    actTraining.t譜面の表示位置を合わせる(false);
                }
                else
                {
                    base.eフェーズID = CStage.Eフェーズ.演奏_演奏終了演出;
                    this.actEnd.Start();
                    for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
                    {
                        if (TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] != 0)
                        {
                            if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] >= 100)
                            {
                                double dbUnit = (((60.0 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))));
                                this.actChara.アクションタイマーリセット(nPlayer);
                                this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].t進行db();
                                this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].db現在の値 = 0D;
                                this.actChara.bマイどんアクション中[nPlayer] = true;
                            }
                        }
                    }
                }
            }
            else if (bIsFinishedEndAnime && base.eフェーズID == Eフェーズ.演奏_演奏終了演出)
            {
                this.eFadeOut完了時の戻り値 = E演奏画面の戻り値.ステージクリア;
                base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_CLEAR_FadeOut;
                this.actFOClear.tFadeOut開始();
            }

            if (bIsFinishedFadeout)
            {
                Debug.WriteLine("Total On進行描画=" + sw.ElapsedMilliseconds + "ms");
                return (int)this.eFadeOut完了時の戻り値;
            }

            ManageMixerQueue();

            // キー入力

            this.tキー入力();
        }
        this.sw.Stop();
        return 0;
    }

    // その他

    #region [ protected ]
    //-----------------
    public class CHITCOUNTOFRANK
    {
        // Fields
        public int Good;
        public int Perfect;
        public int Bad;
        public int Miss;

        // Properties
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.Perfect;

                    case 1:
                        return this.Good;

                    case 2:
                        return this.Bad;

                    case 3:
                        return this.Miss;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.Perfect = value;
                        return;

                    case 1:
                        this.Good = value;
                        return;

                    case 2:
                        this.Bad = value;
                        return;

                    case 3:
                        this.Miss = value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }
    }

    protected struct STMixer
    {
        internal bool bIsAdd;
        internal CSound csound;
        internal bool b演奏終了後も再生が続くチップである;
    };

    /// <summary>
    /// 分岐用のスコアをまとめるクラス。
    /// .2020.04.21.akasoko26
    /// </summary>
    public class CBRANCHSCORE//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに追加
    {
        public CBRANCHSCORE cBigNotes;//大音符分岐時の情報をまとめるため
        public int nRoll;
        public int nPerfect;
        public int nGood;
        public int nMiss;
        public int nScore;
    }

    public CAct演奏AVI actAVI;
    public Rainbow Rainbow;
    public CAct演奏Combo共通 actCombo;
    //protected CActFIFOBlack actFI;
    protected CActFIFOStart actFI;
    protected CActFIFOBlack actFO;
    protected CActFIFOResult actFOClear;
    public CAct演奏ゲージ共通 actGauge;

    public CActDancer actDancer;
    protected CActJudgeString actJudgeString;
    public TaikoLaneFlash actTaikoLaneFlash;
    public CActPanel actPanel;
    public CActLyric actLyric;
    public CActPlayInfo actPlayInfo;
    public CActScore actScore;
    public CActStageFailed actStageFailed;
    protected CActScrollSpeed actScrollSpeed;
    protected CActRoll actRoll;
    protected CAct演奏Drums風船 actBalloon;
    public CActChara actChara;
    protected CAct演奏Drums連打キャラ actRollChara;
    protected CAct演奏Drumsコンボ吹き出し actComboBalloon;
    protected CActComboVoice actComboVoice;
    protected CActPauseMenu actPauseMenu;
    public CActChipEffects actChipEffects;
    public CActRunner actRunner;
    public CActMob actMob;
    public CAct演奏DrumsチップファイアD actChipFireD;
    public CAct演奏Drumsレーン actLane;
    public CActMtaiko actMtaiko;
    public CAct演奏Drumsレーン太鼓 actLaneTaiko;
    public CAct演奏Drums演奏終了演出 actEnd;
    public CAct演奏Drums背景 actBackground;
    public CAct演奏Drums特訓モード actTraining;
    public Dan_Cert actDan;
    public GoGoSplash GoGoSplash;
    public FlyingNotes FlyingNotes;
    public FireWorks FireWorks;
    public PuchiChara PuchiChara;
    protected CAct演奏Drumsゲームモード actGame;
    protected CCounter ct手つなぎ;

    private readonly FrozenDictionary<char, Point> st小文字位置 = new Dictionary<char, Point>()
    {
        {'0', new Point(0, 0)},
        {'1', new Point(32, 0)},
        {'2', new Point(64, 0)},
        {'3', new Point(96, 0)},
        {'4', new Point(128, 0)},
        {'5', new Point(160, 0)},
        {'6', new Point(192, 0)},
        {'7', new Point(224, 0)},
        {'8', new Point(256, 0)},
        {'9', new Point(288, 0)},
        {'%', new Point(320, 0)},
    }.ToFrozenDictionary();

    public bool bPAUSE;
    public bool[] bIsAlreadyCleared;
    public bool[] bIsAlreadyMaxed;
    protected bool b演奏にMIDIInputを使った;
    protected bool b演奏にKeyBoardを使った;
    protected bool b演奏にJoypadを使った;
    protected bool b演奏にMouseを使った;
    public CCounter[] ctChipAnime;
    public CCounter[] ctChipAnimeLag;
    private int bgmlength = 1;
    private bool[] b途中でAutoを切り替えたか;
    private int[] n顔座標 = { 0, 0 };

    protected E演奏画面の戻り値 eFadeOut完了時の戻り値;

    public CHITCOUNTOFRANK[] nヒット数 = new CHITCOUNTOFRANK[2];
    public int n現在のトップChip = -1;

    protected volatile Queue<STMixer> queueMixerSound;		// #24820 2013.1.21 yyagi まずは単純にAdd/Removeを1個のキューでまとめて管理するやり方で設計する
    protected DateTime dtLastQueueOperation;				//
    protected double db再生速度;

    protected CTexture? tx背景;

    public CBRANCHSCORE[] CBranchScore = new CBRANCHSCORE[6];
    public bool[] bIsGOGOTIME = new bool[4];
    public bool[] bUseBranch = new bool[4];
    public int[] n現在のコース = new int[4]; //0:普通譜面 1:玄人譜面 2:達人譜面
    public int[] n次回のコース = new int[4];
    protected bool[] b譜面分岐中 = new bool[] { false, false, false, false };
    protected int[] n分岐した回数 = new int[4];
    protected int[] nJPOSSCROLL = new int[4];

    public bool[] b強制的に分岐させた = new bool[] { false, false, false, false };
    public bool[] bLEVELHOLD = new bool[] { false, false, false, false };

    private List<SKBitmap> listLyric = new();
    private List<SKBitmap> listLyric2 = new();
    private int ShownLyric = 0;
    private int ShownLyric2 = 0;
    public bool[] b連打中 = new bool[] { false, false, false, false }; //奥の手
    private int[] n合計連打数 = new int[4];
    protected int[] n風船残り = new int[4];
    protected int[] n現在の連打数 = new int[4];

    public CDTX.CChip[] chip現在処理中の連打チップ = new CDTX.CChip[4];

    protected const int NOTE_GAP = 25;

    protected int[,] nScore = new int[2, 11];

    protected int[] nHand = new int[4];

    protected CSound[] soundRed = new CSound[2];
    protected CSound[] soundBlue = new CSound[2];
    protected CSound[] soundAdlib = new CSound[2];

    public bool bDoublePlay; // 2016.08.21 kairera0467 表示だけ。

    protected Stopwatch sw;     // 2011.6.13 最適化検討用のストップウォッチ
                                //		protected Stopwatch sw2;
                                //		protected GCLatencyMode gclatencymode;

    private int ListDan_Number;
    private bool IsDanFailed;
    private readonly int[] NowProcessingChip = new int[] { 0, 0 };


    public void AddMixer(CSound cs, bool _b演奏終了後も再生が続くチップである)
    {
        STMixer stm = new STMixer()
        {
            bIsAdd = true,
            csound = cs,
            b演奏終了後も再生が続くチップである = _b演奏終了後も再生が続くチップである
        };
        queueMixerSound.Enqueue(stm);
        //		Debug.WriteLine( "★Queue: add " + Path.GetFileName( stm.csound.strFilename ));
    }
    public void RemoveMixer(CSound cs)
    {
        STMixer stm = new STMixer()
        {
            bIsAdd = false,
            csound = cs,
            b演奏終了後も再生が続くチップである = false
        };
        queueMixerSound.Enqueue(stm);
        //		Debug.WriteLine( "★Queue: remove " + Path.GetFileName( stm.csound.strFilename ));
    }
    public void ManageMixerQueue()
    {
        // もしサウンドの登録/削除が必要なら、実行する
        if (queueMixerSound.Count > 0)
        {
            //Debug.WriteLine( "☆queueLength=" + queueMixerSound.Count );
            DateTime dtnow = DateTime.Now;
            TimeSpan ts = dtnow - dtLastQueueOperation;
            if (ts.Milliseconds > 7)
            {
                for (int i = 0; i < 2 && queueMixerSound.Count > 0; i++)
                {
                    dtLastQueueOperation = dtnow;
                    STMixer stm = queueMixerSound.Dequeue();
                    if (stm.bIsAdd)
                    {
                        TJAPlayer3.SoundManager.AddMixer(stm.csound, db再生速度, stm.b演奏終了後も再生が続くチップである);
                    }
                    else
                    {
                        TJAPlayer3.SoundManager.RemoveMixer(stm.csound);
                    }
                }
            }
        }
    }

    internal EJudge e指定時刻からChipのJUDGEを返す(long nTime, CDTX.CChip pChip)
    {
        var e判定 = e指定時刻からChipのJUDGEを返すImpl(nTime, pChip);

        // When performing calibration, reduce audio distraction from user input.
        // For users who play primarily by watching notes cross the judgment position,
        // you might think that we want them to see visual judgment feedback during
        // calibration, but we do not. Humans are remarkably good at adjusting
        // the timing of their own physical movement, even without realizing it.
        // We are calibrating their input timing for the purposes of judgment.
        // We do not want them subconsciously playing early so as to line up
        // their hits with the perfect, good, etc. judgment results based on their
        // current (and soon to be replaced) input adjust time values.
        // Instead, we want them focused on the sounds of their keyboard, tatacon,
        // other controller, etc. and the visuals of notes crossing the judgment position.
        if (TJAPlayer3.IsPerformingCalibration)
        {
            return e判定 < EJudge.Good ? EJudge.Good : e判定;
        }
        else
        {
            return e判定;
        }
    }

    private EJudge e指定時刻からChipのJUDGEを返すImpl(long nTime, CDTX.CChip pChip)
    {
        if (pChip is not null)
        {
            pChip.nLag = (int)(nTime - pChip.n発声時刻ms);		// #23580 2011.1.3 yyagi: add "nInputAdjustTime" to add input timing adjust feature
            int nDeltaTime = Math.Abs(pChip.nLag);
            //Debug.WriteLine("nAbsTime=" + (nTime - pChip.n発声時刻ms) + ", nDeltaTime=" + (nTime + nInputAdjustTime - pChip.n発声時刻ms));
            if (pChip.nチャンネル番号 == 0x15 || pChip.nチャンネル番号 == 0x16)
            {
                if ((CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) > pChip.n発声時刻ms && (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.cEndChip.n発声時刻ms)
                {
                    return EJudge.Perfect;
                }
            }
            else if (pChip.nチャンネル番号 == 0x17)
            {
                if ((CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) >= pChip.n発声時刻ms - 17 && (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.cEndChip.n発声時刻ms)
                {
                    return EJudge.Perfect;
                }
            }
            if (nDeltaTime <= TJAPlayer3.app.ConfigToml.HitRange.Perfect * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))
            {
                return EJudge.Perfect;
            }
            if (nDeltaTime <= TJAPlayer3.app.ConfigToml.HitRange.Good * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))
            {
                if (TJAPlayer3.app.ConfigToml.PlayOption.Just)
                    return EJudge.Bad;
                return EJudge.Good;
            }
            if (nDeltaTime <= TJAPlayer3.app.ConfigToml.HitRange.Bad * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))
            {
                return EJudge.Bad;
            }
        }
        return EJudge.Miss;
    }

    protected void tサウンド再生(CDTX.CChip pChip)
    {
        int index = pChip.nチャンネル番号;
        if (index == 0x11 || index == 0x13 || index == 0x1A)
        {
            this.soundRed[pChip.nPlayerSide]?.t再生を開始する();
        }
        else if (index == 0x12 || index == 0x14 || index == 0x1B)
        {
            this.soundBlue[pChip.nPlayerSide]?.t再生を開始する();
        }
        else if (index == 0x1F)
        {
            this.soundAdlib[pChip.nPlayerSide]?.t再生を開始する();
        }

        this.nHand[pChip.nPlayerSide] = (this.nHand[pChip.nPlayerSide] + 1) % 2;
    }

    protected bool tRollProcess(CDTX.CChip pChip, double dbProcess_time, int num, int sort, int Input, int nPlayer)
    {
        if (dbProcess_time >= pChip.n発声時刻ms && dbProcess_time < pChip.cEndChip.n発声時刻ms)
        {
            if (pChip.nRollCount == 0)
            {
                this.actRoll.b表示[nPlayer] = true;
                this.n現在の連打数[nPlayer] = 0;
                this.actRoll.t枠表示時間延長(nPlayer);
            }
            this.actRoll.t枠表示時間延長(nPlayer);
            this.b連打中[nPlayer] = true;
            if (this.actRoll.ct連打アニメ[nPlayer].b終了値に達してない)
            {
                this.actRoll.ct連打アニメ[nPlayer] = new CCounter(0, 9, 14, TJAPlayer3.app.Timer);
                this.actRoll.ct連打アニメ[nPlayer].n現在の値 = 1;
            }
            else
            {
                this.actRoll.ct連打アニメ[nPlayer] = new CCounter(0, 9, 14, TJAPlayer3.app.Timer);
            }


            pChip.RollEffectLevel += 10;
            if (pChip.RollEffectLevel >= 100)
            {
                pChip.RollEffectLevel = 100;
                pChip.RollInputTime = new CCounter(0, 1500, 1, TJAPlayer3.app.Timer);
                pChip.RollDelay?.t停止();
            }
            else
            {
                pChip.RollInputTime = new CCounter(0, 150, 1, TJAPlayer3.app.Timer);
                pChip.RollDelay?.t停止();
            }

            pChip.nRollCount++;

            this.n現在の連打数[nPlayer]++;
            this.CBranchScore[nPlayer].nRoll++;
            this.n合計連打数[nPlayer]++;
            if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan) this.actRollChara.Start(nPlayer);

            float mag;
            //2017.01.28 DD CDTXから直接呼び出す
            if (pChip.bGOGOTIME && !TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer]) //2018.03.11 kairera0467 チップに埋め込んだフラグから読み取る
                mag = 1.2f;
            else
                mag = 1.0f;

            // 旧配点・旧筐体配点
            if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 0 || TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 1)
            {
                if (pChip.nチャンネル番号 == 0x15)
                    this.actScore.Add((long)(300 * mag) / 10 * 10, nPlayer);//2020.10.04 "/10*10"は一の位を切り捨てるためなので消さないで。
                else
                    this.actScore.Add((long)(360 * mag) / 10 * 10, nPlayer);
            }
            // AC15配点
            else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 2)
            {
                if (pChip.nチャンネル番号 == 0x15)
                    this.actScore.Add((long)(100 * mag) / 10 * 10, nPlayer);
                else
                    this.actScore.Add((long)(200 * mag) / 10 * 10, nPlayer);
            }
            // AC16配点
            else
            {
                this.actScore.Add(100L, nPlayer);
            }

            //赤か青かの分岐
            if (sort == 0)
            {
                this.soundRed[pChip.nPlayerSide]?.t再生を開始する();
                if (pChip.nチャンネル番号 == 0x15)
                    TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(1, nPlayer, true);
                else
                    TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(3, nPlayer, true);
            }
            else
            {
                this.soundBlue[pChip.nPlayerSide]?.t再生を開始する();
                if (pChip.nチャンネル番号 == 0x15)
                    TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(2, nPlayer, true);
                else
                    TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(4, nPlayer, true);

            }
        }
        else
        {
            this.b連打中[nPlayer] = false;
            return true;
        }

        return false;
    }

    protected bool tBalloonProcess(CDTX.CChip pChip, int nPlayer)
    {
        if ((int)(long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) >= pChip.n発声時刻ms && (int)(long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) <= pChip.cEndChip.n発声時刻ms)
        {
            if (pChip.nRollCount == 0)
            {
                this.n風船残り[nPlayer] = pChip.nBalloon;
            }

            this.b連打中[nPlayer] = true;
            if (actChara.CharaAction_Balloon_Breaking[nPlayer] is not null)
            {
                actChara.アクションタイマーリセット(nPlayer);
                actChara.bマイどんアクション中[nPlayer] = true;
                actChara.CharaAction_Balloon_Breaking[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer] - 1, TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BalloonTimer[nPlayer], TJAPlayer3.app.Timer);

            }
            if (this.actBalloon.ct風船アニメ[nPlayer].b終了値に達してない)
            {
                this.actBalloon.ct風船アニメ[nPlayer] = new CCounter(0, 9, 14, TJAPlayer3.app.Timer);
                this.actBalloon.ct風船アニメ[nPlayer].n現在の値 = 1;
            }
            else
            {
                this.actBalloon.ct風船アニメ[nPlayer] = new CCounter(0, 9, 14, TJAPlayer3.app.Timer);
            }
            pChip.nRollCount++;
            this.n風船残り[nPlayer]--;

            this.n合計連打数[nPlayer]++; //  成績発表の連打数に風船を含めるように (AioiLight)
            //分岐のための処理。実装してない。

            //赤か青かの分岐
            if (pChip.nBalloon == pChip.nRollCount)
            {
                //ﾊﾟｧｰﾝ
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND風船].t再生する();
                //CDTXMania.stage演奏ドラム画面.actChipFireTaiko.Start( 3, nPlayer ); //ここで飛ばす。飛ばされるのは大音符のみ。
                TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(3, nPlayer);
                TJAPlayer3.stage演奏ドラム画面.Rainbow.Start(nPlayer);
                //CDTXMania.stage演奏ドラム画面.actChipFireD.Start( 0, nPlayer );

                if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp != 3)
                {
                    if (pChip.bGOGOTIME && !TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer])
                    {
                        this.actScore.Add(6000L, nPlayer);
                    }
                    else
                    {
                        this.actScore.Add(5000L, nPlayer);
                    }
                }
                else
                {
                    this.actScore.Add(100L, nPlayer);
                }
                pChip.bHit = true;
                pChip.IsHitted = true;
                chip現在処理中の連打チップ[nPlayer].bHit = true;
                //this.b連打中 = false;
                //this.actChara.b風船連打中 = false;
                pChip.b可視 = false;
                this.actChara.bマイどんアクション中[nPlayer] = false; // 風船終了後、再生されていたアクションがされないようにするために追加。(AioiLight)
                if (actChara.CharaAction_Balloon_Broke[nPlayer] is not null)
                {
                    actChara.アクションタイマーリセット(nPlayer);
                    actChara.bマイどんアクション中[nPlayer] = true;
                    actChara.CharaAction_Balloon_Broke[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer] - 1, TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BalloonTimer[nPlayer], TJAPlayer3.app.Timer);
                    if (actChara.CharaAction_Balloon_Delay[nPlayer] is not null) actChara.CharaAction_Balloon_Delay[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BalloonDelay[nPlayer] - 1, 1, TJAPlayer3.app.Timer);
                }
            }
            else
            {
                if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp != 3)
                {
                    if (pChip.bGOGOTIME && !TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer])
                    {
                        this.actScore.Add(360L, nPlayer);
                    }
                    else
                    {
                        this.actScore.Add(300L, nPlayer);
                    }
                }
                else
                {
                    this.actScore.Add(100L, nPlayer);
                }

                this.soundRed[pChip.nPlayerSide]?.t再生を開始する();
            }
            //TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Hit);
        }
        else
        {
            if (chip現在処理中の連打チップ[nPlayer] is not null)
                chip現在処理中の連打チップ[nPlayer].bHit = true;
            this.b連打中[nPlayer] = false;
            this.actChara.b風船連打中[nPlayer] = false;
            return false;
        }

        return true;
    }

    protected unsafe EJudge tチップのヒット処理(long nHitTime, CDTX.CChip pChip, bool bCorrectLane, int nNowInput, int nPlayer)
    {
        //unsafeコードにつき、デバッグ中の変更厳禁!
        bool bAutoPlay = TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[nPlayer];

        if (!pChip.b可視)
            return EJudge.AutoPerfect;

        if (pChip.nチャンネル番号 != 0x15 && pChip.nチャンネル番号 != 0x16 && pChip.nチャンネル番号 != 0x17 && pChip.nチャンネル番号 != 0x18)
        {
            if (!pChip.IsMissed)
            {
                pChip.bHit = true;
                pChip.IsHitted = true;
            }
        }

        EJudge eJudgeResult = EJudge.AutoPerfect;

        //連打が短すぎると発声されない
        eJudgeResult = (bCorrectLane) ? this.e指定時刻からChipのJUDGEを返す(nHitTime, pChip) : EJudge.Miss;

        if (!bAutoPlay && eJudgeResult != EJudge.Miss)
        {
            CLagLogger.Add(nPlayer, pChip);
        }

        if (pChip.nチャンネル番号 == 0x15 || pChip.nチャンネル番号 == 0x16)
        {
            #region[ 連打 ]
            //---------------------------
            this.b連打中[nPlayer] = true;
            if (bAutoPlay)
            {
                if (TJAPlayer3.app.ConfigToml.PlayOption.AutoRoll)
                {
                    if (((CSoundManager.rc演奏用タイマ.n現在時刻ms * ((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) > (pChip.n発声時刻ms + (TJAPlayer3.app.ConfigToml.PlayOption.AutoRollSpeed) * pChip.nRollCount))
                    {
                        this.nHand[nPlayer] = (this.nHand[nPlayer] + 1) % 2;

                        TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Red);
                        TJAPlayer3.stage演奏ドラム画面.FlyingNotes.Start(pChip.nチャンネル番号 == 0x15 ? 1 : 3, nPlayer, true);
                        TJAPlayer3.stage演奏ドラム画面.actMtaiko.tMtaikoEvent(pChip.nチャンネル番号, this.nHand[nPlayer], nPlayer);

                        this.tRollProcess(pChip, (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), 1, 0, 0, nPlayer);
                    }
                }
            }
            else
            {
                this.tRollProcess(pChip, (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), 1, nNowInput, 0, nPlayer);
            }
            if (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM < 0 && (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.HBSCROLL))
                pChip.fBMSCROLLTime -= TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * -0.05;
            //---------------------------
            #endregion
        }
        else if (pChip.nチャンネル番号 == 0x17)
        {
            #region[ 風船 ]
            this.b連打中[nPlayer] = true;
            this.actChara.b風船連打中[nPlayer] = true;

            if (bAutoPlay)
            {
                if (pChip.nBalloon != 0)
                {
                    if ((CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) > (pChip.n発声時刻ms + ((pChip.cEndChip.n発声時刻ms - pChip.n発声時刻ms) / pChip.nBalloon) * pChip.nRollCount))
                    {
                        this.nHand[nPlayer] = (this.nHand[nPlayer] + 1) % 2;

                        TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Red);
                        TJAPlayer3.stage演奏ドラム画面.actMtaiko.tMtaikoEvent(pChip.nチャンネル番号, this.nHand[nPlayer], nPlayer);

                        this.tBalloonProcess(pChip, nPlayer);
                    }
                }
            }
            else
            {
                this.tBalloonProcess(pChip, nPlayer);
            }
            #endregion
        }
        else if (pChip.nチャンネル番号 == 0x18)
        {
            if (pChip.cEndChip.n発声時刻ms <= (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
            {
                this.b連打中[nPlayer] = false;
                //this.actChara.b風船連打中 = false;
                pChip.bHit = true;
                pChip.IsHitted = true;
            }
        }
        else if (pChip.nチャンネル番号 == 0x1F)
        {
            if (eJudgeResult != EJudge.AutoPerfect && eJudgeResult != EJudge.Miss)
            {
                this.actJudgeString.Start(EJudge.Bad, pChip.nLag, pChip, nPlayer);
                TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.Start(0x11, eJudgeResult, true, nPlayer);
                TJAPlayer3.stage演奏ドラム画面.actChipFireD.Start(0x11, eJudgeResult, nPlayer);
            }
        }
        else
        {
            if (eJudgeResult != EJudge.Miss)
            {
                pChip.bShow = false;
            }

            if (eJudgeResult != EJudge.AutoPerfect && eJudgeResult != EJudge.Miss)
            {
                this.actJudgeString.Start(bAutoPlay ? EJudge.AutoPerfect : eJudgeResult, pChip.nLag, pChip, nPlayer);
                TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.Start(pChip.nチャンネル番号, eJudgeResult, true, nPlayer);
                TJAPlayer3.stage演奏ドラム画面.actChipFireD.Start(pChip.nチャンネル番号, eJudgeResult, nPlayer);
            }
            else if (eJudgeResult != EJudge.Bad)
            {
                //this.actJudgeString.Start( 0,bAutoPlay ? EJudge.Auto : eJudgeResult, pChip.nLag, pChip, nPlayer );
            }
        }

        if (pChip.nチャンネル番号 != 0x15 && pChip.nチャンネル番号 != 0x16 && pChip.nチャンネル番号 != 0x17 && pChip.nチャンネル番号 != 0x18 && pChip.nチャンネル番号 != 0x1F)
        {
            actGauge.Damage(pChip.nコース, eJudgeResult, nPlayer);
        }

        if (eJudgeResult != EJudge.Bad && eJudgeResult != EJudge.Miss)
        {
            double dbUnit = (((60.0 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))));

            // ランナー(たたけたやつ)
            this.actRunner.Start(nPlayer, false, pChip);

            if ((int)actGauge.db現在のゲージ値[nPlayer] >= 100 && this.bIsAlreadyMaxed[nPlayer] == false)
            {
                if (TJAPlayer3.app.Skin.Game_Chara_Ptn_SoulIn[nPlayer] != 0 && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                {
                    this.actChara.アクションタイマーリセット(nPlayer);
                    this.actChara.ctキャラクターアクション_魂MAX[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_SoulIn[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_SoulIn[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                    this.actChara.ctキャラクターアクション_魂MAX[nPlayer].t進行db();
                    this.actChara.ctキャラクターアクション_魂MAX[nPlayer].db現在の値 = 0D;
                    this.actChara.bマイどんアクション中[nPlayer] = true;
                }
                this.bIsAlreadyMaxed[nPlayer] = true;
            }
            if ((int)actGauge.db現在のゲージ値[nPlayer] >= 80 && this.bIsAlreadyCleared[nPlayer] == false)
            {
                if (TJAPlayer3.app.Skin.Game_Chara_Ptn_ClearIn[nPlayer] != 0 && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                {
                    this.actChara.アクションタイマーリセット(nPlayer);
                    this.actChara.ctキャラクターアクション_ノルマ[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_ClearIn[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_ClearIn[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                    this.actChara.ctキャラクターアクション_ノルマ[nPlayer].t進行db();
                    this.actChara.ctキャラクターアクション_ノルマ[nPlayer].db現在の値 = 0D;
                    this.actChara.bマイどんアクション中[nPlayer] = true;
                }
                this.bIsAlreadyCleared[nPlayer] = true;
                TJAPlayer3.stage演奏ドラム画面.actBackground.ClearIn(nPlayer);
            }
        }

        if (eJudgeResult == EJudge.Bad || eJudgeResult == EJudge.Miss)
        {
            // ランナー(みすったやつ)
            this.actRunner.Start(nPlayer, true, pChip);
            if ((int)actGauge.db現在のゲージ値[nPlayer] < 100 && this.bIsAlreadyMaxed[nPlayer] == true)
            {
                this.bIsAlreadyMaxed[nPlayer] = false;
            }
            if ((int)actGauge.db現在のゲージ値[nPlayer] < 80 && this.bIsAlreadyCleared[nPlayer] == true)
            {
                this.bIsAlreadyCleared[nPlayer] = false;
            }
        }



        if (pChip.nチャンネル番号 != 0x15 && pChip.nチャンネル番号 != 0x16 && pChip.nチャンネル番号 != 0x17 && pChip.nチャンネル番号 != 0x18)
        {
            switch (eJudgeResult)
            {
                case EJudge.Perfect:
                    {
                        this.CBranchScore[nPlayer].nPerfect++;
                        this.nヒット数[nPlayer].Perfect++;
                        this.actCombo.n現在のコンボ数[nPlayer]++;
                        if (this.actCombo.ctコンボ加算[nPlayer].b終了値に達してない)
                        {
                            this.actCombo.ctコンボ加算[nPlayer].n現在の値 = 1;
                        }
                        else
                        {
                            this.actCombo.ctコンボ加算[nPlayer].n現在の値 = 0;
                        }
                    }
                    break;
                case EJudge.Good:
                    {
                        this.CBranchScore[nPlayer].nGood++;
                        this.nヒット数[nPlayer].Good++;
                        this.actCombo.n現在のコンボ数[nPlayer]++;
                        if (this.actCombo.ctコンボ加算[nPlayer].b終了値に達してない)
                        {
                            this.actCombo.ctコンボ加算[nPlayer].n現在の値 = 1;
                        }
                        else
                        {
                            this.actCombo.ctコンボ加算[nPlayer].n現在の値 = 0;
                        }

                    }
                    break;
                case EJudge.Bad:
                case EJudge.Miss:
                    {
                        if (pChip.nチャンネル番号 == 0x1F)
                            break;
                        this.CBranchScore[nPlayer].nMiss++;
                        this.nヒット数[nPlayer][(int)eJudgeResult]++;
                        this.actCombo.n現在のコンボ数[nPlayer] = 0;
                        this.actComboVoice.tReset(nPlayer);
                    }
                    break;
                default:
                    this.nヒット数[nPlayer][(int)eJudgeResult]++;
                    break;
            }
        }

        #region[ コンボ音声 ]
        if (pChip.nチャンネル番号 < 0x15 || (pChip.nチャンネル番号 >= 0x1A))
        {
            if (this.actCombo.n現在のコンボ数[nPlayer] % 100 == 0 && this.actCombo.n現在のコンボ数[nPlayer] > 0)
            {
                this.actComboBalloon.Start(this.actCombo.n現在のコンボ数[nPlayer], nPlayer);
            }
            this.actComboVoice.t再生(this.actCombo.n現在のコンボ数[nPlayer], nPlayer);

            double dbUnit = (((60.0 / pChip.dbBPM)));

            for (int i = 0; i < 2; i++)
            {
                if (this.actCombo.n現在のコンボ数[i] == 50 || this.actCombo.n現在のコンボ数[i] == 300)
                {
                    ctChipAnimeLag[i] = new CCounter(0, 664, 1, TJAPlayer3.app.Timer);
                }
            }

            if (this.actCombo.n現在のコンボ数[nPlayer] % 10 == 0 && this.actCombo.n現在のコンボ数[nPlayer] > 0)
            {
                if (!pChip.bGOGOTIME) //2018.03.11 kairera0467 チップに埋め込んだフラグから読み取る
                {
                    if (TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo[nPlayer] != 0 && !this.actChara.ctキャラクターアクション_ノルマ[nPlayer].b進行中db && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                    {
                        if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] < 100)
                        {
                            // 魂ゲージMAXではない
                            // ジャンプ_ノーマル
                            this.actChara.アクションタイマーリセット(nPlayer);
                            this.actChara.ctキャラクターアクション_10コンボ[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                            this.actChara.ctキャラクターアクション_10コンボ[nPlayer].t進行db();
                            this.actChara.ctキャラクターアクション_10コンボ[nPlayer].db現在の値 = 0D;
                            this.actChara.bマイどんアクション中[nPlayer] = true;
                            //this.actChara.マイどん_アクション_10コンボ();
                        }
                    }
                    if (TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] != 0 && !this.actChara.ctキャラクターアクション_魂MAX[nPlayer].b進行中db && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                    {
                        if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] >= 100)
                        {
                            // 魂ゲージMAX
                            // ジャンプ_MAX
                            this.actChara.アクションタイマーリセット(nPlayer);
                            this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_10combo_Max[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                            this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].t進行db();
                            this.actChara.ctキャラクターアクション_10コンボMAX[nPlayer].db現在の値 = 0D;
                            this.actChara.bマイどんアクション中[nPlayer] = true;
                        }
                    }
                }
            }
        }
        #endregion

        actDan.Update();
        if ((eJudgeResult != EJudge.Miss) && (eJudgeResult != EJudge.Bad) && (pChip.nチャンネル番号 <= 0x14 || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B))
        {
            int nCombos = this.actCombo.n現在のコンボ数[nPlayer];
            long nAddScore = 0;

            if (TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer])  //2016.07.04 kairera0467 真打モード。
            {
                if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp != 3)
                {
                    nAddScore = TJAPlayer3.DTX[nPlayer].nScoreInit[1, TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]];
                    if (nAddScore == 0)
                    {
                        //可の時に0除算をするとエラーが発生するため、それらしい数値を自動算出する。
                        //メモ
                        //風船1回
                        nAddScore = 100;
                        //( 100万 - ( ( 風船の打数 - 風船音符の数 * 300 ) + ( 風船音符の数 * 5000 ) ) ) / ノーツ数
                        //(最大コンボ数＋大音符数)×初項＋(風船の総打数－風船数)×300＋風船数×5000
                        //int nBallonCount = 0;
                        //int nBallonNoteCount = CDTXMania.DTX.n風船数[ 2 ] + CDTXMania.DTX.n風船数[ 3 ];
                        //int test = ( 1000000 - ( ( nBallonCount - nBallonNoteCount * 300 ) + ( nBallonNoteCount * 5000 ) ) ) / ( CDTXMania.DTX.nノーツ数[ 2 ] + CDTXMania.DTX.nノーツ数[ 3 ] );
                    }

                    if (eJudgeResult == EJudge.Good)
                    {
                        nAddScore = nAddScore / 2;
                    }

                    if (pChip.nチャンネル番号 == 0x13 || pChip.nチャンネル番号 == 0x14 || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B)
                    {
                        nAddScore = nAddScore * 2;
                    }

                    this.actScore.Add(nAddScore, nPlayer);
                }
            }
            else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 3)
            {
                nAddScore = this.nScore[nPlayer, 0];
                if (eJudgeResult == EJudge.Good)
                {
                    nAddScore = nAddScore / 2;
                }
                this.actScore.Add(nAddScore, nPlayer);
            }
            else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 2)
            {
                if (nCombos < 10)
                {
                    nAddScore = this.nScore[nPlayer, 0];
                }
                else if (nCombos >= 10 && nCombos <= 29)
                {
                    nAddScore = this.nScore[nPlayer, 1];
                }
                else if (nCombos >= 30 && nCombos <= 49)
                {
                    nAddScore = this.nScore[nPlayer, 2];
                }
                else if (nCombos >= 50 && nCombos <= 99)
                {
                    nAddScore = this.nScore[nPlayer, 3];
                }
                else if (nCombos >= 100)
                {
                    nAddScore = this.nScore[nPlayer, 4];
                }

                if (eJudgeResult == EJudge.Good)
                {
                    nAddScore = nAddScore / 2;
                }

                if (pChip.bGOGOTIME) //2018.03.11 kairera0467 チップに埋め込んだフラグから読み取る
                {
                    nAddScore = (int)(nAddScore * 1.2f);
                }

                //100コンボ毎のボーナス
                if (nCombos % 100 == 0 && nCombos > 99)
                {
                    if (this.actScore.ctボーナス加算タイマ[nPlayer].b進行中)
                    {
                        this.actScore.ctボーナス加算タイマ[nPlayer].t停止();
                        this.actScore.BonusAdd(nPlayer);
                    }
                    this.actScore.ctボーナス加算タイマ[nPlayer].n現在の値 = 0;
                    this.actScore.ctボーナス加算タイマ[nPlayer] = new CCounter(0, 2, 1000, TJAPlayer3.app.Timer);

                    //combot = new System.Timers.Timer();
                    //if(nPlayer == 0)
                    //{
                    //    combot.Elapsed += new System.Timers.ElapsedEventHandler(combotimer_event_1);
                    //} else
                    //{
                    //    combot.Elapsed += new System.Timers.ElapsedEventHandler(combotimer_event_2);
                    //}

                    //combot.Interval = 2000; // ミリ秒単位で指定
                    //combot.Enabled = true;
                }

                nAddScore = (int)(nAddScore / 10);
                nAddScore = (int)(nAddScore * 10);

                //大音符のボーナス
                if (pChip.nチャンネル番号 == 0x13 || pChip.nチャンネル番号 == 0x14 || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B)
                {
                    nAddScore = nAddScore * 2;
                }

                this.actScore.Add(nAddScore, nPlayer);
                //this.actScore.Add( E楽器パート.DRUMS, bIsAutoPlay, nAddScore );
            }
            else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 1)
            {
                if (nCombos < 10)
                {
                    nAddScore = this.nScore[nPlayer, 0];
                }
                else if (nCombos >= 10 && nCombos <= 19)
                {
                    nAddScore = this.nScore[nPlayer, 1];
                }
                else if (nCombos >= 20 && nCombos <= 29)
                {
                    nAddScore = this.nScore[nPlayer, 2];
                }
                else if (nCombos >= 30 && nCombos <= 39)
                {
                    nAddScore = this.nScore[nPlayer, 3];
                }
                else if (nCombos >= 40 && nCombos <= 49)
                {
                    nAddScore = this.nScore[nPlayer, 4];
                }
                else if (nCombos >= 50 && nCombos <= 59)
                {
                    nAddScore = this.nScore[nPlayer, 5];
                }
                else if (nCombos >= 60 && nCombos <= 69)
                {
                    nAddScore = this.nScore[nPlayer, 6];
                }
                else if (nCombos >= 70 && nCombos <= 79)
                {
                    nAddScore = this.nScore[nPlayer, 7];
                }
                else if (nCombos >= 80 && nCombos <= 89)
                {
                    nAddScore = this.nScore[nPlayer, 8];
                }
                else if (nCombos >= 90 && nCombos <= 99)
                {
                    nAddScore = this.nScore[nPlayer, 9];
                }
                else if (nCombos >= 100)
                {
                    nAddScore = this.nScore[nPlayer, 10];
                }

                if (eJudgeResult == EJudge.Good)
                {
                    nAddScore = nAddScore / 2;
                }


                if (pChip.bGOGOTIME) //2018.03.11 kairera0467 チップに埋め込んだフラグから読み取る
                    nAddScore = (int)(nAddScore * 1.2f);

                nAddScore = (int)(nAddScore / 10.0);
                nAddScore = (int)(nAddScore * 10);

                //大音符のボーナス
                if (pChip.nチャンネル番号 == 0x13 || pChip.nチャンネル番号 == 0x14 || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B)
                {
                    nAddScore = nAddScore * 2;
                }

                this.actScore.Add(nAddScore, nPlayer);
            }
            else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 0)
            {
                if (nCombos < 200)
                {
                    nAddScore = 1000;
                }
                else
                {
                    nAddScore = 2000;
                }

                if (eJudgeResult == EJudge.Good)
                    nAddScore = nAddScore / 2;

                if (pChip.bGOGOTIME) //2018.03.11 kairera0467 チップに埋め込んだフラグから読み取る
                    nAddScore = (int)(nAddScore * 1.2f);

                //大音符のボーナス
                if (pChip.nチャンネル番号 == 0x13 || pChip.nチャンネル番号 == 0x25)
                {
                    nAddScore = nAddScore * 2;
                }


                this.actScore.Add(nAddScore, nPlayer);
                //this.actScore.Add( E楽器パート.DRUMS, bIsAutoPlay, nAddScore );
            }

            //キーを押したときにスコア情報 + nAddScoreを置き換える様に
            this.CBranchScore[nPlayer].nScore = (int)(this.actScore.GetScore(nPlayer) + nAddScore);
        }
        return EJudge.AutoPerfect;
    }

    protected int? r指定時間に一番近い小節線を未来方向に検索する(long nTime, int nPlayer)
    {
        CDTX dTX = TJAPlayer3.DTX[nPlayer];
        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            if (dTX.listChip[i].nチャンネル番号 == 0x50 && dTX.listChip[i].n発声時刻ms >= nTime)
            {
                return dTX.listChip[i].n発声時刻ms;
            }
        }
        return null;
    }

    /// <summary>
    /// 最も判定枠に近いノーツを返します。
    /// </summary>
    /// <param name="nowTime">判定時の時間。</param>
    /// <param name="player">プレイヤー。</param>
    /// <returns>最も判定枠に近いノーツ。</returns>
    protected CDTX.CChip? GetChipOfNearest(long nowTime, int player)
    {
        var nearestChip = new CDTX.CChip();
        var count = TJAPlayer3.DTX[player].listChip.Count;
        var chips = TJAPlayer3.DTX[player].listChip;
        var startPosision = NowProcessingChip[player];
        CDTX.CChip? pastChip; // 判定されるべき過去ノート
        CDTX.CChip? futureChip; // 判定されるべき未来ノート
        var pastJudge = EJudge.Miss;
        var futureJudge = EJudge.Miss;

        if (count <= 0)
        {
            return null;
        }

        if (startPosision >= count)
        {
            startPosision -= 1;
        }

        #region 過去のノーツで、かつ可判定以上のノーツの決定
        CDTX.CChip? afterChip = null;
        for (int pastNote = startPosision - 1; ; pastNote--)
        {
            if (pastNote < 0)
            {
                pastChip = afterChip is not null ? afterChip : null; // afterChipに過去の判定があるかもしれないので
                break;
            }
            var processingChip = chips[pastNote];

            if (!processingChip.IsHitted && processingChip.b可視) // まだ判定されてない音符
            {
                if (((0x11 <= processingChip.nチャンネル番号) && (processingChip.nチャンネル番号 <= 0x18))
                    || processingChip.nチャンネル番号 == 0x1A
                    || processingChip.nチャンネル番号 == 0x1B
                    || processingChip.nチャンネル番号 == 0x1F) // 音符のチャンネルである
                {
                    var thisChipJudge = pastJudge = e指定時刻からChipのJUDGEを返すImpl(nowTime, processingChip);
                    if (thisChipJudge != EJudge.Miss)
                    {
                        // 判定が見過ごし不可ではない(=たたいて不可以上)
                        // その前のノートがもしかしたら存在して、可以上の判定かもしれないからまだ処理を続行する。
                        afterChip = processingChip;
                        continue;
                    }
                    else
                    {
                        // 判定が不可だった
                        // その前のノーツを過去で可以上のノート(つまり判定されるべきノート)とする。
                        pastChip = afterChip;
                        break; // 検索終わり
                    }
                }
            }
            if (processingChip.IsHitted && processingChip.b可視) // 連打
            {
                if ((0x15 <= processingChip.nチャンネル番号) && (processingChip.nチャンネル番号 <= 0x17))
                {
                    if (processingChip.cEndChip.n発声時刻ms > nowTime)
                    {
                        pastChip = processingChip;
                        break;
                    }
                }
            }
        }
        #endregion

        #region 未来のノーツで、かつ可判定以上のノーツの決定
        for (int futureNote = startPosision; ; futureNote++)
        {
            if (futureNote >= count)
            {
                futureChip = null;
                break;
            }
            var processingChip = chips[futureNote];

            if (!processingChip.IsHitted && processingChip.b可視) // まだ判定されてない音符
            {
                if (((0x11 <= processingChip.nチャンネル番号) && (processingChip.nチャンネル番号 <= 0x18))
                    || processingChip.nチャンネル番号 == 0x1A
                    || processingChip.nチャンネル番号 == 0x1B
                    || processingChip.nチャンネル番号 == 0x1F) // 音符のチャンネルである
                {
                    var thisChipJudge = futureJudge = e指定時刻からChipのJUDGEを返すImpl(nowTime, processingChip);
                    if (thisChipJudge != EJudge.Miss)
                    {
                        // 判定が見過ごし不可ではない(=たたいて不可以上)
                        // そのノートを処理すべきなので、検索終わり。
                        futureChip = processingChip;
                        break; // 検索終わり
                    }
                    else
                    {
                        // 判定が不可だった
                        // つまり未来に処理すべきノートはないので、検索終わり。
                        futureChip = null; // 今処理中のノート
                        break; // 検索終わり
                    }
                }
            }
        }
        #endregion

        #region 過去のノーツが見つかったらそれを返却、そうでなければ未来のノーツを返却
        if ((pastJudge == EJudge.Miss || pastJudge == EJudge.Bad) && (pastJudge != EJudge.Miss && pastJudge != EJudge.Bad))
        {
            // 過去の判定が不可で、未来の判定が可以上なら未来を返却。
            nearestChip = futureChip;
        }
        else if (futureChip is null && pastChip is not null)
        {
            // 未来に処理するべきノートがなかったので、過去の処理すべきノートを返す。
            nearestChip = pastChip;
        }
        else if (pastChip is null && futureChip is not null)
        {
            // 過去の検索が該当なしだったので、未来のノートを返す。
            nearestChip = futureChip;
        }
        else
        {
            // 基本的には過去のノートを返す。
            nearestChip = pastChip;
        }
        #endregion
#if DEBUG
        if (player == 0)
        {
            TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, pastChip is not null ? pastChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 20, C文字コンソール.EFontType.白, futureChip is not null ? futureChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 40, C文字コンソール.EFontType.白, nearestChip is not null ? nearestChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 60, C文字コンソール.EFontType.白, startPosision.ToString());

        }
#endif
        return nearestChip;
    }
    /// <summary>
    /// 最も判定枠に近いドンカツを返します。
    /// </summary>
    /// <param name="nowTime">判定時の時間。</param>
    /// <param name="player">プレイヤー。</param>
    /// <param name="don">ドンかどうか。</param>
    /// <returns>最も判定枠に近いノーツ。</returns>
    protected CDTX.CChip? GetChipOfNearest(long nowTime, int player, bool don)
    {
        var nearestChip = new CDTX.CChip();
        var count = TJAPlayer3.DTX[player].listChip.Count;
        var chips = TJAPlayer3.DTX[player].listChip;
        var startPosision = NowProcessingChip[player];
        CDTX.CChip? pastChip; // 判定されるべき過去ノート
        CDTX.CChip? futureChip; // 判定されるべき未来ノート
        var pastJudge = EJudge.Miss;
        var futureJudge = EJudge.Miss;

        bool GetDon(CDTX.CChip note)
        {
            return note.nチャンネル番号 == 0x11 || note.nチャンネル番号 == 0x13 || note.nチャンネル番号 == 0x1A || note.nチャンネル番号 == 0x1F;
        }
        bool GetKatsu(CDTX.CChip note)
        {
            return note.nチャンネル番号 == 0x12 || note.nチャンネル番号 == 0x14 || note.nチャンネル番号 == 0x1B || note.nチャンネル番号 == 0x1F;
        }

        if (count <= 0)
        {
            return null;
        }

        if (startPosision >= count)
        {
            startPosision -= 1;
        }

        #region 過去のノーツで、かつ可判定以上のノーツの決定
        CDTX.CChip? afterChip = null;
        for (int pastNote = startPosision - 1; ; pastNote--)
        {
            if (pastNote < 0)
            {
                pastChip = afterChip is not null ? afterChip : null; // afterChipに過去の判定があるかもしれないので
                break;
            }
            var processingChip = chips[pastNote];

            if (!processingChip.IsHitted && processingChip.b可視) // まだ判定されてない音符
            {
                if (don ? GetDon(processingChip) : GetKatsu(processingChip)) // 音符のチャンネルである
                {
                    var thisChipJudge = pastJudge = e指定時刻からChipのJUDGEを返すImpl(nowTime, processingChip);
                    if (thisChipJudge != EJudge.Miss)
                    {
                        // 判定が見過ごし不可ではない(=たたいて不可以上)
                        // その前のノートがもしかしたら存在して、可以上の判定かもしれないからまだ処理を続行する。
                        afterChip = processingChip;
                        continue;
                    }
                    else
                    {
                        // 判定が不可だった
                        // その前のノーツを過去で可以上のノート(つまり判定されるべきノート)とする。
                        pastChip = afterChip;
                        break; // 検索終わり
                    }
                }
            }
            if (processingChip.IsHitted && processingChip.b可視) // 連打
            {
                if ((0x15 <= processingChip.nチャンネル番号) && (processingChip.nチャンネル番号 <= 0x17))
                {
                    if (processingChip.cEndChip.n発声時刻ms > nowTime)
                    {
                        pastChip = processingChip;
                        break;
                    }
                }
            }
        }
        #endregion

        #region 未来のノーツで、かつ可判定以上のノーツの決定
        for (int futureNote = startPosision; ; futureNote++)
        {
            if (futureNote >= count)
            {
                futureChip = null;
                break;
            }
            var processingChip = chips[futureNote];

            if (!processingChip.IsHitted && processingChip.b可視) // まだ判定されてない音符
            {
                if (don ? GetDon(processingChip) : GetKatsu(processingChip)) // 音符のチャンネルである
                {
                    var thisChipJudge = futureJudge = e指定時刻からChipのJUDGEを返すImpl(nowTime, processingChip);
                    if (thisChipJudge != EJudge.Miss)
                    {
                        // 判定が見過ごし不可ではない(=たたいて不可以上)
                        // そのノートを処理すべきなので、検索終わり。
                        futureChip = processingChip;
                        break; // 検索終わり
                    }
                    else
                    {
                        // 判定が不可だった
                        // つまり未来に処理すべきノートはないので、検索終わり。
                        futureChip = null; // 今処理中のノート
                        break; // 検索終わり
                    }
                }
            }
        }
        #endregion

        #region 過去のノーツが見つかったらそれを返却、そうでなければ未来のノーツを返却
        if ((pastJudge == EJudge.Miss || pastJudge == EJudge.Bad) && (pastJudge != EJudge.Miss && pastJudge != EJudge.Bad))
        {
            // 過去の判定が不可で、未来の判定が可以上なら未来を返却。
            nearestChip = futureChip;
        }
        else if (futureChip is null && pastChip is not null)
        {
            // 未来に処理するべきノートがなかったので、過去の処理すべきノートを返す。
            nearestChip = pastChip;
        }
        else if (pastChip is null && futureChip is not null)
        {
            // 過去の検索が該当なしだったので、未来のノートを返す。
            nearestChip = futureChip;
        }
        else
        {
            // 基本的には過去のノートを返す。
            nearestChip = pastChip;
        }
        #endregion
#if DEBUG
        if (player == 0)
        {
            TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, pastChip is not null ? pastChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 20, C文字コンソール.EFontType.白, futureChip is not null ? futureChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 40, C文字コンソール.EFontType.白, nearestChip is not null ? nearestChip.ToString() : "null");
            TJAPlayer3.app.act文字コンソール.tPrint(0, 60, C文字コンソール.EFontType.白, startPosision.ToString());

        }
#endif
        return nearestChip;
    }

    public bool r検索範囲内にチップがあるか調べる(long nTime, int n検索範囲時間ms, int nPlayer)
    {
        for (int i = 0; i < TJAPlayer3.DTX[nPlayer].listChip.Count; i++)
        {
            CDTX.CChip chip = TJAPlayer3.DTX[nPlayer].listChip[i];
            if (!chip.bHit)
            {
                if (((0x11 <= chip.nチャンネル番号) && (chip.nチャンネル番号 <= 0x14)) || chip.nチャンネル番号 == 0x1A || chip.nチャンネル番号 == 0x1B)
                {
                    if (chip.n発声時刻ms < nTime + n検索範囲時間ms)
                    {
                        if (chip.nコース == this.n現在のコース[nPlayer]) //2016.06.14 kairera0467 譜面分岐も考慮するようにしてみる。
                            return true;
                    }
                }
            }
        }

        return false;
    }

    protected void ChangeInputAdjustTimeInPlaying(IInputDevice keyboard, int plusminus)		// #23580 2011.1.16 yyagi UI for InputAdjustTime in playing screen.
    {
        int offset = (keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl)) ? plusminus : plusminus * 10;

        TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs = Math.Clamp(TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs + offset, -99, 99);
    }

    protected bool tドラムヒット処理(long nHitTime, EPad type, CDTX.CChip pChip, bool b両手入力, int nPlayer)
    {
        if (pChip is null)
            return false;

        int nInput = 0;

        switch (type)
        {
            case EPad.LRed:
            case EPad.RRed:
            case EPad.LRed2P:
            case EPad.RRed2P:
                nInput = 0;
                if (b両手入力)
                    nInput = 2;
                break;
            case EPad.LBlue:
            case EPad.RBlue:
            case EPad.LBlue2P:
            case EPad.RBlue2P:
                nInput = 1;
                if (b両手入力)
                    nInput = 3;
                break;
        }

        if (pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x17)
        {
            this.tチップのヒット処理(nHitTime, pChip, true, nInput, nPlayer);
            return true;
        }
        else if (!((pChip.nチャンネル番号 >= 0x11) && (pChip.nチャンネル番号 <= 0x14) || (pChip.nチャンネル番号 >= 0x1A) && (pChip.nチャンネル番号 <= 0x1B) || pChip.nチャンネル番号 == 0x1F))
        {
            return false;
        }

        EJudge e判定 = this.e指定時刻からChipのJUDGEを返す(nHitTime, pChip);
        //if( pChip.nコース == this.n現在のコース )
        this.actGame.t叩ききりまショー_判定から各数値を増加させる(e判定, (int)(nHitTime - pChip.n発声時刻ms));
        if (e判定 == EJudge.Miss)
        {
            return false;
        }
        this.tチップのヒット処理(nHitTime, pChip, true, nInput, nPlayer);
        if ((e判定 != EJudge.Bad) && (e判定 != EJudge.Miss))
        {
            TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.Start(pChip.nチャンネル番号, e判定, b両手入力, nPlayer);

            int nFly = 0;
            switch (pChip.nチャンネル番号)
            {
                case 0x11:
                    nFly = 1;
                    break;
                case 0x12:
                    nFly = 2;
                    break;
                case 0x13:
                case 0x1A:
                    nFly = b両手入力 ? 3 : 1;
                    break;
                case 0x14:
                case 0x1B:
                    nFly = b両手入力 ? 4 : 2;
                    break;
                case 0x1F:
                    nFly = nInput == 0 ? 1 : 2;
                    break;
                default:
                    nFly = 1;
                    break;
            }


            //this.actChipFireTaiko.Start( nFly, nPlayer );
            this.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Hit);
            this.FlyingNotes.Start(nFly, nPlayer);
        }

        return true;
    }

    protected void t入力処理_ドラム()
    {
        for (int nPad = 0; nPad < (int)EPad.MAX; nPad++)
        {
            List<STInputEvent> listInputEvent = TJAPlayer3.app.Pad.GetEvents((EPad)nPad);

            if ((listInputEvent is null) || (listInputEvent.Count == 0))
                continue;

            this.t入力メソッド記憶();
            int nPadtmp = nPad;//2020.09.24 Mr-Ojii パパママサポートに対応するため、tmpをかませることにする。
            foreach (STInputEvent inputEvent in listInputEvent)
            {
                if (inputEvent.eType != EInputEventType.Pressed)
                    continue;

                long nTime = (long)(((inputEvent.nTimeStamp + TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs - CSoundManager.rc演奏用タイマ.n前回リセットした時のシステム時刻ms) * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)));

                bool bHitted = false;

                int nLane = 0;
                int nHand = 0;
                int nChannel = 0;

                if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1 && TJAPlayer3.DTX[0].bPapaMamaSupport[TJAPlayer3.stage選曲.n確定された曲の難易度[0]])//1P状態でパパママサポートがOnの譜面の場合
                {
                    if (nPadtmp >= (int)EPad.LRed2P && nPadtmp <= (int)EPad.RBlue2P)
                        nPadtmp -= 4;
                }

                //連打チップを検索してから通常音符検索
                //連打チップの検索は、
                //一番近くの連打音符を探す→時刻チェック
                //発声 < 現在時刻 && 終わり > 現在時刻

                //2015.03.19 kairera0467 Chipを1つにまとめて1つのレーン扱いにする。
                int nUsePlayer = 0;
                if (nPadtmp >= (int)EPad.LRed && nPadtmp <= (int)EPad.RBlue)
                {
                    nUsePlayer = 0;
                    if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0])
                        break;
                }
                else if (nPadtmp >= (int)EPad.LRed2P && nPadtmp <= (int)EPad.RBlue2P)
                {
                    nUsePlayer = 1;
                    if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount < 2) //プレイ人数が2人以上でなければ入力をキャンセル
                        break;
                    if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1])
                        break;
                }

                var padTo = nPadtmp - (nUsePlayer * 4);
                var isDon = padTo < 2 ? true : false;

                CDTX.CChip? chipNoHit = chip現在処理中の連打チップ[nUsePlayer] is null ? GetChipOfNearest(nTime, nUsePlayer, isDon) : GetChipOfNearest(nTime, nUsePlayer);
                EJudge e判定 = (chipNoHit is not null) ? this.e指定時刻からChipのJUDGEを返す(nTime, chipNoHit) : EJudge.Miss;

                bool b太鼓音再生フラグ = true;
                if (chipNoHit is not null)
                {
                    if (chipNoHit.nチャンネル番号 == 0x1F && (e判定 == EJudge.Perfect || e判定 == EJudge.Good))
                    {
                        this.soundAdlib[chipNoHit.nPlayerSide]?.t再生を開始する();
                        b太鼓音再生フラグ = false;
                    }
                }

                switch (nPadtmp)
                {
                    case (int)EPad.LRed:
                        nLane = 0;
                        nHand = 0;
                        nChannel = 0x11;
                        if (b太鼓音再生フラグ)
                            this.soundRed[0]?.t再生を開始する();
                        break;
                    case (int)EPad.RRed:
                        nLane = 0;
                        nHand = 1;
                        nChannel = 0x11;
                        if (b太鼓音再生フラグ)
                            this.soundRed[0]?.t再生を開始する();
                        break;
                    case (int)EPad.LBlue:
                        nLane = 1;
                        nHand = 0;
                        nChannel = 0x12;
                        if (b太鼓音再生フラグ)
                            this.soundBlue[0]?.t再生を開始する();
                        break;
                    case (int)EPad.RBlue:
                        nLane = 1;
                        nHand = 1;
                        nChannel = 0x12;
                        if (b太鼓音再生フラグ)
                            this.soundBlue[0]?.t再生を開始する();
                        break;
                    //以下2P
                    case (int)EPad.LRed2P:
                        nLane = 0;
                        nHand = 0;
                        nChannel = 0x11;
                        if (b太鼓音再生フラグ)
                            this.soundRed[1]?.t再生を開始する();
                        break;
                    case (int)EPad.RRed2P:
                        nLane = 0;
                        nHand = 1;
                        nChannel = 0x11;
                        if (b太鼓音再生フラグ)
                            this.soundRed[1]?.t再生を開始する();
                        break;
                    case (int)EPad.LBlue2P:
                        nLane = 1;
                        nHand = 0;
                        nChannel = 0x12;
                        if (b太鼓音再生フラグ)
                            this.soundBlue[1]?.t再生を開始する();
                        break;
                    case (int)EPad.RBlue2P:
                        nLane = 1;
                        nHand = 1;
                        nChannel = 0x12;
                        if (b太鼓音再生フラグ)
                            this.soundBlue[1]?.t再生を開始する();
                        break;
                }

                TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nUsePlayer].Start((PlayerLane.FlashType)nLane);
                TJAPlayer3.stage演奏ドラム画面.actMtaiko.tMtaikoEvent(nChannel, nHand, nUsePlayer);

                if (this.b連打中[nUsePlayer])
                {
                    chipNoHit = this.chip現在処理中の連打チップ[nUsePlayer];
                    e判定 = EJudge.Perfect;
                }

                if (chipNoHit is null)
                {
                    break;
                }

                #region [ (A) ヒットしていればヒット処理して次の inputEvent へ ]
                //-----------------------------
                switch (((EPad)nPadtmp))
                {
                    case EPad.LRed:
                    case EPad.LRed2P:
                        #region[ 面のヒット処理 ]
                        //-----------------------------
                        {
                            if (e判定 != EJudge.Miss && chipNoHit.nチャンネル番号 == 0x11)
                            {
                                this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, false, nUsePlayer);
                                bHitted = true;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A) && !TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                bHitted = true;
                                break;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A) && TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                if (e判定 == EJudge.Bad)
                                {
                                    this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                    bHitted = true;
                                }
                                else if (chipNoHit.eNoteState == ENoteState.none)
                                {
                                    float time = chipNoHit.n発声時刻ms - nTime;
                                    if (time <= 110)
                                    {
                                        chipNoHit.nProcessTime = (int)nTime;
                                        chipNoHit.eNoteState = ENoteState.waitleft;
                                    }
                                }
                                else if (chipNoHit.eNoteState == ENoteState.waitright)
                                {
                                    float time = chipNoHit.n発声時刻ms - (float)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                                    int nWaitTime = TJAPlayer3.app.ConfigToml.PlayOption.BigNotesWaitTime;
                                    if (time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                        bHitted = true;
                                    }
                                    else if (time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, false, nUsePlayer);
                                        bHitted = true;
                                    }
                                }
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 || chipNoHit.nチャンネル番号 == 0x17))
                            {
                                this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, false, nUsePlayer);
                            }

                            if (!bHitted)
                                break;
                            continue;
                        }
                    //-----------------------------
                    #endregion

                    case EPad.RRed:
                    case EPad.RRed2P:
                        #region[ 面のヒット処理 ]
                        //-----------------------------
                        {
                            if (e判定 != EJudge.Miss && chipNoHit.nチャンネル番号 == 0x11)
                            {
                                this.tドラムヒット処理(nTime, EPad.RRed, chipNoHit, false, nUsePlayer);
                                bHitted = true;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A) && !TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                this.tドラムヒット処理(nTime, EPad.RRed, chipNoHit, true, nUsePlayer);
                                bHitted = true;
                                break;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x1A) && TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                if (e判定 == EJudge.Bad)
                                {
                                    this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                    bHitted = true;
                                }
                                else if (chipNoHit.eNoteState == ENoteState.none)
                                {
                                    float time = chipNoHit.n発声時刻ms - nTime;
                                    if (time <= 110)
                                    {
                                        chipNoHit.nProcessTime = (int)nTime;
                                        chipNoHit.eNoteState = ENoteState.waitright;
                                    }
                                }
                                else if (chipNoHit.eNoteState == ENoteState.waitleft)
                                {
                                    float time = chipNoHit.n発声時刻ms - (float)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                                    int nWaitTime = TJAPlayer3.app.ConfigToml.PlayOption.BigNotesWaitTime;
                                    if (time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.RRed, chipNoHit, true, nUsePlayer);
                                        bHitted = true;
                                        break;
                                    }
                                    else if (time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.RRed, chipNoHit, false, nUsePlayer);
                                        bHitted = true;
                                    }
                                }
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16 || chipNoHit.nチャンネル番号 == 0x17))
                            {
                                this.tドラムヒット処理(nTime, EPad.RRed, chipNoHit, false, nUsePlayer);
                            }

                            if (!bHitted)
                                break;

                            continue;
                        }
                    //-----------------------------
                    #endregion

                    case EPad.LBlue:
                    case EPad.LBlue2P:
                        #region[ ふちのヒット処理 ]
                        //-----------------------------
                        {
                            if (e判定 != EJudge.Miss && chipNoHit.nチャンネル番号 == 0x12)
                            {
                                this.tドラムヒット処理(nTime, EPad.LBlue, chipNoHit, false, nUsePlayer);
                                bHitted = true;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B) && !TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                this.tドラムヒット処理(nTime, EPad.LBlue, chipNoHit, true, nUsePlayer);
                                bHitted = true;
                                break;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B) && TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                if (e判定 == EJudge.Bad)
                                {
                                    this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                    bHitted = true;
                                }
                                else if (chipNoHit.eNoteState == ENoteState.none)
                                {
                                    float time = chipNoHit.n発声時刻ms - nTime;
                                    if (time <= 110)
                                    {
                                        chipNoHit.nProcessTime = (int)nTime;
                                        chipNoHit.eNoteState = ENoteState.waitleft;
                                    }
                                }
                                else if (chipNoHit.eNoteState == ENoteState.waitright)
                                {
                                    float time = chipNoHit.n発声時刻ms - (float)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                                    int nWaitTime = TJAPlayer3.app.ConfigToml.PlayOption.BigNotesWaitTime;
                                    if (time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.LBlue, chipNoHit, true, nUsePlayer);
                                        bHitted = true;
                                    }
                                    else if (time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.LBlue, chipNoHit, false, nUsePlayer);
                                        bHitted = true;
                                    }
                                }
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16))
                            {
                                this.tドラムヒット処理(nTime, EPad.LBlue, chipNoHit, false, nUsePlayer);
                            }

                            if (!bHitted)
                                break;
                            continue;
                        }
                    //-----------------------------
                    #endregion

                    case EPad.RBlue:
                    case EPad.RBlue2P:
                        #region[ ふちのヒット処理 ]
                        //-----------------------------
                        {
                            if (e判定 != EJudge.Miss && chipNoHit.nチャンネル番号 == 0x12)
                            {
                                this.tドラムヒット処理(nTime, EPad.RBlue, chipNoHit, false, nUsePlayer);
                                bHitted = true;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B) && !TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                this.tドラムヒット処理(nTime, EPad.RBlue, chipNoHit, true, nUsePlayer);
                                bHitted = true;
                                break;
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1B) && TJAPlayer3.app.ConfigToml.PlayOption.BigNotesJudge)
                            {
                                if (e判定 == EJudge.Bad)
                                {
                                    this.tドラムヒット処理(nTime, EPad.LRed, chipNoHit, true, nUsePlayer);
                                    bHitted = true;
                                }
                                else if (chipNoHit.eNoteState == ENoteState.none)
                                {
                                    float time = chipNoHit.n発声時刻ms - nTime;
                                    if (time <= 110)
                                    {
                                        chipNoHit.nProcessTime = (int)nTime;
                                        chipNoHit.eNoteState = ENoteState.waitright;
                                    }
                                }
                                else if (chipNoHit.eNoteState == ENoteState.waitleft)
                                {
                                    float time = chipNoHit.n発声時刻ms - (float)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                                    int nWaitTime = TJAPlayer3.app.ConfigToml.PlayOption.BigNotesWaitTime;
                                    if (time <= 110 && chipNoHit.nProcessTime + nWaitTime > (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.RBlue, chipNoHit, true, nUsePlayer);
                                        bHitted = true;
                                        break;
                                    }
                                    else if (time <= 110 && chipNoHit.nProcessTime + nWaitTime < (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                                    {
                                        this.tドラムヒット処理(nTime, EPad.RBlue, chipNoHit, false, nUsePlayer);
                                        bHitted = true;
                                    }
                                }
                            }
                            if (e判定 != EJudge.Miss && (chipNoHit.nチャンネル番号 == 0x15 || chipNoHit.nチャンネル番号 == 0x16))
                            {
                                this.tドラムヒット処理(nTime, EPad.RBlue, chipNoHit, false, nUsePlayer);
                            }

                            if (!bHitted)
                                break;
                            continue;
                        }
                        //-----------------------------
                        #endregion
                }
                //2016.07.14 kairera0467 Adlibの場合、一括して処理を行う。
                if (e判定 != EJudge.Miss && chipNoHit.nチャンネル番号 == 0x1F)
                {
                    this.tドラムヒット処理(nTime, (EPad)nPadtmp, chipNoHit, false, nUsePlayer);
                    bHitted = true;
                }

                //-----------------------------
                #endregion
                #region [ (B) ヒットしてなかった場合は、レーンフラッシュ、パッドアニメ、空打ち音再生を実行 ]
                //-----------------------------
                // BAD or TIGHT 時の処理。
                if (TJAPlayer3.app.ConfigToml.PlayOption.Tight && !b連打中[nUsePlayer]) // 18/8/13 - 連打時にこれが発動すると困る!!! (AioiLight)
                    actGauge.Damage(chipNoHit.nコース, EJudge.Miss, 0);
                //-----------------------------
                #endregion
            }
        }
    }

    protected void ドラムスクロール速度アップ(int nPlayer)
    {
        TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] = Math.Min(TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] + 1, 2000);
    }
    protected void ドラムスクロール速度ダウン(int nPlayer)
    {
        TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] = Math.Max(TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] - 1, 1);
    }
    protected void tキー入力()
    {
        IInputDevice keyboard = TJAPlayer3.app.InputManager.Keyboard;
        if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F1))
        {
            if (!this.actPauseMenu.bIsActivePopupMenu && this.bPAUSE == false)
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音].t再生する();

                CSoundManager.rc演奏用タイマ.t一時停止();
                TJAPlayer3.app.Timer.t一時停止();
                TJAPlayer3.DTX[0].t全チップの再生一時停止();
                this.actAVI.tPauseControl();

                this.bPAUSE = true;
                this.actPauseMenu.tActivatePopupMenu(0);
            }

        }
        if ((!this.bPAUSE && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED)) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            this.t入力処理_ドラム();
            if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.UpArrow) && (keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightShift) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftShift)))
            {   // shift (+ctrl) + UpArrow (BGMAdjust)
                TJAPlayer3.DTX[0].t各自動再生音チップの再生時刻を変更する((keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl)) ? 1 : 10);
                TJAPlayer3.DTX[0].tWave再生位置自動補正();
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.DownArrow) && (keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightShift) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftShift)))
            {   // shift + DownArrow (BGMAdjust)
                TJAPlayer3.DTX[0].t各自動再生音チップの再生時刻を変更する((keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl)) ? -1 : -10);
                TJAPlayer3.DTX[0].tWave再生位置自動補正();
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.UpArrow))
            {   // UpArrow(scrollspeed up)
                if (keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl))//2020.05.29 Mr-Ojii Ctrlを押しているかどうかで、対象プレイヤーの変更
                    ドラムスクロール速度アップ(1);
                else
                    ドラムスクロール速度アップ(0);
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.DownArrow))
            {   // DownArrow (scrollspeed down)
                if (keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftControl) || keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightControl))
                    ドラムスクロール速度ダウン(1);
                else
                    ドラムスクロール速度ダウン(0);
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Delete))
            {   // del (debug info)
                TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus = !TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus;
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow))      // #24243 2011.1.16 yyagi UI for InputAdjustTime in playing screen.
            {
                ChangeInputAdjustTimeInPlaying(keyboard, -1);
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow))     // #24243 2011.1.16 yyagi UI for InputAdjustTime in playing screen.
            {
                ChangeInputAdjustTimeInPlaying(keyboard, +1);
            }
            else if ((base.eフェーズID == CStage.Eフェーズ.共通_通常状態) && (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape)) && !this.actPauseMenu.bIsActivePopupMenu)
            {   // escape (exit)
                this.t演奏中止();
            }
            else if ((keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D1) || keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D2) || keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D3)) && TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0])//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
            {
                if (!TJAPlayer3.DTX[0].bHasBranch[TJAPlayer3.stage選曲.n確定された曲の難易度[0]]) return;

                //listBRANCHを廃止したため強制分岐の開始値を
                //rc演奏用タイマ.n現在時刻msから引っ張ることに

                //判定枠に一番近いチップの情報を元に一小節分の値を計算する. 2020.04.21 akasoko26

                int course;
                if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D1))
                    course = 0;
                else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D2))
                    course = 1;
                else
                    course = 2;

                long? n1小節後 = r指定時間に一番近い小節線を未来方向に検索する((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), 0);
                if (n1小節後.HasValue)
                {
                    this.t分岐処理(course, 0, (long)n1小節後);

                    TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t分岐レイヤー_コース変化(TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.stBranch[0].nAfter, course, 0);
                    TJAPlayer3.stage演奏ドラム画面.actMtaiko.tBranchEvent(TJAPlayer3.stage演奏ドラム画面.actMtaiko.After[0], course, 0);
                    this.n現在のコース[0] = course;
                    this.n次回のコース[0] = course;

                    this.b強制的に分岐させた[0] = true;
                }
            }
            else if ((keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D4) || keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D5) || keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D6)) && TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1] && TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount >= 2)//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
            {
                if (!TJAPlayer3.DTX[1].bHasBranch[TJAPlayer3.stage選曲.n確定された曲の難易度[1]]) return;

                //listBRANCHを廃止したため強制分岐の開始値を
                //rc演奏用タイマ.n現在時刻msから引っ張ることに

                //判定枠に一番近いチップの情報を元に一小節分の値を計算する. 2020.04.21 akasoko26

                int course;
                if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D4))
                    course = 0;
                else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.D5))
                    course = 1;
                else
                    course = 2;

                long? n1小節後 = r指定時間に一番近い小節線を未来方向に検索する((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), 1);
                if (n1小節後.HasValue)
                {
                    this.t分岐処理(course, 1, (long)n1小節後);

                    TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t分岐レイヤー_コース変化(TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.stBranch[0].nAfter, course, 1);
                    TJAPlayer3.stage演奏ドラム画面.actMtaiko.tBranchEvent(TJAPlayer3.stage演奏ドラム画面.actMtaiko.After[0], course, 1);
                    this.n現在のコース[1] = course;
                    this.n次回のコース[1] = course;

                    this.b強制的に分岐させた[1] = true;
                }
            }
            if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F4))
            {
                TJAPlayer3.app.ConfigToml.Game.ShowJudgeCountDisplay = !TJAPlayer3.app.ConfigToml.Game.ShowJudgeCountDisplay;
            }
            else if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.F5))
            {
                TJAPlayer3.app.ConfigToml.Game.Background._ClipDispType = (EClipDispType)(((int)TJAPlayer3.app.ConfigToml.Game.Background._ClipDispType + 1) % 4);
            }
#if PLAYABLE
            if ( keyboard.bIsKeyPressed( (int)SlimDXKeys.Key.F6 ) )
            {
                TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0];
                this.b途中でAutoを切り替えたか[0] = true;
            }
            if ( keyboard.bIsKeyPressed( (int)SlimDXKeys.Key.F7 ) )
            {
                TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1] = !TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1];
                this.b途中でAutoを切り替えたか[1] = true;
            }
#endif
        }
        if (!this.actPauseMenu.bIsActivePopupMenu && this.bPAUSE && ((base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED)) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            if (keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Delete))
            {	// del (debug info)
                TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus = !TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus;
            }
            else if ((keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape)))
            {   // escape (exit)
                CSoundManager.rc演奏用タイマ.t再開();
                TJAPlayer3.app.Timer.t再開();
                this.t演奏中止();
            }
        }

        #region [ Minus & Equals Sound Group Level ]
        KeyboardSoundGroupLevelControlHandler.Handle(
            keyboard, TJAPlayer3.SoundGroupLevelController, TJAPlayer3.app.Skin, false);
        #endregion
    }

    protected void t入力メソッド記憶()
    {
        if (TJAPlayer3.app.Pad.stDetectedDevices.Keyboard)
        {
            this.b演奏にKeyBoardを使った = true;
        }
        if (TJAPlayer3.app.Pad.stDetectedDevices.Joypad)
        {
            this.b演奏にJoypadを使った = true;
        }
        if (TJAPlayer3.app.Pad.stDetectedDevices.MIDIIN)
        {
            this.b演奏にMIDIInputを使った = true;
        }
        if (TJAPlayer3.app.Pad.stDetectedDevices.Mouse)
        {
            this.b演奏にMouseを使った = true;
        }
    }

    protected virtual void t進行描画_AVI()
    {
        if (((base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut)) && (TJAPlayer3.app.ConfigToml.Game.Background.Movie))
        {
            this.actAVI.t進行描画();
        }
    }
    protected void t進行描画_STAGEFAILED()
    {
        if (((base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED) || (base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut)) && ((this.actStageFailed.On進行描画() != 0) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut)))
        {
            this.eFadeOut完了時の戻り値 = E演奏画面の戻り値.ステージ失敗;
            base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut;
            this.actFO.tFadeOut開始();
        }
    }

    protected void t進行描画_パネル文字列()
    {
        if ((base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            this.actPanel.On進行描画();
        }
    }
    protected void tパネル文字列の設定()
    {
        // When performing calibration, inform the player that
        // calibration is taking place, rather than
        // displaying the panel title or song title as usual.

        var panelString = TJAPlayer3.IsPerformingCalibration
            ? "Calibrating input..."
            : TJAPlayer3.DTX[0].TITLE;

        string subtitle = (TJAPlayer3.app.ConfigToml.Game._SubtitleDispMode == ESubtitleDispMode.On || (TJAPlayer3.app.ConfigToml.Game._SubtitleDispMode == ESubtitleDispMode.Compliant && TJAPlayer3.DTX[0].SUBTITLEDisp)) ? TJAPlayer3.DTX[0].SUBTITLE : null;

        this.actPanel.SetPanelString(panelString, subtitle, TJAPlayer3.stage選曲.r確定された曲.strGenre, TJAPlayer3.app.Skin.SkinConfig.Game.PanelFont.StageText);
    }


    protected void t進行描画_ゲージ()
    {
        if ((base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED) && (base.eフェーズID != CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            this.actGauge.On進行描画();
        }
    }
    protected bool t進行描画_チップ(int nPlayer)
    {
        if ((base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED) || (base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            return true;
        }
        if ((this.n現在のトップChip == -1) || (this.n現在のトップChip >= TJAPlayer3.DTX[nPlayer].listChip.Count))
        {
            return true;
        }
        if (IsDanFailed)
        {
            return true;
        }

        var n現在時刻ms = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));

        CDTX dTX = TJAPlayer3.DTX[nPlayer];

        if (this.n分岐した回数[nPlayer] == 0)
        {
            this.bUseBranch[nPlayer] = dTX.bHIDDENBRANCH ? false : dTX.bHasBranchChip;
        }

        float play_bpm_time = this.GetNowPBMTime(dTX);
        for (int nCurrentTopChip = dTX.listChip.Count - 1; nCurrentTopChip > 0; nCurrentTopChip--)
        {
            CDTX.CChip pChip = dTX.listChip[nCurrentTopChip];
            pChip.TimeSpan = (int)(pChip.n発声時刻ms - n現在時刻ms);

            if (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.Normal)
            {
                pChip.nバーからの距離dot = (int)(pChip.TimeSpan * pChip.dbBPM * pChip.dbSCROLL * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer] / 502.8594 / 5.0);//2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算を修正
                pChip.nバーからの距離dot_Y = (int)(pChip.TimeSpan * pChip.dbBPM * pChip.dbSCROLL_Y * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer] / 502.8594 / 5.0);
            }
            else if (TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.BMSCROLL || TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.HBSCROLL)
            {
                var dbSCROLL = TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.BMSCROLL ? 1.0 : pChip.dbSCROLL;

                pChip.nバーからの距離dot = (int)(3 * 0.8335 * ((pChip.fBMSCROLLTime * NOTE_GAP) - (play_bpm_time * NOTE_GAP)) * dbSCROLL * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer] / 2 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正

                if (pChip.dbSCROLL_Y != 0)
                {
                    var dbSCROLL_Y = TJAPlayer3.app.ConfigToml.ScrollMode == EScrollMode.BMSCROLL ? 1.0 : pChip.dbSCROLL_Y;
                    pChip.nバーからの距離dot_Y = (int)(3 * 0.8335 * ((pChip.fBMSCROLLTime * NOTE_GAP) - (play_bpm_time * NOTE_GAP)) * dbSCROLL_Y * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer] / 2 / 5.0);
                }
            }
            else
            {
                pChip.nバーからの距離dot = (int)(pChip.TimeSpan * TJAPlayer3.app.ConfigToml.RegSpeedBPM * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer] / 502.8594 / 5.0);//2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算を修正
            }

            if (!pChip.IsMissed && !pChip.bHit)//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
            {
                if (pChip.nチャンネル番号 >= 0x11 && pChip.nチャンネル番号 <= 0x14 || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B)//|| pChip.nチャンネル番号 == 0x9A )
                {
                    //こっちのほうが適格と考えたためフラグを変更.2020.04.20 Akasoko26
                    if (pChip.TimeSpan <= 0)
                    {
                        if (this.e指定時刻からChipのJUDGEを返す(n現在時刻ms, pChip) == EJudge.Miss)
                        {
                            pChip.IsMissed = true;
                            this.tチップのヒット処理(n現在時刻ms, pChip, false, 0, nPlayer);
                        }
                    }
                }
            }

            if (pChip.nバーからの距離dot < -150)
            {
                if (!(pChip.nチャンネル番号 >= 0x11 && pChip.nチャンネル番号 <= 0x14) || pChip.nチャンネル番号 == 0x1A || pChip.nチャンネル番号 == 0x1B)
                {
                    //2016.02.11 kairera0467
                    //太鼓の単音符の場合は座標による判定を行わない。
                    //(ここで判定をすると高スピードでスクロールしている時に見逃し不可判定が行われない。)
                    pChip.bHit = true;
                }
            }

            var cChipCurrentlyInProcess = chip現在処理中の連打チップ[nPlayer];
            if (cChipCurrentlyInProcess is not null && !cChipCurrentlyInProcess.bHit)
            {
                if (cChipCurrentlyInProcess.nチャンネル番号 >= 0x13 && cChipCurrentlyInProcess.nチャンネル番号 <= 0x15)//|| pChip.nチャンネル番号 == 0x9A )
                {
                    if (((cChipCurrentlyInProcess.nバーからの距離dot < -500) && (cChipCurrentlyInProcess.n発声時刻ms <= n現在時刻ms && cChipCurrentlyInProcess.cEndChip.n発声時刻ms >= n現在時刻ms)))
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[nPlayer])
                            this.tチップのヒット処理(n現在時刻ms, cChipCurrentlyInProcess, false, 0, nPlayer);
                    }
                }
            }

            if (pChip.nPlayerSide == nPlayer && pChip.n発声時刻ms >= n現在時刻ms)
            {
                NowProcessingChip[pChip.nPlayerSide] = nCurrentTopChip;
            }

            switch (pChip.nチャンネル番号)
            {
                #region [ 01: BGM ]
                case 0x01:	// BGM
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                        if (TJAPlayer3.app.ConfigToml.PlayOption.BGMSound)
                        {
                            dTX.tチップの再生(pChip, CSoundManager.rc演奏用タイマ.n前回リセットした時のシステム時刻ms + (long)(pChip.n発声時刻ms / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)));
                        }
                    }
                    break;
                #endregion
                #region [ 03: BPM変更 ]
                case 0x03:	// BPM変更
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                        this.actPlayInfo.dbBPM = dTX.BASEBPM; //2016.07.10 kairera0467 太鼓の仕様にあわせて修正。(そもそもの仕様が不明&コードミス疑惑)
                    }
                    break;
                #endregion
                #region [ 04, 07: EmptySlot ]
                case 0x04:
                case 0x07:
                    break;
                #endregion
                #region [ 08: BPM変更(拡張) ]
                case 0x08:	// BPM変更(拡張)
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;
                #endregion

                #region [ 11-1f: 太鼓1P ]
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                    this.t進行描画_チップ_Taiko(ref dTX, ref pChip, nPlayer);
                    break;

                case 0x15:
                case 0x16:
                case 0x17:
                    {
                        //2015.03.28 kairera0467
                        //描画順序を変えるため、メイン処理だけをこちらに残して描画処理は分離。

                        //this.t進行描画_チップ_Taiko連打(configIni, ref dTX, ref pChip);
                        //2015.04.13 kairera0467 ここを外さないと恋文2000の連打に対応できず、ここをつけないと他のコースと重なっている連打をどうにもできない。
                        //常時実行メソッドに渡したら対応できた!?
                        //if ((!pChip.bHit && (pChip.nバーからの距離dot.Drums < 0)))
                        {
                            if ((pChip.n発声時刻ms <= (int)n現在時刻ms && pChip.cEndChip.n発声時刻ms >= (int)n現在時刻ms))
                            {
                                //if( this.n現在のコース == pChip.nコース )
                                if (pChip.b可視)
                                    this.chip現在処理中の連打チップ[nPlayer] = pChip;
                            }
                        }
                        this.t進行描画_チップ_Taiko連打(ref dTX, ref pChip, nPlayer);
                    }

                    break;
                case 0x18:
                    {
                        if ((!pChip.bHit && (pChip.TimeSpan < 0)))
                        {
                            this.b連打中[nPlayer] = false;
                            this.actRoll.b表示[nPlayer] = false;
                            this.actChara.b風船連打中[nPlayer] = false;
                            pChip.bHit = true;
                            if (chip現在処理中の連打チップ[nPlayer] is not null)
                            {
                                chip現在処理中の連打チップ[nPlayer].bHit = true;
                                chip現在処理中の連打チップ[nPlayer].bShow = true;
                                if (chip現在処理中の連打チップ[nPlayer].nBalloon > chip現在処理中の連打チップ[nPlayer].nRollCount && chip現在処理中の連打チップ[nPlayer].nRollCount > 0 && actChara.CharaAction_Balloon_Miss is not null)
                                {
                                    if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer] > 0)
                                    {
                                        actChara.アクションタイマーリセット(nPlayer);
                                        actChara.bマイどんアクション中[nPlayer] = true;
                                        actChara.CharaAction_Balloon_Miss[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer] - 1, TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BalloonTimer[nPlayer], TJAPlayer3.app.Timer);
                                        if (actChara.CharaAction_Balloon_Delay[nPlayer] is not null) actChara.CharaAction_Balloon_Delay[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BalloonDelay[nPlayer] - 1, 1, TJAPlayer3.app.Timer);
                                    }
                                }
                                chip現在処理中の連打チップ[nPlayer] = null;

                            }
                        }
                        this.t進行描画_チップ_Taiko連打(ref dTX, ref pChip, nPlayer);
                    }

                    break;
                case 0x19:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                    break;

                case 0x1a:
                case 0x1b:
                case 0x1f:
                    {
                        this.t進行描画_チップ_Taiko(ref dTX, ref pChip, nPlayer);
                    }
                    break;
                #endregion
                #region [ 20-2F: EmptySlot ]
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    break;
                #endregion
                #region [ 31-3f: EmptySlot ]
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    break;
                #endregion

                #region [ 50: 小節線 ]
                case 0x50:	// 小節線
                    {
                        if (!pChip.bHit && (pChip.TimeSpan < 0))
                        {
                            if (this.actPlayInfo.NowMeasure[nPlayer] == 0)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    ctChipAnime[i] = new CCounter(0, 3, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * 1 / 4 / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                }

                                if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Normal[nPlayer] != 0)
                                {
                                    double dbPtn_Normal = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatNormal[nPlayer] / this.actChara.arモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                    this.actChara.ctChara_Normal[nPlayer] = new CCounter(0, this.actChara.arモーション番号[nPlayer].Length - 1, dbPtn_Normal, CSoundManager.rc演奏用タイマ);
                                }
                                else
                                {
                                    this.actChara.ctChara_Normal[nPlayer] = new CCounter();
                                }
                                if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
                                {
                                    double dbPtn_Clear = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatClear[nPlayer] / this.actChara.arクリアモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                    this.actChara.ctChara_Clear[nPlayer] = new CCounter(0, this.actChara.arクリアモーション番号[nPlayer].Length - 1, dbPtn_Clear, CSoundManager.rc演奏用タイマ);
                                }
                                else
                                {
                                    this.actChara.ctChara_Clear[nPlayer] = new CCounter();
                                }
                                if (TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
                                {
                                    double dbPtn_GoGo = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatGoGo[nPlayer] / this.actChara.arゴーゴーモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                    this.actChara.ctChara_GoGo[nPlayer] = new CCounter(0, this.actChara.arゴーゴーモーション番号[nPlayer].Length - 1, dbPtn_GoGo, CSoundManager.rc演奏用タイマ);
                                }
                                else
                                {
                                    this.actChara.ctChara_GoGo[nPlayer] = new CCounter();
                                }
                                if (TJAPlayer3.app.Skin.Game_Dancer_Ptn != 0)
                                {
                                    double dbUnit_dancer = (((60 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))) / this.actDancer.ar踊り子モーション番号.Length) / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                    this.actDancer.ct踊り子モーション = new CCounter(0, this.actDancer.ar踊り子モーション番号.Length - 1, dbUnit_dancer * TJAPlayer3.app.Skin.SkinConfig.Game.Dancer.Beat, CSoundManager.rc演奏用タイマ);
                                }
                                else
                                {
                                    this.actDancer.ct踊り子モーション = new CCounter();
                                }
                                if (TJAPlayer3.app.Skin.Game_Mob_Ptn != 0 && TJAPlayer3.app.Skin.SkinConfig.Game.Mob.Beat > 0) //2018.6.15 Game_Mob_Beatが0のままCCounter生成をされて無限ループが発生しないよう対策
                                {
                                    this.actMob.ctMob = new CCounter(1, 180, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * TJAPlayer3.app.Skin.SkinConfig.Game.Mob.Beat / 180 / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                    this.actMob.ctMobPtn = new CCounter(0, TJAPlayer3.app.Skin.Game_Mob_Ptn - 1, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * TJAPlayer3.app.Skin.SkinConfig.Game.Mob.PtnBeat / TJAPlayer3.app.Skin.Game_Mob_Ptn / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                }
                                else
                                {
                                    this.actMob.ctMob = new CCounter();
                                    this.actMob.ctMobPtn = new CCounter();
                                }
                                TJAPlayer3.stage演奏ドラム画面.PuchiChara.ChangeBPM(60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                            }
                            if (!bPAUSE)//2020.07.08 Mr-Ojii KabanFriends氏のコードを参考に
                            {
                                actPlayInfo.NowMeasure[nPlayer] = pChip.n整数値_内部番号;
                            }
                            pChip.bHit = true;
                        }
                        this.t進行描画_チップ_小節線(ref dTX, ref pChip, nPlayer);
                        break;
                    }
                #endregion
                #region [ 51: 拍線 ]
                case 0x51:	// 拍線
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;
                #endregion
                #region [ 54: 動画再生 ]
                case 0x54:  // 動画再生
                    if (!pChip.bHit && (pChip.TimeSpan < 0) && pChip.nPlayerSide == 0)
                    {
                        if ((dTX.listVD.TryGetValue(pChip.n整数値, out CVideoDecoder? vd)))
                        {
                            if (TJAPlayer3.app.ConfigToml.Game.Background.Movie)
                                this.actAVI.Start(vd);
                        }
                        pChip.bHit = true;
                    }
                    break;
                #endregion
                #region[ 55-60: EmptySlot ]
                case 0x55:
                case 0x56:
                case 0x57:
                case 0x58:
                case 0x59:
                    break;
                #endregion
                #region [ 61-89: EmptySlot ]
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                case 0x78:
                case 0x79:
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                    break;
                #endregion

                #region[ 90-9A: EmptySlot ]
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9A:
                    break;
                #endregion

                #region[ 9B-9F: 太鼓 ]
                case 0x9B:
                    // 段位認定モードの幕アニメーション
                    if (!pChip.bHit && (pChip.TimeSpan < 0) && pChip.nPlayerSide == 0)
                    {
                        pChip.bHit = true;
                        this.actLyric.tDeleteLyricTexture();
                        if (pChip.nコース == this.n現在のコース[nPlayer])
                        {
                            if (this.actDan.GetFailedAllChallenges())
                            {
                                this.n現在のトップChip = TJAPlayer3.DTX[0].listChip.Count - 1;	// 終端にシーク
                                IsDanFailed = true;
                                return true;
                            }
                            this.actDan.Start(this.ListDan_Number);
                            ListDan_Number++;
                        }
                    }
                    break;
                //0x9C BPM変化(アニメーション用)
                case 0x9C:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                        if (pChip.nコース == this.n現在のコース[nPlayer])
                        {
                            if (dTX.listBPM.TryGetValue(pChip.n整数値_内部番号, out CDTX.CBPM? cBPM))
                            {
                                this.actPlayInfo.dbBPM = cBPM.dbBPM値;// + dTX.BASEBPM;
                            }

                            for (int i = 0; i < 2; i++)
                            {
                                double db値 = ctChipAnime[i].db現在の値;
                                ctChipAnime[i] = new CCounter(0, 3, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * 1 / 4 / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                this.ctChipAnime[i].t時間Resetdb();
                                this.ctChipAnime[i].db現在の値 = db値;
                            }

                            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Normal[nPlayer] != 0)
                            {
                                double dbPtn_Normal = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatNormal[nPlayer] / this.actChara.arモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                double db値 = this.actChara.ctChara_Normal[nPlayer].db現在の値;
                                this.actChara.ctChara_Normal[nPlayer] = new CCounter(0, this.actChara.arモーション番号[nPlayer].Length - 1, dbPtn_Normal, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctChara_Normal[nPlayer].t時間Resetdb();
                                this.actChara.ctChara_Normal[nPlayer].db現在の値 = db値;
                            }
                            else
                            {
                                this.actChara.ctChara_Normal[nPlayer] = new CCounter();
                            }
                            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
                            {
                                double dbPtn_Clear = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatClear[nPlayer] / this.actChara.arクリアモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                double db値 = this.actChara.ctChara_Clear[nPlayer].db現在の値;
                                this.actChara.ctChara_Clear[nPlayer] = new CCounter(0, this.actChara.arクリアモーション番号[nPlayer].Length - 1, dbPtn_Clear, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctChara_Clear[nPlayer].t時間Resetdb();
                                this.actChara.ctChara_Clear[nPlayer].db現在の値 = db値;
                            }
                            else
                            {
                                this.actChara.ctChara_Clear[nPlayer] = new CCounter();
                            }
                            if (TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
                            {
                                double dbPtn_GoGo = (60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM) * TJAPlayer3.app.Skin.SkinConfig.Game.Chara.BeatGoGo[nPlayer] / this.actChara.arゴーゴーモーション番号[nPlayer].Length / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                double db値 = this.actChara.ctChara_GoGo[nPlayer].db現在の値;
                                this.actChara.ctChara_GoGo[nPlayer] = new CCounter(0, this.actChara.arゴーゴーモーション番号[nPlayer].Length - 1, dbPtn_GoGo, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctChara_GoGo[nPlayer].t時間Resetdb();
                                this.actChara.ctChara_GoGo[nPlayer].db現在の値 = db値;
                            }
                            else
                            {
                                this.actChara.ctChara_GoGo[nPlayer] = new CCounter();
                            }
                            if (TJAPlayer3.app.Skin.Game_Dancer_Ptn != 0)
                            {
                                double dbUnit_dancer = (((60 / (TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM))) / this.actDancer.ar踊り子モーション番号.Length) / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0);
                                double db値 = this.actDancer.ct踊り子モーション.db現在の値;
                                this.actDancer.ct踊り子モーション = new CCounter(0, this.actDancer.ar踊り子モーション番号.Length - 1, dbUnit_dancer * TJAPlayer3.app.Skin.SkinConfig.Game.Dancer.Beat, CSoundManager.rc演奏用タイマ);
                                this.actDancer.ct踊り子モーション.t時間Resetdb();
                                this.actDancer.ct踊り子モーション.db現在の値 = db値;
                            }
                            else
                            {
                                this.actDancer.ct踊り子モーション = new CCounter();
                            }
                            if (TJAPlayer3.app.Skin.Game_Mob_Ptn != 0)
                            {
                                double db値 = this.actMob.ctMob.db現在の値;
                                this.actMob.ctMob = new CCounter(1, 180, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * TJAPlayer3.app.Skin.SkinConfig.Game.Mob.Beat / 180 / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                this.actMob.ctMob.t時間Resetdb();
                                this.actMob.ctMob.db現在の値 = db値;

                                db値 = this.actMob.ctMobPtn.db現在の値;
                                this.actMob.ctMobPtn = new CCounter(0, TJAPlayer3.app.Skin.Game_Mob_Ptn - 1, 60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM * TJAPlayer3.app.Skin.SkinConfig.Game.Mob.PtnBeat / TJAPlayer3.app.Skin.Game_Mob_Ptn / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0), CSoundManager.rc演奏用タイマ);
                                this.actMob.ctMobPtn.t時間Resetdb();
                                this.actMob.ctMobPtn.db現在の値 = db値;
                            }
                            else
                            {
                                this.actMob.ctMob = new CCounter();
                                this.actMob.ctMobPtn = new CCounter();
                            }

                            TJAPlayer3.stage演奏ドラム画面.PuchiChara.ChangeBPM(60.0 / TJAPlayer3.stage演奏ドラム画面.actPlayInfo.dbBPM / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                            //this.actDancer.ct踊り子モーション = new CCounter(0, this.actDancer.ar踊り子モーション番号.Length - 1, (dbUnit * CDTXMania.Skin.Game_Dancer_Beat) / this.actDancer.ar踊り子モーション番号.Length, CSoundManager.rc演奏用タイマ);
                            //this.actChara.ctモブモーション = new CCounter(0, this.actChara.arモブモーション番号.Length - 1, (dbUnit) / this.actChara.arモブモーション番号.Length, CSoundManager.rc演奏用タイマ);
                        }

                    }
                    break;

                case 0x9D: //SCROLL
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;

                case 0x9E: //ゴーゴータイム
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                        this.bIsGOGOTIME[nPlayer] = true;
                        //double dbUnit = (((60.0 / (CDTXMania.stage演奏ドラム画面.actPlayInfo.dbBPM))));
                        double dbUnit = (((60.0 / pChip.dbBPM)));
                        if (TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart[nPlayer] != 0 && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                        {
                            if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] < 100)
                            {
                                // 魂ゲージMAXではない
                                // ゴーゴースタート_ノーマル
                                this.actChara.アクションタイマーリセット(nPlayer);
                                this.actChara.ctキャラクターアクション_ゴーゴースタート[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctキャラクターアクション_ゴーゴースタート[nPlayer].t進行db();
                                this.actChara.ctキャラクターアクション_ゴーゴースタート[nPlayer].db現在の値 = 0D;
                                this.actChara.bマイどんアクション中[nPlayer] = true;
                                //this.actChara.マイどん_アクション_10コンボ();
                            }
                        }
                        if (TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer] != 0 && actChara.CharaAction_Balloon_Delay[nPlayer].b終了値に達した)
                        {
                            if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[nPlayer] >= 100)
                            {
                                // 魂ゲージMAX
                                // ゴーゴースタート_MAX
                                this.actChara.アクションタイマーリセット(nPlayer);
                                this.actChara.ctキャラクターアクション_ゴーゴースタートMAX[nPlayer] = new CCounter(0, TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer] - 1, (dbUnit / TJAPlayer3.app.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer]) * 2, CSoundManager.rc演奏用タイマ);
                                this.actChara.ctキャラクターアクション_ゴーゴースタートMAX[nPlayer].t進行db();
                                this.actChara.ctキャラクターアクション_ゴーゴースタートMAX[nPlayer].db現在の値 = 0D;
                                this.actChara.bマイどんアクション中[nPlayer] = true;
                            }
                        }
                        TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.GOGOSTART();
                    }
                    break;
                case 0x9F: //ゴーゴータイム
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                        this.bIsGOGOTIME[nPlayer] = false;
                    }
                    break;
                #endregion

                #region [ a0-a8: EmptySlot ]
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                    break;
                #endregion
                #region [ B1～BC EmptySlot ]
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                    break;
                #endregion
                #region [ c4, c7, d5-d9: EmptySlot ]
                case 0xc4:
                case 0xc7:
                case 0xd5:
                case 0xd6:	// BGA画像入れ替え
                case 0xd7:
                case 0xd8:
                case 0xd9:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;
                #endregion

                #region [ da: ミキサーへチップ音追加 ]
                case 0xDA:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        //Debug.WriteLine( "[DA(AddMixer)] BAR=" + pChip.n発声位置 / 384 + " ch=" + pChip.nチャンネル番号.ToString( "x2" ) + ", wav=" + pChip.n整数値.ToString( "x2" ) + ", time=" + pChip.n発声時刻ms );
                        pChip.bHit = true;
                        if (TJAPlayer3.DTX[0].listWAV.TryGetValue(pChip.n整数値_内部番号, out CDTX.CWAV? wc))	// 参照が遠いので後日最適化する
                        {
                            if (wc.rSound is not null)
                            {
                                //CDTXMania.SoundManager.AddMixer( wc.rSound[ i ] );
                                AddMixer(wc.rSound, pChip.b演奏終了後も再生が続くチップである);
                            }
                        }
                    }
                    break;
                #endregion
                #region [ db: ミキサーからチップ音削除 ]
                case 0xDB:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        //Debug.WriteLine( "[DB(RemoveMixer)] BAR=" + pChip.n発声位置 / 384 + " ch=" + pChip.nチャンネル番号.ToString( "x2" ) + ", wav=" + pChip.n整数値.ToString( "x2" ) + ", time=" + pChip.n発声時刻ms );
                        pChip.bHit = true;
                        if (TJAPlayer3.DTX[0].listWAV.TryGetValue(pChip.n整数値_内部番号, out CDTX.CWAV? wc))	// 参照が遠いので後日最適化する
                        {
                            if (wc.rSound is not null)
                            {
                                //CDTXMania.SoundManager.RemoveMixer( wc.rSound[ i ] );
                                if (!wc.rSound.b演奏終了後も再生が続くチップである)	// #32248 2013.10.16 yyagi
                                {															// DTX終了後も再生が続くチップの0xDB登録をなくすことはできず。
                                    RemoveMixer(wc.rSound);							// (ミキサー解除のタイミングが遅延する場合の対応が面倒なので。)
                                }															// そこで、代わりにフラグをチェックしてミキサー削除ロジックへの遷移をカットする。
                            }
                        }
                    }
                    break;
                #endregion

                #region[ dc-df:太鼓(特殊命令) ]
                case 0xDC: //DELAY
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;
                case 0xDD: //SECTION //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        // 分岐毎にリセットしていたのでSECTIONの命令が来たらリセットする。
                        this.tBranchReset(nPlayer);
                        pChip.bHit = true;
                    }
                    break;
                case 0xDE: //Judgeに応じたCourseを取得//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        if (dTX.listBRANCH.TryGetValue(pChip.n整数値_内部番号, out CDTX.CBRANCH? cBranch))
                        {
                            if (!this.bLEVELHOLD[nPlayer])
                            {
                                TJAPlayer3.stage演奏ドラム画面.bUseBranch[nPlayer] = true;
                                this.tBranchJudge(cBranch, this.CBranchScore[nPlayer], this.CBranchScore[nPlayer].cBigNotes, nPlayer);

                                this.t分岐処理(this.n次回のコース[nPlayer], nPlayer, cBranch.db分岐時刻ms, cBranch.n分岐の種類);

                                TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t分岐レイヤー_コース変化(TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.stBranch[nPlayer].nAfter, this.n次回のコース[nPlayer], nPlayer);
                                TJAPlayer3.stage演奏ドラム画面.actMtaiko.tBranchEvent(TJAPlayer3.stage演奏ドラム画面.actMtaiko.After[nPlayer], this.n次回のコース[nPlayer], nPlayer);
                                this.n現在のコース[nPlayer] = this.n次回のコース[nPlayer];
                            }
                            this.n分岐した回数[nPlayer]++;
                        }
                        pChip.bHit = true;
                    }
                    break;

                case 0x52://End処理
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }

                    break;
                case 0xE0:
                    //#BARLINEONと#BARLINEOFFだったとこ
                    //演奏中は使用しません。
                    break;
                case 0xE1:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        //LEVELHOLD
                        this.bLEVELHOLD[nPlayer] = true;
                    }
                    break;
                case 0xE2:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t判定枠移動(pChip.n発声時刻ms, dTX.listJPOSSCROLL[nJPOSSCROLL[nPlayer]].db移動時間, dTX.listJPOSSCROLL[nJPOSSCROLL[nPlayer]].n移動距離px, nPlayer);
                        this.nJPOSSCROLL[nPlayer]++;
                        pChip.bHit = true;
                    }
                    else if (pChip.bHit && (pChip.TimeSpan > 0))
                    {
                        this.nJPOSSCROLL[nPlayer]--;
                        TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t判定枠戻し(dTX.listJPOSSCROLL[nJPOSSCROLL[nPlayer]].n移動距離px, nPlayer);
                        pChip.bHit = false;
                    }
                    break;
                #endregion
                #region[ f1: 歌詞 ]
                case 0xF1:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        //1Pの歌詞のみ表示
                        if (nPlayer != 0)
                            break;

                        if (this.listLyric.Count > ShownLyric && dTX.nPlayerSide == nPlayer)
                        {
                            this.actLyric.tSetLyricTexture(this.listLyric[ShownLyric++]);
                        }
                        pChip.bHit = true;
                    }
                    break;
                #endregion
                #region[ ff: 譜面の強制終了 ]
                //バグで譜面がとてつもないことになっているため、#ENDがきたらこれを差し込む。
                case 0xFF:
                    if (pChip.TimeSpan < 0)
                    {
                        if (!pChip.bHit)
                        {
                            if (this.bgmlength > CSoundManager.rc演奏用タイマ.n現在時刻ms * ((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed / 20.0))
                                break;

                            pChip.bHit = true;
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                #endregion

                #region [ その他(未定義) ]
                default:
                    if (!pChip.bHit && (pChip.TimeSpan < 0))
                    {
                        pChip.bHit = true;
                    }
                    break;
                    #endregion
            }

        }
        return false;
    }

    protected bool t進行描画_チップ_連打(int nPlayer)
    {
        if ((base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED) || (base.eフェーズID == CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut))
        {
            return true;
        }
        if ((this.n現在のトップChip == -1) || (this.n現在のトップChip >= TJAPlayer3.DTX[nPlayer].listChip.Count))
        {
            return true;
        }

        CDTX dTX = TJAPlayer3.DTX[nPlayer];

        var n現在時刻ms = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));

        for (int nCurrentTopChip = dTX.listChip.Count - 1; nCurrentTopChip > 0; nCurrentTopChip--)
        {
            CDTX.CChip pChip = dTX.listChip[nCurrentTopChip];

            if (!pChip.bHit && pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x19)
            {
                if (pChip.nバーからの距離dot < -40)
                {
                    if (this.e指定時刻からChipのJUDGEを返す(n現在時刻ms, pChip) == EJudge.Miss)
                    {
                        this.tチップのヒット処理(n現在時刻ms, pChip, false, 0, nPlayer);
                    }
                }
            }
        }
        return false;
    }

    public void tBranchReset(int player)
    {
        this.CBranchScore[player].cBigNotes.nPerfect = 0;
        this.CBranchScore[player].cBigNotes.nGood = 0;
        this.CBranchScore[player].cBigNotes.nMiss = 0;
        this.CBranchScore[player].cBigNotes.nRoll = 0;

        this.CBranchScore[player].nPerfect = 0;
        this.CBranchScore[player].nGood = 0;
        this.CBranchScore[player].nMiss = 0;
        this.CBranchScore[player].nRoll = 0;
    }

    //2020.04.23 Mr-Ojii akasokoさんの分岐方法を参考にして、変更
    public void tBranchJudge(CDTX.CBRANCH cBranch, CBRANCHSCORE cBRANCHSCORE, CBRANCHSCORE cBRANCHSCOREBIG, int nPlayer)
    {
        if (this.b強制的に分岐させた[nPlayer]) return;

        int n種類 = cBranch.n分岐の種類;

        double dbRate = 0;
        int n良 = cBRANCHSCORE.nPerfect, n可 = cBRANCHSCORE.nGood, n不可 = cBRANCHSCORE.nMiss;

        if (n種類 == 0)
        {
            if ((n良 + n可 + n不可) != 0)
            {
                dbRate = ((double)(n良 * 2.0 + n可) / (double)((n良 + n可 + n不可) * 2.0)) * 100.0; //2020.05.21 Mr-Ojii 計算式変更
            }
        }
        else if (n種類 == 1)
        {
            dbRate = cBRANCHSCORE.nScore;
        }
        else if (n種類 == 2)
        {
            dbRate = cBRANCHSCORE.nRoll;
        }
        else if (n種類 == 3)
        {
            dbRate = cBRANCHSCOREBIG.nPerfect;
        }
        Debug.Print("dbRate=" + dbRate.ToString());
        Debug.Print("nPlayer=" + nPlayer.ToString());
        Debug.Print("A=" + cBranch.db条件数値A.ToString());
        Debug.Print("B=" + cBranch.db条件数値B.ToString());

        if (n種類 == 0 || n種類 == 1)
        {
            if (dbRate < cBranch.db条件数値A)
            {
                this.n次回のコース[nPlayer] = 0;
            }
            else if (dbRate >= cBranch.db条件数値A && dbRate < cBranch.db条件数値B)
            {
                this.n次回のコース[nPlayer] = 1;
            }
            else if (dbRate >= cBranch.db条件数値B)
            {
                this.n次回のコース[nPlayer] = 2;
            }
        }
        else if (n種類 == 2)
        {
            if (!(cBranch.db条件数値A == 0 && cBranch.db条件数値B == 0))
            {
                if (dbRate < cBranch.db条件数値A)
                {
                    this.n次回のコース[nPlayer] = 0;
                }
                else if (dbRate >= cBranch.db条件数値A && dbRate < cBranch.db条件数値B)
                {
                    this.n次回のコース[nPlayer] = 1;
                }
                else if (dbRate >= cBranch.db条件数値B)
                {
                    this.n次回のコース[nPlayer] = 2;
                }
            }
        }
        else if (n種類 == 3)
        {
            if (!(cBranch.db条件数値A == 0 && cBranch.db条件数値B == 0))
            {
                if (dbRate < cBranch.db条件数値A)
                {
                    this.n次回のコース[nPlayer] = 0;
                }
                else if (dbRate >= cBranch.db条件数値A && dbRate < cBranch.db条件数値B)
                {
                    this.n次回のコース[nPlayer] = 1;
                }
                else if (dbRate >= cBranch.db条件数値B)
                {
                    this.n次回のコース[nPlayer] = 2;
                }
            }
        }
    }

    public void t分岐処理(int n分岐先, int nPlayer, double n発声位置, int n分岐種類 = 0)//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
    {
        CDTX dTX = TJAPlayer3.DTX[nPlayer];

        for (int A = 0; A < dTX.listChip.Count; A++)
        {
            var Chip = dTX.listChip[A].nチャンネル番号;
            var bDontDeleteFlag = Chip >= 0x11 && Chip <= 0x19;
            var bRollAllFlag = Chip >= 0x15 && Chip <= 0x19;
            var bBalloonOnlyFlag = Chip == 0x17;
            var bRollOnlyFlag = Chip >= 0x15 && Chip <= 0x16;
            if (bDontDeleteFlag)
            {
                if (dTX.listChip[A].n発声時刻ms >= n発声位置)
                {
                    if (dTX.listChip[A].nコース == n分岐先)
                    {
                        dTX.listChip[A].b可視 = true;

                        if (dTX.listChip[A].IsEndedBranching)
                        {
                            if (bRollAllFlag)//共通譜面時かつ、連打譜面だったら非可視化
                            {
                                dTX.listChip[A].bHit = true;
                                dTX.listChip[A].bShow = false;
                                dTX.listChip[A].b可視 = false;
                            }
                        }
                    }
                    else
                    {
                        dTX.listChip[A].b可視 = false;
                    }
                    //共通なため分岐させない.
                    dTX.listChip[A].eNoteState = ENoteState.none;

                    if (dTX.listChip[A].IsEndedBranching && (dTX.listChip[A].nコース == 0))
                    {
                        if (bRollOnlyFlag)//共通譜面時かつ、連打譜面だったら可視化
                        {
                            dTX.listChip[A].bHit = false;
                            dTX.listChip[A].bShow = true;
                            dTX.listChip[A].b可視 = true;
                        }
                        else
                        {
                            if (bBalloonOnlyFlag)//共通譜面時かつ、風船譜面だったら可視化
                            {
                                dTX.listChip[A].bShow = true;
                                dTX.listChip[A].b可視 = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public int GetRoll(int player)
    {
        return n合計連打数[player];
    }

    protected float GetNowPBMTime(CDTX tja)
    {
        float play_time = CSoundManager.rc演奏用タイマ.n現在時刻ms * (((float)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0f) - tja.nOFFSET; ;
        float bpm_time = 0;

        for (int i = tja.listBPM.Count - 1; i >= 0; i--)
        {
            CDTX.CBPM cBPM = tja.listBPM[i];
            if ((cBPM.bpm_change_time == 0 || cBPM.bpm_change_course == this.n現在のコース[0]) && tja.listBPM[i].bpm_change_time < play_time)
            {
                bpm_time = (float)cBPM.bpm_change_bmscroll_time + ((play_time - (float)cBPM.bpm_change_time) * (float)cBPM.dbBPM値 / 15000.0f);
                break;
            }
        }

        return bpm_time;
    }

    public void t再読込()
    {
        TJAPlayer3.DTX[0].t全チップの再生停止とミキサーからの削除();
        this.eFadeOut完了時の戻り値 = E演奏画面の戻り値.再読込_再演奏;
        base.eフェーズID = CStage.Eフェーズ.演奏_再読込;
        this.bPAUSE = false;
    }

    public void t演奏やりなおし()
    {
        TJAPlayer3.DTX[0].t全チップの再生停止とミキサーからの削除();
        this.t数値の初期化(true, true);
        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan)
        {
            TJAPlayer3.stage演奏ドラム画面.actDan.Update();
        }
        this.actLyric.tDeleteLyricTexture();
        for (int i = 0; i < 2; i++)
        {
            this.t演奏位置の変更(0, i);
            this.actPlayInfo.NowMeasure[i] = 0;
            actChara.bマイどんアクション中[i] = false;
            this.actChara.b風船連打中[i] = false;
            this.chip現在処理中の連打チップ[i] = null;
        }
        TJAPlayer3.stage演奏ドラム画面.On活性化();
        for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
        {
            this.actChara.アクションタイマーリセット(nPlayer);
        }
        this.bPAUSE = false;
    }

    public void t数値の初期化(bool b演奏記録, bool b演奏状態)
    {
        if (b演奏記録)
        {
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                this.nヒット数[i].Perfect = 0;
                this.nヒット数[i].Good = 0;
                this.nヒット数[i].Bad = 0;
                this.nヒット数[i].Miss = 0;
            }

            this.actCombo.On活性化();
            this.actScore.On活性化();
            this.actGauge.Init(TJAPlayer3.app.ConfigToml.PlayOption.Risky);
        }
        if (b演奏状態)
        {
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                this.bIsGOGOTIME[i] = false;
                this.bLEVELHOLD[i] = false;
                this.b強制的に分岐させた[i] = false;
                this.b譜面分岐中[i] = false;
                this.b連打中[i] = false;
                this.n現在のコース[i] = 0;
                this.n次回のコース[i] = 0;
                this.n現在の連打数[i] = 0;
                this.n合計連打数[i] = 0;
                this.n分岐した回数[i] = 0;
                this.ReSetScore(TJAPlayer3.DTX[i].nScoreInit[0, TJAPlayer3.stage選曲.n確定された曲の難易度[i]], TJAPlayer3.DTX[i].nScoreDiff[TJAPlayer3.stage選曲.n確定された曲の難易度[i]], i);
            }
            for (int i = 0; i < 2; i++)
            {
                this.actComboVoice.tReset(i);
                NowProcessingChip[i] = 0;
            }
        }

        this.nHand = new int[] { 0, 0, 0, 0 };
    }

    public void t演奏位置の変更(int nStartBar, int nPlayer)
    {
        // まず全サウンドオフにする
        TJAPlayer3.DTX[0].t全チップの再生停止();
        this.actAVI.Stop();
        CDTX dTX = TJAPlayer3.DTX[nPlayer];

        if (dTX is null) return; //CDTXがnullの場合はプレイヤーが居ないのでその場で処理終了


        #region [ 再生開始小節の変更 ]

        #region [ 演奏済みフラグのついたChipをリセットする ]
        for (int i = 0; i < dTX.listChip.Count; i++)//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
        {
            //フラグが付いてなくてもすべてのチップをリセットする。(必要がある).2020.04.23.akasoko26
            dTX.listChip[i].bHit = false;
            if (dTX.listChip[i].nチャンネル番号 != 0x50)//2020.08.01 Mr-Ojii BARLINEOFF/ONのための変更
            {
                dTX.listChip[i].bShow = true;
                dTX.listChip[i].b可視 = true;
            }
            dTX.listChip[i].IsHitted = false;
            dTX.listChip[i].IsMissed = false;
            dTX.listChip[i].eNoteState = ENoteState.none;
            dTX.listChip[i].nProcessTime = 0;
            dTX.listChip[i].nRollCount = 0;
        }
        #endregion

        #region [ 処理を開始するチップの特定 ]
        bool bSuccessSeek = false;
        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            CDTX.CChip pChip = dTX.listChip[i];
            if (nStartBar == 0)
            {
                if (pChip.n発声位置 < 384 * nStartBar)
                {
                    continue;
                }
                else
                {
                    bSuccessSeek = true;
                    this.n現在のトップChip = i;
                    break;
                }
            }
            else
            {
                if (pChip.nチャンネル番号 == 0x50 && pChip.n整数値_内部番号 > nStartBar - 1)
                {
                    bSuccessSeek = true;
                    this.n現在のトップChip = i;
                    break;
                }
            }
        }
        if (!bSuccessSeek)
        {
            this.n現在のトップChip = 0;       // 対象小節が存在しないなら、最初から再生
        }
        else
        {
            while (this.n現在のトップChip != 0 && dTX.listChip[this.n現在のトップChip].n発声時刻ms == dTX.listChip[TJAPlayer3.stage演奏ドラム画面.n現在のトップChip - 1].n発声時刻ms)
                TJAPlayer3.stage演奏ドラム画面.n現在のトップChip--;
        }
        #endregion

        #region [ 演奏開始の発声時刻msを取得し、タイマに設定 ]
        int nStartTime = (int)(dTX.listChip[this.n現在のトップChip].n発声時刻ms / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));

        CSoundManager.rc演奏用タイマ.tリセット();	// これでPAUSE解除されるので、次のPAUSEチェックは不要
        CSoundManager.rc演奏用タイマ.t一時停止();
        CSoundManager.rc演奏用タイマ.n現在時刻ms = nStartTime;
        #endregion

        List<CSound> pausedCSound = new List<CSound>();

        #region [ BGMやギターなど、演奏開始のタイミングで再生がかかっているサウンドのの途中再生開始 ] // (CDTXのt入力_行解析_チップ配置()で小節番号が+1されているのを削っておくこと)
        for (int i = this.n現在のトップChip; i >= 0; i--)
        {
            CDTX.CChip pChip = dTX.listChip[i];
            if (pChip.nチャンネル番号 == 0x01 && (pChip.nチャンネル番号 >> 4) != 0xB) // wav系チャンネル、且つ、空打ちチップではない
            {
                int nDuration = pChip.GetDuration();
                long n発声時刻ms = (long)(pChip.n発声時刻ms / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));

                if ((n発声時刻ms + nDuration > 0) && (n発声時刻ms <= nStartTime) && (nStartTime <= n発声時刻ms + nDuration))
                {
                    CDTX.CWAV wc;
                    bool b = dTX.listWAV.TryGetValue(pChip.n整数値_内部番号, out wc);
                    if (!b) continue;

                    if ((wc.bUse && TJAPlayer3.app.ConfigToml.PlayOption.BGMSound))
                    {
                        TJAPlayer3.DTX[0].tチップの再生(pChip, (long)(CSoundManager.rc演奏用タイマ.n前回リセットした時のシステム時刻ms) + (long)(n発声時刻ms));
                        #region [ PAUSEする ]
                        if (wc.rSound is not null)
                        {
                            wc.rSound.t再生を一時停止する();
                            wc.rSound.t再生位置を変更する(nStartTime - n発声時刻ms);
                            pausedCSound.Add(wc.rSound);
                        }
                        #endregion
                    }
                }
            }
        }
        #endregion

        #region [ 演奏開始時点で既に表示されているBGAとAVIの、シークと再生 ]
        if (dTX.listVD.Count > 0)
            for (int i = 0; i < dTX.listChip.Count; i++)
                if (dTX.listChip[i].nチャンネル番号 == 0x54)
                    if (dTX.listChip[i].n発声時刻ms <= nStartTime)
                    {
                        this.actAVI.Seek(nStartTime - dTX.listChip[i].n発声時刻ms);
                        this.actAVI.Start(this.actAVI.rVD);
                        break;
                    }
                    else
                    {
                        this.actAVI.Seek(0);
                    }


        #endregion
        #region [ PAUSEしていたサウンドを一斉に再生再開する(ただしタイマを止めているので、ここではまだ再生開始しない) ]

        if (!(TJAPlayer3.app.ConfigToml.PlayOption.PlayBGMOnlyPlaySpeedEqualsOne && TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed != 20))
            foreach (CSound cs in pausedCSound)
            {
                cs.tサウンドを再生する();
            }
        pausedCSound.Clear();
        pausedCSound = null;
        #endregion
        #region [ タイマを再開して、PAUSEから復帰する ]
        CSoundManager.rc演奏用タイマ.n現在時刻ms = nStartTime;
        TJAPlayer3.app.Timer.tリセット();						// これでPAUSE解除されるので、3行先の再開()は不要
        TJAPlayer3.app.Timer.n現在時刻ms = nStartTime;				// Debug表示のTime: 表記を正しくするために必要
        CSoundManager.rc演奏用タイマ.t再開();
        this.bPAUSE = false;								// システムがPAUSE状態だったら、強制解除
        this.actPanel.Start();
        #endregion
        #endregion
    }

    public void t演奏中止()
    {
        this.actFO.tFadeOut開始();
        base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
        this.eFadeOut完了時の戻り値 = E演奏画面の戻り値.演奏中断;
    }

    protected void t進行描画_チップ_Taiko(ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer)
    {
        #region[ 作り直したもの ]
        if (!pChip.b可視 || pChip.bHit || CSoundManager.rc演奏用タイマ is null)
            return;

        long nPlayTime = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
        if ((!pChip.bHit) && (pChip.n発声時刻ms <= nPlayTime))
        {
            bool bAutoPlay = false;
            switch (nPlayer)
            {
                case 0:
                    bAutoPlay = TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0];
                    break;
                case 1:
                    bAutoPlay = TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[1];
                    break;
                case 2:
                case 3:
                    bAutoPlay = true;
                    break;
            }

            if (bAutoPlay && !this.bPAUSE)
            {
                pChip.bHit = true;
                if (pChip.nチャンネル番号 != 0x1F)
                    this.FlyingNotes.Start(pChip.nチャンネル番号 < 0x1A ? (pChip.nチャンネル番号 - 0x10) : (pChip.nチャンネル番号 - 0x17), nPlayer);
                //this.actChipFireTaiko.Start(pChip.nチャンネル番号 < 0x1A ? (pChip.nチャンネル番号 - 0x10) : (pChip.nチャンネル番号 - 0x17), nPlayer);
                int nLane = (pChip.nチャンネル番号 == 0x12 || pChip.nチャンネル番号 == 0x14 || pChip.nチャンネル番号 == 0x1B) ? 1 : 0;
                TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start((nLane == 0 ? PlayerLane.FlashType.Red : PlayerLane.FlashType.Blue));
                TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.PlayerLane[nPlayer].Start(PlayerLane.FlashType.Hit);
                this.actMtaiko.tMtaikoEvent(pChip.nチャンネル番号, this.nHand[nPlayer], nPlayer);

                int n大音符 = (pChip.nチャンネル番号 == 0x11 || pChip.nチャンネル番号 == 0x12 ? 2 : 0);

                this.tチップのヒット処理(pChip.n発声時刻ms, pChip, true, nLane + n大音符, nPlayer);
                this.tサウンド再生(pChip);
                return;
            }
        }


        if (pChip.nノーツ出現時刻ms != 0 && (nPlayTime < pChip.n発声時刻ms - pChip.nノーツ出現時刻ms))
            pChip.bShow = false;
        else
            pChip.bShow = true;

        int x = 0;
        int y = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[nPlayer];

        if (pChip.nノーツ移動開始時刻ms != 0 && (nPlayTime < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms))
        {
            x = (int)((((pChip.n発声時刻ms) - (pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms)) * pChip.dbBPM * pChip.dbSCROLL * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer]) / 502.8594 / 5.0); // 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
        }
        else
        {
            x = pChip.nバーからの距離dot;
        }

        x += (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer]);

        #region[ 両手待ち時 ]
        if ((pChip.eNoteState == ENoteState.waitleft || pChip.eNoteState == ENoteState.waitright) && TJAPlayer3.app.ConfigToml.Game.BigNotesJudgeFrame)
        {
            x = (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[pChip.nPlayerSide]);
        }
        #endregion

        if (pChip.dbSCROLL_Y != 0.0)
            y = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[nPlayer] + pChip.nバーからの距離dot_Y;

        if (pChip.TimeSpan < 0)
        {
            this.actGame.st叩ききりまショー.b最初のチップが叩かれた = true;
        }

        if ((1400 > x) && TJAPlayer3.app.Tx.Notes is not null)
        {
            int num9 = this.n顔座標[nPlayer];

            int nSenotesY = TJAPlayer3.app.Skin.SkinConfig.Game.SENotesOffsetY[nPlayer];
            this.ct手つなぎ.t進行Loop();
            int nHand = this.ct手つなぎ.n現在の値 < 30 ? this.ct手つなぎ.n現在の値 : 60 - this.ct手つなぎ.n現在の値;


            x = (x) - ((int)((130.0 * 1.0) / 2.0));
            TJAPlayer3.app.Tx.Notes.eBlendMode = CTexture.EBlendMode.Normal;
            if (TJAPlayer3.app.Tx.SENotes is not null)
                TJAPlayer3.app.Tx.SENotes.eBlendMode = CTexture.EBlendMode.Normal;
            var device = TJAPlayer3.app.Device;
            switch (pChip.nチャンネル番号)
            {
                case 0x11:
                    if (TJAPlayer3.app.Tx.Notes is not null && pChip.bShow)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(130, num9, 130, 130));
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                    }
                    break;

                case 0x12:
                    if (TJAPlayer3.app.Tx.Notes is not null && pChip.bShow)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(260, num9, 130, 130));
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                    }
                    break;

                case 0x13:
                    if (TJAPlayer3.app.Tx.Notes is not null && pChip.bShow)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(390, num9, 130, 130));

                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                    }
                    break;
                case 0x14:
                    if (TJAPlayer3.app.Tx.Notes is not null && pChip.bShow)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(520, num9, 130, 130));
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                    }
                    break;

                case 0x1A:
                    if (TJAPlayer3.app.Tx.Notes is not null)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            if (TJAPlayer3.app.Tx.Notes_Arm is not null)
                            {
                                if (nPlayer == 0)
                                {
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 25, (y + 74) + nHand, CTexture.EFlipType.Vertical);
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 60, (y + 104) - nHand, CTexture.EFlipType.Vertical);
                                }
                                else if (nPlayer == 1)
                                {
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 25, (y - 44) + nHand);
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 60, (y - 14) - nHand);
                                }
                            }
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(1690, num9, 130, 130));
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 390, 136, 30));
                    }
                    break;

                case 0x1B:
                    if (TJAPlayer3.app.Tx.Notes is not null)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            if (TJAPlayer3.app.Tx.Notes_Arm is not null)
                            {
                                if (nPlayer == 0)
                                {
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 25, (y + 74) + nHand, CTexture.EFlipType.Vertical);
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 60, (y + 104) - nHand, CTexture.EFlipType.Vertical);
                                }
                                else if (nPlayer == 1)
                                {
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 25, (y - 44) + nHand);
                                    TJAPlayer3.app.Tx.Notes_Arm.t2D描画(device, x + 60, (y - 14) - nHand);
                                }
                            }
                            TJAPlayer3.app.Tx.Notes.t2D描画(device, x, y, new Rectangle(1820, num9, 130, 130));
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                            TJAPlayer3.app.Tx.SENotes.t2D描画(device, x - 2, y + nSenotesY, new Rectangle(0, 420, 136, 30));
                    }
                    break;

                case 0x1F:
                    break;
            }
        }
        #endregion
    }
    protected void t進行描画_チップ_Taiko連打(ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer)
    {
        if (pChip.nチャンネル番号 < 0x15 || pChip.nチャンネル番号 > 0x17 || CSoundManager.rc演奏用タイマ is null)
            return;

        int nSenotesY = TJAPlayer3.app.Skin.SkinConfig.Game.SENotesOffsetY[nPlayer];
        int nノート座標 = 0;
        int nノート末端座標 = 0;

        #region[ 作り直したもの ]
        if (pChip.b可視 && TJAPlayer3.app.Tx.Notes is not null)
        {
            if (pChip.nノーツ出現時刻ms != 0 && ((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ出現時刻ms))
                pChip.bShow = false;
            else if (pChip.nノーツ出現時刻ms != 0 && pChip.nノーツ移動開始時刻ms != 0)
                pChip.bShow = true;

            if (pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms))
            {
                nノート座標 = (int)((((pChip.n発声時刻ms) - (pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms)) * pChip.dbBPM * pChip.dbSCROLL * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer]) / 502.8594 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
                nノート末端座標 = (int)(((pChip.cEndChip.n発声時刻ms - (pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms)) * pChip.cEndChip.dbBPM * pChip.cEndChip.dbSCROLL * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer]) / 502.8594 / 5.0);// 2020.04.18 Mr-Ojii rhimm様のコードを参考にばいそくの計算の修正
            }
            else
            {
                nノート座標 = 0;
                nノート末端座標 = 0;
            }
            //2020.05.06 Mr-Ojii ここらへんから349って書いてあったところを、TJAPlayer3.app.Skin.nScrollFieldX[nPlayer] - 55に置き換えた。
            int x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.nバーからの距離dot - 55;
            int x末端 = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.cEndChip.nバーからの距離dot - 55;
            int y = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[nPlayer];

            if (pChip.nノーツ移動開始時刻ms != 0 && ((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.n発声時刻ms - pChip.nノーツ移動開始時刻ms))
            {
                x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + nノート座標;
                x末端 = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + nノート末端座標;
            }
            else
            {
                x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.nバーからの距離dot - 55;
                x末端 = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.cEndChip.nバーからの距離dot - 55;
            }

            x -= 10;
            x末端 -= 10;

            if (1400 > Math.Min(x, x末端))
            {
                int num9 = this.n顔座標[nPlayer];

                //kairera0467氏 の TJAPlayer2forPC のコードを参考にし、打数に応じて色を変える(打数の変更以外はほとんどそのまんま) ろみゅ～？ 2018/8/20
                pChip.RollInputTime?.t進行();
                pChip.RollDelay?.t進行();

                if (pChip.RollInputTime is not null && pChip.RollInputTime.b終了値に達した)
                {
                    pChip.RollInputTime.t停止();
                    pChip.RollInputTime.n現在の値 = 0;
                    pChip.RollDelay = new CCounter(0, 1, 1, TJAPlayer3.app.Timer);
                }

                if (pChip.RollDelay is not null && pChip.RollDelay.b終了値に達した && pChip.RollEffectLevel > 0)
                {
                    pChip.RollEffectLevel--;
                    pChip.RollDelay = new CCounter(0, 1, 1, TJAPlayer3.app.Timer);
                    pChip.RollDelay.n現在の値 = 0;
                }

                int f減少するカラー = (int)(255 - ((242.0 / 100.0) * pChip.RollEffectLevel));
                Color effectedColor = Color.FromArgb(255, 255, f減少するカラー, f減少するカラー);
                Color normalColor = Color.FromArgb(255, 255, 255, 255);
                float f末端ノーツのテクスチャ位置調整 = 65f;

                if (pChip.nチャンネル番号 == 0x15 && pChip.bShow) //連打(小)
                {
                    int index = Math.Abs(x末端 - x); //連打の距離
                    if (x末端 > x)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            #region[末端をテクスチャ側でつなげる場合の方式]
                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode != CSkin.ERollColorMode.None)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;
                            TJAPlayer3.app.Tx.Notes.vcScaling.X = (index - 65.0f + f末端ノーツのテクスチャ位置調整 + 1) / 128.0f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x + 64, y, new Rectangle(781, 0, 128, 130));

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = 1.0f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + f末端ノーツのテクスチャ位置調整, y, new Rectangle(910, num9, 130, 130));

                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode == CSkin.ERollColorMode.All)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(650, num9, 130, 130));
                            TJAPlayer3.app.Tx.Notes.color = normalColor;
                            #endregion
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                        {
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = index - 44;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 90, y + nSenotesY, new Rectangle(60, 240, 1, 30));
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 30, y + nSenotesY, new Rectangle(0, 240, 60, 30));
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 46, y + nSenotesY, new Rectangle(58, 270, 78, 30));
                        }
                    }
                    else //マイナス
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            #region[末端をテクスチャ側でつなげる場合の方式]
                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode != CSkin.ERollColorMode.None)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;
                            TJAPlayer3.app.Tx.Notes.vcScaling.X = (index - 65.0f + f末端ノーツのテクスチャ位置調整 + 1) / 128.0f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + 64, y, new Rectangle(781, 0, 128, 130), CTexture.EFlipType.Horizontal);

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = 1.0f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 - 130 + f末端ノーツのテクスチャ位置調整, y, new Rectangle(910, num9, 130, 130), CTexture.EFlipType.Horizontal);

                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode == CSkin.ERollColorMode.All)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(650, num9, 130, 130), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.Notes.color = normalColor;
                            #endregion
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                        {
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = index - 44;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 90, y + nSenotesY, new Rectangle(60, 240, 1, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 30, y + nSenotesY, new Rectangle(0, 240, 60, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 46 - 30, y + nSenotesY, new Rectangle(58, 270, 78, 30), CTexture.EFlipType.Horizontal);
                        }
                    }
                }
                if (pChip.nチャンネル番号 == 0x16 && pChip.bShow)
                {
                    int index = Math.Abs(x末端 - x); //連打の距離
                    if (x末端 > x)
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            #region[末端をテクスチャ側でつなげる場合の方式]
                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode != CSkin.ERollColorMode.None)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = (index - 65 + f末端ノーツのテクスチャ位置調整 + 1) / 128f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x + 64, y, new Rectangle(1171, 0, 128, 130));

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + f末端ノーツのテクスチャ位置調整, y, new Rectangle(1300, num9, 130, 130));

                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode == CSkin.ERollColorMode.All)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(1040, num9, 130, 130));
                            TJAPlayer3.app.Tx.Notes.color = normalColor;
                            #endregion
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                        {
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = index - 44;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 90, y + nSenotesY, new Rectangle(60, 240, 1, 30));
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 30, y + nSenotesY, new Rectangle(0, 240, 60, 30));
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 46, y + nSenotesY, new Rectangle(58, 270, 78, 30));
                        }
                    }
                    else //マイナス
                    {
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        {
                            #region[末端をテクスチャ側でつなげる場合の方式]
                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode != CSkin.ERollColorMode.None)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = (index - 65 + f末端ノーツのテクスチャ位置調整 + 1) / 128f;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 + 64, y, new Rectangle(1171, 0, 128, 130), CTexture.EFlipType.Horizontal);

                            TJAPlayer3.app.Tx.Notes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x末端 - 130 + f末端ノーツのテクスチャ位置調整, y, new Rectangle(1300, num9, 130, 130), CTexture.EFlipType.Horizontal);

                            if (TJAPlayer3.app.Skin.SkinConfig.Game._RollColorMode == CSkin.ERollColorMode.All)
                                TJAPlayer3.app.Tx.Notes.color = effectedColor;
                            else
                                TJAPlayer3.app.Tx.Notes.color = normalColor;

                            TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(1040, num9, 130, 130), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.Notes.color = normalColor;
                            #endregion
                        }
                        if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                        {
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = index - 44;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 90, y + nSenotesY, new Rectangle(60, 240, 1, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.vcScaling.X = 1.0f;
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x + 30, y + nSenotesY, new Rectangle(0, 240, 60, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30), CTexture.EFlipType.Horizontal);
                            TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x末端 + 46 - 30, y + nSenotesY, new Rectangle(58, 270, 78, 30), CTexture.EFlipType.Horizontal);
                        }
                    }
                }
                if (pChip.nチャンネル番号 == 0x17 && pChip.bShow)
                {
                    if ((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) >= pChip.n発声時刻ms && (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) < pChip.cEndChip.n発声時刻ms)
                        x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - 55;
                    else if ((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) >= pChip.cEndChip.n発声時刻ms)
                        x = (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.cEndChip.nバーからの距離dot - 55);

                    if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] == EStealthMode.OFF)
                        TJAPlayer3.app.Tx.Notes.t2D描画(TJAPlayer3.app.Device, x, y, new Rectangle(1430, num9, 260, 130));

                    if (TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] != EStealthMode.STEALTH && TJAPlayer3.app.Tx.SENotes is not null)
                        TJAPlayer3.app.Tx.SENotes.t2D描画(TJAPlayer3.app.Device, x - 2, y + nSenotesY, new Rectangle(0, 30 * pChip.nSenote, 136, 30));
                }
            }
        }
        if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[nPlayer] && pChip.n発声時刻ms < (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) && pChip.cEndChip.n発声時刻ms > (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
            this.tチップのヒット処理(pChip.n発声時刻ms, pChip, false, 0, nPlayer);
        #endregion
    }

    protected void t進行描画_チップ_小節線(ref CDTX dTX, ref CDTX.CChip pChip, int nPlayer)
    {
        if (pChip.nコース != this.n現在のコース[nPlayer] || CSoundManager.rc演奏用タイマ is null)
            return;

        int x = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + pChip.nバーからの距離dot;
        int y = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[nPlayer];

        if (pChip.dbSCROLL_Y != 0.0)
        {
            y += (int)(((pChip.n発声時刻ms - (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))) * pChip.dbBPM * pChip.dbSCROLL_Y * this.actScrollSpeed.db現在の譜面スクロール速度[nPlayer]) / 502.8594 / 5.0);
        }

        if (TJAPlayer3.app.ConfigToml.Game.ShowDebugStatus || TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
        {
            if (x >= 310)
            {
                TJAPlayer3.app.act文字コンソール.tPrint(x + 8, y - 26, C文字コンソール.EFontType.白, pChip.n整数値_内部番号.ToString());
            }
        }
        if ((pChip.b可視) && (TJAPlayer3.app.Tx.Bar is not null) && (TJAPlayer3.app.Tx.Bar_Branch is not null))
        {
            if (x >= 0)
            {
                if (pChip.bBranch)
                {
                    TJAPlayer3.app.Tx.Bar_Branch.fRotation = pChip.dbSCROLL != 0 ? (float)-Math.Atan((pChip.dbSCROLL_Y / pChip.dbSCROLL)) : (float)(Math.PI / 2.0);
                    TJAPlayer3.app.Tx.Bar_Branch.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, x - 1.5f, y + 65f, new Rectangle(0, 0, 3, 130));
                }
                else
                {
                    TJAPlayer3.app.Tx.Bar.fRotation = pChip.dbSCROLL != 0 ? (float)-Math.Atan((pChip.dbSCROLL_Y / pChip.dbSCROLL)) : (float)(Math.PI / 2.0);
                    TJAPlayer3.app.Tx.Bar.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, x - 1.5f, y + 65f, new Rectangle(0, 0, 3, 130));
                }
            }
        }
    }

    /// <summary>
    /// 全体にわたる制御をする。
    /// </summary>
    protected void t全体制御メソッド()
    {
        if (CSoundManager.rc演奏用タイマ is null)
            return;

        int time = (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
        //CDTXMania.act文字コンソール.tPrint( 0, 16, C文字コンソール.EFontType.白, t.ToString() );

        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (this.chip現在処理中の連打チップ[i] is not null)
            {
                if (this.chip現在処理中の連打チップ[i].nチャンネル番号 == 0x17 && this.b連打中[i] == true)
                {
                    //if (this.chip現在処理中の連打チップ.n発声時刻ms <= (int)CSoundManager.rc演奏用タイマ.n現在時刻ms && this.chip現在処理中の連打チップ.nノーツ終了時刻ms >= (int)CSoundManager.rc演奏用タイマ.n現在時刻ms)
                    if (this.chip現在処理中の連打チップ[i].n発声時刻ms <= time && this.chip現在処理中の連打チップ[i].cEndChip.n発声時刻ms + 500 >= time)
                    {
                        this.chip現在処理中の連打チップ[i].bShow = false;
                        this.actBalloon.On進行描画(this.chip現在処理中の連打チップ[i].nBalloon, this.n風船残り[i], i);
                    }
                    else
                    {
                        this.n現在の連打数[i] = 0;
                    }
                }
            }
        }

        #region[ 片手判定をこっちに持ってきてみる。]
        //常時イベントが発生しているメソッドのほうがいいんじゃないかという予想。
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            CDTX.CChip? chipNoHit = GetChipOfNearest((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), i);

            if (chipNoHit is not null && (chipNoHit.nチャンネル番号 == 0x13 || chipNoHit.nチャンネル番号 == 0x14 || chipNoHit.nチャンネル番号 == 0x1A || chipNoHit.nチャンネル番号 == 0x1B))
            {
                float timeC = chipNoHit.n発声時刻ms - (float)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                int nWaitTime = TJAPlayer3.app.ConfigToml.PlayOption.BigNotesWaitTime;
                if ((chipNoHit.eNoteState == ENoteState.waitleft || chipNoHit.eNoteState == ENoteState.waitright) && timeC <= 110 && chipNoHit.nProcessTime + nWaitTime <= (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
                {
                    this.tドラムヒット処理(chipNoHit.nProcessTime, EPad.RRed, chipNoHit, false, i);
                    chipNoHit.eNoteState = ENoteState.none;
                    chipNoHit.bHit = true;
                    chipNoHit.IsHitted = true;
                }
            }
        }
        #endregion

    }
    protected void t進行描画_チップアニメ()
    {
        for (int i = 0; i < 2; i++)
        {
            ctChipAnime[i].t進行LoopDb();
            ctChipAnimeLag[i].t進行();

            if (TJAPlayer3.app.Skin.SkinConfig.Game.NotesAnime)
            {
                if (this.actCombo.n現在のコンボ数[i] >= 300 && ctChipAnimeLag[i].b終了値に達した)
                {
                    if ((int)ctChipAnime[i].db現在の値 == 1 || (int)ctChipAnime[i].db現在の値 == 3)
                        this.n顔座標[i] = 260;
                    else
                        this.n顔座標[i] = 0;
                }
                else if (this.actCombo.n現在のコンボ数[i] >= 300 && !ctChipAnimeLag[i].b終了値に達した)
                {
                    if ((int)ctChipAnime[i].db現在の値 == 1 || (int)ctChipAnime[i].db現在の値 == 3)
                        this.n顔座標[i] = 130;
                    else
                        this.n顔座標[i] = 0;
                }
                else if (this.actCombo.n現在のコンボ数[i] >= 150)
                {
                    if ((int)ctChipAnime[i].db現在の値 == 1 || (int)ctChipAnime[i].db現在の値 == 3)
                        this.n顔座標[i] = 130;
                    else
                        this.n顔座標[i] = 0;

                }
                else if (this.actCombo.n現在のコンボ数[i] >= 50 && ctChipAnimeLag[i].b終了値に達した)
                {
                    if ((int)ctChipAnime[i].db現在の値 <= 1)
                        this.n顔座標[i] = 130;
                    else
                        this.n顔座標[i] = 0;
                }
                else if (this.actCombo.n現在のコンボ数[i] >= 50 && !ctChipAnimeLag[i].b終了値に達した)
                    this.n顔座標[i] = 0;
                else
                    this.n顔座標[i] = 0;
            }
            else
                this.n顔座標[i] = 0;
        }
    }

    protected bool t進行描画_FadeIn_アウト()
    {
        switch (base.eフェーズID)
        {
            case CStage.Eフェーズ.共通_FadeIn:
                if (this.actFI.On進行描画() != 0)
                {
                    base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
                }
                break;

            case CStage.Eフェーズ.共通_FadeOut:
            case CStage.Eフェーズ.演奏_STAGE_FAILED_FadeOut:
                if (this.actFO.On進行描画() != 0)
                {
                    return true;
                }
                break;

            case CStage.Eフェーズ.演奏_STAGE_CLEAR_FadeOut:
                if (this.actFOClear.On進行描画() == 0)
                {
                    break;
                }
                return true;

        }
        return false;
    }

    protected void t背景テクスチャの生成()
    {
        try
        {
            if (!String.IsNullOrEmpty(TJAPlayer3.DTX[0].strBGIMAGE_PATH))
                this.tx背景 = TJAPlayer3.app.tCreateTexture(TJAPlayer3.stage選曲.r確定されたスコア.FileInfo.DirAbsolutePath + TJAPlayer3.DTX[0].strBGIMAGE_PATH);
            else
                this.tx背景 = TJAPlayer3.app.tCreateTexture(CSkin.Path(@"Graphics/5_Game/5_Background/0/Background.png"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
            Trace.TraceError("An exception has occurred, but processing continues.");
            this.tx背景 = null;
        }
    }

    public void ReSetScore(int scoreInit, int scoreDiff, int nPlayer)
    {
        //一打目の処理落ちがひどいので、あらかじめここで点数の計算をしておく。
        // -1だった場合、その前を引き継ぐ。
        int nInit = scoreInit != -1 ? scoreInit : this.nScore[nPlayer, 0];
        int nDiff = scoreDiff != -1 ? scoreDiff : this.nScore[nPlayer, 1] - this.nScore[nPlayer, 0];
        int[] n倍率 = { 0, 1, 2, 4, 8 };

        if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 1)
        {
            for (int i = 0; i < 11; i++)
            {
                this.nScore[nPlayer, i] = (int)(nInit + (nDiff * (i)));
            }
        }
        else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 2)
        {
            for (int i = 0; i < 5; i++)
            {
                this.nScore[nPlayer, i] = (int)(nInit + (nDiff * n倍率[i]));

                this.nScore[nPlayer, i] = (int)(this.nScore[nPlayer, i] / 10.0);
                this.nScore[nPlayer, i] = this.nScore[nPlayer, i] * 10;

            }
        }
        else if (TJAPlayer3.DTX[nPlayer].nScoreModeTmp == 3)
            this.nScore[nPlayer, 0] = nInit;
    }

    protected void t進行描画_リアルタイム判定数表示()
    {
        if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1 && TJAPlayer3.app.ConfigToml.Game.ShowJudgeCountDisplay || TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.特訓モード)
        {
            //ボードの横幅は333px
            //数字フォントの小さいほうはリザルトのものと同じ。
            if (TJAPlayer3.app.Tx.Judge_Meter is not null)
                TJAPlayer3.app.Tx.Judge_Meter.t2D描画(TJAPlayer3.app.Device, 0, 360);

            this.t小文字表示(102, 494, string.Format("{0,4:###0}", this.nヒット数[0].Perfect.ToString()));
            this.t小文字表示(102, 532, string.Format("{0,4:###0}", this.nヒット数[0].Good.ToString()));
            this.t小文字表示(102, 570, string.Format("{0,4:###0}", (this.nヒット数[0].Miss + this.nヒット数[0].Bad).ToString()));
            this.t小文字表示(102, 634, string.Format("{0,4:###0}", GetRoll(0)));

            int nNowTotal = this.nヒット数[0].Perfect + this.nヒット数[0].Good + this.nヒット数[0].Bad + this.nヒット数[0].Miss;
            double dbたたけた率 = Math.Round((100.0 * (this.nヒット数[0].Perfect + this.nヒット数[0].Good)) / (double)nNowTotal);
            double dbPERFECT率 = Math.Round((100.0 * this.nヒット数[0].Perfect) / (double)nNowTotal);
            double dbGOOD率 = Math.Round((100.0 * this.nヒット数[0].Good / (double)nNowTotal));
            double dbMISS率 = Math.Round((100.0 * (this.nヒット数[0].Miss + this.nヒット数[0].Bad) / (double)nNowTotal));

            if (double.IsNaN(dbたたけた率))
                dbたたけた率 = 0;
            if (double.IsNaN(dbPERFECT率))
                dbPERFECT率 = 0;
            if (double.IsNaN(dbGOOD率))
                dbGOOD率 = 0;
            if (double.IsNaN(dbMISS率))
                dbMISS率 = 0;

            this.t大文字表示(202, 436, string.Format("{0,3:##0}%", dbたたけた率));
            this.t小文字表示(206, 494, string.Format("{0,3:##0}%", dbPERFECT率));
            this.t小文字表示(206, 532, string.Format("{0,3:##0}%", dbGOOD率));
            this.t小文字表示(206, 570, string.Format("{0,3:##0}%", dbMISS率));
        }
    }

    private void t小文字表示(int x, int y, string str)
    {
        foreach (char ch in str)
        {
            if (this.st小文字位置.TryGetValue(ch, out var pt))
            {
                Rectangle rectangle = new Rectangle(pt.X, pt.Y, 32, 38);
                TJAPlayer3.app.Tx.Result_Number?.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
            }
            x += 22;
        }
    }

    private void t大文字表示(int x, int y, string str)
    {
        foreach (char ch in str)
        {
            if (this.st小文字位置.TryGetValue(ch, out var pt))
            {
                Rectangle rectangle = new Rectangle(pt.X, 38, 32, 42);
                TJAPlayer3.app.Tx.Result_Number?.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
            }
            x += 28;
        }
    }
    #endregion
}
