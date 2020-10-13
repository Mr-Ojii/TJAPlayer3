using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using FDK;
using DiscordRPC;

namespace TJAPlayer3
{
	internal class CStage結果 : CStage
	{
		// プロパティ

		public bool b新記録ランク;
		public float fPerfect率;
		public float fGreat率;
		public float fGood率;
		public float fPoor率;
		public float fMiss率;
		public int nランク値;
		public CScoreIni.C演奏記録[] st演奏記録;


		// コンストラクタ

		public CStage結果()
		{
			this.st演奏記録 = new CScoreIni.C演奏記録[2];
			base.eステージID = CStage.Eステージ.結果;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			base.list子Activities.Add( this.actParameterPanel = new CActResultParameterPanel() );
			base.list子Activities.Add( this.actSongBar = new CActResultSongBar() );
			base.list子Activities.Add( this.actFI = new CActFIFOResult() );
			base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
		}

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "結果ステージを活性化します。" );
			Trace.Indent();
			try
			{
				#region [ 初期化 ]
				//---------------------
				this.eフェードアウト完了時の戻り値 = E戻り値.継続;
				this.bアニメが完了 = false;
				this.bIsCheckedWhetherResultScreenShouldSaveOrNot = false;				// #24609 2011.3.14 yyagi
				this.b新記録ランク = false;
				//---------------------
				#endregion

				#region [ 結果の計算 ]
				//---------------------
				for( int i = 0; i < 1; i++ )
				{
					this.nランク値 = -1;
					this.fPerfect率 = this.fGreat率 = this.fGood率 = this.fPoor率 = this.fMiss率 = 0.0f;	// #28500 2011.5.24 yyagi
					if ( ( i != 0 ) )
					{
						CScoreIni.C演奏記録 part = this.st演奏記録[0];

						bool bIsAutoPlay = TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0];

						this.fPerfect率 = bIsAutoPlay ? 0f : ( ( 100f * part.nPerfect数 ) / ( (float) part.n全チップ数 ) );
						this.fGreat率 = bIsAutoPlay ? 0f : ( ( 100f * part.nGreat数 ) / ( (float) part.n全チップ数 ) );
						this.fGood率 = bIsAutoPlay ? 0f : ( ( 100f * part.nGood数 ) / ( (float) part.n全チップ数 ) );
						this.fPoor率 = bIsAutoPlay ? 0f : ( ( 100f * part.nPoor数 ) / ( (float) part.n全チップ数 ) );
						this.fMiss率 = bIsAutoPlay ? 0f : ( ( 100f * part.nMiss数 ) / ( (float) part.n全チップ数 ) );
						this.nランク値 = CScoreIni.tランク値を計算して返す( part );
					}
				}
				//---------------------
				#endregion

				#region [ .score.ini の作成と出力 ]
				//---------------------
				string str = TJAPlayer3.DTX[0].strファイル名の絶対パス + ".score.ini";
				CScoreIni ini = new CScoreIni( str );

				bool b今までにフルコンボしたことがある = false;

				for( int i = 0; i < 1; i++ )
				{
					// フルコンボチェックならびに新記録ランクチェックは、ini.Record[] が、スコアチェックや演奏型スキルチェックの IF 内で書き直されてしまうよりも前に行う。(2010.9.10)

					b今までにフルコンボしたことがある = ini.stセクション.HiScore.bフルコンボである;

					// #24459 上記の条件だと[HiSkill.***]でのランクしかチェックしていないので、BestRankと比較するよう変更。
					if ( this.nランク値 >= 0 && ini.stファイル.BestRank > this.nランク値 )		// #24459 2011.3.1 yyagi update BestRank
					{
						this.b新記録ランク = true;
						ini.stファイル.BestRank = this.nランク値;
					}

					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false && this.st演奏記録[0].b途中でAutoを切り替えたか == false)
					ini.stセクション.HiScore = this.st演奏記録[0];

					// ラストプレイ #23595 2011.1.9 ikanick
					// オートじゃなければプレイ結果を書き込む
					if( TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false ) {
						ini.stセクション.LastPlay = this.st演奏記録[0];
					}

