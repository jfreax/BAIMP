using System;
using Xwt;
using Xwt.Drawing;
using System.Xml.Serialization;

namespace baimp
{
	public class MarkerNodeView : Canvas
	{
		MarkerNode node;

		public MarkerNodeView(MarkerNode node)
		{
			this.node = node;
		}

		#region drawing

		/// <summary>
		/// Draw the marker.
		/// </summary>
		/// <param name="ctx">Context.</param>
		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{	
			base.OnDraw(ctx, dirtyRect);

			Console.WriteLine("Draw mNode " + Bounds);
			ctx.SetColor(PipelineNodeView.NodeColorBorder);

			Rectangle bndTmp = node.Bounds;
			ctx.SetLineWidth(1);
			ctx.MoveTo(bndTmp.Left, bndTmp.Center.Y);
			ctx.LineTo(bndTmp.Right, bndTmp.Center.Y);
			ctx.Stroke();
		}

		/// <summary>
		/// Draws the edges.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public void DrawEdges(Context ctx)
		{
			foreach (MarkerEdge edge in node.Edges) {
				edge.Draw(ctx, node);
			}
		}

		#endregion

		[XmlIgnore]
		public Rectangle BoundPosition {
			get {
				return node.parent.view.GetChildBounds(this);
			}
			set {
				node.parent.view.SetChildBounds(this, value);
			}
		}
	}
}

