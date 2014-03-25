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
		Filenames
	}

	abstract public class BaseAlgorithm
	{
		public readonly PipelineNode parent;

		/// <summary>
		/// Input data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> compatibleInput;

		/// <summary>
		/// Output data types, their properties and contraints.
		/// </summary>
		protected List<Compatible> compatibleOutput;

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

			compatibleInput = new List<Compatible>();
			compatibleOutput = new List<Compatible>();
			request = new HashSet<RequestType>();
			requestedData = new Dictionary<RequestType, object>();
		}

		abstract public IType[] Run(Dictionary<RequestType, object> requestedData, IType[] inputArgs);

		abstract public AlgorithmType AlgorithmType {
			get;
		}

		abstract public string HelpText {
			get;
		}

		public List<Compatible> CompatibleInput {
			get {
				return compatibleInput;
			}
		}

		public List<Compatible> CompatibleOutput {
			get {
				return compatibleOutput;
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
