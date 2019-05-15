using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FunctionalLayer.Security;
using SacaDev.Diagnostics;

namespace Dammen.Pages.MultiPlayer
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
		private bool IsLoggingIn { get; set; } = false;
		//private OAuthUserData Userdata => App.container.Get<OAuthUserData>();
		private ILogger Logger => App.container.Get<ILogger>();
		private Authorization Auth = App.container.Resolve<Authorization>();

		private const string _googleClientID = "827572789438-tjea1i12q6f677k1me77taf1cbvs773o.apps.googleusercontent.com";
		public LoginPage()
        {
            InitializeComponent();
        }

		public event EventHandler LoginSuccesful;

		private async void Login(OAuthUserData userData, string providerID)
		{
			var auth = this.Auth;
			try {
				if(!auth.TryGetAccountID(userData.ExternID, providerID, out int userID)) {
					userID = await auth.Register(userData.ExternID, providerID);
				}
				await auth.Login(userData.ExternID, providerID);
				LoginSuccesful?.Invoke(this, new EventArgs());
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private async void btnGoogleLogin_Click(object sender, RoutedEventArgs e)
		{
			if(IsLoggingIn)
				return;
			IsLoggingIn = true;
			var gg = App.container.Get<OAuth>();
			try {
				Application.Current.MainWindow.Activate();
				Logger.Write("Start performing google authentication");
				var userdata = await gg.PerformAuthentication();
				Application.Current.MainWindow.Activate();

				Logger.Write("Finished performing google authentication");
				Logger.Write("Start performing Login");

				Login(userdata, gg.OAuthConfig.ClientID);

				Logger.Write("Finished performing Login");
				Application.Current.MainWindow.Activate();
			}
			catch(AuthenticationException ex) {
				Logger.Write($"google authentication failed with the following exception: \"{ex.Message}\"", LogType.ERROR);
				MessageBox.Show(ex.Message);
				IsLoggingIn = false;
				return;
			}
			finally {
			}
		}
	}
}
