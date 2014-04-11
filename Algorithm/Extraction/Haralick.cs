using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baimp
{
	public class Haralick : BaseAlgorithm
	{
		double[,] inputMatrix;
		double[] px;
		double[] py;

		double? sum;
		double? mean;
		double? meanx;
		double? meany;
		double? xdev;
		double? ydev;

		double[] xysum;

		public Haralick(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Matrix", typeof(TMatrix)));

			output.Add(new Compatible("Haralick Features", typeof(TFeatureList<double>)));
		}

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TMatrix tMatrix = inputArgs[0] as TMatrix;
			if (tMatrix == null || tMatrix.Data == null) {
				return null;
			}

			inputMatrix = tMatrix.Data;

			// features
			double asm = 0.0, contrast = 0.0, correlation = 0.0, variance = 0.0, homogeneity = 0.0;
			double sumAverage = 0.0, sumVariance = 0.0, sumEntropy = 0.0, entropy = 0.0;
			double diffVariance = 0.0, diffEntropy = 0.0;
			double informationMeasure1 = 0.0, informationMeasure2 = 0.0;

			// temp variables
			double correlationStep = 0.0;
			double informationMeasure1Step = 0.0, informationMeasure2Step = 0.0;
			double[] xydiff = new double[inputMatrix.GetLength(0)];

			// compute
			Parallel.For(0, inputMatrix.GetLength(0), i => {
				for (int j = 0; j < inputMatrix.GetLength(1); j++) {
					double value = inputMatrix[i, j];

					xydiff[Math.Abs(i - j)] += inputMatrix[i, j];
					correlationStep += (i * j) * inputMatrix[i, j];

					informationMeasure1Step -= inputMatrix[i, j] * Math.Log(Px[i] * Py[j] + double.Epsilon);
					informationMeasure2Step -= Px[i] * Py[j] * Math.Log(Px[i] * Py[j] + double.Epsilon);

					asm += value * value;
					variance += (i - Mean) * inputMatrix[i, j];
					homogeneity += inputMatrix[i, j] / (double) (1 + (i - j) * (i - j));


				}
			});

			correlation = (correlationStep - MeanX * MeanY) / (StandardDeviationX * StandardDeviationY);

			for (int n = 0; n < xydiff.Length; n++) {
				contrast += (n * n) * xydiff[n];
			}

			sumEntropy = Sums.Entropy(double.Epsilon);
			for (int i = 0; i < Sums.Length; i++) {
				sumAverage += i * Sums[i];
				sumVariance += (i - sumEntropy) * (i - sumEntropy) * Sums[i];
			}

			entropy = inputMatrix.Entropy(double.Epsilon);
			diffVariance = xydiff.Variance();
			diffEntropy = xydiff.Entropy(double.Epsilon);

			double entropyX = Px.Entropy(double.Epsilon);
			double entropyY = Py.Entropy(double.Epsilon);
			if (entropyX + entropyY > 0.0) {
				informationMeasure1 = (entropy - informationMeasure1Step) / Math.Max(entropyX, entropyY);
			}
			informationMeasure2 = Math.Sqrt(1.0 - Math.Exp(-2 * (informationMeasure2Step - entropy)));
					
			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("asm", asm)
					.AddFeature("contrast", contrast)
					.AddFeature("correlation", correlation)
					.AddFeature("variance", variance)
					.AddFeature("homogeneity", homogeneity)
					.AddFeature("sumAverage", sumAverage)
					.AddFeature("sumVariance", sumVariance)
					.AddFeature("sumEntropy", sumEntropy)
					.AddFeature("entropy", entropy)
					.AddFeature("diffVariance", diffVariance)
					.AddFeature("diffEntropy", diffEntropy)
					.AddFeature("informationMeasure1", informationMeasure1)
					.AddFeature("informationMeasure2", informationMeasure2)
			};
		}

		/// <summary>
		/// Sum of all matrix elements.
		/// </summary>
		public double Sum {
			get {
				if (sum == null) {
					double s = 0;
					foreach (double x in inputMatrix)
						s += x;
					sum = s;
				}
				return sum.Value;
			}
		}

		/// <summary>
		/// Gets the matrix mean μ.
		/// </summary>
		public double Mean {
			get {
				if (mean == null)
					mean = Sum / inputMatrix.Length;
				return mean.Value;
			}
		}

		/// <summary>
		/// p<sub>x</sub>(i) = Σ<sub>j</sub> p(i,j).
		/// </summary>
		public double[] Px {
			get {
				if (px == null) {
					px = new double[inputMatrix.GetLength(0)];
					for (int i = 0; i < px.Length; i++)
						for (int j = 0; j < px.Length; j++)
							px[i] += inputMatrix[i, j];
				}
				return px;
			}
		}

		/// <summary>
		/// p<sub>y</sub>(j) = Σ<sub>i</sub> p(i,j).
		/// </summary>
		public double[] Py {
			get {
				if (py == null) {
					py = new double[inputMatrix.GetLength(0)];
					for (int j = 0; j < py.Length; j++)
						for (int i = 0; i < py.Length; i++)
							py[j] += inputMatrix[i, j];
				}
				return py;
			}
		}

		/// <summary>
		/// Get the mean value of the <see cref="Px"/> vector.
		/// </summary>
		public double MeanX {
			get {
				if (meanx == null)
					meanx = Px.Mean();
				return meanx.Value;
			}
		}

		/// <summary>
		/// Get he mean value of the <see cref="Py"/> vector.
		/// </summary>
		public double MeanY {
			get {
				if (meany == null)
					meany = Py.Mean();
				return meany.Value;
			}
		}

		/// <summary>
		/// Get the variance of the <see cref="Px"/> vector.
		/// </summary>
		///
		public double StandardDeviationX {
			get {
				if (xdev == null)
					xdev = Px.StandardDeviation(MeanX);
				return xdev.Value;
			}
		}

		/// <summary>
		/// Gets the variance of the <see cref="Py"/> vector.
		/// </summary>
		///
		public double StandardDeviationY {
			get {
				if (ydev == null)
					ydev = Py.StandardDeviation(MeanY);
				return ydev.Value;
			}
		}

		/// <summary>
		/// Gets p<sub>(x+y)</sub>(k), the sum
		/// of elements whose indices sum to k.
		/// </summary>
		public double[] Sums {
			get {
				if (xysum == null) {
					xysum = new double[2 * inputMatrix.GetLength(0)];
					for (int i = 0; i < inputMatrix.GetLength(0); i++)
						for (int j = 0; j < inputMatrix.GetLength(0); j++)
							xysum[i + j] += inputMatrix[i, j];
				}
				return xysum;
			}
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Haralick Texture Features";
			}
		}
	}
}

