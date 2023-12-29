using Commons.Music.Midi;

namespace FDK;

public class CInputManager : IDisposable
{
    // プロパティ

    public List<IInputDevice> listInputDevices
    {
        get;
        private set;
    }
    public IInputDevice? Keyboard
    {
        get
        {
            if (this._Keyboard != null)
            {
                return this._Keyboard;
            }
            foreach (IInputDevice device in this.listInputDevices)
            {
                if (device.eInputDeviceType == EInputDeviceType.Keyboard)
                {
                    this._Keyboard = device;
                    return device;
                }
            }
            return null;
        }
    }
    public IInputDevice? Mouse
    {
        get
        {
            if (this._Mouse != null)
            {
                return this._Mouse;
            }
            foreach (IInputDevice device in this.listInputDevices)
            {
                if (device.eInputDeviceType == EInputDeviceType.Mouse)
                {
                    this._Mouse = device;
                    return device;
                }
            }
            return null;
        }
    }


    // コンストラクタ
    public CInputManager()
    {
        this.listInputDevices = new List<IInputDevice>(10);
        #region [ Enumerate keyboard/mouse: exception is masked if keyboard/mouse is not connected ]
        CInputKeyboard? cinputkeyboard = null;
        CInputMouse? cinputmouse = null;
        try
        {
            cinputkeyboard = new CInputKeyboard();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.ToString());
        }
        if (cinputkeyboard != null)
        {
            this.listInputDevices.Add(cinputkeyboard);
        }
        try
        {
            cinputmouse = new CInputMouse();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.ToString());
        }
        if (cinputmouse != null)
        {
            this.listInputDevices.Add(cinputmouse);
        }
        #endregion
        #region [ Enumerate joypad ]
        try
        {
            for (int joynum = 0; joynum < SDL2.SDL.SDL_NumJoysticks(); joynum++)
            {
                this.listInputDevices.Add(new CInputJoystick(joynum));
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.ToString());
        }
        #endregion

        try
        {
            var midiinlisttmp = MidiAccessManager.Default.Inputs.ToArray();

            for (int i = 0; i < midiinlisttmp.Length; i++)
            {
                var midiintmp = MidiAccessManager.Default.OpenInputAsync(midiinlisttmp[i].Id).Result;
                midiintmp.MessageReceived += onMessageRecevied;
                this.midiInputs.Add(midiintmp);
                CInputMIDI item = new CInputMIDI(uint.Parse(midiinlisttmp[i].Id));
                this.listInputDevices.Add(item);
            }
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
        }
    }


    // メソッド

    public IInputDevice? Joystick(int ID)
    {
        foreach (IInputDevice device in this.listInputDevices)
        {
            if ((device.eInputDeviceType == EInputDeviceType.Joystick) && (device.ID == ID))
            {
                return device;
            }
        }
        return null;
    }
    public IInputDevice? Joystick(string GUID)
    {
        foreach (IInputDevice device in this.listInputDevices)
        {
            if ((device.eInputDeviceType == EInputDeviceType.Joystick) && device.GUID.Equals(GUID))
            {
                return device;
            }
        }
        return null;
    }
    public IInputDevice? MidiIn(int ID)
    {
        foreach (IInputDevice device in this.listInputDevices)
        {
            if ((device.eInputDeviceType == EInputDeviceType.MidiIn) && (device.ID == ID))
            {
                return device;
            }
        }
        return null;
    }
    public void tPolling(bool bIsWindowActive)
    {
        if (CSoundManager.rc演奏用タイマ != null)
            lock (this.objMidiIn排他用)
            {
                //				foreach( IInputDevice device in this.listInputDevices )
                for (int i = this.listInputDevices.Count - 1; i >= 0; i--)    // #24016 2011.1.6 yyagi: change not to use "foreach" to avoid InvalidOperation exception by Remove().
                {
                    IInputDevice device = this.listInputDevices[i];
                    try
                    {
                        device.tPolling(bIsWindowActive);
                    }
                    catch (Exception e)                                      // #24016 2011.1.6 yyagi: catch exception for unplugging USB joystick, and remove the device object from the polling items.
                    {
                        this.listInputDevices.Remove(device);
                        device.Dispose();
                        Trace.TraceError("tPolling時に例外発生。該当deviceをポーリング対象からRemoveしました。");
                        Trace.TraceError(e.ToString());
                    }
                }
            }
    }

    public void tSwapEventList()
    {
        lock (this.objMidiIn排他用)
        {
            //				foreach( IInputDevice device in this.listInputDevices )
            for (int i = this.listInputDevices.Count - 1; i >= 0; i--)    // #24016 2011.1.6 yyagi: change not to use "foreach" to avoid InvalidOperation exception by Remove().
            {
                IInputDevice device = this.listInputDevices[i];
                try
                {
                    device.tSwapEventList();
                }
                catch (Exception e)                                      // #24016 2011.1.6 yyagi: catch exception for unplugging USB joystick, and remove the device object from the polling items.
                {
                    this.listInputDevices.Remove(device);
                    device.Dispose();
                    Trace.TraceError("tClearEventList時に例外発生。該当deviceをポーリング対象からRemoveしました。");
                    Trace.TraceError(e.ToString());
                }
            }
        }
    }

    #region [ IDisposable＋α ]
    //-----------------
    public void Dispose()
    {
        this.Dispose(true);
    }
    public void Dispose(bool disposeManagedObjects)
    {
        if (!this.bDisposed)
        {
            if (disposeManagedObjects)
            {
                midiInputs.Clear();
                foreach (IInputDevice device2 in this.listInputDevices)
                {
                    device2.Dispose();
                }
                lock (this.objMidiIn排他用)
                {
                    this.listInputDevices.Clear();
                }
            }
            this.bDisposed = true;
        }
    }
    ~CInputManager()
    {
        this.Dispose(false);
        GC.KeepAlive(this);
    }
    //-----------------
    #endregion


    // その他

    #region [ private ]
    //-----------------
    private IInputDevice? _Keyboard = null;
    private IInputDevice? _Mouse = null;
    private bool bDisposed;
    private object objMidiIn排他用 = new object();
    private List<IMidiInput> midiInputs = new List<IMidiInput>();

    private void onMessageRecevied(object? sender, MidiReceivedEventArgs? e)
    {
        if (sender is null || e is null)
            return;

        long time = 0;
        if (CSoundManager.rc演奏用タイマ is not null)
            time = CSoundManager.rc演奏用タイマ.nシステム時刻ms; // lock前に取得。演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。

        int dev = int.Parse(((IMidiInput)sender).Details.Id);

        lock (this.objMidiIn排他用)
        {
            if ((this.listInputDevices != null) && (this.listInputDevices.Count != 0))
            {
                foreach (IInputDevice device in this.listInputDevices)
                {
                    CInputMIDI tmidi = (CInputMIDI)device;
                    if ((tmidi != null) && (tmidi.ID == dev))
                    {
                        for (int i = 0; i < e.Length / 3; i++)
                            tmidi.tメッセージからMIDI信号のみ受信(dev, time, e.Data, i);
                        break;
                    }
                }
            }
        }
    }
    //-----------------
    #endregion
}
