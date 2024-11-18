using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drums演奏終了演出 : CActivity
{
    /// <summary>
    /// 課題
    /// _クリア失敗 →素材不足(確保はできる。切り出しと加工をしてないだけ。)
    /// _
    /// </summary>
    public CAct演奏Drums演奏終了演出()
    {
    }

    public void Start()
    {
        this.ct進行メイン = new CCounter(0, 500, 22, TJAPlayer3.app.Timer);
        this.ct進行return用 = new CCounter(0, this.ct進行メイン.n終了値 - 100, 22, TJAPlayer3.app.Timer);
        this.bリザルトボイス再生済み = false;
        // モードの決定。クリア失敗・フルコンボも事前に作っとく。
        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan)
        {
            // 段位認定モード。
            if (!TJAPlayer3.stage演奏ドラム画面.actDan.GetFailedAllChallenges())
            {
                // 段位認定モード、クリア成功
                this.Mode[0] = EndMode.StageCleared;
                this.soundClear?.t再生を開始する();
            }
            else
            {
                // 段位認定モード、クリア失敗
                this.Mode[0] = EndMode.StageFailed;
                this.soundFailed?.t再生を開始する();
            }
        }
        else
        {
            // 通常のモード。
            // ここでフルコンボフラグをチェックするが現時点ではない。
            // 今の段階では魂ゲージ80%以上でチェック。
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                if (TJAPlayer3.stage演奏ドラム画面.actGauge.db現在のゲージ値[i] < 80)
                {
                    this.Mode[i] = EndMode.StageFailed;
                    if (i == 0)
                        this.soundFailed?.t再生を開始する();
                }
                else if (TJAPlayer3.stage演奏ドラム画面.nヒット数[i].Miss != 0 || TJAPlayer3.stage演奏ドラム画面.nヒット数[i].Bad != 0)
                {
                    this.Mode[i] = EndMode.StageCleared;
                    if (i == 0)
                        this.soundClear?.t再生を開始する();
                }
                else if (TJAPlayer3.stage演奏ドラム画面.nヒット数[i].Good != 0)
                {
                    this.Mode[i] = EndMode.StageFullCombo;
                    if (i == 0)
                        this.soundFullCombo?.t再生を開始する();
                }
                else
                {
                    this.Mode[i] = EndMode.StageDonderFullCombo;
                    if (i == 0)
                        this.soundDonderFullCombo?.t再生を開始する();
                }
            }
        }
    }

    public void Stop()
    {
        this.ct進行メイン = null;//nullにすれば、必然的に止まる。
    }

    public override void On活性化()
    {
        this.bリザルトボイス再生済み = false;
        this.Mode = new EndMode[2];

        this.soundFailed = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Failed.ogg"), ESoundGroup.SoundEffect);
        this.soundClear = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Clear.ogg"), ESoundGroup.SoundEffect);
        this.soundFullCombo = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Full Combo.ogg"), ESoundGroup.SoundEffect);
        this.soundDonderFullCombo = TJAPlayer3.SoundManager.tCreateSound(CSkin.Path(@"Sounds/Donder Full Combo.ogg"), ESoundGroup.SoundEffect);
        base.On活性化();
    }

    public override void On非活性化()
    {
        this.ct進行メイン = null;

        this.soundFailed?.t解放する();
        this.soundClear?.t解放する();
        this.soundFullCombo?.t解放する();
        this.soundDonderFullCombo?.t解放する();
        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (base.b初めての進行描画)
        {
            base.b初めての進行描画 = false;
        }
        if (this.ct進行メイン is not null && this.ct進行return用 is not null && (TJAPlayer3.stage演奏ドラム画面.eフェーズID == CStage.Eフェーズ.演奏_演奏終了演出 || TJAPlayer3.stage演奏ドラム画面.eフェーズID == CStage.Eフェーズ.演奏_STAGE_CLEAR_FadeOut))
        {
            this.ct進行メイン.t進行();
            this.ct進行return用.t進行();

            //CDTXMania.act文字コンソール.tPrint( 0, 0, C文字コンソール.EFontType.灰, this.ct進行メイン.n現在の値.ToString() );
            //仮置き
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                int[] y = new int[] { 210, 386 };
                float[] f文字拡大率 = new float[] { 1.04f, 1.11f, 1.15f, 1.19f, 1.23f, 1.26f, 1.30f, 1.31f, 1.32f, 1.32f, 1.32f, 1.30f, 1.30f, 1.26f, 1.25f, 1.19f, 1.15f, 1.11f, 1.05f, 1.0f };
                int[] n透明度 = new int[] { 43, 85, 128, 170, 213, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
                switch (this.Mode[i])
                {
                    case EndMode.StageFailed:
                        if (TJAPlayer3.app.Tx.End_Failed_Text is not null)
                        {
                            #region[ 文字 ]
                            #region[ Opacity ]
                            if (this.ct進行メイン.n現在の値 < 26)
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.Opacity = 0;
                            }
                            if (this.ct進行メイン.n現在の値 <= 36)
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.Opacity = (int)(((this.ct進行メイン.n現在の値 - 26) / 10.0) * 255.0);
                            }
                            else
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.Opacity = 255;
                            }
                            #endregion
                            #region[ Rotate ]
                            int ytxtdiff = 0;
                            if (this.ct進行メイン.n現在の値 < 116)
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.fRotation = 0f;
                            }
                            else if (this.ct進行メイン.n現在の値 <= 118)
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.fRotation = (float)-(((this.ct進行メイン.n現在の値 - 116) / 3.0 * 5.0 / 180.0) * Math.PI);
                                ytxtdiff = (this.ct進行メイン.n現在の値 - 116) * 2;
                            }
                            else
                            {
                                TJAPlayer3.app.Tx.End_Failed_Text.fRotation = (float)-(5.0 / 180.0 * Math.PI);
                                ytxtdiff = 10;
                            }

                            #endregion
                            TJAPlayer3.app.Tx.End_Failed_Text.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 810, y[i] + TJAPlayer3.app.Tx.End_FullCombo_Text.szTextureSize.Height + ytxtdiff);
                            #endregion
                            #region[ バチお ]
                            if (this.ct進行メイン.n現在の値 <= 11)
                            {
                                CTexture? end_failed_l1 = TJAPlayer3.app.Tx.End_Failed_L[1];
                                CTexture? end_failed_r1 = TJAPlayer3.app.Tx.End_Failed_L[1];
                                if (end_failed_l1 is not null)
                                {
                                    end_failed_l1.t2D描画(TJAPlayer3.app.Device, 697, y[i] - 30);
                                    end_failed_l1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                                if (end_failed_r1 is not null)
                                {
                                    end_failed_r1.t2D描画(TJAPlayer3.app.Device, 738, y[i] - 30);
                                    end_failed_r1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 25)
                            {
                                TJAPlayer3.app.Tx.End_Failed_L[0]?.t2D描画(TJAPlayer3.app.Device, 697 - (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                                TJAPlayer3.app.Tx.End_Failed_R[0]?.t2D描画(TJAPlayer3.app.Device, 738 + (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 35)
                            {
                                int ydiff = (int)(Math.Sin((this.ct進行メイン.n現在の値 - 25) / 20.0 * Math.PI) * 100.0);
                                CTexture? end_failed_l2 = TJAPlayer3.app.Tx.End_Failed_L[2];
                                CTexture? end_failed_r2 = TJAPlayer3.app.Tx.End_Failed_R[2];
                                if (end_failed_l2 is not null)
                                {
                                    end_failed_l2.fRotation = (float)(((this.ct進行メイン.n現在の値 - 25) / 20.0 * Math.PI / 2.0));
                                    end_failed_l2.t2D描画(TJAPlayer3.app.Device, 697 - (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - ydiff - 30);
                                }
                                if (end_failed_r2 is not null)
                                {
                                    end_failed_r2.fRotation = (float)-(((this.ct進行メイン.n現在の値 - 25) / 20.0 * Math.PI / 2.0));
                                    end_failed_r2.t2D描画(TJAPlayer3.app.Device, 738 + (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - ydiff - 30);
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 43)
                            {
                                int ydiff = (int)(Math.Sin((this.ct進行メイン.n現在の値 - 25) / 20.0 * Math.PI) * 100.0);
                                CTexture? end_failed_l2 = TJAPlayer3.app.Tx.End_Failed_L[2];
                                CTexture? end_failed_r2 = TJAPlayer3.app.Tx.End_Failed_R[2];
                                if (end_failed_l2 is not null)
                                {
                                    end_failed_l2.fRotation = (float)(Math.PI / 2.0);
                                    end_failed_l2.t2D描画(TJAPlayer3.app.Device, 467, y[i] - ydiff - 30);
                                }
                                if (end_failed_r2 is not null)
                                {
                                    end_failed_r2.fRotation = (float)-(Math.PI / 2.0);
                                    end_failed_r2.t2D描画(TJAPlayer3.app.Device, 968, y[i] - ydiff - 30);
                                }
                            }
                            else
                            {
                                TJAPlayer3.app.Tx.End_Failed_L[3]?.t2D描画(TJAPlayer3.app.Device, 467, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Failed_R[3]?.t2D描画(TJAPlayer3.app.Device, 968, y[i] - 30);
                            }
                            #endregion
                            #region[ Impact ]
                            if (this.ct進行メイン.n現在の値 >= 25 && this.ct進行メイン.n現在の値 < 30 && TJAPlayer3.app.Tx.End_Failed_Impact is not null)
                            {
                                TJAPlayer3.app.Tx.End_Failed_Impact.Opacity = (int)(Math.Sin((this.ct進行メイン.n現在の値 - 25) / 5.0 * Math.PI) * 255);
                                TJAPlayer3.app.Tx.End_Failed_Impact.t2D描画(TJAPlayer3.app.Device, 597, y[i] + 80);
                                TJAPlayer3.app.Tx.End_Failed_Impact.t2D描画(TJAPlayer3.app.Device, 958, y[i] + 80);
                            }
                            #endregion
                        }
                        break;
                    case EndMode.StageCleared:
                        if (TJAPlayer3.app.Tx.End_Clear_Text is not null && TJAPlayer3.app.Tx.End_Clear_Text_Effect is not null)
                        {
                            #region[ 文字 ]
                            //登場アニメは20フレーム。うち最初の5フレームは半透過状態。
                            if (this.ct進行メイン.n現在の値 >= 17)
                            {
                                if (this.ct進行メイン.n現在の値 <= 36)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 17]) - 90)), new Rectangle(0, 0, 90, 90));
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, y[i], new Rectangle(0, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 19)
                            {
                                if (this.ct進行メイン.n現在の値 <= 38)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 19]) - 90)), new Rectangle(90, 0, 90, 90));
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, y[i], new Rectangle(90, 0, 90, 90));
                                }
                            }
                            TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 >= 21)
                            {
                                if (this.ct進行メイン.n現在の値 <= 40)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 21]) - 90), new Rectangle(180, 0, 90, 90));
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i], new Rectangle(180, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 23)
                            {
                                if (this.ct進行メイン.n現在の値 <= 42)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 23]) - 90), new Rectangle(270, 0, 90, 90));
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i], new Rectangle(270, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 25)
                            {
                                if (this.ct進行メイン.n現在の値 <= 44)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, (y[i] + 2) - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 25]) - 90), new Rectangle(360, 0, 90, 90));
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, y[i] + 2, new Rectangle(360, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 50 && this.ct進行メイン.n現在の値 < 90)
                            {
                                if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = (this.ct進行メイン.n現在の値 - 50) * (255 / 20);
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = 255 - ((this.ct進行メイン.n現在の値 - 70) * (255 / 20));
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                            }
                            #endregion
                            #region[ バチお ]
                            if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 1.0f;
                            if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 <= 11)
                            {
                                CTexture? end_clear_l1 = TJAPlayer3.app.Tx.End_Clear_L[1];
                                CTexture? end_clear_r1 = TJAPlayer3.app.Tx.End_Clear_R[1];
                                if (end_clear_l1 is not null)
                                {
                                    end_clear_l1.t2D描画(TJAPlayer3.app.Device, 697, y[i] - 30);
                                    end_clear_l1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                                if (end_clear_r1 is not null)
                                {
                                    end_clear_r1.t2D描画(TJAPlayer3.app.Device, 738, y[i] - 30);
                                    end_clear_r1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 35)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[0]?.t2D描画(TJAPlayer3.app.Device, 697 - (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[0]?.t2D描画(TJAPlayer3.app.Device, 738 + (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 46)
                            {
                                CTexture? end_clear_l0 = TJAPlayer3.app.Tx.End_Clear_L[0];
                                CTexture? end_clear_r0 = TJAPlayer3.app.Tx.End_Clear_R[0];
                                //2016.07.16 kairera0467 またも原始的...
                                float[] fRet = new float[] { 1.0f, 0.99f, 0.98f, 0.97f, 0.96f, 0.95f, 0.96f, 0.97f, 0.98f, 0.99f, 1.0f };
                                if (end_clear_l0 is not null)
                                {
                                    end_clear_l0.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                    end_clear_l0.vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                }
                                if (end_clear_r0 is not null)
                                {
                                    end_clear_r0.t2D描画(TJAPlayer3.app.Device, 1136 - 180 * fRet[this.ct進行メイン.n現在の値 - 36], y[i] - 30);
                                    end_clear_r0.vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 49)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[1]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[1]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 54)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[2]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[2]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 58)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[3]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[3]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[4]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[4]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            #endregion
                        }
                        break;
                    case EndMode.StageFullCombo:
                        if (TJAPlayer3.app.Tx.End_Clear_Text is not null && TJAPlayer3.app.Tx.End_Clear_Text_Effect is not null && TJAPlayer3.app.Tx.End_FullCombo_Text is not null && TJAPlayer3.app.Tx.End_FullCombo_Text_Effect is not null)
                        {
                            #region[ 文字 ]
                            if (this.ct進行メイン.n現在の値 >= 17)
                            {
                                if (this.ct進行メイン.n現在の値 <= 36)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 17]) - 90)), new Rectangle(0, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, y[i], new Rectangle(0, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 19)
                            {
                                if (this.ct進行メイン.n現在の値 <= 38)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 19]) - 90)), new Rectangle(90, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, y[i], new Rectangle(90, 0, 90, 90));
                                }
                            }
                            TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 >= 21)
                            {
                                if (this.ct進行メイン.n現在の値 <= 40)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 21]) - 90), new Rectangle(180, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i], new Rectangle(180, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 23)
                            {
                                if (this.ct進行メイン.n現在の値 <= 42)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 23]) - 90), new Rectangle(270, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i], new Rectangle(270, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 25)
                            {
                                if (this.ct進行メイン.n現在の値 <= 44)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, (y[i] + 2) - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 25]) - 90), new Rectangle(360, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, y[i] + 2, new Rectangle(360, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 50 && this.ct進行メイン.n現在の値 < 90)
                            {
                                if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = (this.ct進行メイン.n現在の値 - 50) * (255 / 20);
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = 255 - ((this.ct進行メイン.n現在の値 - 70) * (255 / 20));
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                            }
                            int ydiff = 0;
                            TJAPlayer3.app.Tx.End_FullCombo_Text.vcScaling.Y = 1f;
                            TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.vcScaling.Y = 1f;
                            if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 90)
                            {
                                double ratio = Math.Sin(((this.ct進行メイン.n現在の値 - 70) / 10.0) * Math.PI);
                                if (ratio > 0)
                                {
                                    ydiff = (int)(ratio * 10.0);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_FullCombo_Text.vcScaling.Y = 0.8f + (float)(ratio + 1.0) * 0.2f;
                                    TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.vcScaling.Y = 0.8f + (float)(ratio + 1.0) * 0.2f;
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 70)
                            {
                                if (this.ct進行メイン.n現在の値 < 80)
                                {
                                    TJAPlayer3.app.Tx.End_FullCombo_Text.Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 10);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_FullCombo_Text.Opacity = 255;
                                }
                                TJAPlayer3.app.Tx.End_FullCombo_Text.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 810, y[i] - ydiff + TJAPlayer3.app.Tx.End_FullCombo_Text.szTextureSize.Height);
                            }
                            if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 90)
                            {
                                if (this.ct進行メイン.n現在の値 < 80)
                                {
                                    TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 10);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.Opacity = 255 - ((this.ct進行メイン.n現在の値 - 80) * (255 / 10));
                                }
                                TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 810, y[i] - ydiff + TJAPlayer3.app.Tx.End_FullCombo_Text_Effect.szTextureSize.Height);
                            }

                            #endregion
                            const int leftfan = 356;
                            const int rightfan = 956;
                            #region[ 扇2 ]
                            //レイヤー変更用に扇の個所を2箇所に分ける
                            if (this.ct進行メイン.n現在の値 >= 79 && TJAPlayer3.app.Tx.End_Fan[3] is not null)
                            {
                                int x補正値, y補正値;
                                if ((this.ct進行メイン.n現在の値 / 2) % 2 == 0)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[3].vcScaling.Y = 1f;
                                    x補正値 = 0;
                                    y補正値 = 0;
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Fan[3].vcScaling.Y = 0.99f;
                                    x補正値 = 1;
                                    y補正値 = 1;
                                }
                                TJAPlayer3.app.Tx.End_Fan[3].fRotation = -20f * (float)Math.PI / 180f;
                                TJAPlayer3.app.Tx.End_Fan[3].t2D描画(TJAPlayer3.app.Device, leftfan - x補正値, y[i] - 15 + y補正値);
                                TJAPlayer3.app.Tx.End_Fan[3].fRotation = 20f * (float)Math.PI / 180f;
                                TJAPlayer3.app.Tx.End_Fan[3].t2D描画(TJAPlayer3.app.Device, rightfan + x補正値, y[i] - 15 + y補正値);
                            }
                            #endregion
                            #region[ バチお ]
                            if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 1.0f;
                            if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 <= 11)
                            {
                                if (TJAPlayer3.app.Tx.End_Clear_L[1] is not null)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_L[1].t2D描画(TJAPlayer3.app.Device, 697, y[i] - 30);
                                    TJAPlayer3.app.Tx.End_Clear_L[1].Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                                if (TJAPlayer3.app.Tx.End_Clear_R[1] is not null)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_R[1].t2D描画(TJAPlayer3.app.Device, 738, y[i] - 30);
                                    TJAPlayer3.app.Tx.End_Clear_R[1].Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 35)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[0]?.t2D描画(TJAPlayer3.app.Device, 697 - (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[0]?.t2D描画(TJAPlayer3.app.Device, 738 + (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 46)
                            {
                                if (TJAPlayer3.app.Tx.End_Clear_L[0] is not null)
                                {
                                    //2016.07.16 kairera0467 またも原始的...
                                    float[] fRet = new float[] { 1.0f, 0.99f, 0.98f, 0.97f, 0.96f, 0.95f, 0.96f, 0.97f, 0.98f, 0.99f, 1.0f };
                                    TJAPlayer3.app.Tx.End_Clear_L[0].t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                    TJAPlayer3.app.Tx.End_Clear_L[0].vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                    TJAPlayer3.app.Tx.End_Clear_R[0].t2D描画(TJAPlayer3.app.Device, 1136 - 180 * fRet[this.ct進行メイン.n現在の値 - 36], y[i] - 30);
                                    TJAPlayer3.app.Tx.End_Clear_R[0].vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 49)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[1]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[1]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 54)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[2]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[2]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 58)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[3]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[3]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 68)
                            {
                                if (this.ct進行メイン.n現在の値 >= 58)
                                {
                                    float xratio = (float)Math.Abs(Math.Cos(((this.ct進行メイン.n現在の値 - 58) / 10.0) * Math.PI));
                                    if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                    if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                }

                                TJAPlayer3.app.Tx.End_Clear_L[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 466, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 956, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 88)
                            {
                                int ysin = (int)(Math.Sin((this.ct進行メイン.n現在の値 - 68) / 20.0 * Math.PI) * 150.0);
                                TJAPlayer3.app.Tx.End_Clear_L[4]?.t2D描画(TJAPlayer3.app.Device, 466 - ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[4]?.t2D描画(TJAPlayer3.app.Device, 956 + ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                            }
                            else
                            {
                                if (this.ct進行メイン.n現在の値 <= 98)
                                {
                                    float xratio = (float)Math.Abs(Math.Cos(((this.ct進行メイン.n現在の値 - 89) / 10.0) * Math.PI));
                                    if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                    if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                }

                                TJAPlayer3.app.Tx.End_Clear_L[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 306, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 1116, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                            }
                            #endregion
                            #region[ 扇1 ]
                            if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 79 && TJAPlayer3.app.Tx.End_Fan is not null)
                            {
                                int num = 0;
                                if (this.ct進行メイン.n現在の値 < 73)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[0].Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 3);
                                    num = 0;
                                }
                                else if (this.ct進行メイン.n現在の値 < 76)
                                {
                                    num = 1;
                                }
                                else if (this.ct進行メイン.n現在の値 < 79)
                                {
                                    num = 2;
                                }
                                if (TJAPlayer3.app.Tx.End_Fan[num] is not null)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[num].fRotation = -20f * (float)Math.PI / 180f;
                                    TJAPlayer3.app.Tx.End_Fan[num].t2D描画(TJAPlayer3.app.Device, leftfan, y[i] - 15);
                                    TJAPlayer3.app.Tx.End_Fan[num].fRotation = 20f * (float)Math.PI / 180f;
                                    TJAPlayer3.app.Tx.End_Fan[num].t2D描画(TJAPlayer3.app.Device, rightfan, y[i] - 15);
                                }
                            }
                            #endregion
                        }
                        break;
                    case EndMode.StageDonderFullCombo:
                        if (TJAPlayer3.app.Tx.End_Clear_Text is not null && TJAPlayer3.app.Tx.End_Clear_Text_Effect is not null && TJAPlayer3.app.Tx.End_FullCombo_Text is not null && TJAPlayer3.app.Tx.End_FullCombo_Text_Effect is not null)
                        {
                            #region[ BG ]
                            if (this.ct進行メイン.n現在の値 >= 70 && TJAPlayer3.app.Tx.End_DonderFullCombo_Lane is not null)
                            {
                                if (this.ct進行メイン.n現在の値 < 80)
                                {
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_Lane.Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 10);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_Lane.Opacity = 255;
                                }
                                TJAPlayer3.app.Tx.End_DonderFullCombo_Lane.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                            #endregion
                            #region[ 文字 ]
                            if (this.ct進行メイン.n現在の値 >= 17)
                            {
                                if (this.ct進行メイン.n現在の値 <= 36)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 17];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 17]) - 90)), new Rectangle(0, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 634, y[i], new Rectangle(0, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 19)
                            {
                                if (this.ct進行メイン.n現在の値 <= 38)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 19];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, (int)(y[i] - ((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 19]) - 90)), new Rectangle(90, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 692, y[i], new Rectangle(90, 0, 90, 90));
                                }
                            }
                            TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 >= 21)
                            {
                                if (this.ct進行メイン.n現在の値 <= 40)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 21];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 21]) - 90), new Rectangle(180, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 750, y[i], new Rectangle(180, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 23)
                            {
                                if (this.ct進行メイン.n現在の値 <= 42)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 23];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i] - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 23]) - 90), new Rectangle(270, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 819, y[i], new Rectangle(270, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 25)
                            {
                                if (this.ct進行メイン.n現在の値 <= 44)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = f文字拡大率[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.Opacity = n透明度[this.ct進行メイン.n現在の値 - 25];
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, (y[i] + 2) - (int)((90 * f文字拡大率[this.ct進行メイン.n現在の値 - 25]) - 90), new Rectangle(360, 0, 90, 90));
                                }
                                else if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text.vcScaling.Y = 1.0f;
                                    TJAPlayer3.app.Tx.End_Clear_Text.t2D描画(TJAPlayer3.app.Device, 890, y[i] + 2, new Rectangle(360, 0, 90, 90));
                                }
                            }
                            if (this.ct進行メイン.n現在の値 >= 50 && this.ct進行メイン.n現在の値 < 90)
                            {
                                if (this.ct進行メイン.n現在の値 < 70)
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = (this.ct進行メイン.n現在の値 - 50) * (255 / 20);
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.Opacity = 255 - ((this.ct進行メイン.n現在の値 - 70) * (255 / 20));
                                    TJAPlayer3.app.Tx.End_Clear_Text_Effect.t2D描画(TJAPlayer3.app.Device, 634, y[i] - 2);
                                }
                            }

                            #endregion
                            const int leftfan = 356;
                            const int rightfan = 956;
                            #region[ 扇2 ]
                            //レイヤー変更用に扇の個所を2箇所に分ける
                            if (this.ct進行メイン.n現在の値 >= 79 && TJAPlayer3.app.Tx.End_Fan[3] is not null)
                            {
                                int x補正値, y補正値;
                                if ((this.ct進行メイン.n現在の値 / 2) % 2 == 0)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[3].vcScaling.Y = 1f;
                                    x補正値 = 0;
                                    y補正値 = 0;
                                }
                                else
                                {
                                    TJAPlayer3.app.Tx.End_Fan[3].vcScaling.Y = 0.99f;
                                    x補正値 = 1;
                                    y補正値 = 1;
                                }
                                TJAPlayer3.app.Tx.End_Fan[3].fRotation = -20f * (float)Math.PI / 180f;
                                TJAPlayer3.app.Tx.End_Fan[3].t2D描画(TJAPlayer3.app.Device, leftfan - x補正値, y[i] - 15 + y補正値);
                                TJAPlayer3.app.Tx.End_Fan[3].fRotation = 20f * (float)Math.PI / 180f;
                                TJAPlayer3.app.Tx.End_Fan[3].t2D描画(TJAPlayer3.app.Device, rightfan + x補正値, y[i] - 15 + y補正値);
                            }
                            #endregion
                            #region[ バチお ]
                            if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 1.0f;
                            if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 1.0f;
                            if (this.ct進行メイン.n現在の値 <= 11)
                            {
                                CTexture? end_clear_l1 = TJAPlayer3.app.Tx.End_Clear_L[1];
                                CTexture? end_clear_r1 = TJAPlayer3.app.Tx.End_Clear_R[1];
                                if (end_clear_l1 is not null)
                                {
                                    end_clear_l1.t2D描画(TJAPlayer3.app.Device, 697, y[i] - 30);
                                    end_clear_l1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                                if (end_clear_r1 is not null)
                                {
                                    end_clear_r1.t2D描画(TJAPlayer3.app.Device, 738, y[i] - 30);
                                    end_clear_r1.Opacity = (int)(11.0 / this.ct進行メイン.n現在の値) * 255;
                                }
                            }
                            else if (this.ct進行メイン.n現在の値 <= 35)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[0]?.t2D描画(TJAPlayer3.app.Device, 697 - (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[0]?.t2D描画(TJAPlayer3.app.Device, 738 + (int)((this.ct進行メイン.n現在の値 - 12) * 10), y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 46)
                            {
                                CTexture? end_clear_l0 = TJAPlayer3.app.Tx.End_Clear_L[0];
                                CTexture? end_clear_r0 = TJAPlayer3.app.Tx.End_Clear_R[0];
                                //2016.07.16 kairera0467 またも原始的...
                                float[] fRet = new float[] { 1.0f, 0.99f, 0.98f, 0.97f, 0.96f, 0.95f, 0.96f, 0.97f, 0.98f, 0.99f, 1.0f };
                                if (end_clear_l0 is not null)
                                {
                                    end_clear_l0.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                    end_clear_l0.vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                }
                                if (end_clear_r0 is not null)
                                {
                                    end_clear_r0.t2D描画(TJAPlayer3.app.Device, 1136 - 180 * fRet[this.ct進行メイン.n現在の値 - 36], y[i] - 30);
                                    end_clear_r0.vcScaling = new Vector2(fRet[this.ct進行メイン.n現在の値 - 36], 1.0f);
                                }

                            }
                            else if (this.ct進行メイン.n現在の値 <= 49)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[1]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[1]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 54)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[2]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[2]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 58)
                            {
                                TJAPlayer3.app.Tx.End_Clear_L[3]?.t2D描画(TJAPlayer3.app.Device, 466, y[i] - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[3]?.t2D描画(TJAPlayer3.app.Device, 956, y[i] - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 68)
                            {
                                if (this.ct進行メイン.n現在の値 >= 58)
                                {
                                    float xratio = (float)Math.Abs(Math.Cos(((this.ct進行メイン.n現在の値 - 58) / 10.0) * Math.PI));
                                    if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_L[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                    if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_R[4].vcScaling.Y = 0.8f + xratio * 0.2f;
                                }
                                TJAPlayer3.app.Tx.End_Clear_L[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 466, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                                TJAPlayer3.app.Tx.End_Clear_R[4]?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 956, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                            }
                            else if (this.ct進行メイン.n現在の値 <= 88)
                            {
                                int ysin = (int)(Math.Sin((this.ct進行メイン.n現在の値 - 68) / 20.0 * Math.PI) * 150.0);
                                if (this.ct進行メイン.n現在の値 <= 78)
                                {
                                    if (TJAPlayer3.app.Tx.End_Clear_L[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_L[4].t2D描画(TJAPlayer3.app.Device, 466 - ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                                    if (TJAPlayer3.app.Tx.End_Clear_R[4] is not null)
                                        TJAPlayer3.app.Tx.End_Clear_R[4].t2D描画(TJAPlayer3.app.Device, 956 + ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                                }
                                else
                                {
                                    if (TJAPlayer3.app.Tx.End_DonderFullCombo_L is not null)
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_L.t2D描画(TJAPlayer3.app.Device, 466 - ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                                    if (TJAPlayer3.app.Tx.End_DonderFullCombo_R is not null)
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_R.t2D描画(TJAPlayer3.app.Device, 956 + ((this.ct進行メイン.n現在の値 - 68) * 8), y[i] - ysin - 30);
                                }
                            }
                            else
                            {
                                if (this.ct進行メイン.n現在の値 <= 98)
                                {
                                    float xratio = (float)Math.Abs(Math.Cos(((this.ct進行メイン.n現在の値 - 89) / 10.0) * Math.PI));
                                    if (TJAPlayer3.app.Tx.End_DonderFullCombo_L is not null)
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_L.vcScaling.Y = 0.8f + xratio * 0.2f;
                                    if (TJAPlayer3.app.Tx.End_DonderFullCombo_R is not null)
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_R.vcScaling.Y = 0.8f + xratio * 0.2f;
                                }
                                if (TJAPlayer3.app.Tx.End_DonderFullCombo_L is not null)
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_L.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 306, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                                if (TJAPlayer3.app.Tx.End_DonderFullCombo_R is not null)
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_R.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.DownLeft, 1116, y[i] + TJAPlayer3.app.Tx.End_Clear_L[4].szTextureSize.Height - 30);
                            }
                            #endregion
                            #region[ 扇1 ]
                            if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 79 && TJAPlayer3.app.Tx.End_Fan is not null)
                            {
                                int num = 0;
                                if (this.ct進行メイン.n現在の値 < 73)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[0].Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 3);
                                    num = 0;
                                }
                                else if (this.ct進行メイン.n現在の値 < 76)
                                {
                                    num = 1;
                                }
                                else if (this.ct進行メイン.n現在の値 < 79)
                                {
                                    num = 2;
                                }
                                if (TJAPlayer3.app.Tx.End_Fan[num] is not null)
                                {
                                    TJAPlayer3.app.Tx.End_Fan[num].fRotation = -20f * (float)Math.PI / 180f;
                                    TJAPlayer3.app.Tx.End_Fan[num].t2D描画(TJAPlayer3.app.Device, leftfan, y[i] - 15);
                                    TJAPlayer3.app.Tx.End_Fan[num].fRotation = 20f * (float)Math.PI / 180f;
                                    TJAPlayer3.app.Tx.End_Fan[num].t2D描画(TJAPlayer3.app.Device, rightfan, y[i] - 15);
                                }
                            }
                            #endregion
                            #region[ ドンダフル文字 ]
                            if (TJAPlayer3.app.Tx.End_DonderFullCombo_Text is not null && TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect is not null)
                            {
                                int ydiff = 0;
                                TJAPlayer3.app.Tx.End_DonderFullCombo_Text.vcScaling.Y = 1f;
                                TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.vcScaling.Y = 1f;
                                if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 90)
                                {
                                    double ratio = Math.Sin(((this.ct進行メイン.n現在の値 - 70) / 10.0) * Math.PI);
                                    if (ratio > 0)
                                    {
                                        ydiff = (int)(ratio * 10.0);
                                    }
                                    else
                                    {
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text.vcScaling.Y = 0.8f + (float)(ratio + 1.0) * 0.2f;
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.vcScaling.Y = 0.8f + (float)(ratio + 1.0) * 0.2f;
                                    }
                                }
                                if (this.ct進行メイン.n現在の値 >= 70)
                                {
                                    if (this.ct進行メイン.n現在の値 < 80)
                                    {
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text.Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 10);
                                    }
                                    else
                                    {
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text.Opacity = 255;
                                    }
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_Text.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 810, y[i] - ydiff + TJAPlayer3.app.Tx.End_DonderFullCombo_Text.szTextureSize.Height);
                                }
                                if (this.ct進行メイン.n現在の値 >= 70 && this.ct進行メイン.n現在の値 < 90)
                                {
                                    if (this.ct進行メイン.n現在の値 < 80)
                                    {
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.Opacity = (this.ct進行メイン.n現在の値 - 70) * (255 / 10);
                                    }
                                    else
                                    {
                                        TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.Opacity = 255 - ((this.ct進行メイン.n現在の値 - 80) * (255 / 10));
                                    }
                                    TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 810, y[i] - ydiff + TJAPlayer3.app.Tx.End_DonderFullCombo_Text_Effect.szTextureSize.Height);
                                }
                            }
                            #endregion
                        }
                        break;
                    default:
                        break;
                }

            }

            if (this.ct進行return用.b終了値に達した)
            {
                if (!this.bリザルトボイス再生済み)
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND成績発表].t再生する();
                    this.bリザルトボイス再生済み = true;
                }
                return 1;
            }
        }

        return 0;
    }

    #region[ private ]
    //-----------------
    bool bリザルトボイス再生済み;
    CCounter ct進行メイン;
    CCounter ct進行return用;
    CSound? soundFailed;
    CSound? soundClear;
    CSound? soundFullCombo;
    CSound? soundDonderFullCombo;
    EndMode[] Mode;
    enum EndMode
    {
        StageFailed,
        StageCleared,
        StageFullCombo,
        StageDonderFullCombo
    }
    //-----------------
    #endregion
}
