namespace FDK;

public enum EInputEventType
{
    Pressed,
    Released,
}

// struct
[StructLayout(LayoutKind.Sequential)]
public struct STInputEvent
{
    public int nKey { get; set; }
    public EInputEventType eType { get; set; }
    public long nTimeStamp { get; set; }
}
