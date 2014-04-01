using System;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class GridView : Canvas
	{
		double minWidthPerChild;
		double margin = 12.0;

		public GridView(double minWidthPerChild)
		{
			this.minWidthPerChild = minWidthPerChild;
		}

		protected override void OnDraw(Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);
		}

		public void Add(Widget widget)
		{
			AddChild(widget);

			RecalculatePosition();
		}

		public void AddRange(List<Widget> widgets)
		{
			foreach (Widget widget in widgets) {
				AddChild(widget);
			}

			RecalculatePosition();
		}

		private void RecalculatePosition()
		{
			int childCount = 0;
			foreach (Widget child in Children) {
				childCount++;
			}

			double w = minWidthPerChild;

			if (childCount * (minWidthPerChild+margin) < Parent.Size.Width) {
				w = (Parent.Size.Width - (childCount+1) * margin) / childCount;
			}

			double colRight = margin;
			double rowHeight = w + margin;
			int row = 0;
			foreach(Widget child in Children) {

				Rectangle newbound = new Rectangle(colRight, rowHeight * row + margin, w, w);
				colRight += w + margin;

				if (colRight + w > Parent.Size.Width) {
					colRight = margin;
					row++;
				}

				this.SetChildBounds(child, newbound);
			}

			this.HeightRequest = rowHeight * row + margin;

			QueueDraw();
		}


		protected override void OnChildPreferredSizeChanged()
		{
			base.OnChildPreferredSizeChanged();
			Console.WriteLine("1234");
		}

		protected override void OnChildPlacementChanged(Widget child)
		{
			base.OnChildPlacementChanged(child);
			Console.WriteLine("434656");
		}
			
	}
}

