using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
