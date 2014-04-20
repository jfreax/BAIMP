using System;
using System.Collections.Generic;

namespace Baimp
{
	public class Census : BaseAlgorithm
	{
		public Census(PipelineNode parent) : base(parent)
		{
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
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

		public override string Headline()
		{
			return "Census Transform";
		}

		public override string ShortName()
		{
			return "census";
		}

		#endregion
	}
}

