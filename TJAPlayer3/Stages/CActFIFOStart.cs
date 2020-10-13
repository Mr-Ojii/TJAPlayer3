using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FDK;

namespace TJAPlayer3
{
	internal class CActFIFOStart : CActivity
	{
		// メソッド

		public void tフェードアウト開始()
		{
			this.mode = EFIFOモード.フェードアウト;

			this.counter = new CCounter( 0, 1500, 1, TJAPlayer3.Timer );
		}
		public void tフェードイン開始()
		{
			this.mode = EFIFOモード.フェードイン;
			this.counter = new CCounter( 0, 1500, 1, TJAPlayer3.Timer );
		}

		// CActivity 実装

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
		public override int On進行描画()
		{
			if( base.b活性化してない || ( this.counter == null ) )
			{
				return 0;
			}
			this.counter.t進行();

			if (TJAPlayer3.ConfigIni.bEnableSkinV2)
			{
				if (TJAPlayer3.Tx.SongLoading_v2_BG != null)
				{
					if (this.mode == EFIFOモード.フェードアウト)
					{
						int x = Math.Max(1000 - this.counter.n現在の値, 0);
						int num = Math.Min(100, x);
						TJAPlayer3.Tx.SongLoading_v2_BG.t2D幕用描画(TJAPlayer3.app.Device, -x, 0, new Rectangle(0, 0, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Height), true, num);
						TJAPlayer3.Tx.SongLoading_v2_BG.t2D幕用描画(TJAPlayer3.app.Device, (TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2) + x, 0, new Rectangle(TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Height), false, num);
					}
					else
					{
						int x = Math.Max(this.counter.n現在の値 - 500, 0);
						int num = Math.Min(100, x);
						TJAPlayer3.Tx.SongLoading_v2_BG.t2D幕用描画(TJAPlayer3.app.Device, -x, 0, new Rectangle(0, 0, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Height), true, num);
						TJAPlayer3.Tx.SongLoading_v2_BG.t2D幕用描画(TJAPlayer3.app.Device, (TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2) + x, 0, new Rectangle(TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, 0, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Width / 2, TJAPlayer3.Tx.SongLoading_v2_BG.szテクスチャサイズ.Height), false, num);
					}
				}
			}
			else
			{
				if (this.mode == EFIFOモード.フェードアウト)
				{
					if (TJAPlayer3.Tx.SongLoading_FadeOut != null)
					{
						int y = this.counter.n現在の値 >= 840 ? 840 : this.counter.n現在の値;
						TJAPlayer3.Tx.SongLoading_FadeOut.t2D描画(TJAPlayer3.app.Device, 0, GameWindowSize.Height - y);
					}
				}
				else
				{
					if (TJAPlayer3.Tx.SongLoading_FadeIn != null)
					{

						int y = this.counter.n現在の値 >= 840 ? 840 : this.counter.n現在の値;
						TJAPlayer3.Tx.SongLoading_FadeIn.t2D描画(TJAPlayer3.app.Device, 0, 0 - y);
					}
				}
			}

			if( this.mode == EFIFOモード.フェードアウト )
			{
				if( this.counter.b終了値に達してない )
				{
					return 0;
				}
			}
			else if( this.mode == EFIFOモード.フェードイン )
			{
				if( this.counter.b終了値に達してない )
				{
					return 0;
				}
			}
			return 1;
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter counter;
		private EFIFOモード mode;
		//-----------------
		#endregion
	}
}
