namespace TJAPlayer3.C曲リストノードComparers;

internal sealed class C曲リストノードComparerGenre : IComparer<C曲リストノード>
{
    public C曲リストノードComparerGenre(int order)
    {
        this.order = order;
    }

    public int Compare(C曲リストノード n1, C曲リストノード n2)
    {
        return TJAPlayer3.app.Skin.nStrGenreToNumForSort(n1.strGenre, order).CompareTo(TJAPlayer3.app.Skin.nStrGenreToNumForSort(n2.strGenre, order));
    }

    private readonly int order;
}
