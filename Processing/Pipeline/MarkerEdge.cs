//
//  MarkerEdge.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;

namespace Baimp
{
	[Serializable]
	public class MarkerEdge : Edge
	{
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
			MarkerNode toNode = to as MarkerNode;
			if (toNode != null) {

				ComputeStroke(ctx, from, new Point(2.5, 2.5));

				ctx.SetLineWidth(4.5);
				ctx.SetColor(Colors.DimGray.WithAlpha(0.3));
				ctx.Stroke();

				ComputeStroke(ctx, from);

				ctx.SetLineWidth(6);
				ctx.SetColor(Colors.DimGray.WithAlpha(0.7));
				ctx.StrokePreserve();

				ctx.SetLineWidth(4.0);
				ctx.SetColor(from.NodeColor.BlendWith(toNode.NodeColor, 0.5).WithAlpha(1.0));
				ctx.Stroke();
			}
		}

		public Context ComputeStroke(Context ctx, MarkerNode from, Point offset = default(Point))
		{
			Rectangle fromBound = from.Bounds.Offset(offset);
			Rectangle toBound = to.Bounds.Offset(offset);

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

