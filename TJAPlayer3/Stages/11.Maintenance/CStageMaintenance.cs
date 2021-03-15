using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using FDK;
using DiscordRPC;

namespace TJAPlayer3
{
    class CStageMaintenance : CStage
    {
		// コンストラクタ

		public CStageMaintenance()
		{
			base.eStageID = CStage.EStage.Maintenance;
			base.b活性化してない = true;
		}
		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation("メンテナンスステージを活性化します。");
			Trace.Indent();
			try
			{
				TJAPlayer3.DiscordClient?.SetPresence(new RichPresence()
				{
					Details = "",
					State = "Maintenance",
					Timestamps = new Timestamps(TJAPlayer3.StartupTime),
					Assets = new Assets()
					{
						LargeImageKey = TJAPlayer3.LargeImageKey,
						LargeImageText = TJAPlayer3.LargeImageText,
					}
				});
				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation("メンテナンスの活性化を完了しました。");
				Trace.Unindent();
			}

		}

		public override void On非活性化()
		{
			Trace.TraceInformation("メンテナンスステージを非活性化します。");
			Trace.Indent();
			try
			{
			}
			finally
			{
				Trace.TraceInformation("メンテナンスステージの非活性化を完了しました。");
				Trace.Unindent();
			}
			base.On非活性化();
		}

		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				//表示用テクスチャの生成
				don = TJAPlayer3.ColorTexture("#ff4000", Width, Height);
				ka = TJAPlayer3.ColorTexture("#00c8ff", Width, Height);
				string[] txt = new string[4] { "左ふち", "左面", "右面", "右ふち" };
				for (int ind = 0; ind < 4; ind++)
				{
					str[ind] = TJAPlayer3.tCreateTexture(new CPrivateFastFont(TJAPlayer3.ConfigIni.FontName, fontsize).DrawPrivateFont(txt[ind], Color.White, Color.Black));
				}
				base.OnManagedリソースの作成();
			}

		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				//表示用テクスチャの解放
				TJAPlayer3.t安全にDisposeする(ref str);
				TJAPlayer3.t安全にDisposeする(ref don);
				TJAPlayer3.t安全にDisposeする(ref ka);
				base.OnManagedリソースの解放();
			}
		}

		public override int On進行描画()
		{
			if (base.b初めての進行描画)
			{
				base.b初めての進行描画 = false;
			}

			//入力信号に合わせて色を描画
			if (TJAPlayer3.Pad.b押された(EPad.LBlue))
				ka.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 4, Y);
			if (TJAPlayer3.Pad.b押された(EPad.LRed))
				don.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 3, Y);
			if (TJAPlayer3.Pad.b押された(EPad.RRed))
				don.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 2, Y);
			if (TJAPlayer3.Pad.b押された(EPad.RBlue))
				ka.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * 1, Y);
			if (TJAPlayer3.Pad.b押された(EPad.LBlue2P))
				ka.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 1, Y);
			if (TJAPlayer3.Pad.b押された(EPad.LRed2P))
				don.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 2, Y);
			if (TJAPlayer3.Pad.b押された(EPad.RRed2P))
				don.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 3, Y);
			if (TJAPlayer3.Pad.b押された(EPad.RBlue2P))
				ka.t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * 4, Y);

			for (int index = 0; index < 4; index++)
			{
				//文字の描画
				str[index].t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 - (Diff + Width) * (4 - index), strY);
				str[index].t2D描画(TJAPlayer3.app.Device, CTexture.RefPnt.Down, 640 + (Diff + Width) * (index + 1), strY);
			}

			if (TJAPlayer3.InputManager.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape))
				return 1;
			return 0;
		}

		#region[private]
		private CTexture don;
		private CTexture ka;
		private CTexture[] str = new CTexture[4];

		private const int Width = 100;
		private const int Height = 100;
		private const int Y = 550;
		private const int strY = 450;
		private const int fontsize = 20;

		private const int Diff = 16;
		#endregion
	}
}
