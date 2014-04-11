using System;
using Xwt;

namespace Baimp
{
	public class TFeature<T> : BaseType<T>
	{
		string name;

		public TFeature(T value) : base(value)
		{
		}

		public TFeature(string name, T value) : base(value)
		{
			this.name = name;
		}
			
		public override Widget ToWidget()
		{
			if (widget == null) {
				widget = new Label(raw.ToString());
			}

			return widget;
		}

	}
}

