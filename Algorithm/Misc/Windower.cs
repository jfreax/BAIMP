//
//  Windower.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class Windower : BaseAlgorithm
	{
		public Windower(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible(
				"Image",
				typeof(TBitmap)
			));

			output.Add(new Compatible(
				"ROI",
				typeof(TBitmap)
			));

			options.Add(new Option("Width", 1, int.MaxValue, 64));
			options.Add(new Option("Height", 1, int.MaxValue, 64));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			int width = (int) options[0].Value;
			int height = (int) options[1].Value;
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			if (tbitmap != null) {
				Bitmap bitmap = tbitmap.Data;
				Rectangle rect = new Rectangle(0, 0, width, height);

				for (int y = 0; y < bitmap.Height-height; y += height) {
					rect.Y = y;

					if (IsCanceled) {
						break;
					}

					for (int x = 0; x < bitmap.Width-width; x += width) {
						rect.X = x;
						IType[] ret = new IType[1];
						ret[0] = new TBitmap(bitmap.Clone(rect, bitmap.PixelFormat));

						Yield(ret, null);
					}
				}
			}

			return null;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Misc;
			}
		}

		public override string HelpText {
			get {
				return "Divide the input image into smaller regions";
			}
		}

		public override string Headline()
		{
			return string.Format("Windower {0}x{1}", options[0].Value, options[1].Value);
		}

		public override string ShortName()
		{
			return "windower";
		}

		#endregion
	}
}

