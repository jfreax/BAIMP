using System;

namespace Baimp
{
	public class AlgorithmEventArgs : EventArgs
	{
		public readonly IType[] Data;
		public readonly IType[] InputRef;

		public AlgorithmEventArgs(IType[] data, params IType[] inputRef)
		{
			this.Data = data;
			this.InputRef = inputRef;
		}
	}
}

