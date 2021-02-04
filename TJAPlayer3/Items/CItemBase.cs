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

		public EItemType eItemType;
		public enum EItemType
		{
			Base,
			Toggle,
			Integer,
			List
		}

		public string strName;
		public string str説明文;


		// コンストラクタ

		public CItemBase()
		{
			this.strName = "";
			this.str説明文 = "";
		}
		public CItemBase( string strName )
			: this()
		{
			this.tInitialize( strName );
		}
		public CItemBase(string strName, string str説明文jp)
			: this() {
			this.tInitialize(strName, str説明文jp);
		}
		public CItemBase(string strName,  string str説明文jp, string str説明文en)
			: this() {
			this.tInitialize(strName, str説明文jp, str説明文en);
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

		public virtual void tInitialize( string strName )
		{
			this.tInitialize(strName, "", "");
		}
		public virtual void tInitialize(string strName, string str説明文jp) {
			this.tInitialize(strName, str説明文jp, str説明文jp);
		}
		public virtual void tInitialize(string strName, string str説明文jp, string str説明文en) {
			this.strName = strName;
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
