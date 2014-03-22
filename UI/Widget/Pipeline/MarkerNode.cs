using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;

namespace baimp
{
	public class MarkerNode : Node
	{
		static public int NodeInOutMarkerSize = 10;
		static public int NodeInOutSpace = 18;

		[XmlIgnore]
		public readonly Compatible compatible;

		[XmlIgnore]
		public readonly PipelineNode parent;

		private int positionNo;

		public MarkerNode ()
		{
		}

		public MarkerNode (PipelineNode parent, Compatible compatible, int positionNo, bool isInput)
		{
			this.parent = parent;
			this.compatible = compatible;
			this.IsInput = isInput;

			this.positionNo = positionNo;
		}

		/// <summary>
		/// Draw the marker.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public override void Draw(Context ctx)
		{
			ctx.SetColor (PipelineNode.NodeColorBorder);

			Rectangle bndTmp = Bounds;
			ctx.SetLineWidth (1);
			ctx.MoveTo (bndTmp.Left, bndTmp.Center.Y);
			ctx.LineTo (bndTmp.Right, bndTmp.Center.Y);
			ctx.Stroke ();
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

			if (IsInput == otherNode.IsInput)
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
						IsInput ? parent.bound.Left - NodeInOutMarkerSize : parent.bound.Right,
						parent.bound.Y + parent.contentOffset.Y + (positionNo + 1) * NodeInOutSpace + positionNo * Height
					), new Size(NodeInOutMarkerSize, Height)
				);
			}

		}

		[XmlIgnore]
		public double Height {
			get;
			set;
		}

		[XmlAttribute("input")]
		public bool IsInput {
			get;
			set;
		}

		#endregion
	}
}

