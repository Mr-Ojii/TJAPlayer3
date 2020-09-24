using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

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

			int _major, _minor, _build;
			tpGetOSVersion(out _major, out _minor, out _build);

			//if (os.Version.Major >= major && os.Version.Minor >= minor)
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


		//public static (int major, int minor, int build) tpGetOSVersion()
		public static void tpGetOSVersion(out int major, out int minor, out int build)
		{
			
			major = 0;
			minor = 0;
			build = 0;

			ManagementClass mc =
				new ManagementClass("Win32_OperatingSystem");
			ManagementObjectCollection moc = mc.GetInstances();

			foreach (ManagementObject mo in moc)
			{
				string ver = mo["Version"].ToString();
				string[] majorminor = ver.Split(new char[] { '.' }, StringSplitOptions.None);

				major = Convert.ToInt32(majorminor[0]);
				minor = Convert.ToInt32(majorminor[1]);
				build = Convert.ToInt32(mo["BuildNumber"]);

				break;  // 1回ループで終了(でいいよね)
			}
			moc.Dispose();
			mc.Dispose();

			//return result;
		}
	}
}

