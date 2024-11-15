namespace TJAPlayer3;

internal class CEnumSongs							// #27060 2011.2.7 yyagi 曲リストを取得するクラス
{													// ファイルキャッシュ(songslist.db)からの取得と、ディスクからの取得を、この一つのクラスに集約。

    public CSongsManager SongsManager						// 曲の探索結果はこのSongsManagerに読み込まれる
    {
        get;
        private set;
    }

    public bool IsSongListEnumCompletelyDone		// 曲リスト探索と、実際の曲リストへの反映が完了した？
    {
        get
        {
            return (this.state == DTXEnumState.CompletelyDone);
        }
    }
    public bool IsEnumerating
    {
        get
        {
            if (thDTXFileEnumerate is null)
            {
                return false;
            }
            return thDTXFileEnumerate.IsAlive;
        }
    }
    public bool IsSongListEnumerated				// 曲リスト探索が完了したが、実際の曲リストへの反映はまだ？
    {
        get
        {
            return (this.state == DTXEnumState.Enumeratad);
        }
    }
    public bool IsSongListEnumStarted				// 曲リスト探索開始後？(探索完了も含む)
    {
        get
        {
            return (this.state != DTXEnumState.None);
        }
    }
    public void SongListEnumCompletelyDone()
    {
        this.state = DTXEnumState.CompletelyDone;
        this.SongsManager = null;						// GCはOSに任せる
    }

    private readonly string strPathSongList = TJAPlayer3.strEXEのあるフォルダ + "songlist.json";

    public Thread thDTXFileEnumerate
    {
        get;
        private set;
    }
    private enum DTXEnumState
    {
        None,
        Ongoing,
        Suspended,
        Enumeratad,				// 探索完了、現在の曲リストに未反映
        CompletelyDone			// 探索完了、現在の曲リストに反映完了
    }
    private DTXEnumState state = DTXEnumState.None;


    /// <summary>
    /// Constractor
    /// </summary>
    public CEnumSongs()
    {
        this.SongsManager = new CSongsManager();
    }

    /// <summary>
    /// 曲リストのキャッシュ(songlist.db)取得スレッドの開始
    /// </summary>
    public void StartEnumFromCache()
    {
        this.thDTXFileEnumerate = new Thread(new ThreadStart(this.t曲リストの構築1));
        this.thDTXFileEnumerate.Name = "曲リストの構築";
        this.thDTXFileEnumerate.IsBackground = true;
        this.thDTXFileEnumerate.Start();
    }

    /// <summary>
    /// 曲検索スレッドの開始
    /// </summary>
    public void StartEnumFromDisk()
    {
        if (state == DTXEnumState.None || state == DTXEnumState.CompletelyDone)
        {
            Trace.TraceInformation("★曲データ検索スレッドを起動しました。");
            lock (this)
            {
                state = DTXEnumState.Ongoing;
            }
            // this.autoReset = new AutoResetEvent( true );

            if (this.SongsManager is null)		// Enumerating Songs完了後、CONFIG画面から再スキャンしたときにこうなる
            {
                this.SongsManager = new CSongsManager();
            }
            this.thDTXFileEnumerate = new Thread(new ThreadStart(this.t曲リストの構築2));
            this.thDTXFileEnumerate.Name = "曲リストの構築";
            this.thDTXFileEnumerate.IsBackground = true;
            this.thDTXFileEnumerate.Priority = ThreadPriority.Normal;
            this.thDTXFileEnumerate.Start();
        }
    }


    /// <summary>
    /// 曲探索スレッドのサスペンド
    /// </summary>
    public void Suspend()
    {
        if (this.state != DTXEnumState.CompletelyDone &&
            ((thDTXFileEnumerate.ThreadState & (System.Threading.ThreadState.Background)) != 0))
        {
            // this.thDTXFileEnumerate.Suspend();		// obsoleteにつき使用中止
            this.SongsManager.bIsSuspending = true;
            this.state = DTXEnumState.Suspended;
            Trace.TraceInformation("★曲データ検索スレッドを中断しました。");
        }
    }

