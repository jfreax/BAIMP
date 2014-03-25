using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		int position = 0;

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
			ScanCollection scans = requestedData[RequestType.ScanCollection] as ScanCollection;

			int i = 0;
			foreach (string key in scans.Keys) {
				foreach (Scan scan in scans[key]) {
					if (i == position) {
						Console.WriteLine(scan.Name + " for position " + position);
						IType[] output = new IType[3];
						output[0] = new TBitmap(scan.GetAsBitmap(ScanType.Intensity));
						output[1] = new TBitmap(scan.GetAsBitmap(ScanType.Topography));
						output[2] = new TBitmap(scan.GetAsBitmap(ScanType.Color));

						position++;
						return output;
					}
					i++;
				}
			}

			return null;
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

