using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class TabButton : Canvas
	{
		readonly TextLayout text = new TextLayout();
		Color deactiveColor = Color.FromBytes(208, 208, 208);
		Color activeColor = Colors.LightSlateGray;
		Color borderColor = Color.FromBytes(182, 182, 182);
		Color deactiveTextColor = Color.FromBytes(64, 64, 64);
		WidgetSpacing padding;

		TabButton previous;
		TabButton next;

		public TabButton()
		{
			Previous = null;
			Next = null;
		}

		public TabButton(string label) : this()
		{
			Label = label;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			const int lean = 5;

			ctx.SetLineWidth(1.0);

			// background
			if (previous != null) {
				ctx.SetColor(previous.Active ? activeColor : deactiveColor);
				ctx.Rectangle(0, 0, lean*2, Size.Height);
				ctx.Fill();

				ctx.SetColor(borderColor);
				ctx.MoveTo(0, 0);
				ctx.LineTo(lean*2, 0);
				ctx.Stroke();
			}
				
			ctx.MoveTo(0, Size.Height + 0.5);
			ctx.CurveTo(0, Size.Height + 0.5, lean, Size.Height + 0.5, lean, Size.Height - lean);
			ctx.LineTo(lean, lean);
			ctx.CurveTo(lean, lean, lean, 0, lean * 2, 0);
	
			if (next == null) {
				ctx.LineTo(Size.Width - (lean * 2), 0);
				ctx.CurveTo(Size.Width - (lean * 2), 0, Size.Width - lean, 0, Size.Width - lean, lean);
				ctx.LineTo(Size.Width - lean, Size.Height - lean);
				ctx.CurveTo(Size.Width - lean, Size.Height - lean, Size.Width - lean, Size.Height, Size.Width, Size.Height);
			} else {
				ctx.LineTo(Size.Width, 0);
				ctx.LineTo(Size.Width, Size.Height);
			}

			ctx.SetColor(Active ? activeColor : deactiveColor);
			ctx.Fill();

			// border
			ctx.MoveTo(0, Size.Height + 0.5);
			ctx.CurveTo(0, Size.Height + 0.5, lean, Size.Height + 0.5, lean, Size.Height - lean);
			ctx.LineTo(lean, lean);
			ctx.CurveTo(lean, lean, lean, 0, lean * 2, 0);

			if (next == null) {
				ctx.LineTo(Size.Width - (lean * 2), 0);
				ctx.CurveTo(Size.Width - (lean * 2), 0, Size.Width - lean, 0, Size.Width - lean, lean);
				ctx.LineTo(Size.Width - lean, Size.Height - lean);
				ctx.CurveTo(Size.Width - lean, Size.Height - lean, Size.Width - lean, Size.Height + 0.5, Size.Width, Size.Height + 0.5);
			} else {
				ctx.LineTo(Size.Width, 0);
			}

			ctx.SetColor(borderColor);
			ctx.Stroke();

			// text
			ctx.SetColor(Active ? Colors.AliceBlue : deactiveTextColor);
			ctx.DrawTextLayout(text, new Point(padding.Left, padding.Top));

		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			Size size = text.GetSize();
			size.Width += padding.HorizontalSpacing;
			size.Height += padding.VerticalSpacing;
			return size;
		}

		protected override void OnBoundsChanged()
		{
			base.OnBoundsChanged();

			padding = new WidgetSpacing(padding.Left, (Size.Height - text.GetSize().Height) / 2, padding.Right, 0);
		}

		#region Mouse events

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			base.OnButtonPressed(args);

			switch (args.Button) {
			case PointerButton.Left:
				Active = !Active;
				if (toggleEvent != null) {
					toggleEvent(this, EventArgs.Empty);
				}
				break;
			}
		}

		#endregion

		#region Custom events

		EventHandler<EventArgs> toggleEvent;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<EventArgs> Toggled {
			add {
				toggleEvent += value;
			}
			remove {
				toggleEvent -= value;
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

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Baimp.TabButton"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active {
			get;
			set;
		}

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

		public TabButton Next {
			get {
				return next;
			}
			set {
				next = value;
				if (next != null) {
					padding = new WidgetSpacing(8, 0, 0, 0);
				} else {
					padding = new WidgetSpacing(8, 0, 8, 0);
				}
			}
		}


		#endregion
	}
}

