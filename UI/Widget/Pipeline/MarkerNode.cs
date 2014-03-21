using System;
using Xwt.Drawing;
using Xwt;

namespace baimp
{
	public class MarkerNode : Node
	{
		static public Size NodeInOutMarkerSize = new Size (10, 8);
		static public int NodeInOutSpace = 8;

		public readonly bool isInput;
		public readonly Compatible compatible;
		public readonly PipelineNode parent;

		private int positionNo;

		public MarkerNode (PipelineNode parent, Compatible compatible, int positionNo, bool isInput)
		{
			this.parent = parent;
			this.compatible = compatible;
			this.isInput = isInput;

			this.positionNo = positionNo;

			// TODO set position
//			bounds = new Rectangle (
//				position.X - (nodeInOutMarkerSize.Width * 0.5),
//				position.Y - (nodeInOutMarkerSize.Height * 0.5),
//				nodeInOutMarkerSize.Width,
//				nodeInOutMarkerSize.Height
//			);
		}

		public override void Draw(Context ctx) {
			foreach (Edge edge in edges) {
				edge.Draw (ctx);
			}

			ctx.SetColor (Colors.DarkKhaki);

			ctx.RoundRectangle (Bounds, 2);
			ctx.Fill ();
		}

		public void AddEdgeTo(Node otherNode)
		{
			edges.Add (new PipelineEdge(this, otherNode));
		}

		public override Rectangle Bounds {
			get {
				return new Rectangle (
					new Point (
						isInput ? parent.bound.Left - (NodeInOutMarkerSize.Width - 2) : parent.bound.Right - 2,
						parent.bound.Top + (positionNo * NodeInOutSpace) + ((positionNo + 1) * NodeInOutMarkerSize.Height)
					), NodeInOutMarkerSize
				);
			}

		}
	}
}

