using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButton : Canvas
	{
		static readonly Image bgNormal = Image.FromResource("Baimp.Resources.btExecuteBase-Normal.png");
		static readonly Image bgHover = Image.FromResource("Baimp.Resources.btExecuteBase-Hover.png");
		static readonly Image bgPressed = Image.FromResource("Baimp.Resources.btExecuteBase-Pressed.png");

		Image icon;

		bool isHover = false;
		bool isPressed = false;

		public ControllButton(Image icon)
		{
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

