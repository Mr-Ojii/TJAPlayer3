using TJAPlayer3.C曲リストノードComparers;

namespace TJAPlayer3;

[Serializable]
internal class CSongsManager
{
    // プロパティ

    public int nスコアキャッシュから反映できたスコア数
    {
        get;
        set;
    }
    public int nファイルから反映できたスコア数
    {
        get;
        set;
    }
    public int n検索されたスコア数
    {
        get;
        set;
    }
    public int n検索された曲ノード数
    {
        get;
        set;
    }
    public List<C曲リストノード> list曲ルート;         // 起動時にフォルダ検索して構築されるlist
    public bool bIsSuspending                           // 外部スレッドから、内部スレッドのsuspendを指示する時にtrueにする
    {                                                   // 再開時は、これをfalseにしてから、次のautoReset.Set()を実行する
        get;
        set;
    }
    [System.Text.Json.Serialization.JsonIgnore]
    public AutoResetEvent autoReset;

    // コンストラクタ

    public CSongsManager()
    {
        this.list曲ルート = new List<C曲リストノード>();
        this.n検索された曲ノード数 = 0;
        this.n検索されたスコア数 = 0;
        this.bIsSuspending = false;                     // #27060
        this.autoReset = new AutoResetEvent(true);  // #27060
    }


    // メソッド

