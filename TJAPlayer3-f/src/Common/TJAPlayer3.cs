using FDK;
using SkiaSharp;

namespace TJAPlayer3;

internal class TJAPlayer3 : Game
{
    // プロパティ
    #region [ properties ]
    public static string VERSION
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version is null)
                return "null";

            return version.ToString();
        }
    }

    public static TJAPlayer3 app
    {
        get;
        private set;
    }
    public C文字コンソール act文字コンソール
    {
        get;
        private set;
    }
    public CConfigIni ConfigIni
    {
        get;
        private set;
    }
    public CConfigToml ConfigToml
    {
        get;
        private set;
    }
    public static CDTX[] DTX
    {
        get
        {
            return dtx;
        }
        set
        {
            for (int nPlayer = 0; nPlayer < 2; nPlayer++)
            {
                if ((dtx[nPlayer] is not null) && (app is not null))
                {
                    dtx[nPlayer].On非活性化();
                    app.listトップレベルActivities.Remove(dtx[nPlayer]);
                }
            }
            dtx = value;
            for (int nPlayer = 0; nPlayer < 2; nPlayer++)
            {
                if ((dtx[nPlayer] is not null) && (app is not null))
                {
                    app.listトップレベルActivities.Add(dtx[nPlayer]);
                }
            }
        }
    }

    public static bool IsPerformingCalibration;

    public CFPS FPS
    {
        get;
        private set;
    }

    public CInputManager InputManager
    {
        get;
        private set;
    }
    public CPad Pad
    {
        get;
        private set;
    }
    public CSkin Skin
    {
        get;
        private set;
    }
    public static CSongsManager SongsManager
    {
        get;
        set;    // 2012.1.26 yyagi private解除 CStage起動でのdesirialize読み込みのため
    }
    public static CEnumSongs EnumSongs
    {
        get;
        private set;
    }
    public static CActEnumSongs actEnumSongs
    {
        get;
        private set;
    }
    public static CActScanningLoudness actScanningLoudness
    {
        get;
        private set;
    }

    public static CSoundManager SoundManager
    {
        get;
        private set;
    }

    public static SongGainController SongGainController
    {
        get;
        private set;
    }

    public static SoundGroupLevelController SoundGroupLevelController
    {
        get;
        private set;
    }

    public static CStageStartUp stageStartUp
    {
        get;
        private set;
    }
    public static CStageTitle stageTitle
    {
        get;
        private set;
    }
    public static CStageConfig stageConfig
    {
        get;
        private set;
    }
    public static CStage選曲 stage選曲
    {
        get;
        private set;
    }
    public static CStageSongLoading stageSongLoading
    {
        get;
        private set;
    }
    public static CStage演奏画面共通 stage演奏ドラム画面
    {
        get;
        private set;
    }
    public static CStageResult stageResult
    {
        get;
        private set;
    }
    public static CStageChangeSkin stageChangeSkin
    {
        get;
        private set;
    }
    public static CStageEnding stageEnding
    {
        get;
        private set;
    }
    public static CStageMaintenance stageMaintenance
    {
        get;
        private set;
    }
    public static CStage r現在のステージ = null;
    public static CStage r直前のステージ = null;
    public static string strEXEのあるフォルダ => AppContext.BaseDirectory;
    public CTimer Timer
    {
        get;
        private set;
    }
    public DiscordRichPresence Discord
    {
        get;
        private set;
    }

    public bool bApplicationActive
    {
        get
        {
            return this.Focused;
        }
    }
    public bool b次のタイミングで垂直帰線同期切り替えを行う
    {
        get;
        set;
    }
    public bool b次のタイミングで全画面_ウィンドウ切り替えを行う
    {
        get;
        set;
    }
    private static Size currentClientSize       // #23510 2010.10.27 add yyagi to keep current window size
    {
        get;
        set;
    }
    #endregion

    // コンストラクタ

    public TJAPlayer3()
        : base("TJAPlayer3-f", 1280, 720)
    {
        TJAPlayer3.app = this;

        Program.Renderer = this.RendererName;
        #region [ Config.toml の読み込み ]
        string tomlpath = strEXEのあるフォルダ + "Config.toml";
        ConfigToml = CConfigToml.Load(tomlpath);
        #endregion
        #region [ Config.ini の読込み ]
        //---------------------
        ConfigIni = new CConfigIni();
        string path = strEXEのあるフォルダ + "Config.ini";
        if (File.Exists(path))
        {
            try
            {
                ConfigIni.tファイルから読み込み(path);
            }
            catch (Exception e)
            {
                //ConfigIni = new CConfigIni();	// 存在してなければ新規生成
                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues.");
            }
        }
        //---------------------
        #endregion
        #region [ ログ出力開始 ]
        //---------------------
        Trace.AutoFlush = true;
        Trace.Listeners.Add(new CTraceLogListener(new StreamWriter(Path.Combine(strEXEのあるフォルダ, "TJAPlayer3-f.log"), false, new UTF8Encoding(false))));

        Trace.WriteLine("");
        Trace.WriteLine("DTXMania powered by YAMAHA Silent Session Drums");
        Trace.WriteLine(string.Format("Release: {0}", VERSION));
        Trace.WriteLine("");
        Trace.TraceInformation("----------------------");
        Trace.TraceInformation("■ アプリケーションの初期化");
        Trace.TraceInformation("OS Version: " + Environment.OSVersion);
        Trace.TraceInformation("ProcessorCount: " + Environment.ProcessorCount.ToString());
        Trace.TraceInformation("CLR Version: " + Environment.Version.ToString());
        //---------------------
        #endregion

        #region [ FFmpegのパス設定 ]
        if (!string.IsNullOrEmpty(ConfigToml.General.FFmpegPath))
            FFmpeg.AutoGen.ffmpeg.RootPath = ConfigToml.General.FFmpegPath;
        #endregion

        #region [ ウィンドウ初期化 ]
        //---------------------
        base.Location = new Point(ConfigToml.Window.X, ConfigToml.Window.Y);   // #30675 2013.02.04 ikanick add


        base.Title = "";

        base.ClientSize = new Size(ConfigToml.Window.Width, ConfigToml.Window.Height);   // #34510 yyagi 2010.10.31 to change window size got from Config.ini

        if (ConfigToml.Window.FullScreen)                       // #23510 2010.11.02 yyagi: add; to recover window size in case bootup with fullscreen mode
        {                                                       // #30666 2013.02.02 yyagi: currentClientSize should be always made
            currentClientSize = new Size(ConfigToml.Window.Width, ConfigToml.Window.Height);
        }

        var icon_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TJAPlayer3.TJAPlayer3-f.ico");
        if (icon_stream is not null)
            base.Icon = icon_stream;
        base.MouseWheel += this.Window_MouseWheel;
        base.Resize += this.Window_ResizeOrMove;                       // #23510 2010.11.20 yyagi: to set resized window size in Config.ini
        base.Move += this.Window_ResizeOrMove;
        //---------------------
        #endregion
        #region [ Direct3D9 デバイスの生成 ]
        //---------------------
        this.FullScreen = ConfigToml.Window.FullScreen;
        this.VSync = ConfigToml.Window.VSyncWait;
        base.ClientSize = new Size(ConfigToml.Window.Width, ConfigToml.Window.Height);   // #23510 2010.10.31 yyagi: to recover window size. width and height are able to get from Config.ini.
                                                                                         //---------------------
        #endregion

        DTX[0] = null;
        DTX[1] = null;

        #region [ Skin の初期化 ]
        //---------------------
        Trace.TraceInformation("スキンの初期化を行います。");
        Trace.Indent();
        try
        {
            Skin = new CSkin(TJAPlayer3.app.ConfigToml.General._AbsSkinPath);
            TJAPlayer3.app.ConfigToml.General.SkinPath = TJAPlayer3.app.Skin.GetCurrentSkinSubfolderFullName(true);    // 旧指定のSkinフォルダが消滅していた場合に備える
            this.LogicalSize = new Size(Skin.SkinConfig.General.Width, Skin.SkinConfig.General.Height);
            Trace.TraceInformation("スキンの初期化を完了しました。");
        }
        catch
        {
            Trace.TraceInformation("スキンの初期化に失敗しました。");
            throw;
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        //-----------
        #region [ Timer の初期化 ]
        //---------------------
        Trace.TraceInformation("タイマの初期化を行います。");
        Trace.Indent();
        try
        {
            Timer = new CTimer();
            Trace.TraceInformation("タイマの初期化を完了しました。");
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        //-----------

        #region [ FPS カウンタの初期化 ]
        //---------------------
        Trace.TraceInformation("FPSカウンタの初期化を行います。");
        Trace.Indent();
        try
        {
            FPS = new CFPS();
            Trace.TraceInformation("FPSカウンタを生成しました。");
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ act文字コンソールの初期化 ]
        //---------------------
        Trace.TraceInformation("文字コンソールの初期化を行います。");
        Trace.Indent();
        try
        {
            act文字コンソール = new C文字コンソール();
            Trace.TraceInformation("文字コンソールを生成しました。");
            act文字コンソール.On活性化();
            Trace.TraceInformation("文字コンソールを活性化しました。");
            Trace.TraceInformation("文字コンソールの初期化を完了しました。");
        }
        catch (Exception exception)
        {
            Trace.TraceError(exception.ToString());
            Trace.TraceError("文字コンソールの初期化に失敗しました。");
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ InputManager の初期化 ]
        //---------------------
        Trace.TraceInformation("InputManagerの初期化を行います。");
        Trace.Indent();
        try
        {
            InputManager = new CInputManager();
            foreach (IInputDevice device in InputManager.listInputDevices)
            {
                if ((device.eInputDeviceType == EInputDeviceType.Joystick) && !ConfigToml.JoystickGUID.ContainsValue(device.GUID))
                {
                    int key = 0;
                    while (ConfigToml.JoystickGUID.ContainsKey(key))
                    {
                        key++;
                    }
                    ConfigToml.JoystickGUID.Add(key, device.GUID);
                }
            }
            InputCTS = new CancellationTokenSource();
            Task.Factory.StartNew(() => InputLoop());
            Trace.TraceInformation("InputManagerの初期化を完了しました。");
        }
        catch
        {
            Trace.TraceError("InputManagerの初期化に失敗しました。");
            throw;
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ Pad の初期化 ]
        //---------------------
        Trace.TraceInformation("パッドの初期化を行います。");
        Trace.Indent();
        try
        {
            Pad = new CPad(ConfigIni, ConfigToml, InputManager);
            Trace.TraceInformation("パッドの初期化を完了しました。");
        }
        catch (Exception exception3)
        {
            Trace.TraceError(exception3.ToString());
            Trace.TraceError("パッドの初期化に失敗しました。");
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ SoundManager の初期化 ]
        //---------------------
        Trace.TraceInformation("サウンドデバイスの初期化を行います。");
        Trace.Indent();
        try
        {
            ESoundDeviceType soundDeviceType;
            switch (TJAPlayer3.app.ConfigToml.SoundDevice.DeviceType)
            {
                case 0:
                    soundDeviceType = ESoundDeviceType.BASS;
                    break;
                case 1:
                    soundDeviceType = ESoundDeviceType.ASIO;
                    break;
                case 2:
                    soundDeviceType = ESoundDeviceType.ExclusiveWASAPI;
                    break;
                case 3:
                    soundDeviceType = ESoundDeviceType.SharedWASAPI;
                    break;
                default:
                    soundDeviceType = ESoundDeviceType.Unknown;
                    break;
            }
            SoundManager = new CSoundManager(soundDeviceType,
                                        TJAPlayer3.app.ConfigToml.SoundDevice.WASAPIBufferSizeMs,
                                        0,
                                        TJAPlayer3.app.ConfigToml.SoundDevice.ASIODevice,
                                        TJAPlayer3.app.ConfigToml.SoundDevice.BASSBufferSizeMs,
                                        TJAPlayer3.app.ConfigToml.SoundDevice.UseOSTimer
            );


            Trace.TraceInformation("Initializing loudness scanning, song gain control, and sound group level control...");
            Trace.Indent();
            try
            {
                actScanningLoudness = new CActScanningLoudness();
                actScanningLoudness.On活性化();
                LoudnessMetadataScanner.ScanningStateChanged +=
                    (_, args) => actScanningLoudness.bIsActivelyScanning = args.IsActivelyScanning;
                LoudnessMetadataScanner.StartBackgroundScanning();

                SongGainController = new SongGainController();
                ConfigIniToSongGainControllerBinder.Bind(ConfigToml, SongGainController);

                SoundGroupLevelController = new SoundGroupLevelController(CSound.listインスタンス);
                ConfigIniToSoundGroupLevelControllerBinder.Bind(ConfigToml, SoundGroupLevelController);
            }
            finally
            {
                Trace.Unindent();
                Trace.TraceInformation("Initialized loudness scanning, song gain control, and sound group level control.");
            }

            ShowWindowTitleWithSoundType();
            CSoundManager.bIsTimeStretch = TJAPlayer3.app.ConfigToml.PlayOption.TimeStretch;
            SoundManager.nMasterVolume = TJAPlayer3.app.ConfigToml.MasterVolume;
            //FDK.CSoundManager.bIsMP3DecodeByWindowsCodec = CDTXMania.ConfigIni.bNoMP3Streaming;
            Trace.TraceInformation("サウンドデバイスの初期化を完了しました。");
        }
        catch (Exception e)
        {
            throw new NullReferenceException("サウンドデバイスがひとつも有効になっていないため、サウンドデバイスの初期化ができませんでした。", e);
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ SongsManager の初期化 ]
        //---------------------
        Trace.TraceInformation("曲リストの初期化を行います。");
        Trace.Indent();
        try
        {
            SongsManager = new CSongsManager();
            //				SongsManager_裏読 = new CSongsManager();
            EnumSongs = new CEnumSongs();
            actEnumSongs = new CActEnumSongs();
            Trace.TraceInformation("曲リストの初期化を完了しました。");
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
            Trace.TraceError("曲リストの初期化に失敗しました。");
        }
        finally
        {
            Trace.Unindent();
        }
        //---------------------
        #endregion
        #region [ ステージの初期化 ]
        //---------------------
        r現在のステージ = null;
        r直前のステージ = null;
        stageStartUp = new CStageStartUp();
        stageTitle = new CStageTitle();
        //			stageオプション = new CStageオプション();
        stageConfig = new CStageConfig();
        stage選曲 = new CStage選曲();
        stageSongLoading = new CStageSongLoading();
        stage演奏ドラム画面 = new CStage演奏画面共通();
        stageResult = new CStageResult();
        stageChangeSkin = new CStageChangeSkin();
        stageEnding = new CStageEnding();
        stageMaintenance = new CStageMaintenance();
        this.listトップレベルActivities = new List<CActivity>();
        this.listトップレベルActivities.Add(actEnumSongs);
        this.listトップレベルActivities.Add(act文字コンソール);
        this.listトップレベルActivities.Add(stageStartUp);
        this.listトップレベルActivities.Add(stageTitle);
        //			this.listトップレベルActivities.Add( stageオプション );
        this.listトップレベルActivities.Add(stageConfig);
        this.listトップレベルActivities.Add(stage選曲);
        this.listトップレベルActivities.Add(stageSongLoading);
        this.listトップレベルActivities.Add(stage演奏ドラム画面);
        this.listトップレベルActivities.Add(stageResult);
        this.listトップレベルActivities.Add(stageChangeSkin);
        this.listトップレベルActivities.Add(stageEnding);
        this.listトップレベルActivities.Add(stageMaintenance);
        //---------------------
        #endregion
        #region Discordの処理
        this.Discord = new DiscordRichPresence("692578108997632051");
        this.Discord.Update("Startup");
        #endregion

        Trace.TraceInformation("アプリケーションの初期化を完了しました。");


        #region [ 最初のステージの起動 ]
        //---------------------
        Trace.TraceInformation("----------------------");
        Trace.TraceInformation("■ 起動");

        r現在のステージ = stageStartUp;

        r現在のステージ.On活性化();

        //---------------------
        #endregion
    }

    // メソッド

    public void t全画面_ウィンドウモード切り替え()
    {
        if ((ConfigToml is not null) && (ConfigToml.Window.FullScreen != this.FullScreen))
        {
            if (ConfigToml.Window.FullScreen)   // #23510 2010.10.27 yyagi: backup current window size before going fullscreen mode
            {
                currentClientSize = this.ClientSize;
                ConfigToml.Window.Width = this.ClientSize.Width;
                ConfigToml.Window.Height = this.ClientSize.Height;
            }
            this.FullScreen = ConfigToml.Window.FullScreen;
            if (!ConfigToml.Window.FullScreen)    // #23510 2010.10.27 yyagi: to resume window size from backuped value
            {
                base.ClientSize =
                    new Size(currentClientSize.Width, currentClientSize.Height);
            }
        }
    }

    // Game 実装

    protected override void OnClosing(CancelEventArgs e)
    {
        if ((EEndingAnime)ConfigToml.Ending.EndingAnime == EEndingAnime.Force && (r現在のステージ.eStageID != CStage.EStage.Ending))
        {
            e.Cancel = true;
            r現在のステージ.On非活性化();
            Trace.TraceInformation("----------------------");
            Trace.TraceInformation("■ 終了");
            stageEnding.On活性化();
            r直前のステージ = r現在のステージ;
            r現在のステージ = stageEnding;
            this.tガベージコレクションを実行する();
        }
        base.OnClosing(e);
    }
    protected override void OnClosed(EventArgs e)
    {
        this.t終了処理();
    }

    protected override void OnRenderFrame(EventArgs e)
    {
        Timer?.t更新();
        CSoundManager.rc演奏用タイマ?.t更新();
        InputManager?.tSwapEventList();
        FPS.tUpdateCounter();

        if (this.Device is null)
            return;

        // #xxxxx 2013.4.8 yyagi; sleepの挿入位置を、EndScnene～Present間から、BeginScene前に移動。描画遅延を小さくするため。
        #region [ スリープ ]
        if (ConfigToml.Window.SleepTimePerFrame > 0)            // #xxxxx 2011.11.27 yyagi
        {
            Thread.Sleep(ConfigToml.Window.SleepTimePerFrame);
        }
        if (ConfigToml.Window.BackSleep > 0 && !this.Focused)
        {
            Thread.Sleep(ConfigToml.Window.BackSleep);
        }
        #endregion

        if (r現在のステージ is not null)
        {
            this.n進行描画の戻り値 = (r現在のステージ is not null) ? r現在のステージ.On進行描画() : 0;

            #region [ 曲検索スレッドの起動/終了 ]					// ここに"Enumerating Songs..."表示を集約
            actEnumSongs.On進行描画();                          // "Enumerating Songs..."アイコンの描画
            switch (r現在のステージ.eStageID)
            {
                case CStage.EStage.Title:
                case CStage.EStage.Config:
                case CStage.EStage.SongSelect:
                case CStage.EStage.SongLoading:
                    if (EnumSongs is not null)
                    {
                        #region [ (特定条件時) 曲検索スレッドの起動_開始 ]
                        if (r現在のステージ.eStageID == CStage.EStage.Title &&
                                r直前のステージ.eStageID == CStage.EStage.StartUp &&
                                this.n進行描画の戻り値 == (int)CStageTitle.E戻り値.継続 &&
                                !EnumSongs.IsSongListEnumStarted)
                        {
                            actEnumSongs.On活性化();
                            TJAPlayer3.stage選曲.act曲リスト.bIsEnumeratingSongs = true;
                            EnumSongs.StartEnumFromDisk();      // 曲検索スレッドの起動_開始
                        }
                        #endregion

                        #region [ 曲検索の中断と再開 ]
                        if (r現在のステージ.eStageID == CStage.EStage.SongSelect && !EnumSongs.IsSongListEnumCompletelyDone)
                        {
                            switch (this.n進行描画の戻り値)
                            {
                                case 0:     // 何もない
                                    EnumSongs.Resume();                     // #27060 2012.2.6 yyagi 中止していたバックグランド曲検索を再開
                                    actEnumSongs.On活性化();
                                    break;

                                case 2:     // 曲決定
                                    EnumSongs.Suspend();                        // #27060 バックグラウンドの曲検索を一時停止
                                    actEnumSongs.On非活性化();
                                    break;
                            }
                        }
                        #endregion

                        #region [ 曲探索中断待ち待機 ]
                        if (r現在のステージ.eStageID == CStage.EStage.SongLoading && !EnumSongs.IsSongListEnumCompletelyDone &&
                            EnumSongs.thDTXFileEnumerate is not null)                           // #28700 2012.6.12 yyagi; at Compact mode, enumerating thread does not exist.
                        {
                            EnumSongs.WaitUntilSuspended();                                 // 念のため、曲検索が一時中断されるまで待機
                        }
                        #endregion

                        #region [ 曲検索が完了したら、実際の曲リストに反映する ]
                        // CStage選曲.On活性化() に回した方がいいかな？
                        if (EnumSongs.IsSongListEnumerated)
                        {
                            actEnumSongs.On非活性化();
                            TJAPlayer3.stage選曲.act曲リスト.bIsEnumeratingSongs = false;

                            bool bRemakeSongTitleBar = (r現在のステージ.eStageID == CStage.EStage.SongSelect) ? true : false;
                            TJAPlayer3.stage選曲.Refresh(EnumSongs.SongsManager, bRemakeSongTitleBar);
                            EnumSongs.SongListEnumCompletelyDone();
                        }
                        #endregion
                    }
                    break;
            }
            #endregion

            switch (r現在のステージ.eStageID)
            {
                case CStage.EStage.StartUp:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        r現在のステージ.On非活性化();
                        Trace.TraceInformation("----------------------");
                        Trace.TraceInformation("■ Title");
                        stageTitle.On活性化();
                        r直前のステージ = r現在のステージ;
                        r現在のステージ = stageTitle;

                        this.tガベージコレクションを実行する();
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Title:
                    #region [ *** ]
                    //-----------------------------
                    switch (this.n進行描画の戻り値)
                    {
                        case (int)CStageTitle.E戻り値.GAMESTART:
                            #region [ 選曲処理へ ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ 選曲");
                            stage選曲.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stage選曲;
                            //-----------------------------
                            #endregion
                            break;

                        case (int)CStageTitle.E戻り値.CONFIG:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ Config");
                            stageConfig.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageConfig;
                            //-----------------------------
                            #endregion
                            break;

                        case (int)CStageTitle.E戻り値.EXIT:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ Ending");
                            stageEnding.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageEnding;
                            //-----------------------------
                            #endregion
                            break;

                        case (int)CStageTitle.E戻り値.MAINTENANCE:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ Maintenance");
                            stageMaintenance.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageMaintenance;
                            //-----------------------------
                            #endregion
                            break;
                    }

                    //this.tガベージコレクションを実行する();		// #31980 2013.9.3 yyagi タイトル画面でだけ、毎フレームGCを実行して重くなっていた問題の修正
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Config:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        switch (r直前のステージ.eStageID)
                        {
                            case CStage.EStage.Title:
                                #region [ *** ]
                                //-----------------------------
                                r現在のステージ.On非活性化();
                                Trace.TraceInformation("----------------------");
                                Trace.TraceInformation("■ タイトル");
                                stageTitle.On活性化();
                                r直前のステージ = r現在のステージ;
                                r現在のステージ = stageTitle;

                                this.tガベージコレクションを実行する();
                                break;
                            //-----------------------------
                            #endregion

                            case CStage.EStage.SongSelect:
                                #region [ *** ]
                                //-----------------------------
                                r現在のステージ.On非活性化();
                                Trace.TraceInformation("----------------------");
                                Trace.TraceInformation("■ 選曲");
                                stage選曲.On活性化();
                                r直前のステージ = r現在のステージ;
                                r現在のステージ = stage選曲;

                                this.tガベージコレクションを実行する();
                                break;
                                //-----------------------------
                                #endregion
                        }
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.SongSelect:
                    #region [ *** ]
                    //-----------------------------
                    switch (this.n進行描画の戻り値)
                    {
                        case (int)CStage選曲.E戻り値.タイトルに戻る:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ タイトル");
                            stageTitle.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageTitle;

                            this.tガベージコレクションを実行する();
                            break;
                        //-----------------------------
                        #endregion

                        case (int)CStage選曲.E戻り値.選曲した:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ 曲読み込み");
                            stageSongLoading.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageSongLoading;

                            this.tガベージコレクションを実行する();
                            break;
                        //-----------------------------
                        #endregion

                        //							case (int) CStage選曲.E戻り値.オプション呼び出し:
                        #region [ *** ]
                        //								//-----------------------------
                        //								r現在のステージ.On非活性化();
                        //								Trace.TraceInformation( "----------------------" );
                        //								Trace.TraceInformation( "■ オプション" );
                        //								stageオプション.On活性化();
                        //								r直前のステージ = r現在のステージ;
                        //								r現在のステージ = stageオプション;
                        //
                        //								this.tガベージコレクションを実行する();
                        //								break;
                        //							//-----------------------------
                        #endregion

                        case (int)CStage選曲.E戻り値.コンフィグ呼び出し:
                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ コンフィグ");
                            stageConfig.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageConfig;

                            this.tガベージコレクションを実行する();
                            break;
                        //-----------------------------
                        #endregion

                        case (int)CStage選曲.E戻り値.スキン変更:

                            #region [ *** ]
                            //-----------------------------
                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ スキン切り替え");
                            stageChangeSkin.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageChangeSkin;
                            break;
                            //-----------------------------
                            #endregion
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.SongLoading:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        TJAPlayer3.app.Pad.stDetectedDevices.Clear();  // 入力デバイスフラグクリア(2010.9.11)
                        r現在のステージ.On非活性化();
                        #region [ ESC押下時は、曲の読み込みを中止して選曲画面に戻る ]
                        if (this.n進行描画の戻り値 == (int)E曲読込画面の戻り値.読込中止)
                        {
                            //DTX.t全チップの再生停止();
                            if (DTX[0] is not null)
                                DTX[0].On非活性化();
                            Trace.TraceInformation("曲の読み込みを中止しました。");
                            this.tガベージコレクションを実行する();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ 選曲");
                            stage選曲.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stage選曲;
                            break;
                        }
                        #endregion

                        Trace.TraceInformation("----------------------");
                        Trace.TraceInformation("■ 演奏（ドラム画面）");
                        r直前のステージ = r現在のステージ;
                        r現在のステージ = stage演奏ドラム画面;

                        this.tガベージコレクションを実行する();
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Playing:
                    #region [ *** ]
                    //-----------------------------
                    switch (this.n進行描画の戻り値)
                    {
                        case (int)E演奏画面の戻り値.再読込_再演奏:
                            #region [ DTXファイルを再読み込みして、再演奏 ]
                            DTX[0].t全チップの再生停止();
                            DTX[0].On非活性化();
                            r現在のステージ.On非活性化();
                            stageSongLoading.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageSongLoading;
                            this.tガベージコレクションを実行する();
                            break;
                        #endregion

                        case (int)E演奏画面の戻り値.継続:
                            break;

                        case (int)E演奏画面の戻り値.演奏中断:
                            #region [ 演奏キャンセル ]
                            //-----------------------------
                            this.tUpdateScoreJson();


                            DTX[0].t全チップの再生停止();
                            DTX[0].On非活性化();
                            r現在のステージ.On非活性化();

                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ 選曲");
                            stage選曲.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stage選曲;

                            this.tガベージコレクションを実行する();

                            break;
                        //-----------------------------
                        #endregion

                        case (int)E演奏画面の戻り値.ステージ失敗:
                            #region [ 演奏失敗(StageFailed) ]
                            //-----------------------------
                            this.tUpdateScoreJson();

                            DTX[0].t全チップの再生停止();
                            DTX[0].On非活性化();
                            r現在のステージ.On非活性化();

                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ 選曲");
                            stage選曲.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stage選曲;

                            this.tガベージコレクションを実行する();
                            break;
                        //-----------------------------
                        #endregion

                        case (int)E演奏画面の戻り値.ステージクリア:
                            #region [ 演奏クリア ]
                            //-----------------------------
                            CScoreJson.CRecord[] cRecords = new CScoreJson.CRecord[4];
                            for (int i = 0; i < ConfigToml.PlayOption.PlayerCount; i++)
                                stage演奏ドラム画面.tSaveToCRecord(out cRecords[i], i);

                            this.tUpdateScoreJson();

                            r現在のステージ.On非活性化();
                            Trace.TraceInformation("----------------------");
                            Trace.TraceInformation("■ Result");
                            for (int i = 0; i < ConfigToml.PlayOption.PlayerCount; i++)
                                stageResult.cRecords[i] = cRecords[i];

                            stageResult.On活性化();
                            r直前のステージ = r現在のステージ;
                            r現在のステージ = stageResult;

                            break;
                            //-----------------------------
                            #endregion
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Result:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        //DTX.t全チップの再生一時停止();
                        DTX[0].t全チップの再生停止とミキサーからの削除();
                        DTX[0].On非活性化();
                        r現在のステージ.On非活性化();
                        this.tガベージコレクションを実行する();

                        Trace.TraceInformation("----------------------");
                        Trace.TraceInformation("■ 選曲");
                        stage選曲.On活性化();
                        r直前のステージ = r現在のステージ;
                        r現在のステージ = stage選曲;

                        this.tガベージコレクションを実行する();
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.ChangeSkin:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        r現在のステージ.On非活性化();
                        Trace.TraceInformation("----------------------");
                        Trace.TraceInformation("■ 選曲");
                        stage選曲.On活性化();
                        r直前のステージ = r現在のステージ;
                        r現在のステージ = stage選曲;
                        this.tガベージコレクションを実行する();
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Ending:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        base.Exit();
                    }
                    //-----------------------------
                    #endregion
                    break;

                case CStage.EStage.Maintenance:
                    #region [ *** ]
                    //-----------------------------
                    if (this.n進行描画の戻り値 != 0)
                    {
                        r現在のステージ.On非活性化();
                        Trace.TraceInformation("----------------------");
                        Trace.TraceInformation("■ 選曲");
                        stage選曲.On活性化();
                        r直前のステージ = r現在のステージ;
                        r現在のステージ = stage選曲;
                        this.tガベージコレクションを実行する();
                    }
                    //-----------------------------
                    #endregion
                    break;
            }

            actScanningLoudness.On進行描画();

            if (r現在のステージ is not null && r現在のステージ.eStageID != CStage.EStage.StartUp && TJAPlayer3.app.Tx.Network_Connection is not null)
            {
                if (Math.Abs(CSoundManager.rc演奏用タイマ.nシステム時刻ms - this.前回のシステム時刻ms) > 10000)
                {
                    this.前回のシステム時刻ms = CSoundManager.rc演奏用タイマ.nシステム時刻ms;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            //IPv4 8.8.8.8にPingを送信する(timeout 5000ms)
                            PingReply reply = new Ping().Send("8.8.8.8", 5000);
                            this.bネットワークに接続中 = reply.Status == IPStatus.Success;
                        }
                        catch
                        {
                            this.bネットワークに接続中 = false;
                        }
                    });
                }
                TJAPlayer3.app.Tx.Network_Connection.t2D描画(app.Device, this.LogicalSize.Width - (TJAPlayer3.app.Tx.Network_Connection.szTextureSize.Width / 2), this.LogicalSize.Height - TJAPlayer3.app.Tx.Network_Connection.szTextureSize.Height, new Rectangle((TJAPlayer3.app.Tx.Network_Connection.szTextureSize.Width / 2) * (this.bネットワークに接続中 ? 0 : 1), 0, TJAPlayer3.app.Tx.Network_Connection.szTextureSize.Width / 2, TJAPlayer3.app.Tx.Network_Connection.szTextureSize.Height));
            }
            // オーバレイを描画する(テクスチャの生成されていない起動ステージは例外
            if (r現在のステージ is not null && r現在のステージ.eStageID != CStage.EStage.StartUp && TJAPlayer3.app.Tx.Overlay is not null)
            {
                TJAPlayer3.app.Tx.Overlay.t2D描画(app.Device, 0, 0);
            }
        }


        for (int i = 0; i < 0x10; i++)
        {
            if (ConfigIni.KeyAssign.Capture[i].Code > 0)
                if (InputManager.Keyboard.bIsKeyPressed((int)ConfigIni.KeyAssign.Capture[i].Code))
                {
                    // Debug.WriteLine( "capture: " + string.Format( "{0:2x}", (int) e.KeyCode ) + " " + (int) e.KeyCode );
                    string strFullPath =
                        Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "Capture_img");
                    strFullPath = Path.Combine(strFullPath, DateTime.Now.ToString("yyyyMMddHHmmss") + ConfigToml.General.ScreenShotExt);
                    this.SaveScreen(strFullPath);
                }
            if (ConfigIni.KeyAssign.FullScreen[i].Code > 0)
                if (InputManager.Keyboard.bIsKeyPressed((int)ConfigIni.KeyAssign.FullScreen[i].Code))
                {
                    if (ConfigToml is not null)
                    {
                        ConfigToml.Window.FullScreen = !ConfigToml.Window.FullScreen;
                        this.t全画面_ウィンドウモード切り替え();
                    }
                }
        }
        if ((InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.LeftAlt) || InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.RightAlt)) && InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return))
        {
            if (ConfigToml is not null)
            {
                ConfigToml.Window.FullScreen = !ConfigToml.Window.FullScreen;
                this.t全画面_ウィンドウモード切り替え();
            }
        }

        this.Render();


        #region [ 全画面_ウインドウ切り替え ]
        if (this.b次のタイミングで全画面_ウィンドウ切り替えを行う)
        {
            ConfigToml.Window.FullScreen = !ConfigToml.Window.FullScreen;
            app.t全画面_ウィンドウモード切り替え();
            this.b次のタイミングで全画面_ウィンドウ切り替えを行う = false;
        }
        #endregion

        #region [ 垂直基線同期切り替え ]
        if (this.b次のタイミングで垂直帰線同期切り替えを行う)
        {
            currentClientSize = this.ClientSize;                                             // #23510 2010.11.3 yyagi: to backup current window size before changing VSyncWait

            this.VSync = ConfigToml.Window.VSyncWait;
            this.b次のタイミングで垂直帰線同期切り替えを行う = false;
            base.ClientSize = new Size(currentClientSize.Width, currentClientSize.Height);   // #23510 2010.11.3 yyagi: to resume window size after changing VSyncWait
        }
        #endregion
    }

    // その他

    #region [ 汎用ヘルパー ]
    //-----------------
    public CTexture? tCreateTexture(string fileName)
    {
        try
        {
            return new CTexture(this.Device, fileName);
        }
        catch (CTextureCreateFailedException e)
        {
            Trace.TraceError(e.ToString());
            Trace.TraceError("テクスチャの生成に失敗しました。({0})", fileName);
            return null;
        }
        catch (FileNotFoundException)
        {
            Trace.TraceWarning("テクスチャファイルが見つかりませんでした。({0})", fileName);
            return null;
        }
    }
    public CTexture? tCreateTexture(SKBitmap image)
    {
        try
        {
            return new CTexture(this.Device, image);
        }
        catch (CTextureCreateFailedException e)
        {
            Trace.TraceError(e.ToString());
            Trace.TraceError("テクスチャの生成に失敗しました。(txData)");
            return null;
        }
    }

    public CTexture? ColorTexture(string htmlcolor, int width = 64, int height = 64)//2020.05.31 Mr-Ojii 単色塗りつぶしテクスチャの生成。必要かって？Tile_Black・Tile_Whiteがいらなくなるじゃん。あと、メンテモードの画像生成に便利かなって。
    {
        if (htmlcolor.Length == 7 && htmlcolor.StartsWith("#"))
            return ColorTexture(SKColor.Parse(htmlcolor.Remove(0, 1)), width, height);
        else
            return ColorTexture(SKColors.Black, width, height);
    }
    /// <summary>
    /// 単色塗りつぶしテクスチャの生成
    /// </summary>
    /// <param name="brush">ブラシの色とかの指定</param>
    /// <param name="width">幅</param>
    /// <param name="height">高さ</param>
    /// <returns></returns>
    public CTexture? ColorTexture(SKColor color, int width = 64, int height = 64)
    {
        using (var bitmap = new SKBitmap(width, height))
        {
            using(var canvas = new SKCanvas(bitmap))
            {
                canvas.DrawColor(color);
                return this.tCreateTexture(bitmap);
            }
        }
    }

    /// <summary>プロパティ、インデクサには ref は使用できないので注意。</summary>
    public static void t安全にDisposeする<T>(ref T? obj) where T : class, IDisposable //2020.06.06 Mr-Ojii twopointzero氏のソースコードをもとに改良
    {
        if (obj is null)
            return;

        obj.Dispose();
        obj = null;
    }

    public static void t安全にDisposeする<T>(ref T?[] array) where T : class, IDisposable //2020.08.01 Mr-Ojii twopointzero氏のソースコードをもとに追加
    {
        if (array is null)
        {
            return;
        }

        for (var i = 0; i < array.Length; i++)
        {
            array[i]?.Dispose();
            array[i] = null;
        }
    }

    /// <summary>
    /// そのフォルダの連番画像の最大値を返す。
    /// </summary>
    public static int t連番画像の枚数を数える(string ディレクトリ名, string プレフィックス = "", string 拡張子 = ".png")
    {
        int num = 0;
        while (File.Exists(ディレクトリ名 + プレフィックス + num + 拡張子))
        {
            num++;
        }
        return num;
    }

    /// <summary>
    /// そのフォルダの連番フォルダの最大値を返す。
    /// </summary>
    public static int t連番フォルダの個数を数える(string ディレクトリ名, string プレフィックス = "")
    {
        int num = 0;
        while (Directory.Exists(ディレクトリ名 + プレフィックス + num))
        {
            num++;
        }
        return num;
    }

    /// <summary>
    /// 曲名テクスチャの縮小倍率を返す。
    /// </summary>
    /// <param name="cTexture">曲名テクスチャ。</param>
    /// <param name="samePixel">等倍で表示するピクセル数の最大値(デフォルト値:645)</param>
    /// <returns>曲名テクスチャの縮小倍率。そのテクスチャがnullならば一倍(1f)を返す。</returns>
    public static float GetSongNameXScaling(ref CTexture cTexture, int samePixel = 660)
    {
        if (cTexture is null) return 1f;
        float scalingRate = (float)samePixel / (float)cTexture.szTextureSize.Width;
        if (cTexture.szTextureSize.Width <= samePixel)
            scalingRate = 1.0f;
        return scalingRate;
    }

    //-----------------
    #endregion

    #region [ private ]
    //-----------------
    private bool b終了処理完了済み;
    private bool bネットワークに接続中 = false;
    private long 前回のシステム時刻ms = long.MinValue;
    private static CDTX[] dtx = new CDTX[4];

    public TextureLoader Tx = new TextureLoader();

    private List<CActivity> listトップレベルActivities;
    private int n進行描画の戻り値;
    private CancellationTokenSource InputCTS = null;

    private void InputLoop()
    {
        while (!InputCTS.IsCancellationRequested)
        {
            InputManager?.tPolling(this.bApplicationActive);
            Thread.Sleep(1);
        }
    }

    public void ShowWindowTitleWithSoundType()
    {
        string delay = "(" + SoundManager.GetSoundDelay() + "ms)";
        AssemblyName asmApp = Assembly.GetExecutingAssembly().GetName();
        base.Title = asmApp.Name + " Ver." + VERSION + " (" + SoundManager.GetCurrentSoundDeviceType() + delay + ")";
    }

    private void t終了処理()
    {
        if (!this.b終了処理完了済み)
        {
            Trace.TraceInformation("----------------------");
            Trace.TraceInformation("■ アプリケーションの終了");
            #region [ 曲検索の終了処理 ]
            //---------------------
            if (actEnumSongs is not null)
            {
                Trace.TraceInformation("曲検索actの終了処理を行います。");
                Trace.Indent();
                try
                {
                    actEnumSongs.On非活性化();
                    actEnumSongs = null;
                    Trace.TraceInformation("曲検索actの終了処理を完了しました。");
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    Trace.TraceError("曲検索actの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ 現在のステージの終了処理 ]
            //---------------------
            if (TJAPlayer3.r現在のステージ is not null && TJAPlayer3.r現在のステージ.b活性化してる)		// #25398 2011.06.07 MODIFY FROM
            {
                Trace.TraceInformation("現在のステージを終了します。");
                Trace.Indent();
                try
                {
                    r現在のステージ.On非活性化();
                    Trace.TraceInformation("現在のステージの終了処理を完了しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region Discordの処理
            this.Discord.Dispose();
            #endregion
            #region [ 曲リストの終了処理 ]
            //---------------------
            if (SongsManager is not null)
            {
                Trace.TraceInformation("曲リストの終了処理を行います。");
                Trace.Indent();
                try
                {
                    SongsManager = null;
                    Trace.TraceInformation("曲リストの終了処理を完了しました。");
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());
                    Trace.TraceError("曲リストの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region TextureLoaderの処理
            Tx.DisposeTexture();
            #endregion
            #region [ スキンの終了処理 ]
            //---------------------
            if (Skin is not null)
            {
                Trace.TraceInformation("スキンの終了処理を行います。");
                Trace.Indent();
                try
                {
                    Skin.Dispose();
                    Skin = null;
                    Trace.TraceInformation("スキンの終了処理を完了しました。");
                }
                catch (Exception exception2)
                {
                    Trace.TraceError(exception2.ToString());
                    Trace.TraceError("スキンの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ サウンドの終了処理 ]
            //---------------------
            if (SoundManager is not null)
            {
                Trace.TraceInformation("サウンド の終了処理を行います。");
                Trace.Indent();
                try
                {
                    SoundManager.Dispose();
                    SoundManager = null;
                    Trace.TraceInformation("サウンド の終了処理を完了しました。");
                }
                catch (Exception exception3)
                {
                    Trace.TraceError(exception3.ToString());
                    Trace.TraceError("サウンド の終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ パッドの終了処理 ]
            //---------------------
            if (Pad is not null)
            {
                Trace.TraceInformation("パッドの終了処理を行います。");
                Trace.Indent();
                try
                {
                    Pad = null;
                    Trace.TraceInformation("パッドの終了処理を完了しました。");
                }
                catch (Exception exception4)
                {
                    Trace.TraceError(exception4.ToString());
                    Trace.TraceError("パッドの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ InputManagerの終了処理 ]
            //---------------------
            if (InputManager is not null)
            {
                Trace.TraceInformation("InputManagerの終了処理を行います。");
                Trace.Indent();
                try
                {
                    InputCTS.Cancel();
                    InputManager.Dispose();
                    InputManager = null;
                    Trace.TraceInformation("InputManagerの終了処理を完了しました。");
                }
                catch (Exception exception5)
                {
                    Trace.TraceError(exception5.ToString());
                    Trace.TraceError("InputManagerの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ 文字コンソールの終了処理 ]
            //---------------------
            if (act文字コンソール is not null)
            {
                Trace.TraceInformation("文字コンソールの終了処理を行います。");
                Trace.Indent();
                try
                {
                    act文字コンソール.On非活性化();
                    act文字コンソール = null;
                    Trace.TraceInformation("文字コンソールの終了処理を完了しました。");
                }
                catch (Exception exception6)
                {
                    Trace.TraceError(exception6.ToString());
                    Trace.TraceError("文字コンソールの終了処理に失敗しました。");
                }
                finally
                {
                    Trace.Unindent();
                }
            }
            //---------------------
            #endregion
            #region [ FPSカウンタの終了処理 ]
            //---------------------
            Trace.TraceInformation("FPSカウンタの終了処理を行います。");
            Trace.Indent();
            try
            {
                if (FPS is not null)
                {
                    FPS = null;
                }
                Trace.TraceInformation("FPSカウンタの終了処理を完了しました。");
            }
            finally
            {
                Trace.Unindent();
            }
            //---------------------
            #endregion
            #region [ タイマの終了処理 ]
            //---------------------
            Trace.TraceInformation("タイマの終了処理を行います。");
            Trace.Indent();
            try
            {
                if (Timer is not null)
                {
                    Timer.Dispose();
                    Timer = null;
                    Trace.TraceInformation("タイマの終了処理を完了しました。");
                }
                else
                {
                    Trace.TraceInformation("タイマは使用されていません。");
                }
            }
            finally
            {
                Trace.Unindent();
            }
            //---------------------
            #endregion
            #region [ Config.iniの出力 ]
            //---------------------
            Trace.TraceInformation("Config.ini を出力します。");
            string str = strEXEのあるフォルダ + "Config.ini";
            Trace.Indent();
            try
            {
                ConfigIni.t書き出し(str);
                Trace.TraceInformation("保存しました。({0})", str);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("Config.ini の出力に失敗しました。({0})", str);
            }
            finally
            {
                Trace.Unindent();
            }

            Trace.TraceInformation("Deinitializing loudness scanning, song gain control, and sound group level control...");
            Trace.Indent();
            try
            {
                SoundGroupLevelController = null;
                SongGainController = null;
                LoudnessMetadataScanner.StopBackgroundScanning(joinImmediately: true);
                actScanningLoudness.On非活性化();
                actScanningLoudness = null;
            }
            finally
            {
                Trace.Unindent();
                Trace.TraceInformation("Deinitialized loudness scanning, song gain control, and sound group level control.");
            }

            ConfigIni = null;

            //---------------------
            #endregion
            #region [ Config.toml の出力]
            string tomlpath = strEXEのあるフォルダ + "Config.toml";
            ConfigToml.Save(tomlpath);
            #endregion
            Trace.TraceInformation("アプリケーションの終了処理を完了しました。");

            this.b終了処理完了済み = true;
        }
    }
    private void tUpdateScoreJson()
    {
        string strFilename = DTX[0].strFilenameの絶対パス + ".score.json";
        CScoreJson json = CScoreJson.Load(strFilename);
        if (!File.Exists(strFilename))
        {
            json.Title = DTX[0].TITLE;
            json.Name = DTX[0].strFilename;
        }
        json.BGMAdjust = DTX[0].nBGMAdjust;

        if (TJAPlayer3.app.ConfigToml.PlayOption.AutoPlay[0] == false)
            json.Records[TJAPlayer3.stage選曲.n確定された曲の難易度[0]].PlayCount++;

        json.Save(strFilename);
    }
    private void tガベージコレクションを実行する()
    {
        GC.Collect(GC.MaxGeneration);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration);
    }

    public void RefleshSkin()
    {
        Trace.TraceInformation("スキン変更:" + TJAPlayer3.app.Skin.GetCurrentSkinSubfolderFullName(false));

        TJAPlayer3.app.act文字コンソール.On非活性化();

        TJAPlayer3.app.Skin.Dispose();
        TJAPlayer3.app.Skin = null;
        TJAPlayer3.app.Skin = new CSkin(TJAPlayer3.app.ConfigToml.General._AbsSkinPath);


        TJAPlayer3.app.Tx.DisposeTexture();
        TJAPlayer3.app.Tx.LoadTexture();

        TJAPlayer3.app.act文字コンソール.On活性化();
    }
    #region [ Windowイベント処理 ]
    //-----------------
    private void Window_MouseWheel(object? sender, FDK.Windowing.MouseWheelEventArgs? e)
    {
        if (TJAPlayer3.r現在のステージ.eStageID == CStage.EStage.SongSelect && ConfigToml.SongSelect.EnableMouseWheel)
            TJAPlayer3.stage選曲.MouseWheel(e.x - e.y);
    }

    private void Window_ResizeOrMove(object? sender, EventArgs? e)               // #23510 2010.11.20 yyagi: to get resized window size
    {
        if (!ConfigToml.Window.FullScreen)
        {
            ConfigToml.Window.X = this.Location.X;   // #30675 2013.02.04 ikanick add
            ConfigToml.Window.Y = this.Location.Y;   //
        }

        ConfigToml.Window.Width = (ConfigToml.Window.FullScreen) ? currentClientSize.Width : this.ClientSize.Width;    // #23510 2010.10.31 yyagi add
        ConfigToml.Window.Height = (ConfigToml.Window.FullScreen) ? currentClientSize.Height : this.ClientSize.Height;
    }

    #endregion
    #endregion
}
