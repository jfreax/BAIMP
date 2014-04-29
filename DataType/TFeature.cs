//
//  TFeature.cs
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
using Xwt;

namespace Baimp
{
	public class TFeature<T> : BaseType<T>, IFeature
	{
		string key;

		public TFeature(string key, T value) : base(value)
		{
			this.key = key;
		}
			
		public override Widget ToWidget()
		{
			if (widget == null) {
				if (string.IsNullOrEmpty(key)) {
					widget = new Label(raw.ToString());
				} else {
					HBox hbox = new HBox();
					hbox.PackStart(new Label(key));
					hbox.PackEnd(new Label(raw.ToString()));

					widget = hbox;
				}
			}

			return widget;
		}

		#region IFeature implementation
		public string Key()
		{
			return key;
		}
		public object Value()
		{
			return raw;
		}
		#endregion
	}
}

