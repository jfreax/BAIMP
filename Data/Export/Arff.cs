using System;
using System.Linq;
using System.Collections.Generic;

namespace Baimp
{
	public class Arff
	{
		readonly List<PipelineNode> pipeline;

		public Arff(List<PipelineNode> nodes)
		{
			pipeline = nodes;
		}

		public void Generate()
		{
			// for every node in the current pipeline
			foreach (PipelineNode pNode in pipeline) {
				int offset = pNode.algorithm.Input.Count;

				// get all output nodes
				for (int i = 0; i < pNode.algorithm.Output.Count; i++) {

					// if this node has a feature as a result
					if (pNode.MNodes[i+offset].compatible.IsFinal()) {

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
		}

		private void AddResult(IFeature feature, Result[] inputs)
		{
			string completeFeatureName = feature.Key();
			string className = string.Empty;

			List<Result> currInputs = new List<Result>();
			currInputs.AddRange(inputs);
			while(currInputs != null && currInputs.Count > 0) {
				List<Result> nextInputs = new List<Result>();
				foreach (Result input in currInputs) {
					if (input.Data != null) {
						if (input.Node.algorithm.AlgorithmType == AlgorithmType.Input && string.IsNullOrEmpty(className)) {
							className = input.Data.ToString();
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
			Console.WriteLine("Class: " + className + " | Feature: " + completeFeatureName + " | Value: " + feature.Value());
		}
	}
}

