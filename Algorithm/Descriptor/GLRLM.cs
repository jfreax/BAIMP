//
//  GLRLM.cs
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
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLRLM : BaseAlgorithm
	{
		public GLRLM(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan)));

			output.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));

			options.Add(new Option("Bpp", 2, 32, 8));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			int bpp = (int) options[0].Value;
			int p = (int) Math.Pow(2, bpp);

			TScan tScan = inputArgs[0] as TScan;
			float[] data = tScan.DataAs(bpp);


			int height = (int) tScan.Size.Height;
			int width = (int) tScan.Size.Width;
			float[,] matrix = new float[p, width+1];

			int maxRunLength = 0;

			for (int y = 0; y < height; y++) {
				int runLength = 1;
				for (int x = 1; x < width; x++) {
					int a = (int) data[width * y + (x - 1)];
					int b = (int) data[width * y + x];

					if (Math.Abs(a - b) <= float.Epsilon)
						runLength++;
					else {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}

						runLength = 1;
					}

					if ((Math.Abs(a - b) <= float.Epsilon) && (x == width - 1)) {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}
					}
					if ((Math.Abs(a - b) > float.Epsilon) && (x == width - 1)) {
						matrix[b, 1]++;
					}
				}
			}
				
			//if (crop) { // make this an option?
			if (maxRunLength < width+1) {
				float[,] matrixTmp = new float[p, maxRunLength+1];
				for (int y = 0; y < maxRunLength+1; y++ ) {
					for (int x = 0; x < p; x++) {
						matrixTmp[x, y] = matrix[x, y];
					}
				}

				matrix = matrixTmp;
			}
			//}


			IType[] ret = { new TMatrix(matrix) };
			return ret;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "Gray Level Run Length";
			}
		}

		public override string ToString()
		{
			return "GLRLM - Gray Level Run Length Matrix";
		}

		public override string Headline()
		{
			return string.Format("GLRLM {0}bpp", options[0].Value);
		}

		public override string ShortName()
		{
			return "glrm";
		}

		#endregion
	}
}

