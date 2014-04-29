//
//  MarkerNode.cs
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
using Xwt.Drawing;
using Xwt;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Baimp
{
	public class MarkerNode : Node
	{
		static Random randomGenerator = new Random();
		static readonly public double NodeInOutMarkerSize = 10;
		static readonly public int NodeInOutSpace = 18;
		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public Compatible compatible;
		/// <summary>
		/// Reference to parent pipeline node.
		/// </summary>
		[XmlIgnore]
		public PipelineNode parent;
		int positionNo;
		ConcurrentQueue<Result> inputData = new ConcurrentQueue<Result>();
		List<Result> inputHistory = new List<Result>();
		object inputHistoryLock = new object();
		/// <summary>Temp variable to store rendering text</summary>
		TextLayout textLayout = new TextLayout();
		/// <summary>
		/// Color of this node.
		/// </summary>
		Color color;

		public MarkerNode()
		{
		}

		public MarkerNode(PipelineNode parent, Compatible compatible, int positionNo, bool isInput) : this()
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

			ctx.RoundRectangle(bndTmp.Inflate(-2, -2), 2);
			if (compatible.IsFinal()) {
				ctx.SetColor(Colors.LightGray);
			} else {
				ctx.RoundRectangle(bndTmp.Inflate(-1, -1), 3);
				LinearGradient g = new LinearGradient(bndTmp.Left, bndTmp.Top, bndTmp.Right, bndTmp.Bottom);
				g.AddColorStop(0, Colors.Black.BlendWith(NodeColor, 0.7));
				g.AddColorStop(1, NodeColor);
				ctx.Pattern = g;
				ctx.Fill();

				ctx.SetColor(NodeColor);
			}
			ctx.Fill();

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
					bool sourceNodeIsAbove = true;
					if (edges.Count > 0 && edges[0] != null) {
						if (edges[0].to.Bounds.Center.Y > Bounds.Center.Y - 4.0) {
							sourceNodeIsAbove = false;
						}
					}

					textLayout.Text = inputBufferSize.ToString();
					double textWidth = textLayout.GetSize().Width;
					double textHeight = textLayout.GetSize().Height;
					Point inputbufferSizeLocation = 
						Bounds.Location.Offset(
							-24 -(textWidth/2),
							sourceNodeIsAbove ? textHeight + 6 : -6 - textHeight);

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
				// if this is a final node
				if (parent.algorithm.Output[positionNo].IsFinal()) {
					ctx.MoveTo(bndTmp.Right + 4, bndTmp.Top);
					ctx.LineTo(bndTmp.Right + 4, bndTmp.Bottom);
					ctx.Stroke();

					// draw output size on end nodes (= number of features
					if (parent.results != null &&
					    parent.results.Count > 0 &&
					    parent.results[0].Item1 != null &&
					    parent.results[0].Item1.Length - 1 >= Position) {

						textLayout.Text = parent.results.Count.ToString();
						double textWidth = textLayout.GetSize().Width;
						double textHeight = textLayout.GetSize().Height;
						Point outputbufferSizeLocation = 
							Bounds.Location.Offset(Bounds.Width * 1.8, 0);

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
						IsInput ? parent.bound.Left + 8 : parent.bound.Right - NodeInOutMarkerSize - 8,
						parent.bound.Y + parent.contentOffset.Y + (positionNo + 1) * NodeInOutSpace + positionNo * Height
					), new Size(Height, Height)
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

		public Color NodeColor {
			get {
				if (color == default(Color)) {
					UInt32 c = ColorList[randomGenerator.Next(ColorList.Length)];
					byte red = (byte) ((c >> 16) & 0xFF);
					byte green = (byte) ((c >> 8) & 0xFF);
					byte blue = (byte) (c & 0xFF);
					color = Color.FromBytes(red, green, blue, 220);
				}
				return color;
			}
			set {
				color = value;
			}
		}

		#endregion

		static uint[] ColorList = {
			0xFFFF4848, 0xFFFF68DD, 0xFFFF62B0, 0xFFFE67EB, 0xFFE469FE, 0xFFD568FD, 0xFF9669FE,
			0xFFFF7575, 0xFFFF79E1, 0xFFFF73B9, 0xFFFE67EB, 0xFFE77AFE, 0xFFD97BFD, 0xFFA27AFE,
			0xFFFF8A8A, 0xFFFF86E3, 0xFFFF86C2, 0xFFFE8BF0, 0xFFEA8DFE, 0xFFDD88FD, 0xFFAD8BFE,
			0xFFFF9797, 0xFFFF97E8, 0xFFFF97CB, 0xFFFE98F1, 0xFFED9EFE, 0xFFE29BFD, 0xFFB89AFE,
			0xFFFFA8A8, 0xFFFFACEC, 0xFFFFA8D3, 0xFFFEA9F3, 0xFFEFA9FE, 0xFFE7A9FE, 0xFFC4ABFE,
			0xFF800080, 0xFF872187, 0xFF9A03FE, 0xFF892EE4, 0xFF3923D6, 0xFF2966B8, 0xFF23819C,
			0xFFBF00BF, 0xFFBC2EBC, 0xFFA827FE, 0xFF9B4EE9, 0xFF6755E3, 0xFF2F74D0, 0xFF2897B7,
			0xFFDB00DB, 0xFFD54FD5, 0xFFB445FE, 0xFFA55FEB, 0xFF8678E9, 0xFF4985D6, 0xFF2FAACE,
			0xFFF900F9, 0xFFDD75DD, 0xFFBD5CFE, 0xFFAE70ED, 0xFF9588EC, 0xFF6094DB, 0xFF44B4D5,
			0xFFFF4AFF, 0xFFDD75DD, 0xFFC269FE, 0xFFAE70ED, 0xFFA095EE, 0xFF7BA7E1, 0xFF57BCD9,
			0xFFFF86FF, 0xFFE697E6, 0xFFCD85FE, 0xFFC79BF2, 0xFFB0A7F1, 0xFF8EB4E6, 0xFF7BCAE1,
			0xFFFFA4FF, 0xFFEAA6EA, 0xFFD698FE, 0xFFCEA8F4, 0xFFBCB4F3, 0xFFA9C5EB, 0xFFA8A8FF,
			0xFF5757FF, 0xFF62A9FF, 0xFF62D0FF, 0xFF06DCFB, 0xFF01FCEF, 0xFF03EBA6, 0xFF01F33E,
			0xFF6A6AFF, 0xFF75B4FF, 0xFF75D6FF, 0xFF24E0FB, 0xFF1FFEF3, 0xFF03F3AB, 0xFF0AFE47,
			0xFF7979FF, 0xFF86BCFF, 0xFF8ADCFF, 0xFF3DE4FC, 0xFF5FFEF7, 0xFF33FDC0, 0xFF4BFE78,
			0xFF8C8CFF, 0xFF99C7FF, 0xFF99E0FF, 0xFF63E9FC, 0xFF74FEF8, 0xFF62FDCE, 0xFF72FE95,
			0xFF9999FF, 0xFF99C7FF, 0xFFA8E4FF, 0xFF75ECFD, 0xFF92FEF9, 0xFF7DFDD7, 0xFF8BFEA8,
			0xFF1FCB4A, 0xFF59955C, 0xFF48FB0D, 0xFF2DC800, 0xFF59DF00, 0xFF9D9D00, 0xFFB6BA18,
			0xFF27DE55, 0xFF6CA870, 0xFF79FC4E, 0xFF32DF00, 0xFF61F200, 0xFFC8C800, 0xFFCDD11B,
			0xFF4AE371, 0xFF80B584, 0xFF89FC63, 0xFF36F200, 0xFF66FF00, 0xFFDFDF00, 0xFFDFE32D,
			0xFF7CEB98, 0xFF93BF96, 0xFF99FD77, 0xFF52FF20, 0xFF95FF4F, 0xFFFFFFAA, 0xFFEDEF85,
			0xFF93EEAA, 0xFFA6CAA9, 0xFFAAFD8E, 0xFF6FFF44, 0xFFABFF73, 0xFFFFFF84, 0xFFEEF093,
			0xFFA4F0B7, 0xFFB4D1B6, 0xFFBAFEA3, 0xFF8FFF6F, 0xFFC0FF97, 0xFFFFFF99, 0xFFF2F4B3,
			0xFFBABA21, 0xFFC8B400, 0xFFDFA800, 0xFFDB9900, 0xFFFFB428, 0xFFFF9331, 0xFFFF800D,
			0xFFE0E04E, 0xFFD9C400, 0xFFF9BB00, 0xFFEAA400, 0xFFFFBF48, 0xFFFFA04A, 0xFFFF9C42,
			0xFFE6E671, 0xFFE6CE00, 0xFFFFCB2F, 0xFFFFB60B, 0xFFFFC65B, 0xFFFFAB60, 0xFFFFAC62,
			0xFFEAEA8A, 0xFFF7DE00, 0xFFFFD34F, 0xFFFFBE28, 0xFFFFCE73, 0xFFFFBB7D, 0xFFFFBD82,
			0xFFEEEEA2, 0xFFFFE920, 0xFFFFDD75, 0xFFFFC848, 0xFFFFD586, 0xFFFFC48E, 0xFFFFC895,
			0xFFF1F1B1, 0xFFFFF06A, 0xFFFFE699, 0xFFFFD062, 0xFFFFDEA2, 0xFFFFCFA4, 0xFFFFCEA2,
			0xFFD1D17A, 0xFFC0A545, 0xFFC27E3A, 0xFFC47557, 0xFFB05F3C, 0xFFC17753, 0xFFB96F6F,
			0xFFD7D78A, 0xFFCEB86C, 0xFFC98A4B, 0xFFCB876D, 0xFFC06A45, 0xFFC98767, 0xFFC48484,
			0xFFDBDB97, 0xFFD6C485, 0xFFD19C67, 0xFFD29680, 0xFFC87C5B, 0xFFD0977B, 0xFFC88E8E,
			0xFFE1E1A8, 0xFFDECF9C, 0xFFDAAF85, 0xFFDAA794, 0xFFCF8D72, 0xFFDAAC96, 0xFFD1A0A0,
			0xFFE9E9BE, 0xFFE3D6AA, 0xFFDDB791, 0xFFDFB4A4, 0xFFD69E87, 0xFFE0BBA9, 0xFFD7ACAC,
			0xFFF70000, 0xFFB9264F, 0xFF990099, 0xFF74138C, 0xFF0000CE, 0xFF1F88A7, 0xFF4A9586,
			0xFFFF2626, 0xFFD73E68, 0xFFB300B3, 0xFF8D18AB, 0xFF5B5BFF, 0xFF25A0C5, 0xFF5EAE9E,
			0xFFFF5353, 0xFFDD597D, 0xFFCA00CA, 0xFFA41CC6, 0xFF7373FF, 0xFF29AFD6, 0xFF74BAAC,
			0xFFFF7373, 0xFFE37795, 0xFFD900D9, 0xFFBA21E0, 0xFF8282FF, 0xFF4FBDDD, 0xFF8DC7BB,
			0xFFFF8E8E, 0xFFE994AB, 0xFFFF2DFF, 0xFFCB59E8, 0xFF9191FF, 0xFF67C7E2, 0xFFA5D3CA,
			0xFFFFA4A4, 0xFFEDA9BC, 0xFFF206FF, 0xFFCB59E8
		};
	}
}

