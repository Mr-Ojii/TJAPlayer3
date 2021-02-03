using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FDK
{
	public class CTimer : CTimerBase
	{
		public override long nシステム時刻ms
		{
			get
			{
				long time = (long)(DateTime.Now - this.生成時刻).TotalMilliseconds; //最初は(long.MaxValue)ms目でループ
																				//次は(ulong.MaxValue)msごとにループするはず。
																				//人が生きている間にはループしそうにないですね。
				if (this.前回時刻 > time)
				{
					if (Math.Abs(this.前回時刻 - time) > long.MaxValue)
					{
						//long.MaxValue以上の差が出た場合、ループしたとする。
						Trace.WriteLine("Timer Looped!");
					}
					else
					{
						//それ以外の場合、ユーザーが意図的に時刻を変更したとみなす。
						this.生成時刻 = this.生成時刻.AddMilliseconds(time - this.前回時刻);
						time = (long)(DateTime.Now - this.生成時刻).TotalMilliseconds;//生成し直す
					}
				}
				else
				{
					if (time - this.前回時刻 > 30000)
					{
						//前回より30秒以上時が進んでいる場合も、ユーザーが意図的に時刻を変更したとみなす。
						this.生成時刻 = this.生成時刻.AddMilliseconds(time - this.前回時刻);
						time = (long)(DateTime.Now - this.生成時刻).TotalMilliseconds;//生成し直す

					}
				}
				this.前回時刻 = time;
				return (long)this.前回時刻;
			}
		}

		public CTimer()
			: base()
		{
			this.生成時刻 = DateTime.Now;
			
			base.tリセット();
		}
		
		public override void Dispose()
		{
		}
		private DateTime 生成時刻;
		private long 前回時刻 = -5000;//最初は0以下
	}
}
