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
			Dictionary<string, int> Dic = TJAPlayer3.Skin.DictionaryList[order];

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
	}
}