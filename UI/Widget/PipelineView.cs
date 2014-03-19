using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Collections;

namespace baimp
{
	public class PipelineView : Canvas
	{
		private Graph<PipelineNode> graph;

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

			graph = new Graph<PipelineNode>();
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
				
			// draw all nodes
			IEnumerator enumerator = graph.GetEnumerator();
			while (enumerator.MoveNext()) {
				Node<PipelineNode> item = (Node<PipelineNode>) enumerator.Current;
				PipelineNode node = item.Value;

				DrawNode (ctx, node);
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
		private void DrawNode(Context ctx, PipelineNode node)
		{
			// draw rect
			ctx.RoundRectangle(node.position, nodeSize.Width, nodeSize.Height, 8);
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.Fill ();

			// draw text
			TextLayout text = new TextLayout ();
			Point textOffset = new Point(0, 8);

			text.Text = node.algorithm.ToString();
			if (text.GetSize().Width < nodeSize.Width) {
				textOffset.X = (nodeSize.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = nodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (text, node.position.Offset(textOffset));

			// set min size
//			if (MinHeight < overallHeight) {
//				MinHeight = overallHeight;
//			}
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

				PipelineNode node = new PipelineNode(algoInstance, e.Position);
				graph.AddNode(node);

				this.QueueDraw();

			} catch( Exception exception) {
				Console.WriteLine (exception.Message);
				e.Success = false;
			}
		}

		protected override void OnDragLeave (EventArgs args) {
			dropOverPosition = Point.Zero;
		}


		class PipelineNode
		{
			public PipelineNode(BaseAlgorithm algorithm, Point position)
			{
				this.algorithm = algorithm;
				this.position = position;
			}

			public BaseAlgorithm algorithm;
			public Point position;
		}
	}
}

