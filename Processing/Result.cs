//
//  Result.cs
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
using System.IO;
using System.Collections.Generic;

namespace Baimp
{
	public class Result : IDisposable
	{
		object removeLock = new object();
		bool preserve;
		readonly Dictionary<PipelineNode, int> usedBy = new Dictionary<PipelineNode, int>();
		readonly PipelineNode node;

		/// <summary>
		/// The payload.
		/// </summary>
		public IType Data;
		string completeDistinctName = string.Empty;
		public int yieldID;

		/// <summary>
		/// The input that was used to compute these data.
		/// </summary>
		public readonly Result[] Input;

		public Result(PipelineNode node, IType data, Result[] input, bool preserve = false, int yieldID = -1)
		{
			this.node = node;
			this.Data = data;
			this.Input = input;
			this.Preserve = preserve;

			if (input != null) {
				completeDistinctName = node.ToString();
				if (yieldID != -1) {
					completeDistinctName += "#" + yieldID;
				}

				HashSet<string> visitedNames = new HashSet<string>();
				foreach (Result r in input) {
					if (!visitedNames.Contains(r.completeDistinctName)) {
						completeDistinctName = string.Format("{0}_{1}", r.completeDistinctName, completeDistinctName);
						visitedNames.Add(r.completeDistinctName);
					}
				}
			}
		}

		/// <summary>
		/// Call when you use this data.
		/// </summary>
		public void Used(PipelineNode by)
		{
			lock (removeLock) {
				if (!usedBy.ContainsKey(by)) {
					usedBy[by] = 0;
				}
				usedBy[by]++;
			}
		}

		/// <summary>
		/// Call when finished using these data.
		/// </summary>
		public void Finish(PipelineNode by)
		{
			lock (removeLock) {
				if (by == null) {
					if (usedBy.Count == 0 && !preserve) {
						Dispose();
					}
				} else if (usedBy.ContainsKey(by)) {
					usedBy[by] = usedBy[by] - 1;
					if (usedBy[by] <= 0) {
						usedBy.Remove(by);

						if (usedBy.Count == 0 && !preserve) {
							Dispose();
						}
					}
				} else if (usedBy.Count == 0 && !preserve) {
					Dispose();
				}
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Baimp.Result"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Baimp.Result"/>. The <see cref="Dispose"/>
		/// method leaves the <see cref="Baimp.Result"/> in an unusable state. After calling <see cref="Dispose"/>, you must
		/// release all references to the <see cref="Baimp.Result"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Baimp.Result"/> was occupying.</remarks>
		public void Dispose()
		{
			if (Data != null) {
				Data.Dispose();
				Data = null;
			}
		}

		#region helper functions

		public bool IsUsed(PipelineNode by)
		{
			return usedBy.ContainsKey(by);
		}

		public string SourceString()
		{
			string ret = "";
			string[] tokens = completeDistinctName.Split('_');
			foreach (string token in tokens) {
				if (token.Contains("#")) {
					ret += token + "_";
				}
			}

			ret = ret.TrimEnd('_');

			return ret;
		}

		public string DistinctSourceString()
		{
			return completeDistinctName;
		}

		#endregion

		#region properties

		public int InUse {
			get {
				return usedBy.Count;
			}
		}

		public PipelineNode Node {
			get {
				return node;
			}
		}

		public bool Preserve {
			get {
				return preserve;
			}
			private set {
				// Features should be preserved
				if (Data.GetType().IsGenericType) {
					Type genericType = Data.GetType().GetGenericTypeDefinition();
					if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition() ||
						genericType == typeof(TFeature<int>).GetGenericTypeDefinition()) {

						preserve = true;
						return;
					}
				}
				preserve = value;
			}
		}

		#endregion
	}
}

