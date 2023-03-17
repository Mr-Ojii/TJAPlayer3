using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FDK;

public class CCommon
{
	// 解放

	public static void tDispose<T>( ref T obj )
	{
		if( obj == null )
			return;

		var d = obj as IDisposable;

		if( d != null )
		{
			d.Dispose();
			obj = default( T );
		}
	}
	public static void tDispose<T>( T obj )
	{
		if( obj == null )
			return;

		var d = obj as IDisposable;

		if( d != null )
			d.Dispose();
	}

	public static void tRunCompleteGC()
	{
		GC.Collect();					// アクセス不可能なオブジェクトを除去し、ファイナライぜーション実施。
		GC.WaitForPendingFinalizers();	// ファイナライゼーションが終わるまでスレッドを待機。
		GC.Collect();					// ファイナライズされたばかりのオブジェクトに関連するメモリを開放。

		// 出展: http://msdn.microsoft.com/ja-jp/library/ms998547.aspx#scalenetchapt05_topic10
	}


	/// <summary>
	/// 指定されたImageで、backColor以外の色が使われている範囲を計測する
	/// </summary>
	public static Rectangle MeasureForegroundArea(Image<Rgba32> bmp, SixLabors.ImageSharp.Color backColor)
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
}
