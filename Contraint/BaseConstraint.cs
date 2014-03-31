using System;

namespace Baimp
{
	public interface BaseConstraint
	{
		bool FulFills(MarkerNode me, MarkerNode other);
	}
}

