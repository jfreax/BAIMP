using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class Windower : BaseAlgorithm
	{
		public Windower(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible(
				"Image",
				typeof(TBitmap)
			));

			output.Add(new Compatible(
				"ROI",
				typeof(TBitmap)
			));

			options.Add(new Option("Width", 1, int.MaxValue, 64));
			options.Add(new Option("Height", 1, int.MaxValue, 64));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			int width = (int) options[0].Value;
			int height = (int) options[1].Value;
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			if (tbitmap != null) {
				Bitmap bitmap = tbitmap.Data;
				Rectangle rect = new Rectangle(0, 0, width, height);

				for (int y = 0; y < bitmap.Height-height; y += height) {
					rect.Y = y;

					for (int x = 0; x < bitmap.Width-width; x += width) {
						rect.X = x;
						IType[] ret = new IType[1];
						ret[0] = new TBitmap(bitmap.Clone(rect, bitmap.PixelFormat));

						Yield(ret, null);
					}
				}
			}

			return null;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Misc;
			}
		}

		public override string HelpText {
			get {
				return "Divide the input image into smaller regions";
			}
		}

		#endregion
	}
}

