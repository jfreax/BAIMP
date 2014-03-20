using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Collections;

namespace baimp
{
	enum MouseAction {
		None = 0,
		DragDrop = 1, // TODO
		MoveNode = 2,
		ConnectNodes = 3
	}

	public class PipelineView : Canvas
	{
		public static object NodeInOutMarkerSize {
			get;
			set;
		}

		private List<PipelineNode> nodes;
		private Dictionary<InOutMarker, List<InOutMarker>> edges;

		static protected WidgetSpacing nodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static protected Size nodeSize = new Size (200, 30);
		static protected Size nodeInOutMarkerSize = new Size (10, 8);
		static protected int nodeInOutSpace = 8;

		private PipelineNode nodeToMove = null;
		private Point nodeToMoveOffset = Point.Zero;
		private InOutMarker connectNodesStartMarker;
		private Point connectNodesEnd;

		private MouseAction mouseAction = MouseAction.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		public PipelineView ()
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.MinHeight = nodeSize.Height + nodeMargin.VerticalSpacing * 3;
			this.BackgroundColor = Colors.WhiteSmoke;

			nodes = new List<PipelineNode> ();
			edges = new Dictionary<InOutMarker, List<InOutMarker>> ();
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

			// draw all edges
			foreach (InOutMarker from in edges.Keys) {
				foreach (InOutMarker to in edges[from]) {
					DrawEdge (ctx, from, to);
				}
			}
				
			// draw all nodes
			foreach(PipelineNode node in nodes) {
				if (mouseAction != MouseAction.MoveNode || node != nodeToMove) {
					DrawNode (ctx, node);
				}
			}


