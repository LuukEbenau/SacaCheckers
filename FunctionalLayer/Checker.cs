using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;
using Newtonsoft.Json;

namespace FunctionalLayer
{
	public class Checker : INotifyPropertyChanged, IChecker
	{
		#region properties

		/// <summary>
		/// The player which owns the checker
		/// </summary>
		public PlayerNumber Owner { get; }
		private PlayerColor _color;
		public PlayerColor Color { get => _color;
			set { _color = value;  NotifyPropertyChanged(); NotifyPropertyChanged(nameof(CheckerImage)); } }//TODO: remove this once finished tesring. no notify needed

		private CheckerType _type;

		public CheckerType Type {
			get => this._type;
			private set {
				this._type = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(CheckerImage));//needed to update the image of the checker in the ui when it transforms in a king
			}
		}

		/// <summary>
		/// Creates a turn for a checker.
		/// </summary>
		/// <param name="checker"></param>
		/// <returns></returns>
		public Turn CreateTurn(IGame game, BoardTileCollection tiles, IPlayer currentPlayer, IPlayer enemyPlayer)
		{
			var checkersCurrentTile = tiles.First(t => t.Checker == this);

			var occupiedCoords = tiles.Where(t => t.Checker != null);
			var possibleMoves = GetPotentionalMoveCoordinates(game, tiles, enemyPlayer: enemyPlayer, startingCoordinate: checkersCurrentTile.Coordinate, movementType: MovementType.Both);
			var walkMoves = possibleMoves[MovementType.Move].Select(m => new WalkMove(this, checkersCurrentTile.Coordinate, m)).ToList();

			List<Move> attackMoves = new List<Move>();
			if(possibleMoves[MovementType.Attack].Any()) {
				attackMoves = tiles.GetMovesForLocation(game, this,  checkersCurrentTile.Coordinate,
					currentPlayer, enemyPlayer, walkMoves.Select(mm => mm.EndLocation).ToList())
					.Select(m => (Move)m).ToList();
			}
			var totalMoves = new List<Move>();
			totalMoves.AddRange(walkMoves);
			totalMoves.AddRange(attackMoves);
			//filter moves to show only moves which are valid based on move priority: attack over walk, and the more atts the higher prio.
			totalMoves = totalMoves.GetOnlyHighestPriorityMoves().ToList();

			return new Turn(totalMoves);
		}

		//public TileCoordinate Coordinate { get; set; }

		/// <summary>
		/// Upgrades the checker to king
		/// </summary>
		public void PromoteToKing()
		{
			this.Type = CheckerType.King;
		}

		/// <summary>
		/// returns wherether or not the checker is currently in the base row of the enemy.
		/// </summary>
		public bool IsInEnemyBaseRow(BoardTileCollection tiles)
		{
			var coordinate = tiles.GetOccupiedTileByChecker(this).Coordinate;

			int row = (this.Owner == PlayerNumber.Two) ? TileCoordinate.MIN_Y : TileCoordinate.MAX_Y;
			return (coordinate.Y == row);
		}

		/// <summary>
		/// The background image of a checker.
		/// </summary>
		[JsonIgnore]
		public Brush CheckerImage {
			get {
				Brush icon;
				
				if(Color == PlayerColor.Dark) {
					icon = (this.Type == CheckerType.Men) ?
						new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Dammen;component/img/zwarte_damsteen.png"))) :
						new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Dammen;component/img/zwarte_damsteen_king.png")));
				}
				else {
					icon = (this.Type == CheckerType.Men) ?
						new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Dammen;component/img/witte_damsteen.png"))) :
						new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Dammen;component/img/witte_damsteen_king.png")));
				}
				return icon;
			}
		}

		#endregion properties
		private static int _idIndex = 0;
		private static readonly object _idLock = new object();
		/// <summary>
		/// Threadsafely gets the next idindex for the next checker
		/// </summary>

		private static int GetNextIdIndex() { lock(_idLock) { return _idIndex++; } }

		public Checker(PlayerNumber owner, PlayerColor playerColor)
		{
			this.Id = GetNextIdIndex();
			this.Color = playerColor;
			this.Owner = owner;
		}
		/// <summary>
		/// For use in scenarioediter
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="game"></param>
		/// <param name="type"></param>
		[JsonConstructor]
		public Checker(PlayerNumber owner, PlayerColor color, CheckerType type)
		{
			this.Id = GetNextIdIndex();
			this.Type = type;
			this.Color = color;
			this.Owner = owner;
		}

		public int Id { get; private set; }

		public object Clone() {
			var c = this.MemberwiseClone() as Checker;
			return c;
		}

