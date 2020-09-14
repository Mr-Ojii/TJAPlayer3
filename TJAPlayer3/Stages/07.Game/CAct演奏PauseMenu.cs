using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.IO;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏PauseMenu : CActSelectPopupMenu
	{
		private readonly string QuickCfgTitle = "ポーズ";
		// コンストラクタ

		public CAct演奏PauseMenu()
		{
			CAct演奏PauseMenuMain();
		}

		private void CAct演奏PauseMenuMain()
		{
			this.bEsc有効 = false;
			lci = new List<List<CItemBase>>();									// この画面に来る度に、メニューを作り直す。
			for ( int nConfSet = 0; nConfSet < 3; nConfSet++ )
			{
				lci.Add( new List<CItemBase>() );									// ConfSet用の3つ分の枠。

				lci[ nConfSet ].Add( null );										// Drum/Guitar/Bassで3つ分、枠を作っておく
				lci[ nConfSet ] = MakeListCItemBase( nConfSet );
				
			}
			base.Initialize( lci[ nCurrentConfigSet ], true, QuickCfgTitle, 2 );	// ConfSet=0, nInst=Drums
		}

		private List<CItemBase> MakeListCItemBase( int nConfigSet )
		{
			List<CItemBase> l = new List<CItemBase>();

			#region [ 共通 SET切り替え/More/Return ]
			l.Add( new CSwitchItemList( "続ける", CItemBase.Eパネル種別.通常, 0, "", "", new string[] { "" } ) );
			l.Add( new CSwitchItemList( "やり直し", CItemBase.Eパネル種別.通常, 0, "", "", new string[] { "" } ) );
			l.Add( new CSwitchItemList( "演奏中止", CItemBase.Eパネル種別.通常, 0, "", "", new string[] { "", "" } ) );
			#endregion

			return l;
		}

		// メソッド
		public override void tActivatePopupMenu(int nPlayer)
		{
			this.CAct演奏PauseMenuMain();
			this.bやり直しを選択した = false;
			base.tActivatePopupMenu(0);
		}
		//public void tDeativatePopupMenu()
		//{
		//	base.tDeativatePopupMenu();
		//}

		public override void t進行描画sub()
		{
			if( this.bやり直しを選択した )
			{
				TJAPlayer3.stage演奏ドラム画面.bPAUSE = false;
				TJAPlayer3.stage演奏ドラム画面.t演奏やりなおし();

				this.tDeativatePopupMenu();
			}
		}

		public override void tEnter押下Main( int nSortOrder )
		{
			switch ( n現在の選択行 )
			{
				case (int) EOrder.Continue:
					TJAPlayer3.stage演奏ドラム画面.bPAUSE = false;

					CSound管理.rc演奏用タイマ.t再開();
					TJAPlayer3.Timer.t再開();
					TJAPlayer3.DTX[0].t全チップの再生再開();
					TJAPlayer3.stage演奏ドラム画面.actAVI.tPauseControl();

					this.tDeativatePopupMenu();
					break;

				case (int) EOrder.Redoing:
					this.bやり直しを選択した = true;
					break;

				case (int) EOrder.Return:
					CSound管理.rc演奏用タイマ.t再開();
					TJAPlayer3.Timer.t再開();
					TJAPlayer3.stage演奏ドラム画面.t演奏中止();
					this.tDeativatePopupMenu();
					break;
				default:
					break;
			}
		}

		public override void tCancel()
		{
		}

		// CActivity 実装

		public override void On活性化()
		{
			base.On活性化();
			this.bGotoDetailConfig = false;
		}
		public override void On非活性化()
		{
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				string pathパネル本体 = CSkin.Path( @"Graphics\ScreenSelect popup auto settings.png" );
				if ( File.Exists( pathパネル本体 ) )
				{
					this.txパネル本体 = TJAPlayer3.tテクスチャの生成( pathパネル本体, true );
				}

				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if ( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする( ref this.txパネル本体 );
				TJAPlayer3.t安全にDisposeする( ref this.tx文字列パネル );
				base.OnManagedリソースの解放();
			}
		}

		#region [ private ]
		//-----------------
		private int nCurrentConfigSet = 0;
		private List<List<CItemBase>> lci;
		private enum EOrder : int
		{
			Continue,
			Redoing,
			Return, END,
			Default = 99
		};

		private CTexture txパネル本体;
		private CTexture tx文字列パネル;
		private bool bやり直しを選択した;
		//-----------------
		#endregion
	}


}
