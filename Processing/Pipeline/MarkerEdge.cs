using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;

namespace Baimp
{
	[Serializable]
	public class MarkerEdge : Edge
	{
		private static Color color = Colors.Black.WithAlpha(0.8);
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

			Rectangle fromBound = from.Bounds;
			Rectangle toBound = to.Bounds;

			ctx.SetColor(color);
			ctx.SetLineWidth(8.0);
			ctx.MoveTo(fromBound.Center.X, fromBound.Center.Y);
			ctx.LineTo(toBound.Center.X, toBound.Center.Y);
			ctx.Stroke();

			ctx.SetColor(Color.FromBytes(123, 119, 230));
			ctx.SetLineWidth(6.0);
			ctx.MoveTo(fromBound.Center.X, fromBound.Center.Y);
			ctx.LineTo(toBound.Center.X, toBound.Center.Y);
			ctx.Stroke();
		}

		#region implemented abstract members of Edge

		public override void Draw(Context ctx)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

