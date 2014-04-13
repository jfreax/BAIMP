﻿using Xwt;
using Xwt.Drawing;
using System.Linq;
using System;

namespace Baimp
{
	public class TMatrix : BaseType<double[,]>
	{
		public TMatrix() : base()
		{
		}

		public TMatrix(double[,] matrix) : base(matrix)
		{
		}

		public override string ToString()
		{
			return String.Format("{0}x{1} Matrix", Data.GetLength(0), Data.GetLength(1));
		}

		#region implemented abstract members of BaseType

		public override Widget ToWidget()
		{
			if (widget == null) {
				if (Data.GetLength(0) <= 16 && Data.GetLength(1) <= 16) {
					Table t = new Table();

					for (int x = 0; x < Data.GetLength(0); x++) {
						for (int y = 0; y < Data.GetLength(1); y++) {
							t.Add(new Label(Data[x, y].ToString()), x, y);
						}
					}

					widget = t;
				} else {

					ImageBuilder ib = new ImageBuilder(Data.GetLength(0), Data.GetLength(1));
					BitmapImage bi = ib.ToBitmap();

					double max = 0.0;
					double[,] copy = Data.Scale(1.0, 65536.0);

					for (int x = 0; x < Data.GetLength(0); x++) {
						for (int y = 0; y < Data.GetLength(1); y++) {
							if (copy[x, y] > 0) {
								copy[x, y] = (Math.Log(copy[x, y]));
							}

							if (copy[x, y] > max) {
								max = copy[x, y];
							}
						}
					}
												
					for (int x = 0; x < Data.GetLength(0); x++) {
						for (int y = 0; y < Data.GetLength(1); y++) {
							byte c = (byte) ((copy[x, y] * 255) / max);
							bi.SetPixel(x, y, Color.FromBytes(c, c, c));
						}
					}

					ib.Dispose();
					widget = new ImageView(bi.WithBoxSize(MaxWidgetSize));
				}
			}

			return widget;
		}

		#endregion

		#region Properties

		#endregion
	}
}

