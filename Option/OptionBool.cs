using System;
using System.Xml.Serialization;

namespace Baimp
{
	public class OptionBool : BaseOption
	{
		[XmlIgnore]
		public readonly IComparable DefaultValue;

		[XmlIgnore]
		bool val;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public OptionBool() {}

		public OptionBool(string name, bool defaultValue)
		{
			this.Name = name;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

	}
}

