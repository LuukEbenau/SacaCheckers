using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI.Algorithms
{
	public class MinimaxMultiThreaded : IGameAI
	{
		private IGame Game { get; set; }
		public int MaxDepth { get; private set; }
		protected float GetValueOfMoveSequence(int depth, BoardTileCollection tiles, float priority, IPlayer currentPlayer, IPlayer enemyPlayer, bool max)
		{
#warning DE BUG dat ai niet goed kijkt komt denk ik omdat ik de checker niet copy aan het begin, dus de checker data (is king, enz.) kan op de ene plek al geupdatet worden, en in de andere tree vervolgens verkeert berekent. FIX DIT.
			//Exit condition
			if(depth == 0) {
				return priority;
			}
			float bestPrio = priority;
			var checkerMovePairs = currentPlayer.GetAllPossibleMoves(Game, enemyPlayer, tiles);
			//This paralelisation is increasing the performance quite a lot, but since its called recursively it's maybe not smart to run it in from the start to the end?
			//I expect this would create quite a bit of overhead by creating millions of tasks.
			Parallel.ForEach(checkerMovePairs, (pair) => {
				#region DEBUG
#if DEBUG
				lock(this._amountOfCheckupsLock)
					this._amountOfCheckups++;
#endif
				#endregion
				List<MoveSequence> possibleSequences;
				Turn turn = pair.Value;
				foreach(var move in turn.Moves) {
					if(move is AttackMove attMove) {
						possibleSequences = GetPossibleMoveSequences(turn, attMove);
					}
					else {
						possibleSequences = new List<MoveSequence> {
							new MoveSequence(turn, new List<Move> { move })
						};
					}
					List<float> betterFoundPrios = new List<float>();
					foreach(MoveSequence sequence in possibleSequences) {
						BoardTileCollection clonedTiles = (BoardTileCollection)tiles.Clone();
						int prio = sequence.Moves[0].Priority;
						foreach(var mov in sequence.Moves) {
							//TODO: atm it doesnt update checker to king when looking in the future. for this we probarly need to clone checker to.
							Board.TakeMove(clonedTiles, mov);
						}

						float newPrio = max ? 
							  priority + prio 
							: priority - prio;

						//now we need to call the same method from the enemy perception again
						// recursively collect all sequences for the followup moves
						var furtherMovesValue = GetValueOfMoveSequence(depth - 1, clonedTiles, newPrio, enemyPlayer, currentPlayer, !max);
						//if(max) {
						lock(_addSequenceLock) {
							if(furtherMovesValue > bestPrio) {
								betterFoundPrios.Add(furtherMovesValue);
							}
						}
							
						//}
						//else {
						//	lock(_addSequenceLock) {
						//		if(furtherMovesValue < bestPrio) {
						//			bestPrio = furtherMovesValue;
						//		}
						//	}
						//}
						//results.Add(furtherMovesValue);
						//return furtherMovesValue;
					}
					//get the best of the fould prios
					if(betterFoundPrios.Any()) {
						lock(_addSequenceLock) {
							bestPrio = betterFoundPrios.Max();
						}
					}

				}
			});

			return bestPrio;
		}
		private readonly object _addSequenceLock = new object();
#if DEBUG
		private readonly object _amountOfCheckupsLock = new object();
		private long _amountOfCheckups;
#endif

		/// <summary>
		/// Recursively search for the best move for the given board
		/// </summary>
		/// <param name="depth">How many recursive iterations you want to take(each iteration is 1 turn of 1 of the players, so steps ahead is depth/2</param>
		/// <param name="tiles">The Tile to iterate over</param>
		/// <param name="initialSequence">Initial sequence of moves, which is used as state to know what sequence </param>
		/// <param name="priority"></param>
		/// <param name="max"></param>
		/// <param name="currentPlayer"></param>
		/// <param name="enemyPlayer"></param>
		/// <returns></returns>
		public MinimaxResult GetBestTurn(BoardTileCollection tiles, IPlayer currentPlayer)
		{
#if DEBUG
			this._amountOfCheckups = 0;
#endif
			IPlayer enemyPlayer = this.Game.GetEnemyPlayer(currentPlayer);
			var results = new List<MinimaxResult>();

			//get all possible initial moves for the player
			var checkerMovePairs = currentPlayer.GetAllPossibleMoves(Game, enemyPlayer, tiles);

			Parallel.ForEach(checkerMovePairs, (pair, loopState) => {
				List<MoveSequence> possibleSequences = null;
				Turn turn = pair.Value;
				foreach(var move in turn.Moves) { //for each initial move of the checker
					if(move is AttackMove attMove) {
						//all possible followup move sequences
						possibleSequences = GetPossibleMoveSequences(turn, attMove);
					}
					else if(move is WalkMove) {
						possibleSequences = new List<MoveSequence> {
							new MoveSequence(turn, new List<Move> { move })
						};
					}
					//in case theres only 1 possible move, we dont need to calculate anything, so cut of.
					if(checkerMovePairs.Count == 1 && possibleSequences.Count == 1) {
						var seq = possibleSequences.First();
						results.Add(new MinimaxResult(seq.Moves, seq.Turn, 0));
						loopState.Break();
					}

					foreach(var sequence in possibleSequences) {
						BoardTileCollection clonedTiles = (BoardTileCollection)tiles.Clone();
						float prio = sequence.Moves[0].Priority;
						foreach(var mov in sequence.Moves) {
							Board.TakeMove(clonedTiles, mov);
						}
						//now we need to call search the move tree for the value of the sequence.
						var sequenceValue = GetValueOfMoveSequence(this.MaxDepth-1, clonedTiles, prio, enemyPlayer, currentPlayer, false);

						var result = new MinimaxResult(sequence.Moves, sequence.Turn, sequenceValue);
						lock(_resultAddLock)
							results.Add(result);
					}
				}
			});

#if DEBUG
			Console.WriteLine($"amount of checkups: {this._amountOfCheckups}");
#endif
			return ChooseBestResult(results);
		}
		private readonly object _resultAddLock = new object();
		private MinimaxResult ChooseBestResult(List<MinimaxResult> results)
		{
#if DEBUG
			var str = "";
			foreach(var result in results)
				str += $"move: {result.Moves.First().StartLocation} => {result.Moves.Last().EndLocation}, value:{result.TurnValue}\n";
			Console.WriteLine($"Turn: {this.Game.Turn}");
			Console.WriteLine(str);
#endif
			if(results.Count() == 0)
				return null;

			IEnumerable<MinimaxResult> bestResults = results.Where(res => res.TurnValue == results.Max(r => r.TurnValue));
			//  : results.Where(res => res.TurnValue == results.Min(r => r.TurnValue));

			var random = new Random();
			var ind = random.Next(0, bestResults.Count());
			var chosenResult = bestResults.ElementAtOrDefault(ind);
			return chosenResult;
		}

		/// <summary>
		/// Get all possible ways the given move can be played out
		/// </summary>
		/// <param name="turn">turn which the move is part of</param>
		/// <param name="lastMove">move to get all possible sequences for</param>
		private List<MoveSequence> GetPossibleMoveSequences(Turn turn, AttackMove lastMove) => GetPossibleMoveSequences(turn, lastMove, new List<Move>());

		/// <summary>
		/// Get all possible ways the given move can be played out
		/// </summary>
		/// <param name="turn">turn which the move is part of</param>
		/// <param name="lastMove">move to get all possible sequences for</param>
		/// <param name="doneMoves">done moves in the sequence</param>
		/// <returns></returns>
		private List<MoveSequence> GetPossibleMoveSequences(Turn turn, AttackMove lastMove, List<Move> doneMoves)
		{
			var sequences = new List<MoveSequence>();
			//FIXME: Sometimes if theres a posibility for a doublejump it only returns a single jump, and forgets the second jump. why does this happen? fix this. This is the reason the AIBase code crashes in the while(!Game.Taketurn(...))
			doneMoves.Add(lastMove);
			foreach(var move in lastMove.FurtherMoves) {
				sequences.AddRange(GetPossibleMoveSequences(turn, move, new List<Move>(doneMoves)));
			}
			if(lastMove.FurtherMoves.Count == 0)
				sequences.Add(new MoveSequence(turn, doneMoves));
			return sequences;
		}

		public MinimaxMultiThreaded(int maxDepth, IGame game)
		{
			this.MaxDepth = maxDepth;
			this.Game = game;
		}
	}
}