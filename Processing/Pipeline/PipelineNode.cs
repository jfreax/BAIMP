﻿using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;

namespace Baimp
{
	[XmlRoot("node")]
	public class PipelineNode
	{
		static readonly public WidgetSpacing NodeMargin = new WidgetSpacing(2, 2, 2, 2);
		static readonly public Size NodeSize = new Size(200, 40);
		static readonly public Size NodeInOutSpace = new Size(8, 8);
		static readonly public int NodeRadius = 2;
		static readonly public Color NodeColor = Color.FromBytes(252, 252, 252);
		static readonly public Color NodeColorBorder = Color.FromBytes(202, 202, 202);
		static readonly public Color NodeColorShadow = Color.FromBytes(232, 232, 232);
		static readonly public Color NodeColorProgress  = Color.FromBytes(190, 200, 250);

		Dictionary<int, int> progress = new Dictionary<int, int>();

		[XmlIgnore]
		private Dictionary<string, LightImageWidget> icons = new Dictionary<string, LightImageWidget>();

		[XmlIgnore]
		PipelineView parent;

		[XmlIgnore]
		public Point contentOffset = Point.Zero;

		[XmlIgnore]
		public BaseAlgorithm algorithm;

		[XmlIgnore]
		public Rectangle bound = new Rectangle(Point.Zero, NodeSize);

		[XmlIgnore]
		public List<MarkerNode> mNodes;

		bool saveResult;

		[XmlIgnore]
		public List<IType[]> results = new List<IType[]>();


		#region initialize

		/// <summary>
		/// Empty constructur for xml serialization.
		/// Do not use!
		/// </summary>
		public PipelineNode() {
			icons.Add("hide", new LightImageWidget(Image.FromResource("Baimp.Resources.hide.png")));
			icons.Add("view", new LightImageWidget(Image.FromResource("Baimp.Resources.view.png")));

			SaveResult = false;

            InitializeWidgets();
		}

