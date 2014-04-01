﻿using Xwt;
using Xwt.Drawing;
using System.Linq;
using System;

namespace Baimp
{
	public class TMatrix : BaseType<int[,]>
	{
		public TMatrix() : base()
		{
		}

		public TMatrix(int[,] matrix) : base(matrix)
		{
		}

		public override string ToString()
		{
//			string ret = "";
//			for(int i = 0; i < Data.GetLength(0); i++) {
//				for(int j = 0; j < Data.GetLength(1); j++) {
//					ret += Data[i, j] + " ";
//				}
//				ret += "\n";
//			}
//			return ret;

			return System.String.Format("{0}x{1} Matrix", Data.GetLength(0), Data.GetLength(1));
		}

		#region implemented abstract members of BaseType

		public override Xwt.Widget ToWidget()
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
				}

				ImageBuilder ib = new ImageBuilder(Data.GetLength(0), Data.GetLength(1));
				BitmapImage bi = ib.ToBitmap();

				double max = 0.0;
				int[,] copy = new int[Data.GetLength(0), Data.GetLength(1)];
				for (int x = 0; x < Data.GetLength(0); x++) {
					for (int y = 0; y < Data.GetLength(1); y++) {
						copy[x, y] = Data[x, y];
						if (copy[x, y] > 0) {
							copy[x, y] = (int) (Math.Log(copy[x, y]) * 100.0);
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

			return widget;
		}

		#endregion
	}
}
