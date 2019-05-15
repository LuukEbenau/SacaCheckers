using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace FunctionalLayer
{
	public class Player : INotifyPropertyChanged, IPlayer
	{
		public IEnumerable<IChecker> GetPlayerOwnedCheckers(BoardTileCollection tiles) => tiles.Where(t => t.Checker != null)
			.Select(t => t.Checker).Where(c => c.Owner == this.PlayerNumber);

		//public IGame Game { get; private set; }
		private string _name;
		public string Name { get => this._name; set { this._name = value; NotifyPropertyChanged(); } }
		public PlayerColor PlayerColor { get; set; }
		public virtual PlayerType PlayerType => PlayerType.Human;
		public PlayerNumber PlayerNumber { get; private set; }

		/// <summary>
		/// Generates a random turn for the given player
		/// </summary>
		/// <param name="currentPlayer"></param>
		/// <param name="enemyPlayer"></param>
		/// <param name="tiles"></param>
		/// <returns></returns>
		public MoveSequence GenerateRandomTurn(IGame game, IPlayer enemyPlayer, BoardTileCollection tiles)
		{
			Turn turn = null;
			var moves = new List<Move>();

			var turns = GetAllPossibleMoves(game, enemyPlayer, tiles);
			var random = new Random();
			int randomNumber = random.Next(0, turns.Count);
			turn = turns.ElementAtOrDefault(randomNumber).Value;
			if(turn == null)
				return null;
			random = new Random();
			var ind = random.Next(0, turn.Moves.Count());
			var firstMove = turn.Moves.ElementAt(ind);
			moves.Add(firstMove);

			if(turn.Moves.ElementAt(0) is AttackMove) {
				var movesInTurn = turn.Moves;
				AttackMove move = firstMove as AttackMove;
				while(move.FurtherMoves.Count() != 0) {
					random = new Random();
					int moveIndex = random.Next(0, move.FurtherMoves.Count());
					var m = move.FurtherMoves.ElementAt(moveIndex);
					moves.Add(m);
					move = m;
				}
			}

			return new MoveSequence(turn, moves);
		}

		public Player(PlayerNumber playerNumber, string name, PlayerColor color)
		{
			this.Name = name;
			this.PlayerColor = color;
			this.PlayerNumber = playerNumber;

		}

		public Dictionary<IChecker, Turn> GetAllPossibleMoves(IGame game,IPlayer enemyPlayer, BoardTileCollection tiles)
		{
			//FIXME: something goes whrong with the ai here...
			var possibleTurnsForEachChecker = new Dictionary<IChecker, Turn>();
			foreach(var checker in GetPlayerOwnedCheckers(tiles)) {
				var turn = checker.CreateTurn(game, tiles, this, enemyPlayer);
				if(turn.Moves.Count() != 0)
					possibleTurnsForEachChecker.Add(checker, turn);
			}
			possibleTurnsForEachChecker = possibleTurnsForEachChecker.Where(t => t.Value.HighestPriorityInTurn ==
			possibleTurnsForEachChecker.Max(pt => pt.Value.HighestPriorityInTurn)).ToDictionary(x => x.Key, x => x.Value);

			return possibleTurnsForEachChecker;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}