using System;
using System.Windows.Media;

namespace FunctionalLayer.CheckersBoard
{
	public interface ITile : ICloneable
	{
		IChecker Checker { get; set; }
		Brush TileBrush { get; }
		TileCoordinate Coordinate { get; set; }
		TileColor TileColor { get; }
	}
}