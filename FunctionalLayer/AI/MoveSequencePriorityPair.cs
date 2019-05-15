using System.Collections.Generic;

using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI
{
	public class MoveSequencePriorityPair : MoveSequence
	{
		public int EnemyPriority;

		public MoveSequencePriorityPair(Turn turn, List<Move> moves, int enemyPriority = 0) : base(turn, moves)
		{
			this.EnemyPriority = enemyPriority;
		}
	}
}