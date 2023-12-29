namespace FDK;

public static class CWebOpen
{
    //ref:https://qiita.com/tsukasa_labz/items/80a94d202f5e88f1ddc0
    public static void Open(string url)
    {
        ProcessStartInfo info = new ProcessStartInfo()
        {
            FileName = url,
            UseShellExecute = true,
        };
        Process.Start(info);
    }
}
