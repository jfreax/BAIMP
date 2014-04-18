using System;
using System.Linq;
using System.Collections.Generic;

namespace Baimp
{
	public static class MathExtras
	{
		#region Scale

		public static float[] Scale(float fromMin, float fromMax, float toMin, float toMax, float[] x)
		{
			float[] result = new float[x.Length];
			for (int i = 0; i < x.Length; i++)
				result[i] = (toMax - toMin) * (x[i] - fromMin) / (fromMax - fromMin) + toMin;

			return result;
		}

		public static float[,] Scale(float fromMin, float fromMax, float toMin, float toMax, float[,] x)
		{
			float[,] result = new float[x.GetLength(0), x.GetLength(1)];

			for (int i = 0; i < x.GetLength(0); i++) {
				for (int j = 0; j < x.GetLength(1); j++) {
					result[i, j] = (toMax - toMin) * (x[i, j] - fromMin) / (fromMax - fromMin) + toMin;
				}
			}

			return result;
		}

		public static float[] Scale(float toMin, float toMax, float[] x)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		public static float[] Scale(this float[] x, float toMin, float toMax)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		public static float[,] Scale(this float[,] x, float toMin, float toMax)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

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
					result[i, j] = (toMax - toMin) * (x[i, j] - fromMin) / (fromMax - fromMin) + toMin;
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

		public static int[] Scale(int fromMin, int fromMax, int toMin, int toMax, int[] x)
		{
			int[] result = new int[x.Length];
			for (int i = 0; i < x.Length; i++)
				result[i] = (toMax - toMin) * (x[i] - fromMin) / (fromMax - fromMin) + toMin;

			return result;
		}

		public static int[] Scale(this int[] x, int toMin, int toMax)
		{
			return Scale(x.Min(), x.Max(), toMin, toMax, x);
		}

		#endregion

		#region Min

		public static double Min(this double[,] values)
		{
			double min = double.MaxValue;
			foreach (double v in values) {
				if (v < min) {
					min = v;
				}
			}
			return min;
		}

		public static float Min(this float[,] values)
		{
			float min = float.MaxValue;
			foreach (float v in values) {
				if (v < min) {
					min = v;
				}
			}
			return min;
		}

		public static double Min(this SparseMatrix<double> matrix)
		{
			double min = double.MaxValue;
			foreach (int row in matrix.GetRows()) {
				foreach (KeyValuePair<int, double> v in matrix.GetRowData(row)) {
					if (v.Value < min) {
						min = v.Value;
					}
				}
			}
			return min;
		}

		public static float Min(this SparseMatrix<float> matrix)
		{
			float min = float.MaxValue;
			foreach (int row in matrix.GetRows()) {
				foreach (KeyValuePair<int, float> v in matrix.GetRowData(row)) {
					if (v.Value < min) {
						min = v.Value;
					}
				}
			}
			return min;
		}

		#endregion

		#region Max

		public static double Max(this double[,] values)
		{
			double max = double.MinValue;
			foreach (double v in values) {
				if (v > max) {
					max = v;
				}
			}
			return max;
		}

		public static float Max(this float[,] values)
		{
			float max = float.MinValue;
			foreach (float v in values) {
				if (v > max) {
					max = v;
				}
			}
			return max;
		}

		public static double Max(this SparseMatrix<double> matrix)
		{
			double max = double.MinValue;
			foreach (int row in matrix.GetRows()) {
				foreach (KeyValuePair<int, double> v in matrix.GetRowData(row)) {
					if (v.Value > max) {
						max = v.Value;
					}
				}
			}
			return max;
		}

		public static float Max(this SparseMatrix<float> matrix)
		{
			float max = float.MinValue;
			foreach (int row in matrix.GetRows()) {
				foreach (KeyValuePair<int, float> v in matrix.GetRowData(row)) {
					if (v.Value > max) {
						max = v.Value;
					}
				}
			}
			return max;
		}

		#endregion

		public static int NextPowerOf2(int x)
		{
			--x;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return ++x;
		}
	}
}

