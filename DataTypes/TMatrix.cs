using System;

namespace baimp
{
	public class TMatrix : BaseType<int[,]>
	{
		public TMatrix() : base()
		{
		}

		public TMatrix(int[,] matrix) : base(matrix)
		{
		}
	}
}

