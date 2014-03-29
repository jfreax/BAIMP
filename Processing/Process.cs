using System;
using System.Collections.Generic;
using System.Threading;
using Xwt;

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
				case RequestType.ScanCollection:
					startNode.algorithm.requestedData.Add(RequestType.ScanCollection, project.scanCollection);
					break;
				}
			}
		}

		/// <summary>
		/// Start to evaluate the pipeline
		/// </summary>
		public void Start(Result[] inputResult)
		{
			IType[] input = null;
			if (inputResult != null) {
				input = new IType[inputResult.Length];
				int i = 0;
				foreach (Result res in inputResult) {
					input[i] = res.data;
					i++;
				}
			}
			OnTaskCompleteDelegate callback = new OnTaskCompleteDelegate(OnFinish);
			ThreadPool.QueueUserWorkItem(o => {
				bool isSeqData = startNode.algorithm.OutputsSequentialData();
				if (isSeqData) {
					startNode.algorithm.Yielded += GetSingleData;
				}

				startNode.algorithm.SetProgress(0);
				IType[] output = startNode.algorithm.Run(
					                 startNode.algorithm.requestedData,
					                 startNode.algorithm.options.ToArray(),
					                 input
				                 );
				startNode.algorithm.SetProgress(100);

				foreach (Result res in inputResult) {
					res.Finish(startNode);
				}

				if (isSeqData) {
					startNode.algorithm.Yielded -= GetSingleData;
				} else {
					if (output != null) { // null means, there is no more data
						Application.Invoke( () => callback(output) );
					}
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

			if (startNode.SaveResult) {
				startNode.results.Add(result);
			}

			int offsetIndex = startNode.algorithm.Input.Count;
			for (int i = 0; i < result.Length; i++) {
				Result resultWrapper = new Result(ref result[i], startNode.SaveResult);

				Result[] resultWrapperList = null;
				if (result[i].GetType().IsArray) {
					resultWrapperList = new Result[(result[i] as IType[]).Length];
					for (int k = 0; k < (result[i] as IType[]).Length; k++) {
						resultWrapperList[k] = new Result(ref (result[i] as IType[])[k], startNode.SaveResult);
					}
				}
					
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
						for (int k = 0; k < (result[i] as IType[]).Length; k++) {
							resultWrapperList[k].Used(targetNode.parent);
							targetNode.EnqueueInput(resultWrapperList[k]);
						}
					} else {
						resultWrapper.Used(targetNode.parent);
						targetNode.EnqueueInput(resultWrapper);
					}

					// start next node
					if (targetNode.parent.IsReady()) {
						Process newProcess = new Process(project, targetNode.parent);
						newProcess.Start(targetNode.parent.DequeueInput());
					}
				}

				// dispose data when no one uses them
				if (resultWrapperList == null) {
					if (resultWrapper.InUse <= 0 && !startNode.SaveResult) {
						resultWrapper.Dispose();
					}
				} else {
					foreach (Result res in resultWrapperList) {
						if (res.InUse <= 0 && !startNode.SaveResult) {
							res.Dispose();
						}
					}
				}
			}
		}

		private void GetSingleData(object sender, AlgorithmDataArgs e)
		{
			OnFinish(e.data);
		}
	}
}

