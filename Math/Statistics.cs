using System;
using System.Linq;

namespace Baimp
{
	public static class Statistics
	{
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
		public static double Variance(this double[] values, double mean)
		{
			double variance = 0.0;

			for (int i = 0; i < values.Length; i++) {
				double x = values[i] - mean;
				variance += x * x;
			}

			return variance / (values.Length - 1);
		}

		/// <summary>
		/// Computes the entropy for a given array.
		/// </summary>
		/// <param name="values">Values.</param>
		public static double Entropy(this double[] values)
		{
			double sum = 0;
			foreach (double v in values)
				sum += v * Math.Log(v);
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
				sum += v * Math.Log(v + eps);
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
				sum += v * Math.Log(v + eps);
			return -sum;
		}
	}
}

