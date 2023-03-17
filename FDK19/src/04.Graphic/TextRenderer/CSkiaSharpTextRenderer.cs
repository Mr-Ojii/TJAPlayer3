using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

using Color = System.Drawing.Color;

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
            paint.Typeface = SKTypeface.FromFamilyName(fontpath, weight, width, slant);

        //stream・filepathから生成した場合に、style設定をどうすればいいのかがわからない
        if (File.Exists(fontpath))
            paint.Typeface = SKTypeface.FromFile(fontpath, 0);

        if (paint.Typeface == null)
            throw new FileNotFoundException(fontpath);

        paint.TextSize = (pt * 1.3f);
        paint.IsAntialias = true;
    }

    public CSkiaSharpTextRenderer(Stream fontstream, int pt, CFontRenderer.FontStyle style)
    {
        paint = new SKPaint();

        //stream・filepathから生成した場合に、style設定をどうすればいいのかがわからない
        paint.Typeface = SKFontManager.Default.CreateTypeface(fontstream);

        paint.TextSize = (pt * 1.3f);
        paint.IsAntialias = true;
    }

    public SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> DrawText(string drawstr, CFontRenderer.DrawMode drawMode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
    {
        if (string.IsNullOrEmpty(drawstr))
        {
            //nullか""だったら、1x1を返す
            return new Image<Rgba32>(1, 1);
        }

        string[] strs = drawstr.Split("\n");
        Image<Rgba32>[] images = new Image<Rgba32>[strs.Length];

        for (int i = 0; i < strs.Length; i++) {
            SKRect bounds = new SKRect();
            int width = (int)Math.Ceiling(paint.MeasureText(drawstr, ref bounds)) + 50;
            int height = (int)Math.Ceiling(paint.FontMetrics.Descent - paint.FontMetrics.Ascent) + 50;

            //少し大きめにとる(定数じゃない方法を考えましょう)
            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKCanvas canvas = new SKCanvas(bitmap);

            if (drawMode.HasFlag(CFontRenderer.DrawMode.Edge))
            {
                SKPaint edgePaint = new SKPaint();
                SKPath path = paint.GetTextPath(strs[i], 25, -paint.FontMetrics.Ascent + 25);
                edgePaint.StrokeWidth = paint.TextSize * 8 / edge_Ratio;
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

            canvas.DrawText(strs[i], 25, -paint.FontMetrics.Ascent + 25, paint);
            canvas.Flush();

            var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(bitmap.Bytes, width, height);
            SixLabors.ImageSharp.Rectangle rect = CCommon.MeasureForegroundArea(image, SixLabors.ImageSharp.Color.Transparent);

            //無だった場合は、スペースと判断する(縦書きレンダリングに転用したいがための愚策)
            if (rect != SixLabors.ImageSharp.Rectangle.Empty)
            {
                image.Mutate(ctx => ctx.Crop(rect));
                images[i] = image;
            }
            else
            {
                image.Dispose();
                images[i] = new Image<Rgba32>((int)paint.TextSize, (int)Math.Ceiling(paint.FontMetrics.Descent - paint.FontMetrics.Ascent));
            }
        }

        int ret_width = 0;
        int ret_height = 0;
        for(int i = 0; i < images.Length; i++)
        {
            ret_width = Math.Max(ret_width, images[i].Width);
            ret_height += images[i].Height;
        }
        
        Image<Rgba32> ret = new Image<Rgba32>(ret_width, ret_height);

        int height_i = 0;
        for (int i = 0; i < images.Length; i++) 
        {
            ret.Mutate(ctx => ctx.DrawImage(images[i], new Point(0, height_i), 1));
            height_i += images[i].Height;
            images[i].Dispose();
        }

        return ret;
    }

    public void Dispose()
    {
        paint.Dispose();
    }

    private SKPaint paint = null;
}
