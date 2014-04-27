//
//  LightImageWidget.cs
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
	public class LightImageWidget
	{
		public LightImageWidget(Image image)
		{
			this.Image = image;
			Visible = false;
		}

		public void OnButtonPressed(object sender, ButtonEventArgs e)
		{
			if (buttonPressed != null) {
				buttonPressed(sender, e);
			}
		}

		public Image Image {
			get;
			set;
		}

		public Rectangle Bounds {
			get;
			set;
		}

		public bool Visible {
			get;
			set;
		}


		EventHandler<ButtonEventArgs> buttonPressed;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ButtonEventArgs> ButtonPressed {
			add {
				buttonPressed += value;
			}
			remove {
				buttonPressed -= value;
			}
		}
	}
}

