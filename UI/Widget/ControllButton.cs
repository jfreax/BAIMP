//
//  ControllButton.cs
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
	public class ControllButton : Canvas
	{
		static readonly Image bgNormal = Image.FromResource("Baimp.Resources.btExecuteBase-Normal.png");
		static readonly Image bgHover = Image.FromResource("Baimp.Resources.btExecuteBase-Hover.png");
		static readonly Image bgPressed = Image.FromResource("Baimp.Resources.btExecuteBase-Pressed.png");
		static readonly Image bgDisabled = Image.FromResource("Baimp.Resources.btExecuteBase-Disabled.png");

		Image icon;

		bool isDisabled;
		bool isHover;
		bool isPressed;

		public ControllButton(Image icon)
		{
			this.icon = icon;
			Size = bgNormal.Size;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (isDisabled) {
				ctx.DrawImage(bgDisabled.WithBoxSize(Size), Point.Zero);
			} else if (isPressed) {
				ctx.DrawImage(bgPressed.WithBoxSize(Size), Point.Zero);
			} else if (isHover) {
				ctx.DrawImage(bgHover.WithBoxSize(Size), Point.Zero);
			} else {
				ctx.DrawImage(bgNormal.WithBoxSize(Size), Point.Zero);
			}

			if (isDisabled) {
				ctx.DrawImage(icon.WithBoxSize(Size).WithAlpha(0.6), Point.Zero);
			} else {
				ctx.DrawImage(icon.WithBoxSize(Size), Point.Zero);
			}
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			if (!isDisabled) {
				base.OnMouseEntered(args);

				isHover = true;
				QueueDraw();
			}
		}

		protected override void OnMouseExited(EventArgs args)
		{
			base.OnMouseExited(args);

			isHover = false;
			QueueDraw();
		}

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			if (!isDisabled) {
				base.OnButtonPressed(args);

				isPressed = true;
				QueueDraw();
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs args)
		{
			base.OnButtonReleased(args);

			isPressed = false;
			QueueDraw();
		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return Size;
		}

		/// <summary>
		/// Disable this button.
		/// </summary>
		public void Disable()
		{
			isDisabled = true;
		}

		/// <summary>
		/// Enable this button.
		/// </summary>
		public void Enable()
		{
			isDisabled = false;
			isPressed = false;
			isHover = false;
		}

		public new Size Size {
			get;
			set;
		}
	}
}

