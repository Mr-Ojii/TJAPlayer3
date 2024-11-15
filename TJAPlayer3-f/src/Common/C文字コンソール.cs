using FDK;

namespace TJAPlayer3;

internal class C文字コンソール : CActivity
{
    public C文字コンソール()
    {
        this.rc文字の矩形領域 = new Rectangle[3, str表記可能文字.Length];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < str表記可能文字.Length; j++)
            {
                const int regionX = 128, regionY = 16;
                this.rc文字の矩形領域[i, j].X = ((i / 2) * regionX) + ((j % regionY) * nFontWidth);
                this.rc文字の矩形領域[i, j].Y = ((i % 2) * regionX) + ((j / regionY) * nFontHeight);
                this.rc文字の矩形領域[i, j].Width = nFontWidth;
                this.rc文字の矩形領域[i, j].Height = nFontHeight;
            }
        }
    }

    // 定数

    public enum EFontType
    {
        白,
        赤,
        灰,
        白細,
        赤細,
        灰細
    }

    // メソッド

    public void tPrint(int x, int y, EFontType font, string str英数字文字列)
    {
        if (!base.b活性化してない && !string.IsNullOrEmpty(str英数字文字列))
        {
            int BOL = x;
            for (int i = 0; i < str英数字文字列.Length; i++)
            {
                char ch = str英数字文字列[i];
                if (ch == '\n')
                {
                    x = BOL;
                    y += nFontHeight;
                }
                else
                {
                    int index = str表記可能文字.IndexOf(ch);
                    if (index >= 0)
                        this.txフォント8x16[(int)((int)font / (int)EFontType.白細)]?.t2D描画(TJAPlayer3.app.Device, x, y, this.rc文字の矩形領域[(int)((int)font % (int)EFontType.白細), index]);
                    x += nFontWidth;
                }
            }
        }
    }


    // CActivity 実装
    public override void On活性化()
    {
        this.txフォント8x16[0] = TJAPlayer3.app.Tx.TxC(@"Console_Font.png");
        this.txフォント8x16[1] = TJAPlayer3.app.Tx.TxC(@"Console_Font_Small.png");
        base.On活性化();
    }
    public override void On非活性化()
    {
        for (int i = 0; i < 2; i++)
        {
            if (this.txフォント8x16[i] is not null)
            {
                this.txフォント8x16[i]?.Dispose();
                this.txフォント8x16[i] = null;
            }
        }
        base.On非活性化();
    }

    // その他

    #region [ private ]
    //-----------------
    private readonly Rectangle[,] rc文字の矩形領域;
    private const string str表記可能文字 = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ";
    private const int nFontWidth = 8, nFontHeight = 16;
    private CTexture?[] txフォント8x16 = new CTexture[2];
    //-----------------
    #endregion
}
