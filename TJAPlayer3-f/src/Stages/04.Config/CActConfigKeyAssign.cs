using FDK;

namespace TJAPlayer3;

internal class CActConfigKeyAssign : CActivity
{
    // プロパティ

    public bool bキー入力待ちの最中である
    {
        get
        {
            return this.bキー入力待ち;
        }
    }


    // メソッド

    public void t開始(EKeyConfigPad pad, string strパッド名)
    {
        this.pad = pad;
        this.strパッド名 = strパッド名;
        for (int i = 0; i < 0x10; i++)
        {
            this.structReset用KeyAssign[i].DeviceType = TJAPlayer3.app.ConfigIni.KeyAssign[(int)pad][i].DeviceType;
            this.structReset用KeyAssign[i].ID = TJAPlayer3.app.ConfigIni.KeyAssign[(int)pad][i].ID;
            this.structReset用KeyAssign[i].Code = TJAPlayer3.app.ConfigIni.KeyAssign[(int)pad][i].Code;
        }
    }

    public void tPushedEnter()
    {
        if (!this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
            switch (this.n現在の選択行)
            {
                case 0x10:
                    for (int i = 0; i < 0x10; i++)
                    {
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][i].DeviceType = this.structReset用KeyAssign[i].DeviceType;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][i].ID = this.structReset用KeyAssign[i].ID;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][i].Code = this.structReset用KeyAssign[i].Code;
                    }
                    return;

