using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using FunctionalLayer.GameTurn;

namespace FunctionalLayer.CheckersBoard
{
	/// <summary>
	/// This class contains the logic of a gameboard.
	/// </summary>
	public class Board : IBoard, INotifyPropertyChanged
	{
		#region properties

		private BoardTileCollection _tiles;

		/// <summary>
		/// All tiles, x and y
		/// </summary>
		public BoardTileCollection Tiles { get => this._tiles; set { this._tiles = value; NotifyPropertyChanged(); } }

		#endregion properties

		#region constructors

		public Board()
		{
			this.Tiles = new BoardTileCollection();

		}

		public void SubscribeToGameEvents(IGame game)
		{
			game.GameStarted += Game_GameStarted;
		}

		private void Game_GameStarted(object sender, EventArgs e)
		{ 
			var game = sender as IGame;
			//InitializePlayers(game, game.Player1, game.Player2);
		}

		#endregion constructors

		#region public methods

		public object Clone()
		{
			var membClone = this.MemberwiseClone() as Board;
			membClone.Tiles = membClone.Tiles.Clone() as BoardTileCollection;

			return membClone;
		}

		/// <summary>
		/// Initialize a board to start the game
		/// </summary>
		public void InitializePlayers(IGame game,IPlayer player1, IPlayer player2)
		{
			ClearCheckers();
			//if(coordinates == null)//default setup
			//{
				foreach(ITile t in this.Tiles.Where(t => t.Coordinate.Y <= TileCoordinate.MIN_Y + 3 && t.Coordinate.X % 2 == t.Coordinate.Y % 2)) {
					t.Checker = new Checker(player1.PlayerNumber, player1.PlayerColor);
				}
				foreach(ITile t in this.Tiles.Where(t => t.Coordinate.Y >= TileCoordinate.MAX_Y - 3 && t.Coordinate.Y <= TileCoordinate.MAX_Y && t.Coordinate.X % 2 == t.Coordinate.Y % 2)) {
					t.Checker = new Checker(player2.PlayerNumber, player2.PlayerColor);
				}
			//, Dictionary<IPlayer, List<TileCoordinate>> coordinates = null
			//}
			//else {
			//	//Place the checkers on the given positio ns
			//	foreach(var pair in coordinates) {
			//		foreach(var coord in pair.Value) {
			//			var tile = this.Tiles.Single(t => t.Coordinate == coord);
			//			tile.Checker = new Checker(pair.Key.PlayerNumber, pair.Key.PlayerColor);
			//		}
			//	}
			//}
			game.Player1StartingCheckerCount = game.Player1.GetPlayerOwnedCheckers(this.Tiles).Count();
			game.Player2StartingCheckerCount = game.Player2.GetPlayerOwnedCheckers(this.Tiles).Count();
		}

		#endregion public methods

		#region private methods

		public void ClearCheckers()
		{
			int tilesSize = this.Tiles.Count - 1;
			for(int i = tilesSize; i >= 0; i--)
				this.Tiles[i].Checker = null;
		}

		/// <summary>
		/// (Re)Generate the tiles collection
		/// </summary>
		public void GenerateTiles()
		{
			//TODO: make it so that the Tiles list only contains black tiles, this is more efficient for followup code, and white tiles can always be ignored.
			int tilesSize = this.Tiles.Count - 1;
			for(int i = tilesSize; i >= 0; i--)
				this.Tiles.RemoveAt(i);

			for(int y = TileCoordinate.MAX_Y; y >= TileCoordinate.MIN_Y; y--) {
				for(int x = TileCoordinate.MIN_X; x <= TileCoordinate.MAX_X; x++) {
					ITile tile;
					if(x % 2 == y % 2) {
						tile = new Tile(TileColor.Dark, new TileCoordinate(x, y));
					}
					else {
						tile = new Tile(TileColor.Light, new TileCoordinate(x, y));
					}
					this.Tiles.Add(tile);
				}
			}
		}

		#endregion private methods

		#region static methods

		/// <summary>
		/// Executes the move
		/// </summary>
		/// <param name="move">the move the user is taking</param>
		/// <returns>Wherether or not there are more followup moves left</returns>
		public static bool TakeMove(BoardTileCollection tiles, Move move)
		{
			bool moreMovesLeft = false;
			//FIXME: When doing a rematch vs an ai and ai starts first, the second time this method can't find the checker anymore this is maybe because ai tries to calculate with an different list of tiles than the list where the move was calculated with? If this is the case i wouldn't have a clue why so.
			var startTile = tiles.First(t => t.Checker?.Id == move.Checker.Id);
			startTile.Checker = null;

			var targetTile = tiles.FirstOrDefault(t => t.Coordinate == move.EndLocation);
			targetTile.Checker = move.Checker;

			if(move is AttackMove attMove) {
				var target = attMove.TargetChecker;
				var enemyTile = tiles.First(t => t.Checker?.Id == target.Id);
				enemyTile.Checker = null;
				if(attMove.FurtherMoves.Count() > 0) {
					moreMovesLeft = true;//more moves left
				}
			}

			if(!moreMovesLeft) {
				var checker = move.Checker;
				if(checker.Type == CheckerType.Men) {
					if(checker.IsInEnemyBaseRow(tiles)) {
						checker.PromoteToKing();
					}
				}
			}

			return moreMovesLeft;
		}

		#endregion static methods

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}