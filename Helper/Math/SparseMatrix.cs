//
//  SparseMatrix.cs
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

namespace Baimp
{
	public class SparseMatrix<T> : IDisposable
	{
		// Dictionary to hold rows of column dictionary
		protected Dictionary<int, Dictionary<int, T>> rows = new Dictionary<int, Dictionary<int, T>>();

		public SparseMatrix(long width, long height)
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

		public long Width {
			get;
			set;
		}
			
		public long Height {
			get;
			set;
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{
			foreach (KeyValuePair<int, Dictionary<int, T>> cols in rows) {
				cols.Value.Clear();
			}

			rows.Clear();
		}

		#endregion
	}
}

