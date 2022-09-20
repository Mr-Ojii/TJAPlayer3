using System;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using FDK.Windowing;

namespace FDK
{
    /// <summary>
    /// Presents an easy to use wrapper for making games and samples.
    /// </summary>
    public abstract class Game : GameWindow
    {
        internal static Game Instance = null;

        public Game(string title)
            : base(title)
        {
            Instance = this;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//CP932用
        }
    }
}
