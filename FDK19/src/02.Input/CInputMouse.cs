using SDL;

namespace FDK;

public unsafe class CInputMouse : IInputDevice, IDisposable
{
    // コンストラクタ

    public CInputMouse()
    {
        this.eInputDeviceType = EInputDeviceType.Mouse;
        this.GUID = "";
        this.ID = 0;

        for (int i = 0; i < this.bMouseState.Length; i++)
            this.bMouseState[i] = false;
        this.listInputEvents = new List<STInputEvent>();
        this.listEventBuffer = new ConcurrentQueue<STInputEvent>();
    }

    public static Point Position
    {
        get
        {
            float x, y;
            SDL3.SDL_GetMouseState(&x, &y);
            return new Point((int)x, (int)y);
        }
    }

    // メソッド

    #region [ IInputDevice 実装 ]
    //-----------------
    public EInputDeviceType eInputDeviceType { get; private set; }
    public string GUID { get; private set; }
    public int ID { get; private set; }
    public List<STInputEvent> listInputEvents { get; private set; }

    public void tPolling(bool bIsWindowActive)
    {
        if (bIsWindowActive)
        {
            //-----------------------------
            float x, y;
            SDL_MouseButtonFlags currentState = SDL3.SDL_GetMouseState(&x, &y);

            {
                for (int j = 0; j < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length; j++)
                {
                    if (this.btmpMouseState[j] == false && currentState.HasFlag(masklist[j]))
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            var ev = new STInputEvent()
                            {
                                nKey = j,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpMouseState[j] = true;
                        this.btmpMousePushDown[j] = true;
                    }
                    else if (this.btmpMouseState[j] == true && !((currentState & masklist[j]) != 0))
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            var ev = new STInputEvent()
                            {
                                nKey = j,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpMouseState[j] = false;
                        this.btmpMousePullUp[j] = true;
                    }
                }
            }
            //-----------------------------

        }
    }


    public void tSwapEventList()
    {
        this.listInputEvents.Clear();
        for (int i = 0; i < Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length; i++)
        {
            //Swap
            this.bMousePullUp[i] = this.btmpMousePullUp[i];
            this.bMousePushDown[i] = this.btmpMousePushDown[i];
            this.bMouseState[i] = this.btmpMouseState[i];

            //Clear
            this.btmpMousePushDown[i] = false;
            this.btmpMousePullUp[i] = false;
        }
        while (this.listEventBuffer.TryDequeue(out var InputEvent))
            this.listInputEvents.Add(InputEvent);
    }

    public bool bIsKeyPressed(int nButton)
    {
        return ((0 <= nButton) && (nButton < this.bMousePushDown.Length) && this.bMousePushDown[nButton]);
    }
    public bool bIsKeyDown(int nButton)
    {
        return ((0 <= nButton) && (nButton < this.bMouseState.Length) && this.bMouseState[nButton]);
    }
    public bool bIsKeyReleased(int nButton)
    {
        return ((0 <= nButton) && (nButton < this.bMousePullUp.Length) && this.bMousePullUp[nButton]);
    }
    public bool bIsKeyUp(int nButton)
    {
        return ((0 <= nButton) && (nButton < this.bMouseState.Length) && !this.bMouseState[nButton]);
    }
    //-----------------
    #endregion

    #region [ IDisposable 実装 ]
    //-----------------
    public void Dispose()
    {
        if (!this.bDisposed)
        {
            this.listEventBuffer.Clear();
            this.listInputEvents.Clear();
            this.bDisposed = true;
        }
    }
    //-----------------
    #endregion


    // その他

    #region [ private ]
    //-----------------
    private bool bDisposed;
    private bool[] bMousePullUp = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    private bool[] bMousePushDown = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    private bool[] bMouseState = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    private bool[] btmpMousePullUp = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    private bool[] btmpMousePushDown = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    private bool[] btmpMouseState = new bool[Enum.GetNames(typeof(SlimDXKeys.Mouse)).Length];
    public ConcurrentQueue<STInputEvent> listEventBuffer;

    private SDL_MouseButtonFlags[] masklist =
    {
        SDL_MouseButtonFlags.SDL_BUTTON_LMASK,
        SDL_MouseButtonFlags.SDL_BUTTON_MMASK,
        SDL_MouseButtonFlags.SDL_BUTTON_RMASK,
    };
    //-----------------
    #endregion
}
