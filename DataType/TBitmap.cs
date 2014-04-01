﻿using System;
using System.Drawing;
using Xwt;
using System.IO;

namespace Baimp
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
			if (widget == null) {
				using (MemoryStream mStream = new MemoryStream()) {
					Data.Save(mStream, System.Drawing.Imaging.ImageFormat.Png);
					mStream.Seek(0, SeekOrigin.Begin);
					widget = new ImageView(Xwt.Drawing.Image.FromStream(mStream).WithBoxSize(MaxWidgetSize));
				}
			}

			return widget;
		}

		#endregion
	}
}
