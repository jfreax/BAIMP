using System;

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
			throw new NotImplementedException();
		}

		#endregion
	}
}

