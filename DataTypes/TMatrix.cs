using System;
using Xwt;

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
			if (Data.GetLength(0) <= 16 && Data.GetLength(1) <= 16) {
				Table t = new Table();

				for(int x = 0; x < Data.GetLength(0); x++) {
					for(int y = 0; y < Data.GetLength(1); y++) {
						t.Add(new Label(Data[x, y].ToString()), x, y);
					}
				}

				return t;
			}

			return new Label("TODO");
		}

		#endregion
	}
}

