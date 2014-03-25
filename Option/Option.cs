using System;

namespace baimp
{
	public class Option
	{
		public readonly string name;
		public readonly object minValue;
		public readonly object maxValue;
		public readonly object defaultValue;

		private object value;

		public Option(string name, object defaultValue)
		{
			this.name = name;
			this.defaultValue = defaultValue;

			this.value = defaultValue;
		}

		public Option(string name, object minValue, object maxValue, object defaultValue)
		{
			this.name = name;
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.defaultValue = defaultValue;

			this.value = defaultValue;
		}

		public object Value
		{
			get {
				return value;
			}
			set {
				this.value = value;
			}
		}
	}
}

