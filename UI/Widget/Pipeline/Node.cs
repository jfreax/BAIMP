using System;
using Xwt;
using Xwt.Drawing;


namespace baimp
{
	public class Node
	{
		static public WidgetSpacing nodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static public Size nodeSize = new Size (200, 40);
		static public Size nodeInOutMarkerSize = new Size (10, 8);
		static public int nodeInOutSpace = 8;

		public BaseAlgorithm algorithm;
		public Rectangle bound;

		private Canvas canvas;

		public Node(BaseAlgorithm algorithm, Rectangle bound)
		{
			this.algorithm = algorithm;
			this.bound = bound;

			canvas = new Canvas ();
		}

		#region draw

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		public void Draw(Context ctx)
		{
			// draw in marker
			DrawNodesInOutMarker (ctx);

			// change height of node if neccessary
			double inMarkerHeight = nodeInOutSpace +
				algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (inMarkerHeight > bound.Height) {
				bound.Height = inMarkerHeight;
			}

			double outMarkerHeight = nodeInOutSpace +
				algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (outMarkerHeight > bound.Height) {
				bound.Height = outMarkerHeight;
			}

			// draw rect
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.RoundRectangle(bound, 4);
			ctx.Fill ();

			// draw text
			TextLayout text = new TextLayout ();
			Point textOffset = new Point(0, 8);

			text.Text = algorithm.ToString();
			if (text.GetSize().Width < nodeSize.Width) {
				textOffset.X = (nodeSize.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = nodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (text, bound.Location.Offset(textOffset));
		}


		/// <summary>
		/// Draws all marker of a specified node.
		/// </summary>
		/// <param name="ctx">Context.</param>
		private void DrawNodesInOutMarker(Context ctx)
		{
			ctx.SetColor (Colors.DarkOrchid);
			int i = 0;
			foreach (string input in algorithm.CompatibleInput) {
				ctx.RoundRectangle (InOutMarker.GetBoundForInOutMarkerOf (this, i, true), 2);
				i++;
			}

			ctx.Fill ();

			ctx.SetColor (Colors.DarkKhaki);
			i = 0;
			foreach (string input in algorithm.CompatibleOutput) {
				ctx.RoundRectangle (InOutMarker.GetBoundForInOutMarkerOf (this, i, false), 2);
				i++;
			}

			ctx.Fill ();
		}

		/// <summary>
		/// Draw one markes on specified position and size
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="position">Position</param>
		/// <param name="isInput">If set to <c>true</c>, then its an input marker</param>
		private void DrawMarker(Context ctx, Point position, bool isInput)
		{
			if (isInput) {
				ctx.SetColor (Colors.DarkOrchid);
			} else {
				ctx.SetColor (Colors.DarkKhaki);
			}

			Rectangle bounds = new Rectangle (
				position.X - (nodeInOutMarkerSize.Width * 0.5),
				position.Y - (nodeInOutMarkerSize.Height * 0.5),
				nodeInOutMarkerSize.Width,
				nodeInOutMarkerSize.Height
			);
			ctx.RoundRectangle (bounds, 2);
			ctx.Fill ();
		}

		#endregion

		#region properties

		public Rectangle BoundWithExtras {
			get {
				return bound.Inflate (
					new Size(
						nodeInOutMarkerSize.Width + nodeMargin.HorizontalSpacing,
						nodeMargin.VerticalSpacing
					)
				);
			}
		}

		#endregion
	}
}

