using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FunctionalLayer.AI;
using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;
using FunctionalLayer.Util;
using Newtonsoft.Json;

namespace FunctionalLayer
{
	public class Game : IGame, INotifyPropertyChanged
	{
		#region fields & properties

		private int _turnTimerSecondsLeft;
		public int TurnTimerSecondsLeft { get => this._turnTimerSecondsLeft; private set { this._turnTimerSecondsLeft = value; NotifyPropertyChanged(); } }

		public System.Timers.Timer TurnTimer;

		private IBoard _board;
		private int _p1score;

		private int _p2score;

		private IPlayer _player1;
		private IPlayer _player2;
		
		private int _turn=1;

		private ISettings _settings;
		public ISettings Settings { get => this._settings; private set { this._settings = value; NotifyPropertyChanged(); } }

		private int _player1StartingCheckerCount;
		public int Player1StartingCheckerCount { get => this._player1StartingCheckerCount; set { this._player1StartingCheckerCount = value; NotifyPropertyChanged(nameof(this.Player1AmountOfLostChecker)); } }
		public int Player1AmountOfLostChecker => this.Player1StartingCheckerCount - this.Player1.GetPlayerOwnedCheckers(this.Board.Tiles).Count();

		private int _player2StartingCheckerCount;
		public int Player2StartingCheckerCount { get => this._player2StartingCheckerCount; set { this._player2StartingCheckerCount = value; NotifyPropertyChanged(nameof(this.Player2AmountOfLostChecker)); } }
		public int Player2AmountOfLostChecker => this.Player2StartingCheckerCount - this.Player2.GetPlayerOwnedCheckers(this.Board.Tiles).Count();

		private bool _paused;
		public bool Paused { get => this._paused; private set { this._paused = value; NotifyPropertyChanged(); } }

		public void Pause()
		{
			this.Paused = true;
		}

		public void Resume()
		{
			this.Paused = false;
		}

		/// <summary>
		/// For calls from other threads, such as the timer
		/// </summary>
		private readonly SynchronizationContext mainSyncContext = SynchronizationContext.Current;

		private ObservableCollection<DoneTurn> _turnHistory;
		public ObservableCollection<DoneTurn> TurnHistory { get => this._turnHistory; private set { this._turnHistory = value; NotifyPropertyChanged(); } }

		public event EventHandler<EventArgs> GameStarted;

		public event EventHandler<EventArgs> PostGameStarted;

		public IBoard Board {
			get {
				if(this._board == null) {
					this._board = new Board();
					this._board.GenerateTiles();
					this._board.InitializePlayers(this, Player1, Player2);
				}
				return this._board;
			}
			set => this._board = value;
		}
		public IBoard InitialBoard { get; private set; }
		
		public IPlayer CurrentPlayer => this.Player1IsCurrentlyPlaying ? this.Player1 : this.Player2;

		
		public IPlayer EnemyPlayer => this.Player1IsCurrentlyPlaying ? this.Player2 : this.Player1;

		
		public int P1Score { get => this._p1score; set { this._p1score = value; NotifyPropertyChanged(); } }

		
		public int P2Score { get => this._p2score; set { this._p2score = value; NotifyPropertyChanged(); } }

		private bool _scoreShown = false;
		public bool ScoreShown { get => this._scoreShown; private set { this._scoreShown = value; NotifyPropertyChanged(); } }

		public virtual IPlayer Player1 { get => this._player1; set { this._player1 = value; NotifyPropertyChanged(); } }

		public virtual IPlayer Player2 { get => this._player2; set { this._player2 = value; NotifyPropertyChanged(); } }

		public int TotalGamesPlayed => this.P1Score + this.P2Score;

		public int Turn {
			get => this._turn; private set {
				this._turn = value;
				NotifyPropertyChanged();
				//NOTE: we have to notify all dependant properties for the databindings to update. its far from elegant but i don't know a better solution.
				NotifyPropertyChanged(nameof(this.Player1IsCurrentlyPlaying));
				NotifyPropertyChanged(nameof(this.Player2IsCurrentlyPlaying));
				NotifyPropertyChanged(nameof(this.CurrentPlayer));
				NotifyPropertyChanged(nameof(this.EnemyPlayer));
			}
		}

