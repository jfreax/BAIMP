﻿using System;
using System.Collections.Generic;
using System.Threading;
using Xwt;
using System.Threading.Tasks;
using System.Reflection;

namespace Baimp
{
	public class Process
	{
		Project project;
		CancellationToken cancellationToken;

		public delegate void OnTaskCompleteDelegate(PipelineNode startNode, IType[] result, Result[] inputRef);

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Process"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public Process(Project project, CancellationToken cancellationToken)
		{
			this.project = project;
			this.cancellationToken = cancellationToken;
		}

		/// <summary>
		/// Start to evaluate the pipeline
		/// </summary>
		public void Start(PipelineNode startNode, Result[] inputResult)
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
				
			//OnTaskCompleteDelegate callback = new OnTaskCompleteDelegate(OnFinish);
			//ThreadPool.QueueUserWorkItem(o => {
			var inputResult2 = inputResult;
			Task<IType[]> startTask = Task<IType[]>.Factory.StartNew( () => {
				var inputResult1 = inputResult2;
				EventHandler<AlgorithmEventArgs> yieldFun = 
					(object sender, AlgorithmEventArgs e) => GetSingleData(startNode, inputResult1, sender, e);

				startNode.algorithm.SetProgress(0);
				startNode.algorithm.Yielded += yieldFun;
				IType[] output = null;
				try {
					output = startNode.algorithm.Run(
						requestedData,
						startNode.algorithm.options.ToArray(),
						input
					);
				} catch (Exception e) {
					Console.WriteLine(e.StackTrace);
					Console.WriteLine(e.Message);
					if (e.InnerException != null) { 
						Console.WriteLine(e.InnerException.Message);
					}
				}
				startNode.algorithm.Yielded -= yieldFun;
				startNode.algorithm.SetProgress(100);

				return output;
			}, TaskCreationOptions.AttachedToParent);
				
			startTask.ContinueWith(fromTask => {
				foreach (Result res in inputResult) {
					res.Finish(startNode);
				}

				IType[] taskOutput = fromTask.Result;
				if (taskOutput != null) { // null means, there is no more data
					OnFinish(startNode, taskOutput, inputResult);
				}
			});
		}

		/// <summary>
		/// Callback function called when algorithm finished
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="result">Output of algorithm.</param>
		/// <param name="input">Reference to the data, that was used to compute these results</param>
		private void OnFinish(PipelineNode startNode, IType[] result, params Result[] input)
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
				HashSet<PipelineNode> markedAsUse = new HashSet<PipelineNode>();
					
				// enqueue new data
				foreach (Edge edge in startNode.MNodes[offsetIndex+i].Edges) {
					MarkerNode targetNode = edge.to as MarkerNode;
					if (targetNode == null) {
						break;
					}

					if (!markedAsUse.Contains(targetNode.parent)) {
						markedAsUse.Add(targetNode.parent);
						resultWrapper.Used(targetNode.parent);
					}

					targetNode.EnqueueInput(resultWrapper);

					// start next node
					if (targetNode.parent.IsReady()) {
						if (cancellationToken.IsCancellationRequested) {
							return;
						}
						this.Start(targetNode.parent, targetNode.parent.DequeueInput());
					}
				}

				// dispose data when no one uses them
				if (resultWrapper.InUse <= 0 && !startNode.SaveResult) {
					resultWrapper.Dispose();
				}
			}
		}

		private void GetSingleData(PipelineNode startNode, Result[] origInput, object sender, AlgorithmEventArgs e)
		{
			if (e.InputRef != null) {
				Result[] inputResults = new Result[e.InputRef.Length];
				int i = 0;
				foreach (IType input in e.InputRef) {
					inputResults[i] = new Result(
						input,
						origInput.Length > i ? origInput[i].Input : null,
						true);
					i++;
				}
				OnFinish(startNode, e.Data, inputResults);
			} else {
				OnFinish(startNode, e.Data, origInput);
			}
		}
	}
}

