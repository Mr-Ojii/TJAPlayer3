using System;
using System.Collections.Generic;
using System.Linq;

namespace TJAPlayer3
{
	internal static class CStrGenreToNum
	{
		internal static int Genre(string strGenre, int order)
		{
			Dictionary<string, int> Dic = TJAPlayer3.Skin.DictionaryList[order];

            int maxValue = Dic.Count != 0 ? Dic.Values.Max() : -1;

			if (Dic.TryGetValue(strGenre, out var value))
				return value;
			else
				return maxValue + 1;
		}
	}
}