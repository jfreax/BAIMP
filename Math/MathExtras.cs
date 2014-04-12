using System;
using System.Linq;

namespace Baimp
{
	public static class MathExtras
	{
		public static double[] Scale(double fromMin, double fromMax, double toMin, double toMax, double[] x)
		{
			double[] result = new double[x.Length];
			for (int i = 0; i < x.Length; i++)
				result[i] = (toMax - toMin) * (x[i] - fromMin) / (fromMax - fromMin) + toMin;

			return result;
		}

		public static double[,] Scale(double fromMin, double fromMax, double toMin, double toMax, double[,] x)
		{
			double[,] result = new double[x.GetLength(0), x.GetLength(1)];

			for (int i = 0; i < x.GetLength(0); i++) {
				for (int j = 0; j < x.GetLength(1); j++) {
					result[i, j] = (toMax - toMin) * (x[i,j] - fromMin) / (fromMax - fromMin) + toMin;
				}
			}

			return result;
		}

		public static double[] Scale(double toMin, double toMax, double[] x)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		public static double[] Scale(this double[] x, double toMin, double toMax)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		public static double[,] Scale(this double[,] x, double toMin, double toMax)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		public static double Min(this double[,] values) {
			double min = double.MaxValue;
			foreach (double v in values) {
				if (v < min) {
					min = v;
				}
			}
			return min;
		}

		public static double Max(this double[,] values) {
			double max = double.MinValue;
			foreach (double v in values) {
				if (v > max) {
					max = v;
				}
			}
			return max;
		}
	}
}

