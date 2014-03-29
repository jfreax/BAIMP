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
			Thread.Sleep(400);
			SetProgress(10);
			Thread.Sleep(400);
			SetProgress(20);
			Thread.Sleep(400);
			SetProgress(30);
			Thread.Sleep(400);
			SetProgress(40);
			Thread.Sleep(400);
			SetProgress(50);
			Thread.Sleep(400);
			SetProgress(60);
			Thread.Sleep(400);
			SetProgress(70);
			Thread.Sleep(400);
			SetProgress(80);
			Thread.Sleep(400);
			SetProgress(90);
			Thread.Sleep(400);
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

