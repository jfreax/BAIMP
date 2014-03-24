using System;
using System.Collections.Generic;

namespace baimp
{
	public class Process
	{
		PipelineNode startNode;

		public Process(PipelineNode startNode)
		{
			this.startNode = startNode;
		}

		/// <summary>
		/// Start to evaluate the pipeline
		/// </summary>
		public void Start()
		{
			IType[] input = new IType[startNode.algorithm.CompatibleInput.Count];
			for (int i = 0; i < startNode.algorithm.CompatibleInput.Count; i++) {
				input[i] = startNode.MNodes[i].inputData.Dequeue();
			}

			List<Compatible> compatibleOutput = startNode.algorithm.CompatibleOutput;
			IType[] output = startNode.algorithm.Run(input); // TODO run in extra thread

			// TODO run this in main thread up from here
			if (output.Length != compatibleOutput.Count) {
				throw new ArgumentOutOfRangeException(); // TODO throw a proper exception
			}

			int offsetIndex = startNode.algorithm.CompatibleInput.Count;
			for(int i = 0; i < output.Length; i++) {
				if (!compatibleOutput[i].Type.IsAssignableFrom(output[i].GetType())) {
					throw new TypeAccessException(); // TODO throw a proper exception
				}

				foreach (MarkerEdge edge in startNode.MNodes[offsetIndex+i].Edges) {
					MarkerNode targetNode = edge.to as MarkerNode;
					targetNode.inputData.Enqueue(output[i]);

					if (targetNode.parent.IsReady()) {
						// TODO queue the start
						Process newProcess = new Process(targetNode.parent);
						newProcess.Start();
					}
				}
			}
		}

	}
}

