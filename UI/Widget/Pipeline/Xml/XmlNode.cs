using System;
using System.Xml.Serialization;

namespace baimp
{
	public class XmlNode
	{
		[XmlAttribute]
		public int id;

		[XmlAttribute]
		public double x;

		[XmlAttribute]
		public double y;

		[XmlAttribute]
		public string type;

		[XmlArrayItem("edge")]
		public XmlEdge[] edge;

		public XmlNode()
		{
		}

		public XmlNode(int id, double x, double y, string type)
		{
			this.id = id;
			this.x = x;
			this.y = y;
			this.type = type;
		}
	}
}

