using ManagedBass;
using ManagedBass.Mix;

namespace FDK;

public class CSoundDeviceBASS : ISoundDevice
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

    public float CPUUsage => (float)Bass.CPUUsage;

    // マスターボリュームの制御コードは、WASAPI/ASIOで全く同じ。
    public int nMasterVolume
    {
        get
        {
            float fVolume = 0.0f;
            bool b = Bass.ChannelGetAttribute(this.hMixer, ChannelAttribute.Volume, out fVolume);
            if (!b)
            {
                Errors be = Bass.LastError;
                Trace.TraceInformation("BASS Master Volume Get Error: " + be.ToString());
            }
            return (int)(fVolume * 100);
        }
        set
        {
            bool b = Bass.ChannelSetAttribute(this.hMixer, ChannelAttribute.Volume, (float)(value / 100.0));
            if (!b)
            {
                Errors be = Bass.LastError;
                Trace.TraceInformation("BASS Master Volume Set Error: " + be.ToString());
            }
        }
    }

    public CSoundDeviceBASS(int UpdatePeriod, int BufferSizems)
    {
        Trace.TraceInformation("Start initialization of BASS");
        this.eOutputDevice = ESoundDeviceType.Unknown;
        this.nOutPutDelayms = 0;
        this.nElapsedTimems = 0;
        this.SystemTimemsWhenUpdatingElapsedTime = CTimer.nUnused;
        this.tmSystemTimer = new CTimer();

        this.libCache = new CBassLibraryLoader();

        this.bIsBASSSoundFree = true;

        // BASS の初期化。

        int nFreq = 44100;
        if (!Bass.Init(-1, nFreq, DeviceInitFlags.Default, IntPtr.Zero))
            throw new Exception(string.Format("BASS の初期化に失敗しました。(BASS_Init)[{0}]", Bass.LastError.ToString()));

        if (!Bass.Configure(Configuration.UpdatePeriod, UpdatePeriod))
        {
            Trace.TraceWarning($"BASS_SetConfig({nameof(Configuration.UpdatePeriod)}) に失敗しました。[{Bass.LastError}]");
        }
        if (!Bass.Configure(Configuration.UpdateThreads, 1))
        {
            Trace.TraceWarning($"BASS_SetConfig({nameof(Configuration.UpdateThreads)}) に失敗しました。[{Bass.LastError}]");
        }

        Bass.Configure(Configuration.PlaybackBufferLength, BufferSizems);
        Bass.Configure(Configuration.LogarithmicVolumeCurve, true);

        this.tSTREAMPROC = new StreamProcedure(StreamProc);
        this.hMainStream = Bass.CreateStream(nFreq, 2, BassFlags.Default, this.tSTREAMPROC, IntPtr.Zero);

        var flag = BassFlags.MixerNonStop | BassFlags.Decode;   // デコードのみ＝発声しない。
        this.hMixer = BassMix.CreateMixerStream(nFreq, 2, flag);

        if (this.hMixer == 0)
        {
            Errors err = Bass.LastError;
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASSミキサ(mixing)の作成に失敗しました。[{0}]", err));
        }

        // BASS ミキサーの1秒あたりのバイト数を算出。

        this.bIsBASSSoundFree = false;

        var mixerInfo = Bass.ChannelGetInfo(this.hMixer);
        int nBytesPerSample = 2;
        long nMixer_BlockAlign = mixerInfo.Channels * nBytesPerSample;
        this.nMixer_BytesPerSec = nMixer_BlockAlign * mixerInfo.Frequency;

        // 単純に、hMixerの音量をMasterVolumeとして制御しても、
        // ChannelGetData()の内容には反映されない。
        // そのため、もう一段mixerを噛ませて、一段先のmixerからChannelGetData()することで、
        // hMixerの音量制御を反映させる。
        this.hMixer_DeviceOut = BassMix.CreateMixerStream(
            nFreq, 2, flag);
        if (this.hMixer_DeviceOut == 0)
        {
            Errors errcode = Bass.LastError;
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception(string.Format("BASSミキサ(最終段)の作成に失敗しました。[{0}]", errcode));
        }
        {
            bool b1 = BassMix.MixerAddChannel(this.hMixer_DeviceOut, this.hMixer, BassFlags.Default);
            if (!b1)
            {
                Errors errcode = Bass.LastError;
                Bass.Free();
                this.bIsBASSSoundFree = true;
                throw new Exception(string.Format("BASSミキサ(最終段とmixing)の接続に失敗しました。[{0}]", errcode));
            };
        }

        this.eOutputDevice = ESoundDeviceType.BASS;

        // 出力を開始。

        if (!Bass.Start())     // 範囲外の値を指定した場合は自動的にデフォルト値に設定される。
        {
            Errors err = Bass.LastError;
            Bass.Free();
            this.bIsBASSSoundFree = true;
            throw new Exception("BASS デバイス出力開始に失敗しました。" + err.ToString());
        }
        else
        {
            Bass.GetInfo(out var info);

            this.nBufferSizems = this.nOutPutDelayms = info.Latency + BufferSizems;//求め方があっているのだろうか…

            Trace.TraceInformation("BASS デバイス出力開始:[{0}ms]", this.nOutPutDelayms);
        }

        Bass.ChannelPlay(this.hMainStream, false);

    }

    #region [ tCreateSound() ]
    public CSound tCreateSound(string strFilename, ESoundGroup soundGroup)
    {
        var sound = new CSound(soundGroup);
        sound.tBASSサウンドを作成する(strFilename, this.hMixer, this.eOutputDevice, BassFlags.Decode);
        return sound;
    }

    public void tCreateSound(string strFilename, CSound sound)
    {
        sound.tBASSサウンドを作成する(strFilename, this.hMixer, this.eOutputDevice, BassFlags.Decode);
    }
    public void tCreateSound(byte[] byArrWAVFileImage, CSound sound)
    {
        sound.tBASSサウンドを作成する(byArrWAVFileImage, this.hMixer, this.eOutputDevice, BassFlags.Decode);
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
        this.eOutputDevice = ESoundDeviceType.Unknown;      // まず出力停止する(Dispose中にクラス内にアクセスされることを防ぐ)
        if (hMainStream != -1)
        {
            Bass.StreamFree(this.hMainStream);
        }
        if (hMixer != -1)
        {
            Bass.StreamFree(this.hMixer);
        }
        if (!this.bIsBASSSoundFree)
        {
            Bass.Stop();
            Bass.Free();// システムタイマより先に呼び出すこと。（Stream処理() の中でシステムタイマを参照してるため）
        }

        if (bManagedDispose)
        {
            this.tmSystemTimer?.Dispose();
            this.libCache?.Dispose();
            this.libCache = null;
        }
    }
    ~CSoundDeviceBASS()
    {
        this.Dispose(false);
    }
    //-----------------
    #endregion

    public int StreamProc(int handle, IntPtr buffer, int length, IntPtr user)
    {
        // BASSミキサからの出力データをそのまま ASIO buffer へ丸投げ。

        int num = Bass.ChannelGetData(this.hMixer_DeviceOut, buffer, length);      // num = 実際に転送した長さ

        if (num == -1) num = 0;

        // 経過時間を更新。
        // データの転送差分ではなく累積転送バイト数から算出する。

        this.nElapsedTimems = (this.nTotalByteCount * 1000 / this.nMixer_BytesPerSec) - this.nOutPutDelayms;
        this.SystemTimemsWhenUpdatingElapsedTime = this.tmSystemTimer is null ? 0 : this.tmSystemTimer.nシステム時刻ms;


        // 経過時間を更新後に、今回分の累積転送バイト数を反映。

        this.nTotalByteCount += num;
        return num;
    }
    private long nMixer_BytesPerSec = 0;
    private long nTotalByteCount = 0;

    protected int hMainStream = -1;
    protected int hMixer = -1;
    protected int hMixer_DeviceOut = -1;
    protected StreamProcedure? tSTREAMPROC = null;
    private bool bIsBASSSoundFree = true;

    //WASAPIとASIOはLinuxでは使えないので、ここだけで良し
    private CBassLibraryLoader? libCache = null;
}
