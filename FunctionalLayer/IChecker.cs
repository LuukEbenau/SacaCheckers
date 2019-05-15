using System;
using System.Collections.Generic;
using System.Windows.Media;

using FunctionalLayer.CheckersBoard;
using FunctionalLayer.GameTurn;

namespace FunctionalLayer
{
	public interface IChecker:ICloneable
	{
		Brush CheckerImage { get; }

		//TileCoordinate Coordinate { get; set; }
		PlayerNumber Owner { get; }
		PlayerColor Color { get; set; }
		CheckerType Type { get; }
		int Id { get; }
		Turn CreateTurn(IGame game, BoardTileCollection tiles, IPlayer currentPlayer, IPlayer enemyPlayer);

		void PromoteToKing();

		Dictionary<MovementType, List<TileCoordinate>> GetPotentionalMoveCoordinates(IGame game, BoardTileCollection tiles, IPlayer enemyPlayer, TileCoordinate startingCoordinate, MovementType movementType);

		bool IsInEnemyBaseRow(BoardTileCollection tiles);
	}
}