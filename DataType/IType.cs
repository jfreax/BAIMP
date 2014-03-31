using System;
using Xwt;

namespace Baimp
{
	public interface IType : IDisposable
	{
		Widget ToWidget();
		string ToString();
	}
}

