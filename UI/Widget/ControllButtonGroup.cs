﻿using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButtonGroup : HBox
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
			
		public ControllButtonGroup()
		{
			this.Spacing = 0;
		}

		public ControllButtonGroup AddButton(Image icon)
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
			if (childCounter == 0) {
				PackStart(new ButtonSegment(SegmentType.Left, icon));
			} else {
				PackStart(new ButtonSegment(SegmentType.Right, icon));
			}

			childCounter++;
			return this;
		}

		private enum SegmentType {
			Left,
			Middle,
			Right
		}

		private class ButtonSegment : Canvas
		{
			SegmentType segmentType;
			Image icon;

			Image[] bg;
			bool isPressed = false;

			public ButtonSegment(SegmentType segmentType, Image icon)
			{
				SegmentType = segmentType;
				this.icon = icon.WithBoxSize(bg[0].Size);
			}

			protected override void OnDraw(Context ctx, Rectangle dirtyRect)
			{
				base.OnDraw(ctx, dirtyRect);

				if (isPressed) {
					ctx.DrawImage(bg[1], Point.Zero);
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
				QueueDraw();
			}

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
		}
	}
}

