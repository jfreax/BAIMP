using System;
using System.Linq;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class GridView : Canvas
	{
		double minWidthPerChild;
		double margin = 12.0;

		bool ignoreSizeChange;

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
			ignoreSizeChange = true;
			int childCount = Children.Count();

			Size parentSize = Parent.Size;
			ScrollView parentScroller = Parent as ScrollView;
			if (parentScroller != null) {
				if (parentScroller.VisibleRect.Width > 10) {
					parentSize = parentScroller.VisibleRect.Size;
				}
			}

			double w = minWidthPerChild;
			int childPerRow = (int) Math.Min(parentSize.Width / (minWidthPerChild + margin), childCount);
			if (childPerRow * (minWidthPerChild+margin) < parentSize.Width) {
				w = (parentSize.Width - (childPerRow+1) * margin) / childPerRow;
			}

			double colRight = margin;
			double rowHeight = w + margin;
			int row = 0;
			foreach(Widget child in Children) {

				Rectangle newbound = new Rectangle(colRight, rowHeight * row + margin, w, w);
				colRight += w + margin;

				if (colRight + w > parentSize.Width) {
					colRight = margin;
					row++;
				}

				this.SetChildBounds(child, newbound);
			}

			this.HeightRequest = rowHeight * row + margin;

			QueueDraw();
			ignoreSizeChange = false;
		}

		protected override void OnBoundsChanged()
		{
			if (!ignoreSizeChange) {
				base.OnBoundsChanged();
				RecalculatePosition();
			}
		}
	}
}

