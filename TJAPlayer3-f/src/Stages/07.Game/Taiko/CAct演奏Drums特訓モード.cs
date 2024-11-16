using FDK;

namespace TJAPlayer3;

class CAct演奏Drums特訓モード : CActivity
{
    public CAct演奏Drums特訓モード()
    {
    }

    public override void On活性化()
    {
        this.n現在の小節線 = 0;
        this.b特訓PAUSE = false;
        this.n最終演奏位置ms = 0;

        base.On活性化();

        if (TJAPlayer3.app.Tx.Tokkun_Background_Up is not null)
            this.ct背景スクロールタイマー = new CCounter(1, TJAPlayer3.app.Tx.Tokkun_Background_Up.szTextureSize.Width, 16, TJAPlayer3.app.Timer);

        CDTX dTX = TJAPlayer3.DTX[0];

        var measureCount = 1;
        var bIsInGoGo = false;

        int endtime = 1;
        int bgmlength = 1;

        for (int index = 0; index < TJAPlayer3.DTX[0].listChip.Count; index++)
        {
            if (TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 == 0xff)
            {
                endtime = TJAPlayer3.DTX[0].listChip[index].n発声時刻ms;
                break;
            }
        }
        for (int index = 0; index < TJAPlayer3.DTX[0].listChip.Count; index++)
        {
            if (TJAPlayer3.DTX[0].listChip[index].nチャンネル番号 == 0x01)
            {
                bgmlength = TJAPlayer3.DTX[0].listChip[index].GetDuration() + TJAPlayer3.DTX[0].listChip[index].n発声時刻ms;
                break;
            }
        }

        length = Math.Max(endtime, bgmlength);

        gogoXList.Clear();
        JumpPointList.Clear();

        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            CDTX.CChip pChip = dTX.listChip[i];

            if (pChip.n整数値_内部番号 > measureCount && pChip.nチャンネル番号 == 0x50) measureCount = pChip.n整数値_内部番号;

            if (pChip.nチャンネル番号 == 0x9E && !bIsInGoGo)
            {
                bIsInGoGo = true;

                var current = ((double)(pChip.db発声時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)));
                var width = 0;
                if (TJAPlayer3.app.Tx.Tokkun_ProgressBar is not null) width = TJAPlayer3.app.Tx.Tokkun_ProgressBar.szTextureSize.Width;

                this.gogoXList.Add((int)(width * (current / length)));
            }
            if (pChip.nチャンネル番号 == 0x9F && bIsInGoGo)
            {
                bIsInGoGo = false;
            }
        }

        this.n小節の総数 = measureCount;
    }

    public override void On非活性化()
    {
        length = 1;
        gogoXList.Clear();
        JumpPointList.Clear();

        this.ctスクロールカウンター = null;
        this.ct背景スクロールタイマー = null;
        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                base.b初めての進行描画 = false;
            }

            TJAPlayer3.app.act文字コンソール.tPrint(0, 0, C文字コンソール.EFontType.白, "TRAINING MODE (BETA)");

            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Space))
            {
                if (this.b特訓PAUSE)
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓再生].t再生する();
                    this.t演奏を再開する();
                }
                else
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓停止].t再生する();
                    this.t演奏を停止する();
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow) || TJAPlayer3.app.Pad.bPressed(EPad.LBlue))
            {
                if (this.b特訓PAUSE)
                {
                    if (this.n現在の小節線 > 1)
                    {
                        this.n現在の小節線--;
                        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                        this.t譜面の表示位置を合わせる(true);
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                    }
                    if (t配列の値interval以下か(ref this.LBlue, CSoundManager.rc演奏用タイマ.nシステム時刻ms, TJAPlayer3.app.ConfigToml.PlayOption.TrainingJumpInterval))
                    {
                        for (int index = this.JumpPointList.Count - 1; index >= 0; index--)
                        {
                            if (this.JumpPointList[index].Time <= CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))
                            {
                                this.n現在の小節線 = this.JumpPointList[index].Measure;
                                TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;
                                this.t譜面の表示位置を合わせる(false);
                                break;
                            }
                        }
                    }
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow) || TJAPlayer3.app.Pad.bPressed(EPad.RBlue))
            {
                if (this.b特訓PAUSE)
                {
                    if (this.n現在の小節線 < this.n小節の総数)
                    {
                        this.n現在の小節線++;
                        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                        this.t譜面の表示位置を合わせる(true);
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                    }
                    if (t配列の値interval以下か(ref this.RBlue, CSoundManager.rc演奏用タイマ.nシステム時刻ms, TJAPlayer3.app.ConfigToml.PlayOption.TrainingJumpInterval))
                    {
                        for (int index = 0; index < this.JumpPointList.Count; index++)
                        {
                            if (this.JumpPointList[index].Time >= CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))
                            {
                                this.n現在の小節線 = this.JumpPointList[index].Measure;
                                TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;
                                this.t譜面の表示位置を合わせる(false);
                                break;
                            }
                        }
                    }

                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.PageDown))
            {
                if (this.b特訓PAUSE)
                {
                    this.n現在の小節線 -= TJAPlayer3.app.ConfigToml.PlayOption.TrainingSkipMeasures;
                    this.n現在の小節線 = Math.Clamp(this.n現在の小節線, 1, this.n小節の総数);

                    TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                    this.t譜面の表示位置を合わせる(true);
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.PageUp))
            {
                if (this.b特訓PAUSE)
                {
                    this.n現在の小節線 += TJAPlayer3.app.ConfigToml.PlayOption.TrainingSkipMeasures;
                    this.n現在の小節線 = Math.Clamp(this.n現在の小節線, 1, this.n小節の総数);

                    TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                    this.t譜面の表示位置を合わせる(true);
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                }
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue2P))
            {
                if (this.b特訓PAUSE)
                {
                    if (TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed > 6)
                    {
                        TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed = TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed - 2;
                        this.t譜面の表示位置を合わせる(false);
                    }
                }
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue2P))
            {
                if (this.b特訓PAUSE)
                {
                    if (TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed < 399)
                    {
                        TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed = TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed + 2;
                        this.t譜面の表示位置を合わせる(false);
                    }
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Home))
            {
                if (this.b特訓PAUSE)
                {
                    if (this.n現在の小節線 > 1)
                    {
                        this.n現在の小節線 = 1;
                        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                        this.t譜面の表示位置を合わせる(true);
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                    }
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.End))
            {
                if (this.b特訓PAUSE)
                {
                    if (this.n現在の小節線 < this.n小節の総数)
                    {
                        this.n現在の小節線 = this.n小節の総数;
                        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;

                        this.t譜面の表示位置を合わせる(true);
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND特訓スクロール].t再生する();
                    }
                }
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.A))
                this.t現在の位置にジャンプポイントを設定する();

            if (this.bスクロール中 && this.ctスクロールカウンター is not null)
            {
                CSoundManager.rc演奏用タイマ.n現在時刻ms = EasingCircular(this.ctスクロールカウンター.n現在の値, (int)this.nスクロール前ms, (int)this.nスクロール後ms - (int)this.nスクロール前ms, this.ctスクロールカウンター.n終了値);

                this.ctスクロールカウンター.t進行();

                if ((int)CSoundManager.rc演奏用タイマ.n現在時刻ms == (int)this.nスクロール後ms)
                {
                    this.bスクロール中 = false;
                    CSoundManager.rc演奏用タイマ.n現在時刻ms = this.nスクロール後ms;
                }
            }
            if (!this.b特訓PAUSE)
            {
                if (this.n現在の小節線 < TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0])
                {
                    this.n現在の小節線 = TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0];
                }

                if (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0) > this.n最終演奏位置ms)
                {
                    this.n最終演奏位置ms = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                }
            }

        }

        var current = (double)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
        var percentage = current / length;

        var currentWhite = (double)(this.n最終演奏位置ms);
        var percentageWhite = currentWhite / length;

        if (TJAPlayer3.app.Tx.Tokkun_ProgressBarWhite is not null) TJAPlayer3.app.Tx.Tokkun_ProgressBarWhite.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[1], new Rectangle(1, 1, (int)(TJAPlayer3.app.Tx.Tokkun_ProgressBarWhite.szTextureSize.Width * percentageWhite), TJAPlayer3.app.Tx.Tokkun_ProgressBarWhite.szTextureSize.Height));
        if (TJAPlayer3.app.Tx.Tokkun_ProgressBar is not null) TJAPlayer3.app.Tx.Tokkun_ProgressBar.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[1], new Rectangle(1, 1, (int)(TJAPlayer3.app.Tx.Tokkun_ProgressBar.szTextureSize.Width * percentage), TJAPlayer3.app.Tx.Tokkun_ProgressBar.szTextureSize.Height));
        if (TJAPlayer3.app.Tx.Tokkun_GoGoPoint is not null)
        {
            foreach (int xpos in gogoXList)
            {
                TJAPlayer3.app.Tx.Tokkun_GoGoPoint.t2D描画(TJAPlayer3.app.Device, xpos + TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[0] - (TJAPlayer3.app.Tx.Tokkun_GoGoPoint.szTextureSize.Width / 2), TJAPlayer3.app.Skin.SkinConfig.Game.Training.GoGoPointY);
            }
        }

        if (TJAPlayer3.app.Tx.Tokkun_JumpPoint is not null)
        {
            foreach (STJUMPP xpos in JumpPointList)
            {
                var width = 0;
                if (TJAPlayer3.app.Tx.Tokkun_ProgressBar is not null) width = TJAPlayer3.app.Tx.Tokkun_ProgressBar.szTextureSize.Width;

                int x = (int)((double)width * ((double)xpos.Time / (double)length));
                TJAPlayer3.app.Tx.Tokkun_JumpPoint.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.app.Skin.SkinConfig.Game.Training.ProgressBarXY[0] - (TJAPlayer3.app.Tx.Tokkun_JumpPoint.szTextureSize.Width / 2), TJAPlayer3.app.Skin.SkinConfig.Game.Training.JumpPointY);
            }
        }

        return base.On進行描画();
    }

    public int On進行描画_背景()
    {
        if (this.ct背景スクロールタイマー is not null)
        {
            this.ct背景スクロールタイマー.t進行Loop();

            if (TJAPlayer3.app.Tx.Tokkun_Background_Up is not null)
            {
                double TexSize = TJAPlayer3.app.LogicalSize.Width / TJAPlayer3.app.Tx.Tokkun_Background_Up.szTextureSize.Width;
                // LogicalWidthをテクスチャサイズで割ったものを切り上げて、プラス+1足す。
                int ForLoop = (int)Math.Ceiling(TexSize) + 1;
                TJAPlayer3.app.Tx.Tokkun_Background_Up.t2D描画(TJAPlayer3.app.Device, 0 - this.ct背景スクロールタイマー.n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[0]);
                for (int l = 1; l < ForLoop + 1; l++)
                {
                    TJAPlayer3.app.Tx.Tokkun_Background_Up.t2D描画(TJAPlayer3.app.Device, +(l * TJAPlayer3.app.Tx.Tokkun_Background_Up.szTextureSize.Width) - this.ct背景スクロールタイマー.n現在の値, TJAPlayer3.app.Skin.SkinConfig.Game.Background.ScrollY[0]);
                }
            }
        }

        if (TJAPlayer3.app.Tx.Tokkun_DownBG is not null) TJAPlayer3.app.Tx.Tokkun_DownBG.t2D描画(TJAPlayer3.app.Device, 0, 360);
        if (TJAPlayer3.app.Tx.Tokkun_BigTaiko is not null) TJAPlayer3.app.Tx.Tokkun_BigTaiko.t2D描画(TJAPlayer3.app.Device, 334, 400);

        return base.On進行描画();
    }

    public void On進行描画_小節_速度()
    {
        if (TJAPlayer3.app.Tx.Tokkun_Speed_Measure is not null)
            TJAPlayer3.app.Tx.Tokkun_Speed_Measure.t2D描画(TJAPlayer3.app.Device, 0, 360);
        var maxMeasureStr = this.n小節の総数.ToString();
        var measureStr = TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0].ToString();
        if (TJAPlayer3.app.Tx.Tokkun_SmallNumber is not null)
        {
            var x = TJAPlayer3.app.Skin.SkinConfig.Game.Training.MaxMeasureCountXY[0];
            foreach (char c in maxMeasureStr)
            {
                var currentNum = int.Parse(c.ToString());
                TJAPlayer3.app.Tx.Tokkun_SmallNumber.t2D描画(TJAPlayer3.app.Device, x, TJAPlayer3.app.Skin.SkinConfig.Game.Training.MaxMeasureCountXY[1], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Training.SmallNumberWidth * currentNum, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Training.SmallNumberWidth, TJAPlayer3.app.Tx.Tokkun_SmallNumber.szTextureSize.Height));
                x += TJAPlayer3.app.Skin.SkinConfig.Game.Training.SmallNumberWidth - 2;
            }
        }

        var subtractVal = (TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth - 2) * (measureStr.Length - 1);

        if (TJAPlayer3.app.Tx.Tokkun_BigNumber is not null)
        {
            var x = TJAPlayer3.app.Skin.SkinConfig.Game.Training.CurrentMeasureCountXY[0];
            foreach (char c in measureStr)
            {
                var currentNum = int.Parse(c.ToString());
                TJAPlayer3.app.Tx.Tokkun_BigNumber.t2D描画(TJAPlayer3.app.Device, x - subtractVal, TJAPlayer3.app.Skin.SkinConfig.Game.Training.CurrentMeasureCountXY[1], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth * currentNum, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth, TJAPlayer3.app.Tx.Tokkun_BigNumber.szTextureSize.Height));
                x += TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth - 2;
            }

            var PlaySpdtmp = TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed / 20.0d * 10.0d;
            PlaySpdtmp = Math.Round(PlaySpdtmp, MidpointRounding.AwayFromZero);

            var playSpd = PlaySpdtmp / 10.0d;
            var playSpdI = playSpd - (int)playSpd;
            var playSpdStr = Decimal.Round((decimal)playSpdI, 1, MidpointRounding.AwayFromZero).ToString();
            var decimalStr = (playSpdStr == "0") ? "0" : playSpdStr[2].ToString();

            TJAPlayer3.app.Tx.Tokkun_BigNumber.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.Training.SpeedDisplayXY[0], TJAPlayer3.app.Skin.SkinConfig.Game.Training.SpeedDisplayXY[1], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth * int.Parse(decimalStr), 0, TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth, TJAPlayer3.app.Tx.Tokkun_BigNumber.szTextureSize.Height));

            x = TJAPlayer3.app.Skin.SkinConfig.Game.Training.SpeedDisplayXY[0] - 25;

            subtractVal = TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth * (((int)playSpd).ToString().Length - 1);

            foreach (char c in ((int)playSpd).ToString())
            {
                var currentNum = int.Parse(c.ToString());
                TJAPlayer3.app.Tx.Tokkun_BigNumber.t2D描画(TJAPlayer3.app.Device, x - subtractVal, TJAPlayer3.app.Skin.SkinConfig.Game.Training.SpeedDisplayXY[1], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth * currentNum, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth, TJAPlayer3.app.Tx.Tokkun_BigNumber.szTextureSize.Height));
                x += TJAPlayer3.app.Skin.SkinConfig.Game.Training.BigNumberWidth - 2;
            }
        }
    }

    public void t演奏を停止する()
    {
        CDTX dTX = TJAPlayer3.DTX[0];

        TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t判定枠Reset();

        this.nスクロール後ms = CSoundManager.rc演奏用タイマ.n現在時刻ms;

        TJAPlayer3.stage演奏ドラム画面.On活性化();
        CSoundManager.rc演奏用タイマ.t一時停止();

        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            CDTX.CChip pChip = dTX.listChip[i];
            pChip.bHit = false;
            if (dTX.listChip[i].nチャンネル番号 != 0x50)
            {
                pChip.bShow = true;
                pChip.b可視 = true;
            }
        }

        TJAPlayer3.DTX[0].t全チップの再生一時停止();
        TJAPlayer3.stage演奏ドラム画面.bPAUSE = true;
        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = this.n現在の小節線;
        this.b特訓PAUSE = true;

        this.t譜面の表示位置を合わせる(false);
    }

    public void t演奏を再開する()
    {
        CDTX dTX = TJAPlayer3.DTX[0];

        TJAPlayer3.stage演奏ドラム画面.actLaneTaiko.t判定枠Reset();

        this.bスクロール中 = false;
        CSoundManager.rc演奏用タイマ.n現在時刻ms = this.nスクロール後ms;

        int n演奏開始Chip = TJAPlayer3.stage演奏ドラム画面.n現在のトップChip;
        int finalStartBar;

        finalStartBar = this.n現在の小節線 - 2;
        if (finalStartBar < 0) finalStartBar = 0;

        TJAPlayer3.stage演奏ドラム画面.t演奏位置の変更(finalStartBar, 0);


        int n少し戻ってから演奏開始Chip = TJAPlayer3.stage演奏ドラム画面.n現在のトップChip;

        TJAPlayer3.stage演奏ドラム画面.actPlayInfo.NowMeasure[0] = 0;
        TJAPlayer3.stage演奏ドラム画面.t数値の初期化(true, true);
        TJAPlayer3.stage演奏ドラム画面.On活性化();

        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            if (i < n演奏開始Chip && (dTX.listChip[i].nチャンネル番号 > 0x10 && dTX.listChip[i].nチャンネル番号 < 0x20)) //2020.07.08 ノーツだけ消す。 null参照回避のために順番変更
            {
                dTX.listChip[i].bHit = true;
                dTX.listChip[i].IsHitted = true;
                dTX.listChip[i].b可視 = false;
                dTX.listChip[i].bShow = false;
            }
            if (i < n少し戻ってから演奏開始Chip && dTX.listChip[i].nチャンネル番号 == 0x01)
            {
                dTX.listChip[i].bHit = true;
                dTX.listChip[i].IsHitted = true;
                dTX.listChip[i].b可視 = false;
                dTX.listChip[i].bShow = false;
            }
            if (dTX.listChip[i].nチャンネル番号 == 0x50 && dTX.listChip[i].n整数値_内部番号 < finalStartBar)
            {
                dTX.listChip[i].bHit = true;
                dTX.listChip[i].IsHitted = true;
            }

        }

        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            TJAPlayer3.stage演奏ドラム画面.chip現在処理中の連打チップ[i] = null;
        }

        this.b特訓PAUSE = false;
    }

    public void t譜面の表示位置を合わせる(bool doScroll)
    {
        this.nスクロール前ms = CSoundManager.rc演奏用タイマ.n現在時刻ms;

        CDTX dTX = TJAPlayer3.DTX[0];

        bool bSuccessSeek = false;
        for (int i = 0; i < dTX.listChip.Count; i++)
        {
            CDTX.CChip pChip = dTX.listChip[i];

            if (pChip.nチャンネル番号 == 0x50 && pChip.n整数値_内部番号 > n現在の小節線 - 1)
            {
                bSuccessSeek = true;
                TJAPlayer3.stage演奏ドラム画面.n現在のトップChip = i;
                break;
            }
        }
        if (!bSuccessSeek)
        {
            TJAPlayer3.stage演奏ドラム画面.n現在のトップChip = 0;
        }
        else
        {
            while (dTX.listChip[TJAPlayer3.stage演奏ドラム画面.n現在のトップChip].n発声時刻ms == dTX.listChip[TJAPlayer3.stage演奏ドラム画面.n現在のトップChip - 1].n発声時刻ms && TJAPlayer3.stage演奏ドラム画面.n現在のトップChip != 0)
                TJAPlayer3.stage演奏ドラム画面.n現在のトップChip--;
        }

        if (doScroll)
        {
            this.nスクロール後ms = (long)(dTX.listChip[TJAPlayer3.stage演奏ドラム画面.n現在のトップChip].n発声時刻ms / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
            this.bスクロール中 = true;

            this.ctスクロールカウンター = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Training.ScrollTime, 1, TJAPlayer3.app.Timer);
        }
        else
        {
            CSoundManager.rc演奏用タイマ.n現在時刻ms = (long)(dTX.listChip[TJAPlayer3.stage演奏ドラム画面.n現在のトップChip].n発声時刻ms / (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
            this.nスクロール後ms = CSoundManager.rc演奏用タイマ.n現在時刻ms;
        }
    }

    public void t現在の位置にジャンプポイントを設定する()
    {
        if (!this.bスクロール中 && this.b特訓PAUSE)
        {
            if (!JumpPointList.Contains(new STJUMPP() { Time = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), Measure = this.n現在の小節線 }))
                JumpPointList.Add(new STJUMPP() { Time = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), Measure = this.n現在の小節線 });
            JumpPointList.Sort((a, b) => a.Time.CompareTo(b.Time));
        }
    }

    private bool t配列の値interval以下か(ref long[] array, long num, int interval)
    {
        long[] arraytmp = array;
        for (int index = 0; index < (array.Length - 1); index++)
        {
            array[index] = array[index + 1];
        }
        array[array.Length - 1] = num;
        return Math.Abs(num - arraytmp[0]) <= interval;
    }

    public int n現在の小節線;
    public int n小節の総数;

    #region [private]
    private long nスクロール前ms;
    private long nスクロール後ms;
    private long n最終演奏位置ms;

    private bool b特訓PAUSE;
    private bool bスクロール中;

    private CCounter? ctスクロールカウンター;
    private CCounter? ct背景スクロールタイマー;
    private long length = 1;

    private List<int> gogoXList = new();
    private List<STJUMPP> JumpPointList = new();
    private long[] LBlue = new long[] { 0, 0, 0, 0, 0 };
    private long[] RBlue = new long[] { 0, 0, 0, 0, 0 };

    private struct STJUMPP
    {
        public long Time;
        public int Measure;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="time">今の時間</param>
    /// <param name="begin">最初の値</param>
    /// <param name="change">最終の値-最初の値</param>
    /// <param name="duration">全体の時間</param>
    /// <returns></returns>
    private int EasingCircular(int time, int begin, int change, int duration)
    {
        double t = time, b = begin, c = change, d = duration;

        t = t / d * 2;
        if (t < 1)
            return (int)(-c / 2 * (Math.Sqrt(1 - t * t) - 1) + b);
        else
        {
            t = t - 2;
            return (int)(c / 2 * (Math.Sqrt(1 - t * t) + 1) + b);
        }
    }

    #endregion
}
