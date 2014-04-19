using System;
using Xwt;

namespace Baimp
{
	public class TFeature<T> : BaseType<T>, IFeature
	{
		string key;

		public TFeature(string key, T value) : base(value)
		{
			this.key = key;
		}
			
		public override Widget ToWidget()
		{
			if (widget == null) {
				if (string.IsNullOrEmpty(key)) {
					widget = new Label(raw.ToString());
				} else {
					HBox hbox = new HBox();
					hbox.PackStart(new Label(key));
					hbox.PackEnd(new Label(raw.ToString()));

					widget = hbox;
				}
			}

			return widget;
		}

		#region IFeature implementation
		public string Key()
		{
			return key;
		}
		public string Value()
		{
			return raw.ToString();
		}
		#endregion
	}
}

