using Xwt;
using Xwt.Drawing;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Baimp
{
	public class TMatrix : IType
	{
		private double[,] matrix;
		private SparseMatrix<double> sparseMatrix;
		private Widget widget = null;

		bool isSparse;

		public TMatrix(double[,] matrix)
		{
			this.matrix = matrix;
		}

		public TMatrix(SparseMatrix<double> matrix)
		{
			this.sparseMatrix = matrix;
			isSparse = true;
		}

		public override string ToString()
		{
			if (isSparse) {
				return String.Format("{0}x{1} Matrix", sparseMatrix.Width, sparseMatrix.Height);
			} else {
				return String.Format("{0}x{1} Matrix", matrix.GetLength(0), matrix.GetLength(1));
			}
		}

		#region implemented interface members

		public Widget ToWidget()
		{
			long coloumns = Width;
			long rows = Height;

			if (widget == null) {
				if (rows <= 16 && coloumns <= 16) {
					Table t = new Table();

					if (isSparse) {
						foreach (int i in sparseMatrix.GetRows()) {
							foreach (KeyValuePair<int, double> value in sparseMatrix.GetRowData(i)) {
								t.Add(new Label(value.Value.ToString()), value.Key, i);
							}
						}
					} else {
						for (int x = 0; x < coloumns; x++) {
							for (int y = 0; y < rows; y++) {
								t.Add(new Label(matrix[x, y].ToString()), x, y);
							}
						}
					}

					widget = t;
				} else {
					BitmapImage bi;

					int xDiv = (int) Math.Round((double) coloumns / (double) BaseType<int>.MaxWidgetSize.Width) + 1;
					int yDiv = (int) Math.Round((double) rows / (double) BaseType<int>.MaxWidgetSize.Height) + 1;

					if (isSparse) {
						ImageBuilder ib = new ImageBuilder(coloumns / xDiv, rows / yDiv);
						bi = ib.ToBitmap();

						double max = sparseMatrix.Max();
						double min = sparseMatrix.Min();

						const double toMax = 65536.0;
						double toMaxLog = Math.Log(toMax);
						foreach (int y in sparseMatrix.GetRows()) {
							foreach (KeyValuePair<int, double> v in sparseMatrix.GetRowData(y)) {
								double toLog = (toMax - 1.0) * ((v.Value - min) / (max - min)) + 1.0;
								byte c = (byte) ((Math.Log(toLog) / toMaxLog) * 255);

								bi.SetPixel(v.Key / xDiv, y / yDiv, Color.FromBytes(c, c, c));
							}
						}

						ib.Dispose();

					} else {
						ImageBuilder ib = new ImageBuilder(coloumns, rows);
						bi = ib.ToBitmap();

						double max = 0.0;
						double[,] copy = matrix.Scale(1.0, 65536.0);

						for (int x = 0; x < coloumns; x++) {
							for (int y = 0; y < rows; y++) {
								if (copy[x, y] > 0) {
									copy[x, y] = (Math.Log(copy[x, y]));
								}

								if (copy[x, y] > max) {
									max = copy[x, y];
								}
							}
						}
												
						for (int x = 0; x < coloumns; x++) {
							for (int y = 0; y < rows; y++) {
								byte c = (byte) ((copy[x, y] * 255) / max);
								if (c > 0) {
									bi.SetPixel(x, y, Color.FromBytes(c, c, c));
								}
							}
						}
						ib.Dispose();
					}

					widget = new ImageView(bi.WithBoxSize(BaseType<int>.MaxWidgetSize));
				}
			}

			return widget;
		}

		public void Dispose()
		{
			if (widget != null) {
				widget.Dispose();
				widget = null;
			}

			if (isSparse) {
				sparseMatrix.Dispose();
			} else {
				matrix = null;
			}
		}
			
		#endregion

		#region Statistic methods

		/// <summary>
		/// Computes the entropy for this matrix.
		/// </summary>
		/// 
		/// <param name="eps">
		/// A very small constant to avoid <see cref="Double.NaN"/>
		/// if there are any zero values
		/// </param>
		public double Entropy(double eps = 0)
		{
			if (isSparse) {
				double sum = 0;
				foreach (int row in sparseMatrix.GetRows()) {
					foreach (KeyValuePair<int, double> v in sparseMatrix.GetRowData(row)) {
						sum += v.Value * Math.Log(v.Value + eps, 2);
					}
				}
				return -sum;
			}

			return matrix.Entropy(eps);
		}

		/// <summary>
		/// Sum of all matrix elements.
		/// </summary>
		public double Sum {
			get {
				if (isSparse) {
					double sum = 0;
					foreach (int row in sparseMatrix.GetRows()) {
						foreach (KeyValuePair<int, double> v in sparseMatrix.GetRowData(row)) {
							sum += v.Value;
						}
					}
					return sum;
				}

				return matrix.Sum();
			}
		}

		/// <summary>
		/// Gets the matrix mean μ.
		/// </summary>
		public double Mean {
			get {
				return Sum / Length;
			}
		}

		public void DivideAll(double by)
		{
			if (isSparse) {
				SparseMatrix<double> newMatrix = new SparseMatrix<double>(Width, Height);
				foreach (int row in sparseMatrix.GetRows()) {
					foreach (KeyValuePair<int, double> v in sparseMatrix.GetRowData(row)) {
						newMatrix[row, v.Key] = v.Value / by;
					}
				}

				sparseMatrix.Dispose();
				sparseMatrix = newMatrix;
			} else {
				for (int i = 0; i < matrix.GetLength(0); i++) {
					for (int j = 0; j < matrix.GetLength(1); j++) {
						matrix[i, j] /= by;
					}
				}
			}
		}

		#endregion

		#region Properties

		public double this[int row, int col] {
			get {
				if (isSparse) {
					return sparseMatrix.GetAt(row, col);
				}

				return matrix[row, col];
			}
			set {
				if (isSparse) {
					sparseMatrix.SetAt(row, col, value);
				} else {
					matrix[row, col] = value;
				}
			}
		}

		public long Width {
			get {
				if (isSparse) {
					return sparseMatrix.Width;
				} 

				return matrix.GetLongLength(0);
			}
		}

		public long Height {
			get {
				if (isSparse) {
					return sparseMatrix.Height;
				} 

				return matrix.GetLongLength(1);
			}
		}

		public long Length {
			get {
				return Width * Height;
			}
		}

		#endregion
	}
}

