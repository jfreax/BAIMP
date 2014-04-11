﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Baimp
{
	public class GLCM : BaseAlgorithm
	{
		Rectangle area = Rectangle.Empty;

		public GLCM(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("Co-occurence matrix", typeof(TMatrix)));

			options.Add(new Option("X Offest", 0, 10, 1));
			options.Add(new Option("Y Offest", 0, 10, 1));
		}

		#region implemented abstract members of BaseAlgorithm

		public override unsafe IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			Bitmap bitmap = tbitmap.Data;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );
				
			int dx = (int) options[0].Value;
			int dy = (int) options[1].Value;

			double[,] matrix = new double[256, 256];
			int height = data.Height;
			int width = data.Width;
			int stride = data.Stride;


			if (area == Rectangle.Empty) {
				area = new Rectangle(0, 0, width, height);
			}

			int windowX = Math.Max(area.X, -dx);
			int windowY = Math.Max(area.Y, -dy);
			int windowWidth = Math.Min(windowX + area.Width, width - Math.Abs(dy));
			int windowHeight = Math.Min(windowY + area.Height, height - Math.Abs(dy));

			int pairs = 0;

			if (data.PixelFormat == PixelFormat.Format8bppIndexed) {
				int offset = stride - width;
				byte* src = (byte*) (data.Scan0) + windowY * stride + windowX;
				byte* srcBegin = (byte*) (data.Scan0) + windowY * stride + windowX;

				int oldProgress = 0;
				for (int j = windowY; j < windowHeight; j++) {
					int y = j - windowY;

					for (int i = windowX; i < windowWidth; i++, src++) {
						int x = i - windowX;

						int posWithOffset = ((y + dy) * stride) + (x + dx);
						matrix[*src, srcBegin[posWithOffset]]++;

						pairs++;
					}

					src += offset;

					int progress = (int) (j * 100.0) / windowHeight;
					if (progress - oldProgress > 10) {
						oldProgress = progress;
						SetProgress(progress);
					}
				}
			} else {
				int pixelSize = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;
				int offset = stride - width * pixelSize;
				byte* src = (byte*) (data.Scan0) + windowY * stride + windowX * pixelSize;
				byte* srcBegin = (byte*) (data.Scan0) + windowY * stride + windowX;

				int oldProgress = 0;
				for (int j = windowY; j < windowHeight; j++) {
					int y = j - windowY;

					for (int i = windowX; i < windowWidth; i++, src += pixelSize) {
						int x = i - windowX;

						float v = (float) (0.2125 * src[0] + 0.7154 * src[1] + 0.0721 * src[2]);
						int posWithOffset = ((y + dy) * stride) + (x + dx) * pixelSize;

						matrix[(int) v, srcBegin[posWithOffset / 2]]++;

						pairs++;
					}

					src += offset;

					int progress = (int) (j * 100.0) / windowHeight;
					if (progress - oldProgress > 10) {
						oldProgress = progress;
						SetProgress((int) (j * 100.0) / windowHeight);
					}
				}
			}

			bitmap.UnlockBits(data);

			// normalize
			if (pairs > 0) {
				fixed (double* ptrMatrix = matrix) {
					double* c = ptrMatrix;
					for (int i = 0; i < matrix.Length; i++, c++) {
						*c /= pairs;
					}
				}
			}

			IType[] ret = { new TMatrix(matrix) };
			return ret;
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Descriptor;
			}
		}

		public override string HelpText {
			get {
				return "GLCM";
			}
		}

		#endregion
	}
}

