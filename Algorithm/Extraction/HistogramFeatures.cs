using System;
using System.Collections.Generic;

namespace Baimp
{
	public class HistogramFeatures : BaseAlgorithm
	{
		public HistogramFeatures(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Histogram", typeof(THistogram)));

			output.Add(new Compatible("Histogram Features", typeof(TFeatureList<double>)));
		}


		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			THistogram tHistogram = inputArgs[0] as THistogram;
			double[] histogram = tHistogram.Data;

			double mean = histogram.Mean();
			double s = histogram.StandardDeviation(mean);

			double skewness = 0.0;
			double kurtosis = 0.0;

			int lastValley = -1;
			int lastHill = -1;
			int hillCount = histogram[0] - histogram[1] > 0 ? 1 : 0;
			for (int i = 0; i < histogram.Length-1; i++) {
				skewness += Math.Pow(histogram[i] - mean, 3);
				kurtosis += Math.Pow(histogram[i] - mean, 4);

				double diff = histogram[i] - histogram[i + 1];

				if (diff > 0 && lastValley == -1) { // new valley begins here
					lastValley = i;
					lastHill = -1; // finished hill
				} else if (diff < 0 && lastHill == -1) { // new hill begins here
					hillCount++;
					lastHill = i;
					if (lastValley != -1) { // there was a valley before
						lastValley = -1; // finished valley
					}
				}
			}

			skewness += Math.Pow(histogram[histogram.Length-1] - mean, 3);
			kurtosis += Math.Pow(histogram[histogram.Length-1] - mean, 4);
			skewness /= (histogram.Length - 1) * s * s * s;
			kurtosis /= (histogram.Length - 1) * s * s * s * s;

			double regularity = (double) hillCount / (histogram.Length / 2);

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("regularity", regularity)
					.AddFeature("skewness", skewness)
					.AddFeature("kurtosis", kurtosis)
				};
		}
		public override string Headline()
		{
			return "Histogram Features";
		}
		public override string ShortName()
		{
			return "histFeatures";
		}
		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}
		public override string HelpText {
			get {
				return "Generic features of a histogram";
			}
		}

		#endregion
	}
}

