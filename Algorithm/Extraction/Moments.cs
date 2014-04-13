using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Baimp
{
	public class Moments : BaseAlgorithm
	{
		public Moments(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("Moments", typeof(TFeatureList<double>)));
		}

		#region implemented abstract members of BaseAlgorithm

		public unsafe override IType[] Run(System.Collections.Generic.Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			Bitmap bitmap = tbitmap.Data;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );

			double M00 = 0, M10 = 0, M01 = 0;
			double M11 = 0, M20 = 0, M02 = 0;
			double M12 = 0, M21 = 0;
			double M30 = 0, M03 = 0;

			double InvM00 = 0;

			double CenterX = 0, CenterY = 0;

			int width = data.Width;
			int height = data.Height;
			int stride = data.Stride;

			int offset = stride - width;

			byte* src = (byte*) (data.Scan0);
			for (int y = 0; y < height; y++) {
				double yNorm = (double)y / height;
				for (int x = 0; x < width; x++, src++) {
					double v = *src / 255.0;

					double xNorm = (double)x / width;

					M00 += v;
					M01 += yNorm * v;
					M10 += xNorm * v;

					M11 += xNorm * yNorm * v;
					M02 += yNorm * yNorm * v;
					M20 += xNorm * xNorm * v;

					M21 += xNorm * xNorm * yNorm * v;
					M12 += xNorm * yNorm * yNorm * v;

					M30 += xNorm * xNorm * xNorm * v;
					M03 += yNorm * yNorm * yNorm * v;
				}

				src += offset;
			}

			bitmap.UnlockBits(data);

			InvM00 = 1f / M00;
			CenterX = M10 * InvM00;
			CenterY = M01 * InvM00;

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("M00", M00)
					.AddFeature("M01", M01)
					.AddFeature("M10", M10)
					.AddFeature("M11", M11)
					.AddFeature("M02", M02)
					.AddFeature("M20", M20)
					.AddFeature("M21", M21)
					.AddFeature("M12", M12)
					.AddFeature("M30", M30)
					.AddFeature("M03", M03)
					.AddFeature("CenterX", CenterX)
					.AddFeature("CenterY", CenterY)
			};
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Raw image moments";
			}
		}

		#endregion
	}
}

