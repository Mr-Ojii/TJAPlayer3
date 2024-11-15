using FDK;


namespace TJAPlayer3;

internal class CActSelectPopupMenu : CActivity
{

    // プロパティ


    public int GetIndex(int pos)
    {
        return lciMenuItems[pos].cItem.GetIndex();
    }
    public object GetObj現在値(int pos)
    {
        return lciMenuItems[pos].cItem.objValue();
    }
    public bool bGotoDetailConfig
    {
        get;
        internal set;
    }

    /// <summary>
    /// ソートメニュー機能を使用中かどうか。外部からこれをtrueにすると、ソートメニューが出現する。falseにすると消える。
    /// </summary>
    public bool bIsActivePopupMenu
    {
        get;
        private set;
    }
    public virtual void tActivatePopupMenu(int nPlayer)
    {
        nItemSelecting = -1;        // #24757 2011.4.1 yyagi: Clear sorting status in each stating menu.
        this.bIsActivePopupMenu = true;
        this.bIsSelectingIntItem = false;
        this.bGotoDetailConfig = false;
    }
    public virtual void tDeativatePopupMenu()
    {
        this.bIsActivePopupMenu = false;
    }

    protected void Initialize(List<CItemBase> menulist, bool showAllItems, string title, int defaultPos)
    {
        ConditionallyInitializePrvFont();

        stqMenuTitle = new stQuickMenuItem();
        stqMenuTitle.cItem = new CItemBase();
        stqMenuTitle.cItem.strName = title;
        using (var bitmap = prvFont.DrawText(title, Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
        {
            stqMenuTitle.txName = TJAPlayer3.app.tCreateTexture(bitmap);
        }
        lciMenuItems = new stQuickMenuItem[menulist.Count];
        for (int i = 0; i < menulist.Count; i++)
        {
            stQuickMenuItem stqm = new stQuickMenuItem();
            stqm.cItem = menulist[i];
            using (var bitmap = prvFont.DrawText(menulist[i].strName, Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
            {
                stqm.txName = TJAPlayer3.app.tCreateTexture(bitmap);
            }
            lciMenuItems[i] = stqm;
        }

        bShowAllItems = showAllItems;
        n現在の選択行 = defaultPos;
    }

    private void ConditionallyInitializePrvFont()
    {
        if (prvFont is null)
        {
            prvFont = new CCachedFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 18);
        }
    }

    public void tPushedEnter()
    {
        if (this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音].t再生する();

            if (this.n現在の選択行 != lciMenuItems.Length - 1)
            {
                if (lciMenuItems[n現在の選択行].cItem.eItemType == CItemBase.EItemType.List ||
                        lciMenuItems[n現在の選択行].cItem.eItemType == CItemBase.EItemType.Toggle)
                {
                    lciMenuItems[n現在の選択行].cItem.tMoveItemValueToNext();
                }
                else if (lciMenuItems[n現在の選択行].cItem.eItemType == CItemBase.EItemType.Integer)
                {
                    bIsSelectingIntItem = !bIsSelectingIntItem;		// 選択状態/選択解除状態を反転する
                }
                else
                {
                    throw new ArgumentException();
                }
                nItemSelecting = n現在の選択行;
            }
            tPushedEnterMain((int)lciMenuItems[n現在の選択行].cItem.GetIndex());
        }
    }

    /// <summary>
    /// Decide押下時の処理を、継承先で記述する。
    /// </summary>
    /// <param name="val">CItemBaseの現在の設定値のindex</param>
    public virtual void tPushedEnterMain(int val)
    {
    }
    /// <summary>
    /// Cancel押下時の追加処理があれば、継承先で記述する。
    /// </summary>
    public virtual void tCancel()
    {
    }
    /// <summary>
    /// 追加の描画処理。必要に応じて、継承先で記述する。
    /// </summary>
    public virtual void t進行描画sub()
    {
    }

    public void t次に移動()
    {
        if (this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
            if (bIsSelectingIntItem)
            {
                lciMenuItems[n現在の選択行].cItem.tMoveItemValueToForward();		// 項目移動と数値上下は方向が逆になるので注意
            }
            else
            {
                if (++this.n現在の選択行 >= this.lciMenuItems.Length)
                {
                    this.n現在の選択行 = 0;
                }
            }
        }
    }
    public void t前に移動()
    {
        if (this.bキー入力待ち)
        {
            TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUNDカーソル移動音].t再生する();
            if (bIsSelectingIntItem)
            {
                lciMenuItems[n現在の選択行].cItem.tMoveItemValueToNext();		// 項目移動と数値上下は方向が逆になるので注意
            }
            else
            {
                if (--this.n現在の選択行 < 0)
                {
                    this.n現在の選択行 = this.lciMenuItems.Length - 1;
                }
            }
        }
    }

    // CActivity 実装

    public override void On活性化()
    {
        //		this.n現在の選択行 = 0;
        this.bキー入力待ち = true;
        for (int i = 0; i < 4; i++)
        {
            this.ctキー反復用[i] = new CCounter(0, 0, 0, TJAPlayer3.app.Timer);
        }

        this.bIsActivePopupMenu = false;
        nItemSelecting = -1;

        ConditionallyInitializePrvFont();

        base.On活性化();
    }
    public override void On非活性化()
    {
        if (!base.b活性化してない)
        {
            //CDTXMania.t安全にDisposeする( ref this.txCursor );
            //CDTXMania.t安全にDisposeする( ref this.txPopupMenuBackground );
            for (int i = 0; i < 4; i++)
            {
                this.ctキー反復用[i] = null;
            }
            TJAPlayer3.t安全にDisposeする(ref this.prvFont);
            base.On非活性化();
        }
    }

    public override int On進行描画()
    {
        throw new InvalidOperationException("t進行描画(bool)のほうを使用してください。");
    }

    public int t進行描画()
    {
        if (!base.b活性化してない && this.bIsActivePopupMenu)
        {
            if (this.bキー入力待ち)
            {
                #region [ キー入力: キャンセル ]
                if ((TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Escape))
                    && this.bEsc有効)
                {	// キャンセル
                    TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND取消音].t再生する();
                    tCancel();
                    this.bIsActivePopupMenu = false;
                }
                #endregion

                #region [ キー入力: 決定 ]
                ESortAction eAction = ESortAction.END;
                if (
                    TJAPlayer3.app.Pad.bPressed(EPad.LRed)
                    || TJAPlayer3.app.Pad.bPressed(EPad.RRed)
                    || (TJAPlayer3.app.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return)))
                {
                    eAction = ESortAction.Decide;
                }
                if (eAction == ESortAction.Decide)	// 決定
                {
                    this.tPushedEnter();
                }
                #endregion
                #region [ キー入力: 前に移動 ]
                this.ctキー反復用.Up.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.UpArrow), new CCounter.DGキー処理(this.t前に移動));
                if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue))
                {
                    this.t前に移動();
                }
                #endregion
                #region [ キー入力: 次に移動 ]
                this.ctキー反復用.Down.tキー反復(TJAPlayer3.app.InputManager.Keyboard.bIsKeyDown((int)SlimDXKeys.Key.DownArrow), new CCounter.DGキー処理(this.t次に移動));
                if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue))
                {
                    this.t次に移動();
                }
                #endregion
            }
            #region [ ポップアップメニュー 背景描画 ]
            if (TJAPlayer3.app.Tx.Menu_Title is not null)
            {
                TJAPlayer3.app.Tx.Menu_Title.t2D描画(TJAPlayer3.app.Device, 160, 40);
            }
            #endregion
            #region [ ソートメニュータイトル描画 ]
            int x = 210, y = 60;
            stqMenuTitle.txName?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Left, x, y);
            #endregion
            #region [ カーソル描画 ]
            if (TJAPlayer3.app.Tx.Menu_Highlight is not null)
            {
                int height = 32;
                int curX = 180;
                int curY = 46 + (height * (this.n現在の選択行 + 1));
                TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, curX, curY, new Rectangle(0, 0, 16, 32));
                curX += 0x10;
                Rectangle rectangle = new Rectangle(8, 0, 0x10, 0x20);
                for (int j = 0; j < 16; j++)
                {
                    TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, curX, curY, rectangle);
                    curX += 16;
                }
                TJAPlayer3.app.Tx.Menu_Highlight.t2D描画(TJAPlayer3.app.Device, curX, curY, new Rectangle(0x10, 0, 16, 32));
            }
            #endregion
            #region [ ソート候補文字列描画 ]
            for (int i = 0; i < lciMenuItems.Length; i++)
            {
                bool bItemBold = (i == nItemSelecting && !bShowAllItems) ? true : false;
                //font.t文字列描画( 190, 80 + i * 32, lciMenuItems[ i ].cItem.strName, bItemBold, 1.0f );
                if (lciMenuItems[i].txName is not null)
                {
                    lciMenuItems[i].txName?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Left, 180, 92 + i * 32);
                }

                bool bValueBold = (bItemBold || (i == nItemSelecting && bIsSelectingIntItem)) ? true : false;
                if (bItemBold || bShowAllItems)
                {
                    string s;
                    switch (lciMenuItems[i].cItem.strName)
                    {
                        case "演奏速度":
                            {
                                double d = (double)((int)lciMenuItems[i].cItem.objValue() / 20.0);
                                s = "x" + d.ToString("0.000");
                            }
                            break;
                        case "ばいそく":
                            {
                                double d = (double)((((int)lciMenuItems[i].cItem.objValue()) + 1) / 10.0);
                                s = "x" + d.ToString("0.0");
                            }
                            break;

                        default:
                            s = lciMenuItems[i].cItem.objValue().ToString();
                            break;
                    }
                    using (var bmpStr = bValueBold ?
                        prvFont?.DrawText(s, Color.White, Color.Black, Color.Yellow, Color.OrangeRed, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio) :
                        prvFont?.DrawText(s, Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio))
                    {
                        using (var ctStr = TJAPlayer3.app.tCreateTexture(bmpStr))
                        {
                            ctStr?.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Left, 330, 92 + i * 32);
                        }
                    }
                }
            }
            #endregion
            t進行描画sub();
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    protected bool bキー入力待ち;
    protected bool bEsc有効;

    internal int n現在の選択行;

    //private CTexture txPopupMenuBackground;
    //private CTexture txCursor;
    CCachedFontRenderer? prvFont;

    internal struct stQuickMenuItem
    {
        internal CItemBase cItem;
        internal CTexture? txName;
    }
    private stQuickMenuItem[] lciMenuItems;
    private stQuickMenuItem stqMenuTitle;
    private bool bShowAllItems;
    private bool bIsSelectingIntItem;

    [StructLayout(LayoutKind.Sequential)]
    private struct STキー反復用カウンタ
    {
        public CCounter Up;
        public CCounter Down;
        public CCounter R;
        public CCounter B;
        public CCounter this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.Up;

                    case 1:
                        return this.Down;

                    case 2:
                        return this.R;

                    case 3:
                        return this.B;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.Up = value;
                        return;

                    case 1:
                        this.Down = value;
                        return;

                    case 2:
                        this.R = value;
                        return;

                    case 3:
                        this.B = value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }
    }
    private STキー反復用カウンタ ctキー反復用;

    private enum ESortAction : int
    {
        Cancel, Decide, Previous, Next, END
    }
    private int nItemSelecting;		// 「n現在の選択行」とは別に設ける。sortでメニュー表示直後にアイテムの中身を表示しないようにするため
    //-----------------
    #endregion
}
