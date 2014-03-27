using System;
using Xwt;

namespace baimp
{
	public abstract class BaseType<T> : IType where T : class
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
		}

		abstract public Xwt.Widget ToWidget();

		#region IDisposable implementation

		public virtual void Dispose()
		{
			if (widget != null) {
				widget.Dispose();
				widget = null;
			}
		}

		#endregion
	}
}

