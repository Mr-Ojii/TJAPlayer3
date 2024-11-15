using FDK;

namespace TJAPlayer3;

public class CPad
{
    // プロパティ

    internal STHIT stDetectedDevices;
    [StructLayout(LayoutKind.Sequential)]
    internal struct STHIT
    {
        public bool Keyboard;
        public bool MIDIIN;
        public bool Joypad;
        public bool Mouse;
        public void Clear()
        {
            this.Keyboard = false;
            this.MIDIIN = false;
            this.Joypad = false;
            this.Mouse = false;
        }
    }


    // コンストラクタ

    internal CPad(CConfigIni configIni, CConfigToml configToml, CInputManager mgrInput)
    {
        this.rConfigIni = configIni;
        this.rConfigToml = configToml;
        this.rInputManager = mgrInput;
        this.stDetectedDevices.Clear();
    }


    // メソッド

    public List<STInputEvent> GetEvents(EPad pad)
    {
        CConfigIni.CKeyAssign.STKEYASSIGN[] stkeyassignArray = this.rConfigIni.KeyAssign[(int)pad];
        List<STInputEvent> list = new List<STInputEvent>();

        // すべての入力デバイスについて…
        foreach (IInputDevice device in this.rInputManager.listInputDevices)
        {
            if ((device.listInputEvents is null) || (device.listInputEvents.Count == 0))
                continue;

            foreach (STInputEvent event2 in device.listInputEvents)
            {
                for (int i = 0; i < stkeyassignArray.Length; i++)
                {
                    switch (stkeyassignArray[i].DeviceType)
                    {
                        case EInputDevice.KeyBoard:
                            if ((device.eInputDeviceType == EInputDeviceType.Keyboard) && (event2.nKey == stkeyassignArray[i].Code))
                            {
                                list.Add(event2);
                                this.stDetectedDevices.Keyboard = true;
                            }
                            break;

                        case EInputDevice.MIDIInput:
                            if (((device.eInputDeviceType == EInputDeviceType.MidiIn) && (device.ID == stkeyassignArray[i].ID)) && (event2.nKey == stkeyassignArray[i].Code))
                            {
                                list.Add(event2);
                                this.stDetectedDevices.MIDIIN = true;
                            }
                            break;

                        case EInputDevice.Joypad:
                            if (((device.eInputDeviceType == EInputDeviceType.Joystick) && (device.ID == stkeyassignArray[i].ID)) && (event2.nKey == stkeyassignArray[i].Code))
                            {
                                list.Add(event2);
                                this.stDetectedDevices.Joypad = true;
                            }
                            break;

                        case EInputDevice.Mouse:
                            if ((device.eInputDeviceType == EInputDeviceType.Mouse) && (event2.nKey == stkeyassignArray[i].Code))
                            {
                                list.Add(event2);
                                this.stDetectedDevices.Mouse = true;
                            }
                            break;
                    }
                }
            }
        }
        return list;
    }
    public bool bPressed(EPad pad)
    {
        CConfigIni.CKeyAssign.STKEYASSIGN[] stkeyassignArray = this.rConfigIni.KeyAssign[(int)pad];
        for (int i = 0; i < stkeyassignArray.Length; i++)
        {
            switch (stkeyassignArray[i].DeviceType)
            {
                case EInputDevice.KeyBoard:
                    if (!this.rInputManager.Keyboard.bIsKeyPressed(stkeyassignArray[i].Code))
                        break;

                    this.stDetectedDevices.Keyboard = true;
                    return true;

                case EInputDevice.MIDIInput:
                    {
                        IInputDevice? device2 = this.rInputManager.MidiIn(stkeyassignArray[i].ID);
                        if ((device2 is null) || !device2.bIsKeyPressed(stkeyassignArray[i].Code))
                            break;

                        this.stDetectedDevices.MIDIIN = true;
                        return true;
                    }
                case EInputDevice.Joypad:
                    {
                        if (!this.rConfigToml.JoystickGUID.ContainsKey(stkeyassignArray[i].ID))
                            break;

                        IInputDevice? device = this.rInputManager.Joystick(stkeyassignArray[i].ID);
                        if ((device is null) || !device.bIsKeyPressed(stkeyassignArray[i].Code))
                            break;

                        this.stDetectedDevices.Joypad = true;
                        return true;
                    }
                case EInputDevice.Mouse:
                    if (!this.rInputManager.Mouse.bIsKeyPressed(stkeyassignArray[i].Code))
                        break;

                    this.stDetectedDevices.Mouse = true;
                    return true;
            }
        }

        return false;
    }
    public bool bDown(EPad pad)
    {
        CConfigIni.CKeyAssign.STKEYASSIGN[] stkeyassignArray = this.rConfigIni.KeyAssign[(int)pad];
        for (int i = 0; i < stkeyassignArray.Length; i++)
        {
            switch (stkeyassignArray[i].DeviceType)
            {
                case EInputDevice.KeyBoard:
                    if (!this.rInputManager.Keyboard.bIsKeyDown(stkeyassignArray[i].Code))
                        break;

                    this.stDetectedDevices.Keyboard = true;
                    return true;

                case EInputDevice.Joypad:
                    {
                        if (!this.rConfigToml.JoystickGUID.ContainsKey(stkeyassignArray[i].ID))
                            break;

                        IInputDevice? device = this.rInputManager.Joystick(stkeyassignArray[i].ID);
                        if ((device is null) || !device.bIsKeyDown(stkeyassignArray[i].Code))
                            break;

                        this.stDetectedDevices.Joypad = true;
                        return true;
                    }
                case EInputDevice.Mouse:
                    if (!this.rInputManager.Mouse.bIsKeyDown(stkeyassignArray[i].Code))
                        break;

                    this.stDetectedDevices.Mouse = true;
                    return true;
            }
        }
        return false;
    }


    // その他

    #region [ private ]
    //-----------------
    private CConfigIni rConfigIni;
    private CConfigToml rConfigToml;
    private CInputManager rInputManager;
    //-----------------
    #endregion
}
