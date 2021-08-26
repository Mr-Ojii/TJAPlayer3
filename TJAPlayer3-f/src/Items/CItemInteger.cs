using System;
using System.Collections.Generic;
using System.Text;

namespace TJAPlayer3
{
	/// <summary>
	/// 「整数」を表すアイテム。
	/// </summary>
	internal class CItemInteger : CItemBase
	{
		// プロパティ

		public int n現在の値;
		public bool b値がフォーカスされている;


		// コンストラクタ

		public CItemInteger()
		{
			base.eItemType = CItemBase.EItemType.Integer;
			this.nMin = 0;
			this.nMax = 0;
			this.n現在の値 = 0;
			this.b値がフォーカスされている = false;
		}
		public CItemInteger( string strName, int nMin, int nMax, int nDefaultNum )
			: this()
		{
			this.tInitialize( strName, nMin, nMax, nDefaultNum );
		}
		public CItemInteger(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP)
			: this() {
			this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP);
		}
		public CItemInteger(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP, string strDescriptionEN)
			: this() {
			this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP, strDescriptionEN);
		}

		// CItemBase 実装

		public override void tPushedEnter()
		{
			this.b値がフォーカスされている = !this.b値がフォーカスされている;
		}
		public override void tMoveItemValueToNext()
		{
			if( ++this.n現在の値 > this.nMax )
			{
				this.n現在の値 = this.nMax;
			}
		}
		public override void tMoveItemValueToForward()
		{
			if( --this.n現在の値 < this.nMin )
			{
				this.n現在の値 = this.nMin;
			}
		}
	
		public void tInitialize( string strName, int nMin, int nMax, int nDefaultNum )
		{
			this.tInitialize( strName, nMin, nMax, nDefaultNum, "", "" );
		}
		public void tInitialize(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP) {
			this.tInitialize(strName, nMin, nMax, nDefaultNum, strDescriptionJP, strDescriptionJP);
		}
		public void tInitialize(string strName, int nMin, int nMax, int nDefaultNum, string strDescriptionJP, string strDescriptionEN) {
			base.tInitialize(strName, strDescriptionJP, strDescriptionEN);
			this.nMin = nMin;
			this.nMax = nMax;
			this.n現在の値 = nDefaultNum;
			this.b値がフォーカスされている = false;
		}
		public override object objValue()
		{
			return this.n現在の値;
		}
		public override int GetIndex()
		{
			return this.n現在の値;
		}
		public override void SetIndex( int index )
		{
			this.n現在の値 = index;
		}
		// その他

		#region [ private ]
		//-----------------
		private int nMin;
		private int nMax;
		//-----------------
		#endregion
	}
}
