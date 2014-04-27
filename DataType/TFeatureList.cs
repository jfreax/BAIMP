//
//  TFeatureList.cs
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
using Xwt;

namespace Baimp
{
	public class TFeatureList<T> : BaseType<List<TFeature<T>>>
	{
		public TFeatureList() : base()
		{
			this.raw = new List<TFeature<T>>();
		}

		public TFeatureList(List<TFeature<T>> values) : base(values)
		{

		}

		public TFeatureList(params Tuple<string, T>[] values)
		{
			this.raw = new List<TFeature<T>>();
			foreach (var v in values) {
				this.raw.Add(new TFeature<T>(v.Item1, v.Item2));
			}
		}

		public TFeatureList<T> AddFeature(string name, T value)
		{
			raw.Add(new TFeature<T>(name, value));
			return this;
		}

		#region implemented abstract members of BaseType

		public override Widget ToWidget()
		{
			if (widget == null) {
				VBox vbox = new VBox();
				widget = vbox;

				foreach (var v in raw) {
					vbox.PackStart(v.ToWidget());
				}
			}

			return widget;
		}

		#endregion
	}
}

