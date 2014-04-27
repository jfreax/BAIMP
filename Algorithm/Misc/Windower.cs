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

namespace Baimp
{
	public class Windower : BaseAlgorithm
	{
		public Windower(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible(
				"Image",
				typeof(TScan)
			));

			output.Add(new Compatible(
				"ROI",
				typeof(TScan)
			));

			options.Add(new Option("Width", 1, int.MaxValue, 64));
			options.Add(new Option("Height", 1, int.MaxValue, 64));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			int blockWidth = (int) options[0].Value;
			int blockHeight = (int) options[1].Value;
			Xwt.Size blockSize = new Xwt.Size(blockWidth, blockHeight);

			TScan scan = inputArgs[0] as TScan;

			int width = (int) scan.Size.Width;
			int height = (int) scan.Size.Height;

			float[] inputData = scan.Data;
			for (int y = 0; y < height-blockHeight; y += blockHeight) {
				for (int x = 0; x < width-blockWidth; x+= blockWidth) {
					float[] data = new float[blockWidth*blockHeight];

					bool empty = true;
					for (int i = 0; i < blockWidth; i++) {
						for (int j = 0; j < blockHeight; j++) {
							float value = inputData[x + i + ((y + j) * width)];
							data[i + (j * blockWidth)] = value;

							if (value > 0.0) {
								empty = false;
							}
						}
					}

					if (!empty) {
						TScan block = new TScan(data, blockSize, scan.IsMultipleAccessModeOn);
						Yield(new IType[] { block }, null);
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

