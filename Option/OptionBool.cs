//
//  OptionBool.cs
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
using System.Xml.Serialization;

namespace Baimp
{
	public class OptionBool : BaseOption
	{
		[XmlIgnore]
		public readonly IComparable DefaultValue;

		[XmlIgnore]
		bool val;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public OptionBool() {}

		public OptionBool(string name, bool defaultValue)
		{
			this.Name = name;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		[XmlElement("value")]
		public override object Value {
			get {
				return val;
			}
			set {
				val = (bool) value;
			}
		}
	}
}

