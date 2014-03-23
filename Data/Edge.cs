using System;
using Xwt.Drawing;
using System.Xml.Serialization;

namespace baimp
{
	public class Edge
	{
		[XmlIgnore]
		private bool active = true;

		[XmlIgnore]
		public Node to;

		public Edge ()
		{

		}

		public Edge (Node to)
		{
			this.to = to;
		}

		public virtual void Draw (Context ctx) {}


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
			
		/// <summary>
		/// ID of target node
		/// </summary>
		/// <value>To node I.</value>
		[XmlAttribute("to")]
		public int ToNodeID
		{
			get {
				if (to == null) {
					return toid;
				}
				return to.ID;
			}
			set {
				toid = value;
			}
		}

		/// <summary>
		/// Temp variable for xml serializer
		/// </summary>
		private int toid = -1;


		#endregion
	}
}

