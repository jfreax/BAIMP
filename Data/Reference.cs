//
//  Reference.cs
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
using System;
using System.Collections.Generic;
using System.Threading;

namespace Baimp
{
	public static class ReferenceGarbaseCollector
	{
		static readonly object list_lock = new object();
		static readonly List<IReference> references = new List<IReference>();

		static ReferenceGarbaseCollector()
		{
			new Timer(o => Free(), null, 0, 60000);
		}

		/// <summary>
		/// Register the specified reference.
		/// </summary>
		/// <param name="reference">Reference.</param>
		public static void Register(IReference reference)
		{
			lock (list_lock) {
				references.Add(reference);
			}
		}

		/// <summary>
		/// Unregister the specified reference.
		/// </summary>
		/// <param name="reference">Reference.</param>
		public static void Unregister(IReference reference)
		{
			lock (list_lock) {
				references.Remove(reference);
			}
		}

		/// <summary>
		/// Free reference data.
		/// Gets called periodically.
		/// </summary>
		static void Free()
		{
			List<IReference> refCopy;
			lock (list_lock) {
				refCopy = new List<IReference>(references);
			}
			foreach (IReference reference in refCopy) {
				if (reference.Free()) {
					Unregister(reference);
				}
			}
		}
	}

	public interface IReference
	{
		bool Free(bool force = false);
	}

	public class Reference<T> : IDisposable, IReference where T : IDisposable
	{
		int referenceCounter;
		readonly T referenceTo;
		readonly bool instantlyFreeing;
		readonly object counter_lock = new object();

		public Reference(T referenceTo, bool instantlyFreeing = false)
		{
			referenceCounter = 0;
			this.referenceTo = referenceTo;
			this.instantlyFreeing = instantlyFreeing;

			ReferenceGarbaseCollector.Register(this);
		}

		/// <summary>
		/// Get the referenced data.
		/// </summary>
		/// <value>The data.</value>
		/// <remarks>
		/// For every 'Data' use, you have to Dispose this instance.
		/// </remarks>
		public T Data {
			get {
				lock (counter_lock) {
					referenceCounter++;
				}

				return referenceTo;
			}
		}

		/// <summary>
		/// Gets the reference count.
		/// </summary>
		/// <value>The reference count.</value>
		public int ReferenceCount {
			get {
				return referenceCounter;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the referenced data was freed.
		/// </summary>
		/// <value><c>true</c> if the references data is freed; otherwise, <c>false</c>.</value>
		public bool IsFreed {
			get;
			private set;
		}

		/// <summary>
		/// Free the referenced data.
		/// </summary>
		/// <param name="force">If set to <c>true</c> force disposing (even when reference count > 0.</param>
		public bool Free(bool force = false)
		{
			if (force || referenceCounter == 0) {
				referenceTo.Dispose();
				IsFreed = true;

				return true;
			}

			return false;
		}

		#region IDisposable implementation

		public void Dispose()
		{
			lock (counter_lock) {
				referenceCounter--;

				if (instantlyFreeing) {
					if (Free()) {
						ReferenceGarbaseCollector.Unregister(this);
					}
				}
			}
		}

		#endregion
	}
}

