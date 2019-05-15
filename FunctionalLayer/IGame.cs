using System;
using System.Collections.ObjectModel;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;
using Newtonsoft.Json;

namespace FunctionalLayer
{
	public interface IGame
	{
		#region events
		/// <summary>
		/// Is invoked when the game is concidered finished. parameter is the player who has won, null if draw.
		/// </summary>
		event Action<IPlayer> GameFinished;
		event EventHandler<EventArgs> GameStarted;

		/// <summary>
		/// Triggers after gameStarted has finished
		/// </summary>
		event EventHandler<EventArgs> PostGameStarted;
		/// <summary>
		/// Is invoked after a entire turn is done.
		/// </summary>
		event EventHandler<TurnStartedEventArgs> TurnStarted;
		event Action<Turn> MoveOfTurnDone;
		#endregion

		#region Player data
		IPlayer Player1 { get; set; }
		[JsonIgnore]
		int Player1AmountOfLostChecker { get; }
		int Player1StartingCheckerCount { get; set; }
		int P1Score { get; set; }

		/// <summary>
		/// Returns wherether it's player1's turn or not.
		/// </summary>
		[JsonIgnore]
		bool Player1IsCurrentlyPlaying { get; }

		IPlayer Player2 { get; set; }
		[JsonIgnore]
		int Player2AmountOfLostChecker { get; }
		int Player2StartingCheckerCount { get; set; }
		int P2Score { get; set; }

		IPlayer CurrentPlayer { get; }
		IPlayer EnemyPlayer { get; }

		#endregion

		#region Game state
		[JsonRequired]
		IBoard Board { get; set; }
		[JsonIgnore]
		IBoard InitialBoard { get; }

		/// <summary>
		/// Seconds left on the turn
		/// </summary>
		int TurnTimerSecondsLeft { get; }
		bool Paused { get; }
		int TotalGamesPlayed { get; }
		int Turn { get; }
		#endregion

		ISettings Settings { get; }
		ObservableCollection<DoneTurn> TurnHistory { get; }
		
		#region methods
		/// <summary>
		/// Get player by its playernumber
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		IPlayer GetPlayer(PlayerNumber number);
		/// <summary>
		/// Get the neemy player of the input player
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		IPlayer GetEnemyPlayer(IPlayer player);
		/// <summary>
		/// Rematch the same player again, and switch sides.
		/// </summary>
		void Rematch();
		/// <summary>
		/// Surrender, and give the enemy the victory
		/// </summary>
		/// <param name="surrenderingPlayer">The losing player</param>
		void Surrender(IPlayer surrenderingPlayer);
		/// <summary>
		/// Pause the game
		/// </summary>
		void Pause();
		/// <summary>
		/// Unpause the game
		/// </summary>
		void Resume();
		/// <summary>
		/// Checks wherether or not the tile is attackable
		/// </summary>
		/// <param name="targetTile">the tile which is being attacked</param>
		/// <param name="attackDirection">The direction the attack is going</param>
		/// <returns>true when the tile is able to be attacked, otherwise false</returns>
		bool CheckTileAttackable(TileCoordinate targetTile, DiagonalDirection attackDirection);
		void StartGame();
		/// <summary>
		/// Actually take the turn.
		/// </summary>
		/// <returns>true if turn is finished, if multimoves left return false</returns>
		/// <param name="moveToTake"> the move the user is going to take</param>
		bool TakeTurn(Turn turn, Move moveToTake);
		#endregion
	}
}