    /// <summary>
    /// 曲探索スレッドのレジューム
    /// </summary>
    public void Resume()
    {
        if (this.state == DTXEnumState.Suspended)
        {
            if ((this.thDTXFileEnumerate.ThreadState & (System.Threading.ThreadState.WaitSleepJoin | System.Threading.ThreadState.StopRequested)) != 0)	//
            {
                // this.thDTXFileEnumerate.Resume();	// obsoleteにつき使用中止
                this.SongsManager.bIsSuspending = false;
                this.SongsManager.autoReset.Set();
                this.state = DTXEnumState.Ongoing;
                Trace.TraceInformation("★曲データ検索スレッドを再開しました。");
            }
        }
    }

    /// <summary>
    /// 曲探索スレッドにサスペンド指示を出してから、本当にサスペンド状態に遷移するまでの間、ブロックする
    /// 500ms * 10回＝5秒でタイムアウトし、サスペンド完了して無くてもブロック解除する
    /// </summary>
    public void WaitUntilSuspended()
    {
        // 曲検索が一時中断されるまで待機
        for (int i = 0; i < 10; i++)
        {
            if (this.state == DTXEnumState.CompletelyDone ||
                (thDTXFileEnumerate.ThreadState & (System.Threading.ThreadState.WaitSleepJoin | System.Threading.ThreadState.Background | System.Threading.ThreadState.Stopped)) != 0)
            {
                break;
            }
            Trace.TraceInformation("★曲データ検索スレッドの中断待ちです: {0}", this.thDTXFileEnumerate.ThreadState.ToString());
            Thread.Sleep(500);
        }

    }

    /// <summary>
    /// songlist.dbからの曲リスト構築
    /// </summary>
    public void t曲リストの構築1()
    {
        // ！注意！
        // 本メソッドは別スレッドで動作するが、プラグイン側でカレントディレクトリを変更しても大丈夫なように、
        // すべてのファイルアクセスは「絶対パス」で行うこと。(2010.9.16)
        // 構築が完了したら、DTXEnumerateState state を DTXEnumerateState.Done にすること。(2012.2.9)
        DateTime now = DateTime.Now;

        try
        {
            #region [ 0) システムサウンドの構築  ]
            //-----------------------------
            TJAPlayer3.stageStartUp.eフェーズID = CStage.Eフェーズ.起動0_システムサウンドを構築;

            Trace.TraceInformation("0) システムサウンドを構築します。");
            Trace.Indent();

            try
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM起動画面].t再生する();
                for (int i = 0; i < TJAPlayer3.app.Skin.nシステムサウンド数; i++)
                {
                    if (!TJAPlayer3.app.Skin[i].b排他)	// BGM系以外のみ読み込む。(BGM系は必要になったときに読み込む)
                    {
                        CSkin.Cシステムサウンド cシステムサウンド = TJAPlayer3.app.Skin[i];
                        try
                        {
                            cシステムサウンド.tLoad();
                            Trace.TraceInformation("システムサウンドを読み込みました。({0})", cシステムサウンド.strFilename);
                            //if ( ( cシステムサウンド == CDTXMania.Skin.bgm起動画面 ) && cシステムサウンド.b読み込み成功 )
                            //{
                            //	cシステムサウンド.t再生する();
                            //}
                        }
                        catch (FileNotFoundException)
                        {
                            Trace.TraceWarning("システムサウンドが存在しません。({0})", cシステムサウンド.strFilename);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceWarning(e.ToString());
                            Trace.TraceWarning("システムサウンドの読み込みに失敗しました。({0})", cシステムサウンド.strFilename);
                        }
                    }
                }
                lock (TJAPlayer3.stageStartUp.list進行文字列)
                {
                    TJAPlayer3.stageStartUp.list進行文字列.Add("SYSTEM SOUND...OK");
                }
            }
            finally
            {
                Trace.Unindent();
            }
            //-----------------------------
            #endregion

            #region [ 00) songlist.dbの読み込みによる曲リストの構築  ]
            //-----------------------------
            TJAPlayer3.stageStartUp.eフェーズID = CStage.Eフェーズ.起動00_songlistから曲リストを作成する;

            Trace.TraceInformation("1) songlist.dbを読み込みます。");
            Trace.Indent();

