using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI
{
	public class MinimaxAI : AIBase
	{
		private IGameAI AI { get; set; }

		public MinimaxAI(IPlayer player, IGame game, IGameAI ai) : base(player, game)
		{
			this.AI = ai;
		}

		public override MoveSequence GenerateMovesForTurn()
		{
			var currentPlayer = this.Player;
			//for each of the moves, evaluate each situation
			var result = this.AI.GetBestTurn(this.Game.Board.Tiles, currentPlayer);

			if(result == null) {
				return null;
			}

			return new MoveSequence(result.Turn, result.Moves);
		}
	}
}