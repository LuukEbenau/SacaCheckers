using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dammen.UC
{
	/// <summary>
	/// Interaction logic for UCArrow.xaml
	/// </summary>
	public partial class UCArrow : UserControl
	{
		public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(UCArrow));
		public double X1 { get; set; }
		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(UCArrow));
		public double Y1 { get; set; }

		public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(UCArrow));
		public double X2 { get; set; }
		public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(UCArrow));
		public double Y2 { get; set; }

		public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(UCArrow));
		public Brush Stroke { get; set; }

		public double LeftPointX { get; }
		public double LeftPointY { get; }

		public double Degrees {
			get {
				var xAdded = this.X2 - this.X1;
				var yAdded = this.Y2 - this.Y1;
				var tan = Math.Tan(xAdded / yAdded);
				var tanh = Math.Tanh(tan);
				return tanh;
			}
		}

		private int arrowCornerLength = 20;

		public double RightPointY { get; }

		public UCArrow()
		{
			InitializeComponent();
		}
	}
}