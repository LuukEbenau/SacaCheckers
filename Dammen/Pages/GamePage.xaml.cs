using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Dammen.Windows;

using FunctionalLayer;

namespace Dammen.Pages
{
	/// <summary>
	/// Interaction logic for GamePage.xaml
	/// </summary>
	public partial class GamePage : Page, INotifyPropertyChanged
	{
		#region properties

		private IGame _game;

		public IGame Game {
			get {
				return App.container.Resolve<IGame, Game>();
			}
		}

		#endregion properties

		#region constructors

		public GamePage()
		{
			InitializeComponent();
			//is needed since the game needs to start once page is opened, not when page is created
			Loaded += GamePage_Loaded;
			this.Game.GameFinished += Game_GameFinished;
			this.Game.TurnStarted += Game_TurnFinished;
			this.Game.PostGameStarted += Game_GameStarted;
		}

		private void Game_TurnFinished(object sender, EventArgs e)
		{
		}

		#endregion constructors

		#region events and handlers
		private void UCGameHistory_HistoryItemHoverEnter(object sender, UC.GameHistoryTurnHoverEventArgs e)
		{
			if(e.DoneTurn != null)
				this.board.HighlightPossibleMovesForTurnInHistory(e.DoneTurn);
		}

		private void UCGameHistory_HistoryItemHoverLeave(object sender, System.EventArgs e)
		{
			this.board.ClearHighlightingFromTurnHistory();
		}

		private void GamePage_Loaded(object sender, RoutedEventArgs e)
		{
			GotFocus -= GamePage_Loaded;

			this.Game.StartGame();
		}

		private void Game_GameStarted(object sender, EventArgs e)
		{
			Resume();
		}

		private void Game_GameFinished(IPlayer winningPlayer)
		{
			Pause();
		}

		private void btnGiveUp_Click(object sender, RoutedEventArgs e)
		{
			var window = new SurrenderConfirmationWindow(this.Game.CurrentPlayer, this.Game.EnemyPlayer);
			window.Owner = Application.Current.MainWindow;
			var result = window.ShowDialog();
			if(result.HasValue && result.Value) {
				this.Game.Surrender(window.SurrenderingPlayer);
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion events and handlers

		private void Pause()
		{
			this.Game.Pause();
			this.board.IsHitTestVisible = false;
		}

		private void Resume()
		{
			this.Game.Resume();
			this.board.IsHitTestVisible = true;
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			if(this.Game.Paused) {
				Resume();
			}
			else {
				Pause();
			}
		}
	}
}