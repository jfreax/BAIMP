using System;

namespace Baimp
{
	public class Census : BaseAlgorithm
	{
		public Census(PipelineNode parent) : base(parent)
		{
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(System.Collections.Generic.Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			throw new NotImplementedException();
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "Census Transform";
			}
		}

		#endregion
	}
}

