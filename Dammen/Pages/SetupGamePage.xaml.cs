using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using FunctionalLayer;
using FunctionalLayer.AI;

namespace Dammen.Pages
{
	/// <summary>
	/// Interaction logic for SetupGamePage.xaml
	/// </summary>
	public partial class SetupGamePage : Page, INotifyPropertyChanged
	{
		private Dictionary<AIType, string> _aiTypes;

		private Dictionary<AIType, string> AiTypes => this._aiTypes = this._aiTypes ?? new Dictionary<AIType, string> {
            { AIType.RandomEasy, "randomeasy"},
			{ AIType.MiniMax, "mini en maxi"},
			{ AIType.AlphaBeta, "α => β"},
			{ AIType.MinimaxMultiThreaded, "Minimax Multi"}
		};

		private Dictionary<GameType, string> _gameTypes;

		private Dictionary<GameType, string> GameTypes => this._gameTypes = this._gameTypes ?? new Dictionary<GameType, string> {
			{ GameType.Normal, "Normal"},
			{ GameType.Scenario, "Scenario"}
		};

		private Dictionary<BoardType, string> _boardTypes;

		private Dictionary<BoardType, string> BoardTypes => this._boardTypes = this._boardTypes ?? new Dictionary<BoardType, string> {
			{ BoardType.Eight, "8x8"},
			{ BoardType.Ten, "10x10"},
			{ BoardType.Twelve, "12x12"},
			{ BoardType.Custom, "Custom"}
		};

		public SetupGamePage()
		{
			InitializeComponent();
			this.cbP1AIType.ItemsSource = this.AiTypes;
			this.cbP1AIType.SelectedIndex = 0;

			this.cbP2AIType.ItemsSource = this.AiTypes;
			this.cbP2AIType.SelectedIndex = 0;

			this.cbGameType.ItemsSource = _gameTypes;
			this.cbGameType.SelectedIndex = 0;

			this.cbBoardType.ItemsSource = this.BoardTypes;
			this.cbBoardType.SelectedIndex = 1;
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			int interval = 0;
			if(this.cbTimerEnabled.IsChecked.HasValue && this.cbTimerEnabled.IsChecked.Value) {
				if(!int.TryParse(this.tbTimerInterval.Text, out interval)) {
					MessageBox.Show("De ingevulde timer interval is ongeldig");
					return;
				}
				if(interval <= 2) {
					MessageBox.Show("De interval van de timer moet minimaal 3 seconden zijn");
					return;
				}
				//game.InitializeTimer(interval);
			}

			var settings = new Settings(interval);
			var game = new Game(settings);

			string p1Name = this.tbP1Name.Text;
			string p2Name = this.tbP2Name.Text;
			if(p1Name.Length < 2) {
				MessageBox.Show("speler 1: kies een naam van minstens 2 tekens");
				return;
			}
			if(p2Name.Length < 2) {
				MessageBox.Show("speler 2: kies een naam van minstens 2 tekens");
				return;
			}
			IPlayer p1, p2;
			PlayerColor p1Color, p2Color;
			if(new Random().Next(0, 2) == 0) {
				p1Color = PlayerColor.Light;
				p2Color = PlayerColor.Dark;
			}
			else {
				p1Color = PlayerColor.Dark;
				p2Color = PlayerColor.Light;
			}

			if(this.rbP1IsAi.IsChecked.HasValue && this.rbP1IsAi.IsChecked.Value) {
				var p1AiType = (AIType)this.cbP1AIType.SelectedValue;
				p1 = new AIPlayer(PlayerNumber.One, p1Name, p1Color, p1AiType);
			}
			else {
				p1 = new Player(PlayerNumber.One, p1Name, p1Color);
			}

			if(this.rbP2IsAi.IsChecked.HasValue && this.rbP2IsAi.IsChecked.Value) {
				var p2AiType = (AIType)this.cbP2AIType.SelectedValue;
				p2 = new AIPlayer(PlayerNumber.Two, p2Name, p2Color, p2AiType);
			}
			else {
				p2 = new Player(PlayerNumber.Two, p2Name, p2Color);
			}

			game.Player1 = p1;
			game.Player2 = p2;

			Start?.Invoke(this,new GameEventArgs(game));
		}

		public event EventHandler<GameEventArgs> Start;

		private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var textBox = sender as TextBox;
			e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void cbGameType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (KeyValuePair<GameType, string>)this.cbGameType.SelectedItem;
			if(item.Equals(default(KeyValuePair<GameType, string>)))
				return;

			if(item.Key == GameType.Normal) {
				GameOptionsNormalGameWrap.Visibility = Visibility.Visible;
				//this.CbScenarioSelectWrap.Visibility = Visibility.Collapsed;
			}
			else {
				GameOptionsNormalGameWrap.Visibility = Visibility.Collapsed;
				//this.CbScenarioSelectWrap.Visibility = Visibility.Visible;
			}
		}

		private void cbBoardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (KeyValuePair<BoardType, string>)this.cbBoardType.SelectedItem;
			if(item.Equals(default(KeyValuePair<BoardType, string>)))
				return;

			if(item.Key == BoardType.Custom) {
				this.BoardSizeWrap.Visibility = Visibility.Visible;
			}
			else {
				this.BoardSizeWrap.Visibility = Visibility.Collapsed;
			}
		}
	}
	public class GameEventArgs : EventArgs {
		public IGame Game { get; private set; }
		public GameEventArgs(IGame game) {
			this.Game = game;
		}
	}
}