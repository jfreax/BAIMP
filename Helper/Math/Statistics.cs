using System;
using System.Linq;
using System.Drawing.Imaging;

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
		public static double Entropy(this double[,] values, double eps = 0)
		{
			double sum = 0;
			foreach (double v in values)
				sum += v * Math.Log(v + eps, 2);
			return -sum;
		}

		#endregion

		#region Sum

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
			double sum = 0;
			foreach (double v in values) {
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
			}

			return sum;
		}

		#endregion
	}
}

