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
	public class CSixLaborsTextRenderer : ITextRenderer
	{
		public CSixLaborsTextRenderer(string fontpath, int pt)
		{
			Initialize(fontpath, pt, FontStyle.Regular);
		}

		public CSixLaborsTextRenderer(string fontpath, int pt, FontStyle style)
		{
			Initialize(fontpath, pt, style);
		}

		protected void Initialize(string fontpath, int pt, FontStyle style)
		{

			this.pt = (pt * 1.3f);
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

		public Image<Rgba32> DrawText(string drawstr, CPrivateFont.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
		{
			if (string.IsNullOrEmpty(drawstr))
			{
				//nullか""だったら、1x1を返す
				return new Image<Rgba32>(1, 1);
			}

			RendererOptions roption = new RendererOptions(this.font);
			FontRectangle size = TextMeasurer.Measure(drawstr, roption);

			//ちょっと大きめにとる
			//(後で定数じゃなくても済む方法を考えましょう。)
			Image<Rgba32> image = new Image<Rgba32>((int)(size.Width - size.X) + 50, (int)(size.Height - size.Y) + 50);

			//変換(ごり押し)
			SixLabors.ImageSharp.Color fontColorL = SixLabors.ImageSharp.Color.FromRgba(fontColor.R, fontColor.G, fontColor.B, fontColor.A);
			SixLabors.ImageSharp.Color edgeColorL = SixLabors.ImageSharp.Color.FromRgba(edgeColor.R, edgeColor.G, edgeColor.B, edgeColor.A);
			SixLabors.ImageSharp.Color gradationTopColorL = SixLabors.ImageSharp.Color.FromRgba(gradationTopColor.R, gradationTopColor.G, gradationTopColor.B, gradationTopColor.A);
			SixLabors.ImageSharp.Color gradationBottomColorL = SixLabors.ImageSharp.Color.FromRgba(gradationBottomColor.R, gradationBottomColor.G, gradationBottomColor.B, gradationBottomColor.A);


			IBrush brush;
			if (drawmode.HasFlag(CPrivateFont.DrawMode.Gradation))
			{
				brush = new LinearGradientBrush(new PointF(0, size.Top), new PointF(0, size.Height), GradientRepetitionMode.None, new ColorStop(0, gradationTopColorL), new ColorStop(1, gradationBottomColorL));
			}
			else
			{
				brush = new SolidBrush(fontColorL);
			}

			//あらかじめ背景色を取っておく
			SixLabors.ImageSharp.Color back = (SixLabors.ImageSharp.Color)image[0, 0].ToVector4();

			if (drawmode.HasFlag(CPrivateFont.DrawMode.Edge))
			{
				DrawingOptions doption = new DrawingOptions();
				IPathCollection pathc = TextBuilder.GenerateGlyphs(drawstr, new PointF(10, 10), roption);
				image.Mutate(ctx => ctx.Draw(doption, new Pen(edgeColorL, this.pt * 8 / edge_Ratio), pathc));

				//どちらを使いましょう？
				//image.Mutate(ctx => ctx.DrawText(drawstr, this.font, brush, new PointF(10, 10)));
				image.Mutate(ctx => ctx.Fill(doption, brush, pathc));
			}
			else
			{
				image.Mutate(ctx => ctx.DrawText(drawstr, this.font, brush, PointF.Empty));
			}

			Rectangle rect = MeasureForegroundArea(image, back);

			//無だった場合は、スペースと判断する(縦書きレンダリングに転用したいがための愚策)
			if (rect != Rectangle.Empty)
				image.Mutate(ctx => ctx.Crop(rect));

			return image;
		}



		/// <summary>
		/// 指定されたImageで、backColor以外の色が使われている範囲を計測する
		/// </summary>
		private static Rectangle MeasureForegroundArea(Image<Rgba32> bmp, SixLabors.ImageSharp.Color backColor)
		{
			//元々のやつの動作がおかしかったので、書き直します。
			//2021-08-02 Mr-Ojii

			//左
			int leftPos = -1;
			for (int x = 0; x < bmp.Width; x++)
			{
				for (int y = 0; y < bmp.Height; y++)
				{
					//backColorではない色であった場合、位置を決定する
					if (bmp[x, y].ToVector4() != ((System.Numerics.Vector4)backColor))
					{
						leftPos = x;
						break;
					}
				}
				if (leftPos != -1)
				{
					break;
				}
			}
			//違う色が見つからなかった時
			if (leftPos == -1)
			{
				return Rectangle.Empty;
			}

			//右
			int rightPos = -1;
			for (int x = bmp.Width - 1; leftPos <= x; x--)
			{
				for (int y = 0; y < bmp.Height; y++)
				{
					if (bmp[x, y].ToVector4() != ((System.Numerics.Vector4)backColor))
					{
						rightPos = x;
						break;
					}
				}
				if (rightPos != -1)
				{
					break;
				}
			}
			if (rightPos == -1)
			{
				return Rectangle.Empty;
			}

			//上
			int topPos = -1;
			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					if (bmp[x, y].ToVector4() != ((System.Numerics.Vector4)backColor))
					{
						topPos = y;
						break;
					}
				}
				if (topPos != -1)
				{
					break;
				}
			}
			if (topPos == -1)
			{
				return Rectangle.Empty;
			}

			//下
			int bottomPos = -1;
			for (int y = bmp.Height - 1; topPos <= y; y--)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					if (bmp[x, y].ToVector4() != ((System.Numerics.Vector4)backColor))
					{
						bottomPos = y;
						break;
					}
				}
				if (bottomPos != -1)
				{
					break;
				}
			}
			if (bottomPos == -1)
			{
				return Rectangle.Empty;
			}

			//結果を返す
			return new Rectangle(leftPos, topPos, rightPos - leftPos + 1, bottomPos - topPos + 1);
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
