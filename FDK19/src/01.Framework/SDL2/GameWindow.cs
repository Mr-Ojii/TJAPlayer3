using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SDL2;
using FDK;

namespace FDK.Windowing
{
    public class GameWindow :IDisposable
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
                SDL.SDL_RenderGetLogicalSize(_renderer_handle, out int width, out int height);
                return new Size(width, height);
            }
            set
            {
                SDL.SDL_RenderSetLogicalSize(_renderer_handle, value.Width, value.Height);
            }
        }

        public Size ClientSize
        {
            get
            {
                SDL.SDL_GetWindowSize(_window_handle, out int width, out int height);
                return new Size(width, height);
            }
            set
            {
                SDL.SDL_SetWindowSize(_window_handle, value.Width, value.Height);
            }
        }

        public int ClientWidth
        {
            get
            {
                SDL.SDL_GetWindowSize(_window_handle, out int width, out int _);
                return width;
            }
        }
        public int ClientHeight
        {
            get
            {
                SDL.SDL_GetWindowSize(_window_handle, out int _, out int height);
                return height;
            }
        }

        public bool Focused
        {
            get
            {
                return _focused;
            }
        }

        public string Title
        {
            get
            {
                return SDL.SDL_GetWindowTitle(_window_handle);
            }
            set
            {
                SDL.SDL_SetWindowTitle(_window_handle, value);
            }
        }

        public int X
        {
            get
            {
                SDL.SDL_GetWindowPosition(_window_handle, out int x, out int y);
                return x;
            }
        }
        public int Y
        {
            get
            {
                SDL.SDL_GetWindowPosition(_window_handle, out int x, out int y);
                return y;
            }
        }

        public Point Location
        {
            get
            {
                SDL.SDL_GetWindowPosition(_window_handle, out int x, out int y);
                return new Point(x, y);
            }
            set 
            {
                SDL.SDL_SetWindowPosition(_window_handle, value.X, value.Y);
            }
        }

        public bool VSync
        {
            set
            {
                SDL.SDL_RenderSetVSync(_renderer_handle, value ? 1 : 0);
            }
        }

        public unsafe Stream Icon
        {
            set
            {
                byte[] bytes = new byte[value.Length];
                value.Read(bytes, 0, bytes.Length);
                fixed (byte* ptr = bytes)
                    SDL.SDL_SetWindowIcon(_window_handle, (IntPtr)ptr);
            }
        }

        public WindowState WindowState
        {
            get
            {
                SDL.SDL_WindowFlags flag = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window_handle);
                if (flag.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN))
                    return WindowState.FullScreen;
                else if (flag.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP))
                    return WindowState.FullScreen_Desktop;
                return WindowState.Normal;
            }
            set
            {
                SDL.SDL_WindowFlags flag = 0;
                switch(value)
                {
                    case WindowState.FullScreen:
                        flag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
                        break;

                    case WindowState.FullScreen_Desktop:
                        flag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
                        break;
                }
                SDL.SDL_SetWindowFullscreen(_window_handle, (uint)flag);
            }
        }

        public string RendererName
        {
            get
            {
                SDL.SDL_GetRendererInfo(this._renderer_handle, out var info);
                return Marshal.PtrToStringUTF8(info.name);
            }
        }

        public void Exit()
        {
            _quit = true;
        }

        public GameWindow(string title, int width, int height)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_JOYSTICK);
            _window_handle = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (_window_handle == IntPtr.Zero)
                throw new Exception("Failed to create window.");

            _window_id = SDL.SDL_GetWindowID(_window_handle);

            _renderer_handle = IntPtr.Zero;
            int render_num = SDL.SDL_GetNumRenderDrivers();
            for(int i = 0; i < render_num; i++) {
                SDL.SDL_RendererInfo info;
                if(SDL.SDL_GetRenderDriverInfo(i, out info) == 0) {
                    if(Marshal.PtrToStringUTF8(info.name).Contains("opengl")) {
                        _renderer_handle = SDL.SDL_CreateRenderer(_window_handle, i, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
                        if(_renderer_handle != IntPtr.Zero)
                            break;
                    }
                }
            }
            if (_renderer_handle == IntPtr.Zero)
            {
                _renderer_handle = SDL.SDL_CreateRenderer(_window_handle, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
                if (_renderer_handle == IntPtr.Zero)
                {
                    SDL.SDL_DestroyWindow(_window_handle);
                    throw new Exception("Failed to create renderer.");
                }
            }
            this.Device = new Device(_window_handle, _renderer_handle);
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "linear");
            SDL.SDL_RenderSetLogicalSize(_renderer_handle, width, height);
        }

        public void Run()
        {
            this.OnLoad(new EventArgs());

            SDL.SDL_ShowWindow(_window_handle);

            SDL.SDL_Event poll_event;
            _quit = false;
            while (!_quit)
            {
                SDL.SDL_SetRenderDrawColor(_renderer_handle, 0x00, 0x00, 0x00, 0xff);
                SDL.SDL_RenderClear(_renderer_handle);

                this.OnRenderFrame(new EventArgs());

                while (SDL.SDL_PollEvent(out poll_event) != 0)
                {
                    switch (poll_event.type)
                    {
                        case SDL.SDL_EventType.SDL_WINDOWEVENT:
                            {
                                if(poll_event.window.windowID == _window_id)
                                    switch (poll_event.window.windowEvent)
                                    {
                                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                                            this.Move(_window_handle, new EventArgs());
                                            break;
                                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                            this.Resize(_window_handle, new EventArgs());
                                            break;
                                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                                            _focused = true;
                                            break;
                                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                                            _focused = false;
                                            break;
                                    }
                                    break;
                            }
                        case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                            this.MouseWheel(_window_handle, new MouseWheelEventArgs(poll_event.wheel.x, poll_event.wheel.y));
                            break;

                        case SDL.SDL_EventType.SDL_QUIT:
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

            this.OnUnload(new EventArgs());
        }

        protected void Render()
        {
            SDL.SDL_RenderPresent(_renderer_handle);
        }

        public bool SaveScreen(string strFullPath)
        {
            string strSavePath = Path.GetDirectoryName(strFullPath);
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
                SDL.SDL_GetRendererOutputSize(this._renderer_handle, out int width, out int height);
                SDL.SDL_Surface* sshot = (SDL.SDL_Surface*)SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
                SDL.SDL_Rect rect = new SDL.SDL_Rect()
                {
                    x = 0,
                    y = 0,
                    w = sshot->w,
                    h = sshot->h,
                };
                SDL.SDL_RenderReadPixels(this._renderer_handle, ref rect, SDL.SDL_PIXELFORMAT_ARGB8888, sshot->pixels, sshot->pitch);
                SDL.SDL_SaveBMP((IntPtr)sshot, strFullPath);
                SDL.SDL_FreeSurface((IntPtr)sshot);
            }

            return true;
        }

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(_renderer_handle);
            SDL.SDL_DestroyWindow(_window_handle);
            SDL.SDL_Quit();
        }

        protected virtual void OnLoad(EventArgs e)
        {

        }

        protected virtual void OnUnload(EventArgs e)
        {

        }

        protected virtual void OnClosing(CancelEventArgs e)
        {

        }

        protected virtual void OnRenderFrame(EventArgs e)
        {

        }

        protected event EventHandler Move;
        protected event EventHandler Resize;
        protected event MouseWheelEventHandler MouseWheel;


        private IntPtr _window_handle;
        private IntPtr _renderer_handle;
        private uint _window_id;
        private bool _quit;
        private bool _focused;
    }
}
