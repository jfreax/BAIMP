//
//  THistogram.cs
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
using System.Linq;
using Xwt.Drawing;
using Xwt;

namespace Baimp
{
	public class THistogram : BaseType<double[]>
	{
		public THistogram() : base()
		{
		}

		public THistogram(double[] histogram) : base(histogram)
		{
		}

		public override string ToString()
		{
			string ret = string.Empty;
			foreach (double v in Data) {
				ret += v + ", ";
			}

			return ret;
		}

		#region implemented abstract members of BaseType

		public override Widget ToWidget()
		{
			if (widget == null) {
				int histHeight = Data.Length * 3 / 4;
				double[] scaledData = Data.Scale(0, histHeight);
				ImageBuilder ib = new ImageBuilder(scaledData.Length, histHeight);

				int i = 0;
				foreach (double v in scaledData) {
					ib.Context.MoveTo(i, histHeight);
					ib.Context.LineTo(i, (int)(histHeight-(int)v));
					i++;
				}
				ib.Context.Stroke();

				widget = new ImageView(ib.ToBitmap().WithBoxSize(MaxWidgetSize));
				ib.Dispose();
			}

			return widget;
		}

		#endregion
	}
}

