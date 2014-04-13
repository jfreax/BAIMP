using System;
using System.Collections.Generic;

namespace Baimp
{
	public class Galloway : BaseAlgorithm
	{
		public Galloway(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));

			output.Add(new Compatible("Galloway Features", typeof(TFeatureList<double>)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TMatrix tMatrix = inputArgs[0] as TMatrix;
			if (tMatrix == null || tMatrix.Data == null) {
				return null;
			}
			double[,] matrix = tMatrix.Data;

			double numberOfRuns = matrix.Sum(); // R_G

			double sre = 0.0, lre = 0.0, lgre = 0.0, hlre = 0.0;
			double gln = 0.0, rln = 0.0;

			int n = 0; // number of pixels in original image
			for (int i = 0; i < matrix.GetLength(0); i++) {
				double glnInner = 0.0;
				for (int j = 0; j < matrix.GetLength(1); j++) {
					if (matrix[i, j] >= double.Epsilon) {
						n += (int) (matrix[i, j] * j);
						sre += matrix[i, j] / (j * j);
						lre += matrix[i, j] * (j * j);
						lgre += matrix[i, j] / (i * i);
						hlre += matrix[i, j] * (i * i);
						glnInner += matrix[i, j];
					}
				}
				gln += glnInner * glnInner;
			}

			SetProgress(50);

			for (int j = 0; j < matrix.GetLength(1); j++) {
				double rlnInner = 0.0;
				for (int i = 0; i < matrix.GetLength(0); i++) {
					rlnInner += matrix[i, j];
				}
				rln += rlnInner * rlnInner;
			}
				
			sre /= numberOfRuns;
			lre /= numberOfRuns;
			gln /= numberOfRuns;
			rln /= numberOfRuns;

			double rp = numberOfRuns / n;

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("shortRunEmphasis", sre)
					.AddFeature("longRunEmphasis", lre)
					.AddFeature("grayLevelNonUniformity", gln)
					.AddFeature("runLengthNonUniformity", rln)
					.AddFeature("runPercentage", rp)
					.AddFeature("lowGrayLevelRunEmphasis", lgre)
					.AddFeature("highGrayLevelRunEmphasis", hlre)
			};
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return
					"Extract 5 + 2 features from a Gray level run length matrix." +
					"These features are proposed from Galloway 1975 in \"Texture analysis using gray level run lengths\" " +
					"and from Chu et al. 1990 in \"Use of gray value distribution of run\nlengths for texture analysis\".";
			}
		}

		#endregion
	}
}

