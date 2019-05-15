using FunctionalLayer.CheckersBoard;

namespace FunctionalLayer.GameTurn
{
	public class WalkMove : Move
	{
		public override MovementType MovementType => MovementType.Move;

		private TileCoordinate _endLocation;
		public override TileCoordinate EndLocation => this._endLocation;
		private TileCoordinate _startLocation;
		public override TileCoordinate StartLocation => this._startLocation;
		public override int Priority => 0;

		public WalkMove(IChecker checker, TileCoordinate startLocation, TileCoordinate endLocation) : base(checker)
		{
			this._startLocation = startLocation;
			this._endLocation = endLocation;
		}

		//public override object Clone()
		//{
		//    return new WalkMove((IChecker)Checker.Clone(),StartLocation, EndLocation);
		//}
	}
}