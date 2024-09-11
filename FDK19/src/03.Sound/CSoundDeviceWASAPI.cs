using ManagedBass;
using ManagedBass.Wasapi;
using ManagedBass.Mix;

namespace FDK;

internal class CSoundDeviceWASAPI : ISoundDevice
{
    // プロパティ

    public ESoundDeviceType eOutputDevice
    {
        get;
        protected set;
    }
    public long nOutPutDelayms
    {
        get;
        protected set;
    }
    public long nBufferSizems
    {
        get;
        protected set;
    }
    public string? strRecordFileType = null;
    // CSoundTimer 用に公開しているプロパティ

    public long nElapsedTimems
    {
        get;
        protected set;
    }
    public long SystemTimemsWhenUpdatingElapsedTime
    {
        get;
        protected set;
    }
    public CTimer tmSystemTimer
    {
        get;
        protected set;
    }

    public float CPUUsage => (float)BassWasapi.CPUUsage;

    public enum EWASAPIMode { Exclusive, Shared }

    public int nMasterVolume
    {
        get
        {
            float fVolume = 0.0f;
            bool b = Bass.ChannelGetAttribute(this.hMixer, ChannelAttribute.Volume, out fVolume);
            if (!b)
            {
                Errors be = Bass.LastError;
                Trace.TraceInformation("WASAPI Master Volume Get Error: " + be.ToString());
            }
            else
            {
                Trace.TraceInformation("WASAPI Master Volume Get Success: " + (fVolume * 100));

            }
            return (int)(fVolume * 100);
        }
        set
        {
            bool b = Bass.ChannelSetAttribute(this.hMixer, ChannelAttribute.Volume, (float)(value / 100.0));
            // If you would like to have a volume control in exclusive mode too, and you're using the BASSmix add-on,
            // you can adjust the source's BASS_ATTRIB_VOL setting via BASS_ChannelSetAttribute.
            // しかし、hMixerに対するBASS_ChannelSetAttribute()でBASS_ATTRIB_VOLを変更: なぜか出力音量に反映されず

            if (!b)
            {
                Errors be = Bass.LastError;
                Trace.TraceInformation("WASAPI Master Volume Set Error: " + be.ToString());
            }
        }
    }
    // メソッド

