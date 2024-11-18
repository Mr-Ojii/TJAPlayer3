using FDK;

namespace TJAPlayer3;

internal class CActSelectPresound : CActivity
{
    // メソッド

    public CActSelectPresound()
    {
    }
    public void t選択曲が変更された()
    {
        Cスコア cスコア = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア;

        this.tサウンドの停止MT();
        if ((cスコア is not null) && ((!(cスコア.FileInfo.DirAbsolutePath + cスコア.譜面情報.strBGMファイル名).Equals(this.str現在のファイル名) || (this.sound is null)) || !this.sound.bPlaying))
        {
            this.tBGMFadeIn開始();
            this.long再生位置 = -1;
            if ((cスコア.譜面情報.strBGMファイル名 is not null) && (cスコア.譜面情報.strBGMファイル名.Length > 0))
            {
                this.ct再生待ちウェイト = new CCounter(0, 1, 500, TJAPlayer3.app.Timer);
            }
        }
    }


    // CActivity 実装

    public override void On活性化()
    {
        this.sound = null;
        token = new CancellationTokenSource();
        this.str現在のファイル名 = "";
        this.ct再生待ちウェイト = null;
        this.ctBGMFadeOut用 = null;
        this.ctBGMFadeIn用 = null;
        this.long再生位置 = -1;
        this.long再生開始時のシステム時刻 = -1;
        base.On活性化();
    }
    public override void On非活性化()
    {
        this.tサウンドの停止MT();
        if (token is not null)
        {
            token.Cancel();
            token.Dispose();
            token = null;
        }
        this.ct再生待ちウェイト = null;
        this.ctBGMFadeIn用 = null;
        this.ctBGMFadeOut用 = null;
        base.On非活性化();
    }
    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            if ((this.ctBGMFadeIn用 is not null) && this.ctBGMFadeIn用.b進行中)
            {
                this.ctBGMFadeIn用.t進行();
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].nAutomationLevel_現在のサウンド = this.ctBGMFadeIn用.n現在の値;
                if (this.ctBGMFadeIn用.b終了値に達した)
                {
                    this.ctBGMFadeIn用.t停止();
                }
            }
            if ((this.ctBGMFadeOut用 is not null) && this.ctBGMFadeOut用.b進行中)
            {
                this.ctBGMFadeOut用.t進行();
                TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].nAutomationLevel_現在のサウンド = CSound.MaximumAutomationLevel - this.ctBGMFadeOut用.n現在の値;
                if (this.ctBGMFadeOut用.b終了値に達した)
                {
                    this.ctBGMFadeOut用.t停止();
                }
            }

            this.t進行処理_プレビューサウンド();

            if (this.sound is not null && CSoundManager.rc演奏用タイマ is not null)
            {
                Cスコア cスコア = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア;
                if (long再生位置 == -1)
                {
                    this.long再生開始時のシステム時刻 = CSoundManager.rc演奏用タイマ.nシステム時刻ms;
                    this.long再生位置 = cスコア.譜面情報.nデモBGMオフセット;
                    this.sound.t再生位置を変更する(cスコア.譜面情報.nデモBGMオフセット);
                }
                else
                {
                    this.long再生位置 = CSoundManager.rc演奏用タイマ.nシステム時刻ms - this.long再生開始時のシステム時刻;
                    if (this.long再生位置 >= this.sound.nDurationms - cスコア.譜面情報.nデモBGMオフセット) //2020.04.18 Mr-Ojii #DEMOSTARTから何度も再生するために追加
                        this.long再生位置 = -1;
                }
            }
        }
        return 0;
    }


    // その他

    #region [ private ]
    //-----------------
    private CancellationTokenSource? token; // 2019.03.23 kairera0467 マルチスレッドの中断処理を行うためのトークン
    private CCounter?   ctBGMFadeOut用,
                        ctBGMFadeIn用,
                        ct再生待ちウェイト;
    private long long再生位置;
    private long long再生開始時のシステム時刻;
    private CSound? sound;
    private string? str現在のファイル名;

    private void tBGMFadeOut開始()
    {
        if (this.ctBGMFadeIn用 is not null)
        {
            this.ctBGMFadeIn用.t停止();
        }
        this.ctBGMFadeOut用 = new CCounter(0, 100, 10, TJAPlayer3.app.Timer);
        this.ctBGMFadeOut用.n現在の値 = 100 - TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].nAutomationLevel_現在のサウンド;
    }
    private void tBGMFadeIn開始()
    {
        if (this.ctBGMFadeOut用 is not null)
        {
            this.ctBGMFadeOut用.t停止();
        }
        this.ctBGMFadeIn用 = new CCounter(0, 100, 20, TJAPlayer3.app.Timer);
        this.ctBGMFadeIn用.n現在の値 = TJAPlayer3.app.Skin.SystemSounds[Eシステムサウンド.BGM選曲画面].nAutomationLevel_現在のサウンド;
    }
    private async void tプレビューサウンドの作成()
    {
        Cスコア cスコア = TJAPlayer3.stage選曲.act曲リスト.r現在選択中のスコア;
        if ((cスコア is not null) && !string.IsNullOrEmpty(cスコア.譜面情報.strBGMファイル名) && TJAPlayer3.stage選曲.eフェーズID != CStage.Eフェーズ.選曲_NowLoading画面へのFadeOut)
        {
            string strPreviewFilename = cスコア.FileInfo.DirAbsolutePath + cスコア.譜面情報.strBGMファイル名;
            try
            {
                // 2020.06.15 Mr-Ojii TJAP2fPCより拝借-----------
                // 2019.03.22 kairera0467 簡易マルチスレッド化
                CSound? tmps = await Task.Run<CSound?>(() =>
                {
                    token = new CancellationTokenSource();
                    return this.tプレビューサウンドの作成MT(strPreviewFilename);
                });

                token?.Token.ThrowIfCancellationRequested();
                this.tサウンドの停止MT();

                this.sound = tmps;
                //------------

                // 2018-08-27 twopointzero - DO attempt to load (or queue scanning) loudness metadata here.
                //                           Initialization, song enumeration, and/or interactions may have
                //                           caused background scanning and the metadata may now be available.
                //                           If is not yet available then we wish to queue scanning.
                var loudnessMetadata = cスコア.譜面情報.SongLoudnessMetadata
                                        ?? LoudnessMetadataScanner.LoadForAudioPath(strPreviewFilename);
                if (this.sound is not null)
                    TJAPlayer3.SongGainController.Set(cスコア.譜面情報.SongVol, loudnessMetadata, this.sound);

                this.long再生位置 = -1;
                this.sound?.t再生を開始する(true);
                if (this.long再生位置 == -1 && CSoundManager.rc演奏用タイマ is not null)
                {
                    this.long再生開始時のシステム時刻 = CSoundManager.rc演奏用タイマ.nシステム時刻ms;
                    this.long再生位置 = cスコア.譜面情報.nデモBGMオフセット;
                    this.sound?.t再生位置を変更する(cスコア.譜面情報.nデモBGMオフセット);
                    this.long再生位置 = CSoundManager.rc演奏用タイマ.nシステム時刻ms - this.long再生開始時のシステム時刻;
                }

                this.str現在のファイル名 = strPreviewFilename;
                this.tBGMFadeOut開始();
                Trace.TraceInformation("プレビューサウンドを生成しました。({0})", strPreviewFilename);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("プレビューサウンドの生成に失敗しました。({0})", strPreviewFilename);
                if (this.sound is not null)
                {
                    this.sound.Dispose();
                }
                this.sound = null;
            }
        }
    }
    private void t進行処理_プレビューサウンド()
    {
        if ((this.ct再生待ちウェイト is not null) && this.ct再生待ちウェイト.b進行中)
        {
            this.ct再生待ちウェイト.t進行();
            if (this.ct再生待ちウェイト.b終了値に達した)
            {
                this.ct再生待ちウェイト.t停止();
                if (!TJAPlayer3.stage選曲.act曲リスト.bスクロール中)
                {
                    this.tプレビューサウンドの作成();
                }
            }
        }
    }

    //Mr-Ojii 以下、TJAP2fPCより拝借＆TJAP3f用に改変
    //-----------------

    public void tサウンドの停止MT()
    {
        if (this.sound is not null)
        {
            if (token is not null)
            {
                token.Cancel();
            }
            this.sound.t再生を停止する();
            this.sound.Dispose();
            this.sound = null;
        }
    }

    /// <summary>
    /// マルチスレッドに対応したプレビューサウンドの作成処理
    /// </summary>
    /// <param name="path">サウンドファイルのパス</param>
    /// <param name="token">中断用トークン</param>
    /// <returns></returns>
    private CSound? tプレビューサウンドの作成MT(string path)
    {
        try
        {
            return TJAPlayer3.SoundManager.tCreateSound(path, ESoundGroup.SongPreview);
        }
        catch
        {
            Trace.TraceError("プレビューサウンドの生成に失敗しました。({0})", path);
        }

        return null;
    }
    #endregion
}
