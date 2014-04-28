//
//  TScan.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
﻿using System;
using System.Linq;
using Xwt;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Baimp
{
	public class TScan : IType
	{
		BaseScan scan;
		string scanType;
		bool multipleUsage;
		bool maskedOnly;
		float[] rawData;
		List<Bitmap> grayScale8bbp = new List<Bitmap>();
		Widget widget;
		Xwt.Size explicitSize = Xwt.Size.Zero;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.TScan"/> class.
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="scanType">Scan type.</param>
		/// <param name="multipleUsage">
		/// If set to <c>true</c> every data retrieving will create a new copy.
		/// </param>
		/// <param name="maskedOnly">Return masked data only. Non masked entries are set to zero.</param>
		public TScan(BaseScan scan, string scanType, bool multipleUsage = false, bool maskedOnly = false)
		{
			this.scan = scan;
			this.scanType = scanType;
			this.multipleUsage = multipleUsage;
			this.maskedOnly = maskedOnly;
		}

		public TScan(float[] data, Xwt.Size size, bool multipleUsage = false)
		{
			rawData = data;
			explicitSize = size;
			this.multipleUsage = multipleUsage;
		}

		/// <summary>
		/// Get plain scan data
		/// </summary>
		/// <value>The data.</value>
		/// <remarks>
		/// Check size!
		/// </remarks>
		public float[] Data {
			get {
				if (rawData == null) {
					rawData = scan.GetAsArray(scanType);

					if (maskedOnly) {
						Bitmap mask = scan.Mask.GetMaskAsBitmap();

						double ratioX = mask.Width / Size.Width;
						double ratioY = mask.Height / Size.Height;


						unsafe {
							BitmapData bmpData = mask.LockBits(
								                     new System.Drawing.Rectangle(0, 0, mask.Width, mask.Height),   
								                     ImageLockMode.ReadOnly, mask.PixelFormat);

							byte* scan0 = (byte*) bmpData.Scan0.ToPointer();

							for (int y = 0; y < Size.Height; y++) {
								for (int x = 0; x < Size.Width; x++) {
									if (scan0[(int) ((y * ratioY) * bmpData.Stride + (x * ratioX * 4))] == 0) {
										rawData[(int) (y * Size.Width) + x] = 0.0f;
									}
								}
							}

							mask.UnlockBits(bmpData);
						}
					
					}
				}

				if (multipleUsage) {
					float[] copy = new float[rawData.Length];
					rawData.CopyTo(copy, 0);

					return copy;
				}
				return rawData;
			}
		}

		public byte[] DataAs8bpp()
		{
			float[] data = Data;
			float max = data.Max();

			byte[] data8bpp = new byte[data.Length];
			int i = 0;
			foreach (float f in data) {
				data8bpp[i] = (byte) ((f * 255) / max);
				i++;
			}

			return data8bpp;
		}
			
		/// <summary>
		/// Enables the multiple access mode.
		/// You can't disable it after enabling.
		/// </summary>
		public void EnableMultipleAccessMode()
		{
			multipleUsage = true;
		}

		public bool IsMultipleAccessModeOn {
			get {
				return multipleUsage;
			}
		}

		/// <summary>
		/// Preloads the data.
		/// </summary>
		public TScan Preload()
		{
			rawData = Data;
			return this;
		}

		public Xwt.Size Size {
			get {
				if (explicitSize.IsZero) {  
					return scan.Size;
				}

				return explicitSize;
			}
		}

		public object RawData()
		{
			return Data;
		}

		public Widget ToWidget()
		{
			if (widget == null) {
				ImageView iv = new ImageView();
	            
				if (scan != null) {
					iv.Image = scan.GetAsImage(scanType, false, false);
				} // else TODO
				widget = iv;
			}

			return widget;
		}

		public override string ToString()
		{
			if (scan == null) {
				return string.Format("Scan (Size: {0})", explicitSize);
			}

			return string.Format("{0}_{1}", scan, scanType);
		}

		public void Dispose()
		{
			if (widget != null) {
				ImageView iv = (widget as ImageView);
				if (iv != null) {
					iv.Image.Dispose();
				}
				widget.Dispose();
				widget = null;
			}

			if (grayScale8bbp != null) {
				foreach (Bitmap gs in grayScale8bbp) {
					gs.Dispose();
				}
				grayScale8bbp.Clear();
			}

			rawData = null;
		}
	}
}

