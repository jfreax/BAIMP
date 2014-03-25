﻿using System;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		public ProjectFiles(PipelineNode parent) : base(parent)
		{
			compatibleOutput.Add(new Compatible("Intensity", typeof(TBitmap[])));
			compatibleOutput.Add(new Compatible("Topography", typeof(TBitmap[]), new MaximumUses(2)));
			compatibleOutput.Add(new Compatible("Color", typeof(TBitmap[])));

			request.Add(RequestType.Filenames);
		}

		#region BaseAlgorithm implementation

		public override IType[] Run(Dictionary<RequestType, object> requestedData, IType[] inputArgs)
		{
			throw new NotImplementedException();
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

