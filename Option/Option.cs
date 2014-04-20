using System;
using System.Xml.Serialization;

namespace Baimp
{
	public class Option : BaseOption
	{
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
			this.Name = name;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		public Option(string name, IComparable minValue, IComparable maxValue, IComparable defaultValue)
		{
			this.Name = name;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.DefaultValue = defaultValue;

			this.val = defaultValue;
		}

		[XmlElement("value")]
		public override object Value
		{
			get {
				return val;
			}
			set {
				IComparable val = value as IComparable;
				if (val != null) {

					if (MaxValue != null && val.CompareTo(MaxValue) > 0) {
						this.val = MaxValue;
					} else if (MinValue != null && val.CompareTo(MinValue) < 0) {
						this.val = MinValue;
					} else {
						this.val = val;
					}
				}
			}
		}
	}
}

