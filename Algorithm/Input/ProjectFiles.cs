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

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			ScanCollection scans = requestedData[RequestType.ScanCollection] as ScanCollection;
			int size = scans.Count;

			int i = 0;
			foreach (BaseScan scan in scans) {
				IType[] data = new IType[3];
				// TODO test available scan types
				data[0] = new TBitmap(scan.GetAsBitmap("Intensity"));
				data[1] = new TBitmap(scan.GetAsBitmap("Topography"));
				data[2] = new TBitmap(scan.GetAsBitmap("Color"));

				Yield(data);
				i++;

				SetProgress((int)((i * 100) / size));
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

