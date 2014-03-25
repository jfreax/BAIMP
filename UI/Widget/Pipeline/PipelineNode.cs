using System;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;

namespace baimp
{
	public class PipelineNode
	{
		static public WidgetSpacing NodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static public Size NodeSize = new Size(200, 40);
		static public Size NodeInOutSpace = new Size(8, 8);
		static public int NodeRadius = 2;
		static public Color NodeColor = Color.FromBytes(252, 252, 252);
		static public Color NodeColorBorder = Color.FromBytes(202, 202, 202);
		static public Color NodeColorShadow = Color.FromBytes(232, 232, 232);
		static public Color NodeColorProgress  = Color.FromBytes(190, 200, 250);

		[XmlIgnore]
		public Point contentOffset = Point.Zero;

		[XmlIgnore]
		public BaseAlgorithm algorithm;

		[XmlIgnore]
		public Rectangle bound = new Rectangle(Point.Zero, NodeSize);

		[XmlIgnore]
		public List<MarkerNode> mNodes;

		[XmlIgnore]
		private int progress = 0;

		#region initialize

		/// <summary>
		/// Empty constructur for xml serialization.
		/// Do not use!
		/// </summary>
		public PipelineNode() {
		}

		public PipelineNode(string algoType, Rectangle bound)
		{
			this.mNodes = new List<MarkerNode>();
			this.AlgorithmType = algoType;
			this.bound = bound;

			int i = 0;
			foreach (Compatible c in algorithm.Input) {
				this.Add(new MarkerNode(this, c, i, true));
				i++;
			}
			i = 0;
			foreach (Compatible c in algorithm.Output) {
				this.Add(new MarkerNode(this, c, i, false));
				i++;
			}
		}

		/// <summary>
		/// Call only, if mNodes are set from outside
		/// </summary>
		public void Initialize()
		{
			foreach (MarkerNode mNode in mNodes) {
				mNode.parent = this;
				if (mNode.IsInput) {
					mNode.compatible = algorithm.Input[mNode.Position];
				} else {
					mNode.compatible = algorithm.Output[mNode.Position];
				}
			}
		}

		#endregion

