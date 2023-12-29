using FDK;

namespace TJAPlayer3;

public class CStage : CActivity
{
    // プロパティ
    internal EStage eStageID;
    public enum EStage
    {
        StartUp,
        Title,
        Config,
        SongSelect,
        SongLoading,
        Playing,
        Result,
        ChangeSkin,						// #28195 2011.5.4 yyagi
        Ending,
        Maintenance                         //2020.06.01 Mr-Ojii
    }

    internal Eフェーズ eフェーズID;
    public enum Eフェーズ
    {
        共通_通常状態,
        共通_FadeIn,
        共通_FadeOut,
        共通_終了状態,
        起動0_システムサウンドを構築,
        起動00_songlistから曲リストを作成する,
        起動1_SongsDBからスコアキャッシュを構築,
        起動2_曲を検索してリストを作成する,
        起動3_スコアキャッシュをリストに反映する,
        起動4_スコアキャッシュになかった曲をファイルから読み込んで反映する,
        起動5_曲リストへ後処理を適用する,
        起動6_スコアキャッシュをSongsDBに出力する,
        起動_テクスチャの読み込み,
        起動7_完了,
        タイトル_起動画面からのFadeIn,
        選曲_結果画面からのFadeIn,
        選曲_コース選択画面へのFadeOut, //2016.10.20 kairera0467
        選曲_NowLoading画面へのFadeOut,
        NOWLOADING_DTXファイルを読み込む,
        NOWLOADING_WAVファイルを読み込む,
        NOWLOADING_BMPファイルを読み込む,
        NOWLOADING_システムサウンドBGMの完了を待つ,
        演奏_STAGE_FAILED,
        演奏_STAGE_FAILED_FadeOut,
        演奏_STAGE_CLEAR_FadeOut,
        演奏_演奏終了演出, //2016.07.15 kairera0467
        演奏_再読込
    }
}
