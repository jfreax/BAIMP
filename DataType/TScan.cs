using System;
using Xwt;
using System.Drawing;

namespace Baimp
{
	public class TScan : IType
	{
		private BaseScan scan;
		private string scanType;
		private bool multipleUsage;

		float[] rawData;
		Bitmap grayScale8bbp;

		private Widget widget = null;

		public TScan(BaseScan scan, string scanType, bool multipleUsage = true)
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

		public Bitmap GrayScale8bbp {
			get {
				if (grayScale8bbp == null) {
					grayScale8bbp = scan.GetAsBitmap(scanType);
				}

				return grayScale8bbp;
			}
		}

		public Xwt.Size Size {
			get {
				return scan.Size;
			}
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
				grayScale8bbp.Dispose();
				grayScale8bbp = null;
			}

			rawData = null;
		}
	}
}

