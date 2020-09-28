using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D9;

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
		public Size sz画像サイズ
		{
			get;
			protected set;
		}
		public Texture texture
		{
			get;
			private set;
		}
		public Format Format
		{
			get;
			protected set;
		}
		public OpenTK.Vector3 vc拡大縮小倍率;
		private Vector3 vc;
		public string filename;

		// 画面が変わるたび以下のプロパティを設定し治すこと。

		public static Size sz論理画面 = Size.Empty;
		public static Size sz物理画面 = Size.Empty;
		public static Rectangle rc物理画面描画領域 = Rectangle.Empty;
		/// <summary>
		/// <para>論理画面を1とする場合の物理画面の倍率。</para>
		/// <para>論理値×画面比率＝物理値。</para>
		/// </summary>
		public static float f画面比率 = 1.0f;

		// コンストラクタ
		
		public CTexture()
		{
			this.sz画像サイズ = new Size(0, 0);
			this.szテクスチャサイズ = new Size(0, 0);
			this._opacity = 0xff;
			this.texture = null;
			this.bSharpDXTextureDispose完了済み = true;
			this.cvPositionColoredVertexies = null;
			this.b加算合成 = false;
			this.fZ軸中心回転 = 0f;
			this.vc拡大縮小倍率 = new OpenTK.Vector3(1f, 1f, 1f);
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
		public CTexture(Device device, string strファイル名, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.filename;
			MakeTexture(device, strファイル名, b黒を透過する, Pool.Managed);
		}
		public CTexture(Device device, byte[] txData, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.bytearray;
			MakeTexture(device, txData, Format.A8R8G8B8, b黒を透過する, Pool.Managed);
		}
		public CTexture(Device device, Bitmap bitmap, bool b黒を透過する)
			: this()
		{
			maketype = MakeType.bitmap;
			MakeTexture(device, bitmap, Format.A8R8G8B8, b黒を透過する, Pool.Managed);
		}

		public void MakeTexture(Device device, string strファイル名, bool b黒を透過する, Pool pool)
		{
			if (!File.Exists(strファイル名))     // #27122 2012.1.13 from: ImageInformation では FileNotFound 例外は返ってこないので、ここで自分でチェックする。わかりやすいログのために。
				throw new FileNotFoundException(string.Format("ファイルが存在しません。\n[{0}]", strファイル名));

			this.filename = Path.GetFileName(strファイル名);
			Byte[] _txData = File.ReadAllBytes(strファイル名);
			MakeTexture(device, _txData, Format.A8R8G8B8, b黒を透過する, pool);
		}
		public void MakeTexture(Device device, byte[] txData, Format format, bool b黒を透過する, Pool pool)
		{
			try
			{
				var information = ImageInformation.FromMemory(txData);
				this.Format = format;
				this.sz画像サイズ = new Size(information.Width, information.Height);
				this.rc全画像 = new Rectangle(0, 0, this.sz画像サイズ.Width, this.sz画像サイズ.Height);
				this.colorKey = (b黒を透過する) ? unchecked((int)0xFF000000) : 0;
				this.szテクスチャサイズ = this.t指定されたサイズを超えない最適なテクスチャサイズを返す(device, this.sz画像サイズ);
				this.Pl = pool;
				//				lock ( lockobj )
				//				{
				//Trace.TraceInformation( "CTexture() start: " );
				this.texture = Texture.FromMemory(device, txData, this.sz画像サイズ.Width, this.sz画像サイズ.Height, 1, Usage.None, format, pool, Filter.Point, Filter.None, this.colorKey);
				//Trace.TraceInformation( "CTexture() end:   " );
				//				}
				this.bSharpDXTextureDispose完了済み = false;
			}
			catch
			{
				this.Dispose();
				// throw new CTextureCreateFailedException( string.Format( "テクスチャの生成に失敗しました。\n{0}", strファイル名 ) );
				throw new CTextureCreateFailedException(string.Format("テクスチャの生成に失敗しました。\n"));
			}
		}
		public void MakeTexture(Device device, Bitmap bitmap, Format format, bool b黒を透過する, Pool pool)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bitmap.Save(ms, ImageFormat.Bmp);
				try
				{
					this.Format = format;
					this.sz画像サイズ = new Size(bitmap.Width, bitmap.Height);
					this.rc全画像 = new Rectangle(0, 0, this.sz画像サイズ.Width, this.sz画像サイズ.Height);
					this.colorKey = (b黒を透過する) ? unchecked((int)0xFF000000) : 0;
					this.szテクスチャサイズ = this.t指定されたサイズを超えない最適なテクスチャサイズを返す(device, this.sz画像サイズ);
					this.Pl = pool;
					this.texture = Texture.FromMemory(device, ms.GetBuffer(), this.sz画像サイズ.Width, this.sz画像サイズ.Height, 1, Usage.None, format, pool, Filter.Point, Filter.None, this.colorKey);
					this.bSharpDXTextureDispose完了済み = false;
				}
				catch
				{
					this.Dispose();
					throw new CTextureCreateFailedException(string.Format("テクスチャの生成に失敗しました。\n"));
				}
			}
		}
		// メソッド

		// 2016.11.10 kairera0467 拡張
		// Rectangleを使う場合、座標調整のためにテクスチャサイズの値をそのまま使うとまずいことになるため、Rectragleから幅を取得して調整をする。
		public void t2D中心基準描画(Device device, float x, float y)
		{
			this.t2D描画(device, x - (this.szテクスチャサイズ.Width / 2), y - (this.szテクスチャサイズ.Height / 2), 1f, this.rc全画像);
		}
		public void t2D中心基準描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x - (rc画像内の描画領域.Width / 2), y - (rc画像内の描画領域.Height / 2), 1f, rc画像内の描画領域);
		}
		public void t2D中心基準描画(Device device, float x, float y, float depth, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, (int)x - (rc画像内の描画領域.Width / 2), (int)y - (rc画像内の描画領域.Height / 2), depth, rc画像内の描画領域);
		}
		public void t2D拡大率考慮右上基準描画(Device device, float x, float y) 
		{
			this.t2D描画(device, (x - (this.rc全画像.Width * this.vc拡大縮小倍率.X)), y);
		}
		public void t2D拡大率考慮左下基準描画(Device device, float x, float y) 
		{
			this.t2D描画(device, x, y - (this.szテクスチャサイズ.Height * this.vc拡大縮小倍率.Y));
		}

		// 下を基準にして描画する(拡大率考慮)メソッドを追加。 (AioiLight)
		public void t2D拡大率考慮下基準描画(Device device, float x, float y)
		{
			this.t2D描画(device, x, y - (szテクスチャサイズ.Height * this.vc拡大縮小倍率.Y), 1f, this.rc全画像);
		}
		public void t2D拡大率考慮下基準描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x, y - (rc画像内の描画領域.Height * this.vc拡大縮小倍率.Y), 1f, rc画像内の描画領域);
		}
		public void t2D拡大率考慮下中心基準描画(Device device, float x, float y)
		{
			this.t2D描画(device, x - (this.szテクスチャサイズ.Width / 2), y - (szテクスチャサイズ.Height * this.vc拡大縮小倍率.Y), 1f, this.rc全画像);
		}

		public void t2D拡大率考慮下拡大率考慮中心基準描画(Device device, float x, float y)
		{
			this.t2D描画(device, x - (this.szテクスチャサイズ.Width * this.vc拡大縮小倍率.X / 2), y - (szテクスチャサイズ.Height * this.vc拡大縮小倍率.Y), 1f, this.rc全画像);
		}

		public void t2D拡大率考慮下中心基準描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x - ((rc画像内の描画領域.Width / 2)), y - (rc画像内の描画領域.Height * this.vc拡大縮小倍率.Y), 1f, rc画像内の描画領域);
		}
		public void t2D中央基準描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x - (this.szテクスチャサイズ.Width / 2), y - (this.szテクスチャサイズ.Height / 2), rc画像内の描画領域);
		}
		public void t2D下中央基準描画(Device device, float x, float y)
		{
			this.t2D下中央基準描画(device, x, y, this.rc全画像);
		}
		public void t2D下中央基準描画(Device device, float x, float y, Rectangle rc画像内の描画領域)
		{
			this.t2D描画(device, x - (rc画像内の描画領域.Width / 2), y - (rc画像内の描画領域.Height), rc画像内の描画領域);
			//this.t2D描画(devicek x, y, rc画像内の描画領域;
		}

		public void t2D拡大率考慮中央基準描画(Device device, float x, float y)
		{
			this.t2D描画(device, x - (this.szテクスチャサイズ.Width / 2 * this.vc拡大縮小倍率.X), y - (szテクスチャサイズ.Height / 2 * this.vc拡大縮小倍率.Y), 1f, this.rc全画像);
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

			if (this.texture.Device.NativePointer != device.NativePointer)
				ReuseTexture(device);

			this.tレンダリングステートの設定(device);

			if (this.fZ軸中心回転 == 0f)
			{
				#region [ (A) 回転なし ]
				//-----------------
				float f補正値X = -0.5f;    // -0.5 は座標とピクセルの誤差を吸収するための座標補正値。(MSDN参照)
				float f補正値Y = -0.5f;    //
				float w = rc画像内の描画領域.Width;
				float h = rc画像内の描画領域.Height;
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
				float f上V値 = ((float)rc画像内の描画領域.Top) / ((float)this.szテクスチャサイズ.Height);
				float f下V値 = ((float)rc画像内の描画領域.Bottom) / ((float)this.szテクスチャサイズ.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);
				int color = this.color.ToArgb();

				if (this.cvTransformedColoredVertexies == null)
					this.cvTransformedColoredVertexies = new TransformedColoredTexturedVertex[4];

				// #27122 2012.1.13 from: 以下、マネージドオブジェクト（＝ガベージ）の量産を抑えるため、new は使わず、メンバに値を１つずつ直接上書きする。

				this.cvTransformedColoredVertexies[0].Position.X = x + f補正値X;
				this.cvTransformedColoredVertexies[0].Position.Y = y + f補正値Y;
				this.cvTransformedColoredVertexies[0].Position.Z = depth;
				this.cvTransformedColoredVertexies[0].Position.W = 1.0f;
				this.cvTransformedColoredVertexies[0].Color = color;
				this.cvTransformedColoredVertexies[0].TextureCoordinates.X = f左U値;
				this.cvTransformedColoredVertexies[0].TextureCoordinates.Y = f上V値;

				this.cvTransformedColoredVertexies[1].Position.X = (x + (w * this.vc拡大縮小倍率.X)) + f補正値X;
				this.cvTransformedColoredVertexies[1].Position.Y = y + f補正値Y;
				this.cvTransformedColoredVertexies[1].Position.Z = depth;
				this.cvTransformedColoredVertexies[1].Position.W = 1.0f;
				this.cvTransformedColoredVertexies[1].Color = color;
				this.cvTransformedColoredVertexies[1].TextureCoordinates.X = f右U値;
				this.cvTransformedColoredVertexies[1].TextureCoordinates.Y = f上V値;

				this.cvTransformedColoredVertexies[2].Position.X = x + f補正値X;
				this.cvTransformedColoredVertexies[2].Position.Y = (y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y;
				this.cvTransformedColoredVertexies[2].Position.Z = depth;
				this.cvTransformedColoredVertexies[2].Position.W = 1.0f;
				this.cvTransformedColoredVertexies[2].Color = color;
				this.cvTransformedColoredVertexies[2].TextureCoordinates.X = f左U値;
				this.cvTransformedColoredVertexies[2].TextureCoordinates.Y = f下V値;

				this.cvTransformedColoredVertexies[3].Position.X = (x + (w * this.vc拡大縮小倍率.X)) + f補正値X;
				this.cvTransformedColoredVertexies[3].Position.Y = (y + (h * this.vc拡大縮小倍率.Y)) + f補正値Y;
				this.cvTransformedColoredVertexies[3].Position.Z = depth;
				this.cvTransformedColoredVertexies[3].Position.W = 1.0f;
				this.cvTransformedColoredVertexies[3].Color = color;
				this.cvTransformedColoredVertexies[3].TextureCoordinates.X = f右U値;
				this.cvTransformedColoredVertexies[3].TextureCoordinates.Y = f下V値;

				device.SetTexture(0, this.texture);
				device.VertexFormat = TransformedColoredTexturedVertex.Format;
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, this.cvTransformedColoredVertexies);
				
				//-----------------
				#endregion
			}
			else
			{
				#region [ (B) 回転あり ]
				//-----------------
				float f補正値X = ((rc画像内の描画領域.Width % 2) == 0) ? -0.5f : 0f;   // -0.5 は座標とピクセルの誤差を吸収するための座標補正値。(MSDN参照)
				float f補正値Y = ((rc画像内の描画領域.Height % 2) == 0) ? -0.5f : 0f;  // 3D（回転する）なら補正はいらない。
				float f中央X = ((float)rc画像内の描画領域.Width) / 2f;
				float f中央Y = ((float)rc画像内の描画領域.Height) / 2f;
				float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
				float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
				float f上V値 = ((float)rc画像内の描画領域.Top) / ((float)this.szテクスチャサイズ.Height);
				float f下V値 = ((float)rc画像内の描画領域.Bottom) / ((float)this.szテクスチャサイズ.Height);

				this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);
				int color = this.color.ToArgb();

				if (this.cvPositionColoredVertexies == null)
					this.cvPositionColoredVertexies = new PositionColoredTexturedVertex[4];

				// #27122 2012.1.13 from: 以下、マネージドオブジェクト（＝ガベージ）の量産を抑えるため、new は使わず、メンバに値を１つずつ直接上書きする。

				this.cvPositionColoredVertexies[0].Position.X = -f中央X + f補正値X;
				this.cvPositionColoredVertexies[0].Position.Y = f中央Y + f補正値Y;
				this.cvPositionColoredVertexies[0].Position.Z = depth;
				this.cvPositionColoredVertexies[0].Color = color;
				this.cvPositionColoredVertexies[0].TextureCoordinates.X = f左U値;
				this.cvPositionColoredVertexies[0].TextureCoordinates.Y = f上V値;

				this.cvPositionColoredVertexies[1].Position.X = f中央X + f補正値X;
				this.cvPositionColoredVertexies[1].Position.Y = f中央Y + f補正値Y;
				this.cvPositionColoredVertexies[1].Position.Z = depth;
				this.cvPositionColoredVertexies[1].Color = color;
				this.cvPositionColoredVertexies[1].TextureCoordinates.X = f右U値;
				this.cvPositionColoredVertexies[1].TextureCoordinates.Y = f上V値;

				this.cvPositionColoredVertexies[2].Position.X = -f中央X + f補正値X;
				this.cvPositionColoredVertexies[2].Position.Y = -f中央Y + f補正値Y;
				this.cvPositionColoredVertexies[2].Position.Z = depth;
				this.cvPositionColoredVertexies[2].Color = color;
				this.cvPositionColoredVertexies[2].TextureCoordinates.X = f左U値;
				this.cvPositionColoredVertexies[2].TextureCoordinates.Y = f下V値;

				this.cvPositionColoredVertexies[3].Position.X = f中央X + f補正値X;
				this.cvPositionColoredVertexies[3].Position.Y = -f中央Y + f補正値Y;
				this.cvPositionColoredVertexies[3].Position.Z = depth;
				this.cvPositionColoredVertexies[3].Color = color;
				this.cvPositionColoredVertexies[3].TextureCoordinates.X = f右U値;
				this.cvPositionColoredVertexies[3].TextureCoordinates.Y = f下V値;

				float n描画領域内X = x + (rc画像内の描画領域.Width / 2.0f);
				float n描画領域内Y = y + (rc画像内の描画領域.Height / 2.0f);
				var vc3移動量 = new Vector3(n描画領域内X - (((float)device.Viewport.Width) / 2f), -(n描画領域内Y - (((float)device.Viewport.Height) / 2f)), 0f);

				this.vc.X = this.vc拡大縮小倍率.X;
				this.vc.Y = this.vc拡大縮小倍率.Y;
				this.vc.Z = this.vc拡大縮小倍率.Z;

				var matrix = Matrix.Identity * Matrix.Scaling(this.vc);
				matrix *= Matrix.RotationZ(this.fZ軸中心回転);
				matrix *= Matrix.Translation(vc3移動量);
				device.SetTransform(TransformState.World, matrix);

				device.SetTexture(0, this.texture);
				device.VertexFormat = PositionColoredTexturedVertex.Format;
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, this.cvPositionColoredVertexies);
				//-----------------
				#endregion
			}
		}
		public void t2D上下反転描画(Device device, int x, int y)
		{
			this.t2D上下反転描画(device, x, y, 1f, this.rc全画像);
		}
		public void t2D上下反転描画(Device device, int x, int y, Rectangle rc画像内の描画領域)
		{
			this.t2D上下反転描画(device, x, y, 1f, rc画像内の描画領域);
		}
		public void t2D上下反転描画(Device device, int x, int y, float depth, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				throw new InvalidOperationException("テクスチャは生成されていません。");

			if (this.texture.Device.NativePointer != device.NativePointer)
				ReuseTexture(device);

			this.tレンダリングステートの設定(device);

			float fx = x * CTexture.f画面比率 + CTexture.rc物理画面描画領域.X - 0.5f;   // -0.5 は座標とピクセルの誤差を吸収するための座標補正値。(MSDN参照)
			float fy = y * CTexture.f画面比率 + CTexture.rc物理画面描画領域.Y - 0.5f;   //
			float w = rc画像内の描画領域.Width * this.vc拡大縮小倍率.X * CTexture.f画面比率;
			float h = rc画像内の描画領域.Height * this.vc拡大縮小倍率.Y * CTexture.f画面比率;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
			float f上V値 = ((float)rc画像内の描画領域.Top) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)rc画像内の描画領域.Bottom) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);
			int color = this.color.ToArgb();

			if (this.cvTransformedColoredVertexies == null)
				this.cvTransformedColoredVertexies = new TransformedColoredTexturedVertex[4];

			// 以下、マネージドオブジェクトの量産を抑えるため new は使わない。

			this.cvTransformedColoredVertexies[0].TextureCoordinates.X = f左U値;  // 左上	→ 左下
			this.cvTransformedColoredVertexies[0].TextureCoordinates.Y = f下V値;
			this.cvTransformedColoredVertexies[0].Position.X = fx;
			this.cvTransformedColoredVertexies[0].Position.Y = fy;
			this.cvTransformedColoredVertexies[0].Position.Z = depth;
			this.cvTransformedColoredVertexies[0].Position.W = 1.0f;
			this.cvTransformedColoredVertexies[0].Color = color;

			this.cvTransformedColoredVertexies[1].TextureCoordinates.X = f右U値;  // 右上 → 右下
			this.cvTransformedColoredVertexies[1].TextureCoordinates.Y = f下V値;
			this.cvTransformedColoredVertexies[1].Position.X = fx + w;
			this.cvTransformedColoredVertexies[1].Position.Y = fy;
			this.cvTransformedColoredVertexies[1].Position.Z = depth;
			this.cvTransformedColoredVertexies[1].Position.W = 1.0f;
			this.cvTransformedColoredVertexies[1].Color = color;

			this.cvTransformedColoredVertexies[2].TextureCoordinates.X = f左U値;  // 左下 → 左上
			this.cvTransformedColoredVertexies[2].TextureCoordinates.Y = f上V値;
			this.cvTransformedColoredVertexies[2].Position.X = fx;
			this.cvTransformedColoredVertexies[2].Position.Y = fy + h;
			this.cvTransformedColoredVertexies[2].Position.Z = depth;
			this.cvTransformedColoredVertexies[2].Position.W = 1.0f;
			this.cvTransformedColoredVertexies[2].Color = color;

			this.cvTransformedColoredVertexies[3].TextureCoordinates.X = f右U値;  // 右下 → 右上
			this.cvTransformedColoredVertexies[3].TextureCoordinates.Y = f上V値;
			this.cvTransformedColoredVertexies[3].Position.X = fx + w;
			this.cvTransformedColoredVertexies[3].Position.Y = fy + h;
			this.cvTransformedColoredVertexies[3].Position.Z = depth;
			this.cvTransformedColoredVertexies[3].Position.W = 1.0f;
			this.cvTransformedColoredVertexies[3].Color = color;

			device.SetTexture(0, this.texture);
			device.VertexFormat = TransformedColoredTexturedVertex.Format;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, this.cvTransformedColoredVertexies);
		}
		public void t2D上下反転描画(Device device, Point pt)
		{
			this.t2D上下反転描画(device, pt.X, pt.Y, 1f, this.rc全画像);
		}
		public void t2D上下反転描画(Device device, Point pt, Rectangle rc画像内の描画領域)
		{
			this.t2D上下反転描画(device, pt.X, pt.Y, 1f, rc画像内の描画領域);
		}
		public void t2D上下反転描画(Device device, Point pt, float depth, Rectangle rc画像内の描画領域)
		{
			this.t2D上下反転描画(device, pt.X, pt.Y, depth, rc画像内の描画領域);
		}

		public static Vector3 t論理画面座標をワールド座標へ変換する(int x, int y)
		{
			return CTexture.t論理画面座標をワールド座標へ変換する(new Vector3((float)x, (float)y, 0f));
		}
		public static Vector3 t論理画面座標をワールド座標へ変換する(float x, float y)
		{
			return CTexture.t論理画面座標をワールド座標へ変換する(new Vector3(x, y, 0f));
		}
		public static Vector3 t論理画面座標をワールド座標へ変換する(Point pt論理画面座標)
		{
			return CTexture.t論理画面座標をワールド座標へ変換する(new Vector3(pt論理画面座標.X, pt論理画面座標.Y, 0.0f));
		}
		public static Vector3 t論理画面座標をワールド座標へ変換する(Vector2 v2論理画面座標)
		{
			return CTexture.t論理画面座標をワールド座標へ変換する(new Vector3(v2論理画面座標, 0f));
		}
		public static Vector3 t論理画面座標をワールド座標へ変換する(Vector3 v3論理画面座標)
		{
			return new Vector3(
				(v3論理画面座標.X - (CTexture.sz論理画面.Width / 2.0f)) * CTexture.f画面比率,
				(-(v3論理画面座標.Y - (CTexture.sz論理画面.Height / 2.0f)) * CTexture.f画面比率),
				v3論理画面座標.Z);
		}

		/// <summary>
		/// テクスチャを 3D 画像と見なして描画する。
		/// </summary>
		public void t3D描画(Device device, OpenTK.Matrix4 mat)
		{
			this.t3D描画(device, mat, this.rc全画像);
		}
		public void t3D描画(Device device, OpenTK.Matrix4 mat, Rectangle rc画像内の描画領域)
		{
			if (this.texture == null)
				return;

			if (this.texture.Device.NativePointer != device.NativePointer)
				ReuseTexture(device);

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
			float z = 0.0f;
			float f左U値 = ((float)rc画像内の描画領域.Left) / ((float)this.szテクスチャサイズ.Width);
			float f右U値 = ((float)rc画像内の描画領域.Right) / ((float)this.szテクスチャサイズ.Width);
			float f上V値 = ((float)rc画像内の描画領域.Top) / ((float)this.szテクスチャサイズ.Height);
			float f下V値 = ((float)rc画像内の描画領域.Bottom) / ((float)this.szテクスチャサイズ.Height);

			this.color = Color.FromArgb(this._opacity, this.color.R, this.color.G, this.color.B);
			int color = this.color.ToArgb();

			if (this.cvPositionColoredVertexies == null)
				this.cvPositionColoredVertexies = new PositionColoredTexturedVertex[4];

			// #27122 2012.1.13 from: 以下、マネージドオブジェクト（＝ガベージ）の量産を抑えるため、new は使わず、メンバに値を１つずつ直接上書きする。

			this.cvPositionColoredVertexies[0].Position.X = -x;
			this.cvPositionColoredVertexies[0].Position.Y = y;
			this.cvPositionColoredVertexies[0].Position.Z = z;
			this.cvPositionColoredVertexies[0].Color = color;
			this.cvPositionColoredVertexies[0].TextureCoordinates.X = f左U値;
			this.cvPositionColoredVertexies[0].TextureCoordinates.Y = f上V値;

			this.cvPositionColoredVertexies[1].Position.X = x;
			this.cvPositionColoredVertexies[1].Position.Y = y;
			this.cvPositionColoredVertexies[1].Position.Z = z;
			this.cvPositionColoredVertexies[1].Color = color;
			this.cvPositionColoredVertexies[1].TextureCoordinates.X = f右U値;
			this.cvPositionColoredVertexies[1].TextureCoordinates.Y = f上V値;

			this.cvPositionColoredVertexies[2].Position.X = -x;
			this.cvPositionColoredVertexies[2].Position.Y = -y;
			this.cvPositionColoredVertexies[2].Position.Z = z;
			this.cvPositionColoredVertexies[2].Color = color;
			this.cvPositionColoredVertexies[2].TextureCoordinates.X = f左U値;
			this.cvPositionColoredVertexies[2].TextureCoordinates.Y = f下V値;

			this.cvPositionColoredVertexies[3].Position.X = x;
			this.cvPositionColoredVertexies[3].Position.Y = -y;
			this.cvPositionColoredVertexies[3].Position.Z = z;
			this.cvPositionColoredVertexies[3].Color = color;
			this.cvPositionColoredVertexies[3].TextureCoordinates.X = f右U値;
			this.cvPositionColoredVertexies[3].TextureCoordinates.Y = f下V値;

			this.tレンダリングステートの設定(device);

			device.SetTransform(TransformState.World, matrix);
			device.SetTexture(0, this.texture);
			device.VertexFormat = PositionColoredTexturedVertex.Format;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, this.cvPositionColoredVertexies);
		}

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDispose完了済み)
			{
				// テクスチャの破棄
				if (this.texture != null)
				{
					this.bSharpDXTextureDispose完了済み = true;
					this.texture.Dispose();
					this.texture = null;
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
				Trace.TraceWarning("CTexture: Dispose漏れを検出しました。(Size=({0}, {1}), filename={2}, maketype={3})", sz画像サイズ.Width, sz画像サイズ.Height, filename, maketype.ToString());
			}
			//マネージド リソースらしいので、解放はしない
		}
		//-----------------
		#endregion


		// その他

		#region [ private ]
		//-----------------
		private int _opacity;
		private bool bDispose完了済み, bSharpDXTextureDispose完了済み;
		private PositionColoredTexturedVertex[] cvPositionColoredVertexies;
		protected TransformedColoredTexturedVertex[] cvTransformedColoredVertexies = new TransformedColoredTexturedVertex[]
		{
			new TransformedColoredTexturedVertex(),
			new TransformedColoredTexturedVertex(),
			new TransformedColoredTexturedVertex(),
			new TransformedColoredTexturedVertex(),
		};
		//		byte[] _txData;
		static object lockobj = new object();

		private void ReuseTexture(Device device)
		{
			if (this.texture != null)
			{
				using (DataStream stream = Texture.ToStream(this.texture, ImageFileFormat.Bmp))
				{
					this.texture.Dispose();
					this.texture = Texture.FromStream(device, stream, this.sz画像サイズ.Width, this.sz画像サイズ.Height, 1, Usage.None, this.Format, this.Pl, Filter.Point, Filter.None, colorKey);
				}
			}
		}

		/// <summary>
		/// どれか一つが有効になります。
		/// </summary>
		/// <param name="device">Direct3Dのデバイス</param>
		private void tレンダリングステートの設定(Device device)
		{
			if (this.b加算合成)
			{
				device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);             // 5
				device.SetRenderState(RenderState.DestinationBlend, Blend.One);                    // 2
			}
			else if (this.b乗算合成)
			{
				//参考:http://sylphylunar.seesaa.net/article/390331341.html
				//C++から引っ張ってきたのでちょっと不安。
				device.SetRenderState(RenderState.SourceBlend, Blend.DestinationColor);
				device.SetRenderState(RenderState.DestinationBlend, Blend.Zero);
			}
			else if (this.b減算合成)
			{
				//参考:http://www3.pf-x.net/~chopper/home2/DirectX/MD20.html
				device.SetRenderState(RenderState.BlendOperation, BlendOperation.Subtract);
				device.SetRenderState(RenderState.SourceBlend, Blend.One);
				device.SetRenderState(RenderState.DestinationBlend, Blend.One);
			}
			else if (this.bスクリーン合成)
			{
				//参考:http://sylphylunar.seesaa.net/article/390331341.html
				//C++から引っ張ってきたのでちょっと不安。
				device.SetRenderState(RenderState.SourceBlend, Blend.InverseDestinationColor);
				device.SetRenderState(RenderState.DestinationBlend, Blend.One);
			}
			else
			{
				device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);             // 5
				device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha); // 6
			}
		}
		private Size t指定されたサイズを超えない最適なテクスチャサイズを返す(Device device, Size sz指定サイズ)
		{
			var deviceCapabilities = device.Capabilities;
			var deviceCapabilitiesTextureCaps = deviceCapabilities.TextureCaps;

			bool b条件付きでサイズは２の累乗でなくてもOK = (deviceCapabilitiesTextureCaps & TextureCaps.NonPow2Conditional) != 0;
			bool bサイズは２の累乗でなければならない = (deviceCapabilitiesTextureCaps & TextureCaps.Pow2) != 0;
			bool b正方形でなければならない = (deviceCapabilitiesTextureCaps & TextureCaps.SquareOnly) != 0;
			int n最大幅 = deviceCapabilities.MaxTextureWidth;
			int n最大高 = deviceCapabilities.MaxTextureHeight;
			var szサイズ = new Size(sz指定サイズ.Width, sz指定サイズ.Height);

			if (bサイズは２の累乗でなければならない && !b条件付きでサイズは２の累乗でなくてもOK)
			{
				// 幅を２の累乗にする
				int n = 1;
				do
				{
					n *= 2;
				}
				while (n <= sz指定サイズ.Width);
				sz指定サイズ.Width = n;

				// 高さを２の累乗にする
				n = 1;
				do
				{
					n *= 2;
				}
				while (n <= sz指定サイズ.Height);
				sz指定サイズ.Height = n;
			}

			if (sz指定サイズ.Width > n最大幅)
				sz指定サイズ.Width = n最大幅;

			if (sz指定サイズ.Height > n最大高)
				sz指定サイズ.Height = n最大高;

			if (b正方形でなければならない)
			{
				if (szサイズ.Width > szサイズ.Height)
				{
					szサイズ.Height = szサイズ.Width;
				}
				else if (szサイズ.Width < szサイズ.Height)
				{
					szサイズ.Width = szサイズ.Height;
				}
			}

			return szサイズ;
		}
		private enum MakeType
		{
			filename,
			bytearray,
			bitmap
		}

		// 2012.3.21 さらなる new の省略作戦

		protected Rectangle rc全画像;                              // テクスチャ作ったらあとは不変
		private int colorKey;
		private Pool Pl;
		public Color color = Color.FromArgb(255, 255, 255, 255);
		private Matrix matrix = Matrix.Identity;
		private MakeType maketype = MakeType.bytearray;
		//-----------------
		#endregion
	}
}
