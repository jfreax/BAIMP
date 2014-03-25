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

			startNode.algorithm.requestedData.Clear();
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
				if( output != null) { // null means, there is no more data
					callback(output);
				}
			});
		}

		/// <summary>
		/// Callback function called when algorithm finished
		/// </summary>
		/// <param name="result">Output of algorithm.</param>
		private void OnFinish(IType[] result)
		{
			List<Compatible> compatibleOutput = startNode.algorithm.Output;

			if (result.Length != compatibleOutput.Count) {
				throw new ArgumentOutOfRangeException(); // TODO throw a proper exception
			}

			// is one of these outputs sequential, then start next run
			foreach (Compatible comp in compatibleOutput) {
				if (comp.Type.IsGenericType &&
				    comp.Type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Sequential<>))) {
					if (startNode.IsReady()) {
						Process newProcess = new Process(project, startNode);
						newProcess.Start(startNode.DequeueInput());
						break;
					}
				}
			}


			int offsetIndex = startNode.algorithm.Input.Count;
			for (int i = 0; i < result.Length; i++) {
//				if (!compatibleOutput[i].Type.IsAssignableFrom(output[i].GetType())) {
//					throw new TypeAccessException(); // TODO throw a proper exception
//				}

				// enqueue new data
				foreach (MarkerEdge edge in startNode.MNodes[offsetIndex+i].Edges) {
					MarkerNode targetNode = edge.to as MarkerNode;

					bool targetIsParallel = false;
					Type tmpType = startNode.MNodes[offsetIndex + i].compatible.Type;
					if (tmpType.IsGenericType &&
					    tmpType.GetGenericTypeDefinition().IsEquivalentTo(typeof(Parallel<>))) {
						targetIsParallel = true;
					}

					if (result[i].GetType().IsArray && !targetIsParallel) {
						foreach (IType t in (result[i] as IType[])) {
							targetNode.inputData.Enqueue(t);
						}
					} else {
						targetNode.inputData.Enqueue(result[i]);
					}

					// start next node
					if (targetNode.parent.IsReady()) {
						Process newProcess = new Process(project, targetNode.parent);
						newProcess.Start(targetNode.parent.DequeueInput());
					}
				}
			}
		}
	}
}

