namespace FDK;

//Linuxでの"BASS must be loaded first"のエラー解消用

//ref:https://www.un4seen.com/forum/?topic=19378.0
//    https://github.com/ManagedBass/ManagedBass/issues/48
internal class CBassLibraryLoader : IDisposable
{
    private static class Libdl
    {
        [DllImport("libdl.so")]
        public static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        public static extern int dlclose(IntPtr libraryHandle);
    }

    private static class Libdl2
    {
        [DllImport("libdl.so.2")]
        public static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so.2")]
        public static extern int dlclose(IntPtr libraryHandle);
    }

    IntPtr libraryHandle;

    public CBassLibraryLoader()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                this.libraryHandle = Libdl.dlopen(AppContext.BaseDirectory + "libbass.so", 0x101);
            }
            catch
            {
                this.libraryHandle = Libdl2.dlopen(AppContext.BaseDirectory + "libbass.so", 0x101);
            }
        }
    }

    public void Dispose()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                Libdl.dlclose(this.libraryHandle);
            }
            catch
            {
                Libdl2.dlclose(this.libraryHandle);
            }
        }
    }
}
