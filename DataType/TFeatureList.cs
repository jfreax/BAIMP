using System;
using System.Collections.Generic;
using Xwt;

namespace Baimp
{
	public class TFeatureList<T> : BaseType<List<TFeature<T>>>
	{
		public TFeatureList() : base()
		{
			this.raw = new List<TFeature<T>>();
		}

		public TFeatureList(List<TFeature<T>> values) : base(values)
		{

		}

		public TFeatureList(params Tuple<string, T>[] values)
		{
			this.raw = new List<TFeature<T>>();
			foreach (var v in values) {
				this.raw.Add(new TFeature<T>(v.Item1, v.Item2));
			}
		}

		public TFeatureList<T> AddFeature(string name, T value)
		{
			raw.Add(new TFeature<T>(name, value));
			return this;
		}

		#region implemented abstract members of BaseType

		public override Widget ToWidget()
		{
			if (widget == null) {
				VBox vbox = new VBox();
				widget = vbox;

				foreach (var v in raw) {
					vbox.PackStart(v.ToWidget());
				}
			}

			return widget;
		}

		#endregion
	}
}

