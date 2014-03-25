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

		public override string ToString()
		{
//			string ret = "";
//			for(int i = 0; i < Data.GetLength(0); i++) {
//				for(int j = 0; j < Data.GetLength(1); j++) {
//					ret += Data[i, j] + " ";
//				}
//				ret += "\n";
//			}
//			return ret;

			return String.Format("{0}x{1} Matrix", Data.GetLength(0), Data.GetLength(1));
		}

		#region implemented abstract members of BaseType

		public override Xwt.Widget ToWidget()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

