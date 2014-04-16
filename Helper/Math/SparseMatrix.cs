using System;
using System.Collections.Generic;

namespace Baimp
{
	public class SparseMatrix<T>
	{
		// Dictionary to hold rows of column dictionary
		protected Dictionary<int, Dictionary<int, T>> rows = new Dictionary<int, Dictionary<int, T>>();

		T max = default(T);
		T min = default(T);

		public SparseMatrix(int width, int height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Gets the value at the specified position.
		/// </summary>
		/// <param name="row">Row</param>
		/// <param name="col">Column</param>
		/// <returns>Value at the given position</returns>
		public T GetAt(int row, int col)
		{
			Dictionary<int, T> cols;
			if (rows.TryGetValue(row, out cols)) {
				T value = default(T);
				if (cols.TryGetValue(col, out value))
					return value;
			}
			return default(T);
		}

		/// <summary>
		/// Set the value at the specified position.
		/// </summary>
		/// <param name="row">Row</param>
		/// <param name="col">Column</param>
		/// <param name="value">New value to set</param>
		public void SetAt(int row, int col, T value)
		{
			if (EqualityComparer<T>.Default.Equals(value, default(T))) {
				RemoveAt(row, col); // exit value, when default value was given
			} else {
				Dictionary<int, T> cols;
				if (!rows.TryGetValue(row, out cols)) {
					cols = new Dictionary<int, T>();
					rows.Add(row, cols);
				}
				cols[col] = value;
			}
		}

		/// <summary>
		/// Get or set the value at the specified position.
		/// </summary>
		/// <param name="row">Row</param>
		/// <param name="col">Column</param>
		public T this[int row, int col] {
			get {
				return GetAt(row, col);
			}
			set {
				SetAt(row, col, value);
			}
		}

		/// <summary>
		/// Remove the value at the specified position.
		/// </summary>
		/// <param name="row">Row</param>
		/// <param name="col">Column</param>
		public void RemoveAt(int row, int col)
		{
			Dictionary<int, T> cols;
			if (rows.TryGetValue(row, out cols)) {
				cols.Remove(col);

				if (cols.Count == 0) {
					rows.Remove(row); // Remove entirely row if empty
				}
			}
		}

		/// <summary>
		/// Returns all items of the specified row.
		/// </summary>
		/// <param name="row">Row</param>
		public IEnumerable<KeyValuePair<int, T>> GetRowData(int row)
		{
			Dictionary<int, T> cols;
			if (rows.TryGetValue(row, out cols)) {
				foreach (KeyValuePair<int, T> pair in cols) {
					yield return pair;
				}
			}
		}

		/// <summary>
		/// Get all row ids.
		/// </summary>
		/// <returns>The row ids.</returns>
		public IEnumerable<int> GetRows()
		{
			Dictionary<int, T> cols;
			foreach(var x in rows) {
				yield return x.Key;
			}
		}

		/// <summary>
		/// Returns all items in the specified column.
		/// </summary>
		/// <param name="col">Matrix column</param>
		/// <returns></returns>
		/// <remarks>
		/// Inefficient!
		/// </remarks>
		public IEnumerable<T> GetColumnData(int col)
		{
			foreach (KeyValuePair<int, Dictionary<int, T>> rowdata in rows) {
				T result;
				if (rowdata.Value.TryGetValue(col, out result)) {
					yield return result;
				}
			}
		}

		/// <summary>
		/// Returns the number of items in the specified row.
		/// </summary>
		/// <param name="row">Row</param>
		public int GetRowCount(int row)
		{
			Dictionary<int, T> cols;
			if (rows.TryGetValue(row, out cols)) {
				return cols.Count;
			}
			return 0;
		}

		/// <summary>
		/// Returns the number of items in the specified column.
		/// </summary>
		/// <param name="col">Column</param>
		public int GetColumnCount(int col)
		{
			int result = 0;

			foreach (KeyValuePair<int, Dictionary<int, T>> cols in rows) {
				if (cols.Value.ContainsKey(col)) {
					result++;
				}
			}
			return result;
		}

		#region Properties

		public int Width {
			get;
			set;
		}
			
		public int Height {
			get;
			set;
		}

		#endregion
	}
}

