using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using FDK;

namespace TJAPlayer3
{
	internal class CActResultParameterPanel : CActivity
	{
		// コンストラクタ

		public CActResultParameterPanel()
		{
			base.b活性化してない = true;
		}


		// メソッド

		public void tアニメを完了させる()
		{
			this.ephase = EPhase.Loop;
			this.ephase_v2 = EPhaseV2.Loop;
			this.ctGauge.n現在の値 = this.ctGauge.n終了値;
			this.ctCrown用.n現在の値 = this.ctCrown用.n終了値;
		}


		// CActivity 実装

		public override void On活性化()
		{
			base.On活性化();
			this.ephase = EPhase.Start;
			this.ctCrown用 = new CCounter(0, 255, 2, TJAPlayer3.Timer);
			this.ct文字V2用 = new CCounter(0, 180, 3, TJAPlayer3.Timer);
			this.ctGauge = new CCounter(0, 100, 35, TJAPlayer3.Timer);
			for (int index = 0; index < 4; index++)
			{
				this.ct文字アニメ用[index] = new CCounter(0, 15, 50, TJAPlayer3.Timer);

				this.ToNextPhase[index] = false;
				this.n表示された桁数[index] = 0;
			}
			this.AllPlayerCannotGetCrown = true;
			for (int index = 0; index < TJAPlayer3.ConfigIni.nPlayerCount; index++) 
			{
				if (TJAPlayer3.stage選曲.n確定された曲の難易度[index] == (int)Difficulty.Dan)
				{
					switch (TJAPlayer3.stage演奏ドラム画面.actDan.GetExamStatus(TJAPlayer3.stage結果.st演奏記録[index].Dan_C, TJAPlayer3.stage結果.st演奏記録[index].Dan_C_Gauge))
					{
						case Exam.Status.Failure:
							this.CrownState[index] = 0;
							break;
						case Exam.Status.Success:
							this.CrownState[index] = 1;
							break;
						case Exam.Status.Better_Success:
							this.CrownState[index] = 2;
							break;
						default:
							break;
					}
				}
				else
				{
					if (TJAPlayer3.stage結果.st演奏記録[index].fゲージ < 80)
					{
						this.CrownState[index] = 0;
					}
					else if (TJAPlayer3.stage結果.st演奏記録[index].nMiss数_Auto含まない != 0)
					{
						this.CrownState[index] = 1;
						this.AllPlayerCannotGetCrown = false;
					}
					else if (TJAPlayer3.stage結果.st演奏記録[index].nGreat数_Auto含まない != 0)
					{
						this.CrownState[index] = 2;
						this.AllPlayerCannotGetCrown = false;
					}
					else
					{
						this.CrownState[index] = 3;
						this.AllPlayerCannotGetCrown = false;
					}
				}
			}
			this.ephase_v2 = EPhaseV2.Start;
		}
		public override void On非活性化()
		{
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				Dan_Plate = TJAPlayer3.tテクスチャの生成(Path.GetDirectoryName(TJAPlayer3.DTX[0].strファイル名の絶対パス) + @"\Dan_Plate.png");
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする(ref Dan_Plate);
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if (base.b活性化してない)
			{
				return 0;
			}
			if (base.b初めての進行描画)
			{
				base.b初めての進行描画 = false;
			}
			if (TJAPlayer3.ConfigIni.bEnableSkinV2)
			{
				this.ct文字V2用.t進行();

				#region[phaseの進行]
				if (ephase_v2 == EPhaseV2.Start)
				{
					this.ctGauge.n現在の値 = 0;
					this.ctGauge.t時間Reset();
					this.NextPhaseV2();

				}
				if (ephase_v2 == EPhaseV2.Gauge) 
				{
					if(this.ctGauge.b終了値に達した)
						this.NextPhaseV2();
				}
				else if (ephase_v2 == EPhaseV2.Skill)
				{
					this.ctCrown用.n現在の値 = 0;
					this.ctCrown用.t時間Reset();
					this.NextPhaseV2();
				}
				else if (ephase_v2 == EPhaseV2.Crown)
				{
					if (this.ctCrown用.b終了値に達した)
						this.NextPhaseV2();
				}
				else if (ephase_v2 != EPhaseV2.Loop)
				{
					if (this.ct文字V2用.b終了値に達した)
						this.NextPhaseV2();
				}
				#endregion

				if (ephase_v2 == EPhaseV2.Crown)
					this.ctCrown用.t進行();
				else if (ephase_v2 == EPhaseV2.Gauge)
					ctGauge.t進行();

				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
				{
					if (TJAPlayer3.Tx.Result_v2_Panel != null)
					{
						if (TJAPlayer3.Tx.Result_v2_Panel[i] != null) 
							TJAPlayer3.Tx.Result_v2_Panel[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultV2PanelX[i], TJAPlayer3.Skin.nResultV2PanelY[i]);
					}
					if (TJAPlayer3.Tx.Result_v2_GaugeBack != null)
					{
						TJAPlayer3.Tx.Result_v2_GaugeBack.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultV2GaugeBackX[i], TJAPlayer3.Skin.nResultV2GaugeBackY[i]);
					}
					if (TJAPlayer3.Tx.Result_v2_GaugeBase != null&& TJAPlayer3.Tx.Result_v2_Gauge != null)
					{
						int width = (int)(TJAPlayer3.Tx.Result_v2_Gauge.szテクスチャサイズ.Width * (Math.Min((TJAPlayer3.stage結果.st演奏記録[i].fゲージ / 100f), this.ctGauge.n現在の値 / 100f))) / (TJAPlayer3.Tx.Result_v2_Gauge.szテクスチャサイズ.Width / 50) * (TJAPlayer3.Tx.Result_v2_Gauge.szテクスチャサイズ.Width / 50);// 2020/10/13 Mr-Ojii 最後の意味が無いように見える乗算、除算には意味があります。消さないで。
						Rectangle rec = new Rectangle(0, 0, width, TJAPlayer3.Tx.Result_v2_Gauge.szテクスチャサイズ.Height);
						TJAPlayer3.Tx.Result_v2_GaugeBase.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultV2GaugeBodyX[i], TJAPlayer3.Skin.nResultV2GaugeBodyY[i]);
						TJAPlayer3.Tx.Result_v2_Gauge.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultV2GaugeBodyX[i], TJAPlayer3.Skin.nResultV2GaugeBodyY[i], rec);
					}

					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2GreatX[i], TJAPlayer3.Skin.nResultV2GreatY[i], TJAPlayer3.stage結果.st演奏記録[i].nPerfect数, false, EPhaseV2.Perfect, i);
					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2GoodX[i], TJAPlayer3.Skin.nResultV2GoodY[i], TJAPlayer3.stage結果.st演奏記録[i].nGreat数, false, EPhaseV2.Good, i);
					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2BadX[i], TJAPlayer3.Skin.nResultV2BadY[i], TJAPlayer3.stage結果.st演奏記録[i].nMiss数, false, EPhaseV2.Poor, i);
					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2RollX[i], TJAPlayer3.Skin.nResultV2RollY[i], TJAPlayer3.stage結果.st演奏記録[i].n連打数, false, EPhaseV2.Roll, i);
					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2ComboX[i], TJAPlayer3.Skin.nResultV2ComboY[i], TJAPlayer3.stage結果.st演奏記録[i].n最大コンボ数, false, EPhaseV2.Combo, i);

					this.t小文字表示V2(TJAPlayer3.Skin.nResultV2ScoreX[i], TJAPlayer3.Skin.nResultV2ScoreY[i], TJAPlayer3.stage結果.st演奏記録[i].nスコア, true, EPhaseV2.Score, i);

					#region 段位認定モード用+王冠
					if (TJAPlayer3.stage選曲.n確定された曲の難易度[i] == (int)Difficulty.Dan)
					{
						TJAPlayer3.stage演奏ドラム画面.actDan.DrawExam(TJAPlayer3.stage結果.st演奏記録[i].Dan_C);

						TJAPlayer3.Tx.Result_Dan?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_XY[0], TJAPlayer3.Skin.Result_Dan_XY[1], new Rectangle(TJAPlayer3.Skin.Result_Dan[0] * CrownState[i], 0, TJAPlayer3.Skin.Result_Dan[0], TJAPlayer3.Skin.Result_Dan[1]));
						// Dan_Plate
						Dan_Plate?.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_Plate_XY[0], TJAPlayer3.Skin.Result_Dan_Plate_XY[1]);
					}
					else
					{
						if (CrownState[i] != 0 && TJAPlayer3.Tx.Crown_t != null)
						{
							TJAPlayer3.Tx.Crown_t.Opacity = this.ctCrown用.n現在の値;
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.X = ((this.ctCrown用.n終了値 - this.ctCrown用.n現在の値) / 255f) * 2f + 1.0f;
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.Y = ((this.ctCrown用.n終了値 - this.ctCrown用.n現在の値) / 255f) * 2f + 1.0f;
							TJAPlayer3.Tx.Crown_t.t2D拡大率考慮中央基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_v2_Crown_X[i], TJAPlayer3.Skin.Result_v2_Crown_Y[i], new Rectangle(CrownState[i] * 100, 0, 100, 100));
						}
					}
					#endregion

				}
				return (ephase_v2 == EPhaseV2.Loop) ? 1 : 0;
			}
			else
			{
				for (int i = 0; i < this.ct文字アニメ用.Length; i++)
					if (!this.ToNextPhase[i])
						this.ct文字アニメ用[i].t進行();
				this.ctCrown用.t進行();

				#region[phaseの進行]
				if (ephase == EPhase.Start)
				{
					ctCrown用.n現在の値 = 0;
					ctCrown用.t時間Reset();
					this.NextPhase();
				}
				else if (ephase == EPhase.Crown)
				{
					if (this.ctCrown用.b終了値に達した || this.AllPlayerCannotGetCrown)
					{
						this.NextPhase();
					}
				}
				else if (ephase == EPhase.HighScore)
				{
					this.NextPhase();
				}
				else if (ephase != EPhase.Loop)
				{
					bool bIstrue = true;
					for (int index = 0; index < TJAPlayer3.ConfigIni.nPlayerCount; index++)
					{
						if (!this.ToNextPhase[index])
							bIstrue = false;
					}

					if (bIstrue)
					{
						this.NextPhase();
					}
				}
				#endregion

				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
				{
					if (TJAPlayer3.Tx.Result_Panel != null)
					{
						TJAPlayer3.Tx.Result_Panel.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultPanelX[i], TJAPlayer3.Skin.nResultPanelY[i]);
					}
					if (TJAPlayer3.Tx.Result_Score_Text != null)
					{
						int[] s_y = { 249, 543 };
						TJAPlayer3.Tx.Result_Score_Text.t2D描画(TJAPlayer3.app.Device, 753, s_y[i]); //点
					}
					if (TJAPlayer3.Tx.Result_Judge != null)
					{
						TJAPlayer3.Tx.Result_Judge.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultJudge_X[i], TJAPlayer3.Skin.nResultJudge_Y[i]);
					}
					if (TJAPlayer3.Tx.Result_Gauge_Base != null && TJAPlayer3.Tx.Result_Gauge != null)
					{
						double Rate = TJAPlayer3.stage結果.st演奏記録[i].fゲージ;
						TJAPlayer3.Tx.Result_Gauge_Base.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.nResultGaugeBaseX[i], TJAPlayer3.Skin.nResultGaugeBaseY[i], new Rectangle(0, 0, 691, 47));
						#region[ ゲージ本体 ]
						int[] y_tmp = { 145, 439 };
						int y = y_tmp[i];
						if (Rate > 2)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 4)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 12, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 6)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 24, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 8)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 36, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 10)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 49, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 12)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 62, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 14)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 74, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 16)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 86, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 18)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 99, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 20)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 112, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 22)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 125, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 24)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 138, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 26)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 150, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 28)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 162, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 30)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 175, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 32)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 187, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 34)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 200, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 36)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 212, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 38)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 225, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 40)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 238, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 42)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 251, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 44)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 263, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 46)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 276, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 48)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 288, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 50)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 301, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 52)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 313, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 54)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 326, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 56)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 339, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 58)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 352, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 60)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 364, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 62)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 377, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 64)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 390, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 66)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 402, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 68)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 415, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 70)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 427, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 72)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 440, y, new Rectangle(0, 20, 12, 20));
						if (Rate > 74)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 452, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 76)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 465, y, new Rectangle(12, 20, 13, 20));
						if (Rate > 78)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 478, y, new Rectangle(12, 20, 13, 20));

						if (Rate > 80)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 491, y - 20, new Rectangle(25, 0, 12, 40));
						if (Rate > 82)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 503, y - 20, new Rectangle(49, 0, 13, 40));
						if (Rate > 84)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 516, y - 20, new Rectangle(37, 0, 12, 40));
						if (Rate > 86)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 528, y - 20, new Rectangle(49, 0, 13, 40));
						if (Rate > 88)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 541, y - 20, new Rectangle(37, 0, 12, 40));
						if (Rate > 90)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 553, y - 20, new Rectangle(49, 0, 13, 40));
						if (Rate > 92)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 566, y - 20, new Rectangle(49, 0, 13, 40));
						if (Rate > 94)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 579, y - 20, new Rectangle(37, 0, 12, 40));
						if (Rate > 96)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 591, y - 20, new Rectangle(49, 0, 13, 40));
						if (Rate > 98)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 604, y - 20, new Rectangle(37, 0, 12, 40));
						if (Rate >= 100)
							TJAPlayer3.Tx.Result_Gauge.t2D描画(TJAPlayer3.app.Device, 559 + 616, y - 20, new Rectangle(49, 0, 10, 40));

						#endregion
					}
					if (TJAPlayer3.Tx.Gauge_Soul != null)
					{
						int[] y_Fire = { 34, 328 };
						int[] y_Soul = { 107, 401 };
						if (TJAPlayer3.Tx.Gauge_Soul_Fire != null && TJAPlayer3.stage結果.st演奏記録[i].fゲージ >= 100.0f)
							TJAPlayer3.Tx.Gauge_Soul_Fire.t2D描画(TJAPlayer3.app.Device, 1100, y_Fire[i], new Rectangle(0, 0, 230, 230));
						TJAPlayer3.Tx.Gauge_Soul.t2D描画(TJAPlayer3.app.Device, 1174, y_Soul[i], new Rectangle(0, 0, 80, 80));
					}
					//演奏中のやつ使いまわせなかった。ファック。
					this.t小文字表示(TJAPlayer3.Skin.nResultScoreX[i], TJAPlayer3.Skin.nResultScoreY[i], TJAPlayer3.stage結果.st演奏記録[i].nスコア, true, EPhase.Score, i);
					this.t小文字表示(TJAPlayer3.Skin.nResultGreatX[i], TJAPlayer3.Skin.nResultGreatY[i], TJAPlayer3.stage結果.st演奏記録[i].nPerfect数, false, EPhase.Perfect, i);
					this.t小文字表示(TJAPlayer3.Skin.nResultGoodX[i], TJAPlayer3.Skin.nResultGoodY[i], TJAPlayer3.stage結果.st演奏記録[i].nGreat数, false, EPhase.Good, i);
					this.t小文字表示(TJAPlayer3.Skin.nResultBadX[i], TJAPlayer3.Skin.nResultBadY[i], TJAPlayer3.stage結果.st演奏記録[i].nMiss数, false, EPhase.Poor, i);

					this.t小文字表示(TJAPlayer3.Skin.nResultComboX[i], TJAPlayer3.Skin.nResultComboY[i], TJAPlayer3.stage結果.st演奏記録[i].n最大コンボ数, false, EPhase.Combo, i);
					this.t小文字表示(TJAPlayer3.Skin.nResultRollX[i], TJAPlayer3.Skin.nResultRollY[i], TJAPlayer3.stage結果.st演奏記録[i].n連打数, false, EPhase.Roll, i);

					#region 段位認定モード用+王冠
					if (TJAPlayer3.stage選曲.n確定された曲の難易度[i] == (int)Difficulty.Dan)
					{
						TJAPlayer3.stage演奏ドラム画面.actDan.DrawExam(TJAPlayer3.stage結果.st演奏記録[i].Dan_C);
						
						TJAPlayer3.Tx.Result_Dan?.t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_XY[0], TJAPlayer3.Skin.Result_Dan_XY[1], new Rectangle(TJAPlayer3.Skin.Result_Dan[0] * CrownState[i], 0, TJAPlayer3.Skin.Result_Dan[0], TJAPlayer3.Skin.Result_Dan[1]));
						// Dan_Plate
						Dan_Plate?.t2D中心基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Dan_Plate_XY[0], TJAPlayer3.Skin.Result_Dan_Plate_XY[1]);
					}
					else
					{
						if (CrownState[i] != 0 && TJAPlayer3.Tx.Crown_t != null)
						{
							TJAPlayer3.Tx.Crown_t.Opacity = this.ctCrown用.n現在の値;
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.X = ((this.ctCrown用.n終了値 - this.ctCrown用.n現在の値) / 255f) * 2f + 1.0f;
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.Y = ((this.ctCrown用.n終了値 - this.ctCrown用.n現在の値) / 255f) * 2f + 1.0f;
							TJAPlayer3.Tx.Crown_t.t2D拡大率考慮中央基準描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_Crown_X[i], TJAPlayer3.Skin.Result_Crown_Y[i], new Rectangle(CrownState[i] * 100, 0, 100, 100));
						}
					}
					#endregion
				}

				return (this.ephase == EPhase.Loop) ? 1 : 0;
			}
		}


		// その他

		#region [ private ]
		//-----------------
		private int[] CrownState = new int[4] { 0, 0, 0, 0 };
		private bool[] ToNextPhase = new bool[4] { false, false, false, false };
		private bool AllPlayerCannotGetCrown = false;
		private CCounter ctCrown用 = new CCounter();

		private CTexture Dan_Plate;
		#region[V1]
		private CCounter[] ct文字アニメ用 = new CCounter[4];
		private int[] n表示された桁数 = new int[4] { 0, 0, 0, 0 };

		private EPhase ephase;

		private void t小文字表示(int x, int y, long n, bool score, EPhase phase, int nPlayer)
		{
			if (phase > ephase)
				return;

			for (int index = 0; index < n.ToString().Length; index++)
			{
				int Num = (int)(n / Math.Pow(10, index) % 10);
				bool IsDigit = false;

				if (ephase == phase && !this.ToNextPhase[nPlayer] && index == this.n表示された桁数[nPlayer])
				{
					if (TJAPlayer3.Skin.sound回転音 != null)
						if (!TJAPlayer3.Skin.sound回転音.b再生中)
							TJAPlayer3.Skin.sound回転音.t再生する();
					Num = this.ct文字アニメ用[nPlayer].n現在の値 % 10;
					IsDigit = true;
				}

				if (score)
				{
					if (TJAPlayer3.Tx.Result_Score_Number != null)
					{
						Rectangle rectangle = new Rectangle(24 * Num, 0, 24, TJAPlayer3.Tx.Result_Score_Number.szテクスチャサイズ.Height);
						if (TJAPlayer3.Tx.Result_Score_Number != null)
						{
							TJAPlayer3.Tx.Result_Score_Number.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
						}
						x -= 24;
					}
				}
				else
				{
					if (TJAPlayer3.Tx.Result_Number != null)
					{
						Rectangle rectangle = new Rectangle(32 * Num, 0, 32, TJAPlayer3.Tx.Result_Number.szテクスチャサイズ.Height / 2);
						if (TJAPlayer3.Tx.Result_Number != null)
						{
							TJAPlayer3.Tx.Result_Number.t2D描画(TJAPlayer3.app.Device, x, y, rectangle);
						}
						x -= 22;
					}
				}
				if (IsDigit)
				{
					if (this.ct文字アニメ用[nPlayer].b終了値に達した)
					{
						this.n表示された桁数[nPlayer]++;
						this.ct文字アニメ用[nPlayer].n現在の値 = 0;
						if (this.n表示された桁数[nPlayer] == n.ToString().Length)
						{
							TJAPlayer3.Skin.sound決定音?.t再生する();
							this.n表示された桁数[nPlayer] = 0;
							this.ToNextPhase[nPlayer] = true;
						}
					}
					break;
				}
			}
		}

		private void NextPhase() 
		{
			ephase += 1;
			for (int index = 0; index < this.ToNextPhase.Length; index++)
			{
				this.ct文字アニメ用[index].n現在の値 = 0;
				this.ct文字アニメ用[index].t時間Reset();
				this.ToNextPhase[index] = false;
			}
		}
		private enum EPhase : int
		{
			Start,
			Crown,
			Score,
			HighScore,
			Perfect,
			Good,
			Poor,
			Combo,
			Roll,
			Loop
		}
		#endregion
		#region[V2]
		private EPhaseV2 ephase_v2;
		private CCounter ct文字V2用 = new CCounter();
		private CCounter ctGauge = new CCounter();

		private void NextPhaseV2()
		{
			ephase_v2 += 1;
			for (int index = 0; index < this.ToNextPhase.Length; index++)
			{
				this.ct文字V2用.n現在の値 = 0;
				this.ct文字V2用.t時間Reset();
				this.ToNextPhase[index] = false;
			}
		}

		private void t小文字表示V2(int x, int y, long n, bool score, EPhaseV2 phase_v2, int nPlayer)
		{
			if (phase_v2 > ephase_v2)
				return;

			else if (phase_v2 == ephase_v2)
			{
				if (score)
				{
					float ratio = (float)Math.Sin(this.ct文字V2用.n現在の値 / 180f * Math.PI) * 0.4f + 1f;
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.X = ratio;
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.Y = ratio;
				}
				else
				{
					float ratio = Math.Max(1f, (float)Math.Cos(this.ct文字V2用.n現在の値 / 180f * Math.PI) * 0.4f + 1f);
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.X = ratio / 1.6f;
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.Y = ratio / 1.6f;
				}
			}
			else
			{
				if (score)
				{
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.X = 1f;
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.Y = 1f;
				}
				else 
				{
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.X = 0.625f;
					TJAPlayer3.Tx.Result_v2_Number.vc拡大縮小倍率.Y = 0.625f;
				}
			}

			for (int index = 0; index < n.ToString().Length; index++)
			{
				int Num = (int)(n / Math.Pow(10, index) % 10);

				if (TJAPlayer3.Tx.Result_v2_Number != null)
				{
					Rectangle rectangle = new Rectangle(TJAPlayer3.Tx.Result_v2_Number.szテクスチャサイズ.Width / 10 * Num, 0, TJAPlayer3.Tx.Result_v2_Number.szテクスチャサイズ.Width / 10, TJAPlayer3.Tx.Result_v2_Number.szテクスチャサイズ.Height);
					if (TJAPlayer3.Tx.Result_v2_Number != null)
					{
						TJAPlayer3.Tx.Result_v2_Number.t2D拡大率考慮中央基準描画(TJAPlayer3.app.Device, x, y, rectangle);
					}
					if(score)
						x -= TJAPlayer3.Tx.Result_v2_Number.szテクスチャサイズ.Width / 10;
					else
						x -= TJAPlayer3.Tx.Result_v2_Number.szテクスチャサイズ.Width / 16;
				}
			}
		}
		private enum EPhaseV2 : int
		{
			Start,
			Gauge,
			Perfect,
			Good,
			Poor,
			Roll,
			Combo,
			Score,
			Skill,
			Crown,
			Loop
		}
        #endregion

        //-----------------
        #endregion
    }
}
