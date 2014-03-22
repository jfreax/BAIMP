using System;
using System.Xml.Serialization;

namespace baimp
{
	[XmlRoot("pipeline")]
	public class XmlPipeline
	{
		public XmlPipeline ()
		{
		}
			
		[XmlArrayItem("node")]
		public XmlNode[] nodes;
	}
}

