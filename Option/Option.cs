using System;

namespace baimp
{
	public class Option
	{
		public readonly string name;
		public readonly IComparable minValue = null;
		public readonly IComparable maxValue = null;
		public readonly IComparable defaultValue;

		private IComparable val;

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
	}
}

