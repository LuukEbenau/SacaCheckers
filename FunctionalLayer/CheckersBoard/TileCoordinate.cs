using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FunctionalLayer.CheckersBoard
{
	public struct TileCoordinate
	{
		public const int MAX_X = 10;
		public const int MAX_Y = 10;
		public const int MIN_X = 1;
		public const int MIN_Y = 1;

		public int X { get; private set; }
		public int Y { get; private set; }

		public TileCoordinate(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			//char mapping: 65 => A, 90 => Z
			var ascci = char.ConvertFromUtf32(this.Y + 64);
			return $"{ascci}{this.X}";
		}

		/// <summary>
		/// Return wherether the given coordinate is a valid one
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns>true if between the borders</returns>
		public static bool IsValid(TileCoordinate coordinate) => !(coordinate.X < MIN_X || coordinate.X > MAX_X || coordinate.Y < MIN_Y || coordinate.Y > MAX_Y);

		public bool IsValid() => IsValid(this);

		#region equalsoverrides

		public override bool Equals(object obj) => Equals((TileCoordinate)obj);

		public bool Equals(TileCoordinate coordinate)
		{
			if(coordinate == default) {
				return false;
			}
			if(ReferenceEquals(this, coordinate)) {
				return true;
			}
			return coordinate.GetType() == GetType() && (this.X == coordinate.X && this.Y == coordinate.Y);
		}

		public override int GetHashCode() => this.X.GetHashCode() ^ this.Y.GetHashCode();

		public static bool operator ==(TileCoordinate c1, TileCoordinate c2) => (c1.X == c2.X && c1.Y == c2.Y);

		public static bool operator !=(TileCoordinate c1, TileCoordinate c2) => (c1.X != c2.X || c1.Y != c2.Y);

		#endregion equalsoverrides

		#region static helpers

		/// <summary>
		/// Gets the corner coordinate for a king to move
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static TileCoordinate GetNearestDiagonalEdgeCoordinate(TileCoordinate currentCoordinate, DiagonalDirection direction)
		{
			//north= y+ south= y- east = x+ west = x-
			int x = (direction.HasFlag(DiagonalDirection.East)) ? MAX_X - currentCoordinate.X :
																   MAX_X - (MAX_X - currentCoordinate.X);
			int y = (direction.HasFlag(DiagonalDirection.North)) ? MAX_Y - currentCoordinate.X :
																   MAX_Y - (MAX_Y - currentCoordinate.Y);
			int steps = Math.Min(x, y);
			int newx = (direction.HasFlag(DiagonalDirection.East)) ? currentCoordinate.X + steps : currentCoordinate.X - steps;
			int newy = (direction.HasFlag(DiagonalDirection.North)) ? currentCoordinate.Y + steps : currentCoordinate.Y - steps;
			return new TileCoordinate(newx, newy);
		}

		/// <summary>
		/// Gets the potentional coordinates for an attackMove from a checker.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		public static IEnumerable<TileCoordinate> GetPotentionalAttackMoveCoordinatesForChecker(TileCoordinate coordinate)
		{
			List<TileCoordinate> possibleCoordinates = new List<TileCoordinate> {
				new TileCoordinate(coordinate.X-1,coordinate.Y-1),
				new TileCoordinate(coordinate.X+1,coordinate.Y-1),
				new TileCoordinate(coordinate.X-1,coordinate.Y+1),
				new TileCoordinate(coordinate.X+1,coordinate.Y+1),
			};

			return possibleCoordinates.Where(c => c.IsValid());
		}

		/// <summary>
		/// Get all potentional moves for a normal checker. these arn't checked for being valid.
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		public static IEnumerable<TileCoordinate> GetPotentionalMovesForChecker(PlayerNumber playerNumber, TileCoordinate coordinate)
		{
			int y = coordinate.Y + ((playerNumber == PlayerNumber.One) ? 1 : -1);
			IEnumerable<TileCoordinate> possibleCoordinates = new int[] { coordinate.X - 1, coordinate.X + 1 }.Select(c => new TileCoordinate(c, y));
			return possibleCoordinates.Where(c => c.IsValid());
		}

		/// <summary>
		/// Gets the direction where a step between the two tiles is heading
		/// </summary>
		/// <param name="startTile">Tile from where the step is heading</param>
		/// <param name="targetTile">target tile where the step is heading</param>
		/// <returns>the direction of the step</returns>
		public static DiagonalDirection GetStepDirection(TileCoordinate startTile, TileCoordinate targetTile)
		{
			DiagonalDirection direction = 0;
			direction = (startTile.Y < targetTile.Y) ? DiagonalDirection.North : DiagonalDirection.South;
			direction |= (startTile.X < targetTile.X) ? DiagonalDirection.East : DiagonalDirection.West;
			return direction;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="startingCoordinate">The coordinate where you want to calculate steps from</param>
		/// <param name="direction">direction where the step is heading</param>
		/// <param name="tiles">amount of steps to take. defaults on 1</param>
		/// <returns>the ending coordinate</returns>
		public static TileCoordinate CalculateLocationAfterSteps(TileCoordinate startingCoordinate, DiagonalDirection direction, int tiles = 1)
		{
			var newX = startingCoordinate.X;
			var newY = startingCoordinate.Y;
			newX += direction.HasFlag(DiagonalDirection.East) ? tiles : -tiles;
			newY += direction.HasFlag(DiagonalDirection.North) ? tiles : -tiles;
			return new TileCoordinate(newX, newY);
		}

		#endregion static helpers
	}

	[Flags]
	public enum DiagonalDirection
	{
		North = 0b1,
		East = 0b10,
		South = 0b100,
		West = 0b1000,
		NE = North | East,
		SE = South | East,
		SW = South | West,
		NW = North | West
	}
}