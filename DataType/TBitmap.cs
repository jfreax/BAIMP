//
//  TBitmap.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

