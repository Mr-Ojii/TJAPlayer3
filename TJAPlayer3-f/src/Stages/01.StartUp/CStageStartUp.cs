namespace TJAPlayer3;

internal class CStageStartUp : CStage
{
    // コンストラクタ

    public CStageStartUp()
    {
        base.eStageID = CStage.EStage.StartUp;
        this.list進行文字列 = new();
    }

    public List<string> list進行文字列;

    // CStage 実装

    public override void On活性化()
    {
        Trace.TraceInformation("起動ステージを活性化します。");
        Trace.Indent();
        try
        {
            this.list進行文字列.Clear();
            base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
            base.On活性化();
            Trace.TraceInformation("起動ステージの活性化を完了しました。");
        }
        finally
        {
            Trace.Unindent();
        }
    }
    public override void On非活性化()
    {
        Trace.TraceInformation("起動ステージを非活性化します。");
        Trace.Indent();
        try
        {
            this.list進行文字列.Clear();
            //2021.01.03 曲リストが生成されてからここに突入するので、esのAbortを削除
            base.On非活性化();
            Trace.TraceInformation("起動ステージの非活性化を完了しました。");
        }
        finally
        {
            Trace.Unindent();
        }
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (base.b初めての進行描画)
            {
                this.list進行文字列.Add("DTXManiaXG Ver.K powered by YAMAHA Silent Session Drums\n");
                this.list進行文字列.Add("Product by.kairera0467\n");
                this.list進行文字列.Add("Release: " + TJAPlayer3.VERSION);

                this.list進行文字列.Add("");
                this.list進行文字列.Add("TJAPlayer3-f forked TJAPlayer3 (Aioilight)");
                this.list進行文字列.Add("TJAPlayer3-f edited by Mr-Ojii(@Mr_Ojii)");
                this.list進行文字列.Add("");

                es = new CEnumSongs();
                es.StartEnumFromCache();										// 曲リスト取得(別スレッドで実行される)
                base.b初めての進行描画 = false;
                return 0;
            }
            #region [ this.str現在進行中 の決定 ]
            //-----------------
            switch (base.eフェーズID)
            {
                case CStage.Eフェーズ.起動0_システムサウンドを構築:
                    this.str現在進行中 = "SYSTEM SOUND...";
                    break;

                case CStage.Eフェーズ.起動00_songlistから曲リストを作成する:
                    this.str現在進行中 = "SONG LIST...";
                    break;

                case CStage.Eフェーズ.起動1_SongsDBからスコアキャッシュを構築:
                    this.str現在進行中 = "SONG DATABASE...";
                    break;

                case CStage.Eフェーズ.起動2_曲を検索してリストを作成する:
                    this.str現在進行中 = string.Format("{0} ... {1}", "Enumerating songs", es.SongsManager.n検索されたスコア数);
                    break;

                case CStage.Eフェーズ.起動3_スコアキャッシュをリストに反映する:
                    this.str現在進行中 = string.Format("{0} ... {1}/{2}", "Loading score properties from songs.db", es.SongsManager.nスコアキャッシュから反映できたスコア数, es.SongsManager.n検索されたスコア数);
                    break;

                case CStage.Eフェーズ.起動4_スコアキャッシュになかった曲をファイルから読み込んで反映する:
                    this.str現在進行中 = string.Format("{0} ... {1}/{2}", "Loading score properties from files", es.SongsManager.nファイルから反映できたスコア数, es.SongsManager.n検索されたスコア数 - es.SongsManager.nスコアキャッシュから反映できたスコア数);
                    break;

                case CStage.Eフェーズ.起動5_曲リストへ後処理を適用する:
                    this.str現在進行中 = string.Format("{0} ... ", "Building songlists");
                    break;

                case CStage.Eフェーズ.起動6_スコアキャッシュをSongsDBに出力する:
                    this.str現在進行中 = string.Format("{0} ... ", "Saving songs.db");
                    break;

                case CStage.Eフェーズ.起動7_完了:
                    this.list進行文字列.Add("LOADING TEXTURES...");
                    TJAPlayer3.app.Tx.LoadTexture();
                    this.list進行文字列.Add("LOADING TEXTURES...OK");
                    this.str現在進行中 = "Setup done.";
                    break;
            }
            //-----------------
            #endregion
            #region [ this.list進行文字列＋this.現在進行中 の表示 ]
            //-----------------
            lock (this.list進行文字列)
            {
                int x = 320;
                int y = 20;
                foreach (string str in this.list進行文字列)
                {
                    TJAPlayer3.app.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, str);
                    y += 24;
                }
                TJAPlayer3.app.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, this.str現在進行中);
            }
            //-----------------
            #endregion

            if (es is not null && es.IsSongListEnumCompletelyDone && TJAPlayer3.app.Tx.IsLoaded)							// 曲リスト作成が終わったら
            {
                TJAPlayer3.SongsManager = es.SongsManager;		// 最後に、曲リストを拾い上げる
                return 1;
            }
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    private string str現在進行中 = "";
    private CEnumSongs es;
    #endregion
}
