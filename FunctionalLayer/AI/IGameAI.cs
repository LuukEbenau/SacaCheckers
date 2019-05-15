using FunctionalLayer.CheckersBoard;

namespace FunctionalLayer.AI
{
	/// <summary>
	/// contract for ai's
	/// </summary>
	public interface IGameAI
	{
		MinimaxResult GetBestTurn(BoardTileCollection tiles, IPlayer currentPlayer);
	}
}