using System;
using System.Collections.Generic;

namespace TJAPlayer3
{
	internal static class CStrGenreToNum
	{
		internal static int Genre(string strGenre, int order)
		{
			Dictionary<string, int> Dic = TJAPlayer3.Skin.DictionaryList[order];

			int maxvalue = -1;
			foreach (KeyValuePair<string, int> pair in Dic) 
			{
				maxvalue = Math.Max(pair.Value, maxvalue);
			}

			if (Dic.ContainsKey(strGenre))
			{
				return Dic[strGenre];
			}
			else
			{
				return maxvalue + 1;
			}
		}
	}
}