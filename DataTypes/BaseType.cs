using System;

namespace baimp
{
	public class BaseType<T>
	{
		protected T raw;

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

