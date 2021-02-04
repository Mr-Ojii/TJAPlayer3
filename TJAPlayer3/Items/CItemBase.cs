using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TJAPlayer3
{
	/// <summary>
	/// すべてのアイテムの基本クラス。
	/// </summary>
	internal class CItemBase
	{
		// プロパティ

		public E種別 e種別;
		public enum E種別
		{
			Base,
			Toggle,
			Integer,
			List
		}

		public string str項目名;
		public string str説明文;


		// コンストラクタ

		public CItemBase()
		{
			this.str項目名 = "";
			this.str説明文 = "";
		}
		public CItemBase( string str項目名 )
			: this()
		{
			this.tInitialize( str項目名 );
		}
		public CItemBase(string str項目名, string str説明文jp)
			: this() {
			this.tInitialize(str項目名, str説明文jp);
		}
		public CItemBase(string str項目名,  string str説明文jp, string str説明文en)
			: this() {
			this.tInitialize(str項目名, str説明文jp, str説明文en);
		}

		
		// メソッド；子クラスで実装する

		public virtual void tPushedEnter()
		{
		}
		public virtual void t項目値を次へ移動()
		{
		}
		public virtual void t項目値を前へ移動()
		{
		}

		public virtual void tInitialize( string str項目名 )
		{
			this.tInitialize(str項目名, "", "");
		}
		public virtual void tInitialize(string str項目名, string str説明文jp) {
			this.tInitialize(str項目名, str説明文jp, str説明文jp);
		}
		public virtual void tInitialize(string str項目名, string str説明文jp, string str説明文en) {
			this.str項目名 = str項目名;
			this.str説明文 = (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja") ? str説明文jp : str説明文en;
		}
		public virtual object obj現在値()
		{
			return null;
		}
		public virtual int GetIndex()
		{
			return 0;
		}
		public virtual void SetIndex( int index )
		{
		}
	}
}
