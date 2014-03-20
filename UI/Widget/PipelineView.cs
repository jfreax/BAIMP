using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Collections;

namespace baimp
{
	[Flags]
	enum MouseAction {
		None = 0,
		DragDrop = 1, // TODO
		MoveNode = 2,
		AddEdge = 4,
		MoveEdge = 8
	}

	public class PipelineView : Canvas
	{
		public static object NodeInOutMarkerSize {
			get;
			set;
		}

		private List<PipelineNode> nodes;
		private Dictionary<InOutMarker, List<InOutMarker>> edges;

		static public WidgetSpacing nodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static public Size nodeSize = new Size (200, 40);
		static public Size nodeInOutMarkerSize = new Size (10, 8);
		static public int nodeInOutSpace = 8;

		private Point nodeToMoveOffset = Point.Zero;
		private InOutMarker connectNodesStartMarker;
		private Point connectNodesEnd;

		private Point mousePosition = Point.Zero;
		private PipelineNode lastSelectedNode = null;
		private Edge lastSelectedEdge = null;

		private MouseAction mouseAction = MouseAction.None;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		public PipelineView ()
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.BackgroundColor = Colors.WhiteSmoke;
			this.CanGetFocus = true;

			nodes = new List<PipelineNode> ();
			edges = new Dictionary<InOutMarker, List<InOutMarker>> ();

			InitializeContextMenus ();
		}

		private Menu contextMenuEdge;
		private Menu contextMenuNode;

		/// <summary>
		/// Initializes all context menus.
		/// </summary>
		private void InitializeContextMenus ()
		{
			contextMenuEdge = new Menu ();
			MenuItem contextMenuEdgeDelete = new MenuItem ("Delete edge");
			contextMenuEdgeDelete.Clicked += delegate(object sender, EventArgs e) {
				if(lastSelectedEdge != null) {
					RemoveEdge(lastSelectedEdge);
					QueueDraw ();
				}
			};
			contextMenuEdge.Items.Add (contextMenuEdgeDelete);


			contextMenuNode = new Menu ();
			MenuItem contextMenuNodeDelete = new MenuItem ("Delete node");
			contextMenuNodeDelete.Clicked += delegate(object sender, EventArgs e) {
				if(lastSelectedNode != null) {
					nodes.Remove(lastSelectedNode);
					QueueDraw ();
				}
			};
			contextMenuNode.Items.Add (contextMenuNodeDelete);
		}