    /// <summary>
    /// WASAPIの初期化
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="n希望バッファサイズms">(未使用; 本メソッド内で自動設定する)</param>
    /// <param name="n更新間隔ms">(未使用; 本メソッド内で自動設定する)</param>
    public CSoundDeviceWASAPI(EWASAPIMode mode, long n希望バッファサイズms, long n更新間隔ms)
    {
        // 初期化。

        Trace.TraceInformation("BASS (WASAPI{0}) の初期化を開始します。", mode.ToString());

        this.eOutputDevice = ESoundDeviceType.Unknown;
        this.nOutPutDelayms = 0;
        this.nElapsedTimems = 0;
        this.SystemTimemsWhenUpdatingElapsedTime = CTimer.nUnused;
        this.tmSystemTimer = new CTimer();
        this.b最初の実出力遅延算出 = true;

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("WASAPI is supported on Windows only.");

        // BASS の設定。

        this.bIsBASSSoundFree = true;
        Debug.Assert(Bass.Configure(Configuration.UpdatePeriod, 0),       // 0:BASSストリームの自動更新を行わない。（BASSWASAPIから行うため）
            string.Format("BASS_SetConfig() に失敗しました。[{0}", Bass.LastError));


        // BASS の初期化。


        int nFreq = 48000;   // 仮決め。lデバイス（≠ドライバ）がネイティブに対応している周波数であれば何でもいい？ようだ。BASSWASAPIでデバイスの周波数は変えられる。いずれにしろBASSMXで自動的にリサンプリングされる。
        // BASS_Initは、WASAPI初期化の直前に行うよう変更。WASAPIのmix周波数を使って初期化することで、余計なリサンプリング処理を省き高速化するため。
        //if( !Bass.BASS_Init( nデバイス, n周波数, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero ) )
        //	throw new Exception( string.Format( "BASS (WASAPI) の初期化に失敗しました。(BASS_Init)[{0}]", Bass.BASS_ErrorGetCode().ToString() ) );


        #region [ デバッグ用: サウンドデバイスのenumerateと、ログ出力 ]
        //(デバッグ用)
        Trace.TraceInformation("サウンドデバイス一覧:");
        int a;
        string? strDefaultSoundDeviceName = null;
        DeviceInfo[] bassDevInfos = new DeviceInfo[Bass.DeviceCount];
        for (int j = 0; j < bassDevInfos.Length; j++)
        {
            bassDevInfos[j] = Bass.GetDeviceInfo(j);
        }
        for (a = 0; a < bassDevInfos.GetLength(0); a++)
        {
            {
                Trace.TraceInformation("Sound Device #{0}: {1}: IsDefault={2}, isEnabled={3}",
                    a,
                    bassDevInfos[a].Name,
                    bassDevInfos[a].IsDefault,
                    bassDevInfos[a].IsEnabled
                );
                if (bassDevInfos[a].IsDefault)
                {
                    // これはOS標準のdefault device。後でWASAPIのdefault deviceと比較する。
                    strDefaultSoundDeviceName = bassDevInfos[a].Name;
                }
            }
        }
        #endregion

        // BASS WASAPI の初期化。

        nFreq = 0;           // デフォルトデバイスの周波数 (0="mix format" sample rate)
        int nチャンネル数 = 0;    // デフォルトデバイスのチャンネル数 (0="mix format" channels)
        this.tWasapiProc = new WasapiProcedure(this.tWASAPI処理);      // アンマネージに渡す delegate は、フィールドとして保持しておかないとGCでアドレスが変わってしまう。

        // WASAPIの更新間隔(period)は、バッファサイズにも影響を与える。
        // 更新間隔を最小にするには、BassWasapi.BASS_WASAPI_GetDeviceInfo( ndevNo ).minperiod の値を使えばよい。
        // これをやらないと、更新間隔ms=6ms となり、バッファサイズを 6ms x 4 = 24msより小さくできない。
        #region [ 既定の出力デバイスと設定されているWASAPIデバイスを検索し、更新間隔msを設定できる最小値にする ]
        int nDevNo = -1;
        WasapiDeviceInfo deviceInfo;
        for (int n = 0; BassWasapi.GetDeviceInfo(n, out deviceInfo); n++)
        {
            // #37940 2018.2.15: BASS_DEVICEINFOとBASS_WASAPI_DEVICEINFOで、IsDefaultとなっているデバイスが異なる場合がある。
            // (WASAPIでIsDefaultとなっているデバイスが正しくない場合がある)
            // そのため、BASS_DEVICEでIsDefaultとなっているものを探し、それと同じ名前のWASAPIデバイスを使用する。
            // #39490 2019.8.19: 更に、環境によっては同じ名前のWASAPIデバイスが複数定義されている場合があるため、
            // 実際に利用可能なWASAPIデバイスのみに対象を絞り込む。
            // (具体的には、defperiod, minperiod, mixchans, mixfreqがすべて0のデバイスは使用不可のため
            //  これらが0でないものを選択する)
            //if ( deviceInfo.IsDefault )
            if (deviceInfo.Name == strDefaultSoundDeviceName && deviceInfo.MixFrequency > 0)
            {
                nDevNo = n;
                #region [ 既定の出力デバイスの情報を表示 ]
                Trace.TraceInformation("WASAPI Device #{0}: {1}: IsDefault={2}, defPeriod={3}s, minperiod={4}s, mixchans={5}, mixfreq={6}",
                    n,
                    deviceInfo.Name,
                    deviceInfo.IsDefault, deviceInfo.DefaultUpdatePeriod, deviceInfo.MinimumUpdatePeriod, deviceInfo.MixChannels, deviceInfo.MixFrequency);
                #endregion
                break;
            }
        }
        if (nDevNo != -1)
        {
            Trace.TraceInformation("Start Bass_Init(device=0(fixed value: no sound), deviceInfo.mixfreq=" + deviceInfo.MixFrequency + ", BASS_DEVICE_DEFAULT, Zero)");
            if (!Bass.Init(0, deviceInfo.MixFrequency, DeviceInitFlags.Default, IntPtr.Zero))  // device = 0:"no device": BASS からはデバイスへアクセスさせない。アクセスは BASSWASAPI アドオンから行う。
                throw new Exception(string.Format("BASS (WASAPI{0}) の初期化に失敗しました。(BASS_Init)[{1}]", mode.ToString(), Bass.LastError.ToString()));

            // Trace.TraceInformation( "Selected Default WASAPI Device: {0}", deviceInfo.name );
            // Trace.TraceInformation( "MinPeriod={0}, DefaultPeriod={1}", deviceInfo.minperiod, deviceInfo.defperiod );

            // n更新間隔ms = ( mode == Eデバイスモード.排他 )?	Convert.ToInt64(Math.Ceiling(deviceInfo.minperiod * 1000.0f)) : Convert.ToInt64(Math.Ceiling(deviceInfo.defperiod * 1000.0f));
            // 更新間隔として、WASAPI排他時はminperiodより大きい最小のms値を、WASAPI共有時はdefperiodより大きい最小のms値を用いる
            // Win10では、更新間隔がminperiod以下だと、確実にBASS_ERROR_UNKNOWNとなる。

            //if ( n希望バッファサイズms <= 0 || n希望バッファサイズms < n更新間隔ms + 1 )
            //{
            //	n希望バッファサイズms = n更新間隔ms + 1; // 2013.4.25 #31237 yyagi; バッファサイズ設定の完全自動化。更新間隔＝バッファサイズにするとBASS_ERROR_UNKNOWNになるので+1する。
            //}
        }
        else
        {
            Trace.TraceError("Error: Default WASAPI Device is not found.");
        }
        #endregion

        //Retry:
        var flags = (mode == EWASAPIMode.Exclusive) ? WasapiInitFlags.AutoFormat | WasapiInitFlags.Exclusive : WasapiInitFlags.Shared | WasapiInitFlags.AutoFormat;
        //var flags = ( mode == Eデバイスモード.排他 ) ? BASSWASAPIInit.BASS_WASAPI_AUTOFORMAT | BASSWASAPIInit.BASS_WASAPI_EVENT | BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE : BASSWASAPIInit.BASS_WASAPI_AUTOFORMAT | BASSWASAPIInit.BASS_WASAPI_EVENT;

        if (mode == EWASAPIMode.Shared)
        {
            flags |= WasapiInitFlags.EventDriven;  // WASAPIをevent drivenで動作させてCPU負荷減、レイテインシ改善
        }

        nFreq = deviceInfo.MixFrequency;
        nチャンネル数 = deviceInfo.MixChannels;

        float fPeriod = (float)((mode == EWASAPIMode.Shared) ? deviceInfo.MinimumUpdatePeriod : deviceInfo.DefaultUpdatePeriod);
        float f更新間隔sec = (n更新間隔ms > 0) ? (n更新間隔ms / 1000.0f) : fPeriod;

        if (f更新間隔sec < fPeriod)
        {
            f更新間隔sec = fPeriod; // Win10では、更新間隔がminperiod以下だと、確実にBASS_ERROR_UNKNOWNとなる。
        }

        // バッファサイズは、更新間隔より大きくする必要あり。(イコールだと、WASAPI排他での初期化時にBASS_ERROR_UNKNOWNとなる)
        // そのため、最低でも、更新間隔より1ms大きく設定する。
        float f希望バッファサイズsec = (n希望バッファサイズms > 0) ? (n希望バッファサイズms / 1000.0f) : fPeriod + 0.001f;
        if (f希望バッファサイズsec < fPeriod)
        {
            f希望バッファサイズsec = fPeriod + 0.001f;
        }

        // Event Driven時は、バッファサイズは更新間隔の2倍必要
        // WASAPI排他時は、バッファサイズは更新間隔の4倍必要
        if (mode == EWASAPIMode.Exclusive)
        {
            if (!(flags.HasFlag(WasapiInitFlags.EventDriven)) &&
                f希望バッファサイズsec < f更新間隔sec * 4)
            {
                f希望バッファサイズsec = f更新間隔sec * 4;
            }
            else if (flags.HasFlag(WasapiInitFlags.EventDriven) &&
                f希望バッファサイズsec < f更新間隔sec * 2)
            {
                f希望バッファサイズsec = f更新間隔sec * 2;
            }
        }
        else if (mode == EWASAPIMode.Shared) // low latency shared mode support
        {
            // バッファ自動設定をユーザーが望む場合は、periodを最小値にする。さもなくば、バッファサイズとしてユーザーが指定した値を、periodとして用いる。
            if (n希望バッファサイズms == 0)
            {
                f更新間隔sec = (float)deviceInfo.MinimumUpdatePeriod;
            }
            else
            {
                f更新間隔sec = n希望バッファサイズms / 1000.0f;
                if (f更新間隔sec < deviceInfo.MinimumUpdatePeriod)
                {
                    f更新間隔sec = (float)deviceInfo.MinimumUpdatePeriod;
                }
            }
            f希望バッファサイズsec = 0.0f;       // in Win10 low latency shared mode support, it must be zero.
        }

        if (BassWasapi.Init(nDevNo, nFreq, nチャンネル数, flags, f希望バッファサイズsec, f更新間隔sec, this.tWasapiProc, IntPtr.Zero))
        {
            if (mode == EWASAPIMode.Exclusive)
            {
                #region [ 排他モードで作成成功。]
                //-----------------
                this.eOutputDevice = ESoundDeviceType.ExclusiveWASAPI;

                nDevNo = BassWasapi.CurrentDevice;
                deviceInfo = BassWasapi.GetDeviceInfo(nDevNo);
                BassWasapi.GetInfo(out var wasapiInfo);
                int n1サンプルのバイト数 = 2 * wasapiInfo.Channels;	// default;
                switch (wasapiInfo.Format)     // BASS WASAPI で扱うサンプルはすべて 32bit float で固定されているが、デバイスはそうとは限らない。
                {
                    case WasapiFormat.Bit8: n1サンプルのバイト数 = 1 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit16: n1サンプルのバイト数 = 2 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit24: n1サンプルのバイト数 = 3 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit32: n1サンプルのバイト数 = 4 * wasapiInfo.Channels; break;
                    case WasapiFormat.Float: n1サンプルのバイト数 = 4 * wasapiInfo.Channels; break;
                    case WasapiFormat.Unknown: throw new ArgumentOutOfRangeException($"WASAPI format error ({wasapiInfo.ToString()})");
                }
                int n1秒のバイト数 = n1サンプルのバイト数 * wasapiInfo.Frequency;
                this.nBufferSizems = (long)(wasapiInfo.BufferLength * 1000.0f / n1秒のバイト数);
                this.nOutPutDelayms = 0;	// 初期値はゼロ
                Trace.TraceInformation("使用デバイス: #" + nDevNo + " : " + deviceInfo.Name);
                Trace.TraceInformation("BASS を初期化しました。(WASAPI排他モード, {0}Hz, {1}ch, フォーマット:{2}, バッファ{3}bytes [{4}ms(希望{5}ms)], 更新間隔{6}ms)",
                    wasapiInfo.Frequency,
                    wasapiInfo.Channels,
                    wasapiInfo.Format.ToString(),
                    wasapiInfo.BufferLength,
                    nBufferSizems.ToString(),
                    (f希望バッファサイズsec * 1000).ToString(),  //n希望バッファサイズms.ToString(),
                    (f更新間隔sec * 1000).ToString()            //n更新間隔ms.ToString()
                    );
                Trace.TraceInformation("デバイスの最小更新時間={0}ms, 既定の更新時間={1}ms", deviceInfo.MinimumUpdatePeriod * 1000, deviceInfo.DefaultUpdatePeriod * 1000);
                this.bIsBASSSoundFree = false;
                //-----------------
                #endregion
            }
            else
            {
                #region [ 共有モードで作成成功。]
                //-----------------
                this.eOutputDevice = ESoundDeviceType.SharedWASAPI;

                BassWasapi.GetInfo(out var wasapiInfo);
                int n1サンプルのバイト数 = 2 * wasapiInfo.Channels;  // default;
                switch (wasapiInfo.Format)     // BASS WASAPI で扱うサンプルはすべて 32bit float で固定されているが、デバイスはそうとは限らない。
                {
                    case WasapiFormat.Bit8: n1サンプルのバイト数 = 1 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit16: n1サンプルのバイト数 = 2 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit24: n1サンプルのバイト数 = 3 * wasapiInfo.Channels; break;
                    case WasapiFormat.Bit32: n1サンプルのバイト数 = 4 * wasapiInfo.Channels; break;
                    case WasapiFormat.Float: n1サンプルのバイト数 = 4 * wasapiInfo.Channels; break;
                    case WasapiFormat.Unknown: throw new ArgumentOutOfRangeException($"WASAPI format error ({wasapiInfo.ToString()})");
                }
                int n1秒のバイト数 = n1サンプルのバイト数 * wasapiInfo.Frequency;
                this.nBufferSizems = (long)(wasapiInfo.BufferLength * 1000.0f / n1秒のバイト数);
                this.nOutPutDelayms = 0;  // 初期値はゼロ
                var devInfo = BassWasapi.GetDeviceInfo(BassWasapi.CurrentDevice); // 共有モードの場合、更新間隔はデバイスのデフォルト値に固定される。
                //Trace.TraceInformation( "BASS を初期化しました。(WASAPI共有モード, 希望バッファサイズ={0}ms, 更新間隔{1}ms)", n希望バッファサイズms, devInfo.defperiod * 1000.0f );
                Trace.TraceInformation("使用デバイス: #" + nDevNo + " : " + deviceInfo.Name);
                Trace.TraceInformation("BASS を初期化しました。(WASAPI共有モード, {0}Hz, {1}ch, フォーマット:{2}, バッファ{3}bytes [{4}ms(希望{5}ms)], 更新間隔{6}ms)",
                    wasapiInfo.Frequency,
                    wasapiInfo.Channels,
                    wasapiInfo.Format.ToString(),
                    wasapiInfo.BufferLength,
                    nBufferSizems.ToString(),
                    (f希望バッファサイズsec * 1000).ToString(),  //n希望バッファサイズms.ToString(),
                    (f更新間隔sec * 1000).ToString()            //n更新間隔ms.ToString()
                );
                Trace.TraceInformation("デバイスの最小更新時間={0}ms, 既定の更新時間={1}ms", deviceInfo.MinimumUpdatePeriod * 1000, deviceInfo.DefaultUpdatePeriod * 1000);
                this.bIsBASSSoundFree = false;
                //-----------------
                #endregion
            }
        }
        #region [ #31737 WASAPI排他モードのみ利用可能とし、WASAPI共有モードは使用できないようにするために、WASAPI共有モードでの初期化フローを削除する。 ]
        else if (mode == EWASAPIMode.Exclusive)
        {
            Errors errcode = Bass.LastError;
            Trace.TraceInformation("Failed to initialize setting BASS_WASAPI_Init (WASAPI{0}): [{1}]", mode.ToString(), errcode);
            #region [ 排他モードに失敗したのなら共有モードでリトライ。]
            //-----------------
            //	mode = Eデバイスモード.共有;
            //	goto Retry;
            //-----------------
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASS (WASAPI{0}) の初期化に失敗しました。(BASS_WASAPI_Init)[{1}]", mode.ToString(), errcode));
            #endregion
        }
        #endregion
        else
        {
            #region [ それでも失敗したら例外発生。]
            //-----------------
            Errors errcode = Bass.LastError;
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASS (WASAPI) の初期化に失敗しました。(BASS_WASAPI_Init)[{0}]", errcode));
            //-----------------
            #endregion
        }


        // WASAPI出力と同じフォーマットを持つ BASS ミキサーを作成。

        BassWasapi.GetInfo(out var info);
        this.hMixer = BassMix.CreateMixerStream(
            info.Frequency,
            info.Channels,
            BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);	// デコードのみ＝発声しない。WASAPIに出力されるだけ。
        if (this.hMixer == 0)
        {
            Errors errcode = Bass.LastError;
            BassWasapi.Free();
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASSミキサ(mixing)の作成に失敗しました。[{0}]", errcode));
        }


        // BASS ミキサーの1秒あたりのバイト数を算出。

        var mixerInfo = Bass.ChannelGetInfo(this.hMixer);
        long nMixer_BlockAlign = mixerInfo.Channels * 4;	// 4 = sizeof(FLOAT)
        this.nMixer_BytesPerSec = nMixer_BlockAlign * mixerInfo.Frequency;



        // 単純に、hMixerの音量をMasterVolumeとして制御しても、
        // ChannelGetData()の内容には反映されない。
        // そのため、もう一段mixerを噛ませて、一段先のmixerからChannelGetData()することで、
        // hMixerの音量制御を反映させる。
        this.hMixer_DeviceOut = BassMix.CreateMixerStream(
            info.Frequency,
            info.Channels,
            BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);	// デコードのみ＝発声しない。WASAPIに出力されるだけ。
        if (this.hMixer_DeviceOut == 0)
        {
            Errors errcode = Bass.LastError;
            BassWasapi.Free();
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASSミキサ(最終段)の作成に失敗しました。[{0}]", errcode));
        }

        {
            bool b1 = BassMix.MixerAddChannel(this.hMixer_DeviceOut, this.hMixer, BassFlags.Default);
            if (!b1)
            {
                Errors errcode = Bass.LastError;
                BassWasapi.Free();
                Bass.Free();
                this.bIsBASSSoundFree = true;
                throw new Exception(string.Format("BASSミキサ(最終段とmixing)の接続に失敗しました。[{0}]", errcode));
            };
        }


        // 出力を開始。

        BassWasapi.Start();
    }
    #region [ tCreateSound() ]
    public CSound tCreateSound(string strFilename, ESoundGroup soundGroup)
    {
        var sound = new CSound(soundGroup);
        sound.tBASSサウンドを作成する(strFilename, this.hMixer, this.eOutputDevice, BassFlags.Decode | BassFlags.Float);
        return sound;
    }

