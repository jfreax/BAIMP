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

		bool isDisabled = false;
		bool isHover = false;
		bool isPressed = false;

		public ControllButton(Image icon)
		{
			this.icon = icon;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (isDisabled) {
				ctx.DrawImage(bgDisabled, Point.Zero);
			} else if (isPressed) {
				ctx.DrawImage(bgPressed, Point.Zero);
			} else if (isHover) {
				ctx.DrawImage(bgHover, Point.Zero);
			} else {
				ctx.DrawImage(bgNormal, Point.Zero);
			}

			if (isDisabled) {
				ctx.DrawImage(icon.WithAlpha(0.6), Point.Zero);
			} else {
				ctx.DrawImage(icon, Point.Zero);
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
			return bgNormal.Size;
		}

		public void Disable()
		{
			isDisabled = true;
		}

		public void Enabled()
		{
			isDisabled = false;
		}
	}
}
