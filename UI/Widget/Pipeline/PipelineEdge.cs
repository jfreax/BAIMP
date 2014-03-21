using System;
using Xwt.Drawing;

namespace baimp
{
	public class PipelineEdge : Edge
	{
		private static Color color = Colors.Black.WithAlpha(0.2);

		/// <summary>
		/// A number between 0 and 1.
		/// 0.0 means, we clicked on the "from"-side of the edge
		/// 1.0, on the "to" side.
		/// Set on click event.
		/// </summary>
		public double r;

		public PipelineEdge (Node from, Node to)
			: base (from, to)
		{
		}

		/// <summary>
		/// Draws a edge from one marker, to another
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public override void Draw(Context ctx) {
			if (!Active) {
				return;
			}

			ctx.SetColor (color);

			ctx.MoveTo (from.Bounds.Center);

			ctx.SetLineWidth (3.0);
			ctx.LineTo (to.Bounds.Center);

			ctx.Stroke ();
		}
	}
}

