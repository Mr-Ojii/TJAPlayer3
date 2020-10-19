using System.Collections.Generic;

namespace TJAPlayer3
{
	internal class CActSortSongs : CActSelectPopupMenu
	{

		// コンストラクタ

		public CActSortSongs()
		{
			List<CItemBase> lci = new List<CItemBase>();
			lci.Add( new CItemList( "絶対パス", 0, "", "", new string[] { "Z,Y,X,...",		"A,B,C,..." } ) );
			lci.Add( new CItemList( "曲名", 0, "", "", new string[] { "Z,Y,X,...",		"A,B,C,..." } ) );
			lci.Add( new CItemList( "ジャンル", 0, "", "", new string[] { "AC15",	"AC8-14" } ) );
			lci.Add( new CItemList( "戻る", 0, "", "", new string[] { "", 				"" } ) );
			
			base.Initialize( lci, false, "SORT MENU" , 0 );
		}


		// メソッド
		public void tActivatePopupMenu( ref CActSelect曲リスト ca )
		{
			this.act曲リスト = ca;
			base.tActivatePopupMenu(0);
		}
		//public void tDeativatePopupMenu()
		//{
		//	base.tDeativatePopupMenu();
		//}


		public override void tEnter押下Main( int nSortOrder )
		{
			switch ( (EOrder)n現在の選択行 )
			{
				case EOrder.Path:
					nSortOrder *= 2;    // 0,1  => -1, 1
					nSortOrder -= 1;
					this.act曲リスト.t曲リストのソート(
						CSongs管理.t曲リストのソート1_絶対パス順, nSortOrder
					);
					this.act曲リスト.t選択曲が変更された(true);
					break;
				case EOrder.Title:
					nSortOrder *= 2;    // 0,1  => -1, 1
					nSortOrder -= 1;
					this.act曲リスト.t曲リストのソート(
						CSongs管理.t曲リストのソート2_タイトル順, nSortOrder
					);
					this.act曲リスト.t選択曲が変更された(true);
					break;
				//ジャンル順
				case EOrder.Genre:
					this.act曲リスト.t曲リストのソート(
						CSongs管理.t曲リストのソート9_ジャンル順, nSortOrder
					);
					this.act曲リスト.t選択曲が変更された( true );
					break;
				case EOrder.Return:
					this.tDeativatePopupMenu();
					break;
				default:
					break;
			}
		}
		
		// CActivity 実装

		public override void On活性化()
		{
			//this.e現在のソート = EOrder.Title;
			base.On活性化();
		}
		public override void On非活性化()
		{
			if( !base.b活性化してない )
			{
				base.On非活性化();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			base.OnManagedリソースの解放();
		}

		#region [ private ]
		//-----------------

		private CActSelect曲リスト act曲リスト;

		private enum EOrder : int
		{
			Path = 0,
			Title = 1,
			Genre = 2,
			Return = 3
		}

		//-----------------
		#endregion
	}


}
