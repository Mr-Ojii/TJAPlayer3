using FDK;
using Tomlyn;

namespace TJAPlayer3;

public class CConfigToml
{
    public static CConfigToml Load(string FilePath)
    {
        CConfigToml ConfigToml = new();
        if (File.Exists(FilePath))
        {
            TomlModelOptions tomlModelOptions = new()
            {
                ConvertPropertyName = (x) => x,
                ConvertFieldName = (x) => x,
                IgnoreMissingProperties = true,
            };
            string? str = CJudgeTextEncoding.ReadTextFile(FilePath);
            if (str is not null)
                ConfigToml = Toml.ToModel<CConfigToml>(str, null, tomlModelOptions);

            if (ConfigToml.General.Version == TJAPlayer3.VERSION)
                ConfigToml.NotExistOrIncorrectVersion = false;
        }
        return ConfigToml;
    }
    private const int MinimumKeyboardSoundLevelIncrement = 1;
    private const int MaximumKeyboardSoundLevelIncrement = 20;
    private const int DefaultKeyboardSoundLevelIncrement = 5;

    public enum ESoundDeviceTypeForConfig
    {
        BASS = 0,
        ASIO,
        WASAPI_Exclusive,
        WASAPI_Shared,
        Unknown = 99,
    }

    public bool NotExistOrIncorrectVersion { get; private set; } = true;

    //Configから読み込ませないのならフィールドに
    public bool EnableSkinV2 = false;
    public EScrollMode ScrollMode = EScrollMode.Normal;
    public int RegSpeedBPM = 120;
    public bool OverrideScrollMode = false;
    public bool SuperHard = false;
    public int MasterVolume = 100;

    //ConfigIniの方に値だけあったので、案として
    //「譜面分岐の参考になる数値などを表示します。」
    public bool BranchGuide = false;
    //

    public CGeneral General { get; set; } = new();
    public class CGeneral
    {
        public string Version { get; set; } = "Unknown";
        public string[] ChartPath { get; set; } = new string[] { "./Songs/" };
        public string SkinPath
        {
            get
            {
                Uri uriRoot = new Uri(System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System/"));
                if (_AbsSkinPath == string.Empty)
                {
                    // Config.iniが空の状態でDTXManiaをViewerとして起動_終了すると、strSystemSkinSubfolderFullName が空の状態でここに来る。
                    // → 初期値として Default/ を設定する。
                    _AbsSkinPath = System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System/Default/");
                }
                Uri uriPath = new Uri(System.IO.Path.Combine(this._AbsSkinPath, "./"));
                string relPath = uriRoot.MakeRelativeUri(uriPath).ToString();				// 相対パスを取得
                return System.Web.HttpUtility.UrlDecode(relPath);						// デコードする
            }
            set
            {
                string absSkinPath = value;
                if (!System.IO.Path.IsPathRooted(value))
                {
                    absSkinPath = System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System");
                    absSkinPath = System.IO.Path.Combine(absSkinPath, value);
                    Uri u = new Uri(absSkinPath);
                    absSkinPath = u.AbsolutePath.ToString();	// str4内に相対パスがある場合に備える
                    absSkinPath = System.Web.HttpUtility.UrlDecode(absSkinPath);						// デコードする
                }
                if (absSkinPath[absSkinPath.Length - 1] != '/')	// フォルダ名末尾に\を必ずつけて、CSkin側と表記を統一する
                {
                    absSkinPath += '/';
                }
                this._AbsSkinPath = absSkinPath;
            }
        }
        public string _AbsSkinPath = "";
        public string FFmpegPath { get; set; } = "";
        public string FontName { get; set; } = CFontRenderer.DefaultFontName;
        public string ScreenShotExt { get; set; } = ".png";
    }

    public CWindow Window { get; set; } = new();
    public class CWindow
    {
        public bool FullScreen { get; set; } = false;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Width
        {
            get { return _Width; }
            set { _Width = Math.Clamp(value, 1, int.MaxValue); }
        }
        private int _Width = 1280;
        public int Height
        {
            get { return _Height; }
            set { _Height = Math.Clamp(value, 1, int.MaxValue); }
        }
        private int _Height = 720;
        public int BackSleep { get; set; } = -1;
        public int SleepTimePerFrame { get; set; } = -1;
        public bool VSyncWait { get; set; } = true;
    }

