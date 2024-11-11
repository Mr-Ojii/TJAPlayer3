using SDL;

namespace FDK;

public class CInputKeyboard : IInputDevice, IDisposable
{
    // コンストラクタ

    public CInputKeyboard()
    {
        this.eInputDeviceType = EInputDeviceType.Keyboard;
        this.GUID = "";
        this.ID = 0;

        for (int i = 0; i < this.bKeyState.Length; i++)
            this.bKeyState[i] = false;

        this.listInputEvents = new List<STInputEvent>();
        this.listEventBuffer = new ConcurrentQueue<STInputEvent>();
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
            unsafe
            {
                //-----------------------------
                int numkeys;
                byte* currentState = (byte*)SDL3.SDL_GetKeyboardState(&numkeys);

                for (int index = 0; index < (int)SDL.SDL_Scancode.SDL_SCANCODE_COUNT; index++)
                {
                    // #xxxxx: 2022.02.09 Mr-Ojii: SDLK (SDL.SDL_Scancode) を SlimDX.DirectInput.Key に変換。
                    if (!DeviceConstantConverter.SDLKToKey.TryGetValue((SDL_Scancode)index, out var key))
                        continue;

                    if (currentState[index] == 1)
                    {
                        if (this.btmpKeyState[(int)key] == false)
                        {
                            if (CSoundManager.rc演奏用タイマ is not null)
                            {
                                var ev = new STInputEvent()
                                {
                                    nKey = (int)key,
                                    eType = EInputEventType.Pressed,
                                    nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                                };
                                this.listEventBuffer.Enqueue(ev);
                            }

                            this.btmpKeyState[(int)key] = true;
                            this.btmpKeyPushDown[(int)key] = true;
                        }
                    }
                    else
                    {
                        if (this.btmpKeyState[(int)key] == true) // 前回は押されているのに今回は押されていない → 離された
                        {
                            if (CSoundManager.rc演奏用タイマ is not null)
                            {
                                var ev = new STInputEvent()
                                {
                                    nKey = (int)key,
                                    eType = EInputEventType.Released,
                                    nTimeStamp = CSoundManager.rc演奏用タイマ.nシステム時刻ms, // 演奏用タイマと同じタイマを使うことで、BGMと譜面、入力ずれを防ぐ。
                                };
                                this.listEventBuffer.Enqueue(ev);
                            }

                            this.btmpKeyState[(int)key] = false;
                            this.btmpKeyPullUp[(int)key] = true;
                        }
                    }
                }
            }
            //-----------------------------
        }
    }
    public void tSwapEventList()
    {
        this.listInputEvents.Clear();
        for (int i = 0; i < 256; i++)
        {
            //Swap
            this.bKeyPullUp[i] = this.btmpKeyPullUp[i];
            this.bKeyPushDown[i] = this.btmpKeyPushDown[i];
            this.bKeyState[i] = this.btmpKeyState[i];

            //Clear
            this.btmpKeyPushDown[i] = false;
            this.btmpKeyPullUp[i] = false;
        }
        while (listEventBuffer.TryDequeue(out var InputEvent))
            this.listInputEvents.Add(InputEvent);
    }


    /// <param name="nKey">
    ///		調べる SlimDX.DirectInput.Key を int にキャストした値。
    /// </param>
    public bool bIsKeyPressed(int nKey)
    {
        return this.bKeyPushDown[nKey];
    }

    /// <param name="nKey">
    ///		調べる SlimDX.DirectInput.Key を int にキャストした値。
    /// </param>
    public bool bIsKeyDown(int nKey)
    {
        return this.bKeyState[nKey];
    }

    /// <param name="nKey">
    ///		調べる SlimDX.DirectInput.Key を int にキャストした値。
    /// </param>
    public bool bIsKeyReleased(int nKey)
    {
        return this.bKeyPullUp[nKey];
    }

    /// <param name="nKey">
    ///		調べる SlimDX.DirectInput.Key を int にキャストした値。
    /// </param>
    public bool bIsKeyUp(int nKey)
    {
        return !this.bKeyState[nKey];
    }
    //-----------------
    #endregion

    #region [ IDisposable 実装 ]
    //-----------------
    public void Dispose()
    {
        if (!this.bDisposed)
        {
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
    private bool bDisposed;
    private bool[] bKeyPullUp = new bool[256];
    private bool[] bKeyPushDown = new bool[256];
    private bool[] bKeyState = new bool[256];
    private bool[] btmpKeyPullUp = new bool[256];
    private bool[] btmpKeyPushDown = new bool[256];
    private bool[] btmpKeyState = new bool[256];
    private ConcurrentQueue<STInputEvent> listEventBuffer;
    //-----------------
    #endregion
}
