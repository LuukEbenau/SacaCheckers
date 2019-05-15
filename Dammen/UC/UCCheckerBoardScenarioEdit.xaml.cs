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
	/// Interaction logic for UCCheckerBoardScenarioEdit.xaml
	/// </summary>
	public partial class UCCheckerBoardScenarioEdit : UserControl, INotifyPropertyChanged
	{
		private IGame _game;

		public static readonly DependencyProperty BoardProperty =
			DependencyProperty.Register(
			"Board", typeof(IBoard),
			typeof(UCCheckerBoardScenarioEdit)
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

		/// <summary>
		/// Resets the highlighting of all tiles that contain any of the given flags to Tilestatus.Normal
		/// </summary>
		/// <param name="statusFlagsToClear"></param>
		private void ClearHistoryTileHighlighting(TileStatus statusFlagsToClear)
		{
			var highlightedTiles = this.UITiles.Where(t => (t.PreviewTileStatus & statusFlagsToClear) > 0);
			foreach(var tile in highlightedTiles) {
				tile.PreviewTileStatus = TileStatus.Normal;
			}
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


		#region constructors

		public UCCheckerBoardScenarioEdit()
		{
			InitializeComponent();
			
		}

		#endregion constructors

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void tile_Drop(object sender, DragEventArgs e)
		{
			var tile = (UCTile)sender;
			if(tile.Tile.TileColor == TileColor.Light)
				return;

			var draggedChecker = ((Checker)e.Data.GetData(typeof(Checker)));
			tile.Tile.Checker = draggedChecker;
		}

		private void tile_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var tile = sender as UCTile;
			tile.Tile.Checker = null;
		}
	}
}
