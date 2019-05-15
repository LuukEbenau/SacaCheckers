using System;

namespace Dammen
{
	public static class Constants
	{
		public const string APPLICATIONNAME = "SacaCheckers-" + BUILDTYPE;
		public const string WINDOWSAPPNAME  = APPLICATIONNAME + "-WINDOWS";
		public const string WINDOWSLOGNAME  = APPLICATIONNAME + "-LOG";
		public readonly static string APPLICATIONDATAPATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + APPLICATIONNAME + "\\";
		public const string BUILDTYPE = "DEBUG";

		public readonly static string SCENARIOSTOREPATH = APPLICATIONDATAPATH + "Scenarios\\";
		public readonly static string LOGFILEPATH = APPLICATIONDATAPATH + "Logs\\log.log";
		public readonly static string AUTHDATAFILEPATH = APPLICATIONDATAPATH + "Auth\\auth.dat";

		public readonly static string CHECKERSAPIPATH = "http://google.com/";//TODO: path to api
	}
}