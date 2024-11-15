namespace FDK;

#region [ DTXMania用拡張 ]
public class CSoundManager  // : CSound
{
    private static ISoundDevice? SoundDevice
    {
        get; set;
    }
    private static ESoundDeviceType SoundDeviceType
    {
        get; set;
    }
    public static CSoundTimer? rc演奏用タイマ = null;
    public static bool bUseOSTimer = false;     // OSのタイマーを使うか、CSoundTimerを使うか。DTXCではfalse, DTXManiaではtrue。
                                                // DTXCでCSoundTimerを使うと、内部で無音のループサウンドを再生するため
                                                // サウンドデバイスを占有してしまい、Viewerとして呼び出されるDTXManiaで、ASIOが使えなくなる。

    // DTXMania単体でこれをtrueにすると、WASAPI/ASIO時に演奏タイマーとしてFDKタイマーではなく
    // システムのタイマーを使うようになる。こうするとスクロールは滑らかになるが、音ズレが出るかもしれない。

    public static bool bIsTimeStretch = false;

    private static int _nMasterVolume;
    public int nMasterVolume
    {
        get
        {
            return _nMasterVolume;
        }
        set
        {
            if (SoundDevice is not null)
                SoundDevice.nMasterVolume = value;
            _nMasterVolume = value;
        }
    }

    public static int nMixing = 0;
    public static int nStreams = 0;
    #region [ WASAPI/ASIO/BASS設定値 ]
    /// <summary>
    /// <para>WASAPI 排他モード出力における再生遅延[ms]（の希望値）。最終的にはこの数値を基にドライバが決定する）。</para>
    /// <para>0以下の値を指定すると、この数値はWASAPI初期化時に自動設定する。正数を指定すると、その値を設定しようと試みる。</para>
    /// </summary>
    private static int SoundDelayExclusiveWASAPI = 0;        // SSTでは、50ms
    /// <summary>
    /// <para>WASAPI 共有モード出力における再生遅延[ms]。ユーザが決定する。</para>
    /// </summary>
    private static int SoundDelaySharedWASAPI = 100;
    /// <summary>
    /// <para>排他WASAPIバッファの更新間隔。出力間隔ではないので注意。</para>
    /// <para>→ 自動設定されるのでSoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
    /// </summary>
    private static int SoundUpdatePeriodExclusiveWASAPI = 6;
    /// <summary>
    /// <para>共有WASAPIバッファの更新間隔。出力間隔ではないので注意。</para>
    /// <para>SoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
    /// </summary>
    private static int SoundUpdatePeriodSharedWASAPI = 6;
    /// <summary>
    /// <para>WASAPI BASS出力における再生遅延[ms]。ユーザが決定する。</para>
    /// </summary>
    private static int SoundDelayBASS = 15;
    /// <para>BASSバッファの更新間隔。出力間隔ではないので注意。</para>
    /// <para>SoundDelay よりも小さい値であること。（小さすぎる場合はBASSによって自動修正される。）</para>
    /// </summary>
    private static int SoundUpdatePeriodBASS = 1;
    /// <summary>
    /// <para>ASIO 出力におけるバッファサイズ。</para>
    /// </summary>
    private static int SoundDelayASIO = 0;                       // 0にすると、デバイスの設定値をそのまま使う。
    private static int ASIODevice = 0;

    public long GetSoundDelay()
    {
        if (SoundDevice is not null)
        {
            return SoundDevice.nBufferSizems;
        }
        else
        {
            return -1;
        }
    }

    public float CPUUsage => (SoundDevice is not null ? SoundDevice.CPUUsage : 0);

    #endregion


    /// <summary>
    /// DTXMania用コンストラクタ
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="soundDeviceType"></param>
    /// <param name="nSoundDelayExclusiveWASAPI"></param>
    /// <param name="nSoundDelayASIO"></param>
    /// <param name="nASIODevice"></param>
    public CSoundManager(ESoundDeviceType soundDeviceType, int nSoundDelayExclusiveWASAPI, int nSoundDelayASIO, int nASIODevice, int nSoundDelayBASS, bool _bUseOSTimer)
    {
        SoundDevice = null;
        //bUseOSTimer = false;
        tInitialize(soundDeviceType, nSoundDelayExclusiveWASAPI, nSoundDelayASIO, nASIODevice, nSoundDelayBASS, _bUseOSTimer);
    }
    public void Dispose()
    {
        t終了();
    }
    public void tInitialize(ESoundDeviceType soundDeviceType, int _nSoundDelayExclusiveWASAPI, int _nSoundDelayASIO, int _nASIODevice, int _nSoundDelayBASS, bool _bUseOSTimer)
    {
        //SoundDevice = null;						// 後で再初期化することがあるので、null初期化はコンストラクタに回す
        rc演奏用タイマ = null;                        // Global.Bass 依存（つまりユーザ依存）
        nMixing = 0;

        SoundDelayExclusiveWASAPI = _nSoundDelayExclusiveWASAPI;
        SoundDelayASIO = _nSoundDelayASIO;
        SoundDelayBASS = _nSoundDelayBASS;
        ASIODevice = _nASIODevice;
        bUseOSTimer = _bUseOSTimer;

        ESoundDeviceType[] ESoundDeviceTypes = new ESoundDeviceType[5]
        {
            ESoundDeviceType.SharedWASAPI,
            ESoundDeviceType.ExclusiveWASAPI,
            ESoundDeviceType.ASIO,
            ESoundDeviceType.BASS,
            ESoundDeviceType.Unknown
        };

        int n初期デバイス = Array.IndexOf(ESoundDeviceTypes, soundDeviceType);
        if (n初期デバイス < 0 || n初期デバイス > ESoundDeviceTypes.Length - 1)
            n初期デバイス = ESoundDeviceTypes.Length - 1;

        for (SoundDeviceType = ESoundDeviceTypes[n初期デバイス]; ; SoundDeviceType = ESoundDeviceTypes[++n初期デバイス])
        {
            try
            {
                t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する();
                break;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues.");
                if (ESoundDeviceTypes[n初期デバイス] == ESoundDeviceType.Unknown)
                {
                    Trace.TraceError(string.Format("サウンドデバイスの初期化に失敗しました。"));
                    break;
                }
            }
        }
    }

