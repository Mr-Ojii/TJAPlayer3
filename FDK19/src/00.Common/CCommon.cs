using SkiaSharp;

namespace FDK;

public class CCommon
{
    // 解放

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
    public static SKRectI MeasureForegroundArea(SKBitmap bmp, SKColor backColor)
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
                if (bmp.GetPixel(x, y) != backColor)
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
            return SKRectI.Empty;
        }

        //右
        int rightPos = -1;
        for (int x = bmp.Width - 1; leftPos <= x; x--)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                if (bmp.GetPixel(x, y) != backColor)
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
            return SKRectI.Empty;
        }

        //上
        int topPos = -1;
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                if (bmp.GetPixel(x, y) != backColor)
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
            return SKRectI.Empty;
        }

        //下
        int bottomPos = -1;
        for (int y = bmp.Height - 1; topPos <= y; y--)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                if (bmp.GetPixel(x, y) != backColor)
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
            return SKRectI.Empty;
        }

        //結果を返す
        return new SKRectI(leftPos, topPos, rightPos + 1, bottomPos + 1);
    }
}
