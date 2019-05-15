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
using System.Collections.ObjectModel;
using FunctionalLayer.Scenarios;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FunctionalLayer;

namespace Dammen.Pages
{
	/// <summary>
	/// Interaction logic for ScenarioSelectionPage.xaml
	/// </summary>
	public partial class ScenarioSelectionPage : Page, INotifyPropertyChanged
	{
		private ObservableCollection<ScenarioMetaData> _scenarios;
		public ObservableCollection<ScenarioMetaData> Scenarios { get => _scenarios; set { _scenarios = value; NotifyPropertyChanged(); } }
		private ScenarioManager Manager { get; set; }
		public event EventHandler<ScenarioChosenEventArgs> ScenarioChosen;
		public ScenarioSelectionPage()
		{
			InitializeComponent();
			Manager = new ScenarioManager(Constants.SCENARIOSTOREPATH);
			var scenarios = Manager.GetStoredScenarios();
			this.Scenarios = new ObservableCollection<ScenarioMetaData>(scenarios);
			
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void Scenario_Double_Click(object sender, MouseButtonEventArgs e)
		{
			var metaData = (sender as ListViewItem).Content as ScenarioMetaData;
			var scenario = Manager.LoadScenario(metaData.Name);
			ScenarioChosen?.Invoke(this, new ScenarioChosenEventArgs(scenario));
			
		}
	}
	public class ScenarioChosenEventArgs : EventArgs {
		public IGame Scenario { get; private set; }
		public ScenarioChosenEventArgs(IGame scenario) {
			this.Scenario = scenario;
		}
	}
}
