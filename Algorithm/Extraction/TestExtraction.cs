using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction(PipelineNode parent) : base(parent)
		{
			compatibleInput.Add(new Compatible("in #1", typeof(TBitmap[]), new MaximumUses(1)));

			compatibleOutput.Add(new Compatible("out #1", typeof(int)));
			compatibleOutput.Add(new Compatible("out #2", typeof(string)));
		}

		public override IType[] Run(params IType[] inputArgs)
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

