namespace TJAPlayer3;

/// <summary>
/// 「トグル」（ON, OFF の2状態）を表すアイテム。
/// </summary>
internal class CItemToggle : CItemBase
{
    // プロパティ

    public bool bON;


    // コンストラクタ

    public CItemToggle()
    {
        base.eItemType = CItemBase.EItemType.Toggle;
        this.bON = false;
    }
    public CItemToggle(string strName, bool bDefault)
        : this()
    {
        this.tInitialize(strName, bDefault);
    }
    public CItemToggle(string strName, bool bDefault, string strDescriptionJP)
        : this()
    {
        this.tInitialize(strName, bDefault, strDescriptionJP);
    }
    public CItemToggle(string strName, bool bDefault, string strDescriptionJP, string strDescriptionEN)
        : this()
    {
        this.tInitialize(strName, bDefault, strDescriptionJP, strDescriptionEN);
    }

    // CItemBase 実装

    public override void tPushedEnter()
    {
        this.tMoveItemValueToNext();
    }
    public override void tMoveItemValueToNext()
    {
        this.bON = !this.bON;
    }
    public override void tMoveItemValueToForward()
    {
        this.tMoveItemValueToNext();
    }

    public void tInitialize(string strName, bool bDefault)
    {
        this.tInitialize(strName, bDefault, "", "");
    }
    public void tInitialize(string strName, bool bDefault, string strDescriptionJP)
    {
        this.tInitialize(strName, bDefault, strDescriptionJP, strDescriptionJP);
    }
    public void tInitialize(string strName, bool bDefault, string strDescriptionJP, string strDescriptionEN)
    {
        base.tInitialize(strName, strDescriptionJP, strDescriptionEN);
        this.bON = bDefault;
    }
    public override object? objValue()
    {
        return (this.bON) ? "ON" : "OFF";
    }
    public override int GetIndex()
    {
        return (this.bON) ? 1 : 0;
    }
    public override void SetIndex(int index)
    {
        switch (index)
        {
            case 0:
                this.bON = false;
                break;
            case 1:
                this.bON = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
