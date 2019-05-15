using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI
{
	public class RandomEasyAI : AIBase
	{
		public RandomEasyAI(IPlayer player, IGame game) : base(player, game)
		{
		}

		public override MoveSequence GenerateMovesForTurn()
		{
			return this.Player.GenerateRandomTurn(Game, this.Game.GetEnemyPlayer(this.Player), this.Game.Board.Tiles);
		}
	}
}