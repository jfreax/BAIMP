using System;
using Xwt;

namespace Baimp
{
	public class TFilePath : BaseType<String>
	{
		public TFilePath(string path) : base(path)
		{
		}

		#region implemented abstract members of BaseType

		public override Xwt.Widget ToWidget()
		{
			if (widget == null) {
				widget = new Label(Data);
			}

			return widget;
		}

		#endregion
	}
}

