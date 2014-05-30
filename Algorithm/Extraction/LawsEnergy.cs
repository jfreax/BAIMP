using System;
using System.Collections.Generic;

namespace Baimp
{
	public class LawsEnergy : BaseAlgorithm
	{
		static readonly int[] l5 = { 1,  4, 6,  4,  1};
		static readonly int[] e5 = {-1, -2, 0,  2,  1};
		static readonly int[] s5 = {-1,  0, 2,  0, -1};
		static readonly int[] w5 = {-1,  2, 0, -2,  1};
		static readonly int[] r5 = { 1, -4, 6, -4,  1};

		static readonly int[,] l5e5 = l5.Cross(e5);
		static readonly int[,] l5s5 = l5.Cross(s5);
		static readonly int[,] l5w5 = l5.Cross(w5);
		static readonly int[,] l5r5 = l5.Cross(r5);

		static readonly int[,] e5l5 = e5.Cross(l5);
		static readonly int[,] e5e5 = e5.Cross(e5);
		static readonly int[,] e5s5 = e5.Cross(s5);
		static readonly int[,] e5w5 = e5.Cross(w5);
		static readonly int[,] e5r5 = e5.Cross(r5);

		static readonly int[,] s5l5 = s5.Cross(l5);
		static readonly int[,] s5e5 = s5.Cross(e5);
		static readonly int[,] s5s5 = s5.Cross(s5);
		static readonly int[,] s5w5 = s5.Cross(w5);
		static readonly int[,] s5r5 = s5.Cross(r5);

		static readonly int[,] w5l5 = w5.Cross(l5);
		static readonly int[,] w5e5 = w5.Cross(e5);
		static readonly int[,] w5s5 = w5.Cross(s5);
		static readonly int[,] w5r5 = w5.Cross(r5);

		static readonly int[,] r5l5 = r5.Cross(l5);
		static readonly int[,] r5e5 = r5.Cross(e5);
		static readonly int[,] r5s5 = r5.Cross(s5);
		static readonly int[,] r5w5 = r5.Cross(w5);
		static readonly int[,] r5r5 = r5.Cross(r5);

		public LawsEnergy(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image (Windowed)", typeof(TScan)));

			output.Add(new Compatible("Laws Energy Features", typeof(TFeatureList<double>)));

			options.Add(new OptionBool("Normalize", true));
		}

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan tScan = inputArgs[0] as TScan;
			bool normalize = (bool) options[0].Value;

			float[] data = tScan.Data;

			int width = tScan.Width;
			int height = tScan.Height;

			if (normalize) {
				float mean = data.Mean();
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						data[(y * width) + x] /= mean;
					}
				}
			}

			float[,] l5e5d = new float[(width - 3), (height - 3)];
			float[,] l5s5d = new float[(width - 3), (height - 3)];
			float[,] l5r5d = new float[(width - 3), (height - 3)];
			float[,] e5e5d = new float[(width - 3), (height - 3)];
			float[,] e5s5d = new float[(width - 3), (height - 3)];
			float[,] e5r5d = new float[(width - 3), (height - 3)];
			float[,] e5l5d = new float[(width - 3), (height - 3)];
			float[,] s5l5d = new float[(width - 3), (height - 3)];
			float[,] s5e5d = new float[(width - 3), (height - 3)];
			float[,] s5r5d = new float[(width - 3), (height - 3)];
			float[,] s5s5d = new float[(width - 3), (height - 3)];
			float[,] r5r5d = new float[(width - 3), (height - 3)];
			float[,] r5e5d = new float[(width - 3), (height - 3)];
			float[,] r5s5d = new float[(width - 3), (height - 3)];
			float[,] r5l5d = new float[(width - 3), (height - 3)];
			for (int y = 2; y < height - 2; y++) {
				for (int x = 2; x < width - 2; x++) {
					for (int i = -2; i <= 2; i++) {
						for (int j = -2; j <= 2; j++) {
							l5e5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * l5e5[i + 2, j + 2];
							l5s5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * l5s5[i + 2, j + 2];
							l5r5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * l5r5[i + 2, j + 2];
							e5e5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * e5e5[i + 2, j + 2];
							e5s5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * e5s5[i + 2, j + 2];
							e5r5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * e5r5[i + 2, j + 2];
							e5l5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * e5l5[i + 2, j + 2];
							s5l5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * s5l5[i + 2, j + 2];
							s5e5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * s5e5[i + 2, j + 2];
							s5r5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * s5r5[i + 2, j + 2];
							s5s5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * s5s5[i + 2, j + 2];
							r5r5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * r5r5[i + 2, j + 2];
							r5e5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * r5e5[i + 2, j + 2];
							r5s5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * r5s5[i + 2, j + 2];
							r5l5d[x - 2, y - 2] += data[((y + j) * width) + x + i] * r5l5[i + 2, j + 2];
						}
					}
				}
			}
								
			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("L5E5/E5L5", l5e5d.AbsMean() / e5l5d.AbsMean())
					.AddFeature("L5S5/S5L5", l5s5d.AbsMean() / s5l5d.AbsMean())
					.AddFeature("L5R5/R5L5", l5r5d.AbsMean() / r5l5d.AbsMean())
					.AddFeature("E5S5/S5E5", e5s5d.AbsMean() / s5e5d.AbsMean())
					.AddFeature("E5R5/R5E5", e5r5d.AbsMean() / r5e5d.AbsMean())
					.AddFeature("S5R5/R5S5", s5r5d.AbsMean() / r5s5d.AbsMean())
					.AddFeature("E5E5", e5e5d.AbsMean())
					.AddFeature("S5S5", s5s5d.AbsMean())
					.AddFeature("R5R5", r5r5d.AbsMean())
				};
		}

		private static float[,] applyKernel(int width, int height, float[] data, int[,] kernel)
		{
			float[,] ret = new float[(width - 3), (height - 3)];
			for (int y = 2; y < height - 2; y++) {
				for (int x = 2; x < width - 2; x++) {
					for (int i = -2; i <= 2; i++) {
						for (int j = -2; j <= 2; j++) {
							ret[x - 2, y - 2] += data[((y + j) * width) + x + i] * kernel[i + 2, j + 2];
						}
					}
				}
			}

			return ret;
		}

		public override string Headline()
		{
			return "Laws Energy";
		}

		public override string ShortName()
		{
			return "laws";
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Laws energy";
			}
		}
	}
}