    public void tCreateSound(string strFilename, CSound sound)
    {
        sound.tBASSサウンドを作成する(strFilename, this.hMixer, this.eOutputDevice, BassFlags.Decode | BassFlags.Float);
    }
    public void tCreateSound(byte[] byArrWAVFileImage, CSound sound)
    {
        sound.tBASSサウンドを作成する(byArrWAVFileImage, this.hMixer, this.eOutputDevice, BassFlags.Decode | BassFlags.Float);
    }
    #endregion

    #region [ Dispose-Finallizeパターン実装 ]
    //-----------------
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected void Dispose(bool bManagedDispose)
    {
        this.eOutputDevice = ESoundDeviceType.Unknown;		// まず出力停止する(Dispose中にクラス内にアクセスされることを防ぐ)
        if (hMixer != -1)
        {
            Bass.StreamFree(this.hMixer);
        }
        if (!this.bIsBASSSoundFree)
        {
            BassWasapi.Free();	// システムタイマより先に呼び出すこと。（tWasapi処理() の中でシステムタイマを参照してるため）
            Bass.Free();
        }
        if (bManagedDispose)
        {
            this.tmSystemTimer?.Dispose();
        }
    }
    ~CSoundDeviceWASAPI()
    {
        this.Dispose(false);
    }
    //-----------------
    #endregion

