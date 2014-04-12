using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class GLRLM : BaseAlgorithm
	{
		public GLRLM(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(System.Collections.Generic.Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
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
			int stride = data.Stride;
			double[,] matrix = new double[256, width+1];

			byte* src = (byte*) (data.Scan0);

			for (int y = 0; y < height; y++) {
				int runLength = 1;
				for (int x = 1; x < width; x++) {
					byte a = src[stride * y + (x - 1)];
					byte b = src[stride * y + x];

					if (a == b)
						runLength++;
					else {
						matrix[a, runLength]++;
						runLength = 1;
					}

					if ((a == b) && (x == width - 1)) {
						matrix[a, runLength]++;
					}
					if ((a != b) && (x == width - 1)) {
						matrix[b, 1]++;
					}
				}
			}

			bitmap.UnlockBits(data);


			IType[] ret = { new TMatrix(matrix) };
			return ret;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "Gray Level Run Length";
			}
		}

		#endregion
	}
}

