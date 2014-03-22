using System;
using System.Xml.Serialization;

namespace baimp
{
	public class XmlEdge
	{
		[XmlAttribute]
		public int from;

		[XmlAttribute]
		public int to;

		public XmlEdge ()
		{
		}

		public XmlEdge (int from, int to)
		{
			this.from = from;
			this.to = to;
		}
	}
}

