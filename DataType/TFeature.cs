using System;
using Xwt;

namespace Baimp
{
	public class TFeature<T> : BaseType<T>
	{
		public TFeature(T value) : base(value)
		{
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

