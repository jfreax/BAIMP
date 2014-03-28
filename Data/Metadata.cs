using System;
using System.Xml.Serialization;

namespace baimp
{
	public class Metadata
	{
		[XmlAttribute]
		public string key = "";

		[XmlAttribute]
		public string value = "";

		public Metadata()
		{
		}

		public Metadata(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}
}

