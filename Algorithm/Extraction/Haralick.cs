using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baimp
{
	public class Haralick : BaseAlgorithm
	{
		public Haralick(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Matrix", typeof(TMatrix)));

			output.Add(new Compatible("ASM", typeof(TFeature<long>)));
			output.Add(new Compatible("Contrast", typeof(TFeature<long>)));
		}
			
		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TMatrix tMatrix = inputArgs[0] as TMatrix;
			if (tMatrix == null || tMatrix.Data == null) {
				return null;
			}
			int[,] inputMatrix = tMatrix.Data;
			int Ng = inputMatrix.GetLength(0);


			// ASM
			long asm = 0L;
			Parallel.For(0, inputMatrix.GetLength(0), i => {
				for (int j = 0; j < inputMatrix.GetLength(1); j++) {
					asm += (long) inputMatrix[i, j] * (long) inputMatrix[i, j];
				}
			});
//				from int item in inputMatrix.AsParallel()
//				select (long)item * (long)item)
//				.Sum();

			// Contrast
			long contrast = 0;
			Parallel.For(0, Ng, n => {
				long contrastPart = 0L;
				for (int i = n; i < inputMatrix.GetLength(0); i++) {
					int j = -n + i;
					contrastPart += (long) inputMatrix[i, j];
				}
				contrast += n * n + contrastPart;
			});
				
			return new IType[] { 
				new TFeature<long>(asm),
				new TFeature<long>(contrast)
			};
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Haralick Texture Features";
			}
		}
	}
}

