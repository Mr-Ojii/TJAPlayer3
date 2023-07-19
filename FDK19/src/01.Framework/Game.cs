using System;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using FDK.Windowing;

namespace FDK;

/// <summary>
/// Presents an easy to use wrapper for making games and samples.
/// </summary>
public abstract class Game : GameWindow
{
    public Game(string title, int width, int height)
        : base(title, width, height)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//CP932用
    }
}