                case 0x11:
                    TJAPlayer3.stageConfig.tアサイン完了通知();
                    return;
            }
            this.bキー入力待ち = true;
        }
    }
    public void t次に移動()
    {
        if (!this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
            this.n現在の選択行 = (this.n現在の選択行 + 1) % 0x12;
        }
    }
    public void t前に移動()
    {
        if (!this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
            this.n現在の選択行 = ((this.n現在の選択行 - 1) + 0x12) % 0x12;
        }
    }


    // CActivity 実装

    public override void On活性化()
    {
        this.pad = EKeyConfigPad.UNKNOWN;
        this.strパッド名 = "";
        this.n現在の選択行 = 0;
        this.bキー入力待ち = false;
        this.structReset用KeyAssign = new CConfigIni.CKeyAssign.STKEYASSIGN[0x10];
        this.fontRenderer = new CCachedFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 16, CFontRenderer.FontStyle.Italic);
        base.On活性化();
    }
    public override void On非活性化()
    {
        if (!base.b活性化してない)
        {
            this.fontRenderer?.Dispose();
            this.fontRenderer = null;
            base.On非活性化();
        }
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if (this.bキー入力待ち)
            {
                if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
                {
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                    this.bキー入力待ち = false;
                    TJAPlayer3.app.InputManager.tSwapEventList();
                }
                else if ((this.tキーチェックとアサイン_Keyboard() || this.tキーチェックとアサイン_MidiIn()) || (this.tキーチェックとアサイン_Joypad() || this.tキーチェックとアサイン_Mouse()))
                {
                    this.bキー入力待ち = false;
                    TJAPlayer3.app.InputManager.tSwapEventList();
                }
            }
            else if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Delete) && (this.n現在の選択行 >= 0)) && (this.n現在の選択行 <= 15))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].DeviceType = EInputDevice.Unknown;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].ID = 0;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].Code = 0;
            }
            if (TJAPlayer3.app.Tx.Menu_Highlight is not null)
            {
                int num = 20;
                int num2 = 0x144;
                int num3 = 0x3e + (num * (this.n現在の選択行 + 1));
                TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, num2, num3, new Rectangle(0, 0, 0x10, 0x20));
                num2 += 0x10;
                Rectangle rectangle = new Rectangle(8, 0, 0x10, 0x20);
                for (int j = 0; j < 14; j++)
                {
                    TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, num2, num3, rectangle);
                    num2 += 0x10;
                }
                TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, num2, num3, new Rectangle(0x10, 0, 0x10, 0x20));
            }
            int num5 = 20;
            int x = 0x134;
            int y = 0x40;
            tDrawText(x, y, this.strパッド名, false, 0.75f);
            y += num5;
            CConfigIni.CKeyAssign.STKEYASSIGN[] stkeyassignArray = TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad];
            for (int i = 0; i < 0x10; i++)
            {
                switch (stkeyassignArray[i].DeviceType)
                {
                    case EInputDevice.KeyBoard:
                        this.tアサインコードの描画_Keyboard(i + 1, x + 20, y, stkeyassignArray[i].ID, stkeyassignArray[i].Code, this.n現在の選択行 == i);
                        break;

                    case EInputDevice.MIDIInput:
                        this.tアサインコードの描画_MidiIn(i + 1, x + 20, y, stkeyassignArray[i].ID, stkeyassignArray[i].Code, this.n現在の選択行 == i);
                        break;

                    case EInputDevice.Joypad:
                        this.tアサインコードの描画_Joypad(i + 1, x + 20, y, stkeyassignArray[i].ID, stkeyassignArray[i].Code, this.n現在の選択行 == i);
                        break;

                    case EInputDevice.Mouse:
                        this.tアサインコードの描画_Mouse(i + 1, x + 20, y, stkeyassignArray[i].ID, stkeyassignArray[i].Code, this.n現在の選択行 == i);
                        break;

                    default:
                        tDrawText(x + 20, y, string.Format("{0,2}.", i + 1), this.n現在の選択行 == i, 0.75f);
                        break;
                }
                y += num5;
            }
            tDrawText(x + 20, y, "Reset", this.n現在の選択行 == 0x10, 0.75f);
            y += num5;
            tDrawText(x + 20, y, "<< Return to List", this.n現在の選択行 == 0x11, 0.75f);
            y += num5;
            if (this.bキー入力待ち && (TJAPlayer3.app.Tx.Config_KeyAssign is not null))
            {
                TJAPlayer3.app.Tx.Config_KeyAssign.t2D描画(TJAPlayer3.app.Device, 0x185, 0xd7);
            }
        }
        return 0;
    }


    // その他

    #region [ private ]
    private bool bキー入力待ち;
    private FrozenDictionary<int, string> KeyLabel = new Dictionary<int, string>() {
        { 0x00, "[ 0 ]" }, { 0x01, "[ 1 ]" }, { 0x02, "[ 2 ]" }, { 0x03, "[ 3 ]" }, { 0x04, "[ 4 ]" }, { 0x05, "[ 5 ]" }, { 0x06, "[ 6 ]" }, { 0x07, "[ 7 ]" }, { 0x08, "[ 8 ]" }, { 0x09, "[ 9 ]" },
        { 0x0a, "[ A ]" }, { 0x0b, "[ B ]" }, { 0x0c, "[ C ]" }, { 0x0d, "[ D ]" }, { 0x0e, "[ E ]" }, { 0x0f, "[ F ]" }, { 0x10, "[ G ]" }, { 0x11, "[ H ]" }, { 0x12, "[ R ]" }, { 0x13, "[ J ]" },
        { 0x14, "[ K ]" }, { 0x15, "[ L ]" }, { 0x16, "[ M ]" }, { 0x17, "[ N ]" }, { 0x18, "[ O ]" }, { 0x19, "[ P ]" }, { 0x1a, "[ Q ]" }, { 0x1b, "[ R ]" }, { 0x1c, "[ S ]" }, { 0x1d, "[ T ]" },
        { 0x1e, "[ U ]" }, { 0x1f, "[ V ]" }, { 0x20, "[ W ]" }, { 0x21, "[ X ]" }, { 0x22, "[ Y ]" }, { 0x23, "[ Z ]" },
        { 0x24, "[ ? ]" }, { 0x25, "[NPad.]" }, { 0x26, "[ ' ]" }, { 0x27, "[ APP ]" }, { 0x28, "[ @ ]" }, { 0x29, "[AX]" }, { 0x2a, "[BSC]" }, { 0x2b, @"[ \ ]" }, { 0x2c, "[Calc]" }, { 0x2d, "[CAPS]" },
        { 0x2e, "[ : ]" }, { 0x2f, "[ , ]" }, { 0x30, "[Henkan]" }, { 0x31, "[Delete]" }, { 0x32, "[Down]" }, { 0x33, "[End]" }, { 0x34, "[ = ]" }, { 0x35, "[ESC]" },
        { 0x36, "[F1]" }, { 0x37, "[F2]" }, { 0x38, "[F3]" }, { 0x39, "[F4]" }, { 0x3a, "[F5]" }, { 0x3b, "[F6]" }, { 0x3c, "[F7]" }, { 0x3d, "[F8]" },
        { 0x3e, "[F9]" }, { 0x3f, "[F10]" }, { 0x40, "[F11]" }, { 0x41, "[F12]" }, { 0x42, "[F13]" }, { 0x43, "[F14]" }, { 0x44, "[F15]" },
        { 0x45, "[ ` ]" }, { 0x46, "[Home]" }, { 0x47, "[Insert]" }, { 0x48, "[Kana]" }, { 0x49, "[Kanji]" }, { 0x4a, "[ [ ]" },
        { 0x4b, "[L-Ctrl]" }, { 0x4c, "[Left]" }, { 0x4d, "[L-Alt]" }, { 0x4e, "[L-Shift]" }, { 0x4f, "[L-Win]" },
        { 0x52, "[MediaStop]" }, { 0x53, "[ - ]" }, { 0x54, "[Mute]" }, { 0x57, "[MuHenkan]" }, { 0x58, "[NumLock]" },
        { 0x59, "[NPad0]" }, { 0x5a, "[NPad1]" }, { 0x5b, "[NPad2]" }, { 0x5c, "[NPad3]" }, { 0x5d, "[NPad4]" }, { 0x5e, "[NPad5]" }, { 0x5f, "[NPad6]" }, { 0x60, "[NPad7]" }, { 0x61, "[NPad8]" }, { 0x62, "[NPad9]" },
        { 0x63, "[NPad,]" }, { 0x64, "[NPEnter]" }, { 0x65, "[NPad=]" }, { 0x66, "[NPad-]" }, { 0x67, "[NPad.]" }, { 0x68, "[NPad+]" },
        { 0x69, "[ / ]" }, { 0x6a, "[ * ]" }, { 0x6c, "[PgDn]" }, { 0x6d, "[PgUp]" }, { 0x6e, "[Pause]" }, { 0x6f, "[ . ]" },
        { 0x70, "[PlayPause]" }, { 0x71, "[Power]" }, { 0x72, "[ ^ ]" }, { 0x73, "[ ] ]" },
        { 0x74, "[R-Ctrl]" }, { 0x75, "[Enter]" }, { 0x76, "[Right]" }, { 0x77, "[R-Alt]" }, { 0x78, "[R-Shift]" }, { 0x79, "[R-Win]" },
        { 0x7a, "[Scroll]" }, { 0x7b, "[ ; ]" }, { 0x7c, "[ / ]" }, { 0x7d, "[Sleep]" }, { 0x7e, "[Space]" }, { 0x7f, "[Stop]" }, { 0x80, "[PrtScn]" },
        { 0x81, "[TAB]" }, { 0x82, "[ _ ]" }, { 0x84, "[Up]" }, { 0x85, "[Volume-]" }, { 0x86, "[Volume+]" }, { 0x87, "[Wake]" }, { 0x8b, "[WebHome]" }, { 0x8f, "[ / ]" },
    }.ToFrozenDictionary();
    private int n現在の選択行;
    private EKeyConfigPad pad;
    private CConfigIni.CKeyAssign.STKEYASSIGN[] structReset用KeyAssign;
    private string? strパッド名;
    private CCachedFontRenderer? fontRenderer;

    private void tアサインコードの描画_Joypad(int line, int x, int y, int nID, int nCode, bool b強調)
    {
        string str = "";
        switch (nCode)
        {
            case 0:
                str = "Left";
                break;

            case 1:
                str = "Right";
                break;

            case 2:
                str = "Up";
                break;

            case 3:
                str = "Down";
                break;

            case 4:
                str = "Forward";
                break;

            case 5:
                str = "Back";
                break;

            case 6:
                str = "CCW";
                break;

            case 7:
                str = "CW";
                break;

            default:
                if ((8 <= nCode) && (nCode < 8 + 128))				// other buttons (128 types)
                {
                    str = string.Format("Button{0}", nCode - 7);
                }
                else if ((8 + 128 <= nCode) && (nCode < 8 + 128 + 8))		// POV HAT ( 8 types; 45 degrees per HATs)
                {
                    str = string.Format("POV {0}", (nCode - 8 - 128) * 45);
                }
                else
                {
                    str = string.Format("Code{0}", nCode);
                }
                break;
        }
        tDrawText(x, y, string.Format("{0,2}. Joypad #{1} ", line, nID) + str, b強調, 0.75f);
    }
    private void tアサインコードの描画_Keyboard(int line, int x, int y, int nID, int nCode, bool b強調)
    {
        string? str = null;
        if (this.KeyLabel.TryGetValue(nCode, out var strLabel))
        {
            str = string.Format("{0,2}. Key {1}", line, strLabel);
        }
        else
        {
            str = string.Format("{0,2}. Key 0x{1:X2}", line, nCode);
        }
        tDrawText(x, y, str, b強調, 0.75f);
    }
    private void tアサインコードの描画_MidiIn(int line, int x, int y, int nID, int nCode, bool b強調)
    {
        tDrawText(x, y, string.Format("{0,2}. MidiIn #{1} code.{2}", line, nID, nCode), b強調, 0.75f);
    }
    private void tアサインコードの描画_Mouse(int line, int x, int y, int nID, int nCode, bool b強調)
    {
        tDrawText(x, y, string.Format("{0,2}. Mouse Button{1}", line, nCode), b強調, 0.75f);
    }
    private bool tキーチェックとアサイン_Joypad()
    {
        foreach (IInputDevice device in TJAPlayer3.app.InputManager.listInputDevices)
        {
            if (device.eInputDeviceType == EInputDeviceType.Joystick)
            {
                for (int i = 0; i < 8 + 0x80 + 8; i++)		// +8 for Axis, +8 for HAT
                {
                    if (device.bIsKeyPressed(i))
                    {
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                        TJAPlayer3.app.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.Joypad, device.ID, i);
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].DeviceType = EInputDevice.Joypad;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].ID = device.ID;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].Code = i;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool tキーチェックとアサイン_Keyboard()
    {
        for (int i = 0; i < 0x100; i++)
        {
            if (i != (int)SlimDXKeys.Key.Escape &&
                i != (int)SlimDXKeys.Key.Return &&
                i != (int)SlimDXKeys.Key.UpArrow &&
                i != (int)SlimDXKeys.Key.DownArrow &&
                i != (int)SlimDXKeys.Key.LeftArrow &&
                i != (int)SlimDXKeys.Key.RightArrow &&
                i != (int)SlimDXKeys.Key.Delete &&
                    TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed(i))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                TJAPlayer3.app.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.KeyBoard, 0, i);
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].DeviceType = EInputDevice.KeyBoard;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].ID = 0;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].Code = i;
                return true;
            }
        }
        return false;
    }
    private bool tキーチェックとアサイン_MidiIn()
    {
        foreach (IInputDevice device in TJAPlayer3.app.InputManager.listInputDevices)
        {
            if (device.eInputDeviceType == EInputDeviceType.MidiIn)
            {
                for (int i = 0; i < 0x100; i++)
                {
                    if (device.bIsKeyPressed(i))
                    {
                        TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                        TJAPlayer3.app.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.MIDIInput, device.ID, i);
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].DeviceType = EInputDevice.MIDIInput;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].ID = device.ID;
                        TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].Code = i;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool tキーチェックとアサイン_Mouse()
    {
        for (int i = 0; i < 8; i++)
        {
            if (TJAPlayer3.app.InputManager.Mouse.bIsKeyPressed(i))
            {
                TJAPlayer3.app.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.Mouse, 0, i);
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].DeviceType = EInputDevice.Mouse;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].ID = 0;
                TJAPlayer3.app.ConfigIni.KeyAssign[(int)this.pad][this.n現在の選択行].Code = i;
                return true;
            }
        }
        return false;
    }

    private void tDrawText(int x, int y, string? str, bool b強調, float fScale)
    {
        if (this.fontRenderer is null || str is null)
            return;

        Color fontcol = b強調 ? Color.Cyan : Color.White;
        using (CTexture? fonttex = TJAPlayer3.app.tCreateTexture(this.fontRenderer.DrawText(str, fontcol, Color.DarkCyan, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio)))
        {
            if (fonttex is not null)
            {
                fonttex.vcScaling.X = fScale;
                fonttex.vcScaling.Y = fScale;
                fonttex.t2D描画(TJAPlayer3.app.Device, x, y);
            }
        }
    }
    //-----------------
    #endregion
}
