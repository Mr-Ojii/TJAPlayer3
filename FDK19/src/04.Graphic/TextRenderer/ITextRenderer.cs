using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Color = System.Drawing.Color;

namespace FDK
{
    internal interface ITextRenderer : IDisposable
    {
        Image<Rgba32> DrawText(string drawstr, CPrivateFont.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio);
    }
}