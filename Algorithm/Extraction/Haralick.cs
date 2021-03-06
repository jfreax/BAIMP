//
//  Haralick.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baimp
{
	public class Haralick : BaseAlgorithm
	{
		public Haralick(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Matrix", typeof(TMatrix)));

			output.Add(new Compatible("Haralick Features", typeof(TFeatureList<double>)));
		}

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TMatrix tMatrix = inputArgs[0] as TMatrix;
			if (tMatrix == null) {
				return null;
			}

			HaralickIntern h = new HaralickIntern(tMatrix);

			// features
			double asm = 0.0, contrast = 0.0, correlation, variance = 0.0, homogeneity = 0.0;
			double sumAverage = 0.0, sumVariance = 0.0, sumEntropy = 0.0, entropy = 0.0;
			double diffVariance = 0.0, diffEntropy = 0.0;
			double informationMeasure1 = 0.0, informationMeasure2 = 0.0;

			// temp variables
			double correlationStep = 0.0;
			double informationMeasure1Step = 0.0, informationMeasure2Step = 0.0;
			double[] xydiff = new double[Math.Max(h.InputMatrix.Width, h.InputMatrix.Height)];

			double mean = h.InputMatrix.Mean;
			// compute
			//Parallel.For(0, h.InputMatrix.GetLength(0), i => {
			for (int i = 0; i < h.InputMatrix.Width; i++) {
				for (int j = 0; j < h.InputMatrix.Height; j++) {
					double value = h.InputMatrix[i, j];

					xydiff[Math.Abs(i - j)] += h.InputMatrix[i, j];
					correlationStep += (i * j) * h.InputMatrix[i, j];

					informationMeasure1Step -= h.InputMatrix[i, j] * Math.Log(h.Px[i] * h.Py[j] + double.Epsilon, 2);
					informationMeasure2Step -= h.Px[i] * h.Py[j] * Math.Log(h.Px[i] * h.Py[j] + double.Epsilon, 2);

					asm += value * value;
					variance += (i - mean) * h.InputMatrix[i, j];
					homogeneity += h.InputMatrix[i, j] / (double) (1 + (i - j) * (i - j));
				}
			} //);

			correlation = (correlationStep - h.MeanX * h.MeanY) / (h.StandardDeviationX * h.StandardDeviationY);

			for (int n = 0; n < xydiff.Length; n++) {
				contrast += (n * n) * xydiff[n];
			}

			SetProgress(75);

			sumEntropy = h.Sums.Entropy(double.Epsilon);
			for (int i = 0; i < h.Sums.Length; i++) {
				sumAverage += i * h.Sums[i];
				sumVariance += (i - sumEntropy) * (i - sumEntropy) * h.Sums[i];
			}

			entropy = h.InputMatrix.Entropy(float.Epsilon);
			diffVariance = xydiff.Variance();
			diffEntropy = xydiff.Entropy(double.Epsilon);

			double entropyX = h.Px.Entropy(double.Epsilon);
			double entropyY = h.Py.Entropy(double.Epsilon);
			informationMeasure1 = (entropy - informationMeasure1Step) / Math.Max(entropyX, entropyY);

			informationMeasure2 = Math.Pow(1.0 - Math.Exp(-2 * (informationMeasure2Step - entropy)), 0.5);

					
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

		class HaralickIntern {

			public readonly TMatrix InputMatrix;
			double[] px;
			double[] py;

			double? meanx;
			double? meany;
			double? xdev;
			double? ydev;

			double[] xysum;

			public HaralickIntern(TMatrix matrix) {
				this.InputMatrix = matrix;
			}
				

			/// <summary>
			/// p<sub>x</sub>(i) = Σ<sub>j</sub> p(i,j).
			/// </summary>
			public double[] Px {
				get {
					if (px == null) {
						px = new double[InputMatrix.Width];
						for (int i = 0; i < InputMatrix.Width; i++)
							for (int j = 0; j < InputMatrix.Height; j++)
								px[i] += InputMatrix[i, j];
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
						py = new double[InputMatrix.Height];
						for (int j = 0; j < InputMatrix.Height; j++)
							for (int i = 0; i < InputMatrix.Width; i++)
								py[j] += InputMatrix[i, j];
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
						xysum = new double[2 * InputMatrix.Width];
						for (int i = 0; i < InputMatrix.Width; i++)
							for (int j = 0; j < InputMatrix.Width; j++)
								xysum[i + j] += InputMatrix[i, j];
					}
					return xysum;
				}
			}
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string Headline()
		{
			return "Haralick";
		}

		public override string ShortName()
		{
			return "haralick";
		}

		public override string HelpText {
			get {
				return "Haralick Texture Features";
			}
		}
	}
}

