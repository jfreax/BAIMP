//
//  Statistics.cs
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
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public static class Statistics
	{
		#region Mean

		/// <summary>
		/// Computes the mean of a given double array.
		/// </summary>
		/// <param name="values">Values.</param>
		/// <returns>Mean of specified data.</returns>
		public static float Mean(this float[] values)
		{
			return values.Sum() / values.Length;
		}

		/// <summary>
		/// Computes the mean of a given double array.
		/// </summary>
		/// <param name="values">Values.</param>
		/// <returns>Mean of specified data.</returns>
		public static double Mean(this double[] values)
		{
			return values.Sum() / values.Length;
		}

		/// <summary>
		/// Compute the mean of all pixel in a given image.
		/// </summary>
		/// <param name="image">Image.</param>
		public static double Mean(this BitmapData image)
		{
			return image.Sum() / (image.Width * image.Height);
		}
			
		#endregion

		#region Variance

		/// <summary>
		/// Compute the variance of a given vector.
		/// </summary>
		/// <param name="values">Values.</param>
		public static double Variance(this double[] values)
		{
			return Variance(values, Mean(values));
		}

		/// <summary>
		/// Compute the variance of a given vector.
		/// </summary>
		/// <param name="values">Values.</param>
		/// <param name="mean">Mean.</param>
		public static float Variance(this float[] values, float mean)
		{
			float variance = 0.0f;

			for (int i = 0; i < values.Length; i++) {
				float x = values[i] - mean;
				variance += x * x;
			}

			return variance / (values.Length - 1);
		}

		/// <summary>
		/// Compute the variance of a given vector.
		/// </summary>
		/// <param name="values">Values.</param>
		/// <param name="mean">Mean.</param>
		public static double Variance(this double[] values, double mean)
		{
			double variance = 0.0;

			for (int i = 0; i < values.Length; i++) {
				double x = values[i] - mean;
				variance += x * x;
			}

			return variance / (values.Length - 1);
		}

		#endregion

		#region Standard Deviation

		/// <summary>
		/// Compute the Standard Deviation.
		/// </summary>
		/// <returns>The Standard Deviation.</returns>
		/// <param name="values">Values.</param>
		/// <param name="mean">Mean.</param>
		public static float StandardDeviation(this float[] values, float mean)
		{
			return (float) Math.Sqrt(Variance(values, mean));
		}

		/// <summary>
		/// Compute the Standard Deviation.
		/// </summary>
		/// <returns>The Standard Deviation.</returns>
		/// <param name="values">Values.</param>
		/// <param name="mean">Mean.</param>
		public static double StandardDeviation(this double[] values, double mean)
		{
			return Math.Sqrt(Variance(values, mean));
		}

		/// <summary>
		/// Compute the Standard Deviation.
		/// </summary>
		/// <returns>The Standard Deviation.</returns>
		/// <param name="image">Values as BitmapData.</param>
		/// <param name="mean">Mean.</param>
		public static unsafe double StandardDeviation(this BitmapData image, double mean)
		{
			int width = image.Width;
			int height = image.Height;
			int offset = image.Stride - image.Width;

			double sum = 0;

			if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
				byte* src = (byte*) image.Scan0;

				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++, src++) {
						double u = (*src) - mean;
						sum += u * u;
					}
					src += offset;
				}
			} else {
				// TODO
			}

			return Math.Sqrt(sum / (width * height - 1));
		}

		#endregion

		#region Entropy

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// <param name="values">Values.</param>
		public static double Entropy(this double[] values)
		{
			double sum = 0;
			foreach (double v in values)
				sum += v * Math.Log(v, 2);
			return -sum;
		}

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// 
		/// <param name="values">Values.</param>
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public static double Entropy(this double[] values, double eps = 0)
		{
			double sum = 0;
			foreach (double v in values)
				sum += v * Math.Log(v + eps, 2);
			return -sum;
		}

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// 
		/// <param name="values">Values.</param>
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public static float Entropy(this float[] values, float eps = 0f)
		{
			float sum = 0f;
			foreach (float v in values)
				sum += v * (float) Math.Log(v + eps, 2);
			return -sum;
		}

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// 
		/// <param name="values">Values.</param>
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public static float Entropy(this float[,] values, float eps = 0f)
		{
			float sum = 0;
			foreach (float v in values)
				sum += v * (float) Math.Log(v + eps, 2);
			return -sum;
		}

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// 
		/// <param name="values">Values.</param>
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public static double Entropy(this double[,] values, double eps = 0)
		{
			double sum = 0;
			foreach (double v in values)
				sum += v * Math.Log(v + eps, 2);
			return -sum;
		}

		/// <summary>
		/// Computes the entropy for a given sparse matrix.
		/// </summary>
		/// 
		/// <param name="matrix">Sparse matrix.</param>
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public static double Entropy(this SparseMatrix<double> matrix, double eps = 0)
		{
			double sum = 0;
			foreach (int row in matrix.GetRows()) {
				foreach (KeyValuePair<int, double> v in matrix.GetRowData(row)) {
					sum += v.Value * Math.Log(v.Value + eps, 2);
				}
			}
			return -sum;
		}

		#endregion

		#region Sum

		/// <summary>
		/// Sum of all values.
		/// </summary>
		/// <param name="values">Values.</param>
		public static float Sum(this float[,] values)
		{
			float sum = 0.0f;
			foreach (float v in values) {
				sum += v;
			}

			return sum;
		}

		/// <summary>
		/// Sum of all values.
		/// </summary>
		/// <param name="values">Values.</param>
		public static double Sum(this double[,] values)
		{
			double sum = 0.0;
			foreach (double v in values) {
				sum += v;
			}

			return sum;
		}

		/// <summary>
		/// Sum of all values.
		/// </summary>
		/// <param name="values">Values.</param>
		public static double Sum(this double[] values)
		{
			double sum = 0.0;
			foreach (double v in values) {
				sum += v;
			}

			return sum;
		}

		/// <summary>
		/// Sum of all values.
		/// </summary>
		/// <param name="values">Values.</param>
		public static float Sum(this float[] values)
		{
			float sum = 0.0f;
			foreach (float v in values) {
				sum += v;
			}

			return sum;
		}

		/// <summary>
		/// Sum of all values.
		/// </summary>
		/// <param name="values">Values.</param>
		public static int Sum(this int[] values)
		{
			int sum = 0;
			foreach (int v in values) {
				sum += v;
			}

			return sum;
		}

		/// <summary>
		/// Sum of all pixel of a given image.
		/// </summary>
		/// <param name="image">Image.</param>
		public static unsafe double Sum(this BitmapData image)
		{
			int width = image.Width;
			int height = image.Height;
			int stride = image.Stride;
			int offset = image.Stride - image.Width;

			int sum = 0;

			if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
				byte* src = (byte*) image.Scan0;

				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						sum += (*src++);
					}

					src += offset;
				}
			} else {
				// TODO
				throw new NotSupportedException();
			}

			return sum;
		}

		#endregion
	}
}

