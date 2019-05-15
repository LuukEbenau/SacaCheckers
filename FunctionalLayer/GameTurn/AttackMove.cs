using System.Collections.Generic;
using System.Linq;

using FunctionalLayer.CheckersBoard;

namespace FunctionalLayer.GameTurn
{
	public class AttackMove : Move
	{
		/// <summary>
		/// The target checker of the move, which will be removed after the move.
		/// </summary>
		public IChecker TargetChecker { get; private set; }

		public TileCoordinate TargetCheckerCoordinate { get; set; }
		public override MovementType MovementType => MovementType.Attack;

		/// <summary>
		/// Calculates the priority of the move, it does this by repeatingly adding the furthermoves prio until max prio is reached
		/// </summary>
		public override int Priority {
			get {
				List<int> prios = new List<int>();
				foreach(var move in this.FurtherMoves) {
					prios.Add(move.Priority);
				}
				//only get the prio of the furthermove with the highest prio
				int xtraPrio = prios.FirstOrDefault(p => p == prios.Max());
				//return the prio of the best next move + 2
				return 1 + xtraPrio;
			}
		}

		public IEnumerable<TileCoordinate> GetPotenionalEndingLocations()
		{
			var endingLocations = new List<TileCoordinate>();
			if(this.FurtherMoves.Count() > 0) {
				foreach(var move in this.FurtherMoves) {
					endingLocations.AddRange(move.GetPotenionalEndingLocations());
				}
			}
			else {
				endingLocations.Add(this.EndLocation);
			}
			return endingLocations;
		}

		/// <summary>
		/// All possible followup moves
		/// </summary>
		public List<AttackMove> FurtherMoves { get; private set; }

		private TileCoordinate _endLocation;
		public override TileCoordinate EndLocation { get => this._endLocation; }
		private TileCoordinate _startLocation;
		public override TileCoordinate StartLocation { get => this._startLocation; }

		/// <summary>
		/// Create a attacking type of move, can contain followup moves.
		/// </summary>
		/// <param name="checker">the checker which executes the move</param>
		/// <param name="target">The checker which gets attacked</param>
		/// <param name="startCoordinate">The coordinate where the checker starts the move</param>
		/// <param name="endingCoordinate">the coordinate where the checker ends after the move</param>
		/// <param name="targetCheckerCoordinate">The coordinate where the target checker is located</param>
		/// <param name="furtherMoves">Any followup moves</param>
		public AttackMove(IChecker checker, IChecker target, TileCoordinate startCoordinate, TileCoordinate endingCoordinate, TileCoordinate targetCheckerCoordinate, List<AttackMove> furtherMoves = null) : base(checker)
		{
			if(furtherMoves == null)
				furtherMoves = new List<AttackMove>();
			this.TargetChecker = target;
			this._startLocation = startCoordinate;
			this._endLocation = endingCoordinate;
			this.TargetCheckerCoordinate = targetCheckerCoordinate;
			this.FurtherMoves = furtherMoves;
		}
	}
}