            try
            {
                if (!TJAPlayer3.app.ConfigToml.NotExistOrIncorrectVersion)
                {
                    CSongsManager s = new CSongsManager();
                    s = Deserialize(strPathSongList);		// 直接this.SongsManagerにdeserialize()結果を代入するのは避ける。nullにされてしまうことがあるため。
                    if (s is not null)
                    {
                        this.SongsManager = s;
                    }

                    int scores = this.SongsManager.n検索されたスコア数;
                    Trace.TraceInformation("songlist.db の読み込みを完了しました。[{0}スコア]", scores);
                    lock (TJAPlayer3.stageStartUp.list進行文字列)
                    {
                        TJAPlayer3.stageStartUp.list進行文字列.Add("SONG LIST...OK");
                    }
                }
                else
                {
                    Trace.TraceInformation("初回の起動であるかまたはDTXManiaのバージョンが上がったため、songlist.db の読み込みをスキップします。");
                    lock (TJAPlayer3.stageStartUp.list進行文字列)
                    {
                        TJAPlayer3.stageStartUp.list進行文字列.Add("SONG LIST...SKIPPED");
                    }
                }
            }
            finally
            {
                Trace.Unindent();
            }

            #endregion
        }
        finally
        {
            TJAPlayer3.stageStartUp.eフェーズID = CStage.Eフェーズ.起動7_完了;
            TimeSpan span = (TimeSpan)(DateTime.Now - now);
            Trace.TraceInformation("起動所要時間: {0}", span.ToString());
            lock (this)							// #28700 2012.6.12 yyagi; state change must be in finally{} for exiting as of compact mode.
            {
                state = DTXEnumState.CompletelyDone;
            }
        }
    }


    /// <summary>
    /// 起動してタイトル画面に遷移した後にバックグラウンドで発生させる曲検索
    /// #27060 2012.2.6 yyagi
    /// </summary>
    private void t曲リストの構築2()
    {
        // ！注意！
        // 本メソッドは別スレッドで動作するが、プラグイン側でカレントディレクトリを変更しても大丈夫なように、
        // すべてのファイルアクセスは「絶対パス」で行うこと。(2010.9.16)
        // 構築が完了したら、DTXEnumerateState state を DTXEnumerateState.Done にすること。(2012.2.9)

        DateTime now = DateTime.Now;

        try
        {

            #region [ 2) 曲データの検索 ]
            //-----------------------------
            //	base.eフェーズID = CStage.Eフェーズ.起動2_曲を検索してリストを作成する;

            Trace.TraceInformation("enum2) 曲データを検索します。");
            Trace.Indent();

            try
            {
                if (TJAPlayer3.app.ConfigToml.General.ChartPath.Length > 0)
                {
                    // 全パスについて…
                    foreach (string str in TJAPlayer3.app.ConfigToml.General.ChartPath)
                    {
                        string path = str;
                        if (!Path.IsPathRooted(path))
                        {
                            path = TJAPlayer3.strEXEのあるフォルダ + str;	// 相対パスの場合、絶対パスに直す(2010.9.16)
                        }

                        if (!string.IsNullOrEmpty(path))
                        {
                            Trace.TraceInformation("検索パス: " + path);
                            Trace.Indent();

                            try
                            {
                                this.SongsManager.t曲を検索してリストを作成する(path, true);
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError(e.ToString());
                                Trace.TraceError("An exception has occurred, but processing continues.");
                            }
                            finally
                            {
                                Trace.Unindent();
                            }
                        }
                    }
                }
                else
                {
                    Trace.TraceWarning("曲データの検索パス(ChartPath)の指定がありません。");
                }
            }
            finally
            {
                Trace.TraceInformation("曲データの検索を完了しました。[{0}曲{1}スコア]", this.SongsManager.n検索された曲ノード数, this.SongsManager.n検索されたスコア数);
                Trace.Unindent();
            }
            //	lock ( this.list進行文字列 )
            //	{
            //		this.list進行文字列.Add( string.Format( "{0} ... {1} scores ({2} songs)", "Enumerating songs", this..SongsManager_裏読.n検索されたスコア数, this.SongsManager_裏読.n検索された曲ノード数 ) );
            //	}
            //-----------------------------
            #endregion
            #region [ 5) 曲リストへの後処理の適用 ]
            //-----------------------------
            //					base.eフェーズID = CStage.Eフェーズ.起動5_曲リストへ後処理を適用する;

            Trace.TraceInformation("enum5) 曲リストへの後処理を適用します。");
            Trace.Indent();

            try
            {
                this.SongsManager.t曲リストへ後処理を適用する();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues. (6480ffa0-1cc1-40d4-9cc9-aceeecd0264b)");
            }
            finally
            {
                Trace.TraceInformation("曲リストへの後処理を完了しました。");
                Trace.Unindent();
            }
            //					lock ( this.list進行文字列 )
            //					{
            //						this.list進行文字列.Add( string.Format( "{0} ... OK", "Building songlists" ) );
            //					}
            //-----------------------------
            #endregion

            //				if ( !bSucceededFastBoot )	// songs2.db読み込みに成功したなら、songs2.dbを新たに作らない
            #region [ 7) songlist.db への保存 ]		// #27060 2012.1.26 yyagi
            Trace.TraceInformation("enum7) 曲データの情報を songlist.db へ出力します。");
            Trace.Indent();

            SerializeSongList(this.SongsManager, strPathSongList);
            Trace.TraceInformation("songlist.db への出力を完了しました。");
            Trace.Unindent();
            //-----------------------------
            #endregion
            //				}

        }
        finally
        {
            //				base.eフェーズID = CStage.Eフェーズ.起動7_完了;
            TimeSpan span = (TimeSpan)(DateTime.Now - now);
            Trace.TraceInformation("曲探索所要時間: {0}", span.ToString());
        }
        lock (this)
        {
            // state = DTXEnumState.Done;		// DoneにするのはCDTXMania.cs側にて。
            state = DTXEnumState.Enumeratad;
        }
    }



    /// <summary>
    /// 曲リストのserialize
    /// </summary>
    private static void SerializeSongList(CSongsManager cs, string strPathSongList)
    {
        bool bSucceededSerialize = true;
        try
        {
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
            {
                Converters = { new ColorJsonConverter() },
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                IncludeFields = true,
            };
            File.WriteAllBytes(strPathSongList, JsonSerializer.SerializeToUtf8Bytes(cs, options));
        }
        catch (Exception e)
        {
            bSucceededSerialize = false;
            Trace.TraceError(e.ToString());
            Trace.TraceError("An exception has occurred, but processing continues. (9ad477a4-d922-412c-b87d-e3a49a608e92)");
        }
        finally
        {
            if (!bSucceededSerialize)
            {
                try
                {
                    File.Delete(strPathSongList);	// serializeに失敗したら、songs2.dbファイルを消しておく
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    Trace.TraceError("An exception has occurred, but processing continues. (62860c67-b44f-46f4-b4fc-999c6fe18cce)");
                }
            }
        }
    }

    /// <summary>
    /// 曲リストのdeserialize
    /// </summary>
    /// <param name="SongsManager"></param>
    /// <param name="strPathSongList"></param>
    private CSongsManager Deserialize(string strPathSongList)
    {
        try
        {
            #region [ SongListDB(songlist.db)を読み込む ]

            if (!File.Exists(strPathSongList))
            {
                return null;
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Converters = { new ColorJsonConverter() },
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                    IncludeFields = true,
                };

                CSongsManager tmp = JsonSerializer.Deserialize<CSongsManager>(File.ReadAllBytes(strPathSongList), options);
                親ノードを設定する(ref tmp.list曲ルート, null);
                return tmp;
            }
            catch (Exception e)
            {
                // SongsManager = null;

                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues. (a4289e34-7140-4b67-b821-3b5370a725e1)");
            }
            #endregion
        }
        catch (Exception e)
        {
            Trace.TraceError("songlist.db の読み込みに失敗しました。");
            Trace.TraceError(e.ToString());
            Trace.TraceError("An exception has occurred, but processing continues. (5a907ed2-f849-4bc4-acd0-d2a6aa3c9c87)");
        }
        return null;
    }

    private static void 親ノードを設定する(ref List<C曲リストノード> cs, C曲リストノード parent)
    {
        foreach (C曲リストノード c in cs)
        {
            if (c.eNodeType == C曲リストノード.ENodeType.BOX && c.list子リスト is not null)
            {
                親ノードを設定する(ref c.list子リスト, c);//再帰
            }
            else
            {
                c.r親ノード = parent;
            }
        }
    }

    private class ColorJsonConverter : System.Text.Json.Serialization.JsonConverter<System.Drawing.Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ColorTranslator.FromHtml(reader.GetString());

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) => writer.WriteStringValue("#" + value.A.ToString("X2") + value.R.ToString("X2") + value.G.ToString("X2") + value.B.ToString("X2"));
    }
}
