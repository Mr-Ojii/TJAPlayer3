using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace FDK
{
	public class CTexture : IDisposable
	{
		// プロパティ
		public EBlendMode eBlendMode = EBlendMode.Normal;

		public float fRotation
		{
			get;
			set;
		}
		public int Opacity
		{
			get
			{
				return this._opacity;
			}
			set
			{
				if (value < 0)
				{
					this._opacity = 0;
				}
				else if (value > 0xff)
				{
					this._opacity = 0xff;
				}
				else
				{
					this._opacity = value;
				}
			}
		}
		public Size szTextureSize
		{
			get;
			private set;
		}
		private int? texture;
		private int? vrtVBO;
		private int? texVBO;
		private Vector3[] vertices = new Vector3[4]{ new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
		private Vector2[] texcoord = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
		public System.Numerics.Vector3 vcScaling;
		private Vector3 vc;
		private string filename;

		// コンストラクタ

		public CTexture()
		{
			this.szTextureSize = new Size(0, 0);
			this._opacity = 0xff;
			this.texture = null;
			this.vrtVBO = null;
			this.texVBO = null;
			this.bTextureDisposed = true;
			this.fRotation = 0f;
			this.vcScaling = new System.Numerics.Vector3(1f, 1f, 1f);
			this.vc = new Vector3(1f, 1f, 1f);
			this.filename = "";
			//			this._txData = null;
		}

		/// <summary>
		/// <para>指定された画像ファイルから Managed テクスチャを作成する。</para>
		/// <para>利用可能な画像形式は、BMP, JPG, PNG, TGA, DDS, PPM, DIB, HDR, PFM のいずれか。</para>
		/// </summary>
		/// <param name="device">Direct3D9 デバイス。</param>
		/// <param name="strFilename">画像ファイル名。</param>
		/// <exception cref="CTextureCreateFailedException">テクスチャの作成に失敗しました。</exception>
		public CTexture(Device device, string strFilename)
			: this()
		{
			maketype = MakeType.filename;
			filename = strFilename;
			MakeTexture(device, strFilename);
		}
		public CTexture(Device device, Bitmap bitmap, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.bitmap;
			MakeTexture(device, ToImageSharpImage(bitmap), b黒を透過する);
		}
		public CTexture(Device device, Image<Argb32> image, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.bitmap;
			MakeTexture(device, image, b黒を透過する);
		}

		public void MakeTexture(Device device, string strFilename)
		{
			if (!File.Exists(strFilename))     // #27122 2012.1.13 from: ImageInformation では FileNotFound 例外は返ってこないので、ここで自分でチェックする。わかりやすいログのために。
				throw new FileNotFoundException(string.Format("File does not exist. \n[{0}]", strFilename));

			MakeTexture(device, SixLabors.ImageSharp.Image.Load<Argb32>(strFilename), false);
		}
		public void MakeTexture(Device device, SixLabors.ImageSharp.Image<Argb32> bitmap, bool b黒を透過する)
		{
			bitmap.Mutate(c => c.Flip(FlipMode.Vertical));
			if (b黒を透過する)
				bitmap.Mutate(c => c.BackgroundColor(SixLabors.ImageSharp.Color.Transparent));
			try
			{
				this.szTextureSize = new Size(bitmap.Width, bitmap.Height);
				this.rcImageRect = new Rectangle(0, 0, this.szTextureSize.Width, this.szTextureSize.Height);

				//VBOをここで生成する
				this.vrtVBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
				GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.DynamicDraw);
				this.texVBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
				GL.BufferData(BufferTarget.ArrayBuffer, this.texcoord.Length * Vector2.SizeInBytes, texcoord, BufferUsageHint.DynamicDraw);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

				this.texture = GL.GenTexture();

				//テクスチャ用バッファのひもづけ
				GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);

				//テクスチャの設定
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 3);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, 0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, 3);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenSgis);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                unsafe
				{
					var data = new byte[bitmap.Width * bitmap.Height * 4];
					for (var x = 0; x < bitmap.Width; x++)
					{
						for (var y = 0; y < bitmap.Height; y++)
						{
							data[((y * bitmap.Width) + x) * 4 + 0] = bitmap[x, y].R;
							data[((y * bitmap.Width) + x) * 4 + 1] = bitmap[x, y].G;
							data[((y * bitmap.Width) + x) * 4 + 2] = bitmap[x, y].B;
							data[((y * bitmap.Width) + x) * 4 + 3] = bitmap[x, y].A;
						}
					}
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
				}

				GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

				this.bTextureDisposed = false;
			}
			catch
			{
				this.Dispose();
				throw new CTextureCreateFailedException(string.Format("Failed to create texture. \n"));
			}
		}
		// メソッド
		public void UpdateTexture(Bitmap bitmap, bool b黒を透過する) 
		{
			if (b黒を透過する)
				bitmap.MakeTransparent(Color.Black);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			if (this.szTextureSize == bitmap.Size) 
			{
				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, bitmap.Width, bitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)data.Scan0);
				bitmap.UnlockBits(data);

				GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}
		}

		//参考:https://gist.github.com/vurdalakov/00d9471356da94454b372843067af24e
		public static Image<Argb32> ToImageSharpImage(System.Drawing.Bitmap bitmap)
		{
			using (var memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

				memoryStream.Seek(0, SeekOrigin.Begin);

				return SixLabors.ImageSharp.Image.Load<Argb32>(memoryStream);
			}
		}

		public void t2D描画(Device device, RefPnt refpnt, float x, float y) 
		{
			this.t2D描画(device, refpnt, x, y, rcImageRect);
		}
		public void t2D描画(Device device, RefPnt refpnt, float x, float y, Rectangle rect)
		{
			this.t2D描画(device, refpnt, x, y, 1f, rect);
		}
		public void t2D描画(Device device, RefPnt refpnt, float x, float y, float depth, Rectangle rect)
		{
			switch (refpnt)
			{
				case RefPnt.UpLeft:
					this.t2D描画(device, x, y, depth, rect);
					break;
				case RefPnt.Up:
					this.t2D描画(device, x - (rect.Width / 2), y, depth, rect);
					break;
				case RefPnt.UpRight:
					this.t2D描画(device, x - rect.Width, y, depth, rect);
					break;
				case RefPnt.Left:
					this.t2D描画(device, x, y - (rect.Height / 2), depth, rect);
					break;
				case RefPnt.Center:
					this.t2D描画(device, x - (rect.Width / 2), y - (rect.Height / 2), depth, rect);
					break;
				case RefPnt.Right:
					this.t2D描画(device, x - rect.Width, y - (rect.Height / 2), depth, rect);
					break;
				case RefPnt.DownLeft:
					this.t2D描画(device, x, y - rect.Height, depth, rect);
					break;
				case RefPnt.Down:
					this.t2D描画(device, x - (rect.Width / 2), y - rect.Height, depth, rect);
					break;
				case RefPnt.DownRight:
					this.t2D描画(device, x - rect.Width, y - rect.Height, depth, rect);
					break;
				default:
					break;
			}
		}

		public void t2D拡大率考慮描画(Device device, RefPnt refpnt, float x, float y)
		{
			this.t2D拡大率考慮描画(device, refpnt, x, y, rcImageRect);
		}
		public void t2D拡大率考慮描画(Device device, RefPnt refpnt, float x, float y, Rectangle rect)
		{
			this.t2D拡大率考慮描画(device, refpnt, x, y, 1f, rect);
		}
		public void t2D拡大率考慮描画(Device device, RefPnt refpnt, float x, float y, float depth, Rectangle rect)
		{
			switch (refpnt)
			{
				case RefPnt.UpLeft:
					this.t2D描画(device, x, y, depth, rect);
					break;
				case RefPnt.Up:
					this.t2D描画(device, x - (rect.Width / 2 * this.vcScaling.X), y, depth, rect);
					break;
				case RefPnt.UpRight:
					this.t2D描画(device, x - rect.Width * this.vcScaling.X, y, depth, rect);
					break;
				case RefPnt.Left:
					this.t2D描画(device, x, y - (rect.Height / 2 * this.vcScaling.Y), depth, rect);
					break;
				case RefPnt.Center:
					this.t2D描画(device, x - (rect.Width / 2 * this.vcScaling.X), y - (rect.Height / 2 * this.vcScaling.Y), depth, rect);
					break;
				case RefPnt.Right:
					this.t2D描画(device, x - rect.Width * this.vcScaling.X, y - (rect.Height / 2 * this.vcScaling.Y), depth, rect);
					break;
				case RefPnt.DownLeft:
					this.t2D描画(device, x, y - rect.Height * this.vcScaling.Y, depth, rect);
					break;
				case RefPnt.Down:
					this.t2D描画(device, x - (rect.Width / 2 * this.vcScaling.X), y - rect.Height * this.vcScaling.Y, depth, rect);
					break;
				case RefPnt.DownRight:
					this.t2D描画(device, x - rect.Width * this.vcScaling.X, y - rect.Height * this.vcScaling.Y, depth, rect);
					break;
				default:
					break;
			}
		}
		public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y)
		{
			this.t2D元サイズ基準描画(device, refpnt, x, y, rcImageRect);
		}
		public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y, Rectangle rect)
		{
			this.t2D元サイズ基準描画(device, refpnt, x, y, 1f, rect);
		}
		public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y, float depth, Rectangle rect)
		{
			switch (refpnt)
			{
				case RefPnt.UpLeft:
					this.t2D描画(device, x, y, depth, rect);
					break;
				case RefPnt.Up:
					this.t2D描画(device, x - (szTextureSize.Width / 2), y, depth, rect);
					break;
				case RefPnt.UpRight:
					this.t2D描画(device, x - szTextureSize.Width, y, depth, rect);
					break;
				case RefPnt.Left:
					this.t2D描画(device, x, y - (szTextureSize.Height / 2), depth, rect);
					break;
				case RefPnt.Center:
					this.t2D描画(device, x - (szTextureSize.Width / 2), y - (szTextureSize.Height / 2), depth, rect);
					break;
				case RefPnt.Right:
					this.t2D描画(device, x - szTextureSize.Width, y - (szTextureSize.Height / 2), depth, rect);
					break;
				case RefPnt.DownLeft:
					this.t2D描画(device, x, y - szTextureSize.Height, depth, rect);
					break;
				case RefPnt.Down:
					this.t2D描画(device, x - (szTextureSize.Width / 2), y - szTextureSize.Height, depth, rect);
					break;
				case RefPnt.DownRight:
					this.t2D描画(device, x - szTextureSize.Width, y - szTextureSize.Height, depth, rect);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// テクスチャを 2D 画像と見なして描画する。
		/// </summary>
		/// <param name="device">Direct3D9 デバイス。</param>
		/// <param name="x">描画位置（テクスチャの左上位置の X 座標[dot]）。</param>
		/// <param name="y">描画位置（テクスチャの左上位置の Y 座標[dot]）。</param>
		public void t2D描画(Device device, float x, float y)
		{
			this.t2D描画(device, x, y, 1f, this.rcImageRect);
		}
		public void t2D描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				return;

			this.tSetBlendMode(device);

			if (this.fRotation == 0f)
			{
				#region [ (A) 回転なし ]
				//-----------------
				float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
				float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
				float w = rc画像内の描画領域.Width;
				float h = rc画像内の描画領域.Height;
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width);
				float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
				float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

				ResetWorldMatrix();

				GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
				GL.Color4(this.color);

				vertices[0].X = -(x + (w * this.vcScaling.X) + f補正値X);
				vertices[0].Y = -(y + f補正値Y);
				vertices[1].X = -(x + f補正値X);
				vertices[1].Y = -(y + f補正値Y);
				vertices[2].X = -(x + f補正値X);
				vertices[2].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);
				vertices[3].X = -(x + (w * this.vcScaling.X) + f補正値X);
				vertices[3].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);

				texcoord[0].X = f右U値;
				texcoord[0].Y = f上V値;
				texcoord[1].X = f左U値;
				texcoord[1].Y = f上V値;
				texcoord[2].X = f左U値;
				texcoord[2].Y = f下V値;
				texcoord[3].X = f右U値;
				texcoord[3].Y = f下V値;

				GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

				GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
				//-----------------
				#endregion
			}
			else
			{
				#region [ (B) 回転あり ]
				//-----------------
				float f中央X = ((float)rc画像内の描画領域.Width) / 2f;
				float f中央Y = ((float)rc画像内の描画領域.Height) / 2f;
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width);
				float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
				float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

				float n描画領域内X = x + (rc画像内の描画領域.Width / 2.0f);
				float n描画領域内Y = y + (rc画像内の描画領域.Height / 2.0f);
				var vc3移動量 = new Vector3(n描画領域内X - (((float)GameWindowSize.Width) / 2f), -(n描画領域内Y - (((float)GameWindowSize.Height) / 2f)), 0f);

				this.vc.X = this.vcScaling.X;
				this.vc.Y = this.vcScaling.Y;
				this.vc.Z = this.vcScaling.Z;

				var matrix = Matrix4.Identity * Matrix4.CreateScale(this.vc);
				matrix *= Matrix4.CreateRotationZ(this.fRotation);
				matrix *= Matrix4.CreateTranslation(vc3移動量);

				LoadWorldMatrix(matrix);

				GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
				GL.Color4(this.color);

				vertices[0].X = -f中央X;
				vertices[0].Y = -f中央Y;
				vertices[1].X = f中央X;
				vertices[1].Y = -f中央Y;
				vertices[2].X = f中央X;
				vertices[2].Y = f中央Y;
				vertices[3].X = -f中央X;
				vertices[3].Y = f中央Y;

				texcoord[0].X = f左U値;
				texcoord[0].Y = f下V値;
				texcoord[1].X = f右U値;
				texcoord[1].Y = f下V値;
				texcoord[2].X = f右U値;
				texcoord[2].Y = f上V値;
				texcoord[3].X = f左U値;
				texcoord[3].Y = f上V値;

				GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

				GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
				//-----------------
				#endregion
			}
		}


		public void t2D幕用描画(Device device, float x, float y, Rectangle rc画像内の描画領域, bool left, int num = 0) 
		{
			if (this.texture == null)
				return;

			this.tSetBlendMode(device);

			#region [ (A) 回転なし ]
			//-----------------
			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width);
			float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
			float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vcScaling.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X) - ((!left) ? num : 0);
			vertices[2].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vcScaling.X) + f補正値X) + ((left) ? num : 0);
			vertices[3].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);

			texcoord[0].X = f右U値;
			texcoord[0].Y = f上V値;
			texcoord[1].X = f左U値;
			texcoord[1].Y = f上V値;
			texcoord[2].X = f左U値;
			texcoord[2].Y = f下V値;
			texcoord[3].X = f右U値;
			texcoord[3].Y = f下V値;

			GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
			//-----------------
			#endregion
		}

		public void t2D上下反転描画(Device device, float x, float y)
		{
			this.t2D上下反転描画(device, x, y, 1f, this.rcImageRect);
		}
		public void t2D上下反転描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D上下反転描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D上下反転描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				throw new InvalidOperationException("Texture is not generated. ");

			this.tSetBlendMode(device);

			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width);
			float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
			float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vcScaling.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X);
			vertices[2].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vcScaling.X) + f補正値X);
			vertices[3].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);

			texcoord[0].X = f右U値;
			texcoord[0].Y = f下V値;
			texcoord[1].X = f左U値;
			texcoord[1].Y = f下V値;
			texcoord[2].X = f左U値;
			texcoord[2].Y = f上V値;
			texcoord[3].X = f右U値;
			texcoord[3].Y = f上V値;

			GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		public void t2D左右反転描画(Device device, float x, float y)
		{
			this.t2D左右反転描画(device, x, y, 1f, this.rcImageRect);
		}
		public void t2D左右反転描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D左右反転描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D左右反転描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				throw new InvalidOperationException("Texture is not generated. ");

			this.tSetBlendMode(device);

			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width);
			float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
			float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vcScaling.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X);
			vertices[2].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vcScaling.X) + f補正値X);
			vertices[3].Y = -((y + (h * this.vcScaling.Y)) + f補正値Y);

			texcoord[0].X = f左U値;
			texcoord[0].Y = f上V値;
			texcoord[1].X = f右U値;
			texcoord[1].Y = f上V値;
			texcoord[2].X = f右U値;
			texcoord[2].Y = f下V値;
			texcoord[3].X = f左U値;
			texcoord[3].Y = f下V値;

			GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		/// <summary>
		/// テクスチャを 3D 画像と見なして描画する。
		/// </summary>
		public void t3D描画(Device device, System.Numerics.Matrix4x4 mat)
		{
			this.t3D描画(device, mat, this.rcImageRect);
		}
		public void t3D描画(Device device, System.Numerics.Matrix4x4 mat, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				return;

			matrix.M11 = mat.M11;
			matrix.M12 = mat.M12;
			matrix.M13 = mat.M13;
			matrix.M14 = mat.M14;
			matrix.M21 = mat.M21;
			matrix.M22 = mat.M22;
			matrix.M23 = mat.M23;
			matrix.M24 = mat.M24;
			matrix.M31 = mat.M31;
			matrix.M32 = mat.M32;
			matrix.M33 = mat.M33;
			matrix.M34 = mat.M34;
			matrix.M41 = mat.M41;
			matrix.M42 = mat.M42;
			matrix.M43 = mat.M43;
			matrix.M44 = mat.M44;

			float x = ((float)rc画像内の描画領域.Width) / 2f;
			float y = ((float)rc画像内の描画領域.Height) / 2f;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szTextureSize.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szTextureSize.Width); 
			float f上V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Top)) / ((float)this.szTextureSize.Height);
			float f下V値 = ((float)(rcImageRect.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szTextureSize.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			this.tSetBlendMode(device);

			LoadWorldMatrix(matrix);

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = x;
			vertices[0].Y = y;
			vertices[1].X = -x;
			vertices[1].Y = y;
			vertices[2].X = -x;
			vertices[2].Y = -y;
			vertices[3].X = x;
			vertices[3].Y = -y;

			texcoord[0].X = f右U値;
			texcoord[0].Y = f上V値;
			texcoord[1].X = f左U値;
			texcoord[1].Y = f上V値;
			texcoord[2].X = f左U値;
			texcoord[2].Y = f下V値;
			texcoord[3].X = f右U値;
			texcoord[3].Y = f下V値;

			GL.VertexPointer(3, VertexPointerType.Float, 0, this.vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, this.texcoord);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDisposed)
			{
				// テクスチャの破棄
				if (this.texture.HasValue)
				{
					this.bTextureDisposed = true;
					GL.DeleteTexture((int)this.texture);
					this.texture = null;
				}
				if (this.vrtVBO.HasValue) 
				{
					GL.DeleteBuffer((int)this.vrtVBO);
					this.vrtVBO = null;
				}
				if (this.texVBO.HasValue)
				{
					GL.DeleteBuffer((int)this.texVBO);
					this.texVBO = null;
				}

				this.bDisposed = true;
			}
		}
		~CTexture()
		{
			// ファイナライザの動作時にtextureのDisposeがされていない場合は、
			// CTextureのDispose漏れと見做して警告をログ出力する
			if (!this.bTextureDisposed)//DTXManiaより
			{
				Trace.TraceWarning("CTexture: Texture memory leak detected.(Size=({0}, {1}), filename={2}, maketype={3})", szTextureSize.Width, szTextureSize.Height, filename, maketype.ToString());
				this.Dispose();//Disposeしておく
			}
		}
		//-----------------
		#endregion

		// その他

		public enum RefPnt
		{
			UpLeft,
			Up,
			UpRight,
			Left,
			Center,
			Right,
			DownLeft,
			Down,
			DownRight,
		}

		public enum EBlendMode 
		{
			Normal,
			Addition,
			Subtract,
			Multiply,
			Screen
		}

		#region [ private ]
		//-----------------
		private int _opacity;
		private bool bDisposed, bTextureDisposed;

		/// <summary>
		/// どれか一つが有効になります。
		/// </summary>
		/// <param name="device">Direct3Dのデバイス</param>
		private void tSetBlendMode(Device device)
		{
			switch (this.eBlendMode) 
			{
				case EBlendMode.Addition:
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					break;
				case EBlendMode.Multiply:
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.SrcColor);
					break;
				case EBlendMode.Subtract:
					GL.BlendEquation(BlendEquationMode.FuncReverseSubtract);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					break;
				case EBlendMode.Screen:
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					GL.BlendFunc(BlendingFactor.OneMinusDstColor, BlendingFactor.One);
					break;
				default:
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
					break;
			}
		}
		private void ResetWorldMatrix()
		{
			Matrix4 tmpmat = CAction.ModelView;
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref tmpmat);
		}

		private void LoadWorldMatrix(Matrix4 mat)
		{
			Matrix4 tmpmat = CAction.ModelView;
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref tmpmat);
			mat *= Matrix4.CreateScale(-1, 1, 0);
			GL.MultMatrix(ref mat);
		}
		private enum MakeType
		{
			filename,
			bytearray,
			bitmap
		}

		// 2012.3.21 さらなる new の省略作戦

		protected Rectangle rcImageRect;                              // テクスチャ作ったらあとは不変
		public Color color = Color.FromArgb(255, 255, 255, 255);
		private Matrix4 matrix = Matrix4.Identity;
		private MakeType maketype = MakeType.bytearray;
		//-----------------
		#endregion
	}
}
