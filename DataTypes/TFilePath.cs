using System;
using Xwt;

namespace baimp
{
	public class TFilePath : BaseType<String>
	{
		public TFilePath(string path) : base(path)
		{
		}

		#region implemented abstract members of BaseType

		public override Xwt.Widget ToWidget()
		{
			return new Label(Data);
		}

		#endregion
	}
}

