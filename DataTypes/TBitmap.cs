using System;
using System.Drawing;
using Xwt;

namespace baimp
{
	public class TBitmap : BaseType<Bitmap>
	{
		public TBitmap()
		{
		}

		public TBitmap(Bitmap bitmap) : base(bitmap)
		{
		}

		#region implemented abstract members of BaseType

		public override Xwt.Widget ToWidget()
		{
			return new ImageView();
		}

		#endregion
	}
}