    public static void t終了()
    {
        SoundDevice?.Dispose();
        SoundDevice = null;
        rc演奏用タイマ?.Dispose();   // Global.Bass を解放した後に解放すること。（Global.Bass で参照されているため）
        rc演奏用タイマ = null;
    }


    public static void t現在のユーザConfigに従ってサウンドデバイスとすべての既存サウンドを再構築する()
    {
        #region [ すでにサウンドデバイスと演奏タイマが構築されていれば解放する。]
        //-----------------
        if (SoundDevice is not null)
        {
            // すでに生成済みのサウンドがあれば初期状態に戻す。

            CSound.tすべてのサウンドを初期状態に戻す();     // リソースは解放するが、CSoundのインスタンスは残す。


            // サウンドデバイスと演奏タイマを解放する。

            SoundDevice?.Dispose();
            SoundDevice = null;
            rc演奏用タイマ?.Dispose();   // Global.SoundDevice を解放した後に解放すること。（Global.SoundDevice で参照されているため）
            rc演奏用タイマ = null;
        }
        //-----------------
        #endregion

        #region [ 新しいサウンドデバイスを構築する。]
        //-----------------
        switch (SoundDeviceType)
        {
            case ESoundDeviceType.ExclusiveWASAPI:
                SoundDevice = new CSoundDeviceWASAPI(CSoundDeviceWASAPI.EWASAPIMode.Exclusive, SoundDelayExclusiveWASAPI, SoundUpdatePeriodExclusiveWASAPI);
                break;

            case ESoundDeviceType.SharedWASAPI:
                SoundDevice = new CSoundDeviceWASAPI(CSoundDeviceWASAPI.EWASAPIMode.Shared, SoundDelaySharedWASAPI, SoundUpdatePeriodSharedWASAPI);
                break;

            case ESoundDeviceType.ASIO:
                SoundDevice = new CSoundDeviceASIO(SoundDelayASIO, ASIODevice);
                break;

            case ESoundDeviceType.BASS:
                SoundDevice = new CSoundDeviceBASS(SoundUpdatePeriodBASS, SoundDelayBASS);
                break;

            default:
                throw new Exception(string.Format("未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString()));
        }
        //-----------------
        #endregion
        #region [ 新しい演奏タイマを構築する。]
        //-----------------
        rc演奏用タイマ = new CSoundTimer(SoundDevice);
        //-----------------
        #endregion

        SoundDevice.nMasterVolume = _nMasterVolume;                 // サウンドデバイスに対して、マスターボリュームを再設定する

        CSound.tすべてのサウンドを再構築する(SoundDevice);        // すでに生成済みのサウンドがあれば作り直す。
    }
    public CSound? tCreateSound(string filename, ESoundGroup soundGroup)
    {
        if (!File.Exists(filename))
        {
            Trace.TraceWarning($"[i18n] File does not exist: {filename}");
            return null;
        }

        if (SoundDevice is null)
        {
            throw new Exception("SoundDevice が null です。");
        }

        if (SoundDeviceType == ESoundDeviceType.Unknown)
        {
            throw new Exception(string.Format("未対応の SoundDeviceType です。[{0}]", SoundDeviceType.ToString()));
        }
        return SoundDevice.tCreateSound(filename, soundGroup);
    }

    public string GetCurrentSoundDeviceType()
    {
        switch (SoundDeviceType)
        {
            case ESoundDeviceType.ExclusiveWASAPI:
                return "WASAPI(Exclusive)";
            case ESoundDeviceType.SharedWASAPI:
                return "WASAPI(Shared)";
            case ESoundDeviceType.ASIO:
                return "ASIO";
            case ESoundDeviceType.BASS:
                return "BASS";
            default:
                return "Unknown";
        }
    }

    public void AddMixer(CSound cs, double db再生速度, bool _b演奏終了後も再生が続くチップである)
    {
        cs.b演奏終了後も再生が続くチップである = _b演奏終了後も再生が続くチップである;
        cs.dbPlaySpeed = db再生速度;
        cs.tBASSサウンドをミキサーに追加する();
    }
    public void RemoveMixer(CSound cs)
    {
        cs.tBASSサウンドをミキサーから削除する();
    }
}
#endregion
