using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class Tamura : BaseAlgorithm
	{
		public Tamura(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("Tamura Features", typeof(TFeatureList<double>)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			Bitmap bitmap = tbitmap.Data;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );

			int height = data.Height;
			int width = data.Width;

			double coarsness = 0.0;

			for (int y = 0; y < height; y++) {
				for (int x = 1; x < width; x++) {
					coarsness += Math.Pow(2, Sopt(data, x, y));
				}
			}

			coarsness /= height * width;

			bitmap.UnlockBits(data);

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("coarsness", coarsness)
			};
		}

		public double Sopt(BitmapData data, int x, int y)
		{
			double result = 0;
			int kOpt = 1;

			for (int k = 0; k < 3; k++) {
				double E_k = Math.Max(E_h(data, x, y, k), E_v(data, x, y, k));
				if (result < E_k) {
					kOpt = k;
					result = E_k;
				}
			}
			return kOpt;
		}

		double E_h(BitmapData data, int x, int y, int k)
		{
			return Math.Abs(
				AverageNeighborhoods(data, x + (int) Math.Pow(2, k - 1), y, k) -
				AverageNeighborhoods(data, x - (int) Math.Pow(2, k - 1), y, k)
			);
		}

		double E_v(BitmapData data, int x, int y, int k)
		{
			return Math.Abs(
				AverageNeighborhoods(data, x, y + (int) Math.Pow(2, k - 1), k) -
				AverageNeighborhoods(data, x, y - (int) Math.Pow(2, k - 1), k)
			);
		}

		unsafe double AverageNeighborhoods(BitmapData data, int x, int y, int k)
		{
			double result = 0, border;
			border = Math.Pow(2, 2 * k);
			int x0 = 0, y0 = 0;

			int p = (int) Math.Pow(2, k - 1);

			byte* src = (byte*) (data.Scan0);
			int stride = data.Stride;

			for (int i = 0; i < border; i++) {
				for (int j = 0; j < border; j++) {
					x0 = x - p + i;
					y0 = y - p + j;

					if (x0 < 0)
						x0 = 0;
					if (y0 < 0)
						y0 = 0;
					if (x0 >= data.Width)
						x0 = data.Width - 1;
					if (y0 >= data.Height)
						y0 = data.Width - 1;
					result += src[(stride * y0) + x0];
				}
			}

			return result / Math.Pow(2, 2 * k);
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Tamura Features";
			}
		}

		#endregion
	}
}

