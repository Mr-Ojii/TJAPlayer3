namespace FDK;

/// <summary>
/// 大規模な変更がめんどくさかったために作ったクラス
/// </summary>
public class Device
{
    internal IntPtr window;
    internal IntPtr renderer;

    internal Device(IntPtr window, IntPtr renderer)
    {
        this.window = window;
        this.renderer = renderer;
    }
}
