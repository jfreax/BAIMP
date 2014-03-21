using System;
using Xwt.Drawing;
using Xwt;

namespace baimp
{
	public class MarkerNode : Node
	{
		static public int NodeInOutMarkerSize = 10;
		static public int NodeInOutSpace = 18;

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
		}

		/// <summary>
		/// Draw the marker.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public override void Draw(Context ctx)
		{
			ctx.SetColor (Colors.Black);

			ctx.Rectangle (Bounds);
			ctx.Fill ();
		}

		/// <summary>
		/// Draws the edges.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public void DrawEdges(Context ctx)
		{
			foreach (Edge edge in edges) {
				edge.Draw (ctx);
			}
		}

		/// <summary>
		/// Tests if another node is compatible with this one.
		/// Compatible == there can be a edge between this nodes.
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="another">The other compatible instance.</param>
		public bool Match(MarkerNode otherNode)
		{
			if (parent == otherNode.parent)
				return false;

			if (this == otherNode)
				return false;

			if (isInput == otherNode.isInput)
				return false;

			return compatible.Match (otherNode.compatible);
		}

		public void AddEdgeTo(Node otherNode)
		{
			edges.Add (new PipelineEdge(this, otherNode));
		}

		#region Properties

		public override Rectangle Bounds {
			get {
				return new Rectangle (
					new Point (
						isInput ? parent.bound.Left - NodeInOutMarkerSize : parent.bound.Right,
						parent.bound.Y + parent.contentOffset.Y + (positionNo + 1) * NodeInOutSpace + positionNo * Height
					), new Size(NodeInOutMarkerSize, Height)
				);
			}

		}

		public double Height {
			get;
			set;
		}

		#endregion
	}
}

