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

		abstract public void Draw (Context ctx);

		public void AddEdge(Edge edge)
		{
			edges.Add (edge);
		}

		public void RemoveEdge(Edge edge)
		{
			edges.Remove (edge);
		}

		[XmlArray("edges")]
		[XmlArrayItem(ElementName="edge", Type=typeof(Edge))]
		[XmlArrayItem(ElementName="edge", Type=typeof(MarkerEdge))]
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

