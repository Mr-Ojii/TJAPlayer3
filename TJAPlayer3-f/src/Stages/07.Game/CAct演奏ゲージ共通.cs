using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayer3;

/// <summary>
/// CAct演奏Drumsゲージ と CAct演奏Gutiarゲージ のbaseクラス。ダメージ計算やDanger/Failed判断もこのクラスで行う。
/// 
/// 課題
/// _STAGE FAILED OFF時にゲージ回復を止める
/// _黒→閉店までの差を大きくする。
/// </summary>
internal class CAct演奏ゲージ共通 : CActivity
{
	// コンストラクタ
	public CAct演奏ゲージ共通()
	{
		this.b活性化してない = true;
	}

	// CActivity 実装

	public override void On活性化()
	{
		this.ct炎 = new CCounter( 0, 6, 50, TJAPlayer3.Timer );
		base.On活性化();
	}
	public override void On非活性化()
	{
		this.ct炎 = null;
		base.On非活性化();
	}

	public override void OnManagedリソースの作成()
	{
		if( !this.b活性化してない )
		{
			if(TJAPlayer3.Skin.SkinConfig.Game.Gauge.RainbowTimer <= 1)
			{
				throw new DivideByZeroException("SkinConfigの設定\"Game.Gauge.RainbowTimer\"を1以下にすることは出来ません。");
			}
			this.ct虹アニメ = new CCounter( 0, TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn - 1, TJAPlayer3.Skin.SkinConfig.Game.Gauge.RainbowTimer, TJAPlayer3.Timer );
			this.ct虹透明度 = new CCounter(0, TJAPlayer3.Skin.SkinConfig.Game.Gauge.RainbowTimer - 1, 1, TJAPlayer3.Timer);
			base.OnManagedリソースの作成();
		}
	}
	public override void OnManagedリソースの解放()
	{
		if( !base.b活性化してない )
		{
			this.ct虹アニメ = null;
			base.OnManagedリソースの解放();
		}
	}
	public override int On進行描画()
	{
		if ( !base.b活性化してない )
		{
			//CDTXMania.act文字コンソール.tPrint( 20, 150, C文字コンソール.EFontType.白, this.db現在のゲージ値.Taiko.ToString() );

			#region [ 初めての進行描画 ]
			if ( base.b初めての進行描画 )
			{
				base.b初めての進行描画 = false;
			}
			#endregion


			int nRectX = (int)( this.db現在のゲージ値[ 0 ] / 2 ) * 14;
			int nRectX2P = (int)( this.db現在のゲージ値[ 1 ] / 2 ) * 14;
			int 虹ベース = ct虹アニメ.n現在の値 + 1;
			if (虹ベース == ct虹アニメ.n終了値+1) 虹ベース = 0;
			/*

			新虹ゲージの仕様  2018/08/10 ろみゅ～？
				
				フェードで動く虹ゲージが、ある程度強化できたので放出。
				透明度255の虹ベースを描画し、その上から透明度可変式の虹ゲージを描画する。
				ゲージのパターン枚数は、読み込み枚数によって決定する。
				ゲージ描画の切り替え速度は、タイマーの値をSkinConfigで指定して行う(初期値50,1にするとエラーを吐く模様)。進行速度は1ms、高フレームレートでの滑らかさを重視。
				虹ゲージの透明度調整値は、「255/パターン数」で算出する。
				こんな簡単なことを考えるのに30分(60f/s換算で108000f)を費やす。
				
			*/
			if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
			{
				int[] ypos = new int[] { 144, 532 };
				for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
				{
					if (TJAPlayer3.Tx.Gauge_Base[nPlayer] != null)
					{
						TJAPlayer3.Tx.Gauge_Base[nPlayer].t2D描画(TJAPlayer3.app.Device, 492, ypos[nPlayer], new Rectangle(0, 0, 700, 44));
					}
				}
			}
			else
			{
				if (TJAPlayer3.Tx.Gauge_Base_Danc != null)
				{
					TJAPlayer3.Tx.Gauge_Base_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, 700, 44));
				}
			}
			#region[ ゲージ1P ]				
			if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] != (int)Difficulty.Dan)
			{
				if (TJAPlayer3.Tx.Gauge[0] != null)
				{
					TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, nRectX, 44));

					if (TJAPlayer3.Tx.Gauge_Line[0] != null)
					{
						if (this.db現在のゲージ値[0] >= 100.0)
						{
							this.ct虹アニメ.t進行Loop();
							this.ct虹透明度.t進行Loop();
							if (TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値] != null)
							{
								TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値].Opacity = 255;
								TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値].t2D描画(TJAPlayer3.app.Device, 492, 144);
								TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
								TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].t2D描画(TJAPlayer3.app.Device, 492, 144);
							}
						}
						TJAPlayer3.Tx.Gauge_Line[0].t2D描画(TJAPlayer3.app.Device, 492, 144);
					}
					#region[ 「クリア」文字 ]
					if (this.db現在のゲージ値[0] >= 80.0)
					{
						TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 1038, 144, new Rectangle(0, 44, 58, 24));
					}
					else
					{
						TJAPlayer3.Tx.Gauge[0].t2D描画(TJAPlayer3.app.Device, 1038, 144, new Rectangle(58, 44, 58, 24));
					}
					#endregion
				}
			}
			else {

				if (TJAPlayer3.Tx.Gauge_Danc != null) 
				{
					TJAPlayer3.Tx.Gauge_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144, new Rectangle(0, 0, nRectX, 44));

					if (TJAPlayer3.Tx.Gauge_Line_Danc != null)
					{
						if (this.db現在のゲージ値[0] >= 100.0)
						{
							this.ct虹アニメ.t進行Loop();
							this.ct虹透明度.t進行Loop();
							if (TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値] != null)
							{
								TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値].Opacity = 255;
								TJAPlayer3.Tx.Gauge_Rainbow_Danc[this.ct虹アニメ.n現在の値].t2D描画(TJAPlayer3.app.Device, 492, 144);
								TJAPlayer3.Tx.Gauge_Rainbow_Danc[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
								TJAPlayer3.Tx.Gauge_Rainbow_Danc[虹ベース].t2D描画(TJAPlayer3.app.Device, 492, 144);
							}
						}
						TJAPlayer3.Tx.Gauge_Line_Danc.t2D描画(TJAPlayer3.app.Device, 492, 144);
					}
				}
			}
			#endregion
			#region[ ゲージ2P ]
			if( TJAPlayer3.stage演奏ドラム画面.bDoublePlay && TJAPlayer3.Tx.Gauge[1] != null )
			{
				TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 492, 532, new Rectangle( 0, 0, nRectX2P, 44 ) );
				if(TJAPlayer3.Tx.Gauge[1] != null )
				{
					if (this.db現在のゲージ値[1] >= 100.0)
					{
						this.ct虹アニメ.t進行Loop();
						this.ct虹透明度.t進行Loop();
						if (TJAPlayer3.Tx.Gauge_Rainbow[this.ct虹アニメ.n現在の値] != null)
						{
							TJAPlayer3.Tx.Gauge_Rainbow[ct虹アニメ.n現在の値].Opacity = 255;
							TJAPlayer3.Tx.Gauge_Rainbow[ct虹アニメ.n現在の値].t2D上下反転描画(TJAPlayer3.app.Device, 492, 532);
							TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].Opacity = (ct虹透明度.n現在の値 * 255 / ct虹透明度.n終了値) / 1;
							TJAPlayer3.Tx.Gauge_Rainbow[虹ベース].t2D上下反転描画(TJAPlayer3.app.Device, 492, 532);
						}
					}
					TJAPlayer3.Tx.Gauge_Line[1].t2D描画( TJAPlayer3.app.Device, 492, 532 );
				}
				#region[ 「クリア」文字 ]
				if( this.db現在のゲージ値[ 1 ] >= 80.0 )
				{
					TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 1038, 554, new Rectangle( 0, 44, 58, 24 ) );
				}
				else
				{
					TJAPlayer3.Tx.Gauge[1].t2D描画( TJAPlayer3.app.Device, 1038, 554, new Rectangle( 58, 44, 58, 24 ) );
				}
				#endregion
			}
			#endregion


			if(TJAPlayer3.Tx.Gauge_Soul_Fire != null )
			{
				//仮置き
				int[] nSoulFire = new int[] { 52, 443, 0, 0 };
				for( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
				{
					if( this.db現在のゲージ値[ i ] >= 100.0 )
					{
						this.ct炎.t進行Loop();
						TJAPlayer3.Tx.Gauge_Soul_Fire.t2D描画( TJAPlayer3.app.Device, 1112, nSoulFire[ i ], new Rectangle( 230 * ( this.ct炎.n現在の値 ), 0, 230, 230 ) );
					}
				}
			}
			if(TJAPlayer3.Tx.Gauge_Soul != null )
			{
				//仮置き
				int[] nSoulY = new int[] { 125, 516, 0, 0 };
				for( int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++ )
				{
					if( this.db現在のゲージ値[ i ] >= 80.0 )
					{
						TJAPlayer3.Tx.Gauge_Soul.t2D描画( TJAPlayer3.app.Device, 1184, nSoulY[ i ], new Rectangle( 0, 0, 80, 80 ) );
					}
					else
					{
						TJAPlayer3.Tx.Gauge_Soul.t2D描画( TJAPlayer3.app.Device, 1184, nSoulY[ i ], new Rectangle( 0, 80, 80, 80 ) );
					}
				}
			}

		}
		return 0;
	}
	
	const double GAUGE_MAX = 100.0;
	const double GAUGE_INITIAL =  2.0 / 3;
	const double GAUGE_MIN = -0.1;
	const double GAUGE_ZERO = 0.0;
	const double GAUGE_DANGER = 0.3;

	public bool bRisky							// Riskyモードか否か
	{
		get;
		private set;
	}
	public int nRiskyTimes_Initial				// Risky初期値
	{
		get;
		private set;
	}
	public int nRiskyTimes						// 残Miss回数
	{
		get;
		private set;
	}
	public bool IsFailed( int nPlayer )	// 閉店状態になったかどうか
	{
		if ( bRisky ) {
			return ( nRiskyTimes <= 0 );
		}
		return this.db現在のゲージ値[ nPlayer ] <= GAUGE_MIN;
	}
	public bool IsDanger( int nPlayer )	// DANGERかどうか
	{
		if ( bRisky )
		{
			switch ( nRiskyTimes_Initial ) {
				case 1:
					return false;
				case 2:
				case 3:
					return ( nRiskyTimes <= 1 );
				default: 
					return ( nRiskyTimes <= 2 );
			}
		}
		return ( this.db現在のゲージ値[ nPlayer ] <= GAUGE_DANGER );
	}

	public double dbゲージ値	// Drums専用
	{
		get
		{
			return this.db現在のゲージ値[ 0 ];
		}
		set
		{
			this.db現在のゲージ値[ 0 ] = value;
			if ( this.db現在のゲージ値[ 0 ] > GAUGE_MAX )
			{
				this.db現在のゲージ値[ 0 ] = GAUGE_MAX;
			}
		}
	}

	public double dbゲージ値2P	// Drums専用
	{
		get
		{
			return this.db現在のゲージ値[ 1 ];
		}
		set
		{
			this.db現在のゲージ値[ 1 ] = value;
			if ( this.db現在のゲージ値[ 1 ] > GAUGE_MAX )
			{
				this.db現在のゲージ値[ 1 ] = GAUGE_MAX;
			}
		}
	}

	/// <summary>
	/// ゲージの初期化
	/// </summary>
	/// <param name="nRiskyTimes_Initial_">Riskyの初期値(0でRisky未使用)</param>
	public void Init(int nRiskyTimes_InitialVal )		// ゲージ初期化
	{
		//ダメージ値の計算は太鼓の達人譜面Wikiのものを参考にしました。

		for ( int i = 0; i < 4; i++ )
		{
			this.db現在のゲージ値[ i ] = 0;
		}

		this.dbゲージ値 = 0;
		this.dbゲージ値2P = 0;

		//ゲージのMAXまでの最低コンボ数を計算
		float[] dbGaugeMaxComboValue = new float[2] { 0 , 0};
		float[,] dbGaugeMaxComboValue_branch = new float[2,3];
		float[] dbDamageRate = new float[2] { 2.0f, 2.0f };

		if( nRiskyTimes_InitialVal > 0 )
		{
			this.bRisky = true;
			this.nRiskyTimes = TJAPlayer3.ConfigIni.nRisky;
			this.nRiskyTimes_Initial = TJAPlayer3.ConfigIni.nRisky;
		}

		for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++) {
			switch (TJAPlayer3.DTX[nPlayer].LEVELtaiko[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]])
			{
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
					{
						if (TJAPlayer3.DTX[nPlayer].bHasBranchChip)
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[0] / 100.0f);
							for (int i = 0; i < 3; i++)
							{
								dbGaugeMaxComboValue_branch[nPlayer,i] = TJAPlayer3.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[0] / 100.0f);
							}
							dbDamageRate[nPlayer] = 1.6f;
						}
						else
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[0] / 100.0f);
							dbDamageRate[nPlayer] = 1.6f;
						}
						break;
					}


				case 8:
					{
						if (TJAPlayer3.DTX[nPlayer].bHasBranchChip)
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[1] / 100.0f);
							for (int i = 0; i < 3; i++)
							{
								dbGaugeMaxComboValue_branch[nPlayer,i] = TJAPlayer3.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[1] / 100.0f);
							}
						}
						else
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[1] / 100.0f);
						}
						break;
					}

				case 9:
				case 10:
					{
						if (TJAPlayer3.DTX[nPlayer].bHasBranchChip)
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
							for (int i = 0; i < 3; i++)
							{
								dbGaugeMaxComboValue_branch[nPlayer,i] = TJAPlayer3.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[2] / 100.0f);
							}
						}
						else
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
						}
						break;
					}

				default:
					{
						if (TJAPlayer3.DTX[nPlayer].bHasBranchChip)
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
							for (int i = 0; i < 3; i++)
							{
								dbGaugeMaxComboValue_branch[nPlayer,i] = TJAPlayer3.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[2] / 100.0f);
							}
						}
						else
						{
							dbGaugeMaxComboValue[nPlayer] = TJAPlayer3.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
						}
						break;
					}
			} 
		}

		double[] nGaugeRankValue = new double[2] { 0D, 0D };
		double[,] nGaugeRankValue_branch = new double[,] { { 0D, 0D, 0D } , { 0D, 0D, 0D } };
		for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
		{
			if (TJAPlayer3.DTX[nPlayer].GaugeIncreaseMode == GaugeIncreaseMode.Normal)
			{
				nGaugeRankValue[nPlayer] = Math.Floor(10000.0f / dbGaugeMaxComboValue[nPlayer]);
				for (int i = 0; i < 3; i++)
				{
					nGaugeRankValue_branch[nPlayer,i] = Math.Floor(10000.0f / dbGaugeMaxComboValue_branch[nPlayer,i]);
				}
			}
			else
			{
				nGaugeRankValue[nPlayer] = 10000.0f / dbGaugeMaxComboValue[nPlayer];
				for (int i = 0; i < 3; i++)
				{
					nGaugeRankValue_branch[nPlayer,i] = 10000.0f / dbGaugeMaxComboValue_branch[nPlayer,i];
				}
			}
		}

		//ゲージ値計算
		//実機に近い計算

		//計算結果がInfintyだった場合も考える。2020.04.21.akasoko26 //2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
		#region [ 計算結果がInfintyだった場合も考えて ]
		float fIsDontInfinty = 0.4f;//適当に0.4で
		float[,] fAddVolume = new float[,] { { 1.0f, 0.5f, dbDamageRate[0] }, { 1.0f, 0.5f, dbDamageRate[1] } };


		for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int l = 0; l < 3; l++)
				{
					if (!double.IsInfinity(nGaugeRankValue_branch[nPlayer,i] / 100.0f))//値がInfintyかチェック
					{
						fIsDontInfinty = (float)(nGaugeRankValue_branch[nPlayer,i] / 100.0f);
						this.dbゲージ増加量_Branch[nPlayer, i, l] = fIsDontInfinty * fAddVolume[nPlayer,l];
					}
				}
			}
			for (int i = 0; i < 3; i++)
			{
				for (int l = 0; l < 3; l++)
				{
					if (double.IsInfinity(nGaugeRankValue_branch[nPlayer,i] / 100.0f))//値がInfintyかチェック
					{
						//Infintyだった場合はInfintyではない値 * 3.0をしてその値を利用する。
						this.dbゲージ増加量_Branch[nPlayer, i, l] = (fIsDontInfinty * fAddVolume[nPlayer,l]) * 3f;
					}
				}
			}
		}
		#endregion


		for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
		{
			this.dbゲージ増加量[nPlayer,0] = (float)nGaugeRankValue[nPlayer] / 100.0f;
			this.dbゲージ増加量[nPlayer,1] = (float)(nGaugeRankValue[nPlayer] / 100.0f) * 0.5f;
			this.dbゲージ増加量[nPlayer,2] = (float)(nGaugeRankValue[nPlayer] / 100.0f) * dbDamageRate[nPlayer];
		}
		//2015.03.26 kairera0467 計算を初期化時にするよう修正。

		#region ゲージの丸め処理
		var increase = new float[,] { { dbゲージ増加量[0, 0], dbゲージ増加量[0, 1], dbゲージ増加量[0, 2] }, { dbゲージ増加量[1, 0], dbゲージ増加量[1, 1], dbゲージ増加量[1, 2] } };
		var increaseBranch = new float[2, 3, 3];

		for (int nPlayer = 0; nPlayer < TJAPlayer3.ConfigIni.nPlayerCount; nPlayer++)
		{
			for (int i = 0; i < 3; i++)
			{
				increaseBranch[nPlayer, i, 0] = dbゲージ増加量_Branch[nPlayer, i, 0];
				increaseBranch[nPlayer, i, 1] = dbゲージ増加量_Branch[nPlayer, i, 1];
				increaseBranch[nPlayer, i, 2] = dbゲージ増加量_Branch[nPlayer, i, 0];
			}
			switch (TJAPlayer3.DTX[nPlayer].GaugeIncreaseMode)
			{
				case GaugeIncreaseMode.Normal:
				case GaugeIncreaseMode.Floor:
					// 切り捨て
					for (int i = 0; i < 3; i++)
					{
						increase[nPlayer, i] = (float)Math.Truncate(increase[nPlayer, i] * 10000.0f) / 10000.0f;
					}
					for (int i = 0; i < 3; i++)
					{
						increaseBranch[nPlayer, i, 0] = (float)Math.Truncate(increaseBranch[nPlayer, i, 0] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 1] = (float)Math.Truncate(increaseBranch[nPlayer, i, 1] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 2] = (float)Math.Truncate(increaseBranch[nPlayer, i, 2] * 10000.0f) / 10000.0f;
					}
					break;
				case GaugeIncreaseMode.Round:
					// 四捨五入
					for (int i = 0; i < 3; i++)
					{
						increase[nPlayer, i] = (float)Math.Round(increase[nPlayer, i] * 10000.0f) / 10000.0f;
					}
					for (int i = 0; i < 3; i++)
					{
						increaseBranch[nPlayer, i, 0] = (float)Math.Round(increaseBranch[nPlayer, i, 0] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 1] = (float)Math.Round(increaseBranch[nPlayer, i, 1] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 2] = (float)Math.Round(increaseBranch[nPlayer, i, 2] * 10000.0f) / 10000.0f;
					}
					break;
				case GaugeIncreaseMode.Ceiling:
					// 切り上げ
					for (int i = 0; i < 3; i++)
					{
						increase[nPlayer, i] = (float)Math.Ceiling(increase[nPlayer, i] * 10000.0f) / 10000.0f;
					}
					for (int i = 0; i < 3; i++)
					{
						increaseBranch[nPlayer, i, 0] = (float)Math.Ceiling(increaseBranch[nPlayer, i, 0] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 1] = (float)Math.Ceiling(increaseBranch[nPlayer, i, 1] * 10000.0f) / 10000.0f;
						increaseBranch[nPlayer, i, 2] = (float)Math.Ceiling(increaseBranch[nPlayer, i, 2] * 10000.0f) / 10000.0f;
					}
					break;
				case GaugeIncreaseMode.NotFix:
				default:
					// 丸めない
					break;
			}

			for (int i = 0; i < 3; i++)
			{
				dbゲージ増加量[nPlayer, i] = increase[nPlayer, i];
			}
			for (int i = 0; i < 3; i++)
			{
				dbゲージ増加量_Branch[nPlayer, i, 0] = increaseBranch[nPlayer, i, 0];
				dbゲージ増加量_Branch[nPlayer, i, 1] = increaseBranch[nPlayer, i, 1];
				dbゲージ増加量_Branch[nPlayer, i, 2] = increaseBranch[nPlayer, i, 2];
			}
		}
		#endregion
	}

	#region [ DAMAGE ]
	#region [ DAMAGELEVELTUNING ]
	// ----------------------------------

	public float[,] dbゲージ増加量 = new float[2, 3];

	//譜面レベル, 判定
	public float[,,] dbゲージ増加量_Branch = new float[2 , 3, 3];


	public float[] fGaugeMaxRate = 
	{
		70.7f,//1～7
		70f,  //8
		75.0f //9～10
	};//おおよその値。

	// ----------------------------------
	#endregion

	public void Damage(int nHitCourse, EJudge e今回の判定, int nPlayer)//2020.04.25 Mr-Ojii akasoko26さんのコードをもとに変更
	{
		float fDamage;
		//現在のコースを当てるのではなくヒットしたノーツのコースを当ててあげる.2020.04.21.akasoko26
		var nコース = nHitCourse;


		switch ( e今回の判定 )
		{
			case EJudge.Perfect:
				{
					if( TJAPlayer3.DTX[nPlayer].bHasBranchChip )
					{
						fDamage = this.dbゲージ増加量_Branch[nPlayer, nコース, 0 ];
					}
					else
						fDamage = this.dbゲージ増加量[nPlayer, 0 ];
				}
				break;
			case EJudge.Good:
				{
					if( TJAPlayer3.DTX[nPlayer].bHasBranchChip )
					{
						fDamage = this.dbゲージ増加量_Branch[nPlayer, nコース, 1 ];
					}
					else
						fDamage = this.dbゲージ増加量[nPlayer, 1 ];
				}
				break;
			case EJudge.Bad:
			case EJudge.Miss:
				{
					if( TJAPlayer3.DTX[nPlayer].bHasBranchChip )
					{
						fDamage = this.dbゲージ増加量_Branch[nPlayer, nコース, 2 ];
					}
					else
						fDamage = this.dbゲージ増加量[nPlayer, 2 ];
					

					if( fDamage >= 0 )
					{
						fDamage = -fDamage;
					}

					if( this.bRisky )
					{
						this.nRiskyTimes--;
					}
				}

				break;

			default:
				{
					if( nPlayer == 0 ? TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] : TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[1] )
					{
						if( TJAPlayer3.DTX[nPlayer].bHasBranchChip )
						{
							fDamage = this.dbゲージ増加量_Branch[nPlayer, nコース, 0 ];
						}
						else
							fDamage = this.dbゲージ増加量[nPlayer, 0 ];
					}
					else
						fDamage = 0;
					break;
				}
		}
		

		this.db現在のゲージ値[ nPlayer ] = Math.Round(this.db現在のゲージ値[ nPlayer ] + fDamage, 5, MidpointRounding.ToEven);

		if (this.db現在のゲージ値[nPlayer] >= 100.0)
			this.db現在のゲージ値[nPlayer] = 100.0;
		else if (this.db現在のゲージ値[nPlayer] <= 0.0)
			this.db現在のゲージ値[nPlayer] = 0.0;

	}

	//-----------------
	#endregion

	public double[] db現在のゲージ値 = new double[ 4 ];
	protected CCounter ct炎;
	protected CCounter ct虹アニメ;
	protected CCounter ct虹透明度;
}
