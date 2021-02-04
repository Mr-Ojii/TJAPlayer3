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
			base.e種別 = CItemBase.E種別.Integer;
			this.n最小値 = 0;
			this.n最大値 = 0;
			this.n現在の値 = 0;
			this.b値がフォーカスされている = false;
		}
		public CItemInteger( string str項目名, int n最小値, int n最大値, int n初期値 )
			: this()
		{
			this.tInitialize( str項目名, n最小値, n最大値, n初期値 );
		}
		public CItemInteger(string str項目名, int n最小値, int n最大値, int n初期値, string str説明文jp)
			: this() {
			this.tInitialize(str項目名, n最小値, n最大値, n初期値, str説明文jp);
		}
		public CItemInteger(string str項目名, int n最小値, int n最大値, int n初期値, string str説明文jp, string str説明文en)
			: this() {
			this.tInitialize(str項目名, n最小値, n最大値, n初期値, str説明文jp, str説明文en);
		}

		// CItemBase 実装

		public override void tPushedEnter()
		{
			this.b値がフォーカスされている = !this.b値がフォーカスされている;
		}
		public override void t項目値を次へ移動()
		{
			if( ++this.n現在の値 > this.n最大値 )
			{
				this.n現在の値 = this.n最大値;
			}
		}
		public override void t項目値を前へ移動()
		{
			if( --this.n現在の値 < this.n最小値 )
			{
				this.n現在の値 = this.n最小値;
			}
		}
	
		public void tInitialize( string str項目名, int n最小値, int n最大値, int n初期値 )
		{
			this.tInitialize( str項目名, n最小値, n最大値, n初期値, "", "" );
		}
		public void tInitialize(string str項目名, int n最小値, int n最大値, int n初期値, string str説明文jp) {
			this.tInitialize(str項目名, n最小値, n最大値, n初期値, str説明文jp, str説明文jp);
		}
		public void tInitialize(string str項目名, int n最小値, int n最大値, int n初期値, string str説明文jp, string str説明文en) {
			base.tInitialize(str項目名, str説明文jp, str説明文en);
			this.n最小値 = n最小値;
			this.n最大値 = n最大値;
			this.n現在の値 = n初期値;
			this.b値がフォーカスされている = false;
		}
		public override object obj現在値()
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
		private int n最小値;
		private int n最大値;
		//-----------------
		#endregion
	}
}
