using System;
using System.Collections.Generic;

namespace Baimp
{
	public class Windower : BaseAlgorithm
	{
		public Windower(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible(
				"Image",
				typeof(TBitmap)
			));

			output.Add(new Compatible(
				"ROI",
				typeof(TBitmap)
			));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			Yield(inputArgs, inputArgs);

			return null;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Misc;
			}
		}

		public override string HelpText {
			get {
				return "Divide the input image into smaller regions";
			}
		}

		#endregion
	}
}

