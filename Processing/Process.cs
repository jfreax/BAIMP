using System;
using System.Collections.Generic;
using System.Threading;
using Xwt;
using System.Threading.Tasks;

namespace Baimp
{
	public class Process
	{
		PipelineNode startNode;
		Project project;

		public delegate void OnTaskCompleteDelegate(IType[] result, Result[] inputRef);

		public Process(Project project, PipelineNode startNode)
		{
			this.project = project;
			this.startNode = startNode;
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
					input[i] = res.Data;
					i++;
				}
			}

			Dictionary<RequestType, object> requestedData = new Dictionary<RequestType, object>();
			foreach (RequestType request in startNode.algorithm.Request) {
				switch (request) {
				case RequestType.ScanCollection:
					requestedData.Add(RequestType.ScanCollection, project.scanCollection);
					break;
				}
			}

			OnTaskCompleteDelegate callback = new OnTaskCompleteDelegate(OnFinish);
			ManagedThreadPool.QueueUserWorkItem(o => {
				bool isSeqData = startNode.algorithm.OutputsSequentialData();
				if (isSeqData) {
					startNode.algorithm.Yielded += GetSingleData;
				}

				startNode.algorithm.SetProgress(0);
				IType[] output = startNode.algorithm.Run(
					                 requestedData,
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
						Application.Invoke( () => callback(output, inputResult) );
					}
				}
			});
		}

		/// <summary>
		/// Callback function called when algorithm finished
		/// </summary>
		/// <param name="result">Output of algorithm.</param>
		/// <param name="input">Reference to the data, that was used to compute these results</param>
		private void OnFinish(IType[] result, params Result[] input)
		{
			List<Compatible> compatibleOutput = startNode.algorithm.Output;

			if (result.Length != compatibleOutput.Count) {
				throw new ArgumentOutOfRangeException(); // TODO throw a proper exception
			}

			if (startNode.SaveResult) {
				startNode.results.Add(new Tuple<IType[], Result[]>(result, input));
			}

			int offsetIndex = startNode.algorithm.Input.Count;
			for (int i = 0; i < result.Length; i++) {
				Result resultWrapper = new Result(result[i], input, startNode.SaveResult);

				Result[] resultWrapperList = null;
//				if (result[i].GetType().IsArray) {
//					resultWrapperList = new Result[(result[i] as IType[]).Length];
//					for (int k = 0; k < (result[i] as IType[]).Length; k++) {
//						resultWrapperList[k] = new Result(ref (result[i] as IType[])[k], startNode.SaveResult);
//					}
//				}
					
				// enqueue new data
				foreach (Edge edge in startNode.MNodes[offsetIndex+i].Edges) {
					MarkerNode targetNode = edge.to as MarkerNode;
					if (targetNode == null) {
						break;
					}

//					bool targetIsParallel = false;
//					Type tmpType = startNode.MNodes[offsetIndex + i].compatible.Type;
//					if (tmpType.IsGenericType &&
//					    tmpType.GetGenericTypeDefinition().IsEquivalentTo(typeof(Parallel<>))) {
//						targetIsParallel = true;
//					}
//
//					if (result[i].GetType().IsArray && !targetIsParallel) {
//						for (int k = 0; k < (result[i] as IType[]).Length; k++) {
//							resultWrapperList[k].Used(targetNode.parent);
//							targetNode.EnqueueInput(resultWrapperList[k]);
//						}
//					} else {
						resultWrapper.Used(targetNode.parent);
						targetNode.EnqueueInput(resultWrapper);
//					}

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

		private void GetSingleData(object sender, AlgorithmEventArgs e)
		{
			if (e.InputRef != null) {
			Result[] inputResults = new Result[e.InputRef.Length];
				int i = 0;
				foreach (IType input in e.InputRef) {
					inputResults[i] = new Result(input, null, true);
					i++;
				}
				OnFinish(e.Data, inputResults);
			} else {
				OnFinish(e.Data, null);
			}
		}
	}
}

