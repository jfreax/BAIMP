using System;
using System.Collections.Generic;

namespace baimp
{
	public class TestExtraction : BaseAlgorithm
	{
		public TestExtraction ()
		{
		}

		public AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}
			
		public List<string> CompatibleInput {
			get {
				throw new NotImplementedException ();
			}
		}
			
		public List<string> CompatibleOutput {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string ToString() {
			return this.GetType().Name;
		}

	}
}

