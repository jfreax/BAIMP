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
				if (string.IsNullOrEmpty(name)) {
					widget = new Label(raw.ToString());
				} else {
					HBox hbox = new HBox();
					hbox.PackStart(new Label(name));
					hbox.PackEnd(new Label(raw.ToString()));

					widget = hbox;
				}
			}

			return widget;
		}

	}
}

