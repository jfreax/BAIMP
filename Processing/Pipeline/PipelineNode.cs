//
//  PipelineNode.cs
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
using System.Xml.Serialization;

namespace Baimp
{
	[XmlRoot("node")]
	public class PipelineNode
	{
		#region static

		static readonly public WidgetSpacing NodeMargin = new WidgetSpacing(0, 6, 0, 6);
		static readonly public double NodeTextMargin = 24;
		static readonly public Size AbsMinNodeSize = new Size(80, 20);
		static readonly public Size AbsMaxNodeSize = new Size(400, 120);
		static readonly public Size NodeInOutSpace = new Size(8, 8);
		static readonly public int NodeRadius = 2;
		static readonly public Color NodeColor = Color.FromBytes(252, 252, 252, 230);
		static readonly public Color NodeColorBorder = Color.FromBytes(222, 222, 222);
		static readonly public Color NodeColorShadow = Color.FromBytes(232, 232, 232);
		static readonly public Color NodeColorGlow = Colors.SkyBlue.WithAlpha(0.4);
		static readonly public Color NodeColorProgress = Color.FromBytes(190, 200, 250);

		#endregion

		Project project;

		Dictionary<int, int> progress = new Dictionary<int, int>();
		[XmlIgnore]
		Dictionary<string, LightImageWidget> icons = new Dictionary<string, LightImageWidget>();
		[XmlIgnore]
		PipelineView parent;
		[XmlIgnore]
		public Point contentOffset = Point.Zero;
		[XmlIgnore]
		public BaseAlgorithm algorithm;
		[XmlIgnore]
		public Rectangle bound = new Rectangle(Point.Zero, AbsMinNodeSize);
		[XmlIgnore]
		public List<MarkerNode> mNodes;
		/// <summary>
		/// Should results from this node be saved?
		/// </summary>
		bool saveResult;
		/// <summary>
		/// True, if mouse is currently over this node
		/// </summary>
		bool hover;
		/// <summary>
		/// Reference to all results + their input data.
		/// Is filled only, if user wants to save them.
		/// 
		/// First element is the actual result data.
		/// Second is the input data.
		/// </summary>
		[XmlIgnore]
		public List<Tuple<IType[], Result[]>> results = new List<Tuple<IType[], Result[]>>();
		/// <summary>
		/// Comments from user
		/// </summary>
		[XmlElement("comment")]
		public string userComment = string.Empty;

		bool isInitialized;

		#region initialize

		/// <summary>
		/// Empty constructur for xml serialization.
		/// Do not use!
		/// </summary>
		public PipelineNode()
		{
			Initialize();
		}

		public PipelineNode(Project project, PipelineView parent, string algoType, Rectangle bound)
		{
			this.project = project;
			this.parent = parent;
			mNodes = new List<MarkerNode>();

			Initialize();

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

			isInitialized = true;
		}

		public void Initialize()
		{
			if (!IsFinalNode()) {
				icons.Add("hide", new LightImageWidget(Image.FromResource("Baimp.Resources.pinup.png")));
				icons.Add("view", new LightImageWidget(Image.FromResource("Baimp.Resources.pindown.png")));
			}

			SaveResult = false;


			InitializeWidgets();
		}

		/// <summary>
		/// Call only, if mNodes are set from outside
		/// </summary>
		public void InitializeNodes(Project project)
		{
			this.project = project;
			AlgorithmType = algorithmType;

			foreach (MarkerNode mNode in mNodes) {
				mNode.parent = this;

				if (mNode.IsInput) {
					if (mNode.Position < algorithm.Input.Count) {
						mNode.compatible = algorithm.Input[mNode.Position];
					}
				} else {
					if (mNode.Position < algorithm.Output.Count) {
						mNode.compatible = algorithm.Output[mNode.Position];
					}
				}
			}

			isInitialized = true;
		}

		public void InitializeWidgets()
		{
			// calculate header height
			TextLayout text = new TextLayout();
			text.Text = "M";
			double textHeight = text.GetSize().Height;
			text.Dispose();

			// add widgets
			if (!IsFinalNode()) {
				icons["hide"].Bounds = new Rectangle(0, 0, textHeight, textHeight);
				icons["view"].Bounds = new Rectangle(0, 0, textHeight, textHeight);

				icons["view"].ButtonPressed += (object sender, ButtonEventArgs e) => SaveResult = false;
				icons["hide"].ButtonPressed += (object sender, ButtonEventArgs e) => SaveResult = true;
			}

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

		void DrawBackground(Context ctx)
		{
			// draw shadow
			ctx.RoundRectangle(bound.Inflate(-1, -1).Offset(1, 3), NodeRadius);
			ctx.SetColor(NodeColorShadow);
			ctx.SetLineWidth(4);
			ctx.Stroke();

			// border
			ctx.RoundRectangle(bound.Inflate(-1, -1), NodeRadius);
			ctx.SetColor(NodeColorBorder);
			ctx.SetLineWidth(2);
			ctx.StrokePreserve();

			// background
			ctx.SetColor(NodeColor);
			ctx.Fill();

			if (hover) { // draw glow
				ctx.RoundRectangle(bound.Inflate(1, 1), NodeRadius * 2);
				ctx.SetColor(NodeColorGlow);
				ctx.SetLineWidth(1);
				ctx.Stroke();
			}
		}

		void DrawProgress(Context ctx)
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
		}

