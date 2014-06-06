using System;
using System.Collections.Generic;

namespace Baimp
{
	public class Autocorrelation : BaseAlgorithm
	{
		public Autocorrelation(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan)));

			output.Add(new Compatible("Correlogram", typeof(THistogram)));

			options.Add(new Option("Offest", 0, 1024, 6));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan scan = inputArgs[0] as TScan;
			int offset = (int) options[0].Value;

			int width = (int) scan.Size.Width;
			int height = (int) scan.Size.Height;

			float[] data = scan.Data;

			double quadrat_sum = 0.0;
			unsafe {
				fixed (float* ptrData = data) {
					float* src = ptrData;

					for (int i = 0; i < width * height; i++) {
						quadrat_sum += *src;
						src++;
					}
				}
			}

			double[] histogram = new double[offset];
			for (int o = 0; o < offset; o++) {
				for (int y = 0; y < height - o; y++) {
					for (int x = 0; x < width - o; x++) {
						histogram[o] += data[y * width + x] * data[(y + o) * width + (x + o)];
					}
				}

				histogram[o] /= quadrat_sum;
			}

			return new IType[] { new THistogram(histogram) };
		}
		public override string Headline()
		{
			return string.Format("Autocorrelation ({0})", options[0].Value);
		}
		public override string ShortName()
		{
			return "autocorrelation";
		}
		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}
		public override string HelpText {
			get {
				return "Autocorrelation";
			}
		}

		#endregion
	}
}

