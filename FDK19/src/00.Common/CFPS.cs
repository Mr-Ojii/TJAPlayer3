namespace FDK;

public class CFPS
{
    // プロパティ

    public int nFPS
    {
        get;
        private set;
    }
    public bool bChangedFPSValue
    {
        get;
        private set;
    }


    // コンストラクタ

    public CFPS()
    {
        this.nFPS = 0;
        this.timer = new CTimer();
        this.nBaseTimems = this.timer.n現在時刻ms;
        this.nLocalFPS = 0;
        this.bChangedFPSValue = false;
    }


    // メソッド

    public void tUpdateCounter()
    {
        this.timer.t更新();
        this.bChangedFPSValue = false;

        const long INTERVAL = 1000;
        while ((this.timer.n現在時刻ms - this.nBaseTimems) >= INTERVAL)
        {
            this.nFPS = this.nLocalFPS;
            this.nLocalFPS = 0;
            this.bChangedFPSValue = true;
            this.nBaseTimems += INTERVAL;
        }
        this.nLocalFPS++;
    }


    // その他

    #region [ private ]
    //-----------------
    private CTimer timer;
    private long nBaseTimems;
    private int nLocalFPS;
    //-----------------
    #endregion
}
