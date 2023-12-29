namespace FDK;

public interface IInputDevice : IDisposable
{
    // プロパティ

    EInputDeviceType eInputDeviceType
    {
        get;
    }
    string GUID
    {
        get;
    }
    int ID
    {
        get;
    }
    List<STInputEvent> listInputEvents
    {
        get;
    }


    // メソッドインターフェース

    void tPolling(bool bIsWindowActive);
    void tSwapEventList();
    bool bIsKeyPressed(int nKey);
    bool bIsKeyDown(int nKey);
    bool bIsKeyReleased(int nKey);
    bool bIsKeyUp(int nKey);
}
