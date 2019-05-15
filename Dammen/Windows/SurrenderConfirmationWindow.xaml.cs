using System.Windows;

using FunctionalLayer;

namespace Dammen.Windows
{
	/// <summary>
	/// Interaction logic for SurrenderConfirmationWindow.xaml
	/// </summary>
	public partial class SurrenderConfirmationWindow : Window
	{
		public IPlayer SurrenderingPlayer { get; private set; }
		public IPlayer EnemyPlayer { get; private set; }

		public SurrenderConfirmationWindow(IPlayer surrenderingPlayer, IPlayer enemyPlayer)
		{
			this.SurrenderingPlayer = surrenderingPlayer;
			this.EnemyPlayer = enemyPlayer;
			InitializeComponent();
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			Close();
		}
	}
}