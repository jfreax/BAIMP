using System;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("in #1", typeof(IType)));

			output.Add(new Compatible("out #1", typeof(int)));
			output.Add(new Compatible("out #2", typeof(string)));
		}

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			for (int i = 0; i < 100; i++) {
				for(int x = 0; x < 3000000; x++) {
				}
				SetProgress(i);
			}
				
			return null;
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

