using System;
using System.Collections.Generic;

namespace baimp
{
	public class Files : BaseAlgorithm
	{
		public Files (PipelineNode parent) : base(parent)
		{
			compatibleOutput.Add (new Compatible(this, "out #1", typeof(string[])));
		}

		#region BaseAlgorithm implementation

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Input;
			}
		}
			
		public override string HelpText {
			get {
				return "Test algorithm";
			}
		}

		#endregion
	}
}

