using System;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Kikisen_VC_WPF.OCR
{
	public static class BitmapEx
	{
		/// <summary>
		/// Bitmap から BitmapSource へ変換
		/// </summary>
		/// <param name="bmp"></param>
		/// <returns></returns>
		public static System.Windows.Media.ImageSource ToImageSource(this Bitmap bmp) {
			BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
				(
					bmp.GetHbitmap(),
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions()
				);
			return bitmapSource;
		}
	}
}
