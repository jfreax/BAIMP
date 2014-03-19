using System;
using System.Collections.Generic;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		private List<string> compatibleInput;
		private List<string> compatibleOutput;

		public TestExtraction ()
		{
			compatibleInput = new List<string> ();
			compatibleOutput = new List<string> ();

			compatibleInput.Add ("in #1");

			compatibleOutput.Add ("out #1");
			compatibleOutput.Add ("out #2");
		}

		public AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}
			
		public List<string> CompatibleInput {
			get {
				return compatibleInput;
			}
		}
			
		public List<string> CompatibleOutput {
			get {
				return compatibleOutput;
			}
		}

		public override string ToString() {
			return this.GetType().Name;
		}

	}
}

