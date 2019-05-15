using System;

using FunctionalLayer.AI.Algorithms;

namespace FunctionalLayer.AI
{
	public class AIPlayer : Player
	{
		protected AIBase AI { get; private set; }
		public override PlayerType PlayerType => PlayerType.AI;
		public AIType AiType { get; private set; }
		public AIPlayer(PlayerNumber playerNumber, string name, PlayerColor playerColor, AIType aiType) : base(playerNumber, name, playerColor)
		{
			AiType = aiType;
			
		}

		public void SubscribeToGameEvents(IGame game)
		{
			game.TurnStarted += Game_TurnFinished;
		}

		public event EventHandler<TurnStartedEventArgs> TurnStarted;

		private void Game_TurnFinished(object sender, TurnStartedEventArgs e)
		{
			//var game = sender as IGame;
			if(e.CurrentPlayer == this) {
				TurnStarted?.Invoke(sender, e);
			}
		}

		public void InitializeAI(IGame game) {
			switch(AiType) {
				case AIType.RandomEasy:
					this.AI = new RandomEasyAI(this, game);
					break;

				case AIType.MiniMax:
					this.AI = new MinimaxAI(this, game, new Minimax(4, game));
					break;

				case AIType.AlphaBeta:
					this.AI = new MinimaxAI(this, game, new AlphaBeta(4, game));
					break;

				case AIType.MinimaxMultiThreaded:
					this.AI = new MinimaxAI(this, game, new MinimaxMultiThreaded(5, game));
					break;

				default:
					throw new ArgumentException("Ai niet geinstantieerd");
			}
		}
	}
}