using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace FDK
{
    internal class CSkiaSharpTextRenderer : ITextRenderer
    {
        //https://monobook.org/wiki/SkiaSharp%E3%81%A7%E6%97%A5%E6%9C%AC%E8%AA%9E%E6%96%87%E5%AD%97%E5%88%97%E3%82%92%E6%8F%8F%E7%94%BB%E3%81%99%E3%82%8B
        public CSkiaSharpTextRenderer(string fontpath, int pt)
        {
            Initialize(fontpath, pt, SixLabors.Fonts.FontStyle.Regular);
        }

        public CSkiaSharpTextRenderer(string fontpath, int pt, SixLabors.Fonts.FontStyle style)
        {
            Initialize(fontpath, pt, style);
        }

        protected void Initialize(string fontpath, int pt, SixLabors.Fonts.FontStyle stylel)
        {
            paint = new SKPaint();

            SKFontStyleWeight weight = SKFontStyleWeight.Normal;
            SKFontStyleWidth width = SKFontStyleWidth.Normal;
            SKFontStyleSlant slant = SKFontStyleSlant.Upright;

            switch (stylel)
            {
                case SixLabors.Fonts.FontStyle.Bold:
                    weight = SKFontStyleWeight.Bold;
                    break;
                case SixLabors.Fonts.FontStyle.Italic:
                    slant = SKFontStyleSlant.Italic;
                    break;
                case SixLabors.Fonts.FontStyle.BoldItalic:
                    weight = SKFontStyleWeight.Bold;
                    slant = SKFontStyleSlant.Italic;
                    break;
            }

            if (!SkiaSharp.SKFontManager.Default.FontFamilies.Contains(fontpath))
                throw new FontFamilyNotFoundException(fontpath);

            paint.Typeface = SKTypeface.FromFamilyName(fontpath, weight, width, slant);

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

            SKRect bounds = new SKRect();
            int width = (int)Math.Ceiling(paint.MeasureText(drawstr, ref bounds)) + 50;
            int height = (int)Math.Ceiling(paint.FontMetrics.Descent - paint.FontMetrics.Ascent) + 50;

            //少し大きめにとる(定数じゃない方法を考えましょう)
            SKBitmap bitmap = new SKBitmap(width, height, false);
            SKCanvas canvas = new SKCanvas(bitmap);

            if (drawMode.HasFlag(CFontRenderer.DrawMode.Edge))
            {
                SKPaint edgePaint = new SKPaint();
                SKPath path = paint.GetTextPath(drawstr, 25, -paint.FontMetrics.Ascent + 25);
                edgePaint.StrokeWidth = paint.TextSize * 8 / edge_Ratio;
                //https://docs.microsoft.com/ja-jp/xamarin/xamarin-forms/user-interface/graphics/skiasharp/paths/paths
                edgePaint.StrokeJoin = SKStrokeJoin.Round;
                edgePaint.Color = new SKColor(edgeColor.R, edgeColor.G, edgeColor.B);
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
                        new SKColor(gradationTopColor.R, gradationTopColor.G, gradationTopColor.B),
                        new SKColor(gradationBottomColor.R, gradationBottomColor.G, gradationBottomColor.B) },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp);
                paint.Color = new SKColor(0xffffffff);
            }
            else
            {
                paint.Shader = null;
                paint.Color = new SKColor(fontColor.R, fontColor.G, fontColor.B);
            }

            canvas.DrawText(drawstr, 25, -paint.FontMetrics.Ascent + 25, paint);
            canvas.Flush();

            Stream stream = new MemoryStream();
            SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgra32>(bitmap.Bytes, width, height).Save(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            stream.Seek(0, SeekOrigin.Begin);
            var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(stream);
            SixLabors.ImageSharp.Rectangle rect = CCommon.MeasureForegroundArea(image, SixLabors.ImageSharp.Color.Transparent);

            //無だった場合は、スペースと判断する(縦書きレンダリングに転用したいがための愚策)
            if (rect != SixLabors.ImageSharp.Rectangle.Empty)
                image.Mutate(ctx => ctx.Crop(rect));
            else
            {
                image.Dispose();
                return new Image<Rgba32>((int)paint.TextSize, 1);
            }

            return image;

        }

        public void Dispose()
        {
            paint.Dispose();
        }

        private SKPaint paint = null;
    }
}
