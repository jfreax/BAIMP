using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Baimp
{
	public class MarkerNode : Node
	{
		static public int NodeInOutMarkerSize = 10;
		static public int NodeInOutSpace = 18;
		[XmlIgnore]
		public Compatible compatible;
		[XmlIgnore]
		public PipelineNode parent;
		private int positionNo;

		[XmlIgnore]
		private Queue<Result> inputData = new Queue<Result>();
		private List<Result> resultHistory = new List<Result>();

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
				foreach (Result res in resultHistory) {
					if (res.IsUsed(parent)) {
						inputBufferSize++;
					}
				}
				if (inputBufferSize > 0) {
					TextLayout text = new TextLayout();
					text.Text = inputBufferSize.ToString();
					double textWidth = text.GetSize().Width;
					double textHeight = text.GetSize().Height;
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
					ctx.DrawTextLayout(text, inputbufferSizeLocation);
				}
			}
		}

		/// <summary>
		/// Draws the edges.
		/// </summary>
		/// <param name="ctx">Context.</param>
		public void DrawEdges(Context ctx)
		{
			foreach (MarkerEdge edge in edges) {
				edge.Draw(ctx, this);
			}
		}

		#endregion

		/// <summary>
		/// Tests if another node is compatible with this one.
		/// Compatible == there can be a edge between this nodes.
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="another">The other compatible instance.</param>
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
			} else {
				return compatible.Match(this, otherNode);
			}
		}

		/// <summary>
		/// Enqueues the a result object to the input buffer.
		/// </summary>
		/// <param name="result">Result.</param>
		public void EnqueueInput(Result result) 
		{
			inputData.Enqueue(result);
			resultHistory.Add(result);

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
			return inputData.Dequeue();
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
			inputData.Clear();
			resultHistory.Clear();
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

