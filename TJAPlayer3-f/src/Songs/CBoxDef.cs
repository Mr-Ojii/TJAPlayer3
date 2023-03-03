using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Drawing;

namespace TJAPlayer3
{
	internal class CBoxDef
	{
		// プロパティ

		public string Genre;
		public string Title;
		public Color ForeColor;
		public Color BackColor;

		// コンストラクタ

		public CBoxDef()
		{
			this.Title = "";
			this.Genre = "";
			ForeColor = Color.White;
			BackColor = Color.Black;
		}

		public CBoxDef( string boxdefFileName )
			: this()
		{
			Encoding boxdefEnc = CJudgeTextEncoding.JudgeFileEncoding(boxdefFileName);
			using (StreamReader reader = new StreamReader(boxdefFileName, boxdefEnc))
			{
				string str = null;
				while ((str = reader.ReadLine()) != null)
				{
					if (str.Length == 0)
						continue;
						
					try
					{
						char[] ignoreCharsWoColon = new char[] { ' ', '\t' };

						str = str.TrimStart(ignoreCharsWoColon);
						if ((str[0] == '#') && (str[0] != ';'))
						{
							if (str.IndexOf(';') != -1)
							{
								str = str.Substring(0, str.IndexOf(';'));
							}

							char[] ignoreChars = new char[] { ':', ' ', '\t' };

							if (str.StartsWith("#TITLE", StringComparison.OrdinalIgnoreCase))
							{
								this.Title = str.Substring(6).Trim(ignoreChars);
							}
							else if (str.StartsWith("#GENRE", StringComparison.OrdinalIgnoreCase))
							{
								this.Genre = str.Substring(6).Trim(ignoreChars);
							}
							else if (str.StartsWith("#FORECOLOR", StringComparison.OrdinalIgnoreCase))
							{
								this.ForeColor = ColorTranslator.FromHtml(str.Substring(10).Trim(ignoreChars));
							}
							else if (str.StartsWith("#BACKCOLOR", StringComparison.OrdinalIgnoreCase))
							{
								this.BackColor = ColorTranslator.FromHtml(str.Substring(10).Trim(ignoreChars));
							}
						}
						continue;
					}
					catch (Exception e)
					{
						Trace.TraceError(e.ToString());
						Trace.TraceError("An exception has occurred, but processing continues.");
						continue;
					}
				}
			}
		}
	}
}
