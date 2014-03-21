using System;
using System.Collections.Generic;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction (PipelineNode parent) : base(parent)
		{
			compatibleInput.Add (new Compatible("in #1", typeof(string[])));

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

