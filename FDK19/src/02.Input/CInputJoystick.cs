using SDL;

namespace FDK;

public unsafe class CInputJoystick : IInputDevice, IDisposable
{
    // コンストラクタ

    public CInputJoystick(SDL_JoystickID joystickindex)
    {
        this.joystick_handle = SDL3.SDL_OpenJoystick(joystickindex);
        this.eInputDeviceType = EInputDeviceType.Joystick;
        this.ID = (int)joystickindex;
        string? _guid = SDL3.SDL_GetJoystickGUID(joystick_handle).ToString();
        this.GUID = _guid is null ? "" : _guid;

        for (int i = 0; i < this.bButtonState.Length; i++)
            this.bButtonState[i] = false;

        this.listInputEvents = new List<STInputEvent>();
        this.listEventBuffer = new ConcurrentQueue<STInputEvent>();
    }


    // メソッド

    #region [ IInputDevice 実装 ]
    //-----------------
    public EInputDeviceType eInputDeviceType
    {
        get;
        private set;
    }
    public string GUID
    {
        get;
        private set;
    }
    public int ID
    {
        get;
        private set;
    }
    public List<STInputEvent> listInputEvents
    {
        get;
        private set;
    }

    public void tPolling(bool bIsWindowActive)
    {
        if (bIsWindowActive)
        {
            #region [ 入力 ]
            //-----------------------------
            {
                #region [ X軸－ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 0) < -16384)
                {
                    if (this.btmpButtonState[0] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 0,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[0] = true;
                        this.btmpButtonPushDown[0] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[0] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 0,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[0] = false;
                        this.btmpButtonPullUp[0] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ X軸＋ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 0) > 16384)
                {
                    if (this.btmpButtonState[1] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 1,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[1] = true;
                        this.btmpButtonPushDown[1] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[1] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent event7 = new STInputEvent()
                            {
                                nKey = 1,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(event7);
                        }

                        this.btmpButtonState[1] = false;
                        this.btmpButtonPullUp[1] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Y軸－ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 1) < -16384)
                {
                    if (this.btmpButtonState[2] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 2,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[2] = true;
                        this.btmpButtonPushDown[2] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[2] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 2,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[2] = false;
                        this.btmpButtonPullUp[2] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Y軸＋ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 1) > 16384)
                {
                    if (this.btmpButtonState[3] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 3,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[3] = true;
                        this.btmpButtonPushDown[3] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[3] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 3,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[3] = false;
                        this.btmpButtonPullUp[3] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Z軸－ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 2) < -16384)
                {
                    if (this.btmpButtonState[4] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 4,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[4] = true;
                        this.btmpButtonPushDown[4] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[4] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 4,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[4] = false;
                        this.btmpButtonPullUp[4] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Z軸＋ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 2) > 16384)
                {
                    if (this.btmpButtonState[5] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 5,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[5] = true;
                        this.btmpButtonPushDown[5] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[5] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent event15 = new STInputEvent()
                            {
                                nKey = 5,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(event15);
                        }

                        this.btmpButtonState[5] = false;
                        this.btmpButtonPullUp[5] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Z軸回転－ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 3) < -16384)
                {
                    if (this.btmpButtonState[6] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 6,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[6] = true;
                        this.btmpButtonPushDown[6] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[4] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 6,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[6] = false;
                        this.btmpButtonPullUp[6] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Z軸回転＋ ]
                //-----------------------------
                if (SDL3.SDL_GetJoystickAxis(joystick_handle, 3) > 16384)
                {
                    if (this.btmpButtonState[7] == false)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent ev = new STInputEvent()
                            {
                                nKey = 7,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(ev);
                        }

                        this.btmpButtonState[7] = true;
                        this.btmpButtonPushDown[7] = true;
                    }
                }
                else
                {
                    if (this.btmpButtonState[7] == true)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent event15 = new STInputEvent()
                            {
                                nKey = 7,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(event15);
                        }

                        this.btmpButtonState[7] = false;
                        this.btmpButtonPullUp[7] = true;
                    }
                }
                //-----------------------------
                #endregion
                #region [ Button ]
                //-----------------------------
                bool bIsButtonPressedReleased = false;
                for (int j = 0; j < 128; j++)
                {
                    bool buttonState = SDL3.SDL_GetJoystickButton(joystick_handle, j);
                    if (this.btmpButtonState[8 + j] == false && buttonState)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent item = new STInputEvent()
                            {
                                nKey = 8 + j,
                                eType = EInputEventType.Pressed,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(item);
                        }

                        this.btmpButtonState[8 + j] = true;
                        this.btmpButtonPushDown[8 + j] = true;
                        bIsButtonPressedReleased = true;
                    }
                    else if (this.btmpButtonState[8 + j] == true && !buttonState)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent item = new STInputEvent()
                            {
                                nKey = 8 + j,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(item);
                        }

                        this.btmpButtonState[8 + j] = false;
                        this.btmpButtonPullUp[8 + j] = true;
                        bIsButtonPressedReleased = true;
                    }
                }
                //-----------------------------
                #endregion
                // #24341 2011.3.12 yyagi: POV support
                #region [ POV HAT 4/8way (only single POV switch is supported)]
                byte hatState = SDL3.SDL_GetJoystickHat(joystick_handle, 0);

