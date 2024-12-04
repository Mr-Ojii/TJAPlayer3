using System.Buffers;
using SDL;
using SkiaSharp;

namespace FDK.Windowing;

public unsafe class GameWindow : IDisposable
{
    public Device Device
    {
        get;
        private set;
    }

    public Size LogicalSize
    {
        get
        {
            int width, height;
            SDL_RendererLogicalPresentation _rlp;
            SDL3.SDL_GetRenderLogicalPresentation(_renderer_handle, &width, &height, &_rlp);
            return new Size(width, height);
        }
        set
        {
            SDL3.SDL_SetRenderLogicalPresentation(_renderer_handle, value.Width, value.Height, _renderer_logical_presentation);
        }
    }

    public Size ClientSize
    {
        get
        {
            int width, height;
            SDL3.SDL_GetWindowSize(_window_handle, &width, &height);
            return new Size(width, height);
        }
        set
        {
            SDL3.SDL_SetWindowSize(_window_handle, value.Width, value.Height);
        }
    }

    public bool Focused
    {
        get
        {
            return _focused;
        }
    }

    public string? Title
    {
        get
        {
            return SDL3.SDL_GetWindowTitle(_window_handle);
        }
        set
        {
            SDL3.SDL_SetWindowTitle(_window_handle, value);
        }
    }

    public Point Location
    {
        get
        {
            int x, y;
            SDL3.SDL_GetWindowPosition(_window_handle, &x, &y);
            return new Point(x, y);
        }
        set
        {
            SDL3.SDL_SetWindowPosition(_window_handle, value.X, value.Y);
        }
    }

    public bool VSync
    {
        set
        {
            SDL3.SDL_SetRenderVSync(_renderer_handle, value ? 1 : 0);
        }
    }

    public unsafe Stream Icon
    {
        set
        {
            byte[] arr = ArrayPool<byte>.Shared.Rent((int)value.Length);
            Span<byte> bytes = new Span<byte>(arr, 0, (int)value.Length);
            if (value.Read(bytes) > 0)
            {
                using (SKBitmap bmp = SKBitmap.Decode(bytes))
                {
                    fixed (void* ptr = bmp.Pixels)
                    {
                        var surface = SDL3.SDL_CreateSurfaceFrom(bmp.Width, bmp.Height, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, (nint)ptr, bmp.Width * 4);
                        SDL3.SDL_SetWindowIcon(_window_handle, surface);
                        SDL3.SDL_DestroySurface(surface);
                    }
                }
            }
            ArrayPool<byte>.Shared.Return(arr);
        }
    }

    public bool FullScreen
    {
        get
        {
            return _full_screen;
        }
        set
        {
            SDL3.SDL_SetWindowFullscreen(_window_handle, value);
            _full_screen = value;
        }
    }
    private bool _full_screen = false;

    public string RendererName
    {
        get
        {
            string? _renderer_name = SDL3.SDL_GetRendererName(this._renderer_handle);
            if (_renderer_name is null)
                return "null";
            else
                return _renderer_name;
        }
    }

    public void Exit()
    {
        _quit = true;
    }

    public GameWindow(string title, int width, int height)
    {
        SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_JOYSTICK);
        _window_handle = SDL3.SDL_CreateWindow(title, width, height, SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY | SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (_window_handle is null)
            throw new Exception("Failed to create window.");

        _window_id = SDL3.SDL_GetWindowID(_window_handle);

        _renderer_handle = SDL3.SDL_CreateRenderer(_window_handle, (byte*)null);
        if (_renderer_handle is null)
        {
            SDL3.SDL_DestroyWindow(_window_handle);
            throw new Exception("Failed to create renderer.");
        }
        this.Device = new Device(_window_handle, _renderer_handle);
        SDL3.SDL_SetRenderLogicalPresentation(_renderer_handle, width, height, _renderer_logical_presentation);
    }

