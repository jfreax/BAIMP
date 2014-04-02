using System;
using System.Linq;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class GridView : ScrollView
	{
		Canvas canvas;

		double minWidthPerChild;
		double margin = 12.0;

		bool ignoreSizeChange;

		public GridView(double minWidthPerChild)
		{
			this.minWidthPerChild = minWidthPerChild;

			this.canvas = new Canvas();
			this.Content = canvas;

			canvas.HorizontalPlacement = WidgetPlacement.Center;
			canvas.VerticalPlacement = WidgetPlacement.Center;

			canvas.BoundsChanged += (object sender, EventArgs e) => RecalculatePosition();
		}
			

		/// <summary>
		/// Add a specified widget.
		/// </summary>
		/// <param name="widget">Widget.</param>
		public void Add(Widget widget)
		{
			canvas.AddChild(widget);

			RecalculatePosition();
		}

		/// <summary>
		/// Add a list of widgets.
		/// </summary>
		/// <param name="widgets">Widgets.</param>
		public void AddRange(List<Widget> widgets)
		{
			foreach (Widget widget in widgets) {
				canvas.AddChild(widget);
			}

			RecalculatePosition();
		}

		/// <summary>
		/// Remove all children
		/// </summary>
		public void Clear()
		{
			canvas.Clear();
		}

		/// <summary>
		/// Recalculates the position of all childs.
		/// </summary>
		private void RecalculatePosition()
		{
			int childCount = canvas.Children.Count();

			if (childCount == 1) {
				Widget child = canvas.Children.First();
				canvas.SetChildBounds(child, new Rectangle(0, 0, 10000, 10000));

				canvas.HorizontalPlacement = WidgetPlacement.Center;
				canvas.VerticalPlacement = WidgetPlacement.Center;
				return;
			}

			canvas.HorizontalPlacement = WidgetPlacement.Start;
			canvas.VerticalPlacement = WidgetPlacement.Start;

			ignoreSizeChange = true;


			double w = minWidthPerChild;

			if ( VisibleRect.Size.Width > 10) {
				int childPerRow = (int) Math.Min( VisibleRect.Size.Width / (minWidthPerChild + margin), childCount);
				if (childPerRow * (minWidthPerChild + margin) <  VisibleRect.Size.Width) {
					w = ( VisibleRect.Size.Width - (childPerRow + 1) * margin) / childPerRow;
				}
			}

			double colRight = margin;
			double rowHeight = w + margin;
			int row = 0;
			foreach(Widget child in canvas.Children) {

				Rectangle newbound = new Rectangle(colRight, rowHeight * row + margin, w, w);
				colRight += w + margin;

				if (colRight + w >  VisibleRect.Size.Width) {
					colRight = margin;
					row++;
				}

				canvas.SetChildBounds(child, newbound);
			}

			canvas.HeightRequest = rowHeight * (row+1) + margin;

			canvas.QueueDraw();
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

