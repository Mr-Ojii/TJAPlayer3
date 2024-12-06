namespace FDK;

public class CInputMIDI : IInputDevice, IDisposable
{
    // プロパティ

    public IntPtr hMidiIn;
    public ConcurrentQueue<STInputEvent> listEventBuffer;

    // コンストラクタ

    public CInputMIDI(int nID, string GUID)
    {
        this.hMidiIn = IntPtr.Zero;
        this.listEventBuffer = new ConcurrentQueue<STInputEvent>();
        this.listInputEvents = new List<STInputEvent>();
        this.eInputDeviceType = EInputDeviceType.MidiIn;
        this.GUID = GUID;
        this.ID = nID;
        this.strDeviceName = "";    // CInputManagerで初期化する
    }

    // メソッド

    public unsafe void tメッセージからMIDI信号のみ受信(string dev, long time, byte[] buf, int count)
    {
        if (this.GUID == dev)
        {
            int nMIDIevent = buf[count * 3];
            int nPara1 = buf[count * 3 + 1];
            int nPara2 = buf[count * 3 + 2];

            if ((nMIDIevent >= 0x90) && (nMIDIevent <= 0x9f) && (nPara2 != 0))      // Note ON
            {
                STInputEvent item = new STInputEvent()
                {
                    nKey = nPara1,
                    eType = EInputEventType.Pressed,
                    nTimeStamp = time,
                };
                this.listEventBuffer.Enqueue(item);
            }
        }
    }


    #region [ IInputDevice 実装 ]
    //-----------------
    public EInputDeviceType eInputDeviceType { get; private set; }
    public string GUID { get; private set; }
    public int ID { get; private set; }
    public List<STInputEvent> listInputEvents { get; private set; }
    public string strDeviceName { get; set; }

    public void tPolling(bool bIsWindowActive)
    {
    }

    public void tSwapEventList()
    {
        this.listInputEvents.Clear();            // #xxxxx 2012.6.11 yyagi; To optimize, I removed new();

        while (this.listEventBuffer.TryDequeue(out var InputEvent))
            this.listInputEvents.Add(InputEvent);
    }

    public bool bIsKeyPressed(int nKey)
    {
        foreach (STInputEvent event2 in this.listInputEvents)
        {
            if ((event2.nKey == nKey) && (event2.eType == EInputEventType.Pressed))
            {
                return true;
            }
        }
        return false;
    }
    public bool bIsKeyDown(int nKey)
    {
        return false;
    }
    public bool bIsKeyReleased(int nKey)
    {
        return false;
    }
    public bool bIsKeyUp(int nKey)
    {
        return false;
    }
    //-----------------
    #endregion

    #region [ IDisposable 実装 ]
    //-----------------
    public void Dispose()
    {
        this.listInputEvents.Clear();
        this.listEventBuffer.Clear();
    }
    //-----------------
    #endregion
}