    public void Run()
    {
        SDL3.SDL_ShowWindow(_window_handle);

        SDL_Event poll_event;
        _quit = false;
        while (!_quit)
        {
            SDL3.SDL_SetRenderDrawColor(_renderer_handle, 0x00, 0x00, 0x00, 0xff);
            SDL3.SDL_RenderClear(_renderer_handle);

            this.OnRenderFrame(new EventArgs());

            while (SDL3.SDL_PollEvent(&poll_event))
            {
                switch ((SDL_EventType)poll_event.type)
                {
                    case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                        if (poll_event.window.windowID == _window_id && this.Move is not null)
                            this.Move((nint)_window_handle, new EventArgs());
                        break;
                    case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                        if (poll_event.window.windowID == _window_id && this.Resize is not null)
                            this.Resize((nint)_window_handle, new EventArgs());
                        break;
                    case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                        if (poll_event.window.windowID == _window_id)
                            _focused = true;
                        break;
                    case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                        if (poll_event.window.windowID == _window_id)
                            _focused = false;
                        break;
                    case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                        if (this.MouseWheel is not null)
                            this.MouseWheel((nint)_window_handle, new MouseWheelEventArgs(poll_event.wheel.x, poll_event.wheel.y));
                        break;

                    case SDL_EventType.SDL_EVENT_QUIT:
                        CancelEventArgs cancelEventArgs = new CancelEventArgs();
                        this.OnClosing(cancelEventArgs);
                        if (!cancelEventArgs.Cancel)
                        {
                            _quit = true;
                        }
                        break;
                }
            }
        }
        OnClosed(new EventArgs());
    }

    protected void Render()
    {
        SDL3.SDL_RenderPresent(_renderer_handle);
    }

    public bool SaveScreen(string strFullPath)
    {
        if (strFullPath is null)
            return false;

        string? strSavePath = Path.GetDirectoryName(strFullPath);

        if (strSavePath is null)
            return false;

        if (!Directory.Exists(strSavePath))
        {
            try
            {
                Directory.CreateDirectory(strSavePath);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues.");
                return false;
            }
        }

        unsafe
        {
            SDL_Surface* sshot = SDL3.SDL_RenderReadPixels(this._renderer_handle, null);
            if (strFullPath.EndsWith("bmp"))
                SDL3.SDL_SaveBMP(sshot, strFullPath);
            else
            {
                SKEncodedImageFormat fmt = SKEncodedImageFormat.Png;
                if (strFullPath.EndsWith("jpg") || strFullPath.EndsWith("jpeg"))
                    fmt = SKEncodedImageFormat.Jpeg;
                else if (strFullPath.EndsWith("webp"))
                    fmt = SKEncodedImageFormat.Webp;

                var io = SDL3.SDL_IOFromDynamicMem();
                if (SDL3.SDL_SaveBMP_IO(sshot, io, false))
                {
                    var io_size = SDL3.SDL_GetIOSize(io);
                    byte[] arr = ArrayPool<byte>.Shared.Rent((int)io_size);
                    SDL3.SDL_SeekIO(io, 0, SDL_IOWhence.SDL_IO_SEEK_SET);
                    fixed (byte* ptr = arr)
                        SDL3.SDL_ReadIO(io, (nint)ptr, (nuint)io_size);
                    using (var bmp = SKBitmap.Decode(arr))
                    {
                        using (var str = File.Create(strFullPath))
                        {
                            var data = bmp.Encode(fmt, 100);
                            data.SaveTo(str);
                        }
                    }
                    ArrayPool<byte>.Shared.Return(arr);
                }
                SDL3.SDL_CloseIO(io);
            }
            SDL3.SDL_DestroySurface(sshot);
        }

        return true;
    }

    public void Dispose()
    {
        SDL3.SDL_DestroyRenderer(_renderer_handle);
        SDL3.SDL_DestroyWindow(_window_handle);
        SDL3.SDL_Quit();
    }

    protected virtual void OnClosing(CancelEventArgs e)
    {

    }

    protected virtual void OnClosed(EventArgs e)
    {

    }

    protected virtual void OnRenderFrame(EventArgs e)
    {

    }

    protected event EventHandler? Move;
    protected event EventHandler? Resize;
    protected event MouseWheelEventHandler? MouseWheel;


    private SDL_Window* _window_handle = null;
    private SDL_Renderer* _renderer_handle = null;
    private SDL_WindowID _window_id;
    private bool _quit;
    private bool _focused;

    private const SDL_RendererLogicalPresentation _renderer_logical_presentation = SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_LETTERBOX;
}
