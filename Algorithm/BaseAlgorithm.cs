using System;
using System.Collections.Generic;

namespace baimp
{
	public enum AlgorithmType {
		Source,
		Filter,
		Extraction,
		Misc
	};

	public interface BaseAlgorithm
	{

		AlgorithmType AlgorithmType {
			get;
		}

		List<Compatible> CompatibleInput {
			get;
		}

		List<Compatible> CompatibleOutput {
			get;
		}
	}
}
