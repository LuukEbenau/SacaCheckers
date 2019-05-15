using System.Threading;
using System.Threading.Tasks;

using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI
{
	public abstract class AIBase
	{
		protected AIPlayer Player { get; }
		protected IGame Game { get; }
		private readonly SynchronizationContext _mainSyncContext = SynchronizationContext.Current;

		public AIBase(IPlayer player, IGame game)
		{
			this.Player = player as AIPlayer;
			this.Game = game;
			this.Player.TurnStarted += Player_TurnStarted;
		}

		private void Player_TurnStarted(object sender, TurnStartedEventArgs e)
		{
			Task.Run(() => {
				var moveSequence = GenerateMovesForTurn();

				if(moveSequence == null) {
					this._mainSyncContext.Send((state) => {
						this.Game.Surrender((IPlayer)state);
					}, this.Player);
					return;
				}

				this._mainSyncContext.Send((s) => {
					var ms = (MoveSequence)s;
					int i = 0;
					while(!this.Game.TakeTurn(ms.Turn, ms.Moves[i++])) ;
				}, moveSequence);
			});
		}

		public abstract MoveSequence GenerateMovesForTurn();
	}
}