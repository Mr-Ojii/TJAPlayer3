using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

namespace FDK
{
    public class CSixLaborsTextRenderer : IDisposable
    {
        public CSixLaborsTextRenderer( string fontpath, int pt )
        {
            Initialize(fontpath, pt, FontStyle.Regular);
        }

        public CSixLaborsTextRenderer(string fontpath, int pt, FontStyle style)
        {
            Initialize(fontpath, pt, style);
        }

        protected void Initialize(string fontpath, int pt, FontStyle style) 
        {
            this.pt = pt;
            this.fontStyle = style;

            if (File.Exists(fontpath))
            {
                this.fontFamily = new FontCollection().Install(fontpath);
            }
            else if (SystemFonts.TryFind(fontpath, out this.fontFamily))
            {
                //システムフォント
            }
            else 
            {
                this.fontFamily = new FontCollection().Install(Assembly.GetExecutingAssembly().GetManifestResourceStream(@"FDK.mplus-1p-medium.ttf"));
            }
            this.font = this.fontFamily.CreateFont(this.pt, this.fontStyle);
        }

        public Image<Rgba32> DrawPrivateFont(string drawstr, Color fontColor)
        {
            return DrawPrivateFont(drawstr, CPrivateFont.DrawMode.Normal, fontColor, Color.White, Color.White, Color.White, 0);
        }

        protected Image<Rgba32> DrawPrivateFont(string drawstr, CPrivateFont.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
        {
            FontRectangle size = TextMeasurer.Measure(drawstr, new RendererOptions(this.font));

            Image<Rgba32> image = new Image<Rgba32>((int)size.Width + 10, (int)size.Height + 10);


            if (drawmode == CPrivateFont.DrawMode.Normal)
            {
                image.Mutate(ctx => ctx.DrawText(drawstr, this.font, fontColor, PointF.Empty));
            }
            else if (drawmode == CPrivateFont.DrawMode.Edge)
            {
                image.Mutate(ctx => ctx.DrawText(drawstr, this.font, new SolidBrush(fontColor), new Pen(edgeColor, edge_Ratio), PointF.Empty));
            }
            else if (drawmode == CPrivateFont.DrawMode.Gradation)
            {
            }
            else if (drawmode == (CPrivateFont.DrawMode.Gradation | CPrivateFont.DrawMode.Edge))
            {
            }
            return image;
        }

        public void Dispose() 
        {
        }

        private int pt = 12;
        private FontStyle fontStyle;
        private FontFamily fontFamily;
        private Font font;
    }
}
