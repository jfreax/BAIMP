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
			Console.WriteLine("Start");
			Thread.Sleep(6000);
			Console.WriteLine("Stop");

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

