//
//  ResultPopupView.cs
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
using System.Collections.Generic;

namespace Baimp
{
	public class ResultPopupView : VBox
	{
		List<Tuple<IType[], Result[]>> results;
		int position;
		Widget widget = null;
		ComboBox combo = new ComboBox();


		public ResultPopupView(List<Tuple<IType[], Result[]>> results, int position)
		{
			this.results = results;
			this.position = position;

			// TODO search all nodes until we find the right root
			// right root == node without further input data and from type "BaseScan"
			foreach (var result in results) {
				Result[] input = result.Item2;
				while (true) {
					if (input[0].Input == null || input[0].Input.Length == 0) {
						break;
					}
					input = input[0].Input;
				}
				combo.Items.Add(input[0].Data);
			}

			InitializeEvents();

			this.PackStart(combo);
			combo.SelectedIndex = 0;
		}

		private void InitializeEvents()
		{
			combo.SelectionChanged += delegate(object sender, EventArgs e) {
				if (widget != null) {
					this.Remove(widget);
				}

				widget = results[combo.SelectedIndex].Item1[position].ToWidget();
				this.PackEnd( widget );
			};

			this.KeyPressed += delegate(object sender, KeyEventArgs e) {
				if(e.Key == Key.Escape) {
					this.Hide();
				}
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (widget != null) {
				this.Remove(widget);
			}
			base.Dispose(disposing);
		}
	}
}

