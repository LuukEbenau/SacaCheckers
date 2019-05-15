using System.Collections.Generic;

using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI
{
	public class MinimaxResult
	{
		public float TurnValue { get; private set; }
		public List<Move> Moves { get; private set; }
		public Turn Turn { get; private set; }

		public MinimaxResult(List<Move> moves, Turn turn, float turnValue)
		{
			this.Moves = moves;
			this.Turn = turn;
			this.TurnValue = turnValue;
		}
	}
}