using FDK;

namespace TJAPlayer3;

internal class CActPlayInfo : CActivity
{
    // プロパティ

    public double dbBPM;
    public readonly int[] NowMeasure = new int[2];

    // コンストラクタ

    public CActPlayInfo()
    {
    }


    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 2; i++)
        {
            NowMeasure[i] = 0;
        }
        this.dbBPM = TJAPlayer3.DTX[0].BASEBPM;
        base.On活性化();
    }
    public override int On進行描画()
    {
        throw new InvalidOperationException("t進行描画(int x, int y) のほうを使用してください。");
    }
    public void t進行描画(int x, int y)
    {
        if (base.b活性化してない)
            return;

        int lastChipTime = (TJAPlayer3.DTX[0].listChip.Count > 0) ? TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms : 0;

        if (CSoundManager.rc演奏用タイマ is null)
            return;

        string[] infoList = new string[]
        {
            string.Format("SCROLLMODE:    {0:####0}", Enum.GetName(typeof(EScrollMode), TJAPlayer3.app.ConfigToml.ScrollMode)),
            string.Format("SCOREMODE:     {0:####0}", TJAPlayer3.DTX[0].nScoreModeTmp),
            string.Format("SCROLL:        {0:####0.00}/{1:####0.00}", TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[0] * 0.1, TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[1] * 0.1),
            string.Format("NoteC:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[3]),
            string.Format("NoteM:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[2]),
            string.Format("NoteE:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[1]),
            string.Format("NoteN:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[0]),
            string.Format("Frame:         {0:####0} fps", TJAPlayer3.app.FPS.nFPS),
            string.Format("BPM:           {0:####0.0000}", this.dbBPM),
            string.Format("Part:          {0:####0}/{1:####0}", NowMeasure[0], NowMeasure[1]),
            string.Format("Time:          {0:####0.00}/{1:####0.00}", ((double)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed) / 20.0))) / 1000.0, ((double)lastChipTime) / 1000.0),
            string.Format("BGM/Taiko Adj: {0:####0}/{1:####0} ms", TJAPlayer3.DTX[0].nBGMAdjust, TJAPlayer3.app.ConfigToml.PlayOption.InputAdjustTimeMs),
            string.Format("Sound CPU :    {0:####0.00}%", TJAPlayer3.SoundManager.CPUUsage ),
        };

        for (int i = 0; i < infoList.Length; i++)
        {
            TJAPlayer3.app.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, infoList[i]);
            y += 15;
        }
    }
}
