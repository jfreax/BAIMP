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
	}
}

