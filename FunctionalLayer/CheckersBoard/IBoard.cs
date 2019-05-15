using System;
using System.Collections.Generic;

namespace FunctionalLayer.CheckersBoard
{
	public interface IBoard: ICloneable
	{
		BoardTileCollection Tiles { get; set; }
		void SubscribeToGameEvents(IGame game);
		void GenerateTiles();
		void InitializePlayers(IGame game, IPlayer player1, IPlayer player2);
		void ClearCheckers();
	}
}