using System;
using Xwt.Drawing;
using Xwt;
using System.Collections.Generic;
using System.Collections;

namespace baimp
{
	abstract public class Node : IEnumerable
	{
		protected Rectangle bounds;
		protected List<Edge> edges;

		public Node ()
		{
			edges = new List<Edge> ();
		}

		abstract public void Draw (Context ctx);


		#region IEnumerable implementation

		public IEnumerator GetEnumerator ()
		{
			return edges.GetEnumerator ();
		}

		#endregion

		public void AddEdge(Edge e)
		{
			edges.Add (e);
		}

		public void RemoveEdge(Edge e)
		{
			edges.Remove (e);
		}

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
	}
}

