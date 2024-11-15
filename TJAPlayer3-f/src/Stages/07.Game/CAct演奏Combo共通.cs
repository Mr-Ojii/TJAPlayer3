using FDK;

namespace TJAPlayer3;

internal class CAct演奏Combo共通 : CActivity
{
    // プロパティ

    public STCOMBO n現在のコンボ数;
    public struct STCOMBO
    {
        public STCOMBO()
        {
            Max = new int[4];
            Combo = new int[4];
        }
        public CAct演奏Combo共通 act;

        public int this[int index]
        {
            get
            {
                return this.Combo[index];
            }
            set
            {
                this.Combo[index] = value;
                if (this.Combo[index] > this.Max[index])
                {
                    this.Max[index] = this.Combo[index];
                }
                this.act.status[index].nCOMBO値 = this.Combo[index];
                this.act.status[index].n最高COMBO値 = this.Max[index];
            }
        }
        public int[] Max { get; private set; }
        private int[] Combo { get; set; }
    }

    protected enum EEvent { 非表示, 数値更新, 同一数値, ミス通知 }
    protected enum EMode { 非表示中, 進行表示中, 残像表示中 }
    protected CSTAT[] status;
    public CCounter[] ctコンボ加算;
    public CCounter ctコンボラメ;


    private float[] ComboScale = new float[]
    {
        0.000f,
        0.079f,
        0.158f,
        0.237f,
        0.211f,
        0.184f,
        0.158f,
        0.132f,
        0.066f,
        0.040f,
        0.026f,
        0.013f,
        0.000f
    };
    private float[,] ComboScale_Ex = new float[,]
    {
        { 0.000f, 0},
        { 0.072f, -1},
        { 0.144f, -2},
        { 0.120f, -3},
        { 0.108f, -3},
        { 0.096f, -2},
        { 0.084f, -1},
        { 0.066f, -1},
        { 0.044f, -1},
        { 0.033f, -1},
        { 0.022f, -1},
        { 0.011f, -0},
        { 0.000f, 0}
    };
    // 内部クラス

    protected class CSTAT
    {
        public CAct演奏Combo共通.EMode e現在のモード;
        public int nCOMBO値;
        public long nコンボが切れた時刻;
        public int nジャンプインデックス値;
        public int n現在表示中のCOMBO値;
        public int n最高COMBO値;
        public int n残像表示中のCOMBO値;
        public long n前回の時刻_ジャンプ用;
    }

    // コンストラクタ

    public CAct演奏Combo共通()
    {
    }


    // メソッド

