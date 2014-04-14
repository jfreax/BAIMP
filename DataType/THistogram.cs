using System;
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