		public PipelineNode(PipelineView parent, string algoType, Rectangle bound) : this()
		{
			this.parent = parent;
			mNodes = new List<MarkerNode>();
			AlgorithmType = algoType;
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

		public void InitializeWidgets()
		{
			// calculate header height
			TextLayout text = new TextLayout();
			text.Text = "M";

			double textHeight = text.GetSize().Height;

			// add widgets
			icons["hide"].Bounds = new Rectangle(10, 3, textHeight, textHeight);
			icons["view"].Bounds = new Rectangle(10, 3, textHeight, textHeight);

			icons["view"].ButtonPressed += (object sender, ButtonEventArgs e) => SaveResult = false;
			icons["hide"].ButtonPressed += (object sender, ButtonEventArgs e) => SaveResult = true;

			// set initial position
			OnMove(null);
		}

		#endregion

		#region drawing

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Drawing context.</param>
		public bool Draw(Context ctx)
		{		
			DrawBackground(ctx);
			DrawProgress(ctx);
			DrawHeader(ctx);
			bool ret = DrawBody(ctx);

			return ret;
		}

		private void DrawBackground(Context ctx)
		{
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
		}

		private void DrawProgress(Context ctx)
		{
			int threadsRunning = progress.Keys.Count;

			Rectangle clipBound = bound.Inflate(-1, -1);
			clipBound.Bottom = clipBound.Top + contentOffset.Y;

			double height = threadsRunning == 0 ? clipBound.Height : clipBound.Height / threadsRunning;
			double width = clipBound.Width;
			clipBound.Bottom = clipBound.Top + height;

			List<int> toRemove = new List<int>();
			foreach (var singleProgress in progress) {
				int progressForThread = singleProgress.Value;
				ctx.Save();

				clipBound.Width = width * (progressForThread / 100.0);

				ctx.Rectangle(clipBound);
				ctx.Clip();
				ctx.RoundRectangle(bound.Inflate(-1, -1), NodeRadius);
				ctx.SetColor(NodeColorProgress);
				ctx.Fill();

				ctx.Restore();

				clipBound.Top += height;

				if (progressForThread >= 100 && progress.ContainsKey(singleProgress.Key)) {
					toRemove.Add(singleProgress.Key);
				}
			}

			foreach (int removeID in toRemove) {
				progress.Remove(removeID);
			}

			// input / output buffer

		}

		private void DrawHeader(Context ctx)
		{
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

			// icons
			foreach (var icon in icons) {
				if (icon.Value.Visible) {
					ctx.DrawImage(
						icon.Value.Image.WithBoxSize(text.GetSize().Height + 2).WithAlpha(0.6),
						bound.Location.Offset(icon.Value.Bounds.Location)
					);
				}
			}

			// stroke under headline
			contentOffset.X = 6;
			contentOffset.Y = textOffset.Y + text.GetSize().Height + 4;

			ctx.SetColor(NodeColorBorder);
			ctx.MoveTo(bound.Location.Offset(contentOffset));
			ctx.LineTo(bound.Right - 6, contentOffset.Y + bound.Location.Y);
			ctx.SetLineWidth(1.0);
			ctx.Stroke();
		}

		private bool DrawBody(Context ctx)
		{
			bool ret = false;
			TextLayout text = new TextLayout();
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

		#region events

		public bool OnButtonPressed(ButtonEventArgs e)
		{
			bool ret = false;
			foreach (var x in icons) {
				if (x.Value.Visible && x.Value.Bounds.Contains(e.Position)) {
					x.Value.OnButtonPressed(this, e);
                    if (queueRedraw != null) {
                        queueRedraw(this, null);
                    }
					ret = true;
					break;
				}
			}

			return ret;
		}

		/// <summary>
		/// Raises when this node was moved.
		/// </summary>
		/// <param name="e">Event args.</param>
		public void OnMove(EventArgs e)
		{
			if (e == null)
				return;
			return;
		}

		#endregion

		/// <summary>
		/// Determines whether the algorithm of this node can be executed
		/// </summary>
		/// <returns><c>true</c> if ready; otherwise, <c>false</c>.</returns>
		public bool IsReady()
		{
			foreach (MarkerNode mNode in MNodes) {
				if (mNode.IsInput && mNode.IsInputEmpty()) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Dequeues a set of input parameters and return them.
		/// </summary>
		/// <returns>The input.</returns>
		public Result[] DequeueInput()
		{
			Result[] input = new Result[algorithm.Input.Count];
			for (int i = 0; i < algorithm.Input.Count; i++) {
				input[i] = mNodes[i].DequeueInput();
			}

			return input;
		}

		/// <summary>
		/// Clears the input queue.
		/// </summary>
		public void ClearInputQueue()
		{
			for (int i = 0; i < algorithm.Input.Count; i++) {
				mNodes[i].ClearInput();
			}
		}

		/// <summary>
		/// Gets the marker at position if there is one.
		/// </summary>
		/// <returns>The <see cref="Baimp.MarkerNode"/>.</returns>
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

		/// <summary>
		/// Add a new node
		/// </summary>
		/// <param name="o">O.</param>
		public void Add(object o)
		{
			var markerNode = o as MarkerNode;
			if (markerNode != null) {
				mNodes.Add(markerNode);
			}
		}

		/// <summary>
		/// Get progress (0-100 in percent) for specific working thread id
		/// </summary>
		/// <returns>The progress.</returns>
		/// <param name="threadID">Thread I.</param>
		public int GetProgress(int threadID)
		{
			if (progress.ContainsKey(threadID)) {
				return progress[threadID];
			}

			return -1;
		}

		/// <summary>
		/// Set progress (between 0 and 100) for specific thread id.
		/// </summary>
		/// <param name="threadID">Thread I.</param>
		/// <param name="progress">Progress.</param>
		public void SetProgress(int threadID, int progress)
		{
			this.progress[threadID] = progress;
			if (queueRedraw != null) {
				queueRedraw(this, null);
			}
		}

		#region custom events

		[XmlIgnore]
		public EventHandler<EventArgs> queueRedraw;

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

		/// <summary>
		/// Bounds of this node inclusive marker.
		/// </summary>
		/// <value>The bound with extras.</value>
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

		[XmlElement("save")]
		public bool SaveResult {
			get {
				return saveResult;
			}
			set {
				saveResult = value;
				if (saveResult) {
					icons["hide"].Visible = false;
					icons["view"].Visible = true;
				} else {
					icons["hide"].Visible = true;
					icons["view"].Visible = false;
				}

				if (queueRedraw != null) {
					queueRedraw(this, new EventArgs());
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
		public List<Option> InternOptions {
			get {
				return _intern_options;
			}
			set {
				_intern_options = value;
			}
		}

		[XmlIgnore]
		public PipelineView Parent {
			get {
				return parent;
			}
			set {
				parent = value;
				InitializeWidgets();
			}
		}
		#endregion
	}
}
