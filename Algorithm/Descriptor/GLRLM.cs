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
			input.Add(new Compatible("Image", typeof(TScan), new MaximumUses(1)));

			output.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			try {
				TScan tScan2 = inputArgs[0] as TScan;
				byte[] data2 = tScan2.DataAs8bpp();
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}

			TScan tScan = inputArgs[0] as TScan;
			byte[] data = tScan.DataAs8bpp();

			int height = (int) tScan.Size.Height;
			int width = (int) tScan.Size.Width;
			float[,] matrix = new float[256, width+1];

			int maxRunLength = 0;

			for (int y = 0; y < height; y++) {
				int runLength = 1;
				for (int x = 1; x < width; x++) {
					byte a = data[width * y + (x - 1)];
					byte b = data[width * y + x];

					if (a == b)
						runLength++;
					else {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}

						runLength = 1;
					}

					if ((a == b) && (x == width - 1)) {
						matrix[a, runLength]++;
						if (runLength > maxRunLength) {
							maxRunLength = runLength;
						}
					}
					if ((a != b) && (x == width - 1)) {
						matrix[b, 1]++;
					}
				}
			}
				
			//if (crop) { // make this an option?
			if (maxRunLength < width+1) {
				float[,] matrixTmp = new float[256, maxRunLength+1];
				for (int y = 0; y < maxRunLength+1; y++ ) {
					for (int x = 0; x < 255; x++) {
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
			return "GLRLM";
		}

		public override string ShortName()
		{
			return "glrm";
		}

		#endregion
	}
}

