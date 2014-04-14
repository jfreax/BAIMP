using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public static class ImageTools
	{
		#region format conversion

		public unsafe static Bitmap ToColor(this Bitmap bitmap)
		{
			if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) {
				Bitmap colorBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);

				BitmapData data = bitmap.LockBits(
					                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					                  ImageLockMode.ReadOnly,
					                  bitmap.PixelFormat
				                  );

				BitmapData colorData = colorBitmap.LockBits(
					                       new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height),
					                       ImageLockMode.WriteOnly,
					                       colorBitmap.PixelFormat
				                       );

				try {
					int width = data.Width;
					int height = data.Height;
					int offset = data.Stride - width;

					byte* src = (byte*) data.Scan0;
					byte* srcData = (byte*) colorData.Scan0;

					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++, src++, srcData++) {
							Color color = HSVtoRGB((double)*src / 1.776, 0.9, 0.9, 1.0);
							*srcData = color.B;
							srcData++;
							*srcData = color.G;
							srcData++;
							*srcData = color.R;
						}

						src += offset;
						srcData += offset * 3;
					}
						
				} finally {
					bitmap.UnlockBits(data);
					colorBitmap.UnlockBits(colorData);
				}

				return colorBitmap;
			}

			return new Bitmap(bitmap);
		}

		#endregion

		#region Color conversion

		public static Color HSVtoRGB(double hue, double saturation, double value, double alpha)
		{
			double Min;
			double Chroma;
			double Hdash;
			double X;

			double r = 0, g = 0, b = 0;

			Chroma = saturation * value;
			Hdash = hue / 60.0;
			X = Chroma * (1.0 - Math.Abs((Hdash % 2.0) - 1.0));

			if (Hdash < 1.0) {
				r = Chroma;
				g = X;
			} else if (Hdash < 2.0) {
				r = X;
				g = Chroma;
			} else if (Hdash < 3.0) {
				g = Chroma;
				b = X;
			} else if (Hdash < 4.0) {
				g = X;
				b = Chroma;
			} else if (Hdash < 5.0) {
				r = X;
				b = Chroma;
			} else if (Hdash <= 6.0) {
				r = Chroma;
				b = X;
			}

			Min = value - Chroma;

			r += Min;
			g += Min;
			b += Min;

			return Color.FromArgb(
				(int) (alpha * 255), 
				(int) ((r + Min) * 255), 
				(int) ((g + Min) * 255), 
				(int) ((b + Min) * 255));
		}

		#endregion
	}
}

