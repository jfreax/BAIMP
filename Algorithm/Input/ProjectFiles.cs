using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		public ProjectFiles (PipelineNode parent) : base(parent)
		{
			compatibleOutput.Add (new Compatible("Intensity", typeof(Image[])));
			compatibleOutput.Add (new Compatible("Topography", typeof(Image[]), new MaximumUses(2)));
			compatibleOutput.Add (new Compatible("Color", typeof(Image[])));
		}

		#region BaseAlgorithm implementation

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Input;
			}
		}
			
		public override string HelpText {
			get {
				return "Test only";
			}
		}

		public override string ToString ()
		{
			return "Project files";
		}

		#endregion
	}
}

