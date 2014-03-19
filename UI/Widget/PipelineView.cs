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
		protected Size nodeInOutMarkerSize = new Size (10, 8);
		protected int nodeInOutSpace = 8;

		private PipelineNode nodeToMove = null;
		private Point nodeToMoveOffset = Point.Zero;

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

		#region drawing

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
		}


		/// <summary>
		/// Draws an element.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		/// <param name="node">Node to draw.</param>
		private void DrawNode(Context ctx, PipelineNode node)
		{
			// draw in marker
			ctx.SetColor (Colors.DarkOrchid);
			int i = 0;
			foreach (string input in node.algorithm.CompatibleInput) {
				ctx.RoundRectangle (
					new Point (
						node.bound.Left - (nodeInOutMarkerSize.Width - 2), 
						node.bound.Top + (i * nodeInOutSpace) + ((i + 1) * nodeInOutMarkerSize.Height)
					),
					nodeInOutMarkerSize.Width, nodeInOutMarkerSize.Height, 2
				);
				i++;
			}

			double inMarkerHeight = nodeInOutSpace +
				node.algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (inMarkerHeight > node.bound.Height) {
				node.bound.Height = inMarkerHeight;
			}

			ctx.Fill ();

			// draw out marker
			ctx.SetColor (Colors.DarkKhaki);
			i = 0;
			foreach (string input in node.algorithm.CompatibleOutput) {
				ctx.RoundRectangle (
					new Point (
						node.bound.Right - 2, 
						node.bound.Top + (i * nodeInOutSpace) + ((i + 1) * nodeInOutMarkerSize.Height)
					),
					nodeInOutMarkerSize.Width, nodeInOutMarkerSize.Height, 2
				);
				i++;
			}

			double outMarkerHeight = nodeInOutSpace +
				node.algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (outMarkerHeight > node.bound.Height) {
				node.bound.Height = outMarkerHeight;
			}

			ctx.Fill ();


			// draw rect
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.RoundRectangle(node.bound, 4);
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
			ctx.DrawTextLayout (text, node.bound.Location.Offset(textOffset));

			// set min size
			if (node.bound.Right > MinWidth) {
				MinWidth = node.bound.Right + nodeMargin.Right;
			}
			if (node.bound.Bottom > MinHeight) {
				MinHeight = node.bound.Bottom + nodeMargin.Bottom;
			}
		}

		#endregion

		#region drag and drop

		protected override void OnDragOver(DragOverEventArgs e)
		{
			e.AllowedAction = DragDropAction.Link;

			dropOverPosition = e.Position;
		}
			
		protected override void OnDragDrop(DragEventArgs e)
		{
			e.Success = true;
			try {
				Type elementType = Type.GetType(e.Data.GetValue (TransferDataType.Text).ToString());
				BaseAlgorithm algoInstance = Activator.CreateInstance(elementType) as BaseAlgorithm;

				PipelineNode node = new PipelineNode(algoInstance, new Rectangle(e.Position, nodeSize));
				graph.AddNode(node);

				this.QueueDraw();

			} catch( Exception exception) {
				Console.WriteLine (exception.Message);
				e.Success = false;
			}
		}

		protected override void OnDragLeave (EventArgs args)
		{
			dropOverPosition = Point.Zero;
		}

		#endregion

		#region events

		protected override void OnButtonPressed(ButtonEventArgs e)
		{
			switch (e.Button) {
			case PointerButton.Left:
				nodeToMove = GetNodeAtPosition (e.Position);
				if (nodeToMove != null) {
					nodeToMoveOffset = new Point (
						nodeToMove.bound.Location.X - e.Position.X,
						nodeToMove.bound.Location.Y - e.Position.Y);
				}

				e.Handled = true;
				break;
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs e)
		{
			nodeToMove = null;
		}

		protected override void OnMouseMoved(MouseMovedEventArgs e)
		{
			if (nodeToMove != null) {
				nodeToMove.bound.Location = e.Position.Offset (nodeToMoveOffset);
				QueueDraw ();
			}
		}


		#endregion

		#region getter and setter

		PipelineNode GetNodeAtPosition (Point position)
		{
			IEnumerator enumerator = graph.GetEnumerator();
			while (enumerator.MoveNext ()) {
				Node<PipelineNode> item = (Node<PipelineNode>) enumerator.Current;
				PipelineNode node = item.Value;

				if (node.bound.Contains (position)) {
					return node;
				}
			}

			return null;
		}

		#endregion

		#region inline classes

		class PipelineNode
		{
			public PipelineNode(BaseAlgorithm algorithm, Rectangle bound)
			{
				this.algorithm = algorithm;
				this.bound = bound;
			}

			public BaseAlgorithm algorithm;
			public Rectangle bound;
		}

		#endregion
	}
}

