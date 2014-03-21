using System;
using Xwt.Drawing;

namespace baimp
{
	public abstract class Edge
	{
		private bool active = true;

		public Node from;
		public Node to;


		public Edge (Node from, Node to)
		{
			this.from = from;
			this.to = to;
		}

		public abstract void Draw (Context ctx);


		/// <summary>
		/// Remove this edge from node.
		/// </summary>
		public void Remove()
		{
			from.RemoveEdge (this);
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="baimp.Edge"/> should be drawn or not.
		/// </summary>
		/// <value><c>true</c> draw; otherwise, <c>false</c>.</value>
		public bool Active {
			get {
				return active;
			}
			set {
				active = value;
			}
		}
	}
}

