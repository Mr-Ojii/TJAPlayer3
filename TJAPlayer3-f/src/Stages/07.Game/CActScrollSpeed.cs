using FDK;

namespace TJAPlayer3;

internal class CActScrollSpeed : CActivity
{
    // プロパティ

    public double[] db現在の譜面スクロール速度
    {
        get;
        private set;
    } = new double[2];

    // コンストラクタ

    public CActScrollSpeed()
    {
    }


    // CActivity 実装

    public override void On活性化()
    {
        for (int nPlayer = 0; nPlayer < 2; nPlayer++)
        {
            this.db現在の譜面スクロール速度[nPlayer] = (double)TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer];
            this.n速度変更制御タイマ[nPlayer] = -1;
        }
        base.On活性化();
    }
    public override unsafe int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (CSoundManager.rc演奏用タイマ is null)
                return 0;

            if (base.b初めての進行描画)
            {
                for (int nPlayer = 0; nPlayer < 2; nPlayer++)
                {
                    this.n速度変更制御タイマ[nPlayer] = (long)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0));
                }
                base.b初めての進行描画 = false;
            }
            long n現在時刻 = CSoundManager.rc演奏用タイマ.n現在時刻ms;

            for (int nPlayer = 0; nPlayer < 2; nPlayer++)
            {
                double db譜面スクロールスピード = (double)TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer];
                if (n現在時刻 < this.n速度変更制御タイマ[nPlayer])
                {
                    this.n速度変更制御タイマ[nPlayer] = n現在時刻;
                }
                while ((n現在時刻 - this.n速度変更制御タイマ[nPlayer]) >= 2)                               // 2msに1回ループ
                {
                    if (this.db現在の譜面スクロール速度[nPlayer] < db譜面スクロールスピード)             // Config.iniのスクロール速度を変えると、それに追いつくように実画面のスクロール速度を変える
                    {
                        this.db現在の譜面スクロール速度[nPlayer] += 0.012;

                        if (this.db現在の譜面スクロール速度[nPlayer] > db譜面スクロールスピード)
                        {
                            this.db現在の譜面スクロール速度[nPlayer] = db譜面スクロールスピード;
                        }
                    }
                    else if (this.db現在の譜面スクロール速度[nPlayer] > db譜面スクロールスピード)
                    {
                        this.db現在の譜面スクロール速度[nPlayer] -= 0.012;

                        if (this.db現在の譜面スクロール速度[nPlayer] < db譜面スクロールスピード)
                        {
                            this.db現在の譜面スクロール速度[nPlayer] = db譜面スクロールスピード;
                        }
                    }
                    this.n速度変更制御タイマ[nPlayer] += 2;
                }

            }
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    private long[] n速度変更制御タイマ = new long[2];
    //-----------------
    #endregion
}
