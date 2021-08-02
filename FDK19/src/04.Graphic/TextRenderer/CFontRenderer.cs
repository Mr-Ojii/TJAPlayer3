using System;
using System.Diagnostics;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

using Color = System.Drawing.Color;

namespace FDK
{

    public class CFontRenderer : IDisposable
    {
		#region[static系]
		public static void SetTextCorrectionX_Chara_List_Vertical(string[] list)
		{
			if (list != null)
				CorrectionX_Chara_List_Vertical = list.Where(c => c != null).ToArray();
		}
		public static void SetTextCorrectionX_Chara_List_Value_Vertical(int[] list)
		{
			if (list != null)
				CorrectionX_Chara_List_Value_Vertical = list;
		}
		public static void SetTextCorrectionY_Chara_List_Vertical(string[] list)
		{
			if (list != null)
				CorrectionY_Chara_List_Vertical = list.Where(c => c != null).ToArray();
		}
		public static void SetTextCorrectionY_Chara_List_Value_Vertical(int[] list)
		{
			if (list != null)
				CorrectionY_Chara_List_Value_Vertical = list;
		}
		public static void SetRotate_Chara_List_Vertical(string[] list)
		{
			if (list != null)
				Rotate_Chara_List_Vertical = list.Where(c => c != null).ToArray();
		}

		private static string[] CorrectionX_Chara_List_Vertical = new string[0];
		private static int[] CorrectionX_Chara_List_Value_Vertical = new int[0];
		private static string[] CorrectionY_Chara_List_Vertical = new string[0];
		private static int[] CorrectionY_Chara_List_Value_Vertical = new int[0];
		private static string[] Rotate_Chara_List_Vertical = new string[0];
		#endregion

        
        #region [ コンストラクタ ]
        public CFontRenderer(string fontpath, int pt, SixLabors.Fonts.FontStyle style)
        {
            Initialize(fontpath, pt, style);
        }
        public CFontRenderer(string fontpath, int pt)
        {
            Initialize(fontpath, pt, SixLabors.Fonts.FontStyle.Regular);
        }
        public CFontRenderer()
        {
            //throw new ArgumentException("CPrivateFont: 引数があるコンストラクタを使用してください。");
        }
        #endregion

        protected void Initialize(string fontpath, int pt, FontStyle style)
		{
			try
			{
				this.textRenderer = new CGDIPlusTextRenderer(fontpath, pt, style);
				return;
			}
			catch (Exception e)
			{
				Trace.TraceWarning("GDI+でのフォント生成に失敗しました。" + e.ToString());
				this.textRenderer.Dispose();
			}
			
			try
            {
                this.textRenderer = new CSixLaborsTextRenderer(fontpath, pt, style);
				return;
            }
            catch (Exception e)
            {
                Trace.TraceWarning("SixLabors.Fontsでのフォント生成に失敗しました。" + e.ToString());
				this.textRenderer.Dispose();
				throw;
            }
        }

		public Image<Rgba32> DrawPrivateFont(string drawstr, Color fontColor)
		{
			return DrawPrivateFont(drawstr, CPrivateFont.DrawMode.Normal, fontColor, Color.White, Color.White, Color.White, 0);
		}

		public Image<Rgba32> DrawPrivateFont(string drawstr, Color fontColor, Color edgeColor, int edge_Ratio)
		{
			return DrawPrivateFont(drawstr, CPrivateFont.DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White, edge_Ratio);
		}

		public Image<Rgba32> DrawPrivateFont(string drawstr, Color fontColor, Color gradationTopColor, Color gradataionBottomColor, int edge_Ratio)
		{
			return DrawPrivateFont(drawstr, CPrivateFont.DrawMode.Gradation, fontColor, Color.White, gradationTopColor, gradataionBottomColor, edge_Ratio);
		}

