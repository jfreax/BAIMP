using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLRLM : BaseAlgorithm
	{
		public GLRLM(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TScan), new MaximumUses(1)));

			output.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan tScan = inputArgs[0] as TScan;
			Bitmap bitmap = tScan.GrayScale8bpp;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );

			int height = data.Height;
			int width = data.Width;
			int stride = data.Stride;
			float[,] matrix = new float[256, width+1];

			byte* src = (byte*) (data.Scan0);

			int maxRunLength = 0;

			for (int y = 0; y < height; y++) {
				int runLength = 1;
				for (int x = 1; x < width; x++) {
					byte a = src[stride * y + (x - 1)];
					byte b = src[stride * y + x];

					if (a == b)
						runLength++;
					else {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}

						runLength = 1;
					}

					if ((a == b) && (x == width - 1)) {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}
					}
					if ((a != b) && (x == width - 1)) {
						matrix[b, 1]++;
					}
				}
			}

			bitmap.UnlockBits(data);

			//if (crop) { // make this an option?
			if (maxRunLength < width+1) {
				float[,] matrixTmp = new float[256, maxRunLength+1];
				for (int y = 0; y < maxRunLength+1; y++ ) {
					for (int x = 0; x < 255; x++) {
						matrixTmp[x, y] = matrix[x, y];
					}
				}

				matrix = matrixTmp;
			}
			//}


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

		public override string ToString()
		{
			return "GLRLM - Gray Level Run Length Matrix";
		}

		public override string Headline()
		{
			return "GLRLM";
		}

		public override string ShortName()
		{
			return "glrm";
		}

		#endregion
	}
}