		public bool Player1IsCurrentlyPlaying {
			get {
				var rest = (this.Player1.PlayerColor == PlayerColor.Light) ? 1 : 0;
				return (this.Turn % 2 == rest);
			}
		}

		public bool Player2IsCurrentlyPlaying => !this.Player1IsCurrentlyPlaying;

		#endregion fields & properties

		#region events

		public event Action<IPlayer> GameFinished;

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler<TurnStartedEventArgs> TurnStarted;

		public event Action<Turn> MoveOfTurnDone;

		#endregion events

		#region eventhandlers

		/// <summary>
		/// Gets triggered at the start of every turn
		/// </summary>
		private void Game_TurnStarted(object sender, EventArgs e)
		{
			if(this.TurnTimer != null) {
				this.TurnTimer.Stop();
				if(this.CurrentPlayer.PlayerType == PlayerType.Human) {
					this.TurnTimerSecondsLeft = this.Settings.TimerInterval;
					this.TurnTimer.Start();
				}
			}
		}

		private void TurnTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(this.Paused)
				return;
			if(--this.TurnTimerSecondsLeft == 0) {
				this.mainSyncContext.Send((object d) => {
					FinishTurn();
				}, null);
			}
		}

		#endregion eventhandlers
		[JsonConstructor]
		public Game(ISettings settings)
		{
			this.Settings = settings;
			if(settings.TimerEnabled) {
				InitializeTimer();
			}
		}

		#region private methods

		private void SaveDoneMoveToHistory(Move doneMove)
		{
			var currentTurn = this.TurnHistory.FirstOrDefault(t => t.TurnNumber == this.Turn);
			if(currentTurn == null) {
				currentTurn = new DoneTurn(this.Turn, this.CurrentPlayer);
				this.TurnHistory.Add(currentTurn);
			}
			if(doneMove != null)
				currentTurn.DoneMoves.Add(doneMove);
		}

		private void TakeRandomTurn()
		{
			var randomTurn = this.CurrentPlayer.GenerateRandomTurn(this, this.EnemyPlayer, this.Board.Tiles);
			if(randomTurn == null)
				Surrender(this.CurrentPlayer);
			int i = 0;
			while(!TakeTurn(randomTurn.Turn, randomTurn.Moves[i++])) ;
		}

		private void FinishTurn()
		{
			TakeRandomTurn();
		}

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected void FinishGame(IPlayer winningPlayer)
		{
			if(winningPlayer == this.Player1)
				this.P1Score++;
			else if(winningPlayer == this.Player2)
				this.P2Score++;
			else
				throw new Exception("The winning player was not even playing in the game!");
			GameFinished?.Invoke(winningPlayer);
		}

		#endregion private methods

		#region public methods

		public void Surrender(IPlayer surrenderingPlayer)
		{
			FinishGame(GetEnemyPlayer(surrenderingPlayer));
		}

		public IPlayer GetEnemyPlayer(IPlayer player)
		{
			IPlayer enemy;
			if(player == this.Player1)
				enemy = this.Player2;
			else if(player == this.Player2)
				enemy = this.Player1;
			else
				throw new ArgumentException("The given player does not belong to this game!");
			return enemy;
		}

