using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Processing;

using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace FDK
{
	internal class CGDIPlusTextRenderer : ITextRenderer
	{
		public CGDIPlusTextRenderer(string fontpath, int pt)
		{
			Initialize(fontpath, pt, SixLabors.Fonts.FontStyle.Regular);
		}

		public CGDIPlusTextRenderer(string fontpath, int pt, SixLabors.Fonts.FontStyle style)
		{
			Initialize(fontpath, pt, style);
		}

		protected void Initialize(string fontpath, int pt, SixLabors.Fonts.FontStyle stylel)
		{
			this._pfc = null;
			this._fontfamily = null;
			this._font = null;
			this._pt = pt;
			this.bDisposed = false;

			FontStyle style;
			switch (stylel) 
			{
				case SixLabors.Fonts.FontStyle.Bold:
					style = FontStyle.Bold;
					break;
				case SixLabors.Fonts.FontStyle.BoldItalic:
					style = FontStyle.Bold | FontStyle.Italic;
					break;
				case SixLabors.Fonts.FontStyle.Italic:
					style = FontStyle.Italic;
					break;
				default:
					style = FontStyle.Regular;
					break;
			}

			try
			{
				this._fontfamily = new FontFamily(fontpath);
			}
			catch 
			{
				Trace.TraceWarning($"{fontpath}はフォント名ではないようです。");
				this._fontfamily = null;
			}

			if (this._fontfamily == null)
			{
				try
				{
					this._pfc = new System.Drawing.Text.PrivateFontCollection();	//PrivateFontCollectionオブジェクトを作成する
					this._pfc.AddFontFile(fontpath);								//PrivateFontCollectionにフォントを追加する
					_fontfamily = _pfc.Families[0];
				}
				catch (System.IO.FileNotFoundException)
				{
					Trace.TraceWarning($"プライベートフォントの追加に失敗しました({fontpath})。代わりに内蔵フォントの使用を試みます。");
					//throw new FileNotFoundException( "プライベートフォントの追加に失敗しました。({0})", Path.GetFileName( fontpath ) );
					//return;
					this._fontfamily = null;
				}
			}

			// 指定されたフォントスタイルが適用できない場合は、フォント内で定義されているスタイルから候補を選んで使用する
			// 何もスタイルが使えないようなフォントなら、例外を出す。
			if (_fontfamily != null)
			{
				if (!_fontfamily.IsStyleAvailable(style))
				{
					FontStyle[] FS = { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Underline, FontStyle.Strikeout };
					style = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;	// null非許容型なので、代わりに全盛をNGワードに設定
					foreach (FontStyle ff in FS)
					{
						if (this._fontfamily.IsStyleAvailable(ff))
						{
							style = ff;
							Trace.TraceWarning("フォント{0}へのスタイル指定を、{1}に変更しました。", Path.GetFileName(fontpath), style.ToString());
							break;
						}
					}
					if (style == (FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout))
					{
						Trace.TraceWarning("フォント{0}は適切なスタイル{1}を選択できませんでした。", Path.GetFileName(fontpath), style.ToString());
					}
				}
				//this._font = new Font(this._fontfamily, pt, style);			//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
				float emSize = pt * 96.0f / 72.0f;
				this._font = new Font(this._fontfamily, emSize, style, GraphicsUnit.Pixel);	//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
				//HighDPI対応のため、pxサイズで指定
			}
			else
			// フォントファイルが見つからなかった場合
			{
				throw new FileNotFoundException($"プライベートフォントの追加に失敗しました。({Path.GetFileName(fontpath)})");
			}
		}

		public SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> DrawText(string drawstr, CFontRenderer.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
		{
			if (this._fontfamily == null || string.IsNullOrEmpty(drawstr))
			{
				// nullを返すと、その後bmp→texture処理や、textureのサイズを見て__の処理で全部例外が発生することになる。
				// それは非常に面倒なので、最小限のbitmapを返してしまう。
				// まずはこの仕様で進めますが、問題有れば(上位側からエラー検出が必要であれば)例外を出したりエラー状態であるプロパティを定義するなり検討します。
				if (drawstr != "")
				{
					Trace.TraceWarning("DrawText()の入力不正。最小値のbitmapを返します。");
				}
				return new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1);
			}

			// 描画サイズを測定する
			Size stringSize;
			using (Bitmap bmptmp = new Bitmap(1, 1))
			{
				using (Graphics gtmp = Graphics.FromImage(bmptmp))
				{
					gtmp.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					using (StringFormat sf = new StringFormat()
					{
						LineAlignment = StringAlignment.Far, // 画面下部（垂直方向位置）
						Alignment = StringAlignment.Center,  // 画面中央（水平方向位置）     
						FormatFlags = StringFormatFlags.NoWrap, // どんなに長くて単語の区切りが良くても改行しない (AioiLight)
						Trimming = StringTrimming.None, // どんなに長くてもトリミングしない (AioiLight)	
					})
					{
						//float to int
						SizeF fstringSize = gtmp.MeasureString(drawstr, this._font, new PointF(0, 0), sf);
						stringSize = new Size((int)fstringSize.Width, (int)fstringSize.Height);
					}
				}
			}

			bool bEdge = ((drawmode & CFontRenderer.DrawMode.Edge) == CFontRenderer.DrawMode.Edge);
			bool bGradation = ((drawmode & CFontRenderer.DrawMode.Gradation) == CFontRenderer.DrawMode.Gradation);

			// 縁取りの縁のサイズは、とりあえずフォントの大きさの(1/SkinConfig)とする
			int nEdgePt = (bEdge) ? (10 * _pt / edge_Ratio) : 0; //SkinConfigにて設定可能に(rhimm)

			//取得した描画サイズを基に、描画先のbitmapを作成する
			Bitmap bmp = new Bitmap(stringSize.Width + nEdgePt * 2 + 20, stringSize.Height + nEdgePt * 2 + 20);
			bmp.MakeTransparent();

			SixLabors.ImageSharp.Color backColor = new SixLabors.ImageSharp.Color(new SixLabors.ImageSharp.PixelFormats.Rgba32(bmp.GetPixel(0, 0).R, bmp.GetPixel(0, 0).G, bmp.GetPixel(0, 0).B, bmp.GetPixel(0, 0).A));
				
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

				// レイアウト枠
				Rectangle r = new Rectangle(0, 0, stringSize.Width + nEdgePt * 2 + 10, stringSize.Height + nEdgePt * 2 + 10);

				if (bEdge)    // 縁取り有りの描画
				{
					using (StringFormat sf = new StringFormat()
					{
						LineAlignment = StringAlignment.Far, // 画面下部（垂直方向位置）
						Alignment = StringAlignment.Center,  // 画面中央（水平方向位置）     
						FormatFlags = StringFormatFlags.NoWrap, // どんなに長くて単語の区切りが良くても改行しない (AioiLight)
						Trimming = StringTrimming.None, // どんなに長くてもトリミングしない (AioiLight)	
					}) 
					{
						// DrawPathで、ポイントサイズを使って描画するために、DPIを使って単位変換する
						// (これをしないと、単位が違うために、小さめに描画されてしまう)
						float sizeInPixels = _font.SizeInPoints * g.DpiY / 72;  // 1 inch = 72 points

						GraphicsPath gp = new GraphicsPath();
						gp.AddString(drawstr, this._fontfamily, (int)this._font.Style, sizeInPixels, r, sf);

						// 縁取りを描画する
						Pen p = new Pen(edgeColor, nEdgePt);
						p.LineJoin = LineJoin.Round;
						g.DrawPath(p, gp);

						// 塗りつぶす
						Brush br;
						if (bGradation)
						{
							br = new LinearGradientBrush(r, gradationTopColor, gradationBottomColor, LinearGradientMode.Vertical);
						}
						else
						{
							br = new SolidBrush(fontColor);
						}
						g.FillPath(br, gp);

						if (br != null) br.Dispose(); br = null;
						if (p != null) p.Dispose(); p = null;
						if (gp != null) gp.Dispose(); gp = null;
					}

				}
				else
				{
					// 縁取りなしの描画
					g.DrawString(drawstr, _font, new SolidBrush(fontColor), new PointF(0, 0));
				}
			}

			SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = CConvert.ToImageSharpImage(bmp);
			bmp.Dispose();
			SixLabors.ImageSharp.Rectangle rect = CCommon.MeasureForegroundArea(image, backColor);
			
			if (rect != SixLabors.ImageSharp.Rectangle.Empty)
				image.Mutate(ctx => ctx.Crop(rect));
			else
			{
				image.Dispose();
				return new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>((int)stringSize.Width, 1);
			}

			return image;
		}


		public void Dispose()
		{
			if (!this.bDisposed)
			{
				if (this._font != null)
				{
					this._font.Dispose();
					this._font = null;
				}
				if (this._pfc != null)
				{
					this._pfc.Dispose();
					this._pfc = null;
				}

				this.bDisposed = true;
			}
		}

		private int _pt = 12;
		private FontFamily _fontfamily;
		private System.Drawing.Text.PrivateFontCollection _pfc;
		private Font _font;
		private bool bDisposed;
	}
}
