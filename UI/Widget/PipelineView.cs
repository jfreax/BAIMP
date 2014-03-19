using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;

namespace baimp
{
	public class PipelineView : Canvas
	{
		private TreeNode<BaseAlgorithm> tree;

		protected WidgetSpacing nodeMargin = new WidgetSpacing(10, 5, 10, 5);
		protected Size nodeSize = new Size (200, 30);

		private Point dropOverPosition = Point.Zero;

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		public PipelineView ()
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.MinHeight = nodeSize.Height + nodeMargin.Top + nodeMargin.Bottom;

			this.BackgroundColor = Colors.WhiteSmoke;
		}


		/// <summary>
		/// Called when the widget needs to be redrawn
		/// </summary>
		/// <param name='ctx'>
		/// Drawing context
		/// </param>
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			if (Bounds.IsEmpty)
				return;

			if (tree == null)
				return;
				
			// draw all nodes
			Stack<TreeNode<BaseAlgorithm>> stack = new Stack<TreeNode<BaseAlgorithm>>();
			stack.Push (tree);
			foreach (var child in stack.Pop()) {
				int siblings = child.Parent == null ? 0 : child.Parent.Children.Count - 1;
				this.DrawNode (ctx, tree.Data, child.Level, siblings, child.Position);
					
				foreach (var grandchild in child.Children ) {
					stack.Push (grandchild);
				}
			}

			// draw marker on drag&drop action
			if (dropOverPosition != Point.Zero) {
				DrawDropMarker (ctx, dropOverPosition);
			}
		}


		/// <summary>
		/// Draws an element.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		/// <param name="algo">Algorithmus.</param>
		/// <param name="depth">Depth.</param>
		/// <param name="numberOfSiblings">Number of siblings.</param>
		/// <param name="bornPosition">Position in this depth.</param>
		private void DrawNode(Context ctx, BaseAlgorithm algo, int depth, int numberOfSiblings, int bornPosition)
		{
			// set position and size
			Rectangle nodeBound = new Rectangle (Point.Zero, nodeSize);

			double overallHeight = 
				(nodeBound.Height + nodeMargin.Top + nodeMargin.Bottom) * (numberOfSiblings+1);
			double spaceFromTop = 
				(nodeBound.Height + nodeMargin.Top + nodeMargin.Bottom) * (bornPosition+1);

			nodeBound.X = 
				(nodeBound.Width + nodeMargin.Left + nodeMargin.Right) * depth;
			nodeBound.Y = Bounds.Center.Y;
			nodeBound.Y += (-overallHeight * 0.5) + spaceFromTop - nodeBound.Height - nodeMargin.Top;

			if (depth == 0) {
				nodeBound.X += nodeMargin.Left;
			}

			// draw rect
			ctx.RoundRectangle(nodeBound, 8);
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.Fill ();

			// draw text
			TextLayout text = new TextLayout ();
			Point textOffset = new Point(0, 8);

			text.Text = algo.ToString() + " " + depth + " " + numberOfSiblings + " " + bornPosition;
			if (text.GetSize().Width < nodeSize.Width) {
				textOffset.X = (nodeBound.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = nodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (text, nodeBound.Location.Offset(textOffset));

			// set min size
			if (MinHeight < overallHeight) {
				MinHeight = overallHeight;
			}
			if (MinWidth < nodeBound.Right + nodeMargin.Right) {
				MinWidth = nodeBound.Right + nodeMargin.Right;
			}
		}

		private void DrawDropMarker(Context ctx, Point position)
		{

		}


		/// <summary>
		/// Raises on DragOver event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnDragOver(DragOverEventArgs e)
		{
			e.AllowedAction = DragDropAction.Link;

			dropOverPosition = e.Position;
			//DrawDropMarker (e.Position);
		}
			
		/// <summary>
		/// Raises on DragDrop event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnDragDrop(DragEventArgs e)
		{
			e.Success = true;
			try {
				Type elementType = Type.GetType(e.Data.GetValue (TransferDataType.Text).ToString());
				BaseAlgorithm algoInstance = Activator.CreateInstance(elementType) as BaseAlgorithm;
				if(tree == null) {
					tree = new TreeNode<BaseAlgorithm>(algoInstance);
				} else {
					tree.AddChild(algoInstance);
				}

				this.QueueDraw();

			} catch( Exception exception) {
				Console.WriteLine (exception.Message);
				e.Success = false;
			}
		}

		protected override void OnDragLeave (EventArgs args) {
			dropOverPosition = Point.Zero;
		}
	}
}

