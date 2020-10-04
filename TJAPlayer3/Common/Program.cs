using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using FDK;
using System.Reflection;

namespace TJAPlayer3
{
	internal class Program
	{
#if NETCOREAPP30
		Encoding.RegisterProvider();//CP932用
#endif
#region [ 二重起動チェック]
		//-----------------------------
		private static Mutex mutex二重起動防止用;
		//-----------------------------
#endregion

		[STAThread] 
		private static void Main()
		{
			mutex二重起動防止用 = new Mutex( false, "TJAPlayer3-f" );
			bool mutexbool = mutex二重起動防止用.WaitOne(0, false);
		kidou:
			if (mutexbool)
			{
				string newLine = Environment.NewLine;
				bool bDLLnotfound = false;

				Trace.WriteLine( "Current Directory: " + Environment.CurrentDirectory );
				Trace.WriteLine( "EXEのあるフォルダ: " + Path.GetDirectoryName( Application.ExecutablePath ) );

				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

				if ( !bDLLnotfound )
				{
					// BEGIN #23670 2010.11.13 from: キャッチされない例外は放出せずに、ログに詳細を出力する。
					// BEGIM #24606 2011.03.08 from: DEBUG 時は例外発生箇所を直接デバッグできるようにするため、例外をキャッチしないようにする。
					//2020.04.15 Mr-Ojii DEBUG 時も例外をキャッチするようにした。
					try
					{
						using ( var mania = new TJAPlayer3() )
							mania.Run();

						Trace.WriteLine( "" );
						Trace.WriteLine( "遊んでくれてありがとう！" );
					}
					catch( Exception e )
					{
						Trace.WriteLine( "" );
						Trace.Write( e.ToString() );
						Trace.WriteLine( "" );
						Trace.WriteLine( "エラーだゴメン！（涙" );
						AssemblyName asmApp = Assembly.GetExecutingAssembly().GetName();
						MessageBox.Show( "エラーが発生しました。\n" +
							"原因がわからない場合は、以下のエラー文を添えて、エラー送信フォームに送信してください。\n" + 
							e.ToString(), asmApp.Name + " Ver." + asmApp.Version.ToString().Substring(0, asmApp.Version.ToString().Length - 2) + " Error", MessageBoxButtons.OK, MessageBoxIcon.Error );    // #23670 2011.2.28 yyagi to show error dialog
						DialogResult result = MessageBox.Show("エラー送信フォームを開きますか?(ブラウザが起動します)\n",
							asmApp.Name + " Ver." + asmApp.Version.ToString().Substring(0, asmApp.Version.ToString().Length - 2),
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Asterisk);
						if (result == DialogResult.Yes)
						{
							DialogResult result2 = MessageBox.Show("GitHubのエラー送信フォームを開きますか?※GitHubアカウントが必要です。\n\nGoogleのエラー送信フォームを開きますか?※アカウントの必要なし\n\nGitHubのからのエラー報告のほうが「Mr.おじい」が早くエラーの存在に気づけます。\n(Y:GitHub / N:Google)",
								asmApp.Name + " Ver." + asmApp.Version.ToString().Substring(0, asmApp.Version.ToString().Length - 2),
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Asterisk);

							if (result2 == DialogResult.Yes)
							{
								Process.Start("https://github.com/Mr-Ojii/TJAPlayer3-f/issues/new?body=エラー文(TJAPlayer3-fから開いた場合は自動入力されます)%0D%0A" +
									System.Web.HttpUtility.UrlEncode(e.ToString()) +
									"%0D%0A" +
									"%0D%0A" +
									"使用しているスキン名・バージョン%0D%0A" +
									"%0D%0A" +
									"%0D%0A" +
									"バグを引き起こすまでの手順を書いてください%0D%0A" +
									"%0D%0A" +
									"%0D%0A" +
									"再生していた譜面(.tja)または画面%0D%0A" +
									"%0D%0A" +
									"%0D%0A" +
									"使用しているOS%0D%0A" +
									"%0D%0A" +
									"%0D%0A" +
									"不具合の内容%0D%0A" +
									"%0D%0A" +
									"%0D%0A" +
									"(追加情報を自由に書いてください(任意))%0D%0A");
							}
							else {
								Process.Start("https://docs.google.com/forms/d/e/1FAIpQLSffkhp-3kDJIZH23xMoweik5sAgy2UyaIkEQd1khn9DuR_RWg/viewform?entry.1025217940=" +
									System.Web.HttpUtility.UrlEncode(e.ToString()));
							}
						}
					}
					// END #24606 2011.03.08 from
					// END #23670 2010.11.13 from

					if ( Trace.Listeners.Count > 1 )
						Trace.Listeners.RemoveAt( 1 );
				}

				// BEGIN #24615 2011.03.09 from: Mutex.WaitOne() が true を返した場合は、Mutex のリリースが必要である。
				mutex二重起動防止用.ReleaseMutex();
				mutex二重起動防止用 = null;

				// END #24615 2011.03.09 from
			}
			else		// DTXManiaが既に起動中
			{
				DialogResult dr = MessageBox.Show("すでにTJAPlayer3-fが起動していますが、起動しますか？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (dr == DialogResult.Yes)
				{
					mutexbool = true;
					goto kidou;
				}
				else {
					Environment.Exit(0);
				}
			}
		}
	}
}
