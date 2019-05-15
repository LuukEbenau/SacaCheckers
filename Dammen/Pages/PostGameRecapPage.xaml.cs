using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using FunctionalLayer;
using FunctionalLayer.CheckersBoard;
using FunctionalLayer.Util;
using SacaDev.Diagnostics;

namespace Dammen.Pages
{
	/// <summary>
	/// Interaction logic for PostGameRecabPage.xaml
	/// </summary>
	public partial class PostGameRecapPage : Page, INotifyPropertyChanged
	{
		private ILogger Logger => App.container.Get<ILogger>();
		public event EventHandler Rematch;
		public event EventHandler NewGame;
		public event EventHandler ReturnToMainMenu;
		public PostGameRecapPageDataObject GameData { get; private set; }

		private IBoard _shownBoard;
		public IBoard ShownBoard { get => _shownBoard;set { _shownBoard = value; NotifyPropertyChanged(); } }

		public PostGameRecapPage(PostGameRecapPageDataObject data)
		{
			this.GameData = data;
			InitializeComponent();
			ShownBoard = this.GameData.Board.Clone() as IBoard;
		}

		private void btnQuit_Click(object sender, RoutedEventArgs e)
		{
			(Application.Current.MainWindow as MainWindow).Quit();
		}

		private void btnNewGame_Click(object sender, RoutedEventArgs e)
		{
			NewGame?.Invoke(this,new EventArgs());
		}

		private void btnRematch_Click(object sender, RoutedEventArgs e)
		{
			Rematch?.Invoke(this, new EventArgs());
		}

		private void btnMainMenu_Click(object sender, RoutedEventArgs e)
		{
			ReturnToMainMenu?.Invoke(this, new EventArgs());
		}

		private BoardTileCollection UpdateCheckersOnTileCollection(BoardTileCollection tiles, Dictionary<TileCoordinate,IChecker> coordinateCheckerMap) {
			foreach(var tile in tiles) {
				//FIXME: this has reference type side effects since it updates values on the input collection, but doesnt matter for current implementation
				if(coordinateCheckerMap.TryGetValue(tile.Coordinate, out IChecker checker)){
					tile.Checker = checker;
				}
				else {
					tile.Checker = null;
				}
			}
			
			return tiles;
		}

		private Dictionary<TileCoordinate,IChecker> GenerateCheckerMap(BoardTileCollection tiles)
		{
			Dictionary<TileCoordinate, IChecker> checkerMap = new Dictionary<TileCoordinate, IChecker>();

			foreach(var tile in tiles) {
				if(tile.Checker != null)
					checkerMap.Add(tile.Coordinate, tile.Checker);
			}
			return checkerMap;
		}

		/// <summary>
		/// Stored the boardstate BEFORE the turnnumber in the KEY has been done.
		/// </summary>
		private readonly Dictionary<int, Dictionary<TileCoordinate, IChecker>> _storedBoardStates = new Dictionary<int, Dictionary<TileCoordinate,IChecker>>();

		private void UCGameHistory_HistoryItemHoverEnter(object sender, UC.GameHistoryTurnHoverEventArgs e)
		{
			this.board.HighlightPossibleMovesForTurnInHistory(e.DoneTurn);

			var history = GameData.Game.TurnHistory;

			if(_storedBoardStates.TryGetValue(e.DoneTurn.TurnNumber, out Dictionary<TileCoordinate, IChecker> checkerMapFromCache)) {
				ShownBoard.Tiles = UpdateCheckersOnTileCollection(ShownBoard.Tiles, checkerMapFromCache);
				return;
			}

			//get last turn
			int lastStoredTurn = 0;
			var storedTurnsTilCurrent = _storedBoardStates.Keys.Where(k => k < e.DoneTurn.TurnNumber);

			IBoard board = GameData.Game.InitialBoard.Clone() as IBoard;
			if(storedTurnsTilCurrent.Count() != 0) { 
				lastStoredTurn = storedTurnsTilCurrent.Max(k => k);

				board.Tiles = UpdateCheckersOnTileCollection(board.Tiles, _storedBoardStates[lastStoredTurn]);
			}

			Dictionary<TileCoordinate, IChecker> lastTileMap = null;
			try {
				for(int i = lastStoredTurn + 1; i < e.DoneTurn.TurnNumber; i++) {
					//add the state before the turn

					lastTileMap = GenerateCheckerMap(board.Tiles);
					_storedBoardStates.Add(i, lastTileMap);

					var currentDoneTurn = history.First(h => h.TurnNumber == i);
					int ii = 0;

					while(Board.TakeMove(board.Tiles, currentDoneTurn.DoneMoves[ii])) {
						ii++;
					}
				}
				if(lastTileMap == null) //only needed for last index, takes the last move instead in that case
					ShownBoard.Tiles = UpdateCheckersOnTileCollection(board.Tiles, _storedBoardStates.Last().Value);
				else
					ShownBoard.Tiles = UpdateCheckersOnTileCollection(board.Tiles, lastTileMap);
			}
			catch(Exception ex) {
				Logger.Write($"Postgamerecap: An error has occured while trying to backtrace turn {lastStoredTurn+1} from gamehistory. \"{ex.Message}\"");
			}
		}

		private void UCGameHistory_HistoryItemHoverLeave(object sender, EventArgs e)
		{
			this.board.ClearHighlightingFromTurnHistory();
			ShownBoard.Tiles = UpdateCheckersOnTileCollection(ShownBoard.Tiles, GenerateCheckerMap(GameData.Board.Tiles));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected struct CoordinateCheckerPair {
			TileCoordinate Coordinate { get; set; }
			IChecker Checker { get; set; }
		}
	}

	public class PostGameRecapPageDataObject: INotifyPropertyChanged
	{
		public IGame Game { get; private set; }
		public IPlayer WinningPlayer { get; private set; }
		public IPlayer LosingPlayer { get; private set; }
		private IBoard _board;
		public IBoard Board { get => _board; private set { _board = value; NotifyPropertyChanged(); } }

		public PostGameRecapPageDataObject(IGame game, IPlayer winningPlayer, IPlayer losingPlayer, IBoard board)
		{
			this.Game = game;
			this.WinningPlayer = winningPlayer;
			this.LosingPlayer = losingPlayer;
			this.Board = board;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}