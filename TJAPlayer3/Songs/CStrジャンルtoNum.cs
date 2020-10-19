using System.Collections.Generic;

namespace TJAPlayer3
{
	internal static class CStrジャンルtoNum
	{
		internal static int ForAC8_14(string strジャンル)
		{
			if (ForAC8_14D.ContainsKey(strジャンル))
			{
				return ForAC8_14D[strジャンル];
			}
			else 
			{
				return 8;
			}
		}

		internal static int ForAC15(string strジャンル)
		{
			if (ForAC15D.ContainsKey(strジャンル))
			{
				return ForAC15D[strジャンル];
			}
			else
			{
				return 8;
			}
		}

		internal static Dictionary<string, int> ForAC8_14D = new Dictionary<string, int>
		{
			{ "アニメ",0 },
			{ "J-POP",1 },
			{ "ゲームミュージック",2 },
			{ "ナムコオリジナル",3 },
			{ "クラシック",4 },
			{ "どうよう",5 },
			{ "バラエティ",6 },
			{ "ボーカロイド",7 },
			{ "VOCALOID",7 },
		};

		internal static Dictionary<string, int> ForAC15D = new Dictionary<string, int>
		{
			{ "J-POP",0 },
			{ "アニメ",1 },
			{ "ボーカロイド",2 },
			{ "VOCALOID",2 },
			{ "どうよう",3 },
			{ "バラエティ",4 },
			{ "クラシック",5 },
			{ "ゲームミュージック",6 },
			{ "ナムコオリジナル",7 },
		};
	}
}