using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;

namespace baimp
{
	public class PipelineView : Canvas
	{
		TreeNode<BaseAlgorithm> tree;

		WidgetSpacing elementMargin = new WidgetSpacing(10, 5, 10, 5);
		Size elementSize = new Size (200, 30);


		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		public PipelineView ()
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.MinHeight = elementSize.Height + elementMargin.Top + elementMargin.Bottom;

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



			Stack<TreeNode<BaseAlgorithm>> stack = new Stack<TreeNode<BaseAlgorithm>>();
			stack.Push (tree);
			foreach (var child in stack.Pop()) {
				int siblings = child.Parent == null ? 0 : child.Parent.Children.Count - 1;
				this.DrawElement (ctx, tree.Data, child.Level, siblings, child.Position);
					

				foreach (var grandchild in child.Children ) {
					Console.WriteLine ("Push " + child.Children.Count);
					stack.Push (grandchild);
				}
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
		private void DrawElement(Context ctx, BaseAlgorithm algo, int depth, int numberOfSiblings, int bornPosition)
		{
			// set position and size
			Rectangle elementBound = new Rectangle (Point.Zero, elementSize);

			double overallHeight = 
				(elementBound.Height + elementMargin.Top + elementMargin.Bottom) * (numberOfSiblings+1);
			double spaceFromTop = 
				(elementBound.Height + elementMargin.Top + elementMargin.Bottom) * (bornPosition+1);

			elementBound.X = 
				(elementBound.Width + elementMargin.Left + elementMargin.Right) * depth;
			elementBound.Y = Bounds.Center.Y;
			elementBound.Y += (-overallHeight * 0.5) + spaceFromTop - elementBound.Height - elementMargin.Top;

			if (depth == 0) {
				elementBound.X += elementMargin.Left;
			}

			// draw rect
			ctx.RoundRectangle(elementBound, 8);
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.Fill ();

			// draw text
			TextLayout text = new TextLayout ();
			Point textOffset = new Point(0, 8);

			text.Text = algo.ToString() + " " + depth + " " + numberOfSiblings + " " + bornPosition;
			if (text.GetSize().Width < elementSize.Width) {
				textOffset.X = (elementBound.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = elementSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (text, elementBound.Location.Offset(textOffset));

			// set min size
			if (MinHeight < overallHeight) {
				MinHeight = overallHeight;
			}
			if (MinWidth < elementBound.Right + elementMargin.Right) {
				MinWidth = elementBound.Right + elementMargin.Right;
			}
		}


		/// <summary>
		/// Raises on DragOver event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnDragOver(DragOverEventArgs e)
		{
			e.AllowedAction = DragDropAction.Link;
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
	}
}