    protected int hMixer = -1;
    protected int hMixer_DeviceOut = -1;
    protected WasapiProcedure? tWasapiProc = null;

    protected int tWASAPI処理(IntPtr buffer, int length, IntPtr user)
    {
        // BASSミキサからの出力データをそのまま WASAPI buffer へ丸投げ。

        int num = Bass.ChannelGetData(this.hMixer_DeviceOut, buffer, length);		// num = 実際に転送した長さ
        if (num == -1) num = 0;


        // 経過時間を更新。
        // データの転送差分ではなく累積転送バイト数から算出する。

        int n未再生バイト数 = BassWasapi.GetData(null, (int)DataFlags.Available);	// 誤差削減のため、必要となるギリギリ直前に取得する。
        this.nElapsedTimems = (this.nTotalByteCount - n未再生バイト数) * 1000 / this.nMixer_BytesPerSec;
        this.SystemTimemsWhenUpdatingElapsedTime = this.tmSystemTimer is null ? 0 : this.tmSystemTimer.nシステム時刻ms;

        // 実出力遅延を更新。
        // 未再生バイト数の平均値。

        long n今回の遅延ms = n未再生バイト数 * 1000 / this.nMixer_BytesPerSec;
        this.nOutPutDelayms = (this.b最初の実出力遅延算出) ? n今回の遅延ms : (this.nOutPutDelayms + n今回の遅延ms) / 2;
        this.b最初の実出力遅延算出 = false;


        // 経過時間を更新後に、今回分の累積転送バイト数を反映。

        this.nTotalByteCount += num;
        return num;
    }

    private long nMixer_BytesPerSec = 0;
    private long nTotalByteCount = 0;
    private bool b最初の実出力遅延算出 = true;
    private bool bIsBASSSoundFree = true;
}
