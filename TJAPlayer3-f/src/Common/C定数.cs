namespace TJAPlayer3;

/// <summary>
/// 難易度。
/// </summary>
public enum Difficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
    Oni = 3,
    Edit = 4,
    Tower = 5,
    Dan = 6,
    Total
}

public enum EScrollMode
{
    Normal,
    BMSCROLL,
    HBSCROLL,
    REGULSPEED
}
public enum EGame
{
    OFF = 0,
    完走叩ききりまショー = 1,
    完走叩ききりまショー激辛 = 2,
    特訓モード = 3
}
public enum EPad			// 演奏用のenum。ここを修正するときは、次に出てくる EKeyConfigPad と EPadFlag もセットで修正すること。
{
    LRed = 0,
    RRed = 1,
    LBlue = 2,
    RBlue = 3,
    LRed2P = 4,
    RRed2P = 5,
    LBlue2P = 6,
    RBlue2P = 7,
    MAX,			// 門番用として定義
    UNKNOWN = 99
}
public enum EKeyConfigPad		// #24609 キーコンフィグで使うenum。capture要素あり。
{
    LRed = EPad.LRed,
    RRed = EPad.RRed,
    LBlue = EPad.LBlue,
    RBlue = EPad.RBlue,
    LRed2P = EPad.LRed2P,
    RRed2P = EPad.RRed2P,
    LBlue2P = EPad.LBlue2P,
    RBlue2P = EPad.RBlue2P,
    Capture,
    FullScreen,
    MAX,
    UNKNOWN = EPad.UNKNOWN
}
public enum ERandomMode
{
    OFF,
    RANDOM,
    MIRROR,
    SUPERRANDOM,
    HYPERRANDOM
}
internal enum EInputDevice
{
    KeyBoard = 0,
    MIDIInput = 1,
    Joypad = 2,
    Mouse = 3,
    Unknown = -1
}
public enum EJudge
{
    //良
    Perfect = 0,
    //可
    Good = 1,
    //叩いて不可
    Bad = 2,
    //見逃し不可
    Miss = 3,
    AutoPerfect = 4,
}
internal enum EFIFOMode
{
    FadeIn,
    FadeOut
}
internal enum E演奏画面の戻り値
{
    継続,
    演奏中断,
    ステージ失敗,
    ステージクリア,
    再読込_再演奏,
    再演奏
}
internal enum E曲読込画面の戻り値
{
    継続 = 0,
    読込完了,
    読込中止
}

public enum ENoteState
{
    none,
    waitleft,
    waitright,
}

public enum EStealthMode
{
    OFF = 0,
    DORON = 1,
    STEALTH = 2
}

[Flags]
public enum EClipDispType
{
    OFF = 0,
    Background = 1,
    Window = 2,
}

public enum ESubtitleDispMode : int
{
    Off,
    Compliant,
    On,
}

public enum EEndingAnime : int
{
    Off,
    On,
    Force
}
