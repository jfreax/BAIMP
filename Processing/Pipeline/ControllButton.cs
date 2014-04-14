using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButton : Canvas
	{
		Image bgNormal;
		Image bgHover;
		Image bgPressed;
		Image icon;

		bool isHover = false;
		bool isPressed = false;

		public ControllButton(Image bgNormal, Image bgHover, Image bgPressed, Image icon)
		{
			this.bgNormal = bgNormal;
			this.bgHover = bgHover;
			this.bgPressed = bgPressed;
			this.icon = icon;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (isPressed) {
				ctx.DrawImage(bgPressed, Point.Zero);
			} else if (isHover) {
				ctx.DrawImage(bgHover, Point.Zero);
			} else {
				ctx.DrawImage(bgNormal, Point.Zero);
			}

			ctx.DrawImage(icon, Point.Zero);
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			base.OnMouseEntered(args);

			isHover = true;
			QueueDraw();
		}

		protected override void OnMouseExited(EventArgs args)
		{
			base.OnMouseExited(args);

			isHover = false;
			QueueDraw();
		}

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			base.OnButtonPressed(args);

			isPressed = true;
			QueueDraw();
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
	}
}

