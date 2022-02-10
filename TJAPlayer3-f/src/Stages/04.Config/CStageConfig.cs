using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using FDK;
using DiscordRPC;

namespace TJAPlayer3
{
	internal class CStageConfig : CStage
	{
		// コンストラクタ

		public CStageConfig()
		{
			base.eStageID = CStage.EStage.Config;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.list子Activities.Add( this.actFIFO = new CActFIFOBlack() );
			base.list子Activities.Add( this.actList = new CActConfigList() );
			base.list子Activities.Add( this.actKeyAssign = new CActConfigKeyAssign() );
			base.b活性化してない = true;
		}
		
		
		// メソッド

		public void tアサイン完了通知()															// CONFIGにのみ存在
		{																						//
			this.eItemPanelモード = EItemPanelモード.パッド一覧;								//
		}																						//
		public void tパッド選択通知( EKeyConfigPad pad )					//
		{																						//
			this.actKeyAssign.t開始( pad, this.actList.ib現在の選択項目.strName );		//
			this.eItemPanelモード = EItemPanelモード.KeyCode一覧;							//
		}																						//
		public void t項目変更通知()																// OPTIONと共通
		{																						//
			this.t説明文パネルに現在選択されている項目の説明を描画する();						//
		}																						//

		
		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "コンフィグステージを活性化します。" );
			Trace.Indent();
			try
			{
				this.n現在のメニュー番号 = 0;                                                    //

				this.privatefont = new CCachedFontRenderer(TJAPlayer3.ConfigIni.FontName, 14, SixLabors.Fonts.FontStyle.Bold);

				for( int i = 0; i < 4; i++ )													//
				{																				//
					this.ctキー反復用[ i ] = new CCounter( 0, 0, 0, TJAPlayer3.Timer );			//
				}																				//
				this.bメニューにフォーカス中 = true;											// ここまでOPTIONと共通
				this.eItemPanelモード = EItemPanelモード.パッド一覧;
				TJAPlayer3.DiscordClient?.SetPresence(new RichPresence()
				{
					Details = "",
					State = "Config",
					Timestamps = new Timestamps(TJAPlayer3.StartupTime),
					Assets = new Assets()
					{
						LargeImageKey = TJAPlayer3.LargeImageKey,
						LargeImageText = TJAPlayer3.LargeImageText,
					}
				});
			}
			finally
			{
				Trace.TraceInformation( "コンフィグステージの活性化を完了しました。" );
				Trace.Unindent();
			}
			base.On活性化();		// 2011.3.14 yyagi: On活性化()をtryの中から外に移動
		}
		public override void On非活性化()
		{
			Trace.TraceInformation( "コンフィグステージを非活性化します。" );
			Trace.Indent();
			try
			{
				TJAPlayer3.ConfigIni.t書き出し( TJAPlayer3.strEXEのあるフォルダ + "Config.ini" );	// CONFIGだけ
				if (this.privatefont != null)                                                    // 以下OPTIONと共通
				{
					this.privatefont.Dispose();
					this.privatefont = null;
				}
				for ( int i = 0; i < 4; i++ )
				{
					this.ctキー反復用[ i ] = null;
				}
				base.On非活性化();
			}
			catch ( UnauthorizedAccessException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "ファイルが読み取り専用になっていないか、管理者権限がないと書き込めなくなっていないか等を確認して下さい" );
				Trace.TraceError( "An exception has occurred, but processing continues." );
			}
			catch ( Exception e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "An exception has occurred, but processing continues." );
			}
			finally
			{
				Trace.TraceInformation( "コンフィグステージの非活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()											// OPTIONと画像以外共通
		{
			if( !base.b活性化してない )
			{
				string[] strMenuItem = {"System", "Drums", "Exit"};
				txMenuItemLeft = new CTexture[strMenuItem.Length, 2];
				using (var prvFont = new CFontRenderer(TJAPlayer3.ConfigIni.FontName, 20))
				{
					for (int i = 0; i < strMenuItem.Length; i++)
					{
						using (var bmpStr = prvFont.DrawText(strMenuItem[i], Color.White, Color.Black, TJAPlayer3.Skin.Font_Edge_Ratio))
						{
							txMenuItemLeft[i, 0] = TJAPlayer3.tCreateTexture(bmpStr);
						}
						using (var bmpStr = prvFont.DrawText(strMenuItem[i], Color.White, Color.Black, Color.Yellow, Color.OrangeRed, TJAPlayer3.Skin.Font_Edge_Ratio))
						{
							txMenuItemLeft[i, 1] = TJAPlayer3.tCreateTexture(bmpStr);
						}
					}
				}

				if( this.bメニューにフォーカス中 )
				{
					this.t説明文パネルに現在選択されているメニューの説明を描画する();
				}
				else
				{
					this.t説明文パネルに現在選択されている項目の説明を描画する();
				}
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()											// OPTIONと同じ(COnfig.iniの書き出しタイミングのみ異なるが、無視して良い)
		{
			if( !base.b活性化してない )
			{
				//CDTXMania.t安全にDisposeする( ref this.tx背景 );
				//CDTXMania.t安全にDisposeする( ref this.tx上部パネル );
				//CDTXMania.t安全にDisposeする( ref this.tx下部パネル );
				//CDTXMania.t安全にDisposeする( ref this.txMenuカーソル );
				TJAPlayer3.t安全にDisposeする( ref this.tx説明文パネル );
				for ( int i = 0; i < txMenuItemLeft.GetLength( 0 ); i++ )
				{
					txMenuItemLeft[ i, 0 ].Dispose();
					txMenuItemLeft[ i, 0 ] = null;
					txMenuItemLeft[ i, 1 ].Dispose();
					txMenuItemLeft[ i, 1 ] = null;
				}
				txMenuItemLeft = null;
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( base.b活性化してない )
				return 0;

			if( base.b初めての進行描画 )
			{
				base.eフェーズID = CStage.Eフェーズ.共通_FadeIn;
				this.actFIFO.tFadeIn開始();
				base.b初めての進行描画 = false;
			}

			// 描画

			#region [ 背景 ]
			//---------------------
			if(TJAPlayer3.Tx.Config_Background != null )
				TJAPlayer3.Tx.Config_Background.t2D描画( TJAPlayer3.app.Device, 0, 0 );
			//---------------------
			#endregion
			#region [ メニューカーソル ]
			//---------------------
			if( TJAPlayer3.Tx.Config_Cursor != null )
			{
				Rectangle rectangle;
				TJAPlayer3.Tx.Config_Cursor.Opacity = this.bメニューにフォーカス中 ? 255 : 128;
				int x = 110;
				int y = (int)( 140 + ( this.n現在のメニュー番号 * 38 ) );
				int num3 = 340;
				TJAPlayer3.Tx.Config_Cursor.t2D描画( TJAPlayer3.app.Device, x, y, new Rectangle( 0, 0, 32, 48 ) );
				TJAPlayer3.Tx.Config_Cursor.t2D描画( TJAPlayer3.app.Device, ( x + num3 ) - 32, y, new Rectangle( 20, 0, 32, 48 ) );
				x += 32;
				for( num3 -= 64; num3 > 0; num3 -= rectangle.Width )
				{
					rectangle = new Rectangle( 16, 0, 32, 48 );
					if( num3 < 32 )
					{
						rectangle.Width -= 32 - num3;
					}
					TJAPlayer3.Tx.Config_Cursor.t2D描画( TJAPlayer3.app.Device, x, y, rectangle );
					x += rectangle.Width;
				}
			}
			//---------------------
			#endregion
			#region [ メニュー ]
			//---------------------
			int menuY = 162 - 22;
			int stepY = 39;
			for ( int i = 0; i < txMenuItemLeft.GetLength( 0 ); i++ )
			{
				int flag = ( this.n現在のメニュー番号 == i ) ? 1 : 0;
				int num4 = txMenuItemLeft[ i, flag ].szTextureSize.Width;
				txMenuItemLeft[i, flag].t2D描画(TJAPlayer3.app.Device, 282 - (num4 / 2) + TJAPlayer3.Skin.Config_ItemText_Correction_X, menuY + TJAPlayer3.Skin.Config_ItemText_Correction_Y ); //55
				menuY += stepY;
			}
			//---------------------
			#endregion
			#region [ 説明文パネル ]
			//---------------------
			if( this.tx説明文パネル != null )
				this.tx説明文パネル.t2D描画( TJAPlayer3.app.Device, 67, 382 );
			//---------------------
			#endregion
			#region [ アイテム ]
			//---------------------
			switch( this.eItemPanelモード )
			{
				case EItemPanelモード.パッド一覧:
					this.actList.t進行描画( !this.bメニューにフォーカス中 );
					break;

				case EItemPanelモード.KeyCode一覧:
					this.actKeyAssign.On進行描画();
					break;
			}
			//---------------------
			#endregion
			//#region [ 上部パネル ]
			////---------------------
			//if( this.tx上部パネル != null )
			//	this.tx上部パネル.t2D描画( CDTXMania.app.Device, 0, 0 );
			////---------------------
			//#endregion
			//#region [ 下部パネル ]
			////---------------------
			//if( this.tx下部パネル != null )
			//	this.tx下部パネル.t2D描画( CDTXMania.app.Device, 0, 720 - this.tx下部パネル.szTextureSize.Height );
			////---------------------
			//#endregion
			#region [ オプションパネル ]
			//---------------------
			//this.actオプションパネル.On進行描画();
			//---------------------
			#endregion
			#region [ FadeIn_アウト ]
			//---------------------
			switch( base.eフェーズID )
			{
				case CStage.Eフェーズ.共通_FadeIn:
					if( this.actFIFO.On進行描画() != 0 )
					{
						TJAPlayer3.Skin.bgmコンフィグ画面.t再生する();
						base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
					}
					break;

				case CStage.Eフェーズ.共通_FadeOut:
					if( this.actFIFO.On進行描画() == 0 )
					{
						break;
					}
					return 1;
			}
			//---------------------
			#endregion

			#region [ Enumerating Songs ]
			// CActEnumSongs側で表示する
			#endregion

			// キー入力

			if( ( base.eフェーズID != CStage.Eフェーズ.共通_通常状態 )
				|| this.actKeyAssign.bキー入力待ちの最中である )
				return 0;

			// 曲データの一覧取得中は、キー入力を無効化する
			if ( !TJAPlayer3.EnumSongs.IsEnumerating || TJAPlayer3.actEnumSongs.bコマンドでの曲データ取得 != true )
			{
				if ( ( TJAPlayer3.InputManager.Keyboard.bIsKeyPressed( (int)SlimDXKeys.Key.Escape )))
				{
					TJAPlayer3.Skin.sound取消音.t再生する();
					if ( !this.bメニューにフォーカス中 )
					{
						if ( this.eItemPanelモード == EItemPanelモード.KeyCode一覧 )
						{
							TJAPlayer3.stageConfig.tアサイン完了通知();
							return 0;
						}
						if ( !this.actList.bIsKeyAssignSelected && !this.actList.bIsFocusingParameter )	// #24525 2011.3.15 yyagi, #32059 2013.9.17 yyagi
						{
							this.bメニューにフォーカス中 = true;
						}
						this.t説明文パネルに現在選択されているメニューの説明を描画する();
						this.actList.tEsc押下();								// #24525 2011.3.15 yyagi ESC押下時の右メニュー描画用
					}
					else
					{
						this.actFIFO.tFadeOut開始();
						base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
					}
				}
				else if (TJAPlayer3.ConfigIni.bEnterがキー割り当てのどこにも使用されていない && TJAPlayer3.InputManager.Keyboard.bIsKeyPressed( (int)SlimDXKeys.Key.Return ) || TJAPlayer3.Pad.bPressed(EPad.LRed) || TJAPlayer3.Pad.bPressed(EPad.RRed) || (TJAPlayer3.Pad.bPressed(EPad.LRed2P) || TJAPlayer3.Pad.bPressed(EPad.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2)
				{
					if ( this.n現在のメニュー番号 == 2 )
					{
						TJAPlayer3.Skin.sound決定音.t再生する();
						this.actFIFO.tFadeOut開始();
						base.eフェーズID = CStage.Eフェーズ.共通_FadeOut;
					}
					else if ( this.bメニューにフォーカス中 )
					{
						TJAPlayer3.Skin.sound決定音.t再生する();
						this.bメニューにフォーカス中 = false;
						this.t説明文パネルに現在選択されている項目の説明を描画する();
					}
					else
					{
						switch ( this.eItemPanelモード )
						{
							case EItemPanelモード.パッド一覧:
								bool bIsKeyAssignSelectedBeforeHitEnter = this.actList.bIsKeyAssignSelected;	// #24525 2011.3.15 yyagi
								this.actList.tPushedEnter();
								if ( this.actList.b現在選択されている項目はReturnToMenuである )
								{
									this.t説明文パネルに現在選択されているメニューの説明を描画する();
									if ( bIsKeyAssignSelectedBeforeHitEnter == false )							// #24525 2011.3.15 yyagi
									{
										this.bメニューにフォーカス中 = true;
									}
								}
								break;

							case EItemPanelモード.KeyCode一覧:
								this.actKeyAssign.tPushedEnter();
								break;
						}
					}
				}
				
				if (this.actList.b要素値にフォーカス中)
				{
					if (TJAPlayer3.Pad.bPressed(EPad.RBlue) || TJAPlayer3.Pad.bPressed(EPad.RBlue2P) && TJAPlayer3.ConfigIni.nPlayerCount >= 2)
					{
						this.tカーソルを上へ移動する();
					}
					if (TJAPlayer3.Pad.bPressed(EPad.LBlue) || TJAPlayer3.Pad.bPressed(EPad.LBlue2P) && TJAPlayer3.ConfigIni.nPlayerCount >= 2)
					{
						this.tカーソルを下へ移動する();
					}
				}
				else
				{
					if (TJAPlayer3.Pad.bPressed(EPad.RBlue) || TJAPlayer3.Pad.bPressed(EPad.RBlue2P) && TJAPlayer3.ConfigIni.nPlayerCount >= 2)
					{
						this.tカーソルを下へ移動する();
					}
					if (TJAPlayer3.Pad.bPressed(EPad.LBlue) || TJAPlayer3.Pad.bPressed(EPad.LBlue2P) && TJAPlayer3.ConfigIni.nPlayerCount >= 2)
					{
						this.tカーソルを上へ移動する();
					}
				}


				this.ctキー反復用.Up.tキー反復( TJAPlayer3.InputManager.Keyboard.bIsKeyDown( (int)SlimDXKeys.Key.UpArrow ) , new CCounter.DGキー処理( this.tカーソルを上へ移動する ) );

				this.ctキー反復用.Down.tキー反復( TJAPlayer3.InputManager.Keyboard.bIsKeyDown( (int)SlimDXKeys.Key.DownArrow ), new CCounter.DGキー処理( this.tカーソルを下へ移動する ) );

			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private enum EItemPanelモード
		{
			パッド一覧,
			KeyCode一覧
		}

		[StructLayout( LayoutKind.Sequential )]
		private struct STキー反復用カウンタ
		{
			public CCounter Up;
			public CCounter Down;
			public CCounter R;
			public CCounter B;
			public CCounter this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.Up;

						case 1:
							return this.Down;

						case 2:
							return this.R;

						case 3:
							return this.B;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.Up = value;
							return;

						case 1:
							this.Down = value;
							return;

						case 2:
							this.R = value;
							return;

						case 3:
							this.B = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}

		private CActFIFOBlack actFIFO;
		private CActConfigKeyAssign actKeyAssign;
		private CActConfigList actList;
		private bool bメニューにフォーカス中;
		private STキー反復用カウンタ ctキー反復用;
		private const int DESC_H = 0x80;
		private const int DESC_W = 220;
		private EItemPanelモード eItemPanelモード;
		private CCachedFontRenderer privatefont;
		private int n現在のメニュー番号;
		//private CTexture txMenuカーソル;
		//private CTexture tx下部パネル;
		//private CTexture tx上部パネル;
		private CTexture tx説明文パネル;
		//private CTexture tx背景;
		private CTexture[ , ] txMenuItemLeft;

		private void tカーソルを下へ移動する()
		{
			if( !this.bメニューにフォーカス中 )
			{
				switch( this.eItemPanelモード )
				{
					case EItemPanelモード.パッド一覧:
						this.actList.t次に移動();
						return;

					case EItemPanelモード.KeyCode一覧:
						this.actKeyAssign.t次に移動();
						return;
				}
			}
			else
			{
				TJAPlayer3.Skin.soundカーソル移動音.t再生する();
				this.n現在のメニュー番号 = ( this.n現在のメニュー番号 + 1 ) % 3;
				switch( this.n現在のメニュー番号 )
				{
					case 0:
						this.actList.t項目リストの設定_System();
						break;

					case 1:
						this.actList.t項目リストの設定_Drums();
						break;

					case 2:
						this.actList.t項目リストの設定_Exit();
						break;
				}
				this.t説明文パネルに現在選択されているメニューの説明を描画する();
			}
		}
		private void tカーソルを上へ移動する()
		{
			if( !this.bメニューにフォーカス中 )
			{
				switch( this.eItemPanelモード )
				{
					case EItemPanelモード.パッド一覧:
						this.actList.t前に移動();
						return;

					case EItemPanelモード.KeyCode一覧:
						this.actKeyAssign.t前に移動();
						return;
				}
			}
			else
			{
				TJAPlayer3.Skin.soundカーソル移動音.t再生する();
				this.n現在のメニュー番号 = ( (this.n現在のメニュー番号 - 1 )+ 3) % 3;
				switch( this.n現在のメニュー番号 )
				{
					case 0:
						this.actList.t項目リストの設定_System();
						break;

					case 1:
						this.actList.t項目リストの設定_Drums();
						break;

					case 2:
						this.actList.t項目リストの設定_Exit();
						break;
				}
				this.t説明文パネルに現在選択されているメニューの説明を描画する();
			}
		}
		private void t説明文パネルに現在選択されているメニューの説明を描画する()
		{
			try
			{
				string[] str = new string[2];
				switch (this.n現在のメニュー番号)
				{
					case 0:
						str[0] = "システムに関係する項目を設定します。";
						str[1] = "Settings for an overall systems.";
						break;

					case 1:
						str[0] = "ドラムの演奏に関する項目を設定します。";
						str[1] = "Settings to play the drums.";
						break;

					case 2:
						str[0] = "設定を保存し、コンフィグ画面を終了します。";
						str[1] = "Save the settings and exit from";
						break;
				}

				int c = (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja") ? 0 : 1;

				if (this.tx説明文パネル != null)
				{
					this.tx説明文パネル.Dispose();
				}
				this.tx説明文パネル = TJAPlayer3.tCreateTexture(this.privatefont.DrawText(str[c], Color.White), true);
			}
			catch (CTextureCreateFailedException e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("説明文テクスチャの作成に失敗しました。");
				this.tx説明文パネル = null;
			}
		}
		private void t説明文パネルに現在選択されている項目の説明を描画する()
		{
			try
			{
				CItemBase item = this.actList.ib現在の選択項目;
				if (this.tx説明文パネル != null)
				{
					this.tx説明文パネル.Dispose();
				}
				this.tx説明文パネル = TJAPlayer3.tCreateTexture(privatefont.DrawText(item.strDescription, Color.White), true);
			}
			catch( CTextureCreateFailedException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "説明文パネルテクスチャの作成に失敗しました。" );
				this.tx説明文パネル = null;
			}
		}
		//-----------------
		#endregion
	}
}
