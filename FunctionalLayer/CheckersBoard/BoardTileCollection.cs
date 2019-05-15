using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using FunctionalLayer.GameTurn;

namespace FunctionalLayer.CheckersBoard
{
	public class BoardTileCollection : ObservableCollection<ITile>, ICloneable
	{
		public BoardTileCollection()
		{
		}

		public BoardTileCollection(IEnumerable<ITile> tiles) : base(tiles)
		{
		}

		public object Clone()
		{
			return new BoardTileCollection(this.Select(tt => (ITile)tt.Clone()));
		}

		public IEnumerable<ITile> GetOccupiedCoords()
		{
			return this.Where(t => t.Checker != null);
		}

		/// <summary>
		/// Returns an enumeration containing the checkers of both players.
		/// </summary>
		public static List<IChecker> GetAllCheckers(BoardTileCollection tiles)
		{
			return tiles.Where(t => t.Checker != null).Select(t => t.Checker).ToList();
		}

		/// <summary>
		/// Returns an enumeration containing the checkers of both players.
		/// </summary>
		public List<IChecker> GetAllCheckers() => GetAllCheckers(this);


		public IEnumerable<AttackMove> GetMovesForLocation(IGame game, IChecker checker, TileCoordinate startLocation, IPlayer currentPlayer, IPlayer enemyPlayer, List<TileCoordinate> doneMoves)
			=> GetMovesForLocation(game, checker, startLocation, currentPlayer, enemyPlayer, this.First(t => t.Checker == checker).Coordinate, doneMoves);

		/// <summary>
		/// This method generates all possible moves for a given location. It will call itself recursively until the endlocation has no further moves left.
		/// </summary>
		/// <param name="checker"></param>
		/// <param name="startLocation"></param>
		/// <param name="originalStartLocation">This parameter is needed because otherwise the game thinks this tile is occupied, when in reality it isnt. defaults to checker.Coordinate</param>
		/// <param name="doneTiles"></param>
		/// <returns></returns>
		public IEnumerable<AttackMove> GetMovesForLocation(IGame game, IChecker checker, TileCoordinate startLocation, IPlayer currentPlayer, IPlayer enemyPlayer,
			TileCoordinate? originalStartLocation, List<TileCoordinate> doneMoves = null)
		{
			List<AttackMove> moves = new List<AttackMove>();
			//get targets for attackmoves
			var potentionalMoves = checker.GetPotentionalMoveCoordinates(game, this, enemyPlayer: enemyPlayer, startingCoordinate: startLocation,
				movementType: MovementType.Attack);
			var attmoves = potentionalMoves[MovementType.Attack].Where(m => GetPlayerOwnedTiles(enemyPlayer.PlayerNumber).Select(t => t.Coordinate).Contains(m)).Except(doneMoves);

			foreach(var attmove in attmoves) {
				var direction = TileCoordinate.GetStepDirection(startLocation, attmove);
				var targetCoordinate = attmove;
				var locationsAfterJunp = GetPossibleMovesInDirectionAfterAttack(checker, targetCoordinate, direction);
				locationsAfterJunp = locationsAfterJunp.Where(l => l.IsValid());

				//for each possible location after the jump, this contains all moves afterattackmove
				foreach(var locationAfterJunp in locationsAfterJunp) {
					var tile = this.FirstOrDefault(t => t.Coordinate == locationAfterJunp);
					bool locationAfterJunpFree = (tile.Coordinate == originalStartLocation //original location is always free, since the attacking checker started there
						|| tile.Checker == null);
					if(!locationAfterJunpFree) //if theres a checker on the final tile, go onto next potentional move
						continue;

					var targetChecker = this.FirstOrDefault(t => t.Coordinate == targetCoordinate).Checker;
					doneMoves.Add(targetCoordinate);//add to donetiles, so the game knows it cant jump on it again this turn

					IEnumerable<AttackMove> movesAfterJump = GetMovesForLocation(game, checker: checker, startLocation: locationAfterJunp,
						currentPlayer: currentPlayer, enemyPlayer: enemyPlayer,
						originalStartLocation: originalStartLocation, doneMoves: doneMoves);
					movesAfterJump = movesAfterJump.GetOnlyHighestPriorityMoves();

					AttackMove move = new AttackMove(checker, targetChecker, startLocation, locationAfterJunp, targetCoordinate, movesAfterJump.ToList());
					moves.Add(move);
				}
			}
			return moves;
		}

		public ITile GetOccupiedTileByChecker(IChecker checker) => this.FirstOrDefault(t => t.Checker == checker);

		public IEnumerable<ITile> GetPlayerOwnedTiles(PlayerNumber player)
		{
			return this.Where(t => t.Checker?.Owner == player);
		}

		/// <summary>
		/// calculates all possible locations to end after doing an attackmove. this is always 1 tile after for a normal checker, but for a king it checks for a blocking checker in the row and returns all coordinates 'til that point.
		/// </summary>
		/// <param name="checker">The checker which makes the move</param>
		/// <param name="enemyCheckerCoordinate">Coordinate of the enemy checker</param>
		/// <param name="direction">Direction the attackmove was heading</param>
		/// <returns>list of possible ending locations</returns>
		private IEnumerable<TileCoordinate> GetPossibleMovesInDirectionAfterAttack(IChecker checker, TileCoordinate enemyCheckerCoordinate, DiagonalDirection direction)
		{
			var moves = new List<TileCoordinate>();
			if(checker.Type == CheckerType.King) {
				TileCoordinate lastCheckedTile = new TileCoordinate(enemyCheckerCoordinate.X, enemyCheckerCoordinate.Y);
				TileCoordinate nextCoordinate = TileCoordinate.CalculateLocationAfterSteps(lastCheckedTile, direction);
				while(nextCoordinate.IsValid()) //continues as long as the code hasnt reached a corner of the board, of until a attackmove is detected
				{
					var nextTile = this.First(t => t.Coordinate == nextCoordinate);
					if(nextTile.Checker != null)//theres a checker here, so check for a possibility of a attackmove.
					{
						break;//stop checking moves in this direction, since we found a checker. no need to check the further moves now.
					}
					else {
						moves.Add(nextCoordinate);
					}
					//update the nextcoordinate, which is needed for the next direction
					nextCoordinate = TileCoordinate.CalculateLocationAfterSteps(nextCoordinate, direction);
				}
			}
			else {
				moves.Add(TileCoordinate.CalculateLocationAfterSteps(enemyCheckerCoordinate, direction));
			}
			return moves;
		}
	}
}