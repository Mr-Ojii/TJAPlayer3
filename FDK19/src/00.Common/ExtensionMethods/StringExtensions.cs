using System;
using System.Linq;

namespace FDK.ExtensionMethods;

public static class StringExtensions
{
    public static double ToDouble(this string str, double min, double max, double def)
    {
        if (double.TryParse(str, out double num))
            return Math.Clamp(num, min, max);

        return def;
    }
    public static int ToInt32(this string str, int min, int max, int def)
    {
        // 1 と違って範囲外の場合ちゃんと丸めて返します。
        if (int.TryParse(str, out int num))
            return Math.Clamp(num, min, max);
            
        return def;
    }
}