using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FunctionalLayer.GameTurn
{
	public class DoneTurn : INotifyPropertyChanged
	{
		public int TurnNumber { get; private set; }
		private ObservableCollection<Move> _doneMoves;
		public ObservableCollection<Move> DoneMoves { get => this._doneMoves; set { this._doneMoves = value; NotifyPropertyChanged(); } }
		public IPlayer Player { get; private set; }

		public DoneTurn(int turnNumber, IPlayer player)
		{
			this.TurnNumber = turnNumber;
			this.Player = player;
			this.DoneMoves = new ObservableCollection<Move>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}