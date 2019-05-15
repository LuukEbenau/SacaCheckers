using System.Collections.Generic;
using System.Linq;

using FunctionalLayer.CheckersBoard;

namespace FunctionalLayer.GameTurn
{
	/// <summary>
	/// A turn contains all possible actions for a checker. it contains all followup moves.
	/// </summary>
	public class Turn
	{
		/// <summary>
		/// This contains all the moves the turn contains
		/// </summary>
		private IEnumerable<Move> TotalMoves { get; set; }

		/// <summary>
		/// This contains the moves for the current movestep. It gets updated after each move in a multimove.
		/// </summary>
		public IEnumerable<Move> Moves { get; private set; }

		/// <summary>
		/// Tell the turn that the given movestep is succesfully executed, so progress to the next move in the multimove chain.
		/// </summary>
		/// <param name="move">The move which has been done</param>
		public void ConfirmMoveStep(AttackMove move)
		{
			this.Moves = move.FurtherMoves;
		}

		/// <summary>
		/// Calculates the highest priority of the turn.
		/// </summary>
		public int HighestPriorityInTurn => (this.TotalMoves.Count() > 0) ? this.TotalMoves.Max(m => m.Priority) : 0;

		public IEnumerable<TileCoordinate> GetPotentionalEndingLocations()
		{
			var endingLocations = new List<TileCoordinate>();
			foreach(var move in this.Moves) {
				if(move is WalkMove)
					endingLocations.Add(move.EndLocation);
				else if(move is AttackMove attMove)
					endingLocations.AddRange(attMove.GetPotenionalEndingLocations());
			}
			return endingLocations;
		}

		public Turn(IEnumerable<Move> moves)
		{
			this.TotalMoves = moves;
			this.Moves = this.TotalMoves;
		}
	}
}