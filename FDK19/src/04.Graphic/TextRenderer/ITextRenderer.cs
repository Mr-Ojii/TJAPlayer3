using SkiaSharp;

namespace FDK;

internal interface ITextRenderer : IDisposable
{
    SKBitmap DrawText(string drawstr, CFontRenderer.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio);
}
