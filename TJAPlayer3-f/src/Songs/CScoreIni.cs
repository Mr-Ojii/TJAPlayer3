using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using FDK;
using FDK.ExtensionMethods;

namespace TJAPlayer3;

public class CScoreIni
{
    // プロパティ

    // [File] セクション
    public STファイル stファイル;
    [StructLayout( LayoutKind.Sequential )]
    public struct STファイル
    {
        public string Title;
        public string Name;
        public int PlayCountDrums;
        // #23596 10.11.16 add ikanick-----/
        public int ClearCountDrums;
        // --------------------------------/
        public int HistoryCount;
        public string[] History;
        public int BGMAdjust;
    }

    // 演奏記録セクション（2種類）
    public STセクション stセクション;
    [StructLayout( LayoutKind.Sequential )]
    public struct STセクション
    {
        public CScoreIni.C演奏記録 HiScore;
        public CScoreIni.C演奏記録 LastPlay;   // #23595 2011.1.9 ikanick
        public CScoreIni.C演奏記録 this[ int index ]
        {
            get
            {
                switch( index )
                {
                    case 0:
                        return this.HiScore;

                    case 1:
                        return this.LastPlay;
                    //------------
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch( index )
                {
                    case 0:
                        this.HiScore = value;
                        return;

                    case 1:
                        this.LastPlay = value;
                        return;
                    //------------------
                }
                throw new IndexOutOfRangeException();
            }
        }
    }
    public enum Eセクション種別 : int
    {
        Unknown = -2,
        File = -1,
        HiScore = 0,
        LastPlayDrums = 1,
    }
    public class C演奏記録
    {
        public bool bTight;
        public bool b演奏にMIDIInputを使用した;
        public bool b演奏にKeyBoardを使用した;
        public bool b演奏にJoypadを使用した;
        public bool b演奏にMouseを使用した;
        public ERandomMode eRandom;
        public float f譜面スクロール速度;
        public int nGoodになる範囲ms;
        public int nGood数;
        public int nMiss数;
        public int nPerfectになる範囲ms;
        public int nPerfect数;
        public int nPoorになる範囲ms;
        public int nPoor数;
        public int nPerfect数_Auto含まない;
        public int nGood数_Auto含まない;
        public int nPoor数_Auto含まない;
        public int nMiss数_Auto含まない;
        public long nスコア;
        public int n連打数;
        public int n演奏速度分子;
        public int n演奏速度分母;
        public int n最大コンボ数;
        public int n全チップ数;
        public string strDTXManiaのバージョン;
        public int nRisky;		// #23559 2011.6.20 yyagi 0=OFF, 1-10=Risky
        public string 最終更新日時;
        public float fゲージ;
        public bool b途中でAutoを切り替えたか;
        public int[] n良 = new int[(int)Difficulty.Total];
        public int[] n可 = new int[(int)Difficulty.Total];
        public int[] n不可 = new int[(int)Difficulty.Total];
        public int[] nハイスコア = new int[(int)Difficulty.Total];
        public int[] nSecondScore = new int[(int)Difficulty.Total];
        public int[] nThirdScore = new int[(int)Difficulty.Total];
        public string[] strHiScorerName = new string[(int)Difficulty.Total];
        public string[] strSecondScorerName = new string[(int)Difficulty.Total];
        public string[] strThirdScorerName = new string[(int)Difficulty.Total];
        public int[] nCrown = new int[(int)Difficulty.Total];
        public Dan_C[] Dan_C;
        public Dan_C Dan_C_Gauge;

        public C演奏記録()
        {
            this.eRandom = ERandomMode.OFF;
            this.f譜面スクロール速度 = new float();
            this.f譜面スクロール速度 = 1f;
            this.n演奏速度分子 = 20;
            this.n演奏速度分母 = 20;
            this.nPerfectになる範囲ms = 34;
            this.nGoodになる範囲ms = 84;
            this.nPoorになる範囲ms = 117;
            this.strDTXManiaのバージョン = "Unknown";
            this.最終更新日時 = "";
            this.nRisky = 0;									// #23559 2011.6.20 yyagi
            this.fゲージ = 0.0f;
            this.b途中でAutoを切り替えたか = false;
            Dan_C = new Dan_C[3];
            Dan_C_Gauge = new Dan_C();
        }

