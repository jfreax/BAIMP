//
//  LBP.cs
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
using System;
using System.Linq;
using System.Collections.Generic;

namespace Baimp
{
	public class LBP : BaseAlgorithm
	{
		public LBP(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("Image", typeof(TScan), new MaximumUses(1)));

			output.Add(new Compatible("LBP Feature Vector", typeof(TFeatureList<double>)));
			output.Add(new Compatible("LBP Histogram", typeof(THistogram)));

			options.Add(new Option("Block size", 2, 32, 3));
			options.Add(new OptionBool("Normalize", true));
			options.Add(new OptionBool("Rotation invariant", true));
			options.Add(new OptionBool("Uniform LBP", true));
		}


		public unsafe override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TScan scan = inputArgs[0] as TScan;
			int blockSize = (int) options[0].Value;
			bool normalize = (bool) options[1].Value;
			bool rotationInvariant = (bool) options[2].Value;
			bool uniformLBP = (bool) options[3].Value;

			byte[] scanData = scan.DataAs8bpp();

			int width = scan.Width;
			int height = scan.Height;

			int[] nPatterns = new int[height * width];

			fixed (byte* ptrData = scanData) {
				fixed (int* ptrNPatterns = nPatterns) {
					// skip the first line
					byte* data = ptrData + width;
					int* neighbor = ptrNPatterns + width;

					for (int y = 1; y < height - 1; y++) {
					
						// and skip first column
						data++;
						neighbor++;

						for (int x = 1; x < width - 1; x++, data++, neighbor++) {
							byte n11 = *(data + width + 1);
							byte n12 = *(data + 1);
							byte n13 = *(data - width + 1);
							byte n21 = *(data + width);
							byte n22 = *data;
							byte n23 = *(data - width);
							byte n31 = *(data + width - 1);
							byte n32 = *(data - 1);
							byte n33 = *(data - width - 1);

							int sum = 0;
							if (n22 < n11)
								sum += 1 << 0;
							if (n22 < n12)
								sum += 1 << 1;
							if (n22 < n13)
								sum += 1 << 2;
							if (n22 < n21)
								sum += 1 << 3;
							if (n22 < n23)
								sum += 1 << 4;
							if (n22 < n31)
								sum += 1 << 5;
							if (n22 < n32)
								sum += 1 << 6;
							if (n22 < n33)
								sum += 1 << 7;

							if (rotationInvariant && sum != 0) {
								while ((sum & 0x80) == 0) {
									sum <<= 1;
								}
							}
									
							*neighbor = sum;
						}

						neighbor++;
						data++;
					}
				}
			}
				
			// compute histogram
			double[] histogram = new double[256];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					histogram[nPatterns[x + y * width]]++;
				}
			}

			histogram[0] = 0d;

			double histogramMax = histogram.Max();

			TFeatureList<double> featureList = new TFeatureList<double>();
			for (int i = 0; i < histogram.Length; i++) {
				if (normalize) {
					histogram[i] /= histogramMax;
				}

				if (!uniformLBP) {
					featureList.AddFeature("LBP#" + i, histogram[i]);
				}
			}
			return new IType[] { 
				featureList,
				new THistogram(histogram)
			};
		}

		public override string Headline()
		{
			bool uniformLBP = (bool) options[3].Value;
			bool rotationInvariant = (bool) options[2].Value;

			string head = "";
			if (uniformLBP) {
				head += "Uniform ";
			}
			if (rotationInvariant) {
				head += "(Rotation Invariant) ";
			}

			return head + "LBP";
		}

		public override string ShortName()
		{
			return "LBP";
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				// TODO
				return "Local Binary Pattern";
			}
		}
	}
}