		public bool CheckTileAttackable(TileCoordinate targetTile, DiagonalDirection attackDirection)
		{
			var locationAfterJunp = TileCoordinate.CalculateLocationAfterSteps(targetTile, attackDirection);
			if(!locationAfterJunp.IsValid())
				return false;

			var tile = this.Board.Tiles.FirstOrDefault(t => t.Coordinate == locationAfterJunp);
			bool locationAfterJunpFree = tile.Checker == null;

			if(!locationAfterJunpFree)
				return false;
			//not a checker, so free
			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="player1"></param>
		/// <param name="player2"></param>
		/// <param name="TimerInterval">interval in miliseconds</param>
		public void InitializeTimer()
		{
			this.TurnTimer = new System.Timers.Timer(1000);
			
			this.TurnTimer.Elapsed += TurnTimer_Elapsed;
			this.TurnTimer.Start();//this is for some reason needed for the scenarioeditor, not sure why since it works normally when using singleplayer
			TurnStarted += Game_TurnStarted;
		}

		public void Rematch()
		{
			Board = InitialBoard;
			this.Player1.PlayerColor = (this.Player1.PlayerColor == PlayerColor.Dark) ? PlayerColor.Light : PlayerColor.Dark;
			this.Player2.PlayerColor = (this.Player2.PlayerColor == PlayerColor.Dark) ? PlayerColor.Light : PlayerColor.Dark;
			this.ScoreShown = true;
			this.Turn = 1;
			StartGame();
		}

		public void StartGame()
		{
			if(this.Player1 == null)
				throw new ArgumentNullException("Player 1 is instanciated");
			if(this.Player2 == null)
				throw new ArgumentNullException("Player 2 is not instanciated");
			//CHECKME: does it matter that this gets executed again after rematch? maybe a initial start method is helpfull?

			if(this.Player1 is AIPlayer p1ai) {
				p1ai.SubscribeToGameEvents(this);
				p1ai.InitializeAI(this);

			}
			if(this.Player2 is AIPlayer p2ai) {
				p2ai.InitializeAI(this);
				p2ai.SubscribeToGameEvents(this);
			}
			this.TurnHistory = new ObservableCollection<DoneTurn>();
			Board.SubscribeToGameEvents(this);

			//store initialBoard for a potentional rematch
			if(InitialBoard==null)
				InitialBoard = Board.Clone() as IBoard;

			GameStarted?.Invoke(this,new EventArgs());
			PostGameStarted?.Invoke(this, new EventArgs());
			TurnStarted?.Invoke(this, new TurnStartedEventArgs(Turn, CurrentPlayer));
			this.TurnTimer?.Start();
		}

		public bool TakeTurn(Turn turn, Move moveToTake)
		{
			bool moreMovesLeft = CheckersBoard.Board.TakeMove(this.Board.Tiles, moveToTake);

			NotifyPropertyChanged(this.CurrentPlayer == this.Player1
				? nameof(this.Player2AmountOfLostChecker)
				: nameof(this.Player1AmountOfLostChecker));

			SaveDoneMoveToHistory(moveToTake);
			MoveOfTurnDone?.Invoke(turn);
			if(moreMovesLeft) {
				turn.ConfirmMoveStep((AttackMove)moveToTake);
				return false;
			}
			//when the game is finished
			if(!this.EnemyPlayer.GetPlayerOwnedCheckers(this.Board.Tiles).Any()) {
				FinishGame(this.CurrentPlayer);
				return true;
			}
			//promote to king of on baseline of enemy player
			//var checker = moveToTake.Checker;
			//if(checker.Type == CheckerType.Men) {
			//	if(checker.IsInEnemyBaseRow(this.Board.Tiles)) {
			//		checker.PromoteToKing();
			//	}
			//}
			this.Turn++;
			TurnStarted?.Invoke(this, new TurnStartedEventArgs(Turn,CurrentPlayer));
			return true;
		}

		public IPlayer GetPlayer(PlayerNumber number)
		{
			if(Player1.PlayerNumber == number)
				return Player1;
			else if(Player2.PlayerNumber == number)
				return Player2;
			else
				throw new ArgumentException("none of the players has the given number, this must be a bug");
		}

		#endregion public methods
	}
	public class TurnStartedEventArgs : EventArgs {
		public int TurnNumber { get; }

		public IPlayer CurrentPlayer { get; }

		public TurnStartedEventArgs(int turnNumber, IPlayer currentPlayer) {
			this.TurnNumber = turnNumber;
			this.CurrentPlayer = currentPlayer;

		}
	}
}