using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLCM : BaseAlgorithm
	{
		public GLCM(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("Co-occurence matrix", typeof(TMatrix)));

			options.Add(new Option("X Offest", 0, 10, 1));
			options.Add(new Option("Y Offest", 0, 10, 1));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			Bitmap bitmap = tbitmap.Data;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );
				
			int dx = (int) options[0].Value;
			int dy = (int) options[1].Value;

			double[,] matrix = new double[256, 256];
			int height = data.Height;
			int width = data.Width;
			int stride = data.Stride;

			int startX = Math.Max(0, -dx);
			int startY = Math.Max(0, -dy);

			int endX = width - Math.Max(0, dx);
			int endY = height - Math.Max(0, dy);

			int pairs = 0;

			int offset = stride - width;
			if (data.PixelFormat == PixelFormat.Format8bppIndexed) {
				byte* src = (byte*) (data.Scan0) + (startY * stride) + startX;
				byte* srcBegin = (byte*) (data.Scan0);

				int oldProgress = 0;
				for (int y = startY; y < endY; y++) {
					for (int x = startX; x < endX; x++, src++) {

						int posWithOffset = ((y + dy) * stride) + (x + dx);
						matrix[*src, srcBegin[posWithOffset]]++;

						pairs++;
					}
					src += offset + Math.Max(0, dx);

					int progress = (int) (y * 100.0) / height;
					if (progress - oldProgress > 10) {
						oldProgress = progress;
						SetProgress(progress);
					}
				}
			} else {
				int pixelSize = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;
				byte* src = (byte*) (data.Scan0);

				int oldProgress = 0;
				for (int j = startY; j < endY; j++) {
					for (int i = startX; i < endX; i++) {

						int pos = (j * stride) + i * pixelSize;
						float v = (float) (0.2125 * src[pos] + 0.7154 * src[pos + 1] + 0.0721 * src[pos + 2]);
						int posWithOffset = ((j + dy) * stride) + (i + dx) * pixelSize;

						matrix[(int) v, src[posWithOffset / 2]]++;

						pairs++;
					}
						
					int progress = (int) (j * 100.0) / height;
					if (progress - oldProgress > 10) {
						oldProgress = progress;
						SetProgress(progress);
					}
				}
			}

			bitmap.UnlockBits(data);

			// normalize
			if (pairs > 0) {
				fixed (double* ptrMatrix = matrix) {
					double* c = ptrMatrix;
					for (int i = 0; i < matrix.Length; i++, c++) {
						*c /= pairs;
					}
				}
			}

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
				return "GLCM - Gray Level Co-occurence Matrix";
			}
		}

		#endregion
	}
}

