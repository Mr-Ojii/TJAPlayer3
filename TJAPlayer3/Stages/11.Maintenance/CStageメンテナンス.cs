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
    class CStageメンテナンス : CStage
    {
		// コンストラクタ

		public CStageメンテナンス()
		{
			base.eステージID = CStage.Eステージ.メンテ;
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
						LargeImageKey = "tjaplayer3-f",
						LargeImageText = "Ver." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
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
				moji[0] = TJAPlayer3.tテクスチャの生成(new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), fontsize).DrawPrivateFont("左ふち", Color.White, Color.Black));
				moji[1] = TJAPlayer3.tテクスチャの生成(new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), fontsize).DrawPrivateFont("左面", Color.White, Color.Black));
				moji[2] = TJAPlayer3.tテクスチャの生成(new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), fontsize).DrawPrivateFont("右面", Color.White, Color.Black));
				moji[3] = TJAPlayer3.tテクスチャの生成(new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), fontsize).DrawPrivateFont("右ふち", Color.White, Color.Black));
				base.OnManagedリソースの作成();
			}

		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				//表示用テクスチャの解放
				TJAPlayer3.t安全にDisposeする(ref moji);
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
			if (TJAPlayer3.Pad.b押された(Eパッド.LBlue))
				ka.t2D下中央基準描画(TJAPlayer3.app.Device, 640 - (Sabunn + Width) * 4, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.LRed))
				don.t2D下中央基準描画(TJAPlayer3.app.Device, 640 - (Sabunn + Width) * 3, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.RRed))
				don.t2D下中央基準描画(TJAPlayer3.app.Device, 640 - (Sabunn + Width) * 2, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.RBlue))
				ka.t2D下中央基準描画(TJAPlayer3.app.Device, 640 - (Sabunn + Width) * 1, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.LBlue2P))
				ka.t2D下中央基準描画(TJAPlayer3.app.Device, 640 + (Sabunn + Width) * 1, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.LRed2P))
				don.t2D下中央基準描画(TJAPlayer3.app.Device, 640 + (Sabunn + Width) * 2, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.RRed2P))
				don.t2D下中央基準描画(TJAPlayer3.app.Device, 640 + (Sabunn + Width) * 3, y);
			if (TJAPlayer3.Pad.b押された(Eパッド.RBlue2P))
				ka.t2D下中央基準描画(TJAPlayer3.app.Device, 640 + (Sabunn + Width) * 4, y);



			for (int index = 0; index < 4; index++)
			{
				//文字の描画
				moji[index].t2D下中央基準描画(TJAPlayer3.app.Device, 640 - (Sabunn + Width) * (4 - index), mojiy);
				moji[index].t2D下中央基準描画(TJAPlayer3.app.Device, 640 + (Sabunn + Width) * (index + 1), mojiy);
			}

			if (TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape))
				return 1;
			return 0;
		}

		#region[private]
		private CTexture don;
		private CTexture ka;
		private CTexture[] moji = new CTexture[4];

		private const int Width = 100;
		private const int Height = 100;
		private const int y = 550;
		private const int mojiy = 450;
		private const int fontsize = 20;

		private const int Sabunn = 16;
		#endregion
	}
}
