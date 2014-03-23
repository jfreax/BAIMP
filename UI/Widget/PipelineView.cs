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
		DragDrop = 1,
		MoveNode = 2,
		AddEdge = 4,
		MoveEdge = 8
	}

	public class PipelineView : Canvas
	{
		private List<PipelineNode> nodes;

		private Point nodeToMoveOffset = Point.Zero;
		private MarkerNode connectNodesStartMarker;
		private Point connectNodesEnd;

		private Point mousePosition = Point.Zero;
		private PipelineNode lastSelectedNode = null;
		private Tuple<MarkerNode, MarkerEdge> lastSelectedEdge = null;

		private MouseAction mouseAction = MouseAction.None;

		private MouseMover mouseMover;

		CursorType oldCursor;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		/// <param name="scrollview">Parent scrollview</param>
		/// <param name="loadedNodes">Add already loaded nodes to this new instance</param>
		public PipelineView (ScrollView scrollview, List<PipelineNode> loadedNodes = null)
		{
			this.SetDragDropTarget (TransferDataType.Text);
			this.BackgroundColor = Colors.WhiteSmoke;
			this.CanGetFocus = true;

			if (loadedNodes == null) {
				nodes = new List<PipelineNode> ();
			} else {
				nodes = loadedNodes;
			}

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
					lastSelectedEdge.Item1.RemoveEdge(lastSelectedEdge.Item2);
					QueueDraw ();
				}
			};
			contextMenuEdge.Items.Add (contextMenuEdgeDelete);


			contextMenuNode = new Menu ();
			MenuItem contextMenuNodeDelete = new MenuItem ("Delete node");
			contextMenuNodeDelete.Clicked += delegate(object sender, EventArgs e) {
				if(lastSelectedNode != null) {
					RemoveNode (lastSelectedNode);
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
				
			// draw all nodes
			foreach(PipelineNode node in nodes) {
				if (!mouseAction.HasFlag (MouseAction.MoveNode)  || node != lastSelectedNode) {
					if (node.bound.IntersectsWith (dirtyRect)) {
						if (node.Draw (ctx)) {
							QueueDraw (node.bound);
						}
					}
				}
					
				// set canvas min size
				Rectangle boundwe = node.BoundWithExtras;
				if (boundwe.Right > MinWidth) {
					MinWidth = boundwe.Right + PipelineNode.NodeMargin.Right;
				}
				if (boundwe.Bottom > MinHeight) {
					MinHeight = boundwe.Bottom + PipelineNode.NodeMargin.Bottom;
				}
				Point offset = new Point (Math.Max(0, -boundwe.Left), Math.Max(0, -boundwe.Top));
				if (offset != Point.Zero) {
					TranslateAllNodesBy (offset);
					QueueDraw ();
					return;
				}
			}

			if (mouseAction.HasFlag (MouseAction.MoveNode)) {
				if (lastSelectedNode.Draw (ctx)) {
					QueueDraw (lastSelectedNode.bound);
				}
			}
				
			// draw alle edges
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					mNode.DrawEdges (ctx);
				}
			}
		
			// draw all markers
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					mNode.Draw (ctx);
				}
			}

			// update things
			if(mouseAction.HasFlag(MouseAction.MoveNode)) {

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

			if(mouseAction.HasFlag(MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) {
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
				string algoType = e.Data.GetValue (TransferDataType.Text).ToString();

				PipelineNode node = new PipelineNode(algoType, new Rectangle(e.Position, PipelineNode.NodeSize));
				SetNodePosition(node);
				nodes.Add(node);

				this.QueueDraw();

			} catch (Exception exception) {
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
					MarkerNode mNode = node.GetMarkerNodeAt (e.Position);
					if (mNode != null) {
						connectNodesStartMarker = mNode;
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
					Tuple<MarkerNode, MarkerEdge> edge = GetEdgeAt (e.Position);
					if (edge != null) { // clicked on edge
						if (edge.Item2.r >= 0.5) {
							connectNodesStartMarker = edge.Item1;
						} else {
							connectNodesStartMarker = (MarkerNode) edge.Item2.to;
						}
						edge.Item2.Active = false;
						lastSelectedEdge = edge;
						mouseAction |= MouseAction.MoveEdge;
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
			MarkerNode mNode = GetInOutMarkerAt (e.Position, PipelineNode.NodeInOutSpace);

			switch (e.Button) {
			case PointerButton.Left:
				// Move node
				if (mouseAction.HasFlag (MouseAction.MoveNode)) {
					SetNodePosition (lastSelectedNode);
					mouseAction ^= MouseAction.MoveNode;
					EmitDataChanged ();
				}

				// Move edge
				if (mouseAction.HasFlag (MouseAction.MoveEdge)) {
					if (mNode != null) {
						if (lastSelectedEdge.Item2.r < 0.5) {
							if (mNode.Match (lastSelectedEdge.Item2.to as MarkerNode)) {
								lastSelectedEdge.Item1.RemoveEdge(lastSelectedEdge.Item2);
								mNode.AddEdgeTo (lastSelectedEdge.Item2.to);
							}
						} else {
							if (mNode.Match (lastSelectedEdge.Item1)) {
								lastSelectedEdge.Item2.to = mNode;
							}
						}
						EmitDataChanged ();
					}

					QueueDraw ();
					lastSelectedEdge.Item2.Active = true;
					lastSelectedEdge = null;
					mouseAction ^= MouseAction.MoveEdge;
				}

				// Add edge
				if (mouseAction.HasFlag (MouseAction.AddEdge)) {
					if (mNode != null) {
						if (mNode.Match (connectNodesStartMarker)) {
							if (mNode.IsInput) {
								connectNodesStartMarker.AddEdgeTo (mNode);
							} else {
								mNode.AddEdgeTo (connectNodesStartMarker);
							}

							lastSelectedEdge = null;
						}

						EmitDataChanged ();
					} 

					QueueDraw ();
					mouseAction ^= MouseAction.AddEdge;
				}

				break;
			case PointerButton.Middle:
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover ();
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
			if (mouseAction.HasFlag (MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) {
				MarkerNode mNode = GetInOutMarkerAt (e.Position, PipelineNode.NodeInOutSpace);
				if (mNode != null) {
					connectNodesEnd = mNode.Bounds.Center;
				} else {
					connectNodesEnd = e.Position;
				}

				QueueDraw ();
			}

			if (!mouseAction.HasFlag(MouseAction.MoveNode)) {
				MarkerNode mNode = GetInOutMarkerAt (e.Position);
				if (mNode != null) {
					TooltipText = mNode.compatible.ToString() + "\n" + mNode.compatible.Type.ToString();
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
				Tuple<MarkerNode, MarkerEdge> edge = GetEdgeAt (mousePosition);
				if (edge != null) {
					edge.Item1.RemoveEdge(edge.Item2);
					QueueDraw ();
					EmitDataChanged ();
				} else {
					PipelineNode node = GetNodeAt (mousePosition, true);
					if (node != null) {
						RemoveNode (node);
						QueueDraw ();
						EmitDataChanged ();
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
			SetNodePosition (nodeToMove);
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
		private void SetNodePosition(PipelineNode nodeToMove, int iteration = 20) {
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

					SetNodePosition(nodeToMove, iteration - 1);
					QueueDraw ();
				}
			}
		}

		/// <summary>
		/// Gets the edge at position.
		/// </summary>
		/// <returns>The <see cref="System.Tuple`2[[baimp.MarkerNode],[baimp.PipelineEdge]]"/>.</returns>
		/// <param name="position">Position.</param>
		private Tuple<MarkerNode, MarkerEdge> GetEdgeAt(Point position)
		{
			double epsilon = 4.0;

			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					foreach (Edge e in mNode.Edges) {
						MarkerEdge edge = (MarkerEdge)e;
						Point from = mNode.Bounds.Center;
						Point to = edge.to.Bounds.Center;

						double segmentLengthSqr = (to.X - from.X) * (to.X - from.X) + (to.Y - from.Y) * (to.Y - from.Y);
						double r = ((position.X - from.X) * (to.X - from.X) + (position.Y - from.Y) * (to.Y - from.Y)) / segmentLengthSqr;
						if (r < 0 || r > 1) {
							continue;
						}
						double sl = ((from.Y - position.Y) * (to.X - from.X) - (from.X - position.X) * (to.Y - from.Y)) / System.Math.Sqrt(segmentLengthSqr);
						if (-epsilon <= sl && sl <= epsilon) {
							edge.r = r;
							return new Tuple<MarkerNode, MarkerEdge>(mNode, edge);
						}
					}
				}
			}

			return null;
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
		private MarkerNode GetInOutMarkerAt (Point position, Size? inflate = null)
		{
			foreach(PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (mNode.Bounds.Inflate(inflate ?? Size.Zero).Contains (position)) {
						return mNode;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Removes a node.
		/// </summary>
		/// <param name="node">Node to remove.</param>
		private void RemoveNode(PipelineNode node) {
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					for (int i = 0; i < mNode.Edges.Count; i++) {
						mNode.RemoveEdge (mNode.Edges [i]);
						//if (mNode.Edges [i].from == mNode) {
						//	mNode.Edges[i].Remove ();
						//}
					}
				}
			}
			nodes.Remove (node);
		}
			
		#endregion

		#region Properties

		public List<PipelineNode> Nodes {
			get {
				return nodes;
			}
			set {
				nodes = value;
				QueueDraw ();
			}
		}

		#endregion

		#region custom events

		EventHandler<SaveStateEventArgs> dataChanged;

		/// <summary>
		/// Occurs when data changed
		/// </summary>
		public event EventHandler<SaveStateEventArgs> DataChanged {
			add {
				dataChanged += value;
			}
			remove {
				dataChanged -= value;
			}
		}

		#endregion

		#region helper

		/// <summary>
		/// Translates all nodes by given offset.
		/// </summary>
		/// <param name="offset">Offset.</param>
		private void TranslateAllNodesBy(Point offset)
		{
			foreach (PipelineNode node in nodes) {
				node.bound = node.bound.Offset (offset);
			}
		}

		private void EmitDataChanged()
		{
			if (dataChanged != null) {
				dataChanged (this, new SaveStateEventArgs (false));
			}
		}

		#endregion

	}
}

