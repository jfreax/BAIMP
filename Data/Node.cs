﻿using System;
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

		public int id;

		protected Rectangle bounds;
		protected List<Edge> edges;

		public Node ()
		{
			id = global_node_id;
			global_node_id++;

			edges = new List<Edge> ();
		}

		abstract public void Draw (Context ctx);


//		#region IEnumerable implementation
//
//		public IEnumerator GetEnumerator ()
//		{
//			return edges.GetEnumerator ();
//		}
//
//		#endregion

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
		[XmlArrayItem(ElementName="edge", Type=typeof(PipelineEdge))]
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

		public void Add(System.Object o) {
			if (o is Edge) {
				AddEdge (o as Edge);
			}
		}
	}
}

