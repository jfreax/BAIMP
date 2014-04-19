using System;
using Xwt;

namespace Baimp
{
	public abstract class BaseType<T> : IType
	{
		public static readonly Size MaxWidgetSize = new Size(300, 200);

		protected T raw;
		protected Widget widget = null;

		public BaseType()
		{
		}

		public BaseType(T raw)
		{
			this.raw = raw;
		}

		public T Data {
			get {
				return raw;
			}
			set {
				raw = value;
			}
		}

		abstract public Widget ToWidget();

		public object RawData()
		{
			return raw as object;
		}

		#region IDisposable implementation

		public virtual void Dispose()
		{
			if (widget != null) {
				widget.Dispose();
				widget = null;
			}

			IDisposable rawDisposable = raw as IDisposable;
			if (rawDisposable != null) {
				rawDisposable.Dispose();
			}

			raw = default(T);
		}

		#endregion
	}
}

