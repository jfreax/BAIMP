//
//  Process.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Baimp
{
	public class PipelineProcessor
	{
		QueuedTaskScheduler qts = new QueuedTaskScheduler();
		Dictionary<int, TaskScheduler> priorizedScheduler = new Dictionary<int, TaskScheduler>();
		Project project;
		CancellationToken cancellationToken;

		public delegate void OnTaskCompleteDelegate(PipelineNode startNode,IType[] result,Result[] inputRef);

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.PipelineProcessor"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public PipelineProcessor(Project project, CancellationToken cancellationToken)
		{
			this.project = project;
			this.cancellationToken = cancellationToken;
		}

		/// <summary>
		/// Start to evaluate the pipeline
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="inputResult"></param>
		/// <param name="priority">Thread priority</param>
		public void Start(PipelineNode startNode, Result[] inputResult, int priority)
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
					requestedData.Add(
						RequestType.ScanCollection, 
						new ScanCollection(project.scanCollection)
					);

					break;
				}
			}

			if (!priorizedScheduler.ContainsKey(priority)) {
				priorizedScheduler[priority] = qts.ActivateNewQueue(priority);
			}
								
			var inputResult2 = inputResult;
			Task startTask = Task<IType[]>.Factory.StartNew((x) => {

				if (cancellationToken.IsCancellationRequested) {
					return null;
				}

				var inputResult1 = inputResult2;
				EventHandler<AlgorithmEventArgs> yieldFun = 
					(object sender, AlgorithmEventArgs e) => GetSingleData(startNode, inputResult1, priority, sender, e);

				startNode.algorithm.SetProgress(0);
				startNode.algorithm.Yielded += yieldFun;
				IType[] output = null;
				try {
					startNode.algorithm.cancellationToken = cancellationToken;
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
					Log.Add(LogLevel.Error, this.GetType().Name,
						"Failed to process node \"" + startNode + "\"\n\t" + e.Message);
				}
				startNode.algorithm.Yielded -= yieldFun;
				startNode.algorithm.SetProgress(100);


				return output;
			}, cancellationToken, TaskCreationOptions.AttachedToParent)
				.ContinueWith(fromTask => {

				foreach (Result res in inputResult) {
					res.Finish(startNode);
				}

				IType[] taskOutput = fromTask.Result;
				if (taskOutput != null) { // null means, there is no more data
					Result[] thisInput = new Result[taskOutput.Length];
					int i = 0;
					foreach (IType to in taskOutput) {
						thisInput[i] = new Result(startNode, to, inputResult, startNode.SaveResult);
						i++;
					}
					OnFinish(startNode, priority, taskOutput, thisInput);
				}
			});

		}

		/// <summary>
		/// Callback function called when algorithm finished
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="priority">Current thread priority.</param>
		/// <param name="result">Output of algorithm.</param>
		/// <param name="input">Reference to the data, that was used to compute these results.</param>
		/// <param name="yieldID">Unique identifier for every yielded output from a node.</param>
		void OnFinish(PipelineNode startNode, int priority, IType[] result, Result[] input, int yieldID = -1)
		{
			List<Compatible> compatibleOutput = startNode.algorithm.Output;
			if (result.Length != compatibleOutput.Count) {
				throw new ArgumentOutOfRangeException(); // TODO throw a proper exception
			}
				
			if (startNode.HasFinalNode()) {
				startNode.results.Add(new Tuple<IType[], Result[]>(result, input));
			}

			int offsetIndex = startNode.algorithm.Input.Count;
			for (int i = 0; i < result.Length; i++) {
				Result resultWrapper = new Result(startNode, result[i], input, startNode.SaveResult, yieldID);
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
						this.Start(targetNode.parent, targetNode.parent.DequeueInput(), priority - 1);
					}
				}

				// dispose data when no one uses them
				if (resultWrapper.InUse <= 0 && !resultWrapper.Preserve) {
					resultWrapper.Finish(null);
				}
			}
		}

		readonly Dictionary<object, int> yieldIds = new Dictionary<object, int>();


		/// <summary>
		/// Gets the single, yielded, result.
		/// </summary>
		/// <param name="startNode">Start node.</param>
		/// <param name="origInput">Original input.</param>
		/// <param name="priority">Priority.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		void GetSingleData(PipelineNode startNode, Result[] origInput, int priority, object sender, AlgorithmEventArgs e)
		{
			object yieldKey = e.InputRef == null ? origInput as object : e.InputRef as object;
			if (yieldIds.ContainsKey(yieldKey)) {
				yieldIds[yieldKey]++;
			} else {
				yieldIds[yieldKey] = 1;
			}

			if (e.InputRef != null) {
				Result[] inputResults = new Result[e.InputRef.Length];
				int i = 0;
				foreach (IType input in e.InputRef) {
					inputResults[i] = new Result(
						startNode,
						input,
						origInput.Length > i ? origInput[i].Input : null,
						startNode.SaveResult);
					i++;
				}
				OnFinish(startNode, priority, e.Data, inputResults, yieldIds[yieldKey]);
			} else {
				OnFinish(startNode, priority, e.Data, origInput, yieldIds[yieldKey]);
			}
		}
	}
}

