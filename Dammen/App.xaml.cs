using System.Diagnostics;
using System.Windows;
using SacaDev.Diagnostics;
using FunctionalLayer.Security;

namespace Dammen
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static DependencyContainer container = new DependencyContainer();

		public App()
		{
			using(Process p = Process.GetCurrentProcess())
				p.PriorityClass = ProcessPriorityClass.High;

			///Initialize Services
			container.Singleton<ILogger,Logger>(new Logger(Constants.LOGFILEPATH));
			container.Singleton(new AuthorizationFile(Constants.AUTHDATAFILEPATH));


			var googleConfig = new OAuthConfig(
				"827572789438-tjea1i12q6f677k1me77taf1cbvs773o.apps.googleusercontent.com",
				"83URAEI-YU2SkBzz0pS-BunH",
				"https://accounts.google.com/o/oauth2/v2/auth",
				"https://www.googleapis.com/oauth2/v4/token",
				"https://www.googleapis.com/oauth2/v3/userinfo"
			);

			container.Singleton(new OAuth(googleConfig));

			container.Singleton(new Authorization(Constants.CHECKERSAPIPATH, googleConfig));
		}
	}
}