					// #23596 10.11.16 add ikanick オートじゃないならクリア回数を1増やす
					//        11.02.05 bオート to t更新条件を取得する use      ikanick
					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false)
					{	
						ini.stファイル.ClearCountDrums++;
					}
					//---------------------------------------------------------------------/
				}
				if (TJAPlayer3.ConfigIni.bScoreIniを出力する)
				{
					ini.t書き出し(str);
				}
				//---------------------
				#endregion

				#region [ 選曲画面の譜面情報の更新 ]
				//---------------------

				{ 
					Cスコア cスコア = TJAPlayer3.stage選曲.r確定されたスコア;

					if (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false && this.st演奏記録[0].b途中でAutoを切り替えたか == false)
					{
						// FullCombo した記録を FullCombo なしで超えた場合、FullCombo マークが消えてしまう。
						// → FullCombo は、最新記録と関係なく、一度達成したらずっとつくようにする。(2010.9.11)
						cスコア.譜面情報.フルコンボ = this.st演奏記録[0].bフルコンボである | b今までにフルコンボしたことがある;

						cスコア.譜面情報.最大ランク = this.nランク値;
						cスコア.譜面情報.最大スキル = this.st演奏記録[0].db演奏型スキル値;

						cスコア.譜面情報.n王冠 = st演奏記録[0].n王冠;//2020.05.22 Mr-Ojii データが保存されない問題の解決策。
						cスコア.譜面情報.nハイスコア = st演奏記録[0].nハイスコア;
						cスコア.譜面情報.nSecondScore = st演奏記録[0].nSecondScore;
						cスコア.譜面情報.nThirdScore = st演奏記録[0].nThirdScore;

						cスコア.譜面情報.strHiScorerName = st演奏記録[0].strHiScorerName;
						cスコア.譜面情報.strSecondScorerName = st演奏記録[0].strSecondScorerName;
						cスコア.譜面情報.strThirdScorerName = st演奏記録[0].strThirdScorerName;
					}
					
					TJAPlayer3.stage選曲.r確定されたスコア = cスコア;
				}
				//---------------------
				#endregion

				string Details = TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION;

				// Discord Presenseの更新
				TJAPlayer3.DiscordClient?.SetPresence(new RichPresence()
				{
					Details = Details.Substring(0, Math.Min(127, Details.Length)),
					State = "Result" + (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == true ? " (Auto)" : ""),
					Timestamps = new Timestamps(TJAPlayer3.StartupTime),
					Assets = new Assets()
					{
						LargeImageKey = "tjaplayer3-f",
						LargeImageText = "Ver." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
					}
				});

				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation( "結果ステージの活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			base.On非活性化();
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
			if( !base.b活性化してない )
			{
				if( this.ct登場用 != null )
				{
					this.ct登場用 = null;
				}
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				int num;
				if( base.b初めての進行描画 )
				{
					this.ct登場用 = new CCounter( 0, 100, 5, TJAPlayer3.Timer );
					this.actFI.tフェードイン開始();
					base.eフェーズID = CStage.Eフェーズ.共通_フェードイン;
					base.b初めての進行描画 = false;
				}
				this.bアニメが完了 = true;
				if( this.ct登場用.b進行中 )
				{
					this.ct登場用.t進行();
					if( this.ct登場用.b終了値に達した )
					{
						this.ct登場用.t停止();
					}
					else
					{
						this.bアニメが完了 = false;
					}
				}

				// 描画

				if(TJAPlayer3.Tx.Result_Background != null )
				{
					TJAPlayer3.Tx.Result_Background.t2D描画( TJAPlayer3.app.Device, 0, 0 );
				}
				if( this.ct登場用.b進行中 && ( TJAPlayer3.Tx.Result_Header != null ) )
				{
					double num2 = ( (double) this.ct登場用.n現在の値 ) / 100.0;
					double num3 = Math.Sin( Math.PI / 2 * num2 );
					num = ( (int) ( TJAPlayer3.Tx.Result_Header.sz画像サイズ.Height * num3 ) ) - TJAPlayer3.Tx.Result_Header.sz画像サイズ.Height;
				}
				else
				{
					num = 0;
				}
				if(TJAPlayer3.Tx.Result_Header != null )
				{
					TJAPlayer3.Tx.Result_Header.t2D描画( TJAPlayer3.app.Device, 0, 0 );
				}
				if ( this.actParameterPanel.On進行描画() == 0 )
				{
					this.bアニメが完了 = false;
				}

				if ( this.actSongBar.On進行描画() == 0 )
				{
					this.bアニメが完了 = false;
				}

				#region ネームプレート
				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
				{
					if (TJAPlayer3.Tx.NamePlate[i] != null)
					{
						TJAPlayer3.Tx.NamePlate[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_NamePlate_X[i], TJAPlayer3.Skin.Result_NamePlate_Y[i]);
					}
				}
				#endregion

				if ( base.eフェーズID == CStage.Eフェーズ.共通_フェードイン )
				{
					if( this.actFI.On進行描画() != 0 )
					{
						base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
					}
				}
				else if( ( base.eフェーズID == CStage.Eフェーズ.共通_フェードアウト ) )			//&& ( this.actFO.On進行描画() != 0 ) )
				{
					return (int) this.eフェードアウト完了時の戻り値;
				}
				#region [ #24609 2011.3.14 yyagi ランク更新or演奏型スキル更新時、リザルト画像をpngで保存する ]
				if ( this.bアニメが完了 == true && this.bIsCheckedWhetherResultScreenShouldSaveOrNot == false	// #24609 2011.3.14 yyagi; to save result screen in case BestRank or HiSkill.
					&& TJAPlayer3.ConfigIni.bScoreIniを出力する
					&& TJAPlayer3.ConfigIni.bIsAutoResultCapture)												// #25399 2011.6.9 yyagi
				{
					CheckAndSaveResultScreen(true);
					this.bIsCheckedWhetherResultScreenShouldSaveOrNot = true;
				}
				#endregion

				// キー入力

				if ((TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.b押された(Eパッド.LRed) || TJAPlayer3.Pad.b押された(Eパッド.RRed) || (TJAPlayer3.Pad.b押された(Eパッド.LRed2P) || TJAPlayer3.Pad.b押された(Eパッド.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && !this.bアニメが完了)
				{
					this.actFI.tフェードイン完了();                 // #25406 2011.6.9 yyagi
					this.actParameterPanel.tアニメを完了させる();
					this.actSongBar.tアニメを完了させる();
					this.ct登場用.t停止();
				}
				if (base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
				{
					if (TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape))
					{
						TJAPlayer3.Skin.sound取消音.t再生する();
						this.actFO.tフェードアウト開始();
						base.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						this.eフェードアウト完了時の戻り値 = E戻り値.完了;
					}
					if ((TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.b押された(Eパッド.LRed) || TJAPlayer3.Pad.b押された(Eパッド.RRed) || (TJAPlayer3.Pad.b押された(Eパッド.LRed2P) || TJAPlayer3.Pad.b押された(Eパッド.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && this.bアニメが完了)
					{
						TJAPlayer3.Skin.sound取消音.t再生する();
						//							this.actFO.tフェードアウト開始();
						base.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						this.eフェードアウト完了時の戻り値 = E戻り値.完了;
					}
				}
				
			}
			return 0;
		}

		public enum E戻り値 : int
		{
			継続,
			完了
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter ct登場用;
		private E戻り値 eフェードアウト完了時の戻り値;
		private CActFIFOResult actFI;
		private CActFIFOBlack actFO;
		private CActResultParameterPanel actParameterPanel;
		private CActResultSongBar actSongBar;
		private bool bアニメが完了;
		private bool bIsCheckedWhetherResultScreenShouldSaveOrNot;				// #24509 2011.3.14 yyagi

		#region [ #24609 リザルト画像をpngで保存する ]		// #24609 2011.3.14 yyagi; to save result screen in case BestRank or HiSkill.
		/// <summary>
		/// リザルト画像のキャプチャと保存。
		/// 自動保存モード時は、ランク更新or演奏型スキル更新時に自動保存。
		/// 手動保存モード時は、ランクに依らず保存。
		/// </summary>
		/// <param name="bIsAutoSave">true=自動保存モード, false=手動保存モード</param>
		private void CheckAndSaveResultScreen(bool bIsAutoSave)
		{
			string datetime = DateTime.Now.ToString( "yyyyMMddHHmmss" );
			if ( bIsAutoSave )
			{
				// リザルト画像を自動保存するときは、dtxファイル名.yyMMddHHmmss_SS.png という形式で保存。

				if (this.b新記録ランク == true)
				{
					string strRank = ((CScoreIni.ERANK)(this.nランク値)).ToString();
					string strFullPath = TJAPlayer3.DTX[0].strファイル名の絶対パス + "." + datetime + "_" + strRank + ".png";

					CSaveScreen.CSaveFromDevice(TJAPlayer3.app.Device, strFullPath);
				}
			}
		}
		#endregion
		//-----------------
		#endregion
	}
}
