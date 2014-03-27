using System;
using Xwt;

namespace baimp
{
	public interface IType : IDisposable
	{
		Widget ToWidget();
		string ToString();
	}
}

