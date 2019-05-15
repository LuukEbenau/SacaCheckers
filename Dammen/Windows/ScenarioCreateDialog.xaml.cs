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
using System.Windows.Shapes;
using FunctionalLayer.Scenarios;

namespace Dammen.Windows
{
	/// <summary>
	/// Interaction logic for ScenarioCreateDialog.xaml
	/// </summary>
	public partial class ScenarioCreateDialog : Window
	{
		public string ScenarioName { get; private set; }
		public ScenarioCreateDialog()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string name = TbScenarioName.Text;
			if(name.Length < 2) {
				MessageBox.Show("The scenarioname length is to short");
				return;
			}
			var manager = new ScenarioManager(Constants.SCENARIOSTOREPATH);
			if(manager.ScenarioExists(name)) {
				var result = MessageBox.Show($"There already exists a scenario with the name \"{name}\", are you sure you want to override this scenario?", "Scenario already exists", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
				if(result != MessageBoxResult.Yes)
					return;
			}
			ScenarioName = name;
			DialogResult = true;
			Close();
		}
	}
}
