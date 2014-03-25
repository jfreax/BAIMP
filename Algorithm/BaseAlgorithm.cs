using System;
using System.Collections.Generic;

namespace baimp
{
	public enum AlgorithmType
	{
		Input,
		Filter,
		Descriptor,
		Extraction,
		Misc
	}

	public enum RequestType
	{
		Filenames,
		ScanCollection
	}

	abstract public class BaseAlgorithm
	{
		public readonly PipelineNode parent;

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
		/// The requested data.
		/// </summary>
		public Dictionary<RequestType, object> requestedData;


		public BaseAlgorithm(PipelineNode parent)
		{
			this.parent = parent;

			input = new List<Compatible>();
			output = new List<Compatible>();
			request = new HashSet<RequestType>();
			requestedData = new Dictionary<RequestType, object>();
		}

		/// <summary>
		/// Executes the algorithm.
		/// </summary>
		/// <param name="requestedData">Requested data.</param>
		/// <param name="inputArgs">Input arguments.</param>
		/// <remarks>
		/// Return null, when no more data is available (important for sequential data output)
		/// </remarks>
		abstract public IType[] Run(Dictionary<RequestType, object> requestedData, IType[] inputArgs);

		abstract public AlgorithmType AlgorithmType {
			get;
		}

		abstract public string HelpText {
			get;
		}

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

		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}
