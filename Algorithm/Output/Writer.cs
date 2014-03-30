using System;
using System.Collections.Generic;

namespace Baimp
{
	public class Writer : BaseAlgorithm
	{
		public Writer(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("in", typeof(IType)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			IType input = inputArgs[0];
			if (input.GetType() == typeof(TMatrix)) {
				TMatrix matrix = input as TMatrix;
				Console.WriteLine(matrix);
			}

			return null;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Output;
			}
		}

		public override string HelpText {
			get {
				return "Writes every input to standard output";
			}
		}

		#endregion
	}
}

