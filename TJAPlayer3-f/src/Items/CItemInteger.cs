namespace TJAPlayer3;

/// <summary>
/// 「整数」を表すアイテム。
/// </summary>
internal class CItemInteger : CItemBase
{
    // プロパティ

    public int nValue;
    public bool bIsFocused;


    // コンストラクタ

    public CItemInteger()
    {
        base.eItemType = CItemBase.EItemType.Integer;
        this.nMin = 0;
        this.nMax = 0;
        this.nValue = 0;
        this.bIsFocused = false;
    }
    public CItemInteger(string strName, int nMin, int nMax, int nDefaultNum)
        : this()
    {
        this.tInitialize(strName, nMin, nMax, nDefaultNum);
    }
    public CItemInteger(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP)
        : this()
    {
        this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP);
    }
    public CItemInteger(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP, string strDescriptionEN)
        : this()
    {
        this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP, strDescriptionEN);
    }

    // CItemBase 実装

    public override void tPushedEnter()
    {
        this.bIsFocused = !this.bIsFocused;
    }
    public override void tMoveItemValueToNext()
    {
        if (++this.nValue > this.nMax)
        {
            this.nValue = this.nMax;
        }
    }
    public override void tMoveItemValueToForward()
    {
        if (--this.nValue < this.nMin)
        {
            this.nValue = this.nMin;
        }
    }

    public void tInitialize(string strName, int nMin, int nMax, int nDefaultNum)
    {
        this.tInitialize(strName, nMin, nMax, nDefaultNum, "", "");
    }
    public void tInitialize(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP)
    {
        this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP, strDescriptionJP);
    }
    public void tInitialize(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP, string strDescriptionEN)
    {
        base.tInitialize(strName, strDescriptionJP, strDescriptionEN);
        this.nMin = nMin;
        this.nMax = nMax;
        this.nValue = nDefaultNum;
        this.bIsFocused = false;
    }
    public override object? objValue()
    {
        return this.nValue;
    }
    public override int GetIndex()
    {
        return this.nValue;
    }
    public override void SetIndex(int index)
    {
        this.nValue = index;
    }
    // その他

    #region [ private ]
    //-----------------
    private int nMin;
    private int nMax;
    //-----------------
    #endregion
}
