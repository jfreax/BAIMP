using System;
using Xwt.Drawing;
using System.Xml.Serialization;

namespace baimp
{
	public abstract class Edge
	{
		[XmlIgnore]
		private bool active = true;

		[XmlIgnore]
		public Node from;

		[XmlIgnore]
		public Node to;

		public Edge ()
		{

		}

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

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="baimp.Edge"/> should be drawn or not.
		/// </summary>
		/// <value><c>true</c> draw; otherwise, <c>false</c>.</value>
		[XmlIgnore]
		public bool Active {
			get {
				return active;
			}
			set {
				active = value;
			}
		}
			
		[XmlAttribute("toMarker")]
		public int ToNodeID
		{
			get {
				return to.id;
			}
			set {
				to.id = value;
			}
		}

		[XmlAttribute("fromMarker")]
		public int FromNodeID
		{
			get {
				return from.id;
			}
			set {
				from.id = value;
			}
		}

		#endregion
	}
}

