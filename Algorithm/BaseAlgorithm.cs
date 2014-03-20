using System;
using System.Collections.Generic;

namespace baimp
{
	public enum AlgorithmType {
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
