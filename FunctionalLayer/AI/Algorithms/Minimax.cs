using System;
using System.Collections.Generic;
using System.Linq;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace FunctionalLayer.AI.Algorithms
{
	public class Minimax : IGameAI
	{
		private IGame Game { get; set; }
		public int MaxDepth { get; private set; }

		protected float GetBestMoveSequence(int depth, BoardTileCollection tiles, float priority, IPlayer currentPlayer, IPlayer enemyPlayer) =>
			GetBestMoveSequence(depth, tiles, priority, currentPlayer, enemyPlayer, int.MaxValue, int.MinValue);

		protected float GetBestMoveSequence(int depth, BoardTileCollection tiles, float priority, IPlayer currentPlayer, IPlayer enemyPlayer, int alpha, int beta)
		{
			if(depth == 0) {
				return priority;
			}

			var checkerMovePairs = currentPlayer.GetAllPossibleMoves(Game, enemyPlayer, tiles);
			//Loop variables
			//stores all the generated move sequences, instantiated outside loop to prevent unneeded creating of variables
			List<MoveSequence> possibleSequences = null;
			foreach(var pair in checkerMovePairs) {
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

					foreach(var sequence in possibleSequences) {
						BoardTileCollection clonedTiles = (BoardTileCollection)tiles.Clone();
						int prio = sequence.Moves[0].Priority;
						foreach(var mov in sequence.Moves) {
							Board.TakeMove(clonedTiles, mov);
						}

						priority += prio;
						//now we need to call the same method from the enemy perception again
						// recursively collect all sequences for the followup moves
						var furtherMovesValue = GetBestMoveSequence(depth - 1, clonedTiles, -priority, enemyPlayer, currentPlayer, -beta, -alpha);
					}
				}
			}

			return priority;
		}

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
			IPlayer enemyPlayer = this.Game.GetEnemyPlayer(currentPlayer);
			var results = new List<MinimaxResult>();

			//get all possible initial moves for the player
			var checkerMovePairs = currentPlayer.GetAllPossibleMoves(Game, enemyPlayer, tiles);
			//loop variables
			List<MoveSequence> possibleSequences = null;
			foreach(var pair in checkerMovePairs) { //for each checker
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

					foreach(var sequence in possibleSequences) {
						BoardTileCollection clonedTiles = (BoardTileCollection)tiles.Clone();
						float prio = sequence.Moves[0].Priority;
						foreach(var mov in sequence.Moves) {
							Board.TakeMove(clonedTiles, mov);
						}

						//now we need to call the same method from the enemy perception again
						var sequenceValue = GetBestMoveSequence(this.MaxDepth, clonedTiles, prio, enemyPlayer, currentPlayer);

						var result = new MinimaxResult(sequence.Moves, sequence.Turn, sequenceValue);
						results.Add(result);
					}
				}
			}

#if DEBUG
			var str = "";
			foreach(var result in results)
				str += $"move: {result.Moves.First().StartLocation} => {result.Moves.Last().EndLocation}, value:{result.TurnValue}\n";
			Console.WriteLine($"Turn: {this.Game.Turn}");
			Console.WriteLine(str);
#endif

			if(results.Count() == 0)
				return null;
			IEnumerable<MinimaxResult> bestResults = ((this.MaxDepth & 0b1) == 0b1) ? results.Where(res => res.TurnValue == results.Max(r => r.TurnValue))
				: results.Where(res => res.TurnValue == results.Min(r => r.TurnValue));

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

		public Minimax(int maxDepth, IGame game)
		{
			this.MaxDepth = maxDepth;
			this.Game = game;
		}
	}
}