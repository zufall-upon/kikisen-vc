using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kikisen_VC_WPF.OCR
{
	/// <summary>
	/// Window1.xaml の相互作用ロジック
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1() {
			InitializeComponent();
		}
		private bool leftbtnclicked = false;
		private double _downedX = 0;
		private double _downedY = 0;
		private double _rectWidth = 0;
		private double _rectHeight = 0;

		public double DownedX { get => _downedX; set => _downedX = value; }
		public double DownedY { get => _downedY; set => _downedY = value; }
		public double RectWidth { get => _rectWidth; set => _rectWidth = value; }
		public double RectHeight { get => _rectHeight; set => _rectHeight = value; }

		private void ContentPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			leftbtnclicked = true;
			// 矩形初期値設定
			Point p = e.GetPosition(this);
			_downedX = p.X;
			_downedY = p.Y;
			rectangle1.Margin = new Thickness(p.X, p.Y, 0, 0);
			rectangle1.Width = _rectWidth;
			rectangle1.Height = _rectHeight;
			rectangle1.Visibility = Visibility.Visible;
		}

		private void ContentPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			_rectWidth = rectangle1.Width;
			_rectHeight = rectangle1.Height;
			// 矩形非表示
			rectangle1.Width = 0;
			rectangle1.Height = 0;
			rectangle1.Visibility = Visibility.Hidden;
			leftbtnclicked = false;
			this.Close();
		}

		private void ContentPanel_MouseMove(object sender, MouseEventArgs e) {
			// 移動中矩形サイズ
			if (rectangle1.Visibility == Visibility.Visible && leftbtnclicked) {
				Point p = e.GetPosition(this);
				if (p.X - _downedX >= 0 && p.Y - _downedY >= 0) {
					// マウスダウン座標から見て右下
					rectangle1.Margin = new Thickness(_downedX, _downedY, 0, 0);
					rectangle1.Width = p.X - _downedX;
					rectangle1.Height = p.Y - _downedY;
				} else if (p.X - _downedX < 0 && p.Y - _downedY >= 0) {
					// マウスダウン座標から見て左下
					rectangle1.Margin = new Thickness(p.X, _downedY, 0, 0);
					rectangle1.Width = _downedX - p.X;
					rectangle1.Height = p.Y - _downedY;
				} else if (p.X - _downedX >= 0 && p.Y - _downedY < 0) {
					// マウスダウン座標から見て右上
					rectangle1.Margin = new Thickness(_downedX, p.Y, 0, 0);
					rectangle1.Width = p.X - _downedX;
					rectangle1.Height = _downedY - p.Y;
				} else {
					// マウスダウン座標から見て左上
					rectangle1.Margin = new Thickness(p.X, p.Y, 0, 0);
					rectangle1.Width = Math.Abs(_downedX - p.X);
					rectangle1.Height = Math.Abs(_downedY - p.Y);
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			if (_downedX != 0 && _downedY != 0) {
				rectangle1.Visibility = Visibility.Visible;
				rectangle1.Margin = new Thickness(_downedX, _downedY, _rectWidth, _rectHeight);
				rectangle1.Width = _rectWidth;
				rectangle1.Height = _rectHeight;
				rectangle1.Visibility = Visibility.Visible;
			}
		}
	}
}
