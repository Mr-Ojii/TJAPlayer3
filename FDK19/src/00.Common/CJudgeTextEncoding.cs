using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FDK;

public class CJudgeTextEncoding
{
    /// <summary>
    /// Hnc8様のReadJEncを使用して文字コードの判別をする。
    /// </summary>
    public static Encoding JudgeFileEncoding(string path)
    {
        if (!File.Exists(path))
            return null;
        FileInfo file = new FileInfo(path);

        using (Hnx8.ReadJEnc.FileReader reader = new Hnx8.ReadJEnc.FileReader(file))
            return reader.Read(file).GetEncoding();
    }
    /// <summary>
    /// Hnc8様のReadJEncを使用してテキストファイルを読み込む。
    /// 改行文字は、勝手に\nに統一する
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReadTextFile(string path)
    {
        if (!File.Exists(path))
            return null;
        string str = null;
        FileInfo file = new FileInfo(path);

        using (Hnx8.ReadJEnc.FileReader reader = new Hnx8.ReadJEnc.FileReader(file))
        {
            reader.Read(file);
            str = reader.Text;
        }
        if(!string.IsNullOrEmpty(str))
            str = str.Replace("\r\n", "\n").Replace("\r", "\n");

        return str;
    }
}
