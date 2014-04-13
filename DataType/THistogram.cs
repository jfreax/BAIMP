using System;
using System.Linq;
using Xwt.Drawing;
using Xwt;

namespace Baimp
{
	public class THistogram : BaseType<int[]>
	{
		public THistogram() : base()
		{
		}

		public THistogram(int[] histogram) : base(histogram)
		{
		}

		public override string ToString()
		{
			string ret = string.Empty;
			foreach (int v in Data) {
				ret += v + ", ";
			}

			return ret;
		}

		#region implemented abstract members of BaseType

		public override Widget ToWidget()
		{
			if (widget == null) {
				int histHeight = Data.Length * 3 / 4;
				int[] scaledData = Data.Scale(0, histHeight);
				ImageBuilder ib = new ImageBuilder(scaledData.Length, histHeight);

				int i = 0;
				foreach (int v in scaledData) {
					ib.Context.MoveTo(i, histHeight);
					ib.Context.LineTo(i, histHeight-v);
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

