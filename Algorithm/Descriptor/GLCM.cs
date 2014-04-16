using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLCM : BaseAlgorithm
	{
		public GLCM(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TScan), new MaximumUses(1)));

			output.Add(new Compatible("Co-occurence matrix", typeof(TMatrix)));

			options.Add(new Option("Bpp", 2, 32, 8));
			options.Add(new Option("X Offest", 0, 10, 1));
			options.Add(new Option("Y Offest", 0, 10, 1));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TScan scan = inputArgs[0] as TScan;
			int bpp = (int) options[0].Value;
			int dx = (int) options[1].Value;
			int dy = (int) options[2].Value;
			int p = (int) Math.Pow(2, bpp);

			SparseMatrix<double> matrix = new SparseMatrix<double>(p + 1, p + 1);
			SparseMatrix<double> normalizedMatrix = new SparseMatrix<double>(p + 1, p + 1);

			int width = (int) scan.Size.Width;
			int height = (int) scan.Size.Height;

			int startX = Math.Max(0, -dx);
			int startY = Math.Max(0, -dy);

			int endX = width - Math.Max(0, dx);
			int endY = height - Math.Max(0, dy);

			int pairs = 0;
			int offset = Math.Max(0, dx);

			float[] data = scan.Data;
			float max = data.Max();
			unsafe {
				fixed (float* ptrData = data) {
					float* src = ptrData;

					int oldProgress = 0;
					for (int y = startY; y < endY; y++) {
						for (int x = startX; x < endX; x++, src++) {
							int posWithOffset = ((y + dy) * width) + (x + dx);

							int fromValue = (int) ((*src / max) * p);
							int toValue = (int) ((data[posWithOffset] / max) * p);
							try {
								matrix[fromValue, toValue]++;
							} catch (Exception e) {
								Console.WriteLine(e);
							}

							pairs++;
						}
						src += offset;

						int progress = (int) (y * 100.0) / height;
						if (progress - oldProgress > 10) {
							oldProgress = progress;
							SetProgress(progress);
						}
					}
				}
			}

			// normalize
			if (pairs > 0) {
				double max2 = 0;
				foreach (int row in matrix.GetRows()) {
					foreach (KeyValuePair<int, double> v in matrix.GetRowData(row)) {
						normalizedMatrix[row, v.Key] = v.Value / pairs;
						if (v.Value / pairs > max2) {
							max2 = v.Value / pairs;
						}
					}
				}
			}

			matrix.Dispose();

			IType[] ret = { new TMatrix(normalizedMatrix) };
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

		public override string Headline()
		{
			return string.Format("GLCM d=({1}, {2}) {0}bpp", options[0].Value, options[1].Value, options[2].Value);
		}

		public override string ToString()
		{
			return "GLCM - Gray Level Co-occurence Matrix";
		}

		#endregion
	}
}

