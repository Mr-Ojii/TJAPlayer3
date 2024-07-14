using SDL;

namespace FDK;

/// <summary>
/// 大規模な変更がめんどくさかったために作ったクラス
/// </summary>
public unsafe class Device
{
    internal SDL_Window* window;
    internal SDL_Renderer* renderer;

    internal Device(SDL_Window* window, SDL_Renderer* renderer)
    {
        this.window = window;
        this.renderer = renderer;
    }
}
