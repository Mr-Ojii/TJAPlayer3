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
        public int SoundDeviceType
        {
            get { return _SoundDeviceType; }
            set { _SoundDeviceType = Math.Clamp(value, 0, 4); }
        }
        private int _SoundDeviceType = 0;
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
        public bool OSTimer { get; set; } = false;
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
            sw.WriteLine("SoundDeviceType = {0}", this.SoundDevice.SoundDeviceType);
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
            sw.WriteLine("OSTimer = {0}", this.SoundDevice.OSTimer.ToString().ToLower());
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
            sw.WriteLine("OpenOneSide = {0}", this.SongSelect.RandomPresence.ToString().ToLower());
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
        }
    }
}