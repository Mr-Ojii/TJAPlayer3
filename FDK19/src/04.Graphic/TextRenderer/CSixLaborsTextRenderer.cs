using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using Color = System.Drawing.Color;

namespace FDK
{
	internal class CSixLaborsTextRenderer : ITextRenderer
	{
		public CSixLaborsTextRenderer(string fontpath, int pt)
		{
			Initialize(fontpath, pt, FontStyle.Regular);
		}

		public CSixLaborsTextRenderer(string fontpath, int pt, FontStyle style)
		{
			Initialize(fontpath, pt, style);
		}

		//For Built-in Font
		public CSixLaborsTextRenderer(Stream fontStream, int pt, FontStyle style)
		{
			Initialize(fontStream, pt, style);
		}

		protected void Initialize(string fontpath, int pt, FontStyle style)
		{
			this.pt = (pt * 1.3f);
			this.fontStyle = style;

			if (File.Exists(fontpath))
			{
				this.fontFamily = new FontCollection().Add(fontpath);
			}
			else if (SystemFonts.TryGet(fontpath, out this.fontFamily))
			{
				//システムフォント
			}
			else
			{
				throw new FileNotFoundException($"Font File Not Found.({fontpath})");
			}
			this.font = this.fontFamily.CreateFont(this.pt, this.fontStyle);
		}

		protected void Initialize(Stream fontStream, int pt, FontStyle style)
		{
			this.pt = (pt * 1.3f);
			this.fontStyle = style;

			this.fontFamily = new FontCollection().Add(fontStream);
			this.font = this.fontFamily.CreateFont(this.pt, this.fontStyle);
		}

		public Image<Rgba32> DrawText(string drawstr, CFontRenderer.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
		{
			if (string.IsNullOrEmpty(drawstr))
			{
				//nullか""だったら、1x1を返す
				return new Image<Rgba32>(1, 1);
			}

			TextOptions toption = new TextOptions(this.font);
			FontRectangle size = TextMeasurer.Measure(drawstr, toption);

			//ちょっと大きめにとる
			//(後で定数じゃなくても済む方法を考えましょう。)
			Image<Rgba32> image = new Image<Rgba32>((int)(size.Width - size.X) + 50, (int)(size.Height - size.Y) + 50);

			//変換(ごり押し)
			SixLabors.ImageSharp.Color fontColorL = SixLabors.ImageSharp.Color.FromRgba(fontColor.R, fontColor.G, fontColor.B, fontColor.A);
			SixLabors.ImageSharp.Color edgeColorL = SixLabors.ImageSharp.Color.FromRgba(edgeColor.R, edgeColor.G, edgeColor.B, edgeColor.A);
			SixLabors.ImageSharp.Color gradationTopColorL = SixLabors.ImageSharp.Color.FromRgba(gradationTopColor.R, gradationTopColor.G, gradationTopColor.B, gradationTopColor.A);
			SixLabors.ImageSharp.Color gradationBottomColorL = SixLabors.ImageSharp.Color.FromRgba(gradationBottomColor.R, gradationBottomColor.G, gradationBottomColor.B, gradationBottomColor.A);


			IBrush brush;
			if (drawmode.HasFlag(CFontRenderer.DrawMode.Gradation))
			{
				brush = new LinearGradientBrush(new PointF(0, size.Top), new PointF(0, size.Height), GradientRepetitionMode.None, new ColorStop(0, gradationTopColorL), new ColorStop(1, gradationBottomColorL));
			}
			else
			{
				brush = new SolidBrush(fontColorL);
			}

			//あらかじめ背景色を取っておく
			SixLabors.ImageSharp.Color back = (SixLabors.ImageSharp.Color)image[0, 0].ToVector4();

			if (drawmode.HasFlag(CFontRenderer.DrawMode.Edge))
            {
				toption.Origin = new System.Numerics.Vector2(25, 25);
                DrawingOptions doption = new DrawingOptions();
				IPathCollection pathc = TextBuilder.GenerateGlyphs(drawstr, toption);
				image.Mutate(ctx => ctx.Draw(doption, new Pen(edgeColorL, this.pt * 8 / edge_Ratio), pathc));

				//どちらを使いましょう？
				//image.Mutate(ctx => ctx.DrawText(drawstr, this.font, brush, new PointF(10, 10)));
				image.Mutate(ctx => ctx.Fill(doption, brush, pathc));
			}
			else
			{
				image.Mutate(ctx => ctx.DrawText(drawstr, this.font, brush, PointF.Empty));
			}

			Rectangle rect = CCommon.MeasureForegroundArea(image, back);

			//無だった場合は、スペースと判断する(縦書きレンダリングに転用したいがための愚策)
			if (rect != Rectangle.Empty)
				image.Mutate(ctx => ctx.Crop(rect));
			else 
			{
				image.Dispose();
				return new Image<Rgba32>((int)this.pt, 1);
			}

			return image;
		}

		public void Dispose()
		{
		}

		private float pt = 12;
		private FontStyle fontStyle;
		private FontFamily fontFamily;
		private Font font;
	}
}
