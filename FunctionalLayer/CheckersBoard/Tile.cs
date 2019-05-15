using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using FunctionalLayer.Util;

namespace FunctionalLayer.CheckersBoard
{
	/// <summary>
	/// This class contains the logic for a tile, and contains all data needed to show a tile.
	/// </summary>
	public class Tile : INotifyPropertyChanged, ITile, ICloneable
	{
		private TileCoordinate _coordinate;
		public TileCoordinate Coordinate { get => this._coordinate; set { this._coordinate = value; NotifyPropertyChanged(); } }

		/// <summary>
		/// The brush used to color the tile
		/// </summary>
		public Brush TileBrush => (this.TileColor == TileColor.Dark) ? new SolidColorBrush(Colors.Brown) : new SolidColorBrush(Colors.LightGray);

		private TileColor _tileColor;

		public TileColor TileColor {
			get => this._tileColor;
			set {
				this._tileColor = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(this.TileBrush));
			}
		}

		private IChecker _checker;

		public IChecker Checker {
			get => this._checker;
			set {
				this._checker = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public object Clone()
		{
			var t = new Tile(this.TileColor, this.Coordinate) {
				Checker = Checker?.Clone() as IChecker
			};

			return t;
		}

		public Tile(TileColor color, TileCoordinate coordinate)
		{
			this.Coordinate = coordinate;
			this.TileColor = color;
		}
	}
}