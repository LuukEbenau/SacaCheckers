using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using FunctionalLayer.GameTurn;

namespace Dammen.UC
{
	/// <summary>
	/// Interaction logic for UCGameHistory.xaml
	/// </summary>
	public partial class UCGameHistory : UserControl
	{
		public ObservableCollection<DoneTurn> TurnHistory { get => (ObservableCollection<DoneTurn>)GetValue(TurnHistoryProperty); set => SetValue(TurnHistoryProperty, value); }

		public static readonly DependencyProperty TurnHistoryProperty =
			DependencyProperty.Register(
			"TurnHistory", typeof(ObservableCollection<DoneTurn>),
			typeof(UCGameHistory)
		);

		public UCGameHistory()
		{
			InitializeComponent();
		}
		public event EventHandler<GameHistoryTurnHoverEventArgs> TurnHoverEnter;
		public event EventHandler<EventArgs> TurnHoverLeave;

		public event EventHandler<GameHistoryEventArgs> MoveHoverEnter;
		public event EventHandler<EventArgs> MoveHoverLeave;

		private DoneTurn _hoveredDoneTurn;

		/// <summary>
		/// When mouse enters a specific move in the doneturn
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Row_MouseEnter(object sender, MouseEventArgs e)
		{
			var row = sender as Grid;
			row.MouseLeave += Row_MouseLeave;
			row.MouseMove += Row_MouseMove;
		}

		private void Row_MouseMove(object sender, MouseEventArgs e)
		{
			var row = sender as Grid;
			//is needed since sometimes it gets executed before gameturn_mouse-enter, so the hovereddoneturn is still null

			if(this._hoveredDoneTurn != null) {
				Move hoveredMove = row.DataContext as Move;
				row.MouseMove -= Row_MouseMove;
				MoveHoverEnter?.Invoke(row, new GameHistoryEventArgs(this._hoveredDoneTurn, hoveredMove));
			}
		}

		private void Row_MouseLeave(object sender, MouseEventArgs e)
		{
			var row = sender as Grid;
			row.MouseLeave -= Row_MouseLeave;
			row.MouseMove -= Row_MouseMove;
			MoveHoverLeave?.Invoke(row, new EventArgs());
		}

		/// <summary>
		/// When the mouse enters the doneturn block
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DoneTurn_MouseEnter(object sender, MouseEventArgs e)
		{
			var doneTurn = sender as Grid;
			doneTurn.MouseLeave += DoneTurn_MouseLeave;
			this._hoveredDoneTurn = doneTurn.DataContext as DoneTurn;
			TurnHoverEnter?.Invoke(doneTurn, new GameHistoryTurnHoverEventArgs(this._hoveredDoneTurn));
		}

		private void DoneTurn_MouseLeave(object sender, MouseEventArgs e)
		{
			var doneTurn = sender as Grid;
			doneTurn.MouseLeave -= DoneTurn_MouseLeave;
			this._hoveredDoneTurn = null;
			TurnHoverLeave?.Invoke(doneTurn, new EventArgs());
		}
	}
	public class GameHistoryTurnHoverEventArgs : EventArgs
	{
		public DoneTurn DoneTurn { get; private set; }

		public GameHistoryTurnHoverEventArgs(DoneTurn doneTurn)
		{
			this.DoneTurn = doneTurn;
		}
	}
	public class GameHistoryEventArgs : EventArgs
	{
		public DoneTurn DoneTurn { get; private set; }
		public Move SelectedMove { get; private set; }

		public GameHistoryEventArgs(DoneTurn doneTurn, Move selectedMove)
		{
			this.DoneTurn = doneTurn;
			this.SelectedMove = selectedMove;
		}
	}
}