    public CSoundDevice SoundDevice { get; set; } = new();
    public class CSoundDevice
    {
        public int DeviceType
        {
            get { return _DeviceType; }
            set { _DeviceType = Math.Clamp(value, 0, 4); }
        }
        private int _DeviceType = (int)(OperatingSystem.IsWindows() ? ESoundDeviceTypeForConfig.WASAPI_Shared : ESoundDeviceTypeForConfig.BASS);
        public int WASAPIBufferSizeMs
        {
            get { return _WASAPIBufferSizeMs; }
            set { _WASAPIBufferSizeMs = Math.Clamp(_WASAPIBufferSizeMs, 1, 9999); }
        }
        private int _WASAPIBufferSizeMs = 2;
        public int ASIODevice { get; set; } = 0;
        public int BASSBufferSizeMs
        {
            get { return _BASSBufferSizeMS; }
            set { _BASSBufferSizeMS = Math.Clamp(value, 1, 9999); }
        }
        private int _BASSBufferSizeMS = 2;
        public bool UseOSTimer { get; set; } = false;
        public int MasterVolume
        {
            get { return _MasterVolume; }
            set { _MasterVolume = Math.Clamp(value, 0, 100); }
        }
        private int _MasterVolume = 100;
    }

    public CSoundConf Sound { get; set; } = new();
    public class CSoundConf : INotifyPropertyChanged
    {
        private bool _applyLoudnessMetadata = true;

        public bool ApplyLoudnessMetadata
        {
            get => _applyLoudnessMetadata;
            set => SetProperty(ref _applyLoudnessMetadata, value, nameof(ApplyLoudnessMetadata));
        }

        // 2018-08-28 twopointzero:
        // There exists a particular large, well-known, well-curated, and
        // regularly-updated collection of content for use with Taiko no
        // Tatsujin simulators. A statistical analysis was performed on the
        // the integrated loudness and true peak loudness of the thousands
        // of songs within this collection as of late August 2018.
        //
        // The analysis allows us to select a target loudness which
        // results in the smallest total amount of loudness adjustment
        // applied to the songs of that collection. The selected target
        // loudness should result in the least-noticeable average
        // adjustment for the most users, assuming their collection is
        // similar to the exemplar.
        //
        // The target loudness which achieves this is -7.4 LUFS.
        private double _targetLoudness = -7.4;

        public double TargetLoudness
        {
            get => _targetLoudness;
            set => SetProperty(
                ref _targetLoudness,
                Math.Clamp(value, CSound.MinimumLufs.ToDouble(), CSound.MaximumLufs.ToDouble()),
                nameof(TargetLoudness));
        }

        private bool _applySongVol = false;

        public bool ApplySongVol
        {
            get => _applySongVol;
            set => SetProperty(ref _applySongVol, value, nameof(ApplySongVol));
        }

        private int _soundEffectLevel = CSound.DefaultSoundEffectLevel;

        public int SoundEffectLevel
        {
            get => _soundEffectLevel;
            set => SetProperty(
                ref _soundEffectLevel,
                Math.Clamp(value, CSound.MinimumGroupLevel, CSound.MaximumGroupLevel),
                nameof(SoundEffectLevel));
        }

        private int _voiceLevel = CSound.DefaultVoiceLevel;

        public int VoiceLevel
        {
            get => _voiceLevel;
            set => SetProperty(
                ref _voiceLevel,
                Math.Clamp(value, CSound.MinimumGroupLevel, CSound.MaximumGroupLevel),
                nameof(VoiceLevel));
        }

        private int _songPreviewLevel = CSound.DefaultSongPreviewLevel;

        public int SongPreviewLevel
        {
            get => _songPreviewLevel;
            set => SetProperty(
                ref _songPreviewLevel,
                Math.Clamp(value, CSound.MinimumGroupLevel, CSound.MaximumGroupLevel),
                nameof(SongPreviewLevel));
        }

        private int _songPlaybackLevel = CSound.DefaultSongPlaybackLevel;

        public int SongPlaybackLevel
        {
            get => _songPlaybackLevel;
            set => SetProperty(
                ref _songPlaybackLevel,
                Math.Clamp(value, CSound.MinimumGroupLevel, CSound.MaximumGroupLevel),
                    nameof(SongPlaybackLevel));
        }

        private int _keyboardSoundLevelIncrement = DefaultKeyboardSoundLevelIncrement;

