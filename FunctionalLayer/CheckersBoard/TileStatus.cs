using System;

namespace FunctionalLayer.CheckersBoard
{
	[Flags]
	public enum TileStatus
	{
		Normal = 0,
		MoveStartLocation = 0b1,
		Attackable = 0b10,
		AttackedTile = 0b100,
		Move = 0b1000,
		PossibleMove = 0b10000,
		TileHistoryUsedTiles = MoveStartLocation | Attackable | AttackedTile | Move,
		MoveHighlightingUsedTiles = AttackedTile | Attackable | Move//|PossibleMove
	}
}