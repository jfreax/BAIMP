using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		public ProjectFiles(PipelineNode parent) : base(parent)
		{
			output.Add(new Compatible(
				"Intensity",
				typeof(Sequential<TBitmap>)
			));
			output.Add(new Compatible(
				"Topography", 
				typeof(Sequential<TBitmap>),
				new MaximumUses(2)
			));
			output.Add(new Compatible(
				"Color",
				typeof(Sequential<TBitmap>)
			));

			request.Add(RequestType.ScanCollection);
		}

		#region BaseAlgorithm implementation

		public override IType[] Run(Dictionary<RequestType, object> requestedData, IType[] inputArgs)
		{
			IType[] output = new IType[3];
			ScanCollection scans = requestedData[RequestType.ScanCollection] as ScanCollection;

			foreach (string key in scans.Keys) {
				foreach (Scan scan in scans[key]) {
				}
			}


			return output;
		}

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

		public override string ToString()
		{
			return "Project files";
		}

		#endregion
	}
}

