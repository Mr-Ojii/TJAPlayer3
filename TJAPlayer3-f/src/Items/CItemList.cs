namespace TJAPlayer3;

/// <summary>
/// 「リスト」（複数の固定値からの１つを選択可能）を表すアイテム。
/// </summary>
internal class CItemList : CItemBase
{
    // プロパティ

    public List<string> list項目値;
    public int n現在選択されている項目番号;


    // コンストラクタ

    public CItemList()
    {
        base.eItemType = CItemBase.EItemType.List;
        this.n現在選択されている項目番号 = 0;
        this.list項目値 = new List<string>();
    }
    public CItemList(string strName, int nDefaultIndex, string strDescriptionJP, params string[] arg項目リスト)
        : this()
    {
        this.tInitialize(strName, nDefaultIndex, strDescriptionJP, arg項目リスト);
    }
    public CItemList(string strName, int nDefaultIndex, string strDescriptionJP, string strDescriptionEN, params string[] arg項目リスト)
        : this()
    {
        this.tInitialize(strName, nDefaultIndex, strDescriptionJP, strDescriptionEN, arg項目リスト);
    }

    // CItemBase 実装

    public override void tPushedEnter()
    {
        this.tMoveItemValueToNext();
    }
    public override void tMoveItemValueToNext()
    {
        if (++this.n現在選択されている項目番号 >= this.list項目値.Count)
        {
            this.n現在選択されている項目番号 = 0;
        }
    }
    public override void tMoveItemValueToForward()
    {
        if (--this.n現在選択されている項目番号 < 0)
        {
            this.n現在選択されている項目番号 = this.list項目値.Count - 1;
        }
    }
    public override void tInitialize(string strName)
    {
        base.tInitialize(strName);
        this.n現在選択されている項目番号 = 0;
        this.list項目値.Clear();
    }
    public void tInitialize(string strName, int nDefaultIndex, params string[] arg項目リスト)
    {
        this.tInitialize(strName, nDefaultIndex, "", "", arg項目リスト);
    }
    public void tInitialize(string strName, int nDefaultIndex, string strDescriptionJP, params string[] arg項目リスト)
    {
        this.tInitialize(strName, nDefaultIndex, strDescriptionJP, strDescriptionJP, arg項目リスト);
    }
    public void tInitialize(string strName, int nDefaultIndex, string strDescriptionJP, string strDescriptionEN, params string[] arg項目リスト)
    {
        base.tInitialize(strName, strDescriptionJP, strDescriptionEN);
        this.n現在選択されている項目番号 = nDefaultIndex;
        foreach (string str in arg項目リスト)
        {
            this.list項目値.Add(str);
        }
    }
    public override object? objValue()
    {
        return this.list項目値[n現在選択されている項目番号];
    }
    public override int GetIndex()
    {
        return n現在選択されている項目番号;
    }
    public override void SetIndex(int index)
    {
        n現在選択されている項目番号 = index;
    }
}
