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

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace FDK
{
	public class CTexture : IDisposable
	{
		// プロパティ
		public bool b加算合成
		{
			get;
			set;
		}
		public bool b乗算合成
		{
			get;
			set;
		}
		public bool b減算合成
		{
			get;
			set;
		}
		public bool bスクリーン合成
		{
			get;
			set;
		}
		public float fZ軸中心回転
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
		public Size szテクスチャサイズ
		{
			get;
			private set;
		}
		private int? texture;
		private int? vrtVBO;
		private int? texVBO;
		private Vector3[] vertices = new Vector3[4]{ new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
		private Vector2[] texcoord = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
		public System.Numerics.Vector3 vc拡大縮小倍率;
		private Vector3 vc;
		public string filename;

		/// <summary>
		/// <para>論理画面を1とする場合の物理画面の倍率。</para>
		/// <para>論理値×画面比率＝物理値。</para>
		/// </summary>
		public static float f画面比率 = 1.0f;

		// コンストラクタ

		public CTexture()
		{
			this.szテクスチャサイズ = new Size(0, 0);
			this.szテクスチャサイズ = new Size(0, 0);
			this._opacity = 0xff;
			this.texture = null;
			this.vrtVBO = null;
			this.texVBO = null;
			this.bSharpDXTextureDispose完了済み = true;
			this.b加算合成 = false;
			this.fZ軸中心回転 = 0f;
			this.vc拡大縮小倍率 = new System.Numerics.Vector3(1f, 1f, 1f);
			this.vc = new Vector3(1f, 1f, 1f);
			this.filename = "";
			//			this._txData = null;
		}

		/// <summary>
		/// <para>指定された画像ファイルから Managed テクスチャを作成する。</para>
		/// <para>利用可能な画像形式は、BMP, JPG, PNG, TGA, DDS, PPM, DIB, HDR, PFM のいずれか。</para>
		/// </summary>
		/// <param name="device">Direct3D9 デバイス。</param>
		/// <param name="strファイル名">画像ファイル名。</param>
		/// <param name="format">テクスチャのフォーマット。</param>
		/// <param name="b黒を透過する">画像の黒（0xFFFFFFFF）を透過させるなら true。</param>
		/// <exception cref="CTextureCreateFailedException">テクスチャの作成に失敗しました。</exception>
		public CTexture(Device device, string strファイル名)
			: this()
		{
			maketype = MakeType.filename;
			MakeTexture(device, strファイル名);
		}
		public CTexture(Device device, Bitmap bitmap, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.bitmap;
			MakeTexture(device, bitmap, b黒を透過する);
		}

		public void MakeTexture(Device device, string strファイル名)
		{
			if (!File.Exists(strファイル名))     // #27122 2012.1.13 from: ImageInformation では FileNotFound 例外は返ってこないので、ここで自分でチェックする。わかりやすいログのために。
				throw new FileNotFoundException(string.Format("ファイルが存在しません。\n[{0}]", strファイル名));

			MakeTexture(device, new Bitmap(strファイル名), false);
		}
		public void MakeTexture(Device device, Bitmap bitmap, bool b黒を透過する)
		{
			if (b黒を透過する)
				bitmap.MakeTransparent(Color.Black);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			try
			{
				this.szテクスチャサイズ = new Size(bitmap.Width, bitmap.Height);
				this.rc全画像 = new Rectangle(0, 0, this.szテクスチャサイズ.Width, this.szテクスチャサイズ.Height);

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
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenSgis);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				//テクスチャ用バッファに色情報を流し込む
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

				bitmap.UnlockBits(data);

				this.bSharpDXTextureDispose完了済み = false;
			}
			catch
			{
				this.Dispose();
				throw new CTextureCreateFailedException(string.Format("テクスチャの生成に失敗しました。\n"));
			}
		}
		// メソッド

		public void t2D描画(Device device, RefPnt refpnt, float x, float y) 
		{
			this.t2D描画(device, refpnt, x, y, rc全画像);
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
			this.t2D拡大率考慮描画(device, refpnt, x, y, rc全画像);
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
					this.t2D描画(device, x - (rect.Width / 2 * this.vc拡大縮小倍率.X), y, depth, rect);
					break;
				case RefPnt.UpRight:
					this.t2D描画(device, x - rect.Width * this.vc拡大縮小倍率.X, y, depth, rect);
					break;
				case RefPnt.Left:
					this.t2D描画(device, x, y - (rect.Height / 2 * this.vc拡大縮小倍率.Y), depth, rect);
					break;
				case RefPnt.Center:
					this.t2D描画(device, x - (rect.Width / 2 * this.vc拡大縮小倍率.X), y - (rect.Height / 2 * this.vc拡大縮小倍率.Y), depth, rect);
					break;
				case RefPnt.Right:
					this.t2D描画(device, x - rect.Width * this.vc拡大縮小倍率.X, y - (rect.Height / 2 * this.vc拡大縮小倍率.Y), depth, rect);
					break;
				case RefPnt.DownLeft:
					this.t2D描画(device, x, y - rect.Height * this.vc拡大縮小倍率.Y, depth, rect);
					break;
				case RefPnt.Down:
					this.t2D描画(device, x - (rect.Width / 2 * this.vc拡大縮小倍率.X), y - rect.Height * this.vc拡大縮小倍率.Y, depth, rect);
					break;
				case RefPnt.DownRight:
					this.t2D描画(device, x - rect.Width * this.vc拡大縮小倍率.X, y - rect.Height * this.vc拡大縮小倍率.Y, depth, rect);
					break;
				default:
					break;
			}
		}
		public void t2D元サイズ基準描画(Device device, RefPnt refpnt, float x, float y)
		{
			this.t2D元サイズ基準描画(device, refpnt, x, y, rc全画像);
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
					this.t2D描画(device, x - (szテクスチャサイズ.Width / 2), y, depth, rect);
					break;
				case RefPnt.UpRight:
					this.t2D描画(device, x - szテクスチャサイズ.Width, y, depth, rect);
					break;
				case RefPnt.Left:
					this.t2D描画(device, x, y - (szテクスチャサイズ.Height / 2), depth, rect);
					break;
				case RefPnt.Center:
					this.t2D描画(device, x - (szテクスチャサイズ.Width / 2), y - (szテクスチャサイズ.Height / 2), depth, rect);
					break;
				case RefPnt.Right:
					this.t2D描画(device, x - szテクスチャサイズ.Width, y - (szテクスチャサイズ.Height / 2), depth, rect);
					break;
				case RefPnt.DownLeft:
					this.t2D描画(device, x, y - szテクスチャサイズ.Height, depth, rect);
					break;
				case RefPnt.Down:
					this.t2D描画(device, x - (szテクスチャサイズ.Width / 2), y - szテクスチャサイズ.Height, depth, rect);
					break;
				case RefPnt.DownRight:
					this.t2D描画(device, x - szテクスチャサイズ.Width, y - szテクスチャサイズ.Height, depth, rect);
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
			this.t2D描画(device, x, y, 1f, this.rc全画像);
		}
		public void t2D描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				return;

			this.tレンダリングステートの設定(device);

			if (this.fZ軸中心回転 == 0f)
			{
				#region [ (A) 回転なし ]
				//-----------------
				float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
				float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
				float w = rc画像内の描画領域.Width;
				float h = rc画像内の描画領域.Height;
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
				float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
				float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

				ResetWorldMatrix();

				GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
				GL.Color4(this.color);

				vertices[0].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
				vertices[0].Y = -(y + f補正値Y);
				vertices[1].X = -(x + f補正値X);
				vertices[1].Y = -(y + f補正値Y);
				vertices[2].X = -(x + f補正値X);
				vertices[2].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);
				vertices[3].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
				vertices[3].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);

				texcoord[0].X = f右U値;
				texcoord[0].Y = f上V値;
				texcoord[1].X = f左U値;
				texcoord[1].Y = f上V値;
				texcoord[2].X = f左U値;
				texcoord[2].Y = f下V値;
				texcoord[3].X = f右U値;
				texcoord[3].Y = f下V値;

				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
				GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

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
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
				float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
				float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

				float n描画領域内X = x + (rc画像内の描画領域.Width / 2.0f);
				float n描画領域内Y = y + (rc画像内の描画領域.Height / 2.0f);
				var vc3移動量 = new Vector3(n描画領域内X - (((float)GameWindowSize.Width) / 2f), -(n描画領域内Y - (((float)GameWindowSize.Height) / 2f)), 0f);

				this.vc.X = this.vc拡大縮小倍率.X;
				this.vc.Y = this.vc拡大縮小倍率.Y;
				this.vc.Z = this.vc拡大縮小倍率.Z;

				var matrix = Matrix4.Identity * Matrix4.CreateScale(this.vc);
				matrix *= Matrix4.CreateRotationZ(this.fZ軸中心回転);
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

				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
				GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

				GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
				//-----------------
				#endregion
			}
		}


		public void t2D幕用描画(Device device, float x, float y, Rectangle rc画像内の描画領域, bool left, int num = 0) 
		{
			if (this.texture == null)
				return;

			this.tレンダリングステートの設定(device);

			#region [ (A) 回転なし ]
			//-----------------
			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
			float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X) - ((!left) ? num : 0);
			vertices[2].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X) + ((left) ? num : 0);
			vertices[3].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);

			texcoord[0].X = f右U値;
			texcoord[0].Y = f上V値;
			texcoord[1].X = f左U値;
			texcoord[1].Y = f上V値;
			texcoord[2].X = f左U値;
			texcoord[2].Y = f下V値;
			texcoord[3].X = f右U値;
			texcoord[3].Y = f下V値;

			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
			GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
			//-----------------
			#endregion
		}

		public void t2D上下反転描画(Device device, float x, float y)
		{
			this.t2D上下反転描画(device, x, y, 1f, this.rc全画像);
		}
		public void t2D上下反転描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D上下反転描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D上下反転描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				throw new InvalidOperationException("テクスチャは生成されていません。");

			this.tレンダリングステートの設定(device);

			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
			float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X);
			vertices[2].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
			vertices[3].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);

			texcoord[0].X = f右U値;
			texcoord[0].Y = f下V値;
			texcoord[1].X = f左U値;
			texcoord[1].Y = f下V値;
			texcoord[2].X = f左U値;
			texcoord[2].Y = f上V値;
			texcoord[3].X = f右U値;
			texcoord[3].Y = f上V値;

			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
			GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		public void t2D左右反転描画(Device device, float x, float y)
		{
			this.t2D左右反転描画(device, x, y, 1f, this.rc全画像);
		}
		public void t2D左右反転描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D左右反転描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D左右反転描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				throw new InvalidOperationException("テクスチャは生成されていません。");

			this.tレンダリングステートの設定(device);

			float f補正値X = -GameWindowSize.Width / 2f - 0.5f;
			float f補正値Y = -GameWindowSize.Height / 2f - 0.5f;
			float w = rc画像内の描画領域.Width;
			float h = rc画像内の描画領域.Height;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
			float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			ResetWorldMatrix();

			GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
			GL.Color4(this.color);

			vertices[0].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
			vertices[0].Y = -(y + f補正値Y);
			vertices[1].X = -(x + f補正値X);
			vertices[1].Y = -(y + f補正値Y);
			vertices[2].X = -(x + f補正値X);
			vertices[2].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);
			vertices[3].X = -(x + (w * this.vc拡大縮小倍率.X) + f補正値X);
			vertices[3].Y = -((y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y);

			texcoord[0].X = f左U値;
			texcoord[0].Y = f上V値;
			texcoord[1].X = f右U値;
			texcoord[1].Y = f上V値;
			texcoord[2].X = f右U値;
			texcoord[2].Y = f下V値;
			texcoord[3].X = f左U値;
			texcoord[3].Y = f下V値;

			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
			GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		/// <summary>
		/// テクスチャを 3D 画像と見なして描画する。
		/// </summary>
		public void t3D描画(Device device, System.Numerics.Matrix4x4 mat)
		{
			this.t3D描画(device, mat, this.rc全画像);
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
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width); 
			float f上V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Top)) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)(rc全画像.Bottom - rc画像内の描画領域.Bottom)) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);

			this.tレンダリングステートの設定(device);

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

			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.vrtVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.vertices.Length * Vector3.SizeInBytes, this.vertices);
			GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, (int)this.texVBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, this.texcoord.Length * Vector2.SizeInBytes, this.texcoord);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);

			GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length * 3);
		}

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDispose完了済み)
			{
				// テクスチャの破棄
				if (this.texture.HasValue)
				{
					this.bSharpDXTextureDispose完了済み = true;
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

				this.bDispose完了済み = true;
			}
		}
		~CTexture()
		{
			// ファイナライザの動作時にtextureのDisposeがされていない場合は、
			// CTextureのDispose漏れと見做して警告をログ出力する
			if (!this.bSharpDXTextureDispose完了済み)//DTXManiaより
			{
				Trace.TraceWarning("CTexture: Dispose漏れを検出しました。(Size=({0}, {1}), filename={2}, maketype={3})", szテクスチャサイズ.Width, szテクスチャサイズ.Height, filename, maketype.ToString());
			}
			//マネージド リソースらしいので、解放はしない
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

		#region [ private ]
		//-----------------
		private int _opacity;
		private bool bDispose完了済み, bSharpDXTextureDispose完了済み;

		/// <summary>
		/// どれか一つが有効になります。
		/// </summary>
		/// <param name="device">Direct3Dのデバイス</param>
		private void tレンダリングステートの設定(Device device)
		{
			if (this.b加算合成)
			{
				GL.BlendEquation(BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
			}
			else if (this.b乗算合成)
			{
				GL.BlendEquation(BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.SrcColor);
			}
			else if (this.b減算合成)
			{
				GL.BlendEquation(BlendEquationMode.FuncReverseSubtract);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
			}
			else if (this.bスクリーン合成)
			{
				GL.BlendEquation(BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactor.OneMinusDstColor, BlendingFactor.One);
			}
			else
			{
				GL.BlendEquation(BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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

		protected Rectangle rc全画像;                              // テクスチャ作ったらあとは不変
		public Color color = Color.FromArgb(255, 255, 255, 255);
		private Matrix4 matrix = Matrix4.Identity;
		private MakeType maketype = MakeType.bytearray;
		//-----------------
		#endregion
	}
}
