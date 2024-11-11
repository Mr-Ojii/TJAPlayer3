using FDK;
using SDL;

namespace TJAPlayer3;

internal class Program
{
    internal static string SkinName = "Unknown";
    internal static string SkinVersion = "Unknown";
    internal static string SkinCreator = "Unknown";
    internal static string Renderer = "Unknown";

    [STAThread]
    private static void Main()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        string version_string = "null";
        if (version is not null)
            version_string = version.ToString();

        Mutex mutex = new Mutex(false, "Global\\TJAPlayer3-f-Ver." + version_string);

        if (mutex.WaitOne(0, false))
        {
            string osplatform = "";
            if (OperatingSystem.IsWindows())
                osplatform = "win";
            else if (OperatingSystem.IsMacOS())
                osplatform = "osx";
            else if (OperatingSystem.IsLinux())
                osplatform = "linux";
            else
                throw new PlatformNotSupportedException("TJAPlayer3-f does not support this OS.");

            string platform = "";

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X64:
                    platform = "x64";
                    break;
                case Architecture.X86:
                    platform = "x86";
                    break;
                case Architecture.Arm:
                    platform = "arm";
                    break;
                case Architecture.Arm64:
                    platform = "arm64";
                    break;
                default:
                    throw new PlatformNotSupportedException($"TJAPlayer3-f does not support this Architecture. ({RuntimeInformation.ProcessArchitecture})");
            }

            FFmpeg.AutoGen.ffmpeg.RootPath = AppContext.BaseDirectory + @"FFmpeg/" + osplatform + "-" + platform + "/";

            var OSPlatformDirName = AppContext.BaseDirectory + @"Libs/" + osplatform + "/";

            if (Directory.Exists(OSPlatformDirName))
            {
                DirectoryInfo info = new DirectoryInfo(OSPlatformDirName);

                //実行ファイルの階層にライブラリをコピー
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
            }

            var PlatformDirName = AppContext.BaseDirectory + @"Libs/" + osplatform + "-" + platform + "/";

            if (Directory.Exists(PlatformDirName))
            {
                DirectoryInfo info = new DirectoryInfo(PlatformDirName);

                //実行ファイルの階層にライブラリをコピー
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
            }


            Trace.WriteLine("Current Directory: " + Environment.CurrentDirectory);
            Trace.WriteLine("EXEのあるフォルダ: " + AppContext.BaseDirectory);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // BEGIN #23670 2010.11.13 from: キャッチされない例外は放出せずに、ログに詳細を出力する。
            // BEGIM #24606 2011.03.08 from: DEBUG 時は例外発生箇所を直接デバッグできるようにするため、例外をキャッチしないようにする。
            //2020.04.15 Mr-Ojii DEBUG 時も例外をキャッチするようにした。
            try
            {
                using (var mania = new TJAPlayer3())
                    mania.Run();

                Trace.WriteLine("");
                Trace.WriteLine("Thank You For Playing!!!");
            }
            catch (Exception e)
            {
                Trace.WriteLine("");
                Trace.Write(e.ToString());
                Trace.WriteLine("");
                Trace.WriteLine("An error has occurred. Sorry.");
                AssemblyName asmApp = Assembly.GetExecutingAssembly().GetName();

                string name = "";
                if (asmApp.Name is not null)
                    name = asmApp.Name;

                //情報リスト
                Dictionary<string, string> errorjsonobject = new Dictionary<string, string>
                {
                    { "Name", name },
                    { "Version", version_string },
                    { "Exception", e.ToString() },
                    { "DateTime", DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.ff") },
                    { "SkinName", SkinName },
                    { "SkinVersion", SkinVersion },
                    { "SkinCreator", SkinCreator },
                    { "Renderer", Renderer },
                    { "OperatingSystem", Environment.OSVersion.ToString() },
                    { "OSDescription", RuntimeInformation.OSDescription },
                    { "OSArchitecture", RuntimeInformation.OSArchitecture.ToString() },
                    { "RuntimeIdentifier", RuntimeInformation.RuntimeIdentifier },
                    { "FrameworkDescription", RuntimeInformation.FrameworkDescription },
                    { "ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString() }
                };

                //エラーが発生したことをユーザーに知らせるため、HTMLを作成する。
                using (StreamWriter writer = new StreamWriter(AppContext.BaseDirectory + "Error.html", false, Encoding.UTF8))
                {
                    writer.WriteLine("<html>");
                    writer.WriteLine("<head>");
                    writer.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html\" charset=\"utf-8\">");
                    writer.WriteLine("<style>");
                    writer.WriteLine("<!--");
                    writer.WriteLine("table{ border-collapse: collapse; } td,th { border: 2px solid; }");
                    writer.WriteLine("-->");
                    writer.WriteLine("</style>");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.WriteLine("<h1>An error has occurred.(エラーが発生しました。)</h1>");
#if PUBLISH
                    writer.WriteLine("<p>Error information has been sent.(エラー情報を送信しました。)</p>");
#else
                    writer.WriteLine("<p>It is a local build, so it did not send any error information.(ローカルビルドのため、エラー情報を送信しませんでした。)</p>");
#endif
                    writer.WriteLine("<table>");
                    writer.WriteLine("<tbody>");
                    writer.Write("<tr>");
                    foreach (KeyValuePair<string, string> keyValuePair in errorjsonobject)
                    {
                        writer.Write($"<th>{keyValuePair.Key}</th>");
                    }
                    writer.WriteLine("</tr>");
                    writer.Write("<tr>");
                    foreach (KeyValuePair<string, string> keyValuePair in errorjsonobject)
                    {
                        writer.Write($"<td>{keyValuePair.Value}</td>");
                    }
                    writer.WriteLine("</tr>");
                    writer.WriteLine("</tbody>");
                    writer.WriteLine("</table>");
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");
                }
                CWebOpen.Open(AppContext.BaseDirectory + "Error.html");

#if PUBLISH
                //エラーの送信
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonSerializer.Serialize(errorjsonobject, new JsonSerializerOptions() { DictionaryKeyPolicy = new LowerCaseJsonNamingPolicy() }), Encoding.UTF8, "application/json");

                    var resString = client.PostAsync("https://script.google.com/macros/s/AKfycbzPWvX1cd5aDcDjs0ohgBveIxBh6wZPvGk0Xvg7xFsEsoXXUFCSUeziaVsn7uoMtm_3/exec", content).Result;
                }
#endif
            }

            if (Trace.Listeners.Count > 1)
                Trace.Listeners.RemoveAt(1);

            mutex.ReleaseMutex();
        }
        else
        {
            unsafe
            {
                var msg = Encoding.UTF8.GetBytes($"TJAPlayer3-f(Ver.{Assembly.GetExecutingAssembly().GetName().Version}) is already running.");
                var app = "TJAPlayer3-f"u8;
                fixed(byte* pmsg = msg)
                    fixed(byte* papp = app) {
                        SDL3.SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING, papp, pmsg, null);
                        Console.WriteLine(msg);
                    }
            }
        }
    }

    //渡されたのをLowerCaseにして返します
    private class LowerCaseJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => name.ToLowerInvariant();
    }
}
