//using FunctionalLayer.CheckersBoard;
//using FunctionalLayer.GameTurn;
//using FunctionalLayer.Util;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FunctionalLayer.AI
//{
//    public class MinimalEnemyDamageAI : AIBase
//    {
//        public MinimalEnemyDamageAI(IPlayer player) : base(player)
//        {
//        }

//        private IEnumerable<MoveSequencePriorityPair> GetPossibleMoveSequences(List<ITile> currentTiles, Turn turn, AttackMove lastMove, List<Move> doneMoves = null)
//        {
//            List<Move> movesThatAreDone;
//            if(doneMoves == null)
//                doneMoves = new List<Move>();
//            movesThatAreDone = doneMoves.Copy();

//            var sequences = new List<MoveSequencePriorityPair>();

//            movesThatAreDone.Add(lastMove);
//            foreach(var move in lastMove.FurtherMoves) {
//                sequences.AddRange(GetPossibleMoveSequences(tiles, turn, move, movesThatAreDone));
//            }

//            if(lastMove.FurtherMoves.Count() == 0) {
//                //chain complete.
//                int moveId = 0;
//                while(!deepCopiedGame.TakeTurn(turn, movesThatAreDone[moveId])) {
//                    moveId++;
//                }

//                var enemyMoves = Game.GetAllPossibleMovesForPlayer(Player, Player.Game.EnemyPlayer, currentTiles);
//                int highestPrioEnemyTurn = enemyMoves.Values.GetHighestPriorityTurn();

//                var sequence = new MoveSequencePriorityPair(turn, movesThatAreDone, highestPrioEnemyTurn);
//                //We need to reset board to original state
//                deepCopiedGame.Turn = Player.Game.Turn;
//                deepCopiedGame.Board.Tiles = Player.Game.Board.Tiles.Copy();
//                sequences.Add(sequence);
//            }
//            return sequences;
//        }

//        private MoveSequencePriorityPair SearchForMostEfficientTurn()
//        {
//            var deepCopiedGame = Player.Game.Copy();
//            //needed to reset after each calculation
//            var turns = Player.Game.CurrentPlayer.GetAllPossibleMoves(Player.Game.EnemyPlayer, deepCopiedGame.Board);

//            var moveSequences = new List<MoveSequencePriorityPair>();
//            foreach(var turnPair in turns) {
//                var turn = turnPair.Value;
//                foreach(var move in turn.Moves) {
//                    deepCopiedGame = Player.Game.Copy();//this somehow works sooo well

//                    if(move is AttackMove attMove) {
//                        moveSequences.AddRange(GetPossibleMoveSequences(deepCopiedGame, turn, attMove));
//                    }
//                    else if(move is WalkMove) {
//                        deepCopiedGame.TakeTurn(turn, move);
//                        int highestPrioEnemyTurn = Game.GetAllPossibleMovesForPlayer(deepCopiedGame.CurrentPlayer, deepCopiedGame.EnemyPlayer, deepCopiedGame.Board.Tiles.ToList()).Values.GetHighestPriorityTurn();

//                        moveSequences.Add(new MoveSequencePriorityPair(turn, new List<Move> { move }, highestPrioEnemyTurn));
//                    }
//                }
//            }

//            var bestMoveSequences = moveSequences.Where(s => s.EnemyPriority == moveSequences.Min(ms => ms.EnemyPriority));

//            var random = new Random();
//            int randomNumber = random.Next(0, bestMoveSequences.Count());
//            var chosenSequence = bestMoveSequences.ElementAtOrDefault(randomNumber);
//            return chosenSequence;
//        }

//        public override MoveSequencePriorityPair GenerateMovesForTurn()
//        {
//            return SearchForMostEfficientTurn();
//        }
//    }
//}