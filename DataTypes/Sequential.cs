using System;

namespace baimp
{
	public class Sequential<T>
		where T : IType
	{
		public Sequential()
		{
		}

		public Type InnerType()
		{
			return typeof(T);
		}

		public override string ToString()
		{
			return string.Format("[Sequential] {0}", typeof(T));
		}
	}
}

