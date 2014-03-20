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
		private List<Node> nodes;
		private Dictionary<InOutMarker, List<InOutMarker>> edges;

		private Point nodeToMoveOffset = Point.Zero;
		private InOutMarker connectNodesStartMarker;
		private Point connectNodesEnd;

		private Point mousePosition = Point.Zero;
		private Node lastSelectedNode = null;
		private Edge lastSelectedEdge = null;

		private MouseAction mouseAction = MouseAction.None;

		private MouseMover mouseMover;

		CursorType oldCursor;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		/// <param name="scrollview">Parent scrollview</param>
		public PipelineView (ScrollView scrollview)
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.BackgroundColor = Colors.WhiteSmoke;
			this.CanGetFocus = true;

			nodes = new List<Node> ();
			edges = new Dictionary<InOutMarker, List<InOutMarker>> ();
			mouseMover = new MouseMover (scrollview);
			mouseMover.Timer = 20;

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
					Edge.DrawEdge (ctx, from, to);
				}
			}
				
			// draw all nodes
			foreach(Node node in nodes) {
				if (!mouseAction.HasFlag (MouseAction.MoveNode)  || node != lastSelectedNode) {
					if (node.bound.IntersectsWith (dirtyRect)) {
						node.Draw (ctx);
					}
				}
					
				// set min size
				Rectangle boundwe = node.BoundWithExtras;
				if (boundwe.Right > MinWidth) {
					MinWidth = boundwe.Right + Node.nodeMargin.Right;
				}
				if (boundwe.Bottom > MinHeight) {
					MinHeight = boundwe.Bottom + Node.nodeMargin.Bottom;
				}
				Point offset = new Point (Math.Max(0, -boundwe.Left), Math.Max(0, -boundwe.Top));
				if (offset != Point.Zero) {
					TranslateAllNodesBy (offset);
					QueueDraw ();
					return;
				}
			}


			// things to draw after
			if(mouseAction.HasFlag(MouseAction.MoveNode)) {
				lastSelectedNode.Draw (ctx); // draw currently moving node last

				// move scrollbar
				Rectangle boundwe = lastSelectedNode.BoundWithExtras;
				ScrollView sv = this.Parent as ScrollView;

				double viewportRight = sv.HorizontalScrollControl.Value + sv.Size.Width;
				double offsetH = (nodeToMoveOffset.X + boundwe.Width) * 0.5;
				if (boundwe.Right - offsetH > viewportRight) {
					sv.HorizontalScrollControl.Value += boundwe.Right - offsetH - viewportRight; 
				} else if (boundwe.Left + offsetH < sv.HorizontalScrollControl.Value) {
					sv.HorizontalScrollControl.Value -= sv.HorizontalScrollControl.Value - offsetH - boundwe.Left;
				}

				double viewportBottom = sv.VerticalScrollControl.Value + sv.Size.Height;
				double offsetV = (nodeToMoveOffset.Y + boundwe.Height) * 0.5;
				if (boundwe.Bottom - offsetV > viewportBottom) {
					sv.VerticalScrollControl.Value += boundwe.Bottom - offsetV - viewportBottom;
				} else if (boundwe.Top + offsetV < sv.VerticalScrollControl.Value) {
					sv.VerticalScrollControl.Value -= sv.VerticalScrollControl.Value - offsetV - boundwe.Top;
				}
			}
			if(mouseAction.HasFlag(MouseAction.AddEdge)) {
				ctx.MoveTo (connectNodesStartMarker.Bounds.Center);
				ctx.LineTo (connectNodesEnd);
				ctx.Stroke ();
			}
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

				Node node = new Node(algoInstance, new Rectangle(e.Position, Node.nodeSize));
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
				Node node = GetNodeAt (e.Position, true);
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
					Node nodeRight = GetNodeAt (e.Position, true);
					if (nodeRight != null) {
						lastSelectedNode = nodeRight;
						contextMenuNode.Popup ();
					}
				}
				break;
			case PointerButton.Middle:
				mouseMover.EnableMouseMover(e.Position);

				if(oldCursor != CursorType.Move) {
					oldCursor = this.Cursor;
					this.Cursor = CursorType.Move;
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
				InOutMarker inOutMarker = GetInOutMarkerAt (e.Position, new Size (Node.nodeInOutSpace, Node.nodeInOutSpace));
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

			switch (e.Button) {
			case PointerButton.Middle:
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover();
					this.Cursor = oldCursor;
				}
				break;
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
				InOutMarker marker = GetInOutMarkerAt (e.Position, new Size(Node.nodeInOutSpace, Node.nodeInOutSpace));
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

		protected override void OnMouseExited(EventArgs e)
		{
			if (mouseMover.Enabled) {
				mouseMover.DisableMouseMover();
				this.Cursor = oldCursor;
			}
		}


		protected override void OnKeyPressed(KeyEventArgs e) {
			switch (e.Key) {
			case Key.Delete:
				Edge edge = GetEdgeAt (mousePosition);
				if (edge != null) {
					RemoveEdge (edge);
					QueueDraw ();
				} else {
					Node node = GetNodeAt (mousePosition, true);
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
		private void SetNodeAt(Node nodeToMove, Point position) {
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
		private void SetNode(Node nodeToMove, int iteration = 20) {
			if (nodeToMove != null) {
				Node intersectingNode = GetNodeAt (nodeToMove.BoundWithExtras, nodeToMove, true);
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
		private Node GetNodeAt (Point position, bool withExtras = false)
		{
			foreach(Node node in nodes) {
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
		private Node GetNodeAt (Rectangle rectangle, Node ignoreNode = null, bool withExtras = false)
		{
			foreach(Node node in nodes) {
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
			foreach(Node node in nodes) {
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

		/// <summary>
		/// Translates all nodes by given offset.
		/// </summary>
		/// <param name="offset">Offset.</param>
		private void TranslateAllNodesBy(Point offset)
		{
			foreach (Node node in nodes) {
				node.bound = node.bound.Offset (offset);
			}
		}

		#endregion

	}
}

