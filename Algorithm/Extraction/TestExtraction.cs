using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction (PipelineNode parent) : base(parent)
		{
			compatibleInput.Add (new Compatible("in #1", typeof(Image[]), new MaximumUses(1)));

			compatibleOutput.Add (new Compatible("out #1", typeof(int)));
			compatibleOutput.Add (new Compatible("out #2", typeof(string)));
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