    protected virtual void tコンボ表示_太鼓(int nCombo値, int nジャンプインデックス, int nPlayer)
    {
        //テスト用コンボ数
        //nCombo値 = 114;
        #region [ 事前チェック。]
        //-----------------
        //if( CDTXMania.ConfigIni.bドラムコンボ表示 == false )
        //	return;		// 表示OFF。

        if (nCombo値 == 0)
            return;		// コンボゼロは表示しない。
        //-----------------
        #endregion

        int[] n位の数 = new int[10];	// 表示は10桁もあれば足りるだろう

        this.ctコンボラメ.t進行Loop();
        this.ctコンボ加算[nPlayer].t進行();

        #region [ nCombo値を桁数ごとに n位の数[] に格納する。（例：nCombo値=125 のとき n位の数 = { 5,2,1,0,0,0,0,0,0,0 } ） ]
        //-----------------
        int n = nCombo値;
        int n桁数 = 0;
        while ((n > 0) && (n桁数 < 10))
        {
            n位の数[n桁数] = n % 10;        // 1の位を格納
            n /= 10;                        // 右へシフト（例: 12345 → 1234 ）
            n桁数++;
        }
        //-----------------
        #endregion

        #region [ n位の数[] を、"COMBO" → 1の位 → 10の位 … の順に、右から左へ向かって順番に表示する。]
        //-----------------
        //X右座標を元にして、右座標 - ( コンボの幅 * 桁数 ) でX座標を求めていく?

        int n数字とCOMBOを合わせた画像の全長px = ((44) * n桁数);
        int x = 245 + (n数字とCOMBOを合わせた画像の全長px / 2);
        //int y = 212;
        //int y = CDTXMania.Skin.nComboNumberY[ nPlayer ];

        #region[ コンボ文字 ]
        if (n桁数 <= 2)
        {
            TJAPlayer3.app.Tx.Taiko_Combo_Text?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextX[nPlayer], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextY[nPlayer], new Rectangle(0, 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextSize[1]));
        }
        else
        {
            TJAPlayer3.app.Tx.Taiko_Combo_Text?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextX[nPlayer], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextY[nPlayer], new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextSize[1], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboTextSize[1]));
        }
        #endregion

        int rightX = 0;
        #region 一番右の数字の座標の決定
        if (n桁数 == 1)
        {
            // 一桁ならそのままSkinConfigの座標を使用する。
            rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboX[nPlayer];
        }
        else if (n桁数 == 2)
        {
            // 二桁ならSkinConfigの座標+パディング/2を使用する
            rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboX[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[0] / 2;
        }
        else if (n桁数 == 3)
        {
            // 三桁ならSkinConfigの座標+パディングを使用する
            rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExX[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[1];
        }
        else if (n桁数 == 4)
        {
            // 四桁ならSkinconfigの座標+パディング/2 + パディングを使用する
            rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4X[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2];
        }
        else
        {
            // 五桁以上の場合
            int rightDigit = 0;
            switch (n桁数 % 2)
            {
                case 0:
                    // 2で割り切れる
                    // パディング/2を足す必要がある
                    // 右に表示される桁数を求め、-1する
                    rightDigit = n桁数 / 2 - 1;
                    rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4X[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] / 2 + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * rightDigit;
                    break;
                case 1:
                    // 2で割るとあまりが出る
                    // そのままパディングを足していく
                    // 右に表示される桁数を求める(中央除く -1)
                    rightDigit = (n桁数 - 1) / 2;
                    rightX = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4X[nPlayer] + TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * rightDigit;
                    break;
                default:
                    break;
            }
        }
        #endregion


        for (int i = 0; i < n桁数; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CTexture? combo = TJAPlayer3.app.Tx.Taiko_Combo[j];
                if (combo is not null)
                    combo.Opacity = 255;
            }

            CTexture? combo0 = TJAPlayer3.app.Tx.Taiko_Combo[0];
            CTexture? combo1 = TJAPlayer3.app.Tx.Taiko_Combo[1];
            if (n桁数 <= 1)
            {
                if (combo0 is not null)
                {
                    var yScalling = ComboScale[this.ctコンボ加算[nPlayer].n現在の値];
                    combo0.vcScaling.Y = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[0] + yScalling;
                    combo0.vcScaling.X = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[0];
                    combo0.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, rightX, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboY[nPlayer], new Rectangle(n位の数[i] * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0], 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[1]));
                }
            }
            else if (n桁数 <= 2)
            {
                //int[] arComboX = { CDTXMania.Skin.Game_Taiko_Combo_X[nPlayer] + CDTXMania.Skin.Game_Taiko_Combo_Padding[0], CDTXMania.Skin.Game_Taiko_Combo_X[nPlayer] - CDTXMania.Skin.Game_Taiko_Combo_Padding[0] };
                if (combo0 is not null)
                {
                    var yScalling = ComboScale[this.ctコンボ加算[nPlayer].n現在の値];
                    combo0.vcScaling.Y = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[0] + yScalling;
                    combo0.vcScaling.X = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[0];
                    combo0.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[0] * i, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboY[nPlayer], new Rectangle(n位の数[i] * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0], 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSize[1]));
                }
            }
            else if (n桁数 == 3)
            {
                if (combo1 is not null)
                {
                    var yScalling = ComboScale_Ex[this.ctコンボ加算[nPlayer].n現在の値, 0];
                    combo1.vcScaling.Y = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1] + yScalling;
                    combo1.vcScaling.X = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1];
                    var yJumping = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExIsJumping ? (int)ComboScale_Ex[this.ctコンボ加算[nPlayer].n現在の値, 1] : 0;
                    combo1.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[1] * i, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExY[nPlayer] + yJumping, new Rectangle(n位の数[i] * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0], 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1]));
                }
                if (TJAPlayer3.app.Tx.Taiko_Combo_Effect is not null)
                {
                    TJAPlayer3.app.Tx.Taiko_Combo_Effect.eBlendMode = CTexture.EBlendMode.Addition;
                    if (this.ctコンボラメ.n現在の値 < 14)
                    {
                        // ひだり
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 7) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 7 && this.ctコンボラメ.n現在の値 < 14) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (24 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[1] * i) - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1]), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExY[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1]) - (int)(1.05 * this.ctコンボラメ.n現在の値));
                    }
                    if (ctコンボラメ.n現在の値 > 4 && ctコンボラメ.n現在の値 < 24)
                    {
                        // みぎ
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 11) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 11 && this.ctコンボラメ.n現在の値 < 24) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (12 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[1] * i) + ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1]), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExY[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1]) - (int)(1.05 * this.ctコンボラメ.n現在の値));

                    }
                    if (ctコンボラメ.n現在の値 > 14)
                    {
                        // まんなか
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 252) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 22 && this.ctコンボラメ.n現在の値 < 30) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (6 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[1] * i), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExY[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[1]) - (int)(1.05 * this.ctコンボラメ.n現在の値));
                    }
                }
            }
            else
            {
                if (combo1 is not null)
                {
                    var yScalling = ComboScale_Ex[this.ctコンボ加算[nPlayer].n現在の値, 0];
                    combo1.vcScaling.Y = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2] + yScalling;
                    combo1.vcScaling.X = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2];
                    var yJumping = TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboExIsJumping ? (int)ComboScale_Ex[this.ctコンボ加算[nPlayer].n現在の値, 1] : 0;
                    combo1.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * i, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4Y[nPlayer] + yJumping, new Rectangle(n位の数[i] * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0], 0, TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0], TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1]));
                }
                if (TJAPlayer3.app.Tx.Taiko_Combo_Effect is not null)
                {
                    TJAPlayer3.app.Tx.Taiko_Combo_Effect.eBlendMode = CTexture.EBlendMode.Addition;
                    if (this.ctコンボラメ.n現在の値 < 14)
                    {
                        // ひだり
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 7) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 7 && this.ctコンボラメ.n現在の値 < 14) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (24 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * i) - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2]), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4Y[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2]) - (int)(1.05 * this.ctコンボラメ.n現在の値));
                    }
                    if (ctコンボラメ.n現在の値 > 4 && ctコンボラメ.n現在の値 < 24)
                    {
                        // みぎ
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 11) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 11 && this.ctコンボラメ.n現在の値 < 24) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (12 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * i) + ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[0] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2]), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4Y[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2]) - (int)(1.05 * this.ctコンボラメ.n現在の値));

                    }
                    if (ctコンボラメ.n現在の値 > 14)
                    {
                        // まんなか
                        #region[透明度制御]
                        if (this.ctコンボラメ.n現在の値 < 22) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = 255;
                        else if (this.ctコンボラメ.n現在の値 >= 22 && this.ctコンボラメ.n現在の値 < 30) TJAPlayer3.app.Tx.Taiko_Combo_Effect.Opacity = (int)(204 - (6 * this.ctコンボラメ.n現在の値));
                        #endregion
                        TJAPlayer3.app.Tx.Taiko_Combo_Effect.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, (rightX - TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboPadding[2] * i), TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboEx4Y[nPlayer] - ((TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboSizeEx[1] / 4) * TJAPlayer3.app.Skin.SkinConfig.Game.Taiko.ComboScale[2]) - (int)(1.05 * this.ctコンボラメ.n現在の値));

                    }
                }

            }
        }

        //-----------------
        #endregion
    }

    // CActivity 実装

    public override void On活性化()
    {
        this.n現在のコンボ数 = new STCOMBO() { act = this };
        this.status = new CSTAT[4] { new(), new(), new(), new() };
        this.ctコンボ加算 = new CCounter[4];
        for (int i = 0; i < 4; i++)
        {
            this.status[i].e現在のモード = EMode.非表示中;
            this.status[i].nCOMBO値 = 0;
            this.status[i].n最高COMBO値 = 0;
            this.status[i].n現在表示中のCOMBO値 = 0;
            this.status[i].n残像表示中のCOMBO値 = 0;
            this.status[i].nジャンプインデックス値 = 99999;
            this.status[i].n前回の時刻_ジャンプ用 = -1;
            this.status[i].nコンボが切れた時刻 = -1;
            this.ctコンボ加算[i] = new CCounter(0, 12, 12, TJAPlayer3.app.Timer);
        }
        this.ctコンボラメ = new CCounter(0, 29, 20, TJAPlayer3.app.Timer);
        base.On活性化();
    }
    public override void On非活性化()
    {
        if (this.status is not null)
            this.status = null;

        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (this.b活性化してない)
            return 0;

        for (int i = 0; i < 4; i++)
        {
            EEvent e今回の状態遷移イベント;

            #region [ 前回と今回の COMBO 値から、e今回の状態遷移イベントを決定する。]
            //-----------------
            if (this.status[i].n現在表示中のCOMBO値 == this.status[i].nCOMBO値)
            {
                e今回の状態遷移イベント = EEvent.同一数値;
            }
            else if (this.status[i].n現在表示中のCOMBO値 > this.status[i].nCOMBO値)
            {
                e今回の状態遷移イベント = EEvent.ミス通知;
            }
            else if ((this.status[i].n現在表示中のCOMBO値 < TJAPlayer3.app.ConfigToml.Game.DispMinCombo) && (this.status[i].nCOMBO値 < TJAPlayer3.app.ConfigToml.Game.DispMinCombo))
            {
                e今回の状態遷移イベント = EEvent.非表示;
            }
            else
            {
                e今回の状態遷移イベント = EEvent.数値更新;
            }
            //-----------------
            #endregion

            #region [ nジャンプインデックス値 の進行。]
            //-----------------
            if (this.status[i].nジャンプインデックス値 < 360)
            {
                if ((this.status[i].n前回の時刻_ジャンプ用 == -1) || (TJAPlayer3.app.Timer.n現在時刻ms < this.status[i].n前回の時刻_ジャンプ用))
                    this.status[i].n前回の時刻_ジャンプ用 = TJAPlayer3.app.Timer.n現在時刻ms;

                const long INTERVAL = 2;
                while ((TJAPlayer3.app.Timer.n現在時刻ms - this.status[i].n前回の時刻_ジャンプ用) >= INTERVAL)
                {
                    if (this.status[i].nジャンプインデックス値 < 2000)
                        this.status[i].nジャンプインデックス値 += 3;

                    this.status[i].n前回の時刻_ジャンプ用 += INTERVAL;
                }
            }
        //-----------------
        #endregion


        Retry:	// モードが変化した場合はここからリトライする。

            switch (this.status[i].e現在のモード)
            {
                case EMode.非表示中:
                    #region [ *** ]
                    //-----------------

                    if (e今回の状態遷移イベント == EEvent.数値更新)
                    {
                        // モード変更
                        this.status[i].e現在のモード = EMode.進行表示中;
                        this.status[i].nジャンプインデックス値 = 0;
                        this.status[i].n前回の時刻_ジャンプ用 = TJAPlayer3.app.Timer.n現在時刻ms;
                        goto Retry;
                    }

                    this.status[i].n現在表示中のCOMBO値 = this.status[i].nCOMBO値;
                    break;
                //-----------------
                #endregion

                case EMode.進行表示中:
                    #region [ *** ]
                    //-----------------

                    if ((e今回の状態遷移イベント == EEvent.非表示) || (e今回の状態遷移イベント == EEvent.ミス通知))
                    {
                        // モード変更
                        this.status[i].e現在のモード = EMode.残像表示中;
                        this.status[i].n残像表示中のCOMBO値 = this.status[i].n現在表示中のCOMBO値;
                        this.status[i].nコンボが切れた時刻 = TJAPlayer3.app.Timer.n現在時刻ms;
                        goto Retry;
                    }

                    if (e今回の状態遷移イベント == EEvent.数値更新)
                    {
                        this.status[i].nジャンプインデックス値 = 0;
                        this.status[i].n前回の時刻_ジャンプ用 = TJAPlayer3.app.Timer.n現在時刻ms;
                    }

                    this.status[i].n現在表示中のCOMBO値 = this.status[i].nCOMBO値;
                    switch (i)
                    {
                        case 0:
                        case 1:
                            this.tコンボ表示_太鼓(this.status[i].nCOMBO値, this.status[i].nジャンプインデックス値, i);
                            break;
                    }
                    break;
                //-----------------
                #endregion

                case EMode.残像表示中:
                    #region [ *** ]
                    //-----------------
                    if (e今回の状態遷移イベント == EEvent.数値更新)
                    {
                        // モード変更１
                        this.status[i].e現在のモード = EMode.進行表示中;
                        goto Retry;
                    }
                    if ((TJAPlayer3.app.Timer.n現在時刻ms - this.status[i].nコンボが切れた時刻) > 1000)
                    {
                        // モード変更２
                        this.status[i].e現在のモード = EMode.非表示中;
                        goto Retry;
                    }
                    this.status[i].n現在表示中のCOMBO値 = this.status[i].nCOMBO値;
                    break;
                    //-----------------
                    #endregion
            }
        }

        return 0;
    }
}
