using SDL;
using SkiaSharp;
using System.Threading;

namespace FDK;

public unsafe class CTexture : IDisposable
{
    // プロパティ

    public float fRotation;
    public int Opacity
    {
        get
        {
            return this._opacity;
        }
        set
        {
            if (this.texture is null)
                return;

            this._opacity = Math.Clamp(value, 0, 0xff);
            SDL3.SDL_SetTextureAlphaMod(this.texture, (byte)this._opacity);
        }
    }
    public Color color
    {
        get
        {
            return this._color;
        }
        set
        {
            this._color = value;
        }
    }
    public Size szTextureSize
    {
        get
        {
            return this.rcImageRect.Size;
        }
    }
    public EBlendMode eBlendMode
    {
        get
        {
            return this._eBlendMode;
        }
        set
        {
            if (this.texture is null)
                return;

            this._eBlendMode = value;
            switch (value)
            {
                case EBlendMode.Addition:
                    SDL3.SDL_SetTextureBlendMode(this.texture, SDL.SDL_BlendMode.SDL_BLENDMODE_ADD);
                    break;
                default:
                    SDL3.SDL_SetTextureBlendMode(this.texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    break;
            }
        }
    }
    public Vector2 vcScaling;

    private bool bTextureDisposed => this.texture is null;

    // コンストラクタ

    public CTexture()
    {
        this._opacity = 0xff;
        this.texture = null;
        this.fRotation = 0f;
        this.vcScaling = new Vector2(1f, 1f);
        this.filename = "";
    }

    /// <summary>
    /// <para>指定された画像ファイルから Managed テクスチャを作成する。</para>
    /// <para>利用可能な画像形式は、BMP, JPG, PNG, TGA, DDS, PPM, DIB, HDR, PFM のいずれか。</para>
    /// </summary>
    /// <param name="device">Direct3D9 デバイス。</param>
    /// <param name="strFilename">画像ファイル名。</param>
    /// <exception cref="CTextureCreateFailedException">テクスチャの作成に失敗しました。</exception>
    public CTexture(Device device, string strFilename)
        : this()
    {
        maketype = MakeType.filename;
        filename = strFilename;
        MakeTexture(device, strFilename);
    }

    public CTexture(Device device, SKBitmap bitmap)
        : this()
    {
        maketype = MakeType.bitmap;
        MakeTexture(device, bitmap);
    }

    public void MakeTexture(Device device, string strFilename)
    {
        if (!File.Exists(strFilename))     // #27122 2012.1.13 from: ImageInformation では FileNotFound 例外は返ってこないので、ここで自分でチェックする。わかりやすいログのために。
            throw new FileNotFoundException(string.Format("File does not exist. \n[{0}]", strFilename));


        using (var bitmap = SKBitmap.Decode(strFilename))
            MakeTexture(device, bitmap);
    }

    public unsafe void MakeTexture(Device device, SKBitmap bitmap)
    {
        try
        {
            this.rcImageRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            this.texture = SDL3.SDL_CreateTexture(device.renderer, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, bitmap.Width, bitmap.Height);

            if (this.texture is null)
                throw new Exception("Failed to create texture.");

            //SKColorはArgb8888で格納されるが、無駄コピーが発生するのでどうにかしたい
            //Dictionary<SKColorType, uint>を作れば良い？
            fixed(void* ptr = bitmap.Pixels)
            {
                SDL3.SDL_UpdateTexture(this.texture, null, (nint)ptr, bitmap.Width * 4);
            }

            this.color = Color.FromArgb(255, 255, 255, 255);
            this.eBlendMode = EBlendMode.Normal;
            this.Opacity = 255;
        }
        catch
        {
            this.Dispose();
            throw new CTextureCreateFailedException(string.Format("Failed to create texture."));
        }
    }

    // メソッド
    public void UpdateTexture(nint bitmap, Size size)
    {
        if (texture is null || this.szTextureSize != size)
            return;

        SDL_Rect rect = new SDL_Rect()
        {
            x = 0,
            y = 0,
            w = size.Width,
            h = size.Height
        };
        SDL3.SDL_UpdateTexture(this.texture, &rect, bitmap, size.Width * 4);
    }

    public void t2D拡大率考慮描画(Device device, RefPnt refpnt, float x, float y)
    {
        this.t2D拡大率考慮描画(device, refpnt, x, y, rcImageRect);
    }
    public void t2D拡大率考慮描画(Device device, RefPnt refpnt, float x, float y, Rectangle rect)
    {
        switch (refpnt)
        {
            case RefPnt.UpLeft:
                this.t2D描画(device, x, y, rect);
                break;
            case RefPnt.Up:
                this.t2D描画(device, x - (rect.Width / 2f * this.vcScaling.X), y, rect);
                break;
            case RefPnt.UpRight:
                this.t2D描画(device, x - rect.Width * this.vcScaling.X, y, rect);
                break;
            case RefPnt.Left:
                this.t2D描画(device, x, y - (rect.Height / 2f * this.vcScaling.Y), rect);
                break;
            case RefPnt.Center:
                this.t2D描画(device, x - (rect.Width / 2f * this.vcScaling.X), y - (rect.Height / 2f * this.vcScaling.Y), rect);
                break;
            case RefPnt.Right:
                this.t2D描画(device, x - rect.Width * this.vcScaling.X, y - (rect.Height / 2f * this.vcScaling.Y), rect);
                break;
            case RefPnt.DownLeft:
                this.t2D描画(device, x, y - rect.Height * this.vcScaling.Y, rect);
                break;
            case RefPnt.Down:
                this.t2D描画(device, x - (rect.Width / 2f * this.vcScaling.X), y - rect.Height * this.vcScaling.Y, rect);
                break;
            case RefPnt.DownRight:
                this.t2D描画(device, x - rect.Width * this.vcScaling.X, y - rect.Height * this.vcScaling.Y, rect);
                break;
            default:
                break;
        }
    }
    public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y)
    {
        this.t2D元サイズ基準描画(device, refpnt, x, y, rcImageRect);
    }
    public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y, Rectangle rect)
    {
        switch (refpnt)
        {
            case RefPnt.UpLeft:
                this.t2D描画(device, x, y, rect);
                break;
            case RefPnt.Up:
                this.t2D描画(device, x - (szTextureSize.Width / 2f), y, rect);
                break;
            case RefPnt.UpRight:
                this.t2D描画(device, x - szTextureSize.Width, y, rect);
                break;
            case RefPnt.Left:
                this.t2D描画(device, x, y - (szTextureSize.Height / 2f), rect);
                break;
            case RefPnt.Center:
                this.t2D描画(device, x - (szTextureSize.Width / 2f), y - (szTextureSize.Height / 2f), rect);
                break;
            case RefPnt.Right:
                this.t2D描画(device, x - szTextureSize.Width, y - (szTextureSize.Height / 2f), rect);
                break;
            case RefPnt.DownLeft:
                this.t2D描画(device, x, y - szTextureSize.Height, rect);
                break;
            case RefPnt.Down:
                this.t2D描画(device, x - (szTextureSize.Width / 2f), y - szTextureSize.Height, rect);
                break;
            case RefPnt.DownRight:
                this.t2D描画(device, x - szTextureSize.Width, y - szTextureSize.Height, rect);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// テクスチャを 2D 画像と見なして描画する。
    /// </summary>
    /// <param name="device">Direct3D9 デバイス。</param>
    /// <param name="x">描画位置（テクスチャの左上位置の X 座標[dot]）。</param>
    /// <param name="y">描画位置（テクスチャの左上位置の Y 座標[dot]）。</param>
    public void t2D描画(Device device, float x, float y)
        => this.t2D描画(device, x, y, this.rcImageRect);

    public void t2D描画(Device device, float x, float y, EFlipType eFlipType)
        => this.t2D描画(device, x, y, this.rcImageRect, eFlipType);

    public void t2D描画(Device device, float x, float y, Rectangle rc画像内の描画領域, EFlipType eFlipType = EFlipType.None)
    {
        if (this.texture is null)
            return;

        dstrect.x = x;
        dstrect.y = y;
        dstrect.w = rc画像内の描画領域.Width * this.vcScaling.X;
        dstrect.h = rc画像内の描画領域.Height * this.vcScaling.Y;

        srcrect.x = rc画像内の描画領域.X;
        srcrect.y = rc画像内の描画領域.Y;
        srcrect.w = rc画像内の描画領域.Width;
        srcrect.h = rc画像内の描画領域.Height;

        SDL3.SDL_SetTextureColorMod(this.texture, color.R, color.G, color.B);
        fixed(SDL_FRect* psrc = &srcrect)
            fixed(SDL_FRect* pdst = &dstrect)
                SDL3.SDL_RenderTextureRotated(device.renderer, this.texture, psrc, pdst, -(this.fRotation * 180 / Math.PI), null, (SDL.SDL_FlipMode)eFlipType);
    }


    public void t2D幕用描画(Device device, float x, float y, Rectangle rc画像内の描画領域, bool left, int num = 0)
    {
        if (this.texture is null)
            return;

        dstrect.x = x;
        dstrect.y = y;
        dstrect.w = rc画像内の描画領域.Width * this.vcScaling.X;
        dstrect.h = rc画像内の描画領域.Height * this.vcScaling.Y;

        srcrect.x = rc画像内の描画領域.X;
        srcrect.y = rc画像内の描画領域.Y;
        srcrect.w = rc画像内の描画領域.Width;
        srcrect.h = rc画像内の描画領域.Height;

        SDL3.SDL_SetTextureColorMod(this.texture, color.R, color.G, color.B);
        fixed(SDL_FRect* psrc = &srcrect)
            fixed(SDL_FRect* pdst = &dstrect)
                SDL3.SDL_RenderTexture(device.renderer, this.texture, psrc, pdst);
    }

    #region [ IDisposable 実装 ]
    //-----------------
    public void Dispose()
    {
        if (!this.bDisposed)
        {
            // テクスチャの破棄
            if (this.texture is not null)
            {
                SDL3.SDL_DestroyTexture(this.texture);
                this.texture = null;
            }

            this.bDisposed = true;
        }
    }
    ~CTexture()
    {
        // ファイナライザの動作時にtextureのDisposeがされていない場合は、
        // CTextureのDispose漏れと見做して警告をログ出力する
        if (!this.bTextureDisposed)//DTXManiaより
        {
            Trace.TraceWarning("CTexture: Texture memory leak detected.(Size=({0}, {1}), filename={2}, maketype={3})", szTextureSize.Width, szTextureSize.Height, filename, maketype.ToString());
        }
    }
    //-----------------
    #endregion

    // その他

    public enum RefPnt
    {
        UpLeft,
        Up,
        UpRight,
        Left,
        Center,
        Right,
        DownLeft,
        Down,
        DownRight,
    }

    public enum EBlendMode
    {
        Normal,
        Addition,
    }

    [Flags]
    public enum EFlipType
    {
        None = 0,
        /// <summary>
        /// 左右反転
        /// </summary>
        Horizontal = 1,
        /// <summary>
        /// 上下反転
        /// </summary>
        Vertical = 2,
    }

    #region [ private ]

    private enum MakeType
    {
        filename,
        bytearray,
        bitmap
    }

    // 2012.3.21 さらなる new の省略作戦

    private SDL_Texture* texture;
    private string filename;
    protected Rectangle rcImageRect;                              // テクスチャ作ったらあとは不変
    private int _opacity;
    private bool bDisposed;
    private EBlendMode _eBlendMode;
    private Color _color;
    private MakeType maketype = MakeType.bytearray;
    [ThreadStatic]
    private static SDL_FRect srcrect;
    [ThreadStatic]
    private static SDL_FRect dstrect;
    //-----------------
    #endregion
}
