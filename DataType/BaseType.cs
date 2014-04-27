//
//  BaseType.cs
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
	public abstract class BaseType<T> : IType
	{
		public static readonly Size MaxWidgetSize = new Size(300, 200);

		protected T raw;
		protected Widget widget = null;

		public BaseType()
		{
		}

		public BaseType(T raw)
		{
			this.raw = raw;
		}

		public T Data {
			get {
				return raw;
			}
			set {
				raw = value;
			}
		}

		abstract public Widget ToWidget();

		public object RawData()
		{
			return raw as object;
		}

		#region IDisposable implementation

		public virtual void Dispose()
		{
			if (widget != null) {
				widget.Dispose();
				widget = null;
			}

			IDisposable rawDisposable = raw as IDisposable;
			if (rawDisposable != null) {
				rawDisposable.Dispose();
			}

			raw = default(T);
		}

		#endregion
	}
}

