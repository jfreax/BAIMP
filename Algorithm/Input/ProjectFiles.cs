using System;
using System.Collections.Generic;

namespace baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		public ProjectFiles (PipelineNode parent) : base(parent)
		{
			compatibleOutput.Add (new Compatible("out #1", typeof(string[])));
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

