using System;
using Xwt.Drawing;
using Xwt;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;

namespace baimp
{
	abstract public class Node
	{
		private static int global_node_id = 0;

		private int id;

		protected Rectangle bounds;
		protected List<Edge> edges;

		public Node ()
		{
			id = global_node_id;
			global_node_id++;

			edges = new List<Edge> ();
		}

		/// <summary>
		/// Draw this node.
		/// </summary>
		/// <param name="ctx">Context.</param>
		abstract public void Draw (Context ctx);

		/// <summary>
		/// Add an edge.
		/// </summary>
		/// <param name="edge">Edge.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		virtual public void AddEdge(Edge edge, bool directed = false)
		{
			if (!directed) {
				edge.to.AddEdge (new Edge (this), true);
			}
			edges.Add (edge);
		}

		/// <summary>
		/// Add an edge to a specified node.
		/// </summary>
		/// <param name="otherNode">Other node.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		public void AddEdgeTo(Node otherNode, bool directed = false)
		{
			AddEdge (new MarkerEdge(otherNode), directed);
		}

		/// <summary>
		/// Removes an edge.
		/// </summary>
		/// <param name="edge">Edge.</param>
		/// <param name="directed">If set to <c>true</c> directed.</param>
		virtual public void RemoveEdge(Edge edge, bool directed = false)
		{
			if (!directed) {
				edge.to.RemoveEdgeTo (this, true);
			}
			edges.Remove (edge);
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
					RemoveEdge (edge, directed);
					return;
				}
			}
		}

		[XmlArray("edges")]
        [XmlArrayItem(ElementName = "connect", Type = typeof(MarkerEdge))]
		[XmlArrayItem(ElementName="edge", Type=typeof(Edge))]
        public List<Edge> Edges
		{
			get {
				return edges;
			}
		}

		public virtual Rectangle Bounds {
			get {
				return bounds;
			}
		}

		[XmlElement("id")]
		public int ID {
			get {
				return id;
			}
			set {
				id = value;
				if (id > global_node_id)
					global_node_id = id;
			}
		}

		public void Add(System.Object o) {
			if (o is Edge) {
				AddEdge (o as Edge);
			}
		}
	}
}

