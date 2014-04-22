using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class TabButton : Canvas
	{
		readonly TextLayout text = new TextLayout();

		public TabButton(string label)
		{
			Label = label;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			ctx.DrawTextLayout(text, Point.Zero);
		}

		protected override Size OnGetPreferredSize(SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return text.GetSize();
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

		public string Label {
			get {
				return text.Text;
			}
			set {
				text.Text = value;
			}
		}

		public bool Active {
			get;
			set;
		}

		#endregion
	}
}

