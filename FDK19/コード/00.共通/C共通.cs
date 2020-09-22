using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace FDK
{
	public class C共通
	{
		// 解放

		public static void tDisposeする<T>( ref T obj )
		{
			if( obj == null )
				return;

			var d = obj as IDisposable;

			if( d != null )
			{
				d.Dispose();
				obj = default( T );
			}
		}
		public static void tDisposeする<T>( T obj )
		{
			if( obj == null )
				return;

			var d = obj as IDisposable;

			if( d != null )
				d.Dispose();
		}
		public static void tCOMオブジェクトを解放する<T>( ref T obj )
		{
			if( obj != null )
			{
				try
				{
					Marshal.ReleaseComObject( obj );
				}
				catch
				{
					// COMがマネージドコードで書かれている場合、ReleaseComObject は例外を発生させる。
					// http://www.infoq.com/jp/news/2010/03/ReleaseComObject-Dangerous
				}

				obj = default( T );
			}
		}

		public static void t完全なガベージコレクションを実施する()
		{
			GC.Collect();					// アクセス不可能なオブジェクトを除去し、ファイナライぜーション実施。
			GC.WaitForPendingFinalizers();	// ファイナライゼーションが終わるまでスレッドを待機。
			GC.Collect();					// ファイナライズされたばかりのオブジェクトに関連するメモリを開放。

			// 出展: http://msdn.microsoft.com/ja-jp/library/ms998547.aspx#scalenetchapt05_topic10
		}


		// ログ
		public static void t例外の詳細をログに出力する( Exception e )
		{
			Trace.WriteLine( "---例外ここから----" );
			Trace.WriteLine( e.ToString() );
			Trace.WriteLine( "---例外ここまで----" );
		}

		// 数学

		public static bool bToggleBoolian( ref bool bFlag )
		{
			if( bFlag == true ) bFlag = false;
			else if( bFlag == false ) bFlag = true;

			return true;
		}
	}	
}
