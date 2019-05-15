using System.Collections.Generic;
using System.Linq;

namespace FunctionalLayer.GameTurn
{
	public static class MoveExtensions
	{
		/// <summary>
		/// Filter a list of moves to return only the one with the highest priority.
		/// </summary>
		/// <typeparam name="TMove"></typeparam>
		/// <param name="moves"></param>
		/// <returns>A list with all highest priority moves.</returns>
		public static IEnumerable<TMove> GetOnlyHighestPriorityMoves<TMove>(this IEnumerable<TMove> moves) where TMove : Move
		{
			return moves.Where(m => m.Priority == moves.Select(mm => mm.Priority).Max());
		}

		public static int GetHighestPriorityTurn(this IEnumerable<Turn> turns) => (turns.Count() == 0) ? 0 : turns.Max(a => a.HighestPriorityInTurn);
	}
}