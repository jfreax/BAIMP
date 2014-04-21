using System.Collections.Generic;

namespace Baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		public ProjectFiles(PipelineNode parent) : base(parent)
		{
			output.Add(new Compatible(
				"Intensity",
				typeof(TScan)
			));
			output.Add(new Compatible(
				"Topography", 
				typeof(TScan),
				new MaximumUses(2)
			));
			output.Add(new Compatible(
				"Color",
				typeof(TScan)
			));

			options.Add(new OptionBool("Masked only", true));

			request.Add(RequestType.ScanCollection);
		}

		#region BaseAlgorithm implementation

		public override IType[] Run(Dictionary<RequestType, object> requestedData, BaseOption[] options, IType[] inputArgs)
		{
			ScanCollection scans = requestedData[RequestType.ScanCollection] as ScanCollection;
			int size = scans.Count;

			bool maskedOnly = (bool) options[0].Value;

			int i = 0;
			foreach (BaseScan scan in scans) {
				if (IsCanceled) {
					break;
				}

				IType[] data = new IType[3];
				if ((maskedOnly && scan.Mask.HasMask()) || !maskedOnly) {
					// TODO test available scan types
					data[0] = new TScan(scan, "Intensity", maskedOnly: maskedOnly).Preload();
					data[1] = new TScan(scan, "Topography", maskedOnly: maskedOnly).Preload();
					data[2] = new TScan(scan, "Color", maskedOnly: maskedOnly).Preload();

					Yield(data, scan);
				}

				i++;

				SetProgress((i * 100) / size);
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
				return "Load scan data.";
			}
		}

		public override string Headline()
		{
			return ToString();
		}

		public override string ShortName()
		{
			return "projectfiles";
		}

		public override string ToString()
		{
			return "Project files";
		}

		#endregion
	}
}

