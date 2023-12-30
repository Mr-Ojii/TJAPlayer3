namespace TJAPlayer3;

internal class C曲リストノード
{
    // プロパティ

    public ENodeType eNodeType = ENodeType.UNKNOWN;
    public enum ENodeType
    {
        SCORE,
        BOX,
        BACKBOX,
        RANDOM,
        UNKNOWN
    }
    [JsonIgnore]
    public int nID { get; private set; }
    public Cスコア arスコア = new Cスコア();
    public Color ForeColor = Color.White;
    public Color BackColor = Color.Black;
    public List<C曲リストノード> list子リスト = new();
    public int nスコア数;
    [JsonIgnore]
    public C曲リストノード? r親ノード;
    [JsonIgnore]
    public int Openindex;
    public string strGenre = "";
    public string strTitle = "";
    public string strBreadcrumbs = "";		// #27060 2011.2.27 yyagi; MUSIC BOXのパンくずリスト (曲リスト構造内の絶対位置捕捉のために使う)

    // コンストラクタ

    public C曲リストノード()
    {
        this.nID = id++;
    }


    // その他

    #region [ private ]
    //-----------------
    private static int id;
    //-----------------
    #endregion
}