    #region [ 曲を検索してリストを作成する ]
    //-----------------
    public void t曲を検索してリストを作成する(string str基点フォルダ, bool b子BOXへ再帰する)
    {
        this.t曲を検索してリストを作成する(str基点フォルダ, b子BOXへ再帰する, this.list曲ルート, null);
    }
    private void t曲を検索してリストを作成する(string str基点フォルダ, bool b子BOXへ再帰する, List<C曲リストノード> listノードリスト, C曲リストノード? node親)
    {
        if (!str基点フォルダ.EndsWith(@"/"))
            str基点フォルダ = str基点フォルダ + @"/";

        DirectoryInfo info = new DirectoryInfo(str基点フォルダ);

        if (TJAPlayer3.app.ConfigToml.Log.SongSearch)
            Trace.TraceInformation("基点フォルダ: " + str基点フォルダ);

        #region [ 個別ファイルからノード作成 ]
        //-----------------------------
        foreach (FileInfo fileinfo in info.GetFiles())
        {
            SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす
            string strExt = fileinfo.Extension.ToLowerInvariant();

            if ((strExt.Equals(".tja")) || strExt.Equals(".tcm") || strExt.Equals(".tci"))
            {
                #region[ 新処理 ]
                CDTX dtx = new CDTX(str基点フォルダ + fileinfo.Name, false, 0, 0, false);
                C曲リストノード c曲リストノード = new C曲リストノード();
                c曲リストノード.eNodeType = C曲リストノード.ENodeType.SCORE;

                c曲リストノード.r親ノード = node親;
                c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード is null) ?
                    str基点フォルダ + fileinfo.Name : c曲リストノード.r親ノード.strBreadcrumbs + " > " + str基点フォルダ + fileinfo.Name;

                c曲リストノード.strTitle = dtx.TITLE;
                if (!string.IsNullOrEmpty(dtx.GENRE))
                {
                    c曲リストノード.strGenre = dtx.GENRE;
                }
                else
                {
                    if (c曲リストノード.r親ノード is not null && c曲リストノード.r親ノード.strGenre != "")
                    {
                        // .tjaのジャンルが存在しなくて、かつ親ノードにジャンルが指定されていればそちらを読み込む。
                        c曲リストノード.strGenre = c曲リストノード.r親ノード.strGenre;
                    }
                }

                if (c曲リストノード.r親ノード is not null)
                {
                    c曲リストノード.ForeColor = c曲リストノード.r親ノード.ForeColor;
                    c曲リストノード.BackColor = c曲リストノード.r親ノード.BackColor;
                }

                c曲リストノード.ForeColor = TJAPlayer3.app.Skin.SkinConfig.SongSelect._ForeColor[TJAPlayer3.app.Skin.nStrジャンルtoNum(c曲リストノード.strGenre)];
                c曲リストノード.BackColor = TJAPlayer3.app.Skin.SkinConfig.SongSelect._BackColor[TJAPlayer3.app.Skin.nStrジャンルtoNum(c曲リストノード.strGenre)];

                bool b = false;

                c曲リストノード.arスコア = new Cスコア();
                c曲リストノード.arスコア.FileInfo.FileAbsolutePath = str基点フォルダ + fileinfo.Name;
                c曲リストノード.arスコア.FileInfo.DirAbsolutePath = str基点フォルダ;
                c曲リストノード.arスコア.FileInfo.FileSize = fileinfo.Length;
                c曲リストノード.arスコア.FileInfo.LastWriteTime = fileinfo.LastWriteTime;
                c曲リストノード.arスコア.譜面情報.Title = dtx.TITLE;
                c曲リストノード.arスコア.譜面情報.SubTitle = dtx.SUBTITLE;
                c曲リストノード.arスコア.譜面情報.Genre = dtx.GENRE;
                c曲リストノード.arスコア.譜面情報.Bpm = dtx.BPM;
                c曲リストノード.arスコア.譜面情報.strBGMファイル名 = dtx.strBGM_PATH is null ? "" : dtx.strBGM_PATH;
                c曲リストノード.arスコア.譜面情報.SongVol = dtx.SongVol;
                c曲リストノード.arスコア.譜面情報.SongLoudnessMetadata = dtx.SongLoudnessMetadata;
                c曲リストノード.arスコア.譜面情報.nデモBGMオフセット = dtx.nデモBGMオフセット;
                c曲リストノード.arスコア.譜面情報.b譜面分岐[0] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[0];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[1] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[1];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[2] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[2];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[3] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[3];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[4] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[4];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[5] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[5];
                c曲リストノード.arスコア.譜面情報.b譜面分岐[6] = dtx.bHIDDENBRANCH ? false : dtx.bHasBranch[6];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[0] = dtx.bPapaMamaSupport[0];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[1] = dtx.bPapaMamaSupport[1];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[2] = dtx.bPapaMamaSupport[2];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[3] = dtx.bPapaMamaSupport[3];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[4] = dtx.bPapaMamaSupport[4];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[5] = dtx.bPapaMamaSupport[5];
                c曲リストノード.arスコア.譜面情報.bPapaMamaSupport[6] = dtx.bPapaMamaSupport[6];
                c曲リストノード.arスコア.譜面情報.Level[0] = dtx.LEVELtaiko[0];
                c曲リストノード.arスコア.譜面情報.Level[1] = dtx.LEVELtaiko[1];
                c曲リストノード.arスコア.譜面情報.Level[2] = dtx.LEVELtaiko[2];
                c曲リストノード.arスコア.譜面情報.Level[3] = dtx.LEVELtaiko[3];
                c曲リストノード.arスコア.譜面情報.Level[4] = dtx.LEVELtaiko[4];
                c曲リストノード.arスコア.譜面情報.Level[5] = dtx.LEVELtaiko[5];
                c曲リストノード.arスコア.譜面情報.Level[6] = dtx.LEVELtaiko[6];
                c曲リストノード.arスコア.譜面情報.b歌詞あり = dtx.bLyrics;
                for (int n = 0; n < (int)Difficulty.Total; n++)
                {
                    if (dtx.b譜面が存在する[n])
                    {
                        c曲リストノード.nスコア数++;
                        c曲リストノード.arスコア.譜面情報.b譜面が存在する[n] = true;
                        if (b == false)
                        {
                            this.n検索されたスコア数++;
                            listノードリスト.Add(c曲リストノード);
                            this.n検索された曲ノード数++;
                            b = true;
                        }
                    }
                }

                try
                {
                    var scoreJsonPath = c曲リストノード.arスコア.FileInfo.FileAbsolutePath + ".score.json";
                    if (File.Exists(scoreJsonPath))
                        this.tLoadScoreJsonAndSetScoreInfo(scoreJsonPath, c曲リストノード.arスコア);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    Trace.TraceError("An exception has occurred, but processing continues.");
                }
                #endregion
            }
        }
        //-----------------------------
        #endregion

