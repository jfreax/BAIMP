using System;
using System.Collections.Generic;
using XD = Xwt.Drawing;
using System.IO;

namespace baimp
{
	public class Mask
	{
		private BaseScan scan;
		private Dictionary<string, XD.ImageBuilder> maskBuilder;

		public Mask(BaseScan scan)
		{
			this.scan = scan;
		}

		/// <summary>
		/// Gets the mask builder to draw on it.
		/// </summary>
		/// <returns>The mask builder.</returns>
		/// <param name="type">Type.</param>
		/// <remarks>
		/// Set the status of this scan to "unsaved"
		/// </remarks>
		public XD.ImageBuilder GetMaskBuilder(string scanType)
		{
			if (!maskBuilder.ContainsKey(scanType) || maskBuilder[scanType] == null) {
				maskBuilder[scanType] = new XD.ImageBuilder(scan.Size.Width, scan.Size.Height);

				XD.Image mask = LoadMask(scanType);
				if (mask != null) {
					maskBuilder[scanType].Context.DrawImage(mask.WithSize(scan.Size), Xwt.Point.Zero, 0.6);
				}
			}

			return maskBuilder[scanType];
		}

		/// <summary>
		/// Gets the mask as image.
		/// </summary>
		/// <returns>The mask as image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image GetMaskAsImage(string scanType)
		{
			return GetMaskBuilder(scanType).ToVectorImage().WithSize(scan.RequestedBitmapSize);
		}

		/// <summary>
		/// Loads mask data
		/// </summary>
		/// <returns>The mask as an image.</returns>
		/// <param name="type">Type.</param>
		public XD.Image LoadMask(string scanType)
		{
			return null;
		}

		public unsafe void SaveMask(ScanType type)
		{

		}
	}
}

