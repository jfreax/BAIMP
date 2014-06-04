//
//  Arff.cs
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
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xwt;
using Xwt.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Baimp
{
	struct DataHolder
	{
		public string className;
		public string distinctSourceString;
		// attribute index -> value
		public Dictionary<int, object> attributes;

		public DataHolder(string className, string distinctSourceString)
		{
			attributes = new Dictionary<int, object>();
			this.className = className;
			this.distinctSourceString = distinctSourceString;
		}
	}

	struct ResultStats
	{
		public string classname;
		public string fibername;
		public string distinctSourceString;
		public string uncompleteFeatureName;

		public ResultStats(string classname, string fibername, string distinctSourceString, string uncompleteFeatureName)
		{
			this.classname = classname;
			this.fibername = fibername;
			this.distinctSourceString = distinctSourceString;
			this.uncompleteFeatureName = uncompleteFeatureName;
		}
	}

	public class Arff : BaseExporter
	{
		readonly List<string> attributes = new List<string>();
		readonly HashSet<string> classes = new HashSet<string>();
		// fiber name -> (class name, (attribute index -> value))
		readonly Dictionary<string, DataHolder> values = new Dictionary<string, DataHolder>();
		VBox main;
		TextEntry filenameEntry;
		bool exportToStdOut;

		public Arff(PipelineView pipeline) : base(pipeline)
		{

		}

		public override Widget Options()
		{
			if (main != null) {
				main.Dispose();
			}

			main = new VBox();

			// filename
			HBox file = new HBox();
			filenameEntry = new TextEntry();
			filenameEntry.Text = Filename;
			filenameEntry.ReadOnly = true;
			filenameEntry.ShowFrame = false;
			filenameEntry.BackgroundColor = Color.FromBytes(232, 232, 232);

			Button browseButton = new Button("Browse...");
			browseButton.SetFocus();
			browseButton.Clicked += Browse;

			// print to std out
			HBox exportToStdOutBox = new HBox();
			CheckBox exportTSCheck = new CheckBox();

			exportToStdOutBox.PackStart(new Label("Export to standard out?"));
			exportToStdOutBox.PackEnd(exportTSCheck);
			exportTSCheck.Toggled += delegate {
				if (exportTSCheck.Active) {
					browseButton.Style = ButtonStyle.Flat;
					browseButton.Clicked -= Browse;
					exportToStdOut = true;
				} else {
					browseButton.Style = ButtonStyle.Normal;
					browseButton.Clicked += Browse;
					exportToStdOut = false;
				}
			};

			file.PackStart(filenameEntry, true);
			file.PackEnd(browseButton);

			main.PackEnd(file, true);
			main.PackEnd(exportToStdOutBox, true);

			return main;
		}

		public override bool Run()
		{
			if (!exportToStdOut && string.IsNullOrEmpty(filename)) {
				Log.Add(LogLevel.Error, "Arff Exporter", "No filename selected!");
				return false;
			}

			// for every node in the current pipeline
			foreach (PipelineNode pNode in pipeline.Nodes) {
				int offset = pNode.algorithm.Input.Count;

				// get all output nodes
				for (int i = 0; i < pNode.algorithm.Output.Count; i++) {

					// if this node has a feature as a result
					if (pNode.MNodes[i + offset].compatible.IsFinal()) {

						// iterate throu all results
						int resultID = 0;
						foreach (Tuple<IType[], Result[]> resultList in pNode.results) {

							if (resultList == null) {
								continue;
							}

							// if its a list of features...
							Type genericType = resultList.Item1[i].GetType().GetGenericTypeDefinition();
							if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition()) {
							
								object data = resultList.Item1[i].RawData();
								List<IFeature> featureList = (data as IEnumerable<object>).Cast<IFeature>().ToList();
								if (featureList != null) {
									AddResults(
										featureList,
										resultList.Item2[i].Input,
										resultList.Item2[i].SourceString(),
										resultList.Item2[i].DistinctSourceString(),
										pNode
									);
								}
							} else {
								// if its only a single feature
								IFeature feature = resultList.Item1[i] as IFeature;
								if (feature != null) {
									AddResult(
										feature,
										resultList.Item2[i].Input,
										resultList.Item2[i].SourceString(),
										resultList.Item2[i].DistinctSourceString(),
										pNode
									);
								}
							}


							resultID++;
						}
					}
				}
			}

			ToArff();
			return true;
		}

		void AddResult(IFeature feature, Result[] inputs, string sourceString, string distinctSourceString, PipelineNode node)
		{
			ResultStats rs = GetResultStats(inputs, sourceString, distinctSourceString, node);

			AddValue(rs, feature);
		}

		void AddResults(List<IFeature> features, Result[] inputs, string sourceString, string distinctSourceString, PipelineNode node)
		{
			ResultStats rs = GetResultStats(inputs, sourceString, distinctSourceString, node);

			foreach (IFeature feature in features) {
				AddValue(rs, feature);
			}
		}

		ResultStats GetResultStats(Result[] inputs, string sourceString, string distinctSourceString, PipelineNode node)
		{
			string uncompleteFeatureName = node.ToString(); // + "_" + feature.Key();
			string className = string.Empty;
			string fibername = string.Empty;

			List<Result> currInputs = new List<Result>();
			currInputs.AddRange(inputs);
			while (currInputs != null && currInputs.Count > 0) {
				List<Result> nextInputs = new List<Result>();
				foreach (Result input in currInputs) {
					if (input.Data != null) {
						if (input.Node.algorithm.AlgorithmType == AlgorithmType.Input) {
							if (string.IsNullOrEmpty(fibername)) {
								fibername = input.Data.ToString();
							}

							BaseScan scan = input.Data as BaseScan;
							if (scan != null) {
								if (scan.Metadata.ContainsKey("LensMagnification")) {
									className = string.Format("{0}_{1}x", scan.FiberType, scan.Metadata["LensMagnification"]);
								} else {
									className = scan.FiberType;
								}
							}
						} else {
							uncompleteFeatureName = string.Format("{0}_{1}", input.Node, uncompleteFeatureName);
						}
					}
					if (input.Input != null) {
						nextInputs.AddRange(input.Input);
					}
				}
				currInputs = nextInputs;
			}

			distinctSourceString = fibername + distinctSourceString;
			fibername += "_" + sourceString;

			return new ResultStats(className, fibername, distinctSourceString, uncompleteFeatureName);
		}

		void AddValue(ResultStats rs, IFeature feature)
		{
			string completeFeatureName = Regex.Replace(Regex.Replace(rs.fibername, @"#\d*", ""), @".*projectfiles.*_", "") + "_" + rs.uncompleteFeatureName + "_" + feature.Key();

			int attributeIndex = attributes.IndexOf(completeFeatureName);
			if (attributeIndex == -1) {
				attributes.Add(completeFeatureName);
				attributeIndex = attributes.Count - 1;
			}

			if (!values.ContainsKey(rs.fibername)) {
				values[rs.fibername] = new DataHolder(rs.classname, rs.distinctSourceString);
			}

			values[rs.fibername].attributes[attributeIndex] = feature.Value();
			classes.Add(rs.classname);
		}

		void ToArff()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("@relation \"{0}\"\n\n", pipeline.PipelineName);
			foreach (string attr in attributes) {
				sb.AppendFormat("@attribute \"{0}\" numeric\n", attr);
			}
			sb.Append("@attribute class {");
			foreach (string className in classes) {
				sb.AppendFormat("\"{0}\",", className);
			}

			sb.Remove(sb.Length - 1, 1);
			sb.Append("}\n\n@data\n");

			foreach (KeyValuePair<string, DataHolder> v in values) {
				sb.AppendFormat("% {0}\n", v.Value.distinctSourceString);
				for (int i = 0; i < attributes.Count; i++) {
					if (v.Value.attributes.ContainsKey(i)) {
					
						double numeric = (double) v.Value.attributes[i]; 
						if (double.IsNaN(numeric) || double.IsInfinity(numeric)) {
							sb.Append("?,");
						} else {
							sb.AppendFormat("{0},", numeric.ToString(System.Globalization.CultureInfo.InvariantCulture));
						}
					} else {
						sb.Append("?,");
					}
				}
				sb.AppendFormat("\"{0}\"\n", v.Value.className);
			}

			if (exportToStdOut) {
				Console.WriteLine(sb);
			} else {
				File.WriteAllText(filename, sb.ToString());
			}
		}

		#region Properties

		public override string Filename {
			get {
				return filename;
			}
			set {
				filename = value;

				if (filenameEntry != null) {
					filenameEntry.Text = value;
				}
			}
		}

		#endregion
	}
}

