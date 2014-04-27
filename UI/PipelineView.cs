//
//  PipelineView.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
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
		AddEdgeNew = 8,
		MoveEdge = 16
	}

	public class PipelineView : Canvas
	{
		static int globalId = 0;
		ScrollView scrollview;
		List<PipelineNode> nodes;
		Point nodeToMoveOffset = Point.Zero;
		MarkerNode connectNodesStartMarker;
		Point connectNodesEnd;
		Point mousePosition = Point.Zero;
		PipelineNode lastSelectedNode;
		PipelineNode currentHoveredNode;
		Tuple<MarkerNode, MarkerEdge> lastSelectedEdge;
		MouseAction mouseAction = MouseAction.None;
		MouseMover mouseMover;
		CancellationTokenSource cancelRequest;
		CursorType oldCursor;
		/// <summary>
		/// True if redraw is already queued.
		/// </summary>
		bool redrawQueued;
		/// <summary>
		/// /Window to show results in.*/
		/// </summary>
		readonly Window popupWindow = new Window();
		/// <summary>
		/// Initial scroll position.
		/// Set when this pipeline was loaded from file.
		/// </summary>
		Point initialScrollPosition = Point.Zero;
		/// <summary>
		/// Current scale (zoom) factor.
		/// </summary>
		double scaleFactor = 1.0;

		#region initialize

		public PipelineView()
		{
			PipelineName = "Untitled " + globalId;
			globalId++;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.PipelineView"/> class.
		/// </summary>
		/// <param name="scrollview">Parent scrollview</param>
		/// <param name="loadedNodes">Add already loaded nodes to this new instance</param>
		/// <param name="scrollX">Initial scrollbar position horizontal</param>
		/// <param name="scrollY">Initial scrollbar position vertical</param>
		public void Initialize(ScrollView scrollview, 
		                       List<PipelineNode> loadedNodes = null, double scrollX = 0.0, double scrollY = 0.0)
		{
			this.scrollview = scrollview;
			scrollview.HorizontalScrollControl.Value = scrollX;
			scrollview.VerticalScrollControl.Value = scrollY;

			scrollview.VerticalScrollControl.ValueChanged += QueueRedraw;
			scrollview.HorizontalScrollControl.ValueChanged += QueueRedraw;

			// Workaround!
			// scrollview gets set to zero from xwt multiple times on widget loading 
			scrollview.HorizontalScrollControl.ValueChanged += delegate {
				if (scrollview.HorizontalScrollControl.Value > 1.0 &&
				    scrollview.HorizontalScrollControl.Value != scrollX) {
					initialScrollPosition = Point.Zero;
				}
			};

			initialScrollPosition = new Point(scrollX, scrollY);

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

			ShowMiniMap = true;
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

			// apply scale factor
			dirtyRect.X /= scaleFactor;
			dirtyRect.Y /= scaleFactor;
			dirtyRect.Width /= scaleFactor;
			dirtyRect.Height /= scaleFactor;

			// actual drawing
			bool redraw = Draw(ctx, dirtyRect, scaleFactor);

			// draw minimap
			if (ShowMiniMap && MinWidth > 0 && MinHeight > 0) {
				Point size = new Point(180.0, 180.0 * (MinHeight / MinWidth));
				using (ImageBuilder ib = new ImageBuilder(size.X, size.Y)) {
					double minimapScale = Math.Min(size.X / MinWidth, size.Y / MinHeight);
					Point minimapPosition = 
						new Point(
							scrollview.HorizontalScrollControl.Value + scrollview.HorizontalScrollControl.PageSize - size.X - 16, 
							scrollview.VerticalScrollControl.Value + 16);

					ctx.RoundRectangle(minimapPosition, size.X, size.Y, 6);
					ctx.SetColor(Colors.LightGray.WithAlpha(0.4));
					ctx.Fill();
				
					Draw(ib.Context, new Rectangle(0, 0, MinWidth, MinHeight), minimapScale);
					ctx.DrawImage(ib.ToVectorImage(), minimapPosition);
					//ctx.Fill();
				}
			}

			// set canvas min size
			foreach (PipelineNode node in nodes) {
				Rectangle boundwe = node.BoundWithExtras;
				if (boundwe.Right * scaleFactor > MinWidth) {
					MinWidth = boundwe.Right * scaleFactor + PipelineNode.NodeMargin.Right;
				}
				if (boundwe.Bottom * scaleFactor > MinHeight) {
					MinHeight = boundwe.Bottom * scaleFactor + PipelineNode.NodeMargin.Bottom;
				}
				Point offset = new Point(Math.Max(0, -boundwe.Left), Math.Max(0, -boundwe.Top));
				if (offset != Point.Zero) {
					TranslateAllNodesBy(offset);
					redraw = true;
					QueueDraw();
				}
			}

			// update things
			if (mouseAction.HasFlag(MouseAction.MoveNode)) {
				// move scrollbar
				Rectangle boundwe = lastSelectedNode.BoundWithExtras;
				boundwe.X *= scaleFactor;
				boundwe.Y *= scaleFactor;
				boundwe.Height *= scaleFactor;
				boundwe.Width *= scaleFactor;

				double viewportRight = scrollview.HorizontalScrollControl.Value + scrollview.Size.Width;
				double offsetH = (nodeToMoveOffset.X + boundwe.Width) * 0.5 / scaleFactor;
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

			if ((mouseAction.HasFlag(MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) &&
			    !mouseAction.HasFlag(MouseAction.AddEdgeNew)) {

				ctx.MoveTo(
					connectNodesStartMarker.IsInput ? 
						connectNodesStartMarker.Bounds.Left : 
						connectNodesStartMarker.Bounds.Right, 
					connectNodesStartMarker.Bounds.Center.Y
				);
				ctx.LineTo(connectNodesEnd);
				ctx.Stroke();
			}

			// set redraw finish?
			redrawQueued = redraw;

			// initial scroll position
			if (!initialScrollPosition.IsEmpty && !redraw &&
			    (scrollview.HorizontalScrollControl.Value < 1.0 || scrollview.VerticalScrollControl.Value < 1.0)) {
				scrollview.HorizontalScrollControl.Value = 
					Math.Min(
					initialScrollPosition.X,
					scrollview.HorizontalScrollControl.UpperValue - scrollview.HorizontalScrollControl.PageSize
				);
				scrollview.VerticalScrollControl.Value = 
					Math.Min(
					initialScrollPosition.Y,
					scrollview.VerticalScrollControl.UpperValue - scrollview.VerticalScrollControl.PageSize
				);
			}
		}

		bool Draw(Context ctx, Rectangle dirtyRect, double scale)
		{
			bool redraw = false;
			ctx.Save();
			ctx.Scale(scale, scale);

			// draw all edges
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (!mNode.IsInput) {
						mNode.DrawEdges(ctx);
					}
				}
			}

			// draw all nodes
			foreach (PipelineNode node in nodes) {
				if (!mouseAction.HasFlag(MouseAction.MoveNode) || node != lastSelectedNode) {
					if (node.bound.IntersectsWith(dirtyRect) || !initialScrollPosition.IsEmpty) {
						if (node.Draw(ctx)) {
							redraw = true;
							QueueDraw(node.bound);
						}
					}
				}
			}

			// draw all markers
			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					mNode.Draw(ctx);
				}
			}

			// draw selected node last
			if (mouseAction.HasFlag(MouseAction.MoveNode)) {
				if (lastSelectedNode.Draw(ctx)) {
					redraw = true;
					QueueDraw(lastSelectedNode.bound);
				}
				foreach (MarkerNode mNode in lastSelectedNode.mNodes) {
					mNode.Draw(ctx);
				}
			}

			ctx.Restore();
			return redraw;
		}

		void QueueRedraw(object sender = null, EventArgs e = null)
		{
			if (!redrawQueued) {
				redrawQueued = true;

				Application.Invoke(delegate {
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
			Task executionTask = Task.Factory.StartNew(() => {
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
				Application.Invoke(() => project.NotifyPipelineStop(this));
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
		void OpenOptionWindow(PipelineNode pNode)
		{
			Dialog d = new Dialog();
			d.Title = String.Format("Option for \"{0}\"", pNode.algorithm);
			Table table = new Table();
			VBox contentBox = new VBox();

			int i = 0;
			Widget[] entries = new Widget[pNode.algorithm.Options.Count];
			foreach (BaseOption option in pNode.algorithm.Options) {
				table.Add(new Label(option.Name), 0, i);

				Widget entry;
				if (option is OptionBool) {
					CheckBox checkbox = new CheckBox();
					checkbox.State = (bool) option.Value ? CheckBoxState.On : CheckBoxState.Off;
					entry = checkbox;
				} else {
					TextEntry entryText = new TextEntry();
					entryText.Text = option.Value.ToString();
					entry = entryText;
				}
				entries[i] = entry;
				table.Add(entry, 1, i);
				i++;
			}

			TextEntry commentEntry = new TextEntry();
			commentEntry.PlaceholderText = "Comments...";
			commentEntry.MultiLine = true;
			commentEntry.Text = pNode.userComment;

			contentBox.PackStart(table);
			contentBox.PackEnd(commentEntry);
			d.Content = contentBox;

			d.Buttons.Add(new DialogButton(Command.Cancel));
			d.Buttons.Add(new DialogButton(Command.Apply));

			var r = d.Run(this.ParentWindow);

			if (r.Id == Command.Apply.Id) {
				i = 0;
				foreach (BaseOption option in pNode.algorithm.Options) {
					try {
						if (option is OptionBool) {
							option.Value = ((CheckBox) entries[i]).State == CheckBoxState.On;
						} else {
							option.Value = 
								Convert.ChangeType(((TextEntry) entries[i]).Text, option.Value.GetType());
						}
					} catch (FormatException e) {
						// TODO show error
						Console.WriteLine(e);
					}
					i++;
				}
				pNode.userComment = commentEntry.Text;
			}

			d.Dispose();
		}

		/// <summary>
		/// Shows popover window with results.
		/// </summary>
		/// <param name="mNode">M node.</param>
		void ShowResultPopover(MarkerNode mNode)
		{
			if (mNode.IsInput) {
				return;
			}

			PipelineNode pNode = mNode.parent;
			if (pNode.results == null || pNode.results.Count == 0) {
				return;
			}

			if (pNode.results[0].Item1.Length - 1 < mNode.Position) {
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

		/// <summary>
		/// Scale view.
		/// </summary>
		/// <param name="d">D.</param>
		public void Scale(double d)
		{
			scaleFactor *= d;
			if (scaleFactor > 1.5) {
				scaleFactor = 1.5;
			}
			if (scaleFactor < 0.3) {
				scaleFactor = 0.3;
			}

			QueueRedraw();
		}

		#endregion

		#region drag and drop

		protected override void OnDragOver(DragOverEventArgs args)
		{
			args.AllowedAction = DragDropAction.Link;
		}

		protected override void OnDragDrop(DragEventArgs args)
		{
			Point position = args.Position;
			position.X /= scaleFactor;
			position.Y /= scaleFactor;

			args.Success = true;
			try {
				string algoType = args.Data.GetValue(TransferDataType.Text).ToString();

				PipelineNode node = 
					new PipelineNode(this, algoType, new Rectangle(position, PipelineNode.AbsMinNodeSize));
				node.QueueRedraw += QueueRedraw;

				SetNodePosition(node);
				nodes.Add(node);

				EmitDataChanged();
				this.QueueDraw();

			} catch (Exception e) {
				Console.WriteLine(e.StackTrace);
				Console.WriteLine(e.Message);
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
			Point position = args.Position;
			position.X /= scaleFactor;
			position.Y /= scaleFactor;

			popupWindow.Hide();
			initialScrollPosition = Point.Zero;

			PipelineNode node = GetNodeAt(position, true);

			if (node != null) {
				ButtonEventArgs nodeArgs = new ButtonEventArgs();
				nodeArgs.X = position.X - node.bound.Location.X;
				nodeArgs.Y = position.Y - node.bound.Location.Y;
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
					} else {
						
						MarkerNode mNode = node.GetMarkerNodeAt(position);
						if (mNode != null && !mNode.compatible.IsFinal()) {
							connectNodesStartMarker = mNode;
							mouseAction |= MouseAction.AddEdge | MouseAction.AddEdgeNew;
						} else {
							if (node.bound.Contains(position)) {
								nodeToMoveOffset = new Point(
									node.bound.Location.X - position.X,
									node.bound.Location.Y - position.Y
								);
								lastSelectedNode = node;
								mouseAction |= MouseAction.MoveNode;
							} 
						}
					}
				} else {
					Tuple<MarkerNode, MarkerEdge> edge = GetEdgeAt(position);
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
				lastSelectedEdge = GetEdgeAt(position);
				if (lastSelectedEdge != null) {
					contextMenuEdge.Popup();
				} else {
					PipelineNode nodeRight = GetNodeAt(position, true);
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
			Point position = args.Position;
			position.X /= scaleFactor;
			position.Y /= scaleFactor;

			MarkerNode mNode = GetInOutMarkerAt(position, PipelineNode.NodeInOutSpace);
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
			Point position = args.Position;
			position.X /= scaleFactor;
			position.Y /= scaleFactor;

			mousePosition = position;

			if (mouseAction.HasFlag(MouseAction.MoveNode)) {
				if (lastSelectedNode != null) {
					lastSelectedNode.bound.Location = position.Offset(nodeToMoveOffset);
					lastSelectedNode.OnMove(null);
					QueueDraw();
				}
			}
			if (mouseAction.HasFlag(MouseAction.AddEdge) || mouseAction.HasFlag(MouseAction.MoveEdge)) {
				mouseAction &= ~MouseAction.AddEdgeNew;
				MarkerNode mNode = GetInOutMarkerAt(position, PipelineNode.NodeInOutSpace);
				if (mNode != null && mNode.Match(connectNodesStartMarker)) {
					connectNodesEnd = new Point(mNode.IsInput ? mNode.Bounds.Left : mNode.Bounds.Right, mNode.Bounds.Center.Y);
				} else {
					connectNodesEnd = position;
				}

				QueueDraw();
			}

			if (!mouseAction.HasFlag(MouseAction.MoveNode)) {
				MarkerNode mNode = GetInOutMarkerAt(position);
				if (mNode != null) {
					TooltipText = mNode.compatible + "\n" + mNode.compatible.Type;
				} else {
					TooltipText = string.Empty;
				}
			}

			PipelineNode node = GetNodeAt(position, true);
			if (node != null) {
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

		protected override void OnMouseScrolled(MouseScrolledEventArgs args)
		{
			//base.OnMouseScrolled(args);
			initialScrollPosition = Point.Zero;

			if (args.Direction == ScrollDirection.Down) {
				Scale(0.9);
			} else {
				Scale(1.1);
			}
			args.Handled = true;
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
		Tuple<MarkerNode, MarkerEdge> GetEdgeAt(Point position)
		{
			const double epsilon = 4.0;

			foreach (PipelineNode pNode in nodes) {
				foreach (MarkerNode mNode in pNode.mNodes) {
					if (!mNode.IsInput) {
						foreach (Edge e in mNode.Edges) {
							MarkerEdge edge = (MarkerEdge) e;
							using (ImageBuilder ib = new ImageBuilder(Bounds.Width, Bounds.Height)) {
								ib.Context.SetLineWidth(12);
								edge.ComputeStroke(ib.Context, mNode);
								if (ib.Context.IsPointInStroke(position)) {
									double fromDist = 
										Math.Pow(mNode.Bounds.Center.X - position.X, 2) +
										Math.Pow(mNode.Bounds.Center.Y - position.Y, 2);
									double toDist = 
										Math.Pow(edge.to.Bounds.Center.X - position.X, 2) +
										Math.Pow(edge.to.Bounds.Center.Y - position.Y, 2);
									if (fromDist < toDist) {
										edge.r = 0;
									} else {
										edge.r = 1;
									}
									return new Tuple<MarkerNode, MarkerEdge>(mNode, edge);
								}
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
				if (withExtras) {
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

		public bool ShowMiniMap {
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
		void TranslateAllNodesBy(Point offset)
		{
			foreach (PipelineNode node in nodes) {
				node.bound = node.bound.Offset(offset);
				node.OnMove(null);
			}
		}

		void EmitDataChanged()
		{
			if (dataChanged != null) {
				dataChanged(this, new SaveStateEventArgs(false));
			}
		}

		#endregion
	}
}

