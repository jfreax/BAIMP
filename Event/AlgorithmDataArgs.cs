using System;

namespace Baimp
{
	public class AlgorithmDataArgs : EventArgs
	{
		public readonly IType[] data;

		public AlgorithmDataArgs(IType[] data)
		{
			this.data = data;
		}
	}
}

