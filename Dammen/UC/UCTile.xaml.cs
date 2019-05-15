using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using FunctionalLayer.CheckersBoard;

namespace Dammen.UC
{
	/// <summary>
	/// Interaction logic for UCTile.xaml
	/// </summary>
	public partial class UCTile : UserControl, INotifyPropertyChanged
	{
		#region properties

		public static readonly DependencyProperty TileProperty = DependencyProperty.Register("Tile", typeof(ITile), typeof(UCTile));

		public ITile Tile {
			get => (ITile)GetValue(TileProperty);
			set => SetValue(TileProperty, value);
		}

		public static readonly DependencyProperty TileStatusProperty = DependencyProperty.Register("TileStatus", typeof(TileStatus), typeof(UCTile));

		private TileStatus _previewTileStatus;

		public TileStatus PreviewTileStatus {
			get => this._previewTileStatus;
			set { this._previewTileStatus = value; NotifyPropertyChanged(); }
		}

		public TileStatus TileStatus {
			get => (TileStatus)GetValue(TileStatusProperty);
			set { SetValue(TileStatusProperty, value); NotifyPropertyChanged(); }
		}

		#endregion properties

		public UCTile()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void UCChecker_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UCChecker selectedChecker = (UCChecker)sender;
			if(selectedChecker.Checker == null)
				return;
			selectedChecker.MouseMove += SelectedChecker_MouseMove;
			selectedChecker.MouseLeave += SelectedChecker_MouseLeave;
		}

		private void SelectedChecker_MouseLeave(object sender, MouseEventArgs e)
		{
			UCChecker selectedChecker = (UCChecker)sender;
			selectedChecker.MouseLeave -= SelectedChecker_MouseLeave;
			selectedChecker.MouseMove -= SelectedChecker_MouseMove;
		}

		public static readonly RoutedEvent DragDropStartedEvent = EventManager.RegisterRoutedEvent("DragDropStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UCTile));
		public event RoutedEventHandler DragDropStarted {
			add => AddHandler(DragDropStartedEvent, value);
			remove => RemoveHandler(DragDropStartedEvent, value);
		}

		public static readonly RoutedEvent DragDropFinishedEvent = EventManager.RegisterRoutedEvent("DragDropFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UCTile));

		public event RoutedEventHandler DragDropFinished {
			add => AddHandler(DragDropFinishedEvent, value);
			remove => RemoveHandler(DragDropFinishedEvent, value);
		}

		private void SelectedChecker_MouseMove(object sender, MouseEventArgs e)
		{
			var currentlyDraggedChecker = (UCChecker)sender;
			currentlyDraggedChecker.MouseMove -= SelectedChecker_MouseMove;

			var data = new DataObject(typeof(UCChecker), currentlyDraggedChecker);
			RaiseEvent(new RoutedEventArgs(DragDropStartedEvent,this));
			//DragDropStarted?.Invoke(this,);
			DragDrop.DoDragDrop(currentlyDraggedChecker, data, DragDropEffects.Move);
			RaiseEvent(new RoutedEventArgs(DragDropFinishedEvent, this));
			//DragDropFinished?.Invoke(this, new RoutedEventArgs());
		}
	}
}