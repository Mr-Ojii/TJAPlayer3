using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Runtime;
using System.Runtime.InteropServices;

namespace FDK
{
    //Linuxでの"BASS must be loaded first"のエラー解消用

    //ref:https://www.un4seen.com/forum/?topic=19378.0
    //    https://github.com/ManagedBass/ManagedBass/issues/48
    internal class CBassLibraryLoader : IDisposable
    {
        [DllImport("libdl.so")]
        static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        static extern int dlclose(IntPtr libraryHandle);

        IntPtr libraryHandle;

        public CBassLibraryLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                this.libraryHandle = dlopen(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/libbass.so", 0x101);
        }

        public void Dispose()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                dlclose(this.libraryHandle);
        }
    }
}
