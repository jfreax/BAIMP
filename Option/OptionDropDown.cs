//
//  OptionDropDown.cs
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
using System.Xml.Serialization;
using Xwt;

namespace Baimp
{
	public class OptionDropDown : BaseOption
	{
		IComparable[] values;

		[XmlIgnore]
		string val = string.Empty;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public OptionDropDown() {}

		public OptionDropDown(string name, params string[] values)
		{
			this.Name = name;
			this.values = values;

			if (string.IsNullOrEmpty(val)) {
				val = values[0];
			}
		}

		public override Widget ToWidget()
		{
			ComboBox combo = new ComboBox();
			foreach (var v in values) {
				combo.Items.Add(v);
			}

			if (string.IsNullOrEmpty(val)) {
				combo.SelectedIndex = 0;
			} else {
				try {
					combo.SelectedItem = val;
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}

			return combo;
		}

		public override object ExtractValueFrom(Widget widget)
		{
			ComboBox cb = widget as ComboBox;
			if (cb != null) {
				return cb.SelectedItem;
			}
			return null;
		}

		[XmlElement("value")]
		public override object Value {
			get {
				return val;
			}
			set {
				val = (string) value;
			}
		}

		[XmlIgnore]
		public IComparable[] Values {
			get {
				return values;
			}
			set {
				values = value;
			}
		}
	}
}

