namespace TJAPlayer3.C曲リストノードComparers;

internal sealed class C曲リストノードComparer絶対パス : IComparer<C曲リストノード>
{
    private readonly int _order;

    public C曲リストノードComparer絶対パス(int order)
    {
        this._order = order;
    }

    public int Compare(C曲リストノード n1, C曲リストノード n2)
    {
        if ((n1.eNodeType == C曲リストノード.ENodeType.BOX) && (n2.eNodeType == C曲リストノード.ENodeType.BOX))
        {
            return _order * n1.arスコア.FileInfo.DirAbsolutePath.CompareTo(n2.arスコア.FileInfo.DirAbsolutePath);
        }

        var str = strファイルの絶対パス(n1);
        var strB = strファイルの絶対パス(n2);

        return _order * str.CompareTo(strB);
    }

    private static string strファイルの絶対パス(C曲リストノード c曲リストノード)
    {
        for (int i = 0; i < (int)Difficulty.Total; i++)
        {
            if (c曲リストノード.arスコア.譜面情報.b譜面が存在する[i] != false)
            {
                return c曲リストノード.arスコア.FileInfo.FileAbsolutePath ?? "";
            }
        }

        return "";
    }
}
