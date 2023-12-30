namespace TJAPlayer3;

public class CScoreJson
{
    public static CScoreJson Load(string FilePath)
    {
        if (File.Exists(FilePath))
        {
            CScoreJson? cScoreJson = JsonSerializer.Deserialize<CScoreJson>(File.ReadAllBytes(FilePath));
            if (cScoreJson is not null)
                return cScoreJson;
        }

        return new CScoreJson();
    }

    public void Save(string FilePath)
    {
        try
        {
            File.WriteAllBytes(FilePath, JsonSerializer.SerializeToUtf8Bytes(this));
        }
        catch (Exception e)
        {
            Trace.TraceWarning(e.ToString());
        }
    }

    public string Title { get; set; } = "";
    public string Name { get; set; } = "";

    public int BGMAdjust { get; set; } = 0;

    public CDifficultyRecord[] Records { get; set; } = new CDifficultyRecord[(int)Difficulty.Total] { new(), new(), new(), new(), new(), new(), new() };

    public class CDifficultyRecord
    {
        public int PlayCount { get; set; } = 0;
        public int ClearCount { get; set; } = 0;
        public int Crown { get; set; } = 0;

        public List<CRecord> HiScore { get; set; } = new();
        public CRecord? LastPlay { get; set; } = null;
    }

    public class CRecord
    {
        public string Version { get; set; } = "Unknown";
        public DateTime DateTime { get; set; } = DateTime.MinValue;
        public bool Tight { get; set; } = false;
        public int Risky { get; set; } = 0;
        public bool Just { get; set; } = false;
        public bool InputMIDI { get; set; } = false;
        public bool InputKeyboard { get; set; } = false;
        public bool InputJoystick { get; set; } = false;
        public bool InputMouse { get; set; } = false;
        public ERandomMode Random { get; set; } = ERandomMode.OFF;
        public double ScrollSpeed { get; set; } = 1;
        public double PlaySpeed { get; set; } = 1;
        public int PerfectRange { get; set; } = 0;
        public int GoodRange { get; set; } = 0;
        public int BadRange { get; set; } = 0;
        public int PerfectCount { get; set; } = 0;
        public int GoodCount { get; set; } = 0;
        public int BadCount { get; set; } = 0;
        public int MissCount { get; set; } = 0;
        public int RollCount { get; set; } = 0;
        public long Score { get; set; } = 0;
        public int MaxCombo { get; set; } = 0;
        public double Gauge { get; set; } = 0;
        public bool Auto { get; set; } = false;
        public string PlayerName { get; set; } = "Unknown";
        public Dan_C[] DanC { get; set; } = new Dan_C[3];
        public Dan_C DanCGauge { get; set; } = new();
    }
}
