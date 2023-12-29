using FDK;

namespace TJAPlayer3;

[Serializable]
internal class Cスコア
{
    // プロパティ

    public STFileInfo FileInfo;
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct STFileInfo
    {
        public string FileAbsolutePath;
        public string DirAbsolutePath;
        public DateTime LastWriteTime;
        public long FileSize;

        public STFileInfo(string FileAbsolutePath, string DirAbsolutePath, DateTime LastWriteTime, long FileSize)
        {
            this.FileAbsolutePath = FileAbsolutePath;
            this.DirAbsolutePath = DirAbsolutePath;
            this.LastWriteTime = LastWriteTime;
            this.FileSize = FileSize;
        }
    }

    public ST譜面情報 譜面情報;
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct ST譜面情報
    {
        public string Title;
        public string SubTitle;
        public string Genre;
        public double Bpm;
        public string strBGMファイル名;
        public int SongVol;
        public LoudnessMetadata? SongLoudnessMetadata;
        public bool b歌詞あり;
        public int nデモBGMオフセット;
        public bool[] b譜面が存在する;
        public bool[] b譜面分岐;
        public bool[] bPapaMamaSupport;
        public int[][] nHiScore;
        public string[][] strHiScorerName;
        public int[] nCrown;
        public int[] Level;
    }

    // コンストラクタ

    public Cスコア()
    {
        this.FileInfo = new STFileInfo("", "", DateTime.MinValue, 0L);
        this.譜面情報 = new ST譜面情報();
        this.譜面情報.Title = "";
        this.譜面情報.SubTitle = "";
        this.譜面情報.Genre = "";
        this.譜面情報.Bpm = 120.0;
        this.譜面情報.strBGMファイル名 = "";
        this.譜面情報.SongVol = CSound.DefaultSongVol;
        this.譜面情報.SongLoudnessMetadata = null;
        this.譜面情報.nデモBGMオフセット = 0;
        this.譜面情報.b譜面が存在する = new bool[(int)Difficulty.Total];
        this.譜面情報.b譜面分岐 = new bool[(int)Difficulty.Total];
        this.譜面情報.bPapaMamaSupport = new bool[(int)Difficulty.Total];
        this.譜面情報.b歌詞あり = false;
        this.譜面情報.nHiScore = new int[(int)Difficulty.Total][] { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 } };
        this.譜面情報.strHiScorerName = new string[(int)Difficulty.Total][] { new string[] { "", "", "" }, new string[] { "", "", "" }, new string[] { "", "", "" }, new string[] { "", "", "" }, new string[] { "", "", "" }, new string[] { "", "", "" }, new string[] { "", "", "" } };
        this.譜面情報.nCrown = new int[(int)Difficulty.Total];
        this.譜面情報.Level = new int[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1 };
    }
}
