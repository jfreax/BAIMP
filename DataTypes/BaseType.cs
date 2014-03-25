using System;

namespace baimp
{
	public abstract class BaseType<T> : IType where T : class
	{
		protected T raw;

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
	}
}