		/// <summary>
		/// Gets all potentional moves for a given checker. The moves still have to be checked for being valid.
		/// </summary>
		/// <param name="playerNumber">The playernumber, it's needed to determine if attacking downwards or upwards</param>
		/// <param name="coordinate">the coordinate from where the move is going to be done.</param>
		/// <param name="checker">The checker for which the moves are requested.</param>
		/// <param name="movementType">An flagbased identifier which indicates what kind of moves the method has to check for.</param>
		/// <param name="startingCoordinate">The coordinate from where the method will search for further moves. It's important to note that you should never use checker.Coordinate,
		/// except for the first move of a multimove. this is because multimoves get calculates before the previous move is executed.</param>
		/// <returns>Dictionary with as key each movement type inside <paramref name="movementType"/>. and as value a list of all the moves of that type.</returns>
		public Dictionary<MovementType, List<TileCoordinate>> GetPotentionalMoveCoordinates(IGame game, BoardTileCollection tiles, IPlayer enemyPlayer, TileCoordinate startingCoordinate, MovementType movementType)
		{
			var moves = new Dictionary<MovementType, List<TileCoordinate>>();

			if(this.Type == CheckerType.King) {
				moves = GetPossibleMovesForKing(tiles, game, startingCoordinate, movementType);
			}
			else {
				var occupiedCoords = tiles.GetOccupiedCoords().ToList().Select(t => t.Coordinate);
				var potentionalWalkMoves = TileCoordinate.GetPotentionalMovesForChecker(this.Owner, startingCoordinate);
				var movementMoveCoordinates = potentionalWalkMoves.Except(occupiedCoords);

				if(movementType.HasFlag(MovementType.Move)) {
					moves.Add(MovementType.Move, new List<TileCoordinate>());
					moves[MovementType.Move].AddRange(movementMoveCoordinates);
				}

				if(movementType.HasFlag(MovementType.Attack)) {
					moves.Add(MovementType.Attack, new List<TileCoordinate>());
					var attmoves = TileCoordinate.GetPotentionalAttackMoveCoordinatesForChecker(startingCoordinate);
					var potentionalAttackMoveTargets = attmoves.Except(movementMoveCoordinates);

					var enemyCheckerCoordinates = tiles.GetPlayerOwnedTiles(enemyPlayer.PlayerNumber).Select(t => t.Coordinate);
					potentionalAttackMoveTargets = potentionalAttackMoveTargets.Where(m => enemyCheckerCoordinates.Contains(m));
					moves[MovementType.Attack].AddRange(potentionalAttackMoveTargets);
				}
			}
			return moves;
		}

		/// <summary>
		/// Generates all possible move targets for a checker, this method takes account for attackmoves
		/// </summary>
		/// <param name="startCoordinate">location where the king starts his move</param>
		/// <returns></returns>
		private Dictionary<MovementType, List<TileCoordinate>> GetPossibleMovesForKing(BoardTileCollection tiles, IGame game, TileCoordinate startCoordinate, MovementType movementType)
		{
			//FIXME: In the case that a king in a multijump has 2 options after a multijump, it can only choose one of the two. scenarioediter will be helpful for debugging this.
#warning King sometimes incorrectly only allows the user to do 1 of the 2 attackmoves, fix this in the future.. UPDATE: this is not only if 2 options left, in some times it just doesnt register the last move, when it should have one more attmove.
			var possibleMoveCoordinates = new Dictionary<MovementType, List<TileCoordinate>>();

			if(movementType.HasFlag(MovementType.Attack))
				possibleMoveCoordinates.Add(MovementType.Attack, new List<TileCoordinate>());
			if(movementType.HasFlag(MovementType.Move))
				possibleMoveCoordinates.Add(MovementType.Move, new List<TileCoordinate>());

			var directions = new DiagonalDirection[] {
				DiagonalDirection.NE,
				DiagonalDirection.SE,
				DiagonalDirection.SW,
				DiagonalDirection.NW
			};

			//now let's check for possible moves for each of the 4 directions!
			foreach(var direction in directions) {
				var possibleMoveCoordinatesForDirection = new Dictionary<MovementType, List<TileCoordinate>>();
				if(movementType.HasFlag(MovementType.Attack))
					possibleMoveCoordinatesForDirection.Add(MovementType.Attack, new List<TileCoordinate>());
				if(movementType.HasFlag(MovementType.Move))
					possibleMoveCoordinatesForDirection.Add(MovementType.Move, new List<TileCoordinate>());

				TileCoordinate lastCheckedTile = new TileCoordinate(startCoordinate.X, startCoordinate.Y);
				TileCoordinate nextCoordinate = TileCoordinate.CalculateLocationAfterSteps(lastCheckedTile, direction);
				while(nextCoordinate.IsValid()) //continues as long as the code hasnt reached a corner of the board, of until a attackmove is detected
				{
					var nextTile = tiles.First(t => t.Coordinate == nextCoordinate);
					if(nextTile.Checker != null)//theres a checker here, so check for a possibility of a attackmove.
					{
						if(movementType.HasFlag(MovementType.Attack)) {
							if(nextTile.Checker.Owner == game.EnemyPlayer.PlayerNumber) {
								if(game.CheckTileAttackable(nextCoordinate, direction)) {
									//the previously generated moves can be removed, since there is a possibility for a attackmove
									if(movementType.HasFlag(MovementType.Move))
										possibleMoveCoordinatesForDirection[MovementType.Move].Clear();
									possibleMoveCoordinatesForDirection[MovementType.Attack].Add(nextCoordinate);
								}
							}
						}
						break;//stop checking moves in this direction, since we found a checker. no need to check the further moves now.
					}
					else {
						if(movementType.HasFlag(MovementType.Move))
							possibleMoveCoordinatesForDirection[MovementType.Move].Add(nextCoordinate);
					}
					//update the nextcoordinate, which is needed for the next direction
					nextCoordinate = TileCoordinate.CalculateLocationAfterSteps(nextCoordinate, direction);
				}
				//add values of this direction to the final result
				foreach(var moveCoordinateForDirection in possibleMoveCoordinatesForDirection) {
					possibleMoveCoordinates[moveCoordinateForDirection.Key].AddRange(moveCoordinateForDirection.Value);
				}
			}
			return possibleMoveCoordinates;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}