using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Baimp
{
	public class MarkerNode : Node
	{
		static readonly public int NodeInOutMarkerSize = 10;
		static readonly public int NodeInOutSpace = 18;

		[XmlIgnore]
		public Compatible compatible;

		[XmlIgnore]
		public PipelineNode parent;

		int positionNo;
		ConcurrentQueue<Result> inputData = new ConcurrentQueue<Result>();
		List<Result> inputHistory = new List<Result>();
		object inputHistoryLock = new object();

		/// <summary>Temp variable to store rendering text</summary>
		TextLayout textLayout = new TextLayout();

		public MarkerNode()
		{
		}

		public MarkerNode(PipelineNode parent, Compatible compatible, int positionNo, bool isInput)
		{
			this.parent = parent;
			this.compatible = compatible;
			this.IsInput = isInput;

			this.positionNo = positionNo;
		}

		#region drawing

		/// <summary>
		/// Draw the marker.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public override void Draw(Context ctx)
		{
			ctx.SetColor(PipelineNode.NodeColorBorder);

			Rectangle bndTmp = Bounds;
			ctx.SetLineWidth(1);
			ctx.MoveTo(bndTmp.Left, bndTmp.Center.Y);
			ctx.LineTo(bndTmp.Right, bndTmp.Center.Y);
			ctx.Stroke();

			if (IsInput) {
				int inputBufferSize = inputData.Count;

				List<Result> ihCopy;
				lock (inputHistoryLock) {
					ihCopy = new List<Result>(inputHistory);
				}
				foreach (Result input in ihCopy) {
					if (input.IsUsed(parent)) {
						inputBufferSize++;
					} else {
						lock (inputHistoryLock) {
							inputHistory.Remove(input);
						}
					}
				}
				if (inputBufferSize > 0) {
					textLayout.Text = inputBufferSize.ToString();
					double textWidth = textLayout.GetSize().Width;
					double textHeight = textLayout.GetSize().Height;
					Point inputbufferSizeLocation = 
						Bounds.Location.Offset(-(textWidth / 2), -(textHeight));

					ctx.Arc(
						inputbufferSizeLocation.X + textWidth / 2,
						inputbufferSizeLocation.Y + textHeight / 2,
						Math.Max(textHeight, textWidth) / 2 + 1,
						0, 360
					);
					ctx.Fill();

					ctx.SetColor(PipelineNode.NodeColor);
					ctx.DrawTextLayout(textLayout, inputbufferSizeLocation);
				}
			} else {
				if (parent.algorithm.Output[positionNo].IsEnd()) {
					ctx.MoveTo(bndTmp.Right, bndTmp.Top);
					ctx.LineTo(bndTmp.Right, bndTmp.Bottom);
					ctx.Stroke();

					// draw output size on end nodes (= number of features
					if (parent.results != null &&
						parent.results.Count != 0 &&
						parent.results[0].Item1.Length-1 >= Position) {

						textLayout.Text = parent.results.Count.ToString();
						double textWidth = textLayout.GetSize().Width;
						double textHeight = textLayout.GetSize().Height;
						Point outputbufferSizeLocation = 
							Bounds.Location.Offset(Bounds.Width*1.8, 0);

						ctx.Arc(
							outputbufferSizeLocation.X + textWidth / 2,
							outputbufferSizeLocation.Y + textHeight / 2,
							Math.Max(textHeight, textWidth) / 2 + 1,
							0, 360
						);
						ctx.Fill();

						ctx.SetColor(PipelineNode.NodeColor);
						ctx.DrawTextLayout(textLayout, outputbufferSizeLocation);
					}
				}
			}
		}

		/// <summary>
		/// Draws the edges.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public void DrawEdges(Context ctx)
		{
			foreach (Edge edge in edges) {
				MarkerEdge mEdge = edge as MarkerEdge;
				if (mEdge != null) {
					mEdge.Draw(ctx, this);
				}
			}
		}

		#endregion

		/// <summary>
		/// Tests if another node is compatible with this one.
		/// Compatible == there can be a edge between this nodes.
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="otherNode">The other compatible instance.</param>
		public bool Match(MarkerNode otherNode)
		{
			if (this == otherNode)
				return false;

			if (parent == otherNode.parent)
				return false;

			if (IsInput == otherNode.IsInput)
				return false;

			foreach (Edge edge in edges) {
				if (edge.to.ID == otherNode.ID) {
					return false;
				}
			}

			if (IsInput) {
				return otherNode.compatible.Match(otherNode, this);
			}

			return compatible.Match(this, otherNode);
		}

		/// <summary>
		/// Enqueues the a result object to the input buffer.
		/// </summary>
		/// <param name="result">Result.</param>
		public void EnqueueInput(Result result)
		{
			inputData.Enqueue(result);

			lock (inputHistoryLock) {
				inputHistory.Add(result);
			}

			if (parent.queueRedraw != null) {
				parent.queueRedraw(this, null);
			}
		}

		/// <summary>
		/// Dequeues an input result.
		/// </summary>
		/// <returns>Result from queue.</returns>
		public Result DequeueInput()
		{
			Result value;
			inputData.TryDequeue(out value);

			return value;
		}

		/// <summary>
		/// Determines whether the input buffer is empty.
		/// </summary>
		/// <returns><c>true</c> if empty; otherwise, <c>false</c>.</returns>
		public bool IsInputEmpty()
		{
			return inputData.Count == 0;
		}

		/// <summary>
		/// Disposes all input buffer data.
		/// </summary>
		/// <remarks>
		/// Delete input history too!
		/// </remarks>
		public void ClearInput()
		{
			foreach (Result res in inputData) {
				res.Dispose();
			}

			inputData = new ConcurrentQueue<Result>();

			lock (inputHistoryLock) {
				inputHistory.Clear();
			}
		}

		#region properties

		public override Rectangle Bounds {
			get {
				return new Rectangle(
					new Point(
						IsInput ? parent.bound.Left - NodeInOutMarkerSize : parent.bound.Right,
						parent.bound.Y + parent.contentOffset.Y + (positionNo + 1) * NodeInOutSpace + positionNo * Height
					), new Size(NodeInOutMarkerSize, Height)
				);
			}

		}

		[XmlIgnore]
		public double Height {
			get;
			set;
		}

		[XmlAttribute("input")]
		public bool IsInput {
			get;
			set;
		}

		[XmlAttribute("position")]
		public int Position {
			get {
				return positionNo;
			}
			set {
				positionNo = value;

			}
		}

		#endregion
	}
}

