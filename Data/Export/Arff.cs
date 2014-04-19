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
										Console.WriteLine(feature.Key() + " -> " + feature.Value());
									}
								}
							} else {
								// if its only a single feature
								IFeature feature = resultList.Item1[i] as IFeature;
								if (feature != null) {
									Console.WriteLine(feature.Key() + " -> " + feature.Value());
								}
							}
						}
					}
				}
			}

		}
	}
}

