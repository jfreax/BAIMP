//
//  Node.cs
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
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;

namespace Baimp
{
	abstract public class Node
	{
		static int global_node_id = 0;
		int id;
		protected Rectangle bounds;
		protected List<Edge> edges;

		object global_id_lock = new object();

		public Node()
		{
			lock (global_id_lock) {
				id = global_node_id;
				global_node_id++;
			}

			edges = new List<Edge>();
		}

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Context.</param>
		abstract public void Draw(Context ctx);

		/// <summary>
		/// Add an edge.
		/// </summary>
		/// <param name="edge">Edge.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		virtual public void AddEdge(Edge edge, bool directed = false)
		{
			if (!directed) {
				edge.to.AddEdge(new Edge(this), true);
			}
			edges.Add(edge);
		}

		/// <summary>
		/// Add an edge to a specified node.
		/// </summary>
		/// <param name="otherNode">Other node.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		public void AddEdgeTo(Node otherNode, bool directed = false)
		{
			AddEdge(new MarkerEdge(otherNode), directed);
		}

		/// <summary>
		/// Removes an edge.
		/// </summary>
		/// <param name="edge">Edge.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		virtual public void RemoveEdge(Edge edge, bool directed = false)
		{
			if (!directed) {
				edge.to.RemoveEdgeTo(this, true);
			}
			edges.Remove(edge);
		}

		/// <summary>
		/// Removes an edge.
		/// </summary>
		/// <param name="otherNode">Other node.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		virtual public void RemoveEdgeTo(Node otherNode, bool directed = false)
		{
			foreach (Edge edge in edges) {
				if (edge.to.ID == otherNode.ID) {
					RemoveEdge(edge, directed);
					RemoveEdgeTo(otherNode, directed);
					return;
				}
			}
		}

		[XmlArray("edges")]
		[XmlArrayItem(ElementName = "connect", Type = typeof(MarkerEdge))]
		[XmlArrayItem(ElementName = "edge", Type = typeof(Edge))]
		public List<Edge> Edges {
			get {
				return edges;
			}
		}

		public virtual Rectangle Bounds {
			get {
				return bounds;
			}
		}

		[XmlAttribute("id")]
		public int ID {
			get {
				return id;
			}
			set {
				id = value;
				if (id > global_node_id)
					global_node_id = id+1;
			}
		}

		public void Add(System.Object o)
		{
			if (o is Edge) {
				AddEdge(o as Edge);
			}
		}
	}
}

