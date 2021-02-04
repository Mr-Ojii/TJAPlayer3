using System;
using System.Collections.Generic;
using System.Text;

namespace TJAPlayer3
{
	/// <summary>
	/// 「トグル」（ON, OFF の2状態）を表すアイテム。
	/// </summary>
	internal class CItemToggle : CItemBase
	{
		// プロパティ

		public bool bON;

		
		// コンストラクタ

		public CItemToggle()
		{
			base.eItemType = CItemBase.EItemType.Toggle;
			this.bON = false;
		}
		public CItemToggle( string strName, bool bDefault )
			: this()
		{
			this.tInitialize( strName, bDefault );
		}
		public CItemToggle(string strName, bool bDefault, string str説明文jp)
			: this() {
			this.tInitialize(strName, bDefault, str説明文jp);
		}
		public CItemToggle(string strName, bool bDefault, string str説明文jp, string str説明文en)
			: this() {
			this.tInitialize(strName, bDefault, str説明文jp, str説明文en);
		}

		// CItemBase 実装

		public override void tPushedEnter()
		{
			this.t項目値を次へ移動();
		}
		public override void t項目値を次へ移動()
		{
			this.bON = !this.bON;
		}
		public override void t項目値を前へ移動()
		{
			this.t項目値を次へ移動();
		}

		public void tInitialize(string strName, bool bDefault)
		{
			this.tInitialize(strName, bDefault, "", "");
		}
		public void tInitialize(string strName, bool bDefault, string str説明文jp) {
			this.tInitialize(strName, bDefault, str説明文jp, str説明文jp);
		}
		public void tInitialize(string strName, bool bDefault, string str説明文jp, string str説明文en) {
			base.tInitialize(strName, str説明文jp, str説明文en);
			this.bON = bDefault;
		}
		public override object obj現在値()
		{
			return ( this.bON ) ? "ON" : "OFF";
		}
		public override int GetIndex()
		{
			return ( this.bON ) ? 1 : 0;
		}
		public override void SetIndex( int index )
		{
			switch ( index )
			{
				case 0:
					this.bON = false;
					break;
				case 1:
					this.bON = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
