using FDK;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace TJAPlayer3
{
	internal class CActSelect曲リスト : CActivity
	{
		// プロパティ

		public bool bIsEnumeratingSongs
		{
			get;
			set;
		}
		public bool bスクロール中
		{
			get
			{
				if (this.n目標のスクロールカウンタ == 0)
				{
					return (this.n現在のスクロールカウンタ != 0);
				}
				return true;
			}
		}
		public int[] n現在のアンカ難易度レベル
		{
			get;
			private set;
		}
		public int[] n現在選択中の曲の難易度レベル
		{
			get
			{
				return new int[] { this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(this.r現在選択中の曲, 0), this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(this.r現在選択中の曲 , 1) };
			}
		}
		public Cスコア r現在選択中のスコア
		{
			get
			{
				if (this.r現在選択中の曲 != null)
				{
					return this.r現在選択中の曲.arスコア;
				}
				return null;
			}
		}

		public C曲リストノード r現在選択中の曲
		{
			get;
			private set;
		}

		public int nスクロールバー相対y座標
		{
			get;
			private set;
		}

		// t選択曲が変更された()内で使う、直前の選曲の保持
		// (前と同じ曲なら選択曲変更に掛かる再計算を省略して高速化するため)
		private C曲リストノード song_last = null;

		// コンストラクタ

		public CActSelect曲リスト()
		{
			this.r現在選択中の曲 = null;
			n現在のアンカ難易度レベル = new int[2];
			for (int nPlayer = 0; nPlayer < 2; nPlayer++)
				this.n現在のアンカ難易度レベル[nPlayer] = TJAPlayer3.ConfigIni.nDefaultCourse;
			base.b活性化してない = true;
			this.bIsEnumeratingSongs = false;
		}


		// メソッド

		public int n現在のアンカ難易度レベルに最も近い難易度レベルを返す(C曲リストノード song, int nPlayer)
		{
			// 事前チェック。

			if (song == null)
				return this.n現在のアンカ難易度レベル[nPlayer];  // 曲がまったくないよ

			if (song.arスコア.譜面情報.b譜面が存在する[this.n現在のアンカ難易度レベル[nPlayer]] != false)
				return this.n現在のアンカ難易度レベル[nPlayer];  // 難易度ぴったりの曲があったよ

			if ((song.eノード種別 == C曲リストノード.Eノード種別.BOX) || (song.eノード種別 == C曲リストノード.Eノード種別.BACKBOX))
				return 0;                               // BOX と BACKBOX は関係無いよ


			// 現在のアンカレベルから、難易度上向きに検索開始。

			int n最も近いレベル = this.n現在のアンカ難易度レベル[nPlayer];

			for (int i = 0; i < (int)Difficulty.Total; i++)
			{
				if (song.arスコア.譜面情報.b譜面が存在する[n最も近いレベル] != false)
					break;  // 曲があった。

				n最も近いレベル = (n最も近いレベル + 1) % (int)Difficulty.Total;  // 曲がなかったので次の難易度レベルへGo。（5以上になったら0に戻る。）
			}


			// 見つかった曲がアンカより下のレベルだった場合……
			// アンカから下向きに検索すれば、もっとアンカに近い曲があるんじゃね？

			if (n最も近いレベル < this.n現在のアンカ難易度レベル[nPlayer])
			{
				// 現在のアンカレベルから、難易度下向きに検索開始。

				n最も近いレベル = this.n現在のアンカ難易度レベル[nPlayer];

				for (int i = 0; i < (int)Difficulty.Total; i++)
				{
					if (song.arスコア.譜面情報.b譜面が存在する[n最も近いレベル] != false)
						break;  // 曲があった。

					n最も近いレベル = ((n最も近いレベル - 1) + (int)Difficulty.Total) % (int)Difficulty.Total;    // 曲がなかったので次の難易度レベルへGo。（0未満になったら4に戻る。）
				}
			}

			return n最も近いレベル;
		}
		public C曲リストノード r指定された曲が存在するリストの先頭の曲(C曲リストノード song)
		{
			List<C曲リストノード> songList = GetSongListWithinMe(song);
			return (songList == null) ? null : songList[0];
		}
		public C曲リストノード r指定された曲が存在するリストの末尾の曲(C曲リストノード song)
		{
			List<C曲リストノード> songList = GetSongListWithinMe(song);
			return (songList == null) ? null : songList[songList.Count - 1];
		}

		private List<C曲リストノード> GetSongListWithinMe(C曲リストノード song)
		{
			if (song.r親ノード == null)                 // root階層のノートだったら

			{
				return TJAPlayer3.Songs管理.list曲ルート; // rootのリストを返す
			}
			else
			{
				if ((song.r親ノード.list子リスト != null) && (song.r親ノード.list子リスト.Count > 0))
				{
					return song.r親ノード.list子リスト;
				}
				else
				{
					return null;
				}
			}
		}


		public delegate void DGSortFunc(List<C曲リストノード> songList, int order, params object[] p);
		/// <summary>
		/// 主にCSong管理.cs内にあるソート機能を、delegateで呼び出す。
		/// </summary>
		/// <param name="sf">ソート用に呼び出すメソッド</param>
		/// <param name="order">-1=降順, 1=昇順</param>
		public void t曲リストのソート(DGSortFunc sf, int order, params object[] p)
		{
			List<C曲リストノード> songList = GetSongListWithinMe(this.r現在選択中の曲);
			if (songList == null)
			{
				// 何もしない;
			}
			else
			{
				//				CDTXMania.Songs管理.t曲リストのソート3_演奏回数の多い順( songList, eInst, order );
				sf(songList, order, p);
				//				this.r現在選択中の曲 = CDTXMania
				this.t現在選択中の曲を元に曲バーを再構成する();
			}
		}

		public void RandomSelect(C曲リストノード c曲) {
			this.r現在選択中の曲 = c曲;
			this.t現在選択中の曲を元に曲バーを再構成する();

			this.t選択曲が変更された(false);                                 // #27648 項目数変更を反映させる
            this.b選択曲が変更された = true;

			TJAPlayer3.stage選曲.t選択曲変更通知();
		}

		public bool tBOXに入る()
		{
			
			bool ret = false;
			if (CSkin.GetSkinName(TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName(false)) != CSkin.GetSkinName(this.r現在選択中の曲.strSkinPath)
				&& CSkin.bUseBoxDefSkin)
			{
				ret = true;
				// BOXに入るときは、スキン変更発生時のみboxdefスキン設定の更新を行う
				TJAPlayer3.Skin.SetCurrentSkinSubfolderFullName(
					TJAPlayer3.Skin.GetSkinSubfolderFullNameFromSkinName(CSkin.GetSkinName(this.r現在選択中の曲.strSkinPath)), false);
			}
			if (TJAPlayer3.ConfigIni.OpenOneSide) {
				List<C曲リストノード> list = TJAPlayer3.Songs管理.list曲ルート;
				list.InsertRange(list.IndexOf(this.r現在選択中の曲) + 1, this.r現在選択中の曲.list子リスト);
				int n回数 = this.r現在選択中の曲.Openindex;
				for (int index = 0; index <= n回数; index++)
					this.r現在選択中の曲 = this.r次の曲(r現在選択中の曲);
				list.RemoveAt(list.IndexOf(this.r現在選択中の曲.r親ノード));
				this.t現在選択中の曲を元に曲バーを再構成する();
				this.t選択曲が変更された(false);                                 // #27648 項目数変更を反映させる
				this.b選択曲が変更された = true;
			}
			else
			{

				if ((this.r現在選択中の曲.list子リスト != null) && (this.r現在選択中の曲.list子リスト.Count > 0))
				{
					if (this.r現在選択中の曲.list子リスト.Count > 1 && this.r現在選択中の曲.Openindex < this.r現在選択中の曲.list子リスト.Count)//見た目だけだと気づきにくいが、Indexがずれてるので、[1]にする。閉じるだけのものは、[0]にする。
						this.r現在選択中の曲 = this.r現在選択中の曲.list子リスト[this.r現在選択中の曲.Openindex];
					else
						this.r現在選択中の曲 = this.r現在選択中の曲.list子リスト[0];
					this.t現在選択中の曲を元に曲バーを再構成する();
					this.t選択曲が変更された(false);                                 // #27648 項目数変更を反映させる
					this.b選択曲が変更された = true;
				}
			}
			TJAPlayer3.stage選曲.t選択曲変更通知();
			return ret;
		}
		public bool tBOXを出る()
		{
			bool ret = false;
			if (CSkin.GetSkinName(TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName(false)) != CSkin.GetSkinName(this.r現在選択中の曲.strSkinPath)
				&& CSkin.bUseBoxDefSkin)
			{
				ret = true;
			}
			// スキン変更が発生しなくても、boxdef圏外に出る場合は、boxdefスキン設定の更新が必要
			// (ユーザーがboxdefスキンをConfig指定している場合への対応のために必要)
			// tBoxに入る()とは処理が微妙に異なるので注意
			TJAPlayer3.Skin.SetCurrentSkinSubfolderFullName(
				(this.r現在選択中の曲.strSkinPath == "") ? "" : TJAPlayer3.Skin.GetSkinSubfolderFullNameFromSkinName(CSkin.GetSkinName(this.r現在選択中の曲.strSkinPath)), false);
			if (TJAPlayer3.ConfigIni.OpenOneSide) {
				List<C曲リストノード> list = TJAPlayer3.Songs管理.list曲ルート;
				this.r現在選択中の曲.r親ノード.Openindex = r現在選択中の曲.r親ノード.list子リスト.IndexOf(this.r現在選択中の曲);
				list.Insert(list.IndexOf(this.r現在選択中の曲) + 1, this.r現在選択中の曲.r親ノード);
				this.r現在選択中の曲 = this.r次の曲(r現在選択中の曲);
				for (int index = 0; index < list.Count; index++) {
					if (this.r現在選択中の曲.list子リスト.Contains(list[index]))
					{
						list.RemoveAt(index); 
						index--;
					}

				}
				this.t現在選択中の曲を元に曲バーを再構成する();
				this.t選択曲が変更された(false);                                 // #27648 項目数変更を反映させる
				this.b選択曲が変更された = true;
			}
			else
			{
				if (this.r現在選択中の曲.r親ノード != null)
				{
					List<C曲リストノード> list = r現在選択中の曲.r親ノード.list子リスト;//t前の曲 t次の曲から、流用。
					this.r現在選択中の曲.r親ノード.Openindex = r現在選択中の曲.r親ノード.list子リスト.IndexOf(this.r現在選択中の曲);
					this.r現在選択中の曲 = this.r現在選択中の曲.r親ノード;
					this.t現在選択中の曲を元に曲バーを再構成する();
					this.t選択曲が変更された(false);                                 // #27648 項目数変更を反映させる
					this.b選択曲が変更された = true;
				}
			}
			TJAPlayer3.stage選曲.t選択曲変更通知();
			return ret;
		}
		public void t現在選択中の曲を元に曲バーを再構成する()
		{
			this.tバーの初期化();
		}
		public void t次に移動()
		{
			if (this.r現在選択中の曲 != null)
			{
				this.n目標のスクロールカウンタ += 100;
			}
			this.b選択曲が変更された = true;
		}
		public void t前に移動()
		{
			if (this.r現在選択中の曲 != null)
			{
				this.n目標のスクロールカウンタ -= 100;
			}
			this.b選択曲が変更された = true;
		}
		public void tかなり次に移動()
		{
			if (this.r現在選択中の曲 != null)
			{
				for (int i = 0; i < TJAPlayer3.ConfigIni.SongSelectSkipCount; i++)
					this.r現在選択中の曲 = r次の曲(r現在選択中の曲);
			}
			this.t現在選択中の曲を元に曲バーを再構成する();
			this.t選択曲が変更された(false);
			this.b選択曲が変更された = true;
			TJAPlayer3.stage選曲.t選択曲変更通知();
		}
		public void tかなり前に移動()
		{
			if (this.r現在選択中の曲 != null)
			{
				for (int i = 0; i < TJAPlayer3.ConfigIni.SongSelectSkipCount; i++)
					this.r現在選択中の曲 = r前の曲(r現在選択中の曲);
			}
			this.t現在選択中の曲を元に曲バーを再構成する();
			this.t選択曲が変更された(false);
			this.b選択曲が変更された = true;
			TJAPlayer3.stage選曲.t選択曲変更通知();
		}
		public void t難易度レベルをひとつ進める(int nPlayer)
		{
			if ((this.r現在選択中の曲 == null) || (this.r現在選択中の曲.nスコア数 <= 1))
				return;     // 曲にスコアが０～１個しかないなら進める意味なし。


			// 難易度レベルを＋１し、現在選曲中のスコアを変更する。

			this.n現在のアンカ難易度レベル[nPlayer] = this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(this.r現在選択中の曲, nPlayer);

			for (int i = 0; i < (int)Difficulty.Total; i++)
			{
				this.n現在のアンカ難易度レベル[nPlayer] = (this.n現在のアンカ難易度レベル[nPlayer] + 1) % (int)Difficulty.Total;  // ５以上になったら０に戻る。
				if (this.r現在選択中の曲.arスコア.譜面情報.b譜面が存在する[this.n現在のアンカ難易度レベル[nPlayer]] != false)    // 曲が存在してるならここで終了。存在してないなら次のレベルへGo。
					break;
			}
		}
		/// <summary>
		/// 不便だったから作った。
		/// </summary>
		public void t難易度レベルをひとつ戻す(int nPlayer)
		{
			if ((this.r現在選択中の曲 == null) || (this.r現在選択中の曲.nスコア数 <= 1))
				return;     // 曲にスコアが０～１個しかないなら進める意味なし。


			// 難易度レベルを＋１し、現在選曲中のスコアを変更する。

			this.n現在のアンカ難易度レベル[nPlayer] = this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(this.r現在選択中の曲, nPlayer);

			this.n現在のアンカ難易度レベル[nPlayer]--;
			if (this.n現在のアンカ難易度レベル[nPlayer] < 0) // 0より下になったら4に戻す。
			{
				this.n現在のアンカ難易度レベル[nPlayer] = 4;
			}

			//2016.08.13 kairera0467 かんたん譜面が無い譜面(ふつう、むずかしいのみ)で、難易度を最上位に戻せない不具合の修正。
			bool bLabel0NotFound = true;
			for (int i = this.n現在のアンカ難易度レベル[nPlayer]; i >= 0; i--)
			{
				if (this.r現在選択中の曲.arスコア.譜面情報.b譜面が存在する[i] != false)
				{
					this.n現在のアンカ難易度レベル[nPlayer] = i;
					bLabel0NotFound = false;
					break;
				}
			}
			if (bLabel0NotFound)
			{
				for (int i = 4; i >= 0; i--)
				{
					if (this.r現在選択中の曲.arスコア.譜面情報.b譜面が存在する[i] != false)
					{
						this.n現在のアンカ難易度レベル[nPlayer] = i;
						break;
					}
				}
			}
		}


		/// <summary>
		/// 曲リストをリセットする
		/// </summary>
		/// <param name="cs"></param>
		public void Refresh(CSongs管理 cs, bool bRemakeSongTitleBar)      // #26070 2012.2.28 yyagi
		{
			//			this.On非活性化();

			if (cs != null && cs.list曲ルート.Count > 0)    // 新しい曲リストを検索して、1曲以上あった
			{
				TJAPlayer3.Songs管理 = cs;
				if (this.r現在選択中の曲 != null)          // r現在選択中の曲==null とは、「最初songlist.dbが無かった or 検索したが1曲もない」
				{
					this.r現在選択中の曲 = searchCurrentBreadcrumbsPosition(TJAPlayer3.Songs管理.list曲ルート, this.r現在選択中の曲.strBreadcrumbs);
					if (bRemakeSongTitleBar)                    // 選曲画面以外に居るときには再構成しない (非活性化しているときに実行すると例外となる)
					{
						this.t現在選択中の曲を元に曲バーを再構成する();
					}
					return;
				}
				
			}
			if (this.b活性化してる)
			{
				this.On非活性化();
				this.r現在選択中の曲 = null;
				this.On活性化();
			}
		}


		/// <summary>
		/// 現在選曲している位置を検索する
		/// (曲一覧クラスを新しいものに入れ替える際に用いる)
		/// </summary>
		/// <param name="ln">検索対象のList</param>
		/// <param name="bc">検索するパンくずリスト(文字列)</param>
		/// <returns></returns>
		private C曲リストノード searchCurrentBreadcrumbsPosition(List<C曲リストノード> ln, string bc)
		{
			foreach (C曲リストノード n in ln)
			{
				if (n.strBreadcrumbs == bc)
				{
					return n;
				}
				else if (n.list子リスト != null && n.list子リスト.Count > 0)    // 子リストが存在するなら、再帰で探す
				{
					C曲リストノード r = searchCurrentBreadcrumbsPosition(n.list子リスト, bc);
					if (r != null) return r;
				}
			}
			return null;
		}

		/// <summary>
		/// BOXのアイテム数と、今何番目を選択しているかをセットする
		/// </summary>
		public void t選択曲が変更された(bool bForce) // #27648
		{
			C曲リストノード song = TJAPlayer3.stage選曲.act曲リスト.r現在選択中の曲;
			if (song == null)
				return;
			if (song == song_last && bForce == false)
				return;

			song_last = song;

			List<C曲リストノード> list = (song.r親ノード != null && !TJAPlayer3.ConfigIni.OpenOneSide) ? song.r親ノード.list子リスト : TJAPlayer3.Songs管理.list曲ルート;
			int index = list.IndexOf(song) + 1;
			if (index <= 0)
			{
				nCurrentPosition = nNumOfItems = 0;
			}
			else
			{
				nCurrentPosition = index;
				nNumOfItems = list.Count;
			}
			TJAPlayer3.stage選曲.act演奏履歴パネル.tSongChange();
		}

		// CActivity 実装

		public override void On活性化()
		{
			if (this.b活性化してる)
				return;

			// Reset to not performing calibration each time we
			// enter or return to the song select screen.
			TJAPlayer3.IsPerformingCalibration = false;

			if (!string.IsNullOrEmpty(TJAPlayer3.ConfigIni.FontName))
			{
				this.pfMusicName = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 30);
				this.pfSubtitle = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 23);
			}

			this.b登場アニメ全部完了 = false;
			this.n目標のスクロールカウンタ = 0;
			this.n現在のスクロールカウンタ = 0;
			this.nスクロールタイマ = -1;

			// フォント作成。
			// 曲リスト文字は２倍（面積４倍）でテクスチャに描画してから縮小表示するので、フォントサイズは２倍とする。

			FontStyle regular = FontStyle.Regular;
			this.ft曲リスト用フォント = new Font(TJAPlayer3.ConfigIni.FontName, 40f, regular, GraphicsUnit.Pixel);


			// 現在選択中の曲がない（＝はじめての活性化）なら、現在選択中の曲をルートの先頭ノードに設定する。

			if ((this.r現在選択中の曲 == null) && (TJAPlayer3.Songs管理.list曲ルート.Count > 0))
				this.r現在選択中の曲 = TJAPlayer3.Songs管理.list曲ルート[0];




			// バー情報を初期化する。

			this.tバーの初期化();

			this.ct三角矢印アニメ = new CCounter();
			this.ct分岐フェード用タイマー = new CCounter(1, 2, 2500, TJAPlayer3.Timer);
			this.ctバー展開用タイマー= new CCounter(0, 100, 1, TJAPlayer3.Timer);
			this.ctバー展開ディレイ用タイマー = new CCounter(0, 200, 1, TJAPlayer3.Timer);

			base.On活性化();

			this.t選択曲が変更された(true);      // #27648 2012.3.31 yyagi 選曲画面に入った直後の 現在位置/全アイテム数 の表示を正しく行うため
		}
		public override void On非活性化()
		{
			if (this.b活性化してない)
				return;

			for (int i = 0; i < 13; i++) {
				this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);
			}

			TJAPlayer3.t安全にDisposeする(ref pfMusicName);
			TJAPlayer3.t安全にDisposeする(ref pfSubtitle);

			TJAPlayer3.t安全にDisposeする(ref this.ft曲リスト用フォント);

			for (int i = 0; i < 13; i++)
				this.ct登場アニメ用[i] = null;

			this.ct三角矢印アニメ = null;



			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if (this.b活性化してない)
				return;


			for (int i = 0; i < 11; i++)
			{
				if (i == 10)
				{
					this.genretext[i] = GenerateGenreTexture("とじる", Color.White, Color.Black);

				}
				else if (i == 9)
				{
					this.genretext[i] = GenerateGenreTexture("ランダム", Color.White, Color.Black);

				}
				else
				{
					this.genretext[i] = GenerateGenreTexture(this.NumtonStrジャンル(i), Color.White, Color.Black);
				}
			}


			int c = (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja") ? 0 : 1;
			#region [ Songs not found画像 ]
			try
			{
				using (Bitmap image = new Bitmap(640, 128))
				using (Graphics graphics = Graphics.FromImage(image))
				{
					string[] s1 = { "曲データが見つかりません。", "Songs not found." };
					string[] s2 = { "曲データをDTXManiaGR.exe以下の", "You need to install songs." };
					string[] s3 = { "フォルダにインストールして下さい。", "" };
					graphics.DrawString(s1[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float)2f, (float)2f);
					graphics.DrawString(s1[c], this.ft曲リスト用フォント, Brushes.White, (float)0f, (float)0f);
					graphics.DrawString(s2[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float)2f, (float)44f);
					graphics.DrawString(s2[c], this.ft曲リスト用フォント, Brushes.White, (float)0f, (float)42f);
					graphics.DrawString(s3[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float)2f, (float)86f);
					graphics.DrawString(s3[c], this.ft曲リスト用フォント, Brushes.White, (float)0f, (float)84f);

					this.txSongNotFound = TJAPlayer3.tテクスチャの生成(image, true);

					this.txSongNotFound.vc拡大縮小倍率 = new Vector3(0.5f, 0.5f, 1f); // 半分のサイズで表示する。
				}
			}
			catch (CTextureCreateFailedException e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("SoungNotFoundテクスチャの作成に失敗しました。");
				this.txSongNotFound = null;
			}
			#endregion
			#region [ "曲データを検索しています"画像 ]
			try
			{
				using (Bitmap image = new Bitmap(640, 96))
				using (Graphics graphics = Graphics.FromImage(image))
				{
					string[] s1 = { "曲データを検索しています。", "Now enumerating songs." };
					string[] s2 = { "そのまましばらくお待ち下さい。", "Please wait..." };
					graphics.DrawString(s1[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float)2f, (float)2f);
					graphics.DrawString(s1[c], this.ft曲リスト用フォント, Brushes.White, (float)0f, (float)0f);
					graphics.DrawString(s2[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float)2f, (float)44f);
					graphics.DrawString(s2[c], this.ft曲リスト用フォント, Brushes.White, (float)0f, (float)42f);

					this.txEnumeratingSongs = TJAPlayer3.tテクスチャの生成(image, true);

					this.txEnumeratingSongs.vc拡大縮小倍率 = new Vector3(0.5f, 0.5f, 1f); // 半分のサイズで表示する。
				}
			}
			catch (CTextureCreateFailedException e)
			{
				Trace.TraceError(e.ToString());
				Trace.TraceError("txEnumeratingSongsテクスチャの作成に失敗しました。");
				this.txEnumeratingSongs = null;
			}
			#endregion
			#region [ 曲数表示 ]
			//this.txアイテム数数字 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\ScreenSelect skill number on gauge etc.png" ), false );
			#endregion
			base.OnManagedリソースの作成();
		}
		public override void OnManagedリソースの解放()
		{
			if (this.b活性化してない)
				return;

			//CDTXMania.t安全にDisposeする( ref this.txアイテム数数字 );

			ClearTitleTextureCache();

			for (int i = 0; i < 13; i++)
			{
				this.stバー情報[i].ttkタイトル = null;
			}

			//CDTXMania.t安全にDisposeする( ref this.txスキル数字 );
			TJAPlayer3.t安全にDisposeする(ref this.txEnumeratingSongs);
			TJAPlayer3.t安全にDisposeする(ref this.txSongNotFound);
			TJAPlayer3.t安全にDisposeする(ref this.genretext);
			//CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Score );
			//CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Box );
			//CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Other );
			//CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Score );
			//CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Box );
			//CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Other );

			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_JPOP );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_アニメ );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_ゲーム );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_ナムコ );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_クラシック );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_どうよう );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_バラエティ );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー_ボカロ );
			//CDTXMania.t安全にDisposeする( ref this.tx曲バー );
			//CDTXMania.t安全にDisposeする( ref this.tx譜面分岐曲バー用 );

			//for( int i = 0; i < 5; i++ )
			//   {
			//       CDTXMania.t安全にDisposeする( ref this.tx曲バー_難易度[ i ] );
			//   }

			//   CDTXMania.t安全にDisposeする( ref this.tx難易度パネル );
			//   CDTXMania.t安全にDisposeする( ref this.txバー中央 );
			//   CDTXMania.t安全にDisposeする( ref this.tx難易度星 );
			//   CDTXMania.t安全にDisposeする( ref this.tx譜面分岐中央パネル用 );
			//   CDTXMania.t安全にDisposeする( ref this.tx上部ジャンル名 );
			//   CDTXMania.t安全にDisposeする( ref this.txレベル数字フォント );

			//   CDTXMania.t安全にDisposeする( ref this.txカーソル左 );
			//   CDTXMania.t安全にDisposeする( ref this.txカーソル右 );

			base.OnManagedリソースの解放();
		}
		public override int On進行描画()
		{
			if (this.b活性化してない)
				return 0;

			#region [ 初めての進行描画 ]
			//-----------------
			if (this.b初めての進行描画)
			{
				for (int i = 0; i < 13; i++)
					this.ct登場アニメ用[i] = new CCounter(-i * 10, 100, 3, TJAPlayer3.Timer);

				this.nスクロールタイマ = (long)(CSound管理.rc演奏用タイマ.n現在時刻ms * (((double)TJAPlayer3.ConfigIni.n演奏速度) / 20.0));
				TJAPlayer3.stage選曲.t選択曲変更通知();

				this.ct三角矢印アニメ.t開始(0, 1000, 1, TJAPlayer3.Timer);
				this.ct分岐フェード用タイマー.t進行();
				base.b初めての進行描画 = false;
			}
			//-----------------
			#endregion


			// まだ選択中の曲が決まってなければ、曲ツリールートの最初の曲にセットする。

			if ((this.r現在選択中の曲 == null) && (TJAPlayer3.Songs管理.list曲ルート.Count > 0))
				this.r現在選択中の曲 = TJAPlayer3.Songs管理.list曲ルート[0];


			// 本ステージは、(1)登場アニメフェーズ → (2)通常フェーズ　と二段階にわけて進む。

			//追加
			if (n現在のスクロールカウンタ == 0) this.ctバー展開ディレイ用タイマー.t進行();
			else { this.ctバー展開ディレイ用タイマー.n現在の値 = 0; this.ctバー展開ディレイ用タイマー.t時間Reset(); }

			if (ctバー展開ディレイ用タイマー.b終了値に達した) this.ct分岐フェード用タイマー.t進行Loop();
			else this.ct分岐フェード用タイマー.n現在の値 = 1;

			if (ctバー展開ディレイ用タイマー.b終了値に達した) this.ctバー展開用タイマー.t進行();
			else { this.ctバー展開用タイマー.n現在の値 = 0; this.ctバー展開用タイマー.t時間Reset(); }


			//展開アニメ用
			TJAPlayer3.Tx.SongSelect_Bar_Center.vc拡大縮小倍率.X = this.ctバー展開用タイマー.n現在の値 / 100f;

			for(int i=0;i< TJAPlayer3.Tx.SongSelect_Box_Center_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Box_Center_Genre[i].vc拡大縮小倍率.X = this.ctバー展開用タイマー.n現在の値 / 100f;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[i].vc拡大縮小倍率.X = this.ctバー展開用タイマー.n現在の値 / 100f;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[i] != null)
				{
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[i].vc拡大縮小倍率.X = this.ctバー展開用タイマー.n現在の値 / 100f;
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[i].vc拡大縮小倍率.Y = Math.Max(this.ctバー展開用タイマー.n現在の値 - (this.ctバー展開用タイマー.n終了値 / 2),0) / 50f;
				}
			//-----


			//難易度選択画面フェード用

			int 全体Opacity;
			if (TJAPlayer3.stage選曲.現在の選曲画面状況 == CStage選曲.E選曲画面.難易度選択In) 
			{
				全体Opacity = (int)(255.0f - (TJAPlayer3.stage選曲.ct難易度選択画面IN用タイマー.n現在の値 * 255.0f / TJAPlayer3.stage選曲.ct難易度選択画面IN用タイマー.n終了値));
			}
			else if(TJAPlayer3.stage選曲.現在の選曲画面状況 == CStage選曲.E選曲画面.難易度選択Out)
			{
				全体Opacity = (int)(TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n現在の値 * 255.0f / TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n終了値);
			}
			else 
			{
				全体Opacity = 255;
			}

			//しょうがないから、この曲リストで、描画するすべてのテクスチャのOpacityをここで決める。
			//その他のいい方法あったら、プルリクお願いします。受け入れます。(CActSelect曲リスト全体の透明度変更できたらいいな。)
			if (TJAPlayer3.Tx.SongSelect_Branch_Text_NEW != null)
				TJAPlayer3.Tx.SongSelect_Branch_Text_NEW.Opacity = Math.Min((int)((ct分岐フェード用タイマー.n現在の値 % 2) * 255.0), 全体Opacity);

			if (TJAPlayer3.Tx.SongSelect_PapaMama != null)
				TJAPlayer3.Tx.SongSelect_PapaMama.Opacity = Math.Min((int)((ct分岐フェード用タイマー.n現在の値 % 2) * 255.0), 全体Opacity);

			if (TJAPlayer3.Tx.SongSelect_Cursor_Left != null)
				TJAPlayer3.Tx.SongSelect_Cursor_Left.Opacity = Math.Min(255 - (ct三角矢印アニメ.n現在の値 * 255 / ct三角矢印アニメ.n終了値),全体Opacity);

			if (TJAPlayer3.Tx.SongSelect_Cursor_Right != null)
				TJAPlayer3.Tx.SongSelect_Cursor_Right.Opacity = Math.Min(255 - (ct三角矢印アニメ.n現在の値 * 255 / ct三角矢印アニメ.n終了値),全体Opacity);

			if (TJAPlayer3.Tx.SongSelect_Bar_Center != null)
				TJAPlayer3.Tx.SongSelect_Bar_Center.Opacity = 全体Opacity; 

			if (TJAPlayer3.Tx.SongSelect_Frame_Score != null)
				TJAPlayer3.Tx.SongSelect_Frame_Score.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Frame_BackBox != null)
				TJAPlayer3.Tx.SongSelect_Frame_BackBox.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Frame_Random != null)
				TJAPlayer3.Tx.SongSelect_Frame_Random.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Level != null)
				TJAPlayer3.Tx.SongSelect_Level.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Branch_Text != null)
				TJAPlayer3.Tx.SongSelect_Branch_Text.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Branch != null)
				TJAPlayer3.Tx.SongSelect_Branch.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.Crown_t != null)
				TJAPlayer3.Tx.Crown_t.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.DanC_Crown_t != null)
				TJAPlayer3.Tx.DanC_Crown_t.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.Difficulty_Icons != null)
				TJAPlayer3.Tx.Difficulty_Icons.Opacity = 全体Opacity;

			if (TJAPlayer3.Tx.SongSelect_Bar_BackBox != null)
				TJAPlayer3.Tx.SongSelect_Bar_BackBox.Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Lyric_Text.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Lyric_Text[i] != null)
					TJAPlayer3.Tx.SongSelect_Lyric_Text[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Box_Center_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Box_Center_Genre[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Bar_Box_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Bar_Box_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Bar_Box_Genre[i].Opacity = 全体Opacity;

			for (int i = 0; i < TJAPlayer3.Tx.SongSelect_Bar_Genre.Length; i++)
				if (TJAPlayer3.Tx.SongSelect_Bar_Genre[i] != null)
					TJAPlayer3.Tx.SongSelect_Bar_Genre[i].Opacity = 全体Opacity;
			//------------ぐちゃぐちゃで意味わからんよね。ごめんね。
			//こう見てみると、意外にテクスチャ少なめ？


			// 進行。
			if (n現在のスクロールカウンタ == 0) ct三角矢印アニメ.t進行Loop();
			else ct三角矢印アニメ.n現在の値 = 0;


			if (!this.b登場アニメ全部完了)
			{
				#region [ (1) 登場アニメフェーズの進行。]
				//-----------------
				for (int i = 0; i < 13; i++)    // パネルは全13枚。
				{
					this.ct登場アニメ用[i].t進行();

					if (this.ct登場アニメ用[i].b終了値に達した)
						this.ct登場アニメ用[i].t停止();
				}

				// 全部の進行が終わったら、this.b登場アニメ全部完了 を true にする。

				this.b登場アニメ全部完了 = true;
				for (int i = 0; i < 13; i++)    // パネルは全13枚。
				{
					if (this.ct登場アニメ用[i].b進行中)
					{
						this.b登場アニメ全部完了 = false;    // まだ進行中のアニメがあるなら false のまま。
						break;
					}
				}
				//-----------------
				#endregion
			}
			else
			{
				#region [ (2) 通常フェーズの進行。]
				//-----------------
				long n現在時刻 = CSound管理.rc演奏用タイマ.n現在時刻ms;

				if (n現在時刻 < this.nスクロールタイマ) // 念のため
					this.nスクロールタイマ = n現在時刻;

				const int nアニメ間隔 = 2;
				while ((n現在時刻 - this.nスクロールタイマ) >= nアニメ間隔)
				{
					int n加速度 = 1;
					int n残距離 = Math.Abs((int)(this.n目標のスクロールカウンタ - this.n現在のスクロールカウンタ));

					#region [ 残距離が遠いほどスクロールを速くする（＝n加速度を多くする）。]
					//-----------------
					if (n残距離 <= 10)
					{
						n加速度 = 1;
					}
					else if (n残距離 <= 100)
					{
						n加速度 = 2;
					}
					else if (n残距離 <= 300)
					{
						n加速度 = 3;
					}
					else if (n残距離 <= 500)
					{
						n加速度 = 4;
					}
					else
					{
						n加速度 = 8;
					}
					//-----------------
					#endregion

					#region [ 加速度を加算し、現在のスクロールカウンタを目標のスクロールカウンタまで近づける。 ]
					//-----------------
					if (this.n現在のスクロールカウンタ < this.n目標のスクロールカウンタ)        // (A) 正の方向に未達の場合：
					{
						this.n現在のスクロールカウンタ += n加速度;                             // カウンタを正方向に移動する。

						if (this.n現在のスクロールカウンタ > this.n目標のスクロールカウンタ)
							this.n現在のスクロールカウンタ = this.n目標のスクロールカウンタ;    // 到着！スクロール停止！
					}

					else if (this.n現在のスクロールカウンタ > this.n目標のスクロールカウンタ)   // (B) 負の方向に未達の場合：
					{
						this.n現在のスクロールカウンタ -= n加速度;                             // カウンタを負方向に移動する。

						if (this.n現在のスクロールカウンタ < this.n目標のスクロールカウンタ)    // 到着！スクロール停止！
							this.n現在のスクロールカウンタ = this.n目標のスクロールカウンタ;
					}
					//-----------------
					#endregion

					if (this.n現在のスクロールカウンタ >= 100)      // １行＝100カウント。
					{
							#region [ パネルを１行上にシフトする。]
							//-----------------

							// 選択曲と選択行を１つ下の行に移動。

							this.r現在選択中の曲 = this.r次の曲(this.r現在選択中の曲);
							this.n現在の選択行 = (this.n現在の選択行 + 1) % 13;


							// 選択曲から７つ下のパネル（＝新しく最下部に表示されるパネル。消えてしまう一番上のパネルを再利用する）に、新しい曲の情報を記載する。

							C曲リストノード song = this.r現在選択中の曲;
							for (int i = 0; i < 6; i++)
								song = this.r次の曲(song);

							int index = (this.n現在の選択行 + 6) % 13;    // 新しく最下部に表示されるパネルのインデックス（0～12）。
							this.stバー情報[index].strタイトル文字列 = song.strタイトル;
							this.stバー情報[index].strサブタイトル = song.strサブタイトル;
							this.stバー情報[index].ar難易度 = song.nLevel;
							this.stバー情報[index].b分岐 = song.arスコア.譜面情報.b譜面分岐;
							this.stバー情報[index].n王冠 = song.arスコア.譜面情報.n王冠;
							this.stバー情報[index].ForeColor = song.ForeColor;
							this.stバー情報[index].BackColor = song.BackColor;
							this.stバー情報[index].song = song;


							// stバー情報[] の内容を1行ずつずらす。

							C曲リストノード song2 = this.r現在選択中の曲;
							for (int i = 0; i < 6; i++)
								song2 = this.r前の曲(song2);

							for (int i = 0; i < 13; i++)
							{
								int n = (((this.n現在の選択行 - 6) + i) + 13) % 13;
								this.stバー情報[n].song = song2;
								this.stバー情報[n].eバー種別 = this.e曲のバー種別を返す(song2);
								this.stバー情報[n].eノード種別 = song2.eノード種別;
								song2 = this.r次の曲(song2);
								this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);

							}

							// 1行(100カウント)移動完了。

							this.n現在のスクロールカウンタ -= 100;
							this.n目標のスクロールカウンタ -= 100;

							this.t選択曲が変更された(false);             // スクロールバー用に今何番目を選択しているかを更新

							this.ttk選択している曲の曲名 = null;
							this.ttk選択している曲のサブタイトル = null;


							if (this.n目標のスクロールカウンタ == 0)
								TJAPlayer3.stage選曲.t選択曲変更通知();      // スクロール完了＝選択曲変更！

							//-----------------
							#endregion
					}
					else if (this.n現在のスクロールカウンタ <= -100)
					{
							#region [ パネルを１行下にシフトする。]
							//-----------------

							// 選択曲と選択行を１つ上の行に移動。

							this.r現在選択中の曲 = this.r前の曲(this.r現在選択中の曲);
							this.n現在の選択行 = ((this.n現在の選択行 - 1) + 13) % 13;


							// 選択曲から５つ上のパネル（＝新しく最上部に表示されるパネル。消えてしまう一番下のパネルを再利用する）に、新しい曲の情報を記載する。

							C曲リストノード song = this.r現在選択中の曲;
							for (int i = 0; i < 6; i++)
								song = this.r前の曲(song);

							int index = ((this.n現在の選択行 - 6) + 13) % 13; // 新しく最上部に表示されるパネルのインデックス（0～12）。
							this.stバー情報[index].strタイトル文字列 = song.strタイトル;
							this.stバー情報[index].strサブタイトル = song.strサブタイトル;
							this.stバー情報[index].ar難易度 = song.nLevel;
							this.stバー情報[index].b分岐 = song.arスコア.譜面情報.b譜面分岐;
							this.stバー情報[index].n王冠 = song.arスコア.譜面情報.n王冠;
							this.stバー情報[index].ForeColor = song.ForeColor;
							this.stバー情報[index].BackColor = song.BackColor;
							this.stバー情報[index].song = song;

							// stバー情報[] の内容を1行ずつずらす。

							C曲リストノード song2 = this.r現在選択中の曲;
							for (int i = 0; i < 6; i++)
								song2 = this.r前の曲(song2);

							for (int i = 0; i < 13; i++)
							{
								int n = (((this.n現在の選択行 - 6) + i) + 13) % 13;
								this.stバー情報[n].song = song2;
								this.stバー情報[n].eバー種別 = this.e曲のバー種別を返す(song2);
								this.stバー情報[n].eノード種別 = song2.eノード種別;
								song2 = this.r次の曲(song2);
								this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);
							}

							// 1行(100カウント)移動完了。

							this.n現在のスクロールカウンタ += 100;
							this.n目標のスクロールカウンタ += 100;

							this.t選択曲が変更された(false);             // スクロールバー用に今何番目を選択しているかを更新

							this.ttk選択している曲の曲名 = null;
							this.ttk選択している曲のサブタイトル = null;

							if (this.n目標のスクロールカウンタ == 0)
								TJAPlayer3.stage選曲.t選択曲変更通知();      // スクロール完了＝選択曲変更！
																	//-----------------
							#endregion
					}

					if (this.b選択曲が変更された && n現在のスクロールカウンタ == 0)
					{
						if (this.ttk選択している曲の曲名 != null)
						{
							this.ttk選択している曲の曲名 = null;
							this.b選択曲が変更された = false;
						}
						if (this.ttk選択している曲のサブタイトル != null)
						{
							this.ttk選択している曲のサブタイトル = null;
							this.b選択曲が変更された = false;
						}
					}
					this.nスクロールタイマ += nアニメ間隔;
				}
				//-----------------
				#endregion
			}


			// 描画。
			if (this.r現在選択中の曲 == null)
			{
				#region [ 曲が１つもないなら「Songs not found.」を表示してここで帰れ。]
				//-----------------
				if (bIsEnumeratingSongs)
				{
					if (this.txEnumeratingSongs != null)
					{
						this.txEnumeratingSongs.t2D描画(TJAPlayer3.app.Device, 320, 160);
					}
				}
				else
				{
					if (this.txSongNotFound != null)
						this.txSongNotFound.t2D描画(TJAPlayer3.app.Device, 320, 160);
				}
				if (TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape))
					{
						TJAPlayer3.Skin.sound取消音.t再生する();
						TJAPlayer3.stage選曲.eフェードアウト完了時の戻り値 = CStage選曲.E戻り値.タイトルに戻る;
						TJAPlayer3.stage選曲.actFIFO.tフェードアウト開始();
						TJAPlayer3.stage選曲.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						return 0;
					}
				//-----------------
				#endregion

				return 0;
			}

			int i選曲バーX座標 = 673; //選曲バーの座標用
			int i選択曲バーX座標 = 665; //選択曲バーの座標用
			int nnGenreBack = this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);

			if (this.r現在選択中の曲.r親ノード != null)
			{
				nnGenreBack = this.nStrジャンルtoNum(this.r現在選択中の曲.r親ノード.strジャンル);
				if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null && !TJAPlayer3.ConfigIni.OpenOneSide)
				{
					int ForLoop = (int)(1280 / 100) + 1;
					for (int i = 0; i < ForLoop; i++) 
						TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, i * 100, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].vc拡大縮小倍率 = new Vector3(1f);
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].t2D中央基準描画(TJAPlayer3.app.Device, BoxCenterx, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 19, new Rectangle(0, 0, TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width, 62));
				}
			}
			else if (this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Box && this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Other)
			{
				nnGenreBack = this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);
				if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null && !TJAPlayer3.ConfigIni.OpenOneSide)
				{
					int ForLoop = (int)(1280 / 100) + 1;
					for (int i = 0; i < ForLoop; i++)
						TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, i * 100, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].vc拡大縮小倍率 = new Vector3(1f);
					TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].t2D中央基準描画(TJAPlayer3.app.Device, BoxCenterx, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 19, new Rectangle(0, 0, TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width, 62));
				}
			}

			nnGenreBack = this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);

			if (!this.b登場アニメ全部完了)
			{
				if ((this.n現在のスクロールカウンタ == 0) && (ctバー展開ディレイ用タイマー.b終了値に達した))
				{
					#region[片開き用の背景]
					if (TJAPlayer3.ConfigIni.OpenOneSide && this.r現在選択中の曲.r親ノード != null)
					{
						int genre = this.nStrジャンルtoNum(this.r現在選択中の曲.r親ノード.strジャンル);
						int basho = 1;
						if (this.r現在選択中の曲.r親ノード.list子リスト.IndexOf(this.r現在選択中の曲) == 0)
							basho = 0;
						else if (this.r現在選択中の曲.r親ノード.list子リスト.IndexOf(this.r現在選択中の曲) == (this.r現在選択中の曲.r親ノード.list子リスト.Count - 1))
							basho = 2;
						int sixbasho = basho;
						int ForLoop;
						ForLoop = Math.Abs((this.ptバーの座標[5].X + 100) - this.ptバーの座標[7].X) / 100;
						for (int lo = 0; lo < ForLoop; lo++)
						{
							if (basho == 0)
							{
								if (lo == 0)
								{
									sixbasho = 0;
								}
								else
								{
									sixbasho = 1;
								}
							}
							else if (basho == 2)
							{
								if (lo == ForLoop - 1)
								{
									sixbasho = 2;
								}
								else
								{
									sixbasho = 1;
								}
							}
							int sixx = this.ptバーの座標[5].X + 100 + lo * 100;
							if(TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre] != null)
								TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre].t2D描画(TJAPlayer3.app.Device, sixx, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(sixbasho * 100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
						}

					}
					#endregion

					if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null && this.r現在選択中の曲.eノード種別 != C曲リストノード.Eノード種別.BACKBOX && this.r現在選択中の曲.eノード種別 != C曲リストノード.Eノード種別.RANDOM)
					{
						if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Other)
						{
							TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
							if (TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack] != null)
								TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
							TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 69 + TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Height / 2 - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Height / 2 * Math.Max(this.ctバー展開用タイマー.n現在の値 - (this.ctバー展開用タイマー.n終了値 / 2), 0) / 50.0 ));
						}
						else if (TJAPlayer3.Tx.SongSelect_Bar_Center != null)
							TJAPlayer3.Tx.SongSelect_Bar_Center.t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Bar_Center.szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
					}
					else if (TJAPlayer3.Tx.SongSelect_Bar_Center != null)
						TJAPlayer3.Tx.SongSelect_Bar_Center.t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Bar_Center.szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);

					if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Other)
					{
						if (r現在選択中の曲.eノード種別 == C曲リストノード.Eノード種別.RANDOM)
						{
							genretextureindex = 9;
						}
						else if (r現在選択中の曲.eノード種別 == C曲リストノード.Eノード種別.BACKBOX)
						{
							genretextureindex = 10;
						}
						else
						{
							genretextureindex = nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);
						}
						genretext[genretextureindex].Opacity = 全体Opacity;
						genretext[genretextureindex].t2D描画(TJAPlayer3.app.Device, 500, TJAPlayer3.Skin.SongSelect_Overall_Y - 64);
					}
					switch (r現在選択中の曲.eノード種別)
					{
						case C曲リストノード.Eノード種別.SCORE:
							{


								if (TJAPlayer3.Tx.SongSelect_Frame_Score != null)
								{
									// 難易度がTower、Danではない
									if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
									{
										for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
										{
											if (r現在選択中のスコア.譜面情報.b譜面が存在する[i])
											{
												TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(1f, 1f, 1f,1f);
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													// エディット
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
												else if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
											}
											else
											{
												TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(0.5f, 0.5f, 0.5f,1f);
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													// エディット
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
												else if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
											}
										}
									}
									else
									{
										if (r現在選択中のスコア.譜面情報.b譜面が存在する[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]])
										{
											// 譜面がありますね
											TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(1f, 1f, 1f, 1f);
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
										}
										else
										{
											// ないですね
											TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(0.5f, 0.5f, 0.5f, 1f);
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
										}
									}
								}
								#region[ 星 ]
								if (TJAPlayer3.Tx.SongSelect_Level != null)
								{
									// 全難易度表示
									// 難易度がTower、Danではない
									if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
									{
										for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
										{
											for (int n = 0; n < this.r現在選択中のスコア.譜面情報.nレベル[i]; n++)
											{
												// 星11以上はループ終了
												//if (n > 9) break;
												// 裏なら鬼と同じ場所に
												if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4) break;
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
												}
												if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
												}
											}
										}
									}
									else
									{
										for (int i = 0; i < this.r現在選択中のスコア.譜面情報.nレベル[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]]; i++)
										{
											TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494, TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (i * 17), new Rectangle(32 * TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0], 0, 32, 32));
										}
									}
								}
								#endregion
							}
							break;

						case C曲リストノード.Eノード種別.BOX:
							if (TJAPlayer3.Tx.SongSelect_Frame_Box != null)
								TJAPlayer3.Tx.SongSelect_Frame_Box.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;

						case C曲リストノード.Eノード種別.BACKBOX:
							if (TJAPlayer3.Tx.SongSelect_Frame_BackBox != null)
								TJAPlayer3.Tx.SongSelect_Frame_BackBox.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;

						case C曲リストノード.Eノード種別.RANDOM:
							if (TJAPlayer3.Tx.SongSelect_Frame_Random != null)
								TJAPlayer3.Tx.SongSelect_Frame_Random.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;
							//case C曲リストノード.Eノード種別.DANI:
							//    if (CDTXMania.Tx.SongSelect_Frame_Dani != null)
							//        CDTXMania.Tx.SongSelect_Frame_Dani.t2D描画(CDTXMania.app.Device, 450, nバーの高さ);
							//    break;
					}

					if (TJAPlayer3.Tx.SongSelect_Branch_Text_NEW == null && TJAPlayer3.Tx.SongSelect_Branch_Text != null && this.r現在選択中のスコア.譜面情報.b譜面分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]])
						TJAPlayer3.Tx.SongSelect_Branch_Text.t2D描画(TJAPlayer3.app.Device, 483, TJAPlayer3.Skin.SongSelect_Overall_Y + 21);

					if (TJAPlayer3.Tx.SongSelect_Lyric_Text[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)] != null && this.r現在選択中のスコア.譜面情報.b歌詞あり)
						TJAPlayer3.Tx.SongSelect_Lyric_Text[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)].t2D描画(TJAPlayer3.app.Device, 483, TJAPlayer3.Skin.SongSelect_Overall_Y + 21);
				}
				#region [ (1) 登場アニメフェーズの描画。]
				//-----------------

				#region[片開き用の背景]
				if (TJAPlayer3.ConfigIni.OpenOneSide)
				{
					for (int i = 0; i < 13; i++) //先にこっちで背景描画をする
					{
						if (i != 6)
						{
							if (this.stバー情報[i].song.r親ノード != null)
							{
								int genre = this.nStrジャンルtoNum(this.stバー情報[i].song.r親ノード.strジャンル);
								int basho = 1;
								if (this.stバー情報[i].song.r親ノード.list子リスト.IndexOf(this.stバー情報[i].song) == 0)
									basho = 0;
								else if (this.stバー情報[i].song.r親ノード.list子リスト.IndexOf(this.stバー情報[i].song) == (this.stバー情報[i].song.r親ノード.list子リスト.Count - 1))
									basho = 2;
								if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre] != null)
									TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre].t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(basho * 100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));

							}
							if (i == 7 && this.r現在選択中の曲.r親ノード != null)
							{
								int genreheader = this.nStrジャンルtoNum(this.r現在選択中の曲.r親ノード.strジャンル);
								if (TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader] != null)
								{
									TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader].vc拡大縮小倍率 = new Vector3(1f);
									TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader].t2D中央基準描画(TJAPlayer3.app.Device, BoxCenterx, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 19, new Rectangle(0, 0, TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width, 62));
								}
							}
						}
					}
				}
				#endregion

				for (int i = 0; i < 13; i++)    // パネルは全13枚。
				{
					if (this.ct登場アニメ用[i].n現在の値 >= 0)
					{
						double db割合0to1 = ((double)this.ct登場アニメ用[i].n現在の値) / 100.0;
						double db回転率 = Math.Sin(Math.PI * 3 / 5 * db割合0to1);
						int nパネル番号 = (((this.n現在の選択行 - 6) + i) + 13) % 13;

						if (i == 6)
						{
							// (A) 選択曲パネルを描画。

							#region [ タイトル名テクスチャを描画。]
							//-----------------
							if (this.stバー情報[nパネル番号].strタイトル文字列 != "" && this.stバー情報[nパネル番号].strタイトル文字列 != null && this.ttk選択している曲の曲名 == null)
								this.ttk選択している曲の曲名 = this.ttk曲名テクスチャを生成する(this.stバー情報[nパネル番号].strタイトル文字列, Color.White, Color.Black);
							if (this.stバー情報[nパネル番号].strサブタイトル != "" && this.stバー情報[nパネル番号].strサブタイトル != null && this.ttk選択している曲のサブタイトル == null)
								this.ttk選択している曲のサブタイトル = this.ttkサブタイトルテクスチャを生成する(this.stバー情報[nパネル番号].strサブタイトル);


							if (this.ttk選択している曲のサブタイトル != null)
							{
								サブタイトルtmp = ResolveTitleTexture(ttk選択している曲のサブタイトル);
								サブタイトルtmp.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, 707 + (サブタイトルtmp.szテクスチャサイズ.Width / 2), TJAPlayer3.Skin.SongSelect_Overall_Y + 440);
								if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null) {
									if (this.ttk選択している曲の曲名 != null)
									{
										if (this.r現在選択中の曲.strジャンル == "" || this.r現在選択中の曲.strジャンル == "段位道場" || this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box && nStrジャンルtoNum(this.r現在選択中の曲.strジャンル) == 0)
										{
											タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
											タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff + 23);
										}
									}
								}
								else if (this.ttk選択している曲の曲名 != null)
								{
									タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
									タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff + 23);
								}
							}
							else
							{
								if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)] != null)
								{
									if (this.ttk選択している曲の曲名 != null)
									{
										if (this.r現在選択中の曲.strジャンル == "" || this.r現在選択中の曲.strジャンル == "段位道場" || this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box && nStrジャンルtoNum(this.r現在選択中の曲.strジャンル) == 0)
										{
											タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
											タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
										}
									}
								}
								else if (this.ttk選択している曲の曲名 != null)
								{
									タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
									タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
								}
							}

							#endregion
						}
						else
						{
							// (B) その他のパネルの描画。

							#region [ バーテクスチャの描画。]
							//-----------------
							int width = (int)(((double)((640 - this.ptバーの基本座標[i].X) + 1)) / Math.Sin(Math.PI * 3 / 5));
							int x = i選曲バーX座標 + 500 - ((int)(db割合0to1 * 500));
							int y = this.ptバーの基本座標[i].Y;
							this.tジャンル別選択されていない曲バーの描画(this.ptバーの座標[nパネル番号].X, TJAPlayer3.Skin.SongSelect_Overall_Y, this.nStrジャンルtoNum(this.stバー情報[nパネル番号].song.strジャンル), this.stバー情報[nパネル番号].eバー種別,this.stバー情報[nパネル番号].eノード種別);
							if (this.stバー情報[nパネル番号].b分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]] == true && i != 6)
								TJAPlayer3.Tx.SongSelect_Branch.t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[nパネル番号].X + 66, TJAPlayer3.Skin.SongSelect_Overall_Y - 5);
							//-----------------
							#endregion
							#region [ タイトル名テクスチャを描画。]
							if (this.stバー情報[nパネル番号].eノード種別 == C曲リストノード.Eノード種別.BACKBOX)
							{
								if (this.stバー情報[nパネル番号].ttkタイトル != null)
									ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23 + TJAPlayer3.Skin.SongSelect_BackBoxText_Y_Diff);
							}
							else {
								if (this.stバー情報[nパネル番号].ttkタイトル != null)
									ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
							}
							bool DanJudge = false;
							if (TJAPlayer3.Tx.Crown_t != null && TJAPlayer3.Tx.DanC_Crown_t != null && (i != 6 || ! ctバー展開ディレイ用タイマー.b終了値に達した) && this.stバー情報[i].eバー種別 == Eバー種別.Score)
							{
								if (this.stバー情報[i].n王冠[(int)Difficulty.Dan] != 0)
								{
									TJAPlayer3.Tx.DanC_Crown_t.vc拡大縮小倍率 = new Vector3(0.75f);
									TJAPlayer3.Tx.DanC_Crown_t.t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X + 30, TJAPlayer3.Skin.SongSelect_Overall_Y - 30, new Rectangle((this.stバー情報[i].n王冠[(int)Difficulty.Dan] - 1) * 50, 0, 50, 100));
									DanJudge = true;
								}
								TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率 = new Vector3(0.5f);
								for (int j = 4; j >= 0 ; j--) {
									if (DanJudge)//2020.05.25 Mr-Ojii 汚いかもしれないけど、gotoを使わないでやるにはこうするしか思いつかなかった。
										break;
									if (this.stバー情報[i].n王冠[j] != 0)
									{
										TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X + 25, TJAPlayer3.Skin.SongSelect_Overall_Y - 23, new Rectangle(this.stバー情報[i].n王冠[j] * 100, 0, 100, 100));
										if (TJAPlayer3.Tx.Difficulty_Icons != null) {
											TJAPlayer3.Tx.Difficulty_Icons.vc拡大縮小倍率 = new Vector3(0.4f);
											TJAPlayer3.Tx.Difficulty_Icons.t2D描画(TJAPlayer3.app.Device, this.ptバーの座標[i].X + 40, TJAPlayer3.Skin.SongSelect_Overall_Y - 15, new Rectangle(j * 100, 0, 100, 100)); 
										}
										break;
									}
								}
							}
							#endregion
						}
					}
				}
				//-----------------
				#endregion
			}
			else
			{
				#region [ (2) 通常フェーズの描画。]
				//-----------------
				#region[片開き用の背景]
				if (TJAPlayer3.ConfigIni.OpenOneSide)
				{
					for (int i = 0; i < 13; i++)
					{
						if ((i == 0 && this.n現在のスクロールカウンタ > 0) ||       // 最上行は、上に移動中なら表示しない。
							(i == 12 && this.n現在のスクロールカウンタ < 0))        // 最下行は、下に移動中なら表示しない。
							continue;

						int nパネル番号 = (((this.n現在の選択行 - 6) + i) + 13) % 13;
						int n見た目の行番号 = i;
						int n次のパネル番号 = (this.n現在のスクロールカウンタ <= 0) ? ((i + 1) % 13) : (((i - 1) + 13) % 13);
						int xAnime = this.ptバーの座標[n見た目の行番号].X + ((int)((this.ptバーの座標[n次のパネル番号].X - this.ptバーの座標[n見た目の行番号].X) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 100.0)));

						if (n見た目の行番号 == 5 && this.stバー情報[(nパネル番号 + 1) % 13].song.r親ノード != null) //5のところでCenterを描画しないと、選択曲変更のとき、背景に隠れてしまう。
						{
							int genre = this.nStrジャンルtoNum(this.stバー情報[(nパネル番号 + 1) % 13].song.r親ノード.strジャンル);
							int basho = 1;
							if (this.stバー情報[(nパネル番号 + 1) % 13].song.r親ノード.list子リスト.IndexOf(this.stバー情報[(nパネル番号 + 1) % 13].song) == 0)
								basho = 0;
							else if (this.stバー情報[(nパネル番号 + 1) % 13].song.r親ノード.list子リスト.IndexOf(this.stバー情報[(nパネル番号 + 1) % 13].song) == (this.stバー情報[(nパネル番号 + 1) % 13].song.r親ノード.list子リスト.Count - 1))
								basho = 2;

							int sixbasho = basho;
							int ForLoop;
							ForLoop = Math.Abs((this.ptバーの座標[5].X + 100) - this.ptバーの座標[7].X) / 100;
							for (int lo = 0; lo < ForLoop; lo++)
							{
								if (basho == 0)
								{
									if (lo == 0)
									{
										sixbasho = 0;
									}
									else
									{
										sixbasho = 1;
									}
								}
								else if (basho == 2)
								{
									if (lo == ForLoop - 1)
									{
										sixbasho = 2;
									}
									else
									{
										sixbasho = 1;
									}
								}
								int sixx = this.ptバーの座標[5].X + 100 + lo * 100;
								sixx -= this.n現在のスクロールカウンタ;
								if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre] != null)
									TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre].t2D描画(TJAPlayer3.app.Device, sixx, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(sixbasho * 100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
							}
						}

						if (this.stバー情報[nパネル番号].song.r親ノード != null)
						{
							int genre = this.nStrジャンルtoNum(this.stバー情報[nパネル番号].song.r親ノード.strジャンル);
							int basho = 1;
							if (this.stバー情報[nパネル番号].song.r親ノード.list子リスト.IndexOf(this.stバー情報[nパネル番号].song) == 0)
								basho = 0;
							else if (this.stバー情報[nパネル番号].song.r親ノード.list子リスト.IndexOf(this.stバー情報[nパネル番号].song) == (this.stバー情報[nパネル番号].song.r親ノード.list子リスト.Count - 1))
								basho = 2;
							if (n見た目の行番号 != 5 && n見た目の行番号 != 6 && n見た目の行番号 != 7)
							{
								if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre] != null)
									TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre].t2D描画(TJAPlayer3.app.Device, xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(basho * 100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
							}
							else if (n見た目の行番号 != 6)
							{
								int fivesevenx = this.ptバーの座標[n見た目の行番号].X;
								fivesevenx -= this.n現在のスクロールカウンタ;
								if (TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre] != null)
									TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[genre].t2D描画(TJAPlayer3.app.Device, fivesevenx, TJAPlayer3.Skin.SongSelect_Overall_Y - 69, new Rectangle(basho * 100, 0, 100, TJAPlayer3.Tx.SongSelect_Bar_Center_Back_Genre[nnGenreBack].szテクスチャサイズ.Height));
							}
						}
					}

					if (this.r現在選択中の曲.r親ノード != null)
					{
						int genreheader = this.nStrジャンルtoNum(this.r現在選択中の曲.r親ノード.strジャンル);
						if (TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader] != null)
						{
							TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader].vc拡大縮小倍率 = new Vector3(1f);
							TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[genreheader].t2D中央基準描画(TJAPlayer3.app.Device, BoxCenterx, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 19, new Rectangle(0, 0, TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width, 62));
						}
					}
				}
				#endregion
				for (int i = 0; i < 13; i++)    // パネルは全13枚。
				{
					if ((i == 0 && this.n現在のスクロールカウンタ > 0) ||       // 最上行は、上に移動中なら表示しない。
						(i == 12 && this.n現在のスクロールカウンタ < 0))        // 最下行は、下に移動中なら表示しない。
						continue;

					int nパネル番号 = (((this.n現在の選択行 - 6) + i) + 13) % 13;
					int n見た目の行番号 = i;
					int n次のパネル番号 = (this.n現在のスクロールカウンタ <= 0) ? ((i + 1) % 13) : (((i - 1) + 13) % 13);
					int x = i選曲バーX座標;
					int xAnime = this.ptバーの座標[n見た目の行番号].X + ((int)((this.ptバーの座標[n次のパネル番号].X - this.ptバーの座標[n見た目の行番号].X) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 100.0)));



					//2020.06.02 曲決定した時のバーの横移動
					if (TJAPlayer3.stage選曲.現在の選曲画面状況 == CStage選曲.E選曲画面.難易度選択Out)
					{
						if (n見た目の行番号 < 6) {
							xAnime += (TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n現在の値 - TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n終了値) * 3;
						}
						else if (n見た目の行番号 > 6)
						{
							xAnime -= (TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n現在の値 - TJAPlayer3.stage選曲.ct難易度選択画面OUT用タイマー.n終了値) * 3;
						}
					}
					else if (TJAPlayer3.stage選曲.現在の選曲画面状況 == CStage選曲.E選曲画面.難易度選択In) 
					{
						if (n見た目の行番号 < 6)
						{
							xAnime -= TJAPlayer3.stage選曲.ct難易度選択画面IN用タイマー.n現在の値 * 3;
						}
						else if (n見た目の行番号 > 6)
						{
							xAnime += TJAPlayer3.stage選曲.ct難易度選択画面IN用タイマー.n現在の値 * 3;
						}
					}
					//-------

					int y = this.ptバーの基本座標[n見た目の行番号].Y + ((int)((this.ptバーの基本座標[n次のパネル番号].Y - this.ptバーの基本座標[n見た目の行番号].Y) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 100.0)));

					{
						// (B) スクロール中の選択曲バー、またはその他のバーの描画。

						#region [ バーテクスチャを描画。]
						//-----------------
						if (n現在のスクロールカウンタ != 0)
							this.tジャンル別選択されていない曲バーの描画(xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y, this.nStrジャンルtoNum(this.stバー情報[nパネル番号].song.strジャンル), this.stバー情報[nパネル番号].eバー種別,this.stバー情報[nパネル番号].eノード種別);
						else if (n見た目の行番号 != 6 || !(ctバー展開ディレイ用タイマー.b終了値に達した))
							this.tジャンル別選択されていない曲バーの描画(xAnime, TJAPlayer3.Skin.SongSelect_Overall_Y, this.nStrジャンルtoNum(this.stバー情報[nパネル番号].song.strジャンル), this.stバー情報[nパネル番号].eバー種別,this.stバー情報[nパネル番号].eノード種別);
						if (this.stバー情報[nパネル番号].b分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]] == true && n見た目の行番号 != 6)
							TJAPlayer3.Tx.SongSelect_Branch.t2D描画(TJAPlayer3.app.Device, xAnime + 66, TJAPlayer3.Skin.SongSelect_Overall_Y - 5);
						//-----------------
						#endregion

						#region [ タイトル名テクスチャを描画。]
						if (this.stバー情報[nパネル番号].eノード種別 == C曲リストノード.Eノード種別.BACKBOX)
						{
							if (n現在のスクロールカウンタ != 0)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, xAnime + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23 + TJAPlayer3.Skin.SongSelect_BackBoxText_Y_Diff);
							else if (n見た目の行番号 != 6 || !(ctバー展開ディレイ用タイマー.b終了値に達した))
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, xAnime + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23 + TJAPlayer3.Skin.SongSelect_BackBoxText_Y_Diff);
						}
						else
						{
							if (n現在のスクロールカウンタ != 0)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, xAnime + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
							else if (n見た目の行番号 != 6 || !(ctバー展開ディレイ用タイマー.b終了値に達した))
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル).t2D描画(TJAPlayer3.app.Device, xAnime + 28, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);

						}

						#endregion

						//-----------------	
						bool DanJudge = false;
						if (TJAPlayer3.Tx.Crown_t != null && TJAPlayer3.Tx.DanC_Crown_t != null && (n見た目の行番号 != 6 || !ctバー展開ディレイ用タイマー.b終了値に達した) && this.stバー情報[nパネル番号].eバー種別 == Eバー種別.Score)
						{
							if (this.stバー情報[nパネル番号].n王冠[(int)Difficulty.Dan] != 0)
							{
								TJAPlayer3.Tx.DanC_Crown_t.vc拡大縮小倍率 = new Vector3(0.75f);
								TJAPlayer3.Tx.DanC_Crown_t.t2D描画(TJAPlayer3.app.Device, xAnime + 30, TJAPlayer3.Skin.SongSelect_Overall_Y - 30, new Rectangle((this.stバー情報[nパネル番号].n王冠[(int)Difficulty.Dan] - 1) * 50, 0, 50, 100));
								DanJudge = true;
							}
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.X = 0.5f;
							TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.Y = 0.5f;
							for (int j = 4; j >= 0; j--)
							{
								if (DanJudge)//2020.05.25 Mr-Ojii 汚いかもしれないけど、gotoを使わないでやるにはこうするしか思いつかなかった。
									break;
								if (this.stバー情報[nパネル番号].n王冠[j] != 0)
								{
									TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, xAnime + 25, TJAPlayer3.Skin.SongSelect_Overall_Y - 23, new Rectangle(this.stバー情報[nパネル番号].n王冠[j] * 100, 0, 100, 100));
									if (TJAPlayer3.Tx.Difficulty_Icons != null)	{
										TJAPlayer3.Tx.Difficulty_Icons.vc拡大縮小倍率.X = 0.4f;
										TJAPlayer3.Tx.Difficulty_Icons.vc拡大縮小倍率.Y = 0.4f;
										TJAPlayer3.Tx.Difficulty_Icons.t2D描画(TJAPlayer3.app.Device, xAnime + 40, TJAPlayer3.Skin.SongSelect_Overall_Y - 15, new Rectangle(j * 100, 0, 100, 100));
									}
									break;
								}
							}
						}
					}
					#endregion
				}

				if ((this.n現在のスクロールカウンタ == 0) && (ctバー展開ディレイ用タイマー.b終了値に達した))
				{

					nnGenreBack = this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);
					if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null && this.r現在選択中の曲.eノード種別 != C曲リストノード.Eノード種別.BACKBOX && this.r現在選択中の曲.eノード種別 != C曲リストノード.Eノード種別.RANDOM)
					{
						if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Other)
						{
							TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
							if(TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack] != null)
								TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Text_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
							TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff - 69 + TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Height / 2 - (int)(TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack].szテクスチャサイズ.Height / 2 * Math.Max(this.ctバー展開用タイマー.n現在の値 - (this.ctバー展開用タイマー.n終了値 / 2), 0) / 50.0));
						}
						else if (TJAPlayer3.Tx.SongSelect_Bar_Center != null)
							TJAPlayer3.Tx.SongSelect_Bar_Center.t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Bar_Center.szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);
					}
					else if (TJAPlayer3.Tx.SongSelect_Bar_Center != null)
						TJAPlayer3.Tx.SongSelect_Bar_Center.t2D描画(TJAPlayer3.app.Device, BoxCenterx - (int)(TJAPlayer3.Tx.SongSelect_Bar_Center.szテクスチャサイズ.Width / 2 * ctバー展開用タイマー.n現在の値 / 100.0), TJAPlayer3.Skin.SongSelect_Overall_Y);

					if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Other)
					{
						if (r現在選択中の曲.eノード種別 == C曲リストノード.Eノード種別.RANDOM)
						{
							genretextureindex = 9;
						}
						else if (r現在選択中の曲.eノード種別 == C曲リストノード.Eノード種別.BACKBOX)
						{
							genretextureindex = 10;
						}
						else
						{
							genretextureindex = nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);
						}
						genretext[genretextureindex].Opacity = 全体Opacity;
						genretext[genretextureindex].t2D描画(TJAPlayer3.app.Device, 500, TJAPlayer3.Skin.SongSelect_Overall_Y - 64);
					}
					switch (r現在選択中の曲.eノード種別)
					{
						case C曲リストノード.Eノード種別.SCORE:
							{
								if (TJAPlayer3.Tx.SongSelect_Frame_Score != null)
								{
									// 難易度がTower、Danではない
									if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
									{
										for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
										{
											if (r現在選択中のスコア.譜面情報.b譜面が存在する[i])
											{
												// レベルが0以上
												TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(1f, 1f, 1f, 1f);
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													// エディット
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
												else if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
											}
											else
											{
												// レベルが0未満 = 譜面がないとみなす
												TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(0.5f, 0.5f, 0.5f, 1f);
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													// エディット
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
												else if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
												}
											}
										}
									}
									else
									{
										if (r現在選択中のスコア.譜面情報.b譜面が存在する[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]])
										{
											// 譜面がありますね
											TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(1f, 1f, 1f, 1f);
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
										}
										else
										{
											// ないですね
											TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = new Color4(0.5f, 0.5f, 0.5f, 1f);
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
										}
									}
								}
								#region[ 星 ]
								if (TJAPlayer3.Tx.SongSelect_Level != null)
								{
									// 全難易度表示
									// 難易度がTower、Danではない
									if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
									{
										for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
										{
											for (int n = 0; n < this.r現在選択中のスコア.譜面情報.nレベル[i]; n++)
											{
												// 星11以上はループ終了
												//if (n > 9) break;
												// 裏なら鬼と同じ場所に
												if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4) break;
												if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
												}
												if (i != 4)
												{
													TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
												}
											}
										}
									}
									else
									{
										for (int i = 0; i < this.r現在選択中のスコア.譜面情報.nレベル[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]]; i++)
										{
											TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(TJAPlayer3.app.Device, 494, TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (i * 17), new Rectangle(32 * TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0], 0, 32, 32));
										}
									}
								}
								#endregion
								#region[譜面分岐や歌詞やパパママ]
								if (TJAPlayer3.Tx.SongSelect_Branch_Text_NEW == null && TJAPlayer3.Tx.SongSelect_Branch_Text != null && this.r現在選択中のスコア.譜面情報.b譜面分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0]])
									TJAPlayer3.Tx.SongSelect_Branch_Text.t2D描画(TJAPlayer3.app.Device, 483, TJAPlayer3.Skin.SongSelect_Overall_Y + 21);

								if (TJAPlayer3.Tx.SongSelect_Lyric_Text[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)] != null && this.r現在選択中のスコア.譜面情報.b歌詞あり)
									TJAPlayer3.Tx.SongSelect_Lyric_Text[nStrジャンルtoNum(this.r現在選択中の曲.strジャンル)].t2D描画(TJAPlayer3.app.Device, 483, TJAPlayer3.Skin.SongSelect_Overall_Y + 21);

								if (TJAPlayer3.Tx.SongSelect_Branch_Text_NEW != null && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
								{
									for (int i = 0; i < 4; i++)
									{
										if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
										{
											if (this.r現在選択中のスコア.譜面情報.b譜面分岐[4])
											{
												TJAPlayer3.Tx.SongSelect_Branch_Text_NEW.t2D描画(TJAPlayer3.app.Device, i * 60 + 479, TJAPlayer3.Skin.SongSelect_Overall_Y + 234, new Rectangle(32, 0, 32, 180));
											}
										}
										else
										{
											if (this.r現在選択中のスコア.譜面情報.b譜面分岐[i])
											{
												TJAPlayer3.Tx.SongSelect_Branch_Text_NEW.t2D描画(TJAPlayer3.app.Device, i * 60 + 479, TJAPlayer3.Skin.SongSelect_Overall_Y + 234, new Rectangle(0, 0, 32, 180));
											}
										}
									}
								}
								if (TJAPlayer3.Tx.SongSelect_PapaMama != null && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] != (int)Difficulty.Dan)
								{
									for (int i = 0; i < 4; i++)
									{
										if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
										{
											if (this.r現在選択中のスコア.譜面情報.bPapaMamaSupport[4])
											{
												TJAPlayer3.Tx.SongSelect_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 60 + 479, TJAPlayer3.Skin.SongSelect_Overall_Y + 234, new Rectangle(32, 0, 32, 180));
											}
										}
										else
										{
											if (this.r現在選択中のスコア.譜面情報.bPapaMamaSupport[i])
											{
												TJAPlayer3.Tx.SongSelect_PapaMama.t2D描画(TJAPlayer3.app.Device, i * 60 + 479, TJAPlayer3.Skin.SongSelect_Overall_Y + 234, new Rectangle(0, 0, 32, 180));
											}
										}
									}
								}
								#endregion
								#region[王冠]
								if (TJAPlayer3.Tx.Crown_t != null && TJAPlayer3.Tx.DanC_Crown_t != null)
								{
									TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.X = 0.25f;
									TJAPlayer3.Tx.Crown_t.vc拡大縮小倍率.Y = 0.25f;
									if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] <= 4)
									{
										for (int i = 0; i < 4; i++)
										{
											if (TJAPlayer3.Tx.Crown_t != null && this.r現在選択中のスコア.譜面情報.n王冠[i] >= 0 && this.r現在選択中のスコア.譜面情報.n王冠[i] <= 3)
											{
												if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == 4)
												{
													TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, i * 60 + 482, TJAPlayer3.Skin.SongSelect_Overall_Y + 85, new Rectangle((this.r現在選択中のスコア.譜面情報.n王冠[4]) * 100, 0, 100, 100));
												}
												else if (this.r現在選択中のスコア.譜面情報.b譜面が存在する[i])
												{
													TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, i * 60 + 482, TJAPlayer3.Skin.SongSelect_Overall_Y + 85, new Rectangle((this.r現在選択中のスコア.譜面情報.n王冠[i]) * 100, 0, 100, 100));
												}
											}
										}
									}
									else if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度[0] == (int)Difficulty.Tower)
									{
										TJAPlayer3.Tx.Crown_t.t2D描画(TJAPlayer3.app.Device, 482, TJAPlayer3.Skin.SongSelect_Overall_Y + 57, new Rectangle((this.r現在選択中のスコア.譜面情報.n王冠[5]) * 100, 0, 100, 100));
									}
									else
									{
										TJAPlayer3.Tx.DanC_Crown_t.vc拡大縮小倍率 = new Vector3(1f);
										if (this.r現在選択中のスコア.譜面情報.n王冠[(int)Difficulty.Dan] != 0)
											TJAPlayer3.Tx.DanC_Crown_t.t2D描画(TJAPlayer3.app.Device, 482, TJAPlayer3.Skin.SongSelect_Overall_Y + 20, new Rectangle((this.r現在選択中のスコア.譜面情報.n王冠[(int)Difficulty.Dan] - 1) * 50, 0, 50, 100));
									}
								}
								#endregion
							}
							break;

						case C曲リストノード.Eノード種別.BOX:
							if (TJAPlayer3.Tx.SongSelect_Frame_Box != null)
								TJAPlayer3.Tx.SongSelect_Frame_Box.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;

						case C曲リストノード.Eノード種別.BACKBOX:
							if (TJAPlayer3.Tx.SongSelect_Frame_BackBox != null)
								TJAPlayer3.Tx.SongSelect_Frame_BackBox.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;

						case C曲リストノード.Eノード種別.RANDOM:
							if (TJAPlayer3.Tx.SongSelect_Frame_Random != null)
								TJAPlayer3.Tx.SongSelect_Frame_Random.t2D描画(TJAPlayer3.app.Device, 450, TJAPlayer3.Skin.SongSelect_Overall_Y);
							break;
							//case C曲リストノード.Eノード種別.DANI:
							//    if (CDTXMania.Tx.SongSelect_Frame_Dani != null)
							//        CDTXMania.Tx.SongSelect_Frame_Dani.t2D描画(CDTXMania.app.Device, 450, nバーの高さ);
							//    break;
					}

				}

				#region [ 項目リストにフォーカスがあって、かつスクロールが停止しているなら、パネルの上下に▲印を描画する。]
				//-----------------
				if ((this.n目標のスクロールカウンタ == 0))
				{
					int Cursor_L = 372 - this.ct三角矢印アニメ.n現在の値 / 50;
					int Cursor_R = 819 + this.ct三角矢印アニメ.n現在の値 / 50;
					int y = 289;

					// 描画。

					if (TJAPlayer3.Tx.SongSelect_Cursor_Left != null)
					{
						TJAPlayer3.Tx.SongSelect_Cursor_Left.t2D描画(TJAPlayer3.app.Device, Cursor_L, y);
					}
					if (TJAPlayer3.Tx.SongSelect_Cursor_Right != null)
					{
						TJAPlayer3.Tx.SongSelect_Cursor_Right.t2D描画(TJAPlayer3.app.Device, Cursor_R, y);
					}
				}
				//-----------------
				#endregion


				for (int i = 0; i < 13; i++)    // パネルは全13枚。
				{
					if ((i == 0 && this.n現在のスクロールカウンタ > 0) ||       // 最上行は、上に移動中なら表示しない。
						(i == 12 && this.n現在のスクロールカウンタ < 0))        // 最下行は、下に移動中なら表示しない。
						continue;

					int nパネル番号 = (((this.n現在の選択行 - 6) + i) + 13) % 13;
					int n見た目の行番号 = i;
					int n次のパネル番号 = (this.n現在のスクロールカウンタ <= 0) ? ((i + 1) % 13) : (((i - 1) + 13) % 13);
					//int x = this.ptバーの基本座標[ n見た目の行番号 ].X + ( (int) ( ( this.ptバーの基本座標[ n次のパネル番号 ].X - this.ptバーの基本座標[ n見た目の行番号 ].X ) * ( ( (double) Math.Abs( this.n現在のスクロールカウンタ ) ) / 100.0 ) ) );
					int x = i選曲バーX座標;
					int xAnime = this.ptバーの座標[n見た目の行番号].X + ((int)((this.ptバーの座標[n次のパネル番号].X - this.ptバーの座標[n見た目の行番号].X) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 100.0)));
					int y = this.ptバーの基本座標[n見た目の行番号].Y + ((int)((this.ptバーの基本座標[n次のパネル番号].Y - this.ptバーの基本座標[n見た目の行番号].Y) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 100.0)));

					if ((i == 6) && (this.n現在のスクロールカウンタ == 0) && (ctバー展開ディレイ用タイマー.b終了値に達した))
					{
						// (A) スクロールが停止しているときの選択曲バーの描画。

						#region [ タイトル名テクスチャを描画。]
						//-----------------
						if (this.stバー情報[nパネル番号].strタイトル文字列 != "" && this.ttk選択している曲の曲名 == null)
							this.ttk選択している曲の曲名 = this.ttk曲名テクスチャを生成する(this.stバー情報[nパネル番号].strタイトル文字列, Color.White, Color.Black);
						if (this.stバー情報[nパネル番号].strサブタイトル != "" && this.ttk選択している曲のサブタイトル == null)
							this.ttk選択している曲のサブタイトル = this.ttkサブタイトルテクスチャを生成する(this.stバー情報[nパネル番号].strサブタイトル);

						//サブタイトルがあったら700

						if (this.ttk選択している曲のサブタイトル != null)
						{
							サブタイトルtmp = ResolveTitleTexture(ttk選択している曲のサブタイトル);
							サブタイトルtmp.t2D拡大率考慮下中心基準描画(TJAPlayer3.app.Device, 707 + (サブタイトルtmp.szテクスチャサイズ.Width / 2), TJAPlayer3.Skin.SongSelect_Overall_Y + 440);
							if ( TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null) {
								if (this.ttk選択している曲の曲名 != null)
								{
									if (this.r現在選択中の曲.strジャンル == "" || this.r現在選択中の曲.strジャンル == "段位道場" || this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box && nStrジャンルtoNum(this.r現在選択中の曲.strジャンル) == 0) 
									{
										タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
										タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
									}
										
								}
							}
							else if (this.ttk選択している曲の曲名 != null)
							{
								タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
								タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + 23);
							}
						}
						else
						{
							if (TJAPlayer3.Tx.SongSelect_Box_Center_Genre[nnGenreBack] != null && TJAPlayer3.Tx.SongSelect_Box_Center_Header_Genre[nnGenreBack] != null)
							{
								if (this.ttk選択している曲の曲名 != null)
								{
									if (this.r現在選択中の曲.strジャンル == "" || this.r現在選択中の曲.strジャンル == "段位道場" || this.e曲のバー種別を返す(this.r現在選択中の曲) != Eバー種別.Box || this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Box && nStrジャンルtoNum(this.r現在選択中の曲.strジャンル) == 0) 
									{
										タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
										タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff + 23);
									}
										
								}
							}
							else if (this.ttk選択している曲の曲名 != null)
							{
								タイトルtmp = ResolveTitleTexture(this.ttk選択している曲の曲名);
								タイトルtmp.t2D描画(TJAPlayer3.app.Device, 750, TJAPlayer3.Skin.SongSelect_Overall_Y + TJAPlayer3.Skin.SongSelect_Box_Center_Header_Y_Diff + 23);
							}
						}

						//if( this.stバー情報[ nパネル番号 ].txタイトル名 != null )
						//	this.stバー情報[ nパネル番号 ].txタイトル名.t2D描画( CDTXMania.app.Device, i選択曲バーX座標 + 65, y選曲 + 6 );

						//CDTXMania.act文字コンソール.tPrint( i選曲バーX座標 - 20, y選曲 + 6, C文字コンソール.Eフォント種別.白, this.r現在選択中のスコア.譜面情報.b譜面分岐[3].ToString() );
						//-----------------
						#endregion
					}

				}
				//-----------------
			}
			#region [ スクロール地点の計算(描画はCActSelectShowCurrentPositionにて行う) #27648 ]
			int py;
			double d = 0;
			if (nNumOfItems > 1)
			{
				d = (336 - 6 - 8) / (double)(nNumOfItems - 1);
				py = (int)(d * (nCurrentPosition - 1));
			}
			else
			{
				d = 0;
				py = 0;
			}
			int delta = (int)(d * this.n現在のスクロールカウンタ / 100);
			if (py + delta <= 336 - 6 - 8)
			{
				this.nスクロールバー相対y座標 = py + delta;
			}
			#endregion

			#region [ アイテム数の描画 #27648 ]
			tアイテム数の描画();
			#endregion

			if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Score)
			{
				genretextureindex = nStrジャンルtoNum(this.r現在選択中の曲.strジャンル);
				genretext[genretextureindex].Opacity = 全体Opacity;
				genretext[genretextureindex].t2D描画(TJAPlayer3.app.Device, 500, TJAPlayer3.Skin.SongSelect_Overall_Y - 64); ;
			}

			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private enum Eバー種別 { Score, Box, Other }

		private struct STバー情報
		{
			public CActSelect曲リスト.Eバー種別 eバー種別;
			public C曲リストノード.Eノード種別 eノード種別;
			public string strタイトル文字列;
			public int[] ar難易度;
			public bool[] b分岐;
			public Color ForeColor;
			public Color BackColor;
			public string strサブタイトル;
			public TitleTextureKey ttkタイトル;
			public int[] n王冠;
			public C曲リストノード song;
		}

		private const int BoxCenterx = 645; 

		public bool b選択曲が変更された = true;
		private bool b登場アニメ全部完了;
		private CCounter[] ct登場アニメ用 = new CCounter[13];
		private CCounter ct三角矢印アニメ;
		private CCounter ct分岐フェード用タイマー;
		private CCounter ctバー展開用タイマー;
		private CCounter ctバー展開ディレイ用タイマー;
		private CPrivateFastFont pfMusicName;
		private CPrivateFastFont pfSubtitle;
		private int genretextureindex;
		internal CTexture タイトルtmp;
		internal CTexture サブタイトルtmp;

		private readonly Dictionary<TitleTextureKey, CTexture> _titledictionary 
			= new Dictionary<TitleTextureKey, CTexture>();

		private Font ft曲リスト用フォント;
		private long nスクロールタイマ;
		private int n現在のスクロールカウンタ;
		private int n現在の選択行;
		private int n目標のスクロールカウンタ;
		private readonly Point[] ptバーの基本座標 = new Point[] { new Point(0x2c4, 5), new Point(0x272, 56), new Point(0x242, 107), new Point(0x222, 158), new Point(0x210, 209), new Point(0x1d0, 270), new Point(0x224, 362), new Point(0x242, 413), new Point(0x270, 464), new Point(0x2ae, 515), new Point(0x314, 566), new Point(0x3e4, 617), new Point(0x500, 668) };
		private Point[] ptバーの座標 = new Point[]
		{ new Point( -160, 180 ), new Point( -60, 180 ), new Point( 40, 180 ), new Point( 140, 180 ), new Point( 240, 180 ), new Point( 340, 180 ),
		  new Point( 590, 180 ),
		  new Point( 840, 180 ), new Point( 940, 180 ), new Point( 1040, 180 ), new Point( 1140, 180 ), new Point( 1240, 180 ), new Point( 1340, 180 ) };//2020.06.16 Mr-Ojii 諸事情により座標変更

		private STバー情報[] stバー情報 = new STバー情報[13];
		private CTexture[] genretext = new CTexture[12];
		private CTexture txSongNotFound, txEnumeratingSongs;
		//private CTexture txスキル数字;
		//private CTexture txアイテム数数字;
		//private STバー tx曲名バー;
		//private ST選曲バー tx選曲バー;
		//      private CTexture txバー中央;
		internal TitleTextureKey ttk選択している曲の曲名;
		internal TitleTextureKey ttk選択している曲のサブタイトル;

		private int nCurrentPosition = 0;
		private int nNumOfItems = 0;

		//private string strBoxDefSkinPath = "";
		private Eバー種別 e曲のバー種別を返す(C曲リストノード song)
		{
			if (song != null)
			{
				switch (song.eノード種別)
				{
					case C曲リストノード.Eノード種別.SCORE:
						return Eバー種別.Score;

					case C曲リストノード.Eノード種別.BOX:
					case C曲リストノード.Eノード種別.BACKBOX:
						return Eバー種別.Box;
				}
			}
			return Eバー種別.Other;
		}
		private C曲リストノード r次の曲(C曲リストノード song)
		{
			if (song == null)
				return null;

			
			List<C曲リストノード> list = (song.r親ノード != null && !TJAPlayer3.ConfigIni.OpenOneSide) ? song.r親ノード.list子リスト : TJAPlayer3.Songs管理.list曲ルート;

			int index = list.IndexOf(song);

			if (index < 0)
				return null;

			if (index == (list.Count - 1))
				return list[0];

			return list[index + 1];
		}
		private C曲リストノード r前の曲(C曲リストノード song)
		{
			if (song == null)
				return null;

			List<C曲リストノード> list = (song.r親ノード != null && !TJAPlayer3.ConfigIni.OpenOneSide) ? song.r親ノード.list子リスト : TJAPlayer3.Songs管理.list曲ルート;

			int index = list.IndexOf(song);

			if (index < 0)
				return null;

			if (index == 0)
				return list[list.Count - 1];

			return list[index - 1];
		}
		private void tバーの初期化()
		{
			C曲リストノード song = this.r現在選択中の曲;

			if (song == null)
				return;

			for (int i = 0; i < 6; i++)
				song = this.r前の曲(song);

			if (song == null)
			{
				if(TJAPlayer3.Songs管理.list曲ルート[0] != null)
					this.r現在選択中の曲 = TJAPlayer3.Songs管理.list曲ルート[0];
				else
					return;
				this.t現在選択中の曲を元に曲バーを再構成する();
				this.t選択曲が変更された(false);
				this.b選択曲が変更された = true;
				TJAPlayer3.stage選曲.t選択曲変更通知();
				return;
			}

			for (int i = 0; i < 13; i++)
			{
				this.stバー情報[i].strタイトル文字列 = song.strタイトル;
				this.stバー情報[i].eバー種別 = this.e曲のバー種別を返す(song);
				this.stバー情報[i].eノード種別 = song.eノード種別;
				this.stバー情報[i].strサブタイトル = song.strサブタイトル;
				this.stバー情報[i].ar難易度 = song.nLevel;
				this.stバー情報[i].b分岐 = song.arスコア.譜面情報.b譜面分岐;
				this.stバー情報[i].n王冠 = song.arスコア.譜面情報.n王冠;
				this.stバー情報[i].ForeColor = song.ForeColor;
				this.stバー情報[i].BackColor = song.BackColor;
				this.stバー情報[i].song = song;

				this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);

				song = this.r次の曲(song);
			}

			this.n現在の選択行 = 6;
		}
		private void tジャンル別選択されていない曲バーの描画(int x, int y, int nジャンル,Eバー種別 Eバー,C曲リストノード.Eノード種別 Eノード)
		{
			if (x >= GameWindowSize.Width || y >= GameWindowSize.Height)
				return;
			const int boxsabun = 10;

			var rc = new Rectangle(0, 48, 128, 48);
			if (Eバー == Eバー種別.Box)
			{
				if (Eノード == C曲リストノード.Eノード種別.BACKBOX && TJAPlayer3.Tx.SongSelect_Bar_BackBox != null)
				{
					TJAPlayer3.Tx.SongSelect_Bar_BackBox.t2D描画(TJAPlayer3.app.Device, x, y);
				}
				else
				{
					if (TJAPlayer3.Tx.SongSelect_Bar_Box_Genre[nジャンル] != null)
						TJAPlayer3.Tx.SongSelect_Bar_Box_Genre[nジャンル].t2D描画(TJAPlayer3.app.Device, x, y - boxsabun);
					else if (TJAPlayer3.Tx.SongSelect_Bar_Genre[nジャンル] != null)
						TJAPlayer3.Tx.SongSelect_Bar_Genre[nジャンル].t2D描画(TJAPlayer3.app.Device, x, y);
				}
			}
			else
			{
				if (TJAPlayer3.Tx.SongSelect_Bar_Genre[nジャンル] != null)
					TJAPlayer3.Tx.SongSelect_Bar_Genre[nジャンル].t2D描画(TJAPlayer3.app.Device, x, y);
			}
		}

		public int nStrジャンルtoNum(string strジャンル)
		{
			int nGenre = 0;
			switch (strジャンル)
			{
				case "J-POP":
					nGenre = 1;
					break;
				case "アニメ":
					nGenre = 2;
					break;
				case "ゲームミュージック":
					nGenre = 3;
					break;
				case "ナムコオリジナル":
					nGenre = 4;
					break;
				case "クラシック":
					nGenre = 5;
					break;
				case "バラエティ":
					nGenre = 6;
					break;
				case "どうよう":
					nGenre = 7;
					break;
				case "ボーカロイド":
				case "VOCALOID":
					nGenre = 8;
					break;
				default:
					nGenre = 0;
					break;

			}

			return nGenre;
		}

		private string NumtonStrジャンル(int intジャンル)
		{
			string Genre = "";
			switch (intジャンル)
			{
				case 1:
					Genre = "J-POP";
					break;
				case 2:
					Genre = "アニメ";
					break;
				case 3:
					Genre = "ゲームミュージック";
					break;
				case 4:
					Genre = "ナムコオリジナル";
					break;
				case 5:
					Genre = "クラシック";
					break;
				case 6:
					Genre = "バラエティ";
					break;
				case 7:
					Genre = "どうよう";
					break;
				case 8:
					Genre = "ボーカロイド™曲";
					break;
				default:
					Genre = "その他";
					break;

			}

			return Genre;
		}

		private TitleTextureKey ttk曲名テクスチャを生成する(string str文字, Color forecolor, Color backcolor)
		{
			return new TitleTextureKey(str文字, pfMusicName, forecolor, backcolor, 410);
		}

		private TitleTextureKey ttkサブタイトルテクスチャを生成する(string str文字)
		{
			return new TitleTextureKey(str文字, pfSubtitle, Color.White, Color.Black, 390);
		}

		private static CTexture GenerateGenreTexture(string str文字, Color forecolor, Color backcolor)
		{
			string drawstr = str文字;

			drawstr = drawstr.Replace("・", "．");

			//描画先とするImageオブジェクトを作成する
			var bmp = new Bitmap(800, 200);
			//ImageオブジェクトのGraphicsオブジェクトを作成する
			Graphics g = Graphics.FromImage(bmp);
			StringFormat sf = new StringFormat();
			//フォントオブジェクトの作成
			int fontsize = 60;
			Font fnt = new Font(TJAPlayer3.ConfigIni.FontName, fontsize);
			SizeF stringSize = g.MeasureString(drawstr, fnt, 1000, sf);
			for (; stringSize.Width > 540; fontsize--)
			{
				fnt = new Font(TJAPlayer3.ConfigIni.FontName, fontsize);
				stringSize = g.MeasureString(drawstr, fnt, 1000, sf);
			}
			//文字列を位置(0,0)、黒色で表示
			//	g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			//	g.DrawString(titleTextureKey.str文字, fnt, Brushes.Black, 300 - ((int)stringSize.Width) / 2, 80 - (int)stringSize.Height / 2);
			/////////////////////////////
			float emSize = (float)fnt.Height * fnt.FontFamily.GetEmHeight(fnt.Style) / fnt.FontFamily.GetLineSpacing(fnt.Style);
			//GraphicsPathオブジェクトの作成
			System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
			//GraphicsPathに文字列を追加する
			gp.AddString(drawstr, fnt.FontFamily, (int)fnt.Style, emSize,
				new Point(290 - ((int)stringSize.Width) / 2, 80 - (int)stringSize.Height / 2), StringFormat.GenericDefault);

			//文字列の中を塗りつぶす
			SolidBrush nakamib = new SolidBrush(forecolor/*, 300 / TJAPlayer3.Skin.Font_Edge_Ratio_Vertical*/);
			//文字列の縁を描画する
			Pen nakamip = new Pen(backcolor, 500 / TJAPlayer3.Skin.Font_Edge_Ratio_Vertical);
			nakamip.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
			g.DrawPath(nakamip, gp);
			g.FillPath(nakamib, gp);

			/////////////////////////////
			//リソースを解放する
			fnt.Dispose();
			g.Dispose();
			sf.Dispose();

			/*using (var bmp = new Bitmap(titleTextureKey.cPrivateFastFont.DrawPrivateFont(
				titleTextureKey.str文字, titleTextureKey.forecolor, titleTextureKey.backcolor, true)))
			{*/
			CTexture tx文字テクスチャ = TJAPlayer3.tテクスチャの生成(bmp);
			bmp.Dispose();

			tx文字テクスチャ.vc拡大縮小倍率.Y = 0.5f;
			tx文字テクスチャ.vc拡大縮小倍率.X = 0.5f;

			return tx文字テクスチャ;
			//	}
		}

		private CTexture ResolveTitleTexture(TitleTextureKey titleTextureKey)
		{
			if (!_titledictionary.TryGetValue(titleTextureKey, out var texture))
			{
				texture = GenerateTitleTexture(titleTextureKey);

				_titledictionary.Add(titleTextureKey, texture);
			}

			return texture;
		}

		private static CTexture GenerateTitleTexture(TitleTextureKey titleTextureKey)
		{
			using (var bmp = new Bitmap(titleTextureKey.cPrivateFastFont.DrawPrivateFont(
				titleTextureKey.str文字, titleTextureKey.forecolor, titleTextureKey.backcolor, true)))
			{
				CTexture tx文字テクスチャ = TJAPlayer3.tテクスチャの生成(bmp);
				if (tx文字テクスチャ.szテクスチャサイズ.Height > titleTextureKey.maxHeight)
				{
					tx文字テクスチャ.vc拡大縮小倍率.Y = (float)(((double)titleTextureKey.maxHeight) / tx文字テクスチャ.szテクスチャサイズ.Height);
				}

				return tx文字テクスチャ;
			}
		}

		private void ClearTitleTextureCache()
		{
			foreach (var titleTexture in _titledictionary.Values)
			{
				titleTexture.Dispose();
			}
			_titledictionary.Clear();
		}

		internal sealed class TitleTextureKey
		{
			public readonly string str文字;
			public readonly CPrivateFastFont cPrivateFastFont;
			public readonly Color forecolor;
			public readonly Color backcolor;
			public readonly int maxHeight;

			public TitleTextureKey(string str文字, CPrivateFastFont cPrivateFastFont, Color forecolor, Color backcolor, int maxHeight)
			{
				this.str文字 = str文字;
				this.cPrivateFastFont = cPrivateFastFont;
				this.forecolor = forecolor;
				this.backcolor = backcolor;
				this.maxHeight = maxHeight;
			}

			private bool Equals(TitleTextureKey other)
			{
				return string.Equals(str文字, other.str文字) &&
					   cPrivateFastFont.Equals(other.cPrivateFastFont) &&
					   forecolor.Equals(other.forecolor) &&
					   backcolor.Equals(other.backcolor) &&
					   maxHeight == other.maxHeight;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				return obj is TitleTextureKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = str文字.GetHashCode();
					hashCode = (hashCode * 397) ^ cPrivateFastFont.GetHashCode();
					hashCode = (hashCode * 397) ^ forecolor.GetHashCode();
					hashCode = (hashCode * 397) ^ backcolor.GetHashCode();
					hashCode = (hashCode * 397) ^ maxHeight;
					return hashCode;
				}
			}

			public static bool operator ==(TitleTextureKey left, TitleTextureKey right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(TitleTextureKey left, TitleTextureKey right)
			{
				return !Equals(left, right);
			}
		}

		private void tアイテム数の描画()
		{
			string s = nCurrentPosition.ToString() + "/" + nNumOfItems.ToString();
			int x = 639 - 8 - 12;
			int y = 362;

			for (int p = s.Length - 1; p >= 0; p--)
			{
				tアイテム数の描画_１桁描画(x, y, s[p]);
				x -= 8;
			}
		}
		private void tアイテム数の描画_１桁描画(int x, int y, char s数値)
		{
			int dx, dy;
			if (s数値 == '/')
			{
				dx = 48;
				dy = 0;
			}
			else
			{
				int n = (int)s数値 - (int)'0';
				dx = (n % 6) * 8;
				dy = (n / 6) * 12;
			}
			//if ( this.txアイテム数数字 != null )
			//{
			//	this.txアイテム数数字.t2D描画( CDTXMania.app.Device, x, y, new Rectangle( dx, dy, 8, 12 ) );
			//}
		}


		//数字フォント
		//-----------------
		#endregion
	}
}
