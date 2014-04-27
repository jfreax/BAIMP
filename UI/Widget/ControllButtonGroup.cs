//
//  ControllButtonGroup.cs
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
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButtonGroup : HBox
	{
			
		public ControllButtonGroup()
		{
			this.Spacing = 0;
		}

		/// <summary>
		/// Adds a new button.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="icon">Icon.</param>
		/// <param name="toggleButton">If set to <c>true</c>, then is this a toggle button.</param>
		public ButtonSegment AddButton(Image icon, bool toggleButton = false)
		{
			int childCounter = 0;
			foreach (var child in Children) {
				ButtonSegment s = child as ButtonSegment;
				if (s != null) {
					if (childCounter == 0) {
						s.SegmentType = SegmentType.Left;
					} else {
						s.SegmentType = SegmentType.Middle;
					}

					childCounter++;
				}
			}

			ButtonSegment segment = 
				new ButtonSegment(childCounter == 0 ? SegmentType.Left : SegmentType.Right, icon, toggleButton);
			PackStart(segment);

			return segment;
		}
	}
}

