namespace FDK;

public class CActivity
{
    // プロパティ

    public bool b活性化してる { get; private set; }
    public bool b活性化してない
    {
        get
        {
            return !this.b活性化してる;
        }
        private set
        {
            this.b活性化してる = !value;
        }
    }
    public List<CActivity> listChildren;

    /// <summary>
    /// <para>初めて On進行描画() を呼び出す場合に true を示す。（On活性化() 内で true にセットされる。）</para>
    /// <para>このフラグは、On活性化() では行えないタイミングのシビアな初期化を On進行描画() で行うために準備されている。利用は必須ではない。</para>
    /// <para>On進行描画() 側では、必要な初期化を追えたら false をセットすること。</para>
    /// </summary>
    protected bool b初めての進行描画 = true;


    // コンストラクタ

    public CActivity()
    {
        this.b活性化してない = true;
        this.listChildren = new List<CActivity>();
    }


    // ライフサイクルメソッド

    #region [ 子クラスで必要なもののみ override すること。]
    //-----------------

    public virtual void On活性化()
    {
        // すでに活性化してるなら何もしない。
        if (this.b活性化してる)
            return;

        this.b活性化してる = true;		// このフラグは、以下の処理をする前にセットする。

        // すべての子 Activity を活性化する。
        foreach (CActivity activity in this.listChildren)
            activity.On活性化();

        // その他の初期化
        this.b初めての進行描画 = true;
    }
    public virtual void On非活性化()
    {
        // 活性化してないなら何もしない。
        if (this.b活性化してない)
            return;

        // すべての 子Activity を非活性化する。
        foreach (CActivity activity in this.listChildren)
            activity.On非活性化();

        this.b活性化してない = true;	// このフラグは、以上のメソッドを呼び出した後にセットする。
    }

    /// <summary>
    /// <para>進行と描画を行う。（これらは分離されず、この１つのメソッドだけで実装する。）</para>
    /// <para>このメソッドは BeginScene() の後に呼び出されるので、メソッド内でいきなり描画を行ってかまわない。</para>
    /// </summary>
    /// <returns>任意の整数。呼び出し元との整合性を合わせておくこと。</returns>
    public virtual int On進行描画()
    {
        // 活性化してないなら何もしない。
        if (this.b活性化してない)
            return 0;


        /* ここで進行と描画を行う。*/


        // 戻り値とその意味は子クラスで自由に決めていい。
        return 0;
    }

    //-----------------
    #endregion
}
