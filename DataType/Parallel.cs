﻿using System;

namespace Baimp
{
	public class Parallel<T>
		where T : IType
	{
		public Parallel()
		{
		}

		public Type InnerType()
		{
			return typeof(T);
		}
	}
}
