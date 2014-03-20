using System;
using Xwt;

namespace baimp
{
	public class InOutMarker
	{
		private int nodeID;

		private Node parentNode;
		public string type;
		public bool isInput;

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView+InOutMarker"/> class.
		/// </summary>
		/// <param name="parentNode">Parent node.</param>
		/// <param name="nodeID">Node ID.</param>
		/// <param name="type">Type.</param>
		/// <param name="isInput">If set to <c>true</c>, then its an input; otherwise output.</param>
		public InOutMarker(Node parentNode, int nodeID, string type, bool isInput)
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
		static public InOutMarker GetInOutMarkerAt (Node node, Point position, Size? inflate = null)
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
		static public Rectangle GetBoundForInOutMarkerOf(Node node, int number, bool isInput )
		{
			return new Rectangle (
				new Point (
					isInput ? node.bound.Left - (Node.nodeInOutMarkerSize.Width - 2) : node.bound.Right - 2, 
					node.bound.Top + (number * Node.nodeInOutSpace) + ((number + 1) * Node.nodeInOutMarkerSize.Height)
				), Node.nodeInOutMarkerSize
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
}

