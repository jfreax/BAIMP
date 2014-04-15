using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public enum SegmentType {
		Left,
		Middle,
		Right
	}

	public class ButtonSegment : Canvas
	{
		static readonly Image[] bgLeft = {
			Image.FromResource("Baimp.Resources.btDebugBase-LeftCap-Normal.png"),
			Image.FromResource("Baimp.Resources.btDebugBase-LeftCap-Pressed.png")
		};
		static readonly Image[] bgMiddle = {
			Image.FromResource("Baimp.Resources.btDebugBase-MidCap-Normal.png"),
			Image.FromResource("Baimp.Resources.btDebugBase-MidCap-Pressed.png")
		};
		static readonly Image[] bgRight = {
			Image.FromResource("Baimp.Resources.btDebugBase-RightCap-Normal.png"),
			Image.FromResource("Baimp.Resources.btDebugBase-RightCap-Pressed.png")
		};

		SegmentType segmentType;
		Image icon;
		bool toggleButton;
		Image[] bg;
		bool isPressed = false;

		public ButtonSegment(SegmentType segmentType, Image icon, bool toggleButton = false)
		{
			SegmentType = segmentType;
			this.icon = icon.WithBoxSize(bg[0].Size);
			this.toggleButton = toggleButton;

			Active = false;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (isPressed || Active) {
				ctx.DrawImage(toggleButton && isPressed ? bg[1].WithAlpha(0.8) : bg[1], Point.Zero);
			} else {
				ctx.DrawImage(bg[0], Point.Zero);
			}

			ctx.DrawImage(icon, new Point(((icon.Width + bg[0].Width) / 2) - icon.Width, 0));
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

			if (toggleButton) {
				Toggle();
			}

			QueueDraw();
		}

		/// <summary>
		/// Gets or sets the type of the segment.
		/// </summary>
		/// <value>The type of the segment.</value>
		public SegmentType SegmentType {
			get {
				return segmentType;
			}
			set {
				segmentType = value;

				if (segmentType == SegmentType.Left) {
					bg = bgLeft;
				} else if (segmentType == SegmentType.Right) {
					bg = bgRight;
				} else {
					bg = bgMiddle;
				}

				QueueDraw();
			}
		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return bg[0].Size;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Baimp.ButtonSegment"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active {
			get;
			set;
		}

		/// <summary>
		/// Toggle active state.
		/// </summary>
		public void Toggle()
		{
			Active = !Active;
		}
	}
}

