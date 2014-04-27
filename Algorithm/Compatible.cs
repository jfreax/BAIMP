//
//  Compatible.cs
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
using System.Linq;

namespace Baimp
{
	public class Compatible
	{
		public readonly BaseConstraint[] Constraints;
		readonly string name;
		Type type;

		public Compatible()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Compatible"/> class.
		/// </summary>
		/// <param name="name">Name of this compatible.</param>
		/// <param name="type">In- or output type.</param>
		/// <param name="constraints">Constraints.</param>
		public Compatible(string name, Type type, params BaseConstraint[] constraints)
		{
			this.name = name;
			this.type = type;
			this.Constraints = constraints;
		}

		/// <summary>
		/// Tests if another instance is compatible with this one
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="nodeFrom"></param>
		/// <param name="nodeTo"></param>
		public bool Match(MarkerNode nodeFrom, MarkerNode nodeTo)
		{
			Compatible another = nodeTo.compatible;


			Type typeFrom = Type;
			Type typeTo = another.Type;

			if (typeFrom.IsGenericType) {
				typeFrom = typeFrom.GetGenericArguments()[0];
			}

			if (typeTo.IsGenericType) {
				typeTo = typeTo.GetGenericArguments()[0];
			}

			if (typeTo.IsInterface) {
				if (!typeFrom.GetInterfaces().Contains(typeTo)) {
					return false;
				}
			} else if (typeFrom.IsInterface) {
				if (!typeTo.GetInterfaces().Contains(typeFrom)) {
					return false;
				}
			} else {
				if (!typeFrom.Equals(typeTo)) {
					return false;
				}
			}
				
			foreach (BaseConstraint constraint in Constraints) {
				if (!constraint.FulFills(nodeFrom, nodeTo)) {
					return false;
				}
			}

			foreach (BaseConstraint constraint in another.Constraints) {
				if (!constraint.FulFills(nodeTo, nodeFrom)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether this is an end part. Aka a feature(-list).
		/// </summary>
		/// <returns><c>true</c> if this instance is end; otherwise, <c>false</c>.</returns>
		public bool IsFinal(){
			if (Type.IsGenericType) {
				Type genericType = Type.GetGenericTypeDefinition();
				if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition() ||
				    genericType == typeof(TFeature<int>).GetGenericTypeDefinition()) {

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Type of in-/output
		/// </summary>
		/// <value>The type.</value>
		public Type Type {
			get { return type; }
		}

		public override string ToString()
		{
			return name;
		}
	}
}

