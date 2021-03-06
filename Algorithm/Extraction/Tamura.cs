//
//  Tamura.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace Baimp
{
	public class Tamura : BaseAlgorithm
	{
		public Tamura(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan)));

			output.Add(new Compatible("Tamura Features", typeof(TFeatureList<double>)));
			output.Add(new Compatible("Directionality Histogram", typeof(THistogram)));

			options.Add(new Option("Directionality Histogram #Bins", 4, int.MaxValue, 64));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan tScan = inputArgs[0] as TScan;
			float[] data = tScan.Data;

			int nrOfBins = (int) options[0].Value;

			int width = (int) tScan.Size.Width;
			int height = (int) tScan.Size.Height;

			double coarsness = 0.0, contrast = 0.0, directionality = 0.0;

			float[,] sumAreaTable = SumAreaTable(data, width, height);

			float mean = data.Mean();
			float sigma = data.StandardDeviation(mean);
			double moment4 = 0.0;

			double[] histogram = new double[nrOfBins];
			double binWindow = (histogram.Length - 1) / Math.PI;
			int bin = -1;


			int oldProgress = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					moment4 += Math.Pow(data[y * width + x] - mean, 4);

					if (x > 0 && y > 0) {
						coarsness += Math.Pow(2, Sopt(sumAreaTable, x, y));

						if (x < width - 1 && y < height - 1) {
							double v = DeltaV(data, width, x, y);
							double h = DeltaH(data, width, x, y);
							if (h > 0.0 && v > 0.0) {
								bin = (int) ((Math.PI / 2 + Math.Atan(v / h)) * binWindow);
								histogram[bin]++;
							}
						}
					}
				}

				int progress = (int) (y * 100.0) / height;
				if (progress - oldProgress > 5) {
					oldProgress = progress;
					SetProgress(progress);
				}
			}

			coarsness /= height * width;

			if (sigma > 0) { // sigma <= 0 only if image is complete black
				double alpha4 = moment4 / (Math.Pow(sigma, 4));
				contrast = sigma / Math.Pow(alpha4, 0.25);
			}

			double histSum = histogram.Sum();
			for (int i = 0; i < histogram.Length; i++) {
				histogram[i] /= histSum;
			}

			int lastValley = -1;
			int lastHill = -1;
			int hillCount = 0; // n_p
			for (int i = 0; i < histogram.Length-1; i++) {
				double diff = histogram[i] - histogram[i + 1];

				if (diff > 0 && lastValley == -1) { // new valley begins here
					lastValley = i;
					lastHill = -1; // finished hill
				} else if (diff < 0 && lastHill == -1) { // new hill begins here
					hillCount++;
					lastHill = i;
					if (lastValley != -1) { // there was a valley before
						for (int j = lastValley; j < i; j++) {
							directionality += Math.Pow(j - i, 2) * (histogram[j]);
						}

						lastValley = -1; // finished valley
					}
				} else if (diff < 0) { // it still goes down
					directionality += Math.Pow(i - lastHill, 2) * (histogram[i]);
				}
			}
				
			directionality = 1 - (binWindow * hillCount * directionality);

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("coarsness", coarsness)
					.AddFeature("contrast", contrast)
					.AddFeature("directionality", directionality),
				new THistogram(histogram)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float Sopt(float[,] sumAreaTable, int x, int y)
		{
			double result = 0;
			int kOpt = 1;

			for (int k = 1; k < 5; k++) {
				int p = (int) Math.Pow(2, k - 1);

				double E_h = Math.Abs(
					             AverageNeighborhoods(sumAreaTable, x + p, y, p) -
					             AverageNeighborhoods(sumAreaTable, x - p, y, p)
				             );
				double E_v = Math.Abs(
					             AverageNeighborhoods(sumAreaTable, x, y + p, p) -
					             AverageNeighborhoods(sumAreaTable, x, y - p, p)
				             );

				double E_k = Math.Max(E_h, E_v);
				if (result < E_k) {
					kOpt = k;
					result = E_k;
				}
			}
			return kOpt;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float AverageNeighborhoods(float[,] sumAreaTable, int x, int y, int p)
		{

			int left = x - p;
			if (left < 0)
				left = 0;

			int right = x + p - 1;
			if (right >= sumAreaTable.GetLength(0))
				right = sumAreaTable.GetLength(0) - 1;

			int top = y - p;
			if (top < 0)
				top = 0;

			int bottom = y + p - 1;
			if (bottom >= sumAreaTable.GetLength(1))
				bottom = sumAreaTable.GetLength(1) - 1;
			if (bottom < 0)
				bottom = 0;

			return MeanSubMatrix(sumAreaTable, left, top, right, bottom);
		}

		static unsafe float[,] SumAreaTable(float[] data, int width, int height)
		{
			float[,] s = new float[width, height];
			float[,] ii = new float[width, height];

			s[0, 0] = data[0];
			ii[0, 0] = s[0, 0];
			for (int x = 1; x < width; x++) {
				s[x, 0] = data[x]; // [x, 0];
				ii[x, 0] = ii[x - 1, 0] + s[x, 0]; 
			}
			for (int y = 1; y < height; y++) {
				ii[0, y] = s[0, y] = s[0, y - 1] + data[(width * y)]; // [0, y];

				for (int x = 1; x < width; x++) {
					s[x, y] = s[x, y - 1] + data[(width * y) + x];
					ii[x, y] = ii[x - 1, y] + s[x, y];
				}
			}
			return ii;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float MeanSubMatrix(float[,] sum, int left, int top, int right, int bottom)
		{
			float v1, v2, v3, v4;
			v1 = (left == 0 || top == 0) ? 0 : sum[left - 1, top - 1];
			v2 = (top == 0) ? 0 : sum[right, top - 1];
			v3 = (left == 0) ? 0 : sum[left - 1, bottom];
			v4 = sum[right, bottom];

			return ((v4 + v1) - (v2 + v3)) / ((bottom - top) + (right - left));
		}

		static double[,] filterH = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
		static double[,] filterV = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static unsafe double DeltaH(float[] data, int width, int x, int y)
		{
			double result = 0;

			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					result = result + data[x - 1 + i + ((y - 1 + j) * width)] * filterH[i, j];
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static unsafe double DeltaV(float[] data, int width, int x, int y)
		{
			double result = 0;

			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					result = result + data[x - 1 + i + ((y - 1 + j) * width)] * filterV[i, j];
				}
			}
			return result;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Tamura Features";
			}
		}

		public override string Headline()
		{
			return string.Format("Tamuara ({0})", options[0].Value);
		}

		public override string ShortName()
		{
			return "tamura";
		}

		#endregion
	}
}

