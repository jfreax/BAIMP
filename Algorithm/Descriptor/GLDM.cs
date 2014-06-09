using System;
using System.Collections.Generic;

namespace Baimp
{
	public class GLDM : BaseAlgorithm
	{
		public GLDM(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan)));

			output.Add(new Compatible("Difference matrix", typeof(TMatrix)));

			options.Add(new Option("Bpp", 2, 32, 8));
			options.Add(new Option("X Offest", 0, 10, 1));
			options.Add(new Option("Y Offest", 0, 10, 1));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan scan = inputArgs[0] as TScan;
			int bpp = (int) options[0].Value;
			int dx = (int) options[1].Value;
			int dy = (int) options[2].Value;
			int p = (int) Math.Pow(2, bpp);

			TMatrix matrix;
			if (bpp > 10) {
				matrix = new TMatrix(new SparseMatrix<float>(p + 1, p + 1));
			} else {
				matrix = new TMatrix(new float[p + 1, p + 1]);
			}

			int width = (int) scan.Size.Width;
			int height = (int) scan.Size.Height;

			int startX = Math.Max(0, -dx);
			int startY = Math.Max(0, -dy);

			int endX = width - Math.Max(0, dx);
			int endY = height - Math.Max(0, dy);

//			float[] data = scan.Data;
//			double max = data.Max();
//
//			int offset = Math.Max(0, dx);
//			if (max > 0.0) {
//				unsafe {
//					fixed (float* ptrData = data) {
//						float* src = ptrData;
//
//						int oldProgress = 0;
//						for (int y = startY; y < endY; y++) {
//							for (int x = startX; x < endX; x++, src++) {
//								int posWithOffset = ((y + dy) * width) + (x + dx);
//
//								int fromValue = (int) ((*src / max) * p);
//								int toValue = (int) ((data[posWithOffset] / max) * p);
//								matrix[fromValue, toValue] += increment;
//
//							}
//							src += offset;
//
//							int progress = (int) (y * 100.0) / height;
//							if (progress - oldProgress > 10) {
//								oldProgress = progress;
//								SetProgress(progress);
//							}
//						}
//					}
//				}
//			}

			IType[] ret = { matrix };
			return ret;
		}

		public override string Headline()
		{
			return string.Format("GLDM d=({1}, {2}) {0}bpp", options[0].Value, options[1].Value, options[2].Value);
		}

		public override string ShortName()
		{
			return "gldm";
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "GLDM - Gray Level Difference Matrix";
			}
		}

		#endregion
	}
}

