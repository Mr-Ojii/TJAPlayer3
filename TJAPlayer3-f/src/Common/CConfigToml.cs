using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
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
            };
            string str = CJudgeTextEncoding.ReadTextFile(FilePath);

            foreach(var st in str.Split("\n"))
                if(st.StartsWith("Version"))
                    if(st.Split("=")[1].Trim() == $"\"{TJAPlayer3.VERSION}\"")
                    {
                        //バージョンが同じ時だけ読み込む
                        ConfigToml = Toml.ToModel<CConfigToml>(str, null, tomlModelOptions);
                        ConfigToml.NotExistOrIncorrectVersion = false;
                        break;
                    }
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

    public CGeneral General { get; set; } = new();
    public class CGeneral
    {
        public string Version { get; set; } = "Unknown";
        public string[] ChartPath { get; set; } = new string[] { "./Songs/" };
        public string SkinPath { get; set; } = "";
        [IgnoreDataMember]
        private string SkinFullPath { get; set; } = "";
        public string FontName { get; set; } = CFontRenderer.DefaultFontName;
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
        private int _DeviceType = (int)(OperatingSystem.IsWindows() ? (COS.bIsWin10OrLater() ? ESoundDeviceTypeForConfig.WASAPI_Shared : ESoundDeviceTypeForConfig.WASAPI_Exclusive) : ESoundDeviceTypeForConfig.BASS);
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
    }
    public CGame Game { get; set; } = new();
    public class CGame
    {
        public bool BGMSound { get; set; } = true;
        public int DispMinCombo { get; set; } = 3;
        public bool ShowDebugStatus { get; set; } = false;
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

    public CPlayOption PlayOption { get; set; } = new();
    public class CPlayOption
    {
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
        public int Risky
        {
            get { return _Risky; }
            set { _Risky = Math.Clamp(value, 0, 10); }
        }
        private int _Risky { get; set; } = 0;
        public bool Tight { get; set; } = false;
        public bool Just { get; set; } = false;
        public int PlayerCount
        {
            get { return _PlayerCount; }
            set { _PlayerCount = Math.Clamp(value, 1, 2); }
        }
        private int _PlayerCount = 1;
        public string[] PlayerName { get; set; } = new string[] { "1P", "2P", "3P", "4P" };
        public int[] ScrollSpeed
        {
            get { return _ScrollSpeed; }
            set { _ScrollSpeed = value.Select(x => Math.Max(0, x)).ToArray(); }
        }
        private int[] _ScrollSpeed = new int[] { 9, 9, 9, 9 };
        public int[] Random
        {
            get { return _Random.Select(x => (int)x).ToArray(); }
            set { _Random = value.Select(x => (ERandomMode)x).ToArray(); }
        }
        public ERandomMode[] _Random = new ERandomMode[] { ERandomMode.OFF, ERandomMode.OFF, ERandomMode.OFF, ERandomMode.OFF };
        public bool[] Shinuchi { get; set; } = new bool[] { false, false, false, false }; 
        public bool[] AutoPlay { get; set; } = new bool[] { true, true, true, true };
        public bool AutoRoll { get; set; } = true;
        public int AutoRollSpeed { get; set; } = 67;
    }

    public CEnding Ending { get; set; } = new();
    public class CEnding
    {
        public int EndingAnime { get; set; } = 0;
    }

    public void Save(string FilePath)
    {
        using(StreamWriter sw = new StreamWriter(FilePath, false))
        {
            sw.WriteLine("[General]");
            sw.WriteLine("# アプリケーションのバージョン");
            sw.WriteLine("# Application Version.");
            sw.WriteLine("Version = \"{0}\"", TJAPlayer3.VERSION);
            sw.WriteLine();
            sw.WriteLine("# 譜面ファイルが格納されているフォルダへの相対パス");
            sw.WriteLine("# Pathes for Chart data.");
            sw.WriteLine("ChartPath = [ {0} ]", string.Join(", ", this.General.ChartPath.Select(x => $"\"{x}\"")));
            sw.WriteLine();
            sw.WriteLine("# 使用スキンのフォルダ名");
            sw.WriteLine("# 例えば System/Default/Graphics/... などの場合は、SkinPath=\"./Default/\" を指定します。" );
            sw.WriteLine("# Skin fonder path.");
            sw.WriteLine("# e.g. System/Default/Graphics/... -> Set SkinPath=\"./Default/\"" );
            sw.WriteLine("SkinPath = \"{0}\"", this.General.SkinPath);
            sw.WriteLine();
            sw.WriteLine("# フォントレンダリングに使用するフォント名");
            sw.WriteLine("# Font name used for font rendering.");
            sw.WriteLine("FontName = \"{0}\"", this.General.FontName);
            sw.WriteLine();
            sw.WriteLine("[Window]");
            sw.WriteLine("# フルスクリーンにするか");
            sw.WriteLine("FullScreen = {0}", this.Window.FullScreen.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# ウィンドウモード時の位置X");
            sw.WriteLine("# X position in the window mode.");
            sw.WriteLine("X = {0}", this.Window.X);
            sw.WriteLine();
            sw.WriteLine("# ウィンドウモード時の位置Y" );
            sw.WriteLine("# Y position in the window mode." );
            sw.WriteLine("Y = {0}", this.Window.Y);
            sw.WriteLine();
            sw.WriteLine("# ウインドウモード時の画面幅");
            sw.WriteLine("# A width size in the window mode.");
            sw.WriteLine("Width = {0}", this.Window.Width);
            sw.WriteLine();
            sw.WriteLine("# ウインドウモード時の画面高さ");
            sw.WriteLine("# A height size in the window mode.");
            sw.WriteLine("Height = {0}", this.Window.Height);
            sw.WriteLine();
            sw.WriteLine("# 非フォーカス時のsleep値[ms]");
            sw.WriteLine("# A sleep time[ms] while the window is inactive.");
            sw.WriteLine("BackSleep = {0}", this.Window.BackSleep);
            sw.WriteLine();
            sw.WriteLine("# フレーム毎のsleep値[ms] (-1でスリープ無し, 0以上で毎フレームスリープ。動画キャプチャ等で活用下さい)" );
            sw.WriteLine("# A sleep time[ms] per frame." );
            sw.WriteLine("SleepTimePerFrame = {0}", this.Window.SleepTimePerFrame );
            sw.WriteLine();
            sw.WriteLine("# 垂直帰線同期");
            sw.WriteLine("VSyncWait = {0}", this.Window.VSyncWait.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("[SoundDevice]");
            sw.WriteLine("# サウンド出力方式(0=BASS, 1=ASIO, 2=WASAPI(排他), 3=WASAPI(共有))" );
            sw.WriteLine("# WASAPIはVista以降のOSで使用可能。推奨方式はWASAPI。" );
            sw.WriteLine("# なお、WASAPIが使用不可ならASIOを、ASIOが使用不可ならBASSを使用します。");
            sw.WriteLine("# Sound device type(0=BASS, 1=ASIO, 2=WASAPI(Exclusive), 3=WASAPI(Shared))");
            sw.WriteLine("# WASAPI can use on Vista or later OSs.");
            sw.WriteLine("# If WASAPI is not available, TJAP3-f try to use ASIO. If ASIO can't be used, TJAP3-f try to use BASS.");
            sw.WriteLine("DeviceType = {0}", this.SoundDevice.DeviceType);
            sw.WriteLine();
            sw.WriteLine("# WASAPI使用時のサウンドバッファサイズ");
            sw.WriteLine("# (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定");
            sw.WriteLine("# WASAPI Sound Buffer Size.");
            sw.WriteLine("# (0=Use system default buffer size, 1-9999=specify the buffer size(ms) by yourself)");
            sw.WriteLine("WASAPIBufferSizeMs = {0}", this.SoundDevice.WASAPIBufferSizeMs );
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
            sw.WriteLine("ASIODevice = {0}", this.SoundDevice.ASIODevice );
            sw.WriteLine();
            sw.WriteLine("# BASS使用時のサウンドバッファサイズ");
            sw.WriteLine("# (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定");
            sw.WriteLine("# BASS Sound Buffer Size.");
            sw.WriteLine("# (0=Use system default buffer size, 1-9999=specify the buffer size(ms) by yourself)");
            sw.WriteLine("BASSBufferSizeMs = {0}", this.SoundDevice.BASSBufferSizeMs);
            sw.WriteLine();
            sw.WriteLine("# 演奏タイマーの種類" );
            sw.WriteLine("# Playback timer" );
            sw.WriteLine("# (false=FDK Timer, true=System Timer)" );
            sw.WriteLine("UseOSTimer = {0}", this.SoundDevice.UseOSTimer.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("[Log]" );
            sw.WriteLine("# 曲データ検索に関するLog出力");
            sw.WriteLine("SongSearch = {0}", this.Log.SongSearch.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 画像やサウンドの作成_解放に関するLog出力");
            sw.WriteLine("CreatedDisposed = {0}", this.Log.CreatedDisposed.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 譜面読み込み詳細に関するLog出力");
            sw.WriteLine("ChartDetails = {0}", this.Log.ChartDetails.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("[HitRange]");
            sw.WriteLine("# Perfect～Bad とみなされる範囲[ms]");
            sw.WriteLine("Perfect = {0}", this.HitRange.Perfect);
            sw.WriteLine("Good = {0}", this.HitRange.Good);
            sw.WriteLine("Bad = {0}", this.HitRange.Bad);
            sw.WriteLine();
            sw.WriteLine("[SongSelect]");
            sw.WriteLine("# 選曲画面でランダム選曲を表示するか");
            sw.WriteLine("# Whether to display random songs on the song selection screen.");
            sw.WriteLine("RandomPresence = {0}", this.SongSelect.RandomPresence.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 片開きにするかどうか(バグの塊)");
            sw.WriteLine("# Box Open One Side.");
            sw.WriteLine("OpenOneSide = {0}", this.SongSelect.OpenOneSide.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# RANDOM SELECT で子BOXを検索対象に含める" );
            sw.WriteLine("RandomIncludeSubBox={0}", this.SongSelect.RandomIncludeSubBox.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でのタイマーを有効にするかどうか");
            sw.WriteLine("# Enable countdown in songselect.");
            sw.WriteLine("CountDownTimer = {0}", this.SongSelect.CountDownTimer.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# TCC風");
            sw.WriteLine("# Enable TCC-like style.");
            sw.WriteLine("TCCLikeStyle = {0}", this.SongSelect.TCCLikeStyle.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でのMouseホイールの有効化");
            sw.WriteLine("# Enable mousewheel in songselect.");
            sw.WriteLine("EnableMouseWheel = {0}", this.SongSelect.EnableMouseWheel.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 選曲画面でPgUp/PgDnを押下した際のスキップ曲数");
            sw.WriteLine("# Number of songs to be skipped when PgUp/PgDn is pressed on the song selection screen.");
            sw.WriteLine("SkipCount = {0}", this.SongSelect.SkipCount);
            sw.WriteLine();
            sw.WriteLine("# 閉じるノードの差し込み間隔");
            sw.WriteLine("# BackBoxes Interval.");
            sw.WriteLine("BackBoxInterval = {0}", this.SongSelect.BackBoxInterval);
            sw.WriteLine();
            sw.WriteLine("[Game]");
            sw.WriteLine("# BGM の再生");
            sw.WriteLine("BGMSound = {0}", this.Game.BGMSound.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 最小表示コンボ数");
            sw.WriteLine("DispMinCombo = {0}", this.Game.DispMinCombo);
            sw.WriteLine();
            sw.WriteLine( "# 演奏情報を表示する" );
            sw.WriteLine( "# Showing playing info on the playing screen." );
            sw.WriteLine( "ShowDebugStatus = {0}", this.Game.ShowDebugStatus.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("[Game.Background]");
            sw.WriteLine("# 背景画像の半透明割合(0:透明～255:不透明)" );
            sw.WriteLine("# Transparency for background image in playing screen.(0:tranaparent - 255:no transparent)" );
            sw.WriteLine("BGAlpha = {0}", this.Game.Background.BGAlpha );
            sw.WriteLine();
            sw.WriteLine("# 動画の表示" );
            sw.WriteLine("Movie = {0}", this.Game.Background.Movie.ToString().ToLower() );
            sw.WriteLine();
            sw.WriteLine("# BGAの表示" );
            sw.WriteLine("BGA = {0}", this.Game.Background.BGA.ToString().ToLower() );
            sw.WriteLine();
            sw.WriteLine("# 動画表示モード( 0:表示しない, 1:背景のみ, 2:窓表示のみ, 3:両方)" );
            sw.WriteLine("ClipDispType = {0}", this.Game.Background.ClipDispType );
            sw.WriteLine();
            sw.WriteLine("[Ending]");
            sw.WriteLine("# 「また遊んでね」画面(0:OFF, 1:ON, 2:Force)" );
            sw.WriteLine("EndingAnime={0}", this.Ending.EndingAnime );
            sw.WriteLine();
            sw.WriteLine("[PlayOption]");
            sw.WriteLine("# 演奏速度(5～40)(→x5/20～x40/20)" );
            sw.WriteLine("PlaySpeed = {0}", this.PlayOption.PlaySpeed );
            sw.WriteLine();
            sw.WriteLine("# 判定タイミング調整(-1000～1000)[ms]" );
            sw.WriteLine("# Revision value to adjust judgment timing.");
            sw.WriteLine("InputAdjustTimeMs = {0}", this.PlayOption.InputAdjustTimeMs);
            sw.WriteLine();
            sw.WriteLine("# RISKYモード(0:OFF, 1-10) 指定回数不可になると、その時点で終了するモードです。" );
            sw.WriteLine("# RISKY mode. 0=OFF, 1-10 is the times of misses to be Failed." );
            sw.WriteLine("Risky = {0}", this.PlayOption.Risky );
            sw.WriteLine();
            sw.WriteLine("# TIGHTモード" );
            sw.WriteLine("# TIGHT mode." );
            sw.WriteLine("Tight = {0}", this.PlayOption.Tight.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine( "# JUST" );
            sw.WriteLine( "Just = {0}", this.PlayOption.Just.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine( "# プレイ人数" );
            sw.WriteLine( "PlayerCount = {0}", this.PlayOption.PlayerCount );
            sw.WriteLine();
            sw.WriteLine("# プレイヤーネーム");
            sw.WriteLine("PlayerName = [ {0} ]", string.Join(", ", this.PlayOption.PlayerName.Select(x => $"\"{x}\"")));
            sw.WriteLine();
            sw.WriteLine("; ドラム譜面スクロール速度(0:x0.1, 9:x1.0, 14:x1.5,…,1999:x200.0)" );
            sw.WriteLine("ScrollSpeed = [ {0} ]", string.Join(", ", this.PlayOption.ScrollSpeed));
            sw.WriteLine();
		    sw.WriteLine("# RANDOMモード(0:OFF, 1:Random, 2:Mirror 3:SuperRandom, 4:HyperRandom)" );
            sw.WriteLine("Random = [ {0} ]", string.Join(", ", this.PlayOption.Random));
            sw.WriteLine();
            sw.WriteLine("# 真打モード");
            sw.WriteLine("# Fixed score mode");
            sw.WriteLine("Shinuchi = [ {0} ]", string.Join(", ", this.PlayOption.Shinuchi.Select(x => x.ToString().ToLower())));
		    sw.WriteLine();
#if PLAYABLE
            sw.WriteLine("# 自動演奏");
            sw.WriteLine("AutoPlay = [ {0} ]", string.Join(", ", this.PlayOption.AutoPlay.Select(x => x.ToString().ToLower())));
            sw.WriteLine();
#endif
            sw.WriteLine("# 自動演奏時の連打");
            sw.WriteLine("AutoRoll = {0}", this.PlayOption.AutoRoll.ToString().ToLower());
            sw.WriteLine();
            sw.WriteLine("# 自動演奏時の連打間隔(ms)");
            sw.WriteLine("# ※フレームレート以上の速度は出ません。");
            sw.WriteLine("AutoRollSpeed = {0}", this.PlayOption.AutoRollSpeed);
            sw.WriteLine();
        }
    }
}