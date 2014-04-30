//
//  Option.cs
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
using Xwt;

namespace Baimp
{
	public class Option : BaseOption
	{
		[XmlIgnore]
		public readonly IComparable MinValue = null;

		[XmlIgnore]
		public readonly IComparable MaxValue = null;

		[XmlIgnore]
		public readonly IComparable DefaultValue;
	
		[XmlIgnore]
		IComparable val;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public Option() {}

		public Option(string name, IComparable defaultValue)
		{
			this.Name = name;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		public Option(string name, IComparable minValue, IComparable maxValue, IComparable defaultValue)
		{
			this.Name = name;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		public override Widget ToWidget()
		{
			TextEntry entryText = new TextEntry();
			entryText.Text = Value.ToString();

			return entryText;
		}

		public override object ExtractValueFrom(Widget widget)
		{
			TextEntry te = widget as TextEntry;
			if (te != null) {
				return te.Text;
			}

			return null;
		}

		[XmlElement("value")]
		public override object Value
		{
			get {
				return val;
			}
			set {
				IComparable val = value as IComparable;
				if (val != null) {

					if (MaxValue != null && val.CompareTo(MaxValue) > 0) {
						this.val = MaxValue;
					} else if (MinValue != null && val.CompareTo(MinValue) < 0) {
						this.val = MinValue;
					} else {
						this.val = val;
					}
				}
			}
		}
	}
}

