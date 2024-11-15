namespace FDK;

public class CTraceLogListener : TraceListener
{
    public CTraceLogListener(StreamWriter stream)
    {
        this.streamWriter = stream;
    }

    public override void Flush()
    {
        if (this.streamWriter is null)
            return;

        try
        {
            this.streamWriter.Flush();
        }
        catch (ObjectDisposedException)
        {
        }
    }
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
    {
        if (this.streamWriter is null)
            return;

        try
        {
            this.tWriteEventType(eventType);
            this.tWriteIndent();
            this.streamWriter.WriteLine(message);
        }
        catch (ObjectDisposedException)
        {
        }
    }
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
    {
        if (format is null || args is null || this.streamWriter is null)
            return;

        try
        {
            this.tWriteEventType(eventType);
            this.tWriteIndent();
            this.streamWriter.WriteLine(string.Format(format, args));
        }
        catch (ObjectDisposedException)
        {
        }
    }
    public override void Write(string? message)
    {
        if (this.streamWriter is null)
            return;

        try
        {
            this.streamWriter.Write(message);
        }
        catch (ObjectDisposedException)
        {
        }
    }
    public override void WriteLine(string? message)
    {
        if (this.streamWriter is null)
            return;

        try
        {
            this.streamWriter.WriteLine(message);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (this.streamWriter is null)
            return;

        try
        {
            this.streamWriter.Close();
        }
        catch
        {
        }

        this.streamWriter = null;

        base.Dispose(disposing);
    }

    #region [ private ]
    //-----------------
    private StreamWriter? streamWriter;

    private void tWriteEventType(TraceEventType eventType)
    {
        if (this.streamWriter is not null)
        {
            try
            {
                var now = DateTime.Now;
                this.streamWriter.Write(string.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3} ", new object[] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond }));
                switch (eventType)
                {
                    case TraceEventType.Error:
                        this.streamWriter.Write("[ERROR] ");
                        return;

                    case (TraceEventType.Error | TraceEventType.Critical):
                        return;

                    case TraceEventType.Warning:
                        this.streamWriter.Write("[WARNING] ");
                        return;

                    case TraceEventType.Information:
                        break;

                    default:
                        return;
                }
                this.streamWriter.Write("[INFO] ");
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
    private void tWriteIndent()
    {
        if ((this.streamWriter is not null) && (base.IndentLevel > 0))
        {
            try
            {
                for (int i = 0; i < base.IndentLevel; i++)
                    this.streamWriter.Write("    ");
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
    //-----------------
    #endregion
}
