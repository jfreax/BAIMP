using System;

namespace Baimp
{
	/// <summary>
	/// Specifies, how often a node can be used.
	/// </summary>
	public class MaximumUses : BaseConstraint
	{
		public readonly int max;

		public MaximumUses(int max)
		{
			this.max = max;
		}

		public bool FulFills(MarkerNode me, MarkerNode other)
		{
			if (me.Edges.Count > max - 1) {
				return false;
			}

			return true;
		}
	}
}

