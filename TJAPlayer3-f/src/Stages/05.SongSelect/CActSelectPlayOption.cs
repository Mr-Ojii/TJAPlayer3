using FDK;

namespace TJAPlayer3;

internal class CActSelectPlayOption : CActivity
{
    // コンストラクタ

    private List<CItemBase> MakeListCItemBase(int nPlayer)
    {
        List<CItemBase> l = new List<CItemBase>();

        #region [ 個別 ScrollSpeed ]
        l.Add(new CItemInteger("ばいそく", 1, 2000, TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer],
            "演奏時のドラム譜面のスクロールの\n" +
            "速度を指定します。\n" +
            "x0.1 ～ x200.0 を指定可能です。",
            "To change the scroll speed for the\n" +
            "drums lanes.\n" +
            "You can set it from x0.1 to x200.0.\n" +
            "(ScrollSpeed=x0.5 means half speed)"));
        #endregion
        #region [ 共通 Dark/Risky/PlaySpeed ]
        if (nPlayer == 0)//1Pのときのみ表示
        {
            l.Add(new CItemInteger("演奏速度", 5, 400, TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed,
                "曲の演奏速度を、速くしたり遅くした\n" +
                "りすることができます。\n" +
                "（※一部のサウンドカードでは正しく\n" +
                "再生できない可能性があります。）",
                "It changes the song speed.\n" +
                "For example, you can play in half\n" +
                " speed by setting PlaySpeed = 0.500\n" +
                " for your practice.\n" +
                "Note: It also changes the songs' pitch."));

            #endregion
            #region [ 個別 Sud/Hid ]
            l.Add(new CItemList("ゲーム", (int)TJAPlayer3.app.ConfigToml.PlayOption._GameMode,
                "ゲームモード\n" +
                "TYPE-A: 完走!叩ききりまショー!\n" +
                "TYPE-B: 完走!叩ききりまショー!(激辛)\n" +
                " \n",
                " \n" +
                " \n" +
                " ",
                new string[] { "なし", "完走!", "完走!激辛", "特訓" }));

        }
        l.Add(new CItemList("ランダム", (int)TJAPlayer3.app.ConfigToml.PlayOption._Random[nPlayer],
            "いわゆるランダム。\n  RANDOM: ちょっと変わる\n  MIRROR: あべこべ \n  SUPER: そこそこヤバい\n  HYPER: 結構ヤバい\nなお、実装は適当な模様",
            "Drums chips come randomly.\n\n Part: swapping lanes randomly for each\n  measures.\n Super: swapping chip randomly\n Hyper: swapping randomly\n  (number of lanes also changes)",
            new string[] { "しない", "ちょこっと", "あべこべ", "きまぐれ", "でたらめ" }));
        l.Add(new CItemList("ドロン", (int)TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer],
            "",
            new string[] { "しない", "ドロン", "ステルス" }));

        l.Add(new CItemList("真打", TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer] ? 1 : 0, "", "", new string[] { "しない", "する" }));
        if (nPlayer == 0)
        {
            l.Add(new CItemInteger("プレイ人数", 1, 2, TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount,
                "プレイヤー人数",
                "PlayerCount"));
        }
        #endregion

        return l;
    }

    private List<ItemTextureList> MakeItemnameTexture(List<CItemBase> cItemBases)
    {
        List<ItemTextureList> textures = new List<ItemTextureList>();
        for (int i = 0; i < cItemBases.Count; i++)
        {
            CTexture NameTexture = TJAPlayer3.app.tCreateTexture(this.Font.DrawText(cItemBases[i].strName, Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio));
            CTexture[] ListTexture;
            if (cItemBases[i].eItemType == CItemBase.EItemType.List)
            {
                ListTexture = new CTexture[((CItemList)cItemBases[i]).list項目値.Count];
                for (int index = 0; index < ((CItemList)cItemBases[i]).list項目値.Count; index++)
                {
                    ListTexture[index] = TJAPlayer3.app.tCreateTexture(this.Font.DrawText(((CItemList)cItemBases[i]).list項目値[index], Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio));
                }
            }
            else
            {
                ListTexture = null;
            }
            textures.Add(new ItemTextureList(NameTexture, ListTexture));
        }
        return textures;
    }

    private void SaveValue(int nPlayer)
    {
        if (nPlayer == 0)
        {
            switch (NowRow[0])
            {
                case (int)EItemList1P.ScrollSpeed:
                    TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] = lci[nPlayer][(int)EItemList1P.ScrollSpeed].GetIndex();
                    break;
                case (int)EItemList1P.PlaySpeed:
                    TJAPlayer3.app.ConfigToml.PlayOption.PlaySpeed = lci[nPlayer][(int)EItemList1P.PlaySpeed].GetIndex();
                    break;
                case (int)EItemList1P.GameMode:
                    TJAPlayer3.app.ConfigToml.PlayOption._GameMode = (EGame)lci[nPlayer][(int)EItemList1P.GameMode].GetIndex();
                    break;
                case (int)EItemList1P.Random:
                    TJAPlayer3.app.ConfigToml.PlayOption._Random[nPlayer] = (ERandomMode)lci[nPlayer][(int)EItemList1P.Random].GetIndex();
                    break;
                case (int)EItemList1P.Stealth:
                    TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] = (EStealthMode)lci[nPlayer][(int)EItemList1P.Stealth].GetIndex();
                    break;
                case (int)EItemList1P.Shinuchi:
                    TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer] = lci[nPlayer][(int)EItemList1P.Shinuchi].GetIndex() == 1;
                    break;
                case (int)EItemList1P.PlayerCount:
                    TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount = lci[nPlayer][(int)EItemList1P.PlayerCount].GetIndex();
                    if (TJAPlayer3.app.ConfigToml.PlayOption.PlayerCount == 1)
                    {
                        this.tDeativatePopupMenu(1);
                        TJAPlayer3.stage選曲.actChangeSE.tDeativateChangeSE(1);
                    }
                    break;
            }
        }
        else if (nPlayer == 1)
        {
            switch (NowRow[1])
            {
                case (int)EItemList2P.ScrollSpeed:
                    TJAPlayer3.app.ConfigToml.PlayOption.ScrollSpeed[nPlayer] = lci[nPlayer][(int)EItemList2P.ScrollSpeed].GetIndex();
                    break;
                case (int)EItemList2P.Random:
                    TJAPlayer3.app.ConfigToml.PlayOption._Random[nPlayer] = (ERandomMode)lci[nPlayer][(int)EItemList2P.Random].GetIndex();
                    break;
                case (int)EItemList2P.Stealth:
                    TJAPlayer3.app.ConfigToml.PlayOption._Stealth[nPlayer] = (EStealthMode)lci[nPlayer][(int)EItemList2P.Stealth].GetIndex();
                    break;
                case (int)EItemList2P.Shinuchi:
                    TJAPlayer3.app.ConfigToml.PlayOption.Shinuchi[nPlayer] = lci[nPlayer][(int)EItemList2P.Shinuchi].GetIndex() == 1;
                    break;
            }
        }
    }

    private enum EItemList1P : int
    {
        ScrollSpeed = 0,
        PlaySpeed,
        GameMode,
        Random,
        Stealth,
        Shinuchi,
        PlayerCount
    }
    private enum EItemList2P : int
    {
        ScrollSpeed = 0,
        Random,
        Stealth,
        Shinuchi
    }

    // メソッド
    public void tActivatePopupMenu(int nPlayer)
    {
        if (ePhase[nPlayer] == EChangeSEPhase.Inactive)
        {
            this.NowRow[nPlayer] = 0;
            ePhase[nPlayer] = EChangeSEPhase.AnimationIn;
            ct登場退場アニメ用[nPlayer].t時間Reset();
            ct登場退場アニメ用[nPlayer].n現在の値 = 0;
        }
    }

    public void tDeativatePopupMenu(int nPlayer)
    {
        if (ePhase[nPlayer] == EChangeSEPhase.Active)
        {
            ePhase[nPlayer] = EChangeSEPhase.AnimationOut;
            ct登場退場アニメ用[nPlayer].t時間Reset();
            ct登場退場アニメ用[nPlayer].n現在の値 = 0;
        }
    }

    // CActivity 実装

    public override void On活性化()
    {
        this.Font = new CCachedFontRenderer(TJAPlayer3.app.ConfigToml.General.FontName, 20);
        lci = new List<CItemBase>[2];
        for (int nPlayer = 0; nPlayer < 2; nPlayer++)
        {
            lci[nPlayer] = MakeListCItemBase(nPlayer);
            NameTexture[nPlayer] = MakeItemnameTexture(lci[nPlayer]);
        }
        base.On活性化();
    }

    public override void On非活性化()
    {
        if (this.Font is not null)
            this.Font.Dispose();

        int count = NameTexture[0].Count;
        for (int index = 0; index < count; index++)
            NameTexture[0][index].Dispose();
        count = NameTexture[1].Count;
        for (int index = 0; index < count; index++)
            NameTexture[1][index].Dispose();

        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (base.b初めての進行描画)
        {
            base.b初めての進行描画 = false;
        }

        for (int nPlayer = 0; nPlayer < 2; nPlayer++)
        {
            this.ct登場退場アニメ用[nPlayer].t進行();
            if (this.ePhase[nPlayer] == EChangeSEPhase.Active)
            {
                this.tDrawBox(TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxX[nPlayer], TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxY[nPlayer], nPlayer);
            }
            else if (this.ePhase[nPlayer] == EChangeSEPhase.AnimationIn)
            {
                int y = (int)((TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxY[nPlayer]) + (float)((Math.Sin(this.ct登場退場アニメ用[nPlayer].n現在の値 / 100.0) - 0.95) * -500f));
                this.tDrawBox(TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxX[nPlayer], y, nPlayer);

                if (this.ct登場退場アニメ用[nPlayer].b終了値に達した)
                    this.ePhase[nPlayer] = EChangeSEPhase.Active;
            }
            else if (this.ePhase[nPlayer] == EChangeSEPhase.AnimationOut)
            {
                int y = (int)((TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxY[nPlayer]) + (float)((Math.Sin((this.ct登場退場アニメ用[nPlayer].n終了値 - this.ct登場退場アニメ用[nPlayer].n現在の値) / 100.0) - 0.95) * -500f));
                this.tDrawBox(TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxX[nPlayer], y, nPlayer);

                if (this.ct登場退場アニメ用[nPlayer].b終了値に達した)
                    this.ePhase[nPlayer] = EChangeSEPhase.Inactive;
            }
        }

        if (this.ePhase[0] == EChangeSEPhase.Active)
        {
            if (TJAPlayer3.app.Pad.bPressed(EPad.LRed) || TJAPlayer3.app.Pad.bPressed(EPad.RRed))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                NowRow[0]++;
                if (NowRow[0] >= lci[0].Count)
                    tDeativatePopupMenu(0);
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                lci[0][NowRow[0]].tMoveItemValueToForward();
                SaveValue(0);
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue) || TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                lci[0][NowRow[0]].tMoveItemValueToNext();
                SaveValue(0);
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.UpArrow))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                NowRow[0]--;
                if (NowRow[0] < 0)
                    NowRow[0] = lci[0].Count - 1;
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.DownArrow))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                NowRow[0]++;
                if (NowRow[0] >= lci[0].Count)
                    NowRow[0] = 0;
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                tDeativatePopupMenu(0);
            }
        }

        if (this.ePhase[1] == EChangeSEPhase.Active)
        {
            if (TJAPlayer3.app.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.app.Pad.bPressed(EPad.RRed2P))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                NowRow[1]++;
                if (NowRow[1] >= lci[1].Count)
                    tDeativatePopupMenu(1);
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.LBlue2P) || (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.LeftArrow) && TJAPlayer3.stage選曲.actDifficultySelect.選択済み[0]))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                lci[1][NowRow[1]].tMoveItemValueToForward();
                SaveValue(1);
            }
            if (TJAPlayer3.app.Pad.bPressed(EPad.RBlue2P) || (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.RightArrow) && TJAPlayer3.stage選曲.actDifficultySelect.選択済み[0]))
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                lci[1][NowRow[1]].tMoveItemValueToNext();
                SaveValue(1);
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.UpArrow) && TJAPlayer3.stage選曲.actDifficultySelect.選択済み[0])
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                NowRow[1]--;
                if (NowRow[1] < 0)
                    NowRow[1] = lci[1].Count - 1;
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.DownArrow) && TJAPlayer3.stage選曲.actDifficultySelect.選択済み[0])
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND変更音]?.t再生する();
                NowRow[1]++;
                if (NowRow[1] >= lci[1].Count)
                    NowRow[1] = 0;
            }
            if (TJAPlayer3.app.InputManager.Keyboard.bIsKeyPressed((int)SlimDXKeys.Key.Return) && TJAPlayer3.stage選曲.actDifficultySelect.選択済み[0])
            {
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.SOUND決定音]?.t再生する();
                tDeativatePopupMenu(1);
            }
        }

        return base.On進行描画();
    }


    /// <param name="x">下中央基準のX</param>
    /// <param name="y">下中央基準のY</param>
    private void tDrawBox(float x, float y, int nPlayer)
    {
        if (TJAPlayer3.app.Tx.PlayOption_List is not null && TJAPlayer3.app.Tx.PlayOption_Active is not null)
        {
            TJAPlayer3.app.Tx.PlayOption_List.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x, y, new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2], TJAPlayer3.app.Tx.PlayOption_List.szTextureSize.Width, TJAPlayer3.app.Tx.PlayOption_List.szTextureSize.Height - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2]));//下部
            y -= TJAPlayer3.app.Tx.PlayOption_List.szTextureSize.Height - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2];
        }
        else
        {
            y -= 100;
        }

        for (int i = 0; i < lci[nPlayer].Count; i++)
        {
            if (TJAPlayer3.app.Tx.PlayOption_List is not null && TJAPlayer3.app.Tx.PlayOption_Active is not null)
            {
                TJAPlayer3.app.Tx.PlayOption_List.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x, y, new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1], TJAPlayer3.app.Tx.PlayOption_List.szTextureSize.Width, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]));//リスト本体
                if (lci[nPlayer].Count - i == NowRow[nPlayer] + 1)
                    TJAPlayer3.app.Tx.PlayOption_Active.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x, y);
            }

            this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemNameTexture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionNameCorrectionX, y - (TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionY);

            if (lci[nPlayer][lci[nPlayer].Count - i - 1].strName.Equals("ばいそく"))
            {
                using (CTexture texture = TJAPlayer3.app.tCreateTexture(this.Font.DrawText((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() * 0.1).ToString("0.0"), Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio)))
                {
                    texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionX - texture.szTextureSize.Width / 2, y - (TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionY);
                }
            }
            else if (lci[nPlayer][lci[nPlayer].Count - i - 1].strName.Equals("演奏速度"))
            {
                using (CTexture texture = TJAPlayer3.app.tCreateTexture(this.Font.DrawText((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex() * 0.05).ToString("0.00"), Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio)))
                {
                    texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionX - texture.szTextureSize.Width / 2, y - (TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionY);
                }
            }
            else if (lci[nPlayer][lci[nPlayer].Count - i - 1].eItemType == CItemBase.EItemType.Integer)
            {
                using (CTexture texture = TJAPlayer3.app.tCreateTexture(this.Font.DrawText((lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()).ToString(), Color.White, Color.Black, TJAPlayer3.app.Skin.SkinConfig.Font.EdgeRatio)))
                {
                    texture.t2D描画(TJAPlayer3.app.Device, x + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionX - texture.szTextureSize.Width / 2, y - (TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionY);
                }
            }
            else if (lci[nPlayer][lci[nPlayer].Count - i - 1].eItemType == CItemBase.EItemType.List)
            {
                this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].t2D描画(TJAPlayer3.app.Device, x + 90 - this.NameTexture[nPlayer][lci[nPlayer].Count - i - 1].ItemListTexture[lci[nPlayer][lci[nPlayer].Count - i - 1].GetIndex()].szTextureSize.Width / 2, y - (TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1]) + TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionListCorrectionY);
            }

            y -= TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[2] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1];
        }

        if (TJAPlayer3.app.Tx.PlayOption_List is not null && TJAPlayer3.app.Tx.PlayOption_Active is not null)
        {
            TJAPlayer3.app.Tx.PlayOption_List.t2D拡大率考慮描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, x, y, new Rectangle(0, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[0], TJAPlayer3.app.Tx.PlayOption_List.szTextureSize.Width, TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[1] - TJAPlayer3.app.Skin.SkinConfig.SongSelect.Difficulty.PlayOptionBoxSectionY[0]));//上部
        }

    }

    public bool[] bIsActive
    {
        get
        {
            return new bool[] { (ePhase[0] != EChangeSEPhase.Inactive), (ePhase[1] != EChangeSEPhase.Inactive) };
        }
    }

    #region [ private ]
    //-----------------
    private class ItemTextureList : IDisposable
    {
        public CTexture ItemNameTexture;
        public CTexture[] ItemListTexture;

        public ItemTextureList(CTexture NameTexture, CTexture[] ListTexture)
        {
            this.ItemNameTexture = NameTexture;
            this.ItemListTexture = ListTexture;
        }

        public void Dispose()
        {

            this.ItemNameTexture?.Dispose();
            if (this.ItemListTexture is not null)
            {
                int count = this.ItemListTexture.Length;
                for (int index = 0; index < count; index++)
                    ItemListTexture[index]?.Dispose();
            }
        }
    }

    private CCounter[] ct登場退場アニメ用 = { new CCounter(0, 188, 2, TJAPlayer3.app.Timer), new CCounter(0, 188, 2, TJAPlayer3.app.Timer) };//Math.PI-Math.Asin(0.95)
    private EChangeSEPhase[] ePhase = { EChangeSEPhase.Inactive, EChangeSEPhase.Inactive };
    private int[] NowRow = { 0, 0 };
    private List<ItemTextureList>[] NameTexture = new List<ItemTextureList>[2];
    private List<CItemBase>[] lci;
    private CCachedFontRenderer Font;

    private enum EChangeSEPhase
    {
        Inactive,
        AnimationIn,
        Active,
        AnimationOut
    }
    //-----------------
    #endregion
}
