using System;
using System.Collections.Generic;
using System.Text;
using FDK;
using System.Diagnostics;

namespace TJAPlayer3
{
	internal class CActPlayInfo : CActivity
	{
		// プロパティ

		public double dbBPM;
		public readonly int[] NowMeasure = new int[2];

		// コンストラクタ

		public CActPlayInfo()
		{
			base.b活性化してない = true;
		}

				
		// CActivity 実装

		public override void On活性化()
		{
			for (int i = 0; i < 2; i++)
			{
				NowMeasure[i] = 0;
			}
			this.dbBPM = TJAPlayer3.DTX[0].BASEBPM;
			base.On活性化();
		}
		public override int On進行描画()
		{
			throw new InvalidOperationException( "t進行描画(int x, int y) のほうを使用してください。" );
		}
		public void t進行描画( int x, int y )
		{
			if ( !base.b活性化してない )
			{
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("SCROLLMODE:    {0:####0}", Enum.GetName(typeof(EScrollMode), TJAPlayer3.ConfigIni.eScrollMode)));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("SCOREMODE:     {0:####0}", TJAPlayer3.DTX[0].nScoreModeTmp));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("SCROLL:        {0:####0.00}/{1:####0.00}", (TJAPlayer3.ConfigIni.n譜面スクロール速度[0] + 1) * 0.1, (TJAPlayer3.ConfigIni.n譜面スクロール速度[1] + 1) * 0.1));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("NoteC:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[3]));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("NoteM:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[2]));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("NoteE:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[1]));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("NoteN:         {0:####0}", TJAPlayer3.DTX[0].nノーツ数[0]));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("Frame:         {0:####0} fps", TJAPlayer3.FPS.nFPS));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("BPM:           {0:####0.0000}", this.dbBPM));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("Part:          {0:####0}/{1:####0}", NowMeasure[0], NowMeasure[1]));
				y += 15;
				int num = (TJAPlayer3.DTX[0].listChip.Count > 0) ? TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms : 0;
				string str = "Time:          " + ((((double)(CSoundManager.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0))) / 1000.0)).ToString("####0.00") + " / " + ((((double)num) / 1000.0)).ToString("####0.00");
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, str);
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.EFontType.白, string.Format("BGM/Taiko Adj: {0:####0}/{1:####0} ms", TJAPlayer3.DTX[0].nBGMAdjust, TJAPlayer3.ConfigIni.nInputAdjustTimeMs));
				y += 15;
				TJAPlayer3.act文字コンソール.tPrint( x, y, C文字コンソール.EFontType.白, string.Format( "Sound CPU :    {0:####0.00}%", TJAPlayer3.SoundManager.GetCPUusage() ) );

			}
		}
	}
}
