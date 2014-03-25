﻿using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("in #1", typeof(IType), new MaximumUses(1)));

			output.Add(new Compatible("out #1", typeof(int)));
			output.Add(new Compatible("out #2", typeof(string)));
		}

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			throw new NotImplementedException();
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Test algorithm";
			}
		}
	}
}

