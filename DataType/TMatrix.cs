using Xwt;
using Xwt.Drawing;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Baimp
{
	public class TMatrix : IType
	{
		private float[,] matrix;
		private SparseMatrix<float> sparseMatrix;
		private Widget widget = null;

		bool isSparse;

		public TMatrix(float[,] matrix)
		{
			this.matrix = matrix;
		}

		public TMatrix(SparseMatrix<float> matrix)
		{
			this.sparseMatrix = matrix;
			isSparse = true;
		}

		public override string ToString()
		{
			if (isSparse) {
				return String.Format("{0}x{1} Matrix", sparseMatrix.Width, sparseMatrix.Height);
			}

			return String.Format("{0}x{1} Matrix", matrix.GetLength(0), matrix.GetLength(1));
		}

		#region implemented interface members

		public object RawData()
		{
			if (isSparse) {
				return sparseMatrix as object;
			}

			return matrix as object;
		}

		public Widget ToWidget()
		{
			long coloumns = Width;
			long rows = Height;

			if (widget == null) {
				if (rows <= 16 && coloumns <= 16) {
					Table t = new Table();

					if (isSparse) {
						foreach (int i in sparseMatrix.GetRows()) {
							foreach (KeyValuePair<int, float> value in sparseMatrix.GetRowData(i)) {
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

					int iScaleFactor = MathExtras.NextPowerOf2(
						(int) Math.Round((float) Math.Max(coloumns, rows) / BaseType<int>.MaxWidgetSize.Width) + 1
					);

					ImageBuilder ib = new ImageBuilder(coloumns / iScaleFactor, rows / iScaleFactor);
					bi = ib.ToBitmap();

					if (isSparse) {
						float max = sparseMatrix.Max();
						float min = sparseMatrix.Min();

						const float toMax = 65536.0f;
						float toMaxLog = (float) Math.Log(toMax);

						foreach (int y in sparseMatrix.GetRows()) {
							foreach (KeyValuePair<int, float> v in sparseMatrix.GetRowData(y)) {
								float toLog = (toMax - 1.0f) * ((v.Value - min) / (max - min)) + 1.0f;
								byte c = (byte) ((Math.Log(toLog) / toMaxLog) * 255);

								bi.SetPixel(v.Key / iScaleFactor, y / iScaleFactor, Color.FromBytes(c, c, c));
							}
						}
					} else {

						float max = 0.0f;
						float[,] copy = matrix.Scale(1.0f, 65536.0f);

						for (int x = 0; x < coloumns; x++) {
							for (int y = 0; y < rows; y++) {
								if (copy[x, y] > 0) {
									copy[x, y] = (float) (Math.Log(copy[x, y]));
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
									bi.SetPixel(x / iScaleFactor, y / iScaleFactor, Color.FromBytes(c, c, c));
								}
							}
						}
					}

					ib.Dispose();
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
		public float Entropy(float eps = 0)
		{
			if (isSparse) {
				float sum = 0f;
				foreach (int row in sparseMatrix.GetRows()) {
					foreach (KeyValuePair<int, float> v in sparseMatrix.GetRowData(row)) {
						sum += v.Value * (float) Math.Log(v.Value + eps, 2);
					}
				}
				return -sum;
			}

			return matrix.Entropy(eps);
		}

		/// <summary>
		/// Sum of all matrix elements.
		/// </summary>
		public float Sum {
			get {
				if (isSparse) {
					float sum = 0f;
					foreach (int row in sparseMatrix.GetRows()) {
						foreach (KeyValuePair<int, float> v in sparseMatrix.GetRowData(row)) {
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
		public float Mean {
			get {
				return Sum / Length;
			}
		}

		public void DivideAll(float by)
		{
			if (isSparse) {
				SparseMatrix<float> newMatrix = new SparseMatrix<float>(Width, Height);
				foreach (int row in sparseMatrix.GetRows()) {
					foreach (KeyValuePair<int, float> v in sparseMatrix.GetRowData(row)) {
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

		public float this[int row, int col] {
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

