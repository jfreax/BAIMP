//
//  Galloway.cs
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
	public class Galloway : BaseAlgorithm
	{
		public Galloway(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("GLRL-Matrix", typeof(TMatrix)));

			output.Add(new Compatible("Galloway Features", typeof(TFeatureList<double>)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			TMatrix matrix = inputArgs[0] as TMatrix;
			if (matrix == null) {
				return null;
			}

			double numberOfRuns = matrix.Sum; // R_G

			double sre = 0.0, lre = 0.0, lgre = 0.0, hlre = 0.0;
			double gln = 0.0, rln = 0.0;

			int n = 0; // number of pixels in original image
			for (int i = 0; i < matrix.Width; i++) {
				double glnInner = 0.0;
				for (int j = 0; j < matrix.Height; j++) {
					if (matrix[i, j] >= double.Epsilon) {
						glnInner += matrix[i, j];

						if (j > 0) {
							n += (int) (matrix[i, j] * j);
							sre += matrix[i, j] / (j * j);
							lre += matrix[i, j] * (j * j);
						}
						if (i > 0) {
							lgre += matrix[i, j] / (i * i);
							hlre += matrix[i, j] * (i * i);
						}
					}
				}
				gln += glnInner * glnInner;
			}

			SetProgress(50);

			for (int j = 0; j < matrix.Height; j++) {
				double rlnInner = 0.0;
				for (int i = 0; i < matrix.Width; i++) {
					rlnInner += matrix[i, j];
				}
				rln += rlnInner * rlnInner;
			}
				
			sre /= numberOfRuns;
			lre /= numberOfRuns;
			gln /= numberOfRuns;
			rln /= numberOfRuns;

			double rp = numberOfRuns / n;

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("shortRunEmphasis", sre)
					.AddFeature("longRunEmphasis", lre)
					.AddFeature("grayLevelNonUniformity", gln)
					.AddFeature("runLengthNonUniformity", rln)
					.AddFeature("runPercentage", rp)
					.AddFeature("lowGrayLevelRunEmphasis", lgre)
					.AddFeature("highGrayLevelRunEmphasis", hlre)
			};
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return
					"Extract 5 + 2 features from a Gray level run length matrix." +
					"These features are proposed from Galloway 1975 in \"Texture analysis using gray level run lengths\" " +
					"and from Chu et al. 1990 in \"Use of gray value distribution of run\nlengths for texture analysis\".";
			}
		}

		public override string Headline()
		{
			return "Galloway";
		}

		public override string ShortName()
		{
			return "galloway";
		}

		#endregion
	}
}

