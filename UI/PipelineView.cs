using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace Baimp
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
		static private int globalId = 0;

		private ScrollView scrollview;

		private List<PipelineNode> nodes;
		private Point nodeToMoveOffset = Point.Zero;
		private MarkerNode connectNodesStartMarker;
		private Point connectNodesEnd;
		private Point mousePosition = Point.Zero;
		private PipelineNode lastSelectedNode = null;
		private PipelineNode currentHoveredNode = null;
		private Tuple<MarkerNode, MarkerEdge> lastSelectedEdge = null;
		private MouseAction mouseAction = MouseAction.None;
		private MouseMover mouseMover;

		CancellationTokenSource cancelRequest;

		CursorType oldCursor;

		private bool redrawQueued;

		Window popupWindow = new Window ();


		#region initialize

		public PipelineView() {
			PipelineName = "Untitled " + globalId;
			globalId++;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.PipelineView"/> class.
		/// </summary>
		/// <param name="scrollview">Parent scrollview</param>
		/// <param name="loadedNodes">Add already loaded nodes to this new instance</param>
		public void Initialize(ScrollView scrollview, List<PipelineNode> loadedNodes = null)
		{
			this.scrollview = scrollview;

			this.SetDragDropTarget(TransferDataType.Text);
			this.BackgroundColor = Colors.WhiteSmoke;
			this.CanGetFocus = true;

			if (loadedNodes == null) {
				nodes = new List<PipelineNode>();
			} else {
				Nodes = loadedNodes;
			}

			mouseMover = new MouseMover(scrollview);
			mouseMover.Timer = 20;

			popupWindow.Decorated = false;
			popupWindow.ShowInTaskbar = false;
			popupWindow.Padding = 2;

			InitializeContextMenus();
		}

		private Menu contextMenuEdge;
		private Menu contextMenuNode;
		private MenuItem contextMenuNodeOptions;

		/// <summary>
		/// Initializes all context menus.
		/// </summary>
		private void InitializeContextMenus()
		{
			// edge context menu
			contextMenuEdge = new Menu();
			MenuItem contextMenuEdgeDelete = new MenuItem("Delete edge");
			contextMenuEdgeDelete.Clicked += delegate(object sender, EventArgs e) {
				if (lastSelectedEdge != null) {
					lastSelectedEdge.Item1.RemoveEdge(lastSelectedEdge.Item2);
					QueueDraw();
				}
			};
			contextMenuEdge.Items.Add(contextMenuEdgeDelete);


			// node context menu
			contextMenuNode = new Menu();
			MenuItem contextMenuNodeDelete = new MenuItem("Delete node");
			contextMenuNodeDelete.Clicked += delegate(object sender, EventArgs e) {
				if (lastSelectedNode != null) {
					RemoveNode(lastSelectedNode);
					QueueDraw();
				}
			};
			contextMenuNode.Items.Add(contextMenuNodeDelete);

			contextMenuNodeOptions = new MenuItem("Options");
			contextMenuNodeOptions.Clicked += delegate(object sender, EventArgs e) {
				if (lastSelectedNode != null) {
					OpenOptionWindow(lastSelectedNode);
					mouseAction = MouseAction.None;
				}
			};
			contextMenuNode.Items.Add(contextMenuNodeOptions);
		}

		#endregion

		#region drawing

		/// <summary>
		/// Called when the widget needs to be redrawn
		/// </summary>
		/// <param name='ctx'>
		/// Drawing context
		/// </param>
		/// <param name="dirtyRect"></param>
		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			if (Bounds.IsEmpty)
				return;

			base.OnDraw(ctx, dirtyRect);
				
			// draw all nodes
			foreach (PipelineNode node in nodes) {
				if (!mouseAction.HasFlag(MouseAction.MoveNode) || node != lastSelectedNode) {
					if (node.bound.IntersectsWith(dirtyRect)) {
						if (node.Draw(ctx)) {
							QueueDraw(node.bound);
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
				Point offset = new Point(Math.Max(0, -boundwe.Left), Math.Max(0, -boundwe.Top));
				if (offset != Point.Zero) {
					TranslateAllNodesBy(offset);
					QueueDraw();
				}
			}
				
			// draw alle edges
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (!mNode.IsInput) {
						mNode.DrawEdges(ctx);
					}
				}
			}
				
			// draw all markers
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					mNode.Draw(ctx);
				}
			}

			if (mouseAction.HasFlag(MouseAction.MoveNode)) {
				if (lastSelectedNode.Draw(ctx)) {
					QueueDraw(lastSelectedNode.bound);
				}
			}

			// update things
			if (mouseAction.HasFlag(MouseAction.MoveNode)) {

				// move scrollbar
				Rectangle boundwe = lastSelectedNode.BoundWithExtras;

				double viewportRight = scrollview.HorizontalScrollControl.Value + scrollview.Size.Width;
				double offsetH = (nodeToMoveOffset.X + boundwe.Width) * 0.5;
				if (boundwe.Right - offsetH > viewportRight) {
					scrollview.HorizontalScrollControl.Value += boundwe.Right - offsetH - viewportRight; 
				} else if (boundwe.Left + offsetH < scrollview.HorizontalScrollControl.Value) {
					scrollview.HorizontalScrollControl.Value -= scrollview.HorizontalScrollControl.Value - offsetH - boundwe.Left;
				}

				double viewportBottom = scrollview.VerticalScrollControl.Value + scrollview.Size.Height;
				double offsetV = (nodeToMoveOffset.Y + boundwe.Height) * 0.5;
				if (boundwe.Bottom - offsetV > viewportBottom) {
					scrollview.VerticalScrollControl.Value += boundwe.Bottom - offsetV - viewportBottom;
				} else if (boundwe.Top + offsetV < scrollview.VerticalScrollControl.Value) {
					scrollview.VerticalScrollControl.Value -= scrollview.VerticalScrollControl.Value - offsetV - boundwe.Top;
				}
			}

			if (mouseAction.HasFlag(MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) {
				ctx.MoveTo(connectNodesStartMarker.Bounds.Center);
				ctx.LineTo(connectNodesEnd);
				ctx.Stroke();
			}

			redrawQueued = false;
		}


		void QueueRedraw(object sender, EventArgs e)
		{
			if (!redrawQueued) {
				redrawQueued = true;

				Application.Invoke( delegate {
					QueueDraw();
				});
			}
		}
		#endregion

		#region start/stop

		/// <summary>
		/// Execute the pipeline of the specified project.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <returns>True if started, otherwise false<returns>
		public bool Execute(Project project)
		{
			if (nodes.Count == 0) {
				return false;
			}

			cancelRequest = new CancellationTokenSource();
			CancellationToken token = cancelRequest.Token;

			project.NotifyPipelineStart(this);

			Process process = new Process(project, token);
			Task executionTask = Task.Factory.StartNew( () => {
				foreach (PipelineNode pNode in nodes) {
					if (token.IsCancellationRequested) {
						break;
					}

					pNode.results.Clear();
					pNode.ClearInputQueue();

					if (pNode.IsReady() && pNode.algorithm.Input.Count == 0) {
						Result[] zeroInput = new Result[0];
						process.Start(pNode, zeroInput, int.MaxValue);
					}
				}
			}).ContinueWith(fromTask => {
				Application.Invoke( () => project.NotifyPipelineStop(this) );
			});

			return true;
		}

		public void StopExecution()
		{
			if (cancelRequest != null) {
				cancelRequest.Cancel();
			}
		}

		#endregion

		#region methods

		/// <summary>
		/// Opens the option window.
		/// </summary>
		/// <param name="pNode">Pipeline node for which the option should be shown.</param>
		private void OpenOptionWindow(PipelineNode pNode)
		{
			Dialog d = new Dialog ();
			d.Title = String.Format("Option for \"{0}\"", pNode.algorithm);
			Table table = new Table ();
			VBox contentBox = new VBox();

			int i = 0;
			TextEntry[] entries = new TextEntry[pNode.algorithm.Options.Count];
			foreach (Option option in pNode.algorithm.Options) {
				table.Add (new Label (option.name), 0, i);
				TextEntry entry = new TextEntry();
				entry.Text = option.Value.ToString();
				entries[i] = entry;
				table.Add (entry, 1, i);
				i++;
			}

			TextEntry commentEntry = new TextEntry();
			commentEntry.PlaceholderText = "Comments...";
			commentEntry.MultiLine = true;
			commentEntry.Text = pNode.userComment;

			contentBox.PackStart(table);
			contentBox.PackEnd(commentEntry);
			d.Content = contentBox;

			d.Buttons.Add (new DialogButton (Command.Cancel));
			d.Buttons.Add (new DialogButton (Command.Apply));

			var r = d.Run (this.ParentWindow);

			if (r.Id == Command.Apply.Id) {
				i = 0;
				foreach (Option option in pNode.algorithm.Options) {
					try {
						option.Value = Convert.ChangeType( entries[i].Text, option.Value.GetType()) as IComparable;
					} catch (FormatException e) {
						// TODO show error
						Console.WriteLine(e);
					}
					i++;
				}
				pNode.userComment = commentEntry.Text;
			}

			d.Dispose ();
		}

		/// <summary>
		/// Shows popover window with results.
		/// </summary>
		/// <param name="mNode">M node.</param>
		private void ShowResultPopover(MarkerNode mNode)
		{
			if (mNode.IsInput) {
				return;
			}

			PipelineNode pNode = mNode.parent;
			if (pNode.results == null || pNode.results.Count == 0) {
				return;
			}

			if (pNode.results[0].Item1.Length-1 < mNode.Position) {
				return;
			}
				
			if (popupWindow.Content != null) {
				popupWindow.Content.Dispose();
			}

			List<Tuple<IType[], Result[]>> resultCopy = new List<Tuple<IType[], Result[]>>();
			resultCopy.AddRange(pNode.results);
			ResultPopupView popupView = new ResultPopupView(resultCopy, mNode.Position);

			popupWindow.Content = popupView;
			popupWindow.Size = new Size(1, 1);
			popupWindow.Location = Desktop.MouseLocation.Offset(-10, -10);
			popupWindow.Show();

		}

		#endregion

		#region drag and drop

		protected override void OnDragOver(DragOverEventArgs args)
		{
			args.AllowedAction = DragDropAction.Link;
		}

		protected override void OnDragDrop(DragEventArgs args)
		{
			args.Success = true;
			try {
				string algoType = args.Data.GetValue(TransferDataType.Text).ToString();

				PipelineNode node = new PipelineNode(this, algoType, new Rectangle(args.Position, PipelineNode.NodeSize));
				node.QueueRedraw += QueueRedraw;

				SetNodePosition(node);
				nodes.Add(node);

				EmitDataChanged();
				this.QueueDraw();

			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
				args.Success = false;
			}
		}

		protected override void OnDragLeave(EventArgs args)
		{
		}

		#endregion

		#region events

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			popupWindow.Hide();

			PipelineNode node = GetNodeAt(args.Position, true);

			if (node != null) {
				ButtonEventArgs nodeArgs = new ButtonEventArgs();
				nodeArgs.X = args.X - node.bound.Location.X;
				nodeArgs.Y = args.Y - node.bound.Location.Y;
				nodeArgs.Button = args.Button;
				nodeArgs.MultiplePress = args.MultiplePress;
				if (node.OnButtonPressed(nodeArgs)) {
					return;
				}
			}

			switch (args.Button) {
			case PointerButton.Left:
				if (node != null) { // clicked on node
					if (args.MultiplePress >= 2) {
						OpenOptionWindow(node);
						mouseAction = MouseAction.None;
						args.Handled = true;
						break;
					}
						
					MarkerNode mNode = node.GetMarkerNodeAt(args.Position);
					if (mNode != null) {
						connectNodesStartMarker = mNode;
						mouseAction |= MouseAction.AddEdge;
					} else {
						if (node.bound.Contains(args.Position)) {
							nodeToMoveOffset = new Point(
								node.bound.Location.X - args.Position.X,
								node.bound.Location.Y - args.Position.Y
							);
							lastSelectedNode = node;
							mouseAction |= MouseAction.MoveNode;
						} 
					}
				} else {
					Tuple<MarkerNode, MarkerEdge> edge = GetEdgeAt(args.Position);
					if (edge != null) { // clicked on edge
						if (edge.Item2.r >= 0.5) {
							connectNodesStartMarker = edge.Item1;
						} else {
							connectNodesStartMarker = (MarkerNode) edge.Item2.to;
						}
						edge.Item1.RemoveEdge(edge.Item2);
						lastSelectedEdge = edge;
						mouseAction |= MouseAction.MoveEdge;
						args.Handled = true;
					}
				}

				break;

			case PointerButton.Right:
				lastSelectedEdge = GetEdgeAt(args.Position);
				if (lastSelectedEdge != null) {
					contextMenuEdge.Popup();
				} else {
					PipelineNode nodeRight = GetNodeAt(args.Position, true);
					if (nodeRight != null) {
						lastSelectedNode = nodeRight;
						if (lastSelectedNode.algorithm.options.Count > 0) {
							contextMenuNodeOptions.Show();
						} else {
							contextMenuNodeOptions.Hide();
						}
						contextMenuNode.Popup();
					}
				}
				break;
			case PointerButton.Middle:
				mouseMover.EnableMouseMover(args.Position);

				if (oldCursor != CursorType.Move) {
					oldCursor = this.Cursor;
					this.Cursor = CursorType.Move;
				}

				break;
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs args)
		{
			MarkerNode mNode = GetInOutMarkerAt(args.Position, PipelineNode.NodeInOutSpace);
			bool actionedLeft = false;

			switch (args.Button) {
			case PointerButton.Left:
				// Move node
				if (mouseAction.HasFlag(MouseAction.MoveNode)) {
					SetNodePosition(lastSelectedNode);
					mouseAction ^= MouseAction.MoveNode;
					EmitDataChanged();
					actionedLeft = true;
				}

				// Move edge
				if (mouseAction.HasFlag(MouseAction.MoveEdge)) {
					if (mNode != null) {
						if (lastSelectedEdge.Item2.r < 0.5) {
							if (mNode.Match(lastSelectedEdge.Item2.to as MarkerNode)) {
								mNode.AddEdgeTo(lastSelectedEdge.Item2.to);
							}
						} else {
							if (mNode.Match(lastSelectedEdge.Item1)) {
								lastSelectedEdge.Item2.to = mNode;
								lastSelectedEdge.Item1.AddEdge(lastSelectedEdge.Item2);
							}
						}
						actionedLeft = true;
						EmitDataChanged();
					} else {
						lastSelectedEdge.Item1.AddEdge(lastSelectedEdge.Item2);
					}

					lastSelectedEdge = null;
					mouseAction ^= MouseAction.MoveEdge;
					QueueDraw();
				}

				// Add edge
				if (mouseAction.HasFlag(MouseAction.AddEdge)) {
					if (mNode != null) {
						if (mNode.Match(connectNodesStartMarker)) {
							if (mNode.IsInput) {
								connectNodesStartMarker.AddEdgeTo(mNode);
							} else {
								mNode.AddEdgeTo(connectNodesStartMarker);
							}

							lastSelectedEdge = null;
							actionedLeft = true;
						}
						EmitDataChanged();
					} 

					mouseAction ^= MouseAction.AddEdge;
					QueueDraw();
				}

				// result popover
				if (mNode != null && !actionedLeft) {
					ShowResultPopover(mNode);
					mouseAction = MouseAction.None;
				}

				break;
			case PointerButton.Middle:
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover();
					this.Cursor = oldCursor;
				}
				break;
			}
		}

		protected override void OnMouseMoved(MouseMovedEventArgs args)
		{
			mousePosition = args.Position;

			if (mouseAction.HasFlag(MouseAction.MoveNode)) {
				if (lastSelectedNode != null) {
					lastSelectedNode.bound.Location = args.Position.Offset(nodeToMoveOffset);
					lastSelectedNode.OnMove(null);
					QueueDraw();
				}
			}
			if (mouseAction.HasFlag(MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) {
				MarkerNode mNode = GetInOutMarkerAt(args.Position, PipelineNode.NodeInOutSpace);
				if (mNode != null) {
					connectNodesEnd = mNode.Bounds.Center;
				} else {
					connectNodesEnd = args.Position;
				}

				QueueDraw();
			}

			if (!mouseAction.HasFlag(MouseAction.MoveNode)) {
				MarkerNode mNode = GetInOutMarkerAt(args.Position);
				if (mNode != null) {
					TooltipText = mNode.compatible + "\n" + mNode.compatible.Type;
				} else {
					TooltipText = string.Empty;
				}
			}

			PipelineNode node = GetNodeAt(args.Position, true);
			if ( node != null) {
				if (currentHoveredNode != node) {
					if (currentHoveredNode != null) {
						currentHoveredNode.OnMouseExited();
					}
					currentHoveredNode = node;
					node.OnMouseEntered();
				}
			} else {
				if (currentHoveredNode != null) {
					currentHoveredNode.OnMouseExited();
					currentHoveredNode = null;
				}
			}
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			this.SetFocus();
		}

		protected override void OnMouseExited(EventArgs args)
		{
			if (mouseMover.Enabled) {
				mouseMover.DisableMouseMover();
				this.Cursor = oldCursor;
			}

			if (currentHoveredNode != null) {
				currentHoveredNode.OnMouseExited();
				currentHoveredNode = null;
			}
		}

		protected override void OnKeyPressed(KeyEventArgs args)
		{
			switch (args.Key) {
			case Key.Delete:
				Tuple<MarkerNode, MarkerEdge> edge = GetEdgeAt(mousePosition);
				if (edge != null) {
					edge.Item1.RemoveEdge(edge.Item2);
					QueueDraw();
					EmitDataChanged();
				} else {
					PipelineNode node = GetNodeAt(mousePosition, true);
					if (node != null) {
						RemoveNode(node);
						QueueDraw();
						EmitDataChanged();
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
		private void SetNodeAt(PipelineNode nodeToMove, Point position)
		{
			nodeToMove.bound.Location = position;
			SetNodePosition(nodeToMove);
			QueueDraw();

			nodeToMove.OnMove(null);
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
		private void SetNodePosition(PipelineNode nodeToMove, int iteration = 20)
		{
			if (nodeToMove != null) {
				PipelineNode intersectingNode = GetNodeAt(nodeToMove.BoundWithExtras, nodeToMove);
				if (intersectingNode != null) {
					Rectangle intersect = 
						nodeToMove.BoundWithExtras.Intersect(intersectingNode.BoundWithExtras);

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
					nodeToMove.OnMove(null);

					SetNodePosition(nodeToMove, iteration - 1);
					QueueDraw();
				}
			}
		}

		/// <summary>
		/// Gets the edge at position.
		/// </summary>
		/// <param name="position">Position.</param>
		private Tuple<MarkerNode, MarkerEdge> GetEdgeAt(Point position)
		{
			const double epsilon = 4.0;

			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (!mNode.IsInput) {
						foreach (Edge e in mNode.Edges) {
							MarkerEdge edge = (MarkerEdge) e;
							Point from = mNode.Bounds.Center;
							Point to = edge.to.Bounds.Center;

							double segmentLengthSqr = (to.X - from.X) * (to.X - from.X) + (to.Y - from.Y) * (to.Y - from.Y);
							double r = ((position.X - from.X) * (to.X - from.X) + (position.Y - from.Y) * (to.Y - from.Y)) / segmentLengthSqr;
							if (r < 0 || r > 1) {
								continue;
							}
							double sl = ((from.Y - position.Y) * (to.X - from.X) - (from.X - position.X) * (to.Y - from.Y)) / Math.Sqrt(segmentLengthSqr);
							if (-epsilon <= sl && sl <= epsilon) {
								edge.r = r;
								return new Tuple<MarkerNode, MarkerEdge>(mNode, edge);
							}
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
		private PipelineNode GetNodeAt(Point position, bool withExtras = false)
		{
			foreach (PipelineNode node in nodes) {
				Rectangle bound = withExtras ? node.BoundWithExtras : node.bound;
				if (bound.Contains(position)) {
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
		/// <param name="ignoreNode">Optional: Ignore this node.</param>
		/// <param name="withExtras">Match not only main body of node, but also in/out marker</param>
		private PipelineNode GetNodeAt(Rectangle rectangle, PipelineNode ignoreNode = null, bool withExtras = true)
		{
			foreach (PipelineNode node in nodes) {
				Rectangle nodeBound = node.bound;
				if(withExtras) {
					nodeBound = node.BoundWithExtras;
				}

				if (node != ignoreNode &&
					nodeBound.IntersectsWith(rectangle)) {
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
		private MarkerNode GetInOutMarkerAt(Point position, Size? inflate = null)
		{
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (mNode.Bounds.Inflate(inflate ?? Size.Zero).Contains(position)) {
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
		private void RemoveNode(PipelineNode node)
		{
			foreach (MarkerNode mNode in node.mNodes) {
				List<Edge> toRemove = new List<Edge>();
				for (int i = 0; i < mNode.Edges.Count; i++) {
					toRemove.Add(mNode.Edges[i]);
				}

				foreach (Edge edge in toRemove) {
					mNode.RemoveEdge(edge);
				}

			}
			nodes.Remove(node);
		}

		#endregion

		#region Properties

		public List<PipelineNode> Nodes {
			get {
				return nodes;
			}
			set {
				nodes = value;
                foreach (PipelineNode pNode in nodes) {
                    pNode.Parent = this;
                    pNode.QueueRedraw += QueueRedraw;
                }
				QueueDraw();
			}
		}

		public ScrollView Scrollview {
			get {
				return scrollview;
			}
		}

		public string PipelineName {
			get;
			set;
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
				node.bound = node.bound.Offset(offset);
				node.OnMove(null);
			}
		}

		private void EmitDataChanged()
		{
			if (dataChanged != null) {
				dataChanged(this, new SaveStateEventArgs(false));
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (popupWindow.Content != null) {
				popupWindow.Content.Dispose();
			}
		}
	}
}

