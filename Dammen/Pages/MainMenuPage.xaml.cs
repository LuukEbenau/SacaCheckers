using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Dammen.Pages
{
	/// <summary>
	/// Interaction logic for MainMenu.xaml
	/// </summary>
	public partial class MainMenuPage : Page
	{
		public event EventHandler<MainMenuItemSelectedEventHandler> MainMenuItemSelected;

		public MainMenuPage()
		{
			InitializeComponent();
		}

		private void BtnSinglePlayer_Click(object sender, RoutedEventArgs e)
		{
			MainMenuItemSelected?.Invoke(this, new MainMenuItemSelectedEventHandler(MainMenuItemOption.SinglePlayer));
		}

		private void BtnScenarioEditor_Click(object sender, RoutedEventArgs e)
		{
			MainMenuItemSelected?.Invoke(this, new MainMenuItemSelectedEventHandler(MainMenuItemOption.ScenarioEditor));
		}

		private void BtnScenarioOverview_Click(object sender, RoutedEventArgs e)
		{
			MainMenuItemSelected?.Invoke(this, new MainMenuItemSelectedEventHandler(MainMenuItemOption.ScenarioOverview));
		}

		private void BtnMultiPlayer_Click(object sender, RoutedEventArgs e)
		{
			MainMenuItemSelected?.Invoke(this, new MainMenuItemSelectedEventHandler(MainMenuItemOption.Multiplayer));
		}
	}
	public class MainMenuItemSelectedEventHandler:EventArgs{
		public MainMenuItemOption SelectedOption { get; private set; }
		public MainMenuItemSelectedEventHandler(MainMenuItemOption selectedOption)
		{
			this.SelectedOption = selectedOption;
		}
	}
	public enum MainMenuItemOption
	{
		SinglePlayer=1,
		Multiplayer,
		ScenarioEditor,
		ScenarioOverview
	}
}
