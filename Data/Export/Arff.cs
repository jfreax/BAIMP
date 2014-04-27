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

namespace Baimp
{
	public class Arff : IExporter
	{
		PipelineView pipeline;

		readonly List<string> attributes = new List<string>();
		readonly HashSet<string> classes = new HashSet<string>();

		// fiber name -> (class name, (attribute index -> value))
		readonly Dictionary<string, Tuple<string, Dictionary<int, string>>> values = 
			new Dictionary<string, Tuple<string, Dictionary<int, string>>>();


		public void Run(PipelineView pipeline)
		{
			this.pipeline = pipeline;

			// for every node in the current pipeline
			foreach (PipelineNode pNode in pipeline.Nodes) {
				int offset = pNode.algorithm.Input.Count;

				// get all output nodes
				for (int i = 0; i < pNode.algorithm.Output.Count; i++) {

					// if this node has a feature as a result
					if (pNode.MNodes[i + offset].compatible.IsFinal()) {

						// iterate throu all results
						foreach (Tuple<IType[], Result[]> resultList in pNode.results) {

							// if its a list of features...
							Type genericType = resultList.Item1[i].GetType().GetGenericTypeDefinition();
							if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition()) {
							
								object data = resultList.Item1[i].RawData();
								List<IFeature> featureList = (data as IEnumerable<object>).Cast<IFeature>().ToList();
								if (featureList != null) {
									// ... then iterate over the whole list
									foreach (IFeature feature in featureList) {
										AddResult(feature, resultList.Item2[i].Input);
									}
								}
							} else {
								// if its only a single feature
								IFeature feature = resultList.Item1[i] as IFeature;
								if (feature != null) {
									AddResult(feature, resultList.Item2[i].Input);
								}
							}
						}
					}
				}
			}

			ToArff();
		}

		private void AddResult(IFeature feature, Result[] inputs)
		{
			string completeFeatureName = feature.Key();
			string className = string.Empty;
			string fiberName = string.Empty;

			List<Result> currInputs = new List<Result>();
			currInputs.AddRange(inputs);
			while (currInputs != null && currInputs.Count > 0) {
				List<Result> nextInputs = new List<Result>();
				foreach (Result input in currInputs) {
					if (input.Data != null) {
						if (input.Node.algorithm.AlgorithmType == AlgorithmType.Input) {
							if (string.IsNullOrEmpty(fiberName)) {
								fiberName = input.Data.ToString();
							}

							BaseScan scan = input.Data as BaseScan;
							if (scan != null) {
								className = scan.FiberType;
							}
						} else {
							completeFeatureName = string.Format("{0}_{1}", input.Node, completeFeatureName);
						}
					}
					if (input.Input != null) {
						nextInputs.AddRange(input.Input);
					}
				}
				currInputs = nextInputs;
			}

			int attributeIndex = attributes.IndexOf(completeFeatureName);
			if (attributeIndex == -1) {
				attributes.Add(completeFeatureName);
				attributeIndex = attributes.Count - 1;
			}

			if (!values.ContainsKey(fiberName)) {
				values[fiberName] = 
					new Tuple<string, Dictionary<int, string>>(className, new Dictionary<int, string>());
			}

			values[fiberName].Item2[attributeIndex] = feature.Value();
			classes.Add(className);
		}

		private void ToArff()
		{
			string arff = string.Format("@relation \"{0}\"\n\n", pipeline.PipelineName);
			foreach (string attr in attributes) {
				arff += string.Format("@attribute \"{0}\" numeric\n", attr);
			}
			arff += "@attribute class {";
			foreach (string className in classes) {
				arff += string.Format("\"{0}\",", className);
			}

			arff = arff.TrimEnd(',') + "}";
			arff += "\n\n@data\n";

			foreach (KeyValuePair<string, Tuple<string, Dictionary<int, string>>> v in values) {
				arff += string.Format("% {0}\n", v.Key);
				for (int i = 0; i < attributes.Count; i++) {
					if (v.Value.Item2.ContainsKey(i)) {
						try {
							double numeric = double.Parse(v.Value.Item2[i]);
							if (double.IsNaN(numeric) || double.IsInfinity(numeric)) {
								arff += "?,";
							} else {
								arff += v.Value.Item2[i].Replace(',', '.') + ",";
							}
						} catch (Exception e) {
							Console.WriteLine(e.Message);
							arff += "?,";
						}
					} else {
						arff += "?,";
					}
				}
				arff += string.Format("\"{0}\"\n", v.Value.Item1);
			}

			Console.WriteLine(arff);
		}
	}
}

