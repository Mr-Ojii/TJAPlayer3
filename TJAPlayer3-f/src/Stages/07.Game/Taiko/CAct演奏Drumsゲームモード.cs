using FDK;

namespace TJAPlayer3;

internal class CAct演奏Drumsゲームモード : CActivity
{
    /// <summary>
    /// 現時点では「完走!叩ききりまショー!」のみ。
    ///
    /// </summary>
    public CAct演奏Drumsゲームモード()
    {
    }

    //叩ききりまショー!
    //<ルール>
    //_某DAMのやつに似てるやつ。
    //_演奏可能な残り時間が減っていく。
    //_複数の項目に対して、一定の条件をクリアしていって延命させていく。
    //_タイマーが0になったらSTAGE FAILED。
    //
    //判定要素
    //_精度
    //_ミス数
    //_ズレ時間
    //_最大コンボ数
    //その他諸々

    public ST叩ききりまショー st叩ききりまショー;
    public struct ST叩ききりまショー
    {
        public bool b最初のチップが叩かれた;
        public bool bタイマー使用中;
        public bool b超激辛;
        public bool b加算アニメ中;
        public int nヒット数_PERFECT;
        public int nヒット数_GOOD;
        public int nヒット数_BAD;
        public int nヒット数_MISS;
        public int n最大コンボ;
        public int n現在のコンボ;
        public int n区間ノート数;
        public int n最大ズレ時間;
        public int n最小ズレ時間;
        public int n現在通過したノート数;
        public int n全体最大ズレ時間;
        public int nおまけ加算が発生した回数;
        public int n延長アニメ速度;
        public CCounter ct残り時間;
        public CCounter ct加算時間表示;
        public CCounter ct加算審査中;
        public CCounter ct針アニメ;
    }
    private int n最後に時間延長した時刻;
    private int n演奏時間;
    private int n前回の延長時間;

    [StructLayout(LayoutKind.Sequential)]
    private struct STボーナス
    {
        public double ret;
        public double point;
        public STボーナス(double ret, double point)
        {
            this.ret = ret;
            this.point = point;
        }
    }

    private STボーナス[] n精度ボーナス;
    private STボーナス[] n最大ズレ時間ボーナス;
    private STボーナス[] n最小ズレ時間ボーナス;
    private STボーナス[] nコンボ率ボーナス;
    private STボーナス[] nミス率ボーナス;

    private STボーナス[] n全体精度ボーナス;
    private STボーナス[] n全体最大ズレ時間ボーナス;
    private STボーナス[] n全体コンボ率ボーナス;
    private STボーナス[] n全体ミス率ボーナス;

    public void t叩ききりまショー_初期化()
    {
        this.st叩ききりまショー = new ST叩ききりまショー();
        this.n演奏時間 = (TJAPlayer3.DTX[0].listChip.Count > 0) ? TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms : 0;
        this.st叩ききりまショー.ct残り時間 = new CCounter(0, 25000, 1, TJAPlayer3.app.Timer);
        this.st叩ききりまショー.ct加算時間表示 = new CCounter();
        this.st叩ききりまショー.ct加算審査中 = new CCounter();
        this.st叩ききりまショー.b最初のチップが叩かれた = false;
        this.st叩ききりまショー.bタイマー使用中 = false;
        this.st叩ききりまショー.b加算アニメ中 = false;
        this.n最後に時間延長した時刻 = 0;

        this.st叩ききりまショー.nヒット数_PERFECT = 0;
        this.st叩ききりまショー.nヒット数_GOOD = 0;
        this.st叩ききりまショー.nヒット数_BAD = 0;
        this.st叩ききりまショー.nヒット数_MISS = 0;
        this.st叩ききりまショー.n区間ノート数 = 0;
        this.st叩ききりまショー.n現在のコンボ = 0;
        this.st叩ききりまショー.n最小ズレ時間 = -1;
        this.st叩ききりまショー.n最大ズレ時間 = -1;
        this.st叩ききりまショー.n全体最大ズレ時間 = -1;
        this.st叩ききりまショー.n現在通過したノート数 = 0;
        this.st叩ききりまショー.b超激辛 = false;
        this.st叩ききりまショー.nおまけ加算が発生した回数 = 0;
        this.st叩ききりまショー.n延長アニメ速度 = 0;
        this.n前回の延長時間 = 0;

        this.st叩ききりまショー.ct針アニメ = new CCounter(0, 1000, 1, TJAPlayer3.app.Timer);

        this.t叩ききりまショー_判定項目と難易度を決める();
    }

