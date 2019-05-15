using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using FunctionalLayer;
using Dammen.UC;
using Newtonsoft.Json;
using FunctionalLayer.Scenarios;
using Dammen.Windows;
using FunctionalLayer.Util;

namespace Dammen.Pages
{
    /// <summary>
    /// Interaction logic for ScenarioEditorPage.xaml
    /// </summary>
    public partial class ScenarioEditorPage : Page, INotifyPropertyChanged
	{
		private IGame _game;
		public IGame Game { get => _game; private set { _game = value; NotifyPropertyChanged(); } }

		private IChecker _player1NormalChecker;
		private IChecker Player1NormalChecker { get => _player1NormalChecker; set { _player1NormalChecker = value; NotifyPropertyChanged(); } }

		private IChecker _player1KingChecker;
		private IChecker Player1KingChecker { get => _player1KingChecker; set { _player1KingChecker = value; NotifyPropertyChanged(); } }

		private IChecker _player2NormalChecker;
		private IChecker Player2NormalChecker { get => _player2NormalChecker; set { _player2NormalChecker = value; NotifyPropertyChanged(); } }

		private IChecker _player2KingChecker;
		private IChecker Player2KingChecker { get => _player2KingChecker; set { _player2KingChecker = value; NotifyPropertyChanged(); } }

		private string SavedScenarioName { get; set; }

		public ScenarioEditorPage(IGame game)
        {
			InitializeComponent();
			this.Game = game;
			this.Player1NormalChecker = new Checker(Game.Player1.PlayerNumber, Game.Player1.PlayerColor, CheckerType.Men);
			this.Player1KingChecker = new Checker(Game.Player1.PlayerNumber,   Game.Player1.PlayerColor, CheckerType.King);
			this.Player2NormalChecker = new Checker(Game.Player2.PlayerNumber, Game.Player2.PlayerColor, CheckerType.Men);
			this.Player2KingChecker = new Checker(Game.Player2.PlayerNumber,   Game.Player2.PlayerColor, CheckerType.King);

			UCCP1NormalChecker.Checker = Player1NormalChecker;
			UCCP1KingChecker.Checker = Player1KingChecker;

			UCCP2NormalChecker.Checker = Player2NormalChecker;
			UCCP2KingChecker.Checker = Player2KingChecker;
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void Checker_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if(sender is UCChecker ucChecker) {
				var c = ucChecker.Checker.Copy();
				DragDrop.DoDragDrop(ucChecker, c, DragDropEffects.Move);
			}
		}

		private void BtnSaveScenario_Click(object sender, RoutedEventArgs e)
		{
			if(SavedScenarioName == null) {
				var scenarioNameDialog = new ScenarioCreateDialog();
				scenarioNameDialog.Owner = Application.Current.MainWindow;
				scenarioNameDialog.ShowDialog();
				var result = scenarioNameDialog.DialogResult;
				if(result.HasValue && result.Value) {
					SavedScenarioName = scenarioNameDialog.ScenarioName;
				}
				else
					return;
			}

			var manager = new ScenarioManager(Constants.SCENARIOSTOREPATH);

			bool succes = manager.SaveScenario(SavedScenarioName, Game, out string savedPath);

		}

		private void BtnTogglePlayerColors_Click(object sender, RoutedEventArgs e)
		{
			var p1 = Game.Player1;
			var p2 = Game.Player2;
			if(p1.PlayerColor == PlayerColor.Dark) {
				p1.PlayerColor = PlayerColor.Light;
				p2.PlayerColor = PlayerColor.Dark;
			}
			else {
				p2.PlayerColor = PlayerColor.Light;
				p1.PlayerColor = PlayerColor.Dark;
			}
			foreach(var checker in Game.Board.Tiles.Select(t => t.Checker).Where(t => t != null)) {
				if(checker.Color == PlayerColor.Dark)
					checker.Color = PlayerColor.Light;
				else
					checker.Color = PlayerColor.Dark;
			}
		}
	}
}
