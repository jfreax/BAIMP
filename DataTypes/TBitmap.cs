using System;
using System.Drawing;
using Xwt;
using System.IO;

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
			MemoryStream mStream = new MemoryStream();
			Data.Save(mStream, System.Drawing.Imaging.ImageFormat.Png);
			mStream.Seek(0, SeekOrigin.Begin);
			ImageView view = new ImageView(Xwt.Drawing.Image.FromStream(mStream).WithBoxSize(MaxWidgetSize));
			mStream.Dispose();

			return view;
		}

		#endregion
	}
}

