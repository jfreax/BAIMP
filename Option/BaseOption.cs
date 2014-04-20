using System;
using System.Xml.Serialization;

namespace Baimp
{
	[XmlInclude(typeof(Option))]
	[XmlInclude(typeof(OptionBool))]
	public abstract class BaseOption
	{
		[XmlElement("key")]
		public string Name {
			get;
			set;
		}

		[XmlElement("value")]
		public virtual object Value {
			get;
			set;
		}
	}
}

