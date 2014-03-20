using System;
using Xwt.Drawing;

namespace baimp
{
	public class Edge
	{
		public InOutMarker from;
		public InOutMarker to;

		/// <summary>
		/// A number between 0 and 1.
		/// 0.0 means, we clicked on the "from"-side of the edge
		/// 1.0, on the "to" side.
		/// Only set on click event.
		/// </summary>
		public double r;

		public Edge(InOutMarker from, InOutMarker to, double r = 0.5) {
			this.from = from;
			this.to = to;
			this.r = r;
		}

		/// <summary>
		/// Draws a edge from one marker, to another
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public void Draw(Context ctx) {
			ctx.SetColor (Colors.Black);

			ctx.MoveTo (from.Bounds.Center);
			ctx.LineTo (to.Bounds.Center);

			ctx.Stroke ();
		}

		/// <summary>
		/// Draws a edge from one marker, to another
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		static public void DrawEdge(Context ctx, InOutMarker fromArg, InOutMarker toArg) {
			ctx.SetColor (Colors.Black);

			ctx.MoveTo (fromArg.Bounds.Center);
			ctx.LineTo (toArg.Bounds.Center);

			ctx.Stroke ();
		}
	}
}