                for (int nWay = 0; nWay < 8; nWay++)
                {
                    if (hatState == hatList[nWay])
                    {
                        int nButtonKey = 8 + 128 + nWay;
                        if (this.btmpButtonState[nButtonKey] == false)
                        {
                            if (CSoundManager.rc演奏用タイマ is not null)
                            {
                                STInputEvent stevent = new STInputEvent()
                                {
                                    nKey = nButtonKey,
                                    eType = EInputEventType.Pressed,
                                    nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                                };
                                this.listEventBuffer.Enqueue(stevent);
                            }

                            this.btmpButtonState[nButtonKey] = true;
                            this.btmpButtonPushDown[nButtonKey] = true;
                        }
                        bIsButtonPressedReleased = true;
                    }
                }
                if (bIsButtonPressedReleased == false) // #xxxxx 2011.12.3 yyagi 他のボタンが何も押され/離されてない＝POVが離された
                {
                    int nWay = 0;
                    for (int i = 8 + 0x80; i < 8 + 0x80 + 8; i++)
                    {                                           // 離されたボタンを調べるために、元々押されていたボタンを探す。
                        if (this.btmpButtonState[i] == true)   // DirectInputを直接いじるならこんなことしなくて良いのに、あぁ面倒。
                        {                                       // この処理が必要なために、POVを1個しかサポートできない。無念。
                            nWay = i;
                            break;
                        }
                    }
                    if (nWay != 0)
                    {
                        if (CSoundManager.rc演奏用タイマ is not null)
                        {
                            STInputEvent stevent = new STInputEvent()
                            {
                                nKey = nWay,
                                eType = EInputEventType.Released,
                                nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                            };
                            this.listEventBuffer.Enqueue(stevent);
                        }

                        this.btmpButtonState[nWay] = false;
                        this.btmpButtonPullUp[nWay] = true;
                    }
                }
                #endregion
                //-----------------------------
                #endregion
            }
        }
    }

    public void tSwapEventList()
    {
        this.listInputEvents.Clear();
        for (int i = 0; i < 256; i++)
        {
            //Swap
            this.bButtonPushDown[i] = this.btmpButtonPushDown[i];
            this.bButtonPullUp[i] = this.btmpButtonPullUp[i];
            this.bButtonState[i] = this.btmpButtonState[i];

            //Clear
            this.btmpButtonPushDown[i] = false;
            this.btmpButtonPullUp[i] = false;
        }
        while (this.listEventBuffer.TryDequeue(out var InputEvent))
            this.listInputEvents.Add(InputEvent);
    }

    public bool bIsKeyPressed(int nButton)
    {
        return this.bButtonPushDown[nButton];
    }
    public bool bIsKeyDown(int nButton)
    {
        return this.bButtonState[nButton];
    }
    public bool bIsKeyReleased(int nButton)
    {
        return this.bButtonPullUp[nButton];
    }
    public bool bIsKeyUp(int nButton)
    {
        return !this.bButtonState[nButton];
    }
    //-----------------
    #endregion

    #region [ IDisposable 実装 ]
    //-----------------
    public void Dispose()
    {
        if (!this.bDisposed)
        {
            if (SDL3.SDL_JoystickConnected(joystick_handle))
            {
                SDL3.SDL_CloseJoystick(joystick_handle);
            }
            this.listInputEvents.Clear();
            this.listEventBuffer.Clear();
            this.bDisposed = true;
        }
    }
    //-----------------
    #endregion


    // その他

    #region [ private ]
    //-----------------
    private bool[] bButtonPullUp = new bool[0x100];
    private bool[] bButtonPushDown = new bool[0x100];
    private bool[] bButtonState = new bool[0x100];      // 0-5: XYZ, 6 - 0x128+5: buttons, 0x128+6 - 0x128+6+8: POV/HAT
    private bool[] btmpButtonPullUp = new bool[0x100];
    private bool[] btmpButtonPushDown = new bool[0x100];
    private bool[] btmpButtonState = new bool[0x100];      // 0-5: XYZ, 6 - 0x128+5: buttons, 0x128+6 - 0x128+6+8: POV/HAT
    private ConcurrentQueue<STInputEvent> listEventBuffer;
    private bool bDisposed;

    private SDL_Joystick* joystick_handle;

    private uint[] hatList =
    {
        SDL3.SDL_HAT_UP,
        SDL3.SDL_HAT_RIGHTUP,
        SDL3.SDL_HAT_RIGHT,
        SDL3.SDL_HAT_RIGHTDOWN,
        SDL3.SDL_HAT_DOWN,
        SDL3.SDL_HAT_LEFTDOWN,
        SDL3.SDL_HAT_LEFT,
        SDL3.SDL_HAT_LEFTUP,
    };
    //-----------------
    #endregion
}
