using FDK;

namespace TJAPlayer3;

internal class CAct演奏AVI : CActivity
{
    // コンストラクタ

    public CAct演奏AVI()
    {
    }


    // メソッド

    public void Start(CVideoDecoder? rVD)
    {
        if (!TJAPlayer3.app.ConfigToml.Game.Background.Movie)
            return;

        this.rVD = rVD;
        if (this.rVD is not null)
        {
            this.ratio1 = Math.Min((float)TJAPlayer3.app.LogicalSize.Height / ((float)this.rVD.FrameSize.Height), (float)TJAPlayer3.app.LogicalSize.Width / ((float)this.rVD.FrameSize.Height));

            this.rVD.Start();
        }
    }
    public void Seek(int ms) => this.rVD?.Seek(ms);

    public void Stop() => this.rVD?.Stop();

    public void tPauseControl() => this.rVD?.PauseControl();

    public unsafe int t進行描画()
    {
        if (!base.b活性化してない && this.rVD is not null)
        {
            this.rVD.GetNowFrame(ref this.tx描画用);

            if (this.tx描画用 is null)
                return 0;
            this.tx描画用.vcScaling.X = this.ratio1;
            this.tx描画用.vcScaling.Y = this.ratio1;

            if (TJAPlayer3.app.ConfigToml.Game.Background._ClipDispType.HasFlag(EClipDispType.Background))
            {
                this.tx描画用.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Center, TJAPlayer3.app.LogicalSize.Width / 2, TJAPlayer3.app.LogicalSize.Height / 2);
            }
        }
        return 0;
    }

    public void t窓表示()
    {
        if (this.rVD is null || this.tx描画用 is null || !TJAPlayer3.app.ConfigToml.Game.Background._ClipDispType.HasFlag(EClipDispType.Window))
            return;

        float[] fRatio = new float[] { 640.0f - 4.0f, 360.0f - 4.0f }; //中央下表示

        float ratio = Math.Min((float)(fRatio[0] / this.rVD.FrameSize.Width), (float)(fRatio[1] / this.rVD.FrameSize.Height));
        this.tx描画用.vcScaling.X = ratio;
        this.tx描画用.vcScaling.Y = ratio;

        this.tx描画用.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, TJAPlayer3.app.LogicalSize.Width / 2, TJAPlayer3.app.LogicalSize.Height);
    }

    // CActivity 実装

    public override void On活性化()
    {
        base.On活性化();
    }
    public override void On非活性化()
    {
        if (this.tx描画用 is not null)
        {
            this.tx描画用.Dispose();
            this.tx描画用 = null;
        }
        base.On非活性化();
    }
    public override int On進行描画()
    {
        throw new InvalidOperationException("t進行描画(int,int)のほうを使用してください。");
    }


    // その他

    #region [ private ]
    //-----------------
    private float ratio1;

    private CTexture? tx描画用;

    public CVideoDecoder? rVD;

    //-----------------
    #endregion
}
