namespace TJAPlayer3;

/// <summary>
/// すべてのアイテムの基本クラス。
/// </summary>
internal class CItemBase
{
    // プロパティ

    public EItemType eItemType;
    public enum EItemType
    {
        Base,
        Toggle,
        Integer,
        List
    }

    public string strName;
    public string strDescription;


    // コンストラクタ

    public CItemBase()
    {
        this.strName = "";
        this.strDescription = "";
    }
    public CItemBase(string strName)
        : this()
    {
        this.tInitialize(strName);
    }
    public CItemBase(string strName, string strDescriptionJP)
        : this()
    {
        this.tInitialize(strName, strDescriptionJP);
    }
    public CItemBase(string strName, string strDescriptionJP, string strDescriptionEN)
        : this()
    {
        this.tInitialize(strName, strDescriptionJP, strDescriptionEN);
    }


    // メソッド；子クラスで実装する

    public virtual void tPushedEnter()
    {
    }
    public virtual void tMoveItemValueToNext()
    {
    }
    public virtual void tMoveItemValueToForward()
    {
    }

    public virtual void tInitialize(string strName)
    {
        this.tInitialize(strName, "", "");
    }
    public virtual void tInitialize(string strName, string strDescriptionJP)
    {
        this.tInitialize(strName, strDescriptionJP, strDescriptionJP);
    }
    public virtual void tInitialize(string strName, string strDescriptionJP, string strDescriptionEN)
    {
        this.strName = strName;
        this.strDescription = (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja") ? strDescriptionJP : strDescriptionEN;
    }
    public virtual object? objValue()
    {
        return null;
    }
    public virtual int GetIndex()
    {
        return 0;
    }
    public virtual void SetIndex(int index)
    {
    }
}
