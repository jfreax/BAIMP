using System;
using Xwt;
using Xwt.Drawing;

namespace baimp
{
	public class PipelineNodeView : Canvas
	{
		private PipelineNode node;

		static public WidgetSpacing NodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static public Size NodeSize = new Size(220, 48);
		static public Size NodeInOutSpace = new Size(8, 8);
		static public int NodeRadius = 2;
		static public Color NodeColor = Color.FromBytes(252, 252, 252);
		static public Color NodeColorBorder = Color.FromBytes(202, 202, 202);
		static public Color NodeColorShadow = Color.FromBytes(232, 232, 232);
		static public Color NodeColorProgress  = Color.FromBytes(190, 200, 250);

		private Image iconHide;
		private Image iconView;

		public Point contentOffset = Point.Zero;


		public PipelineNodeView(PipelineNode node)
		{
			this.node = node;

			iconHide = Image.FromResource("baimp.Resources.hide.png");
			iconView = Image.FromResource("baimp.Resources.view.png");

			this.BoundsChanged += delegate(object sender, EventArgs e) {
				Console.WriteLine("Changed for: " + node.algorithm);
			};

			this.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				Console.WriteLine("I was clicked!");
			};

			this.BackgroundColor = Colors.WhiteSmoke;

		}

		protected override void OnDraw(Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			Console.WriteLine(Bounds + " vs. " + BoundPosition);
			DrawBackground(ctx);
			DrawProgress(ctx);
			DrawHeader(ctx);
			DrawBody(ctx);
		}

		private void DrawBackground(Context ctx)
		{
			// draw shadow
			ctx.RoundRectangle(InnerBound.Offset(0, 3), NodeRadius);
			ctx.SetColor(NodeColorShadow);
			ctx.SetLineWidth(2);
			ctx.Fill();

			// border
			ctx.RoundRectangle(InnerBound.Inflate(-1, -1), NodeRadius);
			ctx.SetColor(NodeColorBorder);
			ctx.SetLineWidth(2);
			ctx.StrokePreserve();

			// background
			ctx.SetColor(NodeColor);
			ctx.Fill();
		}

		private void DrawProgress(Context ctx)
		{
			ctx.Save();

			Rectangle clipBound = Bounds.Inflate(-1, -1);
			clipBound.Width *= node.progress / 100.0;
			clipBound.Bottom = clipBound.Top + contentOffset.Y;
			ctx.Rectangle(clipBound);
			ctx.Clip();
			ctx.RoundRectangle(Bounds.Inflate(-1, -1), NodeRadius);
			ctx.SetColor(NodeColorProgress);
			ctx.Fill();

			ctx.Restore();
		}

		private void DrawHeader(Context ctx)
		{
			TextLayout text = new TextLayout();
			Point textOffset = new Point(0, 4);

			text.Text = node.algorithm.ToString();
			if (text.GetSize().Width < NodeSize.Width) {
				textOffset.X = (NodeSize.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = NodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			Point textPosition = InnerBound.Location.Offset(textOffset);

			ctx.SetColor(Colors.Black);
			ctx.DrawTextLayout(text, textPosition);

			// icons
			if (node.SaveResult) {
				ctx.DrawImage(
					iconView.WithBoxSize(text.GetSize().Height + 2).WithAlpha(0.6),
					InnerBound.Location.Offset(10, 3)
				);
			} else {
				ctx.DrawImage(
					iconHide.WithBoxSize(text.GetSize().Height + 2).WithAlpha(0.6),
					InnerBound.Location.Offset(10, 3)
				);
			}

			// stroke under headline
			contentOffset.X = 6;
			contentOffset.Y = textOffset.Y + text.GetSize().Height + 4;

			ctx.SetColor(NodeColorBorder);
			ctx.MoveTo(InnerBound.Location.Offset(contentOffset));
			ctx.LineTo(InnerBound.Right - 6, contentOffset.Y + InnerBound.Location.Y);
			ctx.SetLineWidth(1.0);
			ctx.Stroke();
		}

		private void DrawBody(Context ctx)
		{
			TextLayout text = new TextLayout();
			ctx.SetColor(Colors.Black);

			foreach (MarkerNode mNode in node.MNodes) {
				text.Text = mNode.compatible.ToString();
				mNode.Height = text.GetSize().Height;
				Point pos = mNode.Bounds.Location;
				if (mNode.IsInput) {
					pos.X = mNode.Bounds.Right + contentOffset.X;
				} else {
					pos.X = InnerBound.Right - contentOffset.X - text.GetSize().Width;
				}
				ctx.DrawTextLayout(text, pos);
				ctx.Stroke();

				// resize widget if necessary
				if (pos.Y + mNode.Height + NodeInOutSpace.Height > Bounds.Height) {
					Rectangle bound = BoundPosition;
					bound.Height += mNode.Height + NodeInOutSpace.Height;

					BoundPosition = bound;
					Console.WriteLine("Bound #21: " + bound);
					//Bounds.Bottom = pos.Y + mNode.Height + NodeInOutSpace.Height;
					//QueueDraw();
				}
			}
		}

		#region properties

		public Rectangle BoundPosition
		{
			get {
				return node.Parent.GetChildBounds(this);
			}
			set {
				node.Parent.SetChildBounds(this, value);
			}
		}

		public Rectangle BoundWithExtras
		{
			get {
				return BoundPosition.Inflate(
					new Size(
						MarkerNode.NodeInOutMarkerSize + NodeMargin.HorizontalSpacing,
						NodeMargin.VerticalSpacing
					)
				);
			}
		}

		public Rectangle InnerBound
		{
			get {
				return Bounds.Inflate(-MarkerNode.NodeInOutMarkerSize, -MarkerNode.NodeInOutMarkerSize);
			}
		}

		public Point ContentOffset {
			get {
				return contentOffset;
			}
			set {
				contentOffset = value;
			}
		}
		#endregion
	}
}

