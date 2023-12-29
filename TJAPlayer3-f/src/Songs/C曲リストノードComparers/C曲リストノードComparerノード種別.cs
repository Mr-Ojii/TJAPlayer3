namespace TJAPlayer3.C曲リストノードComparers;

internal sealed class C曲リストノードComparerNodeType : IComparer<C曲リストノード>
{
    public int Compare(C曲リストノード x, C曲リストノード y)
    {
        return ToComparable(x.eNodeType).CompareTo(ToComparable(y.eNodeType));
    }

    private static int ToComparable(C曲リストノード.ENodeType eNodeType)
    {
        switch (eNodeType)
        {
            case C曲リストノード.ENodeType.BOX:
                return 0;
            case C曲リストノード.ENodeType.SCORE:
                return 1;
            case C曲リストノード.ENodeType.UNKNOWN:
                return 2;
            case C曲リストノード.ENodeType.RANDOM:
                return 3;
            case C曲リストノード.ENodeType.BACKBOX:
                return 4;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