		#endregion

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
				if (!mouseAction.HasFlag (MouseAction.MoveNode)  || node != lastSelectedNode) {
					DrawNode (ctx, node);
				}
			}


			// things to draw after
			if(mouseAction.HasFlag(MouseAction.MoveNode)) {
				DrawNode (ctx, lastSelectedNode); // draw current moving node last
			}
			if(mouseAction.HasFlag(MouseAction.AddEdge)) {
				ctx.MoveTo (connectNodesStartMarker.Bounds.Center);
				ctx.LineTo (connectNodesEnd);
				ctx.Stroke ();
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
				if (node != null) { // clicked on node
					InOutMarker inOutMarker = InOutMarker.GetInOutMarkerAt (node, e.Position);
					if (inOutMarker != null) {
						connectNodesStartMarker = inOutMarker;
						mouseAction |= MouseAction.AddEdge;
					} else {
						if (node.bound.Contains (e.Position)) {
							nodeToMoveOffset = new Point (
								node.bound.Location.X - e.Position.X,
								node.bound.Location.Y - e.Position.Y
							);
							lastSelectedNode = node;
							mouseAction |= MouseAction.MoveNode;
						} 
					}
				} else {
					Edge edge = GetEdgeAt (e.Position);
					if (edge != null) { // clicked on edge
						if (edge.r < 0.5) {
							connectNodesStartMarker = edge.from;
						} else {
							connectNodesStartMarker = edge.to;
						}
						RemoveEdge (edge);
						lastSelectedEdge = edge;
						mouseAction |= MouseAction.AddEdge | MouseAction.MoveEdge;
					}
				}

				e.Handled = true;
				break;

			case PointerButton.Right:
				lastSelectedEdge = GetEdgeAt (e.Position);
				if (lastSelectedEdge != null) {
					contextMenuEdge.Popup ();
				} else {
					PipelineNode nodeRight = GetNodeAt (e.Position, true);
					if(nodeRight != null) {
						lastSelectedNode = nodeRight;
						contextMenuNode.Popup ();
					}
				}

				break;
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs e)
		{
			if (mouseAction.HasFlag (MouseAction.MoveNode)) {
				SetNode (lastSelectedNode);
				mouseAction ^= MouseAction.MoveNode;
			}

			if (mouseAction.HasFlag (MouseAction.AddEdge)) {
				InOutMarker inOutMarker = GetInOutMarkerAt (e.Position, new Size (nodeInOutSpace, nodeInOutSpace));
				if (inOutMarker != null) {
					if (inOutMarker != connectNodesStartMarker &&
					    inOutMarker.isInput != connectNodesStartMarker.isInput) { // TODO check if compatible

						if (inOutMarker.isInput) {
							AddEdge (inOutMarker, connectNodesStartMarker);
						} else {
							AddEdge (connectNodesStartMarker, inOutMarker);
						}
					}
				} else if (mouseAction.HasFlag (MouseAction.MoveEdge) && lastSelectedEdge != null) {
					AddEdge (lastSelectedEdge);
					mouseAction ^= MouseAction.MoveEdge;
				}

				QueueDraw ();
				mouseAction ^= MouseAction.AddEdge;
			}
		}

		protected override void OnMouseMoved(MouseMovedEventArgs e)
		{
			mousePosition = e.Position;


			if (mouseAction.HasFlag (MouseAction.MoveNode)) {
				if (lastSelectedNode != null) {
					lastSelectedNode.bound.Location = e.Position.Offset (nodeToMoveOffset);
					QueueDraw ();
				}
			}
			if (mouseAction.HasFlag (MouseAction.AddEdge)) {
				InOutMarker marker = GetInOutMarkerAt (e.Position, new Size(nodeInOutSpace, nodeInOutSpace));
				if (marker != null) {
					connectNodesEnd = marker.Bounds.Center;
				} else {
					connectNodesEnd = e.Position;
				}

				QueueDraw ();
			}

			if (!mouseAction.HasFlag(MouseAction.MoveNode)) {
				InOutMarker marker = GetInOutMarkerAt (e.Position);
				if (marker != null) {
					TooltipText = marker.type;
				} else {
					TooltipText = string.Empty;
				}
			}
		}

		protected override void OnMouseEntered(EventArgs e)
		{
			this.SetFocus ();
		}

		protected override void OnKeyPressed(KeyEventArgs e) {
			switch (e.Key) {
			case Key.Delete:
				Edge edge = GetEdgeAt (mousePosition);
				if (edge != null) {
					RemoveEdge (edge);
					QueueDraw ();
				} else {
					PipelineNode node = GetNodeAt (mousePosition, true);
					if (node != null) {
						nodes.Remove (node);
						QueueDraw ();
					}
				}
				break;
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
		/// Gets the edge at position.
		/// </summary>
		/// <returns>The <see cref="System.Tuple`2[[baimp.PipelineView+InOutMarker],[baimp.PipelineView+InOutMarker]]"/>.</returns>
		/// <param name="position">Position.</param>
		private Edge GetEdgeAt(Point position)
		{
			double epsilon = 4.0;

			foreach (InOutMarker fromNode in edges.Keys) {
				Point from = fromNode.Bounds.Center;

				foreach (InOutMarker toNode in edges[fromNode]) {
					Point to = toNode.Bounds.Center;

					double segmentLengthSqr = (to.X - from.X) * (to.X - from.X) + (to.Y - from.Y) * (to.Y - from.Y);
					double r = ((position.X - from.X) * (to.X - from.X) + (position.Y - from.Y) * (to.Y - from.Y)) / segmentLengthSqr;
					if (r < 0 || r > 1) {
						continue;
					}
					double sl = ((from.Y - position.Y) * (to.X - from.X) - (from.X - position.X) * (to.Y - from.Y)) / System.Math.Sqrt(segmentLengthSqr);
					if (-epsilon <= sl && sl <= epsilon) {
						return new Edge (fromNode, toNode, from.X < to.X ? r : 1.0-r);
					}
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

		/// <summary>
		/// Adds a new edge.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		private void AddEdge(InOutMarker from, InOutMarker to)
		{
			if (!edges.ContainsKey (from)) {
				edges [from] = new List<InOutMarker> ();
			}

			edges [from].Add (to);
		}

		/// <summary>
		/// Adds a new edge.
		/// </summary>
		/// <param name="edge">Edge to add.</param>
		private void AddEdge(Edge edge)
		{
			AddEdge (edge.from, edge.to);
		}

		/// <summary>
		/// Removes a edge.
		/// </summary>
		/// <param name="edge">Edge.</param>
		private void RemoveEdge(Edge edge)
		{
			edges [edge.from].Remove (edge.to);
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

		protected class Edge
		{
			public InOutMarker from;
			public InOutMarker to;

			/// <summary>
			/// A number between 0 and 1.
			/// 0.0 means, we clicked on the "from"-side of the edge
			/// 1.0, on the "to" side.
			/// Only set on click event.
			/// </summary>
			public double r;

			public Edge(InOutMarker from, InOutMarker to, double r = 0.5) {
				this.from = from;
				this.to = to;
				this.r = r;
			}
		}

		#endregion
	}
}

