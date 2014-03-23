using System;

namespace baimp
{
	/// <summary>
	/// Maximum uses.
	/// </summary>
	public class MaximumUses : BaseConstraint
	{
		public readonly int max;


		public MaximumUses (int max)
		{
			this.max = max;
		}

	}
}

