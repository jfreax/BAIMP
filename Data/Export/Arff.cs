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
			foreach (PipelineNode pNode in pipeline) {
				int offset = pNode.algorithm.Input.Count;
				for (int i = 0; i < pNode.algorithm.Output.Count; i++) {
					if (pNode.MNodes[i+offset].compatible.IsFinal()) {
						foreach (Tuple<IType[], Result[]> resultList in pNode.results) {
							Type genericType = resultList.Item1[i].GetType().GetGenericTypeDefinition();
							if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition()) {
							
								object data = resultList.Item1[i].RawData();
								List<IFeature> featureList = (data as IEnumerable<object>).Cast<IFeature>().ToList();
								if (featureList != null) {
									foreach (IFeature feature in featureList) {
										Console.WriteLine(feature.Key() + " -> " + feature.Value());
									}
								}
							} else {
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

