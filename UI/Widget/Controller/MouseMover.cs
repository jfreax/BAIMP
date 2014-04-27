//
//  MouseMover.cs
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
	public class MouseMover
	{
		ScrollView scrollview;
		long lastMoveTimestamp;
		Point lastPosition;
		long timer = 50;
		bool enabled;

		public MouseMover()
		{
		}

		public MouseMover(ScrollView scrollview)
		{
			RegisterMouseMover(scrollview);
		}

		/// <summary>
		/// Registers a scrollview on which the mouse mover acts
		/// </summary>
		/// <param name="scrollview">Scrollview.</param>
		public void RegisterMouseMover(ScrollView scrollview)
		{
			this.scrollview = scrollview;
			this.enabled = false;
		}

		/// <summary>
		/// Enables the mouse mover.
		/// </summary>
		/// <param name="mousePosition">Initial mouse position.</param>
		public void EnableMouseMover(Point mousePosition)
		{
			if (scrollview != null) {
				this.lastPosition = mousePosition;
				if (MainClass.toolkitType == ToolkitType.Gtk) {
					scrollview.MouseMoved += MouseMovedGtk;
				} else {
					scrollview.MouseMoved += MouseMovedNotGtk;
				}

				enabled = true;
			}
		}

		/// <summary>
		/// Disables the mouse mover.
		/// </summary>
		public void DisableMouseMover()
		{
			if (scrollview != null) {
				if (MainClass.toolkitType == ToolkitType.Gtk) {
					scrollview.MouseMoved -= MouseMovedGtk;
				} else {
					scrollview.MouseMoved -= MouseMovedNotGtk;
				}

				enabled = false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Baimp.MouseMover"/> is enabled or not
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled {
			get { return enabled; }
		}

		/// <summary>
		/// Gets called on mouse move when enabled and gtk toolkit active
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedGtk(object sender, MouseMovedEventArgs e)
		{
			if (e.Timestamp - lastMoveTimestamp > timer) {
				e.Handled = true;
	
				Point oldPosition = lastPosition;

				double newScrollX = scrollview.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
				double newScrollY = scrollview.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

				scrollview.HorizontalScrollControl.Value =
					Math.Min(scrollview.HorizontalScrollControl.UpperValue - scrollview.VisibleRect.Width, newScrollX);
				scrollview.VerticalScrollControl.Value =
					Math.Min(scrollview.VerticalScrollControl.UpperValue - scrollview.VisibleRect.Height, newScrollY);
				
				lastMoveTimestamp = e.Timestamp;
			}
		}

		/// <summary>
		/// Gets called on mouse move when enabled
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedNotGtk(object sender, MouseMovedEventArgs e)
		{
			MouseMovedGtk(sender, e);
			lastPosition = e.Position;
		}

		/// <summary>
		/// Time between two move calls in ms
		/// </summary>
		/// <value>The timer.</value>
		public long Timer {
			get {
				return timer;
			}
			set {
				timer = value;
			}
		}
	}
}

