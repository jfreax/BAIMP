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
		[XmlIgnore]
		public BaseAlgorithm algorithm;

		//[XmlIgnore]
		//public Rectangle bound = new Rectangle(Point.Zero, NodeSize);

		[XmlIgnore]
		private List<MarkerNode> mNodes;

		[XmlIgnore]
		public int progress = 0;

		[XmlIgnore]
		public List<IType[]> results = new List<IType[]>();

		[XmlIgnore]
		public readonly PipelineNodeView view;

		[XmlIgnore]
		private PipelineView parent;


		#region initialize

		/// <summary>
		/// Empty constructur for xml serialization.
		/// Do not use!
		/// </summary>
		public PipelineNode() {
			view = new PipelineNodeView(this);
		}

		public PipelineNode(PipelineView parent, string algoType, Rectangle bound) : this()
		{
			this.parent = parent;
			this.mNodes = new List<MarkerNode>();
			this.AlgorithmType = algoType;

			parent.AddChild(view, bound);

			int i = 0;
			foreach (Compatible c in algorithm.Input) {
				Console.WriteLine("Add input marker");
				this.Add(new MarkerNode(this, c, i, true));
				i++;
			}
			i = 0;
			foreach (Compatible c in algorithm.Output) {
				Console.WriteLine("Add output marker");
				this.Add(new MarkerNode(this, c, i, false));
				i++;
			}
		}

		/// <summary>
		/// Call only if mNodes are set from outside
		/// </summary>
		public void Initialize()
		{
			foreach (MarkerNode mNode in mNodes) {
				mNode.parent = this;
//				view.AddChild(mNode.view,
//					new Rectangle(
//					new Point(
//							mNode.IsInput ? view.BoundPosition.Left - MarkerNode.NodeInOutMarkerSize : view.BoundPosition.Right,
//							view.BoundPosition.Y + view.ContentOffset.Y + (mNode.Position + 1) * MarkerNode.NodeInOutSpace + mNode.Position * 10 /* Height */
//						), new Size(MarkerNode.NodeInOutMarkerSize, 10)
//				));
				Console.WriteLine("Add");
				if (mNode.IsInput) {
					mNode.compatible = algorithm.Input[mNode.Position];
				} else {
					mNode.compatible = algorithm.Output[mNode.Position];
				}
			}
		}

		#endregion

		#region events

		public void OnButtonPressed(ButtonEventArgs e)
		{

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

		private Rectangle tmpPos = Rectangle.Zero;

		[XmlAttribute("y")]
		public double X {
			get {
				return view.Bounds.X;
			}
			set {
				Rectangle bound = view.Bounds;
				bound.X = value;
				if (parent == null) {
					tmpPos.X = bound.X;
				} else {
					Console.WriteLine("Bound #11: " + bound);
					parent.SetChildBounds(view, bound);
				}
			}
		}

		[XmlAttribute("x")]
		public double Y {
			get {
				return view.Bounds.Y;
			}
			set {
				Rectangle bound = view.Bounds;
				bound.Y = value;
				if (parent == null) {
					tmpPos.Y = bound.Y;
				} else {
					Console.WriteLine("Bound #12: " + bound);
					parent.SetChildBounds(view, bound);
				}
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
					view.AddChild(mNode.view);
					mNode.parent = this;
					if (mNode.IsInput) {
						mNode.compatible = algorithm.Input[mNode.Position];
					} else {
						mNode.compatible = algorithm.Output[mNode.Position];
					}
				}
			}
		}

		[XmlElement("save")]
		public bool SaveResult {
			get;
			set;
		}
			
		[XmlIgnore]
		public PipelineView Parent {
			get {
				return parent;
			}
			set {
				parent = value;
				if (tmpPos != Rectangle.Zero) {
					tmpPos.Width = PipelineNodeView.NodeSize.Width;
					tmpPos.Height = PipelineNodeView.NodeSize.Height;

					parent.SetChildBounds(view, tmpPos);
					tmpPos = Rectangle.Zero;

					foreach (MarkerNode mNode in mNodes) {
						mNode.parent = this;
						view.AddChild(mNode.view,
							new Rectangle(
								new Point(
									mNode.IsInput ? view.BoundPosition.Left - MarkerNode.NodeInOutMarkerSize : view.BoundPosition.Right,
									view.BoundPosition.Y + view.ContentOffset.Y + (mNode.Position + 1) * MarkerNode.NodeInOutSpace + mNode.Position * 10 /* Height */
								), new Size(MarkerNode.NodeInOutMarkerSize, 10)
							));
					}
				}
			}
		}

		/// <summary>
		/// Internal use only! (xml serialization)
		/// </summary>
		[XmlIgnore]
		List<Option> _intern_options = new List<Option>();

		/// <summary>
		/// Internal use only! (xml serialization)
		/// </summary>
		/// <value>The _intern_ options.</value>
		[XmlArray("options")]
		[XmlArrayItem(ElementName = "option")]
		public List<Option> _intern_Options {
			get {
				return _intern_options;
			}
			set {
				_intern_options = value;
			}
		}

		#endregion
	}
}

