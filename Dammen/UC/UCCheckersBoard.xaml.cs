using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FunctionalLayer;
using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace Dammen.UC
{
	/// <summary>
	/// Interaction logic for UCCheckersBoard.xaml
	/// </summary>
	public partial class UCCheckersBoard : UserControl, INotifyPropertyChanged
	{
		private IGame _game;

		public static readonly DependencyProperty BoardProperty =
			DependencyProperty.Register(
			"Board", typeof(IBoard),
			typeof(UCCheckersBoard)
		);

		public IBoard Board { get => (IBoard)GetValue(BoardProperty); set => SetValue(BoardProperty, value); }

		public IGame Game {
			get {
				if(this._game != null)
					return this._game;
				this._game = App.container.Resolve<IGame, Game>();
				NotifyPropertyChanged();
				return this._game;
			}
		}

		/// <summary>
		/// This methods gets all instances of the UITiles shown on the board.
		/// </summary>
		public IEnumerable<UCTile> UITiles {
			get {
				List<UCTile> tiles = new List<UCTile>();
				for(int i = 0; i < this.board.Items.Count; i++) {
					ContentPresenter c = (ContentPresenter)this.board.ItemContainerGenerator.ContainerFromItem(this.board.Items[i]);
					var tile = c.ContentTemplate.FindName("tile", c) as UCTile;
					tiles.Add(tile);
				}
				return tiles;
			}
		}

		#region constructors

		public UCCheckersBoard()
		{
			InitializeComponent();
			this.Game.GameStarted += Game_GameStarted;
			this.Game.MoveOfTurnDone += Game_MoveOfTurnDone;
		}

		#endregion constructors

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



		/// <summary>
		/// Gets the possible turn for a selected checker
		/// </summary>
		public Turn PossibleTurnForSelectedChecker {
			get {
				if(this.PossibleTurnsForEachChecker == null)
					return null;
				var turnPair = this.PossibleTurnsForEachChecker.FirstOrDefault(t => t.Key == this.SelectedChecker);
				if(turnPair.Equals(default(KeyValuePair<IChecker, Turn>)))
					return null;
				return turnPair.Value;
			}
		}

		private Dictionary<IChecker, Turn> _possibleTurnsForEachChecker;
		/// <summary>
		/// Stores all the possible moves for all the checkers of the current player.
		/// TODO: code maken zodat dit niet elke turn herberekend hoeft te worden, in principe hoeven alleen de tiles eromheen geupdatet te worden.
		/// De data van alle checkers weten is nodig om alleen de moves met de meeste prio mogelijk te maken
		/// </summary>
		public Dictionary<IChecker, Turn> PossibleTurnsForEachChecker { get => this._possibleTurnsForEachChecker; set { this._possibleTurnsForEachChecker = value; NotifyPropertyChanged(); } }

		/// <summary>
		/// The checker currently selected for highlighting
		/// </summary>
		public IChecker SelectedChecker { get; private set; }


		private void Game_MoveOfTurnDone(Turn turn)
		{
			//When the first move is done, we need to remove other moves from the posibilities, since the user made the choise to take the turn with this checker
			if(this.PossibleTurnsForEachChecker != null) {
				this.PossibleTurnsForEachChecker = this.PossibleTurnsForEachChecker.Where(t => t.Value == turn).ToDictionary(d => d.Key, d => d.Value);
			}
		}

		private void Game_GameStarted(object sender, EventArgs e)
		{
			this.Game.GameFinished += Game_GameFinished;
			this.Game.TurnStarted += Game_TurnStarted;
		}

		private void Game_GameFinished(IPlayer winningPlayer)
		{
			this.Game.TurnStarted -= Game_TurnStarted;
			this.PossibleTurnsForEachChecker = null;
			UpdateHighlighting();
		}

		/// <summary>
		/// Eventhandler for when the turn is finished. this will update the possibleturnsForEachChecker list, and update highlighting of tiles.
		/// </summary>
		private void Game_TurnStarted(object sender, EventArgs e)
		{
			this.PossibleTurnsForEachChecker = null;

			if(this.Game.CurrentPlayer.PlayerType == PlayerType.Human)
				this.PossibleTurnsForEachChecker = this.Game.CurrentPlayer.GetAllPossibleMoves(this.Game, this.Game.EnemyPlayer, this.Game.Board.Tiles);
			UpdateHighlighting();
		}
		/// <summary>
		/// This method is needed in adition to mouseenter event, since sometimes the mouseenter doesnt register before dragdrop starts. this is a rare case however
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tile_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UCTile clickedTile = (UCTile)sender;
			UpdateSelectedChecker(clickedTile);
		}
		private void tile_MouseEnter(object sender, MouseEventArgs e)
		{
			UCTile hoveredTile = (UCTile)sender;
			UpdateSelectedChecker(hoveredTile);
			//in case the checker has no available moves
			if(this.PossibleTurnForSelectedChecker == null)
				return;

			HighlightPossibleMovesForSelectedChecker();
		}

		/// <summary>
		/// After the mouse leaves the tile again, clear highlighting when the user hasnt started a drag drop operation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tile_MouseLeave(object sender, MouseEventArgs e)
		{
			if(!this._dragDropInProgress) {
				ClearSelectedTiles();
			}
		}
		#region drag & drop

		private bool _dragDropInProgress = false;

		private void tile_DragDropFinished(object sender, RoutedEventArgs e)
		{
			this._dragDropInProgress = false;
			ClearSelectedTiles();
		}

		private void tile_DragDropStarted(object sender, RoutedEventArgs e)
		{
			this._dragDropInProgress = true;
		}

		/// <summary>
		/// Event that happens when a user drops an object using drag&drop on a tile. if the object is a checker, try to make a move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tile_Drop(object sender, DragEventArgs e)
		{
			var tile = (UCTile)sender;
			var draggedChecker = ((UCChecker)e.Data.GetData(typeof(UCChecker)))?.Checker;
			if(draggedChecker == null)//its not a checker so ignore it
				return;
			if(draggedChecker.Owner != this.Game.CurrentPlayer.PlayerNumber)
				return;

			var move = this.PossibleTurnForSelectedChecker?.Moves?.FirstOrDefault(m => m.EndLocation == tile.Tile.Coordinate);
			if(move == null) //dragged to invalid tile
				return;
			//perform move now!
			bool turnFinished = this.Game.TakeTurn(this.PossibleTurnForSelectedChecker, move);
			if(!turnFinished) {
				UpdateHighlighting();
			}
		}

		#endregion drag & drop

		#region methods
		/// <summary>
		/// Updates the highlighting of the tiles which have at least one move available
		/// </summary>
		public void UpdateHighlighting()
		{
			IEnumerable<UCTile> tilesOfTheCheckers = null;
			if(this.PossibleTurnsForEachChecker != null)
				tilesOfTheCheckers = this.UITiles.Where(t =>
					this.PossibleTurnsForEachChecker.Select(tp => tp.Key).Contains(t.Tile.Checker));

			HighlightTilesWithPossibleMoves(tilesOfTheCheckers);
		}
		private void HighlightTilesWithPossibleMoves(IEnumerable<UCTile> tilesToHighlight)
		{
			var currentlyHighlightedTiles = this.UITiles.Where(t => t.TileStatus == TileStatus.PossibleMove);
			foreach(var tile in currentlyHighlightedTiles) {
				tile.TileStatus = TileStatus.Normal;
			}
			if(tilesToHighlight != null) {
				foreach(var tile in tilesToHighlight) {
					tile.TileStatus = TileStatus.PossibleMove;
				}
			}
		}

		/// <summary>
		/// Resets the highlighting of all tiles that contain any of the given flags to Tilestatus.Normal
		/// </summary>
		/// <param name="statusFlagsToClear"></param>
		public void ClearTileHighlighting(TileStatus statusFlagsToClear)
		{
			var highlightedTiles = this.UITiles.Where(t => (t.TileStatus & statusFlagsToClear) > 0);
			foreach(var tile in highlightedTiles) {
				tile.TileStatus = TileStatus.Normal;
			}
		}

		/// <summary>
		/// Resets the highlighting of all tiles that contain any of the given flags to Tilestatus.Normal
		/// </summary>
		/// <param name="statusFlagsToClear"></param>
		public void ClearHistoryTileHighlighting(TileStatus statusFlagsToClear)
		{
			var highlightedTiles = this.UITiles.Where(t => (t.PreviewTileStatus & statusFlagsToClear) > 0);
			foreach(var tile in highlightedTiles) {
				tile.PreviewTileStatus = TileStatus.Normal;
			}
		}

		/// <summary>
		/// Clears the attack and move highlight of tiles
		/// </summary>
		public void ClearSelectedTiles()
		{
			ClearTileHighlighting(TileStatus.MoveHighlightingUsedTiles);
			this.SelectedChecker = null;
		}

		/// <summary>
		/// Clears the highlighting applied by the game history
		/// </summary>
		public void ClearHighlightingFromTurnHistory() => ClearHistoryTileHighlighting(TileStatus.TileHistoryUsedTiles);

		/// <summary>
		///  This method will highlight all tiles in the
		/// </summary>
		/// <param name="doneTurn"></param>
		/// <param name="selectedMove"></param>
		public void HighlightPossibleMovesForTurnInHistory(DoneTurn doneTurn)
		{
			foreach(var move in doneTurn.DoneMoves) {
				var startTile = this.UITiles.First(uit => move.StartLocation == uit.Tile.Coordinate);
				var endTile = this.UITiles.First(uit => move.EndLocation == uit.Tile.Coordinate);

				if(startTile.PreviewTileStatus == TileStatus.Normal)
					startTile.PreviewTileStatus = TileStatus.MoveStartLocation;

				if(move.MovementType == MovementType.Move)
					endTile.PreviewTileStatus = TileStatus.Move;
				else
					endTile.PreviewTileStatus = TileStatus.Attackable;

				if(move is AttackMove attmove) {
					var enemyTile = this.UITiles.First(uit => attmove.TargetCheckerCoordinate == uit.Tile.Coordinate);

					enemyTile.PreviewTileStatus = TileStatus.AttackedTile;
				}
			}
		}

		/// <summary>
		/// highlights the moves for the currently selected checker
		/// </summary>
		private void HighlightPossibleMovesForSelectedChecker()
		{
			var attackedTiles = this.UITiles.Where(uit => this.PossibleTurnForSelectedChecker.Moves.Select(m => m.EndLocation).Contains(uit.Tile.Coordinate));
			foreach(var tile in attackedTiles) {
				var move = this.PossibleTurnForSelectedChecker.Moves.Where(m => m.EndLocation == tile.Tile.Coordinate).First();
				if(move is WalkMove)
					tile.TileStatus = TileStatus.Move;
				else if(move is AttackMove attMove) {
					tile.TileStatus = TileStatus.Attackable;

					var targetCheckerCoordinate = this.Game.Board.Tiles.GetOccupiedTileByChecker(attMove.TargetChecker).Coordinate;
					var enemyTile = this.UITiles.First(uit => targetCheckerCoordinate == uit.Tile.Coordinate);
					enemyTile.TileStatus = TileStatus.AttackedTile;
				}
			}
		}

		/// <summary>
		/// Updates the checker which is currently selected
		/// </summary>
		/// <param name="tile"></param>
		private void UpdateSelectedChecker(UCTile tile)
		{
			var selectedChecker = tile?.Tile?.Checker;
			if(selectedChecker == null)
				return;

			if(!this.Game.CurrentPlayer.GetPlayerOwnedCheckers(this.Board.Tiles).Contains(selectedChecker))
				return;
			this.SelectedChecker = selectedChecker;
		}
		#endregion
	}
}