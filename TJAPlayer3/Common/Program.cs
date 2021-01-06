using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Net.Http;
using FDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TJAPlayer3
{
	internal class Program
	{
		internal static string SkinName = "Unknown";
		internal static string SkinVersion = "Unknown";
		internal static string SkinCreator = "Unknown";
		[STAThread]
		private static void Main()
		{
			Trace.WriteLine("Current Directory: " + Environment.CurrentDirectory);
			Trace.WriteLine("EXEのあるフォルダ: " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			// BEGIN #23670 2010.11.13 from: キャッチされない例外は放出せずに、ログに詳細を出力する。
			// BEGIM #24606 2011.03.08 from: DEBUG 時は例外発生箇所を直接デバッグできるようにするため、例外をキャッチしないようにする。
			//2020.04.15 Mr-Ojii DEBUG 時も例外をキャッチするようにした。
			try
			{
				using (var mania = new TJAPlayer3())
					mania.Run();

				Trace.WriteLine("");
				Trace.WriteLine("Thank You For Playing!!!");
			}
			catch (Exception e)
			{
				Trace.WriteLine("");
				Trace.Write(e.ToString());
				Trace.WriteLine("");
				Trace.WriteLine("An error has occurred. Sorry.");
				AssemblyName asmApp = Assembly.GetExecutingAssembly().GetName();

				//エラーが発生したことをユーザーに知らせるため、HTMLを作成する。
				using (StreamWriter writer = new StreamWriter(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Error.html", false, Encoding.UTF8)) 
				{
					writer.WriteLine("<html>");
					writer.WriteLine("<head>");
					writer.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html\" charset=\"utf-8\">");
					writer.WriteLine("<style>");
					writer.WriteLine("<!--");
					writer.WriteLine("table{ border-collapse: collapse; } td,th { border: 2px solid; }");
					writer.WriteLine("-->");
					writer.WriteLine("</style>");
					writer.WriteLine("</head>");
					writer.WriteLine("<body>");
					writer.WriteLine("<h1>An error has occurred.(エラーが発生しました。)</h1>");
#if DEBUG
					writer.WriteLine("<p>It is a debug build, so it did not send any error information.(Debugビルドのため、エラー情報を送信しませんでした。)</p>");
#else
					writer.WriteLine("<p>Error information has been sent.(エラー情報を送信しました。)</p>");
#endif
					writer.WriteLine("<table>");
					writer.WriteLine("<tbody>");
					writer.WriteLine("<tr><th>Name</th><th>Version</th><th>Exception</th><th>DateTime</th><th>SkinName</th><th>SkinVersion</th><th>SkinCreator</th><th>OS</th></tr>");
					writer.WriteLine($"<tr><td>{asmApp.Name}</td><td>{asmApp.Version.ToString()}</td><td>{e.ToString()}</td><td>{DateTime.UtcNow.ToString()}</td><td>{SkinName}</td><td>{SkinVersion}</td><td>{SkinCreator}</td><td>{Environment.OSVersion.ToString()}</td></tr>");
					writer.WriteLine("</tbody>");
					writer.WriteLine("</table>");
					writer.WriteLine("</body>");
					writer.WriteLine("</html>");
				}
				CWebOpen.Open(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Error.html");

#if !DEBUG
				//エラーの送信
				JObject errorjsonobject = new JObject(
					new JProperty("name", asmApp.Name),
					new JProperty("version", asmApp.Version.ToString()),
					new JProperty("exception", e.ToString()),
					new JProperty("datetime", DateTime.UtcNow.ToString()),
					new JProperty("skinname", SkinName),
					new JProperty("skinversion", SkinVersion),
					new JProperty("skincreator", SkinCreator),
					new JProperty("operatingsystem", Environment.OSVersion.ToString())
					);

				using (var client = new HttpClient())
				{
					var content = new StringContent(JsonConvert.SerializeObject(errorjsonobject, Formatting.None), Encoding.UTF8, "application/json");

					var resString = client.PostAsync("https://script.google.com/macros/s/AKfycbw6QWCXEEbciDV7tA5zV72EyLkynlJUCiM0cRz5TR4nDQeRUiA/exec", content).Result;
				}
#endif
			}

			// END #24606 2011.03.08 from
			// END #23670 2010.11.13 from

			if (Trace.Listeners.Count > 1)
				Trace.Listeners.RemoveAt(1);

			// END #24615 2011.03.09 from
		}
	}
}
