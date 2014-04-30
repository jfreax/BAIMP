//
//  GLCM.cs
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
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLCM : BaseAlgorithm
	{
		public GLCM(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan), new MaximumUses(1)));

			output.Add(new Compatible("Co-occurence matrix", typeof(TMatrix)));

			options.Add(new Option("Bpp", 2, 32, 8));
			options.Add(new Option("X Offest", 0, 10, 1));
			options.Add(new Option("Y Offest", 0, 10, 1));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan scan = inputArgs[0] as TScan;
			int bpp = (int) options[0].Value;
			int dx = (int) options[1].Value;
			int dy = (int) options[2].Value;
			int p = (int) Math.Pow(2, bpp);

			TMatrix matrix;
			if (bpp > 10) {
				matrix = new TMatrix(new SparseMatrix<float>(p + 1, p + 1));
			} else {
				matrix = new TMatrix(new float[p + 1, p + 1]);
			}

			int width = (int) scan.Size.Width;
			int height = (int) scan.Size.Height;

			int startX = Math.Max(0, -dx);
			int startY = Math.Max(0, -dy);

			int endX = width - Math.Max(0, dx);
			int endY = height - Math.Max(0, dy);

			int pairs = (endX - startX) * (endY - startY);
			float increment = 1.0f / (float) pairs;

			int offset = Math.Max(0, dx);

			float[] data = scan.Data;
			float max = data.Max();

			if (max > 0.0) {
				unsafe {
					fixed (float* ptrData = data) {
						float* src = ptrData;

						int oldProgress = 0;
						for (int y = startY; y < endY; y++) {
							for (int x = startX; x < endX; x++, src++) {
								int posWithOffset = ((y + dy) * width) + (x + dx);

								int fromValue = (int) ((*src / max) * p);
								int toValue = (int) ((data[posWithOffset] / max) * p);
								matrix[fromValue, toValue] += increment;

							}
							src += offset;

							int progress = (int) (y * 100.0) / height;
							if (progress - oldProgress > 10) {
								oldProgress = progress;
								SetProgress(progress);
							}
						}
					}
				}
			}

			IType[] ret = { matrix };
			return ret;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "GLCM - Gray Level Co-occurence Matrix";
			}
		}

		public override string Headline()
		{
			return string.Format("GLCM d=({1}, {2}) {0}bpp", options[0].Value, options[1].Value, options[2].Value);
		}

		public override string ShortName()
		{
			return "glcm";
		}

		public override string ToString()
		{
			return "GLCM - Gray Level Co-occurence Matrix";
		}

		#endregion
	}
}

