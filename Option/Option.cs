using System;
using System.Xml.Serialization;

namespace baimp
{
	public class Option
	{
		[XmlAttribute]
		public string name;

		[XmlIgnore]
		public readonly IComparable minValue = null;

		[XmlIgnore]
		public readonly IComparable maxValue = null;

		[XmlIgnore]
		public readonly IComparable defaultValue;
	
		[XmlIgnore]
		private IComparable val;

		/// <summary>
		/// For xml serialization only. Do not use!
		/// </summary>
		public Option() {}

		public Option(string name, IComparable defaultValue)
		{
			this.name = name;
			this.defaultValue = defaultValue;

			this.val = defaultValue;
		}

		public Option(string name, IComparable minValue, IComparable maxValue, IComparable defaultValue)
		{
			this.name = name;
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.defaultValue = defaultValue;

			this.val = defaultValue;
		}

		[XmlIgnore]
		public IComparable Value
		{
			get {
				return val;
			}
			set {
				if (maxValue != null && value.CompareTo(maxValue) > 0) {
					this.val = maxValue;
				} else if (minValue != null && value.CompareTo(minValue) < 0) {
					this.val = minValue;
				} else {
					this.val = value;
				}
			}
		}

		[XmlAttribute("value")]
		public object _intern_value {
			get {
				return Value;
			}
			set {
				Value = value as IComparable;
			}
		}
	}
}

