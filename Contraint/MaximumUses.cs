using System;

namespace Baimp
{
	/// <summary>
	/// Specifies, how often a node can be used.
	/// </summary>
	public class MaximumUses : BaseConstraint
	{
		public readonly int Max;

		public MaximumUses(int max)
		{
			this.Max = max;
		}

		public bool FulFills(MarkerNode me, MarkerNode other)
		{
			if (me.Edges.Count > Max - 1) {
				return false;
			}

			return true;
		}
	}
}

