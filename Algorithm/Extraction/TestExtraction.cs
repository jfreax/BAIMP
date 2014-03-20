using System;
using System.Collections.Generic;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		private List<Compatible> compatibleInput;
		private List<Compatible> compatibleOutput;

		public TestExtraction ()
		{
			compatibleInput = new List<Compatible> ();
			compatibleOutput = new List<Compatible> ();

			compatibleInput.Add (new Compatible("in #1", typeof(string)));

			compatibleOutput.Add (new Compatible("out #1", typeof(int)));
			compatibleOutput.Add (new Compatible("out #2", typeof(string)));
		}

		public AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}
			
		public List<Compatible> CompatibleInput {
			get {
				return compatibleInput;
			}
		}
			
		public List<Compatible> CompatibleOutput {
			get {
				return compatibleOutput;
			}
		}

		public override string ToString() {
			return this.GetType().Name;
		}

	}
}