		#region drawing

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		public bool Draw(Context ctx)
		{
			bool ret = false;

			// draw shadow
			ctx.RoundRectangle(bound.Offset(0, 3), NodeRadius);
			ctx.SetColor(NodeColorShadow);
			ctx.SetLineWidth(2);
			ctx.Fill();

			// border
			ctx.RoundRectangle(bound.Inflate(-1, -1), NodeRadius);
			ctx.SetColor(NodeColorBorder);
			ctx.SetLineWidth(2);
			ctx.StrokePreserve();

			// background
			ctx.SetColor(NodeColor);
			ctx.Fill();

			// progress
			ctx.Save();

			Rectangle clipBound = bound.Inflate(-1, -1);
			clipBound.Width *= progress / 100.0;
			clipBound.Bottom = clipBound.Top + contentOffset.Y;
			ctx.Rectangle(clipBound);
			ctx.Clip();
			ctx.RoundRectangle(bound.Inflate(-1, -1), NodeRadius);
			ctx.SetColor(NodeColorProgress);
			ctx.Fill();

			ctx.Restore();

			// draw headline
			TextLayout text = new TextLayout();
			Point textOffset = new Point(0, 4);

			text.Text = algorithm.ToString();
			if (text.GetSize().Width < NodeSize.Width) {
				textOffset.X = (NodeSize.Width - text.GetSize().Width) * 0.5;
			} else {
				text.Width = NodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			Point textPosition = bound.Location.Offset(textOffset);

			ctx.SetColor(Colors.Black);
			ctx.DrawTextLayout(text, textPosition);

			// stroke under headline
			contentOffset.X = 6;
			contentOffset.Y = textOffset.Y + text.GetSize().Height + 4;

			ctx.SetColor(NodeColorBorder);
			ctx.MoveTo(bound.Location.Offset(contentOffset));
			ctx.LineTo(bound.Right - 6, contentOffset.Y + bound.Location.Y);
			ctx.SetLineWidth(1.0);
			ctx.Stroke();

			// in-/output text
			ctx.SetColor(Colors.Black);
			foreach (MarkerNode mNode in mNodes) {
				text.Text = mNode.compatible.ToString();
				mNode.Height = text.GetSize().Height;
				Point pos = mNode.Bounds.Location;
				if (mNode.IsInput) {
					pos.X = mNode.Bounds.Right + contentOffset.X;
				} else {
					pos.X = bound.Right - contentOffset.X - text.GetSize().Width;
				}
				ctx.DrawTextLayout(text, pos);
				ctx.Stroke();

				// resize widget if necessary
				if (pos.Y + mNode.Height + NodeInOutSpace.Height > bound.Bottom) {
					bound.Bottom = pos.Y + mNode.Height + NodeInOutSpace.Height;
					ret = true;
				}
			}

			return ret;
		}

		#endregion

		/// <summary>
		/// Determines whether the algorithm of this node can be executed
		/// </summary>
		/// <returns><c>true</c> if ready; otherwise, <c>false</c>.</returns>
		public bool IsReady()
		{
			foreach (MarkerNode mNode in MNodes) {
				if (mNode.IsInput && mNode.inputData.Count == 0) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Dequeues a set of input parameters and return them.
		/// </summary>
		/// <returns>The input.</returns>
		public IType[] DequeueInput()
		{
			IType[] input = new IType[algorithm.Input.Count];
			for (int i = 0; i < algorithm.Input.Count; i++) {
				input[i] = mNodes[i].inputData.Dequeue();
			}

			return input;
		}

		/// <summary>
		/// Gets the marker at position if there is one.
		/// </summary>
		/// <returns>The <see cref="baimp.MarkerNode"/>.</returns>
		/// <param name="position">Position.</param>
		public MarkerNode GetMarkerNodeAt(Point position)
		{
			foreach (MarkerNode mNode in mNodes) {
				if (mNode.Bounds.Contains(position)) {
					return mNode;
				}
			}

			return null;
		}

		public void Add(object o)
		{
			if (o is MarkerNode) {
				mNodes.Add(o as MarkerNode);
			}
		}

		#region custom events

		EventHandler<EventArgs> queueRedraw;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<EventArgs> QueueRedraw {
			add {
				queueRedraw += value;
			}
			remove {
				queueRedraw -= value;
			}
		}

		#endregion

		#region properties

		public Rectangle BoundWithExtras {
			get {
				return bound.Inflate(
					new Size(
						MarkerNode.NodeInOutMarkerSize + NodeMargin.HorizontalSpacing,
						NodeMargin.VerticalSpacing
					)
				);
			}
		}

		[XmlIgnore]
		public int Progress {
			get {
				return progress;
			} set {
				progress = value;
				if (queueRedraw != null) {
					queueRedraw(this, null);
				}
			}
		}

		[XmlAttribute("y")]
		public double X {
			get {
				return bound.X;
			}
			set {
				bound.X = value;
			}
		}

		[XmlAttribute("x")]
		public double Y {
			get {
				return bound.Y;
			}
			set {
				bound.Y = value;
			}
		}

		[XmlAttribute("type")]
		public string AlgorithmType {
			get {
				return algorithm.GetType().AssemblyQualifiedName;
			}
			set {
				Type algoType = Type.GetType(value);
				algorithm = Activator.CreateInstance(algoType, this) as BaseAlgorithm;
			}
		}

		[XmlArray("markers")]
		[XmlArrayItem(ElementName = "marker")]
		public List<MarkerNode> MNodes {
			get {
				return mNodes;
			}
			set {
				mNodes = value;
				foreach (MarkerNode mNode in mNodes) {
					mNode.parent = this;
					if (mNode.IsInput) {
						mNode.compatible = algorithm.Input[mNode.Position];
					} else {
						mNode.compatible = algorithm.Output[mNode.Position];
					}
				}
			}
		}

		#endregion
	}
}

