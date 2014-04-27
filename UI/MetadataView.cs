//
//  MetadataView.cs
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
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;

namespace Baimp
{
	public class MetadataView : VBox
	{
		Dictionary<int, Widget>[] widgets;
		Table table;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.MetadataView"/> class.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Baimp.MetadataView.Load"></see> to actual see the metadata information/> 
		/// </remarks>
		public MetadataView()
		{
			table = new Table();
			this.PackStart(table, true);

			widgets = new Dictionary<int, Widget>[2];
			widgets[0] = new Dictionary<int, Widget>();
			widgets[1] = new Dictionary<int, Widget>();
		}

		/// <summary>
		/// Show metadata of specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Load(BaseScan scan)
		{
			table.Clear();

			int i = 1;
			foreach (Metadata d in scan.Metadata) {
				table.Add(new Label(d.key), 0, i);

				TextEntry entry = new TextEntry();
				entry.Text = d.value;
				entry.ReadOnly = true;
				entry.ShowFrame = false;
				entry.BackgroundColor = Color.FromBytes(232, 232, 232);
				table.Add(entry, 1, i);

				i++;
			}
		}
	}
}

