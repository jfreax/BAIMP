//
//  ProjectFiles.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
﻿using System.Collections.Generic;

namespace Baimp
{
	public class ProjectFiles : BaseAlgorithm
	{
		OptionDropDown fiberComboBox;

		public ProjectFiles(PipelineNode parent, ScanCollection scanCollection) : base(parent, scanCollection)
		{
			output.Add(new Compatible(
				"Scan",
				typeof(TScan)
			));
				
			options.Add(new OptionBool("Masked only", true));

			fiberComboBox = new OptionDropDown("Fiber type", "Unknown");
			options.Add(fiberComboBox);

			request.Add(RequestType.ScanCollection);


			UpdateFiberTypes(scanCollection);
			scanCollection.FilesChanged += (s, e) => UpdateFiberTypes(scanCollection);
		}

		void UpdateFiberTypes(ScanCollection scanCollection)
		{
			HashSet<string> hash = new HashSet<string>();
			foreach (BaseScan scan in scanCollection) {
				foreach (string scantype in scan.AvailableScanTypes()) {
					hash.Add(scantype);
				}
			}

			if (hash.Count == 0) {
				hash.Add("Unknown");
			}

			fiberComboBox.Values = hash.ToArray();
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
				if ((maskedOnly && scan.HasMask) || !maskedOnly) {
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