        foreach (DirectoryInfo infoDir in info.GetDirectories())
        {
            SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

            #region [ b.box.def を含むフォルダの場合  ]
            //-----------------------------
            if (File.Exists(infoDir.FullName + @"/box.def"))
            {
                CBoxDef boxdef = new CBoxDef(infoDir.FullName + @"/box.def");
                C曲リストノード c曲リストノード = new C曲リストノード();
                c曲リストノード.eNodeType = C曲リストノード.ENodeType.BOX;
                c曲リストノード.strTitle = boxdef.Title;
                c曲リストノード.strGenre = boxdef.Genre;

                c曲リストノード.ForeColor = boxdef.ForeColor;
                c曲リストノード.BackColor = boxdef.BackColor;

                c曲リストノード.ForeColor = TJAPlayer3.app.Skin.SkinConfig.SongSelect._ForeColor[TJAPlayer3.app.Skin.nStrジャンルtoNum(c曲リストノード.strGenre)];
                c曲リストノード.BackColor = TJAPlayer3.app.Skin.SkinConfig.SongSelect._BackColor[TJAPlayer3.app.Skin.nStrジャンルtoNum(c曲リストノード.strGenre)];

                c曲リストノード.nスコア数 = 1;
                c曲リストノード.arスコア = new Cスコア();
                c曲リストノード.arスコア.FileInfo.DirAbsolutePath = infoDir.FullName + @"/";
                c曲リストノード.arスコア.譜面情報.Title = boxdef.Title;
                c曲リストノード.arスコア.譜面情報.Genre = boxdef.Genre;
                c曲リストノード.r親ノード = node親;
                c曲リストノード.Openindex = 1;

                c曲リストノード.strBreadcrumbs = (c曲リストノード.r親ノード is null) ?
                    c曲リストノード.strTitle : c曲リストノード.r親ノード.strBreadcrumbs + " > " + c曲リストノード.strTitle;


                c曲リストノード.list子リスト = new List<C曲リストノード>();
                listノードリスト.Add(c曲リストノード);
                if (b子BOXへ再帰する)
                {
                    this.t曲を検索してリストを作成する(infoDir.FullName + @"/", b子BOXへ再帰する, c曲リストノード.list子リスト, c曲リストノード);
                }
            }
            //-----------------------------
            #endregion

            #region [ c.通常フォルダの場合 ]
            //-----------------------------
            else
            {
                this.t曲を検索してリストを作成する(infoDir.FullName + @"/", b子BOXへ再帰する, listノードリスト, node親);
            }
            //-----------------------------
            #endregion
        }
    }
    //-----------------
    #endregion

    #region [ 曲リストへ後処理を適用する ]
    //-----------------
    public void t曲リストへ後処理を適用する()
    {
        this.t曲リストへ後処理を適用する(this.list曲ルート);
    }
    private void t曲リストへ後処理を適用する(List<C曲リストノード> ノードリスト)
    {
        #region [ リストに１つ以上の曲があるなら RANDOM BOX を入れる ]

        //-----------------------------
        if (ノードリスト.Count > 0 && TJAPlayer3.app.ConfigToml.SongSelect.RandomPresence)
        {
            C曲リストノード itemRandom = new C曲リストノード();
            itemRandom.eNodeType = C曲リストノード.ENodeType.RANDOM;
            itemRandom.strTitle = "ランダムに曲をえらぶ";
            itemRandom.nスコア数 = (int)Difficulty.Total;
            itemRandom.r親ノード = ノードリスト[0].r親ノード;

            itemRandom.strBreadcrumbs = (itemRandom.r親ノード is null) ?
                itemRandom.strTitle : itemRandom.r親ノード.strBreadcrumbs + " > " + itemRandom.strTitle;

            itemRandom.arスコア = new Cスコア();
            for (int i = 0; i < (int)Difficulty.Total; i++)
            {
                itemRandom.arスコア.譜面情報.Title = string.Format("< RANDOM SELECT Lv.{0} >", i + 1);
                itemRandom.arスコア.譜面情報.b譜面が存在する[i] = true;
            }
            ノードリスト.Add(itemRandom);

            #region [ ログ出力 ]
            //-----------------------------
            if (TJAPlayer3.app.ConfigToml.Log.SongSearch)
            {
                StringBuilder sb = new StringBuilder(0x100);
                sb.Append(string.Format("nID#{0:D3}", itemRandom.nID));
                if (itemRandom.r親ノード is not null)
                {
                    sb.Append(string.Format("(in#{0:D3}):", itemRandom.r親ノード.nID));
                }
                else
                {
                    sb.Append("(onRoot):");
                }
                sb.Append(" RANDOM");
                Trace.TraceInformation(sb.ToString());
            }
            //-----------------------------
            #endregion
        }
        //-----------------------------
        #endregion

        // すべてのノードについて…
        foreach (C曲リストノード c曲リストノード in ノードリスト)
        {
            SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

            #region [ BOXノードなら子リストに <<BACK を入れ、子リストに後処理を適用する ]
            //-----------------------------
            if (c曲リストノード.eNodeType == C曲リストノード.ENodeType.BOX)
            {
                int 曲数 = c曲リストノード.list子リスト.Count;//for文に直接書くと、もどるもカウントされてしまう。
                for (int index = 0; index < ((曲数 - 1) / TJAPlayer3.app.ConfigToml.SongSelect.BackBoxInterval) + 2; index++)
                {
                    C曲リストノード itemBack = new C曲リストノード();
                    itemBack.eNodeType = C曲リストノード.ENodeType.BACKBOX;
                    itemBack.strTitle = "とじる";
                    itemBack.nスコア数 = 1;
                    itemBack.r親ノード = c曲リストノード;

                    itemBack.strBreadcrumbs = (itemBack.r親ノード is null) ?
                        itemBack.strTitle : itemBack.r親ノード.strBreadcrumbs + " > " + itemBack.strTitle;

                    itemBack.arスコア = new Cスコア();
                    itemBack.arスコア.FileInfo.DirAbsolutePath = "";
                    itemBack.arスコア.譜面情報.Title = itemBack.strTitle;
                    c曲リストノード.arスコア.譜面情報.b譜面が存在する[0] = true;
                    c曲リストノード.list子リスト.Insert(Math.Min(index * (TJAPlayer3.app.ConfigToml.SongSelect.BackBoxInterval + 1), c曲リストノード.list子リスト.Count), itemBack);


                    #region [ ログ出力 ]
                    //-----------------------------
                    if (TJAPlayer3.app.ConfigToml.Log.SongSearch)
                    {
                        StringBuilder sb = new StringBuilder(0x100);
                        sb.Append(string.Format("nID#{0:D3}", itemBack.nID));
                        if (itemBack.r親ノード is not null)
                        {
                            sb.Append(string.Format("(in#{0:D3}):", itemBack.r親ノード.nID));
                        }
                        else
                        {
                            sb.Append("(onRoot):");
                        }
                        sb.Append(" BACKBOX");
                        Trace.TraceInformation(sb.ToString());
                    }
                    //-----------------------------
                    #endregion
                }

                this.t曲リストへ後処理を適用する(c曲リストノード.list子リスト);
                continue;
            }
            //-----------------------------
            #endregion

            #region [ ノードにタイトルがないなら、最初に見つけたスコアのタイトルを設定する ]
            //-----------------------------
            if (string.IsNullOrEmpty(c曲リストノード.strTitle))
            {
                for (int j = 0; j < (int)Difficulty.Total; j++)
                {
                    if ((c曲リストノード.arスコア.譜面情報.b譜面が存在する[j] != false) && !string.IsNullOrEmpty(c曲リストノード.arスコア.譜面情報.Title))
                    {
                        c曲リストノード.strTitle = c曲リストノード.arスコア.譜面情報.Title;

                        if (TJAPlayer3.app.ConfigToml.Log.SongSearch)
                            Trace.TraceInformation("タイトルを設定しました。(nID#{0:D3}, title={1})", c曲リストノード.nID, c曲リストノード.strTitle);

                        break;
                    }
                }
            }
            //-----------------------------
            #endregion
        }

        #region [ ノードをソートする ]
        //-----------------------------
        if (TJAPlayer3.app.ConfigToml.SongSelect.DefaultSongSort == 0)
        {
            t曲リストのソート1_絶対パス順(ノードリスト);
        }
        else if (TJAPlayer3.app.ConfigToml.SongSelect.DefaultSongSort == 1)
        {
            t曲リストのソート9_ジャンル順(ノードリスト, 0, 0);
        }
        //-----------------------------
        #endregion
    }
    //-----------------
    #endregion

    #region [ 曲リストソート ]
    //-----------------

    public static void t曲リストのソート1_絶対パス順(List<C曲リストノード> ノードリスト)
    {
        t曲リストのソート1_絶対パス順(ノードリスト, 1, 0);

        foreach (C曲リストノード c曲リストノード in ノードリスト)
        {
            if ((c曲リストノード.list子リスト is not null) && (c曲リストノード.list子リスト.Count > 1))
            {
                t曲リストのソート1_絶対パス順(c曲リストノード.list子リスト);
            }
        }
    }

    public static void t曲リストのソート1_絶対パス順(List<C曲リストノード> ノードリスト, int order, params object[] p)
    {
        var comparer = new ComparerChain<C曲リストノード>(
            new C曲リストノードComparerNodeType(),
            new C曲リストノードComparer絶対パス(order),
            new C曲リストノードComparerタイトル(order));

        ノードリスト.Sort(comparer);
        tとじるノードの等間隔追加(ノードリスト);
    }

    public static void t曲リストのソート2_タイトル順(List<C曲リストノード> ノードリスト, int order, params object[] p)
    {
        var comparer = new ComparerChain<C曲リストノード>(
            new C曲リストノードComparerNodeType(),
            new C曲リストノードComparerタイトル(order),
            new C曲リストノードComparer絶対パス(order));

        ノードリスト.Sort(comparer);
        tとじるノードの等間隔追加(ノードリスト);
    }

    public static void t曲リストのソート9_ジャンル順(List<C曲リストノード> ノードリスト, int order, params object[] p)
    {
        try
        {
            var comparer = new ComparerChain<C曲リストノード>(
                new C曲リストノードComparerNodeType(),
                new C曲リストノードComparerGenre(order),
                new C曲リストノードComparer絶対パス(1),
                new C曲リストノードComparerタイトル(1));

            ノードリスト.Sort(comparer);
            tとじるノードの等間隔追加(ノードリスト);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            Trace.TraceError("An exception has occurred, but processing continues. (bca6dda7-76ad-42fc-a415-250f52c0b17d)");
        }
    }

    public static void tとじるノードの等間隔追加(List<C曲リストノード> ノードリスト)
    {
        int 戻るノード数 = 0;
        for (int index = 0; index < ノードリスト.Count; index++)
        {
            if (ノードリスト[index].eNodeType == C曲リストノード.ENodeType.BACKBOX)
            {
                C曲リストノード tmp = new C曲リストノード();//今、入ってるBACKBOXを使いまわす。
                tmp = ノードリスト[index];
                ノードリスト.RemoveAt(index);
                ノードリスト.Insert(Math.Min((戻るノード数) * (TJAPlayer3.app.ConfigToml.SongSelect.BackBoxInterval + 1), ノードリスト.Count), tmp);
                戻るノード数++;
            }
        }
    }

    //-----------------
    #endregion
    #region [ .score.json を読み込んで Cスコア.譜面情報に設定する ]
    //-----------------
    public void tLoadScoreJsonAndSetScoreInfo(string ScoreJsonPath, Cスコア score)
    {
        if (!File.Exists(ScoreJsonPath))
            return;

        try
        {
            var json = CScoreJson.Load(ScoreJsonPath);

            for (int i = 0; i < (int)Difficulty.Total; i++)
            {
                for (int j = 0; j < json.Records[i].HiScore.Count; j++)
                {
                    score.譜面情報.nHiScore[i][j] = (int)json.Records[i].HiScore[j].Score;
                    score.譜面情報.strHiScorerName[i][j] = json.Records[i].HiScore[j].PlayerName;
                }
                score.譜面情報.nCrown[i] = json.Records[i].Crown;
            }
        }
        catch (Exception e)
        {
            Trace.TraceError("演奏記録ファイルの読み込みに失敗しました。[{0}]", ScoreJsonPath);
            Trace.TraceError(e.ToString());
            Trace.TraceError("An exception has occurred, but processing continues. (801f823d-a952-4809-a1bb-cf6a56194f5c)");
        }
    }
    //-----------------
    #endregion

    // その他

    #region [ private ]
    //-----------------
    /// <summary>
    /// 検索を中断_スローダウンする
    /// </summary>
    private void SlowOrSuspendSearchTask()
    {
        if (this.bIsSuspending)     // #27060 中断要求があったら、解除要求が来るまで待機
        {
            autoReset.WaitOne();
        }
    }

    //-----------------
    #endregion
}
