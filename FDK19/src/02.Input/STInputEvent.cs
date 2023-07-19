using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FDK;

// struct
[StructLayout( LayoutKind.Sequential )]
public struct STInputEvent
{
    public int nKey { get; set; }
    public bool bPressed { get; set; }
    public bool bReleased { get; set; }
    public long nTimeStamp { get; set; }
}
