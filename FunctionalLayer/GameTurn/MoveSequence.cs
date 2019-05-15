using System.Collections.Generic;

namespace FunctionalLayer.GameTurn
{
	public class MoveSequence
	{
		public List<Move> Moves = new List<Move>();
		public Turn Turn;

		public MoveSequence(Turn turn, List<Move> moves)
		{
			this.Moves = moves;
			this.Turn = turn;
		}
	}
}