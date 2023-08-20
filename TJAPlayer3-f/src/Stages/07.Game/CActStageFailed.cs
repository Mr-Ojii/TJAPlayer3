using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using FDK;

namespace TJAPlayer3;

internal class CActStageFailed : CActivity
{
    // コンストラクタ

    public CActStageFailed()
    {
        ST文字位置[] st文字位置Array = new ST文字位置[11];

        ST文字位置 st文字位置 = new ST文字位置();
        st文字位置.ch = '0';
        st文字位置.pt = new Point(0, 0);
        st文字位置Array[0] = st文字位置;
        ST文字位置 st文字位置2 = new ST文字位置();
        st文字位置2.ch = '1';
        st文字位置2.pt = new Point(62, 0);
        st文字位置Array[1] = st文字位置2;
        ST文字位置 st文字位置3 = new ST文字位置();
        st文字位置3.ch = '2';
        st文字位置3.pt = new Point(124, 0);
        st文字位置Array[2] = st文字位置3;
        ST文字位置 st文字位置4 = new ST文字位置();
        st文字位置4.ch = '3';
        st文字位置4.pt = new Point(186, 0);
        st文字位置Array[3] = st文字位置4;
        ST文字位置 st文字位置5 = new ST文字位置();
        st文字位置5.ch = '4';
        st文字位置5.pt = new Point(248, 0);
        st文字位置Array[4] = st文字位置5;
        ST文字位置 st文字位置6 = new ST文字位置();
        st文字位置6.ch = '5';
        st文字位置6.pt = new Point(310, 0);
        st文字位置Array[5] = st文字位置6;
        ST文字位置 st文字位置7 = new ST文字位置();
        st文字位置7.ch = '6';
        st文字位置7.pt = new Point(372, 0);
        st文字位置Array[6] = st文字位置7;
        ST文字位置 st文字位置8 = new ST文字位置();
        st文字位置8.ch = '7';
        st文字位置8.pt = new Point(434, 0);
        st文字位置Array[7] = st文字位置8;
        ST文字位置 st文字位置9 = new ST文字位置();
        st文字位置9.ch = '8';
        st文字位置9.pt = new Point(496, 0);
        st文字位置Array[8] = st文字位置9;
        ST文字位置 st文字位置10 = new ST文字位置();
        st文字位置10.ch = '9';
        st文字位置10.pt = new Point(558, 0);
        st文字位置Array[9] = st文字位置10;
        ST文字位置 st文字位置11 = new ST文字位置();
        st文字位置11.ch = '%';
        st文字位置11.pt = new Point(558 + 62, 0);
        st文字位置Array[10] = st文字位置11;
        this.st文字位置 = st文字位置Array;
        base.b活性化してない = true;
    }


    // メソッド

    public void Start()
    {
        this.dbFailedTime = TJAPlayer3.Timer.n現在時刻ms;
        this.ct進行 = new CCounter(0, 1000, 2, TJAPlayer3.Timer);
        if (TJAPlayer3.ConfigToml.PlayOption._GameMode != EGame.OFF)
        {
            this.ct進行 = new CCounter(0, 4000, 2, TJAPlayer3.Timer);
        }
    }


    // CActivity 実装

    public override void On活性化()
    {
        this.b効果音再生済み = false;
        this.ct進行 = new CCounter();
        base.On活性化();
    }
    public override void On非活性化()
    {
        this.ct進行 = null;
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (base.b活性化してない)
        {
            return 0;
        }
        if ((this.ct進行 == null) || this.ct進行.b停止中)
        {
            return 0;
        }
        this.ct進行.t進行();

        if (TJAPlayer3.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー || TJAPlayer3.ConfigToml.PlayOption._GameMode == EGame.完走叩ききりまショー激辛)
        {
            if (TJAPlayer3.Tx.Tile_Black != null)
            {
                for (int i = 0; i <= (TJAPlayer3.app.LogicalSize.Width / 64); i++)
                {
                    for (int j = 0; j <= (TJAPlayer3.app.LogicalSize.Height / 64); j++)
                    {
                        TJAPlayer3.Tx.Tile_Black.t2D描画(TJAPlayer3.app.Device, i * 64, j * 64);
                    }
                }
            }
            if (this.ct進行.n現在の値 > 1500)
            {
                if (TJAPlayer3.Tx.Failed_Game != null)
                    TJAPlayer3.Tx.Failed_Game.t2D描画(TJAPlayer3.app.Device, 0, 0);

                int num = (TJAPlayer3.DTX[0].listChip.Count > 0) ? TJAPlayer3.DTX[0].listChip[TJAPlayer3.DTX[0].listChip.Count - 1].n発声時刻ms : 0;
                this.t文字表示(640, 520, (((this.dbFailedTime) / 1000.0) / (((double)num) / 1000.0) * 100).ToString("##0") + "%");
            }


        }
        else
        {
            if (this.ct進行.n現在の値 < 100)
            {
                int x = (int)(640.0 * Math.Cos((Math.PI / 2 * this.ct進行.n現在の値) / 100.0));
                if ((x != 640) && (TJAPlayer3.Tx.Failed_Stage != null))
                {
                    TJAPlayer3.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 0, 0, new Rectangle(x, 0, 640 - x, 720));
                    TJAPlayer3.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 640 + x, 0, new Rectangle(640, 0, 640 - x, 720));
                }
            }
            else
            {
                if (TJAPlayer3.Tx.Failed_Stage != null)
                {
                    TJAPlayer3.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, 0, 0);
                }
                if (this.ct進行.n現在の値 <= 250)
                {
                    int num2 = Random.Shared.Next(5) - 2;
                    int y = Random.Shared.Next(5) - 2;
                    if (TJAPlayer3.Tx.Failed_Stage != null)
                    {
                        TJAPlayer3.Tx.Failed_Stage.t2D描画(TJAPlayer3.app.Device, num2, y);
                    }
                }
                if (!this.b効果音再生済み)
                {
                    TJAPlayer3.Skin.SystemSounds[Eシステムサウンド.SOUNDステージ失敗音].t再生する();
                    this.b効果音再生済み = true;
                }
            }
        }

        if (!this.ct進行.b終了値に達した)
        {
            return 0;
        }
        return 1;
    }


    // その他

    #region [ private ]
    //-----------------
    private bool b効果音再生済み;
    private CCounter ct進行;
    private double dbFailedTime;
    //-----------------
    private ST文字位置[] st文字位置;

    [StructLayout(LayoutKind.Sequential)]
    public struct ST文字位置
    {
        public char ch;
        public Point pt;
    }

    private void t文字表示(int x, int y, string str)
    {
        foreach (char ch in str)
        {
            for (int i = 0; i < this.st文字位置.Length; i++)
            {
                if (this.st文字位置[i].ch == ch)
                {
                    Rectangle rectangle = new Rectangle(this.st文字位置[i].pt.X, this.st文字位置[i].pt.Y, 62, 80);
                    if (ch == '%')
                    {
                        rectangle.Width = 80;
                    }
                    if (TJAPlayer3.Tx.Balloon_Number_Roll != null)
                    {
                        TJAPlayer3.Tx.Balloon_Number_Roll.t2D描画(TJAPlayer3.app.Device, x - (62 * str.Length / 2), y, rectangle);
                    }
                    break;
                }
            }
            x += 62;
        }
    }


    #endregion
}
