using System;
using System.Collections.Generic;
using Xwt;
using System.Threading;

namespace Baimp
{
	public enum AlgorithmType
	{
		Input,
		Output,
		Filter,
		Descriptor,
		Extraction,
		Misc
	}

	public enum RequestType
	{
		ScanCollection
	}

	abstract public class BaseAlgorithm
	{
		public readonly PipelineNode Parent;

		/// <summary>
		/// Input data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> input;

		/// <summary>
		/// Output data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> output;

		/// <summary>
		/// List of data we want from the program
		/// </summary>
		protected HashSet<RequestType> request;

		/// <summary>
		/// List of all options
		/// </summary>
		public List<Option> options;

		/// <summary>
		/// The cancellation token.
		/// </summary>
		public CancellationToken cancellationToken;


		internal BaseAlgorithm(PipelineNode parent)
		{
			this.Parent = parent;

			input = new List<Compatible>();
			output = new List<Compatible>();
			request = new HashSet<RequestType>();
			options = new List<Option>();
		}

		/// <summary>
		/// Executes the algorithm.
		/// </summary>
		/// <param name="requestedData">Requested data.</param>
		/// <param name="options">User setted options</param>
		/// <param name="inputArgs">Input arguments.</param>
		/// <remarks>
		/// Return null, when no more data is available (important for sequential data output).
		/// Use Yield() function to return data when one output parameter is Parallel.
		/// </remarks>
		abstract public IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs);

		abstract public AlgorithmType AlgorithmType {
			get;
		}

		/// <summary>
		/// Gets the help text.
		/// </summary>
		/// <value>The help text.</value>
		abstract public string HelpText{
			get;
		}

		/// <summary>
		/// Text shown on top of node in pipeline graph.
		/// </summary>
		abstract public string Headline();

		/// <summary>
		/// String used to represent this node in exported data.
		/// Should be short and without any spaces, new lines and special characters.
		/// </summary>
		/// <returns>The name.</returns>
		abstract public string ShortName();

		/// <summary>
		/// Text shown in algorithm tree viewer.
		/// </summary>
		public override string ToString()
		{
			return this.GetType().Name;
		}

		#region do not override

		/// <summary>
		/// Yield the specified data array.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="inputRef">Reference to input data to compute the result.</param>
		protected void Yield(IType[] data, params IType[] inputRef)
		{
			int threadID = Thread.CurrentThread.ManagedThreadId;
			if (yielded[threadID] != null) {
				var lYield = yielded[threadID];
					try {
						lYield(this, new AlgorithmEventArgs(data, inputRef));
					} catch (Exception e) {
						Console.WriteLine(e.StackTrace);
					}
			}
		}

		/// <summary>
		/// Sets the progress
		/// </summary>
		/// <param name="percent">Progress 0-100.</param>
		public void SetProgress(int percent)
		{
			int threadID = Thread.CurrentThread.ManagedThreadId;
			Application.Invoke( () => Parent.SetProgress(threadID, percent));
		}

		public bool IsCanceled {
			get {
				if (cancellationToken.IsCancellationRequested) {
					return true;
				}

				return false;
			}
		}

		#endregion

		#region events

		Dictionary<int, EventHandler<AlgorithmEventArgs>> yielded = 
			new Dictionary<int, EventHandler<AlgorithmEventArgs>>();

		private object yield_lock = new object();

		/// <summary>
		/// Yield sequential data
		/// </summary>
		public event EventHandler<AlgorithmEventArgs> Yielded {
			add {
				lock(yield_lock) {
					int threadID = Thread.CurrentThread.ManagedThreadId;
					if (!yielded.ContainsKey(threadID)) {
						yielded[threadID] = null;
					} 
					yielded[threadID] += value;
				}
			}
			remove {
				lock (yield_lock) {
					yielded[Thread.CurrentThread.ManagedThreadId] -= value;
				}
			}
		}
			
		#endregion

		#region properties

		public List<Compatible> Input {
			get {
				return input;
			}
		}

		public List<Compatible> Output {
			get {
				return output;
			}
		}

		public HashSet<RequestType> Request {
			get {
				return request;
			}
		}

		public List<Option> Options {
			get {
				return options;
			}
		}

		#endregion
	}
}