		public Image<Rgba32> DrawPrivateFont(string drawstr, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradataionBottomColor, int edge_Ratio)
		{
			return DrawPrivateFont(drawstr, CPrivateFont.DrawMode.Edge | CPrivateFont.DrawMode.Gradation, fontColor, edgeColor, gradationTopColor, gradataionBottomColor, edge_Ratio);
		}
		protected Image<Rgba32> DrawPrivateFont(string drawstr, CPrivateFont.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
		{
			//横書きに対してのCorrectionは廃止
            return this.textRenderer.DrawText(drawstr, drawmode, fontColor, edgeColor, gradationTopColor, gradationBottomColor, edge_Ratio);
		}


		public Image<Rgba32> DrawPrivateFont_V(string drawstr, Color fontColor)
		{
			return DrawPrivateFont_V(drawstr, CPrivateFont.DrawMode.Normal, fontColor, Color.White, Color.White, Color.White, 0);
		}

		public Image<Rgba32> DrawPrivateFont_V(string drawstr, Color fontColor, Color edgeColor, int edge_Ratio)
		{
			return DrawPrivateFont_V(drawstr, CPrivateFont.DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White, edge_Ratio);
		}

		public Image<Rgba32> DrawPrivateFont_V(string drawstr, Color fontColor, Color gradationTopColor, Color gradataionBottomColor, int edge_Ratio)
		{
			return DrawPrivateFont_V(drawstr, CPrivateFont.DrawMode.Gradation, fontColor, Color.White, gradationTopColor, gradataionBottomColor, edge_Ratio);
		}

		public Image<Rgba32> DrawPrivateFont_V(string drawstr, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradataionBottomColor, int edge_Ratio)
		{
			return DrawPrivateFont_V(drawstr, CPrivateFont.DrawMode.Edge | CPrivateFont.DrawMode.Gradation, fontColor, edgeColor, gradationTopColor, gradataionBottomColor, edge_Ratio);
		}
		protected Image<Rgba32> DrawPrivateFont_V(string drawstr, CPrivateFont.DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
		{
			if (string.IsNullOrEmpty(drawstr))
			{
				//nullか""だったら、1x1を返す
				return new Image<Rgba32>(1, 1);
			}

			//グラデ(全体)にも対応したいですね？

			string[] strList = new string[drawstr.Length];
			for (int i = 0; i < drawstr.Length; i++)
				strList[i] = drawstr.Substring(i, 1);
			Image<Rgba32>[] strImageList = new Image<Rgba32>[drawstr.Length];

			//レンダリング,大きさ計測
			int nWidth = 0;
			int nHeight = 0;
			for (int i = 0; i < strImageList.Length; i++)
			{
				strImageList[i] = this.textRenderer.DrawText(strList[i], drawmode, fontColor, edgeColor, gradationTopColor, gradationBottomColor, edge_Ratio);

				//回転する文字
				if(Rotate_Chara_List_Vertical.Contains(strList[i]))
					strImageList[i].Mutate(ctx => ctx.Rotate(RotateMode.Rotate90));

				nWidth = Math.Max(nWidth, strImageList[i].Width);
				nHeight += strImageList[i].Height;
			}

			Image<Rgba32> image = new Image<Rgba32>(nWidth, nHeight);

			//1文字ずつ描画したやつを全体キャンバスに描画していく
			int nowHeightPos = 0;
			for (int i = 0; i < strImageList.Length; i++)
			{
				image.Mutate(ctx => ctx.DrawImage(strImageList[i], new Point((nWidth - strImageList[i].Width) / 2, nowHeightPos), 1));
				nowHeightPos += strImageList[i].Height;
			}

			//1文字ずつ描画したやつの解放
			for (int i = 0; i < strImageList.Length; i++)
			{
				strImageList[i].Dispose();
			}

			//返します
			return image;
		}

        public void Dispose()
        {
            this.textRenderer.Dispose();
        }

        private ITextRenderer textRenderer;
    }
}