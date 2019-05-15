using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using FunctionalLayer;

namespace Dammen.UC
{
	/// <summary>
	/// Interaction logic for UCChecker.xaml
	/// </summary>
	public partial class UCChecker : UserControl, INotifyPropertyChanged
	{
		public IChecker Checker { get => (IChecker)GetValue(CheckerProperty); set { SetValue(CheckerProperty, value); NotifyPropertyChanged(); } }

		public static readonly DependencyProperty CheckerProperty = DependencyProperty.Register("Checker", typeof(IChecker), typeof(UCChecker));

		public UCChecker()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}