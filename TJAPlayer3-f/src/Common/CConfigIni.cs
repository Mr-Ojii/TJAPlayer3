using FDK;

namespace TJAPlayer3;

internal class CConfigIni
{
    // クラス

    #region [ CKeyAssign ]
    public class CKeyAssign
    {
        public CConfigIni.CKeyAssign.STKEYASSIGN[] FullScreen;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] Capture;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftRed;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] RightRed;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftBlue;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] RightBlue;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftRed2P;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] RightRed2P;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] LeftBlue2P;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] RightBlue2P;
        public CConfigIni.CKeyAssign.STKEYASSIGN[] this[int index]
        {
            get
            {
                switch (index)
                {
                    case (int)EKeyConfigPad.LRed:
                        return this.LeftRed;

                    case (int)EKeyConfigPad.RRed:
                        return this.RightRed;

                    case (int)EKeyConfigPad.LBlue:
                        return this.LeftBlue;

                    case (int)EKeyConfigPad.RBlue:
                        return this.RightBlue;

                    case (int)EKeyConfigPad.LRed2P:
                        return this.LeftRed2P;

                    case (int)EKeyConfigPad.RRed2P:
                        return this.RightRed2P;

                    case (int)EKeyConfigPad.LBlue2P:
                        return this.LeftBlue2P;

                    case (int)EKeyConfigPad.RBlue2P:
                        return this.RightBlue2P;

                    case (int)EKeyConfigPad.Capture:
                        return this.Capture;

                    case (int)EKeyConfigPad.FullScreen:
                        return this.FullScreen;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case (int)EKeyConfigPad.LRed:
                        this.LeftRed = value;
                        return;

                    case (int)EKeyConfigPad.RRed:
                        this.RightRed = value;
                        return;

                    case (int)EKeyConfigPad.LBlue:
                        this.LeftBlue = value;
                        return;

                    case (int)EKeyConfigPad.RBlue:
                        this.RightBlue = value;
                        return;

                    case (int)EKeyConfigPad.LRed2P:
                        this.LeftRed2P = value;
                        return;

                    case (int)EKeyConfigPad.RRed2P:
                        this.RightRed2P = value;
                        return;

                    case (int)EKeyConfigPad.LBlue2P:
                        this.LeftBlue2P = value;
                        return;

                    case (int)EKeyConfigPad.RBlue2P:
                        this.RightBlue2P = value;
                        return;

                    case (int)EKeyConfigPad.Capture:
                        this.Capture = value;
                        return;

                    case (int)EKeyConfigPad.FullScreen:
                        this.FullScreen = value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STKEYASSIGN
        {
            public EInputDevice DeviceType;
            public int ID;
            public int Code;
            public STKEYASSIGN(EInputDevice eDeviceType, int nID, int nCode)
            {
                this.DeviceType = eDeviceType;
                this.ID = nID;
                this.Code = nCode;
            }
            public STKEYASSIGN(string str)
            {
                this.DeviceType = EInputDevice.Unknown;
                str = str.Trim().ToUpperInvariant();
                if (str.Length < 3)
                    return;

                switch (str[0])
                {
                    case 'J':
                        this.DeviceType = EInputDevice.Joypad;
                        break;
                    case 'K':
                        this.DeviceType = EInputDevice.KeyBoard;
                        break;
                    case 'M':
                        this.DeviceType = EInputDevice.MIDIInput;
                        break;
                    case 'N':
                        this.DeviceType = EInputDevice.Mouse;
                        break;
                }
                this.ID = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(str[1]);  // #24166 2011.1.15 yyagi: to support ID > 10, change 2nd character from Decimal to 36-numeral system. (e.g. J1023 -> JA23)
                if (int.TryParse(str.Substring(2), out var code))
                    this.Code = code;
                else
                    this.Code = -1;
            }

            public override string ToString()
            {
                if (this.DeviceType == EInputDevice.Unknown)
                    return "";
                string str = "";
                switch (this.DeviceType)
                {
                    case EInputDevice.KeyBoard:
                        str += "K";
                        break;
                    case EInputDevice.MIDIInput:
                        str += "M";
                        break;
                    case EInputDevice.Joypad:
                        str += "J";
                        break;
                    case EInputDevice.Mouse:
                        str += "N";
                        break;
                }
                // #24166 2011.1.15 yyagi: to support ID > 10, change 2nd character from Decimal to 36-numeral system. (e.g. J1023 -> JA23)
                str += "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(this.ID, 1);
                str += this.Code.ToString();
                return str;
            }
        }
    }
    #endregion

    // プロパティ

    public CKeyAssign KeyAssign;


    public bool bEnterがキー割り当てのどこにも使用されていない
    {
        get
        {
            for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
            {
                for (int k = 0; k < 0x10; k++)
                {
                    if ((this.KeyAssign[j][k].DeviceType == EInputDevice.KeyBoard) && (this.KeyAssign[j][k].Code == (int)SlimDXKeys.Key.Return))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }


    // コンストラクタ

    public CConfigIni()
    {
        this.tデフォルトのキーアサインに設定する();
    }
    public CConfigIni(string iniファイル名)
        : this()
    {
        this.tファイルから読み込み(iniファイル名);
    }


    // メソッド

    public void t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice DeviceType, int nID, int nCode)
    {
        for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
        {
            for (int k = 0; k < 0x10; k++)
            {
                if (((this.KeyAssign[j][k].DeviceType == DeviceType) && (this.KeyAssign[j][k].ID == nID)) && (this.KeyAssign[j][k].Code == nCode))
                {
                    for (int m = k; m < 15; m++)
                    {
                        this.KeyAssign[j][m] = this.KeyAssign[j][m + 1];
                    }
                    this.KeyAssign[j][15].DeviceType = EInputDevice.Unknown;
                    this.KeyAssign[j][15].ID = 0;
                    this.KeyAssign[j][15].Code = 0;
                    k--;
                }
            }
        }
    }
    public void t書き出し(string iniファイル名)
    {
        StreamWriter sw = new StreamWriter(iniファイル名, false, new UTF8Encoding(false));

        #region [ DrumsKeyAssign ]
        sw.WriteLine();
        sw.WriteLine(";-------------------");
        sw.WriteLine("; キーアサイン");
        sw.WriteLine(";   項　目：Keyboard → 'K'＋'0'＋KeyCode(10進数)");
        sw.WriteLine(";           Mouse    → 'N'＋'0'＋ボタン番号(0～13)");
        sw.WriteLine(";           MIDI In  → 'M'＋デバイス番号1桁(0～9,A～Z)＋ノート番号(10進数)");
        sw.WriteLine(";           Joystick → 'J'＋デバイス番号1桁(0～9,A～Z)＋ 0 ...... Ｘ減少(左)ボタン");
        sw.WriteLine(";                                                         1 ...... Ｘ増加(右)ボタン");
        sw.WriteLine(";                                                         2 ...... Ｙ減少(上)ボタン");
        sw.WriteLine(";                                                         3 ...... Ｙ増加(下)ボタン");
        sw.WriteLine(";                                                         4 ...... Ｚ減少(前)ボタン");
        sw.WriteLine(";                                                         5 ...... Ｚ増加(後)ボタン");
        sw.WriteLine(";                                                         6 ...... Ｚ回転(ＣＣＷ)");
        sw.WriteLine(";                                                         7 ...... Ｚ回転(ＣＷ)");
        sw.WriteLine(";                                                         8～135.. ボタン1～128");
        sw.WriteLine(";           これらの項目を 16 個まで指定可能(',' で区切って記述）。");
        sw.WriteLine(";");
        sw.WriteLine(";   表記例：LeftRed=K044,M042,J18");
        sw.WriteLine(";           → LeftRed を Keyboard の 44 ('Z'), MidiIn#0 の 42, JoyPad#1 の 8(ボタン1) に割当て");
        sw.WriteLine(";");
        sw.WriteLine(";   ※Joystick のデバイス番号とデバイスとの関係は [GUID] セクションに記してあるものが有効。");
        sw.WriteLine(";");
        sw.WriteLine(";   ※改造者はJoystickと呼べるようなものを所持していないため、Ｚ回転のＣＷ、ＣＣＷは逆である可能性があります。");
        sw.WriteLine();
        sw.WriteLine("[DrumsKeyAssign]");
        sw.WriteLine();
        sw.Write("LeftRed=");
        this.tキーの書き出し(sw, this.KeyAssign.LeftRed);
        sw.WriteLine();
        sw.Write("RightRed=");
        this.tキーの書き出し(sw, this.KeyAssign.RightRed);
        sw.WriteLine();
        sw.Write("LeftBlue=");										// #27029 2012.1.4 from
        this.tキーの書き出し(sw, this.KeyAssign.LeftBlue);	//
        sw.WriteLine();											//
        sw.Write("RightBlue=");										// #27029 2012.1.4 from
        this.tキーの書き出し(sw, this.KeyAssign.RightBlue);	//
        sw.WriteLine();
        sw.Write("LeftRed2P=");
        this.tキーの書き出し(sw, this.KeyAssign.LeftRed2P);
        sw.WriteLine();
        sw.Write("RightRed2P=");
        this.tキーの書き出し(sw, this.KeyAssign.RightRed2P);
        sw.WriteLine();
        sw.Write("LeftBlue2P=");										// #27029 2012.1.4 from
        this.tキーの書き出し(sw, this.KeyAssign.LeftBlue2P);	//
        sw.WriteLine();											        //
        sw.Write("RightBlue2P=");										// #27029 2012.1.4 from
        this.tキーの書き出し(sw, this.KeyAssign.RightBlue2P);	//
        sw.WriteLine();
        sw.WriteLine();
        #endregion
        #region [ SystemkeyAssign ]
        sw.WriteLine("[SystemKeyAssign]");
        sw.WriteLine();
        sw.Write("Capture=");
        this.tキーの書き出し(sw, this.KeyAssign.Capture);
        sw.WriteLine();
        sw.Write("FullScreen=");
        this.tキーの書き出し(sw, this.KeyAssign.FullScreen);
        sw.WriteLine();
        sw.WriteLine();
        #endregion

        sw.Close();
    }

    public void tファイルから読み込み(string iniファイル名)
    {
        if (File.Exists(iniファイル名))
        {
            string str = CJudgeTextEncoding.ReadTextFile(iniファイル名);
            this.tキーアサインを全部クリアする();
            t文字列から読み込み(str);
        }
    }

    private void t文字列から読み込み(string strAllSettings)	// 2011.4.13 yyagi; refactored to make initial KeyConfig easier.
    {
        Eセクション種別 unknown = Eセクション種別.Unknown;
        string[] delimiter = { "\n" };
        string[] strSingleLine = strAllSettings.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        foreach (string s in strSingleLine)
        {
            string str = s.Replace('\t', ' ').TrimStart(new char[] { '\t', ' ' });
            if (str.Length == 0 || str[0] == ';')
                continue;

            try
            {
                string str3;
                string str4;
                if (str[0] == '[')
                {
                    #region [ セクションの変更 ]
                    //-----------------------------
                    int index = str.IndexOf(']');
                    string str2;
                    if (index >= 0)
                        str2 = str.Substring(1, index - 1);
                    else
                        str2 = str.Substring(1);

                    if (Enum.TryParse(typeof(Eセクション種別), str2, out var eType))
                        unknown = (Eセクション種別)eType;
                    else
                        unknown = Eセクション種別.Unknown;
                    //-----------------------------
                    #endregion
                }
                else
                {
                    string[] strArray = str.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        str3 = strArray[0].Trim();
                        str4 = strArray[1].Trim();
                        switch (unknown)
                        {
                            #region [ [DrumsKeyAssign] ]
                            //-----------------------------
                            case Eセクション種別.DrumsKeyAssign:
                                {
                                    if (str3.Equals("LeftRed"))
                                    {
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.LeftRed);
                                    }
                                    else if (str3.Equals("RightRed"))
                                    {
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.RightRed);
                                    }
                                    else if (str3.Equals("LeftBlue"))										// #27029 2012.1.4 from
                                    {																	//
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.LeftBlue);	//
                                    }																	//
                                    else if (str3.Equals("RightBlue"))										// #27029 2012.1.4 from
                                    {																	//
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.RightBlue);	//
                                    }

                                    else if (str3.Equals("LeftRed2P"))
                                    {
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.LeftRed2P);
                                    }
                                    else if (str3.Equals("RightRed2P"))
                                    {
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.RightRed2P);
                                    }
                                    else if (str3.Equals("LeftBlue2P"))										// #27029 2012.1.4 from
                                    {																	//
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.LeftBlue2P);	//
                                    }																	//
                                    else if (str3.Equals("RightBlue2P"))										// #27029 2012.1.4 from
                                    {																	//
                                        this.tキーの読み出しと設定(str4, this.KeyAssign.RightBlue2P);	//
                                    }

                                    continue;
                                }
                            //-----------------------------
                            #endregion

                            #region [ [SystemKeyAssign] ]
                            //-----------------------------
                            case Eセクション種別.SystemKeyAssign:
                                if (str3.Equals("Capture"))
                                {
                                    this.tキーの読み出しと設定(str4, this.KeyAssign.Capture);
                                }
                                else if (str3.Equals("FullScreen"))
                                {
                                    this.tキーの読み出しと設定(str4, this.KeyAssign.FullScreen);
                                }
                                continue;
                                //-----------------------------
                                #endregion
                        }
                    }
                }
                continue;
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
                Trace.TraceError("An exception has occurred, but processing continues.");
                continue;
            }
        }
    }

    // その他

    #region [ private ]
    //-----------------
    private enum Eセクション種別
    {
        Unknown,
        DrumsKeyAssign,
        SystemKeyAssign,
    }

    private void tキーアサインを全部クリアする()
    {
        this.KeyAssign = new CKeyAssign();
        for (int j = 0; j < (int)EKeyConfigPad.MAX; j++)
        {
            this.KeyAssign[j] = new CKeyAssign.STKEYASSIGN[16];
            for (int k = 0; k < 16; k++)
            {
                this.KeyAssign[j][k] = new CKeyAssign.STKEYASSIGN(EInputDevice.Unknown, 0, 0);
            }
        }
    }
    private void tキーの書き出し(StreamWriter sw, CKeyAssign.STKEYASSIGN[] assign)
    {
        var str = string.Join(",", assign
            .Select(x => x.ToString())
            .Where(x => !string.IsNullOrEmpty(x)));

        sw.Write(str);
    }
    private void tキーの読み出しと設定(string strキー記述, CKeyAssign.STKEYASSIGN[] assign)
    {
        string[] strArray = strキー記述.Split(new char[] { ',' });
        for (int i = 0; (i < strArray.Length) && (i < 0x10); i++)
        {
            CKeyAssign.STKEYASSIGN stAssign = new CKeyAssign.STKEYASSIGN(strArray[i]);
            if ((stAssign.DeviceType != EInputDevice.Unknown) && (stAssign.ID >= 0) && (stAssign.Code >= 0) && (stAssign.Code <= 0xff))
            {
                this.t指定した入力が既にアサイン済みである場合はそれを全削除する(stAssign.DeviceType, stAssign.ID, stAssign.Code);
                assign[i] = stAssign;
            }
        }
    }
    private void tデフォルトのキーアサインに設定する()
    {
        this.tキーアサインを全部クリアする();

        string strDefaultKeyAssign = @"
[DrumsKeyAssign]
LeftRed=K015
RightRed=K019
LeftBlue=K013
RightBlue=K020
LeftRed2P=K031
RightRed2P=K022
LeftBlue2P=K012
RightBlue2P=K047

[SystemKeyAssign]
Capture=K065
FullScreen=K064
";
        t文字列から読み込み(strDefaultKeyAssign);
    }
    //-----------------
    #endregion
}
