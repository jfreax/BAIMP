using System;
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace baimp
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
		public Queue<IType> inputData = new Queue<IType>();

		[XmlIgnore]
		public MarkerNodeView view;

		public MarkerNode()
		{
			view = new MarkerNodeView(this);
		}

		public MarkerNode(PipelineNode parent, Compatible compatible, int positionNo, bool isInput) : this()
		{
			this.parent = parent;
			this.compatible = compatible;
			this.IsInput = isInput;

			this.positionNo = positionNo;

			Height = 10;
			Console.WriteLine("Add at: " + Bounds + " und right: " + parent.view.Bounds.Width);
			parent.view.AddChild(view, Bounds);
		}

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

		#region properties

		public override Rectangle Bounds {
			get {
				return new Rectangle(
					new Point(
						IsInput ? 0 : parent.view.BoundPosition.Width -NodeInOutMarkerSize,
						parent.view.ContentOffset.Y + (positionNo + 1) * NodeInOutSpace + positionNo * Height
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

