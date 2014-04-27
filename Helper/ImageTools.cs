//
//  ImageTools.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
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
							Color color = HSVtoRGB((double) *src / 1.776, 0.9, 0.9, 1.0);
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

		public static Color ColorFromHSV(double hue, double saturation, double value)
		{
			int hi = (int) (Math.Floor(hue / 60)) % 6;
			double f = hue / 60 - Math.Floor(hue / 60);  
			value = value * 255;
			int v = (int) (value);
			int p = (int) (value * (1 - saturation));
			int q = (int) (value * (1 - f * saturation));
			int t = (int) (value * (1 - (1 - f) * saturation));

			if (hi == 0)
				return Color.FromArgb(255, (byte) v, (byte) t, (byte) p);
			else if (hi == 1)
				return Color.FromArgb(255, (byte) q, (byte) v, (byte) p);
			else if (hi == 2)
				return Color.FromArgb(255, (byte) p, (byte) v, (byte) t);
			else if (hi == 3)
				return Color.FromArgb(255, (byte) p, (byte) q, (byte) v);
			else if (hi == 4)
				return Color.FromArgb(255, (byte) t, (byte) p, (byte) v);
			else
				return Color.FromArgb(255, (byte) v, (byte) p, (byte) q);
		}

		#endregion
	}
}

