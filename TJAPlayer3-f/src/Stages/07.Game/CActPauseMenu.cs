using FDK;

namespace TJAPlayer3;

internal class CActPauseMenu : CActSelectPopupMenu
{
    private readonly string QuickCfgTitle = "Pause";
    // コンストラクタ

    public CActPauseMenu()
    {
        CAct演奏PauseMenuMain();
    }

    private void CAct演奏PauseMenuMain()
    {
        this.bEsc有効 = false;
        lci = new List<List<CItemBase>>();									// この画面に来る度に、メニューを作り直す。
        for (int nConfSet = 0; nConfSet < 3; nConfSet++)
        {
            lci.Add(new List<CItemBase>());									// ConfSet用の3つ分の枠。

            lci[nConfSet].Add(null);										// Drum/Guitar/Bassで3つ分、枠を作っておく
            lci[nConfSet] = MakeListCItemBase(nConfSet);

        }
        base.Initialize(lci[nCurrentConfigSet], true, QuickCfgTitle, 2);	// ConfSet=0, nInst=Drums
    }

    private List<CItemBase> MakeListCItemBase(int nConfigSet)
    {
        List<CItemBase> l = new List<CItemBase>();

        #region [ 共通 SET切り替え/More/Return ]
        l.Add(new CItemList("続ける", 0, "", "", new string[] { "" }));
        l.Add(new CItemList("やり直し", 0, "", "", new string[] { "" }));
        l.Add(new CItemList("演奏中止", 0, "", "", new string[] { "", "" }));
        #endregion

        return l;
    }

    // メソッド
    public override void tActivatePopupMenu(int nPlayer)
    {
        this.CAct演奏PauseMenuMain();
        this.選択完了 = false;
        base.bキー入力待ち = true;
        base.tActivatePopupMenu(0);
    }
    //public void tDeativatePopupMenu()
    //{
    //	base.tDeativatePopupMenu();
    //}
    public void t選択後()
    {
        if (this.選択完了)
        {
            if (!sw.IsRunning)
                this.sw = Stopwatch.StartNew();
            if (sw.ElapsedMilliseconds > 1500)
            {
                switch (選択した行)
                {
                    case (int)EOrder.Continue:
                        TJAPlayer3.stage演奏ドラム画面.bPAUSE = false;

                        CSoundManager.rc演奏用タイマ.t再開();
                        TJAPlayer3.app.Timer.t再開();
                        TJAPlayer3.DTX[0].t全チップの再生再開();
                        TJAPlayer3.stage演奏ドラム画面.actAVI.tPauseControl();
                        break;

                    case (int)EOrder.Redoing:
                        TJAPlayer3.stage演奏ドラム画面.bPAUSE = false;
                        TJAPlayer3.stage演奏ドラム画面.t演奏やりなおし();
                        break;

                    case (int)EOrder.Return:
                        CSoundManager.rc演奏用タイマ.t再開();
                        TJAPlayer3.app.Timer.t再開();
                        TJAPlayer3.stage演奏ドラム画面.t演奏中止();
                        break;
                    default:
                        break;
                }
                this.tDeativatePopupMenu();
                sw.Stop();
                sw.Reset();
                this.選択完了 = false;
            }
        }
    }
    public override void t進行描画sub()
    {
    }

    public override void tPushedEnterMain(int nSortOrder)
    {
        if (!this.選択完了)
        {
            this.選択した行 = n現在の選択行;
            this.選択完了 = true;
            base.bキー入力待ち = false;
        }
    }

    public override void tCancel()
    {
    }

    // CActivity 実装

    public override void On活性化()
    {
        base.On活性化();
        this.sw = new Stopwatch();
        this.bGotoDetailConfig = false;
        base.bキー入力待ち = true;
    }
    public override void On非活性化()
    {
        base.On非活性化();
    }

    #region [ private ]
    //-----------------
    private int nCurrentConfigSet = 0;
    private List<List<CItemBase>> lci;
    private enum EOrder : int
    {
        Continue,
        Redoing,
        Return, END,
        Default = 99
    };

    private bool 選択完了;
    private int 選択した行;
    private Stopwatch sw;
    //-----------------
    #endregion
}
