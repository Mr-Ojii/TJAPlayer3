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

        public Game()
            : base("TJAPlayer3-f")
        {
            Instance = this;

            string osplatform = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osplatform = "win";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osplatform = "osx";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osplatform = "linux";
            }
            else
            {
                throw new PlatformNotSupportedException("TJAPlayer3-f does not support this OS.");
            }

            string platform = "";

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X64:
                    platform = "x64";
                    break;
                case Architecture.X86:
                    platform = "x86";
                    break;
                default:
                    throw new PlatformNotSupportedException($"TJAPlayer3-f does not support this Architecture. ({RuntimeInformation.ProcessArchitecture})");
            }

            FFmpeg.AutoGen.ffmpeg.RootPath = AppContext.BaseDirectory + @"ffmpeg/" + osplatform + "-" + platform + "/";

            DirectoryInfo info = new DirectoryInfo(AppContext.BaseDirectory + @"dll/" + osplatform + "-" + platform + "/");

            //exeの階層にdllをコピー
            foreach (FileInfo fileinfo in info.GetFiles())
            {
                fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//CP932用
        }
    }
}
