using System;
using Xwt.Drawing;
using System.Xml.Serialization;

namespace baimp
{
	public class Edge
	{
		[XmlIgnore]
		public Node to;

		public Edge()
		{

		}

		public Edge(Node to)
		{
			this.to = to;
		}

		public virtual void Draw(Context ctx)
		{
		}

		#region properties

		/// <summary>
		/// ID of target node
		/// </summary>
		/// <value>To node I.</value>
		[XmlAttribute("to")]
		public int ToNodeID {
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

