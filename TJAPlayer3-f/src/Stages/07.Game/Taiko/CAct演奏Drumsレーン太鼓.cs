using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drumsレーン太鼓 : CActivity
{
    /// <summary>
    /// レーンを描画するクラス。
    ///
    ///
    /// </summary>
    public CAct演奏Drumsレーン太鼓()
    {
    }

    public override void On活性化()
    {
        for (int i = 0; i < 4; i++)
        {
            this.st状態[i].ct進行 = new CCounter();
            this.stBranch[i].ct分岐アニメ進行 = new CCounter();
            this.stBranch[i].nフラッシュ制御タイマ = -1;
            this.stBranch[i].nBranchレイヤー透明度 = 0;
            this.stBranch[i].nBranch文字透明度 = 0;
            this.stBranch[i].nY座標 = 0;
        }
        this.ctゴーゴー = new CCounter();

        this.n総移動時間[0] = -1;
        this.n総移動時間[1] = -1;
        this.nDefaultJudgePos[0, 0] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[0];
        this.nDefaultJudgePos[0, 1] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0];
        this.nDefaultJudgePos[1, 0] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[1];
        this.nDefaultJudgePos[1, 1] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[1];
        this.ctゴーゴー炎 = new CCounter(0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Ptn, 50, TJAPlayer3.app.Timer);
        base.On活性化();
    }

    public override void On非活性化()
    {
        for (int i = 0; i < 4; i++)
        {
            this.st状態[i].ct進行 = null;
            this.stBranch[i].ct分岐アニメ進行 = null;
        }
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[0] = this.nDefaultJudgePos[0, 0];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0] = this.nDefaultJudgePos[0, 1];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[1] = this.nDefaultJudgePos[1, 0];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[1] = this.nDefaultJudgePos[1, 1];
        this.ctゴーゴー = null;

        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (base.b初めての進行描画)
        {
            for (int i = 0; i < 4; i++)
                this.stBranch[i].nフラッシュ制御タイマ = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
            base.b初めての進行描画 = false;
        }

        //それぞれが独立したレイヤーでないといけないのでforループはパーツごとに分離すること。

        #region[ レーン本体 ]
        if (TJAPlayer3.app.Tx.Lane_Background_Main is not null)
        {
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                TJAPlayer3.app.Tx.Lane_Background_Main.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
            }
        }
        #endregion
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            #region[ 分岐アニメ制御タイマー ]
            long num = CSoundManager.rc演奏用タイマ.n現在時刻ms;
            if (num < this.stBranch[i].nフラッシュ制御タイマ)
            {
                this.stBranch[i].nフラッシュ制御タイマ = num;
            }
            while ((num - this.stBranch[i].nフラッシュ制御タイマ) >= 30)
            {
                if (this.stBranch[i].nBranchレイヤー透明度 <= 255)
                {
                    this.stBranch[i].nBranchレイヤー透明度 += 10;
                }

                if (this.stBranch[i].nBranch文字透明度 >= 0)
                {
                    this.stBranch[i].nBranch文字透明度 -= 10;
                }

                if (this.stBranch[i].nY座標 != 0 && this.stBranch[i].nY座標 <= 20)
                {
                    this.stBranch[i].nY座標++;
                }

                this.stBranch[i].nフラッシュ制御タイマ += 8;
            }

            if (!this.stBranch[i].ct分岐アニメ進行.b停止中)
            {
                this.stBranch[i].ct分岐アニメ進行.t進行();
                if (this.stBranch[i].ct分岐アニメ進行.b終了値に達した)
                {
                    this.stBranch[i].ct分岐アニメ進行.t停止();
                }
            }
            #endregion
        }
        #region[ 分岐レイヤー ]
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.stage演奏ドラム画面.bUseBranch[i] == true)
            {
                #region[ 動いていない ]
                CTexture? base_tex = TJAPlayer3.app.Tx.Lane_Base[TJAPlayer3.stage演奏ドラム画面.n次回のコース[i]];
                if (base_tex is not null)
                {
                    base_tex.Opacity = 255;
                    base_tex.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                }
                #endregion

                if (TJAPlayer3.app.ConfigToml.Game.BranchAnime == 1)
                {
                    #region[ AC7～14風の背後レイヤー ]
                    if (this.stBranch[i].ct分岐アニメ進行.b進行中)
                    {
                        int n透明度 = ((100 - this.stBranch[i].ct分岐アニメ進行.n現在の値) * 0xff) / 100;

                        if (this.stBranch[i].ct分岐アニメ進行.b終了値に達した)
                        {
                            n透明度 = 255;
                            this.stBranch[i].ct分岐アニメ進行.t停止();
                        }
                        CTexture? lane_base_0 = TJAPlayer3.app.Tx.Lane_Base[0];
                        CTexture? lane_base_1 = TJAPlayer3.app.Tx.Lane_Base[1];
                        CTexture? lane_base_2 = TJAPlayer3.app.Tx.Lane_Base[2];

                        #region[ 普通譜面_レベルアップ ]
                        //普通→玄人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 1)
                        {
                            if (lane_base_0 is not null && lane_base_1 is not null)
                            {
                                lane_base_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_1.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                                lane_base_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        //普通→達人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 2)
                        {
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 100)
                            {
                                n透明度 = ((100 - this.stBranch[i].ct分岐アニメ進行.n現在の値) * 0xff) / 100;
                            }
                            if (lane_base_0 is not null && lane_base_2 is not null)
                            {
                                lane_base_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_2.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                            }
                        }
                        #endregion
                        #region[ 玄人譜面_レベルアップ ]
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 2)
                        {
                            if (lane_base_1 is not null && lane_base_2 is not null)
                            {
                                lane_base_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_2.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                            }
                        }
                        #endregion
                        #region[ 玄人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 0)
                        {
                            if (lane_base_1 is not null && lane_base_0 is not null)
                            {
                                lane_base_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_0.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                            }
                        }
                        #endregion
                        #region[ 達人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 2 && this.stBranch[i].nAfter == 0)
                        {
                            if (lane_base_2 is not null && lane_base_0 is not null)
                            {
                                lane_base_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_base_0.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (TJAPlayer3.app.ConfigToml.Game.BranchAnime == 0)
                {
                    TJAPlayer3.stage演奏ドラム画面.actLane.On進行描画();
                }
            }
        }
        #endregion
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            #region[ ゴーゴータイムレーン背景レイヤー ]
            if (TJAPlayer3.app.Tx.Lane_Background_GoGo is not null && TJAPlayer3.stage演奏ドラム画面.bIsGOGOTIME[i])
            {
                if (!this.ctゴーゴー.b停止中)
                {
                    this.ctゴーゴー.t進行();
                }

                if (this.ctゴーゴー.n現在の値 <= 4)
                {
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.vcScaling.Y = 0.2f;
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 54);
                }
                else if (this.ctゴーゴー.n現在の値 <= 5)
                {
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.vcScaling.Y = 0.4f;
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 40);
                }
                else if (this.ctゴーゴー.n現在の値 <= 6)
                {
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.vcScaling.Y = 0.6f;
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 26);
                }
                else if (this.ctゴーゴー.n現在の値 <= 8)
                {
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.vcScaling.Y = 0.8f;
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 13);
                }
                else if (this.ctゴーゴー.n現在の値 >= 9)
                {
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.vcScaling.Y = 1.0f;
                    TJAPlayer3.app.Tx.Lane_Background_GoGo.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldBGX[i], TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                }
            }
            #endregion
        }

        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.stage演奏ドラム画面.bUseBranch[i] == true)
            {
                if (TJAPlayer3.app.ConfigToml.Game.BranchAnime == 0)
                {
                    if (!this.stBranch[i].ct分岐アニメ進行.b進行中)
                    {
                        CTexture? lane_tex = TJAPlayer3.app.Tx.Lane_Text[TJAPlayer3.stage演奏ドラム画面.n次回のコース[i]];
                        if (lane_tex is not null)
                        {
                            lane_tex.Opacity = 255;
                            lane_tex.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                    }
                    CTexture? lane_text_0 = TJAPlayer3.app.Tx.Lane_Text[0];
                    CTexture? lane_text_1 = TJAPlayer3.app.Tx.Lane_Text[1];
                    CTexture? lane_text_2 = TJAPlayer3.app.Tx.Lane_Text[2];
                    if (this.stBranch[i].ct分岐アニメ進行.b進行中 && lane_text_0 is not null && lane_text_1 is not null && lane_text_2 is not null)
                    {
                        #region[ 普通譜面_レベルアップ ]
                        //普通→玄人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 1)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;

                            lane_text_0.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 60));
                            //CDTXMania.Tx.Lane_Text[1].n透明度 = this.ct分岐アニメ進行.n現在の値 > 100 ? 255 : ( ( ( this.ct分岐アニメ進行.n現在の値 * 0xff ) / 60 ) );
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY);
                                lane_text_1.Opacity = 255;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 30) + this.stBranch[i].nY);
                            }
                            else
                            {
                                lane_text_1.Opacity = 255;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }

                        }

                        //普通→達人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 2)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 12) + this.stBranch[i].nY);
                                lane_text_0.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 100));
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 20) + this.stBranch[i].nY);
                            }
                            //if( this.stBranch[ i ].ct分岐アニメ進行.n現在の値 >= 5 && this.stBranch[ i ].ct分岐アニメ進行.n現在の値 < 60 )
                            //{
                            //    this.stBranch[ i ].nY = this.stBranch[ i ].ct分岐アニメ進行.n現在の値 / 2;
                            //    this.tx普通譜面[ 1 ].t2D描画(CDTXMania.app.Device, 333, CDTXMania.Skin.nScrollFieldY[ i ] + this.stBranch[ i ].nY);
                            //    this.tx普通譜面[ 1 ].n透明度 = this.stBranch[ i ].ct分岐アニメ進行.n現在の値 > 100 ? 0 : ( 255 - ( ( this.stBranch[ i ].ct分岐アニメ進行.n現在の値 * 0xff) / 100));
                            //    this.tx玄人譜面[ 1 ].t2D描画(CDTXMania.app.Device, 333, ( CDTXMania.Skin.nScrollFieldY[ i ] - 10 ) + this.stBranch[ i ].nY);
                            //}
                            else if (this.stBranch[i].ct分岐アニメ進行.n現在の値 >= 60 && this.stBranch[i].ct分岐アニメ進行.n現在の値 < 150)
                            {
                                this.stBranch[i].nY = 21;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_text_1.Opacity = 255;
                                lane_text_2.Opacity = 255;
                            }
                            else if (this.stBranch[i].ct分岐アニメ進行.n現在の値 >= 150 && this.stBranch[i].ct分岐アニメ進行.n現在の値 < 210)
                            {
                                this.stBranch[i].nY = ((this.stBranch[i].ct分岐アニメ進行.n現在の値 - 150) / 2);
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY);
                                lane_text_1.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 100));
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 20) + this.stBranch[i].nY);
                            }
                            else
                            {
                                lane_text_2.Opacity = 255;
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        #endregion
                        #region[ 玄人譜面_レベルアップ ]
                        //玄人→達人
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 2)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;

                            lane_text_1.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 60));
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY);
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 20) + this.stBranch[i].nY);
                            }
                            else
                            {
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        #endregion
                        #region[ 玄人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 0)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;

                            lane_text_1.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 60));
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY);
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 30) - this.stBranch[i].nY);
                            }
                            else
                            {
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        #endregion
                        #region[ 達人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 2 && this.stBranch[i].nAfter == 0)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;

                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_2.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 60));
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY);
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 30) - this.stBranch[i].nY);
                            }
                            else if (this.stBranch[i].ct分岐アニメ進行.n現在の値 >= 60 && this.stBranch[i].ct分岐アニメ進行.n現在の値 < 150)
                            {
                                this.stBranch[i].nY = 21;
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                                lane_text_1.Opacity = 255;
                                lane_text_2.Opacity = 255;
                            }
                            else if (this.stBranch[i].ct分岐アニメ進行.n現在の値 >= 150 && this.stBranch[i].ct分岐アニメ進行.n現在の値 < 210)
                            {
                                this.stBranch[i].nY = ((this.stBranch[i].ct分岐アニメ進行.n現在の値 - 150) / 2);
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY);
                                lane_text_1.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 100));
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 30) - this.stBranch[i].nY);
                            }
                            else if (this.stBranch[i].ct分岐アニメ進行.n現在の値 >= 210)
                            {
                                lane_text_0.Opacity = 255;
                                lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        if (this.stBranch[i].nBefore == 2 && this.stBranch[i].nAfter == 1)
                        {
                            lane_text_0.Opacity = 255;
                            lane_text_1.Opacity = 255;
                            lane_text_2.Opacity = 255;

                            lane_text_2.Opacity = this.stBranch[i].ct分岐アニメ進行.n現在の値 > 100 ? 0 : (255 - ((this.stBranch[i].ct分岐アニメ進行.n現在の値 * 0xff) / 60));
                            if (this.stBranch[i].ct分岐アニメ進行.n現在の値 < 60)
                            {
                                this.stBranch[i].nY = this.stBranch[i].ct分岐アニメ進行.n現在の値 / 2;
                                lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY);
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 30) - this.stBranch[i].nY);
                            }
                            else
                            {
                                lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    if (this.stBranch[i].nY座標 == 21)
                    {
                        this.stBranch[i].nY座標 = 0;
                    }

                    if (this.stBranch[i].nY座標 == 0)
                    {
                        CTexture? lane_tex = TJAPlayer3.app.Tx.Lane_Text[TJAPlayer3.stage演奏ドラム画面.n次回のコース[i]];
                        if (lane_tex is not null)
                        {
                            lane_tex.Opacity = 255;
                            lane_tex.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i]);
                        }
                    }

                    CTexture? lane_text_0 = TJAPlayer3.app.Tx.Lane_Text[0];
                    CTexture? lane_text_1 = TJAPlayer3.app.Tx.Lane_Text[1];
                    CTexture? lane_text_2 = TJAPlayer3.app.Tx.Lane_Text[2];
                    if (this.stBranch[i].nY座標 != 0 && lane_text_0 is not null && lane_text_1 is not null && lane_text_2 is not null)
                    {
                        #region[ 普通譜面_レベルアップ ]
                        //普通→玄人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 1)
                        {
                            lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY座標);
                            lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 20) - this.stBranch[i].nY座標);
                            lane_text_0.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        //普通→達人
                        if (this.stBranch[i].nBefore == 0 && this.stBranch[i].nAfter == 2)
                        {
                            lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY座標);
                            lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 20) - this.stBranch[i].nY座標);
                            lane_text_0.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        #endregion
                        #region[ 玄人譜面_レベルアップ ]
                        //玄人→達人
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 2)
                        {
                            lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - this.stBranch[i].nY座標);
                            lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + 20) - this.stBranch[i].nY座標);
                            lane_text_1.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        #endregion
                        #region[ 玄人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 1 && this.stBranch[i].nAfter == 0)
                        {
                            lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY座標);
                            lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 24) + this.stBranch[i].nY座標);
                            lane_text_1.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        #endregion
                        #region[ 達人譜面_レベルダウン ]
                        if (this.stBranch[i].nBefore == 2 && this.stBranch[i].nAfter == 0)
                        {
                            lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY座標);
                            lane_text_0.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 24) + this.stBranch[i].nY座標);
                            lane_text_2.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        if (this.stBranch[i].nBefore == 2 && this.stBranch[i].nAfter == 1)
                        {
                            lane_text_2.t2D描画(TJAPlayer3.app.Device, 333, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] + this.stBranch[i].nY座標);
                            lane_text_1.t2D描画(TJAPlayer3.app.Device, 333, (TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i] - 24) + this.stBranch[i].nY座標);
                            lane_text_2.Opacity = this.stBranch[i].nBranchレイヤー透明度;
                        }
                        #endregion
                    }
                }

            }
        }

        if (TJAPlayer3.app.Tx.Lane_Background_Sub is not null)
        {
            int[] ypos = new int[] { 326, 502 };
            for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
            {
                TJAPlayer3.app.Tx.Lane_Background_Sub.t2D描画(TJAPlayer3.app.Device, 333, ypos[i]);
            }
        }

        TJAPlayer3.stage演奏ドラム画面.actTaikoLaneFlash.On進行描画();


        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            int[] ypos = new int[] { 136, 360 };
            TJAPlayer3.app.Tx.Taiko_Frame[i]?.t2D描画(TJAPlayer3.app.Device, 329, ypos[i]);
        }

        var nTime = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));


        for (int nPlayer = 0; nPlayer < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; nPlayer++)
        {
            if (this.n総移動時間[nPlayer] != -1)
            {
                TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] = this.n移動開始X[nPlayer] + (int)((((int)nTime - this.n移動開始時刻[nPlayer]) / (double)(this.n総移動時間[nPlayer])) * this.n移動距離px[nPlayer]);
                TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[nPlayer] = this.n移動開始X[nPlayer] + (int)((((int)nTime - this.n移動開始時刻[nPlayer]) / (double)(this.n総移動時間[nPlayer])) * this.n移動距離px[nPlayer]);

                if (((int)nTime) > this.n移動開始時刻[nPlayer] + this.n総移動時間[nPlayer])
                {
                    this.n総移動時間[nPlayer] = -1;
                    TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] = this.n移動目的場所X[nPlayer];
                    TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[nPlayer] = this.n移動目的場所X[nPlayer];
                }
            }
        }

        if (TJAPlayer3.app.ConfigToml.Game.Background.Movie && TJAPlayer3.DTX[0].listVD.Count > 0)
        {
            if (TJAPlayer3.app.Tx.Lane_Background_Main is not null) TJAPlayer3.app.Tx.Lane_Background_Main.Opacity = TJAPlayer3.app.ConfigToml.Game.Background.BGAlpha;
            if (TJAPlayer3.app.Tx.Lane_Background_Sub is not null) TJAPlayer3.app.Tx.Lane_Background_Sub.Opacity = TJAPlayer3.app.ConfigToml.Game.Background.BGAlpha;
            if (TJAPlayer3.app.Tx.Lane_Background_GoGo is not null) TJAPlayer3.app.Tx.Lane_Background_GoGo.Opacity = TJAPlayer3.app.ConfigToml.Game.Background.BGAlpha;
        }
        else
        {
            if (TJAPlayer3.app.Tx.Lane_Background_Main is not null) TJAPlayer3.app.Tx.Lane_Background_Main.Opacity = 255;
            if (TJAPlayer3.app.Tx.Lane_Background_Sub is not null) TJAPlayer3.app.Tx.Lane_Background_Sub.Opacity = 255;
            if (TJAPlayer3.app.Tx.Lane_Background_GoGo is not null) TJAPlayer3.app.Tx.Lane_Background_GoGo.Opacity = 255;
        }

        return base.On進行描画();
    }

    public void ゴーゴー炎()
    {
        //判定枠
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.app.Tx.Judge_Frame is not null)
            {
                int nJudgeX = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - 65; //元の値は349なんだけど...
                int nJudgeY = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[i];
                TJAPlayer3.app.Tx.Judge_Frame.eBlendMode = TJAPlayer3.app.Skin.SkinConfig.Game.JudgeFrameAddBlend ? CTexture.EBlendMode.Addition : CTexture.EBlendMode.Normal;
                TJAPlayer3.app.Tx.Judge_Frame.t2D描画(TJAPlayer3.app.Device, nJudgeX, nJudgeY, new Rectangle(0, 0, 130, 130));
            }
        }

        #region[ ゴーゴー炎 ]
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (TJAPlayer3.stage演奏ドラム画面.bIsGOGOTIME[i])
            {
                this.ctゴーゴー炎.t進行Loop();

                if (TJAPlayer3.app.Tx.Effects_Fire is not null)
                {
                    float f倍率 = 1.0f;

                    float[] ar倍率 = new float[] { 0.8f, 1.2f, 1.7f, 2.5f, 2.3f, 2.2f, 2.0f, 1.8f, 1.7f, 1.6f, 1.6f, 1.5f, 1.5f, 1.4f, 1.3f, 1.2f, 1.1f, 1.0f };

                    f倍率 = ar倍率[this.ctゴーゴー.n現在の値];

                    //this.txゴーゴー炎.b加算合成 = true;

                    //this.ctゴーゴー.n現在の値 = 6;
                    if (this.ctゴーゴー.b終了値に達した)
                    {
                        TJAPlayer3.app.Tx.Effects_Fire.vcScaling = Vector2.One;
                        TJAPlayer3.app.Tx.Effects_Fire.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Width / 2), TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i] - (TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Height / 2), new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Width * (this.ctゴーゴー炎.n現在の値), 0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Height));
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.Effects_Fire.vcScaling = new Vector2(f倍率);
                        TJAPlayer3.app.Tx.Effects_Fire.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i], TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i], new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Width * (this.ctゴーゴー炎.n現在の値), 0, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Width, TJAPlayer3.app.Skin.SkinConfig.Game.Effect.Fire.Height));
                    }
                }
            }
        }
        #endregion
        for (int i = 0; i < TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount; i++)
        {
            if (!this.st状態[i].ct進行.b停止中)
            {
                this.st状態[i].ct進行.t進行();
                if (this.st状態[i].ct進行.b終了値に達した)
                {
                    this.st状態[i].ct進行.t停止();
                }
                //if( this.txアタックエフェクトLower is not null )
                {
                    //this.txアタックエフェクトLower.b加算合成 = true;
                    int n = this.st状態[i].nIsBig == 1 ? 520 : 0;

                    switch (st状態[i].judge)
                    {
                        case EJudge.Perfect:
                        case EJudge.AutoPerfect:
                            //this.txアタックエフェクトLower.t2D描画( CDTXMania.app.Device, 285, 127, new Rectangle( this.st状態[ i ].ct進行.n現在の値 * 260, n, 260, 260 ) );
                            if (this.st状態[i].nIsBig == 1 && TJAPlayer3.app.Tx.Effects_Hit_Perfect_Big[this.st状態[i].ct進行.n現在の値] is not null)
                                TJAPlayer3.app.Tx.Effects_Hit_Perfect_Big[this.st状態[i].ct進行.n現在の値].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - TJAPlayer3.app.Tx.Effects_Hit_Perfect_Big[0].szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i] - TJAPlayer3.app.Tx.Effects_Hit_Perfect_Big[0].szTextureSize.Width / 2);
                            else if (TJAPlayer3.app.Tx.Effects_Hit_Perfect[this.st状態[i].ct進行.n現在の値] is not null)
                                TJAPlayer3.app.Tx.Effects_Hit_Perfect[this.st状態[i].ct進行.n現在の値].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - TJAPlayer3.app.Tx.Effects_Hit_Perfect[0].szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i] - TJAPlayer3.app.Tx.Effects_Hit_Perfect[0].szTextureSize.Width / 2);
                            break;

                        case EJudge.Good:
                            //this.txアタックエフェクトLower.t2D描画( CDTXMania.app.Device, 285, 127, new Rectangle( this.st状態[ i ].ct進行.n現在の値 * 260, n + 260, 260, 260 ) );
                            if (this.st状態[i].nIsBig == 1 && TJAPlayer3.app.Tx.Effects_Hit_Good_Big[this.st状態[i].ct進行.n現在の値] is not null)
                                TJAPlayer3.app.Tx.Effects_Hit_Good_Big[this.st状態[i].ct進行.n現在の値].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - TJAPlayer3.app.Tx.Effects_Hit_Good_Big[0].szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i] - TJAPlayer3.app.Tx.Effects_Hit_Good_Big[0].szTextureSize.Width / 2);
                            else if (TJAPlayer3.app.Tx.Effects_Hit_Good[this.st状態[i].ct進行.n現在の値] is not null)
                                TJAPlayer3.app.Tx.Effects_Hit_Good[this.st状態[i].ct進行.n現在の値].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[i] - TJAPlayer3.app.Tx.Effects_Hit_Good[0].szTextureSize.Width / 2, TJAPlayer3.app.Skin.SkinConfig.Game.JudgePointY[i] - TJAPlayer3.app.Tx.Effects_Hit_Good[0].szTextureSize.Width / 2);
                            break;

                        case EJudge.Miss:
                        case EJudge.Bad:
                            break;
                    }
                }
            }
        }


    }

    public virtual void Start(int nLane, EJudge judge, bool b両手入力, int nPlayer)
    {
        //2017.08.15 kairera0467 排他なので番地をそのまま各レーンの状態として扱う

        //for( int n = 0; n < 1; n++ )
        {
            this.st状態[nPlayer].ct進行 = new CCounter(0, 14, 20, TJAPlayer3.app.Timer);
            this.st状態[nPlayer].judge = judge;
            this.st状態[nPlayer].nPlayer = nPlayer;

            switch (nLane)
            {
                case 0x11:
                case 0x12:
                    this.st状態[nPlayer].nIsBig = 0;
                    break;
                case 0x13:
                case 0x14:
                case 0x1A:
                case 0x1B:
                    {
                        if (b両手入力)
                            this.st状態[nPlayer].nIsBig = 1;
                        else
                            this.st状態[nPlayer].nIsBig = 0;
                    }
                    break;
            }
        }
    }


    public void GOGOSTART()
    {
        this.ctゴーゴー = new CCounter(0, 17, 18, TJAPlayer3.app.Timer);
        if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1 && TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan) TJAPlayer3.stage演奏ドラム画面.GoGoSplash.StartSplash();
    }


    public void t分岐レイヤー_コース変化(int n現在, int n次回, int nPlayer)
    {
        if (n現在 == n次回)
        {
            return;
        }
        this.stBranch[nPlayer].ct分岐アニメ進行 = new CCounter(0, 300, 2, TJAPlayer3.app.Timer);

        this.stBranch[nPlayer].nBranchレイヤー透明度 = 6;
        this.stBranch[nPlayer].nY座標 = 1;

        this.stBranch[nPlayer].nBefore = n現在;
        this.stBranch[nPlayer].nAfter = n次回;

        TJAPlayer3.stage演奏ドラム画面.actLane.t分岐レイヤー_コース変化(n現在, n次回, nPlayer);
    }

    public void t判定枠移動(int n移動開始時間, double db移動時間, int n移動px, int nPlayer)
    {
        if ((CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)) >= n移動開始時間 + (db移動時間 * 1000))
        {
            int position = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + n移動px;
            TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] = position;
            TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[nPlayer] = position;
        }
        else
        {
            this.n移動開始時刻[nPlayer] = n移動開始時間;
            this.n移動開始X[nPlayer] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer];
            this.n総移動時間[nPlayer] = (int)(db移動時間 * 1000);
            this.n移動距離px[nPlayer] = n移動px;
            this.n移動目的場所X[nPlayer] = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] + n移動px;
        }
    }

    public void t判定枠戻し(int n移動px, int nPlayer)
    {
        this.n移動開始時刻[nPlayer] = -1;

        int judgeposition = TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - n移動px;

        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] = judgeposition;
        TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[0] = judgeposition;
    }

    public void t判定枠Reset()
    {
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[0] = this.nDefaultJudgePos[0, 0];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[0] = this.nDefaultJudgePos[0, 1];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldX[1] = this.nDefaultJudgePos[1, 0];
        TJAPlayer3.app.Skin.SkinConfig.Game.ScrollFieldY[1] = this.nDefaultJudgePos[1, 1];
        TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[0] = this.nDefaultJudgePos[0, 0];
        TJAPlayer3.stage演奏ドラム画面.FlyingNotes.StartPointX[1] = this.nDefaultJudgePos[1, 0];
    }

    #region[ private ]
    //-----------------
    //private CTexture txLane;
    //private CTexture txLaneB;
    //private CTexture tx枠線;
    //private CTexture tx判定枠;
    //private CTexture txゴーゴー;
    //private CTexture txゴーゴー炎;
    //private CTexture[] txArゴーゴー炎;
    //private CTexture[] txArアタックエフェクトLower_A;
    //private CTexture[] txArアタックエフェクトLower_B;
    //private CTexture[] txArアタックエフェクトLower_C;
    //private CTexture[] txArアタックエフェクトLower_D;

    //private CTexture[] txLaneFlush = new CTexture[3];

    //private CTexture[] tx普通譜面 = new CTexture[2];
    //private CTexture[] tx玄人譜面 = new CTexture[2];
    //private CTexture[] tx達人譜面 = new CTexture[2];

    //private CTextureAf txアタックエフェクトLower;

    protected STSTATUS[] st状態 = new STSTATUS[4];

    //private CTexture[] txゴーゴースプラッシュ;

    [StructLayout(LayoutKind.Sequential)]
    protected struct STSTATUS
    {
        public bool b使用中;
        public CCounter ct進行;
        public EJudge judge;
        public int nIsBig;
        public int nOpacity;
        public int nPlayer;
    }
    private CCounter ctゴーゴー;
    private CCounter ctゴーゴー炎;



    public STBRANCH[] stBranch = new STBRANCH[4];
    [StructLayout(LayoutKind.Sequential)]
    public struct STBRANCH
    {
        public CCounter ct分岐アニメ進行;
        public int nBefore;
        public int nAfter;

        public long nフラッシュ制御タイマ;
        public int nBranchレイヤー透明度;
        public int nBranch文字透明度;
        public int nY座標;
        public int nY;
    }

    private int[] n総移動時間 = new int[2];
    private int[] n移動開始X = new int[2];
    private int[] n移動開始時刻 = new int[2];
    private int[] n移動距離px = new int[2];
    private int[] n移動目的場所X = new int[2];

    internal int[,] nDefaultJudgePos = new int[2, 2];


    //-----------------
    #endregion
}