    public void t叩ききりまショー_判定項目と難易度を決める()
    {
        //まず通常、激辛時でわける。
        if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー)
        {
            #region[ 通常 ]
            //通常の査定
            // 精度 > 最小ズレ > コンボ > 最大ズレ > ミス
            this.n精度ボーナス = new STボーナス[]{
                new STボーナス( 90, 5 ),
                new STボーナス( 70, 4.5 ),
                new STボーナス( 60, 3 ),
                new STボーナス( 50, 1 ),
                new STボーナス( 30, 0 )
            };
            this.n最小ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 5, 4 ),
                new STボーナス( 10, 3.5 ),
                new STボーナス( 20, 3 ),
                new STボーナス( 50, 1.5 ),
                new STボーナス( 80, -1 )
            };
            this.n最大ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 50, 2 ),
                new STボーナス( 60, 1.5 ),
                new STボーナス( 80, 0.5 ),
                new STボーナス( 90, 0 ),
                new STボーナス( 100, -1 )
            };
            this.nコンボ率ボーナス = new STボーナス[]{
                new STボーナス( 98.0, 3.5 ),
                new STボーナス( 80.0, 1 ),
                new STボーナス( 50.0, 0.5 ),
                new STボーナス( 35.0, -1.5 )
            };
            this.nミス率ボーナス = new STボーナス[]{
                new STボーナス( 0, 2 ),
                new STボーナス( 20.0, 1 ),
                new STボーナス( 50.0, -0.5 )
            };

            this.n全体精度ボーナス = new STボーナス[]{
                new STボーナス( 90, 3.5 ),
                new STボーナス( 70, 2.5 ),
                new STボーナス( 60, 1 ),
                new STボーナス( 50, 0.5 ),
                new STボーナス( 30, -0.5 )
            };
            this.n全体最大ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 50, 4.2 ),
                new STボーナス( 60, 3.6 ),
                new STボーナス( 80, 2 ),
                new STボーナス( 90, -0.5 ),
                new STボーナス( 100, -1 )
            };
            this.n全体コンボ率ボーナス = new STボーナス[]{
                new STボーナス( 98.0, 3 ),
                new STボーナス( 80.0, 2.5 ),
                new STボーナス( 50.0, 0.5 )
            };
            this.n全体ミス率ボーナス = new STボーナス[]{
                new STボーナス( 0, 2 ),
                new STボーナス( 20.0, 1.5 ),
                new STボーナス( 50.0, 0.5 ),
                new STボーナス( 70.0, -0.5 )
            };
            #endregion
        }
        else if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー激辛)
        {
            #region[ 激辛 ]
            //激ムズの査定
            // 最大ズレ > 精度 > コンボ > 最小ズレ > ミス
            //各項目最高値合計で20秒加算になるようにすること。
            this.n精度ボーナス = new STボーナス[]{
                new STボーナス( 100, 3 ),
                new STボーナス( 95, 2 ),
                new STボーナス( 90, 1 ),
                new STボーナス( 70, -2 ),
                new STボーナス( 50, -4 ),
                new STボーナス( 0, -10 )
            };
            this.n最小ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 0, 2 ),
                new STボーナス( 3, 1 ),
                new STボーナス( 5, -2 ),
                new STボーナス( 10, -3 ),
                new STボーナス( 30, -3 ),
                new STボーナス( 108, -4 )
            };
            this.n最大ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 3, 3.5 ),
                new STボーナス( 10, 2 ),
                new STボーナス( 15, 1 ),
                new STボーナス( 20, 0 ),
                new STボーナス( 50, -2 ),
                new STボーナス( 108, -5 )
            };
            this.nコンボ率ボーナス = new STボーナス[]{
                new STボーナス( 100.0, 1 ),
                new STボーナス( 50.0, 0.5 ),
                new STボーナス( 0.0, -5 )
            };
            this.nミス率ボーナス = new STボーナス[]{
                new STボーナス( 0, 1 ),
                new STボーナス( 100.0, -5 )
            };

            this.n全体精度ボーナス = new STボーナス[]{
                new STボーナス( 100, 5 ),
                new STボーナス( 99, 4 ),
                new STボーナス( 90, 1.5 ),
                new STボーナス( 80, 1 ),
                new STボーナス( 50, -1 ),
                new STボーナス( 30, -3 ),
                new STボーナス( 0, -4.5 )
            };
            this.n全体最大ズレ時間ボーナス = new STボーナス[]{
                new STボーナス( 20, 3 ),
                new STボーナス( 30, 1.5 ),
                new STボーナス( 50, 1 ),
                new STボーナス( 80, 0 ),
                new STボーナス( 108, -2.5 )
            };
            this.n全体コンボ率ボーナス = new STボーナス[]{
                new STボーナス( 100.0, 1 ),
                new STボーナス( 0.0, -2 )
            };
            this.n全体ミス率ボーナス = new STボーナス[]{
                new STボーナス( 0, 1 ),
                new STボーナス( 100.0, -2 ),
            };

            //★10の場合超激辛モードになる。
            if (TJAPlayer3.DTX[0].LEVELtaiko[TJAPlayer3.stage選曲.n確定された曲の難易度[0]] >= 10)
            {
                #region[ 超激辛 ]
                this.st叩ききりまショー.b超激辛 = true;

                this.n精度ボーナス = new STボーナス[]{
                    new STボーナス( 100, 3 ),
                    new STボーナス( 95, 2 ),
                    new STボーナス( 88, 1 ),
                    new STボーナス( 80, -3 ),
                    new STボーナス( 50, -6 ),
                    new STボーナス( 0, -10 )
                };

                this.n最大ズレ時間ボーナス = new STボーナス[]{
                    new STボーナス( 2, 4 ),
                    new STボーナス( 10, 1 ),
                    new STボーナス( 30, 0 ),
                    new STボーナス( 50, -1 ),
                    new STボーナス( 70, -3 ),
                    new STボーナス( 108, -5 )
                };
                this.nコンボ率ボーナス = new STボーナス[]{
                    new STボーナス( 100.0, 1 ),
                    new STボーナス( 0.0, -6 )
                };
                this.nミス率ボーナス = new STボーナス[]{
                    new STボーナス( 0, 1 ),
                    new STボーナス( 100.0, -6 )
                };

                this.n全体最大ズレ時間ボーナス = new STボーナス[]{
                    new STボーナス( 20, 3 ),
                    new STボーナス( 60, 1 ),
                    new STボーナス( 108, -5 )
                };
                this.n全体コンボ率ボーナス = new STボーナス[]{
                    new STボーナス( 100.0, 1 ),
                    new STボーナス( 0.0, -5 )
                };
                this.n全体ミス率ボーナス = new STボーナス[]{
                    new STボーナス( 0, 1 ),
                    new STボーナス( 100.0, -5 ),
                };
                #endregion
            }

            if (TJAPlayer3.app.ConfigToml.SuperHard)
            {
                #region[ 超激辛 ]
                this.st叩ききりまショー.b超激辛 = true;

                this.n精度ボーナス = new STボーナス[]{
                    new STボーナス( 100, 3 ),
                    new STボーナス( 98, 2.3 ),
                    new STボーナス( 95, 2 ),
                    new STボーナス( 90, 1.5 ),
                    new STボーナス( 85, 0 ),
                    new STボーナス( 80, -2 ),
                    new STボーナス( 60, -3 ),
                    new STボーナス( 40, -6 ),
                    new STボーナス( 0, -7.5 )
                };

                this.n最大ズレ時間ボーナス = new STボーナス[]{
                    new STボーナス( 8, 5 ),
                    new STボーナス( 18, 3 ),
                    new STボーナス( 40, 1 ),
                    new STボーナス( 50, -0.5 ),
                    new STボーナス( 70, -3 ),
                    new STボーナス( 108, -5 )
                };
                this.nコンボ率ボーナス = new STボーナス[]{
                    new STボーナス( 100.0, 1 ),
                    new STボーナス( 0.0, -6 )
                };
                this.nミス率ボーナス = new STボーナス[]{
                    new STボーナス( 0, 1 ),
                    new STボーナス( 100.0, -6 )
                };

                this.n全体精度ボーナス = new STボーナス[]{
                    new STボーナス( 100, 7 ),
                    new STボーナス( 99, 4 ),
                    new STボーナス( 90, 2 ),
                    new STボーナス( 80, 1 ),
                    new STボーナス( 50, -1 ),
                    new STボーナス( 0, -7 )
                };
                this.n全体最大ズレ時間ボーナス = new STボーナス[]{
                    new STボーナス( 20, 3 ),
                    new STボーナス( 40, 1 ),
                    new STボーナス( 60, -3 ),
                    new STボーナス( 108, -5 )
                };
                this.n全体コンボ率ボーナス = new STボーナス[]{
                    new STボーナス( 100.0, 1 ),
                    new STボーナス( 0.0, -5 )
                };
                this.n全体ミス率ボーナス = new STボーナス[]{
                    new STボーナス( 0, 0 ),
                    new STボーナス( 100.0, -5 ),
                };
                #endregion
            }
            #endregion
        }
    }

    public override int On進行描画()
    {
        if (TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー || TJAPlayer3.app.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー激辛)
        {
            //if( this.st叩ききりまショー.b最初のチップが叩かれた == true )//&&
            //CDTXMania.stage演奏ドラム画面.r検索範囲内にチップがあるか調べる( CSoundManager.rc演奏用タイマ.n現在時刻ms, 0, 3000 ) )
            //this.st叩ききりまショー.ct残り時間.t進行();
            //else
            //{
            //    this.st叩ききりまショー.ct残り時間.n現在の値 = this.st叩ききりまショー.ct残り時間.n現在の値;
            //}


            //if( !this.st叩ききりまショー.ct残り時間.b停止中 )
            if (this.st叩ききりまショー.bタイマー使用中)
            {
                if (!this.st叩ききりまショー.ct残り時間.b停止中 || this.st叩ききりまショー.b加算アニメ中 == true)
                {
                    this.st叩ききりまショー.ct残り時間.t進行();
                    if (!TJAPlayer3.stage演奏ドラム画面.r検索範囲内にチップがあるか調べる((long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)), 5000, 0) || this.st叩ききりまショー.b加算アニメ中 == true)
                    {
                        this.st叩ききりまショー.bタイマー使用中 = false;
                        this.st叩ききりまショー.ct残り時間.t停止();
                    }
                }
            }

            if (!this.st叩ききりまショー.bタイマー使用中 && this.st叩ききりまショー.b加算アニメ中 == false)
            {
                if ((this.st叩ききりまショー.b最初のチップが叩かれた == true && (TJAPlayer3.stage演奏ドラム画面.r検索範囲内にチップがあるか調べる(CSoundManager.rc演奏用タイマ.n現在時刻ms, 2000, 0))))
                {
                    this.st叩ききりまショー.bタイマー使用中 = true;
                    int nCount = this.st叩ききりまショー.ct残り時間.n現在の値;
                    this.st叩ききりまショー.ct残り時間 = new CCounter(0, 25000, 1, TJAPlayer3.app.Timer);
                    this.st叩ききりまショー.ct針アニメ = new CCounter(0, 1000, 1, TJAPlayer3.app.Timer);
                    this.st叩ききりまショー.ct残り時間.n現在の値 = nCount;
                }

            }


            if ((this.st叩ききりまショー.ct残り時間.n現在の値 >= 20000) && this.st叩ききりまショー.ct残り時間.n現在の値 != 25000)
                this.t叩ききりまショー_評価をして残り時間を延長する();

            if (TJAPlayer3.app.Tx.Tile_Black is not null)
            {
                if (this.st叩ききりまショー.ct残り時間.n現在の値 >= 22000 && this.st叩ききりまショー.ct残り時間.n現在の値 < 23000)
                    TJAPlayer3.app.Tx.Tile_Black.Opacity = 64;
                else if (this.st叩ききりまショー.ct残り時間.n現在の値 >= 23000 && this.st叩ききりまショー.ct残り時間.n現在の値 < 24000)
                    TJAPlayer3.app.Tx.Tile_Black.Opacity = 128;
                else if (this.st叩ききりまショー.ct残り時間.n現在の値 >= 24000)
                    TJAPlayer3.app.Tx.Tile_Black.Opacity = 192;
                else
                    TJAPlayer3.app.Tx.Tile_Black.Opacity = 0;

                for (int i = 0; i <= (TJAPlayer3.app.LogicalSize.Width / 64); i++)
                {
                    for (int j = 0; j <= (TJAPlayer3.app.LogicalSize.Height / 64); j++)
                    {
                        TJAPlayer3.app.Tx.Tile_Black.t2D描画(TJAPlayer3.app.Device, i * 64, j * 64);
                    }
                }
            }

            //CDTXMania.act文字コンソール.tPrint( 100, 0, C文字コンソール.EFontType.白, ( 25 - this.st叩ききりまショー.ct残り時間.n現在の値 ).ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16, C文字コンソール.EFontType.白, this.st叩ききりまショー.n区間ノート数.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 2, C文字コンソール.EFontType.白, this.st叩ききりまショー.n現在通過したノート数.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 3, C文字コンソール.EFontType.白, this.st叩ききりまショー.nヒット数_MISS.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 4, C文字コンソール.EFontType.白, this.st叩ききりまショー.n最小ズレ時間.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 5, C文字コンソール.EFontType.白, this.st叩ききりまショー.n最大ズレ時間.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 6, C文字コンソール.EFontType.白, this.st叩ききりまショー.n全体最大ズレ時間.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 7, C文字コンソール.EFontType.白, this.st叩ききりまショー.n最大コンボ.ToString() );
            //CDTXMania.act文字コンソール.tPrint( 100, 16 * 7, C文字コンソール.EFontType.白, this.st叩ききりまショー.ct加算審査中.n現在の値.ToString() );

            #region[ 残り時間描画 ]
            if (TJAPlayer3.app.Tx.Taiko_Combo is not null)
            {
                if (TJAPlayer3.app.Tx.GameMode_Timer_Frame is not null)
                    TJAPlayer3.app.Tx.GameMode_Timer_Frame.t2D描画(TJAPlayer3.app.Device, 230, 84);
                this.st叩ききりまショー.ct針アニメ.t進行Loop();

                float fRotate = -CConvert.DegreeToRadian(360.0f * (this.st叩ききりまショー.ct針アニメ.n現在の値 / 1000.0f));
                if (this.st叩ききりまショー.b加算アニメ中 == true)
                    fRotate = CConvert.DegreeToRadian(360.0f * (this.st叩ききりまショー.ct針アニメ.n現在の値 / (float)this.st叩ききりまショー.n延長アニメ速度));

                if (TJAPlayer3.app.Tx.GameMode_Timer_Tick is not null)
                {
                    if (this.st叩ききりまショー.b最初のチップが叩かれた)
                    {
                        TJAPlayer3.app.Tx.GameMode_Timer_Tick.fRotation = fRotate;
                    }
                    else
                    {
                        TJAPlayer3.app.Tx.GameMode_Timer_Tick.fRotation = 0;
                    }

                    TJAPlayer3.app.Tx.GameMode_Timer_Tick.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, 280, 134);
                }

                string str表示する残り時間 = (this.st叩ききりまショー.ct残り時間.n現在の値 < 1000) ? "25" : ((26000 - this.st叩ききりまショー.ct残り時間.n現在の値) / 1000).ToString();
                this.t小文字表示(230 + (str表示する残り時間.Length * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0] / 4), 84 + TJAPlayer3.app.Tx.GameMode_Timer_Frame.szTextureSize.Height / 2, string.Format("{0,2:#0}", str表示する残り時間));
            }

            if (!this.st叩ききりまショー.ct加算審査中.b停止中)
            {
                if (!this.st叩ききりまショー.ct加算審査中.b停止中)
                {
                    this.st叩ききりまショー.ct加算審査中.t進行();
                    if (this.st叩ききりまショー.ct加算審査中.b終了値に達した)
                    {
                        this.st叩ききりまショー.ct加算審査中.t停止();
                        this.st叩ききりまショー.b加算アニメ中 = false;
                        this.t加算時間描画_Start();
                    }
                }
            }
            if (!this.st叩ききりまショー.ct加算時間表示.b停止中)
            {
                if (!this.st叩ききりまショー.ct加算時間表示.b停止中)
                {
                    this.st叩ききりまショー.ct加算時間表示.t進行();
                    if (this.st叩ききりまショー.ct加算時間表示.b終了値に達した)
                    {
                        this.st叩ききりまショー.ct加算時間表示.t停止();
                    }
                }
                this.t加算時間描画(this.n前回の延長時間);
            }
            #endregion
        }
        return 0;
    }

    private void t叩ききりまショー_評価をして残り時間を延長する()
    {
        double n延長する時間 = 0;

        //最後に延長した時刻から11秒経過していなければ延長を行わない。
        if (this.n最後に時間延長した時刻 + 11000 <= (CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0)))
        {
            //1項目につき5秒
            //-精度
            if (this.st叩ききりまショー.nヒット数_PERFECT != 0)
            {
                double db区間内精度 = ((double)(this.st叩ききりまショー.nヒット数_PERFECT) / this.st叩ききりまショー.n区間ノート数) * 100.0;
                for (int i = 0; i < this.n精度ボーナス.Length; i++)
                {
                    if (db区間内精度 >= this.n精度ボーナス[i].ret)
                    {
                        n延長する時間 += this.n精度ボーナス[i].point;
                        break;
                    }
                }
            }

            //-ラグ時間
            #region[ ラグ時間による判定 ]
            if (this.st叩ききりまショー.n最小ズレ時間 != -1)
            {
                for (int i = 0; i < this.n最小ズレ時間ボーナス.Length; i++)
                {
                    if (this.st叩ききりまショー.n最小ズレ時間 >= this.n最小ズレ時間ボーナス[i].ret)
                    {
                        n延長する時間 += this.n最小ズレ時間ボーナス[i].point;
                        break;
                    }
                }
            }

            if (this.st叩ききりまショー.n最大ズレ時間 != -1)
            {
                for (int i = 0; i < this.n最大ズレ時間ボーナス.Length; i++)
                {
                    if (this.st叩ききりまショー.n最大ズレ時間 <= this.n最大ズレ時間ボーナス[i].ret)
                    {
                        n延長する時間 += this.n最大ズレ時間ボーナス[i].point;
                        break;
                    }
                }
            }
            #endregion
            if (this.st叩ききりまショー.n最大コンボ != 0)
            {
                double db区間内コンボ精度 = ((double)this.st叩ききりまショー.n最大コンボ / this.st叩ききりまショー.n区間ノート数) * 100.0;
                for (int i = 0; i < this.nコンボ率ボーナス.Length; i++)
                {
                    if (db区間内コンボ精度 >= this.nコンボ率ボーナス[i].ret)
                    {
                        n延長する時間 += this.nコンボ率ボーナス[i].point;
                        break;
                    }
                }
            }

            double db区間内ミス率 = (((double)this.st叩ききりまショー.nヒット数_BAD + this.st叩ききりまショー.nヒット数_MISS) / this.st叩ききりまショー.n区間ノート数) * 100.0;
            for (int i = 0; i < this.nミス率ボーナス.Length; i++)
            {
                if (db区間内ミス率 >= this.nミス率ボーナス[i].ret)
                {
                    n延長する時間 += this.nミス率ボーナス[i].point;
                    break;
                }
            }
            #region[ 全体 ]
            if (TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect != 0 || TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good != 0)
            {
                double db全体精度 = ((double)(TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Perfect + TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Good) / this.st叩ききりまショー.n区間ノート数) * 100.0;
                for (int i = 0; i < this.n全体精度ボーナス.Length; i++)
                {
                    if (db全体精度 >= this.n全体精度ボーナス[i].ret)
                    {
                        n延長する時間 += this.n全体精度ボーナス[i].point;
                        break;
                    }
                }
            }

            //-ラグ時間
            #region[ ラグ時間による判定 ]
            if (this.st叩ききりまショー.n全体最大ズレ時間 != -1)
            {
                for (int i = 0; i < this.n全体最大ズレ時間ボーナス.Length; i++)
                {
                    if (this.st叩ききりまショー.n全体最大ズレ時間 <= this.n全体最大ズレ時間ボーナス[i].ret)
                    {
                        n延長する時間 += this.n全体最大ズレ時間ボーナス[i].point;
                        break;
                    }
                }
            }
            #endregion
            if (TJAPlayer3.stage演奏ドラム画面.actCombo.n現在のコンボ数.Max[0] != 0)
            {
                double db全体コンボ率 = ((double)TJAPlayer3.stage演奏ドラム画面.actCombo.n現在のコンボ数.Max[0] / this.st叩ききりまショー.n現在通過したノート数) * 100.0;
                for (int i = 0; i < this.n全体コンボ率ボーナス.Length; i++)
                {
                    if (db全体コンボ率 >= this.n全体コンボ率ボーナス[i].ret)
                    {
                        n延長する時間 += this.n全体コンボ率ボーナス[i].point;
                        break;
                    }
                }
            }

            double db全体ミス率 = (((double)TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Bad + TJAPlayer3.stage演奏ドラム画面.nヒット数[0].Miss) / this.st叩ききりまショー.n現在通過したノート数) * 100.0;
            for (int i = 0; i < this.n全体ミス率ボーナス.Length; i++)
            {
                if (db全体ミス率 >= this.n全体ミス率ボーナス[i].ret)
                {
                    n延長する時間 += this.n全体ミス率ボーナス[i].point;
                    break;
                }
            }
            #endregion


            this.n最後に時間延長した時刻 = (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
            if (n延長する時間 < 0)
                n延長する時間 = 0;
            if (this.st叩ききりまショー.n区間ノート数 == 0)
                n延長する時間 = 15;

            //各数値を初期化
            this.st叩ききりまショー.nヒット数_PERFECT = 0;
            this.st叩ききりまショー.nヒット数_GOOD = 0;
            this.st叩ききりまショー.nヒット数_BAD = 0;
            this.st叩ききりまショー.nヒット数_MISS = 0;
            this.st叩ききりまショー.n区間ノート数 = 0;
            this.st叩ききりまショー.n現在のコンボ = 0;
            this.st叩ききりまショー.n最小ズレ時間 = -1;
            this.st叩ききりまショー.n最大ズレ時間 = -1;

            this.n前回の延長時間 = (int)n延長する時間;
            n延長する時間 = n延長する時間 * 1000;
            if (n延長する時間 > 0)
            {
                this.t加算審査アニメ_Start();
                if (this.st叩ききりまショー.b加算アニメ中 == false)
                    this.t加算時間描画_Start();
            }
            this.st叩ききりまショー.ct残り時間.n現在の値 -= (int)n延長する時間;
        }
        else if (this.st叩ききりまショー.ct残り時間.n現在の値 >= 24000)
        {
            if (this.st叩ききりまショー.nおまけ加算が発生した回数 > 3)
                return;
            if (this.st叩ききりまショー.b超激辛 && (((double)this.st叩ききりまショー.nヒット数_BAD + this.st叩ききりまショー.nヒット数_MISS) > 0))
                return; //ミスが出るようでは上達しませんよ。お兄様。
            if (TJAPlayer3.app.ConfigToml.SuperHard)
                return; //スーパーハード時はボーナス加点無し。


            this.st叩ききりまショー.nおまけ加算が発生した回数++;

            if (this.st叩ききりまショー.nヒット数_PERFECT != 0)
            {
                double db区間内精度 = ((double)(this.st叩ききりまショー.nヒット数_PERFECT) / this.st叩ききりまショー.n区間ノート数) * 100.0;
                if (this.st叩ききりまショー.b超激辛 ? (db区間内精度 >= 95.0) : (db区間内精度 >= 98.0))
                {
                    n延長する時間 += 6;
                }
            }
            #region[ ラグ時間による判定 ]
            if (this.st叩ききりまショー.n最小ズレ時間 != -1)
            {
                if (this.st叩ききりまショー.n最小ズレ時間 >= 0)
                {
                    n延長する時間 += 6;
                }
            }

            if (this.st叩ききりまショー.n最大ズレ時間 != -1)
            {
                if (this.st叩ききりまショー.n最大ズレ時間 <= 30)
                {
                    n延長する時間 += 6;
                }
            }
            #endregion
            double db区間内ミス率 = (((double)this.st叩ききりまショー.nヒット数_BAD + this.st叩ききりまショー.nヒット数_MISS) / this.st叩ききりまショー.n区間ノート数) * 100.0;
            if (db区間内ミス率 >= 5.0)
            {
                n延長する時間 -= 2;
            }


            this.n最後に時間延長した時刻 = (int)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
            if (n延長する時間 < 0)
                n延長する時間 = 0;

            //各数値を初期化
            this.st叩ききりまショー.nヒット数_PERFECT = 0;
            this.st叩ききりまショー.nヒット数_GOOD = 0;
            this.st叩ききりまショー.nヒット数_BAD = 0;
            this.st叩ききりまショー.nヒット数_MISS = 0;
            this.st叩ききりまショー.n区間ノート数 = 0;
            this.st叩ききりまショー.n現在のコンボ = 0;
            this.st叩ききりまショー.n最小ズレ時間 = -1;
            this.st叩ききりまショー.n最大ズレ時間 = -1;

            this.n前回の延長時間 = (int)n延長する時間;
            n延長する時間 = n延長する時間 * 1000;
            if (n延長する時間 > 0)
            {
                this.t加算審査アニメ_Start();
                if (this.st叩ききりまショー.b加算アニメ中 == false)
                    this.t加算時間描画_Start();
            }
            if (n延長する時間 > 5000)
                this.st叩ききりまショー.ct残り時間.n現在の値 -= (int)n延長する時間;
        }

        if (n延長する時間 >= 12000)
            this.st叩ききりまショー.n延長アニメ速度 = 100;
        else if (n延長する時間 < 12000 && n延長する時間 >= 5000)
            this.st叩ききりまショー.n延長アニメ速度 = 250;
        else
            this.st叩ききりまショー.n延長アニメ速度 = 500;
    }

    public void t叩ききりまショー_判定から各数値を増加させる(EJudge eJudge, int nLagTime)
    {
        this.st叩ききりまショー.b最初のチップが叩かれた = true;
        this.st叩ききりまショー.n区間ノート数++;
        this.st叩ききりまショー.n現在通過したノート数++;
        switch (eJudge)
        {
            case EJudge.Perfect:
                this.st叩ききりまショー.nヒット数_PERFECT++;
                break;
            case EJudge.Good:
                this.st叩ききりまショー.nヒット数_GOOD++;
                break;
            case EJudge.Bad:
                this.st叩ききりまショー.nヒット数_BAD++;
                break;
            case EJudge.Miss:
                this.st叩ききりまショー.nヒット数_MISS++;
                break;
        }
        switch (eJudge)
        {
            case EJudge.Perfect:
            case EJudge.Good:
                this.st叩ききりまショー.n現在のコンボ++;
                if (this.st叩ききりまショー.n現在のコンボ >= this.st叩ききりまショー.n最大コンボ)
                    this.st叩ききりまショー.n最大コンボ = this.st叩ききりまショー.n現在のコンボ;
                if (Math.Abs(nLagTime) > this.st叩ききりまショー.n最大ズレ時間)
                {
                    this.st叩ききりまショー.n最大ズレ時間 = Math.Abs(nLagTime);
                }
                if (Math.Abs(nLagTime) > this.st叩ききりまショー.n全体最大ズレ時間)
                {
                    this.st叩ききりまショー.n全体最大ズレ時間 = Math.Abs(nLagTime);
                }
                if (this.st叩ききりまショー.n最小ズレ時間 == -1)
                    this.st叩ききりまショー.n最小ズレ時間 = Math.Abs(nLagTime);
                if (Math.Abs(nLagTime) < this.st叩ききりまショー.n最小ズレ時間)
                {
                    this.st叩ききりまショー.n最小ズレ時間 = Math.Abs(nLagTime);
                }
                break;
            default:
                this.st叩ききりまショー.n現在のコンボ = 0;
                break;
        }
    }

    private void t加算審査アニメ_Start()
    {
        this.st叩ききりまショー.ct加算審査中 = new CCounter(0, 2000, 1, TJAPlayer3.app.Timer);
        this.st叩ききりまショー.b加算アニメ中 = true;
    }
    private void t加算時間描画_Start()
    {
        this.st叩ききりまショー.ct加算時間表示 = new CCounter(0, 1, 1000, TJAPlayer3.app.Timer);
    }

    private void t加算時間描画(int addtime)
    {
        this.t加算文字表示(258, 150, string.Format("{0,2:#0}", addtime.ToString()));
        //CDTXMania.act文字コンソール.tPrint( 236, 80, C文字コンソール.EFontType.赤, "+" + string.Format( "{0,2:#0}", addtime.ToString() ) );
    }

    private void t小文字表示(int x, int y, string str)
    {
        foreach (char ch in str)
        {
            if (int.TryParse(ch.ToString(), out var i))
            {
                Rectangle rectangle = new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0] * i, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[1]);
                if (TJAPlayer3.app.Tx.Taiko_Combo[0] is not null)
                {
                    if (this.st叩ききりまショー.bタイマー使用中)
                        TJAPlayer3.app.Tx.Taiko_Combo[0].Opacity = 255;
                    else if (this.st叩ききりまショー.b最初のチップが叩かれた && !this.st叩ききりまショー.bタイマー使用中)
                        TJAPlayer3.app.Tx.Taiko_Combo[0].Opacity = 128;
                    if (this.st叩ききりまショー.b加算アニメ中)
                        TJAPlayer3.app.Tx.Taiko_Combo[0].Opacity = 0;
                    TJAPlayer3.app.Tx.Taiko_Combo[0].vcScaling.Y = 1f;
                    TJAPlayer3.app.Tx.Taiko_Combo[0].vcScaling.X = 1f;
                    TJAPlayer3.app.Tx.Taiko_Combo[0].t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, x, y, rectangle);
                }
            }
            x += TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[0] * 2;
        }
    }
    protected void t加算文字表示(int x, int y, string str)
    {
        char[] cFont = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        foreach (char ch in str)
        {
            for (int i = 0; i < cFont.Length; i++)
            {
                if (cFont[i] == ch)
                {
                    Rectangle rectangle = new Rectangle(TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[0] * i, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[0], TJAPlayer3.app.Skin.SkinConfig.Game.Score.Size[1]);
                    if (TJAPlayer3.app.Tx.Taiko_Score[0] is not null)
                    {
                        TJAPlayer3.app.Tx.Taiko_Score[0].vcScaling.Y = 1f;
                        TJAPlayer3.app.Tx.Taiko_Score[0].t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
                    }
                }
            }
            x += 20;
        }
    }
}