			// things to draw after
			switch(mouseAction) {
			case MouseAction.None:
				break;
			case MouseAction.MoveNode:
				DrawNode (ctx, nodeToMove); // draw current moving node last
				break;
			case MouseAction.ConnectNodes:
				ctx.MoveTo (connectNodesStartMarker.Bounds.Center);
				ctx.LineTo (connectNodesEnd);
				ctx.Stroke ();
				break;
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
			DrawNodesInOutMarker (ctx, node);

			// change height of node if neccessary
			double inMarkerHeight = nodeInOutSpace +
				node.algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (inMarkerHeight > node.bound.Height) {
				node.bound.Height = inMarkerHeight;
			}

			double outMarkerHeight = nodeInOutSpace +
				node.algorithm.CompatibleOutput.Count * (nodeInOutSpace + nodeInOutMarkerSize.Height);
			if (outMarkerHeight > node.bound.Height) {
				node.bound.Height = outMarkerHeight;
			}

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


		/// <summary>
		/// Draws a edge from one marker, to another
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		private void DrawEdge(Context ctx, InOutMarker from, InOutMarker to) {
			ctx.SetColor (Colors.Black);

			ctx.MoveTo (from.Bounds.Center);
			ctx.LineTo (to.Bounds.Center);

			ctx.Stroke ();
		}


		/// <summary>
		/// Draws all marker of a specified node.
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="node">Node.</param>
		private void DrawNodesInOutMarker(Context ctx, PipelineNode node)
		{
			ctx.SetColor (Colors.DarkOrchid);
			int i = 0;
			foreach (string input in node.algorithm.CompatibleInput) {
				ctx.RoundRectangle (InOutMarker.GetBoundForInOutMarkerOf (node, i, true), 2);
				i++;
			}

			ctx.Fill ();

			ctx.SetColor (Colors.DarkKhaki);
			i = 0;
			foreach (string input in node.algorithm.CompatibleOutput) {
				ctx.RoundRectangle (InOutMarker.GetBoundForInOutMarkerOf (node, i, false), 2);
				i++;
			}

			ctx.Fill ();
		}


		/// <summary>
		/// Draw one markes on specified position and size
		/// </summary>
		/// <param name="ctx">Context.</param>
		/// <param name="position">Position</param>
		/// <param name="isInput">If set to <c>true</c>, then its an input marker</param>
		private void DrawMarker(Context ctx, Point position, bool isInput)
		{
			if (isInput) {
				ctx.SetColor (Colors.DarkOrchid);
			} else {
				ctx.SetColor (Colors.DarkKhaki);
			}

			Rectangle bounds = new Rectangle (
				position.X - (nodeInOutMarkerSize.Width * 0.5),
				position.Y - (nodeInOutMarkerSize.Height * 0.5),
				nodeInOutMarkerSize.Width,
				nodeInOutMarkerSize.Height
			);
			ctx.RoundRectangle (bounds, 2);
			ctx.Fill ();
		}

		#endregion

		#region drag and drop

		protected override void OnDragOver(DragOverEventArgs e)
		{
			e.AllowedAction = DragDropAction.Link;
		}
			
		protected override void OnDragDrop(DragEventArgs e)
		{
			e.Success = true;
			try {
				Type elementType = Type.GetType(e.Data.GetValue (TransferDataType.Text).ToString());
				BaseAlgorithm algoInstance = Activator.CreateInstance(elementType) as BaseAlgorithm;

				PipelineNode node = new PipelineNode(algoInstance, new Rectangle(e.Position, nodeSize));
				SetNode(node);
				nodes.Add(node);

				this.QueueDraw();

			} catch( Exception exception) {
				Console.WriteLine (exception.Message);
				e.Success = false;
			}
		}

		protected override void OnDragLeave (EventArgs args)
		{
		}

		#endregion

		#region events

		protected override void OnButtonPressed(ButtonEventArgs e)
		{
			switch (e.Button) {
			case PointerButton.Left:
				PipelineNode node = GetNodeAt (e.Position, true);
				if (node != null) {
					InOutMarker inOutMarker = InOutMarker.GetInOutMarkerAt (node, e.Position);
					if (inOutMarker != null) {
						connectNodesStartMarker = inOutMarker;
						mouseAction = MouseAction.ConnectNodes;
					} else {
						if (node.bound.Contains (e.Position)) {
							nodeToMoveOffset = new Point (
								node.bound.Location.X - e.Position.X,
								node.bound.Location.Y - e.Position.Y
							);
							nodeToMove = node;
							mouseAction = MouseAction.MoveNode;
						} 
					}
				}

				e.Handled = true;
				break;
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs e)
		{
			switch (mouseAction) {
			case MouseAction.MoveNode:
				SetNode (nodeToMove);
				break;
			case MouseAction.ConnectNodes:
				InOutMarker inOutMarker = GetInOutMarkerAt (e.Position, new Size(nodeInOutSpace, nodeInOutSpace));
				if (inOutMarker != null) {
					if (inOutMarker != connectNodesStartMarker &&
					   inOutMarker.isInput != connectNodesStartMarker.isInput) { // TODO check if compatible

						if (inOutMarker.isInput) {
							AddNode (inOutMarker, connectNodesStartMarker);
						} else {
							AddNode (connectNodesStartMarker, inOutMarker);
						}
					}
					
				}
				QueueDraw ();
				break;
			}

			mouseAction = MouseAction.None;
		}

		protected override void OnMouseMoved(MouseMovedEventArgs e)
		{
			switch(mouseAction) {
			case MouseAction.None:
				break;
			case MouseAction.MoveNode:
				if (nodeToMove != null) {
					nodeToMove.bound.Location = e.Position.Offset (nodeToMoveOffset);
					QueueDraw ();
				}
				break;
			case MouseAction.ConnectNodes:
				InOutMarker marker = GetInOutMarkerAt (e.Position, new Size(nodeInOutSpace, nodeInOutSpace));
				if (marker != null) {
					connectNodesEnd = marker.Bounds.Center;
				} else {
					connectNodesEnd = e.Position;
				}
				QueueDraw ();
				break;
			}

			if (mouseAction != MouseAction.MoveNode) {
				InOutMarker marker = GetInOutMarkerAt (e.Position);
				if (marker != null) {
					TooltipText = marker.type;
				} else {
					TooltipText = string.Empty;
				}
			}
		}


		#endregion

		#region getter and setter

		/// <summary>
		/// Sets new position for node. Moves the node, if another is already on this position.
		/// </summary>
		/// <param name="nodeToMove">Node to move.</param>
		/// <param name="position">Position.</param>
		/// <remarks>
		/// Automatically issues a recall
		/// </remarks>
		private void SetNodeAt(PipelineNode nodeToMove, Point position) {
			nodeToMove.bound.Location = position;
			SetNode (nodeToMove);
			QueueDraw ();
		}

		/// <summary>
		/// Sets new position for node. Moves the node, if another is already on this position.
		/// </summary>
		/// <param name="nodeToMove">Node to move.</param>
		/// <param name="iteration">Max number of iteration to find optimal placement.</param>
		/// <remarks>
		/// Automatically issues a redraw.
		/// After max number of iteration, place the node at the bottom of the graph.
		/// </remarks>
		private void SetNode(PipelineNode nodeToMove, int iteration = 20) {
			if (nodeToMove != null) {
				PipelineNode intersectingNode = GetNodeAt (nodeToMove.BoundWithExtras, nodeToMove, true);
				if (intersectingNode != null) {
					Rectangle intersect = 
						nodeToMove.BoundWithExtras.Intersect (intersectingNode.BoundWithExtras);

					if (iteration > 0 && intersect.Width < intersect.Height) {
						if (nodeToMove.bound.Left < intersectingNode.bound.Left) {
							nodeToMove.bound.Left -= intersect.Width;
						} else {
							nodeToMove.bound.Left += intersect.Width;
						}
					} else {
						if (iteration > 0 && nodeToMove.bound.Top < intersectingNode.bound.Top) {
							nodeToMove.bound.Top -= intersect.Height;
						} else {
							nodeToMove.bound.Top += intersect.Height;
						}
					}

					SetNode(nodeToMove, iteration - 1);
					QueueDraw ();
				}
			}
		}

		/// <summary>
		/// Get the node at a given position.
		/// </summary>
		/// <returns>The node at position; or null</returns>
		/// <param name="position">Position.</param>
		/// <param name="withExtras">Match not only main body of node, but also in/out marker</param>
		private PipelineNode GetNodeAt (Point position, bool withExtras = false)
		{
			foreach(PipelineNode node in nodes) {
				Rectangle bound = withExtras ? node.BoundWithExtras : node.bound;
				if (bound.Contains (position)) {
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the node that intersects a given rectangle
		/// </summary>
		/// <returns>The node; or null</returns>
		/// <param name="rectangle">Rectangle to test with.</param>
		/// <param name="ignorenode">Optional: Ignore this node.</param>
		/// <param name="withExtras">Match not only main body of node, but also in/out marker</param>
		private PipelineNode GetNodeAt (Rectangle rectangle, PipelineNode ignoreNode = null, bool withExtras = false)
		{
			foreach(PipelineNode node in nodes) {
				if (node != ignoreNode && 
					node.BoundWithExtras.IntersectsWith (rectangle)) {
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the in/out-marker at a given position.
		/// </summary>
		/// <returns>The marker description and their position.</returns>
		/// <param name="position">Position.</param>
		/// <param name="inflate">Inflate search region.</param>
		private InOutMarker GetInOutMarkerAt (Point position, Size? inflate = null)
		{
			foreach(PipelineNode node in nodes) {
				var ret = InOutMarker.GetInOutMarkerAt (node, position, inflate);
				if (ret != null) {
					return ret;
				}
			}

			return null;
		}
			
		#endregion

		#region helper

		private void AddNode(InOutMarker from, InOutMarker to)
		{
			if (!edges.ContainsKey (from)) {
				edges [from] = new List<InOutMarker> ();
			}

			edges [from].Add (to);
		}

		#endregion

		#region inline classes

		protected class PipelineNode
		{
			public PipelineNode(BaseAlgorithm algorithm, Rectangle bound)
			{
				this.algorithm = algorithm;
				this.bound = bound;
			}

			public BaseAlgorithm algorithm;
			public Rectangle bound;

			public Rectangle BoundWithExtras {
				get {
					return bound.Inflate (
						new Size(
							nodeInOutMarkerSize.Width + nodeMargin.HorizontalSpacing,
							nodeMargin.VerticalSpacing
						)
					);
				}
			}
		}

		protected class InOutMarker
		{
			private int nodeID;

			private PipelineNode parentNode;
			public string type;
			public bool isInput;

			/// <summary>
			/// Initializes a new instance of the <see cref="baimp.PipelineView+InOutMarker"/> class.
			/// </summary>
			/// <param name="parentNode">Parent node.</param>
			/// <param name="nodeID">Node ID.</param>
			/// <param name="type">Type.</param>
			/// <param name="isInput">If set to <c>true</c>, then its an input; otherwise output.</param>
			public InOutMarker(PipelineNode parentNode, int nodeID, string type, bool isInput)
			{
				this.parentNode = parentNode;
				this.nodeID = nodeID;
				this.type = type;
				this.isInput = isInput;
			}

			/// <summary>
			/// Get the in/out-marker of a specified node at a given position 
			/// </summary>
			/// <returns>The marker description and their position.</returns>
			/// <param name="node">Node.</param>
			/// <param name="position">Position.</param>
			/// <param name="inflate">Inflate search region.</param>
			static public InOutMarker GetInOutMarkerAt (PipelineNode node, Point position, Size? inflate = null)
			{
				Rectangle bound = node.BoundWithExtras;
				if (bound.Contains(position)) {
					for(int i = 0; i < node.algorithm.CompatibleInput.Count; i++) {
						Rectangle markerBound = GetBoundForInOutMarkerOf (node, i, true);

						if (markerBound.Inflate(inflate ?? Size.Zero).Contains (position)) {
							return new InOutMarker(node, i, node.algorithm.CompatibleInput [i], true);
						}
					}
					for(int i = 0; i < node.algorithm.CompatibleOutput.Count; i++) {
						Rectangle markerBound = GetBoundForInOutMarkerOf (node, i, false);

						if (markerBound.Inflate(inflate ?? Size.Zero).Contains (position)) {
							return new InOutMarker(node, i + node.algorithm.CompatibleInput.Count, node.algorithm.CompatibleOutput [i], false);
						}
					}
				}

				return null;
			}

			/// <summary>
			/// Gets the bound rectangle for an in/out-marker.
			/// </summary>
			/// <returns>The bound rectangle</returns>
			/// <param name="node">Node.</param>
			/// <param name="number">Number of marker.</param>
			/// <param name="isInput">If set to <c>true</c>, then its an input marker; otherwise output marker.</param>
			static public Rectangle GetBoundForInOutMarkerOf(PipelineNode node, int number, bool isInput )
			{
				return new Rectangle (
					new Point (
						isInput ? node.bound.Left - (nodeInOutMarkerSize.Width - 2) : node.bound.Right - 2, 
						node.bound.Top + (number * nodeInOutSpace) + ((number + 1) * nodeInOutMarkerSize.Height)
					), nodeInOutMarkerSize
				);
			}


			public Rectangle Bounds {
				get {
					int i = 
						nodeID >= parentNode.algorithm.CompatibleInput.Count ?
						nodeID - parentNode.algorithm.CompatibleInput.Count : nodeID;
					return GetBoundForInOutMarkerOf (parentNode, i, nodeID < parentNode.algorithm.CompatibleInput.Count);;
				}
			}
		}

		#endregion
	}
}