		void DrawHeader(Context ctx)
		{
			TextLayout text = new TextLayout();
			Point textOffset = new Point(8, 4);

			text.Text = algorithm.Headline();
			text.Font = text.Font.WithWeight(FontWeight.Semibold).WithSize(8.0).WithStretch(FontStretch.ExtraCondensed);
			if (text.GetSize().Width >= AbsMaxNodeSize.Width - text.GetSize().Height * 2) {
				text.Width = AbsMaxNodeSize.Width;
				text.Trimming = TextTrimming.WordElipsis;
			}
			Point textPosition = bound.Location.Offset(textOffset);

			// headline background
			contentOffset.X = 6;
			contentOffset.Y = textOffset.Y + text.GetSize().Height + 4;

			ctx.RoundRectangle(bound.Left + 1, bound.Top + 1, bound.Width - 2, contentOffset.Y, NodeRadius);
			ctx.SetColor(Color.FromBytes(222, 222, 222, 110));
			ctx.Fill();

			// text
			ctx.SetColor(Color.FromBytes(32, 32, 32));
			ctx.DrawTextLayout(text, textPosition);

			// icons
			foreach (var icon in icons) {
				if (icon.Value.Visible) {
					icon.Value.Bounds = 
						new Rectangle(bound.Width - icon.Value.Bounds.Width - 10, 3, icon.Value.Bounds.Width, icon.Value.Bounds.Height);
					ctx.DrawImage(
						icon.Value.Image.WithBoxSize(text.GetSize().Height + 2).WithAlpha(0.6),
						bound.Location.Offset(icon.Value.Bounds.Location)
					);
				}
			}

			text.Dispose();
		}

		bool DrawBody(Context ctx)
		{
			bool ret = false;
			TextLayout text = new TextLayout();
			ctx.SetColor(Colors.Black);

			double inputMaxWidth = 0.0;
			double outputMaxWidth = 0.0;

			foreach (MarkerNode mNode in mNodes) {
				text.Text = mNode.compatible.ToString();
				mNode.Height = text.GetSize().Height;
				Point pos = mNode.Bounds.Location;
				if (mNode.IsInput) {
					pos.X = mNode.Bounds.Right + contentOffset.X;
					if (pos.X - bound.X + text.GetSize().Width > inputMaxWidth) {
						inputMaxWidth = pos.X - bound.X + text.GetSize().Width;
					}
				} else {
					pos.X = mNode.Bounds.Left - contentOffset.X - text.GetSize().Width;
					if (bound.Right - pos.X > outputMaxWidth) {
						outputMaxWidth = bound.Right - pos.X;
					}
				}
				ctx.DrawTextLayout(text, pos);
				ctx.Stroke();

				// resize height of widget if necessary
				if (pos.Y + mNode.Height + NodeInOutSpace.Height > bound.Bottom) {
					bound.Bottom = pos.Y + mNode.Height + NodeInOutSpace.Height;
					ret = true;
				}
			}

			// resize width 
			if (inputMaxWidth + outputMaxWidth + NodeTextMargin > bound.Width) {
				bound.Width = inputMaxWidth + outputMaxWidth + NodeTextMargin;
				ret = true;
			}

			text.Dispose();

			return ret;
		}

		#endregion

		/// <summary>
		/// Determines whether the algorithm of this node outputs only final data
		/// </summary>
		/// <returns><c>true</c> if this instance is end node; otherwise, <c>false</c>.</returns>
		public bool IsFinalNode()
		{
			if (algorithm != null && algorithm.Output != null) {
				foreach (Compatible x in algorithm.Output) {
					if (x.IsFinal()) {
						return true;
					}
				}
			}

			return false;
		}

		public override string ToString()
		{
			string name = algorithm.ShortName();

			if (algorithm.Output != null && algorithm.Options.Count >= 1) {
				name += "(";
				foreach (BaseOption option in algorithm.Options) {
					name += option.Value + ",";
				}
				if (name != null) {
					name = name.TrimEnd(',') + ")";
				}
			}
			return name;
		}

		#region events

		public bool OnButtonPressed(ButtonEventArgs e)
		{
			bool ret = false;
			foreach (var x in icons) {
				if (x.Value.Visible && x.Value.Bounds.Contains(e.Position)) {
					x.Value.OnButtonPressed(this, e);
					Redraw();

					ret = true;
					break;
				}
			}

			return ret;
		}

		public void OnMouseEntered()
		{
			hover = true;
			Redraw();
		}

		public void OnMouseExited()
		{
			hover = false;
			Redraw();
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
		/// Add a new marker node.
		/// </summary>
		/// <param name="o">Marker node.</param>
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
			Redraw();
		}

		#region custom events

		void Redraw()
		{
			if (queueRedraw != null) {
				queueRedraw(this, null);
			}
		}

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

		string algorithmType;

		[XmlAttribute("type")]
		public string AlgorithmType {
			get {
				return algorithmType;
			}
			set {
				algorithmType = value;

				if (project != null) {
					Type algoType = Type.GetType(value);
					algorithm = Activator.CreateInstance(algoType, this, project.scanCollection) as BaseAlgorithm;
				}
				SaveResult = saveResult; // to update data
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
				if (IsFinalNode()) {
					saveResult = true;
					icons["hide"].Visible = false;
					icons["view"].Visible = false;
				} else {
					if (algorithm != null && isInitialized) { // after algorithm was set
						Log.Add(LogLevel.Verbose, "Node#" + this, (value ? "Save" : "Discard") + " intermediate results.");
					}

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
		}

		/// <summary>
		/// Internal use only! (xml serialization)
		/// </summary>
		[XmlIgnore]
		List<BaseOption> _intern_options = new List<BaseOption>();

		/// <summary>
		/// Internal use only! (xml serialization)
		/// </summary>
		/// <value>The _intern_ options.</value>
		[XmlArray("options")]
		[XmlArrayItem(ElementName = "option")]
		public List<BaseOption> InternOptions {
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
			}
		}

		#endregion
	}
}

