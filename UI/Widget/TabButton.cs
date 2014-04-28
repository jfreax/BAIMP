//
//  TabButton.cs
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
	public class TabButton : Canvas
	{
		static readonly Image closeNormal = 
			Image.FromResource("Baimp.Resources.btClose.png").WithBoxSize(14.0);
		static readonly Image closeSelected = 
			Image.FromResource("Baimp.Resources.btClose-Selected.png").WithBoxSize(14.0);
		readonly TextLayout text = new TextLayout();
		Color deactiveColor = Color.FromBytes(208, 208, 208);
		Color activeColor = Color.FromBytes(159, 176, 193);
		Color borderColor = Color.FromBytes(182, 182, 182);
		Color deactiveTextColor = Color.FromBytes(64, 64, 64);
		WidgetSpacing padding;
		TabButton previous;
		TabButton next;
		Distance lean;
		bool active;
		bool closeable;
		bool hovered;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.TabButton"/> class.
		/// </summary>
		public TabButton()
		{
			Previous = null;
			Next = null;

			Managed = false;
			Multiple = true;
			Closeable = false;

			Lean = new Distance(5, 5);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.TabButton"/> class
		/// and sets the label.
		/// </summary>
		/// <param name="label">Label.</param>
		public TabButton(string label) : this()
		{
			Label = label;
		}

		/// <summary>
		/// Raises the draw event.
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="dirtyRect">Dirty rect.</param>
		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			ctx.SetLineWidth(1.0);

			if (previous != null && !previous.Active) {
				ctx.SetColor(deactiveColor);
				if (Multiple) {
					ctx.Rectangle(0, 0, Lean.Dx * 2, Size.Height);
					ctx.Fill();

					ctx.SetColor(borderColor);
					ctx.MoveTo(0, 0);
					ctx.LineTo(Lean.Dx * 2, 0);
					ctx.Stroke();
				} else {
					ctx.MoveTo(0, 0);
					ctx.CurveTo(
						0, 0,
						Lean.Dx, 0,
						Lean.Dx, Lean.Dy);
					ctx.LineTo(Lean.Dx, Size.Height - Lean.Dy);
					ctx.CurveTo(
						Lean.Dx, Size.Height - Lean.Dy,
						Lean.Dx, Size.Height + 0.5,
						Lean.Dx * 2, Size.Height + 0.5);
					ctx.LineTo(0, Size.Height);

					ctx.FillPreserve();

					ctx.SetColor(borderColor);
					ctx.Stroke();
				}
			}

			if (previous == null || Multiple || !previous.Active) {
				ctx.MoveTo(0, Size.Height + 0.5);
				ctx.CurveTo(
					0, Size.Height + 0.5,
					Lean.Dx, Size.Height + 0.5,
					Lean.Dx, Size.Height - Lean.Dy);
			} else {
				ctx.MoveTo(Lean.Dx, Size.Height);
			}
			ctx.LineTo(Lean.Dx, Lean.Dy);
			ctx.CurveTo(
				Lean.Dx, Lean.Dy,
				Lean.Dx, 0,
				Lean.Dx * 2, 0);
	
			if (next == null) {
				ctx.LineTo(Size.Width - (Lean.Dx * 2), 0);
				ctx.CurveTo(
					Size.Width - (Lean.Dx * 2), 0,
					Size.Width - Lean.Dx, 0,
					Size.Width - Lean.Dx, Lean.Dy);
				ctx.LineTo(Size.Width - Lean.Dx, Size.Height - Lean.Dy);
				ctx.CurveTo(
					Size.Width - Lean.Dx, Size.Height - Lean.Dy, 
					Size.Width - Lean.Dx, Size.Height,
					Size.Width, Size.Height);
			} else {
				ctx.LineTo(Size.Width, 0);
				ctx.LineTo(Size.Width, Size.Height);
			}

			ctx.SetColor(Active ? activeColor : deactiveColor);
			ctx.Fill();

			// border
			if (previous == null || Multiple || !previous.Active) {
				ctx.MoveTo(0, Size.Height + 0.5);
				ctx.CurveTo(
					0, Size.Height + 0.5,
					Lean.Dx, Size.Height + 0.5,
					Lean.Dx, Size.Height - Lean.Dy);
			} else {
				ctx.MoveTo(Lean.Dx, Size.Height + 0.5 - Lean.Dy);
			}

			ctx.LineTo(Lean.Dx, Lean.Dy);
			ctx.CurveTo(
				Lean.Dx, Lean.Dy,
				Lean.Dx, 0, 
				Lean.Dx * 2, 0);

			if (next == null) {
				ctx.LineTo(Size.Width - (Lean.Dx * 2), 0);
				ctx.CurveTo(
					Size.Width - (Lean.Dx * 2), 0,
					Size.Width - Lean.Dx, 0,
					Size.Width - Lean.Dx, Lean.Dy);
				ctx.LineTo(Size.Width - Lean.Dx, Size.Height - Lean.Dy);
				ctx.CurveTo(
					Size.Width - Lean.Dx, Size.Height - Lean.Dy,
					Size.Width - Lean.Dx, Size.Height + 0.5,
					Size.Width, Size.Height + 0.5);
			} else {
				ctx.LineTo(Size.Width, 0);
			}

			ctx.SetColor(borderColor);
			ctx.Stroke();

			if (previous != null && previous.Active) {
				ctx.SetColor(activeColor);
				if (Multiple) {
					ctx.Rectangle(0, 0, Lean.Dx * 2, Size.Height);
					ctx.Fill();

					ctx.SetColor(borderColor);
					ctx.MoveTo(0, 0);
					ctx.LineTo(Lean.Dx * 2, 0);
					ctx.Stroke();
				} else {
					ctx.MoveTo(0, 0);
					ctx.CurveTo(
						0, 0,
						Lean.Dx, 0,
						Lean.Dx, Lean.Dy);
					ctx.LineTo(Lean.Dx, Size.Height - Lean.Dy);
					ctx.CurveTo(
						Lean.Dx, Size.Height - Lean.Dy,
						Lean.Dx, Size.Height + 0.5,
						Lean.Dx * 2, Size.Height + 0.5);
					ctx.LineTo(0, Size.Height);

					ctx.FillPreserve();

					ctx.SetColor(borderColor);
					ctx.Stroke();
				}
			}

			// text
			ctx.SetColor(Active ? Colors.AliceBlue : deactiveTextColor);
			ctx.DrawTextLayout(text, new Point(padding.Left + Lean.Dx + 2, padding.Top));

			// close button
			if (Closeable) {
				ctx.DrawImage(Hovered ? closeSelected : closeNormal, 
					new Point(
						Size.Width - closeNormal.Width - (next == null ? padding.Right + Lean.Dx : 0), 
						(Size.Height - closeNormal.Height) / 2
					)
				);
			}
		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			Size size = text.GetSize();
			size.Width += padding.HorizontalSpacing + Lean.Dx + 2;
			if (next == null) {
				size.Width += Lean.Dx + 2;
			}

			if (Closeable) {
				size.Width += closeNormal.Width;
			}

			size.Height += padding.VerticalSpacing;
			return size;
		}

		protected override void OnBoundsChanged()
		{
			base.OnBoundsChanged();

			padding = new WidgetSpacing(padding.Left, (Size.Height - text.GetSize().Height) / 2, padding.Right, 0);
		}

		void OnTabClosed(object sender, CloseEventArgs e)
		{
			if (closeEvent != null) {
				closeEvent(sender, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (Next != null) {
				Next.Previous = previous;
				if (Active) {
					Next.Active = true;
				}
			} else if (Previous != null) {
				if (Active) {
					if (Next != null) {
						Next.Active = true;
					} else {
						Previous.Active = true;
					}
				}
				Previous.Next = Next;
			}

			base.Dispose(disposing);
		}

		#region Mouse events

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			base.OnButtonPressed(args);

			switch (args.Button) {
			case PointerButton.Left:
				Rectangle closeRegion = new Rectangle(
					                        Size.Width - closeNormal.Width - (next == null ? padding.Right + Lean.Dx : 0), 
					                        (Size.Height - closeNormal.Height) / 2,
					                        closeNormal.Width, closeNormal.Height
				                        );

				if (closeRegion.Contains(args.Position)) {
					OnTabClosed(this, new CloseEventArgs());
				} else {
					if (!Managed) {
						Active = !Active;
					}
					if (toggleEvent != null) {
						toggleEvent(this, EventArgs.Empty);
					}
				}

				break;
			}
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			base.OnMouseEntered(args);
			Hovered = true;
		}

		protected override void OnMouseExited(EventArgs args)
		{
			base.OnMouseExited(args);
			Hovered = false;
		}

		#endregion

		#region Custom events

		EventHandler<EventArgs> toggleEvent;

		/// <summary>
		/// Occurs when the active status of this button changed
		/// </summary>
		public event EventHandler<EventArgs> Toggled {
			add {
				toggleEvent += value;
			}
			remove {
				toggleEvent -= value;
			}
		}

		EventHandler<CloseEventArgs> closeEvent;

		/// <summary>
		/// Occurs when the close button was pressed.
		/// Only available if closeable is set to true.
		/// </summary>
		public event EventHandler<CloseEventArgs> Closed {
			add {
				closeEvent += value;
			}
			remove {
				closeEvent -= value;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>The label.</value>
		public string Label {
			get {
				return text.Text;
			}
			set {
				text.Text = value;
				HeightRequest = text.GetSize().Height;
			}
		}

		public Distance Lean {
			get {
				return lean;
			}
			set {
				lean = value;
				if (next != null) {
					padding = new WidgetSpacing(Lean.Dx, 0, 0, 0);
				} else {
					padding = new WidgetSpacing(Lean.Dx, 0, Lean.Dx, 0);
				}

				OnPreferredSizeChanged();
				QueueDraw();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Baimp.TabButton"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active {
			get {
				return active;
			}
			set {
				active = value;
				QueueDraw();

				if (next != null) {
					next.QueueDraw();
				}
			}
		}

		/// <summary>
		/// Reference to previous button of th same group
		/// </summary>
		/// <value>The previous button.</value>
		/// <remarks>
		/// Is normally set by <see cref="Baimp.CustomTabHost"/>, so don't mess with this.
		/// </remarks>
		public TabButton Previous {
			get {
				return previous;
			}
			set {
				previous = value;
				if (previous != null) {
					previous.Next = this;
				}
			}
		}

		/// <summary>
		/// Reference to the next button of the same group.
		/// </summary>
		/// <value>The next button.</value>
		/// <remarks>
		/// Is normally set by <see cref="Baimp.CustomTabHost"/>, so don't mess with this.
		/// </remarks>
		public TabButton Next {
			get {
				return next;
			}
			set {
				next = value;
				if (next != null) {
					padding = new WidgetSpacing(Lean.Dx, 0, 0, 0);
				} else {
					padding = new WidgetSpacing(Lean.Dx, 0, Lean.Dx, 0);
				}
				OnBoundsChanged();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Baimp.TabButton"/> is managed by a
		/// <see cref="Baimp.CustomTabHost"/>.
		/// </summary>
		/// <value><c>true</c> if managed; otherwise, <c>false</c>.</value>
		public bool Managed {
			get;
			set;
		}

		/// <summary>
		/// Can multiple button of one group be selected at the same time?
		/// </summary>
		/// <value><c>true</c> if multiple selectable; otherwise, <c>false</c>.</value>
		public bool Multiple {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Baimp.TabButton"/> closeable.
		/// </summary>
		/// <value><c>true</c> if closeable; otherwise, <c>false</c>.</value>
		public bool Closeable {
			get {
				return closeable;
			}
			set {
				closeable = value;
				OnPreferredSizeChanged();
			}
		}

		/// <summary>
		/// Mouse hover status.
		/// </summary>
		/// <value><c>true</c> if hovered; otherwise, <c>false</c>.</value>
		bool Hovered {
			get {
				return hovered;
			}
			set {
				hovered = value;
				QueueDraw();
			}
		}

		#endregion
	}
}

