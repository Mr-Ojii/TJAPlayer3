using System;
using System.Collections.Generic;
using System.Text;

namespace FDK
{
	public static class COS //2020.05.08 Mr-Ojii DTXManiaからいろいろと移植
	{
		/// <summary>
		/// OSがXP以前ならfalse, Vista以降ならtrueを返す
		/// </summary>
		/// <returns></returns>

		public static bool bIsVistaOrLater()
		{
			return bCheckOSVersion(6, 0);
		}
		/// <summary>
		/// OSがVista以前ならfalse, Win7以降ならtrueを返す
		/// </summary>
		/// <returns></returns>
		public static bool bIsWin7OrLater()
		{
			return bCheckOSVersion(6, 1);
		}
		/// <summary>
		/// OSがWin7以前ならfalse, Win8以降ならtrueを返す
		/// </summary>
		/// <returns></returns>
		public static bool bIsWin8OrLater()
		{
			return bCheckOSVersion(6, 2);
		}
		/// <summary>
		/// OSがWin10以前ならfalse, Win10以降ならtrueを返す
		/// </summary>
		/// <returns></returns>
		public static bool bIsWin10OrLater()
		{
			return bCheckOSVersion(10, 0);
		}

		/// <summary>
		/// 指定のOSバージョン以上であればtrueを返す
		/// </summary>
		private static bool bCheckOSVersion(int major, int minor)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)      // NT系でなければ、XP以前か、PC Windows系以外のOS。
			{
				return false;
			}

			int _major, _minor;
			tpGetOSVersion(out _major, out _minor);

			if (_major > major)
			{
				return true;
			}
			else if (_major == major && _minor >= minor)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void tpGetOSVersion(out int major, out int minor)
		{
			var os = Environment.OSVersion;
			major = os.Version.Major;
			minor = os.Version.Minor;
		}
	}
}

