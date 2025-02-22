﻿namespace TJAPlayer3;

/// <summary>
/// 段位認定を管理するクラス。
/// </summary>
public class Dan_C
{
    public Dan_C(Dan_C dan_C) : this(dan_C.Type, dan_C.Value, dan_C.Range)
    {

    }

    /// <summary>
    /// ぬるぽ抑制用
    /// </summary>
    public Dan_C()
    {
        IsEnable = false;
    }

    /// <summary>
    /// 段位認定の条件を初期化し、生成します。
    /// </summary>
    /// <param name="examType">条件の種別。</param>
    /// <param name="value">条件の合格量。</param>
    /// <param name="examRange">条件の合格の範囲。</param>
    public Dan_C(Exam.Type examType, int[] value, Exam.Range examRange)
    {
        IsEnable = true;
        NotReached = false;
        Type = examType;
        Range = examRange;

        if (value.Length > 2 && examType != Exam.Type.Gauge && examType != Exam.Type.Combo && examType != Exam.Type.Score)
        {
            IsForEachSongs = true;
        }
        else
        {
            IsForEachSongs = false;
        }

        #region[Valueの代入]
        List<int> valuetmp = new List<int>();
        this.Value = new int[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] != -1)
                valuetmp.Add(value[i]);
        }
        if (valuetmp.Count % 2 != 0)
            valuetmp.Add(valuetmp[valuetmp.Count - 1]);
        this.Value = valuetmp.ToArray();
        #endregion

        #region[IsClearedの代入]
        this.IsCleared = new bool[Value.Length];
        for (int i = 0; i < Value.Length; i++)
        {
            IsCleared[i] = false;
        }
        #endregion

        #region[Amountの初期化]
        Amount = new int[Value.Length / 2];
        for (int i = 0; i < Value.Length / 2; i++)
        {
            Amount[i] = 0;
        }
        #endregion

        this.NowSongNum = 0;
    }

    /// <summary>
    /// 条件と現在の値を評価して、クリアしたかどうかを判断します。
    /// </summary>
    /// <param name="nowValue">その条件の現在の値。</param>
    public bool Update(int nowValue)
    {
        for (int i = 0; i < this.NowSongNum; i++)
        {
            nowValue -= this.Amount[i];
        }

        var isChangedAmount = false;
        if (!this.IsEnable) return isChangedAmount;
        if (GetAmount() < nowValue) isChangedAmount = true;
        if (this.Range == Exam.Range.Less && nowValue > GetValue(false)) isChangedAmount = false; // n未満でその数を超えたらfalseを返す。
        SetAmount(nowValue);
        SetCleared();

        return isChangedAmount;
    }

    /// <summary>
    /// 各合格条件のボーダーを返します。
    /// </summary>
    /// <param name="isGoldValue">trueを指定すると、金合格条件を返します。</param>
    /// <returns>合格条件の値。</returns>
    public int GetValue(bool isGoldValue)
    {
        return isGoldValue == true ? this.Value[this.NowSongNum * 2 + 1] : this.Value[this.NowSongNum * 2];
    }

    /// <summary>
    /// 各合格条件のボーダーを返します。
    /// </summary>
    /// <param name="isGoldValue">trueを指定すると、金合格条件を返します。</param>
    /// <returns>合格条件の値。</returns>
    public int GetValue(bool isGoldValue, int index)
    {
        return isGoldValue == true ? this.Value[index * 2 + 1] : this.Value[index * 2];
    }

    /// <summary>
    /// 現在の値を設定します。
    /// </summary>
    /// <param name="amount">現在の値。</param>
    public void SetAmount(int amount)
    {
        this.Amount[this.NowSongNum] = amount;
    }

    /// <summary>
    /// 現在の値を返します。
    /// </summary>
    /// <returns>現在の値。</returns>
    public int GetAmount()
    {
        return this.Amount[this.NowSongNum];
    }

    /// <summary>
    /// indexで指定された曲数の値を返します。
    /// </summary>
    /// <returns>現在の値。</returns>
    public int GetAmount(int index)
    {
        return this.Amount[index];
    }

    /// <summary>
    /// 条件にクリアしているかどうか返します。
    /// </summary>
    /// <returns>条件にクリアしているかどうか。</returns>
    public bool GetCleared(bool isGoldValue)//済
    {
        int mod = isGoldValue ? 1 : 0;
        bool clear = true;
        for (int i = 0; i < IsCleared.Length / 2; i++)
        {
            if (!IsCleared[i * 2 + mod])
                clear = false;
        }

        return clear;
    }


    /// <summary>
    /// 現在の曲の条件にクリアしているかどうか返します。
    /// </summary>
    /// <returns>条件にクリアしているかどうか。</returns>
    public bool GetNowCleared(bool isGoldValue)//済
    {
        int mod = isGoldValue ? 1 : 0;
        return this.IsCleared[this.NowSongNum * 2 + mod];
    }

    public void SetNowSongNum(int Num)
    {
        if (this.IsForEachSongs)
            this.NowSongNum = Num;
    }

    /// <summary>
    /// 条件と現在の値をチェックして、合格もしくは金合格をしてるか否かを更新する。
    /// </summary>
    private void SetCleared()
    {
        if (this.Range == Exam.Range.More)
        {
            IsCleared[this.NowSongNum * 2] = (this.GetAmount() >= GetValue(false)) ? true : false;
            IsCleared[this.NowSongNum * 2 + 1] = (this.GetAmount() >= GetValue(true)) ? true : false;
        }
        else
        {
            IsCleared[this.NowSongNum * 2] = (this.GetAmount() < GetValue(false)) ? true : false;
            IsCleared[this.NowSongNum * 2 + 1] = (this.GetAmount() < GetValue(true)) ? true : false;
        }
    }

    /// <summary>
    /// ゲージの描画のための百分率を返す。
    /// </summary>
    /// <returns>Amountの百分率。</returns>
    public int GetAmountToPercent()
    {
        var percent = 0.0D;
        if (GetValue(false) == 0)
        {
            return 0;
        }
        if (this.Range == Exam.Range.More)
        {
            switch (this.Type)
            {
                case Exam.Type.Gauge:
                case Exam.Type.JudgePerfect:
                case Exam.Type.JudgeGood:
                case Exam.Type.JudgeBad:
                case Exam.Type.Score:
                case Exam.Type.Roll:
                case Exam.Type.Hit:
                case Exam.Type.Combo:
                    percent = 1.0 * GetAmount() / GetValue(false);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (this.Type)
            {
                case Exam.Type.Gauge:
                case Exam.Type.JudgePerfect:
                case Exam.Type.JudgeGood:
                case Exam.Type.JudgeBad:
                case Exam.Type.Score:
                case Exam.Type.Roll:
                case Exam.Type.Hit:
                case Exam.Type.Combo:
                    percent = (1.0 * (GetValue(false) - GetAmount())) / GetValue(false);
                    break;
                default:
                    break;
            }
        }
        percent = percent * 100.0;
        if (percent < 0.0)
            percent = 0.0D;
        if (percent > 100.0)
            percent = 100.0D;
        return (int)percent;
    }

    /// <summary>
    /// ゲージの描画のための百分率を返す。
    /// </summary>
    /// <returns>Amountの百分率。</returns>
    public int GetAmountToPercent(int index)
    {
        var percent = 0.0D;
        if (GetValue(false, index) == 0)
        {
            return 0;
        }
        if (this.Range == Exam.Range.More)
        {
            switch (this.Type)
            {
                case Exam.Type.Gauge:
                case Exam.Type.JudgePerfect:
                case Exam.Type.JudgeGood:
                case Exam.Type.JudgeBad:
                case Exam.Type.Score:
                case Exam.Type.Roll:
                case Exam.Type.Hit:
                case Exam.Type.Combo:
                    percent = 1.0 * GetAmount(index) / GetValue(false, index);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (this.Type)
            {
                case Exam.Type.Gauge:
                case Exam.Type.JudgePerfect:
                case Exam.Type.JudgeGood:
                case Exam.Type.JudgeBad:
                case Exam.Type.Score:
                case Exam.Type.Roll:
                case Exam.Type.Hit:
                case Exam.Type.Combo:
                    percent = (1.0 * (GetValue(false, index) - GetAmount(index))) / GetValue(false, index);
                    break;
                default:
                    break;
            }
        }
        percent = percent * 100.0;
        if (percent < 0.0)
            percent = 0.0D;
        if (percent > 100.0)
            percent = 100.0D;
        return (int)percent;
    }

    /// <summary>
    /// 条件に達成できる見込みがあるかどうか値を代入します。
    /// </summary>
    /// <param name="notReached">未達成かどうか。</param>
    public void SetReached(bool notReached)
    {
        NotReached = notReached;
    }

    /// <summary>
    /// 条件に達成できる見込みがあるかどうかを返します。
    /// </summary>
    /// <returns>条件に達成できる見込みがあるかどうか。</returns>
    public bool GetReached()
    {
        return NotReached;
    }


    // オーバーライドメソッド
    /// <summary>
    /// ToString()のオーバーライドメソッド。段位認定モードの各条件の現在状況をString型で返します。
    /// </summary>
    /// <returns>段位認定モードの各条件の現在状況。</returns>
    public override string ToString()
    {
        return String.Format("Type: {0} / Value: {1}/{2} / Range: {3} / Amount: {4} / Clear: {5}/{6} / Percent: {7} / NotReached: {8}", this.Type, GetValue(false), GetValue(true), this.Range, GetAmount(), GetCleared(false), GetCleared(true), GetAmountToPercent(), GetReached());
    }


    // フィールド
    /// <summary>
    /// その条件が有効であるかどうか。
    /// </summary>
    public readonly bool IsEnable;
    /// <summary>
    /// 条件の種別。
    /// </summary>
    public readonly Exam.Type Type;
    /// <summary>
    /// 条件の範囲。
    /// </summary>
    public readonly Exam.Range Range;
    /// <summary>
    /// 条件の値。
    /// </summary>
    public int[] Value;
    /// <summary>
    /// 量。
    /// </summary>
    public int[] Amount;
    /// <summary>
    /// 条件をクリアしているか否か。
    /// </summary>
    public readonly bool[] IsCleared;

    /// <summary>
    /// IsForEachSongs=trueの場合、現在の曲の番号-1
    /// IsForEachSongs=falseの場合、0
    /// </summary>
    private int NowSongNum;

    /// <summary>
    /// 条件の達成見込みがなくなったら、真になる。
    /// この変数が一度trueになれば、基本的にfalseに戻ることはない。
    /// (スコア加算については、この限りではない。)
    /// </summary>
    private bool NotReached = false;

    /// <summary>
    /// 段位条件がそれぞれの曲に対してか。
    /// </summary>
    public readonly bool IsForEachSongs;
}

public static class Exam
{
    /// <summary>
    /// 段位認定の条件の種別。
    /// </summary>
    public enum Type
    {
        Gauge,
        JudgePerfect,
        JudgeGood,
        JudgeBad,
        Score,
        Roll,
        Hit,
        Combo
    }

    /// <summary>
    /// 段位認定の合格範囲。
    /// </summary>
    public enum Range
    {
        /// <summary>
        /// 以上
        /// </summary>
        More,
        /// <summary>
        /// 未満
        /// </summary>
        Less
    }

    /// <summary>
    /// ステータス。
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// 不合格
        /// </summary>
        Failure,
        /// <summary>
        /// 合格
        /// </summary>
        Success,
        /// <summary>
        /// より良い合格
        /// </summary>
        Better_Success
    }
}
