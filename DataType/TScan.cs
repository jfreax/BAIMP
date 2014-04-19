using System;
using Xwt;
using System.Drawing;
using System.Collections.Generic;

namespace Baimp
{
	public class TScan : IType
	{
		private BaseScan scan;
		private string scanType;
		private bool multipleUsage;

		float[] rawData;
		List<Bitmap> grayScale8bbp = new List<Bitmap>();

		private Widget widget = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.TScan"/> class.
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="scanType">Scan type.</param>
		/// <param name="multipleUsage">
		/// If set to <c>true</c> every data retrieving will create a new copy.
		/// </param>
		public TScan(BaseScan scan, string scanType, bool multipleUsage = false)
		{
			this.scan = scan;
			this.scanType = scanType;
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
				}
				if (multipleUsage) {
					float[] copy = new float[rawData.Length];
					rawData.CopyTo(copy, 0);

					return copy;
				}
				return rawData;
			}
		}

		/// <summary>
		/// Gets a grayscale 8bpp representation.
		/// </summary>
		/// <value>The gray scale8bpp.</value>
		public Bitmap GrayScale8bpp {
			get {
				if (grayScale8bbp == null) {
					grayScale8bbp = new List<Bitmap>();
				}

				if (multipleUsage || grayScale8bbp.Count == 0) {
					Bitmap b = scan.GetAsBitmap(scanType);
					grayScale8bbp.Add(b);
					return b;
				}

				return grayScale8bbp[0];
			}
		}

		/// <summary>
		/// Enables the multiple access mode.
		/// You can't disable it after enabling.
		/// </summary>
		public void EnableMultipleAccessMode()
		{
			multipleUsage = true;
		}

		/// <summary>
		/// Preloads the data.
		/// </summary>
		public TScan Preload()
		{
			rawData = scan.GetAsArray(scanType);
			return this;
		}

		public Xwt.Size Size {
			get {
				return scan.Size;
			}
		}

		public object RawData()
		{
			return Data as object;
		}


		public Widget ToWidget()
		{
			if (widget == null) {
				ImageView iv = new ImageView();
				iv.Image = scan.GetAsImage(scanType, false, false);
				widget = iv;
			}

			return widget;
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

