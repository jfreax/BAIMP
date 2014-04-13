﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace Baimp
{
	public class Tamura : BaseAlgorithm
	{
		public Tamura(PipelineNode parent) : base(parent)
		{
			input.Add(new Compatible("Image", typeof(TBitmap), new MaximumUses(1)));

			output.Add(new Compatible("Tamura Features", typeof(TFeatureList<double>)));
		}

		#region implemented abstract members of BaseAlgorithm

		public override IType[] Run(Dictionary<RequestType, object> requestedData, Option[] options, IType[] inputArgs)
		{
			TBitmap tbitmap = inputArgs[0] as TBitmap;
			Bitmap bitmap = tbitmap.Data;

			BitmapData data = bitmap.LockBits(
				                  new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				                  ImageLockMode.ReadOnly,
				                  bitmap.PixelFormat
			                  );

			int height = data.Height;
			int width = data.Width;

			double coarsness = 0.0;
			double[,] sumAreaTable = SumAreaTable(data);

			int oldProgress = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 1; x < width; x++) {
					coarsness += Math.Pow(2, Sopt(sumAreaTable, x, y));
				}

				int progress = (int) (y * 100.0) / height;
				if (progress - oldProgress > 5) {
					oldProgress = progress;
					SetProgress(progress);
				}
			}

			coarsness /= height * width;

			bitmap.UnlockBits(data);

			return new IType[] { 
				new TFeatureList<double>()
					.AddFeature("coarsness", coarsness)
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double Sopt(double[,] sumAreaTable, int x, int y)
		{
			double result = 0;
			int kOpt = 1;

			for (int k = 1; k < 5; k++) {
				int p = (int) Math.Pow(2, k - 1);

				double E_h = Math.Abs(
					AverageNeighborhoods(sumAreaTable, x + p, y, k) -
					AverageNeighborhoods(sumAreaTable, x - p, y, k)
				);
				double E_v = Math.Abs(
					AverageNeighborhoods(sumAreaTable, x, y + p, k) -
					AverageNeighborhoods(sumAreaTable, x, y - p, k)
				);

				double E_k = Math.Max(E_h, E_v);
				if (result < E_k) {
					kOpt = k;
					result = E_k;
				}
			}
			return kOpt;
		}
			
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double AverageNeighborhoods(double[,] sumAreaTable, int x, int y, int k)
		{
			int p = (int) Math.Pow(2, k - 1);

			int left = x - p;
			if (left < 0)
				left = 0;

			int right = x + p - 1;
			if (right >= sumAreaTable.GetLength(0))
				right = sumAreaTable.GetLength(0) - 1;

			int top = y - p;
			if (top < 0)
				top = 0;

			int bottom = y + p - 1;
			if (bottom >= sumAreaTable.GetLength(1))
				bottom = sumAreaTable.GetLength(1) - 1;
			if (bottom < 0)
				bottom = 0;

			return MeanSubMatrix(sumAreaTable, left, top, right, bottom);
		}

		static unsafe double[,] SumAreaTable(BitmapData data)
		{
			byte* src = (byte*) (data.Scan0);

			int stride = data.Stride;
			int n = data.Width;
			int m = data.Height;
			double[,] s = new double[n, m];
			double[,] ii = new double[n, m];

			s[0, 0] = *src;
			ii[0, 0] = s[0, 0];
			for (int x = 1; x < n; x++) {
				s[x, 0] = src[x]; // [x, 0];
				ii[x, 0] = ii[x - 1, 0] + s[x, 0]; 
			}
			for (int y = 1; y < m; y++) {
				ii[0, y] = s[0, y] = s[0, y - 1] + src[(stride * y)]; // [0, y];

				for (int x = 1; x < n; x++) {
					s[x, y] = s[x, y - 1] + src[(stride * y) + x];
					ii[x, y] = ii[x - 1, y] + s[x, y];
				}
			}
			return ii;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double MeanSubMatrix(double[,] sum, int left, int top, int right, int bottom)
		{
			double v1, v2 , v3, v4;
			v1 = (left == 0 || top == 0) ? 0 : sum[left - 1, top - 1];
			v2 = (top == 0) ? 0 : sum[right, top - 1];
			v3 = (left == 0) ? 0 : sum[left - 1, bottom];
			v4 = sum[right, bottom];

			return ((v4 + v1) - (v2 + v3)) / ((bottom - top) + (right - left));
		}

		public override AlgorithmType AlgorithmType {
			get {
				return AlgorithmType.Extraction;
			}
		}

		public override string HelpText {
			get {
				return "Tamura Features";
			}
		}

		#endregion
	}
}

