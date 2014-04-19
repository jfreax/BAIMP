using System;
using System.Xml.Serialization;

namespace Baimp
{
	public class Option
	{
		[XmlAttribute]
		public string name;

		[XmlIgnore]
		public readonly IComparable MinValue = null;

		[XmlIgnore]
		public readonly IComparable MaxValue = null;

		[XmlIgnore]
		public readonly IComparable DefaultValue;
	
		[XmlIgnore]
		IComparable val;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public Option() {}

		public Option(string name, IComparable defaultValue)
		{
			this.name = name;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		public Option(string name, IComparable minValue, IComparable maxValue, IComparable defaultValue)
		{
			this.name = name;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		[XmlIgnore]
		public IComparable Value
		{
			get {
				return val;
			}
			set {
				if (value != null) {

					if (MaxValue != null && value.CompareTo(MaxValue) > 0) {
						this.val = MaxValue;
					} else if (MinValue != null && value.CompareTo(MinValue) < 0) {
						this.val = MinValue;
					} else {
						this.val = value;
					}
				}
			}
		}

		[XmlElement("value")]
		public object InternValue {
			get {
				return Value;
			}
			set {
				Value = value as IComparable;
			}
		}
	}
}

