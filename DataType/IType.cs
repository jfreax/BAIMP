using System;
using Xwt;

namespace Baimp
{
	public interface IType : IDisposable
	{
		object RawData();
		Widget ToWidget();
		string ToString();
	}
}

