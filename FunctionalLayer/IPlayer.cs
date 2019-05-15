using System;
using System.Collections.Generic;
using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace FunctionalLayer
{
	public interface IPlayer
	{
		IEnumerable<IChecker> GetPlayerOwnedCheckers(BoardTileCollection tiles);
		PlayerType PlayerType { get; }
		string Name { get; set; }
		PlayerColor PlayerColor { get; set; }

		MoveSequence GenerateRandomTurn(IGame game, IPlayer enemyPlayer, BoardTileCollection tiles);

		Dictionary<IChecker, Turn> GetAllPossibleMoves(IGame game, IPlayer enemyPlayer, BoardTileCollection tiles);

		PlayerNumber PlayerNumber { get; }
	}
}