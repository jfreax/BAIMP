using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;

namespace Baimp
{
	[Serializable]
	public class MarkerEdge : Edge
	{
		static Color color = Color.FromBytes(123, 119, 230);
		const double absMinLength = 23;
		/// <summary>
		/// A number between 0 and 1.
		/// 0.0 means, we clicked on the "from"-side of the edge
		/// 1.0, on the "to" side.
		/// Set on click event.
		/// </summary>
		[XmlIgnore]
		public double r;

		public MarkerEdge()
		{

		}

		public MarkerEdge(Node to)
			: base(to)
		{
		}

		/// <summary>
		/// Draws a edge from one marker, to another
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="from">From.</param>
		public void Draw(Context ctx, MarkerNode from)
		{
			ctx.SetColor(color);
			ctx.SetLineWidth(5.0);

			ComputeStroke(ctx, from).Stroke();
		}

		public Context ComputeStroke(Context ctx, MarkerNode from)
		{
			Rectangle fromBound = from.Bounds;
			Rectangle toBound = to.Bounds;

			ctx.MoveTo(fromBound.Center.X, fromBound.Center.Y);

			double horizontalSpace = Math.Abs(fromBound.Center.X - toBound.Center.X);
			double verticalSpace = Math.Abs(fromBound.Center.Y - toBound.Center.Y);

			double minLength = absMinLength;
			// if to-node is right of from-node
			if (toBound.Center.X - fromBound.Center.X >= absMinLength * 2) {
				if (horizontalSpace > verticalSpace) {
					minLength = Math.Max((horizontalSpace - verticalSpace) / 2, absMinLength);
				}
				if (verticalSpace > (horizontalSpace - 2 * minLength)) {
					minLength = horizontalSpace / 2;
				}

				if (verticalSpace > (horizontalSpace - minLength) &&
				    minLength > absMinLength) {
					double minSpace = Math.Min(horizontalSpace - absMinLength, verticalSpace + absMinLength);
					int verticalNeg = fromBound.Center.Y - toBound.Center.Y < 0 ? 1 : -1;

					ctx.LineTo(fromBound.Center.X + absMinLength, fromBound.Center.Y); // fromNode-
					if (minSpace / 2 - absMinLength > 0) {
						// \
						ctx.LineTo(fromBound.Center.X + minSpace / 2, fromBound.Center.Y + ((minSpace / 2 - absMinLength) * verticalNeg));
					} 
					if (minSpace / 2 - absMinLength > -minSpace / 2) {
						ctx.LineTo(fromBound.Center.X + Math.Max(minSpace / 2, absMinLength), toBound.Center.Y - minSpace / 2 * verticalNeg); // |
						ctx.LineTo(fromBound.Center.X + Math.Max(minSpace, absMinLength), toBound.Center.Y); // \
					} else {
						ctx.LineTo(fromBound.Center.X + absMinLength, toBound.Center.Y); // |
					}
					ctx.LineTo(toBound.Center.X, toBound.Center.Y); // -toNode

				} else {
					ctx.LineTo(fromBound.Center.X + minLength, fromBound.Center.Y);
					ctx.LineTo(toBound.Center.X - minLength, toBound.Center.Y);
					ctx.LineTo(toBound.Center.X, toBound.Center.Y);
				}
			} else { // to node is left of from node
				MarkerNode toNode = to as MarkerNode;
				if (toNode != null) {
					PipelineNode parent = toNode.parent;
					ctx.LineTo(fromBound.Center.X + absMinLength, fromBound.Center.Y); // fromNode-

					if (toBound.Center.Y - fromBound.Center.Y > 0) {
						ctx.LineTo(fromBound.Center.X + absMinLength, parent.BoundWithExtras.Top); // | 
						ctx.LineTo(parent.bound.Left - minLength, parent.BoundWithExtras.Top); // <- 
					} else {
						ctx.LineTo(fromBound.Center.X + absMinLength, parent.BoundWithExtras.Bottom); // | 
						ctx.LineTo(parent.bound.Left - minLength, parent.BoundWithExtras.Bottom); // <- 
					}
					ctx.LineTo(parent.bound.Left - minLength, toNode.Bounds.Center.Y); // |
					ctx.LineTo(toNode.Bounds.Center.X, toNode.Bounds.Center.Y); // ->
				}
			}

			return ctx;
		}

		#region implemented abstract members of Edge

		public override void Draw(Context ctx)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

