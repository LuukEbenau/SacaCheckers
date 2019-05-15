using FunctionalLayer.CheckersBoard;

namespace FunctionalLayer.GameTurn
{
	/// <summary>
	/// A move is the baseclass for a move. A <see cref="Turn"/> is Everything a player does in a turn, and a move is one of the steps in that turn.
	/// </summary>
	public abstract class Move//:ICloneable
	{
		/// <summary>
		/// The location where the checker will end after doing the move.
		/// </summary>
		public abstract TileCoordinate EndLocation { get; }

		/// <summary>
		/// The location where the move started.
		/// </summary>
		public abstract TileCoordinate StartLocation { get; }

		/// <summary>
		/// The checker which is making the move
		/// </summary>
		public IChecker Checker { get; private set; }

		/// <summary>
		/// Gives the priority of the move for attackorder.
		/// </summary>
		public abstract int Priority { get; }

		/// <summary>
		/// what kind of move this is.
		/// </summary>
		public abstract MovementType MovementType { get; }

		protected Move(IChecker checker)
		{
			this.Checker = checker;
		}

		//public abstract object Clone();
	}
}