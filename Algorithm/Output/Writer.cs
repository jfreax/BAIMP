//
//  Writer.cs
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
	public class Writer : BaseAlgorithm
	{
		public Writer(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			input.Add(new Compatible("in", typeof(IType)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			IType input = inputArgs[0];
			if (input.GetType() == typeof(TMatrix)) {
				TMatrix matrix = input as TMatrix;
				Console.WriteLine(matrix);
			}

			return null;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Output;
			}
		}

		public override string Headline()
		{
			return "Writer";
		}

		public override string ShortName()
		{
			return "Writer";
		}

		public override string HelpText {
			get {
				return "Writes every input to standard output";
			}
		}

		#endregion
	}
}

