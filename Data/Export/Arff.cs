using System;
using System.Linq;
using System.Collections.Generic;

namespace Baimp
{
	public class Arff : IExporter
	{
		readonly List<string> attributes = new List<string>();

		// class name -> (attribute index -> value)
		readonly Dictionary<string, Dictionary<int, string>> values = new Dictionary<string, Dictionary<int, string>>();


		public void Run(List<PipelineNode> pipeline)
		{
			// for every node in the current pipeline
			foreach (PipelineNode pNode in pipeline) {
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

			List<Result> currInputs = new List<Result>();
			currInputs.AddRange(inputs);
			while (currInputs != null && currInputs.Count > 0) {
				List<Result> nextInputs = new List<Result>();
				foreach (Result input in currInputs) {
					if (input.Data != null) {
						if (input.Node.algorithm.AlgorithmType == AlgorithmType.Input) {
							if (string.IsNullOrEmpty(className)) {
								className = input.Data.ToString();
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

			if (!values.ContainsKey(className)) {
				values[className] = new Dictionary<int, string>();
			}

			values[className][attributeIndex] = feature.Value();
		}

		private void ToArff()
		{
			string arff = "";
			foreach (string attr in attributes) {
				arff += string.Format("@attribute {0} numeric\n", attr);
			}
			arff += "@attribute class {";
			foreach (string className in values.Keys) {
				arff += string.Format("\"{0}\",", className);
			}

			arff = arff.TrimEnd(',') + "}";
			arff += "\n\n@data\n";

			foreach (KeyValuePair<string, Dictionary<int, string>> v in values) {
				for (int i = 0; i < attributes.Count; i++) {
					if (v.Value.ContainsKey(i)) {
						try {
							double numeric = double.Parse(v.Value[i]);
							if (double.IsNaN(numeric) || double.IsInfinity(numeric)) {
								arff += "?,";
							} else {
								arff += v.Value[i].Replace(',', '.') + ",";
							}
						} catch (Exception e) {
							arff += "?,";
						}
					} else {
						arff += "?,";
					}
				}
				arff += v.Key + "\n";
			}

			Console.WriteLine(arff);
		}
	}
}

