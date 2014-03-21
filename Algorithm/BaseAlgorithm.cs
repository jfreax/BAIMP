using System;
using System.Collections.Generic;

namespace baimp
{
	public enum AlgorithmType {
		Input,
		Filter,
		Extraction,
		Misc
	};

	abstract public class BaseAlgorithm
	{
		public readonly PipelineNode parent;

		protected List<Compatible> compatibleInput;
		protected List<Compatible> compatibleOutput;


		public BaseAlgorithm (PipelineNode parent) {
			this.parent = parent;

			compatibleInput = new List<Compatible> ();
			compatibleOutput = new List<Compatible> ();
		}

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

		public override string ToString() {
			return this.GetType().Name;
		}
	}
}