        public int KeyboardSoundLevelIncrement
        {
            get => _keyboardSoundLevelIncrement;
            set => SetProperty(
                ref _keyboardSoundLevelIncrement,
                Math.Clamp(value, MinimumKeyboardSoundLevelIncrement, MaximumKeyboardSoundLevelIncrement),
                nameof(KeyboardSoundLevelIncrement));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private bool SetProperty<T>(ref T storage, T value, string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public CLog Log { get; set; } = new();
    public class CLog
    {
        public bool SongSearch { get; set; } = true;
        public bool CreatedDisposed { get; set; } = true;
        public bool ChartDetails { get; set; } = false;
    }
    public CHitRange HitRange { get; set; } = new();
    public class CHitRange
    {
        public int Perfect
        {
            get { return _Perfect; }
            set { _Perfect = Math.Clamp(value, 1, int.MaxValue); }
        }
        private int _Perfect = 25;
        public int Good
        {
            get { return _Good; }
            set { _Good = Math.Clamp(value, 1, int.MaxValue); }
        }
        private int _Good = 75;
        public int Bad
        {
            get { return _Bad; }
            set { _Bad = Math.Clamp(value, 1, int.MaxValue); }
        }
        private int _Bad = 108;
    }

    public CSongSelect SongSelect { get; set; } = new();
    public class CSongSelect
    {
        public bool RandomPresence { get; set; } = true;
        public bool RandomIncludeSubBox { get; set; } = true;
        public bool OpenOneSide { get; set; } = false;
        public bool CountDownTimer { get; set; } = true;
        public bool TCCLikeStyle { get; set; } = false;
        public bool EnableMouseWheel { get; set; } = true;
        public int SkipCount { get; set; } = 7;
        public int BackBoxInterval { get; set; } = 15;
        public int DefaultSongSort { get; set; } = 1;
    }
    public CGame Game { get; set; } = new();
    public class CGame
    {
        public int DispMinCombo { get; set; } = 3;
        public bool ShowDebugStatus { get; set; } = false;
        public bool ShowJudgeCountDisplay { get; set; } = false;
        public bool ShowChara { get; set; } = true;
        public bool ShowDancer { get; set; } = true;
        public bool ShowRunner { get; set; } = true;
        public bool ShowMob { get; set; } = true;
        public bool ShowFooter { get; set; } = true;
        public bool ShowPuchiChara { get; set; } = true;
        public int BranchAnime { get; set; } = 1;
        public bool NoInfo { get; set; } = false;
        public bool BigNotesJudgeFrame { get; set; } = true;
        public int SubtitleDispMode
        {
            get { return (int)_SubtitleDispMode; }
            set { _SubtitleDispMode = (ESubtitleDispMode)value; }
        }
        public ESubtitleDispMode _SubtitleDispMode = ESubtitleDispMode.Compliant;
        public bool SendDiscordPlayingInformation { get; set; } = true;
        public CBackground Background { get; set; } = new();
        public class CBackground
        {
            public int BGAlpha
            {
                get { return _BGAlpha; }
                set { _BGAlpha = Math.Clamp(value, 0, 255); }
            }
            private int _BGAlpha = 100;
            public bool BGA { get; set; } = true;
            public bool Movie { get; set; } = true;
            public int ClipDispType
            {
                get { return (int)_ClipDispType; }
                set { _ClipDispType = (EClipDispType)Math.Clamp(value, 0, 3); }
            }
            public EClipDispType _ClipDispType = EClipDispType.Background;
        }
    }

    public CEnding Ending { get; set; } = new();
    public class CEnding
    {
        public int EndingAnime { get; set; } = 0;
    }

    public CPlayOption PlayOption { get; set; } = new();
    public class CPlayOption
    {
        public bool BGMSound { get; set; } = true;
        public bool PlayBGMOnlyPlaySpeedEqualsOne { get; set; } = false;
        public bool TimeStretch { get; set; } = false;
        public bool UsePanning { get; set; } = true;
        public int MusicPreTimeMs { get; set; } = 1000;
        public int GameMode
        {
            get { return (int)_GameMode; }
            set { _GameMode = (EGame)value; }
        }
        public EGame _GameMode { get; set; } = EGame.OFF;
        public int PlaySpeed
        {
            get { return _PlaySpeed; }
            set { _PlaySpeed = Math.Clamp(value, 5, 400); }
        }
        private int _PlaySpeed = 20;
        public int InputAdjustTimeMs
        {
            get { return _InputAdjustTimeMs; }
            set { _InputAdjustTimeMs = Math.Clamp(value, -1000, 1000); }
        }
        private int _InputAdjustTimeMs = 0;
        public int DefaultScoreMode
        {
            get { return _DefaultScoreMode; }
            set { _DefaultScoreMode = Math.Clamp(value, 0, 3); }
        }
        private int _DefaultScoreMode = 2;
        public int DefaultCourse
        {
            get { return _DefaultCourse; }
            set { _DefaultCourse = Math.Clamp(value, 0, 4); }
        }
        private int _DefaultCourse = 3;
        public int Risky
        {
            get { return _Risky; }
            set { _Risky = Math.Clamp(value, 0, 10); }
        }
        private int _Risky { get; set; } = 0;
        public bool Tight { get; set; } = false;
        public bool Just { get; set; } = false;
        public bool BigNotesJudge { get; set; } = true;
        public int BigNotesWaitTime { get; set; } = 25;
        public int PlayerCount
        {
            get { return _PlayerCount; }
            set { _PlayerCount = Math.Clamp(value, 1, 2); }
        }
        private int _PlayerCount = 1;
        public string[] PlayerName { get; set; } = new string[] { "1P", "2P", "3P", "4P" };
        public bool Session { get; set; } = true;
        public int[] ScrollSpeed
        {
            get { return _ScrollSpeed; }
            set { _ScrollSpeed = value.Select(x => Math.Clamp(x, 1, 2000)).ToArray(); }
        }
        private int[] _ScrollSpeed = new int[] { 10, 10, 10, 10 };
        public int[] Random
        {
            get { return _Random.Select(x => (int)x).ToArray(); }
            set { _Random = value.Select(x => (ERandomMode)x).ToArray(); }
        }
        public ERandomMode[] _Random = new ERandomMode[] { ERandomMode.OFF, ERandomMode.OFF, ERandomMode.OFF, ERandomMode.OFF };
        public int[] Stealth
        {
            get { return _Stealth.Select(x => (int)x).ToArray(); }
            set { _Stealth = value.Select(x => (EStealthMode)x).ToArray(); }
        }
        public EStealthMode[] _Stealth = new EStealthMode[] { EStealthMode.OFF, EStealthMode.OFF, EStealthMode.OFF, EStealthMode.OFF };
        public bool[] Shinuchi { get; set; } = new bool[] { false, false, false, false };
        public bool[] AutoPlay { get; set; } = new bool[] { true, true, true, true };
        public bool AutoRoll { get; set; } = true;
        public int AutoRollSpeed { get; set; } = 67;
        public int TrainingSkipMeasures { get; set; } = 5;
        public int TrainingJumpInterval { get; set; } = 750;
    }

    public Dictionary<int, string> JoystickGUID { get; set; } = new();

    public void Save(string FilePath)
    {
        using (StreamWriter sw = new StreamWriter(FilePath, false))
        {
            //コンパイル時の誤字対策のため"{0} = {1}"でnameofを使うこと
            sw.WriteLine("[{0}]", nameof(this.General));
            sw.WriteLine("# アプリケーションのバージョン");
            sw.WriteLine("# Application Version.");
            sw.WriteLine("{0} = \"{1}\"", nameof(this.General.Version), TJAPlayer3.VERSION); //ここはTJAPlayer3.VERSION
            sw.WriteLine();
            sw.WriteLine("# 譜面ファイルが格納されているフォルダへの相対パス");
            sw.WriteLine("# Pathes for Chart data.");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.General.ChartPath), string.Join(", ", this.General.ChartPath.Select(x => $"\"{x}\"")));
            sw.WriteLine();
            sw.WriteLine("# 使用スキンのフォルダ名");
            sw.WriteLine("# 例えば System/Default/Graphics/... などの場合は、SkinPath=\"./Default/\" を指定します。");
            sw.WriteLine("# Skin fonder path.");
            sw.WriteLine("# e.g. System/Default/Graphics/... -> Set SkinPath=\"./Default/\"");
            sw.WriteLine("{0} = \"{1}\"", nameof(this.General.SkinPath), this.General.SkinPath);
            sw.WriteLine();
            sw.WriteLine("# FFmpegのライブラリディレクトリ");
            sw.WriteLine("# 規定のディレクトリとは異なったディレクトリを指定したい際、FFmpegライブラリの絶対パスを指定します。");
            sw.WriteLine("# FFmpeg library path.");
            sw.WriteLine("# Absolute path to the FFmpeg library.");
            sw.WriteLine("{0} = \"{1}\"", nameof(this.General.FFmpegPath), this.General.FFmpegPath);
            sw.WriteLine();
            sw.WriteLine("# フォントレンダリングに使用するフォント名");
            sw.WriteLine("# Font name used for font rendering.");
            sw.WriteLine("{0} = \"{1}\"", nameof(this.General.FontName), this.General.FontName);
            sw.WriteLine();
            sw.WriteLine("# スクリーンショットの拡張子 (\".bmp\" or \".jpg\" or \".png\" or \".webp\")");
            sw.WriteLine("# Extension for screen shot file. (\".bmp\" or \".jpg\" or \".png\" or \".webp\")");
            sw.WriteLine("{0} = \"{1}\"", nameof(this.General.ScreenShotExt), this.General.ScreenShotExt);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.Window));
            sw.WriteLine("# フルスクリーンにするか");
            sw.WriteLine("{0} = {1}", nameof(this.Window.FullScreen), this.Window.FullScreen.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# ウィンドウモード時の位置X");
            sw.WriteLine("# X position in the window mode.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.X), this.Window.X);
            sw.WriteLine();
            sw.WriteLine("# ウィンドウモード時の位置Y");
            sw.WriteLine("# Y position in the window mode.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.Y), this.Window.Y);
            sw.WriteLine();
            sw.WriteLine("# ウインドウモード時の画面幅");
            sw.WriteLine("# A width size in the window mode.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.Width), this.Window.Width);
            sw.WriteLine();
            sw.WriteLine("# ウインドウモード時の画面高さ");
            sw.WriteLine("# A height size in the window mode.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.Height), this.Window.Height);
            sw.WriteLine();
            sw.WriteLine("# 非フォーカス時のsleep値[ms]");
            sw.WriteLine("# A sleep time[ms] while the window is inactive.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.BackSleep), this.Window.BackSleep);
            sw.WriteLine();
            sw.WriteLine("# フレーム毎のsleep値[ms] (-1でスリープ無し, 0以上で毎フレームスリープ。動画キャプチャ等で活用下さい)");
            sw.WriteLine("# A sleep time[ms] per frame.");
            sw.WriteLine("{0} = {1}", nameof(this.Window.SleepTimePerFrame), this.Window.SleepTimePerFrame);
            sw.WriteLine();
            sw.WriteLine("# 垂直帰線同期");
            sw.WriteLine("{0} = {1}", nameof(this.Window.VSyncWait), this.Window.VSyncWait.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.SoundDevice));
            sw.WriteLine("# サウンド出力方式(0=BASS, 1=ASIO, 2=WASAPI(排他), 3=WASAPI(共有))");
            sw.WriteLine("# WASAPIはVista以降のOSで使用可能。推奨方式はWASAPI。");
            sw.WriteLine("# なお、WASAPIが使用不可ならASIOを、ASIOが使用不可ならBASSを使用します。");
            sw.WriteLine("# Sound device type(0=BASS, 1=ASIO, 2=WASAPI(Exclusive), 3=WASAPI(Shared))");
            sw.WriteLine("# WASAPI can use on Vista or later OSs.");
            sw.WriteLine("# If WASAPI is not available, TJAP3-f try to use ASIO. If ASIO can't be used, TJAP3-f try to use BASS.");
            sw.WriteLine("{0} = {1}", nameof(this.SoundDevice.DeviceType), this.SoundDevice.DeviceType);
            sw.WriteLine();
            sw.WriteLine("# WASAPI使用時のサウンドバッファサイズ");
            sw.WriteLine("# (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定");
            sw.WriteLine("# WASAPI Sound Buffer Size.");
            sw.WriteLine("# (0=Use system default buffer size, 1-9999=specify the buffer size(ms) by yourself)");
            sw.WriteLine("{0} = {1}", nameof(this.SoundDevice.WASAPIBufferSizeMs), this.SoundDevice.WASAPIBufferSizeMs);
            sw.WriteLine();
            sw.WriteLine("# ASIO使用時のサウンドデバイス");
            sw.WriteLine("# 存在しないデバイスを指定すると、TJAP3-fが起動しないことがあります。");
            sw.WriteLine("# Sound device used by ASIO.");
            sw.WriteLine("# Don't specify unconnected device, as the TJAP3-f may not bootup.");
            try
            {
                string[] asiodev = CEnumerateAllAsioDevices.GetAllASIODevices();
                for (int i = 0; i < asiodev.Length; i++)
                    sw.WriteLine("# {0}: {1}", i, asiodev[i]);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
            sw.WriteLine("{0} = {1}", nameof(this.SoundDevice.ASIODevice), this.SoundDevice.ASIODevice);
            sw.WriteLine();
            sw.WriteLine("# BASS使用時のサウンドバッファサイズ");
            sw.WriteLine("# (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定");
            sw.WriteLine("# BASS Sound Buffer Size.");
            sw.WriteLine("# (0=Use system default buffer size, 1-9999=specify the buffer size(ms) by yourself)");
            sw.WriteLine("{0} = {1}", nameof(this.SoundDevice.BASSBufferSizeMs), this.SoundDevice.BASSBufferSizeMs);
            sw.WriteLine();
            sw.WriteLine("# 演奏タイマーの種類");
            sw.WriteLine("# Playback timer");
            sw.WriteLine("# (false=FDK Timer, true=System Timer)");
            sw.WriteLine("{0} = {1}", nameof(this.SoundDevice.UseOSTimer), this.SoundDevice.UseOSTimer.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("[Sound]");
            sw.WriteLine("# BS1770GAIN によるラウドネスメータの測量を適用する");
            sw.WriteLine("# Apply BS1770GAIN loudness metadata");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.ApplyLoudnessMetadata), this.Sound.ApplyLoudnessMetadata.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine($"# BS1770GAIN によるラウドネスメータの目標値 (0). ({CSound.MinimumLufs}-{CSound.MaximumLufs})");
            sw.WriteLine($"# Loudness Target in dB (decibels) relative to full scale (0). ({CSound.MinimumLufs}-{CSound.MaximumLufs})");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.TargetLoudness), this.Sound.TargetLoudness);
            sw.WriteLine();
            sw.WriteLine("# .tjaファイルのSONGVOLヘッダを音源の音量に適用する");
            sw.WriteLine("# Apply SONGVOL (0:OFF, 1:ON)");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.ApplySongVol), this.Sound.ApplySongVol.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine($"# 効果音の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine($"# Sound effect level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.SoundEffectLevel), this.Sound.SoundEffectLevel);
            sw.WriteLine();
            sw.WriteLine($"# 各ボイス、コンボボイスの音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine($"# Voice level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.VoiceLevel), this.Sound.VoiceLevel);
            sw.WriteLine();
            sw.WriteLine($"# 選曲画面のプレビュー時の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine($"# Song preview level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.SongPreviewLevel), this.Sound.SongPreviewLevel);
            sw.WriteLine();
            sw.WriteLine($"# ゲーム中の音源の音量 ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine($"# Song playback level ({CSound.MinimumGroupLevel}-{CSound.MaximumGroupLevel}%)");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.SongPlaybackLevel), this.Sound.SongPlaybackLevel);
            sw.WriteLine();
            sw.WriteLine($"# KeyBoardによる音量変更の増加量、減少量 ({MinimumKeyboardSoundLevelIncrement}-{MaximumKeyboardSoundLevelIncrement})");
            sw.WriteLine($"# Keyboard sound level increment ({MinimumKeyboardSoundLevelIncrement}-{MaximumKeyboardSoundLevelIncrement})");
            sw.WriteLine("{0} = {1}", nameof(this.Sound.KeyboardSoundLevelIncrement), this.Sound.KeyboardSoundLevelIncrement);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.Log));
            sw.WriteLine("# 曲データ検索に関するLog出力");
            sw.WriteLine("{0} = {1}", nameof(this.Log.SongSearch), this.Log.SongSearch.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 画像やサウンドの作成_解放に関するLog出力");
            sw.WriteLine("{0} = {1}", nameof(this.Log.CreatedDisposed), this.Log.CreatedDisposed.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 譜面読み込み詳細に関するLog出力");
            sw.WriteLine("{0} = {1}", nameof(this.Log.ChartDetails), this.Log.ChartDetails.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.HitRange));
            sw.WriteLine("# Perfect～Bad とみなされる範囲[ms]");
            sw.WriteLine("{0} = {1}", nameof(this.HitRange.Perfect), this.HitRange.Perfect);
            sw.WriteLine("{0} = {1}", nameof(this.HitRange.Good), this.HitRange.Good);
            sw.WriteLine("{0} = {1}", nameof(this.HitRange.Bad), this.HitRange.Bad);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.SongSelect));
            sw.WriteLine("# 選曲画面でランダム選曲を表示するか");
            sw.WriteLine("# Whether to display random songs on the song selection screen.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.RandomPresence), this.SongSelect.RandomPresence.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 片開きにするかどうか(バグの塊)");
            sw.WriteLine("# Box Open One Side.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.OpenOneSide), this.SongSelect.OpenOneSide.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# RANDOM SELECT で子BOXを検索対象に含める");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.RandomIncludeSubBox), this.SongSelect.RandomIncludeSubBox.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でのタイマーを有効にするかどうか");
            sw.WriteLine("# Enable countdown in songselect.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.CountDownTimer), this.SongSelect.CountDownTimer.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# TCC風");
            sw.WriteLine("# Enable TCC-like style.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.TCCLikeStyle), this.SongSelect.TCCLikeStyle.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でのMouseホイールの有効化");
            sw.WriteLine("# Enable mousewheel in songselect.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.EnableMouseWheel), this.SongSelect.EnableMouseWheel.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でPgUp/PgDnを押下した際のスキップ曲数");
            sw.WriteLine("# Number of songs to be skipped when PgUp/PgDn is pressed on the song selection screen.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.SkipCount), this.SongSelect.SkipCount);
            sw.WriteLine();
            sw.WriteLine("# 閉じるノードの差し込み間隔");
            sw.WriteLine("# BackBoxes Interval.");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.BackBoxInterval), this.SongSelect.BackBoxInterval);
            sw.WriteLine();
            sw.WriteLine("# デフォルトの曲ソート(0:絶対パス順, 1:ジャンル名ソートRENEWED )");
            sw.WriteLine("# 0:Path, 1:GenreName");
            sw.WriteLine("{0} = {1}", nameof(this.SongSelect.DefaultSongSort), this.SongSelect.DefaultSongSort);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.Game));
            sw.WriteLine("# 最小表示コンボ数");
            sw.WriteLine("{0} = {1}", nameof(this.Game.DispMinCombo), this.Game.DispMinCombo);
            sw.WriteLine();
            sw.WriteLine("# 演奏情報を表示する");
            sw.WriteLine("# Showing playing info on the playing screen.");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowDebugStatus), this.Game.ShowDebugStatus.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 判定数の表示");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowJudgeCountDisplay), this.Game.ShowJudgeCountDisplay.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 各画像の表示設定");
            sw.WriteLine("# キャラクター画像を表示する");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowChara), this.Game.ShowChara.ToString().ToLowerInvariant());
            sw.WriteLine("# ダンサー画像を表示する");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowDancer), this.Game.ShowDancer.ToString().ToLowerInvariant());
            sw.WriteLine("# ランナー画像を表示する");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowRunner), this.Game.ShowRunner.ToString().ToLowerInvariant());
            sw.WriteLine("# モブ画像を表示する");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowMob), this.Game.ShowMob.ToString().ToLowerInvariant());
            sw.WriteLine("# フッター画像");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowFooter), this.Game.ShowFooter.ToString().ToLowerInvariant());
            sw.WriteLine("# ぷちキャラ画像");
            sw.WriteLine("{0} = {1}", nameof(this.Game.ShowPuchiChara), this.Game.ShowPuchiChara.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 譜面分岐のアニメーション(0:7～14, 1:15)");
            sw.WriteLine("{0} = {1}", nameof(this.Game.BranchAnime), this.Game.BranchAnime);
            sw.WriteLine();
            sw.WriteLine("# NoInfo");
            sw.WriteLine("{0} = {1}", nameof(this.Game.NoInfo), this.Game.NoInfo.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 両手判定の待ち時間中に大音符を判定枠に合わせる");
            sw.WriteLine("{0} = {1}", nameof(this.Game.BigNotesJudgeFrame), this.Game.BigNotesJudgeFrame.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# サブタイトルの表示モード(0:表示しない,1:譜面準拠,2:表示する)");   // 2020.10.18 Mr-Ojii
            sw.WriteLine("# SubtitleDisplayMode(0:Off,1:Compliant,2:On)");
            sw.WriteLine("{0} = {1}", nameof(this.Game.SubtitleDispMode), this.Game.SubtitleDispMode);
            sw.WriteLine();
            sw.WriteLine("# Discordに再生中の譜面情報を送信する(0:OFF, 1:ON)");
            sw.WriteLine("# Share Playing .tja file infomation on Discord.");
            sw.WriteLine("{0} = {1}", nameof(this.Game.SendDiscordPlayingInformation), this.Game.SendDiscordPlayingInformation.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("[{0}.{1}]", nameof(this.Game), nameof(this.Game.Background));
            sw.WriteLine("# 背景画像の半透明割合(0:透明～255:不透明)");
            sw.WriteLine("# Transparency for background image in playing screen.(0:tranaparent - 255:no transparent)");
            sw.WriteLine("{0} = {1}", nameof(this.Game.Background.BGAlpha), this.Game.Background.BGAlpha);
            sw.WriteLine();
            sw.WriteLine("# 動画の表示");
            sw.WriteLine("{0} = {1}", nameof(this.Game.Background.Movie), this.Game.Background.Movie.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# BGAの表示");
            sw.WriteLine("{0} = {1}", nameof(this.Game.Background.BGA), this.Game.Background.BGA.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 動画表示モード( 0:表示しない, 1:背景のみ, 2:窓表示のみ, 3:両方)");
            sw.WriteLine("{0} = {1}", nameof(this.Game.Background.ClipDispType), this.Game.Background.ClipDispType);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.Ending));
            sw.WriteLine("# 「また遊んでね」画面(0:OFF, 1:ON, 2:Force)");
            sw.WriteLine("{0} = {1}", nameof(this.Ending.EndingAnime), this.Ending.EndingAnime);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.PlayOption));
            sw.WriteLine("# BGM の再生");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.BGMSound), this.PlayOption.BGMSound.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 演奏速度が一倍速であるときのみBGMを再生する");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.PlayBGMOnlyPlaySpeedEqualsOne), this.PlayOption.PlayBGMOnlyPlaySpeedEqualsOne.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 再生速度変更を、ピッチ変更で行うかどうか(false:ピッチ変更, true:タイムストレッチ)");
            sw.WriteLine("# Set \"false\" if you'd like to use pitch shift with PlaySpeed.");
            sw.WriteLine("# Set \"true\" for time stretch.");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.TimeStretch), this.PlayOption.TimeStretch.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 音源再生前の空白時間 (ms)");
            sw.WriteLine("# Blank time before music source to play. (ms)");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.MusicPreTimeMs), this.PlayOption.MusicPreTimeMs);
            sw.WriteLine();
            sw.WriteLine("# ゲーム(0:OFF, 1:完走!叩ききりまショー!, 2:完走!叩ききりまショー!(激辛), 3:特訓モード)");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.GameMode), this.PlayOption.GameMode);
            sw.WriteLine();
            sw.WriteLine("# 2P演奏時に左右別々に音をならすか");
            sw.WriteLine("# Use audio panning when multiplayer.");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.UsePanning), this.PlayOption.UsePanning.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 演奏速度(5～40)(→x5/20～x40/20)");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.PlaySpeed), this.PlayOption.PlaySpeed);
            sw.WriteLine();
            sw.WriteLine("# 判定タイミング調整(-1000～1000)[ms]");
            sw.WriteLine("# Revision value to adjust judgment timing.");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.InputAdjustTimeMs), this.PlayOption.InputAdjustTimeMs);
            sw.WriteLine();
            sw.WriteLine("# 譜面でスコア計算方法が指定されていない場合のデフォルトスコア計算方法(0:ドンだフルモード, 1:~AC14, 2:AC15, 3:AC16)");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.DefaultScoreMode), this.PlayOption.DefaultScoreMode);
            sw.WriteLine();
            sw.WriteLine("# デフォルトで選択される難易度");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.DefaultCourse), this.PlayOption.DefaultCourse);
            sw.WriteLine();
            sw.WriteLine("# RISKYモード(0:OFF, 1-10) 指定回数不可になると、その時点で終了するモードです。");
            sw.WriteLine("# RISKY mode. 0=OFF, 1-10 is the times of misses to be Failed.");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.Risky), this.PlayOption.Risky);
            sw.WriteLine();
            sw.WriteLine("# TIGHTモード");
            sw.WriteLine("# TIGHT mode.");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.Tight), this.PlayOption.Tight.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# JUST");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.Just), this.PlayOption.Just.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 大音符の両手判定");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.BigNotesJudge), this.PlayOption.BigNotesJudge.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 大音符の両手入力待機時間(ms)");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.BigNotesWaitTime), this.PlayOption.BigNotesWaitTime);
            sw.WriteLine();
            sw.WriteLine("# プレイ人数");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.PlayerCount), this.PlayOption.PlayerCount);
            sw.WriteLine();
            sw.WriteLine("# プレイヤーネーム");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.PlayerName), string.Join(", ", this.PlayOption.PlayerName.Select(x => $"\"{x}\"")));
            sw.WriteLine();
            sw.WriteLine("# 2人プレイの際にセッション譜面を読み込む");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.Session), this.PlayOption.Session.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# ドラム譜面スクロール速度(1:x0.1, 10:x1.0, 15:x1.5,…,2000:x200.0)");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.ScrollSpeed), string.Join(", ", this.PlayOption.ScrollSpeed));
            sw.WriteLine();
            sw.WriteLine("# RANDOMモード(0:OFF, 1:Random, 2:Mirror 3:SuperRandom, 4:HyperRandom)");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.Random), string.Join(", ", this.PlayOption.Random));
            sw.WriteLine();
            sw.WriteLine("# STEALTHモード(0:OFF, 1:ドロン, 2:ステルス)");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.Stealth), string.Join(", ", this.PlayOption.Stealth));
            sw.WriteLine();
            sw.WriteLine("# 真打モード");
            sw.WriteLine("# Fixed score mode");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.Shinuchi), string.Join(", ", this.PlayOption.Shinuchi.Select(x => x.ToString().ToLowerInvariant())));
            sw.WriteLine();
