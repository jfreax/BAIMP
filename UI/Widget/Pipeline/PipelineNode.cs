using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;


namespace baimp
{
	public class PipelineNode : List<MarkerNode>
	{
		static public WidgetSpacing NodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static public Size NodeSize = new Size (200, 40);
		static public Size NodeInOutSpace = new Size(8, 8);

		public BaseAlgorithm algorithm;
		public Rectangle bound;


		public PipelineNode(Type algoType, Rectangle bound)
		{
			BaseAlgorithm algoInstance = Activator.CreateInstance(algoType, this) as BaseAlgorithm;

			this.algorithm = algoInstance;
			this.bound = bound;

			int i = 0;
			foreach(Compatible c in algorithm.CompatibleInput) {
				this.Add (new MarkerNode (this, c, i, true));
				i++;
			}
			i = 0;
			foreach(Compatible c in algorithm.CompatibleOutput) {
				this.Add (new MarkerNode (this, c, i, false));
				i++;
			}
		}

		#region draw

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		public void Draw(Context ctx)
		{
			foreach(MarkerNode mNode in this) {
				mNode.Draw (ctx);
			}

			// change height of node if neccessary
			double inMarkerHeight = NodeInOutSpace.Height +
				algorithm.CompatibleOutput.Count * (NodeInOutSpace.Height + MarkerNode.NodeInOutMarkerSize.Height);
			if (inMarkerHeight > bound.Height) {
				bound.Height = inMarkerHeight;
			}

			double outMarkerHeight = NodeInOutSpace.Width +
				algorithm.CompatibleOutput.Count * (NodeInOutSpace.Width + MarkerNode.NodeInOutMarkerSize.Height);
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
			if (text.GetSize().Width < NodeSize.Width) {
				textOffset.X = (NodeSize.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = NodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (text, bound.Location.Offset(textOffset));
		}


		#endregion

		public MarkerNode GetInOutMarkerAt(Point position)
		{
			foreach (MarkerNode mNode in this) {
				if(mNode.Bounds.Contains(position)) {
					return mNode;
				}
			}

			return null;
		}

		#region properties

		public Rectangle BoundWithExtras {
			get {
				return bound.Inflate (
					new Size(
						MarkerNode.NodeInOutMarkerSize.Width + NodeMargin.HorizontalSpacing,
						NodeMargin.VerticalSpacing
					)
				);
			}
		}

		#endregion
	}
}

