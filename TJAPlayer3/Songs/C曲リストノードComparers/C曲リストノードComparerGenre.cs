using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TJAPlayer3.C曲リストノードComparers
{
	internal sealed class C曲リストノードComparerGenre : IComparer<C曲リストノード>
	{
		public C曲リストノードComparerGenre(int order)
		{
			this.order = order;
		}

		public int Compare(C曲リストノード n1, C曲リストノード n2)
		{
			return CStrジャンルtoNum.Genre(n1.strジャンル, order).CompareTo(CStrジャンルtoNum.Genre(n2.strジャンル, order));
		}

		private readonly int order;
	}
}