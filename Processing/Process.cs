using System;
using System.Collections.Generic;
using System.Threading;

namespace baimp
{
	public class Process
	{
		PipelineNode startNode;
		Project project;

		public delegate void OnTaskCompleteDelegate(IType[] result);

		public Process(Project project, PipelineNode startNode)
		{
			this.project = project;
			this.startNode = startNode;

			foreach (RequestType request in startNode.algorithm.Request) {
				switch (request) {
				case RequestType.Filenames:
					startNode.algorithm.requestedData.Add(RequestType.Filenames, project.Files);
					break;
				case RequestType.ScanCollection:
					startNode.algorithm.requestedData.Add(RequestType.ScanCollection, project.scanCollection);
					break;
				}
			}
		}

		/// <summary>
		/// Start to evaluate the pipeline
		/// </summary>
		public void Start(params IType[] input)
		{
			OnTaskCompleteDelegate callback = new OnTaskCompleteDelegate(OnFinish);
			ThreadPool.QueueUserWorkItem(o => {
				IType[] output = startNode.algorithm.Run(startNode.algorithm.requestedData, input);

				callback(output);
			});
		}

		/// <summary>
		/// Callback function called when algorithm finished
		/// </summary>
		/// <param name="output">Output of algorithm.</param>
		private void OnFinish(IType[] output)
		{
			List<Compatible> compatibleOutput = startNode.algorithm.Output;

			if (output.Length != compatibleOutput.Count) {
				throw new ArgumentOutOfRangeException(); // TODO throw a proper exception
			}

			int offsetIndex = startNode.algorithm.Input.Count;
			for (int i = 0; i < output.Length; i++) {
				if (!compatibleOutput[i].Type.IsAssignableFrom(output[i].GetType())) {
					throw new TypeAccessException(); // TODO throw a proper exception
				}

				foreach (MarkerEdge edge in startNode.MNodes[offsetIndex+i].Edges) {
					MarkerNode targetNode = edge.to as MarkerNode;
					targetNode.inputData.Enqueue(output[i]);

					if (targetNode.parent.IsReady()) {
						// TODO queue the start
						Process newProcess = new Process(project, targetNode.parent);
						newProcess.Start(targetNode.parent.DequeueInput());
					}
				}
			}
		}
	}
}

