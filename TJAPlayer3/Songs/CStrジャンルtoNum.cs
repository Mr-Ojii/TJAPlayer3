using System;
using System.Collections.Generic;

namespace TJAPlayer3
{
	internal static class CStrジャンルtoNum
	{
		/// <summary>
		/// ジャンルソート用関数
		/// </summary>
		/// <param name="strジャンル"></param>
		/// <param name="order"></param>
		/// <returns></returns>
		internal static int Genre(string strジャンル, int order)
		{
			Dictionary<string, int> Dic = DictionaryList[order];

			int maxvalue = -1;
			foreach (KeyValuePair<string, int> pair in Dic) 
			{
				maxvalue = Math.Max(pair.Value, maxvalue);
			}

			if (Dic.ContainsKey(strジャンル))
			{
				return Dic[strジャンル];
			}
			else
			{
				return maxvalue + 1;
			}
		}

		internal static List<Dictionary<string, int>> DictionaryList = new List<Dictionary<string, int>>
		{
			//AC15
			new Dictionary<string, int>
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
			},
			//AC8_14
			new Dictionary<string, int>
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
			},
		};


	}
}