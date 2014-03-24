using System;

namespace baimp
{
	public class BaseType<T> : IType where T : class
	{
		protected T raw;

		public BaseType ()
		{
		}

		public BaseType (T raw)
		{
			this.raw = raw;
		}

		public T Data {
			get {
				return raw;
			}
		}
	}
}

