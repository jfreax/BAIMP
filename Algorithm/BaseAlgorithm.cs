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

		List<string> CompatibleInput {
			get;
		}

		List<string> CompatibleOutput {
			get;
		}
	}
}
