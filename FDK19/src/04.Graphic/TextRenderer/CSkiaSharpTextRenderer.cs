using SkiaSharp;

namespace FDK;

internal class CSkiaSharpTextRenderer : ITextRenderer
{
    //https://monobook.org/wiki/SkiaSharp%E3%81%A7%E6%97%A5%E6%9C%AC%E8%AA%9E%E6%96%87%E5%AD%97%E5%88%97%E3%82%92%E6%8F%8F%E7%94%BB%E3%81%99%E3%82%8B
    public CSkiaSharpTextRenderer(string fontpath, int pt)
        : this(fontpath, pt, CFontRenderer.FontStyle.Regular)
    {
    }

    public CSkiaSharpTextRenderer(string fontpath, int pt, CFontRenderer.FontStyle style)
    {
        paint = new SKPaint();
        font = new SKFont();

        SKFontStyleWeight weight = SKFontStyleWeight.Normal;
        SKFontStyleWidth width = SKFontStyleWidth.Normal;
        SKFontStyleSlant slant = SKFontStyleSlant.Upright;

        if (style.HasFlag(CFontRenderer.FontStyle.Bold))
        {
            weight = SKFontStyleWeight.Bold;
        }
        if (style.HasFlag(CFontRenderer.FontStyle.Italic))
        {
            slant = SKFontStyleSlant.Italic;
        }

        if (SKFontManager.Default.FontFamilies.Contains(fontpath))
            font.Typeface = SKTypeface.FromFamilyName(fontpath, weight, width, slant);

        //stream・filepathから生成した場合に、style設定をどうすればいいのかがわからない
        if (File.Exists(fontpath))
            font.Typeface = SKTypeface.FromFile(fontpath, 0);

        if (font.Typeface is null)
            throw new FileNotFoundException(fontpath);

        font.Size = (pt * 1.3f);
        paint.IsAntialias = true;
    }

    public CSkiaSharpTextRenderer(Stream fontstream, int pt, CFontRenderer.FontStyle style)
    {
        paint = new SKPaint();
        font = new SKFont();

        //stream・filepathから生成した場合に、style設定をどうすればいいのかがわからない
        font.Typeface = SKFontManager.Default.CreateTypeface(fontstream);

        font.Size = (pt * 1.3f);
        paint.IsAntialias = true;
    }

    public SKBitmap DrawText(string drawstr, CFontRenderer.DrawMode drawMode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
    {
        if (string.IsNullOrEmpty(drawstr) || this.paint is null)
        {
            //nullか""だったら、1x1を返す
            return new SKBitmap(1, 1, true);
        }

        string[] strs = drawstr.Split("\n");
        SKBitmap[] images = new SKBitmap[strs.Length];

        for (int i = 0; i < strs.Length; i++)
        {
            int width = (int)Math.Ceiling(this.font.MeasureText(drawstr)) + 50;
            int height = (int)Math.Ceiling(font.Metrics.Descent - font.Metrics.Ascent) + 50;

            //少し大きめにとる(定数じゃない方法を考えましょう)
            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                if (drawMode.HasFlag(CFontRenderer.DrawMode.Edge))
                {
                    SKPaint edgePaint = new SKPaint();
                    SKPath path = font.GetTextPath(strs[i], new SKPoint(25, -font.Metrics.Ascent + 25));
                    edgePaint.StrokeWidth = font.Size * 8 / edge_Ratio;
                    //https://docs.microsoft.com/ja-jp/xamarin/xamarin-forms/user-interface/graphics/skiasharp/paths/paths
                    edgePaint.StrokeJoin = SKStrokeJoin.Round;
                    edgePaint.Color = new SKColor(edgeColor.R, edgeColor.G, edgeColor.B, edgeColor.A);
                    edgePaint.Style = SKPaintStyle.Stroke;
                    edgePaint.IsAntialias = true;

                    canvas.DrawPath(path, edgePaint);
                }

                if (drawMode.HasFlag(CFontRenderer.DrawMode.Gradation))
                {
                    //https://docs.microsoft.com/ja-jp/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/shaders/linear-gradient
                    paint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 25),
                        new SKPoint(0, height - 25),
                        new SKColor[] {
                        new SKColor(gradationTopColor.R, gradationTopColor.G, gradationTopColor.B, gradationTopColor.A),
                        new SKColor(gradationBottomColor.R, gradationBottomColor.G, gradationBottomColor.B, gradationBottomColor.A) },
                        new float[] { 0, 1 },
                        SKShaderTileMode.Clamp);
                    paint.Color = new SKColor(0xffffffff);
                }
                else
                {
                    paint.Shader = null;
                    paint.Color = new SKColor(fontColor.R, fontColor.G, fontColor.B);
                }

                canvas.DrawText(strs[i], 25, -font.Metrics.Ascent + 25, SKTextAlign.Left, font, paint);
                canvas.Flush();
            }

            var rect = CCommon.MeasureForegroundArea(bitmap, SKColor.Empty);

            //無だった場合は、スペースと判断する(縦書きレンダリングに転用したいがための愚策)
            if (rect != SKRectI.Empty)
            {
                SKBitmap crop_bitmap = new SKBitmap(rect.Width, rect.Height);
                using (SKCanvas crop_canvas = new SKCanvas(crop_bitmap))
                {
                    SKRect dest = new SKRect(0, 0, rect.Width, rect.Height);
                    crop_canvas.DrawBitmap(bitmap, rect, dest);
                    bitmap.Dispose();
                    images[i] = crop_bitmap;
                }
            }
            else
            {
                bitmap.Dispose();
                images[i] = new SKBitmap((int)font.Size, (int)Math.Ceiling(font.Metrics.Descent - font.Metrics.Ascent));
            }
        }

        int ret_width = 0;
        int ret_height = 0;
        for (int i = 0; i < images.Length; i++)
        {
            ret_width = Math.Max(ret_width, images[i].Width);
            ret_height += images[i].Height;
        }

        SKBitmap ret = new SKBitmap(ret_width, ret_height, true);
        using (SKCanvas ret_canvas = new SKCanvas(ret))
        {
            int height_i = 0;
            for (int i = 0; i < images.Length; i++)
            {
                ret_canvas.DrawBitmap(images[i], new SKPoint(0, height_i));
                height_i += images[i].Height;
                images[i].Dispose();
            }
        }

        return ret;
    }

    public void Dispose()
    {
        this.paint?.Dispose();
    }

    private SKPaint paint;
    private SKFont font;
}