#if PLAYABLE
            sw.WriteLine("# 自動演奏");
            sw.WriteLine("{0} = [ {1} ]", nameof(this.PlayOption.AutoPlay), string.Join(", ", this.PlayOption.AutoPlay.Select(x => x.ToString().ToLowerInvariant())));
            sw.WriteLine();
#endif
            sw.WriteLine("# 自動演奏時の連打");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.AutoRoll), this.PlayOption.AutoRoll.ToString().ToLowerInvariant());
            sw.WriteLine();
            sw.WriteLine("# 自動演奏時の連打間隔(ms)");
            sw.WriteLine("# ※フレームレート以上の速度は出ません。");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.AutoRollSpeed), this.PlayOption.AutoRollSpeed);
            sw.WriteLine();
            sw.WriteLine("# 特訓モード時にPgUp/PgDnで何小節飛ばすか");
            sw.WriteLine("# マイナスにした場合、PgUp/PgDnの動作が逆となります。");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.TrainingSkipMeasures), this.PlayOption.TrainingSkipMeasures);
            sw.WriteLine();
            sw.WriteLine("# 特訓モード時にジャンプポイントに飛ばすための時間(ms)");
            sw.WriteLine("# 指定ms以内に5回縁を叩いてください");
            sw.WriteLine("{0} = {1}", nameof(this.PlayOption.TrainingJumpInterval), this.PlayOption.TrainingJumpInterval);
            sw.WriteLine();
            sw.WriteLine("[{0}]", nameof(this.JoystickGUID));
            foreach (var pair in JoystickGUID)
            {
                sw.WriteLine("{0} = {1}", pair.Key, $"\"{pair.Value}\"");
            }
            sw.WriteLine();
        }
    }
}