        public bool b全AUTOじゃない
        {
            get
            {
                return !b全AUTOである;
            }
        }
        public bool b全AUTOである
        {
            get
            {
                return (this.n全チップ数 - this.nPerfect数_Auto含まない - this.nGood数_Auto含まない - this.nPoor数_Auto含まない - this.nMiss数_Auto含まない) == this.n全チップ数;
            }
        }
    }

    /// <summary>
    /// <para>.score.ini のファイル名（絶対パス）。</para>
    /// <para>未保存などでファイル名がない場合は null。</para>
    /// </summary>
    public string iniファイル名
    {
        get; 
        private set;
    }


    // コンストラクタ

    /// <summary>
    /// <para>初期化後にiniファイルを読み込むコンストラクタ。</para>
    /// <para>読み込んだiniに不正値があれば、それが含まれるセクションをリセットする。</para>
    /// </summary>
    public CScoreIni( string iniファイル名 )
    {
        this.iniファイル名 = null;
        this.stファイル = new STファイル();
        stファイル.Title = "";
        stファイル.Name = "";
        stファイル.History = new string[] { "", "", "", "", "" };

        this.stセクション = new STセクション();
        stセクション.HiScore = new C演奏記録();
        stセクション.LastPlay = new C演奏記録();

        this.iniファイル名 = Path.GetFileName( iniファイル名 );

        Eセクション種別 section = Eセクション種別.Unknown;

        if( !File.Exists( iniファイル名 ) )
            return;

        string str;
        Encoding inienc = CJudgeTextEncoding.JudgeFileEncoding(iniファイル名);
        using (StreamReader reader = new StreamReader(iniファイル名, inienc))
        {
            while ((str = reader.ReadLine()) != null)
            {
                str = str.Replace('\t', ' ').TrimStart(new char[] { '\t', ' ' });
                if ((str.Length == 0) || (str[0] == ';'))
                    continue;

                try
                {
                    string item;
                    string para;
                    C演奏記録 c演奏記録;
                    #region [ section ]
                    if (str[0] == '[')
                    {
                        StringBuilder builder = new StringBuilder(0x20);
                        int num = 1;
                        while ((num < str.Length) && (str[num] != ']'))
                        {
                            builder.Append(str[num++]);
                        }
                        string str2 = builder.ToString();
                        if (str2.Equals("File"))
                        {
                            section = Eセクション種別.File;
                        }
                        else if (str2.Equals("HiScore.Drums"))
                        {
                            section = Eセクション種別.HiScore;
                        }
                        // #23595 2011.1.9 ikanick
                        else if (str2.Equals("LastPlay.Drums"))
                        {
                            section = Eセクション種別.LastPlayDrums;
                        }
                        //----------------------------------------------------
                        else
                        {
                            section = Eセクション種別.Unknown;
                        }
                    }
                    #endregion
                    else
                    {
                        string[] strArray = str.Split(new char[] { '=' });
                        if (strArray.Length == 2)
                        {
                            item = strArray[0].Trim();
                            para = strArray[1].Trim();
                            switch (section)
                            {
                                case Eセクション種別.File:
                                    {
                                        if (!item.Equals("Title"))
                                        {
                                            goto Label_01C7;
                                        }
                                        this.stファイル.Title = para;
                                        continue;
                                    }
                                case Eセクション種別.HiScore:
                                case Eセクション種別.LastPlayDrums:// #23595 2011.1.9 ikanick
                                    {
                                        c演奏記録 = this.stセクション[(int)section];
                                        if (!item.Equals("Score"))
                                        {
                                            goto Label_03B9;
                                        }
                                        c演奏記録.nスコア = long.Parse(para);


                                        continue;
                                    }
                            }
                        }
                    }
                    continue;
                #region [ File section ]
                Label_01C7:
                    if (item.Equals("Name"))
                    {
                        this.stファイル.Name = para;
                    }
                    else if (item.Equals("PlayCountDrums"))
                    {
                        this.stファイル.PlayCountDrums = para.ToInt32(0, 99999999, 0);
                    }
                    // #23596 10.11.16 add ikanick------------------------------------/
                    else if (item.Equals("ClearCountDrums"))
                    {
                        this.stファイル.ClearCountDrums = para.ToInt32(0, 99999999, 0);
                    }
                    //----------------------------------------------------------------/
                    else if (item.Equals("History0"))
                    {
                        this.stファイル.History[0] = para;
                    }
                    else if (item.Equals("History1"))
                    {
                        this.stファイル.History[1] = para;
                    }
                    else if (item.Equals("History2"))
                    {
                        this.stファイル.History[2] = para;
                    }
                    else if (item.Equals("History3"))
                    {
                        this.stファイル.History[3] = para;
                    }
                    else if (item.Equals("History4"))
                    {
                        this.stファイル.History[4] = para;
                    }
                    else if (item.Equals("HistoryCount"))
                    {
                        this.stファイル.HistoryCount = para.ToInt32(0, 99999999, 0);
                    }
                    else if (item.Equals("BGMAdjust"))
                    {
                        this.stファイル.BGMAdjust = int.Parse(para);
                    }
                    continue;
                #endregion
                #region [ Score section ]
                Label_03B9:
                    if (item.Equals("HiScore1"))
                    {
                        c演奏記録.nハイスコア[0] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore2"))
                    {
                        c演奏記録.nハイスコア[1] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore3"))
                    {
                        c演奏記録.nハイスコア[2] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore4"))
                    {
                        c演奏記録.nハイスコア[3] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore5"))
                    {
                        c演奏記録.nハイスコア[4] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore6"))
                    {
                        c演奏記録.nハイスコア[5] = int.Parse(para);
                    }
                    else if (item.Equals("HiScore7"))
                    {
                        c演奏記録.nハイスコア[6] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore1"))
                    {
                        c演奏記録.nSecondScore[0] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore2"))
                    {
                        c演奏記録.nSecondScore[1] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore3"))
                    {
                        c演奏記録.nSecondScore[2] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore4"))
                    {
                        c演奏記録.nSecondScore[3] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore5"))
                    {
                        c演奏記録.nSecondScore[4] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore6"))
                    {
                        c演奏記録.nSecondScore[5] = int.Parse(para);
                    }
                    else if (item.Equals("SecondScore7"))
                    {
                        c演奏記録.nSecondScore[6] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore1"))
                    {
                        c演奏記録.nThirdScore[0] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore2"))
                    {
                        c演奏記録.nThirdScore[1] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore3"))
                    {
                        c演奏記録.nThirdScore[2] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore4"))
                    {
                        c演奏記録.nThirdScore[3] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore5"))
                    {
                        c演奏記録.nThirdScore[4] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore6"))
                    {
                        c演奏記録.nThirdScore[5] = int.Parse(para);
                    }
                    else if (item.Equals("ThirdScore7"))
                    {
                        c演奏記録.nThirdScore[6] = int.Parse(para);
                    }
                    else if (item.Equals("Perfect"))
                    {
                        c演奏記録.nPerfect数 = int.Parse(para);
                    }
                    else if (item.Equals("Good"))
                    {
                        c演奏記録.nGood数 = int.Parse(para);
                    }
                    else if (item.Equals("Poor"))
                    {
                        c演奏記録.nPoor数 = int.Parse(para);
                    }
                    else if (item.Equals("Miss"))
                    {
                        c演奏記録.nMiss数 = int.Parse(para);
                    }
                    else if (item.Equals("Roll"))
                    {
                        c演奏記録.n連打数 = int.Parse(para);
                    }
                    else if (item.Equals("MaxCombo"))
                    {
                        c演奏記録.n最大コンボ数 = int.Parse(para);
                    }
                    else if (item.Equals("TotalChips"))
                    {
                        c演奏記録.n全チップ数 = int.Parse(para);
                    }
                    else if (item.Equals("Risky"))
                    {
                        c演奏記録.nRisky = int.Parse(para);
                    }
                    else if (item.Equals("TightDrums"))
                    {
                        c演奏記録.bTight = para[0].ToBool();
                    }
                    #endregion
                    else
                    {
                        #region [ ScrollSpeedDrums ]
                        if (item.Equals("ScrollSpeedDrums"))
                        {
                            c演奏記録.f譜面スクロール速度 = (float)decimal.Parse(para);
                        }
                        #endregion
                        #region [ PlaySpeed ]
                        else if (item.Equals("PlaySpeed"))
                        {
                            string[] strArray2 = para.Split(new char[] { '/' });
                            if (strArray2.Length == 2)
                            {
                                c演奏記録.n演奏速度分子 = int.Parse(strArray2[0]);
                                c演奏記録.n演奏速度分母 = int.Parse(strArray2[1]);
                            }
                        }
                        #endregion
                        else
                        {
                            if (item.Equals("UseKeyboard"))
                            {
                                c演奏記録.b演奏にKeyBoardを使用した = para[0].ToBool();
                            }
                            else if (item.Equals("UseMIDIIN"))
                            {
                                c演奏記録.b演奏にMIDIInputを使用した = para[0].ToBool();
                            }
                            else if (item.Equals("UseJoypad"))
                            {
                                c演奏記録.b演奏にJoypadを使用した = para[0].ToBool();
                            }
                            else if (item.Equals("UseMouse"))
                            {
                                c演奏記録.b演奏にMouseを使用した = para[0].ToBool();
                            }
                            else if (item.Equals("PerfectRange"))
                            {
                                c演奏記録.nPerfectになる範囲ms = int.Parse(para);
                            }
                            else if (item.Equals("GoodRange"))
                            {
                                c演奏記録.nGoodになる範囲ms = int.Parse(para);
                            }
                            else if (item.Equals("PoorRange"))
                            {
                                c演奏記録.nPoorになる範囲ms = int.Parse(para);
                            }
                            else if (item.Equals("DTXManiaVersion"))
                            {
                                c演奏記録.strDTXManiaのバージョン = para;
                            }
                            else if (item.Equals("DateTime"))
                            {
                                c演奏記録.最終更新日時 = para;
                            }
                            else if (item.Equals("HiScore1"))
                            {
                                c演奏記録.nハイスコア[0] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore2"))
                            {
                                c演奏記録.nハイスコア[1] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore3"))
                            {
                                c演奏記録.nハイスコア[2] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore4"))
                            {
                                c演奏記録.nハイスコア[3] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore5"))
                            {
                                c演奏記録.nハイスコア[4] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore6"))
                            {
                                c演奏記録.nハイスコア[5] = int.Parse(para);
                            }
                            else if (item.Equals("HiScore7"))
                            {
                                c演奏記録.nハイスコア[6] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore1"))
                            {
                                c演奏記録.nSecondScore[0] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore2"))
                            {
                                c演奏記録.nSecondScore[1] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore3"))
                            {
                                c演奏記録.nSecondScore[2] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore4"))
                            {
                                c演奏記録.nSecondScore[3] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore5"))
                            {
                                c演奏記録.nSecondScore[4] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore6"))
                            {
                                c演奏記録.nSecondScore[5] = int.Parse(para);
                            }
                            else if (item.Equals("SecondScore7"))
                            {
                                c演奏記録.nSecondScore[6] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore1"))
                            {
                                c演奏記録.nThirdScore[0] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore2"))
                            {
                                c演奏記録.nThirdScore[1] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore3"))
                            {
                                c演奏記録.nThirdScore[2] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore4"))
                            {
                                c演奏記録.nThirdScore[3] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore5"))
                            {
                                c演奏記録.nThirdScore[4] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore6"))
                            {
                                c演奏記録.nThirdScore[5] = int.Parse(para);
                            }
                            else if (item.Equals("ThirdScore7"))
                            {
                                c演奏記録.nThirdScore[6] = int.Parse(para);
                            }
                            else if (item.Equals("HiScorerName1"))
                            {
                                c演奏記録.strHiScorerName[0] = para;
                            }
                            else if (item.Equals("HiScorerName2"))
                            {
                                c演奏記録.strHiScorerName[1] = para;
                            }
                            else if (item.Equals("HiScorerName3"))
                            {
                                c演奏記録.strHiScorerName[2] = para;
                            }
                            else if (item.Equals("HiScorerName4"))
                            {
                                c演奏記録.strHiScorerName[3] = para;
                            }
                            else if (item.Equals("HiScorerName5"))
                            {
                                c演奏記録.strHiScorerName[4] = para;
                            }
                            else if (item.Equals("HiScorerName6"))
                            {
                                c演奏記録.strHiScorerName[5] = para;
                            }
                            else if (item.Equals("HiScorerName7"))
                            {
                                c演奏記録.strHiScorerName[6] = para;
                            }
                            else if (item.Equals("SecondScorerName1"))
                            {
                                c演奏記録.strSecondScorerName[0] = para;
                            }
                            else if (item.Equals("SecondScorerName2"))
                            {
                                c演奏記録.strSecondScorerName[1] = para;
                            }
                            else if (item.Equals("SecondScorerName3"))
                            {
                                c演奏記録.strSecondScorerName[2] = para;
                            }
                            else if (item.Equals("SecondScorerName4"))
                            {
                                c演奏記録.strSecondScorerName[3] = para;
                            }
                            else if (item.Equals("SecondScorerName5"))
                            {
                                c演奏記録.strSecondScorerName[4] = para;
                            }
                            else if (item.Equals("SecondScorerName6"))
                            {
                                c演奏記録.strSecondScorerName[5] = para;
                            }
                            else if (item.Equals("SecondScorerName7"))
                            {
                                c演奏記録.strSecondScorerName[6] = para;
                            }
                            else if (item.Equals("ThirdScorerName1"))
                            {
                                c演奏記録.strThirdScorerName[0] = para;
                            }
                            else if (item.Equals("ThirdScorerName2"))
                            {
                                c演奏記録.strThirdScorerName[1] = para;
                            }
                            else if (item.Equals("ThirdScorerName3"))
                            {
                                c演奏記録.strThirdScorerName[2] = para;
                            }
                            else if (item.Equals("ThirdScorerName4"))
                            {
                                c演奏記録.strThirdScorerName[3] = para;
                            }
                            else if (item.Equals("ThirdScorerName5"))
                            {
                                c演奏記録.strThirdScorerName[4] = para;
                            }
                            else if (item.Equals("ThirdScorerName6"))
                            {
                                c演奏記録.strThirdScorerName[5] = para;
                            }
                            else if (item.Equals("ThirdScorerName7"))
                            {
                                c演奏記録.strThirdScorerName[6] = para;
                            }
                            else if (item.Equals("Crown1"))
                            {
                                c演奏記録.nCrown[0] = int.Parse(para);
                            }
                            else if (item.Equals("Crown2"))
                            {
                                c演奏記録.nCrown[1] = int.Parse(para);
                            }
                            else if (item.Equals("Crown3"))
                            {
                                c演奏記録.nCrown[2] = int.Parse(para);
                            }
                            else if (item.Equals("Crown4"))
                            {
                                c演奏記録.nCrown[3] = int.Parse(para);
                            }
                            else if (item.Equals("Crown5"))
                            {
                                c演奏記録.nCrown[4] = int.Parse(para);
                            }
                            else if (item.Equals("Crown6"))
                            {
                                c演奏記録.nCrown[5] = int.Parse(para);
                            }
                            else if (item.Equals("Crown7"))
                            {
                                c演奏記録.nCrown[6] = int.Parse(para);
                            }
                        }

                    }
                    continue;
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());
                    Trace.TraceError("読み込みを中断します。({0})", iniファイル名);
                    break;
                }
            }
        }
    }

    internal void tヒストリを追加する( string str追加文字列 )
    {
        this.stファイル.HistoryCount++;
        for( int i = 3; i >= 0; i-- )
            this.stファイル.History[ i + 1 ] = this.stファイル.History[ i ];
        DateTime now = DateTime.Now;
        this.stファイル.History[ 0 ] = string.Format( "{0:0}.{1:D2}/{2}/{3} {4}", this.stファイル.HistoryCount, now.Year % 100, now.Month, now.Day, str追加文字列 );
    }
    internal void t書き出し( string iniファイル名 )
    {
        this.iniファイル名 = Path.GetFileName( iniファイル名 );

        StreamWriter writer = new StreamWriter( iniファイル名, false, new UTF8Encoding(false));
        writer.WriteLine( "[File]" );
        writer.WriteLine( "Title={0}", this.stファイル.Title );
        writer.WriteLine( "Name={0}", this.stファイル.Name );
        writer.WriteLine( "PlayCountDrums={0}", this.stファイル.PlayCountDrums );
        writer.WriteLine( "ClearCountDrums={0}", this.stファイル.ClearCountDrums );       // #23596 10.11.16 add ikanick
        writer.WriteLine( "HistoryCount={0}", this.stファイル.HistoryCount );
        writer.WriteLine( "History0={0}", this.stファイル.History[ 0 ] );
        writer.WriteLine( "History1={0}", this.stファイル.History[ 1 ] );
        writer.WriteLine( "History2={0}", this.stファイル.History[ 2 ] );
        writer.WriteLine( "History3={0}", this.stファイル.History[ 3 ] );
        writer.WriteLine( "History4={0}", this.stファイル.History[ 4 ] );
        writer.WriteLine( "BGMAdjust={0}", this.stファイル.BGMAdjust );
        writer.WriteLine();

        for (int i = 0; i < 2; i++)
        {
            string[] strArray = { "HiScore.Drums", "LastPlay.Drums" };
            writer.WriteLine("[{0}]", strArray[i]);
            writer.WriteLine("Score={0}", this.stセクション[i].nスコア);
            writer.WriteLine("Perfect={0}", this.stセクション[i].nPerfect数);
            writer.WriteLine("Good={0}", this.stセクション[i].nGood数);
            writer.WriteLine("Poor={0}", this.stセクション[i].nPoor数);
            writer.WriteLine("Miss={0}", this.stセクション[i].nMiss数);
            writer.WriteLine("MaxCombo={0}", this.stセクション[i].n最大コンボ数);
            writer.WriteLine("TotalChips={0}", this.stセクション[i].n全チップ数);
            writer.WriteLine();
            writer.WriteLine("Risky={0}", this.stセクション[i].nRisky);
            writer.WriteLine("TightDrums={0}", this.stセクション[i].bTight ? 1 : 0);
            writer.WriteLine("RandomDrums={0}", (int)this.stセクション[i].eRandom);
            writer.WriteLine("ScrollSpeedDrums={0}", this.stセクション[i].f譜面スクロール速度);
            writer.WriteLine("PlaySpeed={0}/{1}", this.stセクション[i].n演奏速度分子, this.stセクション[i].n演奏速度分母);
            writer.WriteLine("UseKeyboard={0}", this.stセクション[i].b演奏にKeyBoardを使用した ? 1 : 0);
            writer.WriteLine("UseMIDIIN={0}", this.stセクション[i].b演奏にMIDIInputを使用した ? 1 : 0);
            writer.WriteLine("UseJoypad={0}", this.stセクション[i].b演奏にJoypadを使用した ? 1 : 0);
            writer.WriteLine("UseMouse={0}", this.stセクション[i].b演奏にMouseを使用した ? 1 : 0);
            writer.WriteLine("PerfectRange={0}", this.stセクション[i].nPerfectになる範囲ms);
            writer.WriteLine("GoodRange={0}", this.stセクション[i].nGoodになる範囲ms);
            writer.WriteLine("PoorRange={0}", this.stセクション[i].nPoorになる範囲ms);
            writer.WriteLine("DTXManiaVersion={0}", this.stセクション[i].strDTXManiaのバージョン);
            writer.WriteLine("DateTime={0}", this.stセクション[i].最終更新日時);
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"HiScore{j + 1}={this.stセクション[i].nハイスコア[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"SecondScore{j + 1}={this.stセクション[i].nSecondScore[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"ThirdScore{j + 1}={this.stセクション[i].nThirdScore[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"SecondScorerName{j + 1}={this.stセクション[i].strHiScorerName[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"SecondScorerName{j + 1}={this.stセクション[i].strSecondScorerName[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"ThirdScorerName{j + 1}={this.stセクション[i].strThirdScorerName[j]}");
            for (int j = 0; j < (int)Difficulty.Total; j++)
                writer.WriteLine($"Crown{j + 1}={this.stセクション[i].nCrown[j]}");
        }

        writer.Close();
    }
}
