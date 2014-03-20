using System;
using System.Collections.Generic;

namespace baimp
{
	public class Files : BaseAlgorithm
	{
		private List<Compatible> compatibleInput;
		private List<Compatible> compatibleOutput;

		public Files ()
		{
			compatibleInput = new List<Compatible> ();
			compatibleOutput = new List<Compatible> ();

			compatibleOutput.Add (new Compatible("out #1", typeof(string[])));
		}

		#region BaseAlgorithm implementation

		public AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Source;
			}
		}

		public System.Collections.Generic.List<Compatible> CompatibleInput {
			get {
				return compatibleInput;
			}
		}

		public System.Collections.Generic.List<Compatible> CompatibleOutput {
			get {
				return compatibleOutput;
			}
		}

		public override string ToString() {
			return this.GetType().Name;
		}

		#endregion
	}
}

