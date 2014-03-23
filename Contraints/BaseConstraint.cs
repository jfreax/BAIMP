using System;

namespace baimp
{
	public interface BaseConstraint
	{
		bool FulFills(MarkerNode me, MarkerNode other);
	}
}

